using System.Collections.Generic;
using System.Threading.Tasks;

namespace CometBaristaNotes.Services;

public class MockVoiceCommandService : IVoiceCommandService
{
	private readonly INavigationRegistry _navigationRegistry;

	public MockVoiceCommandService(INavigationRegistry navigationRegistry)
	{
		_navigationRegistry = navigationRegistry;
	}

	public bool IsAvailable => true;

	public Task<VoiceCommandResult> ProcessCommand(string spokenText)
	{
		if (string.IsNullOrWhiteSpace(spokenText))
		{
			return Task.FromResult(new VoiceCommandResult
			{
				Understood = false,
				FeedbackMessage = "I didn't hear anything. Please try again."
			});
		}

		var words = spokenText.ToLowerInvariant().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

		// Try each word as a keyword against the navigation registry
		foreach (var word in words)
		{
			var route = _navigationRegistry.FindRoute(word);
			if (route != null)
			{
				if (route.RequiresParameter)
				{
					return Task.FromResult(new VoiceCommandResult
					{
						Understood = true,
						Route = route.Route,
						Action = "navigate",
						FeedbackMessage = $"Navigating to {route.DisplayName}. A parameter ({route.ParameterName}) may be needed.",
						Parameters = new Dictionary<string, object>()
					});
				}

				return Task.FromResult(new VoiceCommandResult
				{
					Understood = true,
					Route = route.Route,
					Action = "navigate",
					FeedbackMessage = $"Navigating to {route.DisplayName}.",
					Parameters = new Dictionary<string, object>()
				});
			}
		}

		return Task.FromResult(new VoiceCommandResult
		{
			Understood = false,
			FeedbackMessage = "I didn't understand that. Try saying something like \"show me my beans\" or \"go to settings\"."
		});
	}
}
