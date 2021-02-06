using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface ITodoService {

        Task<int?> InsertNewTodo([NotNull] Todo todo);

        Task<int?> InsertNewTodoGroup([NotNull] ContentGroup contentGroup);

        Task<bool> UpdateTodoGroup([NotNull] ContentGroup contentGroup);

        Task<bool> UpdateTodo([NotNull] Todo todo);

        Task<bool?> IsTodoCreatedByThisUser([NotNull] int userId,[NotNull] int todoId);

        Task<bool?> IsTodoGroupCreatedByThisUser([NotNull] int userId,[NotNull] int todoGroupId);
        
        Task<Todo> GetTodoById([NotNull] int todoId);
        
        Task<bool?> DeleteTodoById([NotNull] int todoId);

        Task<bool?> SetTodoDeletedOnById([NotNull] int todoId);
        
        Task<bool?> SetTodoGroupDeletedOnById([NotNull] int todoGroupId);
        
        Task<bool?> DeleteTodoGroupById([NotNull] int todoGroupId);
        
        Task<bool?> SetTodoAsDoneById([NotNull] int todoId,[NotNull] int doneById);
        
        Task<bool?> SetTodoAsUndoneById([NotNull] int todoId);
        
        Task<Todo[]> GetPersonalActiveTodos([NotNull] int userId);
        
        Task<Todo[]> GetPersonalDoneTodos([NotNull] int userId);
        
        Task<Todo[]> GetPersonalArchivedTodos([NotNull] int userId);
        
        /// <summary>
        /// Returns Dictionary with Key being the number of items in Value (the number of Todos in ContentGroup)
        /// </summary>
        Task<KeyValuePair<int, ContentGroup>[]> GetPersonalActiveTodoGroups([NotNull] int userId);
        
        /// <summary>
        /// Returns Dictionary with Key being the number of items in Value (the number of Todos in ContentGroup)
        /// </summary>
        Task<KeyValuePair<int, ContentGroup>[]> GetPersonalArchivedTodoGroups([NotNull] int userId);

        Task<Todo[]> GetSharedActiveTodos([NotNull] int userId);
        
        Task<Todo[]> GetSharedDoneTodos([NotNull] int userId);
        
        Task<Todo[]> GetSharedArchivedTodos([NotNull] int userId);
        
        Task<KeyValuePair<int, ContentGroup>[]> GetSharedActiveTodoGroups([NotNull] int userId);
        
        Task<KeyValuePair<int, ContentGroup>[]> GetSharedArchivedTodoGroups([NotNull] int userId);
        
        Task<Todo[]> GetTodosForContentGroupById([NotNull] int groupId);
    }
}