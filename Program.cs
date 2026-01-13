using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using TiendaVirtual.Components;
using TiendaVirtual.Service;
using TiendaVirtual.Context;
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
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno de Railway
var isDevelopment = builder.Environment.IsDevelopment();
QuestPDF.Settings.License = LicenseType.Community;

// **CONFIGURACIÓN DE CONEXIÓN A POSTGRESQL - CORREGIDO**
string connectionString;

if (!isDevelopment)
{
    // Para PRODUCCIÓN (Railway) - usar variables individuales
    var dbHost = Environment.GetEnvironmentVariable("PGHOST") ??
                 Environment.GetEnvironmentVariable("RAILWAY_PRIVATE_DOMAIN");
    var dbPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("PGDATABASE") ??
                 Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "railway";
    var dbUser = Environment.GetEnvironmentVariable("PGUSER") ??
                 Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
    var dbPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ??
                     Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

    if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbPassword))
    {
        connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode=Require;Trust Server Certificate=true";

        Console.WriteLine($"Conectando a PostgreSQL en: {dbHost}:{dbPort}");
    }
    else
    {
        // Fallback a DATABASE_URL si las variables individuales no están
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            try
            {
                // Parsear la URL de Railway
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
                connectionString = builder.Configuration.GetConnectionString("DefaultConnectionPostSQL");
            }
        }
        else
        {
            connectionString = builder.Configuration.GetConnectionString("DefaultConnectionPostSQL");
        }
    }
}
else
{
    // Para DESARROLLO local
    connectionString = builder.Configuration.GetConnectionString("DefaultConnectionPostSQL");
}

Console.WriteLine($"Connection String: {connectionString.Replace("Password=", "Password=******")}");

// **CONFIGURACIÓN DE JWT - CORREGIDO**
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ??
             Environment.GetEnvironmentVariable("Jwt__Key") ??
             builder.Configuration["Jwt:Key"] ??
             "TuClaveSuperSecretaMuyLargaYSegura123456789!";

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
                Environment.GetEnvironmentVariable("Jwt__Issuer") ??
                builder.Configuration["Jwt:Issuer"] ??
                "TiendaVirtualAPI";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                  Environment.GetEnvironmentVariable("Jwt__Audience") ??
                  builder.Configuration["Jwt:Audience"] ??
                  "TiendaVirtualClient";

var key = Encoding.UTF8.GetBytes(jwtKey);

// Configurar autenticación JWT
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Configurar DbContext
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

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
builder.Services.AddHealthChecks();

// Configurar URLs
if (isDevelopment)
{
    builder.WebHost.UseUrls("http://localhost:5252");
}
else
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// **CONFIGURACIÓN DE GOOGLE MAPS - CORREGIDO**
var googleMapsApiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
if (!string.IsNullOrEmpty(googleMapsApiKey))
{
    builder.Configuration["GoogleMaps:ApiKey"] = googleMapsApiKey;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapHealthChecks("/health");
app.MapHub<StockHub>("/stockHub");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Ejecutar migraciones de base de datos
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("Aplicando migraciones de base de datos...");

        // Verificar si podemos conectarnos
        if (dbContext.Database.CanConnect())
        {
            logger.LogInformation("Conexión exitosa. Aplicando migraciones...");
            dbContext.Database.Migrate();
            logger.LogInformation("Migraciones aplicadas exitosamente.");
        }
        else
        {
            logger.LogError("No se puede conectar a la base de datos.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones. La aplicación iniciará pero puede no funcionar correctamente.");
    }
}

app.Run();