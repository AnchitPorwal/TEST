﻿using Beesys.Wasp.Workflow;
using BeeSys.Wasp.KernelController; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeeSys.Wasp3D.Utility
{
    public partial class frmUdtUpdate : Form
    {

//this is a test
        #region Class Variables
        //Note : Our parent table is Category table and child table is Data table.
        UDTHandler _serviceUDTHandler = null;
        string _parentTableName = null;
        string _childTableName = null;
        string _parentPrimaryKeyColumnName = null;
        string _childPrimaryKeyColumnName = null;
        string _textColumn = "Text";
        string _detailColumn = "Details"
        #endregion

        #region Initialize UI Form
        public frmUdtUpdate()
        {
            InitializeComponent();
test();
            this.FormClosing += FrmUdtUpdate_FormClosing ;
        }

       
        #endregion

public void test()
{
int i =0;
int j=10;
int k=j/i;
}

        #region Establish Connection with KC
        private void btnConnect_Click(object sender, EventArgs e)
        {
            
            try
            {
                //To connect with KC and fetch data from UDT
                if (_serviceUDTHandler == null)
                {
                     _serviceUDTHandler = new UDTHandler(@".\SampleUDT"); // UDT Path i.e SampleUDT
                    //_serviceUDTHandler = new UDTHandler("CC2E882C-B402-40FB-881A-CA251FE9CF01"); //SampleUDT id
                    _serviceUDTHandler.OnResponse += _serviceUDTHandler_OnResponse;
                    //Connect with KC
                    _serviceUDTHandler.ConnectWithKC();
                    //Load data in dataset from KC
                    _serviceUDTHandler.LoadData();
                }
                //Findig parent table name as well as child table name  and it's primary key respectively.
                _parentTableName = _serviceUDTHandler.DataSet.Tables[1].TableName;
                _childTableName = _serviceUDTHandler.DataSet.Tables[2].TableName;

                _parentPrimaryKeyColumnName = _serviceUDTHandler.DataSet.Tables[_parentTableName].PrimaryKey[0].ColumnName;
                _childPrimaryKeyColumnName = _serviceUDTHandler.DataSet.Tables[_childTableName].PrimaryKey[0].ColumnName;

                DataTable parentDataTable =  _serviceUDTHandler.DataSet.Tables[_parentTableName];

                cmbBoxCategory.DisplayMember = _textColumn;
                cmbBoxCategory.ValueMember = _parentPrimaryKeyColumnName;
                cmbBoxCategory.DataSource = parentDataTable;


                //Fill data in parent comboBox.
                //foreach (DataRow dr in parentDataTable.Rows)
                //{
                //    cmbBoxCategory.Items.Add(dr[_textColumn]);
                //}

                this.btnConnect.BackColor = Color.LawnGreen;
                this.btnConnect.Text = "Connected with KC";

                MessageBox.Show("Connected with KC !!");
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        /// <summary>
        /// When you add, delete and update operation in UDT.
        /// </summary>
        /// <param name="objUdtArgs"></param>
        private void _serviceUDTHandler_OnResponse(UdtArgs objUdtArgs)
        {
            try
            {
                this.Invoke(new System.Action(() =>
                {
                    try
                    {
                        string tableName = "";
                        string primaryKeyValue = "";

                        DataTable parentDataTable = _serviceUDTHandler.DataSet.Tables[_parentTableName];
                        DataTable childDataTable = _serviceUDTHandler.DataSet.Tables[_childTableName];
                       
                        int parentPrimaryKeyIndex = parentDataTable.Columns.IndexOf(_parentPrimaryKeyColumnName);
                        int childPrimaryKeyIndex = childDataTable.Columns.IndexOf(_childPrimaryKeyColumnName);

                        switch (objUdtArgs.Action.ToLower())
                        {
                            case "updaterow":
                                tableName = _serviceUDTHandler.GetTableName(objUdtArgs.ActionParams[0].UpdateRowParams.TableName);
                                if (string.Compare(tableName, _parentTableName, true) == 0)
                                {                                    
                                    primaryKeyValue = objUdtArgs.ActionParams[0].UpdateRowParams.Data[parentPrimaryKeyIndex];
                                }
                                else if (string.Compare(tableName, _childTableName, true) == 0)
                                {
                                    primaryKeyValue = objUdtArgs.ActionParams[0].UpdateRowParams.Data[childPrimaryKeyIndex];
                                }
                                break;

                            case "deleterow":
                                tableName = _serviceUDTHandler.GetTableName(objUdtArgs.ActionParams[0].DeleteRowParams.TableName);
                                if (string.Compare(tableName, _parentTableName, true) == 0)
                                {
                                    primaryKeyValue = objUdtArgs.ActionParams[0].DeleteRowParams.PrimaryKey[parentPrimaryKeyIndex];
                                }
                                else if (string.Compare(tableName, _childTableName, true) == 0)
                                {
                                    primaryKeyValue = objUdtArgs.ActionParams[0].DeleteRowParams.PrimaryKey[childPrimaryKeyIndex];
                                }
                                break;

                            case "addrow":
                                tableName =  _serviceUDTHandler.GetTableName(objUdtArgs.ActionParams[0].AddRowParams.TableName);
                                if (string.Compare(tableName, _parentTableName, true) == 0)
                                {
                                    primaryKeyValue = objUdtArgs.ActionParams[0].AddRowParams.Data[parentPrimaryKeyIndex];
                                }
                                else if (string.Compare(tableName, _childTableName, true) == 0)
                                {
                                    primaryKeyValue = objUdtArgs.ActionParams[0].AddRowParams.Data[childPrimaryKeyIndex];
                                }
                                break;

                        }
                        if (string.Compare(tableName, _parentTableName, true) == 0)
                        {                          
                            cmbBoxCategory.DisplayMember = _textColumn;
                            cmbBoxCategory.ValueMember = _parentPrimaryKeyColumnName;
                            cmbBoxCategory.DataSource = parentDataTable;
                            if (string.Compare(objUdtArgs.Action, "deleterow", true) == 0)
                            {
                                if(parentDataTable.Rows.Count >0)
                                {
                                    cmbBoxCategory.SelectedValue = parentDataTable.Rows[0][parentPrimaryKeyIndex];
                                }
                            }
                            else
                            {
                                cmbBoxCategory.SelectedValue = primaryKeyValue;
                            }
                           
                           
                        }
                        else if (string.Compare(tableName, _childTableName, true) == 0)
                        { 
                           
                            var primaryParentKey = cmbBoxCategory.SelectedValue.ToString ();
                            var rows = childDataTable.Rows.Cast<DataRow>().ToArray()
                                       .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), primaryParentKey, true) == 0).ToArray();
                            DataTable dtTable = null;
                            if (rows != null && rows.Count() > 0)
                            {
                                dtTable = rows.CopyToDataTable();
                            }
                            else
                            {
                                dtTable = childDataTable.Clone();

                            }

                            cmbBoxData.DisplayMember = _textColumn;
                            cmbBoxData.ValueMember = _childPrimaryKeyColumnName;
                            cmbBoxData.DataSource = dtTable;
                           
                            if (string.Compare(objUdtArgs.Action, "deleterow", true) == 0)
                            {
                                if (parentDataTable.Rows.Count > 0)
                                {
                                    txtBoxDetailsData.Text = "";
                                    txtBoxTextData.Text = "";
                                    cmbBoxData.SelectedIndex = 0;
                                }                            
                            }
                            else
                            {
                                cmbBoxData.SelectedValue = primaryKeyValue;
                            }                            
                        }
                         

                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteLog(ex);
                    }
                }));
                 


            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        #endregion

        #region Category Table (Add Delete Update Operation)

        /// <summary>
        /// Select Category table data (By name) in comboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbBoxCategory_SelectedIndexChanged(object sender, EventArgs e) 
        {
            try
            {
                //Find the datarow with selected item in combobox
                DataRow dataRow = _serviceUDTHandler.DataSet.Tables[_parentTableName].Rows.Cast<DataRow>()
                                      .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), cmbBoxCategory.SelectedValue.ToString(), true) == 0).FirstOrDefault();

                txtBoxTextCategory.Text = dataRow[_textColumn].ToString();

                txtBoxDetailsCategory.Text = dataRow[_detailColumn].ToString();
                //To show category text on data tabpanel
                lblCategoryName.Text = dataRow[_textColumn].ToString();

                string primaryKeyparent = dataRow[_parentPrimaryKeyColumnName].ToString();

                var rows  = _serviceUDTHandler.DataSet.Tables[_childTableName].Rows.Cast<DataRow>().ToArray()
                                       .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), primaryKeyparent, true) == 0).ToArray();
                DataTable dtTable = null;
                if (rows != null && rows.Count ()>0) 
                {
                    dtTable = rows.CopyToDataTable();
                }
                else
                {
                    dtTable = _serviceUDTHandler.DataSet.Tables[_childTableName].Clone();

                }
                txtBoxTextData.Text = "";
                txtBoxDetailsData.Text = "";
                cmbBoxData.DisplayMember = _textColumn;
                cmbBoxData.ValueMember = _childPrimaryKeyColumnName;
                cmbBoxData.DataSource = dtTable;
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        /// <summary>
        /// Add Data in Category table UDT.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            //bool bCheck = false;
            try
            {
                if (_serviceUDTHandler != null)
                {

                    if (!string.IsNullOrEmpty(txtBoxTextCategory.Text))
                    {
                        int parentId = 1;//By default Category table has it's parent that id is 1.

                        Dictionary<string, object> dicColumnValue = new Dictionary<string, object>();

                        dicColumnValue[_textColumn] = txtBoxTextCategory.Text;
                        dicColumnValue[_detailColumn] = txtBoxDetailsCategory.Text;
                        //Add data in UDT 
                        _serviceUDTHandler.Add(_parentTableName, _textColumn, parentId, dicColumnValue);
                        //After adding data in UDT, also add item in combox
                         
                        MessageBox.Show("Added Successfully");
                    }
                    else
                    {
                        MessageBox.Show("Please Enter Text");
                    }
                }
                else
                {
                    MessageBox.Show("Connect with KC!");
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }


        }
        /// <summary>
        /// Update Category table data in UDT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateCategory_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceUDTHandler != null)
                {
                    if (cmbBoxCategory.SelectedIndex != -1)
                    {
                            DataRow pRow = _serviceUDTHandler.DataSet.Tables[_parentTableName].Rows.Cast<DataRow>()
                                              .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), cmbBoxCategory.SelectedValue.ToString(), true) == 0).FirstOrDefault();
                            
                        string sDetailsName = txtBoxDetailsCategory.Text;
                        pRow[_textColumn] = txtBoxTextCategory.Text;
                        pRow[_detailColumn] = sDetailsName;

                            //Update date in UDT
                            _serviceUDTHandler.UpdateRow(pRow);

                            MessageBox.Show("Updated Successfully!!");
                    }
                    else
                    {
                        MessageBox.Show("Select Category");
                    }
                }
                else
                {
                    MessageBox.Show("Connect with KC!");
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        /// <summary>
        /// Delete Category data from UDT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            try
            {

                if (_serviceUDTHandler != null)
                {
                    if (cmbBoxCategory.SelectedIndex != -1)
                    {
                        Dictionary<string, object> dicColumnValue = new Dictionary<string, object>();
                        int parentId = FindParentId();
                        if (parentId != -1)
                        {
                            dicColumnValue[_parentPrimaryKeyColumnName] = parentId;

                            //Delete data from UDT
                            _serviceUDTHandler.Remove(_parentTableName, dicColumnValue);
                            lblCategoryName.Text = "";
                            MessageBox.Show("Deleted successfully!!");
                        }
                        else
                        {
                            MessageBox.Show("Data not found!!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select Category");
                    }
                }
                else
                {
                    MessageBox.Show("Connect with KC!");
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        /// <summary>
        /// Find primaryKey of parent table which data you want to delete.
        /// </summary>
        /// <param name="sCategoryName">Select name in parent table</param>
        /// <returns></returns>
        private int FindParentId()
        {
            //Find the datarow with selected item in Category combobox
            var categoryRow = _serviceUDTHandler.DataSet.Tables[_parentTableName].Rows.Cast<DataRow>()
                              .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), cmbBoxCategory.SelectedValue.ToString(), true) == 0).FirstOrDefault();

            if (categoryRow != null)
            {
                return (int)categoryRow[_parentPrimaryKeyColumnName];
            }

            return -1;
        }
        /// <summary>
        /// First you need to connect with KC to fetch data from the UDT.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///  
        #endregion

        #region Child Table (Add Delete Update Operation)
        /// <summary>
        /// Add Child data in UDT.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddData_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceUDTHandler != null)
                {
                    //Find the datarow with selected item in Category combobox
                    var dtrow = _serviceUDTHandler.DataSet.Tables[_parentTableName].Rows.Cast<DataRow>().ToArray()
                                  .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), cmbBoxCategory.SelectedValue.ToString(), true) == 0).FirstOrDefault();

                    string pId = dtrow[_parentPrimaryKeyColumnName].ToString(); 

                    //if (existChild.ItemArray[2].ToString() != txtBoxNameChild.Text)
                    if (!string.IsNullOrEmpty(txtBoxTextData.Text))
                    {
                        Dictionary<string, object> dicColumnValue = new Dictionary<string, object>();
                        int parentId = Convert.ToInt32(pId);
                        string sDetailsDataName = txtBoxDetailsData.Text; ;
                        dicColumnValue[_textColumn] = txtBoxTextData.Text;
                        dicColumnValue[_detailColumn] = sDetailsDataName;

                        //Add data in UDT
                        _serviceUDTHandler.AddRow(_childTableName, _textColumn, parentId, dicColumnValue);


                        MessageBox.Show("Added Successfully!!");
                    }
                    else
                    {
                        MessageBox.Show("Please Enter Text");
                    }
                }
                else
                {
                    MessageBox.Show("Connect with KC!!");
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        /// <summary>
        /// Delete child data from UDT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteData_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceUDTHandler != null)
                {
                    if (cmbBoxData.SelectedIndex != -1)
                    {
                        Dictionary<string, object> dicColumnValue = new Dictionary<string, object>();
                        int childId = FindChildId();
                        if (childId != -1)
                        {
                            dicColumnValue[_childPrimaryKeyColumnName] = childId;
                            //Delete data from UDT
                            _serviceUDTHandler.Remove(_childTableName, dicColumnValue);
                        }
                        MessageBox.Show("Deleted Successfully!!");
                    }
                    else
                    {
                        MessageBox.Show("Select Data");
                    }
                }
                else
                {
                    MessageBox.Show("Connect with KC!!");
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        /// <summary>
        /// Find primary key of child table which data you want to delete.
        /// </summary>
        /// <param name="sChildName"></param>
        /// <returns></returns>
        private int FindChildId()
        {
            //Find the datarow with selected item in child combobox
            var dataRow = _serviceUDTHandler.DataSet.Tables[_childTableName].Rows.Cast<DataRow>()
                               .Where(row => string.Compare(row[_childPrimaryKeyColumnName].ToString(), cmbBoxData.SelectedValue.ToString(), true) == 0).FirstOrDefault();
            if (dataRow != null)
            {
                return (int)dataRow[_childPrimaryKeyColumnName];
            }

            return -1;
        }
        /// <summary>
        /// Update child data in UDT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateData_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceUDTHandler != null)
                {
                    if (cmbBoxData.SelectedIndex != -1)
                    {
                        //Find the datarow with selected item in parent combobox
                        var dtrow = _serviceUDTHandler.DataSet.Tables[_parentTableName].Rows.Cast<DataRow>().ToArray()
                                           .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), cmbBoxCategory.SelectedValue.ToString(), true) == 0).FirstOrDefault();
                        
                        string parentId = dtrow[_parentPrimaryKeyColumnName].ToString();
                        //Find the datarow with selected item in combobox combobox
                        var datarow = _serviceUDTHandler.DataSet.Tables[_childTableName].Rows.Cast<DataRow>().ToArray()
                                        .Where(row => string.Compare(row[_parentPrimaryKeyColumnName].ToString(), parentId, true) == 0
                                        && string.Compare(row[_childPrimaryKeyColumnName].ToString(), cmbBoxData.SelectedValue.ToString(), true) == 0).FirstOrDefault();

                        string sDetailsData = txtBoxDetailsData.Text;
                        datarow[_textColumn] = txtBoxTextData.Text;
                        datarow[_detailColumn] = sDetailsData;

                        //update data in UDT
                        _serviceUDTHandler.UpdateRow(datarow);

                        MessageBox.Show("Updated Successfully!!");
                        
                    }
                    else
                    {
                        MessageBox.Show("Select Data");
                    }
                }
                else
                {
                    MessageBox.Show("Connect with KC");
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }

        /// <summary>
        /// Select child table data (By name) in comboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbBoxData_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //if Category combobox not selected.
                if (cmbBoxCategory.SelectedIndex == -1)
                {
                    MessageBox.Show("Choose Category Name!!"); 
                }
                else
                {
                    //Find the datarow with selected item in combobox
                    DataRow dtRow = _serviceUDTHandler.DataSet.Tables[_childTableName].Rows.Cast<DataRow>().ToArray()
                                    .Where(row => string.Compare(row[_textColumn].ToString(), cmbBoxData.Text, true) == 0).FirstOrDefault();

                        //Fil data Name textbox and detail textbox respectively.
                        txtBoxTextData.Text = dtRow[_textColumn].ToString();
                        txtBoxDetailsData.Text = dtRow[_detailColumn].ToString();
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }

        /// <summary>
        /// Need to dispose the service client and release the resources that occupied by application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmUdtUpdate_FormClosing(object sender, FormClosingEventArgs e) 
        {
            try
            {
                if (_serviceUDTHandler != null)
                {
                    _serviceUDTHandler.Shutdown();
                    _serviceUDTHandler = null;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        #endregion

    }
 
}
