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
    /// Interaction logic for ErrorControl.xaml
    /// </summary>
    public partial class ErrorControl : ReactiveUserControl<ErrorViewModel>
    {
        public ErrorControl()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.Bind(ViewModel, vm => vm.ErrorMessage, view => view.ErrorTextBlock.Text)
                    .DisposeWith(disposable);
            });
        }
    }
}
