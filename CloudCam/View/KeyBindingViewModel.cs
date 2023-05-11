using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.View
{
    public class KeyBindingViewModel : ReactiveObject
    {
        public UserAction Action { get; }

        [Reactive] public Key SelectedKey { get; private set; }
        
        public ReactiveCommand<Key,Unit> SetKeyInput { get; }

        [ObservableAsProperty]
        public string SelectedKeyAsString { get; }

        public KeyBindingViewModel(UserAction action, Key key)
        {
            Action = action;
            SelectedKey = key;
            this.WhenAnyValue(x => x.SelectedKey).Select(x => x.ToString()).ToPropertyEx(this, x=>x.SelectedKeyAsString);
            SetKeyInput = ReactiveCommand.Create<Key, Unit>(k =>
            {
                SelectedKey = k;
                return Unit.Default;
            });
        }
    }
}