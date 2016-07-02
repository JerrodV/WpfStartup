﻿using System;
using System.Windows;
using WpfStartup.Pages;
using System.Windows.Controls;
using WpfStartup.Models;
using WpfStartup.Helpers;
using System.Data.SqlClient;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;

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


            using (Database db = new Database(Properties.Settings.Default.Base_ConnectionString, true))
            {
                List<dynamic> data = new List<dynamic>();
                string outVal = "";
                //To add parameters
                //db.Parameters.AddWithValue("@ID", 1);
                //To Clear Parameters
                //db.Parameters.Clear();

                data.AddRange(db.GetData(db.Request("GetPeople")));

                if (data != null)
                {
                    data.ForEach(x =>
                        outVal += x.pFirstName.ToString() + "\n"
                    );
                }
                /*
                //The database has a generic function to cast a list into another type
                //This show creating the database command, turning the result into a list of dynamics, then converting it into a list of Person.
                List<Person> people = Database.Convert<Person>(db.GetData(db.Request("GetPeople")));

                //Call the same command and use the result in a linq expression.
                Database.Convert<Person>(db.GetData(db.Request("GetPeople"))).ForEach(person =>
                    outVal += person.pFirstName.ToString() + "\n"
                );

                //A simplified version of the above using just the dynamic collection
                db.RequestData("GetPeople").ForEach(person =>
                    outVal += person.pFirstName.ToString() + "\n"
                );

                //OR Better Yet, the same with a hard typed list of person
                db.RequestList<Person>("GetPeople").ForEach(person =>
                    outVal += person.pFirstName.ToString() + "\n"
                );

                //Setting a list to an object
                List<Person> peeps = db.RequestList<Person>("GetPeople");

                //Getting a known single instance
                Person per = db.RequestObject<Person>("GetPeople");

                //Function on the object
                People p = new People();
                p.GetPeople();

                //This model has an overloaded constructor. If no int is passed, a new instance is created. When it is, a get by ID is performed.
                Person _p = new Person(1);
                //The function for GetByID just returns a new instance: new Person(int);
                _p = Person.GetByID(1);
                */


                db.GetData(db.Request("GetPeople")).Convert<People>();
            }



            #region details
            //////This also leads into the intended data model design for this usage of the database
            //////*Note - The static Database.GetCommand uses an event system to close connections behind itself.
            //////The only stipulation to using it is that SET NOCOUNT ON should NOT be in the stored procedure.
            //////Removing this causes the StatementCompleted event to fire when the reader closes, or a scalar completes,
            //////This is part of what is monitered to close connections
            #endregion

            #region Get	

            //When true is passed(maintainConnection), it allows us to apply a reader after the command executes.
            //So, to make clean up easy here, we can write the calls into a using statement. 
            //The DataReader method sets the CommandBehavior on the reader to close the underlying connection
            //when it is disposed of. In this case, the dispose is called when we leave the using statment.
            //String outVal = "";
            //using (SqlDataReader dr = Database.DataReader(Database.GetCommand("GetPeople",null, true)))
            //{
            //	if (dr.HasRows)
            //	{									
            //		while(dr.Read())
            //		{
            //			if(dr == null){break;}
            //			outVal += dr["pFirstName"].ToString() + "\n";
            //		}					
            //	}
            //}			
            //Helpers.MainWindow.ShowNotification(outVal);
            //or
            //Models.People people = new Models.People(Helpers.Database.GetCommand("GetPeople",null, true));
            //string s = "";
            //foreach (Models.Person p in people)
            //{
            //	s += p.FirstName + "\n";
            //}
            //Helpers.MainWindow.ShowNotification(s);


            //DataGrid
            //*Note: The data grid on this view has been left completely unformatted and without helpers to show
            //the inherent sorting and binding capabilities.
            //Models.People people = new Models.People(Helpers.Database.GetCommand("GetPeople", null, true));
            //Helpers.MainWindow.ShowContent(new DataGridDemo(people));
            //or
            //Helpers.MainWindow.ShowContent(new DataGridDemo(new Models.People(Helpers.Database.GetCommand("GetPeople", null, true))));
            //or
            //DataGridDemo dgd = new DataGridDemo();
            //dgd.people = new Models.People(Helpers.Database.GetCommand("GetPeople", null, true));
            //Helpers.MainWindow.ShowContent(new DataGridDemo(people));

            #endregion

            #region Search
            //Simulates a last name/ first name search using a parmeter set built into the model
            //
            //Models.Person p = new Models.Person();
            //p.FirstName = "Test";
            //p.LastName = "Guy";
            //

            #region Direct Call
            //System.Data.SqlClient.SqlDataReader dr = Helpers.Database.GetCommand("Your SQL Query", p.Parameters).ExecuteReader();            
            //if (dr.HasRows)
            //{
            //    /*Do Something*/
            //}
            //dr.Close();
            //dr.Dispose();
            //
            #endregion



            #region Call with Checks
            //
            //System.Data.SqlClient.SqlCommand cmd = Helpers.Database.GetCommand("Your SQL Query", true, p.Parameters);
            //System.Data.SqlClient.SqlDataReader dr;
            //if(cmd.Connection.State == System.Data.ConnectionState.Open)
            //{
            //  dr = Helpers.Database.DataReader(cmd);    
            //}            
            //if (dr.HasRows)
            //{
            //    /*Do Something*/
            //}
            //dr.Close();
            //dr.Dispose();
            #endregion
            #endregion

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