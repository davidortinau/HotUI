namespace CometAllTheLists.Pages;

public class Product
{
	public string Name { get; set; } = "";
	public string Category { get; set; } = "";
	public decimal Price { get; set; }
	public bool IsNew { get; set; }
	public int Quantity { get; set; }
	public bool IsLoading { get; set; }
}

public class ShoppingPage : View
{
	readonly State<List<Product>> products;

	public ShoppingPage()
	{
		products = new State<List<Product>>(new List<Product>
		{
			new() { Name = "Laptop", Category = "Electronics", Price = 999, IsNew = true, Quantity = 5 },
			new() { Name = "Mouse", Category = "Accessories", Price = 29, IsNew = false, Quantity = 50 },
			new() { Name = "Keyboard", Category = "Accessories", Price = 79, IsNew = true, Quantity = 25 },
			new() { Name = "Monitor", Category = "Electronics", Price = 299, IsNew = false, Quantity = 10 },
			new() { Name = "USB Cable", Category = "Cables", Price = 9, IsNew = false, Quantity = 100 },
			new() { Name = "Headphones", Category = "Audio", Price = 149, IsNew = true, Quantity = 15 },
			new() { Name = "Webcam", Category = "Electronics", Price = 79, IsNew = false, Quantity = 20 },
			new() { Name = "Desk Lamp", Category = "Furniture", Price = 59, IsNew = true, Quantity = 8 },
		});
	}

	[Body]
	View body()
	{
		return new VStack
		{
			new Text("🛍️ Shopping Products")
				.FontSize(24)
				.FontWeight(FontWeight.Bold)
				.Padding(16),
			new CollectionView<Product>(() => products.Value)
			{
				ViewFor = item => RenderProductItem(item),
				ItemsLayout = ItemsLayout.Vertical(spacing: 8),
			}.Padding(8),
		};
	}

	View RenderProductItem(Product item)
	{
		if (item.IsLoading)
		{
			return new VStack
			{
				new Text("Loading more..."),
			}
			.Padding(16)
			.Background(new SolidPaint(Colors.LightGray));
		}

		if (item.Price > 100)
		{
			return RenderPremiumItem(item);
		}

		return RenderStandardItem(item);
	}

	View RenderPremiumItem(Product item)
	{
		return new VStack(spacing: 6)
		{
			new HStack(spacing: 12)
			{
				new VStack(spacing: 4)
				{
					new Text(item.Name)
						.FontSize(16)
						.FontWeight(FontWeight.Bold),
					new Text(item.Category)
						.FontSize(12)
						.Color(Colors.Gray),
				},
				new Spacer(),
				new VStack(alignment: LayoutAlignment.End, spacing: 2)
				{
					new Text($"${item.Price}")
						.FontSize(18)
						.FontWeight(FontWeight.Bold)
						.Color(Colors.Green),
					item.IsNew ? new Text("NEW")
						.FontSize(10)
						.FontWeight(FontWeight.Bold)
						.Color(Colors.White)
						.Padding(new Thickness(4, 2))
						.Background(new SolidPaint(Colors.Orange))
						: new Text("")
				},
			},
			new Text($"In stock: {item.Quantity} units")
				.FontSize(11)
				.Color(Colors.Gray),
		}
		.Padding(12)
		.Background(new SolidPaint(Color.FromArgb("#F0F0F0")));
	}

	View RenderStandardItem(Product item)
	{
		return new HStack(spacing: 12)
		{
			new VStack(spacing: 4)
			{
				new Text(item.Name)
					.FontSize(14)
					.FontWeight(FontWeight.Bold),
				new Text($"${item.Price} • {item.Quantity} in stock")
					.FontSize(11)
					.Color(Colors.Gray),
			},
			new Spacer(),
			new Text($"${item.Price}")
				.FontSize(16)
				.FontWeight(FontWeight.Bold)
		}
		.Padding(10)
		.Background(new SolidPaint(Colors.White));
	}
}
