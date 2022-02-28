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
    /// Specifies a level of necessity and/or availability of a data entry element.
    /// </summary>
    public enum DataEntryRequirementLevelSpecifier
    {
        /// <summary>
        /// No requirement level has been specified for this data element.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// The data element is available but not required.
        /// </summary>
        Optional = 1,
        /// <summary>
        /// The data element is available and required.
        /// </summary>
        Required = 2,
        /// <summary>
        /// The data element is not available.
        /// </summary>
        Unavailable = 3
    }

    /// <summary>
    /// A field that stores the level of necessity and availability associated with a data entry item.
    /// </summary>
    /// <summary>
    /// Field Type used to display a dropdown list of RequirementLevels
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
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
}
