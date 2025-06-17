using FlaUI.Core.AutomationElements;

namespace AutomatedTests.POM;

public class MainWindowPom
{
    private Window _window;

    public MainWindowPom(Window? mainwindow)
    {
        if (mainwindow == null)
        {
            throw new ArgumentNullException();
        }

        _window = mainwindow;
    }

    public SettingsPanelPom SettingsPanel =>
        new SettingsPanelPom(_window.FindFirstDescendant(x => x.ByAutomationId("SettingsControl")));
}