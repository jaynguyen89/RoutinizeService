using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.Models {

    public partial class NoteSegment {

        public bool ValidateBody() {
            if (!Helpers.IsProperString(Body)) return false;

            var body = Body.Trim();
            body = body.Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);

            return Helpers.IsProperString(body);
        }
    }
}