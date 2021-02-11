using System;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NotifierLibrary.Assets;
using NotifierLibrary.Interfaces;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Collaboration;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("collaboration")]
    public sealed class CollaborationController : AppController {

        private readonly ICollaborationService _collaborationService;
        private readonly IContentGroupService _groupService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        public CollaborationController(
            ICollaborationService collaborationService,
            IContentGroupService groupService,
            IAccountService accountService,
            IUserService userService,
            IUserNotifier userNotifier
        ) : base(userNotifier) {
            _collaborationService = collaborationService;
            _groupService = groupService;
            _accountService = accountService;
            _userService = userService;
        }

        [HttpPost("request")]
        public async Task<JsonResult> RequestCollaboration([FromHeader] int accountId,[FromBody] CollabRequestVM collabData) {
            var havePendingOrAcceptedRequest = await _collaborationService.DoTheyHavePendingOrAcceptedCollaborationRequest(accountId, collabData.UniqueId);
            if (!havePendingOrAcceptedRequest.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (havePendingOrAcceptedRequest.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "A request for collaboration between you guys is pending." });
            
            var errorMessage = collabData.ValidateMessage();
            if (errorMessage.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessage });
            
            var requestedCollaborator = await _userService.GetUserByUniqueId(collabData.UniqueId);
            if (requestedCollaborator == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var requestedAccount = await _accountService.GetUserAccountById(requestedCollaborator.AccountId);
            if (requestedAccount == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var (error, requester) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || requester == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var collaboration = new Collaboration {
                UserId = requester.Id,
                CollaboratorId = requestedCollaborator.Id,
                Message = collabData.Message,
                InvitedOn = DateTime.UtcNow
            };

            await _collaborationService.StartTransaction();
            var saveResult = await _collaborationService.InsertNewCollaboratorRequest(collaboration);
            if (!saveResult.HasValue || !saveResult.Value) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }
            
            var isNotified = await SendNotification(
                requester,
                requestedCollaborator.Id,
                new UserNotification {
                    Message = $"{ TokenNotifierName } has just sent you a collaboration request. Please respond as soon as possible.",
                    Title = "Collaboration Request Sent"
                },
                _userService,
                requestedAccount.FcmToken
            );

            if (!isNotified) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while sending notification." });
            }

            await _collaborationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpGet("response/{collaborationId}/{isAccepted}")]
        public async Task<JsonResult> ResponseToCollaborationRequest([FromHeader] int accountId,[FromRoute] int collaborationId,[FromRoute] int isAccepted) {
            var collaborationRequest = await _collaborationService.GetCollaborationRequest(collaborationId, accountId);
            if (collaborationRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            if (isAccepted == 1) {
                collaborationRequest.IsAccepted = true;
                collaborationRequest.AcceptedOn = DateTime.UtcNow;
            }
            else {
                collaborationRequest.IsAccepted = false;
                collaborationRequest.RejectedOn = DateTime.UtcNow;
            }

            await _collaborationService.StartTransaction();
            var updateResult = await _collaborationService.UpdateCollaboration(collaborationRequest);
            if (!updateResult.HasValue || !updateResult.Value) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }
            
            var requestedCollaborator = await _userService.GetUserById(collaborationRequest.CollaboratorId);
            if (requestedCollaborator == null) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Message = "An issue happened while sending notification." });
            }
                        
            var isNotified = await SendNotification(
                requestedCollaborator,
                collaborationRequest.UserId,
                new UserNotification {
                    Message = $"Your collaboration request with { TokenNotifierName } has been { (isAccepted == 1 ? "accepted. You guys can start sharing tasks now." : "rejected.") }.",
                    Title = "Collaboration Request Received"
                },
                _userService
            );
            
            if (!isNotified) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while sending notification." });
            }

            await _collaborationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpGet("get-requests/{status}/{asRequester}")]
        public async Task<JsonResult> GetCollaborationRequests([FromHeader] int accountId,[FromRoute] int status = 0,[FromRoute] int asRequester = 1) {
            var collaborationRequests = status switch {
                1 => await _collaborationService.GetPendingCollaborationRequests(accountId, asRequester == 1),
                2 => await _collaborationService.GetAcceptedCollaborationRequests(accountId, asRequester == 1),
                3 => await _collaborationService.GetRejectedCollaborationRequests(accountId, asRequester == 1),
                _ => await _collaborationService.GetAllCollaborationRequests(accountId, asRequester == 1)
            };

            return collaborationRequests == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = collaborationRequests });
        }

        [HttpDelete("revoke/{collaborationId}")]
        public async Task<JsonResult> RevokeCollaborationRequest([FromHeader] int accountId,[FromRoute] int collaborationId) {
            var collaborationRequest = await _collaborationService.GetCollaborationRequest(collaborationId, accountId, true);
            if (collaborationRequest == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            switch (collaborationRequest.IsAccepted) {
                case true:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Collaboration requested was accepted." });
                
                case false when collaborationRequest.RejectedOn.HasValue:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Collaboration requested was rejected." });
                
                default:
                    var result = await _collaborationService.DeleteCollaborationRequest(collaborationRequest);
                    return !result.HasValue || !result.Value
                        ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                        : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }
        }

        [HttpGet("get-shareables")]
        public async Task<JsonResult> GetShareableCollaborators([FromHeader] int accountId) {
            var shareables = await _collaborationService.GetShareableCollaboratorsForUser(accountId);
            return shareables == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = shareables });
        }

        [HttpPost("share-item")]
        public async Task<JsonResult> ShareItemWithCollaborator([FromHeader] int accountId,[FromBody] ItemSharingVM sharingData) {
            var errorMessage = sharingData.ValidateMessage();
            if (errorMessage.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessage });
            
            var haveThisTask = await _collaborationService.DoesCollaboratorAlreadyHaveThisItemTask(sharingData);
            if (!haveThisTask.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (haveThisTask.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Task already assigned to this collaborator." });
            
            var (error, sharedByUser) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || sharedByUser == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var haveCollaboration = await _collaborationService.AreTheyCollaborating(sharedByUser.Id, sharingData.SharedToUserId);
            switch (haveCollaboration) {
                case null:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case < 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }

            await _collaborationService.StartTransaction();
            var updateItemExpression = sharingData.ItemType switch {
                nameof(Todo) => (Func<Task<bool?>>)(async () => {
                    var todoService = HttpContext.RequestServices.GetService<ITodoService>();
                    if (todoService == null) return default;
                    
                    var todo = await todoService.GetTodoById(sharingData.ItemId);
                    if (todo.IsShared) return true;

                    todo.IsShared = true;
                    return await todoService.UpdateTodo(todo);
                }),
                nameof(Note) => async () => {
                    var noteService = HttpContext.RequestServices.GetService<INoteService>();
                    if (noteService == null) return default;

                    var note = await noteService.GetNoteById(sharingData.ItemId);
                    if (note.IsShared) return true;

                    note.IsShared = true;
                    return await noteService.UpdateNote(note);
                },
                nameof(NoteSegment) => async () => {
                    var noteService = HttpContext.RequestServices.GetService<INoteService>();
                    if (noteService == null) return default;

                    var noteSegment = await noteService.GetNoteSegmentById(sharingData.ItemId);
                    if (noteSegment.IsShared) return true;

                    noteSegment.IsShared = true;
                    return await noteService.UpdateNoteSegment(noteSegment);
                },
                _ => () => default
            };

            var updateItemResult = updateItemExpression.Invoke().Result;
            if (!updateItemResult.HasValue || !updateItemResult.Value) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            var task = new CollaboratorTask {
                CollaborationId = haveCollaboration.Value,
                TaskId = sharingData.ItemId,
                TaskType = sharingData.ItemType,
                Permission = (byte) SharedEnums.Permissions.Read,
                AssignedOn = DateTime.UtcNow,
                Message = sharingData.Message
            };

            var saveResult = await _collaborationService.InsertNewCollaboratorTask(task);
            if (!saveResult.HasValue || saveResult.Value < 1) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }
            
            var isNotified = await SendNotification(
                sharedByUser,
                sharingData.SharedToUserId,
                new UserNotification {
                    Message = $"{ TokenNotifierName } has just assigned a { sharingData.ItemType } to you. Please tap to see the item.",
                    Title = "Task Assigned"
                },
                _userService
            );

            if (!isNotified) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while sending notification." });
            }

            await _collaborationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }

        [HttpPost("share-group")]
        public async Task<JsonResult> ShareContentGroupWithCollaborator([FromHeader] int accountId,[FromBody] GroupSharingVM sharingData) {
            var errorMessage = sharingData.ValidateMessage();
            if (errorMessage.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessage });

            var (error, sharedByUser) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || sharedByUser == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var haveCollaboration = await _collaborationService.AreTheyCollaborating(sharedByUser.Id, sharingData.SharedToUserId);
            switch (haveCollaboration) {
                case null:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case < 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }

            var contentGroup = await _groupService.GetContentGroupById(sharingData.GroupId);
            if (contentGroup == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            sharingData.GroupOfType = contentGroup.GroupOfType;
            var haveThisTask = await _collaborationService.DoesCollaboratorAlreadyHaveThisGroupTask(sharingData);
            if (!haveThisTask.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (haveThisTask.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Task already assigned to this collaborator." });

            var groupShare = new GroupShare {
                GroupId = contentGroup.Id,
                SharedToType = nameof(Collaboration),
                SharedToId = haveCollaboration.Value,
                SharedById = sharedByUser.Id,
                SharedOn = DateTime.UtcNow,
                SideNotes = sharingData.Message
            };

            await _collaborationService.StartTransaction();
            var groupShareSaveResult = await _groupService.InsertNewGroupShare(groupShare);
            if (!groupShareSaveResult.HasValue || groupShareSaveResult.Value < 1) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            var task = new CollaboratorTask {
                CollaborationId = haveCollaboration.Value,
                TaskId = contentGroup.Id,
                TaskType = $"{ nameof(ContentGroup) }.{ contentGroup.GroupOfType }",
                Permission = (byte) SharedEnums.Permissions.Read,
                AssignedOn = DateTime.UtcNow,
                Message = sharingData.Message
            };
            
            var saveResult = await _collaborationService.InsertNewCollaboratorTask(task);
            if (!saveResult.HasValue || saveResult.Value < 1) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }
            
            var isNotified = await SendNotification(
                sharedByUser,
                sharingData.SharedToUserId,
                new UserNotification {
                    Message = $"{ TokenNotifierName } has just assigned a Group of { contentGroup.GroupOfType }s to you. Please tap to view the group.",
                    Title = "Group Task Assigned"
                },
                _userService
            );

            if (!isNotified) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while sending notification." });
            }

            await _collaborationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }

        [HttpDelete("unshare-item/{itemId}/{itemType}/{unsharedToUserId}")]
        public async Task<JsonResult> UnshareItem([FromHeader] int accountId,[FromRoute] int itemId,[FromRoute] string itemType,[FromRoute] int unsharedToUserId) {
            var (error, unsharedByUser) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || unsharedByUser == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var haveCollaboration = await _collaborationService.AreTheyCollaborating(unsharedByUser.Id, unsharedToUserId);
            switch (haveCollaboration) {
                case null:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case < 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }

            await _collaborationService.StartTransaction();
            var updateItemExpression = itemType switch {
                nameof(Todo) => (Func<Task<bool?>>)(async () => {
                    var todoService = HttpContext.RequestServices.GetService<ITodoService>();
                    if (todoService == null) return default;

                    var isTodoCreatedByUser = await todoService.IsTodoCreatedByThisUser(unsharedByUser.Id, itemId);
                    if (!isTodoCreatedByUser.HasValue) return default;
                    if (!isTodoCreatedByUser.Value) return false;

                    var hasOtherSharing = await todoService.IsTodoSharedToAnyoneElseExceptThisCollaborator(unsharedToUserId, itemId, unsharedByUser.Id);
                    if (!hasOtherSharing.HasValue) return default;
                    
                    var todo = await todoService.GetTodoById(itemId);
                    if (hasOtherSharing.Value == todo.IsShared) return true;

                    todo.IsShared = hasOtherSharing.Value;
                    return await todoService.UpdateTodo(todo);
                }),
                nameof(Note) => async () => {
                    var noteService = HttpContext.RequestServices.GetService<INoteService>();
                    if (noteService == null) return default;

                    var isNoteCreatedByUser = await noteService.IsNoteCreatedByThisUser(unsharedByUser.Id, itemId);
                    if (!isNoteCreatedByUser.HasValue) return default;
                    if (!isNoteCreatedByUser.Value) return false;

                    var hasOtherSharing = await noteService.IsNoteSharedToAnyoneElseExceptThisCollaborator(unsharedToUserId, itemId, unsharedByUser.Id);
                    if (!hasOtherSharing.HasValue) return default;
                    
                    var note = await noteService.GetNoteById(itemId);
                    if (hasOtherSharing.Value == note.IsShared) return true;

                    note.IsShared = hasOtherSharing.Value;
                    return await noteService.UpdateNote(note);
                },
                nameof(NoteSegment) => async () => {
                    var noteService = HttpContext.RequestServices.GetService<INoteService>();
                    if (noteService == null) return default;
                    
                    var isNoteSegmentCreatedByUser = await noteService.IsNoteSegmentCreatedByThisUser(unsharedByUser.Id, itemId);
                    if (!isNoteSegmentCreatedByUser.HasValue) return default;
                    if (!isNoteSegmentCreatedByUser.Value) return false;

                    var hasOtherSharing = await noteService.IsNoteSegmentSharedToAnyoneElseExceptThisCollaborator(unsharedToUserId, itemId, unsharedByUser.Id);
                    if (!hasOtherSharing.HasValue) return default;

                    var noteSegment = await noteService.GetNoteSegmentById(itemId);
                    if (hasOtherSharing.Value == noteSegment.IsShared) return true;

                    noteSegment.IsShared = hasOtherSharing.Value;
                    return await noteService.UpdateNoteSegment(noteSegment);
                },
                _ => () => default
            };
            
            var updateItemResult = updateItemExpression.Invoke().Result;
            switch (updateItemResult) {
                case null:
                    await _collaborationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case false:
                    await _collaborationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }

            var task = await _collaborationService.GetCollaboratorTaskFor(haveCollaboration.Value, itemId, itemType);
            if (task == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var deleteResult = await _collaborationService.DeleteCollaboratorTask(task);
            if (!deleteResult.HasValue || !deleteResult.Value) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }
            
            var isNotified = await SendNotification(
                unsharedByUser,
                unsharedToUserId,
                new UserNotification {
                    Message = $"{ TokenNotifierName } has just unassigned a { itemType } to you.",
                    Title = "Task Unassigned"
                },
                _userService
            );

            if (!isNotified) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while sending notification." });
            }

            await _collaborationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpDelete("unshare-group/{groupId}/{unsharedToUserId}")]
        public async Task<JsonResult> UnshareContentGroup([FromHeader] int accountId,[FromRoute] int groupId,[FromRoute] int unsharedToUserId) {
            var (error, unsharedByUser) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || unsharedByUser == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var haveCollaboration = await _collaborationService.AreTheyCollaborating(unsharedByUser.Id, unsharedToUserId);
            switch (haveCollaboration) {
                case null:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case < 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            }

            var isGroupCreatedByUser = await _groupService.IsGroupCreatedByThisUser(unsharedByUser.Id, groupId);
            if (!isGroupCreatedByUser.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!isGroupCreatedByUser.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var itemGroup = await _groupService.GetContentGroupById(groupId);
            if (itemGroup == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasOtherSharing = await _groupService.IsGroupSharedToAnyoneElseExceptThisCollaborator(unsharedToUserId, groupId, itemGroup.GroupOfType, unsharedByUser.Id);
            if (!hasOtherSharing.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            await _collaborationService.StartTransaction();
            if (hasOtherSharing.Value != itemGroup.IsShared) {
                itemGroup.IsShared = hasOtherSharing.Value;
                var groupUpdateResult = await _groupService.UpdateContentGroup(itemGroup);
                
                if (!groupUpdateResult) {
                    await _collaborationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
                }
            }

            var task = await _collaborationService.GetCollaboratorTaskFor(haveCollaboration.Value, groupId, $"{ nameof(ContentGroup) }.{ itemGroup.GroupOfType }");
            if (task == null) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            }

            var deleteResult = await _collaborationService.DeleteCollaboratorTask(task);
            if (!deleteResult.HasValue || !deleteResult.Value) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            var isNotified = await SendNotification(
                unsharedByUser,
                unsharedToUserId,
                new UserNotification {
                    Message = $"{ TokenNotifierName } has just unassigned a Group of { itemGroup.GroupOfType }s to you.",
                    Title = "Group Task Unassigned"
                },
                _userService
            );

            if (!isNotified) {
                await _collaborationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while sending notification." });
            }

            await _collaborationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpGet("get-item-assignees/{itemId}/{itemType}/{isGroup}")]
        public async Task<JsonResult> GetCollaboratorsByItem([FromRoute] int itemId,[FromRoute] string itemType,[FromRoute] int isGroup = 0) {
            int? itemOwnerId;
            if (isGroup == 1)
                itemOwnerId = (await _groupService.GetContentGroupOwner(itemId, itemType))?.Id;
            else {
                var getItemOwnerExpression = itemType switch {
                    nameof(Todo) => (Func<Task<User>>)(async () => {
                        var todoService = HttpContext.RequestServices.GetService<ITodoService>();
                        if (todoService == null) return default;

                        return await todoService.GetTodoOwnerFor(itemId);
                    }),
                    nameof(Note) => async () => {
                        var noteService = HttpContext.RequestServices.GetService<INoteService>();
                        if (noteService == null) return default;

                        return await noteService.GetNoteOwnerFor(itemId);
                    },
                    nameof(NoteSegment) => async () => {
                        var noteService = HttpContext.RequestServices.GetService<INoteService>();
                        if (noteService == null) return default;

                        return await noteService.GetNoteSegmentOwnerFor(itemId);
                    },
                    _ => default
                };

                itemOwnerId = (getItemOwnerExpression?.Invoke().Result)?.Id;
            }

            if (!itemOwnerId.HasValue || itemOwnerId.Value < 1)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var collaboratorsOnItem = await _collaborationService.GetCollaboratorsOnItemFor(itemOwnerId.Value, itemId, itemType, isGroup == 1);
            return collaboratorsOnItem == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = collaboratorsOnItem });
        }
    }
}