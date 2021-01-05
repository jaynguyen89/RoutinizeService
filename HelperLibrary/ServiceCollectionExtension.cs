using Microsoft.Extensions.DependencyInjection;

namespace HelperLibrary {
    
    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterHelperServices(this IServiceCollection services) {
            //Add all services here

            return services;
        }
    }
}
