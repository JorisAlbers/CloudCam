using System.Reactive.Disposables;
using System.Windows.Input;
using ReactiveUI;

namespace CloudCam
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

                if (ViewModel != null)
                {
                    AddKeyBinding(Key.Right, dispose);
                    AddKeyBinding(Key.Left, dispose);
                    AddKeyBinding(Key.Space, dispose);
                    AddKeyBinding(Key.A, dispose);
                    AddKeyBinding(Key.D, dispose);
                }
            });
        }

        private void AddKeyBinding(Key key, CompositeDisposable d)
        {
            KeyBinding binding = new KeyBinding
            {
                Command = ViewModel.KeyPressed,
                Key = key,
                CommandParameter = key
            };
            InputBindings.Add(binding);

            d.Add(Disposable.Create(() =>
            {
                InputBindings.Remove(binding);
            }));
        }
    }
}
