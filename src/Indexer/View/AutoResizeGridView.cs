using System.Windows.Controls;

namespace Indexer.View
{
    public class AutoResizeGridView : GridView
    {
        protected override void PrepareItem(ListViewItem item)
        {
            foreach (GridViewColumn column in Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    // If the value is already NaN, we need to first change it
                    // to anything else to be able to trigger the property change
                    column.Width = column.ActualWidth;
                }

                // Trigger resize a column to its content
                column.Width = double.NaN;
            }
            base.PrepareItem(item);
        }
    }
}
