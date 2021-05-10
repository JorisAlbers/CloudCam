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
    public record Settings(DirectoryInfo FrameFolder, DirectoryInfo MustacheFolder, DirectoryInfo OutputFolder);

    public class SettingsViewModel : ReactiveObject
    {
        private readonly SettingsSerializer _settingsSerializer;
        private Settings _settings;
        
        [Reactive] public string FrameFolder { get; set; }
        [Reactive] public string MustacheFolder { get; set; }
        [Reactive] public string OutputFolder { get; set; }

        public ReactiveCommand<Unit,Settings> Save { get; }

        public SettingsViewModel(SettingsSerializer settingsSerializer)
        {
            _settingsSerializer = settingsSerializer;
            _settings = _settingsSerializer.Load();
            FrameFolder = _settings.FrameFolder.FullName;
            MustacheFolder = _settings.MustacheFolder.FullName;
            OutputFolder = _settings.OutputFolder.FullName;

            Save = ReactiveCommand.Create<Unit, Settings>((_) =>
            {
                _settings = new Settings(new DirectoryInfo(FrameFolder), new DirectoryInfo(MustacheFolder),
                    new DirectoryInfo(OutputFolder));
                return _settings;
            });
        }
    }
}
