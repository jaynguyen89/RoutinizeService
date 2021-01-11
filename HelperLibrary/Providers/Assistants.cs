using System;
using HelperLibrary.Shared;

namespace HelperLibrary {

    public static class Assistants {

        public static string GetEnumValue(this Enum any) {
            var enumType = any.GetType();
            var fieldInfo = enumType.GetField(any.ToString());

            if (fieldInfo != null)
                return fieldInfo.GetCustomAttributes(
                    typeof(StringValueAttribute), false
                ) is StringValueAttribute[] attributes && attributes.Length > 0
                    ? attributes[0].Value
                    : string.Empty;
            
            return string.Empty;
        }
    }
}