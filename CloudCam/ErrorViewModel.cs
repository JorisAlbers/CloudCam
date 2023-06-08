using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class ErrorViewModel : ReactiveObject
    {
        public string ErrorMessage { get; }

        public ErrorViewModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

}