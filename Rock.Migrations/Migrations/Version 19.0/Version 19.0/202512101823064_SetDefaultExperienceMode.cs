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
    /// <summary>
    ///
    /// </summary>
    public partial class SetDefaultExperienceMode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Administration.ChooseExperienceMode", "3733A532-9F34-418C-8EDB-EF07D6112907", false, false );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Choose Experience Mode",
                "Presents a panel that allows the administrator to choose the default experience after a fresh Rock install.",
                "Rock.Blocks.Administration.ChooseExperienceMode",
                "Administration",
                "47418BF9-DDF0-4C12-92A0-667A3C1209D2" );

            Sql( @"
IF EXISTS (SELECT * FROM [Attribute] WHERE [Key] = 'RockInstanceId' AND [EntityTypeQualifierColumn] = 'SystemSetting' AND [CreatedDateTime] IS NOT NULL)
BEGIN
	-- This is an upgrade. Default to Trailblazer mode.
	IF EXISTS (SELECT * FROM [Attribute] WHERE [Key] = 'core_TrailblazerMode' AND [EntityTypeQualifierColumn] = 'SystemSetting')
	BEGIN
		UPDATE [Attribute]
		SET [DefaultValue] = 'True'
		WHERE [Key] = 'core_TrailblazerMode' AND [EntityTypeQualifierColumn] = 'SystemSetting'
	END
	ELSE
	BEGIN
		DECLARE @FieldTypeId INT = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

		INSERT INTO [Attribute]
		([IsSystem], [FieldTypeId], [Key], [Name], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Order], [DefaultValue], [Guid])
		VALUES
		(0, 1, 'core_TrailblazerMode', 'core _ Trailblazer Mode', 0, 0, 0, 'SystemSetting', '', 0, 'True', NEWID())
	END
END
ELSE
BEGIN
	PRINT 'New Install'
	-- This is a new install. Add the Choose Experience Mode block.
	IF NOT EXISTS (SELECT * FROM [Block] WHERE [Guid] = '00ad1f50-dc25-46e9-9790-850d46be20cb')
	BEGIN
		DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '20F97A93-7949-4C2A-8A5E-C756FE8585CA')
		DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '47418bf9-ddf0-4c12-92a0-667a3c1209d2')
		DECLARE @EntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

		-- Shift everything in the Main zone on that page up 1 Order value.
		UPDATE [Block]
		SET [Order] = [Order] + 1
		WHERE [PageId] = @PageId AND [Zone] = 'Main'

		-- Install the new black at the top.
		INSERT INTO [Block]
		(
			[IsSystem], [PageId], [BlockTypeId], [Zone],
			[Order], [Name], [PreHtml], [PostHtml], [OutputCacheDuration],
			[Guid]
		)
        VALUES
		(
            1, @PageId, @BlockTypeId, 'Main',
            0, 'Choose Experience Mode', '', '', 0,
            '00ad1f50-dc25-46e9-9790-850d46be20cb'
		)
	END
END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "00ad1f50-dc25-46e9-9790-850d46be20cb" );

            RockMigrationHelper.DeleteBlockType( "47418BF9-DDF0-4C12-92A0-667A3C1209D2" );
        }
    }
}
