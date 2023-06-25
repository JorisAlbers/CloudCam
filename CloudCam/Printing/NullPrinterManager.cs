using System.Drawing;

namespace CloudCam.Printing
{
    public class NullPrinterManager : IPrinterManager
    {
        public bool IsPrinting { get; } = false;

        public PrinterSpecs Initialize()
        {
            return new PrinterSpecs(0, 0, new Size(0, 0));
        }

        public void Print(Bitmap image)
        {
        }

        public int PrintsRemaining()
        {
            return 10;
        }
    }
}