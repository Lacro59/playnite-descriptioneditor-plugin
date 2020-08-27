using Playnite.Controls;
using Playnite.SDK;
using PluginCommon;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DescriptionEditor.Views
{
    /// <summary>
    /// Logique d'interaction pour DescriptionEditorView.xaml
    /// </summary>
    public partial class DescriptionEditorView : WindowBase
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        public string Description { get; set; }
        private TextBox TextDescription;


        public string imgUrl { get; set; } = "";
        public bool imgCent { get; set; } = true;
        public string imgSize { get; set; } = "";
        public bool imgLeft { get; set; } = false;
        public bool imgCenter { get; set; } = true;
        public bool imgRight { get; set; } = false;


        public DescriptionEditorView(TextBox TextDescription)
        {
            this.TextDescription = TextDescription;
            InitializeComponent();

            Description = TextDescription.Text;

            DataContext = this;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Tools.DesactivePlayniteWindowControl(this);
        }

        private void BtEditorCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtEditorOK_Click(object sender, RoutedEventArgs e)
        {
            TextDescription.Text = DescriptionActual.Text;
            this.Close();
        }


        private void ImgSize_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ((TextBox)sender).Text = Regex.Replace(((TextBox)sender).Text, "[^0-9]+", "");
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            imgUrl = "";
            imgCent = true;
            imgSize = "";
            imgLeft = false;
            imgCenter = true;
            imgRight = false;

            tbImgUrl.Text = imgUrl;
            ckImgCent.IsChecked = imgCent;
            tbImgSize.Text = imgSize;
            rbImgLeft.IsChecked = imgLeft;
            rbImgCenter.IsChecked = imgCenter;
            rbImgRight.IsChecked = imgRight;

            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)((Grid)sender).Parent))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
        }


        #region Common formatter 
        private void BtHtmlFormat_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.HtmlFormat(DescriptionActual.Text);
        }

        private void BtInsertImg_Click(object sender, RoutedEventArgs e)
        {
            string imgAdded = "<img src=\"{0}\" style=\"{1}\">";
            string style = "";
            if (!string.IsNullOrEmpty(imgUrl))
            {
                if (imgCent)
                {
                    style += $"width: 100%;";
                }
                else
                {
                    if (!string.IsNullOrEmpty(imgSize))
                    {
                        style += $"width: {imgSize }px;";

                    }
                    if (imgLeft)
                    {
                        style += $"width: 100%;";
                        //style += $"float: left;";
                        imgAdded = "<table style=\"border: 0; width: 100 %;border-spacing: 10px;\"><tr><td>Your text here!</td>"
                             + $"<td style=\"width: {imgSize }px;vertical-align: top;\">"
                             + imgAdded
                             + "</td>"
                             + "</tr></table>";
                    }
                    if (imgCenter)
                    {
                        //style += $"margin-left: auto;margin-right: auto;";
                        imgAdded = "<div style=\"text-align: center;\">"
                            + imgAdded
                            + "</div>";
                    }
                    if (imgRight)
                    {
                        style += $"width: 100%;";
                        //style += $"float: right;";
                        imgAdded = "<table style=\"border: 0; width: 100 %;border-spacing: 10px;\"><tr>"
                             + $"<td style=\"width: {imgSize }px;vertical-align: top;\">"
                             + imgAdded
                             + "</td>"
                             + "<td>Your text here!</td>"
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
        }

        private void BtRemoveImg_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.RemoveTag(DescriptionActual.Text, "img");
        }
        #endregion

        #region Steam formatter 
        private void SteamRemoveAbout_click(object sender, RoutedEventArgs e)
        {
            int index = DescriptionActual.Text.ToLower().IndexOf("<h1>about the game</h1>");
            if (index > -1)
            {
                DescriptionActual.Text = DescriptionActual.Text.Substring(index + 23);
            }
        }
        #endregion
    }
}
