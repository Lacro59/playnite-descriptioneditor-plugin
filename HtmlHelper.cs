using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using Playnite.SDK.Data;

namespace DescriptionEditor
{
    public class HtmlHelper
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private static readonly string Indentation = "    ";


        #region Html identation
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
        #endregion


        /// <summary>
        /// Serialize html text
        /// </summary>
        /// <param name="Html"></param>
        /// <returns></returns>
        public static string HtmlFormatRemove(string Html)
        {
            Html = Html.Replace(Environment.NewLine, string.Empty);
            Html = Html.Replace(Indentation, string.Empty);
            Html = Regex.Replace(Html, @"\r\n?|\n", string.Empty);
            Html = Regex.Replace(Html, @"\s+", " ");
            Html = Regex.Replace(Html, @"[⠀⠀⠀⠀⠀⠀⠀⠀]", string.Empty);
            Html = Regex.Replace(Html, @"(>)\s+(<)", "$1$2", RegexOptions.IgnoreCase);
            Html = Regex.Replace(Html, @"\s+(<)", "$1", RegexOptions.IgnoreCase);
            return Html;
        }


        #region Html tag manipualtions
        /// <summary>
        /// Delete an html tag with or without replacement
        /// </summary>
        /// <param name="html"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string RemoveTag(string html, string tag, string openReplacement = "", string closeReplacement = "")
        {
            // Only img
            html = Regex.Replace(html, $"<{tag.ToLower()}[^>]*>", openReplacement, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, $"</{tag.ToUpper()}[^>]*>", closeReplacement, RegexOptions.IgnoreCase);
            return html;
        }



        /// <summary>
        /// Transform html header (h1, h2, ...) to html bold (b)
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HeaderToBold(string html)
        {
            html = Regex.Replace(html, $"<h[0-9][^>]*>", "<br><br><b>", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, $"</h[0-9][^>]*>", "</b><br>", RegexOptions.IgnoreCase);
            return html;
        }

        /// <summary>
        /// Remove html paragraph (p)
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ParagraphRemove(string html)
        {
            html = RemoveTag(html, "p", "", "<br><br>");
            return html;
        }
        #endregion


        /// <summary>
        /// Convert Markdown to html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string MarkdownToHtml(string html)
        {
            // List
            html = Regex.Replace(html, "<br>*", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "<br>-", "-", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "<br>+", "", RegexOptions.IgnoreCase);

            html = Markup.MarkdownToHtml(html);

            html = Regex.Replace
            (
                html,
                "!\\[[a-zA-Z0-9- ]*\\][\\s]*\\(((ftp|http|https):\\/\\/(\\w+:{0,1}\\w*@)?(\\S+)(:[0-9]+)?(\\/|\\/([\\w#!:.?+=&%@!\\-\\/]))?)\\)",
                "<img src=\"$1\" width=\"100%\"/>"
            );

            return html;
        }


        #region Image manipulations
        /// <summary>
        /// Add a css hack to center image
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string CenterImage(string html)
        {
            html = Regex.Replace(html, $"(<img[^>]*>)", "<div style=\"text-align: center;\">$1</div>", RegexOptions.IgnoreCase);
            return html;
        }

        /// <summary>
        /// Add a image width style at 100%
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Remove image width & heigth style
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
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
        #endregion
    }
}
