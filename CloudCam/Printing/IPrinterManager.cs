using System.Drawing;

namespace CloudCam.Printing
{
    public interface IPrinterManager
    {
        public bool IsPrinting { get;  }
        PrinterSpecs Initialize();
        void Print(Bitmap image);
        int PrintsRemaining();
    }

    public record PrinterSpecs(int DpiX, int DpiY, Size paperSize);
}