using FlaUI.Core.AutomationElements;

namespace AutomatedTests.POM;

public class PhotoBoothPom
{
    public readonly AutomationElement? _element;

    public PhotoBoothPom(AutomationElement? element)
    {
        if (element == null)
        {
            throw new ArgumentNullException();
        }
        _element = element;
    }

    public bool IsAskingIfPhotoShouldBePrinted
    {
        get
        {
            var element = _element.FindFirstDescendant(x => x.ByAutomationId("ElicitIfImageShouldBePrintedControl"));
            if (element == null)
            {
                return false;
            }

            return element is { IsAvailable: true, IsOffscreen: false };
        }
    }
}