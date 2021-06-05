using System.Collections.Generic;
using CloudCam.Detection;
using OpenCvSharp.Dnn;

namespace CloudCam.Effect
{
    public class EffectManager
    {
        private readonly ImageRepository _mustachesRepository;
        private int _effectIndex = 0;
        private readonly List<IEffect> _effects;
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;


        public EffectManager(string faceCascadeFile, string noseCascadeFile, ImageRepository mustachesRepository)
        {
            _mustachesRepository = mustachesRepository;
            _faceDetection = new FaceDetection(faceCascadeFile);
            _noseDetection = new NoseDetection(noseCascadeFile);

            _effects = new List<IEffect>
            {
                null,
                new OilPainting(),
            };

            for (int i = 0; i < mustachesRepository.Count; i++)
            {
                _effects.Add(new Mustaches(mustachesRepository[i], _faceDetection, _noseDetection));
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
