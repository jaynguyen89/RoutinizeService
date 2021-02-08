using System;
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

namespace RoutinizeCore.Services.DatabaseServices {

    public class ContentGroupService : IContentGroupService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public ContentGroupService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
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
    }
}