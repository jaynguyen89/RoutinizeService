using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using MongoLibrary.Interfaces;
using Newtonsoft.Json;
using NotifierLibrary.Interfaces;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Cooperation;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("cooperation")]
    public sealed class CooperationController : AppController {

        private readonly ICooperationService _cooperationService;
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly IRsaService _rsaService;
        private readonly ICooperationLogService _cooperationLog;

        public CooperationController(
            ICooperationService cooperationService,
            IOrganizationService organizationService,
            IUserService userService,
            IRsaService rsaService,
            ICooperationLogService cooperationLog,
            IUserNotifier userNotifier
        ) : base(userNotifier) {
            _cooperationService = cooperationService;
            _organizationService = organizationService;
            _userService = userService;
            _rsaService = rsaService;
            _cooperationLog = cooperationLog;
        }

        [HttpGet("is-active/{cooperationId}")]
        public async Task<JsonResult> IsCooperationActive([FromHeader] int userId,[FromRoute] int cooperationId) {
            var (participantType, _) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationId);
            return !isCooperationActive.HasValue ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                                 : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = isCooperationActive.Value });
        }

        [HttpPost("request")]
        public async Task<JsonResult> MakeCooperationRequest(CooperationRequestVM cooperationRequest) {
            if (cooperationRequest.RequestedToType.Equals(nameof(Cooperation))) {
                var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationRequest.RequestedToId);
                if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to join: cooperation has ended." });

                var alreadyInCooperation = await _cooperationService.IsAlreadyAParticipantInCooperation(cooperationRequest.RequestedById, cooperationRequest.RequestedByType, cooperationRequest.RequestedToId);
                if (!alreadyInCooperation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                if (alreadyInCooperation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to join: you're already in the cooperation." });
            }
            else {
                var alreadyCooperate = await _cooperationService.AreTheyAlreadyCooperating(cooperationRequest);
                if (!alreadyCooperate.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                if (alreadyCooperate.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to join: you two already cooperate." });
            }

            var isRequestExisted = await _cooperationService.IsCooperationRequestAlreadyMade(cooperationRequest);
            if (!isRequestExisted.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (isRequestExisted.Value)  return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = $"You already requested to cooperate with this { cooperationRequest.RequestedToType }." });

            var dbCooperationRequest = (CooperationRequest) cooperationRequest;
            var saveResult = await _cooperationService.InsertNewCooperationRequest(dbCooperationRequest);
            
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }

        [HttpDelete("revoke/{userId}/{requestId}")]
        public async Task<JsonResult> RevokeCooperationRequest([FromRoute] int userId,[FromRoute] int requestId) {
            var cooperationRequest = await _cooperationService.GetCooperationRequestById(requestId);
            if (cooperationRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool? isAuthorized;
            if (cooperationRequest.RequestedByType.Equals(nameof(User))) isAuthorized = userId == cooperationRequest.RequestedById;
            else isAuthorized = await _organizationService.IsUserBelongedToOrganizationAndAllowedToManageCooperation(userId, cooperationRequest.RequestedById);
            
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var deleteResult = await _cooperationService.RemoveCooperationRequest(cooperationRequest);
            return !deleteResult.HasValue || !deleteResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("respond")]
        public async Task<JsonResult> RespondToCooperationRequest(RequestResponseVM response) {
            var error = response.VerifyResponse();
            if (error.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = error });

            var cooperationRequest = await _cooperationService.GetCooperationRequestById(response.RequestId);
            if (cooperationRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var isAuthorizedToRespond = await IsUserAllowedToRespondToCooperationRequest(cooperationRequest.RequestedToType, cooperationRequest.RequestedToId, response.ResponderId);
            if (!isAuthorizedToRespond.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorizedToRespond.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var hasResponded = await _cooperationService.HasThisCooperationRequestBeenRespondedBy(response.ResponderId, response.RequestId);
            if (!hasResponded.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (hasResponded.Value)  return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You have already responded to this request." });
            
            cooperationRequest = await SetResponseDataToCooperationRequestAndSignIt(cooperationRequest, response);
            if (cooperationRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            await _cooperationService.StartTransaction();
            
            var updateResult = await _cooperationService.UpdateCooperationRequest(cooperationRequest);
            if (!updateResult.HasValue || !updateResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            if (!cooperationRequest.IsAccepted.HasValue || !cooperationRequest.IsAccepted.Value) {
                await _cooperationService.CommitTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }

            Cooperation cooperationToUpdate;
            var newParticipants = AddParticipantsFromRequestToCooperation(cooperationRequest);
            
            if (!cooperationRequest.RequestedToType.Equals(nameof(Cooperation))) {
                cooperationToUpdate = Cooperation.GetDefaultInstance();

                cooperationToUpdate.AgreementSigners = JsonConvert.SerializeObject(
                    new AgreementSignersVM {
                        ExpectedSigner = new ExpectedSignerVM {
                            SignersAsUser = new KeyValuePair<int, int>(0, newParticipants.UserIds.Count),
                            SignerAsOrganization = newParticipants.OrganizationIds.Count == 0
                                ? new Dictionary<int, bool>()
                                : newParticipants.OrganizationIds.ToDictionary(organizationId => organizationId, _ => false)
                        }
                    });
                
                var saveNewCooperationResult = await _cooperationService.InsertNewCooperation(cooperationToUpdate);
                if (!saveNewCooperationResult.HasValue || saveNewCooperationResult.Value < 1) {
                    await _cooperationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
                }

                cooperationToUpdate.Id = saveNewCooperationResult.Value;
            }
            else {
                cooperationToUpdate = await _cooperationService.GetCooperationById(cooperationRequest.RequestedToId);
                var agreementSigners = JsonConvert.DeserializeObject<AgreementSignersVM>(cooperationToUpdate.AgreementSigners);
                
                agreementSigners.ExpectedSigner.SignersAsUser = new KeyValuePair<int, int>(
                    agreementSigners.ExpectedSigner.SignersAsUser.Key,
                    agreementSigners.ExpectedSigner.SignersAsUser.Value + newParticipants.UserIds.Count
                );
                
                newParticipants.OrganizationIds.ForEach(organizationId => agreementSigners.ExpectedSigner.SignerAsOrganization.Add(organizationId, false));
                cooperationToUpdate.AgreementSigners = JsonConvert.SerializeObject(agreementSigners);
            }
            
            var updateCooperationResult = await _cooperationService.UpdateCooperation(cooperationToUpdate);
            if (!updateCooperationResult.HasValue || !updateCooperationResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data."});
            }
            
            var addParticipantResult = await _cooperationService.AddParticipantsToCooperationById(cooperationToUpdate.Id, newParticipants);
            if (addParticipantResult == null) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }
            
            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = cooperationToUpdate.Id });
        }

        private PoolingParticipantsVM AddParticipantsFromRequestToCooperation(CooperationRequest cooperationRequest) {
            var participants = new PoolingParticipantsVM();

            if (cooperationRequest.RequestedByType.Equals(nameof(User))) participants.UserIds.Add(cooperationRequest.RequestedById);
            if (cooperationRequest.RequestedByType.Equals(nameof(Organization))) participants.OrganizationIds.Add(cooperationRequest.RequestedById);
            
            if (cooperationRequest.RequestedToType.Equals(nameof(User))) participants.UserIds.Add(cooperationRequest.RequestedToId);
            if (cooperationRequest.RequestedToType.Equals(nameof(Organization))) participants.OrganizationIds.Add(cooperationRequest.RequestedToId);
            
            return participants;
        }

        private async Task<CooperationRequest> SetResponseDataToCooperationRequestAndSignIt(CooperationRequest cooperationRequest, RequestResponseVM response) {
            var newResponseSignature = await MakeSignatureForResponse(response);
            if (newResponseSignature == null) return null;
            
            switch (cooperationRequest.RequestedToType) {
                case nameof(Cooperation): //Responder is a User participant or a member of Organization participant in the Cooperation
                    var cooperation = await _cooperationService.GetCooperationById(cooperationRequest.RequestedToId);
                    if (cooperation == null) return null;
                    
                    var cooperationRequestAcceptancePolicy = cooperation.RequestAcceptancePolicy;
                    if (!Helpers.IsProperString(cooperationRequestAcceptancePolicy)) return null;
                    
                    var (responderType, organizationId) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(response.ResponderId, cooperationRequest.RequestedToId);
                    if (!Helpers.IsProperString(responderType)) return null;

                    var requestAcceptancePolicy = JsonConvert.DeserializeObject<RequestAcceptancePolicyVM>(cooperationRequestAcceptancePolicy);
                    var isLastResponder = await _cooperationService.IsThisTheLastResponder(cooperationRequest.ResponderSignatures, cooperationRequest.RequestedToId, response.ResponderId);
                    if (!isLastResponder.HasValue) return null;

                    if (requestAcceptancePolicy.AcceptIfAllParticipantsAccept) {
                        cooperationRequest.IsAccepted = true;
                        cooperationRequest.AcceptedOn = DateTime.UtcNow;
                        cooperationRequest.AcceptanceNote = "Accepted by one-acceptance rule.";
                    }
                    else if (requestAcceptancePolicy.AcceptBasingOnMajority) {
                        var (accepterCount, rejecterCount, noResponseCount) = _cooperationService.GetCountsForAccepterRejecterAndNoResponse(cooperationRequest.ResponderSignatures, cooperation.ConfidedRequestResponderIds);
                        if (response.IsAccepted) accepterCount += 1;
                        else rejecterCount += 1;

                        if (requestAcceptancePolicy.EarlyAutoAccept && accepterCount * 1.0 / (rejecterCount + noResponseCount) > requestAcceptancePolicy.PercentageOfMajority) {
                            cooperationRequest.IsAccepted = true;
                            cooperationRequest.AcceptedOn = DateTime.UtcNow;
                            cooperationRequest.AcceptanceNote = "Early auto accepted by overwhelming acceptance rule.";
                        }
                        else {
                            var percentageOfMajority = accepterCount * 1.0 / rejecterCount;
                            var shouldBeDetermined = percentageOfMajority > (requestAcceptancePolicy.PercentageOfMajority - requestAcceptancePolicy.RangeForTurningToBeDetermined) &&
                                                     percentageOfMajority < (requestAcceptancePolicy.PercentageOfMajority + requestAcceptancePolicy.RangeForTurningToBeDetermined);

                            if (!shouldBeDetermined) {
                                cooperationRequest.IsAccepted = percentageOfMajority >= requestAcceptancePolicy.PercentageOfMajority;

                                if (cooperationRequest.IsAccepted.Value) {
                                    cooperationRequest.AcceptedOn = DateTime.UtcNow;
                                    cooperationRequest.AcceptanceNote = "Auto accepted by majority rule.";
                                }
                                else {
                                    cooperationRequest.RejectedOn = DateTime.UtcNow;
                                    cooperationRequest.AcceptanceNote = "Auto rejected by majority rule.";
                                }
                            }
                        }
                    }
                    else if (requestAcceptancePolicy.RejectIfOneParticipantReject && !response.IsAccepted) {
                        cooperationRequest.IsAccepted = false;
                        cooperationRequest.RejectedOn = DateTime.UtcNow;
                        cooperationRequest.AcceptanceNote = "Auto rejected by one-rejecter rule.";
                    }
                    else if (requestAcceptancePolicy.AcceptIfAllParticipantsAccept && isLastResponder.Value) {
                        var isAcceptedByOtherResponders = _cooperationService.DoOtherRespondersAcceptTheRequest(cooperationRequest.ResponderSignatures);
                        if (!isAcceptedByOtherResponders.HasValue) return null;
                        
                        cooperationRequest.IsAccepted = response.IsAccepted && isAcceptedByOtherResponders.Value;
                        cooperationRequest.AcceptedOn = DateTime.UtcNow;
                        cooperationRequest.AcceptanceNote = "Auto accepted by all-acceptance rule.";
                    }

                    var currentSignaturesOnRequest = Helpers.IsProperString(cooperationRequest.ResponderSignatures)
                        ? JsonConvert.DeserializeObject<SignaturePoolVM>(cooperationRequest.ResponderSignatures)
                        : new SignaturePoolVM();
                    
                    if (responderType.Equals(nameof(User)))
                        currentSignaturesOnRequest.UserSignatures.Add(newResponseSignature);
                    else //nameof(Organization)
                        currentSignaturesOnRequest.OrganizationSignatures.Add(
                            new DbOrganizationSignatureRecordVM {
                                OrganizationId = organizationId,
                                Signature = newResponseSignature
                            }
                        );
                    
                    cooperationRequest.ResponderSignatures = JsonConvert.SerializeObject(currentSignaturesOnRequest);

                    break;
                default: //nameof(Organization) or nameof(User)
                    cooperationRequest.IsAccepted = response.IsAccepted;
                    cooperationRequest.ResponderSignatures = JsonConvert.SerializeObject(newResponseSignature);
                    
                    if (response.IsAccepted) {
                        cooperationRequest.AcceptedOn = DateTime.UtcNow;
                        cooperationRequest.AcceptedById = response.ResponderId;
                    }
                    else {
                        cooperationRequest.RejectedOn = DateTime.UtcNow;
                        cooperationRequest.RejectedById = response.ResponderId;
                    }

                    break;
            }

            return cooperationRequest;
        }

        private async Task<DbSignatureRecordVM> MakeSignatureForResponse(RequestResponseVM response) {
            var responderRsaKey = await _userService.GetUserRsaKeyByUserId(response.ResponderId);
            if (responderRsaKey == null) return null;

            _rsaService.PrivateKey = responderRsaKey.PrivateKey;

            var signatureTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var signature = _rsaService.Sign($"{ response.ResponderId }.{ response.RequestId }.{ response.IsAccepted }.{ signatureTimestamp }");
            
            var newDbSignatureRecord = (DbSignatureRecordVM) response;
            newDbSignatureRecord.Timestamp = signatureTimestamp;
            newDbSignatureRecord.Signature = signature;

            return newDbSignatureRecord;
        }

        private async Task<bool?> IsUserAllowedToRespondToCooperationRequest(string requestedToType, int requestedToId, int responderId) {
            bool isAuthorized;
            switch (requestedToType) {
                case nameof(User):
                    isAuthorized = requestedToId != responderId;
                    break;
                case nameof(Organization):
                    var allowedToRespond = await _organizationService.IsUserBelongedToOrganizationAndAllowedToManageCooperation(responderId, requestedToId);
                    if (!allowedToRespond.HasValue) return null;
                    isAuthorized = allowedToRespond.Value;
                    break;
                default: //nameof(Cooperation)
                    var authorizedToRespond = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(responderId, requestedToId);
                    if (!authorizedToRespond.HasValue) return null;
                    isAuthorized = authorizedToRespond.Value;
                    break;
            }

            return isAuthorized;
        }

        [HttpPut("resolve-request/{requestId}/{isAccepted}")]
        public async Task<JsonResult> ResolveCooperationRequestAcceptance([FromHeader] int userId,[FromRoute] int requestId,[FromRoute] int isAccepted = 1) {
            var cooperationRequest = await _cooperationService.GetCooperationRequestById(requestId);
            if (cooperationRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var isAuthorized = await IsUserAllowedToRespondToCooperationRequest(cooperationRequest.RequestedToType, cooperationRequest.RequestedToId, userId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            cooperationRequest.IsAccepted = isAccepted == 1;
            if (cooperationRequest.IsAccepted.Value) {
                cooperationRequest.AcceptedOn = DateTime.UtcNow;
                cooperationRequest.AcceptedById = userId;
                cooperationRequest.AcceptanceNote = "Accepted by manual action.";
            }
            else {
                cooperationRequest.RejectedOn = DateTime.UtcNow;
                cooperationRequest.RejectedById = userId;
                cooperationRequest.AcceptanceNote = "Rejected by manual action.";
            }

            await _cooperationService.StartTransaction();

            var updateRequestResult = await _cooperationService.UpdateCooperationRequest(cooperationRequest);
            if (!updateRequestResult.HasValue || !updateRequestResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            var newParticipant = new CooperationParticipant {
                CooperationId = cooperationRequest.RequestedToId,
                ParticipantId = cooperationRequest.RequestedById,
                ParticipantType = cooperationRequest.RequestedByType,
                ParticipatedOn = DateTime.UtcNow
            };

            var saveParticipantResult = await _cooperationService.InsertNewCooperationParticipant(newParticipant);
            if (!saveParticipantResult.HasValue || saveParticipantResult.Value < 1) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPost("pooling-create")]
        public async Task<JsonResult> CreateCooperationFromPoolingParticipants(PoolingParticipantsVM participants) {
            await _cooperationService.StartTransaction();

            var newCooperation = Cooperation.GetDefaultInstance();
            newCooperation.AgreementSigners = JsonConvert.SerializeObject(
                new AgreementSignersVM {
                    ExpectedSigner = new ExpectedSignerVM {
                        SignersAsUser = new KeyValuePair<int, int>(0, participants.UserIds.Count),
                        SignerAsOrganization = participants.OrganizationIds.ToDictionary(orgId => orgId, _ => false)
                    }
                });
            
            var createCooperationResult = await _cooperationService.InsertNewCooperation(newCooperation);
            if (!createCooperationResult.HasValue || createCooperationResult.Value < 1) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            var addParticipantsResult = await _cooperationService.AddParticipantsToCooperationById(createCooperationResult.Value, participants);
            if (addParticipantsResult == null) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            foreach (var participantId in addParticipantsResult) {
                var saveSigningCheckerResult = await _cooperationService.InsertNewSigningChecker(
                    new SigningChecker {
                        CooperationParticipantId = participantId,
                        ForActivity = "Cooperation Created",
                        CreatedOn = DateTime.UtcNow,
                        IsValid = true
                    });
                
                if (saveSigningCheckerResult > 0) continue;
                
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }
            
            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = createCooperationResult.Value });
        }
        
        [HttpPut("sign/{cooperationId}")]
        public async Task<JsonResult> SignCooperationAgreement([FromHeader] int userId,[FromRoute] int cooperationId) {
            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to sign: cooperation has ended." });
            
            var (participantType, organizationId) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var (shouldSign, signingChecker) = await _cooperationService.SearchValidSigningCheckerForSigner(userId, cooperationId, participantType);
            if (!shouldSign.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldSign.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You do not need to sign anything." });

            var cooperation = await _cooperationService.GetCooperationById(cooperationId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var agreementSigners = JsonConvert.DeserializeObject<AgreementSignersVM>(cooperation.AgreementSigners);
            var userHasSigned = agreementSigners.Signers.Any(signer => signer.UserId == userId);
            if (userHasSigned) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You already signed on this cooperation." });

            var userRsaKeys = await _userService.GetUserRsaKeyByUserId(userId);
            if (userRsaKeys == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            _rsaService.PrivateKey = userRsaKeys.PrivateKey;
            var signatureTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            agreementSigners.Signers.Add(new SignerVM {
                UserId = userId,
                RsaKeyId = userRsaKeys.Id,
                Timestamp = signatureTimestamp,
                Signature = _rsaService.Sign($"{ userId }.{ cooperationId }.{ signatureTimestamp }")
            });

            switch (participantType) {
                case nameof(User):
                    agreementSigners.ExpectedSigner.SignersAsUser = new KeyValuePair<int, int>(
                        agreementSigners.ExpectedSigner.SignersAsUser.Key + 1,
                        agreementSigners.ExpectedSigner.SignersAsUser.Value
                    );
                    break;
                case nameof(Organization):
                    agreementSigners.ExpectedSigner.SignerAsOrganization[organizationId] = true;
                    break;
            }

            if (agreementSigners.ExpectedSigner.SignersAsUser.Key == agreementSigners.ExpectedSigner.SignersAsUser.Value &&
                agreementSigners.ExpectedSigner.SignerAsOrganization.All(signer => signer.Value)
            ) {
                cooperation.IsInEffect = true;
                cooperation.StartedOn = DateTime.UtcNow;
            }

            cooperation.AgreementSigners = JsonConvert.SerializeObject(agreementSigners);
            await _cooperationService.StartTransaction();
            
            var updateCooperationResult = await _cooperationService.UpdateCooperation(cooperation);
            if (!updateCooperationResult.HasValue || !updateCooperationResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            CooperationParticipant participant;
            if (participantType.Equals(nameof(User))) participant = await _cooperationService.GetCooperationParticipantBy(userId, cooperationId, nameof(User));
            else participant = await _cooperationService.GetCooperationParticipantBy(organizationId, cooperationId, nameof(Organization));
            
            if (participant == null) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            }

            participant.IsActive = true;
            var updateParticipantResult = await _cooperationService.UpdateCooperationParticipant(participant);
            if (!updateParticipantResult.HasValue || !updateParticipantResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            signingChecker.IsValid = false;
            signingChecker.InvalidOn = DateTime.UtcNow;
            var updateSigningCheckerResult = await _cooperationService.UpdateSigningChecker(signingChecker);
            if (!updateSigningCheckerResult.HasValue || !updateSigningCheckerResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("add-participant/{cooperationId}/{participantUniqueId}/{participantType}")]
        public async Task<JsonResult> AddParticipantToCooperation(
            [FromHeader] int userId,[FromRoute] int cooperationId,[FromRoute] string participantUniqueId,[FromRoute] string participantType
        ) {
            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to add: cooperation has ended." });
            
            var authorizedToAdd = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, cooperationId);
            if (!authorizedToAdd.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!authorizedToAdd.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            User userParticipant = null;
            Organization organizationParticipant = null;
            if (participantType.Equals(nameof(User))) userParticipant = await _userService.GetUserByUniqueId(participantUniqueId);
            else organizationParticipant = await _organizationService.GetOrganizationByUniqueId(participantUniqueId);
            
            if (userParticipant == null && organizationParticipant == null)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var cooperation = await _cooperationService.GetCooperationById(cooperationId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var cooperationParticipant = new CooperationParticipant {
                CooperationId = cooperationId,
                ParticipantId = userParticipant?.Id ?? organizationParticipant.Id,
                ParticipantType = participantType,
                ParticipatedOn = DateTime.UtcNow
            };

            await _cooperationService.StartTransaction();
            
            var saveParticipantResult = await _cooperationService.InsertNewCooperationParticipant(cooperationParticipant);
            if (!saveParticipantResult.HasValue || saveParticipantResult.Value < 1) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            var saveSigningCheckerResult = await _cooperationService.InsertNewSigningChecker(
                new SigningChecker {
                    CooperationParticipantId = saveParticipantResult.Value,
                    ForActivity = "Added to cooperation",
                    CreatedOn = DateTime.UtcNow,
                    IsValid = true
                });
            if (!saveSigningCheckerResult.HasValue || saveSigningCheckerResult.Value < 1) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            var agreementSigner = JsonConvert.DeserializeObject<AgreementSignersVM>(cooperation.AgreementSigners);
            if (participantType.Equals(nameof(User)))
                agreementSigner.ExpectedSigner.SignersAsUser = new KeyValuePair<int, int>(
                    agreementSigner.ExpectedSigner.SignersAsUser.Key,
                    agreementSigner.ExpectedSigner.SignersAsUser.Value + 1
                );
            else
                agreementSigner.ExpectedSigner.SignerAsOrganization.Add(organizationParticipant.Id, false);

            cooperation.AgreementSigners = JsonConvert.SerializeObject(agreementSigner);
            var saveCooperationResult = await _cooperationService.UpdateCooperation(cooperation);
            if (!saveCooperationResult.HasValue || !saveCooperationResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("update-preference")]
        public async Task<JsonResult> UpdateCooperationPreference([FromHeader] int userId,[FromBody] CooperationPreferenceVM preference) {
            var errors = preference.VerifyPreference();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
            
            var isCooperationActive = await _cooperationService.IsCooperationActive(preference.CooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to update: cooperation has ended." });
            
            var authorizedToAdd = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, preference.CooperationId);
            if (!authorizedToAdd.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!authorizedToAdd.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var unauthorizedIds = new List<int>();
            if (preference.ConfidedRequestResponderIds.Length != 0)
                foreach (var confidedId in preference.ConfidedRequestResponderIds) {
                    var (participantType, _) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(confidedId, preference.CooperationId);
                    if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                    if (participantType.Length == 0) unauthorizedIds.Add(confidedId);
                }

            if (unauthorizedIds.Count != 0) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Denied,
                Data = unauthorizedIds,
                Message = "Found unauthorized users in the confided responders list."
            });

            var cooperation = await _cooperationService.GetCooperationById(preference.CooperationId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            await _cooperationService.StartTransaction();
            
            if (preference.RequireSigning) {
                var (isSuccess, updatedCooperation) = await ClearAgreementSigningAndCreateSigningCheckers(cooperation);
                if (!isSuccess) {
                    await _cooperationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
                }

                cooperation = updatedCooperation;
            }
            
            cooperation.AllowAnyoneToRespondRequest = preference.AllowAnyoneToResponseRequest;
            cooperation.ConfidedRequestResponderIds = JsonConvert.SerializeObject(preference.ConfidedRequestResponderIds);
            cooperation.RequestAcceptancePolicy = JsonConvert.SerializeObject(preference.RequestAcceptancePolicy);
            
            var updateResult = await _cooperationService.UpdateCooperation(cooperation, preference.RequireSigning);
            if (!updateResult.HasValue || !updateResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPut("update")]
        public async Task<JsonResult> UpdateCooperation([FromHeader] int userId,[FromBody] TermsAndConditionsVM termsAndConditions) {
            var error = termsAndConditions.VerifyData();
            if (error.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = error });

            var isCooperationActive = await _cooperationService.IsCooperationActive(termsAndConditions.CooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to update: cooperation has ended." });
            
            var authorizedToAdd = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, termsAndConditions.CooperationId);
            if (!authorizedToAdd.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!authorizedToAdd.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var cooperation = await _cooperationService.GetCooperationById(termsAndConditions.CooperationId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            cooperation.TermsAndConditions = termsAndConditions.TermsAndConditions;
            cooperation.Name = termsAndConditions.Name;

            await _cooperationService.StartTransaction();
            
            if (termsAndConditions.RequireSigning) {
                var (isSuccess, updatedCooperation) = await ClearAgreementSigningAndCreateSigningCheckers(cooperation);
                if (!isSuccess) {
                    await _cooperationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
                }

                cooperation = updatedCooperation;
            }
            
            var updateResult = await _cooperationService.UpdateCooperation(cooperation, termsAndConditions.RequireSigning);
            if (!updateResult.HasValue || !updateResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        private async Task<KeyValuePair<bool, Cooperation>> ClearAgreementSigningAndCreateSigningCheckers(Cooperation cooperation) {
            cooperation.IsInEffect = false;
            var agreementSigners = JsonConvert.DeserializeObject<AgreementSignersVM>(cooperation.AgreementSigners);

            agreementSigners.Signers.Clear();
            agreementSigners.ExpectedSigner.SignersAsUser = new KeyValuePair<int, int>(0, agreementSigners.ExpectedSigner.SignersAsUser.Value);
            agreementSigners.ExpectedSigner.SignerAsOrganization = agreementSigners.ExpectedSigner.SignerAsOrganization.ToDictionary(signer => signer.Key, _ => false);

            cooperation.AgreementSigners = JsonConvert.SerializeObject(agreementSigners);

            var cooperationParticipants = await _cooperationService.GetCooperationParticipantsFor(cooperation.Id);
            foreach (var participant in cooperationParticipants) {
                var signingChecker = new SigningChecker {
                    CooperationParticipantId = participant.Id,
                    ForActivity = "Cooperation Updated",
                    CreatedOn = DateTime.UtcNow,
                    IsValid = true
                };

                var saveSigningCheckerResult = await _cooperationService.InsertNewSigningChecker(signingChecker);
                if (!saveSigningCheckerResult.HasValue || saveSigningCheckerResult.Value < 1) return new KeyValuePair<bool, Cooperation>(false, null);
            }

            return new KeyValuePair<bool, Cooperation>(true, cooperation);
        }

        // [HttpPost("add-document")]
        // public async Task<JsonResult> AddSupportedDocumentsToCooperation() {
        //     
        // }
        
        [HttpPost("grant-access")]
        public async Task<JsonResult> GrantDepartmentAccess([FromHeader] int userId,[FromBody] DepartmentAccessVM departmentAccessData) {
            var (error, falseDepartmentIds) = await GetErrorsUnderlyingDepartmentAccessRequest(userId, departmentAccessData);
            switch (error) {
                case 0:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to update access: cooperation has ended." });
                case 2:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "\"From\" Participant not found in the cooperation." });
                case 3:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "\"To\" Participant not found in the cooperation." });
                case 4:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }
            
            if (falseDepartmentIds.Length != 0) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Denied,
                Data = falseDepartmentIds,
                Message = "Found inexisted or inaccessible departments."
            });
            
            var departmentAccess = (DepartmentAccess) departmentAccessData;
            var saveResult = await _cooperationService.InsertNewDepartmentAccess(departmentAccess);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpPut("update-access")]
        public async Task<JsonResult> UpdateDepartmentAccess([FromHeader] int userId,[FromBody] DepartmentAccessVM departmentAccessData) {
            var (error, falseDepartmentIds) = await GetErrorsUnderlyingDepartmentAccessRequest(userId, departmentAccessData);
            switch (error) {
                case 0:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to update access: cooperation has ended." });
                case 2:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "\"From\" Participant not found in the cooperation." });
                case 3:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "\"To\" Participant not found in the cooperation." });
                case 4:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }
            
            if (falseDepartmentIds.Length != 0) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Denied,
                Data = falseDepartmentIds,
                Message = "Found inexisted or inaccessible departments."
            });

            var departmentAccess = await _cooperationService.GetDepartmentAccessById(departmentAccessData.Id);
            if (departmentAccess == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            departmentAccess.UpdateDataBy(departmentAccessData);
            
            var updateResult = await _cooperationService.UpdateDepartmentAccess(departmentAccess);
            return !updateResult.HasValue || !updateResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        private async Task<KeyValuePair<int, int[]>> GetErrorsUnderlyingDepartmentAccessRequest(int userId, DepartmentAccessVM departmentAccess) {
            var isCooperationActive = await _cooperationService.IsCooperationActive(departmentAccess.CooperationId);
            if (!isCooperationActive.HasValue) return new KeyValuePair<int, int[]>(0, null);
            if (!isCooperationActive.Value) return new KeyValuePair<int, int[]>(1, null);
            
            var isAParticipantInCooperation = await _cooperationService.DoesCooperationHaveThisParticipant(departmentAccess.FromParticipantId, departmentAccess.CooperationId, nameof(Organization));
            if (!isAParticipantInCooperation.HasValue) return new KeyValuePair<int, int[]>(0, null);
            if (!isAParticipantInCooperation.Value) return new KeyValuePair<int, int[]>(2, null);
            
            isAParticipantInCooperation = await _cooperationService.DoesCooperationHaveThisParticipant(departmentAccess.AccessGivenToParticipantId, departmentAccess.CooperationId);
            if (!isAParticipantInCooperation.HasValue) return new KeyValuePair<int, int[]>(0, null);
            if (!isAParticipantInCooperation.Value) return new KeyValuePair<int, int[]>(3, null);

            var isAuthorized = await _organizationService.IsUserBelongedToOrganizationAndAllowedToManageCooperation(userId, departmentAccess.FromParticipantId);
            if (!isAuthorized.HasValue) return new KeyValuePair<int, int[]>(0, null);
            if (!isAuthorized.Value) return new KeyValuePair<int, int[]>(4, null);

            var falseDepartmentIds = new List<int>();
            foreach (var departmentId in departmentAccess.AccessibleDepartmentIds) {
                var existedAndAccessible = await _organizationService.IsThisDepartmentExistedAndForCooperation(departmentId, departmentAccess.FromParticipantId);
                if (!existedAndAccessible.HasValue) return new KeyValuePair<int, int[]>(0, null);
                if (!existedAndAccessible.Value) falseDepartmentIds.Add(departmentId);
            }

            return falseDepartmentIds.Count != 0
                ? new KeyValuePair<int, int[]>(5, falseDepartmentIds.ToArray())
                : new KeyValuePair<int, int[]>(-1, null);
        }

        [HttpPut("leave/{participantId}")]
        public async Task<JsonResult> LeaveCooperation([FromHeader] int userId,[FromRoute] int cooperationParticipantId) {
            //Todo: check if there is any assigned tasks in this participant task vault
            //Todo: check if this participant leaves, is there anyone else allowed to manage request/cooperation, if none, set allowanyonerespond... to true
            var cooperationParticipant = await _cooperationService.GetCooperationParticipantById(cooperationParticipantId);
            if (cooperationParticipant == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationParticipant.CooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to leave: cooperation has ended." });
            
            var (participantType, _) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationParticipant.CooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            cooperationParticipant.IsActive = false;
            cooperationParticipant.LeftOn = DateTime.UtcNow;

            var updateResult = await _cooperationService.UpdateCooperationParticipant(cooperationParticipant);
            if (!updateResult.HasValue || !updateResult.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            await _cooperationLog.InsertCooperationParticipantLog(new CooperationParticipantLog {
                ParticipantId = cooperationParticipant.Id,
                Activity = "Leave the cooperation",
                Timestamp = DateTime.UtcNow
            });
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("request-to-return")]
        public async Task<JsonResult> MakeRequestToReturnToCooperation([FromHeader] int userId,[FromBody] ReturnRequestVM returnRequest) {
            var cooperationParticipant = await _cooperationService.GetCooperationParticipantById(returnRequest.CooperationParticipantId);
            if (cooperationParticipant == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationParticipant.CooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to return: cooperation has ended." });
            
            var (participantType, _) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationParticipant.CooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var dbReturnRequest = new ParticipantReturnRequest {
                CooperationParticipantId = returnRequest.CooperationParticipantId,
                Message = returnRequest.Message,
                RequestedOn = DateTime.UtcNow
            };

            var saveResult = await _cooperationService.InsertNewParticipantReturnRequest(dbReturnRequest);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpDelete("cancel-request-to-return/{requestId}")]
        public async Task<JsonResult> RevokeRequestToReturn([FromHeader] int userId,[FromRoute] int requestId) {
            var isAuthorized = await _cooperationService.IsReturnRequestBelongedToThisUser(requestId, userId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var deleteResult = await _cooperationService.RemoveParticipantReturnRequestById(requestId);
            return !deleteResult.HasValue || !deleteResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        
        [HttpPut("respond-to-request-to-return")]
        public async Task<JsonResult> RespondToRequestToReturnToCooperation([FromHeader] int userId,[FromBody] ReturnRequestResponseVM response) {
            var error = response.VerifyNote();
            if (error.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = error });
            
            var cooperation = await _cooperationService.SearchForCooperationFromParticipantReturnRequest(response.RequestId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var isAuthorized = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, cooperation.Id);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var returnRequest = await _cooperationService.GetParticipantReturnRequestById(response.RequestId);
            if (returnRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var userRsaKeys = await _userService.GetUserRsaKeyByUserId(userId);
            if (userRsaKeys == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            _rsaService.PrivateKey = userRsaKeys.PrivateKey;
            var signatureTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            returnRequest.IsAccepted = response.IsAccepted;
            returnRequest.RespondedById = userId;
            returnRequest.RespondedOn = DateTime.UtcNow;
            returnRequest.RespondNote = JsonConvert.SerializeObject(new DbSignatureRecordVM {
                ResponderId = userId,
                IsAccepted = response.IsAccepted,
                Timestamp = signatureTimestamp,
                Note = response.Note,
                Signature = _rsaService.Sign($"{ userId }.{ returnRequest.Id }.{ response.IsAccepted }.{ signatureTimestamp }")
            });

            await _cooperationService.StartTransaction();
            
            var updateRequestResult = await _cooperationService.UpdateParticipantReturnRequest(returnRequest);
            if (!updateRequestResult.HasValue || !updateRequestResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            var signingChecker = new SigningChecker {
                CooperationParticipantId = returnRequest.CooperationParticipantId,
                CreatedOn = DateTime.UtcNow,
                ForActivity = nameof(ParticipantReturnRequest)
            };

            var saveSigningCheckerResult = await _cooperationService.InsertNewSigningChecker(signingChecker);
            if (!saveSigningCheckerResult.HasValue || saveSigningCheckerResult.Value < 1) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPut("end/{cooperationId}")]
        public async Task<JsonResult> EndCooperation([FromHeader] int userId,[FromRoute] int cooperationId) {
            var isAuthorized = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, cooperationId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to end: cooperation has ended." });

            var cooperation = await _cooperationService.GetCooperationById(cooperationId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var user = await _userService.GetUserById(userId);
            if (user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            cooperation.IsInEffect = false;
            cooperation.EndedOn = DateTime.UtcNow;

            var updateCooperationResult = await _cooperationService.UpdateCooperation(cooperation);
            if (!updateCooperationResult.HasValue || !updateCooperationResult.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            await _cooperationLog.InsertCooperationParticipantLog(new CooperationLog {
                CooperationId = cooperationId,
                Activity = $"Cooperation was deactivated and ended by { user.FirstName } { user.LastName }.",
                Timestamp = DateTime.UtcNow
            });

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPut("revive/{cooperationId}")]
        public async Task<JsonResult> ReviveCooperation([FromHeader] int userId,[FromRoute] int cooperationId) {
            var isAuthorized = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, cooperationId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var isCooperationActive = await _cooperationService.IsCooperationActive(cooperationId);
            if (!isCooperationActive.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (isCooperationActive.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Unable to revive: cooperation is active." });
            
            var user = await _userService.GetUserById(userId);
            if (user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var reviveCooperationResult = await _cooperationService.ReviveCooperationAndRequireSignaturesIfNeeded(cooperationId);
            if (!reviveCooperationResult.HasValue || !reviveCooperationResult.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            
            await _cooperationLog.InsertCooperationParticipantLog(new CooperationLog {
                CooperationId = cooperationId,
                Activity = $"Cooperation was re-activated by { user.FirstName } { user.LastName }.",
                Timestamp = DateTime.UtcNow
            });
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-sent-requests/{sentById}/{sentByType}")]
        public async Task<JsonResult> GetCooperationRequestsSentBy([FromRoute] int sentById,[FromRoute] string sentByType = nameof(User)) { //or Organization
            var sentCooperationRequests = await _cooperationService.GetCooperationRequestsSentBy(sentById, sentByType);
            return sentCooperationRequests == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = sentCooperationRequests });
        }
        
        [HttpGet("get-received-requests/{receivedById}/{receivedByType}")]
        public async Task<JsonResult> GetCooperationRequestsReceivedBy([FromHeader] int userId,[FromRoute] int receivedById,[FromRoute] string receivedByType = nameof(User)) { //or Organization, or Cooperation
            var allowRespond = true;
            var organizationId = -1;
            if (receivedByType.Equals(nameof(Cooperation))) {
                var (participantType, orgId) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, receivedById);
                if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "You are not authorized for this action." });

                var isAuthorized = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, receivedById);
                if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

                allowRespond = isAuthorized.Value;
                organizationId = orgId;
            }

            var cooperationRequests = await _cooperationService.GetCooperationRequestsReceivedBy(receivedById, receivedByType);
            if (cooperationRequests == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            foreach (var requestDetail in cooperationRequests)
            {
                var hasBeenResponded = await _cooperationService.HasThisCooperationRequestBeenRespondedBy(userId, requestDetail.Id, organizationId);

                if (!hasBeenResponded.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                
                requestDetail.AllowRespond = allowRespond && !hasBeenResponded.Value;
            }
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = cooperationRequests });
        }
        
        /// <summary>
        /// If a user wants to know which cooperations they have
        /// </summary>
        [HttpGet("get-for-user/{activeParticipant}")]
        public async Task<JsonResult> GetCooperationsHavingUserActive([FromHeader] int userId,[FromRoute] int activeParticipant = 1) {
            var cooperations = await _cooperationService.GetCooperationsByUserId(userId, activeParticipant == 1);
            return cooperations == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                        : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = cooperations });
        }
        
        /// <summary>
        /// If someone in an organization (with permission to manage cooperation) wants to know which cooperations their organization has
        /// </summary>
        [HttpGet("get-for-organization/{organizationId}")]
        public async Task<JsonResult> GetCooperationsHavingOrganization([FromHeader] int userId,[FromHeader] int organizationId) {
            var isAuthorized = await _organizationService.IsUserBelongedToOrganizationAndAllowedToManageCooperation(userId, organizationId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "You are not authorized for this action." });
            
            var cooperations = await _cooperationService.GetCooperationsByOrganizationId(organizationId);
            return cooperations == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                        : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = cooperations });
        }
        
        [HttpGet("details/{cooperationId}")]
        public async Task<JsonResult> GetCooperationDetails([FromRoute] int cooperationId) {
            var cooperationDetail = await _cooperationService.GetCooperationDetailsFor(cooperationId);
            return cooperationDetail == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                             : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = cooperationDetail });
        }
        
        [HttpGet("get-participants/{cooperationId}")]
        public async Task<JsonResult> GetCooperationParticipants([FromHeader] int userId,[FromRoute] int cooperationId) {
            var (participantType, _) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "You are not authorized for this action." });

            var participants = await _cooperationService.GetCooperationParticipantsFor(cooperationId);
            return participants == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                        : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = participants });
        }
        
        [HttpGet("get-return-requests/{cooperationId}")]
        public async Task<JsonResult> GetParticipantReturnRequestsForCooperation([FromHeader] int userId,[FromRoute] int cooperationId) {
            var (participantType, _) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "You are not authorized for this action." });

            var returnRequests = await _cooperationService.GetReturnRequestsByCooperationId(cooperationId);
            if (returnRequests == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var allowRespond = await _cooperationService.IsUserAParticipantAllowedToManageCooperationAndRequest(userId, cooperationId);
            if (allowRespond == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            foreach (var returnRequest in returnRequests) returnRequest.AllowRespond = allowRespond.Value;
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = returnRequests });
        }
        
        /// <summary>
        /// When a participant wants to know what departments they have the access to,
        /// get the departments that a participant has access to.
        /// Those departments are grouped by its organization that gives the access.
        /// </summary>
        [HttpGet("get-accessible-departments/{participantId}/{cooperationId}")]
        public async Task<JsonResult> GetDepartmentsAccessibleByParticipant([FromRoute] int participantId,[FromRoute] int cooperationId) {
            var isAuthorized = await _cooperationService.DoesCooperationHaveThisParticipant(participantId, cooperationId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "You are not authorized for this action." });

            var accessibleDepartments = await _cooperationService.GetDepartmentsAccessibleBy(participantId, cooperationId);
            return accessibleDepartments == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                                 : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = accessibleDepartments });
        }
        
        /// <summary>
        /// Check if userId is someone within the organization and having permission to manage cooperation.
        /// View the departments that a participant has access to, those departments should be in the viewer's organization
        /// </summary>
        [HttpGet("get-departments-with-accessibility/{organizationId}/{participantId}")]
        public async Task<JsonResult> GetDepartmentsWithAccessibilityStatus([FromHeader] int userId,[FromRoute] int organizationId,[FromRoute] int participantId) {
            var isAuthorized = await _organizationService.IsUserBelongedToOrganizationAndAllowedToManageCooperation(userId, organizationId);
            if (!isAuthorized.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isAuthorized.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "You are not authorized for this action." });

            var departmentsGivenAccess = await _cooperationService.GetOrganizationDepartmentsGivenAccessToParticipant(organizationId, participantId);
            return departmentsGivenAccess == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                                  : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = departmentsGivenAccess });
        }
        
        /// <summary>
        /// Supporting a user needs to sign a cooperation agreement within a cooperation.
        /// </summary>
        [HttpGet("get-signing-tasks")]
        public async Task<JsonResult> ShouldSignCooperationAgreement([FromHeader] int userId) {
            var signingTasks = await _cooperationService.GetSigningTasksFromSigningCheckerByUserId(userId);
            return signingTasks == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                        : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = signingTasks });
        }
    }
}