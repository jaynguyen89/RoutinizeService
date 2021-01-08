using Microsoft.Extensions.DependencyInjection;
using RoutinizeCore.Services.ApplicationServices.CacheService;

namespace RoutinizeCore.Services {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterRoutinizeCoreServices(this IServiceCollection services) {
            //Add all services here

            services.AddSingleton<RoutinizeMemoryCache>();

            return services;
        }
    }
}