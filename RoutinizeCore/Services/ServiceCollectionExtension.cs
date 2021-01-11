using Microsoft.Extensions.DependencyInjection;
using RoutinizeCore.Services.ApplicationServices.CacheService;
using RoutinizeCore.Services.DatabaseServices;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterRoutinizeCoreServices(this IServiceCollection services) {
            //Add all services here

            services.AddSingleton<RoutinizeMemoryCache>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}