using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
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
                this.OneWayBind(ViewModel, vm => vm.ImageSource.ImageSource, v => v.VideoImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Frame.ImageSource, v => v.FrameImage.Source).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.SecondsUntilPictureIsTaken, v => v.CountdownTextBlock.Text,
                    (seconds) =>
                    {
                        if (seconds > 0)
                        {
                            return seconds.ToString();
                        }

                        return string.Empty;
                    }).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.SecondsUntilPictureIsTaken, v => v.FlashRectangle.Visibility,
                    (seconds) => seconds == 0 ? Visibility.Visible : Visibility.Hidden).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.CameraFps, v => v.CameraFpsTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EditingFps, v => v.EditingFpsTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ToDisplayImageFps, v => v.ToDisplayImageTextBlock.Text).DisposeWith(d);
            });
        }
    }
}
