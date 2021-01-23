using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary.Shared;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AssistantLibrary.Services {

    public class GoogleRecaptchaService : IGoogleRecaptchaService {

        private readonly HttpClient GoogleRecaptchaRequest = new HttpClient();

        private string RecaptchaSecretKey { get; set; }

        public GoogleRecaptchaService(IOptions<GoogleRecaptchaOptions> recaptchaOptions) {
            RecaptchaSecretKey = recaptchaOptions.Value.GoogleRecaptchaSecretKey;
            GoogleRecaptchaRequest.BaseAddress = new Uri(recaptchaOptions.Value.GoogleRecaptchaEndpoint);
            
            GoogleRecaptchaRequest.DefaultRequestHeaders.Accept.Clear();
            GoogleRecaptchaRequest.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<GoogleRecaptchaResponse> IsHumanRegistration(string recaptchaToken = null) {
            if (recaptchaToken == null) return new GoogleRecaptchaResponse { Result = false };

            var response = await GoogleRecaptchaRequest.PostAsJsonAsync(
                $"?secret={ RecaptchaSecretKey }&response={ recaptchaToken }",
                HttpCompletionOption.ResponseContentRead
            );

            var verification = new GoogleRecaptchaResponse();
            if (response.IsSuccessStatusCode)
                verification = JsonConvert.DeserializeObject<GoogleRecaptchaResponse>(await response.Content.ReadAsStringAsync());

            return verification;
        }
    }
}