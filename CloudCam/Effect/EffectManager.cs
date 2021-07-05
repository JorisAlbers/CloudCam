using System.Collections.Generic;
using CloudCam.Detection;
using OpenCvSharp.Dnn;

namespace CloudCam.Effect
{
    public class EffectManager
    {
        private readonly ImageRepository _mustachesRepository;
        private readonly ImageRepository _hatsRepository;
        private int _effectIndex = 0;
        private readonly List<IEffect> _effects;
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;


        public EffectManager(string caffeConfigFile, string caffeWeightFile, string noseCascadeFile, ImageRepository mustachesRepository,
            ImageRepository hatsRepository)
        {
            _mustachesRepository = mustachesRepository;
            _hatsRepository = hatsRepository;
            _faceDetection = new FaceDetection(caffeConfigFile, caffeWeightFile);
            _noseDetection = new NoseDetection(noseCascadeFile);

            _effects = new List<IEffect>
            {
                null,
                new OilPainting(),
#if DEBUG
                new DebugDetection(_faceDetection, _noseDetection),
#endif
            };

            for (int i = 0; i < mustachesRepository.Count; i++)
            {
                _effects.Add(new Mustaches(mustachesRepository[i].image, _faceDetection, _noseDetection));
            }

            for (int i = 0; i < hatsRepository.Count; i++)
            {
                _effects.Add(new Hats(hatsRepository[i].image, _faceDetection));
            }
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
