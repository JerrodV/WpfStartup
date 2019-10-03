using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace WpfStartup.Helpers.Extensions.GridExtension
{
    public static class GridExtension
    {
        public static void Add(this Grid grid, Control control, int column, int row)
        {
            Grid.SetColumn(control, column);
            Grid.SetRow(control, row);
            if (!grid.Children.Contains(control))
            {
                grid.Children.Add(control);
            }
        }
    }
}
