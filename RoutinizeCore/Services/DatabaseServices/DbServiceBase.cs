using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.DatabaseServices {

    public class DbServiceBase {
        
        protected readonly IRoutinizeCoreLogService _coreLogService;
        protected readonly RoutinizeDbContext _dbContext;

        protected DbServiceBase(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Use in a combination with CommitChanges.
        /// </summary>
        protected async Task SetChangesToDbContext(object any, string task = SharedConstants.TASK_INSERT) {
            switch (task) {
                case SharedConstants.TASK_INSERT:
                    await _dbContext.AddAsync(any);
                    break;
                case SharedConstants.TASK_UPDATE:
                    _dbContext.Update(any);
                    break;
                default:
                    _dbContext.Remove(any);
                    break;
            }
        }

        /// <summary>
        /// Use in a combination with SetChangesToDbContext.
        /// </summary>
        protected async Task<bool?> CommitChanges() {
            try {
                return (await _dbContext.SaveChangesAsync()) != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(DbServiceBase) }.{ nameof(CommitChanges) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while saving bulk changes to DbContext.\n\n{ e.StackTrace }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        /// <summary>
        /// Caution: make sure you know what you do when using this method
        /// </summary>
        protected void ToggleTransactionAuto(bool auto = true) {
            _dbContext.Database.AutoTransactionsEnabled = auto;
        }

        /// <summary>
        /// StartTransaction, CommitTransaction and RevertTransaction must be used together.
        /// </summary>
        protected async Task StartTransaction() {
            await _dbContext.Database.BeginTransactionAsync();
        }
        
        /// <summary>
        /// StartTransaction, CommitTransaction and RevertTransaction must be used together.
        /// </summary>
        protected async Task CommitTransaction() {
            await _dbContext.Database.CommitTransactionAsync();
        }
        
        /// <summary>
        /// StartTransaction, CommitTransaction and RevertTransaction must be used together.
        /// </summary>
        protected async Task RevertTransaction() {
            await _dbContext.Database.RollbackTransactionAsync();
        }
        
        /// <summary>
        /// Execute an on-demand query against the database.
        /// </summary>
        protected async Task ExecuteRawOn<T>([NotNull] string query) {
            await _dbContext.Database.ExecuteSqlRawAsync(query);
        }
        
        protected async Task<int[]> GetGroupIdsSharedToUserForType<T>(int userId) {
            try {
                return await _dbContext.CollaboratorTasks
                                       .Where(
                                           task => task.Collaboration.CollaboratorId == userId &&
                                                   task.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(T) }")
                                       )
                                       .Select(task => task.TaskId)
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(TodoService) }.{ nameof(GetGroupIdsSharedToUserForType) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroups IDs with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, T) = ({ userId }, { nameof(T) })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }
    }
}