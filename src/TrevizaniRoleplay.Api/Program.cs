using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text;
using TrevizaniRoleplay.Api.Authorization;
using TrevizaniRoleplay.Api.Handlers;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

var builder = WebApplication.CreateBuilder(args);

var language = builder.Configuration.GetValue<string>("Language")!;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture =
    CultureInfo.GetCultureInfo(language);

builder.Logging.AddFile("logs/api-{Date}.txt");

builder.Services.AddControllers();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<DiscordSettings>(builder.Configuration.GetSection("Discord"));
builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection("MercadoPago"));
builder.Services.AddDbContext<DatabaseContext>(x =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    x.LogTo(Console.WriteLine, LogLevel.Information)
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging();
    x.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    x.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
builder.Services.AddScoped<IAuthorizationHandler, AuthorizationRequirementHandler>();

var jwtKey = builder.Configuration.GetSection("Jwt").GetValue<string>("Key") ?? string.Empty;

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Default",
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.None, null)));
    options.AddPolicy(PolicySettings.POLICY_TESTER,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.Tester, null)));
    options.AddPolicy(PolicySettings.POLICY_GAME_ADMIN,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.GameAdmin, null)));
    options.AddPolicy(PolicySettings.POLICY_LEAD_ADMIN,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.LeadAdmin, null)));
    options.AddPolicy(PolicySettings.POLICY_HEAD_ADMIN,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.HeadAdmin, null)));
    options.AddPolicy(PolicySettings.POLICY_MANAGEMENT,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.Management, null)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_FURNITURES,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Furnitures)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_PROPERTIES,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Properties)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_ANIMATIONS,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Animations)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_FACTIONS,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Factions)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_ITEMS,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Items)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_DRUGS,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Drugs)));
    options.DefaultPolicy = options.GetPolicy("Default")!;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "Trevizani Roleplay", Version = "v1" });
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
    x.CustomSchemaIds(type => type.ToString().Replace('+', '.').Replace($"{type.Namespace}.", string.Empty));
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCors(x =>
    x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
        x.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
    });
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();