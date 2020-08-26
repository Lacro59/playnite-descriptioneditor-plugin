using Playnite.Controls;
using Playnite.SDK;
using PluginCommon;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DescriptionEditor.Views
{
    /// <summary>
    /// Logique d'interaction pour DescriptionEditorMultiMetadata.xaml
    /// </summary>
    public partial class DescriptionEditorMultiMetadata : WindowBase
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


        public DescriptionEditorMultiMetadata(TextBox TextDescription)
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
            TextDescription.Text = Description;
            this.Close();
        }

        private void ImgSize_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ((TextBox)sender).Text = Regex.Replace(((TextBox)sender).Text, "[^0-9]+", "");
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
                        //style += $"float: left;";
                        imgAdded = "<table style=\"border: 0; width: 100 %;\">\r\n<tr>\r\n<td>\r\nYour text here!\r\n</td>\r\n"
                             + $"<td style=\"width: {imgSize }px;vertical-align: top;\">\r\n"
                             + imgAdded
                             + "\r\n</td>\r\n"
                             + "</tr>\r\n</table>";
                    }
                    if (imgCenter)
                    {
                        //style += $"margin-left: auto;margin-right: auto;";
                        imgAdded = "<div style=\"text-align: center;\">\r\n"
                            + imgAdded 
                            + "\r\n</div>";
                    }
                    if (imgRight)
                    {
                        //style += $"float: right;";
                        imgAdded = "<table style=\"border: 0; width: 100 %;\">\r\n<tr>\r\n"
                             + $"<td style=\"width: {imgSize }px;vertical-align: top;\">\r\n"
                             + imgAdded
                             + "\r\n</td>\r\n"
                             + "<td>\r\nYour text here!\r\n</td>\r\n"
                             + "</tr>\r\n</table>";
                    }
                }

                imgAdded = "\r\n" + string.Format(imgAdded, imgUrl, style) + "\r\n";

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
                    logger.Debug("find0");
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
        }
    }
}
