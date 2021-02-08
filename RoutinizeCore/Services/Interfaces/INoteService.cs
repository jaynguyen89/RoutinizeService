using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface INoteService {

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
    }
}