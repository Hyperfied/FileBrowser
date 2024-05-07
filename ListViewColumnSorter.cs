using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileBrowser
{
    internal class ListViewColumnSorter : IComparer
    {
        private int ColumnToSort;
        private SortOrder OrderOfSort;
        private CaseInsensitiveComparer ObjectCompare;

        public ListViewColumnSorter()
        {
            ColumnToSort = 0;
            OrderOfSort = SortOrder.None;
            ObjectCompare = new();
        }

        public int Compare(object? x, object? y)
        {
            int compareResult = 0;
            
            ListViewItem? listViewX = x as ListViewItem;
            ListViewItem? listViewY = y as ListViewItem;

            switch (ColumnToSort)
            {
                case 0:
                    compareResult = ObjectCompare.Compare(listViewX.SubItems[0].Text, listViewY.SubItems[0].Text);
                    break;
                case 1:
                    long xSize = FileBrowser.StringToBytes(listViewX.SubItems[1].Text);
                    long ySize = FileBrowser.StringToBytes(listViewY.SubItems[1].Text);

                    compareResult = ObjectCompare.Compare(xSize, ySize);
                    break;
                case 2:
                    DateTime xDate = DateTime.Parse(listViewX.SubItems[2].Text);
                    DateTime yDate = DateTime.Parse(listViewY.SubItems[2].Text);

                    compareResult = ObjectCompare.Compare(xDate, yDate);
                    break;
            }

            switch (OrderOfSort)
            {
                case SortOrder.Ascending:
                    return compareResult;
                case SortOrder.Descending:
                    return -(compareResult);
                default:
                    return 0;
            }
        }

        public int SortColumn { get { return ColumnToSort; } set { ColumnToSort = value; } }
        public SortOrder Order { get { return OrderOfSort; } set {  OrderOfSort = value; } }
    }
}
