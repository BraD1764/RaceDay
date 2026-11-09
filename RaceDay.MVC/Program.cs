var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("RaceDayApi", client => {
    var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        apiBaseUrl = "http://localhost:5001/";
    }

    if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri) ||
        (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
    {
        throw new InvalidOperationException(
            $"Api:BaseUrl must be a valid absolute HTTP(S) URL. Effective value: '{apiBaseUrl}'.");
    }

    client.BaseAddress = baseUri;
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();