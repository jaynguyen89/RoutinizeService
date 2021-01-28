using Microsoft.Extensions.DependencyInjection;
using MongoLibrary.Interfaces;
using MongoLibrary.Services;

namespace MongoLibrary {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterMongoLibraryServices(this IServiceCollection services) {
            //Add all services here
            services.AddScoped<MongoDbContext>();

            services.AddScoped<IRoutinizeCoreLogService, RoutinizeCoreLogService>();
            services.AddScoped<IRoutinizeAccountLogService, RoutinizeAccountLogService>();

            return services;
        }
    }
}