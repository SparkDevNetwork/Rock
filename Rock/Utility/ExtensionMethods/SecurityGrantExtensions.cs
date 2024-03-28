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
using System.Linq;

using Rock.Attribute;
using Rock.Field;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="SecurityGrant"/>.
    /// </summary>
    public static class SecurityGrantExtensions
    {
        /// <summary>
        /// Adds the security grant rules required for the attributes to function properly.
        /// </summary>
        /// <param name="grant">The security grant to add the rules to.</param>
        /// <param name="entity">The entity with attributes.</param>
        /// <param name="currentPerson">The current person to use for authorizing which attributes to process.</param>
        /// <returns>The <see cref="SecurityGrant"/> object.</returns>
        public static SecurityGrant AddRulesForAttributes( this SecurityGrant grant, IHasAttributes entity, Person currentPerson )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return grant;
            }

            var attributes = entity.Attributes
                .Select( a => a.Value )
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) );

            return AddRulesForAttributes( grant, attributes );
        }

        /// <summary>
        /// Adds the security grant rules required for the attributes to function properly.
        /// </summary>
        /// <param name="grant">The security grant to add the rules to.</param>
        /// <param name="attributes">The attributes to use when adding grant rules.</param>
        /// <returns>The <see cref="SecurityGrant"/> object.</returns>
        public static SecurityGrant AddRulesForAttributes( this SecurityGrant grant, IEnumerable<AttributeCache> attributes )
        {
            foreach ( var attribute in attributes )
            {
                var fieldType = attribute.FieldType?.Field;

                if ( fieldType is ISecurityGrantFieldType grantFieldType )
                {
                    grantFieldType.AddRulesToSecurityGrant( grant, attribute.ConfigurationValues );
                }
            }

            return grant;
        }
    }
}
