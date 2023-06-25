using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace CloudCam.Printing
{
    public class PrinterManager : ReactiveObject, IPrinterManager
    {
        private readonly string _printerName;
        private PrintDocument _document;
        private Bitmap _bitmapToPrint;
        private Rectangle _printArea;

        [Reactive] public bool IsPrinting { get; set; }

        public PrinterManager(string printerName)
        {
            _printerName = printerName;
            // set properties
        }
        
        public PrinterSpecs Initialize()
        {
            Log.Logger.Information($"Initializing printer {_printerName}");

            PrintDocument document = new PrintDocument();
            document.PrinterSettings.PrinterName = _printerName;

            var sizes = document.PrinterSettings.PaperSizes;
            var sizesList = new List<PaperSize>();
            foreach (PaperSize paperSize in sizes)
            {
                sizesList.Add(paperSize);
            }


            // Set the page orientation to landscape or portrait
            // TODO inject these setting and allow user to configur
            document.PrinterSettings.DefaultPageSettings.Landscape = false;
            document.PrinterSettings.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            document.PrinterSettings.DefaultPageSettings.PaperSize = sizesList.First(x => x.Width == 211 && x.Height == 615);
            _printArea = new Rectangle(0, 8, 200, 600);

            _document = document;


            int dpiX = document.PrinterSettings.DefaultPageSettings.PrinterResolution.X;
            int dpiY = document.PrinterSettings.DefaultPageSettings.PrinterResolution.Y;

            _document.BeginPrint += DocumentOnBeginPrint;
            _document.EndPrint   += DocumentOnEndPrint;
            _document.PrintPage  += DocumentOnPrintPage;
            return new PrinterSpecs(dpiX, dpiY, new Size(211, 615));

        }

        private void DocumentOnPrintPage(object sender, PrintPageEventArgs e)
        {
            var bitmap = _bitmapToPrint;
            if (bitmap == null)
            {
                return;
            }
            
            e.Graphics.DrawImage(bitmap, _printArea);
        }

        private void DocumentOnEndPrint(object sender, PrintEventArgs e)
        {
            IsPrinting = false;
        }

        private void DocumentOnBeginPrint(object sender, PrintEventArgs e)
        {
            IsPrinting = true;
        }

        public void Print(Bitmap image)
        {
            Log.Logger.Information("Printing image");

            if (IsPrinting)
            {
                Log.Logger.Warning("Cannot print, already printing!");
                return;
            }

            if (PrintsRemaining() < 0)
            {
                Log.Logger.Warning("Cannot print, no print left!");
                return;
            }

            _bitmapToPrint = image;
            _document.Print();
        }

        public int PrintsRemaining()
        {
            PrintServer printServer = new PrintServer();
            PrintQueue printQueue = printServer.GetPrintQueue(_printerName);
            PrintQueueStatus printerStatus = printQueue.QueueStatus;
            if ((printerStatus & PrintQueueStatus.PaperOut) == PrintQueueStatus.PaperOut)
            {
                return 0;
            }
            else
            {
                // get the raw data out of the status string
                string printerStatusString = printerStatus.ToString();
                Console.WriteLine(printerStatusString);
                int startIndex = printerStatusString.IndexOf("of") + 3;
                int length = printerStatusString.IndexOf(" ", startIndex) - startIndex;
                string remaining = printerStatusString.Substring(startIndex, length);
                int remainingInt = int.Parse(remaining);
                return remainingInt;
            }
        }

        public static List<Size> GetAvailablePaperSizes(string printerName)
        {
            PrintDocument document = new PrintDocument();
            document.PrinterSettings.PrinterName = printerName;

            var sizes = document.PrinterSettings.PaperSizes;
            var sizesList = new List<Size>();
            foreach (Size paperSize in sizes)
            {
                sizesList.Add(paperSize);
                Console.Out.WriteLine($"Size accepted by printer:  (w.h) {paperSize.Width} x {paperSize.Height}");
            }
            return sizesList;
        }

        public static List<string> GetPrinterNames()
        {

            var printerNames = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            var printerNamesList = new List<string>();

            foreach (string printerName in printerNames)
            {
                printerNamesList.Add(printerName);
            }
            return printerNamesList;
        }
    }
}
