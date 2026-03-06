using System.Threading;
using System.Threading.Tasks;

namespace CometBaristaNotes.Services;

public interface ISpeechRecognitionService
{
	Task<string?> RecognizeSpeechAsync(CancellationToken ct = default);
	bool IsAvailable { get; }
}
