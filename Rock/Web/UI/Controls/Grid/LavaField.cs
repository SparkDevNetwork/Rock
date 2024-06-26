// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    /// Grid field that will render the output based on the LavaTemplate, using the Row's DataItem as a 'Row' MergeField, plus all the rest of the columns for mergefields as well
    /// </summary>
    [ToolboxData( "<{0}:LavaField runat=server></{0}:LavaField>" )]
    public class LavaField : RockTemplateField
    {
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
            LavaFieldTemplate lavaFieldTemplate = new LavaFieldTemplate();
            this.ItemTemplate = lavaFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

        /// <summary>
        /// Gets or sets the lava template.
        /// </summary>
        /// <value>
        /// The lava template.
        /// </value>
        public string LavaTemplate
        {
            get
            {
                return this.ViewState["LavaTemplate"] as string;
            }

            set
            {
                this.ViewState["LavaTemplate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the merge field key to use to represent the Row's dataitem
        /// </summary>
        /// <value>
        /// The lava key.
        /// </value>
        public string LavaKey
        {
            get
            {
                return this.ViewState["LavaKey"] as string;
            }

            set
            {
                this.ViewState["LavaKey"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [convert to item dictionary] instead of "Row"
        /// Set this to false to have the entire "Row" as a the merge field instead of individual column values
        /// Leave this as true to have the individual properties as the merge field (based on the Column Headers of the other stuff in the grid)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [convert to item dictionary]; otherwise, <c>false</c>.
        /// </value>
        public bool ConvertToItemDictionary
        {
            get
            {
                return this.ViewState["ConvertToItemDictionary"] as bool? ?? true;
            }

            set
            {
                this.ViewState["ConvertToItemDictionary"] = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LavaFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets or sets the lava field.
        /// </summary>
        /// <value>
        /// The lava field.
        /// </value>
        private LavaField LavaField { get; set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                this.LavaField = cell.ContainingField as LavaField;
                Literal lOutputText = new Literal();
                lOutputText.DataBinding += lOutputText_DataBinding;
                cell.Controls.Add( lOutputText );
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the lOutputText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lOutputText_DataBinding( object sender, EventArgs e )
        {
            Literal lOutputText = sender as Literal;
            GridViewRow gridViewRow = lOutputText.NamingContainer as GridViewRow;
            if ( gridViewRow.DataItem != null )
            {
                Dictionary<string, object> mergeValues = new Dictionary<string, object>();
                
                if ( LavaField.ConvertToItemDictionary )
                {
                    // in the case of a Report, the MergeKeys are determined by the ColumnHeader, so add those as merge fields instead the Row object
                    foreach ( var keyValue in this.ToGridItemsDictionary( gridViewRow, gridViewRow.DataItem ) )
                    {
                        mergeValues.TryAdd( keyValue.Key, keyValue.Value );
                    }
                } 
                else
                {
                    mergeValues.Add( "Row", gridViewRow.DataItem );
                }

                lOutputText.Text = this.LavaField.LavaTemplate.ResolveMergeFields( mergeValues );

                // Resolve any dynamic url references
                string appRoot = ( ( RockPage ) lOutputText.Page ).ResolveRockUrl( "~/" );
                string themeRoot = ( ( RockPage ) lOutputText.Page ).ResolveRockUrl( "~~/" );
                lOutputText.Text = lOutputText.Text.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal class DataFieldInfo
        {
            /// <summary>
            /// Gets or sets the property information.
            /// </summary>
            /// <value>
            /// The property information.
            /// </value>
            public System.Reflection.PropertyInfo PropertyInfo { get; set; }

            /// <summary>
            /// Gets or sets the grid field.
            /// </summary>
            /// <value>
            /// The grid field.
            /// </value>
            public BoundField GridField { get; set; }
        }

        /// <summary>
        /// Gets or sets the data item properties.
        /// </summary>
        /// <value>
        /// The data item properties.
        /// </value>
        private Dictionary<string, DataFieldInfo> DataItemPropertiesDictionary { get; set; }

        /// <summary>
        /// To the dictionary.
        /// </summary>
        /// <param name="gridViewRow">The grid view row.</param>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        private Dictionary<string, object> ToGridItemsDictionary( GridViewRow gridViewRow, object dataItem )
        {
            var dictionary = new Dictionary<string, object>();

            if ( this.DataItemPropertiesDictionary == null )
            {
                PopulateDataItemPropertiesDictionary( dataItem );
            }

            foreach ( var dataFieldItem in DataItemPropertiesDictionary )
            {
                var dataFieldValue = dataFieldItem.Value.PropertyInfo.GetValue( dataItem, null );

                if ( dataFieldItem.Value.GridField is DefinedValueField )
                {
                    var definedValue = ( dataFieldItem.Value.GridField as DefinedValueField ).GetDefinedValue( dataFieldValue );
                    dataFieldValue = definedValue != null ? definedValue.Value : null;
                }

                dictionary.Add( dataFieldItem.Key, dataFieldValue );
            }

            return dictionary;
        }

        /// <summary>
        /// Populates the data item properties dictionary.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        private void PopulateDataItemPropertiesDictionary( object dataItem )
        {
            var dataItemProperties = dataItem.GetType().GetProperties().Where( a => a.GetGetMethod() != null && !a.GetGetMethod().IsVirtual ).ToArray();
            this.DataItemPropertiesDictionary = new Dictionary<string, DataFieldInfo>();

            // add MergeFields based on the associated ColumnHeaderText of each property of the dataitem (without spaces or special chars)
            foreach ( var itemPropInfo in dataItemProperties )
            {
                var gridField = LavaField.ParentGrid.Columns.OfType<BoundField>().FirstOrDefault( a => a.DataField == itemPropInfo.Name );
                if ( gridField != null )
                {
                    var mergeFieldName = gridField.HeaderText.Replace( " ", string.Empty ).RemoveSpecialCharacters();

                    // NOTE: since we are using the HeaderText as the mergeFieldName, and that might not be unique, just add the first one if there are duplicates
                    if ( !this.DataItemPropertiesDictionary.ContainsKey( mergeFieldName ) )
                    {
                        this.DataItemPropertiesDictionary.Add( mergeFieldName, new DataFieldInfo { PropertyInfo = itemPropInfo, GridField = gridField } );
                    }
                }
                else
                {
                    // add properties that aren't shown in the grid in the next loop
                }
            }

            // add additional MergeFields for Properties of the dataitem that aren't already a MergeField created from the ColumnHeaderText
            foreach ( var itemPropInfo in dataItemProperties )
            {
                var mergeFieldName = itemPropInfo.Name;
                if ( !this.DataItemPropertiesDictionary.ContainsKey( mergeFieldName ) )
                {
                    this.DataItemPropertiesDictionary.Add( mergeFieldName, new DataFieldInfo { PropertyInfo = itemPropInfo, GridField = null } );
                }
            }
        }
    }
}