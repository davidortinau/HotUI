using System;
using System.Collections.Generic;
using System.Linq;
using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Comet.Layout
{
	public class FlexLayoutManager : ILayoutManager
	{
		private readonly FlexLayout flexLayout;

		public FlexLayoutManager(FlexLayout layout)
		{
			this.flexLayout = layout;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			var isRow = flexLayout.Direction == FlexDirection.Row || flexLayout.Direction == FlexDirection.RowReverse;
			var mainAxisConstraint = isRow ? widthConstraint : heightConstraint;
			var crossAxisConstraint = isRow ? heightConstraint : widthConstraint;

			double mainAxisSize = 0;
			double crossAxisSize = 0;
			double currentLineMainSize = 0;
			double currentLineCrossSize = 0;

			var lines = GetFlexLines(mainAxisConstraint, crossAxisConstraint);

			foreach (var line in lines)
			{
				var lineMainSize = line.Sum(item => item.MainSize);
				var lineCrossSize = line.Any() ? line.Max(item => item.CrossSize) : 0;

				mainAxisSize = Math.Max(mainAxisSize, lineMainSize);
				crossAxisSize += lineCrossSize;
			}

			if (flexLayout.Wrap == FlexWrap.NoWrap)
				crossAxisSize = currentLineCrossSize;

			return isRow 
				? new Size(double.IsInfinity(widthConstraint) ? mainAxisSize : widthConstraint,
				          double.IsInfinity(heightConstraint) ? crossAxisSize : heightConstraint)
				: new Size(double.IsInfinity(widthConstraint) ? crossAxisSize : widthConstraint,
				          double.IsInfinity(heightConstraint) ? mainAxisSize : heightConstraint);
		}

		public Size ArrangeChildren(Rect bounds)
		{
			var isRow = flexLayout.Direction == FlexDirection.Row || flexLayout.Direction == FlexDirection.RowReverse;
			var isReverse = flexLayout.Direction == FlexDirection.RowReverse || flexLayout.Direction == FlexDirection.ColumnReverse;
			var mainAxisSize = isRow ? bounds.Width : bounds.Height;
			var crossAxisSize = isRow ? bounds.Height : bounds.Width;

			var lines = GetFlexLines(mainAxisSize, crossAxisSize);

			double crossAxisPosition = bounds.Y;

			foreach (var line in lines)
			{
				var lineCrossSize = line.Any() ? line.Max(item => item.CrossSize) : 0;
				var totalMainSize = line.Sum(item => item.MainSize);
				var totalGrow = line.Sum(item => item.Grow);
				var freeSpace = mainAxisSize - totalMainSize;

				// Distribute free space
				if (freeSpace > 0 && totalGrow > 0)
				{
					foreach (var item in line)
					{
						if (item.Grow > 0)
							item.MainSize += freeSpace * (item.Grow / totalGrow);
					}
				}

				// Position items on main axis
				double mainAxisPosition = CalculateMainAxisStart(freeSpace, line.Count);

				var sortedLine = isReverse ? line.AsEnumerable().Reverse() : line;

				foreach (var item in sortedLine)
				{
					var alignSelf = item.AlignSelf != FlexAlignSelf.Auto ? item.AlignSelf : (FlexAlignSelf)(int)flexLayout.AlignItems;
					var crossPosition = CalculateCrossAxisPosition(alignSelf, item.CrossSize, lineCrossSize);

					var x = isRow ? bounds.X + mainAxisPosition : bounds.X + crossAxisPosition + crossAxisPosition;
					var y = isRow ? crossAxisPosition + crossAxisPosition : bounds.Y + mainAxisPosition;
					var width = isRow ? item.MainSize : item.CrossSize;
					var height = isRow ? item.CrossSize : item.MainSize;

					var finalBounds = new Rect(x, y, width, height);

					if (item.View is View cv)
						cv.LayoutSubviews(finalBounds);
					else
						item.View.Arrange(finalBounds);

					mainAxisPosition += item.MainSize + CalculateItemSpacing(freeSpace, line.Count);
				}

				crossAxisPosition += lineCrossSize;
			}

			return bounds.Size;
		}

		private List<List<FlexItem>> GetFlexLines(double mainAxisConstraint, double crossAxisConstraint)
		{
			var lines = new List<List<FlexItem>>();
			var currentLine = new List<FlexItem>();
			double currentLineMainSize = 0;

			var isRow = flexLayout.Direction == FlexDirection.Row || flexLayout.Direction == FlexDirection.RowReverse;

			foreach (var view in flexLayout)
			{
				if (view is not View cometView)
					continue;

				var measured = view.Measure(mainAxisConstraint, crossAxisConstraint);
				var basis = cometView.GetFlexBasis();
				var grow = cometView.GetFlexGrow();
				var shrink = cometView.GetFlexShrink();
				var alignSelf = cometView.GetFlexAlignSelf();

				var mainSize = basis >= 0 ? basis : (isRow ? measured.Width : measured.Height);
				var crossSize = isRow ? measured.Height : measured.Width;

				var item = new FlexItem
				{
					View = view,
					MainSize = mainSize,
					CrossSize = crossSize,
					Grow = grow,
					Shrink = shrink,
					AlignSelf = alignSelf
				};

				if (flexLayout.Wrap != FlexWrap.NoWrap && currentLineMainSize + mainSize > mainAxisConstraint && currentLine.Count > 0)
				{
					lines.Add(currentLine);
					currentLine = new List<FlexItem> { item };
					currentLineMainSize = mainSize;
				}
				else
				{
					currentLine.Add(item);
					currentLineMainSize += mainSize;
				}
			}

			if (currentLine.Count > 0)
				lines.Add(currentLine);

			return lines;
		}

		private double CalculateMainAxisStart(double freeSpace, int itemCount)
		{
			return flexLayout.JustifyContent switch
			{
				FlexJustify.Center => freeSpace / 2,
				FlexJustify.End => freeSpace,
				FlexJustify.SpaceAround => freeSpace / (itemCount * 2),
				FlexJustify.SpaceEvenly => freeSpace / (itemCount + 1),
				_ => 0
			};
		}

		private double CalculateItemSpacing(double freeSpace, int itemCount)
		{
			if (itemCount <= 1)
				return 0;

			return flexLayout.JustifyContent switch
			{
				FlexJustify.SpaceBetween => freeSpace / (itemCount - 1),
				FlexJustify.SpaceAround => freeSpace / itemCount,
				FlexJustify.SpaceEvenly => freeSpace / (itemCount + 1),
				_ => 0
			};
		}

		private double CalculateCrossAxisPosition(FlexAlignSelf alignSelf, double itemSize, double lineSize)
		{
			return alignSelf switch
			{
				FlexAlignSelf.Center => (lineSize - itemSize) / 2,
				FlexAlignSelf.End => lineSize - itemSize,
				_ => 0
			};
		}

		private class FlexItem
		{
			public IView View { get; set; }
			public double MainSize { get; set; }
			public double CrossSize { get; set; }
			public double Grow { get; set; }
			public double Shrink { get; set; }
			public FlexAlignSelf AlignSelf { get; set; }
		}
	}
}
