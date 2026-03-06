using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace CometBaristaNotes.Platforms.iOS;

/// <summary>
/// Custom ShellRenderer that uses CustomShellSectionRenderer with PrefersLargeTitles support.
/// Matches the reference BaristaNotes app's large title behavior.
/// </summary>
public class CustomShellRenderer : ShellRenderer
{
	public static bool PrefersLargeTitles { get; set; } = true;

	protected override IShellSectionRenderer CreateShellSectionRenderer(Microsoft.Maui.Controls.ShellSection shellSection)
	{
		return new CustomShellSectionRenderer(this)
		{
			ShellSection = shellSection,
			PrefersLargeTitles = PrefersLargeTitles
		};
	}
}
