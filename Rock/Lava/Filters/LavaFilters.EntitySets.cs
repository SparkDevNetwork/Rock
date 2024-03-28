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
using Rock.Web.Cache;

namespace Rock.Lava
{
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Creates an EntitySet with the specified parameters.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">An array or comma-delimited list of identifiers, or a collection of Rock Entities.</param>
        /// <param name="entityType">
        /// The Entity Type of the entities in the set, specified as the Id, Guid, IdKey or Friendly Name of an Entity Type.
        /// Not required if the filter input is an Entity collection.
        /// </param>
        /// <param name="expireInMinutes">An optional time in minutes after which the EntitySet will expire. If not specified, the default expiry is used.</param>
        /// <param name="entitySetPurposeValueId">An optional identifier that indicates the purpose for which the Entity Set is used.</param>
        /// <param name="note">An optional note.</param>
        /// <param name="parentEntitySetId">An optional identifier for an Entity Set that is the parent of this set.</param>
        /// <returns>A new <see cref="Rock.Model.EntitySet"/> object.</returns>
        public static EntitySet CreateEntitySet( ILavaRenderContext context, object input, string entityType = null, string expireInMinutes = null, string entitySetPurposeValueId = null, string note = null, string parentEntitySetId = null )
        {
            const int defaultExpiryInMinutes = 20;

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            // Parse the entityType parameter.
            int? entityTypeId = null;
            if ( entityType != null )
            {
                // Try to get the EntityType by Id, Guid, or IdKey.
                entityTypeId = EntityTypeCache.Get( entityType, allowIntegerIdentifier: true )?.Id;

                if ( entityTypeId == null )
                {
                    // Try to find the EntityType by name.
                    if ( entityType.Contains( "." ) )
                    {
                        entityTypeId = EntityTypeCache.Get( entityType, createNew: false, rockContext )?.Id;
                    }
                    else
                    {
                        entityTypeId = EntityTypeCache.Get( $"Rock.Model.{entityType}", createNew: false, rockContext )?.Id;
                    }
                }
            }

            // Get the list of entity keys to include in the set.
            var entityIdList = new List<int>();

            bool isValid;
            if ( input is IEnumerable<int> inputIntList )
            {
                // Process the input as a collection of id values.
                entityIdList = inputIntList.ToList();
            }
            else if ( input is string inputString )
            {
                // Process the input as a collection of entity id values.
                isValid = InputParser.TryConvertToIntegerList( inputString, out entityIdList, "," );

                if ( !isValid )
                {
                    throw new Exception( $"CreateEntitySet failed. The entity identifier list is invalid. [Input={inputString.Truncate(100)}]" );
                }
            }
            else if ( input is IEnumerable<object> inputList )
            {
                var isEntityCollection = input.IsRockEntityCollection();
                if ( isEntityCollection )
                {
                    // Process as a collection of Rock Entities.
                    var entitiesList = inputList.Cast<IEntity>().Select( e => new { e.Id, e.TypeId } ).ToList();

                    if ( !entityTypeId.HasValue )
                    {
                        // The first entity in the collection determines the type of the entity set.
                        entityTypeId = entitiesList.Select( e => e.TypeId ).FirstOrDefault();
                    }

                    entityIdList = entitiesList.Select( e => e.Id ).ToList();
                }
                else
                {
                    // Process the input as a collection of entity id values.
                    isValid = InputParser.TryConvertToIntegerList( inputList, out entityIdList );

                    if ( !isValid )
                    {
                        throw new Exception( "CreateEntitySet failed. The entity identifier list is invalid." );
                    }
                }
            }
            else 
            {
                throw new Exception( $"CreateEntitySet failed. The input must be a delimited list of key values or a collection of Rock Entities or keys. [InputType={input?.GetType().Name ?? "(empty)"}]" );
            }

            if ( !entityTypeId.HasValue )
            {
                throw new Exception( $"CreateEntitySet failed. The Entity Type is invalid or could not be determined from the input. [EntityType={entityType.IfEmpty("(none)")}]" );
            }

            // Parse the Expiry parameter.
            int? expiryInMinutes = defaultExpiryInMinutes;
            if ( !string.IsNullOrWhiteSpace( expireInMinutes ) )
            {
                expiryInMinutes = InputParser.ConvertToIntegerOrDefault( expireInMinutes, defaultExpiryInMinutes, defaultExpiryInMinutes );
            }

            // Create the entity set and return it.
            var service = new EntitySetService( rockContext );

            var options = new AddEntitySetActionOptions
            {
                EntityTypeId = entityTypeId.Value,
                EntityIdList = entityIdList,
                ExpiryInMinutes = expiryInMinutes,
                Note = note,
                PurposeValueId = entitySetPurposeValueId.AsIntegerOrNull(),
                ParentEntitySetId = parentEntitySetId.AsIntegerOrNull()
            };

            var entitySetId = service.AddEntitySet( options );

            var entitySet = service.Queryable()
                .Include( s => s.Items )
                .FirstOrDefault( s => s.Id == entitySetId );

            return entitySet;
        }
    }
}
