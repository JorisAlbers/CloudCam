using System.Reactive.Disposables;
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
                this.Bind(ViewModel, vm => vm.ImageSource, v => v.VideoImage.Source).DisposeWith(d);
            });
        }
    }
}
