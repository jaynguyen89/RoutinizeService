using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Cooperation;

namespace RoutinizeCore.Services.Interfaces {

    public interface ICooperationService : IDbServiceBase {


        Task<int?> InsertNewCooperation(Cooperation cooperation);
        
        Task<bool?> AddParticipantsToCooperationById(int cooperationId, PoolingParticipantsVM participants);
        
        Task<CooperationRequest> GetCooperationRequestById(int requestId);
        
        Task<bool?> IsResponderAParticipantAllowedToRespondRequest(int userId, int requestedToId);
        
        Task<bool?> UpdateCooperationRequest(CooperationRequest cooperationRequest);
        
        bool IsThisTheLastResponder(string responderSignatures, int cooperationId, int responderId);
        
        bool DoOtherRespondersAcceptTheRequest(string responderSignatures);

        KeyValuePair<string, int> IsResponderAUserOrAnOrganizationMember(int cooperationId, int responderId);
        
        Task<string> GetRequestAcceptancePolicy(int cooperationId);
        
        Tuple<int, int, int> GetCountsForAccepterRejecterAndNoResponse(string cooperationRequestResponderSignatures);
        
        Task<bool?> RemoveCooperationRequest(CooperationRequest cooperationRequest);
        
        Task<bool?> IsCooperationRequestAlreadyMade(CooperationRequestVM cooperationRequest);
        
        Task<int?> InsertNewCooperationRequest(CooperationRequest cooperationRequest);
        
        Task<int?> InsertNewCooperationParticipant(CooperationParticipant participant);
        
        Task<bool?> UpdateCooperation(Cooperation newCooperation);
        
        /// <summary>
        /// Key == null for error. Key == string.Empty for not authorized.
        /// Key == nameof(User) if cooperator is a User participant, Value == 0 accordingly
        /// Key == nameof(Organization) if cooperator is an Organization participant, Value == organizationId
        /// </summary>
        Task<KeyValuePair<string, int>> IsThisUserAParticipantOrBelongedToAnOrganizationInThisCooperation(int userId, int cooperationId);
        
        Task<Cooperation> GetCooperationById(int cooperationId);
    }
}