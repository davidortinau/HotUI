namespace CometAllTheLists.Pages;

public class StreamingServicePage : View
{
	readonly State<List<string>> recommendedShows;
	readonly State<List<string>> newShows;
	readonly State<List<string>> actionShows;
	readonly State<List<string>> dramaShows;
	readonly State<List<string>> comedyShows;

	public StreamingServicePage()
	{
		recommendedShows = new State<List<string>>(new List<string>
		{
			"Breaking Bad", "The Crown", "Stranger Things", "The Mandalorian", "Succession", "Ozark"
		});

		newShows = new State<List<string>>(new List<string>
		{
			"Wednesday", "The Last of Us", "Beef", "The Bear", "Killers of the Flower Moon", "Emerald"
		});

		actionShows = new State<List<string>>(new List<string>
		{
			"John Wick", "Mission Impossible", "Fast & Furious", "James Bond", "Black Panther", "Dune"
		});

		dramaShows = new State<List<string>>(new List<string>
		{
			"Parasite", "Oppenheimer", "Barbie", "Poor Things", "Killers of the Flower Moon", "Anatomy of a Fall"
		});

		comedyShows = new State<List<string>>(new List<string>
		{
			"The Office", "Parks and Recreation", "Brooklyn Nine-Nine", "Schitt's Creek", "Always Sunny", "It's Always Sunny"
		});
	}

	[Body]
	View body()
	{
		return new VStack(spacing: 16)
		{
			new Text("🎬 Streaming Service")
				.FontSize(24)
				.FontWeight(FontWeight.Bold)
				.Padding(16),

			RenderCollectionSection("Recommended For You", recommendedShows.Value),
			RenderCollectionSection("Newly Added", newShows.Value),
			RenderCollectionSection("Action", actionShows.Value),
			RenderCollectionSection("Drama", dramaShows.Value),
			RenderCollectionSection("Comedy", comedyShows.Value),
		}
		.Padding(8);
	}

	View RenderCollectionSection(string title, List<string> shows)
	{
		return new VStack(spacing: 8)
		{
			new Text(title)
				.FontSize(16)
				.FontWeight(FontWeight.Bold)
				.Padding(new Thickness(12, 0)),
			
			new CollectionView<string>(() => shows)
			{
				ItemsLayout = ItemsLayout.Horizontal(spacing: 12),
				ViewFor = show => RenderShowCard(show),
			}.Frame(height: 140),
		};
	}

	View RenderShowCard(string showName)
	{
		return new VStack(spacing: 4)
		{
			new ShapeView(new RoundedRectangle(cornerRadius: 6))
				.Frame(width: 100, height: 100)
				.Background(new SolidPaint(GetRandomColor())),
			new Text(showName)
				.FontSize(10),
		}
		.Padding(4);
	}

	Color GetRandomColor()
	{
		var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Purple, Colors.Orange, Colors.Teal };
		return colors[DateTime.Now.Millisecond % colors.Length];
	}
}
