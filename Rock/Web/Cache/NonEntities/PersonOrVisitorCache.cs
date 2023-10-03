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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Caches all the person preference records on a per person or visitor
    /// basis.
    /// </summary>
    internal class PersonOrVisitorCache : ItemCache<PersonOrVisitorCache>
    {
        #region Constants

        /// <summary>
        /// The default expiration period. This is how long we keep preferences
        /// in cache before expunging them.
        /// </summary>
        private static TimeSpan DefaultExpiration = TimeSpan.FromHours( 1 );

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override TimeSpan? Lifespan => DefaultExpiration;

        /// <summary>
        /// Gets the preferences for this person or visitor.
        /// </summary>
        /// <value>The preferences for this person or visitor.</value>
        public List<PersonPreferenceCache> Preferences { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonOrVisitorCache"/> class.
        /// </summary>
        /// <param name="preferences">The preferences cached by this item.</param>
        private PersonOrVisitorCache( List<PersonPreferenceCache> preferences )
        {
            Preferences = preferences;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets all of the cached person preferences for a person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>A list of <see cref="PersonPreferenceCache"/> objects.</returns>
        public static List<PersonPreferenceCache> GetAllForPerson( int personId )
        {
            return GetOrAddExisting( GetPersonKey( personId ), () => QueryDbForPerson( personId ) ).Preferences;
        }

        /// <summary>
        /// Gets all of the cached person preferences for an anonymous visitor
        /// by their <see cref="PersonAlias"/> identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>A list of <see cref="PersonPreferenceCache"/> objects.</returns>
        public static List<PersonPreferenceCache> GetAllForVisitor( int personAliasId )
        {
            return GetOrAddExisting( GetVisitorKey( personAliasId ), () => QueryDbForVisitor( personAliasId ) ).Preferences;
        }

        /// <summary>
        /// Flushes the preference cache for the person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        public static void FlushPerson( int personId )
        {
            FlushItem( GetPersonKey( personId ) );
        }

        /// <summary>
        /// Flushes the preference cache for the visitor.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        public static void FlushVisitor( int personAliasId )
        {
            FlushItem( GetVisitorKey( personAliasId ) );
        }

        /// <summary>
        /// Gets the cache item key for a person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>A string that represents the cache item key.</returns>
        private static string GetPersonKey( int personId )
        {
            return $"person:{personId}";
        }

        /// <summary>
        /// Gets the cache item key for a visitor.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>A string that represents the cache item key.</returns>
        private static string GetVisitorKey( int personAliasId )
        {
            return $"visitor:{personAliasId}";
        }

        /// <summary>
        /// Queries the database to get preference records for a person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>An instance of <see cref="PersonOrVisitorCache"/> that will be added to the cache.</returns>
        private static PersonOrVisitorCache QueryDbForPerson( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                var personPreferenceService = new PersonPreferenceService( rockContext );

                var preferences = personPreferenceService.GetPersonPreferencesQueryable( personId )
                    .Include( pp => pp.PersonAlias )
                    .ToList()
                    .Select( pp => new PersonPreferenceCache( pp ) )
                    .ToList();

                return new PersonOrVisitorCache( preferences );
            }
        }

        /// <summary>
        /// Queries the database to get preference records for a visitor.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>An instance of <see cref="PersonOrVisitorCache"/> that will be added to the cache.</returns>
        private static PersonOrVisitorCache QueryDbForVisitor( int personAliasId )
        {
            using ( var rockContext = new RockContext() )
            {
                var personPreferenceService = new PersonPreferenceService( rockContext );

                var preferences = personPreferenceService.GetVisitorPreferencesQueryable( personAliasId )
                    .Include( pp => pp.PersonAlias )
                    .ToList()
                    .Select( pp => new PersonPreferenceCache( pp ) )
                    .ToList();

                return new PersonOrVisitorCache( preferences );
            }
        }

        #endregion
    }
}
