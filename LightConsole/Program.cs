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
            ledController.StartAnimation(new NeoPixelUsbBridge(sidePixels, 3, 6));

            while (true)
            {
                var c = Console.ReadKey();
                if (c.Key == ConsoleKey.Spacebar)
                {
                    ledController.Flash(3);
                }
                else if (c.Key == ConsoleKey.D1)
                {
                    ledController.StartAnimation(new Clear(300));

                }
                else if (c.Key == ConsoleKey.D2)
                {
                    ledController.StartAnimation(new NeoPixelUsbBridge(300, 3, 30));
                }
                else if (c.Key == ConsoleKey.D3)
                {
                    ledController.StartAnimation(new Flicker(300));
                }
                else if (c.Key == ConsoleKey.R)
                {
                    ledController.StartAnimation(new Red(300));
                }
                else if (c.Key == ConsoleKey.G)
                {
                    ledController.StartAnimation(new Green(300));
                }
                else if (c.Key == ConsoleKey.B)
                {
                    ledController.StartAnimation(new Blue(300));
                }
            }
        }
    }
}
