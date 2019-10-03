using System.Windows.Controls;

using WpfStartup.Helpers.Extensions.GridExtension;

namespace WpfStartup.Pages
{
    /// <summary>
    /// Interaction logic for Welcome_Modal.xaml
    /// </summary>
    public partial class Welcome_Modal : Page
    {       
        public Welcome_Modal(string message)
        {
            InitializeComponent();
            grdMain.Add(new Label()
            {
                Content = message,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new System.Windows.Thickness(0, 50, 0, 0)
            }, 0, 0);
        }
    }
}
