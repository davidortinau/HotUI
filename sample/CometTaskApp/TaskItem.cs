namespace CometTaskApp;

public enum TaskPriority
{
	Low,
	Medium,
	High,
	Critical
}

public enum TaskCategory
{
	Personal,
	Work,
	Shopping,
	Health,
	Learning,
	Other
}

public class TaskItem : Comet.BindingObject
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Title
	{
		get => GetProperty<string>() ?? "";
		set => SetProperty(value);
	}
	public string Description
	{
		get => GetProperty<string>() ?? "";
		set => SetProperty(value);
	}
	public bool IsCompleted
	{
		get => GetProperty<bool>();
		set => SetProperty(value);
	}
	public TaskPriority Priority
	{
		get => GetProperty<TaskPriority>();
		set => SetProperty(value);
	}
	public TaskCategory Category
	{
		get => GetProperty<TaskCategory>();
		set => SetProperty(value);
	}
	public DateTime CreatedAt { get; set; } = DateTime.Now;
	public DateTime? DueDate
	{
		get => GetProperty<DateTime?>();
		set => SetProperty(value);
	}
}
