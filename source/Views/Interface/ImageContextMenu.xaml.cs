using Playnite.SDK;
using CommonPluginsShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DescriptionEditor.Views.Interface
{
    /// <summary>
    /// Logique d'interaction pour ImageContextMenu.xaml
    /// </summary>
    public partial class ImageContextMenu : Grid
    {
        private static IResourceProvider ResourceProvider => new ResourceProvider();

        public event RoutedEventHandler BtInsertImgClick;

        public string ImgUrl { get; set; } = string.Empty;
        public bool ImgCent { get; set; } = true;
        public bool ImgPx { get; set; } = false;
        public int ImgSize { get; set; } = 0;
        public bool ImgLeft { get; set; } = false;
        public bool ImgCenter { get; set; } = true;
        public bool ImgRight { get; set; } = false;


        public ImageContextMenu()
        {
            InitializeComponent();
        }

        private void ImgSize_KeyUp(object sender, KeyEventArgs e)
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

            foreach (var ui in UI.FindVisualChildren<Border>((ContextMenu)((FrameworkElement)((FrameworkElement)sender).Parent).Parent))
            {
                if (ui.Name == "HoverBorder")
                {
                    ui.Background = (System.Windows.Media.Brush)ResourceProvider.GetResource("NormalBrush");
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
            ImgUrl = tbImgUrl.Text;
            ImgCent = (bool)ckImgCent.IsChecked;
            ImgPx = (bool)ckImgPx.IsChecked;
            int.TryParse(tbImgSize.Text, out int size);
            ImgSize = size;
            ImgLeft = (bool)rbImgLeft.IsChecked;
            ImgCenter = (bool)rbImgCenter.IsChecked;
            ImgRight = (bool)rbImgRight.IsChecked;

            BtInsertImgClick?.Invoke(this, new RoutedEventArgs());
        }
    }
}