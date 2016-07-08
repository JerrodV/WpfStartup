using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfStartup.Helpers
{
    /// <summary>
    /// Interaction logic for DefaultErrorMessage.xaml
    /// </summary>
    public partial class DefaultErrorMessage : Page
	{
		private string message;
		private Exception ex;
		public DefaultErrorMessage(string message, Exception ex)
		{			
			InitializeComponent();
			this.message = message;
			this.ex = ex;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			txtDefaultMessage.Text = message;
			txtExemptionMessage.Text = "Error: \n";
			txtExemptionMessage.Text += ex.Message + "\n\n";
			txtExemptionMessage.Text += "Stack Trace: \n";
			txtExemptionMessage.Text += ex.StackTrace.ToString() + "\n\n";
			txtExemptionMessage.Text += "Inner Exception: \n";
			txtExemptionMessage.Text += ex.InnerException.ToString() + "\n\n";
		}
	}
}
