using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAccountService {
        
        Task<bool> IsRegistrationEmailAvailable(string email);

        Task<bool> IsUsernameAvailable(string username);

        Task<bool> IsAccountUniqueIdAvailable(string accountUniqueId);

        Task<Account> GetUnactivatedUserAccountByEmail(string email);
        
        Task<bool> UpdateUserAccount(Account userAccount);
    }
}