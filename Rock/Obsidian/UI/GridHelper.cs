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
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// Provides a set of helper methods used by Obsidian Grid code.
    /// </summary>
    internal static class GridHelper
    {
        /// <summary>
        /// The row values that hold the row key when the rows identify one entity type.
        /// </summary>
        private static readonly string[] DefaultRowKeyFieldPaths = new[] { "idKey", "id", "guid" };

        /// <summary>
        /// Adds the entity keys found in the rows of the grid data to the set of keys.
        /// </summary>
        /// <param name="keys">The set of keys to add to.</param>
        /// <param name="gridData">The grid data that was sent to the client.</param>
        /// <param name="options">The options that describe which row values hold keys and the entity type they identify.</param>
        internal static void AddGridEntityKeys( GridEntityKeys keys, GridDataBag gridData, GridEntityKeyOptions options )
        {
            options = options ?? new GridEntityKeyOptions();

            if ( options.EntityTypeId.HasValue )
            {
                keys.AddKeys( options.EntityTypeId.Value, GetGridKeyValues( gridData, options.RowKeyFieldPaths ?? DefaultRowKeyFieldPaths ) );
            }
            else
            {
                keys.AddUntypedKeys( GetGridKeyValues( gridData, options.RowKeyFieldPaths ) );
            }

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;

            keys.AddKeys( personEntityTypeId, GetPersonFieldKeyValues( gridData ) );

            if ( options.PersonKeyFieldPaths != null )
            {
                keys.AddKeys( personEntityTypeId, GetGridKeyValues( gridData, options.PersonKeyFieldPaths ) );
            }
        }

        /// <summary>
        /// Gets the person keys of the person fields in the rows of the grid data.
        /// </summary>
        /// <param name="gridData">The grid data that was sent to the client.</param>
        /// <returns>The person keys.</returns>
        private static IEnumerable<string> GetPersonFieldKeyValues( GridDataBag gridData )
        {
            if ( gridData?.Rows == null )
            {
                yield break;
            }

            foreach ( var value in gridData.Rows.Where( r => r != null ).SelectMany( r => r.Values ) )
            {
                var people = value is PersonFieldBag person
                    ? new[] { person }
                    : value as IEnumerable<PersonFieldBag>;

                if ( people == null )
                {
                    continue;
                }

                foreach ( var idKey in people.Select( p => p?.IdKey ) )
                {
                    if ( idKey.IsNotNullOrWhiteSpace() )
                    {
                        yield return idKey;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the entity key values found in the rows of the grid data.
        /// </summary>
        /// <param name="gridData">The grid data that was sent to the client.</param>
        /// <param name="keyFieldPaths">The dot separated paths of the row values that hold entity keys, or <c>null</c> to search each row for key properties.</param>
        /// <returns>The key values, compared with exact case.</returns>
        /// <remarks>
        /// When <paramref name="keyFieldPaths"/> is <c>null</c>, only values of
        /// properties named <c>IdKey</c>, <c>Guid</c> or <c>Id</c>, or ending in
        /// <c>IdKey</c> or <c>Guid</c>, are included, at any depth in the row.
        /// Integers are only included from <c>Id</c> properties. When paths are
        /// provided, every value found at those paths is included, and text
        /// values are also split into the separate keys they list.
        /// </remarks>
        internal static HashSet<string> GetGridKeyValues( GridDataBag gridData, IEnumerable<string> keyFieldPaths = null )
        {
            // IdKeys are case sensitive, so keys must match exactly.
            var keys = new HashSet<string>( StringComparer.Ordinal );

            if ( gridData?.Rows == null )
            {
                return keys;
            }

            var paths = keyFieldPaths?.Where( p => p.IsNotNullOrWhiteSpace() ).Distinct().ToList();

            foreach ( var row in gridData.Rows )
            {
                if ( row == null )
                {
                    continue;
                }

                var rowToken = JObject.FromObject( row );

                if ( paths == null )
                {
                    AddKeyPropertyValues( keys, rowToken );
                    continue;
                }

                foreach ( var path in paths )
                {
                    JToken token = rowToken;

                    foreach ( var segment in path.Split( '.' ) )
                    {
                        token = ( token as JObject )?.GetValue( segment, StringComparison.OrdinalIgnoreCase );
                    }

                    AddDeclaredKeyValues( keys, token );
                }
            }

            return keys;
        }

        /// <summary>
        /// Adds the values of the key properties found anywhere in the row to the set of keys.
        /// </summary>
        /// <param name="keys">The set of keys to add values to.</param>
        /// <param name="row">The grid row to search for key properties.</param>
        private static void AddKeyPropertyValues( HashSet<string> keys, JObject row )
        {
            foreach ( var value in row.Descendants().OfType<JValue>() )
            {
                var propertyName = ( value.Parent as JProperty )?.Name;

                if ( propertyName == null )
                {
                    continue;
                }

                var isIdProperty = propertyName.Equals( "Id", StringComparison.OrdinalIgnoreCase );

                // Free text such as names and notes can hold anything, so
                // only exact key values are trusted and nothing is split.
                if ( value.Type == JTokenType.String || value.Type == JTokenType.Guid )
                {
                    var isKeyProperty = isIdProperty
                        || propertyName.EndsWith( "IdKey", StringComparison.OrdinalIgnoreCase )
                        || propertyName.EndsWith( "Guid", StringComparison.OrdinalIgnoreCase );
                    var text = value.ToString().Trim();

                    if ( isKeyProperty && text.IsNotNullOrWhiteSpace() )
                    {
                        keys.Add( text );
                    }
                }
                else if ( value.Type == JTokenType.Integer && isIdProperty )
                {
                    keys.Add( value.ToString() );
                }
            }
        }

        /// <summary>
        /// Adds the entity key values contained in a declared key field to the set of keys.
        /// </summary>
        /// <param name="keys">The set of keys to add values to.</param>
        /// <param name="token">The value of the declared key field.</param>
        private static void AddDeclaredKeyValues( HashSet<string> keys, JToken token )
        {
            if ( token == null )
            {
                return;
            }

            if ( token is JValue value )
            {
                if ( value.Type == JTokenType.String || value.Type == JTokenType.Guid )
                {
                    var text = value.ToString().Trim();

                    if ( text.IsNullOrWhiteSpace() )
                    {
                        return;
                    }

                    keys.Add( text );

                    // The client splits legacy key strings on these same
                    // separators when it builds the entity set.
                    foreach ( var part in Regex.Split( text, @"[\s|,;]+" ) )
                    {
                        if ( part.IsNotNullOrWhiteSpace() )
                        {
                            keys.Add( part );
                        }
                    }
                }
                else if ( value.Type == JTokenType.Integer )
                {
                    keys.Add( value.ToString() );
                }

                return;
            }

            foreach ( var child in token.Children() )
            {
                AddDeclaredKeyValues( keys, child );
            }
        }

        /// <summary>
        /// <para>
        /// Creates an entity set along with all the set items.
        /// </para>
        /// <para>
        /// Navigation properties are not available on the returned <see cref="EntitySet"/>.
        /// </para>
        /// </summary>
        /// <param name="entitySetBag">The bag that describes the entity set.</param>
        /// <param name="allowedEntityKeys">The entity keys that may be included in the set, or <c>null</c> to allow every key. Items whose key is not allowed for the set's entity type are dropped.</param>
        /// <returns>An instance of <see cref="EntitySet"/> or <c>null</c> if the set would have been empty.</returns>
        public static EntitySet CreateEntitySet( GridEntitySetBag entitySetBag, GridEntityKeys allowedEntityKeys )
        {
            if ( entitySetBag == null )
            {
                throw new ArgumentNullException( nameof( entitySetBag ) );
            }

            // Determine the entity type of the items this entity set will represent.
            var entityType = entitySetBag.EntityTypeKey.IsNotNullOrWhiteSpace()
                ? EntityTypeCache.Get( entitySetBag.EntityTypeKey.AsGuid() )
                : null;

            // Create the basic entity set, expire in 5 minutes.
            var entitySet = new EntitySet()
            {
                EntityTypeId = entityType?.Id,
                ExpireDateTime = RockDateTime.Now.AddMinutes( 5 )
            };

            var entitySetItems = new List<EntitySetItem>();

            using ( var rockContext = new RockContext() )
            {
                if ( entityType != null )
                {
                    // We have an entity type. Lookup all the identifiers from the
                    // supplied entity keys.
                    var allowedKeys = allowedEntityKeys?.GetKeys( entityType.Id );
                    var items = entitySetBag.Items
                        .Where( i => i.EntityKey.IsNotNullOrWhiteSpace() && ( allowedKeys == null || allowedKeys.Contains( i.EntityKey ) ) )
                        .ToList();
                    var entityKeys = items.Select( i => i.EntityKey ).ToList();
                    var entityIdLookup = Rock.Reflection.GetEntityIdsForEntityType( entityType, entityKeys, true, rockContext );

                    // Create an entity set item for each item that was provided.
                    // If we couldn't find an identifier, then skip it.
                    foreach ( var item in items )
                    {
                        if ( !entityIdLookup.TryGetValue( item.EntityKey, out var entityId ) )
                        {
                            continue;
                        }

                        entitySetItems.Add( new EntitySetItem
                        {
                            EntityId = entityId,
                            AdditionalMergeValues = item.AdditionalMergeValues
                        } );
                    }
                }
                else
                {
                    // Non entity type, so just stuff the merge values into the item.
                    entitySetItems.AddRange( entitySetBag.Items.Select( i => new EntitySetItem
                    {
                        EntityId = 0,
                        AdditionalMergeValues = i.AdditionalMergeValues
                    } ) );
                }

                // Return an error if we couldn't create any items.
                if ( !entitySetItems.Any() )
                {
                    return null;
                }

                // Create the entity set first so we can get the identifier.
                var entitySetService = new EntitySetService( rockContext );
                entitySetService.Add( entitySet );
                rockContext.SaveChanges();

                // Use the entity set identifier to populate all the items.
                entitySetItems.ForEach( a =>
                {
                    a.EntitySetId = entitySet.Id;
                } );

                // Insert everything at once, bypassing EF.
                rockContext.BulkInsert( entitySetItems );
            }

            return entitySet;
        }

        /// <summary>
        /// <para>
        /// Creates a transient communication along with all the recipients.
        /// </para>
        /// <para>
        /// Navigation properties are not available on the returned <see cref="Rock.Model.Communication"/>.
        /// </para>
        /// </summary>
        /// <param name="communicationBag">The bag that describes the entity set.</param>
        /// <param name="requestContext">The current request object.</param>
        /// <param name="allowedEntityKeys">The entity keys that may be used, or <c>null</c> to allow every key. Recipients whose key is not allowed as a person are dropped.</param>
        /// <returns>An instance of <see cref="Rock.Model.Communication"/> or <c>null</c> if there would have been no recipients.</returns>
        public static Rock.Model.Communication CreateCommunication( GridCommunicationBag communicationBag, RockRequestContext requestContext, GridEntityKeys allowedEntityKeys )
        {
            using ( var rockContext = new RockContext() )
            {
                var currentPersonAliasId = requestContext?.CurrentPerson?.PrimaryAliasId;
                var allowedKeys = allowedEntityKeys?.GetKeys( EntityTypeCache.Get<Person>().Id );
                var recipients = communicationBag.Recipients
                    .Where( r => r.EntityKey.IsNotNullOrWhiteSpace() && ( allowedKeys == null || allowedKeys.Contains( r.EntityKey ) ) )
                    .ToList();

                // Lookup all the identifiers from the supplied entity keys.
                var personKeys = recipients.Select( i => i.EntityKey ).ToList();
                var personIdLookup = Rock.Reflection.GetEntityIdsForEntityType( EntityTypeCache.Get<Person>(), personKeys, true, rockContext );

                if ( personIdLookup.Count == 0 )
                {
                    return null;
                }

                // Create the blank communication to be filled in later.
                var communicationRockContext = new RockContext();
                var communicationService = new CommunicationService( communicationRockContext );
                var communication = new Rock.Model.Communication
                {
                    IsBulkCommunication = true,
                    Status = Model.CommunicationStatus.Transient,
                    AdditionalMergeFields = communicationBag.MergeFields,
                    SenderPersonAliasId = currentPersonAliasId,
                    UrlReferrer = communicationBag.FromUrl
                };

                // Save communication to get the Id.
                communicationService.Add( communication );
                communicationRockContext.SaveChanges();

                // Get the primary aliases
                var personAliasIdLookup = GetPrimaryAliasIds( personIdLookup.Values.ToList(), rockContext );

                var currentDateTime = RockDateTime.Now;
                var communicationRecipientList = new List<CommunicationRecipient>( recipients.Count );

                foreach ( var item in recipients )
                {
                    if ( !personIdLookup.TryGetValue( item.EntityKey, out var personId ) )
                    {
                        continue;
                    }

                    if ( !personAliasIdLookup.TryGetValue( personId, out var personAliasId ) )
                    {
                        continue;
                    }

                    // NOTE: Set CreatedDateTime, ModifiedDateTime, etc. manually
                    // since we are using BulkInsert.
                    var recipient = new CommunicationRecipient
                    {
                        CommunicationId = communication.Id,
                        PersonAliasId = personAliasId,
                        AdditionalMergeValues = item.AdditionalMergeValues,
                        CreatedByPersonAliasId = currentPersonAliasId,
                        ModifiedByPersonAliasId = currentPersonAliasId,
                        CreatedDateTime = currentDateTime,
                        ModifiedDateTime = currentDateTime
                    };

                    communicationRecipientList.Add( recipient );
                }

                // BulkInsert to quickly insert the CommunicationRecipient records. Note: This is much faster, but will bypass EF and Rock processing.
                var communicationRecipientRockContext = new RockContext();
                communicationRecipientRockContext.BulkInsert( communicationRecipientList );

                return communication;
            }
        }

        /// <summary>
        /// Gets the primary alias ids from the person ids. This is used by the
        /// communication process to translate the person identifiers we have
        /// from the grid into the person alias identifiers needed by the
        /// communication recipient records.
        /// </summary>
        /// <param name="personIds">The person identifiers to be translated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> where the key is the person identifier and the value is the person alias identifier.</returns>
        private static Dictionary<int, int> GetPrimaryAliasIds( List<int> personIds, RockContext rockContext )
        {
            var personAliasIdLookup = new Dictionary<int, int>();
            var personAliasService = new PersonAliasService( rockContext );

            // Get the data in chunks just in case we have a large list of
            // PersonIds (to avoid a SQL Expression limit error).
            while ( personIds.Any() )
            {
                var personIdsChunk = personIds.Take( 1_000 );
                personIds = personIds.Skip( 1_000 ).ToList();

                var chunkedPrimaryAliasIds = personAliasService.Queryable()
                    .Where( pa => pa.PersonId == pa.AliasPersonId && personIdsChunk.Contains( pa.PersonId ) )
                    .Select( pa => new
                    {
                        pa.Id,
                        pa.PersonId
                    } )
                    .ToList();

                foreach ( var aliasId in chunkedPrimaryAliasIds )
                {
                    personAliasIdLookup.TryAdd( aliasId.PersonId, aliasId.Id );
                }
            }

            return personAliasIdLookup;
        }
    }
}
