using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace HelperLibrary {

    public static class Helpers {

        public static byte[] EncodeDataUtf8(object data) {
            var serializedData = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(serializedData);
        }

        public static T DecodeUtf8<T>(byte[] data) {
            var plainData = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(plainData);
        }

        public static bool IsProperString(string any) {
            return !string.IsNullOrEmpty(any) && !string.IsNullOrWhiteSpace(any);
        }

        public static string RemoveCharactersFromString(string any, List<char> characters) {
            return new string(any.Where(character => !characters.Contains(character)).ToArray());
        }

        public static string AppendCharacterToString(
            string any,
            char characterToAppend = SharedConstants.ACCOUNT_UNIQUE_ID_DELIMITER,
            int step = SharedConstants.ACCOUNT_UNIQUE_ID_GROUP_LENGTH
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

        public static string GenerateSha512Hash(string plainText) {
            using var sha512 = new SHA512Managed();
            var hashedBytes = sha512.ComputeHash(EncodeDataUtf8(plainText));

            return DecodeUtf8<string>(hashedBytes);
        }

        public static string FormatDateTimeString(DateTime dateTime) {
            return dateTime.ToString(SharedEnums.DateTimeFormats.COMPACT_H_DMY.GetEnumValue());
        }

        public static DateTime? ParseDateTimeString(string dateTime, string format = "dd-MM-yyyy HH:mm") {
            if (DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            return null;
        }

        public static long ConvertToUnixTimestamp(DateTime any) {
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
        public static int GetRandomNumber(int max, int min = 0) {
            var random = new Random();
            return random.Next(min, max + 1);
        }
    }
}