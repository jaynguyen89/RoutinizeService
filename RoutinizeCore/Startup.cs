using System;
using HelperLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoLibrary;
using RoutinizeCore.Services;
using RoutinizeCore.Services.ApplicationServices.CacheService;

namespace RoutinizeCore {
    
    public class Startup {
        
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            services.AddMvc(options => options.EnableEndpointRouting = false)
                    .AddSessionStateTempDataProvider()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(double.Parse(Configuration.GetSection("Session")["IdleTimeout"]));
                options.Cookie.IsEssential = bool.Parse(Configuration.GetSection("Session")["RequireCookie"]);
            });

            services.AddHttpContextAccessor();

            services.RegisterHelperServices();
            services.RegisterRoutinizeCoreServices();

            services.Configure<MongoDbOptions>(Configuration.GetSection("MongoServer"));
            services.RegisterMongoLibraryServices();

            services.AddStackExchangeRedisCache(options => {
                options.Configuration = Configuration.GetSection("RedisServer")["Connection"];
                options.InstanceName = Configuration.GetSection("RedisServer")["RoutinizeCache"];
            });
            
            services.Configure<CacheOptions>(Configuration.GetSection("MemoryCache"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}"
                );
            });
        }
    }
}
