using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace DataObjectsSample
{
    public class Player: DictionaryBase, IDisposable
    {

        private DataEngine _DE = new DataEngine();

        private bool _IsDeleted;

        private IntPtr _Handle;
        private IContainer _Components;
        private bool _Disposed = false;

        private string _strKeyName;

        private const string _tableName = "TableName"; //updat this

        private const string _spSELECT = "SELECT Stored Proc Name";
        private const string _spSAVE = "SAVE Stored Proc Name";

        private const string DATABASE_NAME = "Database Name";

        public Player(string Key = "")
        {
            //Purpose:  Initialize class object and set properties - Constructor
            if (!(Key.Length == 0))
            {
                LoadData(Key);
            }
        }
        ~Player()
        {

            Dispose(false);
        }

        public void LoadData(string Key)
        {
            //Purpose:  Load Data values for the class properties
            object obj = this;
            _DE.LoadDataClass(ref obj, _spSELECT, "NONE", KeyName, Key, DATABASE_NAME);
            
        }

        public string UpdateDB()
        {
            //save SP should handle both INSERT\UPDATE so we only have to call one method in Code
            string returnVal = "";

            object obj = this;

            returnVal = _DE.SaveDataClass((DictionaryBase)obj, _tableName, _spSAVE);

            if (returnVal.ToUpper() == "FAILED")
            {
                returnVal = "Saving Record Failed";
                return returnVal;
            }

            //reset isNew adter the save in case it was true
            //returnVal = "1"; //success
            return returnVal.ToString();
        
        }

        public string Key
        {
            //purpose: Return Key value of this class
            get 
            {
                string[] arrKeyName;
                string strKeyVal = "";
                PropertyInfo propInfo = null;
                arrKeyName = KeyName.Split('^');

                for (int i = 0; i <= arrKeyName.GetUpperBound(0); i++)
                {
                    propInfo = this.GetType().GetProperty(arrKeyName[i]);
                    strKeyVal += Convert.ToString(propInfo.GetValue(this, null)) + "^";
                }

                if (!(strKeyVal.Length == 0))
                {
                    strKeyVal = strKeyVal.Substring(0, strKeyVal.Length - 1);
                }
                               
                return strKeyVal;
            }
        
        }

        public string KeyName
        {
            //purpose: Return the name of the Key for this class
            get
            {
                string functionReturnValue = null;
                functionReturnValue = _DE.GetTableKeyName(_tableName);
                return functionReturnValue;
            }
        }

        //Start properties here, prop name needs to match name in table, case sensitive



        public bool IsDeleted { get; set; }
     

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
