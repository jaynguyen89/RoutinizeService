using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IUserService {

        Task<int?> InsertBlankUserOnAccountRegistration([NotNull] int accountId);
    }
}