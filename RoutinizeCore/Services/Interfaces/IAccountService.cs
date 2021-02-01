using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAccountService {
        
        Task<bool> IsRegistrationEmailAvailable([NotNull] string email);

        Task<bool> IsUsernameAvailable([NotNull] string username);

        Task<bool> IsAccountUniqueIdAvailable([NotNull] string accountUniqueId);

        Task<Account> GetUserAccountByEmail([NotNull] string email, bool activated = false);
        
        Task<bool> UpdateUserAccount([NotNull] Account userAccount);

        Task<Account> GetUserAccountById([NotNull] int accountId, bool activated = true);

        Task<Account> GetUserAccountByUsername([NotNull] string username, bool activated = false);

        Task<bool?> SaveFcmToken([NotNull] int accountId, [NotNull] string fcmToken);
    }
}