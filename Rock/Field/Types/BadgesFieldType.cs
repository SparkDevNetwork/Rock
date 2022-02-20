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
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Select multiple Badges from a checkbox list. Stored as a comma-delimited list of Badge Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class BadgesFieldType : SelectFromListFieldType
    {
        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return base.EditControl( configurationValues, id );
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<BadgeCache> badges = null;
            int? entityTypeId = null;

            if ( configurationValues != null && configurationValues.TryGetValue( BadgesFieldAttribute.ENTITY_TYPE_KEY, out var value ) )
            {
                entityTypeId = ( value.Value as string ).AsIntegerOrNull();
            }

            if ( entityTypeId.HasValue )
            {
                badges = BadgeCache.All( entityTypeId.Value );
            }
            else
            {
                badges = BadgeCache.All();
            }

            var orderedBadges = badges.OrderBy( x => x.Order )
                .ToDictionary( k => k.Guid.ToString(), v => v.Name );

            return orderedBadges;
        }
    }
}
