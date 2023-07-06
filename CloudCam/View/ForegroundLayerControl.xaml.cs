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
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace CloudCam.View
{
    /// <summary>
    /// Interaction logic for ForegroundLayerControl.xaml
    /// </summary>
    public partial class ForegroundLayerControl : UserControl
    {
        private Size _frameSize = new Size(1280, 720);


        public ForegroundLayerControl()
        {
            InitializeComponent();
        }

        public void InitializeFrameSize(Size frameSize)
        {
            _frameSize = frameSize;
        }


        /// <summary>
        /// Add items that do not yet exist
        /// </summary>
        /// <param name="foregrounds"></param>
        public void SetItems(List<ForegroundImage> foregrounds)
        {
            TheCanvas.Children.Clear();

            var widthFactor  = ActualWidth / _frameSize.Width;
            var heightFactor = ActualHeight / _frameSize.Height;


            foreach (var foreground in foregrounds)
            {
                // TODO perform this only once
                var imageSource = foreground.image.ToBitmapSource();
                var rect = foreground.rect;
                
                Image image = new Image();
                image.Width = rect.Width * widthFactor;
                image.Height = rect.Height * heightFactor;
                Canvas.SetLeft(image, rect.X * widthFactor);
                Canvas.SetTop(image, rect.Y * heightFactor);
                image.Source = imageSource;
                TheCanvas.Children.Add(image);
            }
        }

        /// <summary>
        /// Move items that already exist
        /// </summary>
        /// <param name="rectangles"></param>
        public void MoveItems(List<Rect> rectangles)
        {
            if (TheCanvas.Children.Count != rectangles.Count)
            {
                throw new ArgumentException("The number of rects does not equal the number of children on the canvas");
            }

            // TODO adjust rectables based on width of the canvas

            for (int i = 0; i < rectangles.Count; i++)
            {
                Image image = TheCanvas.Children[i] as Image;
                image.Width = rectangles[i].Width;
                image.Height = rectangles[i].Height;
                Canvas.SetLeft(image, rectangles[i].X);
                Canvas.SetTop(image, rectangles[i].Y);
            }
        }

        public void Clear()
        {
            TheCanvas.Children.Clear();
        }
    }
}
