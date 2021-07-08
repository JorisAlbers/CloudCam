using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class SettingsViewModel : ReactiveObject
    {
        public List<CameraDevice> CameraDevices { get; }
        [Reactive] public CameraDevice SelectedCameraDevice { get; set; }
        [Reactive] public string FrameFolder { get; set; }
        [Reactive] public string MustacheFolder { get; set; }
        [Reactive] public string HatFolder { get; set; }
        [Reactive] public string GlassesFolder { get; set; }
        [Reactive] public string OutputFolder { get; set; }

        public KeyBindingViewModel[] KeyBindingViewModels { get; }

        public ReactiveCommand<Unit, Settings> Apply { get; }
        public ReactiveCommand<Unit, Settings> Start { get; }

        public SettingsViewModel(Settings settings, List<CameraDevice> cameraDevices)
        {
            CameraDevices = cameraDevices;
            SelectedCameraDevice = cameraDevices.FirstOrDefault(x => x.Name == settings.CameraDevice);
            FrameFolder = settings.FrameFolder;
            MustacheFolder = settings.MustacheFolder;
            GlassesFolder = settings.GlassesFolder;
            OutputFolder = settings.OutputFolder;
            HatFolder = settings.HatFolder;
            KeyBindingViewModels = settings.KeyBindings.Select(x => new KeyBindingViewModel(x.Action, x.Key)).ToArray();


            Apply = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder, HatFolder, GlassesFolder, OutputFolder, SelectedCameraDevice.Name, 
                KeyBindingViewModels.Select(x=> new KeyBindingSetting(x.Action, x.SelectedKey)).ToArray()));
            Start = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder, HatFolder, GlassesFolder, OutputFolder, SelectedCameraDevice.Name,
                KeyBindingViewModels.Select(x => new KeyBindingSetting(x.Action, x.SelectedKey)).ToArray()));
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

        public KeyBindingSetting[] KeyBindings { get; set; }

        public Settings(string frameFolder, string mustacheFolder, string hatFolder,string glassesFolder, string outputFolder, string cameraDevice, KeyBindingSetting[] keyBindings)
        {
            FrameFolder = frameFolder;
            MustacheFolder = mustacheFolder;
            HatFolder = hatFolder;
            GlassesFolder = glassesFolder;
            OutputFolder = outputFolder;
            CameraDevice = cameraDevice;
            KeyBindings = keyBindings;
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
}
