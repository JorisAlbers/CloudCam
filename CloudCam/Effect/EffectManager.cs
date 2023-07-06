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
        private readonly List<IFaceDetectionEffect> _effects;
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

            _effects = new List<IFaceDetectionEffect>
            {
                null,
                // TODO enable oilpainting and debug detection
                //new OilPainting(),
#if DEBUG
                //new DebugDetection(_faceDetection, _noseDetection, _eyesDetection),
#endif
            };

            for (int i = 0; i < mustachesRepository.Count; i++)
            {
                EffectImageWithSettings settings = LoadImage(mustachesRepository[i]);
                _effects.Add(new Mustaches(settings.Image.Size(), settings, _faceDetection, _noseDetection));
            }

            for (int i = 0; i < hatsRepository.Count; i++)
            {
                EffectImageWithSettings settings = LoadImage(hatsRepository[i]);
                _effects.Add(new Hats(settings.Image.Size(), settings, _faceDetection));
            }

            for (int i = 0; i < glassesRepository.Count; i++)
            {
                EffectImageWithSettings settings = LoadImage(glassesRepository[i]);
                _effects.Add(new Glasses( settings.Image.Size(), settings, _faceDetection, _eyesDetection));
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

        public IFaceDetectionEffect Next()
        {
            if (++_effectIndex > _effects.Count - 1)
            {
                _effectIndex = 0;
            }
            
            return _effects[_effectIndex];
        }

        public IFaceDetectionEffect Previous()
        {
            if (--_effectIndex < 0)
            {
                _effectIndex = _effects.Count - 1;
            }

            return _effects[_effectIndex];
        }
    }
}
