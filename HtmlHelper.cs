using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using AngleSharp.Parser.Html;
using System.IO;
using AngleSharp.Html;
using System.Net;

namespace DescriptionEditor
{
    public class HtmlHelper
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private static readonly string Indentation = "    ";


        public static string HtmlFormat(string Html)
        {
            Html = HtmlFormatRemove(Html);

            // TODO With recent version of AngleSharp
            /*
            var parser = new HtmlParser();
            var document = parser.ParseDocument(Html);

            using (var writer = new StringWriter())
            {
                var formatter = new PrettyMarkupFormatter
                {
                    Indentation = Indentation
                };
                document.Body.ChildNodes.ToHtml(writer, formatter);
                Html = writer.ToString();
            }
            */

            var parser = new HtmlParser();
            var document = parser.Parse(Html);

            using (var writer = new StringWriter())
            {
                var formatter = new PrettyMarkupFormatter
                {
                    Indentation = Indentation
                };
                document.Body.ChildNodes.ToHtml(writer, formatter);
                Html = writer.ToString();
            }


            Html = Regex.Replace(Html, @"(<[^>]*>)\s+(.)", "$1$2", RegexOptions.IgnoreCase);
            Html = Regex.Replace(Html, @"(<br>)(</li>)", "$2", RegexOptions.IgnoreCase);

            Html = Regex.Replace(Html, @"(<[^>]*>)(<strong>)(\s+)", "$1" + Environment.NewLine + "$3$2", RegexOptions.IgnoreCase);

            Html = Regex.Replace(Html, @"(.)(<br>)", "$1" + Environment.NewLine + "$2", RegexOptions.IgnoreCase);
            Html = Regex.Replace(Html, @"(<br>)(.)", "$1" + Environment.NewLine + "$2", RegexOptions.IgnoreCase);

            Html = Regex.Replace(Html, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

            Html = WebUtility.HtmlDecode(Html);

            return Html;
        }

        public static string HtmlFormatRemove(string Html)
        {
            Html = Html.Replace(Environment.NewLine, string.Empty);
            Html = Regex.Replace(Html, @"\r\n?|\n", string.Empty);
            Html = Regex.Replace(Html, @"\s+", " ");
            Html = Regex.Replace(Html, @"(>)\s+(<)", "$1$2", RegexOptions.IgnoreCase);
            return Html;
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
