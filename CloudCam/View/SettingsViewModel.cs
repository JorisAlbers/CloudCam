using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.View
{
    public class SettingsViewModel : ReactiveObject
    {
        public List<int> AvailableComPorts => new List<int>(Enumerable.Range(0, 50));
        public List<CameraDevice> CameraDevices { get; }
        [Reactive] public CameraDevice SelectedCameraDevice { get; set; }
        [Reactive] public string FrameFolder { get; set; }
        [Reactive] public string MustacheFolder { get; set; }
        [Reactive] public string HatFolder { get; set; }
        [Reactive] public string GlassesFolder { get; set; }
        [Reactive] public string OutputFolder { get; set; }
        [Reactive] public int ComPortLeds { get; set; }

        public KeyBindingViewModel[] KeyBindingViewModels { get; }

        public PrinterSettingsViewModel PrinterSettingsViewModel { get; }

        public ReactiveCommand<Action<string>, string> SelectFrameFolder { get; }
        public ReactiveCommand<Action<string>, string> SelectMustacheFolder { get; }
        public ReactiveCommand<Action<string>, string> SelectHatFolder { get; }
        public ReactiveCommand<Action<string>, string> SelectGlassesFolder { get; }
        public ReactiveCommand<Action<string>, string> SelectOutputFolder { get; }
        public ReactiveCommand<Unit, Settings> Apply { get; }
        public ReactiveCommand<Unit, Settings> Start { get; }

        public SettingsViewModel(Settings settings, List<CameraDevice> cameraDevices)
        {
            CameraDevices = cameraDevices;
            SelectedCameraDevice = cameraDevices.FirstOrDefault(x => x.Name == settings.CameraDevice);
            FrameFolder = settings.FrameFolder;
            MustacheFolder = settings.MustacheFolder;
            HatFolder = settings.HatFolder;
            GlassesFolder = settings.GlassesFolder;
            OutputFolder = settings.OutputFolder;
            ComPortLeds = settings.ComPortLeds;
            KeyBindingViewModels = settings.KeyBindings.Select(x => new KeyBindingViewModel(x.Action, x.Key)).ToArray();
            PrinterSettingsViewModel = new PrinterSettingsViewModel(settings.PrinterSettings);

            SelectFrameFolder = ReactiveCommand.Create<Action<string>, string>((propertyName) => FrameFolder = ShowFolderDialog(FrameFolder));
            SelectMustacheFolder = ReactiveCommand.Create<Action<string>, string>((propertyName) => MustacheFolder = ShowFolderDialog(MustacheFolder));
            SelectHatFolder = ReactiveCommand.Create<Action<string>, string>((propertyName) => HatFolder = ShowFolderDialog(HatFolder));
            SelectGlassesFolder = ReactiveCommand.Create<Action<string>, string>((propertyName) => GlassesFolder = ShowFolderDialog(GlassesFolder));
            SelectOutputFolder = ReactiveCommand.Create<Action<string>, string>((propertyName) => OutputFolder = ShowFolderDialog(OutputFolder));

            Apply = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder, HatFolder, GlassesFolder, OutputFolder, SelectedCameraDevice.Name, 
                KeyBindingViewModels.Select(x=> new KeyBindingSetting(x.Action, x.SelectedKey)).ToArray(), ComPortLeds, PrinterSettingsViewModel.GetSettings()));
            Start = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder, HatFolder, GlassesFolder, OutputFolder, SelectedCameraDevice.Name,
                KeyBindingViewModels.Select(x => new KeyBindingSetting(x.Action, x.SelectedKey)).ToArray(), ComPortLeds, PrinterSettingsViewModel.GetSettings()));
        }


        private string ShowFolderDialog(string openOnFolder)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = openOnFolder;
            dialog.ShowDialog();
            return dialog.SelectedPath;
        }

    }

    public class Settings
    {
        public string FrameFolder { get; }
        public string MustacheFolder { get; }
        public string OutputFolder { get; }
        public string CameraDevice { get; }
        public string HatFolder { get; set; }
        public string GlassesFolder { get; set; }

        public int ComPortLeds { get; set; } = -1;

        public KeyBindingSetting[] KeyBindings { get; set; }

        public PrinterSettings PrinterSettings { get; set; }

        public Settings(string frameFolder, string mustacheFolder, string hatFolder,string glassesFolder, string outputFolder, string cameraDevice, KeyBindingSetting[] keyBindings, int comPortLeds, PrinterSettings printerSettings)
        {
            FrameFolder = frameFolder;
            MustacheFolder = mustacheFolder;
            HatFolder = hatFolder;
            GlassesFolder = glassesFolder;
            OutputFolder = outputFolder;
            CameraDevice = cameraDevice;
            KeyBindings = keyBindings;
            ComPortLeds = comPortLeds;
            PrinterSettings = printerSettings;
        }
    }

    public class KeyBindingSetting
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public UserAction Action { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Key Key { get; set; }

        public KeyBindingSetting(UserAction action, Key key)
        {
            Action = action;
            Key = key;
        }
    }

    public class PrinterSettings
    {
        public string SelectedPrinter { get; set; }

        public string BackgroundImagePath { get; set; }

        public PrinterSettings(string selectedPrinter, string backgroundImagePath)
        {
            SelectedPrinter = selectedPrinter;
            BackgroundImagePath = backgroundImagePath;
        }

    }
}
