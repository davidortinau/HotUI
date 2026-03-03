namespace CometAllTheLists.Pages;

public class CollectionViewPage : View
{
	readonly State<List<string>> items;

	public CollectionViewPage()
	{
		items = new State<List<string>>(
			Enumerable.Range(1, 30).Select(i => $"Item {i}").ToList()
		);
	}

	[Body]
	View body()
	{
		return new VStack
		{
			new Text("📦 Collections")
				.FontSize(24)
				.FontWeight(FontWeight.Bold)
				.Padding(16),
			
			new VStack(spacing: 16)
			{
				new Text("Vertical Collection")
					.FontSize(16)
					.FontWeight(FontWeight.Bold)
					.Padding(new Thickness(12, 0)),
				
				new CollectionView<string>(() => items.Value)
				{
					ViewFor = item => RenderVerticalItem(item),
					ItemsLayout = ItemsLayout.Vertical(spacing: 8),
				}.Frame(height: 250),

				new Text("Horizontal Collection")
					.FontSize(16)
					.FontWeight(FontWeight.Bold)
					.Padding(new Thickness(12, 0)),
				
				new CollectionView<string>(() => items.Value.Take(10).ToList())
				{
					ViewFor = item => RenderHorizontalItem(item),
					ItemsLayout = ItemsLayout.Horizontal(spacing: 8),
				}.Frame(height: 120),

				new Text("Grid Collection (2 columns)")
					.FontSize(16)
					.FontWeight(FontWeight.Bold)
					.Padding(new Thickness(12, 0)),
				
				new CollectionView<string>(() => items.Value.Take(12).ToList())
				{
					ViewFor = item => RenderGridItem(item),
					ItemsLayout = GridItemsLayout.Vertical(span: 2, spacing: 8),
				}.Frame(height: 280),
			}
			.Padding(8),
		};
	}

	View RenderVerticalItem(string item)
	{
		return new VStack(spacing: 4)
		{
			new HStack(spacing: 8)
			{
				new ShapeView(new Circle())
					.Frame(width: 10, height: 10)
					.Background(new SolidPaint(Colors.Blue)),
				new Text(item).FontSize(14),
				new Spacer(),
				new Text("→").FontSize(12).Color(Colors.Gray),
			},
		}
		.Padding(12)
		.Background(new SolidPaint(Color.FromArgb("#F5F5F5")));
	}

	View RenderHorizontalItem(string item)
	{
		return new VStack(spacing: 4)
		{
			new ShapeView(new Circle())
				.Frame(width: 30, height: 30)
				.Background(new SolidPaint(Colors.Cyan)),
			new Text(item).FontSize(10),
		}
		.Padding(8)
		.Background(new SolidPaint(Colors.White));
	}

	View RenderGridItem(string item)
	{
		return new VStack
		{
			new ShapeView(new Rectangle())
				.Frame(height: 100)
				.Background(new SolidPaint(Colors.Purple)),
			new Text(item)
				.FontSize(12)
				.Padding(6),
		}
		.Background(new SolidPaint(Colors.White));
	}
}
