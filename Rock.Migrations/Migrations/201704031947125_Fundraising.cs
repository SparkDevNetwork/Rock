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
    public partial class Fundraising : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // NOTE: There needs to be a SQL script that does the equivalent of this migration so that it can be run on 1.6.x versions of Rock

            // Fundraising Public | Group
            RockMigrationHelper.UpdateGroupAttributeCategory( "Fundraising Public", "fa fa-money", "", "91B43FBD-F924-4934-9CCE-7990513275CF" );

            // 'Fundraising Opportunity Type' Defined Values
            RockMigrationHelper.AddDefinedType( "Group", "Fundraising Opportunity Type", "This is what a fundraising opportunity is described as, such as Trip, Internship, Project, etc", "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D" );
            RockMigrationHelper.UpdateDefinedValue( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Trip", "", "3BB5607B-8A77-434D-8AEF-F10D513BE963", false );
            RockMigrationHelper.UpdateDefinedValue( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Internship", "", "DB378B20-525E-40E6-B7E3-80ACBD2AE8A0", false );
            RockMigrationHelper.UpdateDefinedValue( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Project", "", "DFF45DA6-6077-4651-A804-BCBE9CD68375", false );

            // GroupType: Fundraising Opportunity 4BE7FC44-332D-40A8-978E-47B7035D7A0C
            RockMigrationHelper.AddGroupType( "Fundraising Opportunity", "A group that can be used to manage a fundraising opportunity such as a mission trip or internship.", "Group", "Member", false, true, true, "fa fa-certificate ", 0, null, 0, null, "4BE7FC44-332D-40A8-978E-47B7035D7A0C" );

            // Let GroupType: Fundraising Opportunity have child group types of GroupType: Fundraising Opportunity
            this.Sql( @"INSERT INTO GroupTypeAssociation (
	GroupTypeId
	,ChildGroupTypeId
	)
SELECT Id
	,Id
FROM GroupType gt
WHERE [Guid] = '4BE7FC44-332D-40A8-978E-47B7035D7A0C'
	AND NOT EXISTS (
		SELECT *
		FROM GroupTypeAssociation gta
		WHERE GroupTypeId = gt.Id
			AND ChildGroupTypeId = gt.Id
		)" );

            // Group Type Group Attributes
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Opportunity Title", @"The public name of the fundraising opportunity so the group name could be used for internal 
use.", 0, "", "F3338652-D1A2-4778-82A7-D56B9F4CFD7F", true );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "9C7D431C-875C-4792-9E76-93F3A32BB850", "Opportunity Date Range", @"Used to display start and end date", 1, "", "237463F7-A206-4B43-AFDD-84E422527E87" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Opportunity Location", @"The location description of the opportunity such as the city or country.", 2, "", "2339847F-2746-41D9-8CB5-2410FC8358D2" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Opportunity Summary", @"", 3, "", "697FDCF1-CA91-4DB5-9306-CD4835108613" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Opportunity Photo", @"", 4, "", "125F7AAC-F01D-4527-AA5E-5C8345AC3F66" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Opportunity Details", @"", 5, "", "1E2F1416-2C4C-44DF-BE19-7D8FA9523115" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "3EE69CBC-35CE-4496-88CC-8327A447603F", "Individual Fundraising Goal", @"The default individual fundraising goal.", 6, "", "7CD834F8-43F2-400E-A352-898030124102" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Opportunity Type", @"What the opportunity is described as, such as Trip, Internship, Project,Etc", 7, "", "F0846135-1A61-4AFA-8F9B-76D9821084DE", true );


            // update Attribute Qualifier for Opportunity Type to use 'Fundraising Opportunity Type' DefinedType
            Sql( @"
DECLARE @DefinedTypeFundraisingType INT = (
		SELECT TOP 1 Id
		FROM DefinedType
		WHERE [Guid] = '53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D'
		)
DECLARE @AttributeId INT = (
		SELECT TOP 1 Id
		FROM Attribute
		WHERE [Guid] = 'F0846135-1A61-4AFA-8F9B-76D9821084DE'
		)

IF NOT EXISTS (
		select *
		FROM AttributeQualifier
		WHERE [Guid] = 'C8FE5DF2-E174-4DEC-8B50-D478592E938F'
		)
BEGIN
	INSERT INTO AttributeQualifier (
		AttributeId
		,[Key]
		,[Value]
		,[Guid]
		,IsSystem
		)
	SELECT @AttributeId
		,'definedtype'
		,@DefinedTypeFundraisingType
		,'C8FE5DF2-E174-4DEC-8B50-D478592E938F'
		,0
END
" );

            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Update Content Channel", @"The content channel to use to display any blog-like updates for the fundraising opportunity", 8, "", "6756D396-97F8-48A0-B69C-279E561F9D48" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Commenting", @"Helps to determine if commenting is allowed (default is no commenting).", 9, "False", "38E1065D-4F6A-428E-B781-48F6BDACA614" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Registration Instance", @"The Id of the registration instance (if any) that is associated with this fundraising opportunity", 10, "", "E06EBFAD-E0B1-4AE2-B9B1-4C988EFFA844" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Individual Disabling of Contribution Requests", @"Determines if individuals should be allowed to disable their contribution requests.", 11, "False", "9BEA4F1C-E2FD-4669-B2CD-1269D4DCB97A" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Individual Editing of Fundraising Goal", @"Determines if individuals should be allowed to edit their fundraising goal on their profile page.", 12, "False", "878F6172-68EB-4ACD-9C16-F848CAF19D47" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Cap Fundraising Amount", @"If this is set to 'Yes', the individual won't be able to fundraise for more than the Individual Fundraising Goal amount", 13, "False", "49012757-0ADE-419A-981C-384417D2E543" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Financial Account", @"The financial account that the donations should be tied to.", 14, "", "7C6FF01B-F68E-4A83-A96D-85071A92AAF1", true );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public", @"Determines if this Fundraising Opportunity should be included in lists that are displayed on the public web site.", 15, true.ToString(), "BBD6C818-765C-43FB-AA72-5AF66F91B499", true );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Registration Notes", @"", 16, "", "7360CF56-7DF5-42E9-AD2B-AD839E0D4EDB" );

            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "3EE69CBC-35CE-4496-88CC-8327A447603F", "Individual Fundraising Goal", @"Optional override of the default individual fund raising goal.  This is configurable only in internal group member editor. An individual could not adjust this themselves.", 0, "", "EABAE672-0886-450B-9296-2BADC56A0137" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Personal Opportunity Introduction", @"A personal note to display on the individual's fundraising participant page.", 1, "", "018B201C-D9C2-4EDE-9FC9-B52E2F799325" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Public Contribution Requests", @"Set this to Yes to hide both the fundraising progress and request for donations on the profile page.", 2, "False", "2805298E-E21A-4679-B5CA-69D6FF4EAD31" );

            // set Category of some of the Attributes to 'Fundraising Public'
            Sql( @"
-- Opportunity Title
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = 'F3338652-D1A2-4778-82A7-D56B9F4CFD7F' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)

-- Opportunity Date Range
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = '237463F7-A206-4B43-AFDD-84E422527E87' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)

-- Opportunity Location
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = '2339847F-2746-41D9-8CB5-2410FC8358D2' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)

-- Opportunity Summary
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = '697FDCF1-CA91-4DB5-9306-CD4835108613' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)

-- Opportunity Photo
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = '125F7AAC-F01D-4527-AA5E-5C8345AC3F66' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)

