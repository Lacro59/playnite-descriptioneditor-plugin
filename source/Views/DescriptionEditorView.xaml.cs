using DescriptionEditor.PlayniteResources.Controls;
using Playnite.SDK;
using CommonPluginsShared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DescriptionEditor.Views.Interface;
using CommonPlayniteShared.Extensions.Markup;
using System.Windows.Input;

namespace DescriptionEditor.Views
{
    /// <summary>
    /// Logique d'interaction pour DescriptionEditorView.xaml
    /// </summary>
    public partial class DescriptionEditorView : UserControl
    {
        private static IResourceProvider resources { get; set; } = new ResourceProvider();

        public string Description { get; set; }
        private TextBox _TextDescription { get; set; }

        private HtmlTextView htmlTextView { get; set; } = new HtmlTextView();


        private bool DisableEvent { get; set; } = false;
        private int IndexUndo { get; set; } = 0;
        private List<string> ListUndo { get; set; } = new List<string>();


        public DescriptionEditorView(TextBox TextDescription)
        {
            InitializeComponent();

            _TextDescription = TextDescription;
            Description = TextDescription.Text;

            PlayniteTools.SetThemeInformation();
            string DescriptionViewFile = ThemeFile.GetFilePath("DescriptionView.html");

            Common.LogDebug(true, $"{DescriptionViewFile}");

            try
            {
                htmlTextView.Visibility = Visibility.Visible;
                htmlTextView.TemplatePath = DescriptionViewFile;
                htmlTextView.HtmlText = Description;

                htmlTextView.HtmlFontSize = (double)resources.GetResource("FontSize");
                htmlTextView.HtmlFontFamily = (FontFamily)resources.GetResource("FontFamily");
                htmlTextView.HtmlForeground = (Color)resources.GetResource("TextColor");
                htmlTextView.LinkForeground = (Color)resources.GetResource("GlyphColor");

                PART_HtmlDescription.Children.Add(htmlTextView);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, "Error on creation HtmlTextView");
            }

            DataContext = this;
        }


