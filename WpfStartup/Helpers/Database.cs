using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using WpfStartup.Models;
using JSON = Newtonsoft.Json.JsonConvert;
/// <summary>
/// Summary description for Database
/// </summary>
public class Database : IDisposable
{
    /// <summary>
    /// Private SqlConnectionStringBuilder so we can do some logic in the setter on the public var
    /// </summary>
    private SqlConnectionStringBuilder _ConnectionString = null;

    /// <summary>
    /// Returns a string representing the connection properties for our connection
    /// <para>This will not allow an empty string to be set. It will also fail if the connection string is not valid.</para>
    /// <para>Finally, if everything is ok, it will create the connection object the class will use later.</para>
    /// </summary>
    public string ConnectionString
    {
        get
        {
            if (_ConnectionString == null)
            {
                return "";
            }
            return _ConnectionString.ToString();
        }

        set
        {
            if (value.Trim() == "")
            {
                throw new Exception("Connection string cannot be empty");
            }

            //If this throws an error, there is a problem with the connection string
            _ConnectionString = new SqlConnectionStringBuilder(value);
            MakeConnection();
        }
    }

    private SqlConnection _Connection = null;
    public SqlConnection Connection
    {
        get
        {           
            return _Connection;
        }

        set
        {
            _Connection = value;
        }
    }

    public List<SqlParameter> Parameters = new List<SqlParameter>();
    
    #region Constructors
    /// <summary>
    /// Creates a new instance of the Database class.
    /// <para>Note, if this constructor is used, you will need to provide an entire SqlConnection object before making Request calls.</para>
    /// </summary>
    public Database() { }

    /// <summary>
    /// Creates a new instance of the database class
    /// </summary>
    /// <param name="ConnectionString"></param>
    /// <param name="ready"></param>
    public Database(string ConnectionString, bool ready = false)
    {        
        this.ConnectionString = ConnectionString;
        MakeConnection();
        if (ready)
        {
            Ready();
        }
    }
    #endregion

    #region Core Functions

    /// <summary>
    /// This will reinitialize the settings and ready the Database object for reuse.
    /// </summary>
    /// <returns>True if connection state is open</returns>
    public bool Ready()
    {
        //If we don't have a connection, the developer used the empty constructor and never provided a connection.
        if (Connection == null) { throw new NullReferenceException("Cannot ready a null connection"); }

        //Clear any parameters just in case the connection has been used previously.
        Parameters.Clear();

        //Open the connection
        if (Connection.State != System.Data.ConnectionState.Open)
        {
            Connection.Open();//This is left to error if it cannot be opened.
        }

        //If the connection is open return true, else return false.
        return Connection.IsReady();

    }   

    /// <summary>
    /// Use this when you need to establish or recreate the connection
    /// </summary>
    private void MakeConnection()
    {
        //First, if we have a connection, just close it for good practice, since we are about to make a new one.
        if (Connection != null && Connection.IsReady()) { Connection.Close(); Connection.Dispose(); }
        
        //Make a new SqlConnection
        Connection = new SqlConnection(ConnectionString);
    }

    /// <summary>
    /// Returns an SqlCommand object ready to have an execution method called.
    /// </summary>
    /// <param name="ProcedureName">Name of the stored procedure.</param>
    /// <returns>An SqlCommand with parameters loaded, type set to stored procedure, command text set, and connection open</returns>
    public SqlCommand Request(string ProcedureName, List<SqlParameter> Parameters = null)
    {
        //Create a new SqlCommand Object. This will be our return value.
        SqlCommand cmd = new SqlCommand();

        //Set the command type. We always use stored procedures, so I'm just doing this. When I've needed to use tSql, I found it easy to adapt.
        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //Set the local instance of SqlConnection to the command
        cmd.Connection = Connection;

        //Add the stored procedure name
        cmd.CommandText = ProcedureName;
        
        //If we have as SqlParameter list as a parameter, load it.
        if (this.Parameters != null && this.Parameters.Count > 0)
        {
            cmd.Parameters.AddRange(this.Parameters.ToArray());
        }

        return cmd;
    }    

