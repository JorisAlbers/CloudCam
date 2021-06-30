using Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }
    }
}
