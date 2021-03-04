using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.ItemGroup;

namespace RoutinizeCore.Services.Interfaces {

    public interface IContentGroupService : IDbServiceBase {
        
        Task<int?> InsertNewContentGroup([NotNull] ContentGroup contentGroup);

        Task<bool> UpdateContentGroup([NotNull] ContentGroup contentGroup);
        
        Task<ContentGroup> GetContentGroupById([NotNull] int groupId);
        
        Task<int?> InsertNewGroupShare([NotNull] GroupShare groupShare);
        
        Task<bool?> IsGroupCreatedByThisUser([NotNull] int userId,[NotNull] int groupId);
        
        Task<bool?> IsGroupSharedToAnyoneElseExceptThisCollaborator([NotNull] int collaboratorId,[NotNull] int groupId,[NotNull] string itemType,[NotNull] int ownerId);
        
        Task<User> GetContentGroupOwner([NotNull] int itemId,[NotNull] string itemType);

        Task<bool?> DeleteContentGroupById([NotNull] int groupId);
        
        Task<bool?> IsContentGroupCreatedByThisUser([NotNull] int userId,[NotNull] int groupId);
        
        /// <summary>
        /// Returns Dictionary with Key being the number of items in Value (the number of Todos in ContentGroup)
        /// </summary>
        Task<KeyValuePair<int, ContentGroup>[]> GetOwnerActiveContentGroups([NotNull] int userId,[NotNull] string groupType);
        
        /// <summary>
        /// Returns Dictionary with Key being the number of items in Value (the number of Todos in ContentGroup)
        /// </summary>
        Task<KeyValuePair<int, ContentGroup>[]> GetOwnerArchivedContentGroups([NotNull] int userId,[NotNull] string groupType);
        
        /// <summary>
        /// Returns Dictionary with Key being the number of items in Value (the number of Todos in ContentGroup)
        /// </summary>
        Task<KeyValuePair<int, ContentGroup>[]> GetSharedActiveContentGroups([NotNull] int userId,[NotNull] string groupType);
        
        /// <summary>
        /// Returns Dictionary with Key being the number of items in Value (the number of Todos in ContentGroup)
        /// </summary>
        Task<KeyValuePair<int, ContentGroup>[]> GetSharedArchivedContentGroups([NotNull] int userId,[NotNull] string groupType);
        
        Task<object[]> GetItemsForContentGroupById([NotNull] int groupId);
        
        bool? AddItemsToContentGroupFrom(ItemGroupVM itemGroup);
        
        bool? RemoveItemsFromContentGroupFor(ItemGroupVM itemGroup);
    }
}