using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IContentGroupService {
        
        Task<int?> InsertNewContentGroup([NotNull] ContentGroup contentGroup);

        Task<bool> UpdateContentGroup([NotNull] ContentGroup contentGroup);
        
        Task<ContentGroup> GetContentGroupById([NotNull] int groupId);
        
        Task<int?> InsertNewGroupShare([NotNull] GroupShare groupShare);
        
        Task<bool?> IsGroupCreatedByThisUser([NotNull] int userId,[NotNull] int groupId);
        
        Task<bool?> IsGroupSharedToAnyoneElseExceptThisCollaborator([NotNull] int collaboratorId,[NotNull] int groupId,[NotNull] string itemType,[NotNull] int ownerId);
        
        Task<User> GetContentGroupOwner([NotNull] int itemId,[NotNull] string itemType);
    }
}