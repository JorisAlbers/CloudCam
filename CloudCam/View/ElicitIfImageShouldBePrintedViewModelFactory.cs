using System;
using System.Collections.Generic;

namespace CloudCam.View
{
    public class ElicitIfImageShouldBePrintedViewModelFactory
    {
        private const int _TIMEOUT_SECONDS = 5;
        private readonly Random _random;
        private readonly List<string> _requestMessages;
        private readonly List<string> _cancelMessages;
        private readonly List<string> _okMessages;

        public ElicitIfImageShouldBePrintedViewModelFactory(Random random, List<string> requestMessages, List<string> cancelMessages, List<string> okMessages)
        {
            _random = random;
            _requestMessages = requestMessages;
            _cancelMessages = cancelMessages;
            _okMessages = okMessages;
        }

        public ElicitIfImageShouldBePrintedViewModel Create()
        {
            var requestMessage = _requestMessages[_random.Next(0, _requestMessages.Count)];
            var cancelMessage = _cancelMessages[_random.Next(0, _cancelMessages.Count)];
            var okMessage = _okMessages[_random.Next(0, _okMessages.Count)];
            return new ElicitIfImageShouldBePrintedViewModel(requestMessage, cancelMessage, okMessage, _TIMEOUT_SECONDS);
        }
    }
}