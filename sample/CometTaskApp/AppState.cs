using Comet;

namespace CometTaskApp;

/// <summary>
/// Shared app state accessible across all pages.
/// Uses State<T> for reactive MVU updates.
/// </summary>
public class AppState
{
	public static AppState Instance { get; } = new();

	public readonly State<List<TaskItem>> Tasks = new(new List<TaskItem>
	{
		new() { Title = "Buy groceries", Description = "Milk, eggs, bread, cheese", Priority = TaskPriority.Medium, Category = TaskCategory.Shopping },
		new() { Title = "Finish project report", Description = "Q4 summary with charts", Priority = TaskPriority.High, Category = TaskCategory.Work },
		new() { Title = "Go for a run", Description = "30 minutes in the park", Priority = TaskPriority.Low, Category = TaskCategory.Health },
		new() { Title = "Read Comet docs", Description = "Learn MVU patterns for MAUI", Priority = TaskPriority.High, Category = TaskCategory.Learning },
		new() { Title = "Call dentist", Description = "Schedule annual checkup", Priority = TaskPriority.Medium, Category = TaskCategory.Personal },
		new() { Title = "Fix kitchen faucet", Description = "Leaking when turned off", Priority = TaskPriority.Critical, Category = TaskCategory.Personal },
	});

	public readonly State<TaskCategory?> FilterCategory = new(null);
	public readonly State<bool> ShowCompletedTasks = new(true);
	public readonly State<string> SearchText = new("");
	public readonly State<bool> DarkMode = new(false);

	public void AddTask(TaskItem task)
	{
		var list = new List<TaskItem>(Tasks.Value!) { task };
		Tasks.Value = list;
	}

	public void RemoveTask(string id)
	{
		var list = new List<TaskItem>(Tasks.Value!);
		list.RemoveAll(t => t.Id == id);
		Tasks.Value = list;
	}

	public void ToggleComplete(string id)
	{
		var list = new List<TaskItem>(Tasks.Value!);
		var task = list.FirstOrDefault(t => t.Id == id);
		if (task != null)
		{
			task.IsCompleted = !task.IsCompleted;
			Tasks.Value = list;
		}
	}

	public List<TaskItem> GetFilteredTasks()
	{
		var tasks = Tasks.Value ?? new List<TaskItem>();
		var search = SearchText.Value ?? "";

		return tasks
			.Where(t => ShowCompletedTasks.Value || !t.IsCompleted)
			.Where(t => FilterCategory.Value == null || t.Category == FilterCategory.Value)
			.Where(t => string.IsNullOrEmpty(search) ||
				t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
				t.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
			.OrderByDescending(t => t.Priority)
			.ThenBy(t => t.IsCompleted)
			.ToList();
	}

	public int CompletedCount => Tasks.Value?.Count(t => t.IsCompleted) ?? 0;
	public int TotalCount => Tasks.Value?.Count ?? 0;
	public int PendingCount => TotalCount - CompletedCount;
}
