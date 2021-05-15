using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ReactiveUI;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

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
                FocusManager.SetFocusedElement(this, this);
                Keyboard.Focus(this);
                this.Bind(ViewModel, vm => vm.ImageSource, v => v.VideoImage.Source).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Frame, v => v.FrameImage.Source).DisposeWith(d);

                this.Events().KeyDown
                    .Where(x => x.Key == Key.Right || x.Key == Key.Left)
                    .Select(x => x.Key == Key.Right)
                    .InvokeCommand(this, x => x.ViewModel.NextFrame);

            });
        }
    }
}