-- Opportunity Details
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = '1E2F1416-2C4C-44DF-BE19-7D8FA9523115' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)
" );


            RockMigrationHelper.AddSecurityAuthForAttribute( "F3338652-D1A2-4778-82A7-D56B9F4CFD7F", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "237463F7-A206-4B43-AFDD-84E422527E87", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "2339847F-2746-41D9-8CB5-2410FC8358D2", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "697FDCF1-CA91-4DB5-9306-CD4835108613", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "125F7AAC-F01D-4527-AA5E-5C8345AC3F66", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "1E2F1416-2C4C-44DF-BE19-7D8FA9523115", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "7CD834F8-43F2-400E-A352-898030124102", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "F0846135-1A61-4AFA-8F9B-76D9821084DE", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "6756D396-97F8-48A0-B69C-279E561F9D48", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "38E1065D-4F6A-428E-B781-48F6BDACA614", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "E06EBFAD-E0B1-4AE2-B9B1-4C988EFFA844", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "9BEA4F1C-E2FD-4669-B2CD-1269D4DCB97A", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "878F6172-68EB-4ACD-9C16-F848CAF19D47", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "49012757-0ADE-419A-981C-384417D2E543", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "7C6FF01B-F68E-4A83-A96D-85071A92AAF1", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "2039F657-1CA0-4444-8FDA-C82F80CDD131", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "EABAE672-0886-450B-9296-2BADC56A0137", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "018B201C-D9C2-4EDE-9FC9-B52E2F799325", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "2805298E-E21A-4679-B5CA-69D6FF4EAD31", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "BBD6C818-765C-43FB-AA72-5AF66F91B499", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );

            RockMigrationHelper.AddGroupTypeRole( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "Participant", string.Empty, 1, null, null, "F82DF077-9664-4DA8-A3D9-7379B690124D", true, false, true );
            RockMigrationHelper.AddGroupTypeRole( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "Leader", string.Empty, 0, null, null, "253973A5-18F2-49B6-B2F1-F8F84294AAB2", true, true, false );


            // Add NoteType 'Fundraising Opportunity Comment' Guid:9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95
            // make sure AllUsers have EDIT auth (the block will control when Edit/Add is allowed)
            RockMigrationHelper.UpdateNoteType( "Fundraising Opportunity Comment", "Rock.Model.Group", true, "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", true );
            RockMigrationHelper.AddSecurityAuthForNoteType( "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", 0, Rock.Security.Authorization.EDIT, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );

            // Catchup on migrations for new block and attributes that haven't been done yet
            RockMigrationHelper.UpdateBlockType( "Stark Dynamic Attributes", "Template block for developers to use to start a new detail block that supports dynamic attributes.", "~/Blocks/Utility/StarkDynamicAttributes.ascx", "Utility", "7C34A0FA-ED0D-4B8B-B458-6EC970711726" );
            // Attrib for BlockType: Group Tree View:Initial Active Setting
            RockMigrationHelper.UpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Active Setting", "InitialActiveSetting", "", "Select whether to initially show all or just active groups in the treeview", 8, @"1", "2AD968BA-6721-4B69-A4FE-B57D8FB0ECFB" );
            // Attrib for BlockType: Group Tree View:Initial Count Setting
            RockMigrationHelper.UpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Count Setting", "InitialCountSetting", "", "Select the counts that should be initially shown in the treeview.", 7, @"0", "36D18581-3874-4C5A-A01B-793A458F9F91" );
            // Attrib for BlockType: Group Member Detail:Show 'Move to another group' button
            RockMigrationHelper.UpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Move to another group' button", "ShowMoveToOtherGroup", "", "Set to false to hide the 'Move to another group' button", 1, @"True", "260A458D-BC35-4A36-B966-172870AFB24B" );
            // Attrib for BlockType: Group Member List:Show Date Added
            RockMigrationHelper.UpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Added", "ShowDateAdded", "", "Should the date that person was added to the group be displayed for each group member?", 6, @"False", "F281090E-A05D-4F81-AD80-A3599FB8E2CD" );
            // Attrib for BlockType: Group Member List:Show Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "", "Setting to show/hide campus filter.", 4, @"True", "65B9EA6C-D904-4105-8B51-CCA784DDAAFA" );
            // Attrib for BlockType: Group Member List:Show First/Last Attendance
            RockMigrationHelper.UpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show First/Last Attendance", "ShowAttendance", "", "If the group allows attendance, should the first and last attendance date be displayed for each group member?", 5, @"False", "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664" );
            // Attrib for BlockType: Transaction List:Default Transaction View
            RockMigrationHelper.UpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Default Transaction View", "DefaultTransactionView", "", "Select whether you want to initially see Transactions or Transaction Details", 6, @"Transactions", "8D067930-6355-4DC7-98E1-3619C871AC83" );

            //// Pages and Blocks migration for Fundraising

            RockMigrationHelper.AddPage( true,  "142627AE-6590-48E3-BFCA-3669260B8CF2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Fundraising Matching", "", "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "5A8FBB92-85E5-4FD3-AF88-F3897C6CBC35", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Fundraising Opportunity View", "", "BA673ABE-A45A-4835-A3A0-94A60341B96F", "" ); // Site:External Website

            // Turn off BreadCrumbDisplayName for Fundraising Opportunity View
            Sql( @"UPDATE [Page]
SET BreadCrumbDisplayName = 0
WHERE[Guid] = 'BA673ABE-A45A-4835-A3A0-94A60341B96F'" );

            // If the "Missions" page doesn't exist, the Fundraising Opportunity View won't have a parent page, so change it to Installed Plugins Instead
            Sql( @"DECLARE @InstalledPluginsPageId INT = (
		SELECT TOP 1 Id
		FROM [Page]
		WHERE [Guid] = '5B6DBC42-8B03-4D15-8D92-AAFA28FD8616'
		)

UPDATE [Page]
SET ParentPageId = @InstalledPluginsPageId
WHERE [Guid] = 'BA673ABE-A45A-4835-A3A0-94A60341B96F'
	AND ParentPageId IS NULL
" );

            RockMigrationHelper.AddPage( true, "BA673ABE-A45A-4835-A3A0-94A60341B96F", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Fundraising Leader Toolbox", "", "9DADC93F-C9E7-4567-B73E-AD264A93E37D", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "BA673ABE-A45A-4835-A3A0-94A60341B96F", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Fundraising Donation", "", "E40BEA3D-0304-4AD2-A45D-9BAD9852E3BA", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "BA673ABE-A45A-4835-A3A0-94A60341B96F", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Fundraising Participant", "", "9F76591C-CEE4-4824-8478-E3BDA48D66ED", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "E40BEA3D-0304-4AD2-A45D-9BAD9852E3BA", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Fundraising Transaction Entry", "", "F04D69C1-786A-4204-8A67-5669BDFEB533", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Transaction Entity Matching", "Used to assign an Entity to a Transaction Detail record", "~/Blocks/Finance/TransactionEntityMatching.ascx", "Finance", "A58BCB1E-01D9-4F60-B925-D831A9537051" );
            RockMigrationHelper.UpdateBlockType( "Fundraising Donation Entry", "Block that starts out a Fundraising Donation by prompting for information prior to going to a TransactionEntry block", "~/Blocks/Fundraising/FundraisingDonationEntry.ascx", "Fundraising", "A24D68F2-C58B-4322-AED8-6556DBED1B76" );
            RockMigrationHelper.UpdateBlockType( "Fundraising Leader Toolbox", "The Leader Toolbox for a fundraising opportunity", "~/Blocks/Fundraising/FundraisingLeaderToolbox.ascx", "Fundraising", "B90F730D-6319-4749-A3C0-BBFDD69D9BC3" );
            RockMigrationHelper.UpdateBlockType( "Fundraising List", "Lists Fundraising Opportunities (Groups) that have the ShowPublic attribute set to true", "~/Blocks/Fundraising/FundraisingList.ascx", "Fundraising", "E664BB02-D501-40B0-AAD6-D8FA0E63438B" );
            RockMigrationHelper.UpdateBlockType( "Fundraising Opportunity View", "Public facing block that shows a fundraising opportunity", "~/Blocks/Fundraising/FundraisingOpportunityView.ascx", "Fundraising", "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241" );
            RockMigrationHelper.UpdateBlockType( "Fundraising Opportunity Participant", "Public facing block that shows a fundraising opportunity participant", "~/Blocks/Fundraising/FundraisingParticipant.ascx", "Fundraising", "1FEA697F-DD12-4FE0-BC58-EE896123E7F1" );

            // Add Block to Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlock( true,  "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", "", "A58BCB1E-01D9-4F60-B925-D831A9537051", "Fundraising Transaction Group Member Matching", "Main", @"", @"", 0, "85B35E05-BAD4-44F1-8E81-EF77959F199B" );
            // Add Block to Page: Missions, Site: External Website
            RockMigrationHelper.AddBlock( true, "5A8FBB92-85E5-4FD3-AF88-F3897C6CBC35", "", "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "Fundraising List", "Main", @"", @"", 1, "7759CDA6-BCE0-42D9-99C8-E991600F7E0D" );
            // Add Block to Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlock( true, "BA673ABE-A45A-4835-A3A0-94A60341B96F", "", "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "Fundraising Opportunity View", "Main", @"", @"", 0, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C" );
            // Add Block to Page: Fundraising Participant, Site: External Website
            RockMigrationHelper.AddBlock( true, "9F76591C-CEE4-4824-8478-E3BDA48D66ED", "", "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "Fundraising Opportunity Participant", "Main", @"", @"", 0, "BAF6AD44-BFBB-46AE-B1F2-89511C273FAE" );
            // Add Block to Page: Fundraising Donation Entry, Site: External Website
            RockMigrationHelper.AddBlock( true, "E40BEA3D-0304-4AD2-A45D-9BAD9852E3BA", "", "A24D68F2-C58B-4322-AED8-6556DBED1B76", "Fundraising Donation Entry", "Main", @"", @"", 0, "B557FC47-0D19-4EED-A386-04CF569E5967" );
            // Add Block to Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlock( true, "F04D69C1-786A-4204-8A67-5669BDFEB533", "", "74EE3481-3E5A-4971-A02E-D463ABB45591", "Fundraising Transaction Entry", "Main", @"", @"", 0, "1BAD904E-2F79-4488-B8BE-EECD67AE2925" );
            // Add Block to Page: Fundraising Leader Toolbox, Site: External Website
            RockMigrationHelper.AddBlock( true, "9DADC93F-C9E7-4567-B73E-AD264A93E37D", "", "B90F730D-6319-4749-A3C0-BBFDD69D9BC3", "Fundraising Leader Toolbox", "Main", @"", @"", 0, "558375A3-2DFF-43F3-A9EF-04F503C7EB55" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D43D2BD0-4D68-4C75-9126-40DEC929CF5E'" );  // Page: Missions,  Zone: Main,  Block: Content
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '7759CDA6-BCE0-42D9-99C8-E991600F7E0D'" );  // Page: Missions,  Zone: Main,  Block: Fundraising List

            // Attrib for BlockType: Transaction Entry:Account Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Account Campus Context", "AccountCampusContext", "", "Should any context be applied to the Account List", 4, @"-1", "0440B425-57B2-4E65-84C8-5B05D9A46708" );
            // Attrib for BlockType: Transaction Entry:Allow Accounts In URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Accounts In URL", "AllowAccountsInURL", "", "Should the option to display accounts via URL param be enabled.  Valid URL Param are AccountIds=1,2,3 or AccountGlCodes=40100,40110", 1, @"False", "E4492D68-45EA-41FF-A611-760DB13EC36E" );
            // Attrib for BlockType: Transaction Entry:Only Public Accounts In URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Public Accounts In URL", "OnlyPublicAccountsInURL", "", "Should the accounts via URL only display Accounts which are public", 2, @"True", "A13AC34C-4790-430F-8182-43AAC01FF177" );
            // Attrib for BlockType: Transaction Entry:Enable Initial Back button
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Initial Back button", "EnableInitialBackbutton", "", "Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry", 10, @"False", "86A2A716-3F48-4AA1-8B18-E2BB47C8FD40" );
            // Attrib for BlockType: Transaction Entry:Entity Id Param
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Id Param", "EntityIdParam", "", "The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)", 8, @"", "8E45ABBB-43A8-46B1-A32C-DB9474A65BE0" );
            // Attrib for BlockType: Transaction Entry:Transaction Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Transaction Entity Type", "TransactionEntityType", "", "The Entity Type for the Transaction Detail Record (usually left blank)", 7, @"", "4F712013-75C7-4157-8EF4-2EF26210378B" );
            // Attrib for BlockType: Transaction Entry:Account Header Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Account Header Template", "AccountHeaderTemplate", "", "The Lava Template to use as the amount input label for each account", 3, @"{{ Account.PublicName }}", "F71BD118-F1EB-4E93-AD7B-86D2A40AAE95" );
            // Attrib for BlockType: Transaction Entry:Invalid Account Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Account Message", "InvalidAccountMessage", "", "Display this text (HTML) as an error alert if an invalid 'account' or 'glaccount' is passed through the URL.", 3, @"", "00106DDE-CD23-4E4B-A4B6-B3819E196364" );
            // Attrib for BlockType: Transaction Entry:Transaction Header
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Transaction Header", "TransactionHeader", "", "The Lava template which will be displayed prior to the Amount entry", 9, @"", "65FB0B9A-670E-4AB9-9666-77959B4B702E" );
            // Attrib for BlockType: Transaction Entry:Transaction Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Type", "TransactionType", "", "", 6, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "ADB22E3F-1DC0-4BA6-AC77-09FE8580CD21" );
            // Attrib for BlockType: Transaction Entry:Allowed Transaction Attributes From URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Allowed Transaction Attributes From URL", "AllowedTransactionAttributesFromURL", "", "Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world", 5, @"", "B4C8AA1A-E43E-48F1-9221-C83F9E750352" );
            // Attrib for BlockType: Content Channel View:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this content channel block.", 0, @"", "BA867DBE-12CE-464D-AA4D-865E3E50E5F9" );
            // Attrib for BlockType: Transaction Entity Matching:TransactionTypeGuid
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "TransactionTypeGuid", "TransactionTypeGuid", "", "", 0, @"", "6752891C-45D3-4AF5-810C-FF3056AC8392" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeGuid
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "EntityTypeGuid", "EntityTypeGuid", "", "", 0, @"", "F58FE9C9-94E9-46EC-9F06-082B47570CD7" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeQualifierColumn
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "EntityTypeQualifierColumn", "EntityTypeQualifierColumn", "", "", 0, @"", "A9BB04F1-7C63-4ABF-A174-D93003B2833F" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeQualifierValue
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "EntityTypeQualifierValue", "EntityTypeQualifierValue", "", "", 0, @"", "9488B744-D932-4CB9-AEEC-EEF54573DB8B" );
            // Attrib for BlockType: Fundraising Donation Entry:Show First Name Only
            RockMigrationHelper.UpdateBlockTypeAttribute( "A24D68F2-C58B-4322-AED8-6556DBED1B76", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show First Name Only", "ShowFirstNameOnly", "", "Only show the First Name of each participant instead of Full Name", 2, @"False", "AF583D88-2DCA-4589-AF53-CBE61295C02E" );
            // Attrib for BlockType: Fundraising Donation Entry:Transaction Entry Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "A24D68F2-C58B-4322-AED8-6556DBED1B76", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Entry Page", "TransactionEntryPage", "", "The Transaction Entry page to navigate to after prompting for the Fundraising Specific inputs", 1, @"", "4E9D70B9-9CF6-4F6C-87B4-8B0DDFDFEB3E" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Main Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Main Page", "MainPage", "", "The main page for the fundraising opportunity", 3, @"", "E58C0A8D-FE41-4FE0-B5F4-F7E8B3934D7A" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Participant Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Page", "ParticipantPage", "", "The partipant page for a participant of this fundraising opportunity", 2, @"", "A2C8F514-8805-4E0A-9493-75289F543B43" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Summary Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Summary Lava Template", "SummaryLavaTemplate", "", "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.", 1, @"", "6F245AB0-5EEC-4EF9-A029-6C6BFB0ED64B" );
            // Attrib for BlockType: Fundraising List:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use for the results", 2, @"", "ED2EA497-4316-4E44-A5A4-69E69CC7ECBC" );
            // Attrib for BlockType: Fundraising List:Details Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Details Page", "DetailsPage", "", "", 1, @"", "F17BD62D-8134-47A5-BDBC-F7F6CD07974E" );
            // Attrib for BlockType: Fundraising Opportunity View:Leader Toolbox Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Leader Toolbox Page", "LeaderToolboxPage", "", "The toolbox page for a leader of this fundraising opportunity", 6, @"", "9F8F6E06-338F-403E-9D2D-A4FCBA13A844" );
            // Attrib for BlockType: Fundraising Opportunity View:Participant Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Page", "ParticipantPage", "", "The partipant page for a participant of this fundraising opportunity", 7, @"", "EF966837-E420-45B9-A740-F1E43C08469D" );
            // Attrib for BlockType: Fundraising Opportunity View:Donation Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Donation Page", "DonationPage", "", "The page where a person can donate to the fundraising opportunity", 5, @"", "685DA61C-AF28-4389-AA7F-4BC26BED6CDD" );
            // Attrib for BlockType: Fundraising Opportunity View:Summary Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Summary Lava Template", "SummaryLavaTemplate", "", "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.", 1, @"", "B802BE78-42DE-4E0F-8C1E-5788582C905B" );
            // Attrib for BlockType: Fundraising Opportunity View:Sidebar Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Sidebar Lava Template", "SidebarLavaTemplate", "", "Lava template for what to display on the left side bar. Usually used to show event registration or other info.", 2, "", "393030EB-18B6-4D91-943F-BAB3853B84BD" );
            // Attrib for BlockType: Fundraising Opportunity View:Updates Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Updates Lava Template", "UpdatesLavaTemplate", "", "Lava template for the Updates (Content Channel Items)", 3, @"", "AFC2C61D-87F8-4C4E-9CC2-98F2009A500C" );
            // Attrib for BlockType: Fundraising Opportunity View:Note Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "", "Note Type to use for comments", 4, @"9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", "287571CE-B731-477A-B948-FD05736C2CFE" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Note Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "", "Note Type to use for participant comments", 4, @"FFFC3644-60CD-4D14-A714-E8DCC202A0E1", "C3494517-31E3-4B04-AE37-570331073903" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Profile Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Profile Lava Template", "ProfileLavaTemplate", "", "Lava template for what to display at the top of the main panel. Usually used to display information about the participant such as photo, name, etc.", 1, @"", "84C3DD64-436E-40BC-ADC3-7F86BBB890C0" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Updates Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Updates Lava Template", "UpdatesLavaTemplate", "", "Lava template for the Updates (Content Channel Items)", 3, @"", "17DF7E42-B2D7-4E5D-9EF7-EE25758139FC" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Main Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Main Page", "MainPage", "", "The main page for the fundraising opportunity", 6, @"", "592C88ED-6993-4292-96FA-C05CB8A6F00C" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Donation Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Donation Page", "DonationPage", "", "The page where a person can donate to the fundraising opportunity", 5, @"", "5A0D8B2E-8692-481D-BD1E-48236021BFF0" );
            // Attrib Value for Block:Fundraising List, Attribute:Details Page Page: Missions, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "7759CDA6-BCE0-42D9-99C8-E991600F7E0D", "F17BD62D-8134-47A5-BDBC-F7F6CD07974E", @"ba673abe-a45a-4835-a3a0-94a60341b96f" );

            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Leader Toolbox Page Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "9F8F6E06-338F-403E-9D2D-A4FCBA13A844", @"9dadc93f-c9e7-4567-b73e-ad264a93e37d" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Note Type Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "287571CE-B731-477A-B948-FD05736C2CFE", @"9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Participant Page Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "EF966837-E420-45B9-A740-F1E43C08469D", @"9f76591c-cee4-4824-8478-e3bda48d66ed" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Donation Page Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "685DA61C-AF28-4389-AA7F-4BC26BED6CDD", @"e40bea3d-0304-4ad2-a45d-9bad9852e3ba" );

            // Attrib Value for Block:Fundraising Opportunity Participant, Attribute:Main Page Page: Fundraising Participant, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "BAF6AD44-BFBB-46AE-B1F2-89511C273FAE", "592C88ED-6993-4292-96FA-C05CB8A6F00C", @"ba673abe-a45a-4835-a3a0-94a60341b96f" );

            // Attrib Value for Block:Fundraising Opportunity Participant, Attribute:Note Type Page: Fundraising Participant, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "BAF6AD44-BFBB-46AE-B1F2-89511C273FAE", "C3494517-31E3-4B04-AE37-570331073903", @"FFFC3644-60CD-4D14-A714-E8DCC202A0E1" );
            // Attrib Value for Block:Fundraising Opportunity Participant, Attribute:Donation Page Page: Fundraising Participant, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "BAF6AD44-BFBB-46AE-B1F2-89511C273FAE", "5A0D8B2E-8692-481D-BD1E-48236021BFF0", @"e40bea3d-0304-4ad2-a45d-9bad9852e3ba" );

            // Attrib Value for Block:Fundraising Transaction Group Member Matching, Attribute:EntityTypeQualifierColumn Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "85B35E05-BAD4-44F1-8E81-EF77959F199B", "A9BB04F1-7C63-4ABF-A174-D93003B2833F", @"GroupTypeId" );

            // Attrib Value for Block:Fundraising Transaction Group Member Matching, Attribute:EntityTypeQualifierValue Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "85B35E05-BAD4-44F1-8E81-EF77959F199B", "9488B744-D932-4CB9-AEEC-EEF54573DB8B", @"" );

            // Set the EntityTypeQualifierValue for Transaction Group Member Matching to the Fundraising Opportunity GroupTypeId
            this.Sql( @"
DECLARE @GroupTypeId INT = (
		SELECT Id
		FROM GroupType
		WHERE [Guid] = '4BE7FC44-332D-40A8-978E-47B7035D7A0C'
		)

UPDATE [AttributeValue]
SET Value = @GroupTypeId
WHERE AttributeId = (
		SELECT TOP 1 Id
		FROM Attribute
		WHERE [Guid] = '9488B744-D932-4CB9-AEEC-EEF54573DB8B'
		)
" );


            // Attrib Value for Block:Fundraising Transaction Group Member Matching, Attribute:TransactionTypeGuid Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "85B35E05-BAD4-44F1-8E81-EF77959F199B", "6752891C-45D3-4AF5-810C-FF3056AC8392", @"2d607262-52d6-4724-910d-5c6e8fb89acc" );
            // Attrib Value for Block:Fundraising Transaction Group Member Matching, Attribute:EntityTypeGuid Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "85B35E05-BAD4-44F1-8E81-EF77959F199B", "F58FE9C9-94E9-46EC-9F06-082B47570CD7", @"49668b95-fedc-43dd-8085-d2b0d6343c48" );

            // Attrib Value for Block:Fundraising Donation Entry, Attribute:Transaction Entry Page Page: Fundraising Donation Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "B557FC47-0D19-4EED-A386-04CF569E5967", "4E9D70B9-9CF6-4F6C-87B4-8B0DDFDFEB3E", @"f04d69c1-786a-4204-8a67-5669bdfeb533" );
            // Attrib Value for Block:Fundraising Donation Entry, Attribute:Show First Name Only Page: Fundraising Donation Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "B557FC47-0D19-4EED-A386-04CF569E5967", "AF583D88-2DCA-4589-AF53-CBE61295C02E", @"False" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Account Header Template Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "F71BD118-F1EB-4E93-AD7B-86D2A40AAE95", @"Donation Amount" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Allow Accounts In URL Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "E4492D68-45EA-41FF-A611-760DB13EC36E", @"True" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Only Public Accounts In URL Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "A13AC34C-4790-430F-8182-43AAC01FF177", @"True" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Invalid Account Message Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "00106DDE-CD23-4E4B-A4B6-B3819E196364", @"" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Account Campus Context Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "0440B425-57B2-4E65-84C8-5B05D9A46708", @"-1" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Allowed Transaction Attributes From URL Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "B4C8AA1A-E43E-48F1-9221-C83F9E750352", @"" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Transaction Type Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "ADB22E3F-1DC0-4BA6-AC77-09FE8580CD21", @"" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Entity Id Param Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "8E45ABBB-43A8-46B1-A32C-DB9474A65BE0", @"GroupMemberId" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Transaction Header Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "65FB0B9A-670E-4AB9-9666-77959B4B702E", @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = groupMember | Attribute:'IndividualFundraisingGoal','RawValue' %}
{% if fundraisingGoal == '' %}
  {% assign fundraisingGoal = groupMember.Group | Attribute:'IndividualFundraisingGoal','RawValue' %}
{% endif %}

{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}

{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}

{% assign amountRemaining = fundraisingGoal | Minus:TransactionEntityTransactionsTotal %}

<div class='row'>
    <div class='col-md-6'>
      <dl>
        <dt>Fundraising Opportunity</dt>
        <dd>{{ groupMember.Group | Attribute:'OpportunityTitle' }}</dd>
        <dt>Fundraising Goal</dt>
        <dd>
            {{ fundraisingGoal | FormatAsCurrency }}
        </dd>
      </dl>
      <p></p>
    </div>
    <div class='col-md-6'>
        <dl>
            <dt>Participant</dt>
            <dd>{{ groupMember.Person.FullName }}</dd>
            <dt>Amount Remaining</dt>
            <dd>{{ amountRemaining | FormatAsCurrency  }}</dd>
        </dl>
    </div>
</div>" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Enable Initial Back button Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "86A2A716-3F48-4AA1-8B18-E2BB47C8FD40", @"True" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Transaction Entity Type Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "4F712013-75C7-4157-8EF4-2EF26210378B", @"49668b95-fedc-43dd-8085-d2b0d6343c48" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Credit Card Gateway Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "3D478949-1F85-4E81-A403-22BBA96B8F69", @"6432d2d2-32ff-443d-b5b3-fb6c8414c3ad" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:ACH Gateway Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7", @"6432d2d2-32ff-443d-b5b3-fb6c8414c3ad" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Source Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "5C54E6E7-1C21-4959-98EA-FB1C2D0A0D61", @"7d705ce7-7b11-4342-a58e-53617c5b4e69" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Address Type Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "DBF313AB-0488-4BF7-A11D-1998D7A3476D", @"8c52e53c-2a66-435a-ae6e-5ee307d9a0dc" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Layout Style Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "23B31F2F-9366-446D-9D3E-4CA68A6876D1", @"Vertical" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Accounts Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "DAB27F0A-D0C0-4275-93F4-DEF227F6B1A2", @"" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Add Account Text Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "1133170C-8E4C-4020-B795-F799F893D70D", @"Add Another Account" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Scheduled Transactions Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "63CA1F26-6942-48F4-9A15-F0A2D40E3FAB", @"False" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Prompt for Phone Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "8A572D6B-5CC1-4357-BFD5-8D887433A0AB", @"False" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Prompt for Email Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "8B67B723-1C71-44EF-81F0-F4225CE7039B", @"True" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Confirmation Header Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "FA6300AD-9268-47FD-BBBE-BB6A415B0002", @"<p>
    Please confirm the information below. Once you have confirmed that the information is
    accurate click the 'Finish' button to complete your transaction.
</p>
" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Confirmation Footer Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "B1F9196D-B51D-4ECD-A7BE-89F34431D736", @"<div class='alert alert-info'>
    {% assign opportunityType = TransactionEntity.Group | Attribute:'OpportunityType' | Downcase  %}
    
    In accordance of tax laws that prohibit donations designated to an individual, funds raised to support individuals 
    for {{ opportunityType | Pluralize }} will go into a pool to support the entire {{ opportunityType }} and its participants. Individual support will be tracked
    internally to determine the effectiveness of the individual's ability to raise support. {{ 'Global' | Attribute:'OrganizationName' }} must have 
    full control of the donated funds and discretion as to their use in order for your donation to be tax deductible.
    
</div>

" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Success Header Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "1597A542-E6EB-4E29-A435-E5C23785251E", @"{% assign groupMember = TransactionEntity %}

<div class='row'>
    <div class='col-md-6'>
      <dl>
        <dt>Transaction Date</dt>
        <dd>
            {{ FinancialTransaction.TransactionDateTime }}
        </dd>
      </dl>
    </div>
    <div class='col-md-6'>
    </div>
</div>        
<div class='row'>
    <div class='col-md-6'>
        <dl>
            <dt>Fundraising Opportunity</dt>
            <dd>{{ groupMember.Group | Attribute:'OpportunityTitle' }}</dd>
        </dl>
    </div>
    <div class='col-md-6'>
        <dl>
            <dt>Participant</dt>
            <dd>{{ groupMember.Person.FullName }}</dd>
        </dl>
    </div>
</div>    
<p>Thank you for your contribution for this {{ groupMember.Group | Attribute:'OpportunityType' }}. We are grateful for your commitment.</p>" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Success Footer Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "188C6D55-CC08-4019-AA5F-706251509696", @"" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Impersonation Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "FE64D72C-EA41-4C4E-9F0C-48048EEAB8A1", @"False" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Additional Accounts Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "EAF2435D-FACE-40F2-832D-CDB5A4D51BF3", @"False" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Confirm Account Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "95F2EFCF-6A68-4857-90AB-CC3A467EDF9A", @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Batch Name Prefix Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "245BDD4E-E8FF-4039-8C0B-C7AC1C185D1D", @"Fundraising Donations" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Success Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "6CFD636A-F614-42A1-ACE3-390AB73625AE", @"Donation Information" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Save Account Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "7697364F-C7F8-4FC2-8664-F9581C326B85", @"Make Giving Even Easier" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Panel Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "DC96A27B-A0F6-43C6-AF97-04DD65457A31", @"Gifts" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Contribution Info Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "983C0903-1AFC-4686-8358-6245F1BBE8B2", @"Contribution Information" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Personal Info Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "DD6A5846-E5F5-4B31-89DB-D89449A1C5AD", @"Donor Information" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Payment Info Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "9A9D054D-7AD9-4BD6-B5AD-2D66BF03F54A", @"Payment Information" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Confirmation Title Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "E6F83950-B4C7-47CB-9BE3-3C1AED78292B", @"Confirm Information" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Connection Status Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "743F636C-0EBF-421F-A1DD-4C4848638F6F", @"368dd475-242c-49c4-a42c-7278be690cc2" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Record Status Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "36299C3A-53F5-4DC1-AB2C-89482554A305", @"283999ec-7346-42e3-b807-bce9b2babb49" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Receipt Email Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "BB694CEB-DFE3-4670-92F1-D57FBA510DFF", @"" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Payment Comment Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "1F37A67E-AAA5-4D2F-918D-DDA11C00CCC1", @"Fundraising" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Enable Business Giving Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "16E84C69-7E88-440C-930F-2AA03BA4B8B7", @"False" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Comment Entry Label Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "7C1C46F9-6713-4825-976E-6859702EDBAA", @"Comment" );
            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Enable Comment Entry Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "12FDEC08-5257-4E67-B486-480AAFC43E6B", @"False" );
            // Attrib Value for Block:Fundraising Leader Toolbox, Attribute:Summary Lava Template Page: Fundraising Leader Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "558375A3-2DFF-43F3-A9EF-04F503C7EB55", "6F245AB0-5EEC-4EF9-A029-6C6BFB0ED64B", @"<h1>{{ Group | Attribute:'OpportunityTitle' }}</h1>
{% assign dateRangeParts = Group | Attribute:'OpportunityDateRange','RawValue' | Split:',' %}
{% assign dateRangePartsSize = dateRangeParts | Size %}
{% if dateRangePartsSize == 2 %}
    {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }} to {{ dateRangeParts[1] | Date:'MMMM dd, yyyy' }}<br/>
{% elsif dateRangePartsSize == 1  %}      
    {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }}
{% endif %}
{{ Group | Attribute:'OpportunityLocation' }}

<br />
<br />
<p>
{{ Group | Attribute:'OpportunitySummary' }}
</p>
" );
            // Attrib Value for Block:Fundraising Leader Toolbox, Attribute:Main Page Page: Fundraising Leader Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "558375A3-2DFF-43F3-A9EF-04F503C7EB55", "E58C0A8D-FE41-4FE0-B5F4-F7E8B3934D7A", @"ba673abe-a45a-4835-a3a0-94a60341b96f" );
            // Attrib Value for Block:Fundraising Leader Toolbox, Attribute:Participant Page Page: Fundraising Leader Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "558375A3-2DFF-43F3-A9EF-04F503C7EB55", "A2C8F514-8805-4E0A-9493-75289F543B43", @"9f76591c-cee4-4824-8478-e3bda48d66ed" );

            // Attrib for BlockType: Fundraising Opportunity View:Set Page Title to Opportunity Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title to Opportunity Title", "SetPageTitletoOpportunityTitle", "", "", 8, @"True", "61C77239-8954-4826-8C18-E1C822C540E7" );

            // Attrib for BlockType: Fundraising Opportunity Participant:Show Clipboard Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Clipboard Icon", "ShowClipboardIcon", "", "Show a clipboard icon which will copy the page url to the users clipboard", 7, @"True", "8CEF54C3-D5E1-4A8F-911B-A20DE91A9007" );

            // Attrib for BlockType: Fundraising Opportunity View:Max Occurrences
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "", "The maximum number of event registration occurrences to show.", 10, @"100", "4FAB5601-E142-456A-A081-3E1D05694FA5" );
            // Attrib for BlockType: Fundraising Opportunity View:Registration Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", "The page to use for registrations.", 11, @"", "98CE1B2F-F478-443E-9217-E864B3799D79" );
            // Attrib for BlockType: Fundraising List:Fundraising Opportunity Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Fundraising Opportunity Types", "FundraisingOpportunityTypes", "", "Select which opportunity types are shown, or leave blank to show all", 1, @"", "8BE53032-17AE-449F-9A42-1CC83A66DA2B" );
            // Attrib for BlockType: Fundraising Opportunity View:Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "", "Optional date range to filter the event registration occurrences on.", 9, @",", "A49B230A-C23D-442A-B3A6-7DCFD3F4DACE" );

            // Attrib Value for Block:Fundraising List, Attribute:Fundraising Opportunity Types Page: Missions, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "7759CDA6-BCE0-42D9-99C8-E991600F7E0D", "8BE53032-17AE-449F-9A42-1CC83A66DA2B", @"3bb5607b-8a77-434d-8aef-f10d513be963,db378b20-525e-40e6-b7e3-80acbd2ae8a0" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Set Page Title to Opportunity Title Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "61C77239-8954-4826-8C18-E1C822C540E7", @"True" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Date Range Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "A49B230A-C23D-442A-B3A6-7DCFD3F4DACE", @"All||||" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Registration Page Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "98CE1B2F-F478-443E-9217-E864B3799D79", @"f7ca6e0f-c319-47ab-9a6d-247c5716d846" );
            // Attrib Value for Block:Fundraising Opportunity View, Attribute:Max Occurrences Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C", "4FAB5601-E142-456A-A081-3E1D05694FA5", @"100" );

            /** Migration Rollups **/

            // NA: ufnCrm_GetAddress
            Sql( @"/*
<doc>
	<summary>
 		This function returns the address of the person provided.
	</summary>


	<returns>
		Address of the person.
	</returns>
	<remarks>
		This function allows you to request an address for a specific person. It will return
		the first address of that type (multiple address are possible if the individual is in
		multiple families). 
		
		You can provide the address type by specifing 'Home', 'Previous', 
		'Work'. For custom address types provide the AddressTypeId like '19'.


		You can also determine which component of the address you'd like. Values include:
			+ 'Full' - the full address 
			+ 'Street1'
			+ 'Street2'
			+ 'City'
			+ 'State'
			+ 'PostalCode'
			+ 'Country'
			+ 'Latitude'
			+ 'Longitude'


	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Full')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street1')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street2')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'City')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'State')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'PostalCode')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Country')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Latitude')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Longitude')
	</code>
