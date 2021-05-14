using System.Windows.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private readonly Settings _settings;

        [Reactive] public ImageSource VideoFrame { get; set; }

        public PhotoBoothViewModel(Settings settings)
        {
            _settings = settings;
        }

        public void Start()
        {

        }
    }
}