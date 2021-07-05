using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Light;
using Light.Chases;

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

        private async Task Animate()
        {
            AnimationCreator creator = new AnimationCreator(_pixelsSide);
            Random random = new Random();
            await Task.Run(async () =>
            {
                byte[] randomBytes = new byte[3];
                while (true)
                {
                    random.NextBytes(randomBytes);

                    var chase = creator.CreateSlowFading(randomBytes[0], randomBytes[1], randomBytes[2]);
                    _ledController.StartAnimationAtSide(chase);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
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

    public class AnimationCreator
    {
        private readonly int _pixels;

        public AnimationCreator(int pixels)
        {
            _pixels = pixels;
        }

        public IEnumerator<int[]> CreateSlowFading(byte r, byte g, byte b)
        {
            // fade in steps
            // todo SIN
            int fadesteps = 100;
            byte rStep = (byte) (r / fadesteps);
            byte gStep = (byte) (r / fadesteps);
            byte bStep = (byte) (r / fadesteps);

            int[][] colors = new int[fadesteps][];


            for (int i = 0; i < fadesteps; i++)
            {
                colors[i] = new[] {LedAnimator.RgbToInt((byte) (rStep * i), (byte) (gStep * i), (byte) (bStep * i))};
            }

            for (int i = fadesteps - 1; i >= 0; i--)
            {
                colors[i] = new[] { LedAnimator.RgbToInt((byte)(rStep * i), (byte)(gStep * i), (byte)(bStep * i)) };
            }

            return new RepeatingPatternsDrawer(_pixels, colors);
        }
    }
}
