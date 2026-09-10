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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    using Rock.Security;

    /*
        8/31/2026 - CLAUDE

        This migration is the schema-and-CmsSkill slice of the Code Composer
        feature. The ForgeContent table and the Cms skill ship now; the Forge
        Content block type, the Forge Content Builder and Lava Application
        Builder skills, and the Code Composer agents ship in a later migration
        alongside the code they depend on.

        Reason: Only the schema change and the Cms skill are landing in this release.
    */

    /// <summary>
    ///
    /// </summary>
    public partial class AddForgeContentAndCmsSkill : Rock.Migrations.RockMigration
    {
        #region Constants

        /// <summary>
        /// The AISkill Guid of the Cms skill.
        /// </summary>
        private const string CmsSkillGuid = "613D7110-6453-4BAB-892B-064222F8397C";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CmsSkill</c>.
        /// </summary>
        private const string CmsSkillEntityTypeGuid = "7A63570D-6FC3-4573-BDF2-89CFF605D5AB";

        #endregion Constants

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ForgeContent",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    BlockId = c.Int(),
                    Source = c.String(),
                    CompiledContent = c.String(),
                    CompiledVueVersion = c.String( maxLength: 50 ),
                    CompiledDateTime = c.DateTime(),
                    IsActive = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Block", t => t.BlockId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.BlockId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            RegisterEntityTypes_Up();
            AddCmsSkill_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /*
                8/17/2026 - CLAUDE

                The EntityType rows are deliberately left in place. They are
                recreated by startup registration, carry no configuration of
                their own, and deleting them would orphan anything else that
                came to reference them. The skill and tool rows are removed
                because startup registration recreates them on the next run of
                a build that still contains the skill classes.

                Reason: A downgrade must not orphan rows that reference the EntityTypes.
            */
            RemoveCmsSkillAndTools_Down();

            DropForeignKey( "dbo.ForgeContent", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ForgeContent", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ForgeContent", "BlockId", "dbo.Block" );
            DropIndex( "dbo.ForgeContent", new[] { "Guid" } );
            DropIndex( "dbo.ForgeContent", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ForgeContent", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.ForgeContent", new[] { "BlockId" } );
            DropTable( "dbo.ForgeContent" );
        }

        #region Entity Types

        /// <summary>
        /// Registers the EntityTypes this migration references. Startup EntityType
        /// registration runs after migrations, so anything the seeding below joins
        /// against must be registered explicitly here first: a missing EntityType
        /// yields a null CodeEntityTypeId and a skill that exposes nothing.
        /// </summary>
        private void RegisterEntityTypes_Up()
        {
            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.ForgeContent",
                "Forge Content",
                "Rock.Model.ForgeContent, Rock, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                SystemGuid.EntityType.FORGE_CONTENT );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CmsSkill",
                CmsSkillEntityTypeGuid,
                false,
                false );
        }

        #endregion Entity Types

        #region Skills

        /*
            8/17/2026 - CLAUDE

            The seeded skill name below is exactly the class name split-cased, and
            every seeded description is exactly the class or method [Description]
            text, character for character. Startup re-registration derives those
            values from the classes and overwrites the rows on every application
            start, so any drift here lasts only until the first restart and then
            silently disappears.

            Reason: Seeded values must match what startup registration derives.
        */

        /// <summary>
        /// Registers the Cms skill, its tools, and administrator-only security
        /// on the mutating tools. The skill itself carries no security
        /// rules: its read tools (LookupSites and friends) are usable by any
        /// audience, so the lockdown is per tool rather than per skill.
        /// </summary>
        private void AddCmsSkill_Up()
        {
            AddOrUpdateCodeAISkill(
                "Cms Skill",
                "Explore and manage sites, pages, and blocks in Rock's CMS.",
                CmsSkillEntityTypeGuid,
                CmsSkillGuid );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Lookup Sites",
                "Retrieves all configured websites in Rock.",
                "6234BB68-99B8-4B7C-884D-0D760B1F081C" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Get Site",
                "Gets the details of a single site, including its theme, default page, and login page.",
                "16C84C00-62DC-4AE9-9A85-F7CDE7D20FC8" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Pages",
                "Lists one level of the page tree: the root pages of every site, or the immediate children of a parent page. Call repeatedly with a returned page's IdKey to walk deeper.",
                "1F7C1F00-F481-468A-860F-314D1B43A477" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Pages For Site",
                "Lists every page belonging to one site as a flat list. Each page includes its parent page so the hierarchy can be reconstructed.",
                "8968B4EF-3A1D-472A-9BC6-17A80B8F824F" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Search Pages",
                "Searches CMS pages by a partial name match so a page can be resolved and confirmed with the user before adding a child page under it or adding a block to it.",
                "C668CAE0-CFA7-4AFF-87FF-5025860170BA" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Get Page",
                "Gets the details of a single page, including its routes, layout, and the blocks already placed on it.",
                "E2CFF69F-C4B2-47F5-B322-4041D841F37C" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Add Or Update Page",
                "Adds a new child page under a parent page or updates an existing page. New pages inherit the parent's layout unless a layout is specified. Pass a kebab-case route so the page gets a friendly URL.",
                "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Layouts",
                "Lists the layouts pages can render with, optionally filtered to one site. Returns the layoutIdKey that AddOrUpdatePage accepts.",
                "82C06D71-800E-4064-B72D-98F1B2A684D7" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Block Types",
                "Lists the block types available to place on a page, filtered by a partial name or category. Returns the blockTypeIdKey that AddOrUpdateBlock needs.",
                "F9A5AC4D-E40C-4FAF-895D-8C0E10A37EEC" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Blocks",
                "Lists the blocks placed on a page, a layout, or a site. At least one filter is required.",
                "98F33433-0712-4248-9C71-EAE4D9F9CA38" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Add Or Update Block",
                "Adds a block to a page, layout, or site, or updates an existing block. Returns the block's IdKey, which is the block id the Forge Content Builder skill's AddOrUpdateForgeContent tool needs.",
                "05C9C108-4516-46B7-85FB-5C8FE6212CCF" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Delete Page",
                "Deletes a page along with its blocks, routes, and optionally its interaction history. Pages with child pages are refused; delete or move the children first.",
                "BB6C42F3-C448-49D5-BB85-4072960178FC" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Delete Block",
                "Deletes a block from its page, layout, or site, along with any Forge Content stored against it.",
                "B30F66EA-0D9E-4854-BB82-A96BE7719D00" );

            // Only the mutating tools are locked to administrators. The read
            // tools stay visible and rely on per-person VIEW filtering inside
            // each tool.
            AddAdministratorOnlySecurityForAISkillTool(
                "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633", // Add Or Update Page
                "4A95B31D-A629-4F76-B68A-6D74B7C578EE",
                "C2D66CDA-336D-4D1E-BF29-B2960A25AAB6" );

            AddAdministratorOnlySecurityForAISkillTool(
                "05C9C108-4516-46B7-85FB-5C8FE6212CCF", // Add Or Update Block
                "3FC10ED8-FF27-4B28-9C11-19B4F1993A80",
                "70241AD4-ED95-4757-A19D-ECD27FDB430A" );

            AddAdministratorOnlySecurityForAISkillTool(
                "BB6C42F3-C448-49D5-BB85-4072960178FC", // Delete Page
                "7483110B-1155-45FC-A7F7-B77959DB3982",
                "A4118437-30B1-48C7-88D9-89E34E0C4B46" );

            AddAdministratorOnlySecurityForAISkillTool(
                "B30F66EA-0D9E-4854-BB82-A96BE7719D00", // Delete Block
                "AFBD660A-35C7-48BB-8A82-15099CF595AE",
                "557884F7-F369-4FE5-9F18-6C41FBE13900" );
        }

        /// <summary>
        /// Removes the seeded security, tools, and skill. Runs before the table
        /// is removed.
        /// </summary>
        private void RemoveCmsSkillAndTools_Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "4A95B31D-A629-4F76-B68A-6D74B7C578EE" );
            RockMigrationHelper.DeleteSecurityAuth( "C2D66CDA-336D-4D1E-BF29-B2960A25AAB6" );
            RockMigrationHelper.DeleteSecurityAuth( "3FC10ED8-FF27-4B28-9C11-19B4F1993A80" );
            RockMigrationHelper.DeleteSecurityAuth( "70241AD4-ED95-4757-A19D-ECD27FDB430A" );
            RockMigrationHelper.DeleteSecurityAuth( "7483110B-1155-45FC-A7F7-B77959DB3982" );
            RockMigrationHelper.DeleteSecurityAuth( "A4118437-30B1-48C7-88D9-89E34E0C4B46" );
            RockMigrationHelper.DeleteSecurityAuth( "AFBD660A-35C7-48BB-8A82-15099CF595AE" );
            RockMigrationHelper.DeleteSecurityAuth( "557884F7-F369-4FE5-9F18-6C41FBE13900" );

            Sql( $@"
DELETE FROM [AISkillTool]
WHERE [AISkillId] IN (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{CmsSkillGuid}')

DELETE FROM [AISkill]
WHERE [Guid] = '{CmsSkillGuid}'" );
        }

        #endregion Skills

        #region Helper Methods

        /// <summary>
        /// Adds or updates a code-based AI skill. Copied from hotfix 309, which is
        /// the established pattern for seeding these records.
        /// </summary>
        /// <param name="name">The name of the AI skill.</param>
        /// <param name="description">The user-friendly description of the AI skill.</param>
        /// <param name="codeEntityTypeGuid">The GUID of the code entity type.</param>
        /// <param name="skillGuid">The GUID of the AI skill.</param>
        private void AddOrUpdateCodeAISkill( string name, string description, string codeEntityTypeGuid, string skillGuid )
        {
            if ( codeEntityTypeGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( codeEntityTypeGuid ), "The code entity type guid must be a valid guid." );
            }

            if ( skillGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( skillGuid ), "The skill guid must be a valid guid." );
            }

            Sql( $@"
DECLARE @CodeEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{codeEntityTypeGuid}')

IF NOT EXISTS (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{skillGuid}')
BEGIN
    INSERT INTO [AISkill] (
        [Name]
        , [Description]
        , [CodeEntityTypeId]
        , [Guid]
    )
    VALUES (
        '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , @CodeEntityTypeId
        , '{skillGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkill]
    SET [Name] = '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , [Description] = '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , [CodeEntityTypeId] = @CodeEntityTypeId
    WHERE [Guid] = '{skillGuid}'
END
" );
        }

        /// <summary>
        /// Adds or updates a code-based AI skill tool. Copied from hotfix 309,
        /// which is the established pattern for seeding these records.
        /// </summary>
        /// <param name="skillGuid">The GUID of the AI skill this tool belongs to.</param>
        /// <param name="name">The name of the AI skill tool.</param>
        /// <param name="description">The user-friendly description of the AI skill tool.</param>
        /// <param name="toolGuid">The GUID of the AI skill tool.</param>
        private void AddOrUpdateCodeAISkillTool( string skillGuid, string name, string description, string toolGuid )
        {
            if ( skillGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( skillGuid ), "The skill guid must be a valid guid." );
            }

            if ( toolGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( toolGuid ), "The tool guid must be a valid guid." );
            }

            Sql( $@"
DECLARE @SkillId INT = (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{skillGuid}')

IF NOT EXISTS (SELECT [Id] FROM [AISkillTool] WHERE [Guid] = '{toolGuid}')
BEGIN
    INSERT INTO [AISkillTool] (
        [Name]
        , [Description]
        , [ToolType]
        , [AISkillId]
        , [Guid]
    )
    VALUES (
        '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , @SkillId
        , '{toolGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkillTool]
    SET [Name] = '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , [Description] = '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , [ToolType] = {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , [AISkillId] = @SkillId
    WHERE [Guid] = '{toolGuid}'
END
" );
        }

        /// <summary>
        /// Grants VIEW on a single tool to Rock administrators and denies it to
        /// everyone else. Used for mutating tools on a skill whose read tools
        /// stay open, so the lockdown lands on the tool rather than the skill.
        /// A tool a person is not authorized to VIEW is simply never offered to
        /// the model.
        /// </summary>
        /// <param name="toolGuid">The Guid of the AISkillTool to secure.</param>
        /// <param name="allowAuthGuid">The Guid of the administrator allow rule.</param>
        /// <param name="denyAuthGuid">The Guid of the all-users deny rule.</param>
        private void AddAdministratorOnlySecurityForAISkillTool( string toolGuid, string allowAuthGuid, string denyAuthGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAISkillTool(
                toolGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                allowAuthGuid );

            RockMigrationHelper.AddSecurityAuthForAISkillTool(
                toolGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                denyAuthGuid );
        }

        #endregion Helper Methods
    }
}
