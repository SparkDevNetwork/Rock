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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Record Status Badge
    /// </summary>
    [Description( "Record Status Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Record Status" )]
    public class RecordStatus : TextBadge
    {

        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override HighlightLabel GetLabel( Person person )
        {
            if ( Person != null )
            {
                // Show record status only if it's not 'Active'
                if ( Person.RecordStatusValueId.HasValue )
                {
                    var recordStatusValue = DefinedValueCache.Read( Person.RecordStatusValueId.Value );
                    if ( string.Compare( recordStatusValue.Guid.ToString(), Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE, true ) != 0 )
                    {
                        var recordReasonValue = DefinedValueCache.Read( Person.RecordStatusReasonValueId.Value );
                        var label = new HighlightLabel();
                        label.LabelType = LabelType.Danger;
                        label.Text = recordStatusValue.Name;
                        label.ToolTip = ( recordReasonValue != null ) ? recordReasonValue.Name : "";
                        return label;
                    }
                }
            }

            return null;
        }
    }
}
