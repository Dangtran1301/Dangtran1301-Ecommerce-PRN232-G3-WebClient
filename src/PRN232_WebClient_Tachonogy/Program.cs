using Microsoft.AspNetCore.DataProtection;
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

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
    .SetApplicationName("TachonogyApp");

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
builder.Services.AddScoped<IOrderApiClient, OrderApiClient>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductAttributeApiClient, ProductAttributeApiClient>();
builder.Services.AddScoped<IProductAttributeService, ProductAttributeService>();
builder.Services.AddScoped<IProductVariantApiClient, ProductVariantApiClient>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IStockApiClient, StockApiClient>();
builder.Services.AddScoped<IStockService, StockService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<TokenRefreshMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
