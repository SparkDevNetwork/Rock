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
        /// <para>
        /// Creates an entity set along with all the set items.
        /// </para>
        /// <para>
        /// Navigation properties are not available on the returned <see cref="EntitySet"/>.
        /// </para>
        /// </summary>
        /// <param name="entitySetBag">The bag that describes the entity set.</param>
        /// <returns>An instance of <see cref="EntitySet"/> or <c>null</c> if the set would have been empty.</returns>
        public static EntitySet CreateEntitySet( GridEntitySetBag entitySetBag )
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
                    var entityKeys = entitySetBag.Items.Select( i => i.EntityKey ).ToList();
                    var entityIdLookup = Rock.Reflection.GetEntityIdsForEntityType( entityType, entityKeys, true, rockContext );

                    // Create an entity set item for each item that was provided.
                    // If we couldn't find an identifier, then skip it.
                    foreach ( var item in entitySetBag.Items )
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
        /// <returns>An instance of <see cref="Rock.Model.Communication"/> or <c>null</c> if there would have been no recipients.</returns>
        public static Rock.Model.Communication CreateCommunication( GridCommunicationBag communicationBag, RockRequestContext requestContext )
        {
            using ( var rockContext = new RockContext() )
            {
                var currentPersonAliasId = requestContext?.CurrentPerson?.PrimaryAliasId;

                // Lookup all the identifiers from the supplied entity keys.
                var personKeys = communicationBag.Recipients.Select( i => i.EntityKey ).ToList();
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
                var communicationRecipientList = new List<CommunicationRecipient>( communicationBag.Recipients.Count );

                foreach ( var item in communicationBag.Recipients )
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
