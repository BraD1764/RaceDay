using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly string _jwtSecret;

        public AuthService(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtSecret = configuration["Jwt:Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
                return null;

            // Validate role
            if (registerDto.Role != "Organiser" && registerDto.Role != "Participant")
                return null;

            // Create user with hashed password
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create role-specific record
            if (registerDto.Role == "Organiser")
            {
                var organiser = new Organiser
                {
                    UserID = user.UserID,
                    Organisation = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Organisers.Add(organiser);
            }
            else
            {
                var participant = new Participant
                {
                    UserID = user.UserID,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            return new AuthResponseDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Participant)
                .Include(u => u.Organiser)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return null;

            var profile = new UserProfileDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            if (user.Role == "Participant" && user.Participant != null)
            {
                profile.DateOfBirth = user.Participant.DateOfBirth;
                profile.Gender = user.Participant.Gender;
                profile.EmergencyContact = user.Participant.EmergencyContact;
                profile.MedicalInfo = user.Participant.MedicalInfo;
            }
            else if (user.Role == "Organiser" && user.Organiser != null)
            {
                profile.Organisation = user.Organiser.Organisation;
                profile.Bio = user.Organiser.Bio;
            }

            return profile;
        }

        public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto updateDto)
        {
            var user = await _context.Users
                .Include(u => u.Participant)
                .Include(u => u.Organiser)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return null;

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Email = updateDto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.Role == "Participant" && user.Participant != null)
            {
                user.Participant.DateOfBirth = updateDto.DateOfBirth;
                user.Participant.Gender = updateDto.Gender;
                user.Participant.EmergencyContact = updateDto.EmergencyContact;
                user.Participant.MedicalInfo = updateDto.MedicalInfo;
                user.Participant.UpdatedAt = DateTime.UtcNow;
            }
            else if (user.Role == "Organiser" && user.Organiser != null)
            {
                user.Organiser.Organisation = updateDto.Organisation;
                user.Organiser.Bio = updateDto.Bio;
                user.Organiser.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}