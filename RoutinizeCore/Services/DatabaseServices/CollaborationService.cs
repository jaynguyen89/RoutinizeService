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
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using static HelperLibrary.Shared.SharedEnums;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class CollaborationService : ICollaborationService {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public CollaborationService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }


        public async Task<int?> DoesUserHasThisCollaborator([NotNull] int userId,[NotNull] int collaboratorId) {
            try {
                var collaboration = await _dbContext.Collaborations.SingleOrDefaultAsync(
                    collab =>
                        (collab.UserId == userId && collab.CollaboratorId == collaboratorId && collab.IsAccepted) ||
                        (collab.UserId == collaboratorId && collab.CollaboratorId == userId && collab.IsAccepted)
                );

                return collaboration?.Id ?? -1;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(DoesUserHasThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting user by SingleOrDefault due to Null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(collaboratorId) }) = ({ userId }, { collaboratorId })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewCollaboratorTask([NotNull] CollaboratorTask task) {
            try {
                await _dbContext.CollaboratorTasks.AddAsync(task);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : task.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(InsertNewCollaboratorTask) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Users.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(task) } = { JsonConvert.SerializeObject(task) }",
                    Severity = LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsTodoAssociatedWithThisCollaborator(
            [NotNull] int userId,[NotNull] int todoId, Permissions permission = Permissions.Edit
        ) {
            try {
                var todo = await _dbContext.Todos.FindAsync(todoId);
                if (todo == null) return null;
                if (!todo.IsShared) return false;

                var contentGroupIdContainingTodo = await _dbContext.ContentGroups
                                                                   .Where(group => group.Id == todo.GroupId && group.GroupOfType.Equals(nameof(Todo)))
                                                                   .Select(group => group.Id)
                                                                   .FirstOrDefaultAsync();

                var hasAccessToTodoGroup = await IsTodoGroupAssociatedWithThisCollaborator(userId, contentGroupIdContainingTodo, permission);
                if (!hasAccessToTodoGroup.HasValue) return null;

                var isAssociatedWithTodoItem = await _dbContext.CollaboratorTasks
                                                                     .AnyAsync(
                                                                         task => task.TaskId == todoId &&
                                                                                 (byte) permission <= task.Permission &&
                                                                                 task.Collaboration.CollaboratorId == userId
                                                                     );
                
                return hasAccessToTodoGroup.Value || isAssociatedWithTodoItem;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(IsTodoAssociatedWithThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error in lambda Join-Where-AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(todoId) }) = ({ userId }, { todoId })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsTodoGroupAssociatedWithThisCollaborator(int userId, int todoGroupId, Permissions permission = SharedEnums.Permissions.Edit) {
            try {
                return await _dbContext.CollaboratorTasks.AnyAsync(
                    task => task.Collaboration.CollaboratorId == userId &&
                            task.Collaboration.IsAccepted &&
                            task.TaskId == todoGroupId &&
                            task.TaskType.Equals($"{nameof(ContentGroup)}.{nameof(Todo)}") &&
                            (byte) permission <= task.Permission
                    );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(IsTodoGroupAssociatedWithThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error in lambda Join-Where-AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(todoGroupId) }) = ({ userId }, { todoGroupId })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}