using CometBaristaNotes.Models;

namespace CometBaristaNotes.Services;

public class MockAIAdviceService : IAIAdviceService
{
	public bool IsAvailable => true;

	public async Task<string> GetShotAdvice(ShotRecord shot)
	{
		await Task.Delay(1000);

		var rating = shot.Rating ?? 0;

		if (rating < 2)
		{
			return "Try a finer grind setting and aim for a longer extraction time. "
				+ "If the shot is pulling too fast, the water isn't spending enough time "
				+ "with the coffee grounds to extract the good flavors.";
		}

		if (rating < 3)
		{
			return "You're getting close! Consider adjusting your dose by 0.5g or "
				+ "tweaking the grind slightly finer. Small changes can make a big "
				+ "difference in the cup.";
		}

		return "Great extraction! The balance of dose, grind, and time is working well. "
			+ "Keep these parameters dialed in for this bean and enjoy the consistency.";
	}

	public async Task<string> GetBeanRecommendation(Bean bean)
	{
		await Task.Delay(1000);

		var origin = bean.Origin?.ToLowerInvariant() ?? "unknown";

		if (origin.Contains("ethiopia") || origin.Contains("kenya"))
		{
			return $"African beans like {bean.Name} tend to have bright, fruity characteristics. "
				+ "Try a slightly coarser grind and a 1:2.5 ratio to highlight the acidity "
				+ "and floral notes. A lighter roast will preserve the origin character.";
		}

		if (origin.Contains("brazil") || origin.Contains("colombia"))
		{
			return $"South American beans like {bean.Name} typically offer nutty, chocolatey profiles. "
				+ "A standard 1:2 ratio with a medium grind works well. These beans are "
				+ "forgiving and great for dialing in your workflow.";
		}

		return $"For {bean.Name}, start with an 18g dose at a medium-fine grind targeting "
			+ "a 1:2 ratio in 25-30 seconds. Adjust based on taste — if sour, grind finer; "
			+ "if bitter, grind coarser.";
	}

	public async Task<string> AnalyzeBrewingPattern(IEnumerable<ShotRecord> recentShots)
	{
		await Task.Delay(1000);

		var shots = recentShots.ToList();

		if (shots.Count == 0)
		{
			return "No shots recorded yet. Start logging your shots to receive "
				+ "personalized brewing insights and pattern analysis.";
		}

		var ratedShots = shots.Where(s => s.Rating.HasValue).ToList();
		if (ratedShots.Count == 0)
		{
			return $"You've logged {shots.Count} shot(s) but none have ratings yet. "
				+ "Rate your shots to unlock trend analysis and improvement suggestions.";
		}

		var avgRating = ratedShots.Average(s => s.Rating!.Value);

		return $"Your average rating across {ratedShots.Count} rated shot(s) is {avgRating:F1}/5. "
			+ "Your ratings have been improving over recent sessions. Keep experimenting "
			+ "with small grind adjustments to continue the upward trend.";
	}
}
