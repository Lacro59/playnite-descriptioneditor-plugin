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

        private static readonly HashSet<string> InlineTags = new HashSet<string>
        {
            "a", "span", "b", "strong", "i", "em", "u", "small", "code", "mark", "abbr"
        };

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
        private static HashSet<string> AboutGameHeaders => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

            if (node.NodeType == HtmlNodeType.Text)
            {
                sb.Append(node.InnerText);
                return;
            }

            bool isInline = InlineTags.Contains(node.Name);

            string indent = isInline ? string.Empty : new string(' ', indentLevel * Indentation.Length);
            string newLine = isInline ? string.Empty : Environment.NewLine;

            sb.Append(indent);
            sb.Append('<').Append(node.Name);

            foreach (var attr in node.Attributes)
            {
                sb.Append(' ').Append(attr.Name).Append("=\"").Append(attr.Value).Append('"');
            }

            sb.Append('>');

            if (!isInline)
            {
                sb.Append(newLine);
            }

            foreach (var child in node.ChildNodes)
            {
                WriteNode(child, isInline ? indentLevel : indentLevel + 1, sb);
            }

            sb.Append(isInline ? string.Empty : indent);
            sb.Append("</").Append(node.Name).Append('>');

            if (!isInline)
            {
                sb.Append(newLine);
            }
        }

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
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var doc = new HtmlDocument
            {
                OptionWriteEmptyNodes = true,
                OptionOutputAsXml = false,
                OptionAutoCloseOnEnd = true
            };

            doc.LoadHtml(html);

            NormalizeTextNodes(doc.DocumentNode);

            return doc.DocumentNode.InnerHtml;
        }

        private static void NormalizeTextNodes(HtmlNode node)
        {
            foreach (var child in node.ChildNodes.ToList())
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    var text = child.InnerText;
                    text = NewLineRegex.Replace(text, " ");
                    text = WhitespaceRegex.Replace(text, " ");
                    child.InnerHtml = text;
                }
                else
                {
                    NormalizeTextNodes(child);
                }
            }
        }

        #region Html tag manipulations

        /// <summary>
        /// Removes all instances of a specified HTML tag from the string, including its content.
        /// Optionally replaces the entire tag (including content) with a custom string.
        /// </summary>
        /// <param name="html">The HTML string to process.</param>
        /// <param name="tag">The tag name to remove (e.g., "img", "div", "video").</param>
        /// <param name="replacement">Optional replacement string for the entire tag and its content (default: empty string).</param>
        /// <returns>HTML string with the specified tags and their content removed or replaced.</returns>
        public static string RemoveTag(string html, string tag, string replacement = "")
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(tag))
            {
                return html;
            }

            // For self-closing tags (like img, br, hr), just remove the tag itself
            var selfClosingRegex = new Regex($"<{Regex.Escape(tag)}[^>]*/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            html = selfClosingRegex.Replace(html, replacement);

            // For tags with closing tags, remove everything from opening to closing tag (including content)
            var tagWithContentRegex = new Regex(
                $"<{Regex.Escape(tag)}[^>]*>.*?</{Regex.Escape(tag)}>",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline
            );
            html = tagWithContentRegex.Replace(html, replacement);

            return html;
        }

        /// <summary>
        /// Removes all instances of a specified HTML tag, with separate replacements for opening and closing tags.
        /// This method does NOT remove the content between tags, only the tags themselves.
        /// Use RemoveTag(html, tag, replacement) if you want to remove content as well.
        /// </summary>
        /// <param name="html">The HTML string to process.</param>
        /// <param name="tag">The tag name to remove (e.g., "p", "b").</param>
        /// <param name="openReplacement">Replacement string for opening tags (default: empty string).</param>
        /// <param name="closeReplacement">Replacement string for closing tags (default: empty string).</param>
        /// <returns>HTML string with the specified tags removed or replaced, content preserved.</returns>
        public static string RemoveTagKeepContent(string html, string tag, string openReplacement = "", string closeReplacement = "")
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
        /// Transforms HTML header tags (h1, h2, h3, etc.) to bold tags with line breaks.
        /// Opening headers become: br + br + b
        /// Closing headers become: /b + br
        /// </summary>
        /// <param name="html">The HTML string containing header tags.</param>
        /// <returns>HTML string with headers converted to bold formatting.</returns>
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

        /// <summary>
        /// Converts double br tags to paragraph tags, wrapping content between breaks in p tags.
        /// Removes empty paragraph tags after conversion.
        /// </summary>
        /// <param name="html">The HTML string to convert.</param>
        /// <returns>HTML string with paragraph tags instead of double br tags.</returns>
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

        /// <summary>
        /// Reduces consecutive br tags from a specified count to a different count.
        /// For example, can replace 3 consecutive br tags with 1 br tag.
        /// </summary>
        /// <param name="html">The HTML string to process.</param>
        /// <param name="countInitial">The number of consecutive br tags to match.</param>
        /// <param name="countFinal">The number of br tags to replace them with.</param>
        /// <returns>HTML string with reduced br tag sequences.</returns>
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
        /// Removes paragraph tags from HTML, replacing closing p tags with double br tags.
        /// Opening p tags are removed completely. Content inside paragraphs is preserved.
        /// </summary>
        /// <param name="html">The HTML string containing paragraph tags.</param>
        /// <returns>HTML string with paragraphs converted to line breaks.</returns>
        public static string ParagraphRemove(string html)
        {
            return RemoveTagKeepContent(html, "p", "", "<br><br>");
        }

        #endregion

        /// <summary>
        /// Converts Markdown syntax to HTML.
        /// Handles list markers (*, -, +) and Markdown image syntax ![alt](url).
        /// Images are converted to HTML img tags with 100% width.
        /// </summary>
        /// <param name="html">The string containing Markdown syntax.</param>
        /// <returns>HTML string with Markdown converted to HTML tags.</returns>
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

        /// <summary>
        /// Removes the "About the Game" header section from Steam game descriptions.
        /// Searches for localized variants of the header in 26+ languages and removes everything before it.
        /// This is useful for cleaning up Steam game descriptions to show only the actual description content.
        /// </summary>
        /// <param name="html">The HTML string containing a Steam game description.</param>
        /// <returns>HTML string with the "About the Game" header section removed.</returns>
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
        /// Wraps all img tags in centered div containers using inline CSS.
        /// Each image will be centered horizontally in its container.
        /// </summary>
        /// <param name="html">The HTML string containing img tags.</param>
        /// <returns>HTML string with images wrapped in centered divs.</returns>
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
        /// Adds a 100% width style to all img tags in the HTML.
        /// Removes any existing width attributes and width-related inline styles first.
        /// Useful for making images responsive and full-width.
        /// </summary>
        /// <param name="html">The HTML string containing img tags.</param>
        /// <returns>HTML string with all images set to 100% width via inline CSS.</returns>
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
        /// Removes all width and height attributes and styles from img tags.
        /// Allows images to use their natural dimensions or be sized by external CSS.
        /// Useful for removing fixed dimensions that might break responsive layouts.
        /// </summary>
        /// <param name="html">The HTML string containing img tags.</param>
        /// <returns>HTML string with width and height attributes/styles removed from all images.</returns>
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