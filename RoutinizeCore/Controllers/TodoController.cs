using System;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Todo;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("todo")]
    [RoutinizeActionFilter]
    public sealed class TodoController {

        private readonly ITodoService _todoService;
        private readonly ICollaborationService _collaborationService;
        private readonly IUserService _userService;
        private readonly IContentGroupService _groupService;

        public TodoController(
            ITodoService todoService,
            ICollaborationService collaborationService,
            IUserService userService,
            IContentGroupService groupService
        ) {
            _todoService = todoService;
            _collaborationService = collaborationService;
            _userService = userService;
            _groupService = groupService;
        }

        /// <summary>
        /// To add new PERSONAL item WITHOUT attachments and WITHOUT group.
        /// </summary>
        [HttpPost("create-personal")]
        public async Task<JsonResult> AddNewPersonalTodo(Todo todo) {
            var errors = todo.VerifyTodoData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
            
            var result = await _todoService.InsertNewTodo(todo);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpPut("update/{updatedByUserId}")]
        public async Task<JsonResult> UpdateTodo([FromRoute] int updatedByUserId,[FromBody] Todo todo) {
            var errors = todo.VerifyTodoData(true);
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var shouldUpdateTodo = await IsTodoAndUserAssociated(updatedByUserId, todo.Id, SharedEnums.Permissions.Edit);
            if (!shouldUpdateTodo.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldUpdateTodo.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var result = await _todoService.UpdateTodo(todo);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpPost("create-group")]
        public async Task<JsonResult> CreateTodoGroup(ContentGroup todoGroup) {
            var errors = todoGroup.VerifyContentGroupData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            todoGroup.GroupOfType = nameof(Todo);
            
            var result = await _groupService.InsertNewContentGroup(todoGroup);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpPut("update-group/{updatedByUserId}")]
        public async Task<JsonResult> UpdateTodoGroup([FromRoute] int updatedByUserId,[FromBody] ContentGroup todoGroup) {
            var errors = todoGroup.VerifyContentGroupData(true);
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var shouldUpdateTodoGroup = await IsTodoGroupAndUserAssociated(updatedByUserId, todoGroup.Id, SharedEnums.Permissions.Edit);
            if (!shouldUpdateTodoGroup.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldUpdateTodoGroup.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var result = await _groupService.UpdateContentGroup(todoGroup);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        /// <summary>
        /// Add new SHARED item WITHOUT attachment and WITHOUT group.
        /// </summary>
        [HttpPost("create-shared")]
         public async Task<JsonResult> AddNewSharedTodo(SharedTodoVM sharedTodoData) {
            var errorMessages = sharedTodoData.VerifySharedTodoData();
            if (errorMessages.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });

            var hasCollaboration = await _collaborationService.DoesUserHasThisCollaborator(sharedTodoData.Todo.UserId, sharedTodoData.SharedToId);
            switch (hasCollaboration) {
                case null:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case < 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Collaborator not found." });
            }

            sharedTodoData.Todo.IsShared = true;
            var saveTodoResult = await _todoService.InsertNewTodo(sharedTodoData.Todo);
            if (!saveTodoResult.HasValue || saveTodoResult.Value < 1)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            var collaboratorTask = new CollaboratorTask {
                CollaborationId = hasCollaboration.Value,
                TaskId = saveTodoResult.Value,
                TaskType = nameof(Todo),
                Message = sharedTodoData.Message,
                AssignedOn = DateTime.UtcNow,
                Permission = (byte) sharedTodoData.Permission
            };

            var saveTaskResult = await _collaborationService.InsertNewCollaboratorTask(collaboratorTask);
            if (!saveTaskResult.HasValue || saveTaskResult.Value < 1)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveTodoResult.Value });
        }

        [HttpPut("toggle-emphasized/{userId}/{todoId}")]
        public async Task<JsonResult> ToggleEmphasizeTodo([FromRoute] int userId,[FromRoute] int todoId) {
            var shouldEmphasizeTodo = await IsTodoAndUserAssociated(userId, todoId, SharedEnums.Permissions.Edit);
            if (!shouldEmphasizeTodo.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldEmphasizeTodo.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var todo = await _todoService.GetTodoById(todoId);
            if (todo == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            todo.Emphasized = !todo.Emphasized;
            var result = await _todoService.UpdateTodo(todo);
            
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpDelete("delete/{userId}/{todoId}")]
        public async Task<JsonResult> DeleteTodo([FromRoute] int userId, [FromRoute] int todoId) {
            var shouldDeleteTodo = await IsTodoAndUserAssociated(userId, todoId, SharedEnums.Permissions.Delete);
            if (!shouldDeleteTodo.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldDeleteTodo.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedTodo = await _userService.DoesUserHasPremiumOrTodoUnlocked(userId);
            if (!premiumOrUnlockedTodo.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var result = premiumOrUnlockedTodo.Value ? await _todoService.SetTodoDeletedOnById(todoId)
                                                          : await _todoService.DeleteTodoById(todoId);
            
            if (!result.HasValue || !result.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpDelete("delete-group/{userId}/{todoGroupId}")]
        public async Task<JsonResult> DeleteTodoGroup([FromRoute] int userId, [FromRoute] int todoGroupId) {
            var shouldDeleteGroup = await IsTodoGroupAndUserAssociated(userId, todoGroupId, SharedEnums.Permissions.Delete);
            if (!shouldDeleteGroup.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldDeleteGroup.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedTodo = await _userService.DoesUserHasPremiumOrTodoUnlocked(userId);
            if (!premiumOrUnlockedTodo.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var result = premiumOrUnlockedTodo.Value ? await _todoService.SetTodoGroupDeletedOnById(todoGroupId)
                                                          : await _todoService.DeleteTodoGroupById(todoGroupId);
            
            if (!result.HasValue || !result.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("set-done/{doneById}/{todoId}")]
        public async Task<JsonResult> SetTodoAsDone([FromRoute] int doneById,[FromRoute] int todoId) {
            //Todo: doneById == 0 then auto set doneBy
            
            var shouldSetDone = await IsTodoAndUserAssociated(doneById, todoId, SharedEnums.Permissions.Edit);
            if (!shouldSetDone.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldSetDone.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var result = await _todoService.SetTodoAsDoneById(todoId, doneById);
            if (!result.HasValue || !result.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("set-undone/{userId}/{todoId}")]
        public async Task<JsonResult> SetTodoAsUndone([FromRoute] int userId,[FromRoute] int todoId) {
            var shouldSetDone = await IsTodoAndUserAssociated(userId, todoId, SharedEnums.Permissions.Edit);
            if (!shouldSetDone.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldSetDone.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var result = await _todoService.SetTodoAsUndoneById(todoId);
            if (!result.HasValue || !result.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpGet("get-active-personal/{userId}")]
        public async Task<JsonResult> GetPersonalActiveTodosForUser([FromRoute] int userId) {
            var activeTodos = await _todoService.GetPersonalActiveTodos(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeTodos });
        }
        
        [HttpGet("get-done-personal/{userId}")]
        public async Task<JsonResult> GetPersonalDoneTodosForUser([FromRoute] int userId) {
            var doneTodos = await _todoService.GetPersonalDoneTodos(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = doneTodos });
        }
        
        [HttpGet("get-archived-personal/{userId}")]
        public async Task<JsonResult> GetPersonalArchivedTodosForUser([FromRoute] int userId) {
            var archivedTodos = await _todoService.GetPersonalArchivedTodos(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedTodos });
        }
        
        [HttpGet("get-groups-active-personal/{userId}")]
        public async Task<JsonResult> GetPersonalActiveTodoGroupsForUser([FromRoute] int userId) {
            var activeGroups = await _todoService.GetPersonalActiveTodoGroups(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeGroups });
        }
        
        [HttpGet("get-groups-archived-personal/{userId}")]
        public async Task<JsonResult> GetPersonalArchivedTodoGroupsForUser([FromRoute] int userId) {
            var archivedGroups = await _todoService.GetPersonalArchivedTodoGroups(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedGroups });
        }

        [HttpGet("get-active-shared/{userId}")]
        public async Task<JsonResult> GetSharedActiveTodosForUser([FromRoute] int userId) {
            var activeTodos = await _todoService.GetSharedActiveTodos(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeTodos });
        }
        
        [HttpGet("get-done-shared/{userId}")]
        public async Task<JsonResult> GetSharedDoneTodosForUser([FromRoute] int userId) {
            var doneTodos = await _todoService.GetSharedDoneTodos(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = doneTodos });
        }
        
        [HttpGet("get-archived-shared/{userId}")]
        public async Task<JsonResult> GetSharedArchivedTodosForUser([FromRoute] int userId) {
            var archivedTodos = await _todoService.GetSharedArchivedTodos(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedTodos });
        }
        
        [HttpGet("get-groups-active-shared/{userId}")]
        public async Task<JsonResult> GetSharedActiveTodoGroupsForUser([FromRoute] int userId) {
            var activeGroups = await _todoService.GetSharedActiveTodoGroups(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeGroups });
        }
        
        [HttpGet("get-groups-archived-shared/{userId}")]
        public async Task<JsonResult> GetSharedArchivedTodoGroupsForUser([FromRoute] int userId) {
            var archivedGroups = await _todoService.GetSharedArchivedTodoGroups(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedGroups });
        }

        [HttpGet("get-group-items/{groupId}")]
        public async Task<JsonResult> GetTodosForGroup([FromRoute] int groupId) {
            var todos = await _todoService.GetTodosForContentGroupById(groupId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = todos });
        }

        private async Task<bool?> IsTodoAndUserAssociated(int userId, int todoId, SharedEnums.Permissions permission) {
            var isTodoCreatedByUser = await _todoService.IsTodoCreatedByThisUser(userId, todoId);
            if (!isTodoCreatedByUser.HasValue) return null;
            if (isTodoCreatedByUser.Value) return true;
            
            return await _collaborationService.IsTodoAssociatedWithThisCollaborator(userId, todoId, permission);
        }

        private async Task<bool?> IsTodoGroupAndUserAssociated(int userId, int todoGroupId, SharedEnums.Permissions permission) {
            var isTodoGroupCreatedByUser = await _todoService.IsTodoGroupCreatedByThisUser(userId, todoGroupId);
            if (!isTodoGroupCreatedByUser.HasValue) return null;
            if (isTodoGroupCreatedByUser.Value) return true;

            return await _collaborationService.IsTodoGroupAssociatedWithThisCollaborator(userId, todoGroupId, permission);
        }
    }
}