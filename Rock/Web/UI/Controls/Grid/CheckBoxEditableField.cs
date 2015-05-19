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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// The ASP:CheckBoxField doesn't work very well for retrieving changed values, especially when the value is changed from True to False (weird)
    /// This CheckBoxEditableField works like the ASP:CheckBoxField except it gives the CheckBox's IDs so their changed values will consistantly persist on postbacks
    /// </summary>
    public class CheckBoxEditableField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Gets or sets the data field.
        /// </summary>
        /// <value>
        /// The data field.
        /// </value>
        public string DataField
        {
            get
            {
                return ViewState["DataField"] as string;
            }

            set
            {
                ViewState["DataField"] = value;
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
            this.ItemTemplate = new CheckBoxFieldTemplate();

            return base.Initialize( sortingEnabled, control );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CheckBoxFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets the data field.
        /// </summary>
        /// <value>
        /// The data field.
        /// </value>
        public string DataField { get; private set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            var cell = container as DataControlFieldCell;
            cell.AddCssClass( "grid-select-field" );
            string dataField = ( ( container as DataControlFieldCell ).ContainingField as CheckBoxEditableField ).DataField;
            CheckBox checkBox = new CheckBox();
            checkBox.ID = "checkBox_" + dataField;
            DataField = dataField;
            checkBox.DataBinding += checkBox_DataBinding;
            container.Controls.Add( checkBox );
        }

        /// <summary>
        /// Handles the DataBinding event of the checkBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void checkBox_DataBinding( object sender, EventArgs e )
        {
            CheckBox checkBox = sender as CheckBox;
            GridViewRow gridViewRow = checkBox.NamingContainer as GridViewRow;
            if ( gridViewRow.DataItem != null && !string.IsNullOrWhiteSpace( DataField ) )
            {
                object dataValue = DataBinder.Eval( gridViewRow.DataItem, DataField );

                checkBox.Checked = (bool)dataValue;
            }
        }
    }
}
