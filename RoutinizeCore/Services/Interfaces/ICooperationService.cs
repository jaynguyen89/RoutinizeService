using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Cooperation;

namespace RoutinizeCore.Services.Interfaces {

    public interface ICooperationService : IDbServiceBase {

        Task<CooperationParticipant> SearchCooperationParticipantBy(int userId, int cooperationId, string participantType);
        
        Task<int?> InsertNewCooperation(Cooperation cooperation);
        
        /// <summary>
        /// Insert 1 CooperationParticipant instance for each participant in `participants` and create SigningChecker.
        /// </summary>
        Task<int[]> AddParticipantsToCooperationById(int cooperationId, PoolingParticipantsVM participants);
        
        Task<CooperationRequest> GetCooperationRequestById(int requestId);
        
        Task<bool?> UpdateCooperationRequest(CooperationRequest cooperationRequest);
        
        /// <summary>
        /// Check if a participant in cooperation is allowed to manage cooperation request by cooperation policy
        /// </summary>
        Task<bool?> IsUserAParticipantAllowedToManageCooperationAndRequest(int userId, int cooperationId);
        
        bool IsThisTheLastResponder(string responderSignatures, int cooperationId, int responderId);
        
        bool DoOtherRespondersAcceptTheRequest(string responderSignatures);
        
        /// <summary>
        /// Key == null for error. Key == string.Empty for not authorized.
        /// Key == nameof(User) if cooperator is a User participant, Value == 0 accordingly
        /// Key == nameof(Organization) if cooperator is an Organization participant, Value == organizationId
        /// </summary>
        Task<KeyValuePair<string, int>> IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(int userId, int cooperationId);
        
        /// <summary>
        /// Simply check if a participant is found in a cooperation. Returns boolean for existed/inexisted, null for error.
        /// participantType == null -> check for both types of participant
        /// participantType == nameof(User) || nameof(Organization) -> check for the specified type of participant
        /// </summary>
        Task<bool?> DoesCooperationHaveThisParticipant(int participantId, int cooperationId, string participantType = null);
        
        Tuple<int, int, int> GetCountsForAccepterRejecterAndNoResponse(string responderSignatures);
        
        Task<bool?> RemoveCooperationRequest(CooperationRequest cooperationRequest);
        
        Task<bool?> IsCooperationRequestAlreadyMade(CooperationRequestVM cooperationRequest);
        
        Task<int?> InsertNewCooperationRequest(CooperationRequest cooperationRequest);
        
        /// <summary>
        /// Insert 1 CooperationParticipant instance. Create SigningChecker after insertion done.
        /// </summary>
        Task<int?> InsertNewCooperationParticipant(CooperationParticipant participant);
        
        /// <summary>
        /// Inserts 1 Cooperation instance. RequireSigning == true -> also insert a SigningChecker for each of its participants afterwards.
        /// </summary>
        Task<bool?> UpdateCooperation(Cooperation newCooperation, bool requireSigning = false);
        
        Task<Cooperation> GetCooperationById(int cooperationId);
        
        Task<int?> InsertNewDepartmentAccess(DepartmentAccess departmentAccess);
        
        Task<bool?> UpdateDepartmentAccess(object departmentAccess);
        
        Task<CooperationParticipant> GetCooperationParticipantById(int cooperationParticipantId);
        
        Task<bool?> UpdateCooperationParticipant(CooperationParticipant cooperationParticipant);
        
        Task<bool?> IsCooperationActive(int cooperationId);
        
        Task<int?> InsertNewParticipantReturnRequest(ParticipantReturnRequest returnRequest);
        
        Task<bool?> IsReturnRequestBelongedToThisUser(int requestId, int userId);
        
        Task<bool?> RemoveParticipantReturnRequestById(int requestId);
        
        Task<Cooperation> SearchForCooperationFromParticipantReturnRequest(int requestId);
        
        Task<ParticipantReturnRequest> GetParticipantReturnRequestById(int responseRequestId);
        
        Task<bool?> UpdateParticipantReturnRequest(ParticipantReturnRequest returnRequest);
        
        Task<KeyValuePair<bool?, SigningChecker>> SearchValidSigningCheckerForSigner(int userId, int cooperationId, string participantType);
        
        Task<int?> InsertNewSigningChecker(SigningChecker signingChecker);
        
        Task<bool?> UpdateSigningChecker(SigningChecker signingChecker);
        
        Task<bool?> ReviveCooperationAndRequireSignaturesIfNeeded(int cooperationId);
        
        Task<CooperationRequestDetailVM[]> GetCooperationRequestsSentBy(int sentById, string sentByType);
        
        Task<CooperationRequestDetailVM[]> GetCooperationRequestsReceivedBy(int participantId, string participantType);
        
        Task<CooperationDetailVM> GetCooperationDetailsFor(int cooperationId);
        
        Task<CooperationVM[]> GetCooperationsByUserId(int userId);
        
        Task<CooperationVM[]> GetCooperationsByOrganizationId(int organizationId);
        
        Task<CooperationParticipantVM[]> GetCooperationParticipantsFor(int cooperationId);
        
        Task<ReturnRequestDetailVM[]> GetReturnRequestsByCooperationId(int cooperationId);
        
        Task<AccessibleDepartmentVM[]> GetDepartmentsAccessibleTo(int participantId, int cooperationId);
        
        Task<bool?> IsAlreadyAParticipantInCooperation(int requestedById, string requestedByType, int cooperationId);
        
        Task<bool?> AreTheyAlreadyCooperating(CooperationRequestVM cooperationRequest);
        
        Task<DepartmentAccessDetailVM[]> GetOrganizationDepartmentsGivenAccessToParticipant(int organizationId, int participantId);
    }
}