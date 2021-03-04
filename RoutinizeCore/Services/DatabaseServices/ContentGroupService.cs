using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using RoutinizeCore.ViewModels.ItemGroup;

namespace RoutinizeCore.Services.DatabaseServices {

    public class ContentGroupService : DbServiceBase, IContentGroupService {

        public ContentGroupService(
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

        public async Task<int?> InsertNewContentGroup(ContentGroup contentGroup) {
            try {
                await _dbContext.ContentGroups.AddAsync(contentGroup);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : contentGroup.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(InsertNewContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to TodoGroups.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(contentGroup) } = { JsonConvert.SerializeObject(contentGroup) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool> UpdateContentGroup(ContentGroup contentGroup) {
            try {
                _dbContext.ContentGroups.Update(contentGroup);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(UpdateContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to TodoGroups.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(contentGroup) } = { JsonConvert.SerializeObject(contentGroup) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<ContentGroup> GetContentGroupById(int groupId) {
            try {
                return await _dbContext.ContentGroups.FindAsync(groupId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetContentGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting ContentGroup using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewGroupShare(GroupShare groupShare) {
            try {
                await _dbContext.GroupShares.AddAsync(groupShare);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : groupShare.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(InsertNewGroupShare) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to GroupShares.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupShare) } = { JsonConvert.SerializeObject(groupShare) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsGroupCreatedByThisUser(int userId, int groupId) {
            try {
                return await _dbContext.ContentGroups.AnyAsync(group => group.Id == groupId && group.CreatedById == userId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(IsGroupCreatedByThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error in lambda AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(groupId) }) = ({ userId }, { groupId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsGroupSharedToAnyoneElseExceptThisCollaborator(int collaboratorId, int groupId, string itemType, int ownerId) {
            try {
                var collaborationIdHavingThisCollaborator = await _dbContext.Collaborations
                                                                            .Where(
                                                                                collaboration => collaboration.CollaboratorId == collaboratorId &&
                                                                                                 collaboration.UserId == ownerId &&
                                                                                                 collaboration.IsAccepted
                                                                            )
                                                                            .Select(collaboration => collaboration.Id)
                                                                            .SingleOrDefaultAsync();
                
                var isSharedToOtherCollaborators = await _dbContext.CollaboratorTasks
                                                                   .AnyAsync(
                                                                       task => task.CollaborationId != collaborationIdHavingThisCollaborator &&
                                                                               task.Id == groupId &&
                                                                               task.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(itemType) }")
                                                                   );
                if (isSharedToOtherCollaborators) return true;

                var isSharedToATeam = await _dbContext.TeamTasks
                                                      .AnyAsync(
                                                          teamTask => teamTask.TaskId == groupId &&
                                                                      teamTask.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(itemType) }")
                                                      );
                if (isSharedToATeam) return true;

                var isSharedToProjectIteration = await _dbContext.IterationTasks
                                                                 .AnyAsync(
                                                                     iterationTask => iterationTask.TaskId == groupId &&
                                                                                      iterationTask.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(itemType) }")
                                                                 );
                return isSharedToProjectIteration;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(IsGroupSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(groupId) }, { nameof(itemType) }, { nameof(ownerId) }) = ({ collaboratorId }, { groupId }, { itemType }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(IsGroupSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching Todo with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(groupId) }, { nameof(itemType) }, { nameof(ownerId) }) = ({ collaboratorId }, { groupId }, { itemType }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<User> GetContentGroupOwner(int itemId, string itemType) {
            try {
                return await _dbContext.ContentGroups
                                       .Where(group => group.Id == itemId && group.GroupOfType.Equals(itemType))
                                       .Select(group => group.CreatedBy)
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetContentGroupOwner) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error in lambda with Where-Select-SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(itemId) }, { nameof(itemType) }) = ({ itemId }, { itemType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetContentGroupOwner) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching ContentGroup, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(itemId) }, { nameof(itemType) }) = ({ itemId }, { itemType })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteContentGroupById(int groupId) { 
            try {
                var contentGroup = await _dbContext.ContentGroups.SingleOrDefaultAsync(group => group.Id == groupId);
                if (contentGroup == null) return null;
            
                _dbContext.ContentGroups.Remove(contentGroup);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(DeleteContentGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(DeleteContentGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroup with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
            
                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(DeleteContentGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching TodoGroup with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<bool?> IsContentGroupCreatedByThisUser(int userId, int groupId) {
            try {
                return await _dbContext.ContentGroups.AnyAsync(
                    group => group.Id == groupId &&
                             group.CreatedById == userId
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(IsContentGroupCreatedByThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroup with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(groupId) }) = ({ userId }, { groupId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<KeyValuePair<int, ContentGroup>[]> GetOwnerActiveContentGroups(int userId, string groupType) {
            try {
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => !group.IsShared &&
                                                    !group.DeletedOn.HasValue &&
                                                    group.GroupOfType.Equals(groupType) &&
                                                    group.CreatedById == userId
                                       )
                                       .Select(
                                           group => new KeyValuePair<int, ContentGroup>(
                                               GetGroupItemsCount(group, groupType),
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
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetOwnerActiveContentGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching ContentGroups with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(groupType) }) = ({ userId }, { groupType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
        }

        public async Task<KeyValuePair<int, ContentGroup>[]> GetOwnerArchivedContentGroups(int userId, string groupType) {
            try {
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => !group.IsShared &&
                                                    group.DeletedOn.HasValue &&
                                                    group.GroupOfType.Equals(groupType) &&
                                                    group.CreatedById == userId
                                       )
                                       .Select(
                                           group => new KeyValuePair<int, ContentGroup>(
                                               GetGroupItemsCount(group, groupType),
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
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetOwnerArchivedContentGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching ContentGroups with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(groupType) }) = ({ userId }, { groupType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
        }
        
        public async Task<KeyValuePair<int, ContentGroup>[]> GetSharedActiveContentGroups(int userId, string groupType) {
            try {
                var todoGroupIdsSharedToUser = GetGroupIdsSharedToUser(userId, groupType);
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => group.IsShared &&
                                                    group.GroupOfType.Equals(groupType) &&
                                                    todoGroupIdsSharedToUser.Contains(group.Id) &&
                                                    !group.DeletedOn.HasValue
                                       )
                                       .Select(group => new KeyValuePair<int, ContentGroup>(
                                               GetGroupItemsCount(group, groupType),
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
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetSharedActiveContentGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(groupType) }) = ({ userId }, { groupType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
        }

        public async Task<KeyValuePair<int, ContentGroup>[]> GetSharedArchivedContentGroups(int userId, string groupType) {
            try {
                var todoGroupIdsSharedToUser = GetGroupIdsSharedToUser(userId, groupType);
                return await _dbContext.ContentGroups
                                       .Where(
                                           group => group.IsShared &&
                                                    group.GroupOfType.Equals(groupType) &&
                                                    todoGroupIdsSharedToUser.Contains(group.Id) &&
                                                    group.DeletedOn.HasValue
                                       )
                                       .Select(group => new KeyValuePair<int, ContentGroup>(
                                               GetGroupItemsCount(group, groupType),
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
                    Location = $"{ nameof(ContentGroupService) }.{ nameof(GetSharedArchivedContentGroups) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Todos with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(groupType) }) = ({ userId }, { groupType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
        }

        public async Task<object[]> GetItemsForContentGroupById(int groupId) {
            try {
                var contentGroup = await GetContentGroupById(groupId);
                if (contentGroup == null) return null;

                var getItemsExpression = contentGroup.GroupOfType switch {
                    nameof(Todo) => (Func<Task<object[]>>)(async () => 
                            await _dbContext.Todos
                                            .Where(
                                                todo => todo.GroupId.HasValue &&
                                                        todo.GroupId.Value == groupId &&
                                                        (todo.Group.DeletedOn.HasValue || !todo.DeletedOn.HasValue)
                                            )
                                            .ToArrayAsync()
                    ),
                    nameof(Note) => async () =>
                            await _dbContext.Notes
                                            .Where(
                                                note => note.GroupId.HasValue &&
                                                        note.GroupId.Value == groupId &&
                                                        (note.Group.DeletedOn.HasValue || !note.DeletedOn.HasValue)
                                            )
                                            .ToArrayAsync()
                    ,
                    nameof(RandomIdea) => async () =>
                            await _dbContext.RandomIdeas
                                            .Where(
                                                idea => idea.GroupId.HasValue &&
                                                        idea.GroupId.Value == groupId &&
                                                        (idea.Group.DeletedOn.HasValue || !idea.DeletedOn.HasValue)
                                            )
                                            .ToArrayAsync()
                    ,
                    _ => default
                };
                
                return getItemsExpression?.Invoke().Result;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(GetItemsForContentGroupById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching TodoGroups IDs with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
        }

        public bool? AddItemsToContentGroupFrom(ItemGroupVM itemGroup) {
            var addItemsToGroupExpression = itemGroup.ItemType switch {
                nameof(Todo) => (Func<Task<bool?>>)(async () => await AddTodosIntoContentGroup(itemGroup.ItemIds, itemGroup.GroupId)),
                nameof(Note) => async () => await AddNotesIntoContentGroup(itemGroup.ItemIds, itemGroup.GroupId),
                nameof(RandomIdea) => async () => await AddRandomIdeasIntoContentGroup(itemGroup.ItemIds, itemGroup.GroupId),
                _ => default
            };

            return addItemsToGroupExpression?.Invoke().Result;
        }

        private async Task<bool?> AddRandomIdeasIntoContentGroup(int[] randomIdeaIds, int groupId) {
            try {
                var randomIdeasToAdd = await _dbContext.RandomIdeas
                                                       .Where(
                                                           idea => randomIdeaIds.Contains(idea.Id) &&
                                                                   !idea.DeletedOn.HasValue
                                                       )
                                                       .ToArrayAsync();

                foreach (var randomIdea in randomIdeasToAdd) randomIdea.GroupId = groupId;
                
                _dbContext.RandomIdeas.UpdateRange(randomIdeasToAdd);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(AddRandomIdeasIntoContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching items from RandomIdeas with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(groupId) }, { nameof(randomIdeaIds) }) = ({ groupId }, { JsonConvert.SerializeObject(randomIdeaIds) })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(AddRandomIdeasIntoContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while adding item to ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(groupId) }, { nameof(randomIdeaIds) }) = ({ groupId }, { JsonConvert.SerializeObject(randomIdeaIds) })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        private async Task<bool?> AddNotesIntoContentGroup(int[] noteIds, int groupId) {
            try {
                var notesToAdd = await _dbContext.Notes
                                                       .Where(
                                                           note => noteIds.Contains(note.Id) &&
                                                                   !note.DeletedOn.HasValue
                                                       )
                                                       .ToArrayAsync();
                
                foreach (var note in notesToAdd) note.GroupId = groupId;
                
                _dbContext.Notes.UpdateRange(notesToAdd);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(AddNotesIntoContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching items from Notes with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(groupId) }, { nameof(noteIds) }) = ({ groupId }, { JsonConvert.SerializeObject(noteIds) })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(AddNotesIntoContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while adding item to ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(groupId) }, { nameof(noteIds) }) = ({ groupId }, { JsonConvert.SerializeObject(noteIds) })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        private async Task<bool?> AddTodosIntoContentGroup(int[] todoIds, int groupId) {
            try {
                var todosToAdd = await _dbContext.Todos
                                                 .Where(
                                                     note => todoIds.Contains(note.Id) &&
                                                             !note.DeletedOn.HasValue
                                                 )
                                                 .ToArrayAsync();
                
                foreach (var todo in todosToAdd) todo.GroupId = groupId;
                
                _dbContext.Todos.UpdateRange(todosToAdd);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(AddTodosIntoContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching items from Todos with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(groupId) } = { groupId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(AddTodosIntoContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while adding item to ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(groupId) }, { nameof(todoIds) }) = ({ groupId }, { JsonConvert.SerializeObject(todoIds) })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        public bool? RemoveItemsFromContentGroupFor(ItemGroupVM itemGroup) {
            var removeItemsFromGroupExpression = itemGroup.ItemType switch {
                nameof(Todo) => (Func<Task<bool?>>)(async () => await RemoveTodosFromContentGroup(itemGroup.ItemIds)),
                nameof(Note) => async () => await RemoveNotesFromContentGroup(itemGroup.ItemIds),
                nameof(RandomIdea) => async () => await RemoveRandomIdeasFromContentGroup(itemGroup.ItemIds),
                _ => default
            };

            return removeItemsFromGroupExpression?.Invoke().Result;
        }

        private async Task<bool?> RemoveRandomIdeasFromContentGroup(int[] randomIdeaIds) {
            try {
                var ideasToRemove = await _dbContext.RandomIdeas
                                                    .Where(
                                                        idea => randomIdeaIds.Contains(idea.Id) &&
                                                                !idea.DeletedOn.HasValue
                                                    )
                                                    .ToArrayAsync();
                
                foreach (var idea in ideasToRemove) idea.GroupId = null;
                
                _dbContext.RandomIdeas.UpdateRange(ideasToRemove);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(RemoveRandomIdeasFromContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching items from RandomIdeas with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdeaIds) } = { JsonConvert.SerializeObject(randomIdeaIds) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(RemoveRandomIdeasFromContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing RandomIdea items from ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdeaIds) } = { JsonConvert.SerializeObject(randomIdeaIds) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        private async Task<bool?> RemoveNotesFromContentGroup(int[] noteIds) {
            try {
                var notesToRemove = await _dbContext.Notes
                                                    .Where(
                                                        note => noteIds.Contains(note.Id) &&
                                                                !note.DeletedOn.HasValue
                                                    )
                                                    .ToArrayAsync();
                
                foreach (var note in notesToRemove) note.GroupId = null;
                
                _dbContext.Notes.UpdateRange(notesToRemove);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(RemoveNotesFromContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching items from Notes with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(noteIds) } = { JsonConvert.SerializeObject(noteIds) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(RemoveNotesFromContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing Note items from ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(noteIds) } = { JsonConvert.SerializeObject(noteIds) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        private async Task<bool?> RemoveTodosFromContentGroup(int[] todoIds) {
            try {
                var todosToRemove = await _dbContext.Todos
                                                    .Where(
                                                        todo => todoIds.Contains(todo.Id) &&
                                                                !todo.DeletedOn.HasValue
                                                    )
                                                    .ToArrayAsync();
                
                foreach (var todo in todosToRemove) todo.GroupId = null;
                
                _dbContext.Todos.UpdateRange(todosToRemove);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(RemoveTodosFromContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching items from Todos with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoIds) } = { JsonConvert.SerializeObject(todoIds) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
        
                return default;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(ContentGroupService) }.{ nameof(RemoveTodosFromContentGroup) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing Todo items from ContentGroup.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(todoIds) } = { JsonConvert.SerializeObject(todoIds) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        private int[] GetGroupIdsSharedToUser(int userId, string groupType) {
            var getGroupIdsExpression = groupType switch {
                nameof(Todo) => (Func<Task<int[]>>)(async () => await GetGroupIdsSharedToUserForType<Todo>(userId)),
                nameof(Note) => async () => await GetGroupIdsSharedToUserForType<Note>(userId),
                _ => default
            };

            return getGroupIdsExpression?.Invoke().Result;
        }

        private int GetGroupItemsCount(ContentGroup group, string groupType) {
            return groupType switch {
                nameof(Todo) => group.Todos.Count,
                nameof(Note) => group.Notes.Count,
                _ => default
            };
        }
    }
}