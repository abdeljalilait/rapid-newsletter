using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Infrastructure.Persistence;
using NewsletterPlatform.Infrastructure.Persistence.Repositories;
using NewsletterPlatform.Infrastructure.Services;
using NewsletterPlatform.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NewsletterPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NewsletterPlatformDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgres"));
            if (configuration.GetValue<bool>("Dev:DetailedDbLogging"))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IWorkspaceAuthorization, NewsletterRepository>();
        services.AddScoped<IPublicWorkspaceReader, NewsletterRepository>();
        services.AddScoped<ISubscriberRepository, NewsletterRepository>();
        services.AddScoped<ITagRepository, NewsletterRepository>();
        services.AddScoped<IListRepository, NewsletterRepository>();
        services.AddScoped<IPlanRepository, NewsletterRepository>();
        services.AddScoped<IPaymentRepository, NewsletterRepository>();
        services.AddScoped<IProviderAccountRepository, NewsletterRepository>();
        services.AddScoped<IPostRepository, NewsletterRepository>();
        services.AddScoped<ICampaignRepository, NewsletterRepository>();
        services.AddScoped<IAnalyticsRepository, NewsletterRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IPasswordTokenGenerator, PasswordTokenGenerator>();
        services.AddSingleton<ITokenHasher, TokenHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ISecretProtector, AesSecretProtector>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IEmailSender, DevEmailSender>();
        services.AddScoped<ICampaignDispatcher, CampaignDispatcher>();

        services.Configure<CampaignDispatchWorkerOptions>(configuration.GetSection(CampaignDispatchWorkerOptions.SectionName));
        services.AddHostedService<CampaignDispatchWorker>();

        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NewsletterPlatform.Application.Common.JwtOptions>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NewsletterPlatform.Application.Common.AuthOptions>>().Value);

        return services;
    }
}
