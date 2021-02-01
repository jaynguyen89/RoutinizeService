using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface ITodoService {

        Task<int?> InsertNewTodo([NotNull] Todo todo);

        Task<int?> InsertNewTodoGroup([NotNull] TodoGroup todoGroup);

        Task<bool> UpdateTodoGroup([NotNull] TodoGroup todoGroup);

        Task<bool> UpdateTodo([NotNull] Todo todo);
    }
}