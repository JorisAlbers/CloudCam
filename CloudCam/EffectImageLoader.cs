using OpenCvSharp;

namespace CloudCam
{
    public class EffectImageLoader
    {
        private readonly ImageRepository _imageRepository;
        private readonly ImageSettingsRepository _imageSettingsRepository;

        public EffectImageLoader(ImageRepository imageRepository, ImageSettingsRepository imageSettingsRepository)
        {
            _imageRepository = imageRepository;
            _imageSettingsRepository = imageSettingsRepository;
        }

        public int Count => _imageRepository.Count;

        public EffectImage this[int index]
        {
            get
            {
                (string name, Mat image) imageAndName = _imageRepository[index];

                if (_imageSettingsRepository.TryLoad(imageAndName.name, out ImageSettings settings))
                {
                    return new EffectImageWithSettings(imageAndName.image, settings);
                }

                return new EffectImage(imageAndName.image);
            }
        }

        public void Load()
        {
            _imageRepository.Load();
        }
    }
}