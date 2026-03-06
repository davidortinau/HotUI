using CometBaristaNotes.Models;

namespace CometBaristaNotes.Services;

public interface IAIAdviceService
{
	Task<string> GetShotAdvice(ShotRecord shot);
	Task<string> GetBeanRecommendation(Bean bean);
	Task<string> AnalyzeBrewingPattern(IEnumerable<ShotRecord> recentShots);
	bool IsAvailable { get; }
}
