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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Data;

using Rock.Net;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter groups based on member count" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Member Count" )]
    [Rock.SystemGuid.EntityTypeGuid( "27E521A3-9F5C-424E-8AED-43A228B1702F" )]
    public class SimpleMemberCountFilter : MemberCountFilter
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        ///   </value>
        public override string GetTitle( Type entityType )
        {
            return "Member Count";
        }

        /// <summary>
        /// Gets a value indicating whether [simple member count mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [simple member count mode]; otherwise, <c>false</c>.
        /// </value>
        internal override bool SimpleMemberCountMode
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/Group/simpleMemberCountFilter.obs";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionValues = selection.Split( '|' );

            if ( selectionValues.Length > 1 )
            {
                return new Dictionary<string, string>
                {
                    { "comparisonType", selectionValues[0] },
                    { "count", selectionValues[1] },
                };
            }

            return new Dictionary<string, string>();
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            return string.Format( "{0}|{1}|{2}|{3}", data.GetValueOrDefault( "comparisonType", "" ), data.GetValueOrDefault( "count", "" ), null, null );
        }
    }
}