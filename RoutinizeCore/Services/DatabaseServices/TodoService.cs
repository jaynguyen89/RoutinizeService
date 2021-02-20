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

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class TodoService : DbServiceBase, ITodoService {

        public TodoService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) : base(coreLogService, dbContext) { }
        
        public new async Task SetChangesToDbContext(object any, string task = SharedConstants.TaskInsert) {
            await base.SetChangesToDbContext(any, task);
        }

        public new async Task<bool?> CommitChanges() {
            return await base.CommitChanges();
        }

        public new void ToggleTransactionAuto(bool auto = true) {
            base.ToggleTransactionAuto(auto);
        }

        public new async Task StartTransaction() {
            await base.StartTransaction();
        }

        public new async Task CommitTransaction() {
            await base.CommitTransaction();
        }

        public new async Task RevertTransaction() {
            await base.RevertTransaction();
        }

        public new async Task ExecuteRawOn<T>(string query) {
            await base.ExecuteRawOn<T>(query);
        }

        public async Task<int?> InsertNewTodo([NotNull] Todo todo) {
            try {
                await _dbContext.Todos.AddAsync(todo);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : todo.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(InsertNewTodo) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to Todos.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todo) } = { JsonConvert.SerializeObject(todo) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool> UpdateTodo([NotNull] Todo todo) {
            try {
                _dbContext.Todos.Update(todo);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(UpdateTodo) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to TodoGroups.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todo) } = { JsonConvert.SerializeObject(todo) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool?> IsTodoCreatedByThisUser([NotNull] int userId,[NotNull] int todoId) {
            try {
                return await _dbContext.Todos.AnyAsync(todo => todo.Id == todoId && todo.UserId == userId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(IsTodoCreatedByThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todo with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(todoId) }) = ({ userId }, { todoId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Todo> GetTodoById([NotNull] int todoId) {
            try {
                return await _dbContext.Todos.FindAsync(todoId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(GetTodoById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting user by SingleOrDefault due to Null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoId) } = { todoId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteTodoById([NotNull] int todoId) {
            try {
                var todo = await _dbContext.Todos.FindAsync(todoId);
                if (todo == null) return null;

                _dbContext.Todos.Remove(todo);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(DeleteTodoById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting entry from Todos.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoId) } = { todoId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> SetTodoAsDoneById([NotNull] int todoId,[NotNull] int doneById) {
            try {
                var todo = await _dbContext.Todos.FindAsync(todoId);
                if (todo == null) return null;

                todo.DoneById = doneById;
                todo.ActuallyDoneOn = DateTime.UtcNow;

                _dbContext.Todos.Update(todo);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(SetTodoAsDoneById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Todos.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(todoId) }.{ doneById }) = ({ todoId }.{ doneById })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> SetTodoAsUndoneById([NotNull] int todoId) {
            try {
                var todo = await _dbContext.Todos.FindAsync(todoId);
                if (todo == null) return null;

                todo.DoneById = null;
                todo.ActuallyDoneOn = null;

                _dbContext.Todos.Update(todo);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(SetTodoAsUndoneById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Todos.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoId) } = { todoId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Todo[]> GetPersonalActiveTodos(int userId) {
            try {
                return await _dbContext.Todos
                                       .Where(
                                           todo => (!todo.GroupId.HasValue || !todo.Group.DeletedOn.HasValue) &&
                                                   !todo.DoneById.HasValue &&
                                                   !todo.ActuallyDoneOn.HasValue &&
                                                   !todo.DeletedOn.HasValue &&
                                                   !todo.IsShared &&
                                                   todo.UserId == userId
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetPersonalActiveTodos) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<Todo[]> GetPersonalDoneTodos(int userId) {
            try {
                return await _dbContext.Todos
                                       .Where(
                                           todo => (!todo.GroupId.HasValue || !todo.Group.DeletedOn.HasValue) &&
                                                   todo.DoneById.HasValue &&
                                                   todo.ActuallyDoneOn.HasValue &&
                                                   !todo.DeletedOn.HasValue &&
                                                   !todo.IsShared &&
                                                   todo.UserId == userId
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetPersonalDoneTodos) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<Todo[]> GetPersonalArchivedTodos(int userId) {
            try {
                return await _dbContext.Todos.Where(
                    todo => (
                                (!todo.GroupId.HasValue && todo.DeletedOn.HasValue) ||
                                (todo.GroupId.HasValue && todo.Group.DeletedOn.HasValue)
                            ) &&
                            !todo.IsShared &&
                            todo.UserId == userId
                )
                .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetPersonalArchivedTodos) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<Todo[]> GetSharedActiveTodos(int userId) {
            try {
                var todoGroupIdsSharedToUser = await GetGroupIdsSharedToUserForType<Todo>(userId);
                return await _dbContext.Todos
                                       .Where(
                                           todo => todo.GroupId.HasValue &&
                                                   todo.Group.IsShared &&
                                                   todoGroupIdsSharedToUser.Contains(todo.GroupId.Value) &&
                                                   !todo.DeletedOn.HasValue &&
                                                   !todo.DoneById.HasValue &&
                                                   !todo.ActuallyDoneOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetSharedActiveTodos) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<Todo[]> GetSharedDoneTodos(int userId) {
            try {
                var todoGroupIdsSharedToUser = await GetGroupIdsSharedToUserForType<Todo>(userId);
                return await _dbContext.Todos
                                       .Where(
                                           todo => todo.GroupId.HasValue &&
                                                   todo.Group.IsShared &&
                                                   todoGroupIdsSharedToUser.Contains(todo.GroupId.Value) &&
                                                   !todo.DeletedOn.HasValue &&
                                                   todo.DoneById.HasValue &&
                                                   todo.ActuallyDoneOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetSharedDoneTodos) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<Todo[]> GetSharedArchivedTodos(int userId) {
            try {
                var todoGroupIdsSharedToUser = await GetGroupIdsSharedToUserForType<Todo>(userId);
                return await _dbContext.Todos
                                       .Where(
                                           todo => todo.GroupId.HasValue &&
                                                   todo.Group.IsShared &&
                                                   todoGroupIdsSharedToUser.Contains(todo.GroupId.Value) &&
                                                   todo.DeletedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetSharedArchivedTodos) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<bool?> IsTodoSharedToAnyoneElseExceptThisCollaborator([NotNull] int collaboratorId,[NotNull] int todoId,[NotNull] int ownerId) {
            try {
                var collaborationIdHavingThisCollaborator = await _dbContext.Collaborations
                                                                            .Where(
                                                                                collaboration => collaboration.CollaboratorId == collaboratorId &&
                                                                                                 collaboration.UserId == ownerId &&
                                                                                                 collaboration.IsAccepted
                                                                            )
                                                                            .Select(collaboration => collaboration.Id)
                                                                            .SingleOrDefaultAsync();
                
                var isSharedDirectlyToOtherCollaborators = await _dbContext.CollaboratorTasks
                                                                           .AnyAsync(
                                                                               task => task.CollaborationId != collaborationIdHavingThisCollaborator &&
                                                                                       task.Id == todoId &&
                                                                                       task.TaskType.Equals(nameof(Todo))
                                                                            );
                if (isSharedDirectlyToOtherCollaborators) return true;

                var groupIdHavingThisTodo = await _dbContext.Todos
                                                            .Where(todo => todo.GroupId.HasValue && todo.Id == todoId)
                                                            .Select(todo => todo.GroupId)
                                                            .SingleOrDefaultAsync();

                var isSharedInGroupToOtherCollaborators = await _dbContext.GroupShares
                                                                          .AnyAsync(
                                                                              groupShare => groupShare.GroupId == groupIdHavingThisTodo &&
                                                                                            groupShare.SharedToType.Equals(nameof(Collaboration)) &&
                                                                                            groupShare.SharedToId != collaborationIdHavingThisCollaborator
                                                                          );
                if (isSharedInGroupToOtherCollaborators) return true;

                var isSharedDirectlyToATeam = await _dbContext.TeamTasks.AnyAsync(teamTask => teamTask.TaskId == todoId && teamTask.TaskType.Equals(nameof(Todo)));
                if (isSharedDirectlyToATeam) return true;

                var isSharedDirectlyToAProjectIteration = await _dbContext.IterationTasks.AnyAsync(task => task.TaskId == todoId && task.TaskType.Equals(nameof(Todo)));
                if (isSharedDirectlyToAProjectIteration) return true;

                var isSharedInGroupToATeam = await _dbContext.TeamTasks
                                                             .AnyAsync(
                                                                 teamTask => teamTask.TaskId == groupIdHavingThisTodo &&
                                                                             teamTask.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(Todo) }")
                                                             );
                if (isSharedInGroupToATeam) return true;

                var isSharedInGroupToAProjectIteration = await _dbContext.IterationTasks
                                                                         .AnyAsync(
                                                                             iterationTask => iterationTask.TaskId == groupIdHavingThisTodo &&
                                                                                              iterationTask.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(Todo) }")
                                                                         );
                return isSharedInGroupToAProjectIteration;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(TodoService) }.{ nameof(IsTodoSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(todoId) }, { nameof(ownerId) }) = ({ collaboratorId }, { todoId }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(IsTodoSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching Todo with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(todoId) }, { nameof(ownerId) }) = ({ collaboratorId }, { todoId }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<User> GetTodoOwnerFor(int itemId) {
            try {
                return await _dbContext.Todos
                                       .Where(todo => todo.Id == itemId)
                                       .Select(todo => todo.User)
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(TodoService) }.{ nameof(GetTodoOwnerFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroups IDs with Where-Select.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(itemId) } = { itemId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetTodoOwnerFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching Todo with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(itemId) } = { itemId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}