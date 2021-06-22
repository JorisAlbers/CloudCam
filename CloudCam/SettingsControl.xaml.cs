using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ReactiveUI;

namespace CloudCam
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
                this.WhenAnyValue(x => x.ViewModel).Subscribe(x => DataContext = x);
                this.OneWayBind(ViewModel, vm => vm.CameraDevices, v => v.CameraDeviceComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedCameraDevice, v => v.CameraDeviceComboBox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FrameFolder, v => v.FramesPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MustacheFolder, v => v.MustachesPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.HatFolder, v => v.HatsPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OutputFolder, v => v.OutputPathTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Apply, v => v.ApplyButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Start, v => v.StartButton).DisposeWith(d);
            });
        }

        private void InputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            KeyBindingViewModel keyBindingViewModel = (KeyBindingViewModel) tb.DataContext;
            keyBindingViewModel.SetKeyInput.Execute(e.Key).Subscribe();
            tb.Text = string.Empty;
            e.Handled = true;
        }
    }
}

