using Microsoft.EntityFrameworkCore;
using SSO.Repository.Database;
using SSO.Repository.Implementations;
using SSO.Repository.Interfaces;
using SSO.Services.Implementations;
using SSO.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

// === Logging ===
Environment.SetEnvironmentVariable("Logging__LogLevel__Default", builder.Configuration["Logging:LogLevel:Default"] ?? "Information");
Environment.SetEnvironmentVariable("Logging__LogLevel__Microsoft.AspNetCore", builder.Configuration["Logging:LogLevel:Microsoft.AspNetCore"] ?? "Warning");

// === AllowedHosts ===
Environment.SetEnvironmentVariable("AllowedHosts", builder.Configuration["AllowedHosts"] ?? "*");

// === Authorization ===
Environment.SetEnvironmentVariable("Authorization__ClientId", builder.Configuration["Authorization:ClientId"] ?? "");
Environment.SetEnvironmentVariable("Authorization__ClientSecret", builder.Configuration["Authorization:ClientSecret"] ?? "");
Environment.SetEnvironmentVariable("Authorization__RedirectUri", builder.Configuration["Authorization:RedirectUri"] ?? "");

// === ConnectionStrings ===
Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "");

// === JwtSettings ===
Environment.SetEnvironmentVariable("JwtSettings__Issuer", builder.Configuration["JwtSettings:Issuer"] ?? "");
Environment.SetEnvironmentVariable("JwtSettings__Audience", builder.Configuration["JwtSettings:Audience"] ?? "");
Environment.SetEnvironmentVariable("JwtSettings__ExpiryMinutes", builder.Configuration["JwtSettings:ExpiryMinutes"] ?? "");

// === SSO ===
Environment.SetEnvironmentVariable("SSO__ui", builder.Configuration["SSO:ui"] ?? "");

// === OAuthProviders:Google ===
Environment.SetEnvironmentVariable("OAuthProviders__Google__TokenUrl", builder.Configuration["OAuthProviders:Google:TokenUrl"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Google__Scopes", builder.Configuration["OAuthProviders:Google:Scopes"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Google__EmailClaim", builder.Configuration["OAuthProviders:Google:EmailClaim"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Google__ClientId", builder.Configuration["OAuthProviders:Google:ClientId"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Google__ClientSecret", builder.Configuration["OAuthProviders:Google:ClientSecret"] ?? "");

// === OAuthProviders:Microsoft ===
Environment.SetEnvironmentVariable("OAuthProviders__Microsoft__TokenUrl", builder.Configuration["OAuthProviders:Microsoft:TokenUrl"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Microsoft__Scopes", builder.Configuration["OAuthProviders:Microsoft:Scopes"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Microsoft__EmailClaim", builder.Configuration["OAuthProviders:Microsoft:EmailClaim"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Microsoft__ClientId", builder.Configuration["OAuthProviders:Microsoft:ClientId"] ?? "");
Environment.SetEnvironmentVariable("OAuthProviders__Microsoft__ClientSecret", builder.Configuration["OAuthProviders:Microsoft:ClientSecret"] ?? "");


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("allowall",
        policy => policy
                .AllowAnyOrigin()
    );
});

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("allowall");

app.Run();
