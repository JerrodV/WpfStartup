using System;
using System.Windows;
using WpfStartup.Pages;
using System.Windows.Controls;
using System.Threading;
using System.ComponentModel;

namespace WpfStartup
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            Helpers.MainWindow.ShowContent(new Welcome_Modal("I hope you check out the helper classes."));
		}
	}
}
