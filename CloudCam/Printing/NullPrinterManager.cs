using System.Drawing;

namespace CloudCam.Printing
{
    public class NullPrinterManager : IPrinterManager
    {
        public void Initialize()
        {
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