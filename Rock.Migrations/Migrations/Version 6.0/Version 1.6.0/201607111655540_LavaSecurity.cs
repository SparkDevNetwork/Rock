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
    
    /// <summary>
    ///
    /// </summary>
    public partial class LavaSecurity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add new field type
            RockMigrationHelper.UpdateFieldType( "Lava Commands", "Used to select Lava commands.", "Rock", "Rock.Field.Types.LavaCommandsFieldType", Rock.SystemGuid.FieldType.LAVA_COMMANDS );

            // add security to lava rest enpoint
            Sql( @"IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.LavaController') 
	INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
		VALUES ( 'Lava', 'Rock.Rest.Controllers.LavaController', NEWID() )

		
IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'POSTapi/Lava/RenderTemplate?template={template}') 
	INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
		SELECT [Id], 'POST', 'POSTapi/Lava/RenderTemplate?template={template}', 'api/Lava/RenderTemplate?template={template}', NEWID()
		FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.LavaController'


INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
	VALUES (
		(SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), 
		(SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'POSTapi/Lava/RenderTemplate?template={template}'), 
		0, 'View', 'D', 1, 
		NULL, 
		'0BC9F7C3-C3FF-C39E-4EC3-1A06F5E5090B')" );

            // create Global Attribute for lava security
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.LAVA_COMMANDS, "", "", "Default Enabled Lava Commands", "Allows you to globally enable/disable Lava Commands.", 0, "", SystemGuid.Attribute.GLOBAL_ENABLED_LAVA_COMMANDS );

            // add attribute value
            Sql( @"DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '933CFB7D-C9E1-BDAE-40AD-231002A91626')
INSERT INTO [AttributeValue]
([AttributeId], [EntityId], [Value], [Guid], [IsSystem])
VALUES
(@AttributeId, null, 'RockEntity', '503A02A4-A104-C1A9-4848-E98283307EB2', 1)" );

            // add category to  attribute
            Sql( @"DECLARE @ConfigCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'BB40B563-18D1-4133-94B9-D7F67D95E4E3')
  DECLARE @LavaSecAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '933CFB7D-C9E1-BDAE-40AD-231002A91626')

  INSERT INTO [AttributeCategory] 
	([AttributeId], [CategoryId])
  VALUES
	(@LavaSecAttributeId, @ConfigCategoryId)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
            DELETE FROM [Auth] WHERE [Guid] = '0BC9F7C3-C3FF-C39E-4EC3-1A06F5E5090B'

            DELETE FROM [RestAction] WHERE [ApiId] = 'POSTapi/Lava/RenderTemplate?template={template}'
            DELETE FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.LavaController'

            DECLARE @ConfigCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'BB40B563-18D1-4133-94B9-D7F67D95E4E3')
            DECLARE @LavaSecAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '933CFB7D-C9E1-BDAE-40AD-231002A91626')

            DELETE FROM [AttributeCategory] WHERE [AttributeId] = @LavaSecAttributeId AND [CategoryId] = @ConfigCategoryId

            DELETE FROM [AttributeValue] WHERE [Guid] = '503A02A4-A104-C1A9-4848-E98283307EB2'
" );

            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.GLOBAL_ENABLED_LAVA_COMMANDS );
        }
    }
}
