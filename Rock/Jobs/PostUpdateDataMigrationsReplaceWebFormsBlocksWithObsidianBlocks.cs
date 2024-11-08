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
using System.IO;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
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

    [CustomRadioListField(
        "Migration Strategy",
        Key = AttributeKey.MigrationStrategy,
        Description = "Determines if the blocks should be chopped instead of swapped. By default,the old blocks are swapped with the new ones.",
        IsRequired = true,
        DefaultValue = "Swap",
        ListSource = "Swap, Chop",
        Order = 2 )]

    [BooleanField(
        "Should Keep Old Blocks (FOR TESTING PURPOSES ONLY)",
        Key = AttributeKey.ShouldKeepOldBlocks,
        Description = "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted. NOTE: This attribute is ignored incase of the Chop Migration Strategy.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 3 )]

    [KeyValueListField( "Webform BlockType Attribute Keys To Ignore During Validation",
        Description = "A Guid [key] of the old Webform BlockType and the [value] as a comma delimited list of BlockType Attribute keys to ignore when validating the Obsidian and Webforms blocks have the same keys.",
        Key = AttributeKey.BlockAttributeKeysToIgnore,
        IsRequired = false,
        Order = 4 )]

    [RockInternal( "1.16" )]
    internal class PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks : RockJob
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
            public const string ShouldKeepOldBlocks = "ShouldKeepOldBlocks";
            public const string BlockTypeGuidReplacementPairs = "BlockTypeGuidReplacementPairs";
            public const string MigrationStrategy = "MigrationStrategy";
            public const string BlockAttributeKeysToIgnore = "BlockAttributeKeysToIgnore";
        }

        #endregion

        #region Properties

        private readonly List<string> ErrorMessage = new List<string>();

        private Dictionary<Guid, Guid> BlockTypeGuidReplacementPairs
        {
            get
            {
                return new Field.Types.KeyValueListFieldType().GetValuesFromString( null, GetAttributeValue( AttributeKey.BlockTypeGuidReplacementPairs ), null, false )
                    // Calling Guid?.Value intentionally on the next line so that exceptions are thrown if the field value is invalid.
                    .ToDictionary( kvp => kvp.Key.AsGuidOrNull().Value, kvp => kvp.Value.ToString().AsGuidOrNull().Value );
            }
        }

        private Dictionary<Guid, HashSet<string>> BlockAttributeKeysToIgnore
        {
            get
            {
                return new Field.Types.KeyValueListFieldType().GetValuesFromString( null, GetAttributeValue( AttributeKey.BlockAttributeKeysToIgnore ), null, false )
                    // Calling Guid?.Value intentionally on the next line so that exceptions are thrown if the field value is invalid.
                    .ToDictionary( kvp => kvp.Key.AsGuidOrNull().Value, kvp => kvp.Value.ToString().SplitDelimitedValues().ToHashSet() );
            }
        }

        /// <summary>
        /// Determines if old blocks should be kept instead of being deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> to keep old blocks; otherwise, <c>false</c> to delete old blocks.
        /// </value>
        private bool ShouldKeepOldBlocks => this.GetAttributeValue( AttributeKey.ShouldKeepOldBlocks ).AsBoolean();

        /// <summary>
        /// Determines if old blocks should chopped off and be deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> to delete the old block type from Rock; otherwise, <c>false</c> Keep the old block type.
        /// </value>
        private string MigrationStrategy => this.GetAttributeValue( AttributeKey.MigrationStrategy );

        #endregion

        #region Public Methods

        /// <summary>
        /// The function to serialize the key value pair of the current guid to the new guid to be stored in the AttributeValue table in the database
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string SerializeDictionary( Dictionary<string, string> dictionary )
        {
            const string keyValueSeparator = "^";

            if ( dictionary?.Any() != true )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var first = dictionary.First();
            sb.Append( $"{first.Key}{keyValueSeparator}{first.Value}" );

            foreach ( var kvp in dictionary.Skip( 1 ) )
            {
                sb.Append( $"|{kvp.Key}{keyValueSeparator}{kvp.Value}" );
            }

            return sb.ToString();
        }

        #endregion

        /// <inheritdoc/>
        public override void Execute()
        {
            ReplaceBlocks();

            if ( ErrorMessage.Any() )
            {
                // If there were errors, fail the job and make it non-system so that the admins may choose to run it again or delete it based on their discretion
                RockContext rockContext = new RockContext();
                var serviceJob = ( new ServiceJobService( rockContext ) ).Get( this.ServiceJobId );
                serviceJob.IsSystem = false;
                rockContext.SaveChanges();

                throw new RockJobWarningException( string.Join( ",\n", ErrorMessage ) );
            }

            DeleteJob();
        }

        /// <summary>
        /// Replaces site, page, and layout WebForms block instances with Obsidian block instances.
        /// <para>Uses a combination of EF and the migration helper.</para>
        /// </summary>
        private void ReplaceBlocks()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            // Register the attributes of the new block type in the database if not already registered.
            BlockTypeService.VerifyBlockTypeInstanceProperties( BlockTypeGuidReplacementPairs.Values.Select( g => BlockTypeCache.GetId( g ).Value ).ToArray() );

            foreach ( var blockTypeGuidPair in BlockTypeGuidReplacementPairs )
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;
                    var jobMigration = new JobMigration( rockContext );
                    var migrationHelper = new MigrationHelper( jobMigration );
                    ReplaceBlocksOfOneBlockTypeWithBlocksOfAnotherBlockType( blockTypeGuidPair.Key, blockTypeGuidPair.Value, rockContext, migrationHelper );
                }
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
            // If the old block is not found in the Cache, it mostly likely was deleted in a previous migration in a previous version.
            // So we merely log it to the exception table and continue
            if ( !oldBlockTypeId.HasValue )
            {
                ExceptionLogService.LogException( $"BlockType could not be found for guid '{oldBlockTypeGuid}' for the current block" );
                return;
            }

            var newBlockTypeId = BlockTypeCache.GetId( newBlockTypeGuid );
            if ( !newBlockTypeId.HasValue )
            {
                ErrorMessage.Add( $"BlockType could not be found for guid '{newBlockTypeGuid}' for the new block" );
                return;
            }

            var blockTypeEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.BLOCK_TYPE.AsGuid() );
            if ( !blockTypeEntityTypeId.HasValue )
            {
                ErrorMessage.Add( $"EntityType could not be found for guid '{SystemGuid.EntityType.BLOCK_TYPE}'" );
                return;
            }

            var blockEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.BLOCK.AsGuid() );

            var oldBlockTypeAttributeKeys = AttributeCache.GetByEntityTypeQualifier( blockEntityTypeId, "BlockTypeId", oldBlockTypeId.ToString(), false )
                .Select( a => a.Key )
                .ToHashSet( StringComparer.OrdinalIgnoreCase );
            oldBlockTypeAttributeKeys.RemoveAll( BlockAttributeKeysToIgnore.GetValueOrDefault( oldBlockTypeGuid, new HashSet<string>() ) );
            var newBlockTypeAttributeKeys = AttributeCache.GetByEntityTypeQualifier( blockEntityTypeId, "BlockTypeId", newBlockTypeId.ToString(), false )
                .Select( a => a.Key )
                .ToHashSet( StringComparer.OrdinalIgnoreCase );

            // If the new block type fails to have all the required attributes of the old block type which it is replacing, skip it.
            if ( !oldBlockTypeAttributeKeys.IsSubsetOf( newBlockTypeAttributeKeys ) )
            {
                var missingBlockAttributes = new HashSet<string>( oldBlockTypeAttributeKeys, StringComparer.OrdinalIgnoreCase );
                missingBlockAttributes.RemoveAll( newBlockTypeAttributeKeys );
                ErrorMessage.Add( $"The new {BlockTypeCache.Get( newBlockTypeId.Value ).Name} block does not have the attribute(s): {missingBlockAttributes.Select( a => a ).JoinStrings( ", " )} of the previous {BlockTypeCache.Get( oldBlockTypeId.Value ).Name} block. Skipping this block for now." );
                return;
            }

            try
            {
                var flushPageIds = new List<int>();
                var flushLayoutIds = new List<int>();
                var flushSiteIds = new List<int>();

                rockContext.WrapTransaction( () =>
                {
                    var copiedBlockMappings = AddCopiesOfBlocksInSameLocationsButWithNewBlockType( oldBlockTypeGuid, newBlockTypeId.Value, rockContext );
                    rockContext.SaveChanges(); // saving the new blocks so that the attributes and person preferences may be copied over.

                    foreach ( var block in copiedBlockMappings.Keys )
                    {
                        if ( block.PageId.HasValue && !flushPageIds.Contains( block.PageId.Value ) )
                        {
                            flushPageIds.Add( block.PageId.Value );
                        }

                        if ( block.LayoutId.HasValue && !flushLayoutIds.Contains( block.LayoutId.Value ) )
                        {
                            flushLayoutIds.Add( block.LayoutId.Value );
                        }

                        if ( block.SiteId.HasValue && !flushSiteIds.Contains( block.SiteId.Value ) )
                        {
                            flushSiteIds.Add( block.SiteId.Value );
                        }
                    }

                    CopyAttributeValuesFromOldBlocksToNewBlocks( rockContext, migrationHelper, copiedBlockMappings, oldBlockTypeAttributeKeys );

                    CopyAuthFromOldBlocksToNewBlocks( migrationHelper, copiedBlockMappings );

                    CopyPersonPreferenceFromOldBlocksToNewBlocks( rockContext, copiedBlockMappings );

                    DeleteOldBlocks( migrationHelper, copiedBlockMappings );

                    ChopBlock( oldBlockTypeGuid, rockContext );
                    rockContext.SaveChanges();
                } );

                foreach ( var pageId in flushPageIds )
                {
                    PageCache.FlushPage( pageId );
                }

                foreach ( var layoutId in flushLayoutIds )
                {
                    PageCache.FlushPagesForLayout( layoutId );
                }

                foreach ( var siteId in flushSiteIds )
                {
                    PageCache.FlushPagesForSite( siteId );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                ErrorMessage.Add( $"Error while replacing the block {BlockTypeCache.Get( oldBlockTypeId.Value ).Name} with the block {BlockTypeCache.Get( newBlockTypeId.Value ).Name}. Error message: {ex.Message}" );
                return;
            }
        }

        private void CopyPersonPreferenceFromOldBlocksToNewBlocks( RockContext rockContext, Dictionary<Block, Block> copiedBlockMappings )
        {
            var blockEntityType = EntityTypeCache.Get( typeof( Block ) );
            PersonPreferenceService personPreferenceService = new PersonPreferenceService( rockContext );
            var oldBlockIds = copiedBlockMappings.Keys
                .Select( b => b.Id )
                .ToList();
            var blockIdMap = copiedBlockMappings
                .ToDictionary( c => c.Key.Id, c => c.Value.Id );
            var oldBlocksPreferences = personPreferenceService
                .Queryable()
                .AsNoTracking()
                .Where( p => p.EntityTypeId == blockEntityType.Id && oldBlockIds.Contains( p.EntityId.Value ) )
                .ToList();

            var newBlockPreferences = new List<PersonPreference>();
            foreach ( var oldBlockPersonPreference in oldBlocksPreferences )
            {
                // Copy over the Person preference from the old block to new block with the appropriate key
                // For instance for an old block with Block Id = 913 and Person Preference Key = block-913-GroupIds
                // the new corresponding block having the Id = 1279 will have the Person Preference Key = block-1279-GroupIds
                var newBlockPersonPreference = oldBlockPersonPreference.CloneWithoutIdentity();
                newBlockPersonPreference.EntityId = blockIdMap[newBlockPersonPreference.EntityId.Value];
                newBlockPreferences.Add( newBlockPersonPreference );
                var newBlockPersonPreferenceKeyPrefix = PersonPreferenceService.GetPreferencePrefix( blockEntityType.GetEntityType(), newBlockPersonPreference.EntityId.ToIntSafe() );
                var oldBlockPersonPreferenceKeyPrefix = PersonPreferenceService.GetPreferencePrefix( blockEntityType.GetEntityType(), oldBlockPersonPreference.EntityId.ToIntSafe() );
                newBlockPersonPreference.Key = $"{newBlockPersonPreferenceKeyPrefix}{oldBlockPersonPreference.Key.Substring( oldBlockPersonPreferenceKeyPrefix.Length )}";
            }

            personPreferenceService.AddRange( newBlockPreferences );
            rockContext.SaveChanges();
        }

        private void ChopBlock( Guid oldBlockTypeGuid, RockContext rockContext )
        {
            if ( this.MigrationStrategy != "Chop" )
            {
                return;
            }
            var blockTypeService = new BlockTypeService( rockContext );
            var blockTypeToBeDeleted = blockTypeService.Get( oldBlockTypeGuid );

            var blockTypeFilePath = blockTypeToBeDeleted?.Path?
                .Replace( '/', Path.DirectorySeparatorChar )
                .Replace( "~", AppDomain.CurrentDomain.BaseDirectory ) ?? "";
            if ( File.Exists( blockTypeFilePath ) )
            {
                File.Delete( blockTypeFilePath );
            }
            var sourceCodeBlockTypeFilePath = blockTypeFilePath + ".cs";
            if ( File.Exists( sourceCodeBlockTypeFilePath ) )
            {
                File.Delete( sourceCodeBlockTypeFilePath );
            }
            blockTypeService.Delete( blockTypeToBeDeleted );
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

            return replacedBlocksMap;
        }

        private static void CopyAttributeValuesFromOldBlocksToNewBlocks( RockContext rockContext, MigrationHelper migrationHelper, Dictionary<Model.Block, Model.Block> copiedBlockMappings, HashSet<string> oldBlockTypeAttributes )
        {
            // Load the attributes for old and new blocks so we can copy values.
            copiedBlockMappings.Keys.LoadAttributes( rockContext );
            copiedBlockMappings.Values.LoadAttributes( rockContext );

            // Copy attribute values.
            foreach ( var copiedBlockMapping in copiedBlockMappings )
            {
                var oldBlock = copiedBlockMapping.Key;
                var newBlock = copiedBlockMapping.Value;
                var oldBlockAttributeValues = oldBlock.AttributeValues
                    .Where( a => oldBlockTypeAttributes.Contains( a.Key ) )
                    .ToDictionary( a => a.Key, a => a.Value );
                var newBlockAttributes = newBlock.Attributes.ToDictionary( a => a.Key, a => a.Value, StringComparer.OrdinalIgnoreCase );

                // Copy attribute values from the old block to the new block.
                foreach ( var oldBlockAttributeValueKvp in oldBlockAttributeValues )
                {
                    var oldBlockAttributeValue = oldBlockAttributeValueKvp.Value;

                    // Check if the new block has an attribute value with the same attribute key.
                    if ( newBlockAttributes.TryGetValue( oldBlockAttributeValueKvp.Key, out var newBlockAttribute ) )
                    {
                        // Copy the attribute value from the old block to the new block.
                        migrationHelper.AddBlockAttributeValue( newBlock.Guid.ToString(), newBlockAttribute.Guid.ToString(), oldBlockAttributeValue.Value );
                    }
                }
            }
        }

        private void DeleteOldBlocks( MigrationHelper migrationHelper, Dictionary<Model.Block, Model.Block> copiedBlockMappings )
        {
            // Keeping old blocks is not compactible with Chop. So ignore the Should Keep Old Blocks if the Migration Type is set to chop.
            if ( this.MigrationStrategy == "Chop" )
            {
                return;
            }
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