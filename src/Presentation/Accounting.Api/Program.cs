using Accounting.Api.Filters;
using Accounting.Api.Options;
using Accounting.Composition;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Host
builder.Host.UseSerilog((context , services , configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});
#endregion Host

#region Composition
builder.Services.AddHttpContextAccessor();

builder.Services.AddAccountingApi(
    builder.Configuration ,
    typeof(Program).Assembly);
#endregion Composition

#region JwtSettings
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey) ,
        "Jwt:SecretKey zorunludur.")
    .Validate(s => s.SecretKey.Length >= 32 ,
        "Jwt:SecretKey en az 32 karakter olmalıdır (HS256 için 256-bit anahtar).")
    .Validate(s => s.AccessTokenMinutes > 0 ,
        "Jwt:AccessTokenMinutes pozitif olmalıdır.")
    .Validate(s => s.RefreshTokenDays > 0 ,
        "Jwt:RefreshTokenDays pozitif olmalıdır.")
    .ValidateOnStart();

JwtSettings jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt yapılandırması eksik.");
#endregion JwtSettings

#region Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true ,
            ValidIssuer = jwtSettings.Issuer ,
            ValidateAudience = true ,
            ValidAudience = jwtSettings.Audience ,
            ValidateIssuerSigningKey = true ,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)) ,
            ValidateLifetime = true ,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
#endregion Authentication

#region Mvc
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultActionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();
#endregion Mvc

var app = builder.Build();

#region Startup Seeders
await RunStartupSeedersAsync(app);
#endregion Startup Seeders

#region Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
#endregion Pipeline

app.Run();

#region Helpers
static async Task RunStartupSeedersAsync(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();

    IEnumerable<IStartupSeeder> seeders = scope.ServiceProvider.GetServices<IStartupSeeder>();

    foreach (IStartupSeeder seeder in seeders)
    {
        Result result = await seeder.SeedAsync();

        if (result.IsSuccess)
        {
            Log.Information("Seed tamamlandı: {Seeder} — {Message}" , seeder.GetType().Name , result.Message);
        }
        else
        {
            Log.Warning("Seed başarısız: {Seeder} — {Message}" , seeder.GetType().Name , result.Message);
        }
    }
}
#endregion Helpers