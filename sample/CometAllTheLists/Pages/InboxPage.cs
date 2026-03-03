namespace CometAllTheLists.Pages;

public class Message
{
	public string From { get; set; } = "";
	public string Subject { get; set; } = "";
	public string Preview { get; set; } = "";
	public DateTime Date { get; set; }
	public bool IsUnread { get; set; }
}

public class InboxPage : View
{
	readonly State<List<Message>> inbox;
	readonly State<string> selectedMessage;

	public InboxPage()
	{
		selectedMessage = new State<string>("");
		inbox = new State<List<Message>>(new List<Message>
		{
			new() { From = "Alice Johnson", Subject = "Project Update", Preview = "Here's the latest status on the Q2 project...", Date = DateTime.Now.AddHours(-2), IsUnread = true },
			new() { From = "Bob Smith", Subject = "Meeting Tomorrow", Preview = "Let me know if you're available at 2pm...", Date = DateTime.Now.AddHours(-5), IsUnread = true },
			new() { From = "Carol Davis", Subject = "Expense Report", Preview = "Please review the attached expense report...", Date = DateTime.Now.AddHours(-8), IsUnread = false },
			new() { From = "David Wilson", Subject = "Feedback on Design", Preview = "Great work on the mockups! A few suggestions...", Date = DateTime.Now.AddHours(-12), IsUnread = false },
			new() { From = "Eve Martinez", Subject = "Team Lunch Friday", Preview = "Are you interested in team lunch this Friday?...", Date = DateTime.Now.AddDays(-1), IsUnread = false },
			new() { From = "Frank Brown", Subject = "Code Review", Preview = "Please review PR #1234 when you have time...", Date = DateTime.Now.AddDays(-1), IsUnread = false },
		});
	}

	[Body]
	View body()
	{
		return new VStack
		{
			new Text("📧 Inbox")
				.FontSize(24)
				.FontWeight(FontWeight.Bold)
				.Padding(16),
			
			new Text(() => $"Selected: {selectedMessage.Value}")
				.FontSize(12)
				.Color(Colors.Gray)
				.Padding(8),
			
			new ListView<Message>(() => inbox.Value)
			{
				ViewFor = msg => RenderMessageItem(msg),
				ItemSelected = selection =>
				{
					selectedMessage.Value = ((Message)selection.item).From;
				},
				Header = new Text($"Messages ({inbox.Value.Count})")
					.FontSize(14)
					.Padding(12)
					.Background(new SolidPaint(Colors.LightGray)),
			}.Padding(8),
		};
	}

	View RenderMessageItem(Message msg)
	{
		return new VStack(spacing: 2)
		{
			new HStack(spacing: 8)
			{
				new VStack(spacing: 4)
				{
					new HStack(spacing: 6)
					{
						new Text(msg.From)
							.FontSize(14)
							.FontWeight(FontWeight.Bold),
						msg.IsUnread ? new ShapeView(new Circle())
							.Frame(width: 8, height: 8)
							.Background(new SolidPaint(Colors.Blue))
							: new Text("")
					},
					new Text(msg.Subject)
						.FontSize(12)
						.Color(Colors.Gray),
					new Text(msg.Preview)
						.FontSize(11)
						.Color(Colors.DarkGray),
				},
				new Spacer(),
				new Text(FormatDate(msg.Date))
					.FontSize(10)
					.Color(Colors.Gray),
			},
		}
		.Padding(12)
		.Background(new SolidPaint(msg.IsUnread ? Color.FromArgb("#F0F8FF") : Colors.White));
	}

	string FormatDate(DateTime date)
	{
		var diff = DateTime.Now - date;
		if (diff.TotalHours < 1) return "now";
		if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h";
		return $"{(int)diff.TotalDays}d";
	}
}
