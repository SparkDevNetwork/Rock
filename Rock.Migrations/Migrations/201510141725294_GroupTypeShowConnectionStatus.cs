// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupTypeShowConnectionStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "ShowConnectionStatus", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationTemplateFormField", "IsInternal", c => c.Boolean(nullable: false));

            // DT: Fix benevolence security
            Sql( MigrationSQL._201510141725294_GroupTypeShowConnectionStatus_BenevolenceSecurity );

            // MP: SundayDate Stored Proc updates
            Sql( MigrationSQL._201510141725294_GroupTypeShowConnectionStatus_spCheckin_WeeksAttendedInDuration );
            Sql( MigrationSQL._201510141725294_GroupTypeShowConnectionStatus_spCheckin_BadgeAttendance );

            // DT: Giving Receipt Email
            RockMigrationHelper.UpdateSystemEmail( "Finance", "Giving Receipt", "", "", "", "", "", "Giving Receipt from {{ GlobalAttribute.OrganizationName}}", @"{{ GlobalAttribute.EmailHeader }}
<p>
    Thank you {{ FirstNames }} for your generous contribution. Below is the confirmation number and 
    details for your gift.
</p>

<p><strong>Confirmation Number:</strong> {{ TransactionCode }}</p>

<table style=""width: 100%"">
{% for amount in Amounts %}
    <tr>
        <td style=""min-width: 200px;"">{{ amount.AccountName }}</td>
        <td style=""min-width: 100px;"">{{ 'Global' | Attribute:'CurrencySymbol' }} {{ amount.Amount | Format:'#,##0.00' }}</td>
    </tr>
{% endfor %}
    <tr>
        <td style=""min-width: 200px;""><strong>Total:</strong></td>
        <td style=""min-width: 100px;""><strong>{{ 'Global' | Attribute:'CurrencySymbol' }} {{ TotalAmount | Format:'#,##0.00' }}</strong></td>
    </tr>
</table>


{{ GlobalAttribute.EmailFooter }}
", "7DBF229E-7DEE-A684-4929-6C37312A0039" );

            // Attrib for BlockType: Transaction Entry:Receipt Email
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Receipt Email", "ReceiptEmail", "", "The system email to use to send the receipt.", 27, @"7dbf229e-7dee-a684-4929-6c37312a0039", "BB694CEB-DFE3-4670-92F1-D57FBA510DFF" );
            RockMigrationHelper.AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "BB694CEB-DFE3-4670-92F1-D57FBA510DFF", @"7dbf229e-7dee-a684-4929-6c37312a0039" );

            // MP: File Manager Block
            RockMigrationHelper.AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "File Manager", "", "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1", "fa fa-folder-open" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "File Manager", "Block that can be used to browse and manage files on the web server", "~/Blocks/Cms/FileManager.ascx", "CMS", "BA327D25-BD8A-4B67-B04C-17B499DDA4B6" );
            // Add Block to Page: File Manager, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1", "", "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "File Manager", "Main", "", "", 0, "B4847448-371F-40B5-9EDA-5C376F233E48" );
            // Attrib for BlockType: File Manager:Root Folder
            RockMigrationHelper.AddBlockTypeAttribute( "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Root Folder", "RootFolder", "", "The Root folder to browse", 0, @"~/Content", "025F71E0-9827-47A3-9299-A81B88817379" );
            // Attrib for BlockType: File Manager:Browse Mode
            RockMigrationHelper.AddBlockTypeAttribute( "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Browse Mode", "BrowseMode", "", "Select 'image' to show only image files. Select 'doc' to show all files. Also, in 'image' mode, the ImageUploader handler will process uploaded files instead of FileUploader.", 0, @"doc", "38DC15E2-B059-4E2E-BBE4-8E1607BC9028" );

            // JE: Give edit access to the Merge block for the Data Integrity Team
            Sql( @"
DECLARE @MergeBlockTypeId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '9D41F7DE-537C-4B7C-8BD8-66B972A606F7')
DECLARE @BlockEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
DECLARE @DataIntegrityGroup int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '40517E10-0F2D-4C61-AA8D-BDE36D58C63A')

IF NOT EXISTS (SELECT * FROM [Auth] WHERE [EntityTypeId] = @BlockEntityTypeId AND [EntityId] = @MergeBlockTypeId AND [GroupId] = @DataIntegrityGroup)
BEGIN
	INSERT INTO [Auth]
		([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
	VALUES
		(@BlockEntityTypeId, @MergeBlockTypeId, 0, 'Edit', 'A', 0, @DataIntegrityGroup, '8F0D96A9-74C5-9B85-4328-910809CFABCA')
END" );

            // JE: Security to allow access to Staff/Staff Like to edit Attribute Values
            Sql( @"
DECLARE @EntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'D2BDCCF0-D3F4-4F29-B286-DA5B7BFA41C6')
DECLARE @StaffGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4')

IF NOT EXISTS (SELECT * FROM [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [GroupId] = @StaffGroupId)
BEGIN
	INSERT INTO [Auth]
		([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
	VALUES
		(@EntityTypeId, 0, 0, 'Edit', 'A', 0, @StaffGroupId, '685469DB-D633-CABA-4AFD-B4C15D0466C3')
END

DECLARE @StaffLikeGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745')

IF NOT EXISTS (SELECT * FROM [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [GroupId] = @StaffLikeGroupId)
BEGIN
	INSERT INTO [Auth]
		([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
	VALUES
		(@EntityTypeId, 0, 0, 'Edit', 'A', 0, @StaffLikeGroupId, 'A587DE2B-F7CD-5085-4842-347885E1137E')
END");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP: File Manager Block
            RockMigrationHelper.DeletePage( "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1" ); //  Page: File Manager, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeleteBlockType( "BA327D25-BD8A-4B67-B04C-17B499DDA4B6" ); // File Manager
            // Remove Block: File Manager, from Page: File Manager, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B4847448-371F-40B5-9EDA-5C376F233E48" );
            // Attrib for BlockType: File Manager:Root Folder
            RockMigrationHelper.DeleteAttribute( "025F71E0-9827-47A3-9299-A81B88817379" );
            // Attrib for BlockType: File Manager:Browse Mode
            RockMigrationHelper.DeleteAttribute( "38DC15E2-B059-4E2E-BBE4-8E1607BC9028" );
            
            // DT: Giving Receipt Email
            RockMigrationHelper.DeleteAttribute( "BB694CEB-DFE3-4670-92F1-D57FBA510DFF" );
            
            DropColumn("dbo.RegistrationTemplateFormField", "IsInternal");
            DropColumn("dbo.GroupType", "ShowConnectionStatus");
        }
    }
}
