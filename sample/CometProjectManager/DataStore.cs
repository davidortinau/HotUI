using CometProjectManager.Models;

namespace CometProjectManager;

/// <summary>
/// In-memory data store replacing the template's repository + seed data service.
/// All state changes trigger reactive UI updates via State<T>.
/// </summary>
public class DataStore
{
	public static DataStore Instance { get; } = new();

	private int _nextProjectId = 4;
	private int _nextTaskId = 10;
	private int _nextCategoryId = 5;
	private int _nextTagId = 5;

	public readonly State<List<Category>> Categories = new(new List<Category>
	{
		new() { ID = 1, Title = "Development", ColorHex = "#512BD4" },
		new() { ID = 2, Title = "Design", ColorHex = "#E91E63" },
		new() { ID = 3, Title = "Marketing", ColorHex = "#FF9800" },
		new() { ID = 4, Title = "Research", ColorHex = "#4CAF50" },
	});

	public readonly State<List<Tag>> Tags = new(new List<Tag>
	{
		new() { ID = 1, Title = "Urgent", ColorHex = "#F44336" },
		new() { ID = 2, Title = "Bug", ColorHex = "#E91E63" },
		new() { ID = 3, Title = "Feature", ColorHex = "#2196F3" },
		new() { ID = 4, Title = "Docs", ColorHex = "#4CAF50" },
	});

	public readonly State<List<Project>> Projects;
	public readonly State<List<ProjectTask>> AllTasks;

	public DataStore()
	{
		var tasks = new List<ProjectTask>
		{
			new() { ID = 1, Title = "Set up CI/CD pipeline", IsCompleted = true, ProjectID = 1 },
			new() { ID = 2, Title = "Implement authentication", IsCompleted = false, ProjectID = 1 },
			new() { ID = 3, Title = "Write unit tests", IsCompleted = false, ProjectID = 1 },
			new() { ID = 4, Title = "Create wireframes", IsCompleted = true, ProjectID = 2 },
			new() { ID = 5, Title = "Design landing page", IsCompleted = false, ProjectID = 2 },
			new() { ID = 6, Title = "Write blog post", IsCompleted = false, ProjectID = 3 },
			new() { ID = 7, Title = "Social media campaign", IsCompleted = true, ProjectID = 3 },
			new() { ID = 8, Title = "Prepare demo", IsCompleted = false, ProjectID = 1 },
			new() { ID = 9, Title = "Review PRs", IsCompleted = false, ProjectID = 1 },
		};

		AllTasks = new State<List<ProjectTask>>(tasks);

		var projects = new List<Project>
		{
			new() { ID = 1, Name = "Mobile App", Description = "Cross-platform mobile application", Icon = "📱", CategoryID = 1,
				Tags = new() { new() { ID = 1, Title = "Urgent", ColorHex = "#F44336" }, new() { ID = 3, Title = "Feature", ColorHex = "#2196F3" } } },
			new() { ID = 2, Name = "Website Redesign", Description = "Refresh the company website", Icon = "🌐", CategoryID = 2,
				Tags = new() { new() { ID = 3, Title = "Feature", ColorHex = "#2196F3" } } },
			new() { ID = 3, Name = "Product Launch", Description = "Q2 product launch campaign", Icon = "🚀", CategoryID = 3,
				Tags = new() { new() { ID = 1, Title = "Urgent", ColorHex = "#F44336" }, new() { ID = 4, Title = "Docs", ColorHex = "#4CAF50" } } },
		};

		// Link tasks to projects
		foreach (var project in projects)
			project.Tasks = tasks.Where(t => t.ProjectID == project.ID).ToList();

		Projects = new State<List<Project>>(projects);
	}

	public string Today => DateTime.Now.ToString("dddd, MMM d");

	public List<CategoryChartData> GetCategoryChartData()
	{
		var categories = Categories.Value ?? new();
		var projects = Projects.Value ?? new();
		return categories.Select(c =>
		{
			var taskCount = projects.Where(p => p.CategoryID == c.ID).SelectMany(p => p.Tasks).Count();
			return new CategoryChartData(c.Title, taskCount, c.Color);
		}).ToList();
	}

	public void ToggleTaskComplete(int taskId)
	{
		var tasks = new List<ProjectTask>(AllTasks.Value!);
		var task = tasks.FirstOrDefault(t => t.ID == taskId);
		if (task != null)
		{
			task.IsCompleted = !task.IsCompleted;
			AllTasks.Value = tasks;
			RefreshProjects();
		}
	}

	public void AddTask(ProjectTask task)
	{
		task.ID = _nextTaskId++;
		var tasks = new List<ProjectTask>(AllTasks.Value!) { task };
		AllTasks.Value = tasks;
		RefreshProjects();
	}

	public void DeleteTask(int taskId)
	{
		var tasks = new List<ProjectTask>(AllTasks.Value!);
		tasks.RemoveAll(t => t.ID == taskId);
		AllTasks.Value = tasks;
		RefreshProjects();
	}

	public void CleanCompletedTasks()
	{
		var tasks = new List<ProjectTask>(AllTasks.Value!);
		tasks.RemoveAll(t => t.IsCompleted);
		AllTasks.Value = tasks;
		RefreshProjects();
	}

	public void AddProject(Project project)
	{
		project.ID = _nextProjectId++;
		var projects = new List<Project>(Projects.Value!) { project };
		Projects.Value = projects;
	}

	public void SaveProject(Project project)
	{
		var projects = new List<Project>(Projects.Value!);
		var idx = projects.FindIndex(p => p.ID == project.ID);
		if (idx >= 0)
			projects[idx] = project;
		Projects.Value = projects;
	}

	public void DeleteProject(int projectId)
	{
		var projects = new List<Project>(Projects.Value!);
		projects.RemoveAll(p => p.ID == projectId);
		Projects.Value = projects;

		var tasks = new List<ProjectTask>(AllTasks.Value!);
		tasks.RemoveAll(t => t.ProjectID == projectId);
		AllTasks.Value = tasks;
	}

	public void AddCategory(Category category)
	{
		category.ID = _nextCategoryId++;
		Categories.Value = new List<Category>(Categories.Value!) { category };
	}

	public void DeleteCategory(int categoryId)
	{
		var cats = new List<Category>(Categories.Value!);
		cats.RemoveAll(c => c.ID == categoryId);
		Categories.Value = cats;
	}

	public void AddTag(Tag tag)
	{
		tag.ID = _nextTagId++;
		Tags.Value = new List<Tag>(Tags.Value!) { tag };
	}

	public void DeleteTag(int tagId)
	{
		var tags = new List<Tag>(Tags.Value!);
		tags.RemoveAll(t => t.ID == tagId);
		Tags.Value = tags;
	}

	private void RefreshProjects()
	{
		var projects = new List<Project>(Projects.Value!);
		var tasks = AllTasks.Value ?? new();
		foreach (var project in projects)
			project.Tasks = tasks.Where(t => t.ProjectID == project.ID).ToList();
		Projects.Value = projects;
	}
}
