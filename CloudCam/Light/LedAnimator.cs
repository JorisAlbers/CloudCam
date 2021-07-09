using System;
using System.Collections.Generic;
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
                byte[] randomBytes = new byte[3];
                while (true)
                {

                    try
                    {


                        IEnumerator<int[]> chase;

                        int mode = random.Next(0, 3);
                        if (mode == 0)
                        {
                            random.NextBytes(randomBytes);

                            chase = creator.CreateSlowFading(randomBytes[0], randomBytes[1], randomBytes[2]);
                        }
                        else if (mode == 1)
                        {
                            chase = creator.CreateMovingPattern(random);
                        }
                        else
                        {
                            chase = creator.CreateSlidingPatterns(random);
                        }
                        _ledController.StartAnimationAtSide(chase);

                    }
                    catch (Exception e)
                    {
                        Console.Out.WriteLine("Failed to create animation");
                        Console.Out.WriteLine(e);
                    }



                    await Task.Delay(TimeSpan.FromSeconds(10));
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

        public AnimationCreator(int pixels)
        {
            _pixels = pixels;
        }

        public IEnumerator<int[]> CreateBlueSlidingPatterns()
        {
            int[] pattern = new int[]
            {
                LedAnimator.RgbToInt(0, 0, 255),
                LedAnimator.RgbToInt(0,0, 200),
                LedAnimator.RgbToInt(0,0, 150),
                LedAnimator.RgbToInt(0,0, 100),
                LedAnimator.RgbToInt(0,0, 50),
                LedAnimator.RgbToInt(0, 0, 0),
                LedAnimator.RgbToInt(0,0, 50),
                LedAnimator.RgbToInt(0,0, 100),
                LedAnimator.RgbToInt(0,0, 150),
                LedAnimator.RgbToInt(0,0, 200),
                LedAnimator.RgbToInt(0,0, 255),
            };

            return new SlidingPatternDrawer(_pixels, pattern);
        }

        public IEnumerator<int[]> CreateGreenSlidingPatterns()
        {
            int[] pattern = new int[]
            {
                LedAnimator.RgbToInt(0, 255, 0),
                LedAnimator.RgbToInt(0,250, 0),
                LedAnimator.RgbToInt(0,220, 0),
                LedAnimator.RgbToInt(0,255, 0),
                LedAnimator.RgbToInt(0,200, 0),
                LedAnimator.RgbToInt(0, 0, 0),
                LedAnimator.RgbToInt(0,50, 0),
                LedAnimator.RgbToInt(0,100, 0),
                LedAnimator.RgbToInt(0,150, 0),
                LedAnimator.RgbToInt(0,200, 0),
                LedAnimator.RgbToInt(0,255, 0),
            };

            return new SlidingPatternDrawer(_pixels, pattern);
        }

        public IEnumerator<int[]> CreateRedSlidingPatterns()
        {
            int[] pattern = new int[]
            {
                LedAnimator.RgbToInt(255, 0, 0),
                LedAnimator.RgbToInt(200, 0, 0),
                LedAnimator.RgbToInt(150, 0, 0),
                LedAnimator.RgbToInt(100, 0, 0),
                LedAnimator.RgbToInt(50, 0, 0),
                LedAnimator.RgbToInt(0, 0, 0),
                LedAnimator.RgbToInt(50, 0, 0),
                LedAnimator.RgbToInt(100, 0, 0),
                LedAnimator.RgbToInt(150, 0, 0),
                LedAnimator.RgbToInt(200, 0, 0),
                LedAnimator.RgbToInt(255, 0, 0),
            };

            return new SlidingPatternDrawer(_pixels, pattern);
        }

        public IEnumerator<int[]> CreateSlowFading(byte r, byte g, byte b)
        {
            // fade in steps
            // todo SIN
            int fadesteps = 100;
            byte rStep = (byte) (r / fadesteps);
            byte gStep = (byte) (g / fadesteps);
            byte bStep = (byte) (b / fadesteps);

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

        public IEnumerator<int[]> CreateMovingPattern(Random random)
        {
            List<int[]> patterns = new List<int[]>();
            patterns.Add(new int[]
            {
                LedAnimator.RgbToInt(0,0,0),
                LedAnimator.RgbToInt(0,255,0),
            });

            patterns.Add(new int[]
            {
                LedAnimator.RgbToInt(0,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(200,0,0),
                LedAnimator.RgbToInt(180,0,0),
                LedAnimator.RgbToInt(100,0,0),
                LedAnimator.RgbToInt(80,0,0),
                LedAnimator.RgbToInt(70,0,0),
                LedAnimator.RgbToInt(60,0,0),
                LedAnimator.RgbToInt(50,0,0),
                LedAnimator.RgbToInt(40,0,0),
                LedAnimator.RgbToInt(30,0,0),
                LedAnimator.RgbToInt(20,0,0),
                LedAnimator.RgbToInt(0,0,0),
            });

            patterns.Add(new int[]
            {
                LedAnimator.RgbToInt(0,0,0),
                LedAnimator.RgbToInt(20,0,0),
                LedAnimator.RgbToInt(50,0,0),
                LedAnimator.RgbToInt(60,0,0),
                LedAnimator.RgbToInt(70,0,0),
                LedAnimator.RgbToInt(80,0,0),
                LedAnimator.RgbToInt(100,0,0),
                LedAnimator.RgbToInt(180,0,0),
                LedAnimator.RgbToInt(200,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(255,0,0),
                LedAnimator.RgbToInt(200,0,0),
                LedAnimator.RgbToInt(180,0,0),
                LedAnimator.RgbToInt(100,0,0),
                LedAnimator.RgbToInt(80,0,0),
                LedAnimator.RgbToInt(70,0,0),
                LedAnimator.RgbToInt(60,0,0),
                LedAnimator.RgbToInt(50,0,0),
                LedAnimator.RgbToInt(40,0,0),
                LedAnimator.RgbToInt(30,0,0),
                LedAnimator.RgbToInt(20,0,0),
                LedAnimator.RgbToInt(0,0,0),
            });

            return new MovingPatternDrawer(_pixels,patterns[random.Next(0,patterns.Count-1)]);
        }

        public IEnumerator<int[]> CreateSlidingPatterns(Random random)
        {
            int mode = random.Next(0, 4);
            if (mode == 0)
            {
                return CreateRedSlidingPatterns();
            }

            if (mode == 1)
            {
                return CreateBlueSlidingPatterns();
            }

            if (mode == 2)
            {
                return BlueLight();
            }

            return CreateGreenSlidingPatterns();
        }

        public IEnumerator<int[]> BlueLight()
        {
            int[] pattern = new int[]
            {
                LedAnimator.RgbToInt(0, 0, 0),
                LedAnimator.RgbToInt(0, 0, 50),
                LedAnimator.RgbToInt(0, 0,100),
                LedAnimator.RgbToInt(0, 0,150),
                LedAnimator.RgbToInt(0, 0,200),
                LedAnimator.RgbToInt(0, 0,220),
                LedAnimator.RgbToInt(0, 0,230),
                LedAnimator.RgbToInt(0, 0,245),
                LedAnimator.RgbToInt(255, 255,255),
                LedAnimator.RgbToInt(0, 0,245),
                LedAnimator.RgbToInt(0, 0,230),
                LedAnimator.RgbToInt(0, 0,220),
                LedAnimator.RgbToInt(0, 0,200),
                LedAnimator.RgbToInt(0, 0,150),
                LedAnimator.RgbToInt(0, 0,100),
                LedAnimator.RgbToInt(0, 0,50),
                LedAnimator.RgbToInt(0, 0,0),
            };

            return new SlidingPatternDrawer(_pixels, pattern);
        }
    }
}
