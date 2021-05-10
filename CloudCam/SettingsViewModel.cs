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
    public record Settings(string FrameFolder, string MustacheFolder, string OutputFolder);

    public class SettingsViewModel : ReactiveObject
    {
        [Reactive] public string FrameFolder { get; set; }
        [Reactive] public string MustacheFolder { get; set; }
        [Reactive] public string OutputFolder { get; set; }

        public ReactiveCommand<Unit,Settings> Save { get; }

        public SettingsViewModel(Settings settings)
        {
            
            FrameFolder = settings.FrameFolder;
            MustacheFolder = settings.MustacheFolder;
            OutputFolder = settings.OutputFolder;

            Save = ReactiveCommand.Create<Unit, Settings>((_) => new Settings(FrameFolder, MustacheFolder,OutputFolder));
        }
    }
}
