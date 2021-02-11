using System;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using NotifierLibrary.Assets;
using NotifierLibrary.Interfaces;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Note;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("note")]
    public sealed class NoteController : AppController {
        
        private readonly INoteService _noteService;
        private readonly ICollaborationService _collaborationService;
        private readonly IUserService _userService;

        public NoteController(
            INoteService noteService,
            ICollaborationService collaborationService,
            IUserService userService,
            IUserNotifier userNotifier
        ) : base(userNotifier) {
            _noteService = noteService;
            _collaborationService = collaborationService;
            _userService = userService;
        }

        [HttpPost("create-personal")]
        public async Task<JsonResult> AddNewPersonalNote(Note note) {
            var errorMessage = note.ValidateNoteAndSegments();
            if (errorMessage.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessage });

            await _noteService.StartTransaction();

            var saveNoteResult = await _noteService.InsertNewNote(note);
            if (!saveNoteResult.HasValue || saveNoteResult.Value < 1) {
                await _noteService.RevertTransaction();
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data."});
            }
            
            foreach (var segment in note.NoteSegments) {
                segment.NoteId = saveNoteResult.Value;
                var saveSegmentResult = await _noteService.InsertNoteSegment(segment);

                if (saveSegmentResult > 0) continue;
                
                await _noteService.RevertTransaction();
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data."});
            }

            await _noteService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveNoteResult.Value });
        }
        
        [HttpPost("create-shared")]
        public async Task<JsonResult> AddNewSharedNote(SharedNoteVM noteData) {
            var errorMessages = noteData.VerifySharedNoteData();
            if (errorMessages.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });

            var hasCollaboration = await _collaborationService.DoesUserHasThisCollaborator(noteData.Note.UserId, noteData.SharedToId);
            switch (hasCollaboration) {
                case null:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                case < 1:
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Collaborator not found." });
            }

            noteData.Note.IsShared = true;
            await _noteService.StartTransaction();
            
            var saveNoteResult = await _noteService.InsertNewNote(noteData.Note);
            if (!saveNoteResult.HasValue || saveNoteResult.Value < 1) {
                await _noteService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            var collaboratorTask = new CollaboratorTask {
                CollaborationId = hasCollaboration.Value,
                TaskId = saveNoteResult.Value,
                TaskType = nameof(Note),
                Message = noteData.Message,
                AssignedOn = DateTime.UtcNow,
                Permission = (byte) noteData.Permission
            };
            
            var saveTaskResult = await _collaborationService.InsertNewCollaboratorTask(collaboratorTask);
            if (!saveTaskResult.HasValue || saveTaskResult.Value < 1) {
                await _noteService.RevertTransaction();
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data."});
            }

            var noteOwner = await _userService.GetUserById(noteData.Note.UserId);
            if (noteOwner == null) {
                await _noteService.RevertTransaction();
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data."});
            }

            var isNotified = await SendNotification(
                noteOwner,
                noteData.SharedToId,
                new UserNotification {
                    Message = $"{ TokenNotifierName } has just assigned a Todo to you. Please tap to see the item.",
                    Title = "Task Assigned"
                },
                _userService
            );

            if (!isNotified) {
                await _noteService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }
            
            await _noteService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveNoteResult.Value });
        }

        [HttpPost("save-segment")]
        public async Task<JsonResult> AddSegmentToNote([FromHeader] int accountId,[FromBody] NoteSegment segment) {
            var errorMessage = segment.ValidateBody();
            if (errorMessage) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Segment body is empty." });

            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(segment.NoteId, user.Id, SharedEnums.Permissions.Edit);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var saveResult = await _noteService.InsertNoteSegment(segment);

            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpPost("update-segment")]
        public async Task<JsonResult> UpdateNoteSegment([FromHeader] int accountId,[FromBody] NoteSegment segment) {
            var errorMessage = segment.ValidateBody();
            if (errorMessage) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Segment body is empty." });

            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(segment.NoteId, user.Id, SharedEnums.Permissions.Edit);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var saveResult = await _noteService.UpdateNoteSegment(segment);

            return !saveResult ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                               : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("update")]
        public async Task<JsonResult> UpdateNote([FromHeader] int accountId,[FromBody] Note note) {
            note.ValidateNote();

            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(note.Id, user.Id, SharedEnums.Permissions.Edit);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var saveResult = await _noteService.UpdateNote(note);

            return !saveResult ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                               : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("remove-segment/{segmentId}")]
        public async Task<JsonResult> RemoveNoteSegment([FromHeader] int accountId,[FromRoute] int segmentId) {
            var segment = await _noteService.GetNoteSegmentById(segmentId);
            if (segment == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(segment.NoteId, user.Id, SharedEnums.Permissions.Delete);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedNote = await _userService.DoesUserHasPremiumOrNoteUnlocked(user.Id);
            if (!premiumOrUnlockedNote.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool? result;
            if (premiumOrUnlockedNote.Value) {
                segment.DeletedOn = DateTime.UtcNow;
                result = await _noteService.UpdateNoteSegment(segment);
            }
            else
                result = await _noteService.DeleteNoteSegment(segment);
            
            return !result.HasValue || !result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("remove/{noteId}")]
        public async Task<JsonResult> DeleteNote([FromHeader] int accountId,[FromRoute] int noteId) {
            var note = await _noteService.GetNoteById(noteId);
            if (note == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(noteId, user.Id, SharedEnums.Permissions.Delete);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedNote = await _userService.DoesUserHasPremiumOrNoteUnlocked(user.Id);
            if (!premiumOrUnlockedNote.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            bool? result;
            if (premiumOrUnlockedNote.Value) {
                note.DeletedOn = DateTime.UtcNow;
                result = await _noteService.UpdateNote(note);
            }
            else
                result = await _noteService.DeleteNote(note);

            return !result.HasValue || !result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("revive/{noteId}")]
        public async Task<JsonResult> ReviveNote([FromHeader] int accountId,[FromRoute] int noteId) {
            var note = await _noteService.GetNoteById(noteId);
            if (note == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(noteId, user.Id, SharedEnums.Permissions.Edit);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });
            
            var premiumOrUnlockedNote = await _userService.DoesUserHasPremiumOrNoteUnlocked(user.Id);
            if (!premiumOrUnlockedNote.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!premiumOrUnlockedNote.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Please subscribe Premium to use this feature." });

            note.DeletedOn = null;
            var result = await _noteService.UpdateNote(note);
            
            return !result ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                           : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPost("revive-segment/{segmentId}")]
        public async Task<JsonResult> ReviveNoteSegment([FromHeader] int accountId,[FromRoute] int segmentId) {
            var segment = await _noteService.GetNoteSegmentById(segmentId);
            if (segment == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var (error, user) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || user == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var hasAssociation = await _collaborationService.IsNoteAssociatedWithThisUser(segment.NoteId, user.Id, SharedEnums.Permissions.Edit);
            if (!hasAssociation.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasAssociation.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var premiumOrUnlockedNote = await _userService.DoesUserHasPremiumOrNoteUnlocked(user.Id);
            if (!premiumOrUnlockedNote.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!premiumOrUnlockedNote.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Please subscribe Premium to use this feature." });

            segment.DeletedOn = null;
            var result = await _noteService.UpdateNoteSegment(segment);
            
            return !result ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-active-personal/{userId}")]
        public async Task<JsonResult> GetPersonalActiveTodosForOwner([FromRoute] int userId) {
            var activeNotes = await _noteService.GetPersonalActiveNotes(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeNotes });
        }
        
        [HttpGet("get-archived-personal/{userId}")]
        public async Task<JsonResult> GetPersonalArchivedTodosForOwner([FromRoute] int userId) {
            var archivedNotes = await _noteService.GetPersonalArchivedNotes(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedNotes });
        }
        
        [HttpGet("get-active-shared/{userId}")]
        public async Task<JsonResult> GetSharedActiveTodosForUser([FromRoute] int userId) {
            var activeNotes = await _noteService.GetSharedActiveNotes(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = activeNotes });
        }
        
        [HttpGet("get-archived-shared/{userId}")]
        public async Task<JsonResult> GetSharedArchivedTodosForUser([FromRoute] int userId) {
            var archivedNotes = await _noteService.GetSharedArchivedNotes(userId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = archivedNotes });
        }
    }
}