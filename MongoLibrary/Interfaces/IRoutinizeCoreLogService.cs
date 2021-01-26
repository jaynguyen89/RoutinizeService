using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MongoLibrary.Models;

namespace MongoLibrary.Interfaces {

    public interface IRoutinizeCoreLogService {

        Task InsertRoutinizeCoreLog([NotNull] RoutinizeCoreLog log);

        Task<List<RoutinizeCoreLog>> GetRoutinizeCoreLogInRange([NotNull] int start,[NotNull] int end);
    }
}