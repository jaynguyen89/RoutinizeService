using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface ICollaborationService {

        Task<int?> DoesUserHasThisCollaborator([NotNull] int userId,[NotNull] int collaboratorId);

        Task<int?> InsertNewCollaboratorTask([NotNull] CollaboratorTask task);

        Task<bool?> IsTodoAssociatedWithThisCollaborator([NotNull] int userId,[NotNull] int todoId, SharedEnums.Permissions permission = SharedEnums.Permissions.Edit);

        Task<bool?> IsTodoGroupAssociatedWithThisCollaborator([NotNull] int userId,[NotNull] int todoGroupId, SharedEnums.Permissions permission = SharedEnums.Permissions.Edit);
    }
}