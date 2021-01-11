using System.Threading.Tasks;
using AssistantLibrary.Models;

namespace AssistantLibrary.Interfaces {

    public interface IGoogleRecaptchaService {

        Task<GoogleRecaptchaResponse> IsHumanRegistration(string recaptchaToken = null);
    }
}