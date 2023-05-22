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
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonPreferenceService
    {
        /// <summary>
        /// <para>
        /// Gets queryable that will return all person preferences for the
        /// specified person.
        /// </para>
        /// <para>
        /// Do not call this with the nameless person identifier.
        /// </para>
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>A <see cref="IQueryable"/> for the matching <see cref="PersonPreference"/> records.</returns>
        public IQueryable<PersonPreference> GetPersonPreferencesQueryable( int personId )
        {
            return Queryable()
                .Where( pp => pp.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// <para>
        /// Gets queryable that will return all person preferences for the
        /// specified nameless visitor.
        /// </para>
        /// <para>
        /// A visitor is a nameless person that has a unique person alias
        /// record but is tied to the single nameless person record.
        /// </para>
        /// <para>
        /// Do not call this with a person alias identifier for a regular person.
        /// </para>
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>A <see cref="IQueryable"/> for the matching <see cref="PersonPreference"/> records.</returns>
        public IQueryable<PersonPreference> GetVisitorPreferencesQueryable( int personAliasId )
        {
            return Queryable()
                .Where( pp => pp.PersonAliasId == personAliasId );
        }

        /// <summary>
        /// Gets the preference prefix to use when scoped to <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity that the preferences should be scoped to.</param>
        /// <returns>A string that should be prefixed to all preference keys.</returns>
        public static string GetPreferencePrefix( IEntity entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            return GetPreferencePrefix( entity.GetType(), entity.Id );
        }

        /// <summary>
        /// Gets the preference prefix to use when scoped to <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity that the preferences should be scoped to.</param>
        /// <returns>A string that should be prefixed to all preference keys.</returns>
        public static string GetPreferencePrefix( IEntityCache entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            return GetPreferencePrefix( EntityTypeCache.Get( entity.CachedEntityTypeId ).GetEntityType(), entity.Id );
        }

        /// <summary>
        /// Gets the preference prefix to use when scoped to specified entity.
        /// </summary>
        /// <param name="type">The entity type that should be used to define the scope.</param>
        /// <param name="id">The entity identifier that should be used to define the scope.</param>
        /// <returns>A string that should be prefixed to all preference keys.</returns>
        public static string GetPreferencePrefix( Type type, int id )
        {
            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            var prefix = type.Name
                .SplitCase()
                .ToLower()
                .Trim()
                .Replace( " ", "-" );

            prefix = Regex.Replace( prefix, "[^a-zA-Z0-9-]", string.Empty );

            return $"{prefix}-{id}-";
        }

        /// <summary>
        /// Gets the preference prefix to use when not scoped to a specified entity.
        /// </summary>
        /// <returns>A string represents the prefix to use.</returns>
        public static string GetGlobalPreferencePrefix()
        {
            return "global-0-";
        }
    }
}
