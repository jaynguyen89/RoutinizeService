using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoLibrary.Interfaces {

    public interface IRoutinizeAccountLogService {

        Task<bool> InsertRoutinizeAccountLog<T>(T data);

        Task<List<object>> GetRoutinizeAccountLogInRange(int start, int end);

        Task<T> GetRoutinizeAccountLogByAccountIdFor<T>(int id, string activity, bool completed);

        Task<bool> RemoveAccountLogEntry<T>(T entry);
    }
}