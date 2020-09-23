using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace DescriptionEditor
{
    public class HtmlHelper
    {
        private static readonly ILogger logger = LogManager.GetLogger();


        public static string HtmlFormat(string html)
        {
            html = Regex.Replace(html, "(</[^>]*>)", "\r\n$1");
            html = Regex.Replace(html, "(<[br]*[br/]*[br /]*[BR]*[BR/]*[BR /]*>)", "\r\n$1");
            html = Regex.Replace(html, "(<[^>]*>)", "$1\r\n");

            string[] stringSeparators = new string[] { "\r\n" };
            List<string> lines = new List<string>(html.Split(stringSeparators, StringSplitOptions.None));

            // Remove space tab
            lines = lines.Select(s => s.Trim()).ToList();

            // Remove line empty
            lines = lines.FindAll(x => !string.IsNullOrEmpty(x));

            // Add space tab
            List<string> lastTag = new List<string>();
            for(int i = 0; i < lines.Count; i++)
            {
                if (Regex.IsMatch(lines[i], "<[^>]*>") 
                    && lines[i].ToLower() != "<br>" && lines[i].ToLower() != "<br/>" && lines[i].ToLower() != "<br />"
                    && lines[i].ToLower().IndexOf("<img") == -1)
                {
                    if (Regex.IsMatch(lines[i], "</[^>]*>"))
                    {
                        lastTag.RemoveAt(lastTag.Count - 1);
                        lines[i] = AddSpace(lines[i], lastTag.Count);
                    }
                    else if (!lastTag.Contains(lines[i]))
                    {
                        lastTag.Add(lines[i]);
                        lines[i] = AddSpace(lines[i], (lastTag.Count - 1));
                    }
                }
                else
                {
                    lines[i] = AddSpace(lines[i], lastTag.Count);
                }
            }

            return String.Join("\r\n", lines.ToArray()); ;
        }

        private static string AddSpace(string line, int index)
        {
            string spaces = "";
            for (int j = 0; j < (index * 5); j++)
            {
                spaces += " ";
            }
            return spaces + line;
        }

        public static string RemoveTag(string html, string tag)
        {
            // Only img
            html = Regex.Replace(html, $"<{tag.ToLower()}[^>]*>", "");
            html = Regex.Replace(html, $"<{tag.ToUpper()}[^>]*>", "");
            return html;
        }
    }
}
