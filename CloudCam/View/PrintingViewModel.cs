using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ReactiveUI;

namespace CloudCam.View
{
    public class PrintingViewModel : ReactiveObject
    {
        public string Message { get; }
        public BitmapSource Image1 { get; }
        public BitmapSource Image2 { get; }
        public BitmapSource Image3 { get; }

        public PrintingViewModel(string message, BitmapSource image1, BitmapSource image2, BitmapSource image3)
        {
            Message = message;
            Image1 = image1;
            Image2 = image2;
            Image3 = image3;
        }
    }
}
