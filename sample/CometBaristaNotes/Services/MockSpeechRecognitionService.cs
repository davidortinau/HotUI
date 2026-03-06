using System.Threading;
using System.Threading.Tasks;

namespace CometBaristaNotes.Services;

public class MockSpeechRecognitionService : ISpeechRecognitionService
{
	private static readonly string[] SimulatedPhrases = new[]
	{
		"show me my beans",
		"go to settings",
		"log a new shot",
		"open equipment",
		"show activity feed"
	};

	private int _phraseIndex;

	public bool IsAvailable => true;

	public async Task<string?> RecognizeSpeechAsync(CancellationToken ct = default)
	{
		// Simulate listening delay
		await Task.Delay(2000, ct);

		var phrase = SimulatedPhrases[_phraseIndex % SimulatedPhrases.Length];
		_phraseIndex++;
		return phrase;
	}
}
