using HelperLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AssistantLibrary;
using AssistantLibrary.Models;
using MediaLibrary;
using MongoLibrary;
using RoutinizeCore.Services;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore {
    
    public class Startup {
        
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddCors();
            services.AddControllers();
            
            services.AddMvc(options => options.EnableEndpointRouting = false)
                    .AddSessionStateTempDataProvider()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddHttpContextAccessor();

            services.Configure<ApplicationOptions>(Configuration.GetSection(nameof(ApplicationOptions)));
            services.Configure<GoogleRecaptchaOptions>(Configuration.GetSection("RoutinizeSettings"));
            services.Configure<MailServerOptions>(Configuration.GetSection("RoutinizeSettings"));
            services.Configure<QrCodeOptions>(Configuration.GetSection(nameof(QrCodeOptions)));

            services.RegisterHelperServices();
            services.RegisterAssistantLibraryServices();
            services.RegisterRoutinizeCoreServices();

            services.Configure<MongoDbOptions>(Configuration.GetSection(nameof(MongoDbOptions)));
            services.RegisterMongoLibraryServices();
            services.RegisterMediaApiServices();

            services.AddStackExchangeRedisCache(options => {
                options.Configuration = Configuration.GetSection("RedisServer")["Connection"];
                options.InstanceName = Configuration.GetSection("RedisServer")["RoutinizeCache"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors(builder => builder
                                   .AllowAnyHeader()
                                   .AllowAnyMethod()
                                   //.AllowCredentials().WithOrigin("")
                                   .AllowAnyOrigin()
            );

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
