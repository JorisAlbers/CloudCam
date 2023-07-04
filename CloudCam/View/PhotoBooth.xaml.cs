using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ReactiveUI;

namespace CloudCam.View
{
    /// <summary>
    /// Interaction logic for PhotoBooth.xaml
    /// </summary>
    /// 
    public partial class PhotoBooth
    {
        private Point initialTouchPoint;
        private const double SwipeThreshold = 100; // Adjust this value as needed
        private Boolean swipeInProgress = false;

        public PhotoBooth()
        {
            InitializeComponent();
            
            this.WhenActivated((d) =>
            {
                Cursor = Cursors.None;

                Random random = new Random();
                this.OneWayBind(ViewModel, vm => vm.ImageSource.ImageSource, v => v.VideoImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Frame.ImageSource, v => v.FrameImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TakenImage, v => v.TakenPhotoImage.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TakenImage, v => v.TakenPhotoImage.LayoutTransform,
                    (_) => new RotateTransform(random.Next(-20, 20), 0.5, 0.5)).DisposeWith(d);


                this.OneWayBind(ViewModel, vm => vm.MultipleTakenImage1, v => v.TakenImage1.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.MultipleTakenImage2, v => v.TakenImage2.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.MultipleTakenImage3, v => v.TakenImage3.Source).DisposeWith(d);


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

                this.OneWayBind(ViewModel, vm => vm.MultipleTakenImage1, v => v.VideoPlayer.Visibility,
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

                this.OneWayBind(ViewModel, vm => vm.PrintingViewModel, v => v.VideoPlayer.Visibility,
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
                this.OneWayBind(ViewModel, vm => vm.PhotoCountdownText, v => v.PhotoCountdownTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CameraFps, v => v.CameraFpsTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EditingFps, v => v.EditingFpsTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ToDisplayImageFps, v => v.ToDisplayImageTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ElicitIfImageShouldBePrintedViewModel, v => v.ElicitPrintImageViewModel.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PrintingViewModel, v => v.PrintViewModelViewHost.ViewModel).DisposeWith(d);
            });


            // Touch events
            this.TouchDown += PhotoBooth_TouchDown;
            this.TouchMove += PhotoBooth_TouchMove;
            this.TouchUp += PhotoBooth_TouchUp;
            /*// Mouse events
            this.MouseDown += PhotoBooth_MouseDown;
            this.MouseMove += PhotoBooth_MouseMove;
            this.MouseUp += PhotoBooth_MouseUp;*/
        }

        private void PhotoBooth_TouchMove(object sender, TouchEventArgs e)
        {
            if (ViewModel == null) { return; }
            if (swipeInProgress) { return; }
            
            Point currentTouchPoint = e.GetTouchPoint(this).Position;

            double horizontalDelta = currentTouchPoint.X - initialTouchPoint.X;
            double verticalDelta = currentTouchPoint.Y - initialTouchPoint.Y;

            if (Math.Abs(verticalDelta) >= SwipeThreshold && Math.Abs(verticalDelta) >= Math.Abs(horizontalDelta))
            {
                if (verticalDelta > 0)
                {
                    ViewModel.HandelSwipeInput(SwipeDirection.Down);
                }
                else
                {
                    ViewModel.HandelSwipeInput(SwipeDirection.Up);
                }
                this.swipeInProgress = true;
            }
            else if (Math.Abs(horizontalDelta) >= SwipeThreshold && Math.Abs(verticalDelta) <= Math.Abs(horizontalDelta))
            {
                if (horizontalDelta > 0)
                {
                    ViewModel.HandelSwipeInput(SwipeDirection.Right);
                }
                else
                {
                    ViewModel.HandelSwipeInput(SwipeDirection.Left);
                }
                this.swipeInProgress = true;
            }
        }

        private void PhotoBooth_TouchDown(object sender, TouchEventArgs e)
        {
            if (ViewModel == null) { return; }

            initialTouchPoint = e.GetTouchPoint(this).Position;
        }

        private void PhotoBooth_TouchUp(object sender, TouchEventArgs e)
        {
            this.swipeInProgress = false;
        }

        private void VideoPlayer_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Position = TimeSpan.Zero;
            VideoPlayer.Play();
        }
    }
}
