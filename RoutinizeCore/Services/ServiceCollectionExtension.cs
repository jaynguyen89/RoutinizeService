using Microsoft.Extensions.DependencyInjection;

namespace RoutinizeCore.Services {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterRoutinizeCoreServices(this IServiceCollection services) {
            //Add all services here

            return services;
        }
    }
}