using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace WpfStartup.Models
{
    /// <summary>
    /// When creating classes the following design is recommended for use with the database helper classes.
    /// Note that the ID has no Get/Set. This will keep it from showing in datagrids when attaching 
    /// observable collections of the object as the data source. Also, the way to go about handling field names on
    /// datagrids, you will need to use the DataGridAutoGeneratingColumn columns event. Because the 
    /// datagrid will always generate columns in the order they are defined in the class, you can add 
    /// their friendly name to a list available to the DataGridAutoGeneratingColumn event handler in 
    /// the same order. Then, just extract the current column index from e, and match it to the list index.
    /// This of course assumes you do not define the column bindings in the xaml.
    /// </summary>

    //By inheriting DatabaseObject, it just brings in a connection string property we can use in other functions
    //It allowed me to shorten the name to the config file. It also allows for objects to be populated from different data sources.
    public class Person : DatabaseObject
	{
		public Person()
        {
            ConnectionString = Properties.Settings.Default.Base_ConnectionString;
        }
              
        public Person(int? id = null)
        {
            ConnectionString = Properties.Settings.Default.Base_ConnectionString;

            if (id.HasValue)
            {

                using (var db = new Database(ConnectionString, true))
                {
                    db.Parameters.AddWithValue("@pID", id.Value);
                    UpdateSelf(db.RequestObject<Person>("Person_GetByID"));                    
                }
            }
        }

        private void UpdateSelf(Person p)
        {
            pID = p.pID;
            pFirstName = p.pFirstName;
            pLastName = p.pLastName;
            pPhoneNumber = p.pPhoneNumber;
            pDOB = p.pDOB;
            pActive = p.pActive;
        }
        
		public int? pID;//<--Not Bindable(No Get/Set)
		public string pFirstName { get; set; }
		public string pLastName { get; set; }
		public string pPhoneNumber { get; set; }
        public DateTime pDOB { get; set; }
        public bool pActive { get; set; }

		public List<SqlParameter> Parameters
		{
            get
            {                
                List<SqlParameter> pL = new List<SqlParameter>();
                if (pID.HasValue) { pL.AddWithValue("@ID", pID.Value); }
                if (pFirstName != "") { pL.AddWithValue("@pFirstName", pFirstName); }
                if (pLastName != "") { pL.AddWithValue("@pLastName", pLastName); }
                if (pPhoneNumber != "") { pL.AddWithValue("@pPhoneNumber", pPhoneNumber); }
                return pL;
            }
		}

        public static Person GetByID(int id)
        {
            return new Person(id);
        }

        public void UpdateDatabase()
        {
            UpdateSelf(Set(this));
            
        }

        public static Person Set(Person person)
        {
            Person retVal = null;
            using (Database db = new Database(new Person().ConnectionString, true))
            {
                retVal = db.RequestObject<Person>("Person_Set", person.Parameters);
            }
            return retVal;
        }
    }

    #region pluralization    

    public class People : List<Person>
    {
        public People(){}
        public void GetPeople()
        {
            Clear();
            using (Database db = new Database(Properties.Settings.Default.Base_ConnectionString, true))
            {
                AddRange(db.RequestList<Person>("GetPeople"));       
            }
        }
    }

	#endregion

}
