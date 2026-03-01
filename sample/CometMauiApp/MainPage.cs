using System;
using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace CometMauiApp
{
	public class MainPage : View
	{
		readonly State<int> count = new State<int>(0);

		[Body]
		View body() =>
			new ScrollView
			{
				new VStack(spacing: 25)
				{
					new Image("dotnet_bot.png")
						.Frame(height: 185)
						.SemanticDescription("dot net bot in a hovercraft number nine"),

					new Text("Hello, World!")
						.FontSize(32)
						.FontWeight(FontWeight.Bold)
						.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

					new Text("Welcome to \n.NET Multi-platform App UI")
						.FontSize(18)
						.SemanticHeadingLevel(SemanticHeadingLevel.Level2)
						.SemanticDescription("Welcome to dot net Multi platform App U I"),

					new Button(() => count.Value == 0
							? "Click me"
							: count.Value == 1
								? $"Clicked {count.Value} time"
								: $"Clicked {count.Value} times",
						() => { count.Value++; })
						.SemanticHint("Counts the number of times you click")
				}
				.Padding(new Thickness(30, 0))
			};
	}
}
