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

using System.Collections.Generic;

using Rock.Security;

namespace Rock.Field
{
    /// <summary>
    /// Methods required for a field type to be able to provide additional
    /// rules to <see cref="SecurityGrant"/> objects.
    /// </summary>
    internal interface ISecurityGrantFieldType
    {
        /// <summary>
        /// Adds the rules to security grant that are needed by this field type
        /// for it to function properly.
        /// </summary>
        /// <param name="grant">The grant which the rules will be added to.</param>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field.</param>
        void AddRulesToSecurityGrant( SecurityGrant grant, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Adds the rules to security grant that are needed by this field type
        /// for it to function properly. The is used to get security grants for
        /// new attributes being created.
        /// </summary>
        /// <param name="grant">The grant which the rules will be added to.</param>
        void AddRulesToSecurityGrant( SecurityGrant grant );
    }
}
