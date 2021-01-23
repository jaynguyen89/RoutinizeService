using System.Collections.Generic;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAuthenticationService {

        Task<int> InsertNewUserAccount(RegisterAccountVM registrationData, string uniqueId, string activationToken);

        Task<bool> RemoveNewlyInsertedUserAccount(int accountId);

        Task<KeyValuePair<bool, bool?>> ActivateUserAccount(AccountActivationVM activator);

        Task<KeyValuePair<bool, Account>> AuthenticateUserAccount(AuthenticationVM authenticationData);

        Task<bool?> InsertAuthenticationRecord(AuthRecord authRecord);

        Task<AuthRecord> GetLatestAuthRecordForUserAccount(SessionAuthVM sessionAuth);

        Task<Account> GetAccountById(int accountId);
    }
}