</doc>
*/


ALTER FUNCTION[dbo].[ufnCrm_GetAddress](
    @PersonId int,
    @AddressType varchar( 20 ),
    @AddressComponent varchar( 20 ) )


RETURNS nvarchar( 250 ) AS


BEGIN

    DECLARE @AddressTypeId int


    -- get address type

    IF( @AddressType = 'Home' )

        BEGIN

        SET @AddressTypeId = 19

        END

    ELSE IF( @AddressType = 'Work' )

        BEGIN

        SET @AddressTypeId = 20

        END

    ELSE IF( @AddressType = 'Previous' )

        BEGIN

        SET @AddressTypeId = 137

        END

    ELSE

        SET @AddressTypeId = CAST( @AddressType AS int )


    -- return address component

    IF( @AddressComponent = 'Street1' )

        BEGIN

        RETURN( SELECT[Street1] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'Street2' )

        BEGIN

        RETURN( SELECT[Street2] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'City' )

        BEGIN

        RETURN( SELECT[City] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'State' )

        BEGIN

        RETURN( SELECT[State] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'PostalCode' )

        BEGIN

        RETURN( SELECT[PostalCode] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'Country' )

        BEGIN

        RETURN( SELECT[Country] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'Latitude' )

        BEGIN

        RETURN( SELECT[GeoPoint].[Lat] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE IF( @AddressComponent = 'Longitude' )

        BEGIN

        RETURN( SELECT[GeoPoint].[Long] FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10 ))) 
		END
    ELSE

        BEGIN

        RETURN( SELECT ISNULL([Street1], '' ) + ' ' + ISNULL([Street2], '') +' ' + ISNULL([City], '') +', ' + ISNULL([State], '') +' ' + ISNULL([PostalCode], '') FROM[Location] WHERE[Id] = ( SELECT TOP 1[LocationId] FROM[GroupLocation] WHERE[GroupLocationTypeValueId] = @AddressTypeId AND[GroupId] = ( SELECT TOP 1[GroupId] FROM[GroupMember] gm INNER JOIN[Group] g ON g.[Id] = gm.[GroupId] WHERE[PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END


    RETURN ''
END" );


            // JE: Active Registration Instance Block Setting
            RockMigrationHelper.UpdateBlockType( "Registration Instance Active List", "Block to display active Registration Instances.", "~/Blocks/Event/RegistrationInstanceActiveList.ascx", "Event", "CFE8CAFA-587B-4EF2-A457-18047AC6BA39" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "CFE8CAFA-587B-4EF2-A457-18047AC6BA39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "3EE06828-E47B-4DA6-8136-C7D808EBC1CF" );
            RockMigrationHelper.AddBlockAttributeValue( "682AC7FB-84ED-4F6F-866C-60C3A2E92AAE", "3EE06828-E47B-4DA6-8136-C7D808EBC1CF", @"844dc54b-daec-47b3-a63a-712dd6d57793,564a51f5-da47-35af-4278-e8c810fa8d25" ); // Detail Page  

            // MP: Fix TransactionLinks on Business using caching
            // fix business detail TranactionLinks so it doesn't use Caching
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // MP: Add Marital Statuses
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Divorced", "Used when the individual is divorced.", "3B689240-24C2-434B-A7B9-A4A6CBA7928C" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteByGuid( "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", "NoteType" );

            RockMigrationHelper.DeleteAttribute( "F3338652-D1A2-4778-82A7-D56B9F4CFD7F" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Title
            RockMigrationHelper.DeleteAttribute( "237463F7-A206-4B43-AFDD-84E422527E87" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Date Range
            RockMigrationHelper.DeleteAttribute( "2339847F-2746-41D9-8CB5-2410FC8358D2" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Location
            RockMigrationHelper.DeleteAttribute( "697FDCF1-CA91-4DB5-9306-CD4835108613" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Summary
            RockMigrationHelper.DeleteAttribute( "125F7AAC-F01D-4527-AA5E-5C8345AC3F66" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Photo
            RockMigrationHelper.DeleteAttribute( "1E2F1416-2C4C-44DF-BE19-7D8FA9523115" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Details
            RockMigrationHelper.DeleteAttribute( "7CD834F8-43F2-400E-A352-898030124102" );    // GroupType - Group Attribute, Fundraising Opportunity: Individual Fundraising Goal
            RockMigrationHelper.DeleteAttribute( "F0846135-1A61-4AFA-8F9B-76D9821084DE" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Type
            RockMigrationHelper.DeleteAttribute( "6756D396-97F8-48A0-B69C-279E561F9D48" );    // GroupType - Group Attribute, Fundraising Opportunity: Update Content Channel
            RockMigrationHelper.DeleteAttribute( "38E1065D-4F6A-428E-B781-48F6BDACA614" );    // GroupType - Group Attribute, Fundraising Opportunity: Enable Commenting
            RockMigrationHelper.DeleteAttribute( "E06EBFAD-E0B1-4AE2-B9B1-4C988EFFA844" );    // GroupType - Group Attribute, Fundraising Opportunity: Registration Instance
            RockMigrationHelper.DeleteAttribute( "9BEA4F1C-E2FD-4669-B2CD-1269D4DCB97A" );    // GroupType - Group Attribute, Fundraising Opportunity: Allow Individual Disabling of Contribution Requests
            RockMigrationHelper.DeleteAttribute( "49012757-0ADE-419A-981C-384417D2E543" );    // GroupType - Group Attribute, Fundraising Opportunity: Cap Fundraising Amount
            RockMigrationHelper.DeleteAttribute( "7C6FF01B-F68E-4A83-A96D-85071A92AAF1" );    // GroupType - Group Attribute, Fundraising Opportunity: Financial Account
            RockMigrationHelper.DeleteAttribute( "BBD6C818-765C-43FB-AA72-5AF66F91B499" );    // GroupType - Group Attribute, Fundraising Opportunity: Show Public
            RockMigrationHelper.DeleteAttribute( "EABAE672-0886-450B-9296-2BADC56A0137" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Individual Fundraising Goal
            RockMigrationHelper.DeleteAttribute( "018B201C-D9C2-4EDE-9FC9-B52E2F799325" );    // GroupType - Group Member Attribute, Fundraising Opportunity: PersonalOpportunityIntroduction
            RockMigrationHelper.DeleteAttribute( "2805298E-E21A-4679-B5CA-69D6FF4EAD31" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Disable Public Contribution Requests

            RockMigrationHelper.DeleteGroupTypeRole( "F82DF077-9664-4DA8-A3D9-7379B690124D" );
            RockMigrationHelper.DeleteGroupTypeRole( "253973A5-18F2-49B6-B2F1-F8F84294AAB2" );

            RockMigrationHelper.DeleteGroupType( "4BE7FC44-332D-40A8-978E-47B7035D7A0C" );

            RockMigrationHelper.DeleteDefinedType( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D" );

            RockMigrationHelper.DeleteCategory( "91B43FBD-F924-4934-9CCE-7990513275CF" );


            //// Down() for Fundraising Blocks/Pages migration
            // Attrib for BlockType: Fundraising Leader Toolbox:Participant Page
            RockMigrationHelper.DeleteAttribute( "A2C8F514-8805-4E0A-9493-75289F543B43" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Main Page
            RockMigrationHelper.DeleteAttribute( "E58C0A8D-FE41-4FE0-B5F4-F7E8B3934D7A" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Summary Lava Template
            RockMigrationHelper.DeleteAttribute( "6F245AB0-5EEC-4EF9-A029-6C6BFB0ED64B" );
            // Attrib for BlockType: Transaction Entry:Transaction Entity Type
            RockMigrationHelper.DeleteAttribute( "4F712013-75C7-4157-8EF4-2EF26210378B" );
            // Attrib for BlockType: Transaction Entry:Enable Initial Back button
            RockMigrationHelper.DeleteAttribute( "86A2A716-3F48-4AA1-8B18-E2BB47C8FD40" );
            // Attrib for BlockType: Transaction Entry:Transaction Header
            RockMigrationHelper.DeleteAttribute( "65FB0B9A-670E-4AB9-9666-77959B4B702E" );
            // Attrib for BlockType: Transaction Entry:Entity Id Param
            RockMigrationHelper.DeleteAttribute( "8E45ABBB-43A8-46B1-A32C-DB9474A65BE0" );
            // Attrib for BlockType: Transaction Entry:Transaction Type
            RockMigrationHelper.DeleteAttribute( "ADB22E3F-1DC0-4BA6-AC77-09FE8580CD21" );
            // Attrib for BlockType: Transaction Entry:Allowed Transaction Attributes From URL
            RockMigrationHelper.DeleteAttribute( "B4C8AA1A-E43E-48F1-9221-C83F9E750352" );
            // Attrib for BlockType: Transaction Entry:Account Campus Context
            RockMigrationHelper.DeleteAttribute( "0440B425-57B2-4E65-84C8-5B05D9A46708" );
            // Attrib for BlockType: Transaction Entry:Invalid Account Message
            RockMigrationHelper.DeleteAttribute( "00106DDE-CD23-4E4B-A4B6-B3819E196364" );
            // Attrib for BlockType: Transaction Entry:Only Public Accounts In URL
            RockMigrationHelper.DeleteAttribute( "A13AC34C-4790-430F-8182-43AAC01FF177" );
            // Attrib for BlockType: Transaction Entry:Allow Accounts In URL
            RockMigrationHelper.DeleteAttribute( "E4492D68-45EA-41FF-A611-760DB13EC36E" );
            // Attrib for BlockType: Transaction Entry:Account Header Template
            RockMigrationHelper.DeleteAttribute( "F71BD118-F1EB-4E93-AD7B-86D2A40AAE95" );
            // Attrib for BlockType: Fundraising Donation Entry:Show First Name Only
            RockMigrationHelper.DeleteAttribute( "AF583D88-2DCA-4589-AF53-CBE61295C02E" );
            // Attrib for BlockType: Fundraising Donation Entry:Transaction Entry Page
            RockMigrationHelper.DeleteAttribute( "4E9D70B9-9CF6-4F6C-87B4-8B0DDFDFEB3E" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Donation Page
            RockMigrationHelper.DeleteAttribute( "5A0D8B2E-8692-481D-BD1E-48236021BFF0" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Note Type
            RockMigrationHelper.DeleteAttribute( "C3494517-31E3-4B04-AE37-570331073903" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Updates Lava Template
            RockMigrationHelper.DeleteAttribute( "17DF7E42-B2D7-4E5D-9EF7-EE25758139FC" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Main Page
            RockMigrationHelper.DeleteAttribute( "592C88ED-6993-4292-96FA-C05CB8A6F00C" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Profile Lava Template
            RockMigrationHelper.DeleteAttribute( "84C3DD64-436E-40BC-ADC3-7F86BBB890C0" );
            // Attrib for BlockType: Fundraising Opportunity View:Updates Lava Template
            RockMigrationHelper.DeleteAttribute( "AFC2C61D-87F8-4C4E-9CC2-98F2009A500C" );
            // Attrib for BlockType: Fundraising Opportunity View:Sidebar Lava Template
            RockMigrationHelper.DeleteAttribute( "393030EB-18B6-4D91-943F-BAB3853B84BD" );
            // Attrib for BlockType: Fundraising Opportunity View:Summary Lava Template
            RockMigrationHelper.DeleteAttribute( "B802BE78-42DE-4E0F-8C1E-5788582C905B" );
            // Attrib for BlockType: Fundraising Opportunity View:Donation Page
            RockMigrationHelper.DeleteAttribute( "685DA61C-AF28-4389-AA7F-4BC26BED6CDD" );
            // Attrib for BlockType: Fundraising Opportunity View:Participant Page
            RockMigrationHelper.DeleteAttribute( "EF966837-E420-45B9-A740-F1E43C08469D" );
            // Attrib for BlockType: Fundraising Opportunity View:Note Type
            RockMigrationHelper.DeleteAttribute( "287571CE-B731-477A-B948-FD05736C2CFE" );
            // Attrib for BlockType: Fundraising Opportunity View:Leader Toolbox Page
            RockMigrationHelper.DeleteAttribute( "9F8F6E06-338F-403E-9D2D-A4FCBA13A844" );
            // Attrib for BlockType: Fundraising List:Lava Template
            RockMigrationHelper.DeleteAttribute( "ED2EA497-4316-4E44-A5A4-69E69CC7ECBC" );
            // Attrib for BlockType: Fundraising List:Details Page
            RockMigrationHelper.DeleteAttribute( "F17BD62D-8134-47A5-BDBC-F7F6CD07974E" );

            // Attrib for BlockType: Transaction Entity Matching:EntityTypeQualifierValue
            RockMigrationHelper.DeleteAttribute( "9488B744-D932-4CB9-AEEC-EEF54573DB8B" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeQualifierColumn
            RockMigrationHelper.DeleteAttribute( "A9BB04F1-7C63-4ABF-A174-D93003B2833F" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeId
            RockMigrationHelper.DeleteAttribute( "9FDA9232-AF40-4471-B371-FDB480F7F2CF" );
            // Attrib for BlockType: Transaction Entity Matching:TransactionTypeId
            RockMigrationHelper.DeleteAttribute( "3F6A0F3A-EEC9-4DD8-B0CB-3030103D3422" );

            // Remove Block: Fundraising Leader Toolbox, from Page: Fundraising Leader Toolbox, Site: External Website
            RockMigrationHelper.DeleteBlock( "558375A3-2DFF-43F3-A9EF-04F503C7EB55" );
            // Remove Block: Fundraising Transaction Entry, from Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.DeleteBlock( "1BAD904E-2F79-4488-B8BE-EECD67AE2925" );
            // Remove Block: Fundraising Donation Entry, from Page: Fundraising Donation Entry, Site: External Website
            RockMigrationHelper.DeleteBlock( "B557FC47-0D19-4EED-A386-04CF569E5967" );
            // Remove Block: Fundraising Opportunity Participant, from Page: Fundraising Participant, Site: External Website
            RockMigrationHelper.DeleteBlock( "BAF6AD44-BFBB-46AE-B1F2-89511C273FAE" );
            // Remove Block: Fundraising Opportunity View, from Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.DeleteBlock( "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C" );
            // Remove Block: Fundraising List, from Page: Missions, Site: External Website
            RockMigrationHelper.DeleteBlock( "7759CDA6-BCE0-42D9-99C8-E991600F7E0D" );
            // Remove Block: Fundraising Transaction Group Member Matching, from Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "85B35E05-BAD4-44F1-8E81-EF77959F199B" );

            RockMigrationHelper.DeleteBlockType( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1" ); // Fundraising Opportunity Participant
            RockMigrationHelper.DeleteBlockType( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241" ); // Fundraising Opportunity View
            RockMigrationHelper.DeleteBlockType( "E664BB02-D501-40B0-AAD6-D8FA0E63438B" ); // Fundraising List
            RockMigrationHelper.DeleteBlockType( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3" ); // Fundraising Leader Toolbox
            RockMigrationHelper.DeleteBlockType( "A24D68F2-C58B-4322-AED8-6556DBED1B76" ); // Fundraising Donation Entry
            RockMigrationHelper.DeleteBlockType( "A58BCB1E-01D9-4F60-B925-D831A9537051" ); // Transaction Entity Matching
            RockMigrationHelper.DeletePage( "F04D69C1-786A-4204-8A67-5669BDFEB533" ); //  Page: Fundraising Transaction Entry, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "9F76591C-CEE4-4824-8478-E3BDA48D66ED" ); //  Page: Fundraising Participant, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "E40BEA3D-0304-4AD2-A45D-9BAD9852E3BA" ); //  Page: Fundraising Donation Entry, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "9DADC93F-C9E7-4567-B73E-AD264A93E37D" ); //  Page: Fundraising Leader Toolbox, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "BA673ABE-A45A-4835-A3A0-94A60341B96F" ); //  Page: Fundraising Opportunity View, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD" ); //  Page: Fundraising Transaction Matching, Layout: Full Width, Site: Rock RMS
        }
    }
}
