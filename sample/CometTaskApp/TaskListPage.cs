namespace CometTaskApp;

public class TaskListPageState { }

/// <summary>
/// Main task list with search, filtering, gestures, and navigation.
/// Exercises: CollectionView, state binding, typed navigation, gestures, NavigationView, conditional rendering.
/// </summary>
public class TaskListPage : Component<TaskListPageState>
{
[State] readonly AppState _state = AppState.Instance;

static Color PriorityColor(TaskPriority p) => p switch
{
TaskPriority.Critical => Colors.Red,
TaskPriority.High => Colors.OrangeRed,
TaskPriority.Medium => Colors.Orange,
TaskPriority.Low => Colors.Green,
_ => Colors.Gray
};

static string CategoryEmoji(TaskCategory c) => c switch
{
TaskCategory.Personal => "🏠",
TaskCategory.Work => "💼",
TaskCategory.Shopping => "🛒",
TaskCategory.Health => "💪",
TaskCategory.Learning => "📚",
TaskCategory.Other => "📌",
_ => "📌"
};

View TaskRow(TaskItem task)
{
var row = HStack(10,
new ShapeView(new Circle())
.Frame(12, 12)
.Background(new SolidPaint(PriorityColor(task.Priority))),
VStack(2,
Text(() => $"{CategoryEmoji(task.Category)} {task.Title}")
.FontSize(16)
.FontWeight(() => task.IsCompleted ? FontWeight.Regular : FontWeight.Semibold)
.Color(() => task.IsCompleted ? Colors.Gray : Colors.Black),
Text(() => task.Description)
.FontSize(13)
.Color(Colors.DarkGray)
),
Spacer(),
Text(task.IsCompleted ? "✅" : "⬜")
.FontSize(20)
.OnTap(_ => _state.ToggleComplete(task.Id))
)
.Padding(new Thickness(12, 8))
.SemanticDescription($"Task: {task.Title}, Priority: {task.Priority}, {(task.IsCompleted ? "Completed" : "Pending")}")
.OnTap(_ => Navigation?.Navigate<TaskDetailPage>(new TaskDetailProps
{
TaskId = task.Id,
}));

row.SetAutomationId($"TaskRow-{task.Id}");
return row;
}

public override View Render() =>
NavigationView(
VStack(
TextField(_state.SearchText, "Search tasks...")
.Padding(new Thickness(12, 8))
.SemanticDescription("Search tasks")
.AutomationId("TaskSearchField"),

ScrollView(Orientation.Horizontal,
HStack(8,
FilterPill("All", null),
FilterPill("🏠 Personal", TaskCategory.Personal),
FilterPill("💼 Work", TaskCategory.Work),
FilterPill("🛒 Shopping", TaskCategory.Shopping),
FilterPill("💪 Health", TaskCategory.Health),
FilterPill("📚 Learning", TaskCategory.Learning)
)
.Padding(new Thickness(12, 4))
),

HStack(16,
Text(() => $"📋 {_state.TotalCount} total")
.FontSize(12).Color(Colors.DarkGray),
Text(() => $"✅ {_state.CompletedCount} done")
.FontSize(12).Color(Colors.Green),
Text(() => $"⏳ {_state.PendingCount} pending")
.FontSize(12).Color(Colors.Orange),
Spacer()
)
.Padding(new Thickness(12, 4)),

new CollectionView<TaskItem>(() => _state.GetFilteredTasks())
{
ViewFor = TaskRow,
SelectionMode = SelectionMode.None,
}
.SemanticDescription("Task list")
.AutomationId("TaskList"),

Button("+ Add Task", () =>
{
Navigation?.Navigate(new AddTaskPage());
})
.Padding(new Thickness(16, 12))
.SemanticDescription("Add a new task")
.AutomationId("AddTaskButton")
)
)
.Title("My Tasks");

View FilterPill(string label, TaskCategory? category)
{
var isSelected = _state.FilterCategory.Value == category;
return Text(label)
.FontSize(13)
.Color(isSelected ? Colors.White : Colors.DarkGray)
.Background(new SolidPaint(isSelected ? Colors.DodgerBlue : Colors.LightGray))
.Padding(new Thickness(10, 6))
.ClipShape(new RoundedRectangle(12))
.OnTap(_ => _state.FilterCategory.Value = category);
}
}
