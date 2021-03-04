using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface ITodoService : IDbServiceBase {

        Task<int?> InsertNewTodo([NotNull] Todo todo);

        Task<bool> UpdateTodo([NotNull] Todo todo);

        Task<bool?> IsTodoCreatedByThisUser([NotNull] int userId,[NotNull] int todoId);
        
        Task<Todo> GetTodoById([NotNull] int todoId);
        
        Task<bool?> DeleteTodoById([NotNull] int todoId);
        
        Task<bool?> SetTodoAsDoneById([NotNull] int todoId,[NotNull] int doneById);
        
        Task<bool?> SetTodoAsUndoneById([NotNull] int todoId);
        
        Task<Todo[]> GetPersonalActiveTodos([NotNull] int userId);
        
        Task<Todo[]> GetPersonalDoneTodos([NotNull] int userId);
        
        Task<Todo[]> GetPersonalArchivedTodos([NotNull] int userId);

        Task<Todo[]> GetSharedActiveTodos([NotNull] int userId);
        
        Task<Todo[]> GetSharedDoneTodos([NotNull] int userId);
        
        Task<Todo[]> GetSharedArchivedTodos([NotNull] int userId);
        
        Task<bool?> IsTodoSharedToAnyoneElseExceptThisCollaborator([NotNull] int collaboratorId,[NotNull] int todoId,[NotNull] int ownerId);
        
        Task<User> GetTodoOwnerFor([NotNull] int itemId);
        
        Task<bool?> DoAllTodosBelongToThisUser(int[] todoIds, int userId);
    }
}