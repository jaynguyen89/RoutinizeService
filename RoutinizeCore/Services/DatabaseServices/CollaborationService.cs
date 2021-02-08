using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Collaboration;
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
                    DetailedInformation = $"Error while getting User by SingleOrDefault due to Null argument.\n\n{ e.StackTrace }",
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
                    DetailedInformation = $"Error while updating entry to CollaboratorTasks.\n\n{ e.StackTrace }",
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

        public async Task<bool?> IsTodoGroupAssociatedWithThisCollaborator(int userId, int todoGroupId, Permissions permission = Permissions.Edit) {
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

        public async Task<bool?> InsertNewCollaboratorRequest(Collaboration collaboration) {
            try {
                await _dbContext.Collaborations.AddAsync(collaboration);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(InsertNewCollaboratorRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to Collaborations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(collaboration) } = { JsonConvert.SerializeObject(collaboration) }",
                    Severity = LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Collaboration> GetCollaborationRequest(int collaborationId, int accountId, bool forRequester = false) {
            try {
                var user = await _dbContext.Users.SingleOrDefaultAsync(aUser => aUser.AccountId == accountId);
                if (user == null) return null;
                
                return await _dbContext.Collaborations.SingleOrDefaultAsync(
                    collaboration => collaboration.Id == collaborationId &&
                                     (forRequester ? collaboration.UserId == user.Id : collaboration.CollaboratorId == user.Id)
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetCollaborationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaboration with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaborationId) }, { nameof(accountId) }, { nameof(forRequester) }) = ({ collaborationId }, { accountId }, { forRequester })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(GetCollaborationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching Collaboration with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaborationId) }, { nameof(accountId) }, { nameof(forRequester) }) = ({ collaborationId }, { accountId }, { forRequester })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DoTheyHavePendingOrAcceptedCollaborationRequest(int accountId, string collaboratorUniqueId) {
            try {
                var requesterId = await _dbContext.Users.Where(user => user.AccountId == accountId).Select(user => user.Id).SingleOrDefaultAsync();
                var requesteeId = await _dbContext.Users
                                                .Where(user => user.Account.UniqueId.Equals(collaboratorUniqueId.ToUpper()))
                                                .Select(user => user.Id)
                                                .SingleOrDefaultAsync();
                
                return await _dbContext.Collaborations
                                       .AnyAsync(
                                           collaboration => ((collaboration.UserId == requesterId && collaboration.CollaboratorId == requesteeId) ||
                                                            (collaboration.UserId == requesteeId && collaboration.CollaboratorId == requesterId)) &&
                                                            !collaboration.RejectedOn.HasValue
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(DoTheyHavePendingOrAcceptedCollaborationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaboration with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(accountId) }, { nameof(collaboratorUniqueId) }) = ({ accountId }, { collaboratorUniqueId })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(DoTheyHavePendingOrAcceptedCollaborationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching User with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(accountId) }, { nameof(collaboratorUniqueId) }) = ({ accountId }, { collaboratorUniqueId })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateCollaboration(Collaboration collaboration) {
            try {
                _dbContext.Collaborations.Update(collaboration);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(UpdateCollaboration) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Collaborations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(collaboration) } = { JsonConvert.SerializeObject(collaboration) }",
                    Severity = LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Collaboration[]> GetPendingCollaborationRequests(int accountId, bool asRequester = true) {
            try {
                var user = await _dbContext.Users.SingleOrDefaultAsync(aUser => aUser.AccountId == accountId);
                if (user == null) return null;

                return await _dbContext.Collaborations
                                       .Where(
                                           collaboration => (asRequester ? collaboration.UserId == user.Id : collaboration.CollaboratorId == user.Id) &&
                                                            !collaboration.IsAccepted &&
                                                            !collaboration.RejectedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetPendingCollaborationRequests) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaborations with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(accountId) }, { nameof(asRequester) }) = ({ accountId }, { asRequester })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Collaboration[]> GetAcceptedCollaborationRequests(int accountId, bool asRequester = true) {
            try {
                var user = await _dbContext.Users.SingleOrDefaultAsync(aUser => aUser.AccountId == accountId);
                if (user == null) return null;

                return await _dbContext.Collaborations
                                       .Where(
                                           collaboration => (asRequester ? collaboration.UserId == user.Id : collaboration.CollaboratorId == user.Id) &&
                                                            collaboration.IsAccepted
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetAcceptedCollaborationRequests) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaborations with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(accountId) }, { nameof(asRequester) }) = ({ accountId }, { asRequester })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Collaboration[]> GetRejectedCollaborationRequests(int accountId, bool asRequester = true) {
            try {
                var user = await _dbContext.Users.SingleOrDefaultAsync(aUser => aUser.AccountId == accountId);
                if (user == null) return null;

                return await _dbContext.Collaborations
                                       .Where(
                                           collaboration => (asRequester ? collaboration.UserId == user.Id : collaboration.CollaboratorId == user.Id) &&
                                                            !collaboration.IsAccepted &&
                                                            collaboration.RejectedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetRejectedCollaborationRequests) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaborations with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(accountId) }, { nameof(asRequester) }) = ({ accountId }, { asRequester })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Collaboration[]> GetAllCollaborationRequests(int accountId, bool asRequester = true) {
            try {
                var user = await _dbContext.Users.SingleOrDefaultAsync(aUser => aUser.AccountId == accountId);
                if (user == null) return null;

                return await _dbContext.Collaborations
                                       .Where(
                                           collaboration => (asRequester ? collaboration.UserId == user.Id : collaboration.CollaboratorId == user.Id)
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetAllCollaborationRequests) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaborations with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(accountId) }, { nameof(asRequester) }) = ({ accountId }, { asRequester })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteCollaborationRequest(Collaboration collaborationRequest) {
            try {
                _dbContext.Collaborations.Remove(collaborationRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(DeleteCollaborationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting entry from Collaborations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(collaborationRequest) } = { JsonConvert.SerializeObject(collaborationRequest) }",
                    Severity = LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<User[]> GetShareableCollaboratorsForUser(int accountId) {
            try {
                var user = await _dbContext.Users.SingleOrDefaultAsync(aUser => aUser.AccountId == accountId);
                if (user == null) return null;
                
                return await _dbContext.Collaborations
                                       .Where(collaboration => collaboration.IsAccepted)
                                       .Select(collaboration => (collaboration.UserId == user.Id ? collaboration.Collaborator : collaboration.User))
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetShareableCollaboratorsForUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Collaborator with Where-Select-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(GetShareableCollaboratorsForUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching User with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> AreTheyCollaborating(int userId1, int userId2) {
            try {
                return await _dbContext.Collaborations
                                       .Where(
                                           collaboration => ((collaboration.UserId == userId1 && collaboration.CollaboratorId == userId2) ||
                                                            (collaboration.UserId == userId2 && collaboration.CollaboratorId == userId1)) &&
                                                            collaboration.IsAccepted
                                       )
                                       .Select(collaboration => collaboration.Id)
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(AreTheyCollaborating) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CollaborationId with Where-Select-Single.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId1) }, { nameof(userId2) }) = ({ userId1 }, { userId2 })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(AreTheyCollaborating) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching CollaborationId with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId1) }, { nameof(userId2) }) = ({ userId1 }, { userId2 })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DoesCollaboratorAlreadyHaveThisItemTask(ItemSharingVM sharingData) {
            try {
                return await _dbContext.CollaboratorTasks
                                       .AnyAsync(
                                           task => task.TaskId == sharingData.ItemId &&
                                                   task.TaskType.Equals(sharingData.ItemType) &&
                                                   task.Collaboration.CollaboratorId == sharingData.SharedToUserId
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(DoesCollaboratorAlreadyHaveThisItemTask) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CollaboratorTasks with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(sharingData) } = { JsonConvert.SerializeObject(sharingData) }",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<bool?> DoesCollaboratorAlreadyHaveThisGroupTask(GroupSharingVM sharingData) {
            try {
                return await _dbContext.CollaboratorTasks
                                       .AnyAsync(
                                           task => task.TaskId == sharingData.GroupId &&
                                                   task.TaskType.Equals($"{ nameof(ContentGroup) }.{ sharingData.GroupOfType }") &&
                                                   task.Collaboration.CollaboratorId == sharingData.SharedToUserId
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(DoesCollaboratorAlreadyHaveThisItemTask) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CollaboratorTasks with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(sharingData) } = { JsonConvert.SerializeObject(sharingData) }",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<CollaboratorTask> GetCollaboratorTaskFor(int collaborationId, int itemId, string itemType) {
            try {
                return await _dbContext.CollaboratorTasks
                                       .Where(
                                           task => task.CollaborationId == collaborationId &&
                                                   task.TaskId == itemId &&
                                                   task.TaskType.Equals(itemType)
                                       )
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetCollaboratorTaskFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CollaboratorTask with Where-Single.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaborationId) }, { nameof(itemId) }, { nameof(itemType) }) = ({ collaborationId }, { itemId }, { itemType })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(GetCollaboratorTaskFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching CollaboratorTask with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaborationId) }, { nameof(itemId) }, { nameof(itemType) }) = ({ collaborationId }, { itemId }, { itemType })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteCollaboratorTask(CollaboratorTask task) {
            try {
                _dbContext.CollaboratorTasks.Remove(task);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CollaborationService) }.{ nameof(DeleteCollaboratorTask) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing entry to CollaboratorTasks.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(task) } = { JsonConvert.SerializeObject(task) }",
                    Severity = LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<User[]> GetCollaboratorsOnItemFor(int ownerId, int itemId, string itemType, bool isGroup = false) {
            try {
                var tasksByOwnerId = await _dbContext.CollaboratorTasks
                                                    .Where(
                                                        task => task.TaskId == itemId &&
                                                                task.TaskType.Equals(isGroup ? $"{ nameof(ContentGroup) }.{ itemType }" : itemType)
                                                    )
                                                    .ToArrayAsync();
                
                return tasksByOwnerId.Where(task => task.Collaboration.IsAccepted)
                                     .Select(
                                         task => (task.Collaboration.UserId == ownerId ? task.Collaboration.Collaborator : task.Collaboration.User)
                                     )
                                     .ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(CollaborationService) }.{ nameof(GetCollaboratorTaskFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while filtering User through CollaboratorTask with Where-SelectMany-Single.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(ownerId) }, { nameof(itemId) }, { nameof(itemType) }, { isGroup }) = ({ ownerId }, { itemId }, { itemType }, { isGroup })",
                    Severity = LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }
    }
}