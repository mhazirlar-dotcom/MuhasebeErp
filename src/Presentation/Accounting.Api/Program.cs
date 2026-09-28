using Accounting.Api.Filters;
using Accounting.Api.Options;
using Accounting.Composition;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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

    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var modelErrors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .SelectMany(kv => kv.Value!.Errors.Select(e => new
            {
                Field = kv.Key ,
                Message = string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Geçersiz değer." : e.ErrorMessage ,
                Exception = e.Exception?.Message
            }))
            .ToList();

        Log.Warning("[ApiBehavior ModelState] {@Errors}" , modelErrors);

        IReadOnlyList<Error> errorList = [.. modelErrors.Select(e => new Error(
            ResultStatus.ValidationError ,
            $"{e.Field}: {e.Message}" ,
            ResultStatus.ValidationError ,
            e.Field))];

        return new BadRequestObjectResult(Result.ValidationFailure(errorList));
    };
});

builder.Services.AddOpenApi();
#endregion Mvc

var app = builder.Build();

#region Startup Database Migrations + Seeders (Arka Planda)
// Migration ve seed işlemleri Kestrel'i bloklamaz. API hemen dinlemeye başlar,
// UI hızlıca bağlanır. Migration ve seed arka planda tamamlanır.
_ = Task.Run(async () =>
{
    try
    {
        using IServiceScope scope = app.Services.CreateScope();
        ICompanyDatabaseMigrator migrator = scope.ServiceProvider.GetRequiredService<ICompanyDatabaseMigrator>();
        Result<int> migrateResult = await migrator.MigrateAllAsync();

        if (migrateResult.IsFailure)
        {
            Log.Warning("[Startup] Firma veritabanları güncellenemedi — {Message}" , migrateResult.Message);
        }

        IEnumerable<IStartupSeeder> seeders = scope.ServiceProvider.GetServices<IStartupSeeder>();

        foreach (IStartupSeeder seeder in seeders)
        {
            Result seedResult = await seeder.SeedAsync();

            if (seedResult.IsSuccess)
            {
                Log.Information("Seed tamamlandı: {Seeder} — {Message}" , seeder.GetType().Name , seedResult.Message);
            }
            else
            {
                Log.Warning("Seed başarısız: {Seeder} — {Message}" , seeder.GetType().Name , seedResult.Message);
            }
        }

        Log.Information("[Startup] Arka plan migration ve seed işlemleri tamamlandı.");
    }
    catch (Exception ex)
    {
        Log.Error(ex , "[Startup] Arka plan migration/seed sırasında hata oluştu.");
    }
});
#endregion Startup Database Migrations + Seeders

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
