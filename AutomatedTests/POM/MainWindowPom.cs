using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;

namespace AutomatedTests.POM;

public class MainWindowPom
{
    public Window _window;

    public MainWindowPom(Window? mainwindow)
    {
        if (mainwindow == null)
        {
            throw new ArgumentNullException();
        }

        _window = mainwindow;
    }

    public SettingsPanelPom SettingsPanel =>
        new SettingsPanelPom(
            Retry.WhileNull(()=>_window.FindFirstDescendant(x => x.ByAutomationId("SettingsControl"))).Result
            );

    public PhotoBoothPom PhotoBooth => new PhotoBoothPom(Retry.WhileNull(()=>_window.FindFirstDescendant(x => x.ByAutomationId("PhotoBoothControl"))).Result);
}