using System.Text;
using UnityEngine;

namespace Terrain.Utils
{
    /// <summary>
    /// A utility class to quickly format text that supports RichText.
    /// </summary>
    public static class RichText
    {
        private static StringBuilder str = new StringBuilder();

        public static string InBold(this string input)
        {
            str.Clear();
            str.Append("<b>");
            str.Append(input);
            str.Append("</b>");

            return str.ToString();
        }

        public static string InColor(this string input, Color c)
        {
            str.Clear();
            string hex = ColorUtility.ToHtmlStringRGBA(c);
            str.Append("<color=#");
            str.Append(hex);
            str.Append(">");
            str.Append(input);
            str.Append($"</color>");

            return str.ToString();
        }

        public static string InItalics(this string input)
        {
            str.Clear();
            str.Append("<i>");
            str.Append(input);
            str.Append("</i>");

            return str.ToString();
        }
    }
}