using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace CloudCam.View.Gallery
{
    public class GalleryViewModel : ReactiveObject
    {
        private readonly OutputImageRepository _outputImageRepository;
        private readonly int _period;
        private readonly Random _random;
        private CancellationTokenSource _cancellationToken;

        [Reactive] public ImageSource CurrentImage { get; private set; }

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

            if (images == null|| images.Length == 0)
            {
                return;
            }


            var cancellationToken = new CancellationTokenSource();
            _cancellationToken = cancellationToken;
            while (!cancellationToken.IsCancellationRequested)
            {
                Mat image = null;
                try
                {
                    int nextImage = _random.Next(0, images.Length - 1);
                    image = _outputImageRepository.Load(images[nextImage]);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Failed to load image to show in the gallery",ex);
                }
               
                await Task.Delay(_period * 1000, cancellationToken.Token);

                if (image != null)
                {
                    CurrentImage = image.ToBitmapSource();
                }

            }
        }

        public void Stop()
        {
            _cancellationToken?.Cancel();
            CurrentImage = null;
        }


    }
}