using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace DataObjectsSample
{
    class DataEngine
    {
        private string _DataSource;

        public DataEngine()
        {
            _DataSource = "";

            try
            {
                //try to find the database name from the app.config 
                _DataSource = System.Configuration.ConfigurationManager.AppSettings["DataSource"].ToString();
            }
            catch
            {
                //default if nothing is found in app.config or appname.exe.config
                _DataSource = "localhost";
                MessageBox.Show("No app.config \"DataSource\" entry. Defaulting to " + _DataSource);
            }

        }

        private SqlConnection Connect(string DatabaseName = "")
        {
            if (DatabaseName == "")
            {
                DatabaseName = "localhost";
            }

            SqlConnection sqlConn = new SqlConnection("Data Source=" + _DataSource + ";Initial Catalog=" + DatabaseName + ";Integrated Security=SSPI;Application Name=" + Application.ProductName);

            try
            {
                sqlConn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Could not connect to " + _DataSource, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return sqlConn;
        }

        public string SQLFind(string strSQL, string dbName = "")
        {
            //only returns one value

            SqlConnection con = Connect(dbName);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSQL;
            cmd.CommandTimeout = 180;
            cmd.Connection = con;

            SqlDataReader reader = cmd.ExecuteReader();
            string value = "";

            reader.Read();
            if (reader.HasRows)
            {
                value = reader[0].ToString();
            }
            return value;

        }

        //dsName is to give the dataset a name when needed for Crystal Reports binding
        public DataSet GetSQLDataSet(string strSQL, string database, string dsName)
        {
            SqlConnection con = Connect(database);
            SqlDataAdapter adp = new SqlDataAdapter();
            DataSet dset = new DataSet();
            SqlCommand cmd = new SqlCommand();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSQL;
            cmd.CommandTimeout = 30;
            cmd.Connection = con;
            //set up adapter
            adp.SelectCommand = cmd;
            adp.TableMappings.Add("Table", dsName);

            try
            {
                adp.Fill(dset, dsName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                cmd.Dispose();
                adp.Dispose();
                con.Close();
                con.Dispose();
            }
            return dset;
        }

        public DataSet GetSQLDataSet(string strSQL)
        {
            return GetSQLDataSet(strSQL, "localhost", "CalledAnything");
        }

        public DataSet GetSQLDataSet(string strSQL, string dsName)
        {
            return GetSQLDataSet(strSQL, "localhost", dsName);
        }

        public DataSet GetSPDataSet(string spName, string spParamName, string spParamValue, string DatabaseName)
        {
            //fill a dataset from a stored procedure
            SqlConnection con = Connect(DatabaseName);
            SqlParameter parm = new SqlParameter();
            SqlCommand cmd = new SqlCommand();
            String[] arrParamValues;
            String[] arrParamNames;
            int i;
            SqlDataAdapter adp = new SqlDataAdapter();
            DataSet dset = new DataSet();
            string fieldName;

            //Set up the SQL Command object and data adapter
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;
            cmd.Connection = con;
            adp.SelectCommand = cmd;

            if (spParamValue.Length != 0)
            {

                arrParamValues = spParamValue.Split('^');
                arrParamNames = spParamName.Split('^');

                //make sure the number of field values matches the number of field names
                if (arrParamValues.GetUpperBound(0) != arrParamNames.GetUpperBound(0))
                {
                    MessageBox.Show("Fields and Names not matching", "Number of field values does not matcht the number of field names passed in", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }

                for (i = 0; i < arrParamValues.GetUpperBound(0); i++)
                {
                    if (arrParamValues[i].Trim() != "")
                    {
                        fieldName = arrParamNames[i];
                        parm = cmd.Parameters.Add(new SqlParameter("@" + fieldName, arrParamValues[i]));
                        parm.Direction = ParameterDirection.Input;
                    }
                }
            
            }

            try
            {
                adp.Fill(dset);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

            }
            finally
            {
                cmd.Dispose();
                adp.Dispose();
                con.Close();
                con.Dispose();

            }
            return dset;

        }

        public string SaveCollection(Hashlist objcollection, string TableName, string spSave)
        {
            string Result = "";
            //pass in the database name to the method - need to add arguement to the signature
            SqlConnection con = Connect("DatabaseName");

            //open connection as transactional
            SqlTransaction trans = con.BeginTransaction();

            //iterate thru the collection and send into SaveDataClass

            //TO DO - add the rest

            return Result;
        }

        public void LoadDataClass(ref object objClass, string spName, string tableName, string paramFieldNames, string paramValues, string DatabaseName)
        {
            //iterate through the properties of the class and fill them with any matching field names
            //from the SQL table or Stored Proc aliases(case senstive)
            DataSet dset;
            int i;
            PropertyInfo propInfo;
            DataRow row;

            if (tableName != "NONE")
            {
                dset = null;
            }
            else
            {
                dset = GetSPDataSet(spName, paramFieldNames, paramValues, DatabaseName);
            }

            if (dset != null)
            {
                i = 0;
                while (i < dset.Tables[0].Rows.Count)
                {
                    //iterate through the fields and assign values to the properties
                    foreach (DataColumn col in dset.Tables[0].Columns)
                    {
                        propInfo = objClass.GetType().GetProperty(col.ColumnName);

                      
                        row = dset.Tables[0].Rows[i];

                        try
                        {
                            propInfo.SetValue(objClass, row[col.ColumnName], null);
                        }
                        catch
                        {
                            //maybe pop up an error msg with some more info?
                            propInfo = null;
                        }

                    }
                    i++;
                
                
                }
            }

        
        }

        public string SaveDataClass(DictionaryBase objClass, string TableName, string spSave)
        {
            string rtnVal = "";
            SqlConnection con = Connect("LocalHost");
            SqlTransaction trans = con.BeginTransaction();

            try
            {
                rtnVal = SaveDataClass(objClass, TableName, spSave, ref con, ref trans);
                trans.Commit();
            }
            catch (Exception e)
            {
                MessageBox.Show("Saveing the Data Class didn't work. - " + e.Message + e.InnerException, "Your code stinks!!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                trans.Rollback();
                rtnVal = "FAILED";
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
            return rtnVal;

        }

        public string SaveDataClass(DictionaryBase objClass, string TableName, string spSave, ref SqlConnection con, ref SqlTransaction trans)
        {
            string Result = "";
            SqlDbType mSQLDBType;
            PropertyInfo propInfo;
            //this contains a delimited string using the the ^ symbol
            Array arrFldAttribs = GetTableAttributes(TableName);
            Int32 i;
            Int32 FieldLength;
            String FieldName;
            Boolean hasIDField = false;
            Boolean hasParam;

            try
            {
                //set up the sql command object
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                cmd.Transaction = trans;

                SqlParameter param = new SqlParameter();

                cmd.CommandText = spSave;

                i = 0;
                int q = arrFldAttribs.GetUpperBound(1);
                while (i <= q)
                {
                    //set the property from the data class -- not all fields have to be in data clase
                    propInfo = objClass.GetType().GetProperty(Convert.ToString(arrFldAttribs.GetValue(0, i)));

                    if (propInfo != null)
                    {
                        hasParam = true;

                        //do not send an empty key - sting vs GUID problem - may no longer need????
                        if (Convert.ToString(arrFldAttribs.GetValue(1, i)) != "System.Guid")
                        {
                            mSQLDBType = GetSQLDataType(Convert.ToString(arrFldAttribs.GetValue(1, i)), Convert.ToInt32(arrFldAttribs.GetValue(2, i)));
                            FieldName = Convert.ToString(arrFldAttribs.GetValue(0, i));

                            switch (Convert.ToString(arrFldAttribs.GetValue(1, i)))
                            {
                                case "System.String":
                                    FieldLength = Convert.ToInt32(arrFldAttribs.GetValue(2, i));
                                    param = cmd.Parameters.Add(new SqlParameter("@" + FieldName, mSQLDBType, FieldLength));
                                    break;

                                case "System.Int32":
                                    //added for FK, because when FK is null it defaults to 0
                                    //also when the PK is less then 0 we have a new record because in a collection we have to insert a pseudo keyy
                                    if (Convert.ToInt32(propInfo.GetValue(objClass, null)) > 0)
                                    {
                                        param = cmd.Parameters.Add(new SqlParameter("@" + FieldName, mSQLDBType));
                                    }
                                    else
                                    {
                                        hasParam = false;

                                    }
                                    break;

                                case "System.Int16":
                                    if (Convert.ToInt16(propInfo.GetValue(objClass, null)) > 0)
                                    {
                                        param = cmd.Parameters.Add(new SqlParameter("@" + FieldName, mSQLDBType));
                                    }
                                    else
                                    {
                                        hasParam = false;

                                    }
                                    break;

                                default:
                                    param = cmd.Parameters.Add(new SqlParameter("@" + FieldName, mSQLDBType));
                                    break;
                            }

                            if (hasParam == true)
                            {
                                param.Direction = ParameterDirection.Input;

                                if (propInfo.GetValue(objClass, null) == null)
                                {
                                    param.Value = System.DBNull.Value;
                                }
                                else
                                {
                                    SetParamValue(ref param, Convert.ToString(arrFldAttribs.GetValue(1, i)), propInfo.GetValue(objClass, null));
                                }
                            }
                        } //if System.guid -- don't use friggin guids
                    }//if propinfo

                    if (Convert.ToString(arrFldAttribs.GetValue(3, 1)) == "True")
                    {
                        hasIDField = true;
                    }

                    i++;
                } //while i

                //get the id back from the save
                if (hasIDField)
                {
                    param = cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int));
                    param.Direction = ParameterDirection.Output;
                }

                //need some set up to deleter, how do we check the delete flag of the data object
                // some classes may not have the IsDeleted property because we never delete things
                if (objClass.GetType().GetProperty("IsDeleted") != null)
                {
                    if (Convert.ToBoolean(objClass.GetType().GetProperty("IsDeleted").GetValue(objClass, null)) == true)
                    {
                        param = cmd.Parameters.Add("@Delete", SqlDbType.Bit);
                        param.Direction = ParameterDirection.Input;
                        param.Value = 1;
                    }
                }

                cmd.ExecuteNonQuery();

                if (hasIDField)
                {
                    string strLastIdentity = Convert.ToString(cmd.Parameters["@ID"].Value);
                    Result = strLastIdentity;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Saving the Data Class(" + TableName + ") didn't work. - " + e.Message + e.InnerException, "Your code ain't workin!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Result = "Failed";
            }
            finally
            {
                propInfo = null;
                arrFldAttribs = null;
            }
            return Result;

            }

        private Array GetTableAttributes(string TableName)
        {

            SqlConnection con = Connect("LocalDatabase");
            String SQLCmd = "SELECT * FROM " + TableName + " WHERE 1=0";
            SqlCommand cmd = new SqlCommand(SQLCmd, con);
            SqlDataReader rdr;
            DataTable schemaTable = null;
            String[,] arrAttributes = null;

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = SQLCmd;
            cmd.Connection = con;

            rdr = cmd.ExecuteReader(CommandBehavior.SchemaOnly);

            int i = 0;

            if (rdr != null)
            {
                schemaTable = rdr.GetSchemaTable();
                //get the number of rows in the table
                int numRows = schemaTable.Rows.Count;
                arrAttributes = new String[5, numRows];

                foreach (DataRow row in schemaTable.Rows)
                {
                    arrAttributes[0, i] = Convert.ToString(row["ColumnName"]);
                    arrAttributes[1, i] = Convert.ToString(row["DataType"]);
                    arrAttributes[2, i] = Convert.ToString(row["ColumnSize"]);
                    arrAttributes[3, i] = Convert.ToString(row["IsIdentity"]);
                    arrAttributes[4, i] = Convert.ToString(row["IsKey"]);
                    i++;
                }

            }// end if rdr

            con.Close();
            cmd.Dispose();
            schemaTable.Dispose();
            rdr = null;

            return arrAttributes;


        }

        private SqlDbType GetSQLDataType(string pCSharpType, Int32 pLength)
        {
            //convert the C# data type to the SQL Data Type
            SqlDbType pSQLType = new SqlDbType();

            switch (pCSharpType)
            {
                case "System.Int32":
                    pSQLType = SqlDbType.Int;
                    break;

                case "System.String":
                    if (pLength == 1)
                    {
                        pSQLType = SqlDbType.Char;
                    }
                    else
                    {
                        pSQLType = SqlDbType.VarChar;
                    }
                    break;

                case "System.DateTime":
                    if (pLength == 4)
                    {
                        pSQLType = SqlDbType.SmallDateTime;
                    }
                    else
                    {
                        pSQLType = SqlDbType.DateTime;
                    }
                    break;

                case "System.Int16":
                    pSQLType = SqlDbType.SmallInt;
                    break;

                case "System.Decimal":
                    if (pLength == 4)
                    {
                        pSQLType = SqlDbType.SmallMoney;
                    }
                    else
                    {
                        pSQLType = SqlDbType.Money;
                    }
                    break;

                case "System.Single":
                    pSQLType = SqlDbType.Real;
                    break;

                case "System.Boolean":
                    pSQLType = SqlDbType.Bit;
                    break;

                case "System.Byte[]":
                    pSQLType = SqlDbType.Image;
                    break;

                case "System.Byte":
                    pSQLType = SqlDbType.TinyInt;
                    break;

                case "System.Guid":
                    pSQLType = SqlDbType.UniqueIdentifier;
                    break;

                case "System.Double":
                    pSQLType = SqlDbType.Float;
                    break;

                default:
                    MessageBox.Show("Unable to convert C# type " + pSQLType + " to SQL type.", "Danger Will Robinson!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;

            }

            return pSQLType;

        }
        
        public string GetTableKeyName(string TableName)
        {
            //This tries to get the PK of the table, in the past the isKey would tell us but is not reliable because it would
            //also return the clusted index fields which may or may not be the4 PK. The OK in our database is usually the
            //identity column so we now use the isIdentity to determin the PK? Not sure what the heck this means in present day
            string SQLcmd = "SELECT * FROM " + TableName + " WHERE 1=0";
            SqlConnection con = Connect("DatabaseName"); //maybe set property of dataengine for database name
            SqlCommand cmd = new SqlCommand(SQLcmd, con);
            SqlDataReader rdr;
            DataTable schemaTable = new DataTable();
            DataRow dr;
            string strKeyName = "";

            try
            { 
                rdr = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
            }
            catch
            {
                //clean up your mess
                con.Dispose();
                cmd.Dispose();
                schemaTable.Dispose();
                return "NONE";
            }

            if (rdr != null)
            {
                schemaTable = rdr.GetSchemaTable();

                for (int i = 0; i < schemaTable.Rows.Count - 1; i++)
                {
                    dr = schemaTable.Rows[i];

                    if (dr["IsIdentity"].Equals(true))
                    {
                        //this should only ever be true once in our SQL world
                        strKeyName = dr["ColumnName"].ToString();
                    }                
                }
            }

            //clean up again - combine this in a finally?
            con.Dispose();
            cmd.Dispose();
            schemaTable.Dispose();

            return strKeyName;

        }

        private void SetParamValue(ref SqlParameter param, String pCSharpType, Object pValue)
        {
            //dynamically set the value of the sql passed parameter
            String pPassString = null;
            Int16 pPassInt16 = new Int16();
            Int32 pPassInt32 = new Int32();
            DateTime pPassDateTime = new DateTime();
            Decimal pPassDecimal = new Decimal();
            Single pPassSingle = new Single();
            Double pPassDouble = new Double();
            Boolean pPassBoolean = new Boolean();

            switch (pCSharpType)
            {
                case "System.Int32":
                    pPassInt32 = Convert.ToInt32(pValue);
                    param.Value = pPassInt32;
                    break;

                case "System.String":
                    pPassString = Convert.ToString(pValue);
                    param.Value = pPassString;
                    break;

                case "System.DateTime":
                    if (Convert.ToString(pValue) == "01/01/0001 12:00:00 AM" || Convert.ToString(pValue) == "1/1/0001 12:00:00 AM")
                    {
                        param.Value = System.DBNull.Value;
                    }
                    else
                    {
                        pPassDateTime = Convert.ToDateTime(pValue);
                        param.Value = pPassDateTime;
                    }
                    break;

                case "System.Int16":
                    pPassInt16 = Convert.ToInt16(pValue);
                    param.Value = pPassInt16;
                    break;

                case "System.Decimal":
                    pPassDecimal = Convert.ToDecimal(pValue);
                    param.Value = pPassDecimal;
                    break;

                case "System.Single":
                    pPassSingle = Convert.ToSingle(pValue);
                    param.Value = pPassSingle;
                    break;

                case "System.Boolean":
                    pPassBoolean = Convert.ToBoolean(pValue);
                    param.Value = pPassBoolean;
                    break;

                case "System.Byte[]":
                    param.Value = pValue;
                    break;

                case "System.Guid":
                    if (String.IsNullOrEmpty(Convert.ToString(pValue)) == false)
                    {
                        pPassString = Convert.ToString(pValue);
                        Guid passGuid = new Guid(pPassString);
                        param.Value = passGuid;
                    }
                    else
                    {
                        pPassString = Convert.ToString(pValue);
                        param.Value = pPassString;
                    }
                    break;

                case "System.Double":
                    pPassDouble = Convert.ToDouble(pValue);
                    param.Value = pPassDouble;
                    break;

                default:
                    MessageBox.Show("Can not set Parameter Value " + pCSharpType + " whild converting to SQL Type.", "Yo Dude! Something is friggin wrong.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
            }

        }

        public int ExecuteSQL(string SQL)
        {
            SqlConnection con = Connect("LocalHost");
            SqlCommand cmd = new SqlCommand();
            int result = 0;

            try
            {
                cmd = new SqlCommand(SQL, con);
                SqlDataReader rdr = cmd.ExecuteReader();
                rdr = null;
                result = 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Execute SQL Problem Dude!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
                con.Dispose();
            }

            return result;
        }

    }
}
