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
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")
      ));


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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

app.Run();
