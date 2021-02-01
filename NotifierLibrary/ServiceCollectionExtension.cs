using Microsoft.Extensions.DependencyInjection;

namespace NotifierLibrary {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterMongoLibraryServices(this IServiceCollection services) {
            //Add all services here

            return services;
        }
    }
}