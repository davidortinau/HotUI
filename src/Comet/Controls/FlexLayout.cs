using Comet.Layout;
using Microsoft.Maui.Layouts;

namespace Comet
{
	public class FlexLayout : AbstractLayout
	{
		private readonly FlexDirection direction;
		private readonly FlexWrap wrap;
		private readonly FlexJustify justifyContent;
		private readonly FlexAlignItems alignItems;
		private readonly FlexAlignContent alignContent;

		public FlexLayout(
			FlexDirection direction = FlexDirection.Row,
			FlexWrap wrap = FlexWrap.NoWrap,
			FlexJustify justifyContent = FlexJustify.Start,
			FlexAlignItems alignItems = FlexAlignItems.Stretch,
			FlexAlignContent alignContent = FlexAlignContent.Stretch)
		{
			this.direction = direction;
			this.wrap = wrap;
			this.justifyContent = justifyContent;
			this.alignItems = alignItems;
			this.alignContent = alignContent;
		}

		public FlexDirection Direction => direction;
		public FlexWrap Wrap => wrap;
		public FlexJustify JustifyContent => justifyContent;
		public FlexAlignItems AlignItems => alignItems;
		public FlexAlignContent AlignContent => alignContent;

		protected override ILayoutManager CreateLayoutManager() => new Layout.FlexLayoutManager(this);
	}

	public enum FlexDirection
	{
		Row,
		Column,
		RowReverse,
		ColumnReverse
	}

	public enum FlexWrap
	{
		NoWrap,
		Wrap,
		Reverse
	}

	public enum FlexJustify
	{
		Start,
		Center,
		End,
		SpaceBetween,
		SpaceAround,
		SpaceEvenly
	}

	public enum FlexAlignItems
	{
		Start,
		Center,
		End,
		Stretch
	}

	public enum FlexAlignContent
	{
		Start,
		Center,
		End,
		Stretch,
		SpaceBetween,
		SpaceAround
	}

	public enum FlexAlignSelf
	{
		Auto,
		Start,
		Center,
		End,
		Stretch
	}
}
