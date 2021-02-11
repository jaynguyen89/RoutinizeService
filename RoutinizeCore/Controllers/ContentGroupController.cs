using System;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("content-group")]
    public class ContentGroupController {
        
        private readonly IContentGroupService _groupService;
        private readonly IUserService _userService;
        private readonly ICollaborationService _collaborationService;

        public ContentGroupController(
            IContentGroupService groupService,
            IUserService userService,
            ICollaborationService collaborationService
        ) {
            _groupService = groupService;
            _userService = userService;
            _collaborationService = collaborationService;
        }
        
        [HttpPost("create/{groupType}")]
        public async Task<JsonResult> CreateGroup([FromRoute] string groupType,[FromBody] ContentGroup contentGroup) {
            var errors = contentGroup.VerifyContentGroupData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            contentGroup.GroupOfType = groupType;
            var result = await _groupService.InsertNewContentGroup(contentGroup);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpPut("update/{updatedByUserId}")]
        public async Task<JsonResult> UpdateGroup([FromRoute] int updatedByUserId,[FromBody] ContentGroup contentGroup) {
            var errors = contentGroup.VerifyContentGroupData(true);
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var shouldUpdateTodoGroup = await IsContentGroupAndUserAssociated(updatedByUserId, contentGroup, SharedEnums.Permissions.Edit);
            if (!shouldUpdateTodoGroup.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldUpdateTodoGroup.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var result = await _groupService.UpdateContentGroup(contentGroup);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }
        
        [HttpDelete("delete/{userId}/{groupId}")]
        public async Task<JsonResult> DeleteContentGroup([FromRoute] int userId,[FromRoute] int groupId) {
            var contentGroup = await _groupService.GetContentGroupById(groupId);
            if (contentGroup == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var shouldDeleteGroup = await IsContentGroupAndUserAssociated(userId, contentGroup, SharedEnums.Permissions.Delete);
            if (!shouldDeleteGroup.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldDeleteGroup.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedFeature = await _userService.DoesUserHasPremiumOrTodoUnlocked(userId);
            if (!premiumOrUnlockedFeature.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool? result;
            if (premiumOrUnlockedFeature.Value) {
                contentGroup.DeletedOn = DateTime.UtcNow;
                result = await _groupService.UpdateContentGroup(contentGroup);
            }
            else
                result = await _groupService.DeleteContentGroupById(groupId);
            
            if (!result.HasValue || !result.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("revive-group/{groupId}")]
        public async Task<JsonResult> ReviveTodoGroup([FromRoute] int userId,[FromRoute] int groupId) {
            var contentGroup = await _groupService.GetContentGroupById(groupId);
            if (contentGroup == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var shouldReviveGroup = await IsContentGroupAndUserAssociated(userId, contentGroup, SharedEnums.Permissions.Delete);
            if (!shouldReviveGroup.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldReviveGroup.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedFeature = await _userService.DoesUserHasPremiumOrTodoUnlocked(userId);
            if (!premiumOrUnlockedFeature.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!premiumOrUnlockedFeature.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Please subscribe Premium to use this feature." });

            contentGroup.DeletedOn = null;
            var result = await _groupService.UpdateContentGroup(contentGroup);
            return !result ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                           : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-active-personal/{userId}/{groupType}")]
        public async Task<JsonResult> GetPersonalActiveContentGroupsForOwner([FromRoute] int userId,[FromRoute] string groupType) {
            var activeGroups = await _groupService.GetOwnerActiveContentGroups(userId, groupType);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeGroups });
        }
        
        [HttpGet("get-archived-personal/{userId}/{groupType}")]
        public async Task<JsonResult> GetPersonalArchivedTodoGroupsForOwner([FromRoute] int userId,[FromRoute] string groupType) {
            var archivedGroups = await _groupService.GetOwnerArchivedContentGroups(userId, groupType);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedGroups });
        }
        
        [HttpGet("get-active-shared/{userId}/{groupType}")]
        public async Task<JsonResult> GetSharedActiveContentGroupsForUser([FromRoute] int userId,[FromRoute] string groupType) {
            var activeGroups = await _groupService.GetSharedActiveContentGroups(userId, groupType);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeGroups });
        }
        
        [HttpGet("get-archived-shared/{userId}/{groupType}")]
        public async Task<JsonResult> GetSharedArchivedContentGroupsForUser([FromRoute] int userId,[FromRoute] string groupType) {
            var archivedGroups = await _groupService.GetSharedArchivedContentGroups(userId, groupType);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedGroups });
        }
        
        [HttpGet("get-items/{groupId}")]
        public async Task<JsonResult> GetItemsForGroup([FromRoute] int groupId) {
            var items = await _groupService.GetItemsForContentGroupById(groupId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = items });
        }
        
        private async Task<bool?> IsContentGroupAndUserAssociated(int userId, ContentGroup group, SharedEnums.Permissions permission) {
            var isTodoGroupCreatedByUser = await _groupService.IsContentGroupCreatedByThisUser(userId, group.Id);
            if (!isTodoGroupCreatedByUser.HasValue) return null;
            if (isTodoGroupCreatedByUser.Value) return true;

            return await _collaborationService.IsContentGroupAssociatedWithThisCollaborator(userId, group.Id, group.GroupOfType, permission);
        }

#pragma warning disable 1998
        private async Task<bool?> DoesUserHasPremiumOrUnlockedFeatureFor(string featureType, int userId) {
            var premiumOrUnlockedFeature = featureType switch {
                nameof(Todo) => (Func<Task<bool?>>)(async () => await _userService.DoesUserHasPremiumOrTodoUnlocked(userId)),
                nameof(Note) => async () => await _userService.DoesUserHasPremiumOrNoteUnlocked(userId),
                _ => default
            };

            return premiumOrUnlockedFeature?.Invoke().Result;
        }
    }
}