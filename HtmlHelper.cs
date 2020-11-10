using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using AngleSharp.Parser.Html;
using System.IO;
using AngleSharp.Html;
using System.Net;
using HtmlAgilityPack;

namespace DescriptionEditor
{
    public class HtmlHelper
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private static readonly string Indentation = "    ";


        public static string HtmlFormat(string Html)
        {
            Html = HtmlFormatRemove(Html);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Html);

            Html = string.Empty;
            if (doc.DocumentNode != null)
            {
                foreach (var node in doc.DocumentNode.ChildNodes)
                {
                    Html += WriteNode(node, 0);
                }
            }
            return Html;

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
        }

        public static string WriteNode(HtmlNode _node, int _indentLevel)
        {
            string Result = string.Empty;

            // check parameter
            if (_node == null) return string.Empty;

            // init 
            string INDENT = Indentation;
            string NEW_LINE = System.Environment.NewLine;

            // case: no children
            if (_node.HasChildNodes == false)
            {
                for (int i = 0; i < _indentLevel; i++)
                {
                    Result += INDENT;
                }
                Result += _node.OuterHtml;
                Result += NEW_LINE;
            }

            // case: node has childs
            else
            {
                // indent
                for (int i = 0; i < _indentLevel; i++)
                {
                    Result += INDENT;
                }

                // open tag
                Result += string.Format("<{0}", _node.Name);
                if (_node.HasAttributes)
                {
                    foreach (var attr in _node.Attributes)
                    {
                        Result += string.Format(" {0}=\"{1}\"", attr.Name, attr.Value);
                    }
                }
                Result += string.Format(">{0}", NEW_LINE);

                // childs
                foreach (var chldNode in _node.ChildNodes)
                {
                    Result += WriteNode(chldNode, _indentLevel + 1);
                }

                // close tag
                for (int i = 0; i < _indentLevel; i++)
                {
                    Result += INDENT;
                }
                Result += string.Format("</{0}>{1}", _node.Name, NEW_LINE);
            }

            return Result;
        }


        public static string HtmlFormatRemove(string Html)
        {
            Html = Html.Replace(Environment.NewLine, string.Empty);
            Html = Html.Replace(Indentation, string.Empty);
            Html = Regex.Replace(Html, @"\r\n?|\n", string.Empty);
            Html = Regex.Replace(Html, @"\s+", " ");
            Html = Regex.Replace(Html, @"[⠀⠀⠀⠀⠀⠀⠀⠀]", string.Empty);
            Html = Regex.Replace(Html, @"(>)\s+(<)", "$1$2", RegexOptions.IgnoreCase);
            Html = Regex.Replace(Html, @">\s+", string.Empty, RegexOptions.IgnoreCase);
            return Html;
        }


        public static string RemoveTag(string html, string tag)
        {
            // Only img
            html = Regex.Replace(html, $"<{tag.ToLower()}[^>]*>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, $"<{tag.ToUpper()}[^>]*>", "", RegexOptions.IgnoreCase);
            return html;
        }

        public static string CenterImage(string html)
        {
            html = Regex.Replace(html, $"(<img[^>]*>)", "<div style=\"text-align: center;\">$1</div>", RegexOptions.IgnoreCase);
            return html;
        }

        public static string Add100PercentStyle(string html)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(html);

            foreach(var ImgTag in document.QuerySelectorAll("img"))
            {
                ImgTag.RemoveAttribute("width");

                List<string> ActualStyle = new List<string>();
                if (!ImgTag.GetAttribute("style").IsNullOrEmpty())
                {
                    ActualStyle = ImgTag.GetAttribute("style").Split(';').ToList(); ;
                }

                string NewStyle = string.Empty;
                if (ActualStyle.Count > 0)
                {
                    foreach(string StyleProperty in ActualStyle)
                    {
                        if (!StyleProperty.ToLower().Contains("width"))
                        {
                            NewStyle += StyleProperty.ToLower() + ";";
                        }
                    }
                }

                ImgTag.SetAttribute("style", NewStyle + "width: 100%;");
            }

            return document.Body.InnerHtml;
        }

        public static string RemoveSizeStyle(string html)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(html);

            foreach (var ImgTag in document.QuerySelectorAll("img"))
            {
                ImgTag.RemoveAttribute("width");
                ImgTag.RemoveAttribute("height");

                List<string> ActualStyle = new List<string>();
                if (!ImgTag.GetAttribute("style").IsNullOrEmpty())
                {
                    ActualStyle = ImgTag.GetAttribute("style").Split(';').ToList(); ;
                }

                string NewStyle = string.Empty;
                if (ActualStyle.Count > 0)
                {
                    foreach (string StyleProperty in ActualStyle)
                    {
                        if (!StyleProperty.ToLower().Contains("width") && !StyleProperty.ToLower().Contains("height"))
                        {
                            NewStyle += StyleProperty.ToLower() + ";";
                        }
                    }
                }

                ImgTag.SetAttribute("style", NewStyle);
            }

            return document.Body.InnerHtml;
        }
    }
}
