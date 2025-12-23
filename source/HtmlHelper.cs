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
        private static string Indentation => "    ";


        #region Html identation

        public static string HtmlFormat(string html)
        {
            html = HtmlFormatRemove(html);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            html = string.Empty;
            if (doc.DocumentNode != null)
            {
                foreach (HtmlNode node in doc.DocumentNode.ChildNodes)
                {
                    html += WriteNode(node, 0);
                }
            }
            return html;

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
            string result = string.Empty;

            // check parameter
            if (_node == null)
            {
                return string.Empty;
            }

            // init 
            string INDENT = Indentation;
            string NEW_LINE = Environment.NewLine;

            // case: no children
            if (_node.HasChildNodes == false)
            {
                for (int i = 0; i < _indentLevel; i++)
                {
                    result += INDENT;
                }
                result += _node.OuterHtml;
                result += NEW_LINE;
            }

            // case: node has childs
            else
            {
                // indent
                for (int i = 0; i < _indentLevel; i++)
                {
                    result += INDENT;
                }

                // open tag
                result += string.Format("<{0}", _node.Name);
                if (_node.HasAttributes)
                {
                    foreach (HtmlAttribute attr in _node.Attributes)
                    {
                        result += string.Format(" {0}=\"{1}\"", attr.Name, attr.Value);
                    }
                }
                result += string.Format(">{0}", NEW_LINE);

                // childs
                foreach (HtmlNode chldNode in _node.ChildNodes)
                {
                    result += WriteNode(chldNode, _indentLevel + 1);
                }

                // close tag
                for (int i = 0; i < _indentLevel; i++)
                {
                    result += INDENT;
                }
                result += string.Format("</{0}>{1}", _node.Name, NEW_LINE);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Serialize html text
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlFormatRemove(string html)
        {
            html = html.Replace(Environment.NewLine, string.Empty);
            html = html.Replace(Indentation, string.Empty);
            html = Regex.Replace(html, @"\r\n?|\n", string.Empty);
            html = Regex.Replace(html, @"\s+", " ");
            html = Regex.Replace(html, @"[⠀⠀⠀⠀⠀⠀⠀⠀]", string.Empty);
            html = Regex.Replace(html, @"(>)\s+(<)", "$1$2", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"\s+(<)", "$1", RegexOptions.IgnoreCase);
            return html;
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

        public static string BrBrToP(string html)
        {
            html = HtmlFormatRemove(html);
            html = Regex.Replace(html, @"<[/]?br><[/]?br>", "{{BREAK}}", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(.*?)(\{\{BREAK\}\}|\z)", m => m.Value.Trim().IsNullOrEmpty() ? string.Empty : $"<p>{m.Value.Trim()}</p>");
            html = Regex.Replace(html, "{{BREAK}}", string.Empty);
            html = Regex.Replace(html, "<p></p>", string.Empty, RegexOptions.IgnoreCase);
            return html;
        }

        public static string BrRemove(string html, int countInitial, int countFinal)
        {
            html = HtmlFormatRemove(html);

            string final = string.Empty;
            for (int i = 0; i < countFinal; i++)
            {
                final += "<br>";
            }

            html = Regex.Replace(html, @"(<br>){" + countInitial + "}", final, RegexOptions.IgnoreCase);
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

        public static string SteamRemoveAbout(string html)
        {
            List<string> AboutGame = new List<string>();
            AboutGame.Add("<h1>about the game</h1>".ToLower());
            AboutGame.Add("<h1>à propos du jeu</h1>".ToLower());
            AboutGame.Add("<h1>Относно играта</h1>".ToLower());
            AboutGame.Add("<h1>关于游戏</h1>".ToLower());
            AboutGame.Add("<h1>關於此遊戲</h1>".ToLower());
            AboutGame.Add("<h1>O hře</h1>".ToLower());
            AboutGame.Add("<h1>Om spillet</h1>".ToLower());
            AboutGame.Add("<h1>Info over het spel</h1>".ToLower());
            AboutGame.Add("<h1>Tietoja pelistä</h1>".ToLower());
            AboutGame.Add("<h1>Über das Spiel</h1>".ToLower());
            AboutGame.Add("<h1>Σχετικά με το παιχνίδι</h1>".ToLower());
            AboutGame.Add("<h1>A játékról:&nbsp;</h1>".ToLower());
            AboutGame.Add("<h1>Informazioni sul gioco</h1>".ToLower());
            AboutGame.Add("<h1>ゲームについて</h1>".ToLower());
            AboutGame.Add("<h1>게임 정보</h1>".ToLower());
            AboutGame.Add("<h1>Om spillet</h1>".ToLower());
            AboutGame.Add("<h1>Informacje o&nbsp;grze</h1>".ToLower());
            AboutGame.Add("<h1>Acerca do Jogo</h1>".ToLower());
            AboutGame.Add("<h1>Sobre o jogo</h1>".ToLower());
            AboutGame.Add("<h1>Despre joc</h1>".ToLower());
            AboutGame.Add("<h1>Об игре</h1>".ToLower());
            AboutGame.Add("<h1>Acerca del juego</h1>".ToLower());
            AboutGame.Add("<h1>Acerca del juego</h1>".ToLower());
            AboutGame.Add("<h1>Om spelet</h1>".ToLower());
            AboutGame.Add("<h1>ข้อมูลเกม</h1>".ToLower());
            AboutGame.Add("<h1>Oyun Açıklaması</h1>".ToLower());
            AboutGame.Add("<h1>Про гру</h1>".ToLower());
            AboutGame.Add("<h1>Về trò chơi này</h1>".ToLower());

            bool isFind = false;
            int index = -1;
            string aboutFind = string.Empty;

            html = Regex.Replace(html, @"\r\n", string.Empty);
            html = HtmlHelper.HtmlFormatRemove(html);

            foreach (string about in AboutGame)
            {
                index = html.ToLower().IndexOf(about);
                if (index > -1)
                {
                    aboutFind = about;
                    isFind = true;
                    break;
                }
            }

            if (isFind)
            {
                html = html.Substring(index + aboutFind.Length);
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
            HtmlParser parser = new HtmlParser();
            AngleSharp.Dom.Html.IHtmlDocument document = parser.Parse(html);

            foreach (AngleSharp.Dom.IElement imgTag in document.QuerySelectorAll("img"))
            {
                _ = imgTag.RemoveAttribute("width");

                List<string> actualStyle = new List<string>();
                if (!imgTag.GetAttribute("style").IsNullOrEmpty())
                {
                    actualStyle = imgTag.GetAttribute("style").Split(';').ToList(); ;
                }

                string newStyle = string.Empty;
                if (actualStyle.Count > 0)
                {
                    foreach (string styleProperty in actualStyle)
                    {
                        if (!styleProperty.ToLower().Contains("width"))
                        {
                            newStyle += styleProperty.ToLower() + ";";
                        }
                    }
                }

                imgTag.SetAttribute("style", newStyle + "width: 100%;");
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
            HtmlParser parser = new HtmlParser();
            AngleSharp.Dom.Html.IHtmlDocument document = parser.Parse(html);

            foreach (AngleSharp.Dom.IElement imgTag in document.QuerySelectorAll("img"))
            {
                _ = imgTag.RemoveAttribute("width");
                _ = imgTag.RemoveAttribute("height");

                List<string> actualStyle = new List<string>();
                if (!imgTag.GetAttribute("style").IsNullOrEmpty())
                {
                    actualStyle = imgTag.GetAttribute("style").Split(';').ToList(); ;
                }

                string newStyle = string.Empty;
                if (actualStyle.Count > 0)
                {
                    foreach (string styleProperty in actualStyle)
                    {
                        if (!styleProperty.ToLower().Contains("width") && !styleProperty.ToLower().Contains("height"))
                        {
                            newStyle += styleProperty.ToLower() + ";";
                        }
                    }
                }

                imgTag.SetAttribute("style", newStyle);
            }

            return document.Body.InnerHtml;
        }

        #endregion
    }
}