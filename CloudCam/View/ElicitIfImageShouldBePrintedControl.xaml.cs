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
    /// Interaction logic for ElicitIfImageShouldBePrintedControl.xaml
    /// </summary>
    public partial class ElicitIfImageShouldBePrintedControl : ReactiveUserControl<ElicitIfImageShouldBePrintedViewModel>
    {
        public ElicitIfImageShouldBePrintedControl()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel, vm => vm.Message, view => view.MessageTextBlock.Text)
                    .DisposeWith(disposable);


            });
        }
    }
}
