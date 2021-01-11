using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;

namespace AssistantLibrary.Services {

    public class GoogleRecaptchaService : IGoogleRecaptchaService {

        public Task<GoogleRecaptchaResponse> IsHumanRegistration(string recaptchaToken = null) {
            throw new System.NotImplementedException();
        }
    }
}