using System.Text.RegularExpressions;

namespace Common.Extensions
{
    public static class StringExtension
    {
        public static string CamelToSentenceCase(this string camelCase, bool firstLetterToUpper = true)
        {
            if (string.IsNullOrWhiteSpace(camelCase)) return string.Empty;

            var result = camelCase;

            //Strip leading "_" character
            result = Regex.Replace(result, "^_", "").Trim();
            //Add a space between each lower case character and upper case character
            result = Regex.Replace(result, "([a-z])([A-Z])", "$1 $2").Trim();
            //Add a space between 2 upper case characters when the second one is followed by a lower space character
            result = Regex.Replace(result, "([A-Z])([A-Z][a-z])", "$1 $2").Trim();

            result = result.ToLower();

            if (firstLetterToUpper) result = char.ToUpperInvariant(result[0]) + result.Substring(1);
            return result;
        }
    }
}
