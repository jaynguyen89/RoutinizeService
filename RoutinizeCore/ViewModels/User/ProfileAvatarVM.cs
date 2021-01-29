using System.Collections.Generic;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;

namespace RoutinizeCore.ViewModels.User {

    /// <summary>
    /// In Routinize app, user can select an image from app asset or upload their own image for avatar.
    /// So only AvatarName or AvatarFile contains value at any time.
    /// </summary>
    public class ProfileAvatarVM {
        
        public int AccountId { get; set; }

        public string AvatarName { get; set; } = null;

        public IFormFile AvatarFile { get; set; } = null;
        
        public List<int> CheckAvatar() {
            var errors = new List<int>();

            if (AvatarFile == null && !Helpers.IsProperString(AvatarName))
                errors.Add(-1);

            if (AvatarFile != null) {
                if (!SharedConstants.IMAGE_TYPES.Contains(AvatarFile.ContentType)) {
                    errors.Add(0);
                    return errors;
                }

                if (AvatarFile.Length == 0) errors.Add(1);
                if (AvatarFile.Length > SharedConstants.IMAGE_FILE_MAX_SIZE) errors.Add(2);
            }

            return errors;
        }
        
        public List<string> GenerateErrorMessages(List<int> errors) {
            var messages = new List<string>();

            if (errors.Contains(-1)) messages.Add("No photo was submitted.");
            if (errors.Contains(0)) messages.Add("The photo is not of expected type. Expected: JPG, PNG, GIF.");
            if (errors.Contains(1)) messages.Add("The photo seems to be blank.");
            if (errors.Contains(2)) messages.Add("Photo size is too large. Max 2MB allowed.");

            return messages;
        }
    }
}