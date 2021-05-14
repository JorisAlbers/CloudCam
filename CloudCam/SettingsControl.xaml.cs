using System.Reactive.Disposables;
using ReactiveUI;

namespace CloudCamDotNet4
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl 
    {
        public SettingsControl()
        {
            InitializeComponent();

            this.WhenActivated((d) =>
            {
                this.OneWayBind(ViewModel, vm => vm.CameraDevices, v => v.CameraDeviceComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedCameraDevice, v => v.CameraDeviceComboBox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FrameFolder, v => v.FramesPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MustacheFolder, v => v.MustachesPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OutputFolder, v => v.OutputPathTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Apply, v => v.ApplyButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Start, v => v.StartButton).DisposeWith(d);
            });
        }
    }
}

