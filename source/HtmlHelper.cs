using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using Playnite.SDK.Data;

namespace DescriptionEditor
{
    /// <summary>
    /// Provides utility methods for HTML manipulation, formatting, and transformation.
    /// All methods are optimized with compiled regex and efficient string operations.
    /// </summary>
    public class HtmlHelper
    {
        /// <summary>
        /// Gets the indentation string used for HTML formatting (4 spaces).
        /// </summary>
        private static string Indentation => "    ";

        #region Compiled Regex - Performance optimization

        /// <summary>
        /// Matches newline characters (\r\n, \r, \n).
        /// </summary>
        private static readonly Regex NewLineRegex = new Regex(@"\r\n?|\n", RegexOptions.Compiled);

        /// <summary>
        /// Matches one or more whitespace characters.
        /// </summary>
        private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Matches special Unicode spacing characters (braille pattern blank).
        /// </summary>
        private static readonly Regex SpecialCharsRegex = new Regex(@"[⠀⠀⠀⠀⠀⠀⠀⠀]", RegexOptions.Compiled);

        /// <summary>
        /// Matches whitespace between HTML tags (>  <).
        /// </summary>
        private static readonly Regex TagSpacingRegex = new Regex(@"(>)\s+(<)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches leading whitespace before opening HTML tags.
        /// </summary>
        private static readonly Regex LeadingSpaceRegex = new Regex(@"\s+(<)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches double br tags (<br><br> or </br></br>).
        /// </summary>
        private static readonly Regex BrBrRegex = new Regex(@"<[/]?br><[/]?br>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches br tags followed by asterisks (<br>*).
        /// </summary>
        private static readonly Regex BrAsteriskRegex = new Regex("<br>\\*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches br tags followed by dashes (<br>-).
        /// </summary>
        private static readonly Regex BrDashRegex = new Regex("<br>-", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches br tags followed by plus signs (<br>+).
        /// </summary>
        private static readonly Regex BrPlusRegex = new Regex("<br>\\+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches empty paragraph tags (<p></p>).
        /// </summary>
        private static readonly Regex EmptyParagraphRegex = new Regex("<p></p>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches Markdown image syntax and extracts the URL.
        /// </summary>
        private static readonly Regex MarkdownImageRegex = new Regex(
            "!\\[[a-zA-Z0-9- ]*\\][\\s]*\\(((ftp|http|https):\\/\\/(\\w+:{0,1}\\w*@)?(\\S+)(:[0-9]+)?(\\/|\\/([\\w#!:.?+=&%@!\\-\\/]))?)\\)",
            RegexOptions.Compiled
        );

        #endregion

        #region About Game Headers - Static HashSet

        /// <summary>
        /// Contains localized "About the Game" header variants in multiple languages.
        /// Used for identifying and removing Steam game description headers.
        /// </summary>
        private static readonly HashSet<string> AboutGameHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "<h1>about the game</h1>",
            "<h1>à propos du jeu</h1>",
            "<h1>Относно играта</h1>",
            "<h1>关于游戏</h1>",
            "<h1>關於此遊戲</h1>",
            "<h1>O hře</h1>",
            "<h1>Om spillet</h1>",
            "<h1>Info over het spel</h1>",
            "<h1>Tietoja pelistä</h1>",
            "<h1>Über das Spiel</h1>",
            "<h1>Σχετικά με το παιχνίδι</h1>",
            "<h1>A játékról:&nbsp;</h1>",
            "<h1>Informazioni sul gioco</h1>",
            "<h1>ゲームについて</h1>",
            "<h1>게임 정보</h1>",
            "<h1>Informacje o&nbsp;grze</h1>",
            "<h1>Acerca do Jogo</h1>",
            "<h1>Sobre o jogo</h1>",
            "<h1>Despre joc</h1>",
            "<h1>Об игре</h1>",
            "<h1>Acerca del juego</h1>",
            "<h1>Om spelet</h1>",
            "<h1>ข้อมูลเกม</h1>",
            "<h1>Oyun Açıklaması</h1>",
            "<h1>Про гру</h1>",
            "<h1>Về trò chơi này</h1>"
        };

        #endregion

        #region Html indentation

        /// <summary>
        /// Formats HTML with proper indentation and line breaks.
        /// Removes existing formatting and rebuilds the HTML structure with consistent indentation.
        /// </summary>
        /// <param name="html">The HTML string to format.</param>
        /// <returns>Formatted HTML with proper indentation.</returns>
        public static string HtmlFormat(string html)
        {
            html = HtmlFormatRemove(html);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (doc.DocumentNode == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(html.Length * 2); // Preallocate
            foreach (HtmlNode node in doc.DocumentNode.ChildNodes)
            {
                WriteNode(node, 0, sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Recursively writes an HTML node and its children to a StringBuilder with proper indentation.
        /// This is an optimized version that uses StringBuilder to avoid string concatenation overhead.
        /// </summary>
        /// <param name="node">The HTML node to write.</param>
        /// <param name="indentLevel">The current indentation level (number of indents).</param>
        /// <param name="sb">The StringBuilder to append the formatted HTML to.</param>
        public static void WriteNode(HtmlNode node, int indentLevel, StringBuilder sb)
        {
            if (node == null)
            {
                return;
            }

            string indent = new string(' ', indentLevel * Indentation.Length);
            string newLine = Environment.NewLine;

            // Case: no children
            if (!node.HasChildNodes)
            {
                sb.Append(indent);
                sb.Append(node.OuterHtml);
                sb.Append(newLine);
                return;
            }

            // Case: node has children
            sb.Append(indent);
            sb.Append('<');
            sb.Append(node.Name);

            if (node.HasAttributes)
            {
                foreach (HtmlAttribute attr in node.Attributes)
                {
                    sb.Append(' ');
                    sb.Append(attr.Name);
                    sb.Append("=\"");
                    sb.Append(attr.Value);
                    sb.Append('"');
                }
            }

            sb.Append('>');
            sb.Append(newLine);

            // Children
            foreach (HtmlNode childNode in node.ChildNodes)
            {
                WriteNode(childNode, indentLevel + 1, sb);
            }

            // Close tag
            sb.Append(indent);
            sb.Append("</");
            sb.Append(node.Name);
            sb.Append('>');
            sb.Append(newLine);
        }

        // Backward compatibility - keep old signature
        /// <summary>
        /// Recursively writes an HTML node and its children with proper indentation.
        /// This is a legacy method maintained for backward compatibility.
        /// For better performance, use the StringBuilder overload.
        /// </summary>
        /// <param name="node">The HTML node to write.</param>
        /// <param name="indentLevel">The current indentation level (number of indents).</param>
        /// <returns>The formatted HTML string for the node and its children.</returns>
        public static string WriteNode(HtmlNode node, int indentLevel)
        {
            var sb = new StringBuilder();
            WriteNode(node, indentLevel, sb);
            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// Removes all formatting from HTML, including newlines, indentation, and excessive whitespace.
        /// Results in a single-line, compact HTML string.
        /// </summary>
        /// <param name="html">The HTML string to serialize.</param>
        /// <returns>A serialized, single-line HTML string with no formatting.</returns>
        public static string HtmlFormatRemove(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            html = html.Replace(Environment.NewLine, string.Empty);
            html = html.Replace(Indentation, string.Empty);
            html = NewLineRegex.Replace(html, string.Empty);
            html = WhitespaceRegex.Replace(html, " ");
            html = SpecialCharsRegex.Replace(html, string.Empty);
            html = TagSpacingRegex.Replace(html, "$1$2");
            html = LeadingSpaceRegex.Replace(html, "$1");

            return html;
        }

        #region Html tag manipulations

        /// <summary>
        /// Delete an html tag with or without replacement
        /// </summary>
        /// <param name="html"></param>
        /// <param name="tag"></param>
        /// <param name="openReplacement"></param>
        /// <param name="closeReplacement"></param>
        /// <returns></returns>
        public static string RemoveTag(string html, string tag, string openReplacement = "", string closeReplacement = "")
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(tag))
            {
                return html;
            }

            var openTagRegex = new Regex($"<{Regex.Escape(tag.ToLower())}[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var closeTagRegex = new Regex($"</{Regex.Escape(tag.ToUpper())}[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            html = openTagRegex.Replace(html, openReplacement);
            html = closeTagRegex.Replace(html, closeReplacement);

            return html;
        }

        /// <summary>
        /// Transform html header (h1, h2, ...) to html bold (b)
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HeaderToBold(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var openHeaderRegex = new Regex(@"<h[0-9][^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var closeHeaderRegex = new Regex(@"</h[0-9][^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            html = openHeaderRegex.Replace(html, "<br><br><b>");
            html = closeHeaderRegex.Replace(html, "</b><br>");

            return html;
        }

        public static string BrBrToP(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            html = HtmlFormatRemove(html);
            html = BrBrRegex.Replace(html, "{{BREAK}}");
            html = Regex.Replace(html, @"(.*?)(\{\{BREAK\}\}|\z)", m =>
            {
                string trimmed = m.Value.Trim();
                return string.IsNullOrEmpty(trimmed) ? string.Empty : $"<p>{trimmed}</p>";
            }, RegexOptions.Compiled);
            html = html.Replace("{{BREAK}}", string.Empty);
            html = EmptyParagraphRegex.Replace(html, string.Empty);

            return html;
        }

        public static string BrRemove(string html, int countInitial, int countFinal)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            html = HtmlFormatRemove(html);

            // Optimize string concatenation
            string final = countFinal > 0
                ? string.Concat(Enumerable.Repeat("<br>", countFinal))
                : string.Empty;

            var brRemovalRegex = new Regex(@"(<br>){" + countInitial + "}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            html = brRemovalRegex.Replace(html, final);

            return html;
        }

        /// <summary>
        /// Remove html paragraph (p)
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ParagraphRemove(string html)
        {
            return RemoveTag(html, "p", "", "<br><br>");
        }

        #endregion

        /// <summary>
        /// Convert Markdown to html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string MarkdownToHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            // List
            html = BrAsteriskRegex.Replace(html, "*");
            html = BrDashRegex.Replace(html, "-");
            html = BrPlusRegex.Replace(html, "+");

            html = Markup.MarkdownToHtml(html);

            html = MarkdownImageRegex.Replace(html, "<img src=\"$1\" width=\"100%\"/>");

            return html;
        }

        public static string SteamRemoveAbout(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            html = NewLineRegex.Replace(html, string.Empty);
            html = HtmlFormatRemove(html);

            string htmlLower = html.ToLower(); // Convert once

            int bestIndex = -1;
            int bestLength = 0;

            // Find the first matching header
            foreach (string about in AboutGameHeaders)
            {
                int index = htmlLower.IndexOf(about, StringComparison.Ordinal);
                if (index > -1 && (bestIndex == -1 || index < bestIndex))
                {
                    bestIndex = index;
                    bestLength = about.Length;
                }
            }

            if (bestIndex > -1)
            {
                html = html.Substring(bestIndex + bestLength);
            }

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
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var imgRegex = new Regex(@"(<img[^>]*>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return imgRegex.Replace(html, "<div style=\"text-align: center;\">$1</div>");
        }

        /// <summary>
        /// Add a image width style at 100%
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string Add100PercentStyle(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var parser = new HtmlParser();
            var document = parser.Parse(html);

            foreach (AngleSharp.Dom.IElement imgTag in document.QuerySelectorAll("img"))
            {
                imgTag.RemoveAttribute("width");

                string currentStyle = imgTag.GetAttribute("style") ?? string.Empty;

                if (!string.IsNullOrEmpty(currentStyle))
                {
                    var styleProperties = currentStyle.Split(';')
                        .Where(s => !string.IsNullOrWhiteSpace(s) && !s.ToLower().Contains("width"))
                        .Select(s => s.Trim());

                    currentStyle = string.Join(";", styleProperties);
                    if (!string.IsNullOrEmpty(currentStyle))
                    {
                        currentStyle += ";";
                    }
                }

                imgTag.SetAttribute("style", currentStyle + "width: 100%;");
            }

            return document.Body.InnerHtml;
        }

        /// <summary>
        /// Remove image width & height style
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveSizeStyle(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var parser = new HtmlParser();
            var document = parser.Parse(html);

            foreach (AngleSharp.Dom.IElement imgTag in document.QuerySelectorAll("img"))
            {
                imgTag.RemoveAttribute("width");
                imgTag.RemoveAttribute("height");

                string currentStyle = imgTag.GetAttribute("style") ?? string.Empty;

                if (!string.IsNullOrEmpty(currentStyle))
                {
                    var styleProperties = currentStyle.Split(';')
                        .Where(s => !string.IsNullOrWhiteSpace(s) &&
                                   !s.ToLower().Contains("width") &&
                                   !s.ToLower().Contains("height"))
                        .Select(s => s.Trim());

                    currentStyle = string.Join(";", styleProperties);
                }

                imgTag.SetAttribute("style", currentStyle);
            }

            return document.Body.InnerHtml;
        }

        #endregion
    }
}