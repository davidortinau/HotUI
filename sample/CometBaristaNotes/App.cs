using Microsoft.Maui.Controls;

namespace CometBaristaNotes;

public class BaristaNotesApp : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new BaristaShell());
	}
}
