using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.View
{
    public class ElicitIfImageShouldBePrintedViewModel : ReactiveObject
    {
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private bool _shouldPrint;

        private readonly string _cancelMessage;
        private readonly string _okMessage;
        [Reactive] public string Message { get; private set; }
        [Reactive] public int TenthOfSecondsBeforeTimeout { get; private set; }

        public ElicitIfImageShouldBePrintedViewModel(string requestMessage, string cancelMessage, string okMessage, int secondsBeforeTimeout)
        {
            _cancelMessage = cancelMessage;
            _okMessage = okMessage;
            Message = requestMessage;
            TenthOfSecondsBeforeTimeout = secondsBeforeTimeout * 10;
        }

        public async Task<bool> Start()
        {
            while (TenthOfSecondsBeforeTimeout-- > 0 && !_cancelTokenSource.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            if (_shouldPrint)
            {
                Message = _okMessage;
                return true;
            }

            Message = _cancelMessage;
            return false;
        }

        public void Accept()
        {
            _shouldPrint = true;
            _cancelTokenSource.Cancel();
        }

        public void Cancel()
        {
            _cancelTokenSource.Cancel();
        }
    }
}