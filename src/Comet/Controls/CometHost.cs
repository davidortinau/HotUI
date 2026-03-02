using Microsoft.Maui.Controls;

namespace Comet;

/// <summary>
/// A Microsoft.Maui.Controls.View that hosts a Comet MVU View inside MAUI pages (e.g. Shell ContentPages).
/// This is the reverse of MauiViewHost: it allows embedding Comet views inside MAUI XAML/code pages.
/// </summary>
public class CometHost : Microsoft.Maui.Controls.View
{
	public static readonly BindableProperty CometViewProperty = BindableProperty.Create(
		nameof(CometView), typeof(Comet.View), typeof(CometHost));

	public Comet.View CometView
	{
		get => (Comet.View)GetValue(CometViewProperty);
		set => SetValue(CometViewProperty, value);
	}

	public CometHost() { }

	public CometHost(Comet.View view)
	{
		CometView = view;
	}
}
