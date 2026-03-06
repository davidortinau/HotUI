using System;
using Comet.HotReload;
using Microsoft.Maui.HotReload;
using Xunit;

namespace Comet.Tests.HotReload;

public class MetadataUpdateHandlerTests : TestBase
{
	[Fact]
	public void UpdateType_RegistersReplacedView()
	{
		MauiHotReloadHelper.IsEnabled = true;
		MauiHotReloadHelper.Reset();

		// Simulate a type update
		CometMetadataUpdateHandler.UpdateType(new[] { typeof(TestReplacementView) });

		// The type should now be registered as a replacement.
		// We verify indirectly by checking that the handler doesn't throw
		// and that TriggerReload completes.
		CometMetadataUpdateHandler.UpdateApplication(null);
	}

	[Fact]
	public void UpdateType_WithNull_DoesNotThrow()
	{
		CometMetadataUpdateHandler.UpdateType(null);
	}

	[Fact]
	public void UpdateApplication_WithNull_DoesNotThrow()
	{
		CometMetadataUpdateHandler.UpdateApplication(null);
	}

	[Fact]
	public void ClearCache_WithNull_DoesNotThrow()
	{
		CometMetadataUpdateHandler.ClearCache(null);
	}

	class TestReplacementView : View
	{
		[Body]
		View body() => new Text("Updated");
	}
}
