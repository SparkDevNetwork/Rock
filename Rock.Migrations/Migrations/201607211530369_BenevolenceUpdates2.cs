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
    public partial class BenevolenceUpdates2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
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
            RockMigrationHelper.DeleteSecurityAuth( "3EF0EE1E-A2F5-0C95-48AA-3B1FD2A6E5A1" );
            RockMigrationHelper.DeleteSecurityAuth( "4D486E0B-FD09-61A6-463C-10022C0C68AA" );
            RockMigrationHelper.DeleteSecurityAuth( "7A7A6C2A-5032-07AD-428F-3695F726E6A7" );
        }
    }
}
