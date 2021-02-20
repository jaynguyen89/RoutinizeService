using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace HelperLibrary {

    public static class Helpers {

        public static byte[] EncodeDataUtf8([NotNull] object data) {
            var serializedData = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(serializedData);
        }

        public static T DecodeUtf8<T>([NotNull] byte[] data) {
            var plainData = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(plainData);
        }

        public static bool IsProperString([AllowNull] string any) {
            return !string.IsNullOrEmpty(any) && !string.IsNullOrWhiteSpace(any);
        }

        public static string RemoveCharactersFromString([NotNull] string any,[NotNull] List<char> characters) {
            return new string(any.Where(character => !characters.Contains(character)).ToArray());
        }

        public static string AppendCharacterToString(
            [NotNull] string any,
            [NotNull] char characterToAppend = SharedConstants.AccountUniqueIdDelimiter,
            [NotNull] int step = SharedConstants.AccountUniqueIdGroupLength
        ) {
            return any.ToCharArray().Select(character => character.ToString()).ToList()
                      .Aggregate((previousChar, nextChar) => {
                            if (
                                    (any.IndexOf(nextChar, StringComparison.Ordinal) + 1) % step == 0 &&
                                    any.IndexOf(nextChar, StringComparison.Ordinal) != any.Length - 1
                                )
                                return $"{ previousChar }{ nextChar }{ characterToAppend }";
                            else
                                return $"{ previousChar }{ nextChar }";
                      });
        }

        public static string GenerateSha512Hash([NotNull] string plainText) {
            using var sha512 = new SHA512Managed();
            var hashedBytes = sha512.ComputeHash(EncodeDataUtf8(plainText));

            return Convert.ToBase64String(hashedBytes);
        }

        public static string FormatDateTimeString([NotNull] DateTime dateTime) {
            return dateTime.ToString(SharedEnums.DateTimeFormats.COMPACT_H_DMY.GetEnumValue());
        }

        public static DateTime? ParseDateTimeString([AllowNull] string dateTime,[NotNull] string format = "dd-MM-yyyy HH:mm") {
            if (DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            return null;
        }

        public static long ConvertToUnixTimestamp([NotNull] DateTime any) {
            any = DateTime.SpecifyKind(any, DateTimeKind.Utc);
            
            var datetimeOffset = new DateTimeOffset(any);
            return datetimeOffset.ToUnixTimeMilliseconds();
        }

        public static DateTime ConvertUnixTimestampToDatetime(long timestamp) {
            var unixBaseDatetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var wantedDatetime = unixBaseDatetime.AddMilliseconds(timestamp).ToUniversalTime();

            return wantedDatetime;
        }

        /// <summary>
        /// Max is inclusive
        /// </summary>
        public static int GetRandomNumber([NotNull] int max,[NotNull] int min = 0) {
            var random = new Random();
            return random.Next(min, max + 1);
        }
        
        public static string GenerateRandomString([NotNull] int length,[NotNull] bool includeSpecialChars = false) {
            const string SCHARS = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnmn1234567890!@#$%^&*_+.";
            const string NCHARS = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnmn1234567890";
            
            var randomString = new string(
                Enumerable.Repeat(includeSpecialChars ? SCHARS : NCHARS, length)
                          .Select(p => p[(new Random()).Next(p.Length)])
                          .ToArray()
            );

            return randomString;
        }
        
        public static string CapitalizeFirstLetterOfEachWord([NotNull] string sentence) {
            var newSentence = sentence
                              .Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace)
                              .ToLower();

            newSentence = Regex.Replace(newSentence, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            return newSentence;
        }

        public static string CapitalizeFirstLetterOfSentence([AllowNull] string sentence) {
            return !IsProperString(sentence)
                ? default
                : sentence.First().ToString().ToUpper().Concat(sentence.Substring(1)).ToString();
        }

        public static string ExtractImageNameFromPath([NotNull] string imageName) {
            var pathTokens = imageName.Split(SharedConstants.Fslash);
            return pathTokens[^1];
        }

        public static byte FindAttachmentType([NotNull] string fileTypeString) {
            fileTypeString = fileTypeString.ToLower();

            if (fileTypeString.Equals(SharedEnums.AttachmentTypes.Photo.GetEnumValue())) return (byte) SharedEnums.AttachmentTypes.Photo;
            if (fileTypeString.Equals(SharedEnums.AttachmentTypes.Audio.GetEnumValue())) return (byte) SharedEnums.AttachmentTypes.Audio;
            if (fileTypeString.Equals(SharedEnums.AttachmentTypes.Video.GetEnumValue())) return (byte) SharedEnums.AttachmentTypes.Video;
            
            return (byte) SharedEnums.AttachmentTypes.File;
        }

        public static bool IsAttachmentAHttpLink([AllowNull] string attachmentFileUrl) {
            return IsProperString(attachmentFileUrl) && attachmentFileUrl.Contains("http");
        }
    }
}