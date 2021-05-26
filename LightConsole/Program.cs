using Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Light.LedEfects;

namespace LightConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            LedController ledController = new LedController(300, "COM11");
            ledController.StartAsync();
            ledController.StartAnimation(new ColorChase(300, 3, 6));

            while (true)
            {
                var c = Console.ReadKey();
                if (c.Key == ConsoleKey.F)
                {
                    ledController.Flash();
                }
            }
        }
    }
}
