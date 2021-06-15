using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

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

            return LoadImage(_currentFrameIndex, size);
        }

        private ImageSourceWithMat LoadImage(int index, Size size)
        {
            if (index == -1)
            {
                return null;
            }

            var image = _frameRepository[_currentFrameIndex];
            Cv2.Resize(image, image, size);
            var imageSource = image.ToBitmapSource();
            imageSource.Freeze();
            return new ImageSourceWithMat(imageSource, image);
        }
    }
}