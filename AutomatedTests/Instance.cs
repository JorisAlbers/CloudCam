using System.Diagnostics;
using System.Reflection;
using CloudCam.View;
using System.Windows.Automation;
using System.Windows.Navigation;
using FlaUI.Core;
using FlaUI.UIA3;
using AutomatedTests.POM;

namespace AutomatedTests
{
    public class Instance
    {
        private readonly UIA3Automation _automation;
        private readonly Application _application;

        public Instance()
        {
            string exePath = GetProgramExe();
            var startInfo = new ProcessStartInfo(exePath);

             _application = Application.Launch(startInfo);
             _automation = new UIA3Automation();
        }


        public MainWindowPom MainWindow => new MainWindowPom(_application.GetMainWindow(_automation));


        private string GetProgramExe()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string folderPath = System.IO.Path.GetDirectoryName(exePath);
            return Path.Combine(folderPath, "CloudCam.exe");
        }
    }
}
