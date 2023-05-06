using System.Drawing;

namespace CloudCam.Printing
{
    public class PrinterManager
    {
        private readonly string _printerName;

        public PrinterManager(string printerName)
        {
            _printerName = printerName;
            // set properties
        }
        
        public void Initialize()
        {
            // load stuff that might take a while
        }

        public void Print(Bitmap image)
        {
            // print an image
        }

        public int PrintsRemaining()
        {
            // return number of prints remaining
            return -1;
        }

    }
}
