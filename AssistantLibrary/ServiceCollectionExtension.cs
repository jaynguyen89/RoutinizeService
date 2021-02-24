using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AssistantLibrary {

    public static class ServiceCollectionExtension {

        public static IServiceCollection RegisterAssistantLibraryServices(this IServiceCollection services) {
            //Add all services here

            services.AddScoped<IAssistantService, AssistantService>();
            services.AddScoped<IEmailSenderService, EmailSenderService>();
            services.AddScoped<IGoogleRecaptchaService, GoogleRecaptchaService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<IQrCodeMaker, QrCodeMaker>();
            services.AddScoped<IRsaService, RsaService>();

            return services;
        }
    }
}