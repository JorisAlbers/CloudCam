using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Light;
using Light.Chases;

namespace CloudCam.Light
{
    public interface ILedAnimator
    {
        Task StartAsync();
        Task Animate();
        void StartFlash();
        void EndFlash();
    }

    public class LedAnimator : ILedAnimator
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

        public async Task Animate()
        {
            AnimationCreator creator = new AnimationCreator(_pixelsSide);
            Random random = new Random();
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        IEnumerator<int[]> chase = creator.CreateRandomAnimation(random);
                        _ledController.StartAnimationAtSide(chase);

                    }
                    catch (Exception e)
                    {
                        Console.Out.WriteLine("Failed to create animation");
                        Console.Out.WriteLine(e);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }

        public void StartFlash()
        {
            _ledController.StartAnimationAtFront(new SingleColor(_pixelsFront, RgbToInt(255,255,255)));
        }

        public void EndFlash()
        {
            _ledController.StartAnimationAtFront(new SingleColor(_pixelsFront, RgbToInt(0,0,0)));
        }

        public static int RgbToInt(byte r, byte g, byte b)
        {
            return g << 16 | r << 8 | b;
        }
    }

    public class AnimationCreator
    {
        private readonly int _pixels;
        private Color[] _niceColors = new Color[]
        {
            Color.Blue,
            Color.Aquamarine,
            Color.BlueViolet,
            Color.Brown,
            Color.Orange,
            Color.Red,
            Color.Gold,
            Color.RoyalBlue,
            Color.Salmon,
            Color.Lime,
            Color.Firebrick
        };




        public AnimationCreator(int pixels)
        {
            _pixels = pixels;
        }

        public IEnumerator<int[]> CreateRandomAnimation(Random random)
        {
            Color color = _niceColors[random.Next(0, _niceColors.Length)];
            int mode = random.Next(0, 3);
            if (mode == 0)
            {
                 return CreateSlowFading(color);
            }
            if (mode == 1)
            {
                return CreateMovingPattern(color);
            }
            return CreateSlidingPatterns(color);
        }
        

        public IEnumerator<int[]> CreateSlowFading(Color color)
        {
            // fade in steps
            // todo SIN
            int fadesteps = 100;
            byte rStep = (byte) (color.R / fadesteps);
            byte gStep = (byte) (color.G / fadesteps);
            byte bStep = (byte) (color.B / fadesteps);

            List<int[]> colors = new List<int[]>();

            for (int i = 0; i < fadesteps; i++)
            {
                colors.Add(new[] {LedAnimator.RgbToInt((byte) (rStep * i), (byte) (gStep * i), (byte) (bStep * i))});
            }

            for (int i = fadesteps - 1; i >= 0; i--)
            {
                colors.Add(new[] { LedAnimator.RgbToInt((byte)(rStep * i), (byte)(gStep * i), (byte)(bStep * i)) });
            }

            return new RepeatingPatternsDrawer(_pixels, colors.ToArray());
        }

        public IEnumerator<int[]> CreateMovingPattern(Color color)
        {
            List<int> pattern = new List<int>();

            int steps = 20;
            int redIncrement = color.R / steps;
            int greenIncrement = color.G / steps;
            int blueIncrement = color.B / steps;

            pattern.Add(LedAnimator.RgbToInt(0,0,0));
            for (int i = 1; i < steps+1; i++)
            {
                pattern.Add(LedAnimator.RgbToInt((byte) (i * redIncrement), (byte) (i* greenIncrement), (byte) (i* blueIncrement)));
            }
            for (int i = steps - 1; i >= 0; i--)
            {
                pattern.Add(LedAnimator.RgbToInt((byte)(i * redIncrement), (byte)(i * greenIncrement), (byte)(i * blueIncrement)));
            }
            pattern.Add(LedAnimator.RgbToInt(0, 0, 0));
            
            return new MovingPatternDrawer(_pixels, pattern.ToArray());
        }

        public IEnumerator<int[]> CreateSlidingPatterns(Color color)
        {
            List<int> pattern = new List<int>();

            int steps = 20;
            int redIncrement = color.R / steps;
            int greenIncrement = color.G / steps;
            int blueIncrement = color.B / steps;

            pattern.Add(LedAnimator.RgbToInt(0, 0, 0));
            for (int i = 1; i < steps + 1; i++)
            {
                pattern.Add(LedAnimator.RgbToInt((byte)(i * redIncrement), (byte)(i * greenIncrement), (byte)(i * blueIncrement)));
            }

            for (int i = steps - 1; i >= 0; i--)
            {
                pattern.Add(LedAnimator.RgbToInt((byte)(i * redIncrement), (byte)(i * greenIncrement), (byte)(i * blueIncrement)));
            }

            pattern.Add(LedAnimator.RgbToInt(0, 0, 0));
            return new SlidingPatternDrawer(_pixels, pattern.ToArray());
        }
    }
}
