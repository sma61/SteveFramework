using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace DataObjectsSample
{
    class PublicFunctions
    {
        public enum VK_CHECK
        { 
            VK_Alpha,
            VK_Alpha_Numeric,
            VK_Numeric_Integer,
            VK_Numeric_Decimal,
            VK_Numeric_Int_Dash
        }

        public enum PROC_DATA_CLASS
        { 
            PDC_SetDataProperToCtlValue,
            PDC_SetCtlValueToDataProp
        }

        public enum PROC_CONTROLS
        { 
            PC_Mandatory,
            PC_Disable,
            PC_ReadOnly,
            PC_DisplayValuesFromGrid,
            PC_Clear,
            PC_Enable,
            PC_NotReadOnly
        }

        public static void processControls(Control.ControlCollection ctlColl, PROC_CONTROLS action, bool includeGrids = false)
        {
            foreach (System.Windows.Forms.Control c in ctlColl)
            {

                switch (action)
                {
                    //grids may need to be cleared but don't have the tag proper which this will skip
                    //normally skips controls where tag prop is empty
                    case PROC_CONTROLS.PC_Clear:
                        if ((Convert.ToString(c.Tag) != "" && Convert.ToString(c.Tag) != "^" && c.Tag != null) ||
                            c.GetType().Name == "FormattedGrid")
                        {
                            switch (c.GetType().Name)
                            {
                                case "CheckBox":
                                    CheckBox ck = (CheckBox)c;
                                    ck.CheckState = CheckState.Unchecked;
                                    break;

                                case "RadioButton":
                                    RadioButton rb = (RadioButton)c;
                                    rb.Checked = false;
                                    break;

                                case "FormattedGrid": //this is a custom grid control that was formatted so I didn't have to format in everyproject for consistency
                                    if (includeGrids)
                                    {
                                        //UltraGrid ugrid = (UltraGrid)c;  //this is a Infragistics grid
                                        //ugrid.DataSource = null;
                                    }
                                    break;

                                default:
                                    c.Text = "";
                                    break;
                            }
                        }

                        break;
                }

                //recursion - if this control is a container and has children, then loop thru the children and so on
                if (c.Controls.Count > 0)
                {
                    processControls(c.Controls, action, includeGrids);
                }
            
            }
        
        }

        public static void processDataClass(System.Windows.Forms.Control.ControlCollection controls, PROC_DATA_CLASS action, ref List<object> objColl)
        {
            PropertyInfo propInfo;
            string strPropName;
            object strPropValue;
            //object obj;
            //int x;
            string strTableName;
            string strTagTable;
            string strPropDataType;
            object value;

            //loop thru the objects in the objCollection passed in - List
            for (int i = 0; i <= objColl.Count - 1; i++)
            {

                string strObjName = objColl[i].ToString();
                int startChar = strObjName.IndexOf(".") + 1;
                int lenString = strObjName.Length - startChar;
                strTableName = strObjName.Substring(startChar, lenString);


                foreach (Control c in controls)
                {

                    switch (action)
                    {
                        case PROC_DATA_CLASS.PDC_SetCtlValueToDataProp:
                            //the tag is case sensitive and needs to match the property in the data class
                            if (Convert.ToString(c.Tag) != "" && Convert.ToString(c.Tag) != "^" && c.Tag != null)
                            {
                                int endTagChar = c.Tag.ToString().IndexOf(".");
                                strTagTable = c.Tag.ToString().Substring(0, endTagChar);

                                if (strTagTable.ToUpper() == strTableName.ToUpper())
                                {
                                    int startPropChar = c.Tag.ToString().IndexOf(".") + 1;
                                    int lenPropChar = c.Tag.ToString().Length - startPropChar - 2; //-2 for the ,X at the end for the mandatory field
                                    strPropName = c.Tag.ToString().Substring(startPropChar, lenPropChar);
                                    propInfo = objColl[i].GetType().GetProperty(strPropName);
                                    //we can have diff datatypes coming out but to diplay we need to convert all toString
                                    strPropValue = propInfo.GetValue(objColl[i], null);

                                    switch (c.GetType().Name)
                                    {
                                        case "UltraDateTimeEditor": // Infragistics but can be change of out of box MS control
                                            if (strPropValue != null)
                                            {
                                                if (strPropValue.ToString() == "12:00:00 AM")
                                                {
                                                    c.Text = c.Text;
                                                }
                                                else
                                                {
                                                    c.Text = strPropValue.ToString();
                                                }
                                            }
                                            else
                                            {
                                                c.Text = null;
                                            }
                                            break;

                                        case "CheckBox":
                                            CheckBox chk = (CheckBox)c;
                                            if (strPropValue.ToString().ToUpper() == "TRUE")
                                            {
                                                chk.CheckState = CheckState.Checked;
                                            }
                                            else
                                            {
                                                chk.CheckState = CheckState.Unchecked;
                                            }
                                            break;

                                        case "RadioButton":
                                            RadioButton rb = (RadioButton)c;
                                            rb.Checked = Convert.ToBoolean(strPropValue);
                                            break;

                                        default:
                                            if (strPropValue != null)
                                            {
                                                if (strPropValue.ToString().Trim() != "12:00:00 AM")
                                                {
                                                    //FK with null value will 0 in the data class so we need to skip 0
                                                    if (strPropValue.ToString() != "0")
                                                    {
                                                        c.Text = strPropValue.ToString();
                                                    }
                                                    else
                                                    {
                                                        c.Text = "";
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                c.Text = "";
                                            }
                                            break;

                                    }
                                }
                            }
                            break;

                        case PROC_DATA_CLASS.PDC_SetDataProperToCtlValue:

                            if (Convert.ToString(c.Tag) != "" && Convert.ToString(c.Tag) != "^" && c.Tag != null)
                            {
                                int endTagChar = c.Tag.ToString().IndexOf(".");
                                strTagTable = c.Tag.ToString().Substring(0, endTagChar);

                                if (strTagTable.ToUpper() == strTableName.ToUpper())
                                {
                                    int startFldChar = c.Tag.ToString().IndexOf(".") + 1;
                                    int lenFldChar = c.Tag.ToString().Length - startFldChar - 2; //-2 for the ,X at end fo mandatory
                                    string strField = c.Tag.ToString().Substring(startFldChar, lenFldChar);
                                    //need to handle if we are trying to set the 'Key' property which is not allowed
                                    if (strField.ToUpper() != "KEY")
                                    {
                                        propInfo = objColl[i].GetType().GetProperty(strField);

                                        switch (c.GetType().Name)
                                        {
                                            case "UltraDateTimeEditor": //name of an Infragistics control
                                                if (c.Text == "__/__/____" || c.Text == "")
                                                {
                                                    value = null;
                                                }
                                                else
                                                {
                                                    value = Convert.ToDateTime(c.Text);
                                                }
                                                propInfo.SetValue(objColl[i], value, null);
                                                break;

                                            case "CheckBox":
                                                CheckBox chk = (CheckBox)c;
                                                if (chk.Checked)
                                                {
                                                    value = true;
                                                }
                                                else
                                                {
                                                    value = false;
                                                }
                                                propInfo.SetValue(objColl[i], value, null);
                                                break;

                                            case "RadioButton":
                                                RadioButton rb = (RadioButton)c;
                                                value = rb.Checked;
                                                propInfo.SetValue(objColl[i], value, null);
                                                break;

                                            case "Label":
                                                //do nothing because lables are read only
                                                //there may be some cases that we need to fill a value in the data class
                                                break;

                                            default:
                                                strPropDataType = propInfo.PropertyType.ToString();
                                                propInfo.SetValue(objColl[i], getValue(strPropDataType, c.Text), null);
                                                break;

                                        }

                                    }
                                }

                            }
                            break;

                    }
                    //time to recurse

                    if (c.Controls.Count > 0)//if the control has child controls
                    {
                        processDataClass(c.Controls, action, ref objColl);
                    }

                }


            }

        }

        private static object getValue(string propDataType, string value)
        {
            object gVal = null;


            switch (propDataType)
            {
                case "System.String":
                    gVal = (string)value;
                    break;

                case "System.Decimal":
                    gVal = Convert.ToDecimal(value);
                    break;

                case "System.Long":
                    long lngVal = Int64.Parse(value);
                    gVal = lngVal;
                    break;

                case "System.Boolean":
                    if (value == null)
                    {
                        gVal = false;
                    }
                    else
                    {
                        gVal = Convert.ToBoolean(value);
                    }
                    break;

                case "System.Integer":
                    gVal = Convert.ToInt16(value);
                    break;

                case "System.Date":
                    gVal = Convert.ToDateTime(value);
                    break;

                case "System.DateTime":
                    gVal = Convert.ToDateTime(value);
                    break;

                case "System.Int32":
                    if (value == null || value.Trim() == "" || value == "0")
                    {
                        gVal = null;
                    }
                    else
                    {
                        gVal = Convert.ToInt32(value);
                    }
                    break;

                case "System.Int16":
                    if (value == null || value.Trim() == "" || value == "0")
                    {
                        gVal = null;
                    }
                    else
                    {
                        gVal = Convert.ToInt16(value);
                    }
                    break;

                case "System.Double":
                    if (value.Trim() == "")
                    {
                        value = "0";
                    }
                    gVal = Convert.ToDouble(value);
                    break;
                    
            }

            return gVal;

        }

    }
}
