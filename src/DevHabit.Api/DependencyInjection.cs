using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Middleware;
using DevHabit.Api.Services.Sorting;
using DevHabit.Api.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DevHabit.Api.Settings;
using System.Text;

namespace DevHabit.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;
        })
        .AddNewtonsoftJson(options =>
            options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver())
        .AddXmlSerializerFormatters();

        builder.Services.Configure<MvcOptions>(options =>
        {
            NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
            .OfType<NewtonsoftJsonOutputFormatter>()
            .First();

            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
        });

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                // using CurrentImplementationApiVersionSelector for using the latest version
                // but to be more explicit, we can use DefaultApiVersionSelector
                options.ApiVersionSelector = new DefaultApiVersionSelector(options);

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new MediaTypeApiVersionReader(),
                    new MediaTypeApiVersionReaderBuilder()
                        .Template("application/vnd.dev-habit.hateaos.{version}+json")
                        .Build());
            })
            .AddMvc();

        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd(
                    "requestId",
                    context.HttpContext.TraceIdentifier);
            };
        });
        // remember to add the fluent validator before the global exception handler
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Postgres"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .UseSnakeCaseNamingConvention());
        // using a separate db setup for future detachability
        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Postgres"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
        .WithTracing(tracing => tracing
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddNpgsql())
        .WithMetrics(metrics => metrics
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation())
        .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        // if validators are internal, need to add includeInternalTypes: true
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        // Mapping services
        builder.Services.AddTransient<SortMappingProvider>();
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ =>
            HabitMappings.SortMapping);
        // Data shaping
        builder.Services.AddTransient<DataShapingService>();
        // Link services
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<LinkService>();
        // Token provider
        builder.Services.AddTransient<TokenProvider>();

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));
        JwtAuthOptions? jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>() ??
            throw new InvalidOperationException("Jwt configuration is missing");

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}
