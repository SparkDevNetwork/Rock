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
    public partial class BenevolenceAdditions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BenevolenceRequestDocument",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BenevolenceRequestId = c.Int(nullable: false),
                        BinaryFileId = c.Int(nullable: false),
                        Order = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BenevolenceRequest", t => t.BenevolenceRequestId, cascadeDelete: true)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BenevolenceRequestId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            AddColumn("dbo.BenevolenceRequest", "ProvidedNextSteps", c => c.String());
            AddColumn("dbo.BenevolenceRequest", "CampusId", c => c.Int());
            CreateIndex("dbo.BenevolenceRequest", "CampusId");
            AddForeignKey("dbo.BenevolenceRequest", "CampusId", "dbo.Campus", "Id");

            RockMigrationHelper.UpdateBinaryFileType( Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE, "Benevolence Request Documents", "Related documents for benevolence requests.", "fa fa-files-o", Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, false, true );

            // add security to the document
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, 0, "View", true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, Model.SpecialRole.None, "3EF0EE1E-A2F5-0C95-48AA-3B1FD2A6E5A1" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, 1, "View", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, Model.SpecialRole.None, "4D486E0B-FD09-61A6-463C-10022C0C68AA" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, 2, "View", false, "", Model.SpecialRole.AllUsers, "7A7A6C2A-5032-07AD-428F-3695F726E6A7" );

            // enable show on grid field on the attributes page
            RockMigrationHelper.AddBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "", "Should the 'Show In Grid' option be displayed when editing attributes?", 2, @"False", "920FE120-AD75-4D5C-BFE0-FA5745B1118B" );
            RockMigrationHelper.AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"True" ); // Enable Show In Grid

            // move the entity attributes page under 'System Settings'
            Sql( @"DECLARE @EntityAttributesPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '23507C90-3F78-40D4-B847-6FE8941FCD32')
DECLARE @SystemsSettingPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'C831428A-6ACD-4D49-9B2D-046D399E3123')

UPDATE [Page]
	SET [ParentPageId] = @SystemsSettingPageId
WHERE 
	[Id] = @EntityAttributesPageId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.BenevolenceRequestDocument", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BenevolenceRequestDocument", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BenevolenceRequestDocument", "BinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.BenevolenceRequestDocument", "BenevolenceRequestId", "dbo.BenevolenceRequest");
            DropForeignKey("dbo.BenevolenceRequest", "CampusId", "dbo.Campus");
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ForeignKey" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ForeignGuid" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ForeignId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "Guid" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "BinaryFileId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "BenevolenceRequestId" });
            DropIndex("dbo.BenevolenceRequest", new[] { "CampusId" });
            DropColumn("dbo.BenevolenceRequest", "CampusId");
            DropColumn("dbo.BenevolenceRequest", "ProvidedNextSteps");
            DropTable("dbo.BenevolenceRequestDocument");

            RockMigrationHelper.DeleteSecurityAuth( "3EF0EE1E-A2F5-0C95-48AA-3B1FD2A6E5A1" );
            RockMigrationHelper.DeleteSecurityAuth( "4D486E0B-FD09-61A6-463C-10022C0C68AA" );
            RockMigrationHelper.DeleteSecurityAuth( "7A7A6C2A-5032-07AD-428F-3695F726E6A7" );
        }
    }
}
