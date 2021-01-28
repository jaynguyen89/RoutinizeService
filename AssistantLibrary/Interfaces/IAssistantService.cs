using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace AssistantLibrary.Interfaces {

    public interface IAssistantService {

        KeyValuePair<string, string> GenerateHashAndSalt([NotNull] string plainText);

        string GenerateSaltForHash([NotNull] int length = 8);

        bool IsHashMatchesPlainText([NotNull] string hash,[NotNull] string plainText);
    }
}