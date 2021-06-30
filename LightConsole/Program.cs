using Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Light.Chases;
using static Light.LedEffects;

namespace LightConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int frontPixels = 30;
            int sidePixels = 250;

            LedController ledController = new LedController(frontPixels, sidePixels, "COM12", 15);
            ledController.StartAsync();
            ledController.StartAnimationAtFront(new NeoPixelUsbBridge(sidePixels, 3, 6));

            while (true)
            {
                var c = Console.ReadKey();
                if (c.Key == ConsoleKey.Spacebar)
                {
                    ledController.StartAnimationAtFront(new SingleColor(sidePixels, RgbToInt(255,255,255)));
                }
                else if (c.Key == ConsoleKey.D1)
                {
                    ledController.StartAnimationAtFront(new Clear(sidePixels));

                }
                else if (c.Key == ConsoleKey.D2)
                {
                    ledController.StartAnimationAtFront(new NeoPixelUsbBridge(sidePixels, 3, 30));
                }
                else if (c.Key == ConsoleKey.D3)
                {
                    ledController.StartAnimationAtFront(new Flicker(sidePixels));
                }
                else if (c.Key == ConsoleKey.R)
                {
                    ledController.StartAnimationAtFront(new SingleColor(sidePixels, RgbToInt(255, 0, 0)));
                }
                else if (c.Key == ConsoleKey.G)
                {
                    ledController.StartAnimationAtFront(new SingleColor(sidePixels, RgbToInt(0, 255, 0)));
                }
                else if (c.Key == ConsoleKey.B)
                {
                    ledController.StartAnimationAtFront(new SingleColor(sidePixels, RgbToInt(0, 0, 255)));
                }
                else if(c.Key == ConsoleKey.M)
                {
                    ledController.StartAnimationAtFront(new MovingPatternDrawer(sidePixels,
                        new []
                        {
                            RgbToInt(255,0,0),
                            RgbToInt(250,0,0),
                            RgbToInt(240,0,0),
                            RgbToInt(230,0,0),
                            RgbToInt(210,0,0),
                            RgbToInt(200,0,0),
                            RgbToInt(180,0,0),
                            RgbToInt(170,0,0),
                            RgbToInt(150,0,0),
                            RgbToInt(100,0,0),
                            RgbToInt(60,0,0),
                            RgbToInt(10,0,0),
                            RgbToInt(0,0,0),
                        }));
                }
                else if(c.Key == ConsoleKey.S)
                {
                    ledController.StartAnimationAtFront(new SlidingPatternDrawer(sidePixels,
                        new[]
                        {
                            RgbToInt(0,0,255),
                            RgbToInt(0,0,255),
                            RgbToInt(0,0,255),
                            RgbToInt(255,255,255),
                            RgbToInt(0,0,255),
                            RgbToInt(0,0,255),
                            RgbToInt(0,0,255),
                        }));
                }
            }
        }
    }
}
