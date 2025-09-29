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
using System.Data.SqlClient;
using System.Linq;

namespace Rock.Data
{
    /// <summary>
    /// <para>
    /// Provides helper methods for retrieving, saving, and deleting metadata
    /// values associated with entities in the database.
    /// </para>
    /// <para>
    /// Metadata is information that is not required for the entity to function,
    /// but provides additional information or computed/cached values.
    /// Modifying metadata does not cause the entity to be considered modified.
    /// </para>
    /// </summary>
    internal class MetadataHelper
    {
        /// <summary>
        /// Retrieves the value associated with the specified key for a given
        /// entity.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/>.</param>
        /// <param name="entityId">The identifier of the <see cref="Data.IEntity"/>.</param>
        /// <param name="key">The metadata key whose value is to be retrieved.</param>
        /// <param name="rockContext">The database context to use for the query.</param>
        /// <returns>A string containing the value associated with the specified key, or null if no matching value is found.</returns>
        public virtual string GetEntityValue( int entityTypeId, int entityId, string key, RockContext rockContext )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                throw new ArgumentOutOfRangeException( nameof( key ), "Key must be provided." );
            }

            return rockContext.Database
                .SqlQuery<string>(
                    "SELECT TOP 1 [Value] FROM [EntityMetadata] WHERE [EntityTypeId] = @p0 AND [EntityId] = @p1 AND [Key] = @p2",
                    entityTypeId,
                    entityId,
                    key )
                .FirstOrDefault();
        }

        /// <summary>
        /// Saves or updates a metadata value for a specified entity and key.
        /// If the value is null or whitespace, the existing metadata entry
        /// for the key is deleted.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/>.</param>
        /// <param name="entityId">The identifier of the <see cref="Data.IEntity"/>.</param>
        /// <param name="key">The metadata key to be updated.</param>
        /// <param name="value">The value to assign to the specified key. If null or whitespace, the metadata entry for the key will be deleted.</param>
        /// <param name="rockContext">The database context to use for the operation.</param>
        public virtual void SaveEntityValue( int entityTypeId, int entityId, string key, string value, RockContext rockContext )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                throw new ArgumentOutOfRangeException( nameof( key ), "Key must be provided." );
            }

            if ( string.IsNullOrWhiteSpace( value ) )
            {
                DeleteEntityValue( entityTypeId, entityId, key, rockContext );
                return;
            }

            rockContext.Database
                .ExecuteSqlCommand(
                    @"
IF EXISTS (SELECT 1 FROM [EntityMetadata] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @EntityId AND [Key] = @Key)
BEGIN
    UPDATE [EntityMetadata]
    SET [Value] = @Value
    WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @EntityId AND [Key] = @Key
END
ELSE
BEGIN
    INSERT INTO [EntityMetadata] ([EntityTypeId], [EntityId], [Key], [Value])
    VALUES (@EntityTypeId, @EntityId, @Key, @Value)
END
",
                    new SqlParameter( "@EntityTypeId", entityTypeId ),
                    new SqlParameter( "@EntityId", entityId ),
                    new SqlParameter( "@Key", key ),
                    new SqlParameter( "@Value", value ) );
        }

        /// <summary>
        /// Deletes the metadata value associated with the specified entity
        /// and key from the database.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/>.</param>
        /// <param name="entityId">The identifier of the <see cref="Data.IEntity"/>.</param>
        /// <param name="key">The key of the metadata value to delete.</param>
        /// <param name="rockContext">The database context to use for the operation.</param>
        public virtual void DeleteEntityValue( int entityTypeId, int entityId, string key, RockContext rockContext )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                throw new ArgumentOutOfRangeException( nameof( key ), "Key must be provided." );
            }

            rockContext.Database
                .ExecuteSqlCommand(
                    "DELETE FROM [EntityMetadata] WHERE [EntityTypeId] = @p0 AND [EntityId] = @p1 AND [Key] = @p2",
                    entityTypeId,
                    entityId,
                    key );
        }

        /// <summary>
        /// Deletes all orphaned metadata values for a specified entity type.
        /// This will be done in chunks to avoid long-running transactions.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/>.</param>
        /// <param name="entityTableName">The name of the table associated with <paramref name="entityTypeId"/>. This will be used in a SQL statement to query the Id column.</param>
        /// <param name="batchSize">The number of rows to delete per query.</param>
        /// <param name="rockContext">The database context to use for the operation.</param>
        /// <returns>The total number of database records deleted.</returns>
        public virtual int DeleteOrphanedEntityValues( int entityTypeId, string entityTableName, int batchSize, RockContext rockContext )
        {
            int iterationCount;
            var deletedRecords = 0;

            do
            {
                iterationCount = rockContext.Database
                    .ExecuteSqlCommand(
                        $@"
DELETE TOP (@p1)
FROM [EntityMetadata]
WHERE [EntityTypeId] = @p0
  AND [EntityId] NOT IN (SELECT [Id] FROM [{entityTableName}])",
                        entityTypeId,
                        batchSize );

                deletedRecords += iterationCount;
            } while ( iterationCount > 0 );

            return deletedRecords;
        }
    }
}
