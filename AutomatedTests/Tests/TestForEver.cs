using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AutomatedTests.Tests
{
    public class TestForEver
    {
        private Instance _instance;

        [OneTimeSetUp]
        public void Setup()
        {
            _instance = new Instance();
            _instance.MainWindow.SettingsPanel.StartButton.Click();
        }




        [Test]
        public void RunForEver()
        {
            ;
        }
    }
}
