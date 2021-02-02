using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MediaLibrary.Models;

namespace MediaLibrary.Interfaces {

    public interface IApiAccessService {

        Task<int?> SaveApiToken([NotNull] Token token);
    }
}