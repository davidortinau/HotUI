namespace CometStressTest.Pages;

public class ListTestPage : View
{
	static readonly Color[] IndicatorColors = new[]
	{
		Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple,
		Colors.Teal, Colors.Brown, Colors.Magenta, Colors.Cyan, Colors.Gold,
	};

	readonly State<string> statusText = new State<string>("No item selected");
	readonly ObservableCollection<string> items;

	public ListTestPage()
	{
		items = new ObservableCollection<string>(
			Enumerable.Range(1, 50).Select(i => $"Item {i}")
		);
	}

	[Body]
	View body()
	{
		var list = new ListView<string>(() => items.ToList())
		{
			ViewFor = item =>
			{
				var index = items.IndexOf(item);
				var color = IndicatorColors[index % IndicatorColors.Length];
				return new HStack(spacing: 10)
				{
					new ShapeView(new Circle())
						.Frame(width: 12, height: 12)
						.Background(new SolidPaint(color)),
					new VStack(LayoutAlignment.Start, spacing: 2)
					{
						new Text(item)
							.FontSize(16),
						new Text($"Subtitle for {item}")
							.FontSize(12)
							.Color(Colors.Gray),
					},
				}.Padding(8);
			},
			Header = new Text("📋 List Stress Test (50 Items)")
				.FontSize(20)
				.Padding(12)
				.Background(new SolidPaint(Colors.LightGray)),
			Footer = new Text("— End of list —")
				.FontSize(14)
				.Padding(12)
				.Color(Colors.Gray),
			ItemSelected = selection =>
			{
				statusText.Value = $"Selected: {selection.item} (row {selection.row})";
			},
		};

		return new VStack
		{
			new Text(() => $"{statusText.Value}")
				.FontSize(14)
				.Padding(8)
				.Background(new SolidPaint(Colors.LightYellow)),
			new Button("Add Item", () =>
			{
				items.Add($"Item {items.Count + 1} (dynamic)");
				statusText.Value = $"Added item #{items.Count}";
			}).Padding(8),
			list,
		};
	}
}
