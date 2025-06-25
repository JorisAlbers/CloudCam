using FlaUI.Core.AutomationElements;

namespace AutomatedTests.POM;

public class SettingsPanelPom
{
    private readonly AutomationElement? _element;


    public SettingsPanelPom(AutomationElement? element)
    {
        if(element == null)
        {
            throw new ArgumentNullException();
        }
        _element = element;
    }

    public Button StartButton =>
        _element.FindFirstDescendant(x => x.ByAutomationId("StartButton")).AsButton();

}