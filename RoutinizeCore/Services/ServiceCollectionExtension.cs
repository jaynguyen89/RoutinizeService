using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Services.ApplicationServices.CacheService;
using RoutinizeCore.Services.DatabaseServices;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterRoutinizeCoreServices(this IServiceCollection services) {
            //Add all services here
            services.AddScoped<RoutinizeDbContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IRoutinizeMemoryCache, RoutinizeMemoryCache>();
            services.AddScoped<IRoutinizeRedisCache, RoutinizeRedisCache>();
            
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChallengeService, ChallengeService>();
            services.AddScoped<IAddressService, AddressService>();

            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<ICollaborationService, CollaborationService>();
            services.AddScoped<IContentGroupService, ContentGroupService>();

            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IDepartmentRoleService, DepartmentRoleService>();
            
            services.AddScoped<ITaskRelationService, TaskRelationService>();

            return services;
        }
    }
}