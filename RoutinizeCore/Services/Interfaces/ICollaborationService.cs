using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Collaboration;

namespace RoutinizeCore.Services.Interfaces {

    public interface ICollaborationService : IDbServiceBase {

        Task<int?> DoesUserHasThisCollaborator([NotNull] int userId,[NotNull] int collaboratorId);

        Task<int?> InsertNewCollaboratorTask([NotNull] CollaboratorTask task);

        Task<bool?> IsTodoAssociatedWithThisCollaborator([NotNull] int userId,[NotNull] int todoId, SharedEnums.Permissions permission = SharedEnums.Permissions.Edit);

        Task<bool?> IsContentGroupAssociatedWithThisCollaborator([NotNull] int userId,[NotNull] int groupId,[NotNull] string groupType, SharedEnums.Permissions permission = SharedEnums.Permissions.Edit);
        
        Task<bool?> InsertNewCollaboratorRequest([NotNull] Collaboration collaboration);
        
        Task<Collaboration> GetCollaborationRequest([NotNull] int collaborationId,[NotNull] int accountId, bool forRequester = false);
        
        Task<bool?> DoTheyHavePendingOrAcceptedCollaborationRequest([NotNull] int accountId,[NotNull] string collabDataUniqueId);
        
        Task<bool?> UpdateCollaboration([NotNull] Collaboration collaboration);
        
        Task<Collaboration[]> GetPendingCollaborationRequests([NotNull] int accountId,[NotNull] bool asRequester = true);
        
        Task<Collaboration[]> GetAcceptedCollaborationRequests([NotNull] int accountId,[NotNull] bool asRequester = true);
        
        Task<Collaboration[]> GetRejectedCollaborationRequests([NotNull] int accountId,[NotNull] bool asRequester = true);
        
        Task<Collaboration[]> GetAllCollaborationRequests([NotNull] int accountId,[NotNull] bool asRequester = true);
        
        Task<bool?> DeleteCollaborationRequest([NotNull] Collaboration collaborationRequest);
        
        Task<User[]> GetShareableCollaboratorsForUser([NotNull] int accountId);
        
        Task<int?> AreTheyCollaborating([NotNull] int userId1,[NotNull] int userId2);
        
        Task<bool?> DoesCollaboratorAlreadyHaveThisItemTask([NotNull] ItemSharingVM sharingData);
        
        Task<bool?> DoesCollaboratorAlreadyHaveThisGroupTask([NotNull] GroupSharingVM sharingData);
        
        Task<CollaboratorTask> GetCollaboratorTaskFor([NotNull] int collaborationId,[NotNull] int itemId,[NotNull] string itemType);
        
        Task<bool?> DeleteCollaboratorTask([NotNull] CollaboratorTask task);
        
        Task<User[]> GetCollaboratorsOnItemFor([NotNull] int ownerId,[NotNull] int itemId,[NotNull] string itemType,[NotNull] bool isGroup = false);
        
        Task<bool?> IsNoteAssociatedWithThisUser(int noteId, int userId, SharedEnums.Permissions permission);
    }
}