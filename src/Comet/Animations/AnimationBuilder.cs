using System;

namespace Comet.Animations
{
/// <summary>
/// Animation helper methods for common patterns.
/// These use Comet's built-in animation system with StateBuilder.
/// </summary>
public static class AnimationBuilder
{
/// <summary>
/// Create a fade-in animation
/// </summary>
public static void AnimateFadeIn(this View view, Action onComplete = null)
{
using (new StateBuilder(view))
{
view.Opacity(0);
}
_ = System.Threading.Tasks.Task.Delay(50).ContinueWith(_ =>
{
using (new StateBuilder(view))
{
view.Opacity(1);
}
onComplete?.Invoke();
});
}

/// <summary>
/// Create a fade-out animation
/// </summary>
public static void AnimateFadeOut(this View view, Action onComplete = null)
{
using (new StateBuilder(view))
{
view.Opacity(1);
}
_ = System.Threading.Tasks.Task.Delay(50).ContinueWith(_ =>
{
using (new StateBuilder(view))
{
view.Opacity(0);
}
onComplete?.Invoke();
});
}

/// <summary>
/// Pulse animation - fades in and out repeatedly
/// </summary>
public static async void AnimatePulse(this View view, int count = 1)
{
for (int i = 0; i < count; i++)
{
using (new StateBuilder(view))
{
view.Opacity(0.5);
}
await System.Threading.Tasks.Task.Delay(200);

using (new StateBuilder(view))
{
view.Opacity(1);
}
await System.Threading.Tasks.Task.Delay(200);
}
}
}
}
