using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("todo")]
    [RoutinizeActionFilter]
    public sealed class TodoController {

        private readonly ITodoService _todoService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        public TodoController(
            ITodoService todoService,
            IAccountService accountService,
            IUserService userService
        ) {
            _todoService = todoService;
            _accountService = accountService;
            _userService = userService;
        }

        /// <summary>
        /// To add new PERSONAL item WITHOUT any attachments.
        /// </summary>
        [HttpPost("create-personal-no-attachment")]
        public async Task<JsonResult> AddNewTodo(Todo todo) {
            var errors = todo.VerifyTodoData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
            
            var result = await _todoService.InsertNewTodo(todo);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpPut("update")]
        public async Task<JsonResult> UpdateTodo(Todo todo) {
            var errors = todo.VerifyTodoData(true);
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
            
            var result = await _todoService.UpdateTodo(todo);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpPost("create-group")]
        public async Task<JsonResult> CreateTodoGroup(TodoGroup todoGroup) {
            var errors = todoGroup.VerifyTodoGroupData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var result = await _todoService.InsertNewTodoGroup(todoGroup);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpPut("update-group")]
        public async Task<JsonResult> UpdateTodoGroup(TodoGroup todoGroup) {
            var errors = todoGroup.VerifyTodoGroupData(true);
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var result = await _todoService.UpdateTodoGroup(todoGroup);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        /// <summary>
        /// Add new PERSONAL item with attachments.
        /// </summary>
        public async Task<JsonResult> AddNewTodoWithAttachments() {
            
        }
        
        public async Task<JsonResult> ShareTodoWithCollaborator() { }
        
        public async Task<JsonResult> ShareTodoGroupWithCollaborator() { }
        
        public async Task<JsonResult> UnshareTodo() { }
        
        public async Task<JsonResult> UnshareTodoGroup() { }
        
        public async Task<JsonResult> DeleteTodo() { }
        
        public async Task<JsonResult> DeleteTodoGroup() { }
        
        public async Task<JsonResult> MarkTodoAsDone() { }
        
        public async Task<JsonResult> MarkTodoAsUndone() { }
    }
}