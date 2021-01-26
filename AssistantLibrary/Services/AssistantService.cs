using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AssistantLibrary.Interfaces;
using BCrypt;
using Microsoft.AspNetCore.Http;

namespace AssistantLibrary.Services {

    public sealed class AssistantService : IAssistantService {

        public KeyValuePair<string, string> GenerateHashAndSalt(string plainText) {
            var salt = BCryptHelper.GenerateSalt();
            var hash = BCryptHelper.HashPassword(plainText, salt);

            return new KeyValuePair<string, string>(hash, salt);
        }

        public string GenerateRandomString([NotNull] int length = 8) {
            return BCryptHelper.GenerateSalt(length);
        }

        public bool IsHashMatchesPlainText([NotNull] string hash,[NotNull] string plainText) {
            return BCryptHelper.CheckPassword(hash, plainText);
        }
    }
}