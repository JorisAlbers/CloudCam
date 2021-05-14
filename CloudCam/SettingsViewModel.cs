using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public record Settings(string FrameFolder, string MustacheFolder, string OutputFolder, string CameraDevice);

    public class SettingsViewModel : ReactiveObject
    {
        public List<CameraDevice> CameraDevices { get; }
        [Reactive] public CameraDevice SelectedCameraDevice { get; set; }
        [Reactive] public string FrameFolder { get; set; }
        [Reactive] public string MustacheFolder { get; set; }
        [Reactive] public string OutputFolder { get; set; }

        public ReactiveCommand<Unit,Settings> Apply { get; }

        public SettingsViewModel(Settings settings, List<CameraDevice> cameraDevices)
        {
            CameraDevices = cameraDevices;
            SelectedCameraDevice = cameraDevices.FirstOrDefault(x => x.Name == settings.CameraDevice);
            FrameFolder = settings.FrameFolder;
            MustacheFolder = settings.MustacheFolder;
            OutputFolder = settings.OutputFolder;
            
            Apply = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder,OutputFolder, SelectedCameraDevice.Name));
        }
    }
}
