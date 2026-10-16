using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using System.Text;


public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

 
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=localhost\\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            Console.WriteLine("⚠️ Using fallback connection string");
        }
        else
        {
            Console.WriteLine($"✅ Connection string loaded");
        }

        builder.Services.AddDbContext<RaceDayDbContext>(options =>
            options.UseSqlServer(connectionString));

        
        // Dependency 
        
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IEnrolmentService, EnrolmentService>();
        builder.Services.AddScoped<IResultService, ResultService>();

        
        // Controllers
       
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

       
        // JWT Authentication
        
        var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization
       
        builder.Services.AddAuthorization();

        
        // Swagger Configuration
      
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RaceDay API",
                Version = "v1",
                Description = "RESTful API for the RaceDay Event Management System"
            });

            
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null, null),
                    new List<string>()
                }
            });
        });

        
        // 7. CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Configure Pipeline
       

        // Seed database
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            {

}dbContext.Database.EnsureCreated();
        }

        // Swagger - enable in all environments easy testing
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
            c.RoutePrefix = string.Empty; // Swagger at root
        });

        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}