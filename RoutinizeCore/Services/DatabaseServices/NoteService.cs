using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class NoteService : DbServiceBase, INoteService {

        public NoteService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) : base(coreLogService, dbContext) { }
        
        public new async Task SetChangesToDbContext(object any, string task = SharedConstants.TaskInsert) {
            await base.SetChangesToDbContext(any, task);
        }

        public new async Task<bool?> CommitChanges() {
            return await base.CommitChanges();
        }

        public new void ToggleTransactionAuto(bool auto = true) {
            base.ToggleTransactionAuto(auto);
        }

        public new async Task StartTransaction() {
            await base.StartTransaction();
        }

        public new async Task CommitTransaction() {
            await base.CommitTransaction();
        }

        public new async Task RevertTransaction() {
            await base.RevertTransaction();
        }

        public new async Task ExecuteRawOn<T>(string query) {
            await base.ExecuteRawOn<T>(query);
        }

        public async Task<Note> GetNoteById(int noteId) {
            try {
                return await _dbContext.Notes.FindAsync(noteId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetNoteById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting Note using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(noteId) } = { noteId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool> UpdateNote(Note note) {
            try {
                _dbContext.Notes.Update(note);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(UpdateNote) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating Note.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(note) } = { JsonConvert.SerializeObject(note) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<NoteSegment> GetNoteSegmentById(int segmentId) {
            try {
                return await _dbContext.NoteSegments.FindAsync(segmentId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetNoteSegmentById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting NoteSegment using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(segmentId) } = { segmentId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool> UpdateNoteSegment(NoteSegment noteSegment) {
            try {
                _dbContext.NoteSegments.Update(noteSegment);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(UpdateNoteSegment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating NoteSegment.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(noteSegment) } = { JsonConvert.SerializeObject(noteSegment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool?> IsNoteCreatedByThisUser(int userId, int noteId) {
            try {
                return await _dbContext.Notes.AnyAsync(note => note.Id == noteId && note.UserId == userId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(IsNoteCreatedByThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Note with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { noteId }) = ({ noteId }, { userId })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool?> IsNoteSharedToAnyoneElseExceptThisCollaborator(int collaboratorId, int noteId, int ownerId) {
            try {
                var collaborationIdHavingThisCollaborator = await _dbContext.Collaborations
                                                                            .Where(
                                                                                collaboration => collaboration.CollaboratorId == collaboratorId &&
                                                                                                 collaboration.UserId == ownerId &&
                                                                                                 collaboration.IsAccepted
                                                                            )
                                                                            .Select(collaboration => collaboration.Id)
                                                                            .SingleOrDefaultAsync();

                var isSharedDirectlyToOtherCollaborators = await _dbContext.CollaboratorTasks
                                                                           .AnyAsync(
                                                                               task => task.CollaborationId != collaborationIdHavingThisCollaborator &&
                                                                                       task.Id == noteId &&
                                                                                       task.TaskType.Equals(nameof(Note))
                                                                           );
                if (isSharedDirectlyToOtherCollaborators) return true;

                var groupIdHavingThisNote = await _dbContext.Notes
                                                            .Where(note => note.Id == noteId && note.GroupId.HasValue)
                                                            .Select(note => note.GroupId)
                                                            .SingleOrDefaultAsync();

                var isSharedInGroupToOtherCollaborators = await _dbContext.GroupShares
                                                                          .AnyAsync(
                                                                              groupShare => groupShare.GroupId == groupIdHavingThisNote &&
                                                                                            groupShare.SharedToType.Equals(nameof(Collaboration)) &&
                                                                                            groupShare.SharedToId != collaborationIdHavingThisCollaborator
                                                                          );
                if (isSharedInGroupToOtherCollaborators) return true;

                var isSharedDirectlyToATeam = await _dbContext.TeamTasks.AnyAsync(teamTask => teamTask.TaskId == noteId && teamTask.TaskType.Equals(nameof(Note)));
                if (isSharedDirectlyToATeam) return true;

                var isSharedDirectlyToAProjectIteration = await _dbContext.IterationTasks.AnyAsync(task => task.TaskId == noteId && task.TaskType.Equals(nameof(Note)));
                if (isSharedDirectlyToAProjectIteration) return true;

                var isSharedInGroupToATeam = await _dbContext.TeamTasks
                                                             .AnyAsync(
                                                                 teamTask => teamTask.TaskId == groupIdHavingThisNote &&
                                                                             teamTask.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(Note) }")
                                                             );
                if (isSharedInGroupToATeam) return true;

                var isSharedInGroupToAProjectIteration = await _dbContext.IterationTasks
                                                                         .AnyAsync(
                                                                             iterationTask => iterationTask.TaskId == groupIdHavingThisNote &&
                                                                                              iterationTask.TaskType.Equals($"{ nameof(ContentGroup) }.{ nameof(Note) }")
                                                                         );
                return isSharedInGroupToAProjectIteration;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(IsNoteSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Notes with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(noteId) }, { nameof(ownerId) }) = ({ collaboratorId }, { noteId }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(IsNoteSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching Note with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(noteId) }, { nameof(ownerId) }) = ({ collaboratorId }, { noteId }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsNoteSegmentCreatedByThisUser(int userId, int segmentId) {
            try {
                return await _dbContext.NoteSegments
                                       .AnyAsync(
                                           segment => segment.Id == segmentId &&
                                                      segment.Note.UserId == userId
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(IsNoteSegmentCreatedByThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching NoteSegments with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(segmentId) }) = ({ userId }, { segmentId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<bool?> IsNoteSegmentSharedToAnyoneElseExceptThisCollaborator(int collaboratorId, int segmentId, int ownerId) {
            try {
                var noteIdHavingThisSegment = await _dbContext.NoteSegments
                                                              .Where(segment => segment.Id == segmentId)
                                                              .Select(segment => segment.Note.Id)
                                                              .SingleOrDefaultAsync();
                
                return await IsNoteSharedToAnyoneElseExceptThisCollaborator(collaboratorId, noteIdHavingThisSegment, ownerId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(IsNoteSegmentSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Notes with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(segmentId) }, { nameof(ownerId) }) = ({ collaboratorId }, { segmentId }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(IsNoteSegmentSharedToAnyoneElseExceptThisCollaborator) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching Note with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(collaboratorId) }, { nameof(segmentId) }, { nameof(ownerId) }) = ({ collaboratorId }, { segmentId }, { ownerId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<User> GetNoteOwnerFor(int noteId) {
            try {
                return await _dbContext.Notes
                                       .Where(note => note.Id == noteId)
                                       .Select(note => note.User)
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetNoteOwnerFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching User through Notes with Where-Select-Single.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(noteId) } = { noteId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetNoteOwnerFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching User through Notes with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(noteId) } = { noteId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<User> GetNoteSegmentOwnerFor(int segmentId) {
            try {
                return await _dbContext.NoteSegments
                                       .Where(segment => segment.Id == segmentId)
                                       .Select(segment => segment.Note.User)
                                       .SingleOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetNoteOwnerFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching User through NoteSegment with Where-Select-Single.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(segmentId) } = { segmentId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetNoteOwnerFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while searching User through NoteSegment with SingleOrDefault, >1 entry matching predicate..\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(segmentId) } = { segmentId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewNote(Note note) {
            try {
                await _dbContext.Notes.AddAsync(note);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : note.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(InsertNewNote) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting Note.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(note) } = { JsonConvert.SerializeObject(note) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNoteSegment(NoteSegment segment) {
            try {
                await _dbContext.NoteSegments.AddAsync(segment);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : segment.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(InsertNoteSegment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting NoteSegment.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(segment) } = { JsonConvert.SerializeObject(segment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteNoteSegment(NoteSegment segment) {
            try {
                _dbContext.NoteSegments.Remove(segment);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(DeleteNoteSegment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing NoteSegment.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(segment) }.Id = { segment.Id }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteNote(Note note) {
            try {
                _dbContext.Notes.Remove(note);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(DeleteNote) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing Note.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(note) }.Id = { note.Id }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Note[]> GetPersonalActiveNotes(int userId) {
            try {
                return await _dbContext.Notes
                                       .Where(
                                           note => note.UserId == userId &&
                                                   !note.IsShared &&
                                                   !note.DeletedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetPersonalActiveNotes) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Notes with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Note[]> GetPersonalArchivedNotes(int userId) {
            try {
                return await _dbContext.Notes
                                       .Where(
                                           note => note.UserId == userId &&
                                                   !note.IsShared &&
                                                   note.DeletedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetPersonalArchivedNotes) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Notes with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Note[]> GetSharedActiveNotes(int userId) {
            try {
                return await _dbContext.Notes
                                       .Where(
                                           note => note.UserId == userId &&
                                                   note.IsShared &&
                                                   !note.DeletedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetSharedActiveNotes) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Notes with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Note[]> GetSharedArchivedNotes(int userId) {
            try {
                return await _dbContext.Notes
                                       .Where(
                                           note => note.UserId == userId &&
                                                   note.IsShared &&
                                                   note.DeletedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(NoteService) }.{ nameof(GetSharedArchivedNotes) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Notes with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}