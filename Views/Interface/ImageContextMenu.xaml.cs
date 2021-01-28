using Playnite.SDK;
using CommonPluginsShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DescriptionEditor.Views.Interface
{
    /// <summary>
    /// Logique d'interaction pour ImageContextMenu.xaml
    /// </summary>
    public partial class ImageContextMenu : Grid
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        public event RoutedEventHandler BtInsertImgClick;

        public string imgUrl { get; set; } = string.Empty;
        public bool imgCent { get; set; } = true;
        public bool imgPx { get; set; } = false;
        public int imgSize { get; set; } = 0;
        public bool imgLeft { get; set; } = false;
        public bool imgCenter { get; set; } = true;
        public bool imgRight { get; set; } = false;


        public ImageContextMenu()
        {
            InitializeComponent();
        }

        private void ImgSize_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            tbImgSize.Text = Regex.Replace(tbImgSize.Text, "[^0-9]+", string.Empty);

            int.TryParse(tbImgSize.Text, out int imgSize);
            gPosition.IsEnabled = true;
            if (imgSize >= 100 && (bool)ckImgCent.IsChecked)
            {
                gPosition.IsEnabled = false;
            }
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            tbImgUrl.Text = string.Empty;
            ckImgCent.IsChecked = true;
            ckImgPx.IsChecked = false;
            tbImgSize.Text = string.Empty;
            rbImgLeft.IsChecked = false;
            rbImgCenter.IsChecked = true;
            rbImgRight.IsChecked = false;
            btInsertImg.IsEnabled = false;
            gPosition.IsEnabled = true;

            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)((FrameworkElement)((FrameworkElement)sender).Parent).Parent))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
        }

        private void CkImgCent_Click(object sender, RoutedEventArgs e)
        {
            ckImgPx.IsChecked = !ckImgCent.IsChecked;
            ImgSize_KeyUp(null, null);
        }

        private void CkImgPx_Click(object sender, RoutedEventArgs e)
        {
            ckImgCent.IsChecked = !ckImgPx.IsChecked;
            ImgSize_KeyUp(null, null);
        }

        private void TbImgUrl_KeyUp(object sender, KeyEventArgs e)
        {
            btInsertImg.IsEnabled = false;
            if (Uri.IsWellFormedUriString(tbImgUrl.Text, UriKind.RelativeOrAbsolute))
            {
                btInsertImg.IsEnabled = true;
            }
        }

        public void BtInsertImg_Click(object sender, RoutedEventArgs e)
        {
            imgUrl = tbImgUrl.Text;
            imgCent = (bool)ckImgCent.IsChecked;
            imgPx = (bool)ckImgPx.IsChecked;
            int.TryParse(tbImgSize.Text, out int size);
            imgSize = size;
            imgLeft = (bool)rbImgLeft.IsChecked;
            imgCenter = (bool)rbImgCenter.IsChecked;
            imgRight = (bool)rbImgRight.IsChecked;

            if (BtInsertImgClick != null)
            {
                BtInsertImgClick(this, new RoutedEventArgs());
            }
        }
    }
}
