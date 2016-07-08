using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfStartup.Pages
{
    /// <summary>
    /// Interaction logic for DragDrop.xaml
    /// </summary>
    public partial class DragAndDrop : Page
    {
        //Just a collection of stuff to play with. It does not have to be a key value pair. In fact, it can be any object.
        //The DisplayMemberPath will determine the field that is used when displaying a value is needed.
        private List<KeyValuePair<int, string>> Months;

        public DragAndDrop()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {           
            Months = new List<KeyValuePair<int, string>>() 
            { 
                new KeyValuePair<int, string>(1,"January"),
                new KeyValuePair<int, string>(2,"February"),
                new KeyValuePair<int, string>(3,"March"),
                new KeyValuePair<int, string>(4,"April"),
                new KeyValuePair<int, string>(5,"May"),
                new KeyValuePair<int, string>(6,"June"),
                new KeyValuePair<int, string>(7,"July"),
                new KeyValuePair<int, string>(8,"August"),
                new KeyValuePair<int, string>(9,"September"),
                new KeyValuePair<int, string>(10,"October"),
                new KeyValuePair<int, string>(11,"November"),
                new KeyValuePair<int, string>(12,"December")
            };

            //For the first two, I set the collection to the moth, but change the display member.
            lb1.ItemsSource = Months;
            lb1.DisplayMemberPath = "Value";

            lb2.ItemsSource = Months;
            lb2.DisplayMemberPath = "Key";
            
            //For these two, they are intended to receive values, though you can also drag items one they are populated. 
            //They do not get a item source.
            lb3.DisplayMemberPath = "Value";
            
            lb4.DisplayMemberPath = "Key";
        }

        private void lb_MouseMove(object sender, MouseEventArgs e)
        {   
            //In many demonstrations, in addition to this check, you would also check how far the mouse had moved since the left button was clicked.
            //This prevents the object from being copied into memory just because a list item was clicked. For this example, the object is very small and the additional
            //complexity of adding the additional check we deemed unnecessary. However, if you were dragging a larger object, or running process that made false drags 
            // an issue, you may want to consider that tactice. There are plenty of examples on the net if you need an example.
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBox lb = sender as ListBox;
                List<KeyValuePair<int, string>> obj =
                    lb.SelectedItems.OfType<KeyValuePair<int, string>>().ToList();
                DataObject theDO = new DataObject();
                theDO.SetData("theFormat", obj);
                if (obj != null)
                {
                    DragDrop.DoDragDrop(lb, theDO, DragDropEffects.Copy);
                }
            }
        }

        //This toggles the mouse pointer to show a drop of type copy is allowed
        private void lb_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        //This performs out drop logic
        private void lb_Drop(object sender, DragEventArgs e)
        {
            //Is this data we can take as a drop?
            if (e.Data.GetDataPresent("theFormat"))
            {
                //It was, so get the data by the format, parse it, and poof, we have data.
                List<KeyValuePair<int, string>> obj = e.Data.GetData("theFormat") as List<KeyValuePair<int, string>>;
                
                //Create a pointer to listbox that the drop event occured on
                ListBox lb = sender as ListBox;
                //Add each of the items in the drag collection to the list.
                foreach(KeyValuePair<int, string> kvp in obj)
                {
                    lb.Items.Add(kvp);
                }
                //Turn of the drop cursor.
                e.Effects = DragDropEffects.None;
            }
        }

    }
}
