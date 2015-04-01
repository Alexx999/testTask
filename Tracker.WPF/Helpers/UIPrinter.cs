using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tracker.WPF.Helpers
{
    class UIPrinter
    {

        #region Properties

        public Int32 VerticalOffset { get; set; }
        public Int32 HorizontalOffset { get; set; }
        public String Title { get; set; }
        public UIElement Content { get; set; }

        #endregion

        #region Initialization

        public UIPrinter()
        {
            HorizontalOffset = 20;
            VerticalOffset = 20;
            Title = "Expenses Printout";
        }

        #endregion

        #region Methods

        public Int32 Print()
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                //---FIRST PAGE---//
                // Size the Grid.
                Content.Measure(new Size(Double.PositiveInfinity,
                                         Double.PositiveInfinity));

                Size sizeGrid = Content.DesiredSize;

                //check the width
                if (sizeGrid.Width > dlg.PrintableAreaWidth)
                {
                    MessageBoxResult result = MessageBox.Show("Grid too wide, do you still want to print?", "Print", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                        return -1;
                }

                // Position of the grid 
                var ptGrid = new Point(HorizontalOffset, VerticalOffset);

                sizeGrid = new Size(sizeGrid.Width + HorizontalOffset, sizeGrid.Height + VerticalOffset);

                // Layout of the grid
                Content.Arrange(new Rect(ptGrid, sizeGrid));

                //print
                dlg.PrintVisual(Content, Title);

                //---MULTIPLE PAGES---//
                double diff;
                int i = 1;
                while ((diff = sizeGrid.Height - (dlg.PrintableAreaHeight - VerticalOffset * i) * i) > 0)
                {
                    //Position of the grid 
                    var ptSecondGrid = new Point(HorizontalOffset, -sizeGrid.Height + diff + VerticalOffset);

                    // Layout of the grid
                    Content.Arrange(new Rect(ptSecondGrid, sizeGrid));

                    //print
                    int k = i + 1;
                    dlg.PrintVisual(Content, Title + " (Page " + k + ")");

                    i++;
                }
                var parent = VisualTreeHelper.GetParent(Content) as UIElement;
                if (parent != null)
                {
                    parent.InvalidateMeasure();
                }
                return i;
            }

            return -1;
        }

        #endregion
    }
}
