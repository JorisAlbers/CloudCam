using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                this.OneWayBind(ViewModel, vm => vm.CameraDevices, v => v.CameraDeviceComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedCameraDevice, v => v.CameraDeviceComboBox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FrameFolder, v => v.FramesPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MustacheFolder, v => v.MustachesPathTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OutputFolder, v => v.OutputPathTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);
            });
        }
    }
}

