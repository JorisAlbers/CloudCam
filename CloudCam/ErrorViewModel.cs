using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class ErrorViewModel : ReactiveObject
    {
        [Reactive] public bool HasError { get; private set; }

        [Reactive] public string ErrorMessage { get; private set; }

        public void SetError(string error)
        {
            HasError = true;
            ErrorMessage = error;
        }
    }
}