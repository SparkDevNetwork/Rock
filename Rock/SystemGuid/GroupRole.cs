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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Group Role System Guids
    /// </summary>
    public static class GroupRole
    {
        #region Family Members

        /// <summary>
        /// Gets the adult family member role
        /// </summary>
        public const string GROUPROLE_FAMILY_MEMBER_ADULT= "2639F9A5-2AAE-4E48-A8C3-4FFE86681E42";
        
        /// <summary>
        /// Gets the child family member role
        /// </summary>
        public const string GROUPROLE_FAMILY_MEMBER_CHILD= "C8B1814F-6AA7-4055-B2D7-48FE20429CB9";

        #endregion

        #region Known Relationships

        /// <summary>
        /// Gets the Known Relationships owner role.
        /// </summary>
        /// <value>
        /// The role Guid
        /// </value>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_OWNER= "7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42";
        
        /// <summary>
        /// Gets the Can check in role
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN = "DC8E5C35-F37C-4B49-A5C6-DF3D94FC808F";

        /// <summary>
        /// Gets the Allow check in role
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_CHECK_IN_BY = "FF9869F1-BC56-4410-8A12-CAFC32C62257";

        #endregion

        #region Implied Relationships

        /// <summary>
        /// Gets the Implied Relationships owner role.
        /// </summary>
        /// <value>
        /// The role Guid.
        /// </value>
        public const string GROUPROLE_IMPLIED_RELATIONSHIPS_OWNER= "CB9A0E14-6FCF-4C07-A49A-D7873F45E196";

        /// <summary>
        /// Gets the Implied Relationships related role.
        /// </summary>
        /// <value>
        /// The role Guid.
        /// </value>
        public const string GROUPROLE_IMPLIED_RELATIONSHIPS_RELATED= "FEA75948-97CB-4DE9-8A0D-43FA2646F55B";

        #endregion

    }
}