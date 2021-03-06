using System.Collections.Generic;
using CloudCam.Detection;
using OpenCvSharp.Dnn;

namespace CloudCam.Effect
{
    public class EffectManager
    {
        private readonly EffectImageLoader _mustachesRepository;
        private readonly EffectImageLoader _hatsRepository;
        private readonly EffectImageLoader _glassesRepository;
        private int _effectIndex = 0;
        private readonly List<IEffect> _effects;
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;
        private EyesDetection _eyesDetection;


        public EffectManager(string caffeConfigFile, string caffeWeightFile, string noseCascadeFile,
            string eyesCascadeFile, EffectImageLoader mustachesRepository,
            EffectImageLoader hatsRepository, EffectImageLoader glassesRepository)
        {
            _mustachesRepository = mustachesRepository;
            _hatsRepository = hatsRepository;
            _glassesRepository = glassesRepository;
            _faceDetection = new FaceDetection(caffeConfigFile, caffeWeightFile);
            _noseDetection = new NoseDetection(noseCascadeFile);
            _eyesDetection = new EyesDetection(eyesCascadeFile);

            _effects = new List<IEffect>
            {
                null,
                new OilPainting(),
#if DEBUG
                new DebugDetection(_faceDetection, _noseDetection, _eyesDetection),
#endif
            };

            for (int i = 0; i < mustachesRepository.Count; i++)
            {
                EffectImageWithSettings settings = LoadImage(mustachesRepository[i]);
                ImageOverlayer overlayer = new ImageOverlayer(settings.Image);
                _effects.Add(new Mustaches(overlayer,settings.Image.Size(), settings.Settings, _faceDetection, _noseDetection));
            }

            for (int i = 0; i < hatsRepository.Count; i++)
            {
                EffectImageWithSettings settings = LoadImage(hatsRepository[i]);
                ImageOverlayer overlayer = new ImageOverlayer(settings.Image);
                _effects.Add(new Hats(overlayer, settings.Image.Size(), settings.Settings, _faceDetection));
            }

            for (int i = 0; i < glassesRepository.Count; i++)
            {
                EffectImageWithSettings settings = LoadImage(glassesRepository[i]);
                ImageOverlayer overlayer = new ImageOverlayer(settings.Image);
                _effects.Add(new Glasses(overlayer, settings.Image.Size(), settings.Settings, _faceDetection, _eyesDetection));
            }
        }

        private EffectImageWithSettings LoadImage(EffectImage effectImage)
        {
            if (effectImage is EffectImageWithSettings effectImageWithSettings)
            {
                return effectImageWithSettings;
            }

            return new EffectImageWithSettings(effectImage.Image, GetDefaultImageSettings());
        }

        private ImageSettings GetDefaultImageSettings()
        {
            return new ImageSettings(0, 0, 1);
        }

        public IEffect Next()
        {
            if (++_effectIndex > _effects.Count - 1)
            {
                _effectIndex = 0;
            }
            
            return _effects[_effectIndex];
        }

        public IEffect Previous()
        {
            if (--_effectIndex < 0)
            {
                _effectIndex = _effects.Count - 1;
            }

            return _effects[_effectIndex];
        }
    }
}
