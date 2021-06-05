using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudCam
{
    public class EffectManager
    {
        private int _effectIndex = 0;
        private readonly List<IEffect> _effects;
        

        public EffectManager()
        {
            _effects = new List<IEffect>
            {
                new OilPainting()
            };
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
