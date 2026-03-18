namespace Comet.Styles
{
	/// <summary>
	/// Interface that controls implement to participate in theming.
	/// When the active theme changes, <see cref="ApplyTheme"/> is called
	/// so the control can update its appearance.
	/// </summary>
	public interface IThemeable
	{
		/// <summary>
		/// Called when a theme is applied to update the control's visual properties.
		/// </summary>
		void ApplyTheme(Theme theme);
	}
}
