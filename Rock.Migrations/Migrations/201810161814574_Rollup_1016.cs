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
    public partial class Rollup_1016 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            RemoveUpdateWelcomeMessageChecklistItem();
            AddLabelToInstallFont();
            UpdateFirstVisitsSproc();
            RemoveAreaStringFromCheckinConfig();
            UpdateSharedDocsPage();
            AddCommunicationSetting();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodGenMigrationsDown();
        }

        /// <summary>
        /// Up migrations created by CodeGen_PagesBlocksAttributesMigration.sql
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType( "System Settings", "Block used to set values specific to system settings.", "~/Blocks/Administration/SystemSettings.ascx", "Administration", "1151DE1C-5847-4EE8-90ED-D4E268225F9D" );
            // Attrib for BlockType: Add Group:Generate Alternate Identifier
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Generate Alternate Identifier", "GenerateAlternateIdentifier", "", @"If enabled, a custom alternate identifier will be generated for each person.", 30, @"True", "E792B334-3D9D-4FEB-85E3-0197A9754CD0" );
            // Attrib for BlockType: System Settings:Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "1151DE1C-5847-4EE8-90ED-D4E268225F9D", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "", @"The Attribute Category to display attributes from", 0, @"", "F64152C8-90A5-4C22-A6D1-C408AE7A82BF" );
        }

        /// <summary>
        /// Down migrations created by CodeGen_PagesBlocksAttributesMigration.sql
        /// </summary>
        private void CodGenMigrationsDown()
        {
            // Attrib for BlockType: Add Group:Generate Alternate Identifier
            RockMigrationHelper.DeleteAttribute( "E792B334-3D9D-4FEB-85E3-0197A9754CD0" );
            // Attrib for BlockType: System Settings:Category
            RockMigrationHelper.DeleteAttribute( "F64152C8-90A5-4C22-A6D1-C408AE7A82BF" );
            RockMigrationHelper.DeleteBlockType( "1151DE1C-5847-4EE8-90ED-D4E268225F9D" ); // System Settings
        }

        /// <summary>
        /// [ED] Remove Update Checklist Item "Update Welcome Message" 
        /// </summary>
        private void RemoveUpdateWelcomeMessageChecklistItem()
        {
            RockMigrationHelper.DeleteDefinedValue( "0B922507-8CCB-48C3-A41E-0E790D6B5702" );
        }

        /// <summary>
        /// [ED] Add Check-in Label to install Icon Font
        /// </summary>
        private void AddLabelToInstallFont()
        {
            Sql( MigrationSQL._201810161814574_Rollup_1016_AddCheck_inLabelToInstallIconFont );
        }

        /// <summary>
        /// [ED] v8.x Issue in Family Analytics Block
        /// </summary>
        private void UpdateFirstVisitsSproc()
        {
            Sql( MigrationSQL._201810161814574_Rollup_1016_spCrm_FamilyAnalyticsFirstVisitsAttributeUpdate );
        }

        /// <summary>
        /// [ED] Remove the word Area from the check-in configurations
        /// </summary>
        private void RemoveAreaStringFromCheckinConfig()
        {
            Sql( @"UPDATE [GroupType]
                SET [Name] = 'Volunteer Check-in'
                WHERE [Guid] = '92435F1D-E525-4FD2-BEC7-4956DC056A2B'
                AND [Name] = 'Volunteer Check-in Area'" );

            Sql( @"UPDATE [GroupType]
                SET [Name] = 'Weekly Service Check-in'
                WHERE [Guid] = 'FEDD389A-616F-4A53-906C-63D8255631C5'
                AND [Name] = 'Weekly Service Check-in Area'" );
        }

        /// <summary>
        /// [GP] Fixed spelling mistake on the 'Shared Documents' page
        /// </summary>
        private void UpdateSharedDocsPage()
        {
            Sql( @"UPDATE [dbo].[HtmlContent]
                SET [Content] = N'<p>  Page for posting common documents that are used often.&nbsp; Suggestions:</p> <ul>  <li>   Staff Phone Lists</li>  <li>   Referal Lists</li>  <li>   Policies and Procedures</li>  <li>   Holiday Schedules</li>  <li>   etc.</li> </ul> '
                WHERE [Id] = 54
                AND [Content] = N'<p>  Page for posting common documents that are used often.&nbsp; Suggestions:</p> <ul>  <li>   Staff Phone Lists</li>  <li>   Refferal Lists</li>  <li>   Policies and Procedures</li>  <li>   Holiday Schedules</li>  <li>   etc.</li> </ul> '" );
        }

        /// <summary>
        /// [SK] Add Communication Setting (Contain Helper method UpdateSystemSetting too)
        /// </summary>
        private void AddCommunicationSetting()
        {
            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Settings", "", "5B67480F-418D-4916-9C39-A26D2F8FA95C", iconCssClass: "fa fa-cog" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "System Setting", "Block used to set values specific to system settings.", "~/Blocks/Administration/SystemSettings.ascx", "Administration", "41A585E0-4522-40FA-8CC6-A411C70340F7" );
            RockMigrationHelper.AddBlock( true, "5B67480F-418D-4916-9C39-A26D2F8FA95C", "", "41A585E0-4522-40FA-8CC6-A411C70340F7", "Communication Setting", "Main", @"", @"", 0, "8083E072-A5F6-4BA0-8110-7D3A9E94A05F" );
            RockMigrationHelper.UpdateCategory( "5997C8D3-8840-4591-99A5-552919F90CBD", "Communication Settings", "", "", "1059CCF2-933F-488E-8DBF-4FEC64A12409" );
            Sql( "UPDATE [Category] SET [EntityTypeQualifierColumn]='EntityTypeId' where [Guid]='1059CCF2-933F-488E-8DBF-4FEC64A12409'" );
            UpdateSystemSetting( "core_DoNotDisturbStart", "21:00:00", Rock.SystemGuid.FieldType.TIME, "4A558666-32C7-4490-B860-0F41358E14CA" );
            UpdateSystemSetting( "core_DoNotDisturbEnd", "09:00:00", Rock.SystemGuid.FieldType.TIME, "661802FC-E636-4CE2-B75A-4AC05595A347" );
            UpdateSystemSetting( "core_DoNotDisturbActive", "False", Rock.SystemGuid.FieldType.BOOLEAN, "1BE30413-5C90-4B78-B324-BD31AA83C002" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "41A585E0-4522-40FA-8CC6-A411C70340F7", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "", @"The Attribute Category to display attributes from", 0, @"", "25E02584-4B8F-4A8F-8558-8D2EDA2C5393" );
            RockMigrationHelper.AddBlockAttributeValue( "8083E072-A5F6-4BA0-8110-7D3A9E94A05F", "25E02584-4B8F-4A8F-8558-8D2EDA2C5393", @"1059ccf2-933f-488e-8dbf-4fec64a12409" );
        }

        /// <summary>
        /// Updates the system setting for AddCommunicationSetting.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        /// <param name="fieldGuid">The field unique identifier.</param>
        /// <param name="guid">The unique identifier.</param>
        private void UpdateSystemSetting( string attributeKey, string value, string fieldGuid, string guid )
        {
            var attributeName = attributeKey.SplitCase();
            string updateSql = $@"
                DECLARE 
                    @FieldTypeId int
                    ,@AttributeId int
                    ,@CategoryId int

                SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '1059CCF2-933F-488E-8DBF-4FEC64A12409')
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{fieldGuid}')
                SET @AttributeId = (SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] IS NULL
                    AND [EntityTypeQualifierColumn] = '{Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER}'
                    AND [Key] = '{attributeKey}')

                IF @AttributeId IS NOT NULL
                BEGIN
                    UPDATE [Attribute]
                    SET [DefaultValue] = '{value.Replace( "'", "''" )}'
                    WHERE [Id] = @AttributeId
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Order],[IsGridColumn],[IsMultiValue],[IsRequired],                        
                        [Key],[Name],[DefaultValue], [Guid])
                    VALUES(
                        1,@FieldTypeId,NULL,'{Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER}','',
                        0,0,0,0,
                        '{attributeKey}','{attributeName}', '{value.Replace( "'", "''" )}', '{guid}')

                    SET @AttributeId = SCOPE_IDENTITY()
                    INSERT INTO [AttributeCategory] (
                        [AttributeId],[CategoryId])
                    VALUES(
                        @AttributeId,@CategoryId)
                END";

            Sql( updateSql );
        }
    }
}
