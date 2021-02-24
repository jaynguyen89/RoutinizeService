using System.Collections.Generic;
using HelperLibrary.Shared;

namespace AssistantLibrary.Interfaces {

    public interface IRsaService {
        
        string PublicKey { get; set; }

        string PrivateKey { get; set; }

        void GenerateRsaKeyPair();

        string Sign(string plainText);

        bool? Verify(string cipher, string plainText);
    }
}