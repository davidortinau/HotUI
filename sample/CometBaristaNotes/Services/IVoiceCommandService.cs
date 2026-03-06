using System.Collections.Generic;
using System.Threading.Tasks;

namespace CometBaristaNotes.Services;

public class VoiceCommandResult
{
	public bool Understood { get; set; }
	public string? Route { get; set; }
	public string? Action { get; set; }
	public string? FeedbackMessage { get; set; }
	public Dictionary<string, object>? Parameters { get; set; }
}

public interface IVoiceCommandService
{
	Task<VoiceCommandResult> ProcessCommand(string spokenText);
	bool IsAvailable { get; }
}
