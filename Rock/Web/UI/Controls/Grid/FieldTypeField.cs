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
using System.Web.UI;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Grid Bound Field for showing a FieldType 
    /// </summary>
    [ToolboxData( "<{0}:FieldTypeField runat=server></{0}:FieldTypeField>" )]
    public class FieldTypeField : RockBoundField
    {
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
            try
            {
                int? dataValueAsInt = null;
                Guid? dataValueAsGuid = null;
                if ( dataValue is int )
                {
                    dataValueAsInt = ( int ) dataValue;
                }
                else if ( dataValue is Guid )
                {
                    dataValueAsGuid = ( Guid ) dataValue;
                }
                else if ( dataValue is string )
                {
                    dataValueAsInt = ( dataValue as string ).AsIntegerOrNull();
                    dataValueAsGuid = ( dataValue as string ).AsGuidOrNull();
                }

                FieldTypeCache fieldTypeCache = null;
                if ( dataValueAsInt.HasValue )
                {
                    fieldTypeCache = FieldTypeCache.Get( dataValueAsInt.Value );
                }
                else if ( dataValueAsGuid.HasValue )
                {
                    fieldTypeCache = FieldTypeCache.Get( dataValueAsGuid.Value );
                }

                dataValue = fieldTypeCache != null ? fieldTypeCache.Name : null;

                return base.FormatDataValue( dataValue, encode );
            }
            catch { }

            return string.Empty;
        }
    }
}
