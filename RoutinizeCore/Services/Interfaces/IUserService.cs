using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IUserService {

        Task<int?> InsertBlankUserOnAccountRegistration(int accountId);
    }
}