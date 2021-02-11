using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAddressService : IDbServiceBase {

        Task<int?> SaveNewAddress(Address address);

        Task<bool?> UpdateAddress(Address address);

        Task<bool?> RemoveAddress(int addressId);
    }
}