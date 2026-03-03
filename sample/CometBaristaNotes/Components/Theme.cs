using Microsoft.Maui.Graphics;

namespace CometBaristaNotes.Components;

public static class Theme
{
	// Colors
	public static readonly Color Primary = Color.FromArgb("#86543F");
	public static readonly Color Background = Color.FromArgb("#D2BCA5");
	public static readonly Color Surface = Color.FromArgb("#FCEFE1");
	public static readonly Color SurfaceVariant = Color.FromArgb("#ECDAC4");
	public static readonly Color SurfaceElevated = Color.FromArgb("#FFF7EC");
	public static readonly Color TextPrimary = Color.FromArgb("#352B23");
	public static readonly Color TextSecondary = Color.FromArgb("#7C7067");
	public static readonly Color TextMuted = Color.FromArgb("#A38F7D");
	public static readonly Color Outline = Color.FromArgb("#D7C5B2");
	public static readonly Color Success = Color.FromArgb("#4CAF50");
	public static readonly Color Warning = Color.FromArgb("#FFA726");
	public static readonly Color Error = Color.FromArgb("#EF5350");

	// Spacing
	public const int SpacingXS = 4;
	public const int SpacingS = 8;
	public const int SpacingM = 16;
	public const int SpacingL = 24;
	public const int SpacingXL = 32;

	// Radii
	public const float RadiusPill = 25;
	public const float RadiusCard = 12;
	public const float RadiusEditor = 16;

	// Sizes
	public const float FormFieldHeight = 50;
	public const float ButtonHeight = 48;
	public const float IconSizeSmall = 14;
	public const float IconSizeMedium = 24;
	public const float IconSizeLarge = 64;
}
