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

using Rock.Attribute;

namespace Rock.Field.Types
{
    /// <summary>
    /// A field that stores the level of necessity and availability associated with a data entry item.
    /// </summary>
    /// <summary>
    /// Field Type used to display a dropdown list of RequirementLevels
    /// </summary>
    [Serializable]
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL )]
    // The base class 
#pragma warning disable CS0618 // Type or member is obsolete
    public class DataEntryRequirementLevelFieldType : EnumFieldType<DataEntryRequirementLevelSpecifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryRequirementLevelFieldType"/> class.
        /// </summary>
        public DataEntryRequirementLevelFieldType()
        {
            var values = new Dictionary<DataEntryRequirementLevelSpecifier, string>();

            values.Add( DataEntryRequirementLevelSpecifier.Optional, "Optional" );
            values.Add( DataEntryRequirementLevelSpecifier.Required, "Required" );
            values.Add( DataEntryRequirementLevelSpecifier.Unavailable, "Hidden" );

            base.SetAvailableValues( values );
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
