using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Infrastructure.Authentication;
using CollateralPortal.Infrastructure.BackgroundServices;
using CollateralPortal.Infrastructure.Documents;
using CollateralPortal.Infrastructure.Identity;
using CollateralPortal.Infrastructure.Integrations.Mocks;
using CollateralPortal.Infrastructure.Integrations.Real;
using CollateralPortal.Infrastructure.Messaging;
using CollateralPortal.Infrastructure.Persistence;
using CollateralPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CollateralPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<DocumentStorageOptions>(configuration.GetSection(DocumentStorageOptions.SectionName));
        // ---- Identity ----
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));


        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
 .AddJwtBearer(options =>
 {
     // Keep JWT claim names exactly as issued:
     // sub, jti, SessionId, etc.
     options.MapInboundClaims = false;

     options.TokenValidationParameters =
         new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,

             ValidIssuer = jwtSettings.Issuer,
             ValidAudience = jwtSettings.Audience,

             IssuerSigningKey =
                 new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(
                         jwtSettings.Key)),

             ClockSkew = TimeSpan.FromMinutes(1)
         };

     options.Events = new JwtBearerEvents
     {
         OnTokenValidated = context =>
         {
             var blacklistService = context.HttpContext
                     .RequestServices
                     .GetRequiredService<ITokenBlacklistService>();

             // MapInboundClaims = false,
             // therefore read the raw JWT claim names.
             var userId =
                 context.Principal?
                     .FindFirst(JwtRegisteredClaimNames.Sub)?
                     .Value;

             var sessionId =
                 context.Principal?
                     .FindFirst("SessionId")?
                     .Value;

             if (string.IsNullOrWhiteSpace(userId) ||
                 string.IsNullOrWhiteSpace(sessionId))
             {
                 context.Fail(
                     "Invalid authentication session.");

                 return Task.CompletedTask;
             }

             if (blacklistService.IsBlacklisted(
                 userId,
                 sessionId))
             {
                 context.Fail(
                     "Token has been revoked.");

                 return Task.CompletedTask;
             }

             return Task.CompletedTask;
         },

         OnAuthenticationFailed = context =>
         {
             Console.WriteLine(
                 $"JWT authentication failed: " +
                 $"{context.Exception.Message}");

             return Task.CompletedTask;
         }
     };
 });

        services.AddAuthorization();

        // User and Auth
        services.AddMemoryCache();
        services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();

        // ---- Azure Service Bus ----
        services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.SectionName));
        services.AddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();
        services.AddHostedService<CollateralNotificationConsumer>();
        services.AddHostedService<InsuranceRenewalReminderService>();

        // ---- Supporting services ----
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICollateralDocumentService, CollateralDocumentService>();
        services.AddScoped<ICollateralReportDocumentService,CollateralReportDocumentService>();
        services.AddScoped<ICollateralIngestionService, CollateralIngestionService>();
        services.AddScoped<ICsvExportService, CsvExportService>();

        services.AddHostedService<CollateralImportWorker>();
        services.AddScoped<ITatService, TatService>();
        services.Configure<TatSettings>(configuration.GetSection(TatSettings.SectionName));

        // ---- Third-party integrations ----
        // Newgen / Vision / Credit Risk: always mocked for now (per project decision).
        services.AddSingleton<ILoanManagementClient, MockLoanManagementClient>();
        services.AddSingleton<ICoreBankingClient, MockCoreBankingClient>();
        services.AddSingleton<ICreditRiskClient, MockCreditRiskClient>();
        //services.AddSingleton<IValuationPortalClient, MockValuationPortalClient>();

        // Credit Bureau / NGX market data: mocked by default, flip the config
        // flag once real credentials/API keys are provisioned.
        // ---- Third-party integrations ----

        // Newgen / Vision / Credit Risk
        services.AddSingleton<ILoanManagementClient, MockLoanManagementClient>();
        services.AddSingleton<ICoreBankingClient, MockCoreBankingClient>();
        services.AddSingleton<ICreditRiskClient, MockCreditRiskClient>();

        // ---------------------------------------------------------
        // Credit Bureau
        // ---------------------------------------------------------
        var useCreditBureauMock =
            configuration.GetValue(
                "Integrations:CreditBureau:UseMock",
                true);

        if (useCreditBureauMock)
        {
            services.AddSingleton<
                ICreditBureauClient,
                MockCreditBureauClient>();
        }
        else
        {
            services.AddHttpClient<
                ICreditBureauClient,
                CreditBureauAdapter>(client =>
                {
                    client.BaseAddress = new Uri(
                configuration[
                    "Integrations:CreditBureau:BaseUrl"]!);
                });
        }

        // ---------------------------------------------------------
        // NGX Market Data
        // ---------------------------------------------------------
        var useMarketDataMock =
            configuration.GetValue(
                "Integrations:MarketData:UseMock",
                true);

        if (useMarketDataMock)
        {
            services.AddSingleton<
                IMarketDataClient,
                MockMarketDataClient>();
        }
        else
        {
            services.AddHttpClient<
                IMarketDataClient,
                MarketDataAdapter>(client =>
                {
                    client.BaseAddress = new Uri(
                configuration[
                    "Integrations:MarketData:BaseUrl"]!);
                });
        }

        // ---------------------------------------------------------
        // Valuation Portal
        // ---------------------------------------------------------
        var useValuationPortalMock =
            configuration.GetValue(
                "Integrations:ValuationPortal:UseMock",
                true);

        if (useValuationPortalMock)
        {
            services.AddSingleton<
                IValuationPortalClient,
                MockValuationPortalClient>();
        }
        else
        {
            var valuationPortalBaseUrl =
                configuration[
                    "Integrations:ValuationPortal:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Valuation Portal BaseUrl is not configured.");

            services.AddHttpClient<
                IValuationPortalClient,
                ValuationPortalAdapter>(client =>
                {
                    client.BaseAddress =
                new Uri(valuationPortalBaseUrl);
                });
        }

        return services;
    }
}