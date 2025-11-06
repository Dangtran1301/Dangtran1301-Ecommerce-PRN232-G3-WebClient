using PRN232_WebClient_Tachonogy.ApiClients;
using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.Extensions;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Middlewares;
using PRN232_WebClient_Tachonogy.Services;
using PRN232_WebClient_Tachonogy.Services.Interfaces;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(1);
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient<IAdminClient, AdminClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient<ODataApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IBrandApiClient, BrandApiClient>();
builder.Services.AddScoped<IUserProfileApiClient, UserProfileApiClient>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminClient, AdminClient>();
builder.Services.AddScoped<ICategoryApiClient, CategoryApiClient>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<TokenRefreshMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
