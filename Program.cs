using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PanierMVC.Data;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

string JwtKey = "C1CF4B7DC4C4175B6618DE4F55CA4";//symetric key
string JwtAudience = "SecureApiUser";
string JwtIssuer = "https://localhost:44381";
Double JwtExpireDays = 30;


builder.Services.AddDbContext<PanierMVCContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PanierMVCContext") ?? throw new InvalidOperationException("Connection string 'PanierMVCContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();



///Ajout du cache memory

builder.Services.AddMemoryCache();

///Ajout de la configuration des loggers
builder.Host.UseSerilog((hostingContext, LoggerConfiguration) =>
{
    LoggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/logs.txt", rollingInterval: RollingInterval.Day);
    
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/User/login";
    options.AccessDeniedPath = "/User/AccessDenied";
    options.Cookie.Name = "AccessToken";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = JwtIssuer,
        ValidAudience = JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Produits}/{action=Index}/{id?}");

app.Run();
