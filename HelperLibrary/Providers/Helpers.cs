using System.Text;
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

        public static bool IsStringNotNullOrBlankOrSpace(string any) {
            return !string.IsNullOrEmpty(any) && !string.IsNullOrWhiteSpace(any);
        }
    }
}