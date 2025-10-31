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
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using Rock.Attribute;
using Rock.Core;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Updates the usage details about what entities are being used by other elements of Rock.
    /// </summary>
    [DisplayName( "Update Entity Usage" )]
    [Description( "Updates the usage details about what entities are being used by other elements of Rock." )]

    #region Job Attributes

    [EnumField( "Usage Type",
        Key = AttributeKey.UsageType,
        EnumSourceType = typeof( UsageType ),
        Description = "The type of usage metrics to calculate.",
        IsRequired = true,
        Order = 0 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the SQL operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Order = 1 )]

    #endregion

    public class UpdateEntityUsage : RockJob
    {
        #region Keys

        private static class AttributeKey
        {
            public const string UsageType = "UsageType";

            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override void Execute()
        {
            var usageType = GetAttributeValue( AttributeKey.UsageType ).ConvertToEnumOrNull<UsageType>();
            var processedCount = 0;

            if ( usageType == UsageType.MediaUsage )
            {
                using ( var rockContext = CreateRockContext() )
                {
                    UpdateMediaUsage( rockContext, ref processedCount );
                }
            }
            else
            {
                throw new Exception( "Invalid job configuration. A valid Usage Type must be selected." );
            }

            UpdateLastStatusMessage( $"Processed {processedCount:N0} items." );
        }

        /// <summary>
        /// Updates the metadata for all media elements to reflect their
        /// current usage by entities referencing them through Media Element
        /// attributes.
        /// </summary>
        /// <param name="rockContext">The database context to use for retrieving and updating media element and attribute data.</param>
        /// <param name="processedCount">The number of media elements that were processed.</param>
        internal void UpdateMediaUsage( RockContext rockContext, ref int processedCount )
        {
            var mediaElementFieldTypeGuid = Rock.SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid();

            // Find all attributes that are of type Media Element.
            var attributeIdQry = new AttributeService( rockContext )
                .Queryable()
                .Where( a => a.FieldType.Guid == mediaElementFieldTypeGuid
                    && a.EntityTypeId.HasValue )
                .Select( a => a.Id );

            // Find all attribute values that reference a Media Element.
            var referencingEntities = new AttributeValueService( rockContext )
                .Queryable()
                .Where( av => attributeIdQry.Contains( av.AttributeId )
                    && !string.IsNullOrEmpty( av.Value )
                    && av.EntityId.HasValue )
                .Select( av => new
                {
                    MediaElementGuid = av.Value,
                    EntityTypeId = av.Attribute.EntityTypeId.Value,
                    av.EntityId
                } )
                .ToList();

            // We then filter out any that don't have a valid Guid value.
            var validReferencingEntities = referencingEntities
                .Select( re => new ReferenceEntity<Guid>
                {
                    TargetKey = re.MediaElementGuid.AsGuid(),
                    EntityTypeId = re.EntityTypeId,
                    EntityId = re.EntityId.Value
                } )
                .Where( re => re.TargetKey != Guid.Empty )
                .ToList();

            var mediaElementReferences = LoadSummaries( validReferencingEntities, rockContext );

            // Load all media elements so we can start processing them.
            // We need to load everything because we need to clear out any
            // existing metadata values that are no longer valid.
            var mediaElements = new MediaElementService( rockContext )
                .Queryable()
                .ToList();

            foreach ( var mediaElement in mediaElements )
            {
                if ( mediaElementReferences.TryGetValue( mediaElement.Guid, out var summaries ) && summaries.Any() )
                {
                    mediaElement.SaveMetadataValue( MetadataKey.EntityUsage, summaries, rockContext );
                }
                else
                {
                    mediaElement.DeleteMetadataValue( MetadataKey.EntityUsage, rockContext );
                }

                processedCount++;
            }
        }

        /// <summary>
        /// Loads the linkage summaries for the specified referencing entities.
        /// </summary>
        /// <typeparam name="TKey">The type of the key used when matching referencing to the returned dictionary.</typeparam>
        /// <param name="referencingEntities">The entity references that need to be summarized.</param>
        /// <param name="rockContext">The database context to use when querying the database</param>
        /// <returns>A dictionary of <see cref="LinkageSummary"/> records grouped by the key that identifies the target entity.</returns>
        private IDictionary<TKey, List<LinkageSummary>> LoadSummaries<TKey>( IList<ReferenceEntity<TKey>> referencingEntities, RockContext rockContext )
        {
            var mediaElementReferences = new Dictionary<TKey, List<LinkageSummary>>();

            // Also group them by entity type for fast loading.
            var groupedEntitiesByEntityType = referencingEntities
                .GroupBy( re => re.EntityTypeId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            foreach ( var entityTypeGroup in groupedEntitiesByEntityType )
            {
                var entityQry = GetEntityQuerayble( entityTypeGroup.Key, rockContext );

                if ( entityQry == null )
                {
                    continue;
                }

                var ids = entityTypeGroup.Value.Select( e => e.EntityId ).ToList();
                var entities = new List<IEntity>();

                while ( ids.Count > 0 )
                {
                    var chunkIds = ids.Take( 1000 ).ToList();
                    ids = ids.Skip( 1000 ).ToList();

                    var chunk = entityQry
                        .Where( e => chunkIds.Contains( e.Id ) )
                        .ToList();

                    entities.AddRange( chunk );
                }

                foreach ( var entity in entities )
                {
                    var reference = entityTypeGroup.Value.First( e => e.EntityId == entity.Id );
                    var summary = entity.SummarizeLinkage( rockContext );

                    if ( !mediaElementReferences.TryGetValue( reference.TargetKey, out var summaries ) )
                    {
                        summaries = new List<LinkageSummary>();
                        mediaElementReferences.Add( reference.TargetKey, summaries );
                    }

                    summaries.Add( summary );
                }
            }

            return mediaElementReferences;
        }

        /// <summary>
        /// Small helper method to get the queryable for a given entity type
        /// identifier.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the entity type.</param>
        /// <param name="rockContext">The database context to use when loading data.</param>
        /// <returns>A queryable or <c>null</c> if the entity type was not valid.</returns>
        [ExcludeFromCodeCoverage]
        private IQueryable<IEntity> GetEntityQuerayble( int entityTypeId, RockContext rockContext )
        {
            var type = EntityTypeCache.Get( entityTypeId, rockContext )?.GetEntityType();

            if ( type == null )
            {
                return null;
            }

            return Reflection.GetQueryableForEntityType( type, rockContext );
        }

        /// <summary>
        /// Creates and configures a new database context for use by the job.
        /// </summary>
        /// <returns>An instance of <see cref="RockContext"/>.</returns>
        [ExcludeFromCodeCoverage]
        private RockContext CreateRockContext()
        {
            var rockContext = new RockContext();

            rockContext.Database.SetCommandTimeout( GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180 );

            return rockContext;
        }

        #endregion

        #region Support Classes

        private class ReferenceEntity<T>
        {
            public T TargetKey { get; set; }

            public int EntityTypeId { get; set; }

            public int EntityId { get; set; }
        }

        #endregion

        private enum UsageType
        {
            MediaUsage = 0,
        }
    }
}
