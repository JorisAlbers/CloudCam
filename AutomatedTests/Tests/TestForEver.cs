using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AutomatedTests.Tests
{
    public class TestForEver
    {
        private Instance _instance;

        [OneTimeSetUp]
        public void Setup()
        {
            Retry.DefaultTimeout = TimeSpan.FromSeconds(20);

            _instance = new Instance();
            var mw = Retry.WhileNull(() => _instance.MainWindow).Result;
            var sp = Retry.WhileNull(() => mw?.SettingsPanel).Result;
            
            sp.StartButton.Click();

        }




        [Test]
        public void RunForEver()
        {
            int counter = 0;
            var random = new Random(7);
            while (true)
            {
                try
                {
                    if (counter++ > 10)
                    {
                        RemoveAllOutput();
                        counter = 0;
                    }
                    
                    // move to next frame
                    Keyboard.Press(VirtualKeyShort.LEFT);
                    Thread.Sleep(100);
                    // move to next effect
                    Keyboard.Press(VirtualKeyShort.KEY_A);
                    Thread.Sleep(100);
                    // take picture
                    Keyboard.Press(VirtualKeyShort.SPACE);
                    Thread.Sleep(100);
                    // wait for "print?" 
                    if (random.Next(0, 2) == 0)
                    { 
                        Keyboard.Press(VirtualKeyShort.LEFT);
                    }
                    {
                        Keyboard.Press(VirtualKeyShort.SPACE);
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine($"Test crashed, retry! error = {e.Message} {e.StackTrace}");
                }
            }
        }

        private void RemoveAllOutput()
        {
            Directory.Delete("C:\\ProgramData\\CloudCam\\Output", true);
            Directory.CreateDirectory("C:\\ProgramData\\CloudCam\\Output");
        }
    }
}
