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
            RockMigrationHelper.UpdateGroupAttributeCategory ( "Fundraising Public", "fa fa-money", "", "91B43FBD-F924-4934-9CCE-7990513275CF" );

            // 'Fundraising Opportunity Term' Defined Values
            RockMigrationHelper.AddDefinedType( "Group", "Fundraising Opportunity Term", "This is what a fundraising opportunity is described as, such as Trip, Internship, Project, etc", "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D" );
            RockMigrationHelper.AddDefinedValue( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Trip", "", "3BB5607B-8A77-434D-8AEF-F10D513BE963", false );
            RockMigrationHelper.AddDefinedValue( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Internship", "", "DB378B20-525E-40E6-B7E3-80ACBD2AE8A0", false );
            RockMigrationHelper.AddDefinedValue( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Project", "", "DFF45DA6-6077-4651-A804-BCBE9CD68375", false );

            // GroupType: Fundraising Opportunity 4BE7FC44-332D-40A8-978E-47B7035D7A0C
            RockMigrationHelper.AddGroupType( "Fundraising Opportunity", "A group that can be used to manage a fundraising opportunity such as a mission trip or internship", "Group", "Member", false, true, true, "fa fa-money", 0, null, 0, null, "4BE7FC44-332D-40A8-978E-47B7035D7A0C" );

            // Group Type Group Attributes
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Opportunity Title", @"The public name of the fundraising opportunity so the group name could be used for internal 
use.", 0, "", "F3338652-D1A2-4778-82A7-D56B9F4CFD7F" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "9C7D431C-875C-4792-9E76-93F3A32BB850", "Opportunity Date Range", @"Used to display start and end date", 1, "", "237463F7-A206-4B43-AFDD-84E422527E87" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Opportunity Location", @"The location description of the opportunity such as the city or country.", 2, "", "2339847F-2746-41D9-8CB5-2410FC8358D2" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Opportunity Summary", @"", 3, "", "697FDCF1-CA91-4DB5-9306-CD4835108613" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Opportunity Photo", @"", 4, "", "125F7AAC-F01D-4527-AA5E-5C8345AC3F66" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Opportunity Details", @"", 5, "", "1E2F1416-2C4C-44DF-BE19-7D8FA9523115" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "3EE69CBC-35CE-4496-88CC-8327A447603F", "Individual Fundraising Goal", @"The default individual fundraising goal.", 6, "", "7CD834F8-43F2-400E-A352-898030124102" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Opportunity Term", @"What the opportunity is described as, such as Trip, Internship, Project,Etc", 7, "", "F0846135-1A61-4AFA-8F9B-76D9821084DE" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Update Content Channel", @"The content channel to use to display any blog-like updates for the fundraising opportunity", 8, "", "6756D396-97F8-48A0-B69C-279E561F9D48" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Commenting", @"Helps to determine if commenting is allowed (default is no commenting).", 9, "False", "38E1065D-4F6A-428E-B781-48F6BDACA614" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Registration Instance", @"The Id of the registration instance (if any) that is associated with this fundraising opportunity", 10, "", "E06EBFAD-E0B1-4AE2-B9B1-4C988EFFA844" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Individual Disabling of Contribution Requests", @"Determines if individuals should be allowed to disable their contribution requests.", 11, "True", "9BEA4F1C-E2FD-4669-B2CD-1269D4DCB97A" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Cap Fundraising Amount", @"If this is set to 'Yes', the individual won't be able to fundraise for more than the Individual Fundraising Goal amount", 12, "False", "49012757-0ADE-419A-981C-384417D2E543" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Financial Account", @"The financial account that the donations should be tied to.", 13, "", "7C6FF01B-F68E-4A83-A96D-85071A92AAF1" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "3EE69CBC-35CE-4496-88CC-8327A447603F", "Individual Fundraising Goal", @"Optional override of the default individual fund raising goal.  This is configurable only in internal group member editor. An individual could not adjust this themselves.", 0, "", "EABAE672-0886-450B-9296-2BADC56A0137" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Personal Trip Introduction", @"A personal note to display on the individual's fundraising participant page.", 1, "", "018B201C-D9C2-4EDE-9FC9-B52E2F799325" );
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

-- Personal Trip Introduction
INSERT INTO AttributeCategory (AttributeId, CategoryId) SELECT a.Id, c.Id FROM Attribute a, Category c WHERE a.[Guid] = '018B201C-D9C2-4EDE-9FC9-B52E2F799325' AND c.[Guid] = '91B43FBD-F924-4934-9CCE-7990513275CF' AND NOT EXISTS (SELECT AttributeId, CategoryId FROM AttributeCategory ac WHERE AttributeId = a.Id AND CategoryId = c.Id)
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
            RockMigrationHelper.AddSecurityAuthForAttribute( "49012757-0ADE-419A-981C-384417D2E543", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "7C6FF01B-F68E-4A83-A96D-85071A92AAF1", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "2039F657-1CA0-4444-8FDA-C82F80CDD131", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "EABAE672-0886-450B-9296-2BADC56A0137", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "018B201C-D9C2-4EDE-9FC9-B52E2F799325", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( "2805298E-E21A-4679-B5CA-69D6FF4EAD31", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );

            RockMigrationHelper.AddGroupTypeRole( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "Participant", string.Empty, 1, null, null, "F82DF077-9664-4DA8-A3D9-7379B690124D", true, false, true );
            RockMigrationHelper.AddGroupTypeRole( "4BE7FC44-332D-40A8-978E-47B7035D7A0C", "Leader", string.Empty, 0, null, null, "253973A5-18F2-49B6-B2F1-F8F84294AAB2", true, true, false );


            // Add TransactionType of Fundraising 142EA7C8-04E5-4708-9E29-9C89127061C7
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Fundraising", "A Fundraising Donation Transaction", "142EA7C8-04E5-4708-9E29-9C89127061C7", true );

            // Add NoteType 'Fundraising Opportunity Comment' Guid:9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95
            // make sure AllUsers have EDIT auth (the block will control when Edit/Add is allowed)
            RockMigrationHelper.UpdateNoteType( "Fundraising Opportunity Comment", "Rock.Model.Group", "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", true );
            RockMigrationHelper.AddSecurityAuthForNoteType( "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", 0, Rock.Security.Authorization.VIEW, true, null, (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );

            //// TODO: Pages and Blocks migration
            // the TransactionEntry block for Fundraising will need a bunch of attributes set
            //  + Disable Business Giving
            //  + Turn off the 'Additional Accounts' option
            //  + Set the TransactionEntityType and EntityIdParam attributes to GroupMember, GroupMemberId
            //  + Set the various Lava Templates Options
            //  + (sweep for others)

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteByGuid( "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", "NoteType" );
            RockMigrationHelper.DeleteDefinedValue( "142EA7C8-04E5-4708-9E29-9C89127061C7" );

            RockMigrationHelper.DeleteAttribute( "F3338652-D1A2-4778-82A7-D56B9F4CFD7F" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Title
            RockMigrationHelper.DeleteAttribute( "237463F7-A206-4B43-AFDD-84E422527E87" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Date Range
            RockMigrationHelper.DeleteAttribute( "2339847F-2746-41D9-8CB5-2410FC8358D2" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Location
            RockMigrationHelper.DeleteAttribute( "697FDCF1-CA91-4DB5-9306-CD4835108613" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Summary
            RockMigrationHelper.DeleteAttribute( "125F7AAC-F01D-4527-AA5E-5C8345AC3F66" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Photo
            RockMigrationHelper.DeleteAttribute( "1E2F1416-2C4C-44DF-BE19-7D8FA9523115" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Details
            RockMigrationHelper.DeleteAttribute( "7CD834F8-43F2-400E-A352-898030124102" );    // GroupType - Group Attribute, Fundraising Opportunity: Individual Fundraising Goal
            RockMigrationHelper.DeleteAttribute( "F0846135-1A61-4AFA-8F9B-76D9821084DE" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Term
            RockMigrationHelper.DeleteAttribute( "6756D396-97F8-48A0-B69C-279E561F9D48" );    // GroupType - Group Attribute, Fundraising Opportunity: Update Content Channel
            RockMigrationHelper.DeleteAttribute( "38E1065D-4F6A-428E-B781-48F6BDACA614" );    // GroupType - Group Attribute, Fundraising Opportunity: Enable Commenting
            RockMigrationHelper.DeleteAttribute( "E06EBFAD-E0B1-4AE2-B9B1-4C988EFFA844" );    // GroupType - Group Attribute, Fundraising Opportunity: Registration Instance
            RockMigrationHelper.DeleteAttribute( "9BEA4F1C-E2FD-4669-B2CD-1269D4DCB97A" );    // GroupType - Group Attribute, Fundraising Opportunity: Allow Individual Disabling of Contribution Requests
            RockMigrationHelper.DeleteAttribute( "49012757-0ADE-419A-981C-384417D2E543" );    // GroupType - Group Attribute, Fundraising Opportunity: Cap Fundraising Amount
            RockMigrationHelper.DeleteAttribute( "7C6FF01B-F68E-4A83-A96D-85071A92AAF1" );    // GroupType - Group Attribute, Fundraising Opportunity: Financial Account
            RockMigrationHelper.DeleteAttribute( "2039F657-1CA0-4444-8FDA-C82F80CDD131" );    // GroupType - Group Attribute, Fundraising Opportunity: IS This secured
            RockMigrationHelper.DeleteAttribute( "EABAE672-0886-450B-9296-2BADC56A0137" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Individual Fundraising Goal
            RockMigrationHelper.DeleteAttribute( "018B201C-D9C2-4EDE-9FC9-B52E2F799325" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Personal Trip Introduction
            RockMigrationHelper.DeleteAttribute( "2805298E-E21A-4679-B5CA-69D6FF4EAD31" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Disable Public Contribution Requests

            RockMigrationHelper.DeleteGroupTypeRole( "F82DF077-9664-4DA8-A3D9-7379B690124D" );
            RockMigrationHelper.DeleteGroupTypeRole( "253973A5-18F2-49B6-B2F1-F8F84294AAB2" );

            RockMigrationHelper.DeleteGroupType( "4BE7FC44-332D-40A8-978E-47B7035D7A0C" );

            RockMigrationHelper.DeleteDefinedType( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D" );

            RockMigrationHelper.DeleteCategory( "91B43FBD-F924-4934-9CCE-7990513275CF" );
        }
    }
}
