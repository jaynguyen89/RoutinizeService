using System;
using System.Collections.Generic;
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

    public sealed class TodoService : ITodoService {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public TodoService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
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

        public async Task<bool?> IsTodoGroupCreatedByThisUser(int userId, int todoGroupId) {
            try {
                return await _dbContext.ContentGroups.AnyAsync(
                    group => group.Id == todoGroupId &&
                             group.CreatedById == userId &&
                             group.GroupOfType.Equals(nameof(Todo))
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(IsTodoGroupCreatedByThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroup with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(todoGroupId) }) = ({ userId }, { todoGroupId })",
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

        public async Task<bool?> SetTodoDeletedOnById([NotNull] int todoId) {
            try {
                var todo = await _dbContext.Todos.FindAsync(todoId);
                if (todo == null) return null;
                
                todo.DeletedOn = DateTime.UtcNow;
                _dbContext.Todos.Update(todo);
                
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(SetTodoDeletedOnById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Todos.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoId) } = { todoId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> SetTodoGroupDeletedOnById([NotNull] int todoGroupId) {
            try {
                var todoGroup = await _dbContext.ContentGroups.SingleOrDefaultAsync(
                    group => group.Id == todoGroupId && group.GroupOfType.Equals(nameof(Todo))
                );
                if (todoGroup == null) return null;
                
                todoGroup.DeletedOn = DateTime.UtcNow;
                _dbContext.ContentGroups.Update(todoGroup);

                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(SetTodoGroupDeletedOnById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to ContentGroups.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoGroupId) } = { todoGroupId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(SetTodoGroupDeletedOnById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroup with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoGroupId) } = { todoGroupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(SetTodoGroupDeletedOnById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching TodoGroup with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoGroupId) } = { todoGroupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteTodoGroupById([NotNull] int todoGroupId) {
            try {
                var todoGroup = await _dbContext.ContentGroups.SingleOrDefaultAsync(
                    group => group.Id == todoGroupId && group.GroupOfType.Equals(nameof(Todo))
                );
                if (todoGroup == null) return null;

                _dbContext.ContentGroups.Remove(todoGroup);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(DeleteTodoGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to ContentGroups.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoGroupId) } = { todoGroupId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(DeleteTodoGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroup with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoGroupId) } = { todoGroupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(DeleteTodoGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching TodoGroup with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoGroupId) } = { todoGroupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
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

        public async Task<KeyValuePair<int, ContentGroup>[]> GetPersonalActiveTodoGroups(int userId) {
            try {
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => !group.IsShared &&
                                                    !group.DeletedOn.HasValue &&
                                                    group.GroupOfType.Equals(nameof(Todo)) &&
                                                    group.CreatedById == userId
                                       )
                                       .Select(
                                           group => new KeyValuePair<int, ContentGroup>(
                                               group.Todos.Count,
                                               new ContentGroup {
                                                   Id = group.Id,
                                                   Description = group.Description,
                                                   GroupName = group.GroupName,
                                                   GroupOfType = group.GroupOfType,
                                                   CreatedById = group.CreatedById,
                                                   CreatedOn = group.CreatedOn
                                               }
                                           )
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetPersonalActiveTodoGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<KeyValuePair<int, ContentGroup>[]> GetPersonalArchivedTodoGroups(int userId) {
            try {
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => !group.IsShared &&
                                                    group.DeletedOn.HasValue &&
                                                    group.GroupOfType.Equals(nameof(Todo)) &&
                                                    group.CreatedById == userId
                                       )
                                       .Select(
                                           group => new KeyValuePair<int, ContentGroup>(
                                               group.Todos.Count,
                                               new ContentGroup {
                                                   Id = group.Id,
                                                   Description = group.Description,
                                                   GroupName = group.GroupName,
                                                   GroupOfType = group.GroupOfType,
                                                   CreatedById = group.CreatedById,
                                                   CreatedOn = group.CreatedOn
                                               }
                                           )
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetPersonalArchivedTodoGroups) }",
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
                var todoGroupIdsSharedToUser = await GetTodoGroupIdsSharedByUser(userId);
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
                var todoGroupIdsSharedToUser = await GetTodoGroupIdsSharedByUser(userId);
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
                var todoGroupIdsSharedToUser = await GetTodoGroupIdsSharedByUser(userId);
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

        public async Task<KeyValuePair<int, ContentGroup>[]> GetSharedActiveTodoGroups(int userId) {
            try {
                var todoGroupIdsSharedToUser = await GetTodoGroupIdsSharedByUser(userId);
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => group.IsShared &&
                                                    group.GroupOfType.Equals(nameof(Todo)) &&
                                                    todoGroupIdsSharedToUser.Contains(group.Id) &&
                                                    !group.DeletedOn.HasValue
                                       )
                                       .Select(group => new KeyValuePair<int, ContentGroup>(
                                               group.Todos.Count,
                                               new ContentGroup {
                                                   Id = group.Id,
                                                   Description = group.Description,
                                                   GroupName = group.GroupName,
                                                   GroupOfType = group.GroupOfType,
                                                   CreatedById = group.CreatedById,
                                                   CreatedOn = group.CreatedOn
                                               }
                                           )
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetSharedActiveTodoGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<KeyValuePair<int, ContentGroup>[]> GetSharedArchivedTodoGroups(int userId) {
            try {
                var todoGroupIdsSharedToUser = await GetTodoGroupIdsSharedByUser(userId);
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => group.IsShared &&
                                                    group.GroupOfType.Equals(nameof(Todo)) &&
                                                    todoGroupIdsSharedToUser.Contains(group.Id) &&
                                                    group.DeletedOn.HasValue
                                       )
                                       .Select(group => new KeyValuePair<int, ContentGroup>(
                                               group.Todos.Count,
                                               new ContentGroup {
                                                   Id = group.Id,
                                                   Description = group.Description,
                                                   GroupName = group.GroupName,
                                                   GroupOfType = group.GroupOfType,
                                                   CreatedById = group.CreatedById,
                                                   CreatedOn = group.CreatedOn
                                               }
                                           )
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(TodoService) }.{ nameof(GetSharedArchivedTodoGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<Todo[]> GetTodosForContentGroupById(int groupId) {
            try {
                return await _dbContext.Todos
                                       .Where(
                                           todo => todo.GroupId.HasValue &&
                                                   todo.GroupId.Value == groupId &&
                                                   (todo.Group.DeletedOn.HasValue || !todo.DeletedOn.HasValue)
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(TodoService) }.{ nameof(GetTodoGroupIdsSharedByUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroups IDs with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
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

        private async Task<int[]> GetTodoGroupIdsSharedByUser(int userId) {
            try {
                return await _dbContext.CollaboratorTasks
                                       .Where(
                                           task => task.Collaboration.CollaboratorId == userId &&
                                                   task.TaskType.Equals($"{nameof(ContentGroup)}.{nameof(Todo)}")
                                       )
                                       .Select(task => task.TaskId)
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(TodoService) }.{ nameof(GetTodoGroupIdsSharedByUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroups IDs with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }
    }
}