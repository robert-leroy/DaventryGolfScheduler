using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using GolfScheduler.Api.Configuration;
using GolfScheduler.Api.Data;
using GolfScheduler.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Check if we should bypass authentication (for development)
var bypassAuth = builder.Configuration.GetValue<bool>("BypassAuthentication");

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

if (bypassAuth)
{
    // Development mode: bypass authentication
    builder.Services.AddAuthentication("DevAuth")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthHandler>(
            "DevAuth", options => { });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireAssertion(_ => true));
    });
}
else
{
    // Production mode: use internal JWT authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("AdminOnly", policy =>
            policy.RequireAssertion(context =>
            {
                var isAdminClaim = context.User.FindFirst("isAdmin")?.Value;
                return isAdminClaim == "true" || isAdminClaim == "True";
            }));
}

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Golf Scheduler API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply pending migrations and seed dev user on startup (for development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Create a dev user if bypassing auth
    if (bypassAuth && !db.Users.Any(u => u.Email == "dev@localhost"))
    {
        db.Users.Add(new GolfScheduler.Api.Models.User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 12),
            Email = "dev@localhost",
            FirstName = "Dev",
            LastName = "User",
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

app.Run();

// Development authentication handler
public class DevAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public DevAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        // Use the dev user's actual GUID so GetCurrentUserAsync can find the user
        var devUserId = "11111111-1111-1111-1111-111111111111";
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, devUserId),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "dev@localhost"),
            new System.Security.Claims.Claim("name", "Dev User"),
            new System.Security.Claims.Claim("isAdmin", "true"),
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "DevAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "DevAuth");

        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}
