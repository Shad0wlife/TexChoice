using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using System.IO.Compression;
using DirectXTexNet;
using SharpDX.Direct3D11;

namespace TexChoice
{
    /// <summary>
    /// Interaktionslogik für ImageSize.xaml
    /// </summary>
    public partial class ImageSize : Window
    {
        public ObservableCollection<ComboBoxItem> resolutions { get; set; }
        public ComboBoxItem selected { get; set; }

        private ScratchImage image;
        private float alpha;

        public ImageSize(float opacity, string imagePath)
        {
            InitializeComponent();
            DataContext = this;

            alpha = opacity;
            resolutions = new ObservableCollection<ComboBoxItem>();
            image = TexHelper.Instance.LoadFromWICFile(getFullPath("rawData/" + imagePath + ".png"), WIC_FLAGS.NONE);
            bool first = true;
            int width = image.GetMetadata().Width;
            int height = image.GetMetadata().Height;

            for (int size = Math.Min(width, height); size > 256; size /= 2)
            {
                ComboBoxItem item = new ComboBoxItem { Content = width.ToString() + "x" + height.ToString() };
                if (first)
                {
                    selected = item;
                    first = false;
                }
                resolutions.Add(item);
                width /= 2;
                height /= 2;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private string getFullPath(string localPath)
        {
            return AppDomain.CurrentDomain.BaseDirectory + localPath;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string choice = (string)((ComboBoxItem)ResolutionChoice.SelectedItem).Content;
            string[] parts = choice.Split('x');
            int width, height;
            int.TryParse(parts[0], out width);
            int.TryParse(parts[1], out height);
            if(image.GetMetadata().Width != width || image.GetMetadata().Height != height)
            {
                image = image.Resize(width, height, TEX_FILTER_FLAGS.DEFAULT);
            }
            image = image.TransformImage(changeAlpha);
            if (BC3.IsChecked == true)
            {
                image = image.Compress(DXGI_FORMAT.BC3_UNORM, TEX_COMPRESS_FLAGS.DEFAULT, 0.5f);
            }
            else if (BC7.IsChecked == true)
            {
                image = convertBC7(image);
            }
            string textureDir = getFullPath("work/textures/sky");
            System.IO.Directory.CreateDirectory(textureDir);

            image.SaveToDDSFile(DDS_FLAGS.NONE, textureDir + "/skyrimgalaxy.dds");
            image.Dispose();

            string folder = getFullPath("work");
            string target = getFullPath("UltraHiResNightsky.zip");

            ZipFile.CreateFromDirectory(folder, target, CompressionLevel.Fastest, false);

            Close();
        }

        private ScratchImage convertBC7(ScratchImage image)
        {
            SharpDX.DXGI.Factory1 factory = new SharpDX.DXGI.Factory1();
            SharpDX.DXGI.Adapter adapter = factory.GetAdapter(0);
            Device device = new Device(adapter);
            return image.Compress(device.NativePointer, DXGI_FORMAT.BC7_UNORM, TEX_COMPRESS_FLAGS.DEFAULT, 0.5f);
        }

        private unsafe void changeAlpha(IntPtr dest, IntPtr src, IntPtr width, IntPtr y)
        {
            float* target = (float*)dest.ToPointer();
            float* source = (float*)src.ToPointer();
            for (int cnt = 0; cnt < width.ToInt32(); cnt++)
            {
                for (int color = 0; color < 3; color++)
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
