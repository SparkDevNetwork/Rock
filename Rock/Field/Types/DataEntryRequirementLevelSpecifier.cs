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

namespace Rock.Field.Types
{
    /// <summary>
    /// Specifies a level of necessity and/or availability of a data entry element.
    /// </summary>
    [Obsolete( "Use Rock.Enums.Controls.RequirementLevel instead." )]
    [RockObsolete( "20.0" )]
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
}
