using System;
using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using static Comet.CometControls;

namespace CometControlsGallery.Pages
{
	public class TransformsPageState
	{
		public string StatusText { get; set; } = "Use the buttons below to animate the box";
	}

	public class TransformsPage : Component<TransformsPageState>
	{
		Border? targetBox;
		Border? anchorBox;
		Border? compositeBox;
		double anchorX = 0.5;
		double anchorY = 0.5;
		string anchorLabel = "Anchor: (0.5, 0.5) -- center";

		public override View Render()
		{
			targetBox = Border(new Spacer())
				.Background(Colors.DodgerBlue)
				.CornerRadius(8)
				.StrokeThickness(0)
				.Frame(width: 100, height: 100)
				.HorizontalLayoutAlignment(LayoutAlignment.Center);

			var statusLabel = Text(State.StatusText)
				.FontSize(14)
				.Color(Colors.Grey)
				.HorizontalLayoutAlignment(LayoutAlignment.Center);

			var translateBtn = Button("TranslateTo (100, 0)", () =>
			{
				SetState(s => s.StatusText = "TranslateTo...");
				targetBox?.TranslateTo(100, 0, 0.5, Easing.CubicInOut);
				SetState(s => s.StatusText = "TranslateTo complete");
			});

			var scaleBtn = Button("ScaleTo 1.5x", () =>
			{
				SetState(s => s.StatusText = "ScaleTo...");
				targetBox?.ScaleTo(scale: 1.5, duration: 0.5, easing: Easing.CubicInOut);
				SetState(s => s.StatusText = "ScaleTo complete");
			});

			var rotateBtn = Button("RotateTo 90", () =>
			{
				SetState(s => s.StatusText = "RotateTo...");
				targetBox?.RotateTo(90, 0.5, Easing.CubicInOut);
				SetState(s => s.StatusText = "RotateTo complete");
			});

			var fadeBtn = Button("FadeTo 0.3", () =>
			{
				SetState(s => s.StatusText = "FadeTo...");
				targetBox?.FadeTo(0.3, 0.5, Easing.CubicInOut);
				SetState(s => s.StatusText = "FadeTo complete");
			});

			var resetBtn = Button("Reset All", () =>
			{
				SetState(s => s.StatusText = "Resetting...");
				targetBox?.TranslateTo(0, 0, 0.3);
				targetBox?.ScaleTo(scale: 1, duration: 0.3);
				targetBox?.RotateTo(0, 0.3);
				targetBox?.FadeTo(1, 0.3);
				SetState(s => s.StatusText = "Reset complete");
			}).Background(Colors.Crimson).Color(Colors.White);

			anchorBox = Border(new Spacer())
				.Background(Colors.MediumPurple)
				.CornerRadius(8)
				.StrokeThickness(0)
				.Frame(width: 80, height: 80)
				.HorizontalLayoutAlignment(LayoutAlignment.Center);

			var anchorLabelView = GalleryPageHelpers.BodyText(anchorLabel);

			var anchorTopLeftBtn = Button("Anchor (0, 0)", () =>
			{
				anchorX = 0;
				anchorY = 0;
				anchorLabel = "Anchor: (0, 0) -- top-left";
				anchorBox?.AnchorX(0);
				anchorBox?.AnchorY(0);
				anchorBox?.RotateTo(360, 0.8);
				SetState(s => { });
			});

			var anchorCenterBtn = Button("Anchor (0.5, 0.5)", () =>
			{
				anchorX = 0.5;
				anchorY = 0.5;
				anchorLabel = "Anchor: (0.5, 0.5) -- center";
				anchorBox?.AnchorX(0.5);
				anchorBox?.AnchorY(0.5);
				anchorBox?.RotateTo(360, 0.8);
				SetState(s => { });
			});

			var anchorBottomRightBtn = Button("Anchor (1, 1)", () =>
			{
				anchorX = 1;
				anchorY = 1;
				anchorLabel = "Anchor: (1, 1) -- bottom-right";
				anchorBox?.AnchorX(1);
				anchorBox?.AnchorY(1);
				anchorBox?.RotateTo(360, 0.8);
				SetState(s => { });
			});

			compositeBox = Border(new Spacer())
				.Background(Colors.Coral)
				.CornerRadius(8)
				.StrokeThickness(0)
				.Frame(width: 80, height: 80)
				.HorizontalLayoutAlignment(LayoutAlignment.Center);

			var compositeBtn = Button("Run Composite Animation", () =>
			{
				SetState(s => s.StatusText = "Composite animation running...");
				compositeBox?.Animate(v =>
				{
					v.TranslationX(80);
					v.ScaleX(1.5);
					v.ScaleY(1.5);
					v.Rotation(180);
					v.Opacity(0.4);
				}, duration: 0.6, autoReverses: true);
				SetState(s => s.StatusText = "Composite animation complete");
			});

			return GalleryPageHelpers.Scaffold("Transforms",
				statusLabel,
				GalleryPageHelpers.Section("Basic Transforms",
					targetBox,
					HStack(8,
						translateBtn,
						scaleBtn,
						rotateBtn,
						fadeBtn
					).HorizontalLayoutAlignment(LayoutAlignment.Center),
					resetBtn
				),
				GalleryPageHelpers.Section("AnchorX / AnchorY",
					anchorLabelView,
					anchorBox,
					HStack(8,
						anchorTopLeftBtn,
						anchorCenterBtn,
						anchorBottomRightBtn
					).HorizontalLayoutAlignment(LayoutAlignment.Center)
				),
				GalleryPageHelpers.Section("Composite Animation",
					GalleryPageHelpers.BodyText("Translate + Scale + Rotate + Fade simultaneously")
						.Color(Colors.Grey),
					compositeBox,
					compositeBtn
				)
			);
		}
	}
}
