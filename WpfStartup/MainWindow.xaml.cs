using System;
using System.Windows;
using WpfStartup.Pages;
using System.Windows.Controls;
using System.Threading;
using System.ComponentModel;

namespace WpfStartup
{

    /*
    
        *Note: The database is behind in development. I generated the stored procs from a code builder I'm also working on, but the 
        * schema has changed. The Person_Set proc will probably not work the way it is.
        * I intend to demonstrate complex object loading, but I will need to fix the DB first. Look for these changes in a future commit.
        * (JV 7/7/2016)
         
    */

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

            RunDemo();

		}

        private void RunDemo()
        {
            #region Dialog Example
            //Dialog Example                        
            //Helpers.MainWindow.ShowModal(new Welcome_Modal());
            #endregion

            #region Dynamic Menu Example
            /*
				This is completely unnecessary as menu item can easily be added to the menu
				via layout. However, I thought it would be a good opertunity to show how to add
				controls programatically.
			 */


            //Menu Setup
            //MenuItem(s) nest within each other to produce the tree structure
            //I wrote a little function (MakeMenuItem) to throw together these menu items. I get easily confused and
            //placing that kind of code elsewhere helps.
            MenuItem demos = MakeMenuItem("Demos", "Demos");

            MenuItem userNotifications = MakeMenuItem("User Notifications", "Demo_UserNotifications_Items");
            MenuItem dialog = MakeMenuItem("Dialog", "Demo_Dialog");
            MenuItem notification = MakeMenuItem("Notification(Normal)", "Demo_Notification");
            MenuItem notificationPersist = MakeMenuItem("Notification(Persist)", "Demo_Notification_Persist");
            MenuItem statusTimed = MakeMenuItem("Status Text(5 sec)", "Demo_Status_Timed");
            MenuItem statusPersist = MakeMenuItem("Status Text (Persist)", "Demo_Status_Persist");
            //This represents the first tree of the menu
            /*
                Demos
             *		-User Notifications
             *			-Dialog
             *			-Notification
             *			-StatusText(5 sec)
             *			-Status Text(Persist)
             */
            //So, we add the lowest items first
            userNotifications.Items.Add(dialog);
            userNotifications.Items.Add(notification);
            userNotifications.Items.Add(notificationPersist);
            userNotifications.Items.Add(statusTimed);
            userNotifications.Items.Add(statusPersist);

            MenuItem reset = MakeMenuItem("Reset", "Demo_Reset_Items");
            MenuItem resetNotification = MakeMenuItem("Reset Notification", "Demo_Reset_Notification");
            MenuItem resetStatus = MakeMenuItem("Reset Status", "Demo_Reset_Status");
            reset.Items.Add(resetNotification);
            reset.Items.Add(resetStatus);
            userNotifications.Items.Add(reset);


            //Now we can do one for the data binding demo
            //There is only one so we will not have to nest as much.
            MenuItem dataBinding = MakeMenuItem("Data Binding", "Demo_DataBinding");
            MenuItem dragAndDrop = MakeMenuItem("Drag Drop", "Demo_DragDrop");
            MenuItem clearContent = MakeMenuItem("CLEAR CONTENT", "Demo_ClearConent");
            //Then add the category to the parent			 
            demos.Items.Add(userNotifications);
            demos.Items.Add(dataBinding);
            demos.Items.Add(dragAndDrop);
            demos.Items.Add(clearContent);

            //Then add our collection of menu items to the Main Menu
            MainMenu.Items.Add(demos);
            #endregion

            #region Database rescue modal			
            //optional: This will allow a user to populate 
            //the connection string if it is lost, or on setup.
            //*Note: For this to work automatically in this example, the connectionstring property in Settings cannot have a value
            //Helpers.Database.CheckConnectionString();
            #endregion

            #region Notification Example
            //////Shows single row (standard)
            //Helpers.MainWindow.ShowNotification("Welcome To The Show");


            //////Shows block mode (over 18 Characters)
            //Helpers.MainWindow.ShowNotification("Welcome To The Show Welcome To The Show", false, null);

            //This one is kind of diffrent. I wanted to populate the list with a few and thought I'd use a 
            //date stamp to make the message unique, since it filters out duplicates. But, by default the ToString on the 
            //time didn't produce enough precision to make a difference. While easy to correct, I descided to instead take
            //a chance to demonstrate how to make cross thread calls using this application structure.
            //BackgroundWorker bgw = new BackgroundWorker();
            //bgw.DoWork += bgw_DoWork;
            //bgw.RunWorkerAsync();

            #endregion

            #region Status Text
            //Helpers.MainWindow.SetStatus("Hi", 5000);
            //Helpers.MainWindow.SetStatus("Hi");           
            #endregion

            #region Binding Demo
            //Demonstrates the abiliy to bind, and validate using the helpers.
            //See: Pages.Databindings*
            //Helpers.MainWindow.ShowModal(new DataBinding());
            #endregion

            #region Database Helper Example

            //The database class has a battery of tests appended to the end. 
            //It is designed to walk through line by line in (break point) debug mode.
            //Explained: You can place a break point on this line and F11 to follow the various calls as they occur.            
            DatabaseTests.Run(Properties.Settings.Default.Base_ConnectionString);           

            #endregion

            #region DragDrop Example
            //Drag/Drop
            //Helpers.MainWindow.ShowContent(new DragAndDrop());
            #endregion
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
		{
			//We can just use this handler recursively. Our message collection is a static property exposed through our helper,
			//So we can use that to key the count and let the event finish.
			//This can also be used to do things like running or monitor long running processes and reporting back their progress.
			if (Helpers.MainWindow.NotifictionMessageCollection.Count < 4)
			{
				App.AppMainWindow.Dispatcher.Invoke((Action)delegate()
				{
					Helpers.MainWindow.ShowNotification("Welcome To The Show Welcome To The Show\r\n" + DateTime.Now.ToString(), false, null);					
				});
				Thread.Sleep(1000);
				bgw_DoWork(sender, e);
			}
		}

		private MenuItem MakeMenuItem(string display, string name)
		{
			MenuItem mi = new MenuItem();
			mi.Header = display;
			mi.Name = name;			
			mi.Click += HandleMenuClick;			
			return mi;
		}

		private void HandleMenuClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			switch(mi.Name)
			{
				case "Demo_Dialog":
					Helpers.MainWindow.ShowModal(new Welcome_Modal());
				break;

				case "Demo_Notification":
					Helpers.MainWindow.ShowNotification("Welcome To The Show");
				break;

				case "Demo_Notification_Persist":
					Helpers.MainWindow.ShowNotification("Welcome To The Show Welcome To The Show", true, null);
				break;

				case "Demo_Status_Timed":
					Helpers.MainWindow.SetStatus("Hi", 5000);
				break;

				case "Demo_Status_Persist":
					Helpers.MainWindow.SetStatus("Hello");
				break;

				case "Demo_DataBinding":
					Helpers.MainWindow.ShowContent(new DataBinding());
				break;

				case "Demo_Reset_Notification":
					Helpers.MainWindow.HideNotification();
				break;

				case "Demo_Reset_Status":
					Helpers.MainWindow.SetStatus("",0);
				break;
				case "Demo_DragDrop":
					Helpers.MainWindow.ShowContent(new DragAndDrop());
				break;
				case "Demo_ClearConent":
				Helpers.MainWindow.ShowContent(new Page());
				break;
			}
			e.Handled = true;
		}
	}
}
