using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary.Shared;

namespace RoutinizeCore.Services.Interfaces {

    public interface IDbServiceBase {

        /// <summary>
        /// Use in a combination with CommitChanges.
        /// </summary>
        Task SetChangesToDbContext(object any, string task = SharedConstants.TASK_INSERT);

        /// <summary>
        /// Use in a combination with SetChangesToDbContext.
        /// </summary>
        Task<bool?> CommitChanges();

        /// <summary>
        /// Caution: make sure you know what you do when using this method
        /// </summary>
        void ToggleTransactionAuto(bool auto = true);

        /// <summary>
        /// StartTransaction, CommitTransaction and RevertTransaction must be used together.
        /// </summary>
        Task StartTransaction();

        /// <summary>
        /// StartTransaction, CommitTransaction and RevertTransaction must be used together.
        /// </summary>
        Task CommitTransaction();

        /// <summary>
        /// StartTransaction, CommitTransaction and RevertTransaction must be used together.
        /// </summary>
        Task RevertTransaction();

        /// <summary>
        /// Execute an on-demand query against the database.
        /// </summary>
        Task ExecuteRawOn<T>([NotNull] string query);
    }
}