using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
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
            OutputFolder = settings.OutputFolder;
            HatFolder = settings.HatFolder;

            KeyBindingViewModels = new KeyBindingViewModel[]
            {
                new KeyBindingViewModel(UserAction.TakePicture, Key.Space)
            };

            Apply = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder, HatFolder,OutputFolder, SelectedCameraDevice.Name));
            Start = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder, HatFolder,OutputFolder, SelectedCameraDevice.Name));
        }
    }

    public class Settings
    {
        public string FrameFolder { get; }
        public string MustacheFolder { get; }
        public string OutputFolder { get; }
        public string CameraDevice { get; }
        public string HatFolder { get; set; }

        public Settings(string frameFolder, string mustacheFolder, string hatFolder, string outputFolder, string cameraDevice)
        {
            FrameFolder = frameFolder;
            MustacheFolder = mustacheFolder;
            HatFolder = hatFolder;
            OutputFolder = outputFolder;
            CameraDevice = cameraDevice;
        }
    }

}
