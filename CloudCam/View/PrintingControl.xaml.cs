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

namespace CloudCam.View
{
    /// <summary>
    /// Interaction logic for PrintingControl.xaml
    /// </summary>
    public partial class PrintingControl
    {
        public PrintingControl()
        {
            InitializeComponent();

            this.WhenActivated((d) =>
            {
                this.OneWayBind(ViewModel, vm => vm.Message, view => view.MessageTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Image1, view => view.Image1.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Image2, view => view.Image2.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Image3, view => view.Image3.Source).DisposeWith(d);
            });
        }
    }
}
