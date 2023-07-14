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
using System.Linq;
using System.Text;
using EntityFramework.Utilities;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v15 to replace WebForms blocks with Obsidian blocks.
    /// </summary>
    [DisplayName( "Rock Update Helper v15.0 - Replace WebForms Blocks with Obsidian blocks" )]
    [Description( "This job will replace WebForms blocks with Obsidian blocks on all sites, pages, and layouts." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400,
        Order = 0 )]

    [KeyValueListField(
        "Block Type Guid Replacement Pairs",
        Key = AttributeKey.BlockTypeGuidReplacementPairs,
        Description = "The key-value pairs of replacement BlockType.Guid values, where the key is the existing BlockType.Guid and the value is the new BlockType.Guid. Blocks of BlockType.Guid == key will be replaced by blocks of BlockType.Guid == value in all sites, pages, and layouts.",
        IsRequired = true,
        Order = 1 )]

    [BooleanField(
        "Should Keep Old Blocks",
        Key = AttributeKey.ShouldKeepOldBlocks,
        Description = "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 2 )]

    [RockInternal( "1.15" )]
    internal class PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianBlocks : RockJob
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
            public const string ShouldKeepOldBlocks = "ShouldKeepOldBlocks";
            public const string BlockTypeGuidReplacementPairs = "BlockTypeGuidReplacementPairs";
        }

        #endregion

        #region Properties

        private Dictionary<Guid, Guid> BlockTypeGuidReplacementPairs
        {
            get
            {
                return new Field.Types.KeyValueListFieldType().GetValuesFromString( null, GetAttributeValue( AttributeKey.BlockTypeGuidReplacementPairs ), null, false )
                    // Calling Guid?.Value intentionally on the next line so that exceptions are thrown if the field value is invalid.
                    .ToDictionary( kvp => kvp.Key.AsGuidOrNull().Value, kvp => kvp.Value.ToString().AsGuidOrNull().Value );
            }
        }

        /// <summary>
        /// Determines if old blocks should be kept instead of being deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> to keep old blocks; otherwise, <c>false</c> to delete old blocks.
        /// </value>
        private bool ShouldKeepOldBlocks => this.GetAttributeValue( AttributeKey.ShouldKeepOldBlocks ).AsBoolean();

        #endregion

        /// <inheritdoc/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );
            using ( var rockContext = new RockContext() )
            {
                ReplaceBlocks( rockContext, migrationHelper );
            }

            DeleteJob();
        }

        /// <summary>
        /// Replaces site, page, and layout WebForms block instances with Obsidian block instances.
        /// <para>Uses a combination of EF and the migration helper.</para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="migrationHelper">The migration helper.</param>
        private void ReplaceBlocks( RockContext rockContext, MigrationHelper migrationHelper )
        {
            foreach ( var blockTypeGuidPair in BlockTypeGuidReplacementPairs )
            {
                ReplaceBlocksOfOneBlockTypeWithBlocksOfAnotherBlockType( blockTypeGuidPair.Key, blockTypeGuidPair.Value, rockContext, migrationHelper );
            }
        }

        /// <summary>
        /// Replaces blocks of one block type with blocks of another block type.
        /// <para>Uses a combination of EF and the migration helper.</para>
        /// </summary>
        /// <param name="oldBlockTypeGuid">The old block type unique identifier.</param>
        /// <param name="newBlockTypeGuid">The new block type unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="migrationHelper">The migration helper.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// $"BlockType could not be found for guid '{oldBlockTypeGuid}'
        /// or
        /// $"BlockType could not be found for guid '{newBlockTypeGuid}'
        /// or
        /// $"EntityType could not be found for guid '{SystemGuid.EntityType.BLOCK_TYPE}'
        /// </exception>
        private void ReplaceBlocksOfOneBlockTypeWithBlocksOfAnotherBlockType( Guid oldBlockTypeGuid, Guid newBlockTypeGuid, RockContext rockContext, MigrationHelper migrationHelper )
        {
            var oldBlockTypeId = BlockTypeCache.GetId( oldBlockTypeGuid );
            if ( !oldBlockTypeId.HasValue )
            {
                throw new Exception( $"BlockType could not be found for guid '{oldBlockTypeGuid}'" );
            }

            var newBlockTypeId = BlockTypeCache.GetId( newBlockTypeGuid );
            if ( !newBlockTypeId.HasValue )
            {
                throw new Exception( $"BlockType could not be found for guid '{newBlockTypeGuid}'" );
            }

            var blockTypeEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.BLOCK_TYPE.AsGuid() );
            if ( !blockTypeEntityTypeId.HasValue )
            {
                throw new Exception( $"EntityType could not be found for guid '{SystemGuid.EntityType.BLOCK_TYPE}'" );
            }

            var copiedBlockMappings = AddCopiesOfBlocksInSameLocationsButWithNewBlockType( oldBlockTypeGuid, newBlockTypeId.Value, rockContext );

            CopyAttributeQualifiersAndValuesFromOldBlocksToNewBlocks( rockContext, migrationHelper, copiedBlockMappings );

            CopyAuthFromOldBlocksToNewBlocks( migrationHelper, copiedBlockMappings );

            DeleteOldBlocks( migrationHelper, copiedBlockMappings );
        }

        /// <summary>
        /// For each blocks of a given block type, adds a copy of that block in the same location but with a different block type.
        /// </summary>
        /// <param name="oldBlockTypeGuid">The old block type unique identifier.</param>
        /// <param name="newBlockTypeId">The new block type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A mapping from the old block to the new block.</returns>
        private static Dictionary<Model.Block, Model.Block> AddCopiesOfBlocksInSameLocationsButWithNewBlockType( Guid oldBlockTypeGuid, int newBlockTypeId, RockContext rockContext )
        {
            var blockService = new BlockService( rockContext );

            var oldBlocks = blockService.Queryable()
                .Where( b => b.BlockType.Guid == oldBlockTypeGuid )
                .OrderBy( b => b.Order )
                .ToList();
            var replacedBlocksMap = new Dictionary<Model.Block, Model.Block>();

            // Add the new blocks as copies of the old blocks.
            foreach ( var oldBlock in oldBlocks )
            {
                // Shallow clone the old block without copying its identity information so we can save new blocks.
                var newBlock = oldBlock.CloneWithoutIdentity();

                // Overwrite the block type ID.
                newBlock.BlockTypeId = newBlockTypeId;

                blockService.Add( newBlock );

                // Keep track of the block instance that is replacing the old one.
                replacedBlocksMap.AddOrReplace( oldBlock, newBlock );
            }

            // Save the new blocks.
            rockContext.SaveChanges();

            return replacedBlocksMap;
        }

        private static void CopyAttributeQualifiersAndValuesFromOldBlocksToNewBlocks( RockContext rockContext, MigrationHelper migrationHelper, Dictionary<Model.Block, Model.Block> copiedBlockMappings )
        {
            // Load the attributes for old and new blocks so we can copy values.
            copiedBlockMappings.Keys.LoadAttributes( rockContext );
            copiedBlockMappings.Values.LoadAttributes( rockContext );

            // Copy attribute qualifiers.
            foreach ( var copiedBlockMapping in copiedBlockMappings )
            {
                var oldBlock = copiedBlockMapping.Key;
                var newBlock = copiedBlockMapping.Value;

                foreach ( var oldBlockAttributeValueKvp in oldBlock.Attributes )
                {
                    var oldBlockAttribute = oldBlockAttributeValueKvp.Value;

                    // Check if the new block has an attribute with the same attribute key.
                    if ( newBlock.Attributes.TryGetValue( oldBlockAttribute.Key, out var newBlockAttribute ) )
                    {
                        // Copy the attribute qualifiers from the old block attribute to the new block attribute.
                        foreach ( var qualifierKvp in oldBlockAttribute.QualifierValues )
                        {
                            migrationHelper.AddAttributeQualifierForSQL( newBlockAttribute.Guid.ToString(), qualifierKvp.Key, qualifierKvp.Value.Value, Guid.NewGuid().ToString() );
                        }
                    }
                }
            }

            // Copy attribute values.
            foreach ( var copiedBlockMapping in copiedBlockMappings )
            {
                var oldBlock = copiedBlockMapping.Key;
                var newBlock = copiedBlockMapping.Value;

                // Copy attribute values from the old block to the new block.
                foreach ( var oldBlockAttributeValueKvp in oldBlock.AttributeValues )
                {
                    var oldBlockAttributeValue = oldBlockAttributeValueKvp.Value;

                    // Check if the new block has an attribute value with the same attribute key.
                    if ( newBlock.Attributes.TryGetValue( oldBlockAttributeValueKvp.Key, out var newBlockAttribute ) )
                    {
                        // Copy the attribute value from the old block to the new block.
                        migrationHelper.AddBlockAttributeValue( newBlock.Guid.ToString(), newBlockAttribute.Guid.ToString(), oldBlockAttributeValue.Value );
                    }
                }
            }
        }

        private void DeleteOldBlocks( MigrationHelper migrationHelper, Dictionary<Model.Block, Model.Block> copiedBlockMappings )
        {
            if ( !this.ShouldKeepOldBlocks )
            {
                foreach ( var oldBlock in copiedBlockMappings.Keys )
                {
                    migrationHelper.DeleteBlock( oldBlock.Guid.ToString() );
                }
            }
        }

        private static void CopyAuthFromOldBlocksToNewBlocks( MigrationHelper migrationHelper, Dictionary<Model.Block, Model.Block> copiedBlockMappings )
        {
            foreach ( var copiedBlockMapping in copiedBlockMappings )
            {
                var oldBlock = copiedBlockMapping.Key;
                var newBlock = copiedBlockMapping.Value;

                migrationHelper.CopySecurityForEntity( SystemGuid.EntityType.BLOCK, oldBlock.Guid.ToString(), newBlock.Guid.ToString() );
            }
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}