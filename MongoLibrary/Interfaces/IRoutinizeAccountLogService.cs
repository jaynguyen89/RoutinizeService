using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MongoLibrary.Interfaces {

    public interface IRoutinizeAccountLogService {

        Task<bool> InsertRoutinizeAccountLog<T>([NotNull] T data);

        Task<List<object>> GetRoutinizeAccountLogInRange([NotNull] int start,[NotNull] int end);

        Task<T> GetRoutinizeAccountLogByAccountIdFor<T>([NotNull] int id,[NotNull] string activity,[NotNull] bool completed);

        Task<bool> RemoveAccountLogEntry<T>([NotNull] T entry);
    }
}