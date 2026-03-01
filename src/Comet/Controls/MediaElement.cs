using System;
using Microsoft.Maui.Graphics;

namespace Comet
{
/// <summary>
/// Media element for audio/video playback.
/// Requires Microsoft.Maui.Controls.MediaElement NuGet package.
/// </summary>
public class MediaElement : View
{
private Binding<string> _source;
public Binding<string> Source
{
get => _source;
set => this.SetBindingValue(ref _source, value);
}

private Binding<bool> _autoPlay;
public Binding<bool> AutoPlay
{
get => _autoPlay;
set => this.SetBindingValue(ref _autoPlay, value);
}

private Binding<bool> _showsPlaybackControls;
public Binding<bool> ShowsPlaybackControls
{
get => _showsPlaybackControls;
set => this.SetBindingValue(ref _showsPlaybackControls, value);
}

private Binding<double> _volume;
public Binding<double> Volume
{
get => _volume;
set => this.SetBindingValue(ref _volume, value);
}

private Binding<double> _speed;
public Binding<double> Speed
{
get => _speed;
set => this.SetBindingValue(ref _speed, value);
}

public Action OnMediaEnded { get; set; }
public Action OnMediaOpened { get; set; }
public Action<Exception> OnMediaFailed { get; set; }
}
}
