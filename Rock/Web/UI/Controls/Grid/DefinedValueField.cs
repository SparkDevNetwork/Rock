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
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for selecting a defined value where the DataField is the DefinedValue.Id
    /// </summary>
    [ToolboxData( "<{0}:DefinedValueField runat=server></{0}:DefinedValueField>" )]
    public class DefinedValueField : RockBoundField
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
            DefinedValueCache definedValueCache = GetDefinedValue( dataValue );

            if ( definedValueCache != null )
            {
                return base.FormatDataValue( definedValueCache.Value, encode );
            }

            // If we did not find a defined value we might have a list of values
            if ( dataValue is string && dataValue.ToString().Contains(",") )
            {
                return base.FormatDataValue( GetDefinedValues( dataValue.ToString() ), encode );
            }

            // If we didn't find any values return what was sent.
            return base.FormatDataValue( dataValue, encode );
        }

        /// <summary>
        /// Gets the defined value from data value.
        /// </summary>
        /// <param name="dataValue">The data value.</param>
        /// <returns></returns>
        public DefinedValueCache GetDefinedValue( object dataValue )
        {
            int? dataValueAsInt = null;
            Guid? dataValueAsGuid = null;

            if ( dataValue is int )
            {
                dataValueAsInt = (int)dataValue;
            }
            else if ( dataValue is Guid )
            {
                dataValueAsGuid = (Guid)dataValue;
            }
            else if ( dataValue is string )
            {
                dataValueAsInt = ( dataValue as string ).AsIntegerOrNull();
                dataValueAsGuid = ( dataValue as string ).AsGuidOrNull();
            }

            DefinedValueCache definedValueCache = null;
            if ( dataValueAsInt.HasValue )
            {
                definedValueCache = DefinedValueCache.Get( dataValueAsInt.Value );
            }
            else if ( dataValueAsGuid.HasValue )
            {
                definedValueCache = DefinedValueCache.Get( dataValueAsGuid.Value );
            }

            return definedValueCache;
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public override object GetExportValue( GridViewRow row )
        {
            var dataValue = base.GetExportValue( row );
            return FormatDataValue( dataValue, false );
        }

        /// <summary>
        /// For multiple select defined values the object passed to FormatDataValue will be a CSV of Guid or Ids.
        /// Parses the list, gets the value for each Id/Guid and returns a CSV of DefinedValue.Values
        /// </summary>
        /// <param name="definedValueIdCsv">The defined value identifier CSV.</param>
        /// <returns></returns>
        private string GetDefinedValues( string definedValueIdCsv )
        {
            string[] definedValueIdList = definedValueIdCsv.ToString().Split( ',' );
            string definedValues = string.Empty;

            foreach ( string definedValueId in definedValueIdList )
            {
                definedValues += GetDefinedValue( definedValueId ) + ", ";
            }

            definedValues = definedValues.TrimEnd( ',', ' ' );
            return definedValues;
        }
    }
}
