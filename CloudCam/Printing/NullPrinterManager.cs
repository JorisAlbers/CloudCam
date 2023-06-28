using System.Drawing;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.Printing
{
    public class NullPrinterManager : ReactiveObject, IPrinterManager
    {
        [Reactive] public bool IsPrinting { get; set; }
        public int DpiX { get; } = 300;
        public int DpiY { get; } = 300;

        public PrinterSpecs Initialize()
        {
            return new PrinterSpecs(DpiX, DpiY, new Size(1920, 1080));
        }

        public void Print(Bitmap image)
        {
            IsPrinting = true;
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                IsPrinting = false;
            });
        }

        public int PrintsRemaining()
        {
            return 10;
        }
    }
}