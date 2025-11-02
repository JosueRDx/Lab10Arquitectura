
using System.Text;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Application.MediaTr;
using Lab10_RodrigoApaza.Infrastructure.Configuration;
using Lab10_RodrigoApaza.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Lab10_RodrigoApaza.Api.Configuration;
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureServices(configuration);
        
        // Le decimos a MediatR que escanee el proyecto de la capa de aplicación
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssemblies(typeof(ApplicationAssemblyMarker).Assembly));
        
        // Registrar servicios de la capa de aplicación
        //services.AddScoped<IAuthService, AuthService>();
        //services.AddScoped<ITicketService, TicketService>();
        //services.AddScoped<IResponseService, ResponseService>();

        // Registra servicios de la API
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ticketera API Lab10",
                Version = "v1",
                Description = "API para gestionar tickets de soporte"
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Autorización JWT usando el esquema Bearer.\n\nIngrese 'Bearer' seguido de un espacio y el token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        });

        // --- Configuración de Autenticación JWT ---
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // Valida quién emitió el token
                    ValidateAudience = true, // Valida para quién es el token
                    ValidateLifetime = true, // Valida que el token no haya expirado
                    ValidateIssuerSigningKey = true, // Valida la firma del token
        
                    ValidIssuer = configuration["Jwt:Issuer"], // Lee el Issuer desde appsettings.json
                    ValidAudience = configuration["Jwt:Audience"], // Lee el Audience desde appsettings.json
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])) // Lee la clave secreta y la codifica
                };
            });
        
        // Añade la autorización (necesaria para usar [Authorize])
        services.AddAuthorization();

        return services;
    }
}