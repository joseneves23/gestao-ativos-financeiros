using Microsoft.AspNetCore.Authentication.Cookies;
using AtivosFinanceiros.Models;
using AtivosFinanceiros.Services;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Setup for distributed memory cache and session management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Essential for the session cookie
});


// Add authentication services
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
    });

// Add authorization politic
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministradorPolicy", policy =>
        policy.RequireClaim("TipoPerfil", "ADMINISTRADOR"));
});

var connectionString = $"Host={Env.GetString("POSTGRES_HOST")};" +
                       $"Port={Env.GetString("POSTGRES_PORT")};" +
                       $"Database={Env.GetString("POSTGRES_DB")};" +
                       $"Username={Env.GetString("POSTGRES_USER")};" +
                       $"Password={Env.GetString("POSTGRES_PASSWORD")}";

builder.Services.AddDbContext<MeuDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();
app.UseSession(); // Initialize session middleware here

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
