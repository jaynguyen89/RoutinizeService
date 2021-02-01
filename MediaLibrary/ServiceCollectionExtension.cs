using MediaLibrary.DbContexts;
using MediaLibrary.Interfaces;
using MediaLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediaLibrary {
    
    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterMediaApiServices(
            this IServiceCollection services
        ) {
            //Register all services here
            services.AddScoped<MediaDbContext>();

            services.AddScoped<IAvatarService, AvatarService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPhotoService, PhotoService>();

            return services;
        }
    }
}