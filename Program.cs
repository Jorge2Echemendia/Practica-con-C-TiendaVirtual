using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using TiendaVirtual.Components;
using TiendaVirtual.Service;
using TiendaVirtual.Context;
using Oracle.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Provider;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Blazored.LocalStorage;
using TiendaVirtual.Model;
using QuestPDF.Infrastructure;
using TiendaVirtual;
using Microsoft.AspNetCore.SignalR;
using DotNetEnv;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddSignalR();
builder.Services.AddScoped<CarritoService>();
builder.Services.AddScoped<ReciboCompra>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ImagenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddScoped<ButtonViewService>();
builder.Services.AddScoped<TarjetaService>();
builder.Services.AddScoped<ReciboCompraDocument>();
builder.Services.AddScoped<AdminDashboardService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway usa un formato de URL, se convierte al formato de Npgsql
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    }.ToString();
}
else
{
    // Para desarrollo local, usa la cadena de appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnectionPostSQL");
}

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

Env.Load();

var apiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
builder.Configuration["GoogleMaps:ApiKey"] = apiKey;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapHub<StockHub>("/stockHub");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Esto aplica las migraciones automáticamente
}

app.Run();
