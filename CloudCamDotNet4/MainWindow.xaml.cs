using System.Reactive.Disposables;
using ReactiveUI;

namespace CloudCamDotNet4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ViewModel = new MainWindowViewModel();


            this.WhenActivated((dispose) =>
            {
                this.Bind(ViewModel, vm => vm.SelectedViewModel, v => v.ViewModelHost.ViewModel)
                    .DisposeWith(dispose);
            });
        }
    }
}
