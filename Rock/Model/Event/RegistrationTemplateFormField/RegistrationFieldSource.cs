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

using System;

namespace Rock.Model
{
    /// <summary>
    /// The entity that attribute applies to
    /// </summary>
    public enum RegistrationFieldSource
    {
        /// <summary>
        /// Person field
        /// </summary>
        PersonField = 0,

        /// <summary>
        /// Person attribute
        /// </summary>
        PersonAttribute = 1,

        /// <summary>
        /// Group Member attribute
        /// </summary>
        GroupMemberAttribute = 2,

        /// <summary>
        /// Registrant attribute
        /// </summary>
        RegistrantAttribute = 4,

        /// <summary>
        /// Registration attribute
        /// NOTE: Put obsolete Enums AFTER the one that replaces it so that enum.ConvertToString() returns the non-obsolete name
        /// </summary>
        [Obsolete( "Use RegistrantAttribute instead", true )]
        [RockObsolete( "1.9" )]
        RegistrationAttribute = RegistrantAttribute,
    }


}
