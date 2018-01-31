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
    public partial class TagEntityUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.TaggedItem", "EntityTypeId", c => c.Int( nullable: false ) );

            Sql( @"
    UPDATE I SET [EntityTypeId] = T.[EntityTypeId]
    FROM [TaggedItem] I
    INNER JOIN [Tag] T ON T.[Id] = I.[TagId]
" );
            CreateIndex( "dbo.TaggedItem", "EntityTypeId" );
            AddForeignKey( "dbo.TaggedItem", "EntityTypeId", "dbo.EntityType", "Id" );

            DropIndex( "dbo.Tag", new[] { "EntityTypeId" } );
            AlterColumn( "dbo.Tag", "EntityTypeId", c => c.Int() );
            CreateIndex( "dbo.Tag", "EntityTypeId" );

            AddColumn( "dbo.EntityType", "LinkUrlLavaTemplate", c => c.String() );

            AddColumn( "dbo.ContentChannel", "IsTaggingEnabled", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.ContentChannel", "ItemTagCategoryId", c => c.Int() );
            CreateIndex( "dbo.ContentChannel", "ItemTagCategoryId" );
            AddForeignKey( "dbo.ContentChannel", "ItemTagCategoryId", "dbo.Category", "Id" );
            DropColumn( "dbo.ContentChannel", "ItemTagCategories" );

            Sql( @"
    UPDATE [EntityType] SET [LinkUrlLavaTemplate] = '~/Person/{{ Entity.Id }}' WHERE [Name] = 'Rock.Model.Person'
" );

            Sql( @"
    DECLARE @TagEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Tag' )

    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
    SELECT @TagEntityTypeId, 0, ( ROW_NUMBER() OVER( ORDER BY [Name] DESC) ) - 1, 'Tag', 'A', 0, [Id], NEWID()
    FROM [Group] 
    WHERE [Guid] IN ( '628C51A8-4613-43ED-A18D-4A6FB999273E', '2C112948-FF4C-46E7-981A-0257681EADF4', '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )

    DECLARE @Order int = ( SELECT MAX([Order]) FROM [Auth] WHERE [EntityTypeId] = @TagEntityTypeId AND [EntityId] = 0 AND [Action] = 'Tag' )
    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid] )
    VALUES ( @TagEntityTypeId, 0, @Order, 'Tag', 'D', 1, NEWID() )
" );

            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Tag Categories", "", "86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F", "fa fa-tag" ); // Site:Rock RMS
            RockMigrationHelper.AddBlock( "86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", @"", @"", 0, "EEFCAF48-E520-4383-A550-34CB1339951C" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "C405A507-7889-4287-8342-105B89710044", @"d34258d0-d366-4efb-aa76-84b059fb5434" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "8AFA681F-27F8-4BDC-90C1-F5FF4A112159", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "57530519-239F-473E-A44A-EC1E441C998E", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "69BB4619-AA98-4B36-962B-9062C9A55CB8", @"" );

            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F'
" );

            // MP: Fix Communication List Categories not working due to missing block settings
            // Attrib for BlockType: Categories:Enable Hierarchy
            RockMigrationHelper.UpdateBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Hierarchy", "EnableHierarchy", "", "When set allows you to drill down through the category hierarchy.", 3, @"True", "736C3F4B-34CC-4B4B-9811-7171C82DDC41" );
            // Attrib for BlockType: Categories:Entity Qualifier Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "", "Column to evaluate to determine entities that this category applies to.", 1, @"", "65C4A655-6E1D-4504-838B-28B91FCC6DF8" );
            // Attrib for BlockType: Categories:Entity Qualifier Value
            RockMigrationHelper.UpdateBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "", "The value of the column that this category applies to.", 2, @"", "19A79376-3F07-45E4-95CB-5AD5D3C4DDCF" );
            // Attrib Value for Block:Categories, Attribute:Enable Hierarchy Page: Communication List Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "736C3F4B-34CC-4B4B-9811-7171C82DDC41", @"True" );
            // Attrib Value for Block:Categories, Attribute:Entity Qualifier Column Page: Communication List Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "65C4A655-6E1D-4504-838B-28B91FCC6DF8", @"GroupTypeId" );
            // Attrib Value for Block:Categories, Attribute:Entity Qualifier Value Page: Communication List Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "19A79376-3F07-45E4-95CB-5AD5D3C4DDCF", @"" );
            // Update the EntityQualifierValue to be whatever the GroupTypeId is based on the GroupType.Guid
            Sql( @"UPDATE AttributeValue
SET [Value] = (
 SELECT TOP 1 Id
 FROM GroupType
 WHERE [Guid] = 'D1D95777-FFA3-CBB3-4A6D-658706DAED33'
 )
WHERE AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE [Guid] = '19A79376-3F07-45E4-95CB-5AD5D3C4DDCF'
 )
 AND EntityId IN (
 SELECT Id
 FROM [Block]
 WHERE [Guid] = '25F82ADE-BD0A-404C-A659-30874AFC50A1'
 )" );

            // Hide redundant breadcrumbname for this page
            Sql( @"
 UPDATE [Page] SET [BreadCrumbDisplayName] = 0 
 WHERE [GUID] = '307570FD-9472-48D5-A67F-80B2056C5308'
" );

            // MP: BlockTypeAttributes Catchup
            // Attrib for BlockType: Account Entry:Phone Types Required
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types Required", "PhoneTypesRequired", "", "The phone numbers that are required.", 19, @"", "CAA75DCD-2F31-4367-BC46-E902CB1167BA" );
            // Attrib for BlockType: Account Entry:Phone Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types", "PhoneTypes", "", "The phone numbers to display for editing.", 18, @"", "A2206A18-CD71-4CAF-81E8-92872D96A52F" );
            // Attrib for BlockType: Account Entry:Minimum Age
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Age", "MinimumAge", "", "The minimum age allowed to create an account. Warning: The Children's Online Privacy Protection Act disallows children under the age of 13 from giving out personal information without their parents' permission.", 17, @"13", "56D93B62-4C73-4056-A500-86DE58A644EB" );
            // Attrib for BlockType: Account Entry:Show Address
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Address", "ShowAddress", "", "Allows hiding the address field.", 13, @"False", "17183F5D-3C01-4645-8861-0FBFA8384B1C" );
            // Attrib for BlockType: Account Entry:Location Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Location Type", "LocationType", "", "The type of location that address should use.", 14, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "6386D657-234A-47A8-B836-24EE92B5C90C" );
            // Attrib for BlockType: Account Entry:Address Required
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "", "Whether the address is required.", 15, @"False", "66AC7C39-5030-4C5F-A362-DF90958B1AD8" );
            // Attrib for BlockType: Account Entry:Show Phone Numbers
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Phone Numbers", "ShowPhoneNumbers", "", "Allows hiding the phone numbers.", 16, @"False", "0296B61F-21EA-4128-B06B-FAB9171D60FE" );
            // Attrib for BlockType: Transaction Entry:Anonymous Giving Tooltip
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Anonymous Giving Tooltip", "AnonymousGivingTooltip", "", "The tooltip for the 'Give Anonymously' checkbox.", 33, @"", "07E3A9E2-473D-4F7D-B1B9-BBC5BC4D9346" );
            // Attrib for BlockType: Prayer Request Entry:Default To Public
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "", "If enabled, all prayers will be set to public by default", 9, @"False", "0C7BCEEF-7E4D-439E-BBE8-FEFC85419395" );
            // Attrib for BlockType: Prayer Request Entry:Require Last Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "", "Require that a last name be entered", 11, @"True", "39033000-2EDF-4056-8E55-DE58B66A62F5" );
            // Attrib for BlockType: Prayer Request Entry:Show Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "", "Show a campus picker", 12, @"True", "6C5F28E7-7EEF-4994-B4F5-7D836C787924" );
            // Attrib for BlockType: Prayer Request Entry:Require Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "", "Require that a campus be selected", 13, @"False", "521E6E6F-369B-464A-9ED9-A91F31176312" );
            // Attrib for BlockType: Prayer Request Entry:Refresh Page On Save
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Refresh Page On Save", "RefreshPageOnSave", "", "If enabled, on successful save control will reload the current page. NOTE: This is ignored if 'Navigate to Parent On Save' is enabled.", 15, @"False", "B5808317-84E6-4444-860C-0E2DD35A019B" );
            // Attrib for BlockType: Connection Opportunity Search:Show Search
            RockMigrationHelper.UpdateBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Search", "ShowSearch", "", "Determines if the search fields should be displayed. Sometimes listing all the options is enough.", 9, @"True", "04CA2C37-4B3C-4E8E-81E7-AE9AF222062A" );
            // Attrib for BlockType: Group Finder:Page Sizes
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Sizes", "PageSizes", "", "To show a dropdown of page sizes, enter a comma delimited list of page sizes. For example: 10,20 will present a drop down with 10,20,All as options with the default as 10", 0, @"", "61E505A6-5706-45FB-B629-B3D3BDB8F16E" );
            // Attrib for BlockType: Group Finder:Display Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Campus Filter", "DisplayCampusFilter", "", "", 0, @"False", "294C255A-A91D-4932-9670-C930E849D873" );
            // Attrib for BlockType: Group Finder:Sort By Distance
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Sort By Distance", "SortByDistance", "", "", 0, @"False", "13D9DC59-BE7C-4B39-85C5-D876F8F4CFB4" );
            // Attrib for BlockType: Group Finder:Enable Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "", "", 0, @"False", "7974D4E6-7BEC-4897-AC91-FF016AE16199" );
            // Attrib for BlockType: Group Finder:Show Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "", "", 0, @"False", "82FBB981-BCE8-48E1-AE10-D0F281B3108F" );
            // Attrib for BlockType: Disc:Always Allow Retakes
            RockMigrationHelper.UpdateBlockTypeAttribute( "A161D12D-FEA7-422F-B00E-A689629680E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Allow Retakes", "AlwaysAllowRetakes", "", "Determines if the retake button should be shown.", 5, @"False", "C8FDA050-2C30-4295-880B-2A76047758F5" );
            // Attrib for BlockType: Group Registration:Allowed Group Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Allowed Group Types", "AllowedGroupTypes", "", "This setting restricts which types of groups a person can be added to, however selecting a specific group via the Group setting will override this restriction.", 0, @"50FCFB30-F51A-49DF-86F4-2B176EA1820B", "33852765-0521-4432-A1A2-669A48A39FD2" );
            // Attrib for BlockType: Group Registration:Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "", "Optional group to add person to. If omitted, the group's Guid should be passed via the Query string (GroupGuid=).", 0, @"", "CC9E422E-F9D4-46BE-B261-5A2730E2079E" );
            // Attrib for BlockType: Transaction Entry - Kiosk:Payment Comment
            RockMigrationHelper.UpdateBlockTypeAttribute( "D10900A8-C2C1-4414-A443-3781A5CF371C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Comment", "PaymentComment", "", "The comment to include with the payment transaction when sending to Gateway", 12, @"Kiosk", "FC385AA0-1564-489E-9722-763482A8F7CC" );
            // Attrib for BlockType: Calendar Lava:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 3, @"", "EC5705EE-3386-470C-BD80-5E062E2AC7E9" );
            // Attrib for BlockType: Registration Entry:Force Email Update
            RockMigrationHelper.UpdateBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Force Email Update", "ForceEmailUpdate", "", "Force the email to be updated on the person's record.", 9, @"False", "F21DB123-6EAF-48A7-BB8E-6AC5BE471A4A" );
            // Attrib for BlockType: Registration Entry:Allow InLine Digital Signature Documents
            RockMigrationHelper.UpdateBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow InLine Digital Signature Documents", "SignInline", "", "Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline", 6, @"True", "597B0D0D-7E4D-441A-ABED-590AA5087C72" );
            // Attrib for BlockType: Registration Entry:Family Term
            RockMigrationHelper.UpdateBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Term", "FamilyTerm", "", "The term to use for specifying which household or family a person is a member of.", 8, @"immediate family", "23816526-6A3C-432D-842A-EDB39471DFD5" );
            // Attrib for BlockType: Fundraising Opportunity View:Participant Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Participant Lava Template", "ParticipantLavaTemplate", "", "Lava template for how the partipant actions and progress bar should be displayed", 4, @"{% include '~~/Assets/Lava/FundraisingOpportunityParticipant.lava' %}", "C8C55E78-6CE4-4DA8-BF2E-F5CFC7534F2F" );
            // Attrib for BlockType: Fundraising Opportunity View:Image CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image CSS Class", "ImageCssClass", "", "CSS class to apply to the image.", 11, @"img-thumbnail", "FA8A723A-B597-4038-8C6B-2138F3402C9E" );
            // Attrib for BlockType: Fundraising Donation Entry:Allow Automatic Selection
            RockMigrationHelper.UpdateBlockTypeAttribute( "A24D68F2-C58B-4322-AED8-6556DBED1B76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Automatic Selection", "AllowAutomaticSelection", "", "If enabled and there is only one participant and registrations are not enabled then that participant will automatically be selected and this page will get bypassed.", 3, @"False", "5EA12375-A63D-4A47-A1E5-A80A2162091E" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Image CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image CSS Class", "ImageCssClass", "", "CSS class to apply to the image.", 9, @"img-thumbnail", "0E664056-A3B2-4021-B5C0-1E79822346B3" );
            // Attrib for BlockType: Fundraising Opportunity Participant:PersonAttributes
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "PersonAttributes", "PersonAttributes", "", "The Person Attributes that the participant can edit", 7, @"", "5C093172-515B-4E94-BA2A-CE0D4B361350" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Progress Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Progress Lava Template", "ProgressLavaTemplate", "", "Lava template for how the progress bar should be displayed ", 2, @"{% include '~~/Assets/Lava/FundraisingParticipantProgress.lava' %}", "C412D0FE-B533-410F-A964-BF54D44BD33A" );

            // Attrib for BlockType: Tags By Letter:User-Selectable Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User-Selectable Entity Type", "ShowEntityType", "", "Should user be able to select the entity type to show tags for?", 0, @"True", "CA3A8DD3-C700-4878-A110-BCF780A739F5" );
            // Attrib for BlockType: Tag List:Show Qualifier Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Qualifier Columns", "ShowQualifierColumns", "", "Should the 'Qualifier Column' and 'Qualifier Value' fields be displayed in the grid?", 0, @"False", "92042339-E73A-4B09-81E9-BDA57382E28F" );
            // Attrib for BlockType: Tags By Letter:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", "The entity type to display tags for. If entity type is user-selectable, this will be the default entity type", 0, @"72657ED8-D16E-492E-AC12-144C5E7567E7", "1F216A62-37BB-45DB-941F-1A79D54CC036" );
            // Attrib for BlockType: Person Bio:Tag Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Tag Category", "TagCategory", "", "Optional category to limit the tags to. If specified all new personal tags will be added with this category.", 13, @"", "5E60D632-9F8C-4496-8A67-DAF2CD0DBA41" );
            // Attrib for BlockType: Content Channel Items View:Content Channel Types Include
            RockMigrationHelper.UpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "DF974799-6656-4F0C-883D-85E44EEC999A", "Content Channel Types Include", "ContentChannelTypesInclude", "", "Select any specific content channel types to show in this block. Leave all unchecked to show all content channel types ( except for excluded content channel types )", 2, @"", "5CACE343-343A-4457-A54E-FB6F2B08E871" );
            // Attrib for BlockType: Content Channel Items View:Content Channel Types Exclude
            RockMigrationHelper.UpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "DF974799-6656-4F0C-883D-85E44EEC999A", "Content Channel Types Exclude", "ContentChannelTypesExclude", "", "Select content channel types to exclude from this block. Note that this setting is only effective if 'Content Channel Types Include' has no specific content channel types selected.", 3, @"", "9028FF45-D410-4B0C-B60C-3D2DB38FE32A" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "EEFCAF48-E520-4383-A550-34CB1339951C" );
            RockMigrationHelper.DeletePage( "86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F" );

            AddColumn( "dbo.ContentChannel", "ItemTagCategories", c => c.String(maxLength: 100));
            DropForeignKey("dbo.TaggedItem", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.ContentChannel", "ItemTagCategoryId", "dbo.Category");
            DropIndex("dbo.Tag", new[] { "EntityTypeId" });
            DropIndex("dbo.TaggedItem", new[] { "EntityTypeId" });
            DropIndex("dbo.ContentChannel", new[] { "ItemTagCategoryId" });
            AlterColumn("dbo.Tag", "EntityTypeId", c => c.Int(nullable: false));
            DropColumn("dbo.TaggedItem", "EntityTypeId");
            DropColumn("dbo.ContentChannel", "ItemTagCategoryId");
            DropColumn("dbo.ContentChannel", "IsTaggingEnabled");
            DropColumn("dbo.EntityType", "LinkUrlLavaTemplate");
            CreateIndex("dbo.Tag", "EntityTypeId");
        }
    }
}
