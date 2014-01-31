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
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public List<int> SelectedKeys 
        {
            get { return _selectedKeys; }
            internal set { _selectedKeys = value; } 
        }
        private List<int> _selectedKeys = new List<int>();

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
            this.ItemTemplate = new SelectFieldTemplate();
            var grid = control as Grid;
            if (grid != null)
            {
                ColumnIndex = grid.Columns.IndexOf( this );
            }

            return base.Initialize( sortingEnabled, control );
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SelectFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets the selection mode.
        /// </summary>
        /// <value>
        /// The selection mode.
        /// </value>
        public SelectionMode SelectionMode { get; private set; }

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
                        cb = new RadioButton();
                        if (SelectionMode == SelectionMode.SingleRow)
                        {
                            ( (RadioButton)cb ).GroupName = "cbSelect_" + ColumnIndex.ToString();
                        }
                    }

                    cb.ID = "cbSelect_" + ColumnIndex.ToString();
                    cb.DataBinding += cb_DataBinding;
                    cell.Controls.Add( cb );
                }
            }
        }

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
                    if ( !string.IsNullOrWhiteSpace( DataSelectedField ) )
                    {
                        object dataValue = DataBinder.Eval( gridViewRow.DataItem, DataTextField );
                        cb.Text = dataValue.ToString();

                        object selectValue = DataBinder.Eval( gridViewRow.DataItem, DataSelectedField );
                        cb.Checked = (bool)selectValue;

                        if ( SelectionMode == SelectionMode.SingleColumn )
                        {
                            ( (RadioButton)cb ).GroupName = "cbSelect_" + gridViewRow.RowIndex.ToString();
                        }
                    }
                }
            }
        }
    }

    public enum SelectionMode
    {
        /// <summary>
        /// Renders a checkbox
        /// </summary>
        Multiple = 0,

        /// <summary>
        /// Renders a radio button that allows selecting one column for every row
        /// </summary>
        SingleColumn = 1,

        /// <summary>
        /// Renders a radio button that allows selecting one row for the column
        /// </summary>
        SingleRow = 2
    }
}
