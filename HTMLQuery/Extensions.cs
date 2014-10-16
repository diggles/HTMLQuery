using System;

namespace HTMLQuery
{
    /// <summary>
    /// Static Class containing extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Removed the top character from a string
        /// </summary>
        /// <param name="input">The target string</param>
        /// <returns>The target string minus the first chracter</returns>
        public static string Delete(this string input)
        {
            return input.Substring(1);
        }

        /// <summary>
        /// Returns a substring between two characters
        /// </summary>
        /// <param name="input">The target string</param>
        /// <param name="from">The start character</param>
        /// <param name="to">The end character</param>
        /// <returns>The strign between the from and to characters</returns>
        public static string Between(this string input, char from, char to)
        {
            int start = input.IndexOf(from) + 1;
            return input.Substring(start, input.IndexOf(to) - start);
        }
    }
}
