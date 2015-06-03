// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// The ASP:CheckBoxField doesn't work very well for retrieving changed values, especially when the value is changed from True to False (weird)
    /// This CheckBoxEditableField works like the ASP:CheckBoxField except it gives the CheckBox's IDs so their changed values will consistantly persist on postbacks
    /// </summary>
    public class SelectField : TemplateField, INotRowSelectedField
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectField" /> class.
        /// </summary>
        public SelectField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-select-field";
            this.ItemStyle.CssClass = "grid-select-field";

        }

        #region Properties

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public List<object> SelectedKeys 
        {
            get { return _selectedKeys; }
            internal set { _selectedKeys = value; } 
        }
        private List<object> _selectedKeys = new List<object>();

        /// <summary>
        /// Gets or sets the selection mode.
        /// </summary>
        /// <value>
        /// The selection mode.
        /// </value>
        public SelectionMode SelectionMode
        {
            get
            {
                var obj = ViewState["SelectionMode"];
                if (obj != null)
                {
                    return (SelectionMode)obj;
                }
                
                SelectionMode = SelectionMode.Multiple;
                return SelectionMode.Multiple;
            }
            set
            {
                ViewState["SelectionMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Select All checkbox when in multiselect mode
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show select all]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSelectAll
        {
            get
            {
                return ViewState["ShowSelectAll"] as bool? ?? true;
            }
            set
            {
                ViewState["ShowSelectAll"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the data visible field.
        /// </summary>
        /// <value>
        /// The data visible field.
        /// </value>
        public string DataVisibleField
        {
            get
            {
                return ViewState["DataVisibleField"] as string;
            }

            set
            {
                ViewState["DataVisibleField"] = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the data selected field.
        /// </summary>
        /// <value>
        /// The data selected field.
        /// </value>
        public string DataSelectedField
        {
            get
            {
                return ViewState["DataSelectedField"] as string;
            }

            set
            {
                ViewState["DataSelectedField"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the data field.
        /// </summary>
        /// <value>
        /// The data field.
        /// </value>
        public string DataTextField
        {
            get
            {
                return ViewState["DataTextField"] as string;
            }

            set
            {
                ViewState["DataTextField"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the checkbox tooltip
        /// </summary>
        /// <value>
        /// The checkbox tooltip
        /// </value>
        public string Tooltip
        {
            get
            {
                return ViewState["Tooltip"] as string;
            }

            set
            {
                ViewState["Tooltip"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the column.
        /// </summary>
        /// <value>
        /// The index of the column.
        /// </value>
        public int ColumnIndex
        {
            get
            {
                return ViewState["ColumnIndex"] as int? ?? 0;
            }
            set
            {
                ViewState["ColumnIndex"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField" />.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            base.Initialize( sortingEnabled, control );

            this.HeaderTemplate = new SelectFieldHeaderTemplate();
            this.ItemTemplate = new SelectFieldTemplate();
            var grid = control as Grid;
            if ( grid != null )
            {
                ColumnIndex = grid.Columns.IndexOf( this );
            }

            string script = string.Format( @"
    $('input[id$=""_cbSelectHead_{0}""]').click( function() {{
    $(this).closest('table').find('input[id$=""_cbSelect_{0}""]').prop('checked', $(this).prop('checked'));
    }});
", ColumnIndex );
            ScriptManager.RegisterStartupScript( control, control.GetType(), "select-all-" + ColumnIndex.ToString(), script, true );

            return false;
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class SelectFieldTemplate : ITemplate
    {

        #region Properties

        /// <summary>
        /// Gets the selection mode.
        /// </summary>
        /// <value>
        /// The selection mode.
        /// </value>
        public SelectionMode SelectionMode { get; private set; }

        /// <summary>
        /// Gets the data visible field.
        /// </summary>
        /// <value>
        /// The data visible field.
        /// </value>
        public string DataVisibleField { get; private set; }

        /// <summary>
        /// Gets the data selected field.
        /// </summary>
        /// <value>
        /// The data selected field.
        /// </value>
        public string DataSelectedField { get; private set; }

        /// <summary>
        /// Gets the data text field.
        /// </summary>
        /// <value>
        /// The data text field.
        /// </value>
        public string DataTextField { get; private set; }

        /// <summary>
        /// Gets the index of the column.
        /// </summary>
        /// <value>
        /// The index of the column.
        /// </value>
        public int ColumnIndex { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            var cell = container as DataControlFieldCell;
            if (cell != null)
            {
                var selectField = cell.ContainingField as SelectField;
                if ( selectField != null )
                {
                    SelectionMode = selectField.SelectionMode;
                    DataVisibleField = selectField.DataVisibleField;
                    DataSelectedField = selectField.DataSelectedField;
                    DataTextField = selectField.DataTextField;
                    ColumnIndex = selectField.ColumnIndex;

                    CheckBox cb = null;

                    if ( SelectionMode == SelectionMode.Multiple )
                    {
                        cb = new CheckBox();
                    }
                    else
                    {
                        cb = new RockRadioButton();
                    }

                    cb.ID = "cbSelect_" + ColumnIndex.ToString();
                    cb.DataBinding += cb_DataBinding;
                    cell.ToolTip = selectField.Tooltip;
                    cell.Controls.Add( cb );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the DataBinding event of the cb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void cb_DataBinding( object sender, EventArgs e )
        {
            var cb = sender as CheckBox;
            if ( cb != null )
            {
                GridViewRow gridViewRow = cb.NamingContainer as GridViewRow;

                if ( gridViewRow.DataItem != null )
                {
                    if ( !string.IsNullOrWhiteSpace( DataTextField ) )
                    {
                        object dataValue = DataBinder.Eval( gridViewRow.DataItem, DataTextField );
                        cb.Text = dataValue.ToString();
                    }
                    else
                    {
                        cb.Text = string.Empty;
                    }

                    if ( !string.IsNullOrWhiteSpace( DataSelectedField ) )
                    {
                        object selectValue = DataBinder.Eval( gridViewRow.DataItem, DataSelectedField );
                        cb.Checked = (bool)selectValue;
                    }
                    else
                    {
                        cb.Checked =false;
                    }

                    if ( !string.IsNullOrWhiteSpace( DataVisibleField ) )
                    {
                        object visibleValue = DataBinder.Eval( gridViewRow.DataItem, DataVisibleField );
                        cb.Visible = (bool)visibleValue;
                    }
                    else
                    {
                        cb.Visible = true;
                    }

                    if ( SelectionMode == SelectionMode.Single )
                    {
                        ( (RockRadioButton)cb ).GroupName = "cbSelect_" + gridViewRow.RowIndex.ToString();
                    }
                }
            }
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class SelectFieldHeaderTemplate : ITemplate
    {
        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            var cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                var selectField = cell.ContainingField as SelectField;
                if ( selectField != null )
                {
                    Literal l = new Literal();
                    l.Text = selectField.HeaderText;
                    cell.Controls.Add( l );

                    if ( selectField.SelectionMode == SelectionMode.Multiple && selectField.ShowHeader && selectField.ShowSelectAll )
                    {
                        string colIndex = selectField.ColumnIndex.ToString();
                        CheckBox cb = new CheckBox();
                        cb.ID = "cbSelectHead_" + colIndex;
                        cb.AddCssClass( "select-all" );
                        cell.AddCssClass( "grid-select-field" );
                        cell.Controls.Add( cb );
                    }
                }
            }
        }
    }

    #region Enumerations

    /// <summary>
    /// 
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>
        /// Renders a checkbox
        /// </summary>
        Multiple,

        /// <summary>
        /// Renders a radio button that allows selecting one row for the column
        /// </summary>
        Single,
    }

    #endregion
}
