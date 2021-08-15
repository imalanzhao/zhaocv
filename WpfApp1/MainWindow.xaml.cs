using core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Bitmap _bmp;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "pictures | *.jpg;*.bmp;*.tif";
            var result = dg.ShowDialog();
            if (result == null || !result.Value) return;


            Stopwatch watch = Stopwatch.StartNew();
            ZImage<byte> image = ZImage.Read(dg.FileName);

            watch.Stop();
            //Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            tb.Text = watch.Elapsed.TotalMilliseconds.ToString();

            _bmp = image.ToBitmap();
            ImageCtrl.Source = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_bmp);

            _bmp.Save(@"D:\mybmp.jpg");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "pictures | *.jpg;*.bmp;*.tif";
            var result = dg.ShowDialog();
            if (result == null || !result.Value) return;

            Stopwatch watch = Stopwatch.StartNew();
            Mat img = Cv2.ImRead(dg.FileName, ImreadModes.Color);
            watch.Stop();
            tb.Text = watch.Elapsed.TotalMilliseconds.ToString();

            Mat[] mats = Cv2.Split(img);
            //Cv2.ImWrite(@"D:\myopencvgray_blue.jpg", mats[0]);
            //Cv2.ImWrite(@"D:\myopencvgray_green.jpg", mats[1]);
            //Cv2.ImWrite(@"D:\myopencvgray_red.jpg", mats[2]);
            Bitmap bmp = BitmapConverter.ToBitmap(mats[0]);
        }

        private void ImageCtrl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(ImageCtrl);
            int row = (int)p.Y;
            int col = (int)p.X;

            if(_bmp != null)
            {
                System.Drawing.Color clr = _bmp.GetPixel(_bmp.Width - 1, _bmp.Height - 1);
                mtb.Text = $"{col},{row}   R:{clr.R}, G:{clr.G}, B:{clr.B}";
            }
        }

        private void win_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "pictures | *.jpg;*.bmp;*.tif";
            var result = dg.ShowDialog();
            if (result == null || !result.Value) return;

            Bitmap bitmap = new Bitmap(dg.FileName);
            _bmp = bitmap;
            ImageCtrl.Source = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(bitmap);
        }
    }
}
