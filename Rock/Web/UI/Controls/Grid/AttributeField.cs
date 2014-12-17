﻿// <copyright>
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
using System.Web;
using System.Web.UI.WebControls;
using Rock.Attribute;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeField : RockBoundField
    {
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
                // First check if DataItem has attributes
                var dataItem = row.DataItem as IHasAttributes;
                if ( dataItem == null )
                {
                    // If the DataItem does not have attributes, check to see if there is an object list
                    var grid = row.NamingContainer as Grid;
                    if (grid != null && grid.ObjectList != null)
                    {
                        // If an object list exists, check to see if the associated object has attributes
                        string key = grid.DataKeys[row.RowIndex].Value.ToString();
                        if (!string.IsNullOrWhiteSpace(key) && grid.ObjectList.ContainsKey(key))
                        {
                            dataItem = grid.ObjectList[key] as IHasAttributes;
                        }
                    }
                }

                if (dataItem != null)
                {
                    if ( dataItem.Attributes == null )
                    {
                        dataItem.LoadAttributes();
                    }

                    bool exists = dataItem.Attributes.ContainsKey( this.DataField );
                    if ( exists )
                    {
                        var attrib = dataItem.Attributes[this.DataField];
                        string rawValue = dataItem.GetAttributeValue( this.DataField );
                        string resultHtml = attrib.FieldType.Field.FormatValue( controlContainer, rawValue, attrib.QualifierValues, true );
                        return new HtmlString( resultHtml ?? string.Empty );
                    }
                }
            }

            return string.Empty;
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
}