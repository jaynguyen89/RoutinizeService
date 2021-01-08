using System.Collections.Generic;
using System.Threading.Tasks;
using MongoLibrary.Models;

namespace MongoLibrary.Interfaces {

    public interface IRoutinizeCoreLogService {

        Task InsertRoutinizeCoreLog(RoutinizeCoreLog log);

        Task<List<RoutinizeCoreLog>> GetRoutinizeCoreLogInRange(int start, int end);
    }
}