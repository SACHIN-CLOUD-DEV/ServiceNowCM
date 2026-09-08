using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Infrastructure.Persistence;
using ServiceNowCM.Infrastructure.Repositories;
using ServiceNowCM.Infrastructure.Security;
using ServiceNowCM.Infrastructure.ServiceNow;

namespace ServiceNowCM.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<ServiceNowCMDbContext>(options =>options.UseSqlServer(configuration.GetConnectionString("ServiceNowCM")));

            services.AddScoped<IServiceNowConnectionRepository, ServiceNowConnectionRepository>();


            services.AddScoped<ISyncFailureRepository, SyncFailureRepository>();
            services.AddScoped<IIntegrationRepository, IntegrationRepository>();

            services.AddScoped<IContentManagerConnectionRepository, ContentManagerConnectionRepository>();

            services.AddScoped<ISyncJobRepository, SyncJobRepository>();

            services.AddDataProtection();

            services.AddScoped<ICredentialStore,DatabaseCredentialStore>();

            services.AddHttpClient<IServiceNowClient,ServiceNowClient> (client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);

            });

            return services;
        }
    }
}
