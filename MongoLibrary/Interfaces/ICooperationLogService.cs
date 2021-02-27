using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MongoLibrary.Interfaces {

    public interface ICooperationLogService {

        Task<bool> InsertCooperationParticipantLog<T>([NotNull] T data);
    }
}