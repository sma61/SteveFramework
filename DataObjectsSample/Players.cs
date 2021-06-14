using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.ComponentModel;


namespace DataObjectsSample
{
    public class Players : IDisposable
    {
        public class PlayerHashList : Hashlist
        {
            protected new Dictionary<object, Player> m_oValues = new Dictionary<object, Player>();

            public new Player this[object Key]
            { 
                get { return (Player)base[Key]; }
                set { m_oValues[Key] = value; }
            }


            public new Player this[int Index]
            {
                get
                {
                    object oTemp = base[Index];
                    return (Player)oTemp;
                }
            }

            public PlayerHashList()
            { 
            }
        }

        
        public Hashlist _colClassObj;
        private IntPtr _Handle;
        private IContainer _Components;

        private bool _Disposed = false;

        private DataEngine _DE = new DataEngine();

        private const string _tableName = "Player"; //table in SQL database

        private const string _dataClassName = "Player";
        private const string _spSELECT = "Player_Select"; //SP in database to get data for class - propert name must match fieldnames
        private const string _spSave = "Player_Save"; //SP in databast to Save data - Insert and Update in same SP

        private const string DATABASE_NAME = ""; //used for parameter in DataEngine

        public Players(string ParentKey = " ", string ParentKeyName = " ")
        {
            //Purpose: Initialize class object and set properties.
            if (!(string.IsNullOrEmpty(ParentKey)) & !(string.IsNullOrEmpty(ParentKeyName)))
            {
                LoadData(ParentKeyName, ParentKey);
            }
        
        }
        ~Players()
        {
            //This Finalize method will run only if the Dispose method does not get called.
            //By default, methods are NotOverrridable. This prevents a derived class from ovveriding this method
            //Do not
            Dispose(false);
        }

        public void LoadData(string ParentKeyName = "", string ParentKey = "")
        {
            object objClass = null;
            Int32 i = default(Int32);
            DataSet dset = null;
            PropertyInfo propInfo = null;
            Assembly objAssembly = null;
            Type objType = null;
            object[] argsKey = { "" };
            _colClassObj = new PlayerHashList();

            dset = _DE.GetSPDataSet(_spSELECT, ParentKeyName, ParentKey, DATABASE_NAME);
            if ((dset != null))
                {
                objAssembly = Assembly.GetExecutingAssembly();
                //get the executing assebly name from the objAssembly (first comma)
                int intComma = objAssembly.FullName.IndexOf(",");
                string strMainAssembly = objAssembly.FullName.Substring(0, intComma);
                objType = objAssembly.GetType(strMainAssembly + "." + _dataClassName, true, true);

                while (i < dset.Tables[0].Rows.Count)
                {
                    objClass = Activator.CreateInstance(objType, argsKey);

                    //Loop through the fields and assign values to properties, properties and table name are case sensitive and must 
                    //match exactly. Table Names can be aliases in SPs to match properties
                    foreach (DataColumn col in dset.Tables[0].Columns)
                    {
                        try
                        {
                            propInfo = objClass.GetType().GetProperty(col.ColumnName);
                            propInfo.SetValue(objClass, dset.Tables[0].Rows[i][col.Ordinal], null);
                        }
                        catch (Exception)
                        {
                            Console.Write(col.ColumnName + ",");
                            propInfo = null;
                        }
                    
                    }

                    //Add it the collection
                    propInfo = objClass.GetType().GetProperty("Key");

                    _colClassObj.Add(propInfo.GetValue((Player)objClass, null).ToString(), (Player)objClass);

                    objClass = null;
                    i += 1;
                }
            }
                    
        
        }

        public void Add(Player objClass)
        {
            //Pupose: Add a class object to the collection
            if (_colClassObj == null)
            {
                LoadData();
            }
            if (_colClassObj.ContainsKey(objClass.Key.ToString()) == true)
            {
                throw new ArgumentException("An element with the same key already exists in the collection.");
            }
            _colClassObj.Add(objClass.Key.ToString(), objClass);
        }

        public void Remove(object Key)
        {
            //Purpose: Remove a class object from the collection
            if (_colClassObj == null)
            {
                LoadData();
            }
            if (_colClassObj.Count > 0)
            {
                if (_colClassObj.ContainsKey(Key.ToString()) == true)
                {
                    _colClassObj.Remove(Key);
                }
            
            }
        }

        public void Clear()
        {
            _colClassObj.Clear();
        }

        public bool Contains(object Key)
        {
            if (_colClassObj == null)
            {
                LoadData();
            }
            if (_colClassObj.ContainsKey(Key.ToString()) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        
        }

        public bool Exists(object Key)
        {
            if (_colClassObj == null)
            {
                LoadData();
            }
            return _colClassObj.ContainsKey(Key.ToString());
        }

        public Player Item(Int32 Index)
        {
            //Porpoise: Return a single record class from the collection
            if (_colClassObj == null)
            {
                LoadData();
            }
            return (Player)_colClassObj[Index];
        }

        public void UpdateCollection(Player objClass)
        {
            //Purpose: Update collection with changes in object
            if (_colClassObj.ContainsKey(objClass.Key.ToString()) == true)
            {
                _colClassObj.Remove(objClass.Key);
            }
            Add(objClass);
        }

        public Int32 Count
        {
            //Purpose: Return the number of class objects in the collection
            get 
            {
                if (_colClassObj == null)
                {
                    LoadData();
                }
                return _colClassObj.Count;
            }
        }

        public string UpdateDB()
        {
            string functionReturnValue = "";

            functionReturnValue = _DE.SaveCollection((Hashlist)_colClassObj, _tableName, _spSave);

            return functionReturnValue;
            
        }

        public void SortCollection(Type objType, string psSortPropertyName, bool psAscending)
        {
            _colClassObj.Sort(objType, psSortPropertyName, psAscending);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            PropertyInfo[] objProperties = null;
            PropertyInfo propInfo = null;
            //Check to see if the Dispose has already been called.
            if (!(_Disposed))
            {
                //If disposeing equals true, dispose all managed and unmanaged resources.
                if ((disposing))
                {
                    if ((_Components != null))
                    {
                        //Dispose managed resource.
                        _Components.Dispose();
                    }
                    objProperties = base.GetType().GetProperties();
                    foreach (PropertyInfo propInfo_loopVariable in objProperties)
                    {
                        propInfo = propInfo_loopVariable;
                        propInfo = null;
                    }
                    objProperties = null;
                }
            }
            _Disposed = true;
        }
    }
}
