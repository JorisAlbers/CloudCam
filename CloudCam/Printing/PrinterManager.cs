using System.Drawing;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Printing;

/*namespace printerTestCsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Adjusting settings for the Sinfonia does not work via c#. 
            // Make sure to manually adjust the default settings via the printer dialog. 




            Console.Out.WriteLine("Running print test!");

            // Get the list of installed printers
            var printerNames = PrinterSettings.InstalledPrinters;
            var printerNamesList = new List<string>();

            // Print the list of printer names to the console
            foreach (string printerName in printerNames)
            {
                Console.WriteLine($"Found printer: {printerName}");
                printerNamesList.Add(printerName);
            }



            // Create a PrintDocument object
            PrintDocument document = new PrintDocument();

            // Set the printer name (replace with the name of your printer)
            document.PrinterSettings.PrinterName = printerNamesList.First(x => x.Contains("Sinfonia"));
            var settings = document.PrinterSettings;


            var sizes = document.PrinterSettings.PaperSizes;
            var sizesList = new List<PaperSize>();

            foreach (PaperSize paperSize in sizes)
            {
                sizesList.Add(paperSize);
                Console.Out.WriteLine($"Size accepted by printer:  (w.h) {paperSize.Width} x {paperSize.Height}");
            }

            // 615 x 211

            // 713 x 516

            // Set the page orientation to landscape or portrait
            settings.DefaultPageSettings.Landscape = false; // or false for portrait
            settings.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            settings.DefaultPageSettings.PaperSize = sizesList.First(x => x.Width == 211 && x.Height == 615);

            int dpiX = document.PrinterSettings.DefaultPageSettings.PrinterResolution.X;
            int dpiY = document.PrinterSettings.DefaultPageSettings.PrinterResolution.Y;
            *//*int widthInPixels = (int)(settings.DefaultPageSettings.PaperSize.Width / 100.0 * dpiX);
            int heightInPixels = (int)(settings.DefaultPageSettings.PaperSize.Height / 100.0 * dpiY);*//*

            float widthInInches = settings.DefaultPageSettings.PaperSize.Width / 100.0f;
            float heightInInches = settings.DefaultPageSettings.PaperSize.Height / 100.0f;
            int widthInPixels = (int)(widthInInches * dpiX);
            int heightInPixels = (int)(heightInInches * dpiY);

            // Load the bitmap image

            Image background = Image.FromFile("wedfest_background.png");

            Point picture1Start = new Point(18, 68);
            Point picture1End = new Point(197, 175);
            Size picture1Size = new Size(picture1End.X - picture1Start.X, picture1End.Y - picture1Start.Y);
            Image picture1 = CreatePicture("test.png", picture1Size, dpiX, dpiY);

            picture1.Save("picture1.png", ImageFormat.Png);

            var output = OverlayPicture(background, picture1, picture1Start);
            output.Save("output.png", ImageFormat.Png);


            //*Console.Out.WriteLine($"Margin: left: {settings.DefaultPageSettings.Margins.Left} top: {settings.DefaultPageSettings.Margins.Top}");
            Console.Out.WriteLine($"printable area : left: {settings.DefaultPageSettings.PrintableArea.Left} top: {settings.DefaultPageSettings.PrintableArea.Top}");
            Console.Out.WriteLine($"paper size: width: {settings.DefaultPageSettings.PaperSize.Width} height: {settings.DefaultPageSettings.PaperSize.Height}");
            Console.Out.WriteLine("--");
            Console.Out.WriteLine($"image pixels: width: {widthInPixels} height: {heightInPixels}");
            Console.Out.WriteLine($"DPI: x: {dpiX} y: {dpiY}");
            Console.Out.WriteLine($"Settings is valid: {settings.IsValid}");
            Console.Out.WriteLine("");*//*



            // Draw the original image onto the resized bitmap using the graphics object

            // Set the print page event handler
            document.PrintPage += (sender, e) =>
            {
                // Draw the image on the print page
                Console.Out.WriteLine($"While printing, this is the page size: (w.h) {e.PageSettings.PaperSize.Width} x {e.PageSettings.PaperSize.Height}");
                e.Graphics.DrawImage(output, e.PageBounds);

                Console.ReadLine();
                return;
                // Set the page as printed
                e.HasMorePages = false;
            };

            Console.Out.WriteLine("Press enter to print");
            Console.ReadLine();

            // Print the document
            document.Print();


            Console.Out.WriteLine("Done!");
            Console.ReadLine();
        }

        private static Image OverlayPicture(Image background, Image picture1, Point picture1Start)
        {
            Image output = new Bitmap(background.Width, background.Height);
            using Graphics graphics = Graphics.FromImage(output);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(background, 0, 0);
            graphics.DrawImage(picture1, picture1Start.X, picture1Start.Y);
            return output;
        }


        private static Bitmap CreatePicture(string fileName, Size resizeTo, float dpiX, float dpiY)
        {
            using Image bitmapImage = Image.FromFile(fileName);

            var resized = new Bitmap(resizeTo.Width, resizeTo.Height);
            //resized.SetResolution(dpiX, dpiY);

            using Graphics graphics = Graphics.FromImage(resized);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(bitmapImage, 0, 0, resizeTo.Width, resizeTo.Height);

            return resized;
        }

    }
}*/


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
            PrintDocument document = new PrintDocument();
            document.PrinterSettings.PrinterName = _printerName;

            var sizes = document.PrinterSettings.PaperSizes;
            var sizesList = new List<PaperSize>();
            foreach (PaperSize paperSize in sizes)
            {
                sizesList.Add(paperSize);
                Console.Out.WriteLine($"Size accepted by printer:  (w.h) {paperSize.Width} x {paperSize.Height}");
            }

            // Set the page orientation to landscape or portrait
            document.PrinterSettings.DefaultPageSettings.Landscape = false; // or false for portrait
            document.PrinterSettings.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            document.PrinterSettings.DefaultPageSettings.PaperSize = sizesList.First(x => x.Width == 211 && x.Height == 615);

            /*int dpiX = document.PrinterSettings.DefaultPageSettings.PrinterResolution.X;
            int dpiY = document.PrinterSettings.DefaultPageSettings.PrinterResolution.Y;*/

            float widthInInches = document.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100.0f;
            float heightInInches = document.PrinterSettings.DefaultPageSettings.PaperSize.Height / 100.0f;
            /*int widthInPixels = (int)(widthInInches * dpiX);
            int heightInPixels = (int)(heightInInches * dpiY);*/

            document.PrintPage += (sender, e) =>
            {
                // Draw the image on the print page
                Console.Out.WriteLine($"While printing, this is the page size: (w.h) {e.PageSettings.PaperSize.Width} x {e.PageSettings.PaperSize.Height}");
                e.Graphics.DrawImage(image, e.PageBounds);

                Console.ReadLine();
                return;
            };

            Console.Out.WriteLine("Press enter to print");
            Console.ReadLine();

            // Print the document
            document.Print();

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
    }
}
