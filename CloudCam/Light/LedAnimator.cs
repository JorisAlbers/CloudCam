using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Light;

namespace CloudCam.Light
{
    public class LedAnimator
    {
        private readonly int _pixelsFront;
        private readonly int _pixelsSide;
        private readonly LedController _ledController;

        public LedAnimator(int pixelsFront, int pixelsSide,LedController ledController)
        {
            _pixelsFront = pixelsFront;
            _pixelsSide = pixelsSide;
            _ledController = ledController;
        }

        public async Task StartAsync()
        {
            await _ledController.StartAsync();
        }

        public async Task Flash(int seconds)
        {
            _ledController.StartAnimationAtFront(new SingleColor(_pixelsFront, RgbToInt(255,255,255)));
            await Task.Delay(seconds * 1000);
            _ledController.StartAnimationAtFront(null);
        }

        public static int RgbToInt(byte r, byte g, byte b)
        {
            return g << 16 | r << 8 | b;
        }

    }
}
