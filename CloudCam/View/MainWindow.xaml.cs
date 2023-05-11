using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using ReactiveUI;

namespace CloudCam.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CompositeDisposable _keybindingsDisposable;

        public MainWindow()
        {
            InitializeComponent();
            this.ViewModel = new MainWindowViewModel();

            this.WhenActivated((dispose) =>
            {
                this.Bind(ViewModel, vm => vm.SelectedViewModel, v => v.ViewModelHost.ViewModel)
                    .DisposeWith(dispose);

                this.WhenAnyValue(x => x.ViewModel.KeyToUserActionDic).Subscribe(x =>
                {
                    _keybindingsDisposable?.Dispose();
                    if (x == null)
                    {
                        return;
                    }

                    CompositeDisposable disposable = new CompositeDisposable();
                    foreach (var key in x.Keys)
                    {
                        AddKeyBinding(key, disposable);
                    }

                    _keybindingsDisposable = disposable;
                });
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
