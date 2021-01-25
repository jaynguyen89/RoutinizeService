using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace AssistantLibrary.Interfaces {

    public interface IAssistantService {

        KeyValuePair<string, string> GenerateHashAndSalt(string plainText);

        string GenerateRandomString(int length = 8);

        bool IsHashMatchesPlainText(string hash, string plainText);
    }
}