    /// <summary>
    /// Given an SqlCommand, Creates a List(T) of dynamic(ExpandoObject)
    /// </summary>
    /// <param name="cmd">SqlCommandObject to execute and read</param>
    /// <returns>List(T) of dynamic</dynamic></returns>
    public List<dynamic> GetData(SqlCommand cmd)
    { 
        //*Note, it is assumed that this class will have been used to create the command object.
        //If something is wrong with the command object, I am not going to stop the errors.

        //Create a place for our return value
        List<dynamic> retList = new List<dynamic>();

        //Create a reader
        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            //If the reader has rows
            if (dr.HasRows)
            {
                //While we have records
                while (dr.Read())
                {
                    //Create a dynamic object to add to the return list
                    dynamic dyno = new ExpandoObject();
                    
                    //Make a pointer to it that we can set names dynamically to.
                    IDictionary<string, object> d = dyno;

                    //Create a field counter
                    int c = 0;
                    
                    //while we still have fields in the record
                    while (c < dr.VisibleFieldCount)
                    {
                        //Add the value, byt field name, to the dictionary                        
                        d[dr.GetName(c)] = dr.GetValue(c);

                        //Increment the field counter
                        c++;
                    }

                    //Add the dynamic object to the list. Because we created a reference pointer, it holds the values we assigned to the dictionary
                    retList.Add(d);
                }
            }
        }

        return retList;
    }

    #endregion

    #region Conversion

    /// <summary>
    /// Converts a List(T) of dynamic objects to a typed (generic) list</Dynamic>
    /// </summary>
    /// <typeparam name="T">Type of the individual list items</typeparam>
    /// <param name="list">List(T) of dynamics to convert</param>
    /// <returns>List(T) of T</returns>
    public static List<T> Convert<T>(List<dynamic> List) 
    {
        //Create a return list
        List<T> retVal = new List<T>();

        //Convert each object and add it to the return list
        List.ForEach(x => retVal.Add(Convert<T>(x)));

        return retVal;
    }

    /// <summary>
    /// Converts a single dynamic object to an object type expressed in T 
    /// </summary>
    /// <typeparam name="T">Type of the resulting object</typeparam>
    /// <param name="Obj">dynamic object to convert</param>
    /// <returns>Object of type T</returns>
    public static T Convert<T>(dynamic Obj)
    {
        //Pretty simple. We are using Newtonsoft to cast the dynamic to a static type using a string intermediary.

        T retVal = default(T);
        try
        {
            retVal = JSON.DeserializeObject<T>(JSON.SerializeObject(Obj));
        }
        catch
        {
            /*
             * We cannot return null T, since it could be non-nullable (like int). 
             * retVal will be default(T) already so we don't need to do anything here. 
             */
        }
        return retVal;
    }

    /// <summary>
    /// Given a stored procedure name and optional list of parameters, fetches data into a List(T) of dynamic"/>
    /// </summary>
    /// <param name="ProcedureName">String name of stored procedure</param>
    /// <param name="Parameters">List(T) of SqlParameter</param>
    /// <returns>List(T) of dynamic</returns>
    public List<dynamic> RequestData(string ProcedureName, List<SqlParameter> Parameters = null)
    {
        //Using the Request and GetData function, return a List(T) of dynamic containing the data from the request
        return GetData(Request(ProcedureName, Parameters));
    }

    /// <summary>
    /// Requests a list of data from the database
    /// </summary>
    /// <typeparam name="T">Type (singular) each instance of the list will be typed</typeparam>
    /// <param name="ProcedureName">Sql stored procedure name to call</param>
    /// <param name="Parameters">Optional List(T) of SqlParameter objects for use in the stored procedure</param>
    /// <returns></returns>
    public List<T> RequestList<T>(string ProcedureName, List<SqlParameter> Parameters = null) 
    {      
        //Uses the RequestData function, plus the List(T) of dynamic extension method to produce a typed list
        return RequestData(ProcedureName, Parameters).Convert<T>();
    }

    /// <summary>
    /// Request a single strongly typed object from the database
    /// </summary>
    /// <typeparam name="T">Type of the object to be returned</typeparam>
    /// <param name="ProcedureName">Sql stored procedure name to call</param>
    /// <param name="Parameters">Optional List(T) of SqlParameter objects for use in the stored procedure</param>
    /// <returns>An object of the requested type</returns>
    public T RequestObject<T>(string ProcedureName, List<SqlParameter> Parameters = null)
    {
        //Use the RequestList function and return just the first instance in the list.
        List<T> l = RequestList<T>(ProcedureName, Parameters);
        
        //If we have a list and there is at least one item
        if (l != null && l.Count > 0)
        {
            //Return the first item
            return l[0];
        }

        //The result was unusable, return the default for the given type
        return default(T);
    }

    #endregion


    /// <summary>
    /// Closes and disposes of the underlying connection
    /// </summary>
    public void Dispose()
    {
        //If we have a connection
        if (Connection != null)
        {
            //Close and dispose of it
            Connection.Close();
            Connection.Dispose();
        }

        //One thing I wonder about here is if we are able to detect an attached reader.
        //It is possible that a coding flaw could lead to an attempt to close a connection in use.
        //If we could detect it, would we just throw an error, or close and dispose of the reader?
    }

}

