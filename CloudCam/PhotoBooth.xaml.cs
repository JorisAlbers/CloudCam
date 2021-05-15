using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ReactiveUI;

namespace CloudCam
{
    /// <summary>
    /// Interaction logic for PhotoBooth.xaml
    /// </summary>
    public partial class PhotoBooth
    {
        public PhotoBooth()
        {
            InitializeComponent();
            
            this.WhenActivated((d) =>
            {
                this.OneWayBind(ViewModel, vm => vm.ImageSource, v => v.VideoImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Frame, v => v.FrameImage.Source).DisposeWith(d);
            });
        }
    }
}
