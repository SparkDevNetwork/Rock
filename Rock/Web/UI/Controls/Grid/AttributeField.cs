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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Field.Types;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// If DataItem implements IHasAttributes, this gets the Attribute value for the dataitem using DataField as the Attribute.Key.
    /// Otherwise, it will look in the grid.ObjectList[key] using the DataField as the lookup key.
    /// Example: If using a Person Grid, the value of the Attribute "FavoriteColor" would be shown in the grid by specifying this grid column
    /// &lt;Rock:AttributeField DataField="FavoriteColor" HeaderText="Person's Favorite Color" /&gt;
    /// </summary>
    public class AttributeField : RockBoundField
    {
        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public int? AttributeId
        {
            get { return ViewState["AttributeId"] as int?; }
            set
            {
                ViewState["AttributeId"] = value;
                this.SortExpression = string.Format( "attribute:{0}", value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AttributeField"/> is condensed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if condensed; otherwise, <c>false</c>.
        /// </value>
        public bool Condensed
        {
            get { return ViewState["Condensed"] as bool? ?? true; }
            set { ViewState["Condensed"] = value; }
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public override object GetExportValue( GridViewRow row )
        {
            return GetRowValue( row, false, false );
        }

        /// <summary>
        /// Retrieves the value of the field bound to the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="controlContainer">The container for the field value.</param>
        /// <returns>
        /// The value of the field bound to the <see cref="T:System.Web.UI.WebControls.BoundField" />.
        /// </returns>
        protected override object GetValue( System.Web.UI.Control controlContainer )
        {
            var row = controlContainer as GridViewRow;
            if ( row != null )
            {
                return GetRowValue( row, this.Condensed, true );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the row value.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <param name="formatAsHtml">if set to <c>true</c> [format as HTML].</param>
        /// <returns></returns>
        private object GetRowValue( GridViewRow row, bool condensed, bool formatAsHtml )
        {
            // First try to get an IHasAttributes from the grid's object list
            IHasAttributes dataItem = GetAttributeObject( row );
            if ( dataItem == null )
            {
                // If unsuccessful, check to see if row has attributes
                dataItem = row.DataItem as IHasAttributes;
            }

            if ( dataItem != null )
            {
                if ( dataItem.Attributes == null )
                {
                    dataItem.LoadAttributes();
                }

                AttributeCache attrib = null;
                string rawValue = string.Empty;

                bool exists = dataItem.Attributes.ContainsKey( this.DataField );
                if ( exists )
                {
                    attrib = dataItem.Attributes[this.DataField];
                    rawValue = dataItem.GetAttributeValue( this.DataField );
                }
                else
                {
                    if ( AttributeId.HasValue )
                    {
                        attrib = dataItem.Attributes.Where( a => a.Value.Id == AttributeId.Value ).Select( a => a.Value ).FirstOrDefault();
                        if ( attrib != null )
                        {
                            exists = true;
                            rawValue = dataItem.GetAttributeValue( attrib.Key );
                        }
                    }
                }

                if ( exists )
                {
                    if ( formatAsHtml )
                    {
                        if ( attrib?.FieldType?.Field is BooleanFieldType )
                        {
                            if ( this.ItemStyle.HorizontalAlign != HorizontalAlign.Center )
                            {
                                this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                                this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                            }

                            var boolValue = rawValue.AsBoolean();
                            return boolValue ? "<i class=\"fa fa-check\"></i>" : string.Empty;
                        }

                        string resultHtml = attrib.FieldType.Field.FormatValueAsHtml( null, attrib.EntityTypeId, dataItem.Id, rawValue, attrib.QualifierValues, condensed );
                        return new HtmlString( resultHtml ?? string.Empty );
                    }
                    else
                    {
                        string result = attrib.FieldType.Field.FormatValue( null, attrib.EntityTypeId, dataItem.Id, rawValue, attrib.QualifierValues, condensed );
                        return result ?? string.Empty;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the attribute object.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        private IHasAttributes GetAttributeObject(GridViewRow row)
        {
            // Get the parent grid
            var grid = row.NamingContainer as Grid;

            // check to see if there is an object list for the grid
            if ( grid != null && grid.ObjectList != null )
            {
                // If an object list exists, check to see if the associated object has attributes
                string key = grid.DataKeys[row.RowIndex].Value.ToString();
                if ( !string.IsNullOrWhiteSpace( key ) && grid.ObjectList.ContainsKey( key ) )
                {
                    return grid.ObjectList[key] as IHasAttributes;
                }
            }

            return null;
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            return base.FormatDataValue( dataValue, false );
        }
    }

    /// <summary>
    /// Helper class that can be used by blocks to pre-load attributes/values so that
    /// the attribute field columns don't need to call LoadAttributes or query for attribute 
    /// values for every row/column
    /// </summary>
    public class AttributeFieldObject : IHasAttributes
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues" /> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the associated attribute value
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, AttributeValueCache> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attribute value defaults.  This property can be used by a subclass to override the parent class's default
        /// value for an attribute
        /// </summary>
        /// <value>
        /// The attribute value defaults.
        /// </value>
        public Dictionary<string, string> AttributeValueDefaults
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                return this.AttributeValues[key].Value;
            }

            if ( this.Attributes != null &&
                this.Attributes.ContainsKey( key ) )
            {
                return this.Attributes[key].DefaultValue;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A list of string values or an empty list if none exist.
        /// </returns>
        public List<string> GetAttributeValues( string key )
        {
            string value = GetAttributeValue( key );
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                return value.SplitDelimitedValues().ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                this.AttributeValues[key].Value = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFieldObject"/> class.
        /// </summary>
        public AttributeFieldObject()
        {
            Attributes = new Dictionary<string, AttributeCache>();
            AttributeValues = new Dictionary<string, AttributeValueCache>();
        }
    }
}