using DescriptionEditor.PlayniteResources.Controls;
using Playnite.SDK;
using CommonPluginsShared;
using System;
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
    public partial class DescriptionEditorView : UserControl
    {
        private static IResourceProvider resources { get; set; } = new ResourceProvider();

        public string Description { get; set; }
        private TextBox _TextDescription { get; set; }
        private HtmlTextView htmlTextView { get; set; } = new HtmlTextView();

        private bool DisableEvent { get; set; } = false;
        private int IndexUndo { get; set; } = 0;
        private List<string> ListUndo { get; set; } = new List<string>();

        #region Constructor

        public DescriptionEditorView(TextBox TextDescription)
        {
            InitializeComponent();

            _TextDescription = TextDescription;
            Description = TextDescription.Text;

            PlayniteTools.SetThemeInformation();
            string DescriptionViewFile = ThemeFile.GetFilePath("DescriptionView.html");

            try
            {
                htmlTextView.Visibility = Visibility.Visible;
                htmlTextView.TemplatePath = DescriptionViewFile;
                htmlTextView.HtmlText = Description;

                htmlTextView.HtmlFontSize = (double)resources.GetResource("FontSize");
                htmlTextView.HtmlFontFamily = (FontFamily)resources.GetResource("FontFamily");
                htmlTextView.HtmlForeground = (Color)resources.GetResource("TextColor");
                htmlTextView.LinkForeground = (Color)resources.GetResource("GlyphColor");

                _ = HtmlPreviewPanel.Children.Add(htmlTextView);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, "Error on creation HtmlTextView");
            }

            DataContext = this;
        }

        #endregion

        #region Window Actions

        private void BtEditorCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }

        private void BtEditorOK_Click(object sender, RoutedEventArgs e)
        {
            _TextDescription.Text = DescriptionTextBox.Text;
            ((Window)this.Parent).Close();
        }

        #endregion

        #region Toolbar Actions (HTML / Markdown)

        private void BtHtmlFormat_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.HtmlFormat(DescriptionTextBox.Text);

        private void BtHtmlFormatRemove_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.HtmlFormatRemove(DescriptionTextBox.Text);

        private void BtMarkdownToHtml_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.MarkdownToHtml(DescriptionTextBox.Text);

        private void BtHtoB_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.HeaderToBold(DescriptionTextBox.Text);

        private void BtPremove_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.ParagraphRemove(DescriptionTextBox.Text);

        private void BtBrtoP_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.BrBrToP(DescriptionTextBox.Text);

        private void BtBr2to1_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.BrRemove(DescriptionTextBox.Text, 2, 1);

        private void BtBr3to1_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.BrRemove(DescriptionTextBox.Text, 3, 1);

        private void BtBr3to2_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.BrRemove(DescriptionTextBox.Text, 3, 2);

        private void SteamRemoveAbout_click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.SteamRemoveAbout(DescriptionTextBox.Text);

        #endregion

        #region Image Actions

        private void BtAddImg_Click(object sender, RoutedEventArgs e)
        {
            btAddImgContextMenu.Visibility = Visibility.Visible;
        }

        private void BtInsertImg_Click(object sender, RoutedEventArgs e)
        {
            var imageContextMenu = (ImageContextMenu)btAddImgContextMenu.Items[0];
            if (string.IsNullOrEmpty(imageContextMenu.ImgUrl)) return;

            string imgHtml = BuildImageHtml(imageContextMenu.ImgUrl, imageContextMenu);

            int insertPos = DescriptionTextBox.CaretIndex;
            _ = DescriptionTextBox.Focus();
            DescriptionTextBox.Text = DescriptionTextBox.Text.Insert(insertPos, "\r\n" + imgHtml + "\r\n");

            btAddImgContextMenu.Visibility = Visibility.Collapsed;
        }

        private string BuildImageHtml(string url, ImageContextMenu config)
        {
            string style = string.Empty;
            if (config.ImgCent && config.ImgSize > 0)
                style += $"width:{config.ImgSize}%;";
            else if (config.ImgPx && config.ImgSize > 0)
                style += $"width:{config.ImgSize}px;";

            string imgTag = $"<img src=\"{url}\" style=\"{style}\">";

            if (config.ImgCenter) imgTag = $"<div style=\"text-align:center;\">{imgTag}</div>";

            if (config.ImgLeft || config.ImgRight)
            {
                string tdLeft = config.ImgLeft ? imgTag : resources.GetString("LOCDescriptionEditorTextHere");
                string tdRight = config.ImgRight ? imgTag : resources.GetString("LOCDescriptionEditorTextHere");

                imgTag = $"<table style=\"border:0;width:100%;border-spacing:10px;\"><tr><td>{tdLeft}</td><td>{tdRight}</td></tr></table>";
            }

            return HtmlHelper.HtmlFormat(imgTag);
        }

        private void BtRemoveImg_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.RemoveTag(DescriptionTextBox.Text, "img");

        private void Bt100PercentImg_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.Add100PercentStyle(DescriptionTextBox.Text);

        private void BtRemoveImgStyle_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.RemoveSizeStyle(DescriptionTextBox.Text);

        private void BtCenterImg_Click(object sender, RoutedEventArgs e) =>
            DescriptionTextBox.Text = HtmlHelper.CenterImage(DescriptionTextBox.Text);

        #endregion

        #region Undo / Redo

        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            htmlTextView.HtmlText = DescriptionTextBox.Text;

            if (DisableEvent) { DisableEvent = false; return; }

            AddToUndo(DescriptionTextBox.Text);

            PART_Undo.IsEnabled = ListUndo.Count > 1;
            PART_Redo.IsEnabled = false;
        }

        private void AddToUndo(string text)
        {
            if (ListUndo.Count == 0 || IndexUndo == ListUndo.Count - 1)
            {
                ListUndo.Add(text);
            }
            else
            {
                ListUndo.RemoveRange(IndexUndo + 1, ListUndo.Count - IndexUndo - 1);
                ListUndo.Add(text);
            }
            IndexUndo = ListUndo.Count - 1;
        }

        private void PART_Undo_Click(object sender, RoutedEventArgs e)
        {
            if (IndexUndo <= 0) return;

            DisableEvent = true;
            IndexUndo--;
            DescriptionTextBox.Text = ListUndo[IndexUndo];

            PART_Redo.IsEnabled = true;
            PART_Undo.IsEnabled = IndexUndo > 0;
        }

        private void PART_Redo_Click(object sender, RoutedEventArgs e)
        {
            if (IndexUndo >= ListUndo.Count - 1) return;

            DisableEvent = true;
            IndexUndo++;
            DescriptionTextBox.Text = ListUndo[IndexUndo];

            PART_Undo.IsEnabled = true;
            PART_Redo.IsEnabled = IndexUndo < ListUndo.Count - 1;
        }

        private void DescriptionTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (e.Key == Key.Z && PART_Undo.IsEnabled) { PART_Undo_Click(null, null); e.Handled = true; }
                if (e.Key == Key.Y && PART_Redo.IsEnabled) { PART_Redo_Click(null, null); e.Handled = true; }
            }
        }

        #endregion

        #region Selection Highlight

        private void DescriptionTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            htmlTextView.HtmlText = DescriptionTextBox.Text;

            string selected = DescriptionTextBox.SelectedText;
            if (string.IsNullOrEmpty(selected)) return;

            int start = Math.Max(DescriptionTextBox.SelectionStart - 1, 0);
            int length = DescriptionTextBox.SelectionLength + 2;
            length = Math.Min(length, htmlTextView.HtmlText.Length - start);

            string extended = htmlTextView.HtmlText.Substring(start, length);
            if (!Regex.IsMatch(extended, @"^.?<") && !Regex.IsMatch(extended, @">.?$") && !Regex.IsMatch(extended, @"\/.?$"))
            {
                htmlTextView.HtmlText = htmlTextView.HtmlText.Replace(selected, $"<span style=\"background-color: yellow; color: black;\">{selected}</span>");
            }
        }

        #endregion
    }
}