using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using JSON = Newtonsoft.Json.JsonConvert;
/// <summary>
/// Summary description for Database
/// </summary>
public class Database : IDisposable
{

    private string ConnectionString = "";

    public SqlConnection Connection;
    public List<SqlParameter> Parameters = new List<SqlParameter>();

    /// <summary>
    /// Use this when processing an Ajax call to set the return content type to Json
    /// </summary>
    public static string JsonContentType
    {
        get
        {
            return "application/json; charset=utf-8";
        }
    }

    #region Constructors
    /// <summary>
    /// Creates a new instance of the Database class.
    /// </summary>
    public Database() { }

    public Database(string connectionString)
    {
        ConnectionString = connectionString;
        makeConnection();
    }

    public Database(string connectionString, bool ready)
    {
        ConnectionString = connectionString;
        makeConnection();
        if (ready)
        {
            Ready();
        }
    }
    #endregion


    /// <summary>
    /// This will reinitialize the settings and ready the Database object for reuse.
    /// </summary>
    /// <returns></returns>
    public bool Ready()
    {
        Parameters.Clear();
        try
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Similar to the Ready call, however, it only rebuilds and 
    /// re-establishes the connection and does not clear the parameters.
    /// </summary>
    /// <returns>True if connection was re-established successfully. False if an error was detected.</returns>
    public bool Refresh()
    {
        return makeConnection();
    }

    /// <summary>
    /// Returns an SqlCommand object ready to have an execution method called.
    /// </summary>
    /// <param name="procedureName">Name of the stored procedure.</param>
    /// <returns>An SqlCommand with parameters loaded, type set to stored procedure, command text set, and connection open</returns>
    public SqlCommand Request(string procedureName)
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Connection = Connection;
        cmd.CommandText = procedureName;

        if (cmd.Connection != null && cmd.Connection.State != System.Data.ConnectionState.Open)
        {
            try
            {
                cmd.Connection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not open the database connection:\n" + ex.Message);
            }
        }

        if (Parameters.Count > 0)
        {
            cmd.Parameters.AddRange(Parameters.ToArray());
        }

        return cmd;
    }

    /// <summary>
    /// An overload of Request(string procedureName) that allows the developer to set the parameters inline if desired. 
    /// Note that the instance parameters (this.Parameters) collection is overwritten when this is used
    /// so that the parameters can be checked later and be accurate.
    /// </summary>
    /// <param name="procedureName">Name of the stored procedure.</param>
    /// <param name="parameters">List(T) of SQLParameter objects</param>
    /// <returns>An SqlCommand with parameters loaded, type set to stored procedure, command text set, and connection open</returns>
    public SqlCommand Request(string procedureName, List<SqlParameter> parameters)
    {
        Parameters = parameters;
        return Request(procedureName);
    }

    /// <summary>
    /// Use this when you need to establish or refresh the connection
    /// </summary>
    private bool makeConnection()
    {
        //First, if we have a connection
        if (Connection != null && Connection.State == System.Data.ConnectionState.Open) { Connection.Close(); }

        if (ConnectionString != "")
        {
            Connection = new SqlConnection(ConnectionString);

            try
            {
                Connection.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Closes and disposes of the underlying connection
    /// </summary>
    public void Dispose()
    {
        if (Connection != null)
        {
            Connection.Close();
            Connection.Dispose();
        }
    }

    /// <summary>
    /// Given an SqlCommand, Creates a List(T) of dynamic(ExpandoObject)
    /// </summary>
    /// <param name="cmd">SqlCommandObject to execute and read</param>
    /// <returns>List(T) of dynamic</dynamic></returns>
    public List<dynamic> GetData(SqlCommand cmd)
    {
        List<dynamic> data = new List<dynamic>();

        if (cmd != null)
        {
            if (cmd.Connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    cmd.Connection.Open();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        dynamic dyno = new ExpandoObject();
                        IDictionary<string, object> d = dyno;
                        int c = 0;
                        while (c < dr.VisibleFieldCount)
                        {
                            string key = dr.GetName(c);
                            object value = dr.GetValue(c);
                            d[key] = value;
                            c++;
                        }
                        data.Add(d);
                    }
                }
            }
        }

        return data;
    }

    /// <summary>
    /// Converts a List(T) of dynamic objects to a typed (generic) list</Dynamic>
    /// </summary>
    /// <typeparam name="T">Type of the individual list items</typeparam>
    /// <param name="list">List(T) of dynamics to convert</param>
    /// <returns>List(T) of T</returns>
    public static List<T> Convert<T>(List<dynamic> list) 
    {
        List<T> retVal = new List<T>();
        list.ForEach(x =>
            retVal.Add(JSON.DeserializeObject<T>(JSON.SerializeObject(x)))
        );
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
        if (Parameters != null)
        {
            this.Parameters = Parameters;
        }
        return GetData(Request(ProcedureName));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ProcedureName"></param>
    /// <param name="Parameters"></param>
    /// <returns></returns>
    public List<T> RequestList<T>(string ProcedureName, List<SqlParameter> Parameters = null) 
    {
        List<T> retVal = null;
        if (Parameters != null)
        {
            this.Parameters = Parameters;
        }
        var type = typeof(T);
        retVal = Convert<T>(GetData(Request(ProcedureName)));
        return retVal;
    }

    public T RequestObject<T>(string ProcedureName, List<SqlParameter> Parameters = null)
    {
        if (Parameters != null)
        {
            this.Parameters = Parameters;
        }        
        return Convert<T>(RequestData(ProcedureName, Parameters))[0];
    }

}

public static partial class MyExtentions
{
    public static void AddWithValue(this List<SqlParameter> Parameters, string name, object value)
    {
        Parameters.Add(new SqlParameter(name, value));
    }

    public static void Convert<T>(this List<dynamic> list)
    {
        List<T> fill = new List<T>();
        fill = Database.Convert<T>(list);
        list.Clear();
        fill.ForEach(x =>
            list.Add(x)
        );
    }
}