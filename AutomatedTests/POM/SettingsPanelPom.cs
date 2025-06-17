using FlaUI.Core.AutomationElements;

namespace AutomatedTests.POM;

public class SettingsPanelPom
{
    private readonly AutomationElement? _findFirstDescendant;


    public SettingsPanelPom(AutomationElement? findFirstDescendant)
    {
        _findFirstDescendant = findFirstDescendant;
    }

    public Button StartButton =>
        _findFirstDescendant.FindFirstDescendant(x => x.ByAutomationId("StartButton")).AsButton();

}