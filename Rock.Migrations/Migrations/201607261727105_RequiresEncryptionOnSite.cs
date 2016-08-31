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
    public partial class RequiresEncryptionOnSite : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "RequiresEncryption", c => c.Boolean(nullable: false));
            
            // Update workflow name block attribute value on BinaryFileDetail
            Sql( @"IF NOT EXISTS (SELECT * FROM [Attribute] WHERE [Guid] = '706D453F-89A2-F38F-4B09-F532E87E2A20')
BEGIN
	DECLARE @FieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')
	DECLARE @EntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
	DECLARE @BlockTypeId int = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'B97B2E51-5C9C-459B-999F-C7797DAD8B69')

	INSERT INTO [dbo].[Attribute]
			   ([IsSystem]
			   ,[FieldTypeId]
			   ,[EntityTypeId]
			   ,[EntityTypeQualifierColumn]
			   ,[EntityTypeQualifierValue]
			   ,[Key]
			   ,[Name]
			   ,[Description]
			   ,[Order]
			   ,[IsGridColumn]
			   ,[DefaultValue]
			   ,[IsMultiValue]
			   ,[IsRequired]
			   ,[Guid])
		 VALUES
			   (1
			   ,@FieldTypeId
			   ,@EntityTypeId
			   ,'BlockTypeId'
			   , @BlockTypeId
			   ,'WorkflowButtonText'
			   ,'Workflow Button Text'
			   ,'The button text to show for the rerun workflow button.'
			   ,1
			   ,0
			   ,'Rerun Workflow'
			   ,0
			   ,0
			   ,'706D453F-89A2-F38F-4B09-F532E87E2A20')


	DECLARE @CategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '171E45E4-74EC-4962-9AEA-56D899217AFB')


	INSERT INTO [AttributeCategory]
	([AttributeId], [CategoryId])
	VALUES
	(SCOPE_IDENTITY(), @CategoryId) 
END" );
            RockMigrationHelper.AddBlockAttributeValue( "F52CEDB1-F822-485C-9A1C-BA6D05383FAA", "706D453F-89A2-F38F-4B09-F532E87E2A20", "Reload Merge Fields" );

            // Give Benevolence Requests a consistent Entity Type Guid
            RockMigrationHelper.UpdateEntityType( "Rock.Model.BenevolenceRequest", "CF0CE5C1-9286-4310-9B50-10D040F8EBD2", true, true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Site", "RequiresEncryption");
        }
    }
}
