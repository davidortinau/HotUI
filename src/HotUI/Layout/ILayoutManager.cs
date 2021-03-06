namespace HotUI.Layout
{
    public interface ILayoutManager
    {
        void Invalidate();
        SizeF Measure(AbstractLayout layout, SizeF available);
        void Layout(AbstractLayout layout, SizeF measured);
    }
}