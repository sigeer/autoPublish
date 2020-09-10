using autoPublish.Core.Models;
using autoPublish.Core.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace autoPublish.Web.Model
{
    public class InjectModel
    {
    }

    public static class ProjectInject
    {
        public static IServiceCollection AddRepositoryModel(this IServiceCollection services, IConfiguration configuration)
        {
            var repositoryModel = configuration.GetSection("Repositories").Get<List<RepositoryModel>>();
            services.AddSingleton(repositoryModel);
            services.AddScoped<Project>();
            return services;
        }
    }
}
