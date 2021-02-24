using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
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

        public CooperationController(
            ICooperationService cooperationService,
            IOrganizationService organizationService,
            IUserService userService,
            IRsaService rsaService,
            IUserNotifier userNotifier
        ) : base(userNotifier) {
            _cooperationService = cooperationService;
            _organizationService = organizationService;
            _userService = userService;
            _rsaService = rsaService;
        }

        [HttpPost("request")]
        public async Task<JsonResult> MakeCooperationRequest(CooperationRequestVM cooperationRequest) {
            
            
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
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
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
                case nameof(Cooperation): //Responder is a User or a member of Organization in the Cooperation
                    var cooperationRequestAcceptancePolicy = await _cooperationService.GetRequestAcceptancePolicy(cooperationRequest.RequestedToId);
                    if (!Helpers.IsProperString(cooperationRequestAcceptancePolicy)) return null;
                    
                    var (responderType, organizationId) = _cooperationService.IsResponderAUserOrAnOrganizationMember(cooperationRequest.RequestedToId, response.ResponderId);
                    if (!Helpers.IsProperString(responderType)) return null;

                    var requestAcceptancePolicy = JsonConvert.DeserializeObject<RequestAcceptancePolicyVM>(cooperationRequestAcceptancePolicy);
                    var isLastResponder = _cooperationService.IsThisTheLastResponder(cooperationRequest.ResponderSignatures, cooperationRequest.RequestedToId, response.ResponderId);

                    if (requestAcceptancePolicy.AcceptBasingOnMajority) {
                        var (accepterCount, rejecterCount, noResponseCount) = _cooperationService.GetCountsForAccepterRejecterAndNoResponse(cooperationRequest.ResponderSignatures);
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
                    else if (requestAcceptancePolicy.AcceptIfAllParticipantsAccept && isLastResponder) {
                        var isAcceptedByOtherResponders = _cooperationService.DoOtherRespondersAcceptTheRequest(cooperationRequest.ResponderSignatures);
                        cooperationRequest.IsAccepted = response.IsAccepted && isAcceptedByOtherResponders;
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
                    var authorizedToRespond = await _cooperationService.IsResponderAParticipantAllowedToRespondRequest(responderId, requestedToId);
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
            if (!addParticipantsResult.HasValue || !addParticipantsResult.Value) {
                await _cooperationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            await _cooperationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = createCooperationResult.Value });
        }
        
        [HttpPut("sign/{cooperationId}")]
        public async Task<JsonResult> SignCooperationAgreement([FromHeader] int userId,[FromRoute] int cooperationId) {
            var (participantType, organizationId) = await _cooperationService.IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(userId, cooperationId);
            if (participantType == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (participantType.Length == 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var cooperation = await _cooperationService.GetCooperationById(cooperationId);
            if (cooperation == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var agreementSigners = JsonConvert.DeserializeObject<AgreementSignersVM>(cooperation.AgreementSigners);
            var userHasSigned = agreementSigners.Signers.Any(signer => signer.UserId == userId);
            if (userHasSigned) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You already signed on this cooperation." });

            var userRsaKeys = await _userService.GetUserRsaKeyByUserId(userId);

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
            var updateResult = await _cooperationService.UpdateCooperation(cooperation);

            return !updateResult.HasValue || !updateResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("add-participant")]
        public async Task<JsonResult> AddParticipantToCooperation() {
            
        }
        
        // [HttpPost("add-document")]
        // public async Task<JsonResult> AddSupportedDocumentsToCooperation() {
        //     
        // }
        
        [HttpPost("grant-access")]
        public async Task<JsonResult> GrantDepartmentAccess() {
            
        }
        
        [HttpPut("update-access")]
        public async Task<JsonResult> UpdateDepartmentAccess() {
            
        }
        
        [HttpPost("create-task-vault")]
        public async Task<JsonResult> CreateCooperationTaskVault() {
            
        }
        
        [HttpPut("update-task-vault")]
        public async Task<JsonResult> UpdateCooperationTaskVault() {
            
        }
        
        [HttpPut("end")]
        public async Task<JsonResult> EndCooperation() {
            
        }

        [HttpPut("leave")]
        public async Task<JsonResult> LeaveCooperation() {
            
        }

        [HttpGet("get-sent-requests")]
        public async Task<JsonResult> GetCooperationRequestsSentByOrganizationOrUser() {
            
        }
        
        [HttpGet("get-received-requests-by-user")]
        public async Task<JsonResult> GetCooperationRequestsReceivedByUser() {
            
        }
        
        [HttpGet("get-received-requests-by-organization")]
        public async Task<JsonResult> GetCooperationRequestsReceivedByOrganization() {
            
        }
        
        [HttpGet("get-received-requests-by-coop")]
        public async Task<JsonResult> GetCooperationRequestsReceivedByCooperation() {
            
        }
        
        [HttpGet("get")]
        public async Task<JsonResult> GetCooperationsHavingUserOrOrganization() {
            
        }

        [HttpPut("update-preference")]
        public async Task<JsonResult> UpdateCooperationPreference() {
            
        }
        
        [HttpPut("update-tnc")]
        public async Task<JsonResult> UpdateCooperationTermsAndConditions() {
            
        }
    }
}