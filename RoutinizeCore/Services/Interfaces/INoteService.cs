using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface INoteService : IDbServiceBase {

        Task<Note> GetNoteById([NotNull] int noteId);
        
        Task<bool> UpdateNote([NotNull] Note note);
        
        Task<NoteSegment> GetNoteSegmentById([NotNull] int segmentId);
        
        Task<bool> UpdateNoteSegment([NotNull] NoteSegment noteSegment);
        
        Task<bool?> IsNoteCreatedByThisUser([NotNull] int userId,[NotNull] int noteId);
        
        Task<bool?> IsNoteSharedToAnyoneElseExceptThisCollaborator([NotNull] int collaboratorId,[NotNull] int noteId,[NotNull] int ownerId);
        
        Task<bool?> IsNoteSegmentCreatedByThisUser([NotNull] int userId,[NotNull]  int segmentId);
        
        Task<bool?> IsNoteSegmentSharedToAnyoneElseExceptThisCollaborator([NotNull] int collaboratorId,[NotNull] int segmentId,[NotNull] int ownerId);
        
        Task<User> GetNoteOwnerFor([NotNull] int noteId);
        
        Task<User> GetNoteSegmentOwnerFor([NotNull] int segmentId);
        
        Task<int?> InsertNewNote([NotNull] Note note);
        
        Task<int?> InsertNoteSegment([NotNull] NoteSegment segment);
        
        Task<bool?> DeleteNoteSegment([NotNull] NoteSegment segment);
        
        Task<bool?> DeleteNote([NotNull] Note note);
        
        Task<Note[]> GetPersonalActiveNotes([NotNull] int userId);
        
        Task<Note[]> GetPersonalArchivedNotes([NotNull] int userId);
        
        Task<Note[]> GetSharedActiveNotes([NotNull] int userId);
        
        Task<Note[]> GetSharedArchivedNotes([NotNull] int userId);
        
        Task<bool?> DoAllNotesBelongToThisUser(int[] noteIds, int userId);
    }
}