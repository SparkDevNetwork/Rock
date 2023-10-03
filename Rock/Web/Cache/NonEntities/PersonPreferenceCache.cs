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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Web.Cache
{
    /// <summary>
    /// An instance of a cached person preference. Instances are not normally
    /// accessed directly. Instead use a <see cref="PersonPreferenceCollection"/>
    /// returned by one of the static methods.
    /// </summary>
    public class PersonPreferenceCache
    {
        #region Properties

        /// <summary>
        /// Gets the identifier of the database record.
        /// </summary>
        /// <value>The identifier of the database record.</value>
        public int Id { get; }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>The person identifier.</value>
        public int PersonId { get; }

        /// <summary>
        /// Gets the person alias identifier.
        /// </summary>
        /// <value>The person alias identifier.</value>
        public int PersonAliasId { get; }

        /// <summary>
        /// Gets the entity type identifier. If <c>null</c> then the preference
        /// is considered a "global" user preference. It still belongs to just
        /// the one person, but it does not apply to just a single block or
        /// other entity.
        /// </summary>
        /// <value>The entity type identifier.</value>
        public int? EntityTypeId { get; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <value>The entity identifier.</value>
        public int? EntityId { get; }

        /// <summary>
        /// Gets the key that uniquely identifies this value.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the last accessed date time. This is only updated once per day.
        /// </summary>
        /// <value>The last accessed date time.</value>
        public DateTime LastAccessedDateTime { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPreferenceCache"/> class.
        /// </summary>
        /// <param name="personPreference">The person preference object to initialize values from.</param>
        internal PersonPreferenceCache( PersonPreference personPreference )
        {
            Id = personPreference.Id;
            PersonId = personPreference.PersonAlias.PersonId;
            PersonAliasId = personPreference.PersonAliasId;
            EntityTypeId = personPreference.EntityTypeId;
            EntityId = personPreference.EntityId;
            Key = personPreference.Key;
            Value = personPreference.Value;
            LastAccessedDateTime = personPreference.LastAccessedDateTime;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the preferences for a person. If <paramref name="entityTypeCache"/>
        /// and <paramref name="entityId"/> are specified then only preferences that
        /// are attached to that entity will be returned. Otherwise only the global
        /// preferences that are not attached to any entity will be returned.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="entityTypeCache">The cache that describes the type of entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>A list of <see cref="PersonPreferenceCache"/> objects that represent the preferences.</returns>
        private static List<PersonPreferenceCache> GetPersonPreferences( int personId, EntityTypeCache entityTypeCache, int? entityId )
        {
            if ( entityTypeCache != null )
            {
                if ( !entityId.HasValue )
                {
                    throw new ArgumentNullException( "entityId", "Parameter is required when 'entityTypeCache' is not null." );
                }

                return PersonOrVisitorCache.GetAllForPerson( personId )
                    .Where( pp => pp.EntityTypeId == entityTypeCache.Id
                        && pp.EntityId == entityId.Value )
                    .ToList();
            }
            else
            {
                return PersonOrVisitorCache.GetAllForPerson( personId )
                    .Where( pp => !pp.EntityTypeId.HasValue )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the preferences for a visitor. If <paramref name="entityTypeCache"/>
        /// and <paramref name="entityId"/> are specified then only preferences that
        /// are attached to that entity will be returned. Otherwise only the global
        /// preferences that are not attached to any entity will be returned.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="entityTypeCache">The cache that describes the type of entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>A list of <see cref="PersonPreferenceCache"/> objects that represent the preferences.</returns>
        private static List<PersonPreferenceCache> GetVisitorPreferences( int personAliasId, EntityTypeCache entityTypeCache, int? entityId )
        {
            if ( entityTypeCache != null )
            {
                if ( !entityId.HasValue )
                {
                    throw new ArgumentNullException( "entityId", "Parameter is required when 'entityTypeCache' is not null." );
                }

                return PersonOrVisitorCache.GetAllForVisitor( personAliasId )
                    .Where( pp => pp.EntityTypeId == entityTypeCache.Id
                        && pp.EntityId == entityId.Value )
                    .ToList();
            }
            else
            {
                return PersonOrVisitorCache.GetAllForVisitor( personAliasId )
                    .Where( pp => !pp.EntityTypeId.HasValue )
                    .ToList();
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// <para>
        /// Gets the person preference collection that will provide access
        /// to global preferences. These are unique to the individual but
        /// not attached to any specific entity.
        /// </para>
        /// <para>
        /// If you pass in the Anonymous Visitor person record then an empty
        /// preference collection will be returned. Instead use the
        /// GetVisitorPreferenceCollection methods.
        /// </para>
        /// </summary>
        /// <param name="person">The person object. The <see cref="Person.Aliases"/> property should be pre-fetched.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person )
        {
            if ( person == null || person.Guid == new Guid( Rock.SystemGuid.Person.ANONYMOUS_VISITOR ) )
            {
                return new PersonPreferenceCollection();
            }

            var preferences = GetPersonPreferences( person.Id, null, null );
            var prefix = PersonPreferenceService.GetGlobalPreferencePrefix();

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, null, null, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the person preference collection that will provide access
        /// to preferences attached to a specific entity.
        /// </para>
        /// <para>
        /// If you pass in the Anonymous Visitor person record then an empty
        /// preference collection will be returned. Instead use the
        /// GetVisitorPreferenceCollection methods.
        /// </para>
        /// </summary>
        /// <param name="person">The person object. The <see cref="Person.Aliases"/> property should be pre-fetched.</param>
        /// <param name="entity">The entity that the preferences will be scoped to.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person, IEntity entity )
        {
            if ( person == null || person.Guid == new Guid( Rock.SystemGuid.Person.ANONYMOUS_VISITOR ) )
            {
                return new PersonPreferenceCollection();
            }

            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.GetType() );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entity );
            var preferences = GetPersonPreferences( person.Id, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the person preference collection that will provide access
        /// to preferences attached to a specific entity.
        /// </para>
        /// <para>
        /// If you pass in the Anonymous Visitor person record then an empty
        /// preference collection will be returned. Instead use the
        /// GetVisitorPreferenceCollection methods.
        /// </para>
        /// </summary>
        /// <param name="person">The person object. The <see cref="Person.Aliases"/> property should be pre-fetched.</param>
        /// <param name="entity">The entity that the preferences will be scoped to.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person, IEntityCache entity )
        {
            if ( person == null || person.Guid == new Guid( Rock.SystemGuid.Person.ANONYMOUS_VISITOR ) )
            {
                return new PersonPreferenceCollection();
            }

            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.CachedEntityTypeId );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entityTypeCache.GetEntityType(), entity.Id );
            var preferences = GetPersonPreferences( person.Id, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the person preference collection that will provide access
        /// to preferences attached to a specific entity.
        /// </para>
        /// <para>
        /// If you pass in the Anonymous Visitor person record then an empty
        /// preference collection will be returned. Instead use the
        /// GetVisitorPreferenceCollection methods.
        /// </para>
        /// </summary>
        /// <param name="person">The person object. The <see cref="Person.Aliases"/> property should be pre-fetched.</param>
        /// <param name="entityTypeCache">The cache that describes the type of entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person, EntityTypeCache entityTypeCache, int entityId )
        {
            if ( person == null || person.Guid == new Guid( Rock.SystemGuid.Person.ANONYMOUS_VISITOR ) )
            {
                return new PersonPreferenceCollection();
            }

            if ( entityTypeCache == null )
            {
                throw new ArgumentNullException( nameof( entityTypeCache ) );
            }

            var prefix = PersonPreferenceService.GetPreferencePrefix( entityTypeCache.GetEntityType(), entityId );
            var preferences = GetPersonPreferences( person.Id, entityTypeCache, entityId );

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, entityTypeCache.Id, entityId, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the visitor preference collection that will provide access
        /// to global preferences. These are unique to the individual but
        /// not attached to any specific entity.
        /// </para>
        /// <para>
        /// A visitor is a nameless person that has a unique person alias
        /// record but is tied to the single nameless person record.
        /// </para>
        /// <para>
        /// Do not call this with a person alias identifier for a regular person.
        /// </para>
        /// </summary>
        /// <param name="personAliasId">The person alias identifier of the visitor.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId )
        {
            var preferences = GetVisitorPreferences( personAliasId, null, null );
            var prefix = PersonPreferenceService.GetGlobalPreferencePrefix();

            return new PersonPreferenceCollection( null, personAliasId, null, null, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the visitor preference collection that will provide access
        /// to preferences attached to the specified entity.
        /// </para>
        /// <para>
        /// A visitor is a nameless person that has a unique person alias
        /// record but is tied to the single nameless person record.
        /// </para>
        /// <para>
        /// Do not call this with a person alias identifier for a regular person.
        /// </para>
        /// </summary>
        /// <param name="personAliasId">The person alias identifier of the visitor.</param>
        /// <param name="entity">The entity that the preferences will be scoped to.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId, IEntity entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.GetType() );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entity );
            var preferences = GetVisitorPreferences( personAliasId, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( null, personAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the visitor preference collection that will provide access
        /// to preferences attached to the specified entity.
        /// </para>
        /// <para>
        /// A visitor is a nameless person that has a unique person alias
        /// record but is tied to the single nameless person record.
        /// </para>
        /// <para>
        /// Do not call this with a person alias identifier for a regular person.
        /// </para>
        /// </summary>
        /// <param name="personAliasId">The person alias identifier of the visitor.</param>
        /// <param name="entity">The entity that the preferences will be scoped to.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId, IEntityCache entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.GetType() );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entityTypeCache.GetEntityType(), entity.Id );
            var preferences = GetVisitorPreferences( personAliasId, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( null, personAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        /// <summary>
        /// <para>
        /// Gets the visitor preference collection that will provide access
        /// to preferences attached to the specified entity.
        /// </para>
        /// <para>
        /// A visitor is a nameless person that has a unique person alias
        /// record but is tied to the single nameless person record.
        /// </para>
        /// <para>
        /// Do not call this with a person alias identifier for a regular person.
        /// </para>
        /// </summary>
        /// <param name="personAliasId">The person alias identifier of the visitor.</param>
        /// <param name="entityTypeCache">The cache that describes the type of entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that will provide access to the preferences.</returns>
        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId, EntityTypeCache entityTypeCache, int entityId )
        {
            if ( entityTypeCache == null )
            {
                throw new ArgumentNullException( nameof( entityTypeCache ) );
            }

            var prefix = PersonPreferenceService.GetPreferencePrefix( entityTypeCache.GetEntityType(), entityId );
            var preferences = GetVisitorPreferences( personAliasId, entityTypeCache, entityId );

            return new PersonPreferenceCollection( null, personAliasId, entityTypeCache.Id, entityId, prefix, preferences );
        }

        /// <summary>
        /// Removes all cached items from the preference cache.
        /// </summary>
        public static void Clear()
        {
            PersonOrVisitorCache.Clear();
        }

        /// <summary>
        /// Flushes the cached preferences for the specified person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        public static void FlushPerson( int personId )
        {
            PersonOrVisitorCache.FlushPerson( personId );
        }

        /// <summary>
        /// <para>
        /// Flushes the cached preferences for the specified visitor.
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
        public static void FlushVisitor( int personAliasId )
        {
            PersonOrVisitorCache.FlushVisitor( personAliasId );
        }

        #endregion
    }
}
