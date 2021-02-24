using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.Extensions.Logging;

namespace AssistantLibrary.Services {

    public class RsaService : IRsaService {

        private RSACryptoServiceProvider _rsaService;
        private readonly ILogger<RsaService> _logger;

        private int _keySize;
        public int KeySize {
            set => _keySize = (value < 512 || value > 2048) ? SharedConstants.PreferedRsaKeyLength : value;
        }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public RsaService(ILogger<RsaService> logger) {
            _logger = logger;
        }

        public void GenerateRsaKeyPair() {
            _rsaService = new RSACryptoServiceProvider(_keySize);
            
            var privateKeyParam = _rsaService.ExportParameters(true);
            var publicKeyParam = _rsaService.ExportParameters(false);

            var stringWriter = new StringWriter();
            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            
            xmlSerializer.Serialize(stringWriter, privateKeyParam);
            PrivateKey = stringWriter.ToString();
            
            stringWriter.Flush();
            xmlSerializer.Serialize(stringWriter, publicKeyParam);
            PublicKey = stringWriter.ToString();
            
            stringWriter.Close();
        }

        public string Sign(string plainText) {
            try {
                if (!Helpers.IsProperString(PrivateKey)) return null;
                
                var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
                var streamReader = new StreamReader(PrivateKey);

                var publicKeyParam = (RSAParameters) xmlSerializer.Deserialize(streamReader);
                streamReader.Close();

                _rsaService = new RSACryptoServiceProvider();
                _rsaService.ImportParameters(publicKeyParam);

                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var cipher = _rsaService.Encrypt(plainBytes, false);

                PrivateKey = string.Empty;
                return Convert.ToBase64String(cipher);
            }
            catch (NullReferenceException e) {
                _logger.LogError("RsaService.Encrypt - Error: " + e.StackTrace);
                return null;
            }
        }

        public bool? Verify(string cipher, string plainText) {
            try {
                if (!Helpers.IsProperString(PublicKey)) return null;
                var cipherBytes = Convert.FromBase64String(cipher);

                var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
                var streamReader = new StreamReader(PublicKey);

                var privateKeyParam = (RSAParameters) xmlSerializer.Deserialize(streamReader);
                streamReader.Close();

                _rsaService = new RSACryptoServiceProvider();
                _rsaService.ImportParameters(privateKeyParam);

                var revertedBytes = _rsaService.Decrypt(cipherBytes, false);
                var revertedText = Encoding.UTF8.GetString(revertedBytes);

                PublicKey = string.Empty;
                return plainText.Equals(revertedText);
            }
            catch (NullReferenceException e) {
                _logger.LogError("RsaService.Decrypt - Error: " + e.StackTrace);
                return null;
            }
        }
    }
}