using System;
using System.Linq;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.Models {

    public partial class Note {

        public string[] ValidateNoteAndSegments() {
            if (!Helpers.IsProperString(Title)) {
                Title = null;
                return Array.Empty<string>();
            }

            Title = Title.Trim();
            Title = Title.Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            Title = Helpers.CapitalizeFirstLetterOfSentence(Title);

            if (NoteSegments.Count == 0) return new[] { "Note has no content." };
            return NoteSegments.Any(segment => !segment.ValidateBody())
                ? new[] { "One or more segments have empty body." }
                : Array.Empty<string>();
        }
        
        public void ValidateNote() {
            if (!Helpers.IsProperString(Title)) {
                Title = null;
                return;
            }

            Title = Title.Trim();
            Title = Title.Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            Title = Helpers.CapitalizeFirstLetterOfSentence(Title);
        }
    }
}