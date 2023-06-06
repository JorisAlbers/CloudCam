using System.Drawing;

namespace CloudCam.Printing
{
    public interface IPrinterManager
    {
        void Initialize();
        void Print(Bitmap image);
        int PrintsRemaining();
    }
}