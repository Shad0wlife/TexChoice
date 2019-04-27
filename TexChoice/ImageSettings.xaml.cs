using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using DirectXTexNet;

namespace TexChoice
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private float alpha = 1.0f;
        private BitmapImage pre;
        private string previewPath;

        public MainWindow()
        {
            InitializeComponent();
            Preview.CacheMode = new BitmapCache();
            OrionSingle.IsChecked = true;
            AlphaSlider.Value = 1.0;
        }

        private void OrionSingle_Checked(object sender, RoutedEventArgs e)
        {
            previewPath = "orionSingle/SingleOrion";
            redrawPreview();
        }

        private void OrionDouble_Checked(object sender, RoutedEventArgs e)
        {
            previewPath = "orionDouble/DoubleOrion";
            redrawPreview();
        }

        private void RoseSingle_Checked(object sender, RoutedEventArgs e)
        {
            previewPath = "roseSingle/SingleRose";
            redrawPreview();
        }

        private void RoseDouble_Checked(object sender, RoutedEventArgs e)
        {
            previewPath = "roseDouble/DoubleRose";
            redrawPreview();
        }

        private void Cluster_Checked(object sender, RoutedEventArgs e)
        {
            previewPath = "cluster/Cluster";
            redrawPreview();
        }

        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            alpha = (float)AlphaSlider.Value;
            alphaValue.Content = String.Format("{0:f1} %", alpha*100.0f);
            redrawPreview();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ImageSize next = new ImageSize(alpha, previewPath);
            next.Show();
            Close();
        }

        private void redrawPreview()
        {
            generatePreview();
            pre = new BitmapImage();
            pre.BeginInit();
            pre.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            pre.CacheOption = BitmapCacheOption.OnLoad;
            pre.UriSource = new Uri(getPreviewPath(), UriKind.RelativeOrAbsolute);
            pre.EndInit();
            pre.Freeze();
            Preview.Source = pre;
        }

        private void generatePreview()
        {
            ScratchImage image = TexHelper.Instance.LoadFromWICFile(getFullPath(previewPath + "_preview.png"), WIC_FLAGS.NONE);
            image = image.TransformImage(changeAlpha);
            image.SaveToWICFile(0, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG), getPreviewPath());
        }

        private string getFullPath(string localPath)
        {
            return AppDomain.CurrentDomain.BaseDirectory + "rawData/" + localPath;
        }

        private string getPreviewPath()
        {
            return getFullPath("/temp/preview.png");
        }

        private unsafe void changeAlpha(IntPtr dest, IntPtr src, IntPtr width, IntPtr y)
        {
            float* target = (float*)dest.ToPointer();
            float* source = (float*)src.ToPointer();
            for(int cnt = 0; cnt < width.ToInt32(); cnt++)
            {
                for(int color = 0; color < 3; color++)
                {
                    *target = *source;
                    target++;
                    source++;
                }
                *target = clamp(*source * alpha);
                target++;
                source++;
            }
        }

        private float clamp(float val)
        {
            return Math.Max(0.0f, Math.Min(1.0f, val));
        }
    }
}
