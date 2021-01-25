using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAccountService {
        
        Task<bool> IsRegistrationEmailAvailable(string email);

        Task<bool> IsUsernameAvailable(string username);

        Task<bool> IsAccountUniqueIdAvailable(string accountUniqueId);

        Task<Account> GetUserAccountByEmail(string email, bool activated = false);
        
        Task<bool> UpdateUserAccount(Account userAccount);

        Task<Account> GetUserAccountById(int accountId, bool activated = true);

        Task<Account> GetUserAccountByUsername(string username, bool activated = false);
    }
}