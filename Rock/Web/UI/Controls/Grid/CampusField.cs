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
    /// Control for selecting a campus where DataField is the Campus.Id
    /// Displays Campus.Name using CampusCache
    /// </summary>
    [ToolboxData( "<{0}:CampusField runat=server></{0}:CampusField>" )]
    public class CampusField : RockBoundField
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

            CampusCache campusCache = null;
            if ( dataValueAsInt.HasValue )
            {
                campusCache = CampusCache.Get( dataValueAsInt.Value );
            }
            else if ( dataValueAsGuid.HasValue )
            {
                campusCache = CampusCache.Get( dataValueAsGuid.Value );
            }

            if ( campusCache != null )
            {
                dataValue = campusCache.Name;
            }

            return base.FormatDataValue( dataValue, encode );
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
    }
}
