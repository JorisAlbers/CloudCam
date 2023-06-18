using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.View
{
    public class ElicitIfImageShouldBePrintedViewModel : ReactiveObject, IDisposable
    {
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly string _cancelMessage;
        private readonly string _okMessage;
        [Reactive] public string Message { get; private set; }
        [Reactive] public int SecondsBeforeTimeout { get; private set; }


        public ElicitIfImageShouldBePrintedViewModel(string requestMessage, string cancelMessage, string okMessage, int secondsBeforeTimeout)
        {
            _cancelMessage = cancelMessage;
            _okMessage = okMessage;
            Message = requestMessage;
            SecondsBeforeTimeout = secondsBeforeTimeout;
        }

        public async Task<bool> Start()
        {
            while (SecondsBeforeTimeout-- > 0 && !_cancelTokenSource.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }

            if (_cancelTokenSource.IsCancellationRequested)
            {
                Message = _okMessage;
                return true;
            }

            Message = _cancelMessage;
            return false;
        }

        public void Dispose()
        {
            _cancelTokenSource.Cancel();
        }
    }
}