        private void BtEditorCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }

        private void BtEditorOK_Click(object sender, RoutedEventArgs e)
        {
            _TextDescription.Text = DescriptionActual.Text;
            ((Window)this.Parent).Close();
        }


        #region Common formatter 
        private void BtHtmlFormat_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.HtmlFormat(DescriptionActual.Text);
        }


        private void BtHtmlFormatRemove_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.HtmlFormatRemove(DescriptionActual.Text);
        }

        private void BtMarkdownToHtml_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.MarkdownToHtml(DescriptionActual.Text);
        }

        private string RemoveParagraph(string Text)
        {
            if (Text.Substring(Text.Length - 1, 1) == "\n")
            {
                Text = Text.Substring(0, Text.Length - 1);
            }

            string StartString = Text.Substring(0, 3).ToLower();
            string EndString = Text.Substring(Text.Length - 4, 4).ToLower();

            if (StartString == "<p>" && EndString == "</p>")
            {
                Text = Text.Substring(3, Text.Length - 3);
                Text = Text.Substring(0, Text.Length - 4);
            }

            return Text;
        }


        private void BtHtoB_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.HeaderToBold(DescriptionActual.Text);
        }

        private void BtPremove_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.ParagraphRemove(DescriptionActual.Text);
        }


        private void BtInsertImg_Click(object sender, RoutedEventArgs e)
        {
            ImageContextMenu imageContextMenu = ((ImageContextMenu)btAddImgContextMenu.Items[0]);

            string imgUrl = imageContextMenu.imgUrl;
            bool imgCent = imageContextMenu.imgCent;
            bool imgPx = imageContextMenu.imgPx;
            int imgSize = imageContextMenu.imgSize;
            bool imgLeft = imageContextMenu.imgLeft;
            bool imgCenter = imageContextMenu.imgCenter;
            bool imgRight = imageContextMenu.imgRight;


            string imgAdded = "<img src=\"{0}\" style=\"{1}\">";
            string style = string.Empty;
            if (!string.IsNullOrEmpty(imgUrl))
            {
                if (imgCent && imgSize > 0)
                {
                    style += $"width: {imgSize }%;";
                }
                else if (imgPx && imgSize > 0)
                {
                    style += $"width: {imgSize }px;";
                }

                if (imgPx || (imgCent && imgSize < 100))
                {
                    if (imgRight)
                    {
                        imgAdded = "<table style=\"border: 0; width: 100 %;border-spacing: 10px;\"><tr>"   
                             + "<td>" + resources.GetString("LOCDescriptionEditorTextHere") + "</td>"
                             + $"<td style=\"{style}vertical - align: top;\">"
                             + imgAdded
                             + "</td>"
                             + "</tr></table>";
                    }
                    if (imgCenter)
                    {
                        imgAdded = "<div style=\"text-align: center;\">"
                            + imgAdded
                            + "</div>";
                    }
                    if (imgLeft)
                    {
                        imgAdded = "<table style=\"border: 0; width: 100 %;border-spacing: 10px;\"><tr>"
                             + $"<td style=\"{style}vertical-align: top;\">"
                             + imgAdded
                             + "</td>"
                             + "<td>" + resources.GetString("LOCDescriptionEditorTextHere") + "</td>"
                             + "</tr></table>";
                    }
                }

                imgAdded = "\r\n" + HtmlHelper.HtmlFormat(string.Format(imgAdded, imgUrl, style)) + "\r\n";

                if (DescriptionActual.CaretIndex <= 0)
                {
                    DescriptionActual.Focus();
                    DescriptionActual.Text = DescriptionActual.Text.Insert(DescriptionActual.CaretIndex, imgAdded);
                }
                else
                {
                    DescriptionActual.Text = DescriptionActual.Text.Insert(DescriptionActual.CaretIndex, imgAdded);
                }
            }

            btAddImgContextMenu.Visibility = Visibility.Collapsed;
        }

        private void BtRemoveImg_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.RemoveTag(DescriptionActual.Text, "img");
        }

        private void Bt100PercentImg_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.Add100PercentStyle(DescriptionActual.Text);
        }

        private void BtRemoveImgStyle_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.RemoveSizeStyle(DescriptionActual.Text);
        }

        private void BtCenterImg_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.CenterImage(DescriptionActual.Text);
        }


        private void BtBr2to1_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.BrRemove(DescriptionActual.Text, 2, 1);
        }

        private void BtBr3to1_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.BrRemove(DescriptionActual.Text, 3, 1);
        }

        private void BtBr3to2_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.BrRemove(DescriptionActual.Text, 3, 2);
        }
        #endregion


        #region Steam formatter 
        private void SteamRemoveAbout_click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.SteamRemoveAbout(DescriptionActual.Text);
        }
        #endregion
        

        private void DescriptionActual_TextChanged(object sender, TextChangedEventArgs e)
        {
            htmlTextView.HtmlText = ((TextBox)sender).Text;

            if (!DisableEvent)
            {
                if (ListUndo.Count == 0)
                {
                    ListUndo.Add(((TextBox)sender).Text);
                }
                else if (IndexUndo == ListUndo.Count - 1)
                {
                    ListUndo.Add(((TextBox)sender).Text);
                }
                else
                {
                    ListUndo.RemoveRange(IndexUndo, ListUndo.Count - 1 - IndexUndo);
                    ListUndo.Add(((TextBox)sender).Text);
                }

                IndexUndo = ListUndo.Count - 1;

                if (ListUndo.Count > 1)
                {
                    PART_Undo.IsEnabled = true;
                    PART_Redo.IsEnabled = false;
                }
            }
            else
            {
                DisableEvent = false;
            }
        }


        private void BtAddImg_Click(object sender, RoutedEventArgs e)
        {
            btAddImgContextMenu.Visibility = Visibility.Visible;
        }


        #region Undo/Redo
        private void PART_Undo_Click(object sender, RoutedEventArgs e)
        {
            DisableEvent = true;

            IndexUndo = IndexUndo - 1;
            DescriptionActual.Text = ListUndo[IndexUndo];

            PART_Redo.IsEnabled = true;
            if (IndexUndo == 0)
            {
                PART_Undo.IsEnabled = false;
            }
        }

        private void PART_Redo_Click(object sender, RoutedEventArgs e)
        {
            DisableEvent = true;

            IndexUndo = IndexUndo + 1;
            DescriptionActual.Text = ListUndo[IndexUndo];

            PART_Undo.IsEnabled = true;
            if (IndexUndo == ListUndo.Count - 1)
            {
                PART_Redo.IsEnabled = false;
            }
        }

        private void DescriptionActual_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Z && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            {
                if (PART_Undo.IsEnabled)
                {
                    PART_Undo_Click(null, null);
                }
                e.Handled = true;
            }

            if (e.Key == Key.Y && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            {
                if (PART_Redo.IsEnabled)
                {
                    PART_Redo_Click(null, null);
                }
                e.Handled = true;
            }
        }
        #endregion
    }
}
