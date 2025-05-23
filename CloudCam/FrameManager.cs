using System;
using CloudCam.View;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Serilog;

namespace CloudCam
{
    public class FrameManager
    {
        private readonly ImageRepository _frameRepository;
        private int _currentFrameIndex = -1;

        public FrameManager(ImageRepository frameRepository)
        {
            _frameRepository = frameRepository;
        }

        public ImageSourceWithMat Next(Size size)
        {
            _currentFrameIndex++;
            if (_currentFrameIndex > _frameRepository.Count - 1)
            {
                _currentFrameIndex = -1;
            }

            return LoadImage(_currentFrameIndex, size);
        }

        public ImageSourceWithMat Previous(Size size)
        {
            _currentFrameIndex--;
            if (_currentFrameIndex == -2)
            {
                _currentFrameIndex = _frameRepository.Count - 1;
            }

            try
            {
                return LoadImage(_currentFrameIndex, size);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex,"Failed to load frame!");
            }

            return null;
        }

        private ImageSourceWithMat LoadImage(int index, Size size)
        {
            if (index == -1)
            {
                return new ImageSourceWithMat(null,null);
            }

            var image = _frameRepository[_currentFrameIndex].image;
            Cv2.Resize(image, image, size);
            var imageSource = image.ToBitmapSource();
            imageSource.Freeze();
            return new ImageSourceWithMat(imageSource, image);
        }
    }
}