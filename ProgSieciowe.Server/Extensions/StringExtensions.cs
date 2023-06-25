using System.Text.RegularExpressions;

namespace ProgSieciowe.Server.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsValidFileName(this string fileName)
        {
            return !Regex.IsMatch(fileName, @"\\|/");
        }
    }
}
