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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 999, "21.0" )]
    public class JPH : Migration
    {
        private const string InterimGroupMemberListBlockTypeGuid = "5959A986-A40B-45C6-A757-E66C67AE3BD9";
        private const string InterimGroupMemberListEntityTypeGuid = "8CD71FCE-5F8A-46E0-A45A-504925002260";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_RemoveInterimGroupMemberListBlockType_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_RemoveInterimGroupMemberListBlockType_Down();
        }

        #region Remove Interim GroupMemberList Block Type

        /// <summary>
        /// JPH: Removes the previously-added, never-implemented Obsidian GroupMemberList block type and related
        /// records to pave the way for a proper chop of the legacy block - up.
        /// </summary>
        private void JPH_RemoveInterimGroupMemberListBlockType_Up()
        {
            /*
                9/11/2026 - JPH

                This block [and entity] type was added as a partially-implemented Obsidian block type, but was never
                chopped, swapped or snuck into place on any page. We're covering our bases by removing it from the
                database so we can properly chop legacy block instances with the newest implementation of the
                Group Member List block type. If anyone managed to add this interim block type to a page (unlikely),
                it's still worth simply removing, as it would have been added in an error state, since there were no
                accompanying client files to support it.

                Reason: Explain unconventional block type deletion.
            */
            Sql( $@"
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}');
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{InterimGroupMemberListBlockTypeGuid}');

IF @BlockTypeId IS NOT NULL
BEGIN
    DELETE [Auth]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityId] IN (SELECT [Id] FROM [Block] WHERE [BlockTypeId] = @BlockTypeId);

    DELETE [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS NVARCHAR(200));

    DELETE [Block]
    WHERE [BlockTypeId] = @BlockTypeId;
END" );

            // Delete BlockType 
            //   Name: Group Member List
            //   Category: Obsidian > Group
            //   Path: -
            //   EntityType: Group Member List
            RockMigrationHelper.DeleteBlockType( InterimGroupMemberListBlockTypeGuid );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupMemberList
            RockMigrationHelper.DeleteEntityType( InterimGroupMemberListEntityTypeGuid );
        }

        /// <summary>
        /// JPH: Removes the previously-added, never-implemented Obsidian GroupMemberList block type and related
        /// records to pave the way for a proper chop of the legacy block - down.
        /// </summary>
        private void JPH_RemoveInterimGroupMemberListBlockType_Down()
        {
            // There's no need to restore the interim block type on downgrade, as it was never implemented or used.
            // If re-added to the file system, Rock's startup processes will automatically add it back to the database.
        }

        #endregion Remove Interim GroupMemberList Block Type
    }
}
