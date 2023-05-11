using System.Collections.Generic;
using System.Drawing;
using CloudCam.Printing;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.View
{
    public class PrinterSettingsViewModel
    {
        [Reactive] public string SelectedPrinter { get; set; }
        [Reactive] public Size SelectedPaperSize { get; set; }
        [Reactive] public string BackgroundImagePath { get; set; }
        public List<string> AvailablePrinters => PrinterManager.GetPrinterNames();

        public PrinterSettingsViewModel(PrinterSettings settings)
        {
            if (settings == null) return;

            SelectedPrinter = settings.SelectedPrinter;
            BackgroundImagePath = settings.BackgroundImagePath;
        }

        public PrinterSettings GetSettings()
        {
            return new PrinterSettings(SelectedPrinter, BackgroundImagePath );
        }
    }
}