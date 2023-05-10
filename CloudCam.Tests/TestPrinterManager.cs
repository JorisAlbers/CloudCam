using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCam.Printing;
using NUnit.Framework;

namespace CloudCam.Tests
{
    public class TestPrinterManager
    {
        [Test]
        public void ATest()
        {
            string printerName = "myPrinter";
            string imagePath = @"C:\testImage.png";
            var bitmap = new Bitmap(Image.FromFile(imagePath));

            var printerManager = new PrinterManager(printerName);
            printerManager.Initialize();
            printerManager.Print(bitmap);
        }


        [Test]

        public void BTest()
        {
            /*string printerName = "myPrinter";
            string imagePath = @"C:\testImage.png";
            var bitmap = new Bitmap(Image.FromFile(imagePath));

            var printerManager = new PrinterManager(printerName);
            printerManager.Initialize();
            printerManager.Print(bitmap);*/
        }
    }
}
