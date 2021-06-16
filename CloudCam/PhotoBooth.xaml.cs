using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReactiveUI;
using Color = System.Drawing.Color;

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
                Random random = new Random();
                this.OneWayBind(ViewModel, vm => vm.ImageSource.ImageSource, v => v.VideoImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Frame.ImageSource, v => v.FrameImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TakenImage, v => v.TakenPhotoImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TakenImage, v => v.TakenPhotoImage.LayoutTransform,
                    (_) => new RotateTransform(random.Next(-20, 20), 0.5, 0.5)).DisposeWith(d);

                
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

                this.OneWayBind(ViewModel, vm => vm.TakenImage, v => v.VideoPlayer.Visibility,
                    (picture) =>
                    {
                        if (picture != null)
                        {
                           VideoPlayer.Play();
                            return Visibility.Visible;

                        }
                        VideoPlayer.Pause();
                        return Visibility.Hidden;
                        
                    }).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.DebugModeActive, v => v.DebugPanel.Visibility,
                    (b) => b ? Visibility.Visible : Visibility.Hidden).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.PickupLine, v => v.PickupLineTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CameraFps, v => v.CameraFpsTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EditingFps, v => v.EditingFpsTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ToDisplayImageFps, v => v.ToDisplayImageTextBlock.Text).DisposeWith(d);
            });
        }

        private void VideoPlayer_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Position = TimeSpan.Zero;
            VideoPlayer.Play();
        }
    }
}
