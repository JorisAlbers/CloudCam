using System.Windows.Input;
using ReactiveUI;

namespace CloudCam
{
    public class KeyBindingViewModel : ReactiveObject
    {
        public UserAction Action { get; }

        public string SelectedKey { get; set; }

        public KeyBindingViewModel(UserAction action, Key key)
        {
            Action = action;
            SelectedKey = key.ToString();
        }
    }
}