public static partial class MyExtentions
{

    /// <summary>
    /// Allows for a List(T) of SqlParameter to act similarly to an SqlParameterCollection's AddWithValue function
    /// </summary>
    /// <param name="Parameters">List(T) of SqlParameter</param>
    /// <param name="Name">Parameter name</param>
    /// <param name="Value">Parameter value</param>
    public static void AddWithValue(this List<SqlParameter> Parameters, string Name, object Value)
    {
        //Pretty simple. Just add a new instance of SqlParameter to self using the given parameters.
        Parameters.Add(new SqlParameter(Name, Value));
    }

    /// <summary>
    /// Converts and instance of List(T) of dynamic to a known type
    /// </summary>
    /// <typeparam name="T">Type (singular) to convert the List(T) of dynamic to</typeparam>
    /// <param name="List">List(T) of dynamic</param>
    /// <returns></returns>
    public static List<T> Convert<T>(this List<dynamic> List)
    {
        //Use the Database Convert function to create a new list based on the current List(T) of dynamic
        return Database.Convert<T>(List);
    }

    /// <summary>
    /// Checks that a connection exists and that it is currently open to the database.
    /// </summary>
    /// <param name="Conn">SqlConnection</param>
    /// <returns>True if connection is present and open</returns>
    public static bool IsReady(this SqlConnection Conn)
    {
        //We need to check for null before we check the state, so just return false if null
        if (Conn == null)
        {
            return false;
        }

        //If the connection is open, return true, otherwise false.
        return Conn.State == System.Data.ConnectionState.Open;

    }

}

//Consider abandoning this. I don't like the connection string being exposed on each model. This could make it problematic if this class was ever used for an API.
//I think a better approach would be to set it to a pointer in application scope accessible by the model (duh like Properties.Settings.Default.Base_ConnectionString... It's just soooo long).
public class DatabaseObject
{
    protected string ConnectionString { get; set; }
}


#region Testing

public static class DatabaseTests
{
    public static void Run(string ConnectionString)
    {
        //We will test from the ground up.
        Database db0 = new Database();
        //This instance is worthless.
        //This will error
        try
        {
            db0.Ready();
        }
        catch { }

        //However, if we assign a connection string, we should get a usable connection
        db0.ConnectionString = ConnectionString;

        //Check the connection state
        bool db0IsOpen = db0.Connection.State == System.Data.ConnectionState.Open;//Expect False

        db0IsOpen = db0.Ready();//Expect True if connection string was good

        //Prove it's really open
        db0IsOpen = db0.Connection.State == System.Data.ConnectionState.Open;//Expect True
        
        //We will test data gathering on a connection later.
        db0.Dispose();




        //We should also be able to plug in our own connection object, then use the class as intended. In this case, the connection is not automatically opened.
        Database db1 = new Database();

        //Create and add a new SqlConnection and check using included functions
        db1.Connection = new SqlConnection(ConnectionString);

        bool db1IsOpen = db1.Connection.State == System.Data.ConnectionState.Open;//Expect False

        //Expect True if connection string was good
        db1IsOpen = db1.Ready();

        //Prove it's really open
        db1IsOpen = db1.Connection.State == System.Data.ConnectionState.Open;//Expect True

        db1.Dispose();



        //Constructed from Connection string, not ready
        Database db2 = new Database(ConnectionString);

        //Check the connection state
        bool db2IsOpen = db2.Connection.State == System.Data.ConnectionState.Open;//Expect False

        db2IsOpen = db2.Ready();//Expect True if connection string was good

        //Prove it's really open
        db2IsOpen = db2.Connection.State == System.Data.ConnectionState.Open;//Expect True
                
        db2.Dispose();




        //Constructed from Connection string, ready
        Database db3 = new Database(ConnectionString, true);

        //Check the connection state
        bool db3IsOpen = db3.Connection.State == System.Data.ConnectionState.Open;//Expect True

        db3IsOpen = db3.Ready();//Expect True if connection string was good

        //Prove it's really open
        db3IsOpen = db3.Connection.State == System.Data.ConnectionState.Open;//Expect True

        db3.Dispose();




        //Construct in using with outer connection object
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            using (Database db = new Database())
            {
                db.Connection = conn;

                //Check the connection state
                bool dbIsOpen = db.Connection.State == System.Data.ConnectionState.Open;//Expect False

                dbIsOpen = db.Ready();//Expect True if connection string was good

                //Prove it's really open
                dbIsOpen = db.Connection.State == System.Data.ConnectionState.Open;//Expect True

            }//It is possible this could break. Database Dispose will dispose of the connection. Then, hitting the end bracket will also dispose of the connection.
        }





