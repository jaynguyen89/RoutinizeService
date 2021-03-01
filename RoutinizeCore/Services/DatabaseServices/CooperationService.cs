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
using RoutinizeCore.ViewModels.Cooperation;
using RoutinizeCore.ViewModels.Organization;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.Services.DatabaseServices {

    public class CooperationService : DbServiceBase, ICooperationService {
        
        public CooperationService(
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

        public async Task<CooperationParticipant> GetCooperationParticipantBy(int userIdOrOrganizationId, int cooperationId, string participantType) {
            try {
                return await _dbContext.CooperationParticipants.SingleOrDefaultAsync(
                    participant => participant.CooperationId == cooperationId &&
                                   participant.ParticipantId == userIdOrOrganizationId &&
                                   participant.ParticipantType.Equals(participantType) &&
                                   participant.IsActive
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationParticipantBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting CooperationParticipant with SingleOrDefault, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userIdOrOrganizationId) }, { nameof(cooperationId) }, { nameof(participantType) }) = ({ userIdOrOrganizationId }, { cooperationId }, { participantType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationParticipantBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting CooperationParticipant with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userIdOrOrganizationId) }, { nameof(cooperationId) }, { nameof(participantType) }) = ({ userIdOrOrganizationId }, { cooperationId }, { participantType })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewCooperation(Cooperation cooperation) {
            try {
                await _dbContext.Cooperations.AddAsync(cooperation);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : cooperation.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(InsertNewCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Cooperations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperation) } = { JsonConvert.SerializeObject(cooperation) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int[]> AddParticipantsToCooperationById(int cooperationId, PoolingParticipantsVM participants) {
            try {
                List<CooperationParticipant> cooperationParticipants = new();
                
                participants.UserIds.ForEach(
                    userId => cooperationParticipants.Add(new CooperationParticipant {
                        CooperationId = cooperationId,
                        ParticipantId = userId,
                        ParticipantType = nameof(User),
                        ParticipatedOn = DateTime.UtcNow
                    })
                );
                
                participants.OrganizationIds.ForEach(
                    organizationId => cooperationParticipants.Add(new CooperationParticipant {
                        CooperationId = cooperationId,
                        ParticipantId = organizationId,
                        ParticipantType = nameof(Organization),
                        ParticipatedOn = DateTime.UtcNow
                    })
                );

                await _dbContext.CooperationParticipants.AddRangeAsync(cooperationParticipants);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? null : cooperationParticipants.Select(participant => participant.Id).ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(AddParticipantsToCooperationById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while creating List of CooperationParticipant using ForEach, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(cooperationId) }, { nameof(participants) }) = ({ cooperationId }, { JsonConvert.SerializeObject(participants) })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(AddParticipantsToCooperationById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entries to CooperationParticipants using AddRange.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(cooperationId) }, { nameof(participants) }) = ({ cooperationId }, { JsonConvert.SerializeObject(participants) })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationRequest> GetCooperationRequestById(int requestId) {
            try {
                return await _dbContext.CooperationRequests.FindAsync(requestId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationRequestById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting CooperationRequest using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(requestId) } = { requestId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateCooperationRequest(CooperationRequest cooperationRequest) {
            try {
                _dbContext.CooperationRequests.Update(cooperationRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(UpdateCooperationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to CooperationRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationRequest) } = { JsonConvert.SerializeObject(cooperationRequest) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsUserAParticipantAllowedToManageCooperationAndRequest(int userId, int cooperationId) {
            try {
                return await _dbContext.Cooperations.AnyAsync(
                    cooperation => cooperation.Id == cooperationId &&
                                   cooperation.IsInEffect && (
                                       cooperation.AllowAnyoneToRespondRequest ||
                                       JsonConvert.DeserializeObject<int[]>(cooperation.ConfidedRequestResponderIds).Any(id => id == userId)
                                   )
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsUserAParticipantAllowedToManageCooperationAndRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in Cooperations with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(cooperationId) }) = ({ userId }, { cooperationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsThisTheLastResponder(string responderSignatures, int cooperationId, int responderId) {
            try {
                var cooperation = await GetCooperationById(cooperationId);
                if (cooperation == null) return null;

                if (cooperation.AllowAnyoneToRespondRequest) return true;
                
                var signatures = JsonConvert.DeserializeObject<SignaturePoolVM>(responderSignatures);
                
                var confidedResponderIds = JsonConvert.DeserializeObject<int[]>(cooperation.ConfidedRequestResponderIds);
                var responderIdsFromSignatures = signatures.UserSignatures
                                                           .Select(userSignature => userSignature.ResponderId)
                                                           .Concat(
                                                               signatures.OrganizationSignatures
                                                                         .Select(organizationSignature => organizationSignature.Signature.ResponderId)
                                                           )
                                                           .ToArray();

                var pendingSignerIds = confidedResponderIds.Except(responderIdsFromSignatures).ToArray();
                
                return pendingSignerIds.Length == 1 &&
                       pendingSignerIds.Contains(responderId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsThisTheLastResponder) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while checking if intersect of confidedUserIds and signerIds contains responderId, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(cooperationId) }, { nameof(responderId) }, { nameof(responderSignatures) }) = ({ cooperationId }, { responderId }, { responderSignatures })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public bool? DoOtherRespondersAcceptTheRequest(string responderSignatures) {
            try {
                var signatures = JsonConvert.DeserializeObject<SignaturePoolVM>(responderSignatures);
                return signatures.UserSignatures.All(signature => signature.IsAccepted) &&
                       signatures.OrganizationSignatures.All(signature => signature.Signature.IsAccepted);
            }
            catch (ArgumentNullException e) {
                _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsThisTheLastResponder) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while checking IsAccepted in SignaturePoolVM using All, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(responderSignatures) } = { responderSignatures }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<KeyValuePair<string, int>> IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(int userId, int cooperationId) {
            try {
                var cooperationParticipants = await _dbContext.CooperationParticipants
                                                              .Where(
                                                                  participant => participant.CooperationId == cooperationId &&
                                                                                 participant.IsActive
                                                              )
                                                              .ToArrayAsync();

                var isUserParticipant = cooperationParticipants.Any(participant => participant.ParticipantId == userId && participant.ParticipantType.Equals(nameof(User)));
                if (isUserParticipant) return new KeyValuePair<string, int>(nameof(User), 0);
                
                foreach (var participant in cooperationParticipants) {
                    if (participant.ParticipantType.Equals(nameof(User))) continue;

                    var memberOrganizationId = await _dbContext.UserOrganizations
                                                               .Where(
                                                                   userOrganization => userOrganization.IsActive &&
                                                                                       userOrganization.OrganizationId == participant.ParticipantId &&
                                                                                       userOrganization.UserId == userId
                                                               )
                                                               .Select(userOrganization => userOrganization.OrganizationId)
                                                               .SingleOrDefaultAsync();

                    if (memberOrganizationId != default) return new KeyValuePair<string, int>(nameof(Organization), memberOrganizationId);
                }

                return new KeyValuePair<string, int>(string.Empty, 0);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CooperationParticipants wih Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(cooperationId) }) = ({ userId }, { cooperationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<string, int>(null, 0);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching UserOrganizations with Where-SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(cooperationId) }) = ({ userId }, { cooperationId })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<string, int>(null, 0);
            }
        }

        public async Task<bool?> DoesCooperationHaveThisParticipant(int participantId, int cooperationId, string participantType = null) {
            try {
                return await _dbContext.CooperationParticipants.AnyAsync(
                    participant => participant.Id == participantId &&
                                   participant.CooperationId == cooperationId &&
                                   participant.IsActive && (
                                       !Helpers.IsProperString(participantType) ||
                                       participant.ParticipantType.Equals(participantType)
                                   )
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CooperationParticipants wih AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(participantId) }, { nameof(cooperationId) }, { nameof(participantType) }) = ({ participantId }, { cooperationId }, { participantType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public Tuple<int, int, int> GetCountsForAccepterRejecterAndNoResponse(string responderSignatures, string confidedResponderIds) {
            try {
                int accepterCount = 0, rejecterCount = 0;
                var signatures = JsonConvert.DeserializeObject<SignaturePoolVM>(responderSignatures);

                var expectedResponderIds = JsonConvert.DeserializeObject<int[]>(confidedResponderIds);
                
                signatures.UserSignatures.ForEach(signature => {
                    if (signature.IsAccepted) accepterCount++;
                    else rejecterCount++;
                });
                
                signatures.OrganizationSignatures.ForEach(signature => {
                        if (signature.Signature.IsAccepted) accepterCount++;
                        else rejecterCount++;
                    });

                var noRespondCount = expectedResponderIds.Except(
                    signatures.UserSignatures
                              .Select(signature => signature.ResponderId)
                              .Concat(
                                  signatures.OrganizationSignatures.Select(signature => signature.Signature.ResponderId)
                              )
                ).Count();

                return new Tuple<int, int, int>(accepterCount, rejecterCount, noRespondCount);
            }
            catch (ArgumentNullException e) {
                _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCountsForAccepterRejecterAndNoResponse) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while processing arrays with ForEach/Select/Concat, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(responderSignatures) }, { nameof(confidedResponderIds) }) = ({ responderSignatures }, { confidedResponderIds })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> RemoveCooperationRequest(CooperationRequest cooperationRequest) {
            try {
                _dbContext.CooperationRequests.Remove(cooperationRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(RemoveCooperationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing entry from CooperationRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationRequest) } = { JsonConvert.SerializeObject(cooperationRequest) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsCooperationRequestAlreadyMade(CooperationRequestVM cooperationRequest) {
            try {
                return await _dbContext.CooperationRequests.AnyAsync(
                    request => !request.IsAccepted.HasValue &&
                               request.RequestedById == cooperationRequest.RequestedById &&
                               request.RequestedToId == cooperationRequest.RequestedToId &&
                               request.RequestedByType.Equals(cooperationRequest.RequestedByType) &&
                               request.RequestedToType.Equals(cooperationRequest.RequestedToType)
                );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsCooperationRequestAlreadyMade) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in CooperationRequests with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationRequest) } = { cooperationRequest }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewCooperationRequest(CooperationRequest cooperationRequest) {
            try {
                await _dbContext.CooperationRequests.AddAsync(cooperationRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : cooperationRequest.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(InsertNewCooperationRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to CooperationRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationRequest) } = { JsonConvert.SerializeObject(cooperationRequest) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewCooperationParticipant(CooperationParticipant participant) {
            try {
                await _dbContext.CooperationParticipants.AddAsync(participant);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : participant.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(InsertNewCooperationParticipant) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to CooperationRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(participant) } = { JsonConvert.SerializeObject(participant) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateCooperation(Cooperation cooperation, bool requireSigning = false) {
            try {
                if (requireSigning) await StartTransaction();

                _dbContext.Cooperations.Update(cooperation);
                var updateCoopResult = await _dbContext.SaveChangesAsync();

                if (updateCoopResult == 0) {
                    if (requireSigning) await RevertTransaction();
                    return false;
                }

                if (!requireSigning) return true;
                
                var participants = _dbContext.CooperationParticipants
                                             .Where(
                                                 participant => participant.CooperationId == cooperation.Id &&
                                                                participant.IsActive
                                             )
                                             .AsEnumerable();
                
                var signingCheckers = participants
                                      .Select(participant => new SigningChecker {
                                          CooperationParticipantId = participant.Id,
                                          CreatedOn = DateTime.UtcNow,
                                          IsValid = true
                                      })
                                      .ToArray();

                await _dbContext.SigningCheckers.AddRangeAsync(signingCheckers);
                var saveCheckersResult = await _dbContext.SaveChangesAsync();

                if (saveCheckersResult != 0) return true;
                
                await RevertTransaction();
                return false;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(UpdateCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Cooperations.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(requireSigning) }, { nameof(cooperation) }) = ({ requireSigning }, { JsonConvert.SerializeObject(cooperation) })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Cooperation> GetCooperationById(int cooperationId) {
            try {
                return await _dbContext.Cooperations.FindAsync(cooperationId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting Cooperation using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewDepartmentAccess(DepartmentAccess departmentAccess) {
            try {
                await _dbContext.DepartmentAccesses.AddAsync(departmentAccess);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : departmentAccess.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(InsertNewDepartmentAccess) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to DepartmentAccesses.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentAccess) } = { JsonConvert.SerializeObject(departmentAccess) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateDepartmentAccess(DepartmentAccess departmentAccess) {
            try {
                _dbContext.DepartmentAccesses.Update(departmentAccess);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(UpdateDepartmentAccess) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to DepartmentAccesses.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentAccess) } = { JsonConvert.SerializeObject(departmentAccess) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationParticipant> GetCooperationParticipantById(int cooperationParticipantId) {
            try {
                return await _dbContext.CooperationParticipants.FindAsync(cooperationParticipantId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationParticipantById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting CooperationParticipant using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationParticipantId) } = { cooperationParticipantId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateCooperationParticipant(CooperationParticipant cooperationParticipant) {
            try {
                _dbContext.CooperationParticipants.Update(cooperationParticipant);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(UpdateCooperationParticipant) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to CooperationParticipants.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationParticipant) } = { JsonConvert.SerializeObject(cooperationParticipant) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsCooperationActive(int cooperationId) {
            try {
                return await _dbContext.Cooperations.AnyAsync(cooperation => cooperation.Id == cooperationId && cooperation.IsInEffect);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsCooperationActive) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in Cooperations with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewParticipantReturnRequest(ParticipantReturnRequest returnRequest) {
            try {
                await _dbContext.ParticipantReturnRequests.AddAsync(returnRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : returnRequest.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(InsertNewParticipantReturnRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to ParticipantReturnRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(returnRequest) } = { JsonConvert.SerializeObject(returnRequest) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsReturnRequestBelongedToThisUser(int requestId, int userId) {
            try {
                var belongToUserParticipant = await _dbContext.ParticipantReturnRequests
                                                              .AnyAsync(
                                                                  request => request.Id == requestId &&
                                                                             request.CooperationParticipant.IsActive &&
                                                                             request.CooperationParticipant.ParticipantId == userId &&
                                                                             request.CooperationParticipant.ParticipantType.Equals(nameof(User))
                                                              );
                if (belongToUserParticipant) return true;

                var organizationId = await _dbContext.ParticipantReturnRequests
                                                              .Where(
                                                                  request => request.Id == requestId &&
                                                                             !request.CooperationParticipant.IsActive &&
                                                                             request.CooperationParticipant.ParticipantType.Equals(nameof(Organization))
                                                              )
                                                              .Select(request => request.CooperationParticipant.ParticipantId)
                                                              .SingleOrDefaultAsync();

                var belongToOrganization = await _dbContext.UserOrganizations
                                                           .AnyAsync(
                                                               userOrganization => userOrganization.OrganizationId == organizationId &&
                                                                                   userOrganization.UserId == userId &&
                                                                                   userOrganization.IsActive
                                                           );

                return belongToOrganization;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsCooperationActive) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in ParticipantReturnRequests with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(requestId) }) = ({ userId }, { requestId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching ParticipantReturnRequests, CooperationParticipants with Where-SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(requestId) }) = ({ userId }, { requestId })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> RemoveParticipantReturnRequestById(int requestId) {
            try {
                var returnRequest = await GetParticipantReturnRequestById(requestId);
                if (returnRequest == null) return null;

                _dbContext.ParticipantReturnRequests.Remove(returnRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(RemoveParticipantReturnRequestById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing entry from ParticipantReturnRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(requestId) } = { requestId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Cooperation> SearchForCooperationFromParticipantReturnRequest(int requestId) {
            try {
                return await _dbContext.ParticipantReturnRequests
                                       .Where(request => request.Id == requestId)
                                       .Select(request => request.CooperationParticipant.Cooperation)
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(SearchForCooperationFromParticipantReturnRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting Cooperation from ParticipantReturnRequest with Where-Select-SingleOrDefault, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(requestId) } = { requestId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(SearchForCooperationFromParticipantReturnRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting Cooperation from ParticipantReturnRequest with Where-Select-SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(requestId) } = { requestId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<ParticipantReturnRequest> GetParticipantReturnRequestById(int requestId) {
            try {
                return await _dbContext.ParticipantReturnRequests.FindAsync(requestId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetParticipantReturnRequestById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting ParticipantReturnRequests using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(requestId) } = { requestId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateParticipantReturnRequest(ParticipantReturnRequest returnRequest) {
            try {
                _dbContext.ParticipantReturnRequests.Update(returnRequest);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(UpdateParticipantReturnRequest) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to ParticipantReturnRequests.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(returnRequest) } = { JsonConvert.SerializeObject(returnRequest) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<KeyValuePair<bool?, SigningChecker>> SearchValidSigningCheckerForSigner(int userId, int cooperationId, string participantType) {
            try {
                var participantToSign = await _dbContext.CooperationParticipants
                                                        .SingleOrDefaultAsync(
                                                            participant => !participant.IsActive &&
                                                                           participant.CooperationId == cooperationId && (
                                                                               participantType.Equals(nameof(User))
                                                                                   ? participant.ParticipantId == userId
                                                                                   : _dbContext.UserOrganizations.Any(
                                                                                       userOrganization => userOrganization.IsActive &&
                                                                                                           userOrganization.UserId == userId &&
                                                                                                           userOrganization.OrganizationId == participant.ParticipantId
                                                                                   )
                                                                           )
                                                        );
                if (participantToSign == null) return new KeyValuePair<bool?, SigningChecker>(false, null);

                var signingChecker = await _dbContext.SigningCheckers
                                                     .Where(
                                                         checker => checker.CooperationParticipantId == participantToSign.Id &&
                                                                    checker.IsValid
                                                     )
                                                     .SingleOrDefaultAsync();
                
                return new KeyValuePair<bool?, SigningChecker>(signingChecker != null, signingChecker);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(SearchValidSigningCheckerForSigner) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching CooperationParticipants, SigningCheckers with Where-SingleOrDefault, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(cooperationId) }, { nameof(participantType) }) = ({ userId }, { cooperationId }, { participantType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<bool?, SigningChecker>(null, null);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(SearchValidSigningCheckerForSigner) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching CooperationParticipants, SigningCheckers with Where-SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(cooperationId) }, { nameof(participantType) }) = ({ userId }, { cooperationId }, { participantType })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool?, SigningChecker>(null, null);
            }
        }

        public async Task<int?> InsertNewSigningChecker(SigningChecker signingChecker) {
            try {
                await _dbContext.SigningCheckers.AddAsync(signingChecker);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : signingChecker.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(InsertNewSigningChecker) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to SigningCheckers.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(signingChecker) } = { JsonConvert.SerializeObject(signingChecker) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateSigningChecker(SigningChecker signingChecker) {
            try {
                _dbContext.SigningCheckers.Update(signingChecker);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(UpdateSigningChecker) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to SigningCheckers.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(signingChecker) } = { JsonConvert.SerializeObject(signingChecker) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> ReviveCooperationAndRequireSignaturesIfNeeded(int cooperationId) {
            try {
                var cooperation = await GetCooperationById(cooperationId);
                if (cooperation == null) return null;
                
                cooperation.IsInEffect = true;
                cooperation.EndedOn = null;

                var activeParticipants = await _dbContext.CooperationParticipants
                                                         .Where(
                                                             participant => participant.CooperationId == cooperationId &&
                                                                            participant.IsActive
                                                         )
                                                         .ToArrayAsync();
                if (activeParticipants.Length != 0) await StartTransaction();
                
                _dbContext.Cooperations.Update(cooperation);
                var updateCoopResult = await _dbContext.SaveChangesAsync();

                if (updateCoopResult == 0) {
                    if (activeParticipants.Length != 0) await RevertTransaction();
                    return false;
                }

                if (activeParticipants.Length == 0) return true;
                
                var signingCheckers = new List<SigningChecker>();
                
                foreach (var participant in activeParticipants) {
                    participant.IsActive = false;
                    
                    signingCheckers.Add(new SigningChecker {
                        CooperationParticipantId = participant.Id,
                        CreatedOn = DateTime.UtcNow,
                        IsValid = true
                    });
                }

                await _dbContext.SigningCheckers.AddRangeAsync(signingCheckers);
                _dbContext.CooperationParticipants.UpdateRange(activeParticipants);

                var saveResult = await _dbContext.SaveChangesAsync();
                return saveResult != 0;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(ReviveCooperationAndRequireSignaturesIfNeeded) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in CooperationParticipants with Where-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(ReviveCooperationAndRequireSignaturesIfNeeded) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entries to Cooperations, SigningCheckers and CooperationParticipants.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationRequestDetailVM[]> GetCooperationRequestsSentBy(int sentById, string sentByType) {
            try {
                var cooperationRequests =  _dbContext.CooperationRequests.Where(request => request.RequestedById == sentById && request.RequestedByType.Equals(sentByType));

                var detailedRequests = new List<CooperationRequestDetailVM>();
                
                async Task PrepareRequestDetails(CooperationRequest request) {
                    CooperationRequestDetailVM.CommunicatorVM communicator = null;
                                            
                    switch (request.RequestedToType) {
                        case nameof(User):
                            var user = await _dbContext.Users.FindAsync(request.RequestedToId);
                            communicator = new CooperationRequestDetailVM.CommunicatorVM {
                                User = user,
                                Type = nameof(User)
                            };
                            break;
                        case nameof(Organization):
                            var organization = await _dbContext.Organizations.FindAsync(request.RequestedToId);
                            communicator = new CooperationRequestDetailVM.CommunicatorVM {
                                Organization = organization,
                                Type = nameof(Organization)
                            };
                            break;
                        case nameof(Cooperation):
                            var cooperation = await _dbContext.Cooperations.FindAsync(request.RequestedToId);
                            communicator = new CooperationRequestDetailVM.CommunicatorVM {
                                Cooperation = cooperation,
                                Type = nameof(Cooperation)
                            };
                            break;
                    }

                    detailedRequests.Add(new CooperationRequestDetailVM { Id = request.Id, Communicator = communicator });
                }

                await cooperationRequests.ForEachAsync(async request => await PrepareRequestDetails(request));
                return detailedRequests.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationRequestsSentBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in CooperationRequests with Where, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(sentById) }, { nameof(sentByType) }) = ({ sentById }, { sentByType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationRequestDetailVM[]> GetCooperationRequestsReceivedBy(int receivedById, string receivedByType) {
            try {
                var cooperationRequests =  _dbContext.CooperationRequests.Where(request => request.RequestedToId == receivedById && request.RequestedToType.Equals(receivedByType));

                var detailedRequests = new List<CooperationRequestDetailVM>();

                async Task PrepareRequestDetails(CooperationRequest request) {
                    CooperationRequestDetailVM.CommunicatorVM communicator = null;

                    switch (request.RequestedByType) {
                        case nameof(User):
                            var user = await _dbContext.Users.FindAsync(request.RequestedToId);
                            communicator = new CooperationRequestDetailVM.CommunicatorVM {
                                User = user,
                                Type = nameof(User)
                            };
                            break;
                        case nameof(Organization):
                            var organization = await _dbContext.Organizations.FindAsync(request.RequestedToId);
                            communicator = new CooperationRequestDetailVM.CommunicatorVM {
                                Organization = organization,
                                Type = nameof(Organization)
                            };
                            break;
                    }
                    
                    detailedRequests.Add(new CooperationRequestDetailVM { Id = request.Id, Communicator = communicator });
                }

                await cooperationRequests.ForEachAsync(async request => await PrepareRequestDetails(request));
                return detailedRequests.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationRequestsReceivedBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in CooperationRequests with Where, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(receivedById) }, { nameof(receivedByType) }) = ({ receivedById }, { receivedByType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationDetailVM> GetCooperationDetailsFor(int cooperationId) {
            try {
                var cooperation = await _dbContext.Cooperations.FindAsync(cooperationId);
                var cooperationDetail = (CooperationDetailVM) cooperation;

                var requestResponderIds = JsonConvert.DeserializeObject<int[]>(cooperation.ConfidedRequestResponderIds);
                var confidedResponders = new List<UserVM>();
                
                foreach (var requestResponderId in requestResponderIds) {
                    var user = await _dbContext.Users.FindAsync(requestResponderId);
                    confidedResponders.Add(user);
                }

                cooperationDetail.ConfidedRequestResponders = confidedResponders.ToArray();
                return cooperationDetail;
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationRequestById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting CooperationRequest details using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationVM[]> GetCooperationsByUserId(int userId, bool activeStatus = true) {
            try {
                var cooperationsHavingUserAsAParticipant = _dbContext.CooperationParticipants
                                                                           .Where(
                                                                               participant => participant.IsActive == activeStatus &&
                                                                                              participant.ParticipantId == userId &&
                                                                                              participant.ParticipantType.Equals(nameof(User))
                                                                           )
                                                                           .Select(participant => participant.Cooperation);

                bool FilterOrganizationsHavingUser(CooperationParticipant participant) {
                    var participantFilter = participant.IsActive == activeStatus && participant.ParticipantType.Equals(nameof(Organization));
                    
                    var organizationHavingUser = _dbContext.UserOrganizations
                                                           .Any(
                                                               userOrganization => userOrganization.IsActive &&
                                                                                   userOrganization.OrganizationId == participant.ParticipantId &&
                                                                                   userOrganization.UserId == userId
                                                           );

                    return participantFilter && organizationHavingUser;
                }

                var cooperationsHavingOrganizationWithUser = _dbContext.CooperationParticipants
                                                                       .Where(FilterOrganizationsHavingUser)
                                                                       .Select(participant => participant.Cooperation);

                var cooperationsHavingUser = await cooperationsHavingUserAsAParticipant
                                                   .Union(cooperationsHavingOrganizationWithUser)
                                                   .Where(cooperation => cooperation.IsInEffect)
                                                   .Select(cooperation => (CooperationVM) cooperation)
                                                   .ToArrayAsync();
                return cooperationsHavingUser;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationsByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in Cooperations from CooperationParticipant with Where-Select-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationVM[]> GetCooperationsByOrganizationId(int organizationId) {
            try {
                var participantsAsOrganization = _dbContext.CooperationParticipants
                                                               .Where(
                                                                   participant => participant.IsActive &&
                                                                                  participant.ParticipantId == organizationId &&
                                                                                  participant.ParticipantType.Equals(nameof(Organization))
                                                               );

                return await participantsAsOrganization
                             .Select(participant => (CooperationVM) participant.Cooperation)
                             .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationsByOrganizationId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in Cooperations from CooperationParticipant with Where-Select-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organizationId) } = { organizationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<CooperationParticipantVM[]> GetCooperationParticipantsFor(int cooperationId) {
            try {
                var dbParticipants = _dbContext.CooperationParticipants
                                               .Where(
                                                   participant => participant.IsActive &&
                                                                  participant.CooperationId == cooperationId
                                               );

                var participants = new List<CooperationParticipantVM>();
                async Task GetParticipantDetails(CooperationParticipant participant) {
                    var participantVm = (CooperationParticipantVM) participant;

                    if (participant.ParticipantType.Equals(nameof(User))) participantVm.UserParticipant = await _dbContext.Users.FindAsync(participant.ParticipantId);
                    else participantVm.OrganizationParticipant = await _dbContext.Organizations.FindAsync(participant.ParticipantId);
                }

                await dbParticipants.ForEachAsync(async participant => await GetParticipantDetails(participant));
                return participants.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetCooperationParticipantsFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting CooperationParticipant details with Where-Select-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<ReturnRequestDetailVM[]> GetReturnRequestsByCooperationId(int cooperationId) {
            try {
                var returnRequests = _dbContext.ParticipantReturnRequests
                                                                .Where(
                                                                    request => !request.CooperationParticipant.IsActive &&
                                                                               request.CooperationParticipant.CooperationId == cooperationId
                                                                );

                var returnRequestDetails = new List<ReturnRequestDetailVM>();
                
                async Task GetReturnRequestDetails(ParticipantReturnRequest request) {
                    var requestDetail = (ReturnRequestDetailVM) request;
                    
                    requestDetail.Participant = request.CooperationParticipant;
                    if (requestDetail.Participant.Type.Equals(nameof(User))) requestDetail.Participant.UserParticipant = await _dbContext.Users.FindAsync(request.CooperationParticipant.ParticipantId);
                    else requestDetail.Participant.OrganizationParticipant = await _dbContext.Organizations.FindAsync(request.CooperationParticipant.ParticipantId);

                    if (requestDetail.Response != null)
                        requestDetail.Response.RespondedByUser = await _dbContext.Users.FindAsync(request.RespondedById);
                    
                    returnRequestDetails.Add(requestDetail);
                }

                await returnRequests.ForEachAsync(async request => await GetReturnRequestDetails(request));
                return returnRequestDetails.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetReturnRequestsByCooperationId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting ParticipantReturnRequest details with Where and ForEach, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationId) } = { cooperationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsAlreadyAParticipantInCooperation(int requestedById, string requestedByType, int cooperationId) {
            try {
                return await _dbContext.CooperationParticipants
                                       .AnyAsync(
                                           participant => participant.CooperationId == cooperationId &&
                                                          participant.ParticipantId == requestedById &&
                                                          participant.ParticipantType.Equals(requestedByType)
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(IsAlreadyAParticipantInCooperation) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in CooperationParticipants with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(requestedById) }, { nameof(requestedByType) }, { nameof(cooperationId) }) = ({ requestedById }, { requestedByType }, { cooperationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> AreTheyAlreadyCooperating(CooperationRequestVM cooperationRequest) {
            try {
                return await _dbContext.Cooperations
                                       .AnyAsync(
                                           cooperation => cooperation.CooperationParticipants.Any(
                                                              participant => participant.ParticipantType.Equals(cooperationRequest.RequestedByType) &&
                                                                             participant.ParticipantId == cooperationRequest.RequestedById
                                                          ) &&
                                                          cooperation.CooperationParticipants.Any(
                                                              participant => participant.ParticipantType.Equals(cooperationRequest.RequestedToType) &&
                                                                             participant.ParticipantId == cooperationRequest.RequestedToId
                                                          )
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(AreTheyAlreadyCooperating) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in Cooperations and CooperationParticipants with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(cooperationRequest) } = { JsonConvert.SerializeObject(cooperationRequest) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<AccessibleDepartmentVM[]> GetDepartmentsAccessibleBy(int participantId, int cooperationId) {
            try {
                var departmentAccessesByAParticipant = await _dbContext.DepartmentAccesses
                                                                       .Where(
                                                                           departmentAccess => departmentAccess.CooperationId == cooperationId &&
                                                                                               departmentAccess.AccessGivenToParticipantId == participantId
                                                                       )
                                                                       .ToArrayAsync();

                var departmentAccesses = new List<AccessibleDepartmentVM>();
                
                foreach (var dbDepartmentAccess in departmentAccessesByAParticipant) {
                    var departmentAccess = new AccessibleDepartmentVM {
                        Organization = await _dbContext.Organizations.FindAsync(dbDepartmentAccess.FromParticipantId)
                    };

                    var departmentIds = JsonConvert.DeserializeObject<int[]>(dbDepartmentAccess.AccessibleDepartmentIds);
                    var accessibleDepartments = new List<GenericDepartmentVM>();
                    
                    foreach (var departmentId in departmentIds)
                        accessibleDepartments.Add(await _dbContext.Departments.FindAsync(departmentId));

                    departmentAccess.Departments = accessibleDepartments.ToArray();
                    departmentAccesses.Add(departmentAccess);
                }

                return departmentAccesses.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetDepartmentsAccessibleBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting DepartmentAccess, Organization, Department with Where-ToArray and Find, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(participantId) }, { nameof(cooperationId) }) = ({ participantId }, { cooperationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<GenericDepartmentVM[]> GetOrganizationDepartmentsGivenAccessToParticipant(int organizationId, int participantId) {
            try {
                var departmentIdsAccessedByParticipant = await _dbContext.DepartmentAccesses
                                                                         .Where(
                                                                             departmentAccess => departmentAccess.FromParticipantId == organizationId &&
                                                                                                 departmentAccess.AccessGivenToParticipantId == participantId
                                                                         )
                                                                         .Select(departmentAccess => departmentAccess.AccessibleDepartmentIds)
                                                                         .FirstOrDefaultAsync();
                
                if (departmentIdsAccessedByParticipant == null) return Array.Empty<GenericDepartmentVM>();

                var accessibleDepartmentIds = JsonConvert.DeserializeObject<int[]>(departmentIdsAccessedByParticipant);
                var departmentAccesses = new List<GenericDepartmentVM>();
                
                foreach (var departmentId in accessibleDepartmentIds)
                    departmentAccesses.Add(await _dbContext.Departments.FindAsync(departmentId));

                return departmentAccesses.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetDepartmentsAccessibleBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting DepartmentAccess, Department with Where-Select-FirstOrDefault and Find, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(participantId) }, { nameof(organizationId) }) = ({ participantId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<DepartmentAccess> GetDepartmentAccessById(int departmentAccessId) {
            try {
                return await _dbContext.DepartmentAccesses.FindAsync(departmentAccessId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(GetDepartmentAccessById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting DepartmentAccess using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentAccessId) } = { departmentAccessId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> HasThisCooperationRequestBeenRespondedBy(int userId, int requestId, int organizationId = -1) {
            try
            {
                var cooperationRequest = await _dbContext.CooperationRequests.FindAsync(requestId);
                var signatures = JsonConvert.DeserializeObject<SignaturePoolVM>(cooperationRequest.ResponderSignatures);

                var isRespondedByUserParticipant = signatures.UserSignatures.Any(signature => signature.ResponderId == userId);
                if (isRespondedByUserParticipant) return true;

                if (organizationId == -1) return null;

                var isRespondedByOrganizationParticipant = signatures.OrganizationSignatures.Any(signature => signature.OrganizationId == organizationId);
                return isRespondedByOrganizationParticipant;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(HasThisCooperationRequestBeenRespondedBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching in CooperationRequests with AnyAsync, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(requestId) }, { nameof(organizationId) }) = ({ userId }, { requestId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<SigningTaskVM[]> GetSigningTasksFromSigningCheckerByUserId(int userId) {
            try {
                bool SigningCheckerSelector(SigningChecker checker) {
                    var fromParticipantAsUser = checker.CooperationParticipant.ParticipantId == userId && checker.CooperationParticipant.ParticipantType.Equals(nameof(User));

                    var organizationIdsHavingUser = _dbContext.UserOrganizations
                        .Where(
                            userOrganization => userOrganization.IsActive &&
                                                userOrganization.UserId == userId
                        )
                        .Select(userOrganization => userOrganization.OrganizationId)
                        .ToArray();

                    var fromParticipantAsOrganizationHavingUser = organizationIdsHavingUser.Contains(checker.CooperationParticipant.ParticipantId) &&
                                                                       checker.CooperationParticipant.ParticipantType.Equals(nameof(Organization));

                    return (fromParticipantAsUser || fromParticipantAsOrganizationHavingUser) && checker.IsValid;
                }

                var userSigningCheckers = _dbContext.SigningCheckers.Where(SigningCheckerSelector);
                var signingTasks = new List<SigningTaskVM>();
                
                foreach (var signingChecker in userSigningCheckers) {
                    var cooperation = await _dbContext.CooperationParticipants
                                                      .Where(participant => participant.Id == signingChecker.CooperationParticipantId)
                                                      .Select(participant => participant.Cooperation)
                                                      .FirstOrDefaultAsync();
                    
                    signingTasks.Add(new SigningTaskVM {
                            SigningCheckerId = signingChecker.Id,
                            Cooperation = cooperation,
                            CreatedOn = signingChecker.CreatedOn
                        });
                }

                return signingTasks.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(CooperationService) }.{ nameof(HasThisCooperationRequestBeenRespondedBy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting SigningCheckers with Cooperation data with Where-Select-ToArray, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}