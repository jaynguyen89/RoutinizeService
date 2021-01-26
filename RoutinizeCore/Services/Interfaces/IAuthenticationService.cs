using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAuthenticationService {

        Task<int> InsertNewUserAccount([NotNull] RegisterAccountVM registrationData,[NotNull] string uniqueId,[NotNull] string activationToken);

        Task<bool> RemoveNewlyInsertedUserAccount([NotNull] int accountId);

        Task<KeyValuePair<bool, bool?>> ActivateUserAccount([NotNull] AccountActivationVM activator);

        Task<KeyValuePair<bool, Account>> AuthenticateUserAccount([NotNull] AuthenticationVM authenticationData);

        Task<bool?> InsertAuthenticationRecord([NotNull] AuthRecord authRecord);

        Task<AuthRecord> GetLatestAuthRecordForUserAccount([NotNull] SessionAuthVM sessionAuth);

        Task<Account> GetAccountById([NotNull] int accountId);
    }
}