        //Construct in using with ready
        using (Database db = new Database(ConnectionString, true))
        {
            //Check the connection state
            bool dbIsOpen = db.Connection.State == System.Data.ConnectionState.Open;//Expect True

            dbIsOpen = db.Ready();//Expect True if connection string was good

            //Prove it's really open
            dbIsOpen = db.Connection.State == System.Data.ConnectionState.Open;//Expect True

        }


        //Ok, we can construct the object as expected... I hope


        //Now, lets test some of the functions that will allow us to use a ready connection
        //Since the Request function returns a command object, we can hijack it and run a test.
        Database db4 = new Database(ConnectionString, true);//Connection should be open

        //Get a command object from the database instance
        SqlCommand cmd = db4.Request("");

        //Set the command type to tSql to prove it is set to Stored Procedure
        System.Data.CommandType curType = cmd.CommandType;

        //Set the command type to text for a test
        cmd.CommandType = System.Data.CommandType.Text;

        //Test the command text to a simple select.
        cmd.CommandText = "select 'Hello World'";

        //Get the database to execute the select statment
        string db4T1 = cmd.ExecuteScalar() as string;//Expect Hello World


        //We now know we can use the database class to talk to a database.
        db4.Dispose();


        //Use the predefined stored procedure to test data gathering
        string GetP = "GetPeople";

        
        //Might as well also test running several commands under one instance of a database class
        using (Database db = new Database(ConnectionString, true))
        {
            //Again, since Request just returns a SqlCommandObject, we can run a reader right against the function call
            using (SqlDataReader dr = db.Request(GetP).ExecuteReader())
            {
                if (dr.HasRows) { }//Expect True
            }

            List<dynamic> dat0 = db.GetData(db.Request(GetP));//We should have the list (2 records)

            //Now we can test the function that uses GetData to create the list for us
            List<dynamic> dat1 = db.RequestData(GetP);//We should have the list (2 records)

            //Now we can test the function that uses RequestData and types an object
            Person p1 = Database.Convert<Person>(dat1[0]);//One person expected

            //Now we can test the function that uses RequestData and types a whole list
            List<Person> p2 = Database.Convert<Person>(dat1);//We should have the list (2 records)

            //Now we can test the function that uses //We should have the list (2 records) to get an object
            Person p3 = db.RequestObject<Person>(GetP);//One person expected

            //Now we can test the function that uses Database.Convert<>() to get a whole list
            List<Person> p4 = db.RequestList<Person>(GetP);//We should have the list (2 records)

            //And finally, the extension method that also uses //We should have the list (2 records)
            //*Note, db.RequestData returns a List<dynamic> which has been extended with Convert<T>
            List<Person> p5 = db.RequestData(GetP).Convert<Person>();


            //A get by ID call. This would be similar to what you would have in your Model.
            db.Parameters.AddWithValue("@pID", 1);
            Person p6 = db.RequestObject<Person>("Person_GetByID");
            
        }

        //And the rest would be to test some of the functions built into the model
        Person p = new Person(1);//This is eq to a get by ID call

        //I also have
        Person p7 = Person.GetByID(1);

        //And an example of a set call
        //Person p8 = Person.Set(p7);

        //Or Better Yet
        //p8.pActive = false;
        //p8.UpdateDatabase();
        //Note that the results of this call do in fact update the model with the latest data from the database.


    }

}

#endregion