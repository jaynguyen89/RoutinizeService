using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AssistantLibrary.Models;

namespace AssistantLibrary.Interfaces {

    public interface IGoogleRecaptchaService {

        Task<GoogleRecaptchaResponse> IsHumanRegistration([AllowNull] string recaptchaToken = null);
    }
}