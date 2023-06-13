using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    column.Width = column.ActualWidth;

               
                column.Width = double.NaN;
            }
            base.PrepareItem(item);
        }
    }
}