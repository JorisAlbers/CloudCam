using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam.View.Gallery
{
    public class GalleryViewModel : ReactiveObject
    {
        private readonly OutputImageRepository _outputImageRepository;
        private readonly int _period;
        private readonly Random _random;
        private CancellationTokenSource _cancellationToken;

        [Reactive] public Image CurrentImage { get; private set; }

        public GalleryViewModel(OutputImageRepository outputImageRepository, int periodInSeconds, Random random)
        {
            _outputImageRepository = outputImageRepository;
            _period = periodInSeconds;
            _random = random;
        }

        /// <summary>
        /// Start showing images.
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            var images = _outputImageRepository.GetNames();
            if (_cancellationToken != null && !_cancellationToken.IsCancellationRequested)
            {
                _cancellationToken.Cancel();
            }

            var cancellationToken = new CancellationTokenSource();
            _cancellationToken = cancellationToken;
            while (!cancellationToken.IsCancellationRequested)
            {
                int nextImage = _random.Next(0, images.Length - 1);
                var image = _outputImageRepository.Load(images[nextImage]);
                await Task.Delay(_period * 1000, cancellationToken.Token);

                CurrentImage = image;
            }
        }

        public void Stop()
        {
            _cancellationToken?.Cancel();
        }


    }
}