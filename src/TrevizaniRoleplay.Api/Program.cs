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

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture =
    CultureInfo.GetCultureInfo("pt-BR");

var builder = WebApplication.CreateBuilder(args);

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
    options.AddPolicy(PolicySettings.POLICY_SERVER_SUPPORT,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.ServerSupport, null)));
    options.AddPolicy(PolicySettings.POLICY_JUNIOR_SERVER_ADMIN,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.JuniorServerAdmin, null)));
    options.AddPolicy(PolicySettings.POLICY_LEAD_SERVER_ADMIN,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.LeadServerAdmin, null)));
    options.AddPolicy(PolicySettings.POLICY_SERVER_MANAGER,
        policy => policy.Requirements.Add(new AuthorizationRequirement(UserStaff.ServerManager, null)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_FURNITURES,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Furnitures)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_PROPERTIES,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Properties)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_ANIMATIONS,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Animations)));
    options.AddPolicy(PolicySettings.POLICY_STAFF_FLAG_FACTIONS,
        policy => policy.Requirements.Add(new AuthorizationRequirement(null, StaffFlag.Factions)));
    options.DefaultPolicy = options.GetPolicy("Default")!;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "Los Santos Chronicles", Version = "v1" });
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