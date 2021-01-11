using System.Threading.Tasks;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAuthenticationService {

        Task<bool> IsRegistrationEmailAvailable(string email);

        Task<bool> IsUsernameAvailable(string username);
    }
}