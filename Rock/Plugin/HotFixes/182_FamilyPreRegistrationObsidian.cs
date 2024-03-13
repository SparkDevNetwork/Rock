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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 182, "1.15.1" )]
    public class FamilyPreRegistrationObsidian : Migration
    {
        
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddFamilyPreRegistrationObsidianBlock();
            AddFamilyPreRegistrationObsidianBlockAttributes();
            ReplaceWebFormFamilyPreRegistrationWithObsidianFamilyPreRegistration();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        private void AddFamilyPreRegistrationObsidianBlockAttributes() {

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Planned Visit Information Panel Title              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Planned Visit Information Panel Title", "PlannedVisitInformationPanelTitle", "Planned Visit Information Panel Title", @"The title for the Planned Visit Information panel", 17, @"Visit Information", "1B13DB8D-EF44-4678-A688-FD42084251C6" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Create Account Title              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Create Account Title", "CreateAccountTitle", "Create Account Title", @"Configures the description for the create account card.", 11, @"Create Account", "D862D05C-DE4E-4172-92E6-B2F6F60C5794" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Create Account Description              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Create Account Description", "CreateAccountDescription", "Create Account Description", @"Allows the first adult to create an account for themselves.", 12, @"Create an account to personalize your experience and access additional capabilities on our site.", "4B182A44-886A-431E-8F69-77924D8B813A" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Relationship Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Relationship Types", "Relationships", "Relationship Types", @"The possible child-to-adult relationships. The value 'Child' will always be included.", 0, @"0", "F5333493-76E4-4D05-80E3-758316F985F9" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Same Immediate Family Relationships              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Same Immediate Family Relationships", "FamilyRelationships", "Same Immediate Family Relationships", @"The relationships which indicate the child is in the same immediate family as the adult(s) rather than creating a new family for the child. In most cases, 'Child' will be the only value included in this list. Any values included in this list that are not in the Relationship Types list will be ignored.", 1, @"0", "D5D816FF-3ABF-4BE4-8046-74F6D742AD70" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Can Check-in Relationship              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Can Check-in Relationship", "CanCheckinRelationships", "Can Check-in Relationship", @"Any relationships that, if selected, will also create an additional 'Can Check-in' relationship between the adult and the child. This is only necessary if the relationship (selected by the user) does not have the 'Allow Check-in' option.", 2, @"", "7F217F6E-CFAC-4006-8CEC-3250828E5013" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Show Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 0, @"True", "104270CC-A571-4133-A745-9B11338B325A" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Require Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected", 2, @"True", "8FEE586C-116C-4685-8073-DCED606A2B8F" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Allow Updates              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Updates", "AllowUpdates", "Allow Updates", @"If the person visiting this block is logged in, should the block be used to update their family? If not, a new family will always be created unless 'Auto Match' is enabled and the information entered matches an existing person.", 9, @"False", "5BC01595-753D-476B-8D38-EB544D705ED3" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Auto Match              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Match", "AutoMatch", "Auto Match", @"Should this block attempt to match people to current records in the database?", 10, @"True", "996D3A77-8FF6-4957-9B6F-BE18762B75C3" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Planned Visit Date              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Planned Visit Date", "PlannedVisitDate", "Planned Visit Date", @"How should the Planned Visit Date field be displayed. The date selected by the user is only used for the workflow. If the 'Campus Schedule Attribute' block setting has a selection this will control if schedule date/time are required or not but not if it shows or not. The Lava merge field for this in workflows is 'PlannedVisitDate'.", 6, @"Optional", "5ED06816-2200-4D20-BA4E-AD558A5D155B" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Show SMS Opt-in              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show SMS Opt-in", "DisplaySmsOptIn", "Show SMS Opt-in", @"If 'Mobile Phone' is not set to 'Hide' then this option will show the SMS Opt-In option for the selection.", 18, @"Hide", "EBD8E47A-9E0F-4918-8150-780D50963F4C" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Suffix              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Suffix", "AdultSuffix", "Suffix", @"How should Suffix be displayed for adults?", 0, @"Hide", "F3FAC7F8-8967-4C56-9C10-317CD20D1561" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Gender              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "AdultGender", "Gender", @"How should Gender be displayed for adults?", 1, @"Optional", "231E41F5-D29F-4DE8-ACAD-EFDD98B029FB" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Birth Date              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Birth Date", "AdultBirthdate", "Birth Date", @"How should Birth Date be displayed for adults?", 2, @"Optional", "0155CB9E-6447-4C3B-A6DA-9709F930A9C3" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Marital Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Marital Status", "AdultMaritalStatus", "Marital Status", @"How should Marital Status be displayed for adults?", 3, @"Required", "6E401464-0416-4989-9532-FF78AD076A5A" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Email              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email", "AdultEmail", "Email", @"How should Email be displayed for adults?", 4, @"Required", "23B2B7FE-C2C6-4385-BEF7-0B3188EE3265" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Mobile Phone              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "AdultMobilePhone", "Mobile Phone", @"How should Mobile Phone be displayed for adults?", 5, @"Required", "229EE5B1-A92A-405A-A11C-F312C700D77F" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Display Communication Preference              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Communication Preference", "AdultDisplayCommunicationPreference", "Display Communication Preference", @"How should Communication Preference be displayed for adults?", 7, @"Hide", "F8556A9B-4D4D-4908-8B23-C45FA62ADAF9" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Address              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Address", "AdultAddress", "Address", @"How should Address be displayed for adults?", 8, @"Optional", "11914030-CBB9-42EC-9F31-4DE43D156258" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Profile Photos              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Profile Photos", "AdultProfilePhoto", "Profile Photos", @"How should Profile Photo be displayed for adults?", 9, @"Hide", "95F9CB84-21CC-4B19-A826-CC1EB53F2AF3" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: First Adult Create Account              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "First Adult Create Account", "FirstAdultCreateAccount", "First Adult Create Account", @"Allows the first adult to create an account for themselves.", 10, @"Hide", "BFD448A6-5B5B-44A6-9762-4C8C3C9958AA" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Race              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "RaceOption", "Race", @"Allow Race to be optionally selected.", 13, @"Hide", "87E5368D-0419-4517-9AF5-7FD2FB4267DB" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Ethnicity              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "EthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 14, @"Hide", "027CED04-DC7A-46B2-A8F8-ABB143DEF2B8" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Suffix              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Suffix", "ChildSuffix", "Suffix", @"How should Suffix be displayed for children?", 0, @"Hide", "8CEDE3CF-6141-44B1-8C4C-A71D44EB8DBD" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Gender              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "ChildGender", "Gender", @"How should Gender be displayed for children?", 1, @"Optional", "02AF099C-9672-455D-BA91-1D08235AAA3B" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Birth Date              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Birth Date", "ChildBirthdate", "Birth Date", @"How should Birth Date be displayed for children?", 2, @"Required", "AE63CB9B-0616-4305-A715-2507BB0EB071" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Grade              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Grade", "ChildGrade", "Grade", @"How should Grade be displayed for children?", 3, @"Optional", "852B5984-BD95-4288-A078-9C01065A9DF8" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Email              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email", "ChildEmail", "Email", @"How should Email be displayed for children?  Be sure to seek legal guidance when collecting email addresses on minors.", 4, @"Hide", "DF6FACAC-83E6-4DBC-A2CB-593F63792065" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Mobile Phone              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "ChildMobilePhone", "Mobile Phone", @"How should Mobile Phone be displayed for children?", 5, @"Hide", "DF937BE2-8946-48C7-BC7F-D78270E93CA2" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Display Communication Preference              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Communication Preference", "ChildDisplayCommunicationPreference", "Display Communication Preference", @"How should Communication Preference be displayed for children?", 7, @"Hide", "6C83437A-E8C6-4452-980C-BC0EB301D7F2" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Profile Photos              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Profile Photos", "ChildProfilePhoto", "Profile Photos", @"How should Profile Photo be displayed for children?", 8, @"Hide", "03167683-A694-42B2-A90C-6DF72ECAC9DA" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Race              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "ChildRaceOption", "Race", @"Allow Race to be optionally selected.", 9, @"Hide", "79A77E89-5295-4357-A0F3-72C0079FBC9E" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Ethnicity              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "ChildEthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 10, @"Hide", "65721A25-B667-45B8-919C-FAC421D18FC8" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Scheduled Days Ahead              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Scheduled Days Ahead", "ScheduledDaysAhead", "Scheduled Days Ahead", @"When using campus specific scheduling this setting determines how many days ahead a person can select. The default is 28 days.", 7, @"28", "C7015F0C-2E28-46AF-AB53-BA8D3BCFF28F" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Campus Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 3, @"", "32A3FC54-C30C-495C-9375-FF42F20D863C" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Campus Statuses              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 4, @"", "A9E7882B-22A1-4F98-8DA8-8F8E479E5AE5" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Connection Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status that should be used when adding new people.", 11, @"B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "5F49FB4E-D0E2-4119-8C3E-51234D625689" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Record Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status that should be used when adding new people.", 12, @"618F906C-C33D-4FA3-8AEF-E58CB7B63F1E", "0C2AB655-8C59-4158-8873-CF3C35EB69A0" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Default Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Default Campus", "DefaultCampus", "Default Campus", @"An optional campus to use by default when adding a new family.", 1, @"", "B75D0768-AF2F-49EE-B74D-11A9F9DF3E04" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Parent Workflow              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Parent Workflow", "ParentWorkflow", "Parent Workflow", @"If set, this workflow type will launch for each parent provided. The parent will be passed to the workflow as the Entity.", 14, @"", "41128993-C459-473C-93FC-BAD2A184A396" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Child Workflow              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Child Workflow", "ChildWorkflow", "Child Workflow", @"If set, this workflow type will launch for each child provided. The child will be passed to the workflow as the Entity.", 15, @"", "39C38561-1941-4913-B4E8-A89C6816569F" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Attribute Categories              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "AdultAttributeCategories", "Attribute Categories", @"The adult Attribute Categories to display attributes from.", 6, @"", "3BF600C6-A4AC-468C-839A-1F97159D79C9" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Attribute Categories              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "ChildAttributeCategories", "Attribute Categories", @"The children Attribute Categories to display attributes from.", 6, @"", "0951B6E0-F562-43D6-8CB0-1453761C4FDF" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Redirect URL              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Redirect URL", "RedirectURL", "Redirect URL", @"The URL to redirect user to when they have completed the registration. The merge fields that are available includes 'Family', which is an object for the primary family that is created/updated; 'RelatedChildren', which is a list of the children who have a relationship with the family, but are not in the family; 'ParentIds' which is a comma-delimited list of the person ids for each adult; 'ChildIds' which is a comma-delimited list of the person ids for each child; and 'PlannedVisitDate' which is the value entered for the Planned Visit Date field if it was displayed.", 16, @"/FamilyPreRegistrationSuccess?FamilyId={{ Family.Id }}&Parents={{ ParentIds }}&Children={{ ChildIds }}&When={{ PlannedVisitDate }}", "A11E86ED-481A-4F6F-BC44-58D064ECDCA5" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Workflow Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Types", "WorkflowTypes", "Workflow Types", @"The workflow type(s) to launch when a family is added. The primary family will be passed to each workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: ParentIds, ChildIds, PlannedVisitDate.", 13, @"", "F9367D5A-1EFB-4029-BAAD-87E980AAE53B" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Campus Schedule Attribute              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Campus Schedule Attribute", "CampusScheduleAttribute", "Campus Schedule Attribute", @"Allows you to select a campus attribute that contains schedules for determining which dates and times for which pre-registration is available. This requires the creation of an Entity attribute for 'Campus' using a Field Type of 'Schedules'. The schedules can then be selected in the 'Edit Campus' block. The Lava merge field for this in workflows is 'ScheduleId'.", 5, @"", "AECAEED0-3638-4756-82FE-A1593DBAE9A9" );

            // Attribute for BlockType              
            //   BlockType: Family Pre Registration              
            //   Category: Obsidian > CRM              
            //   Attribute: Family Attributes              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Family Attributes", "FamilyAttributes", "Family Attributes", @"The Family attributes that should be displayed", 8, @"", "DC8CFB0F-63C5-4876-8E65-5E650A03C9CC" );
        }

        /// <summary>
        /// GJ: Fix Breadcrumb Display
        /// </summary>
        private void AddFamilyPreRegistrationObsidianBlock()
        {
            // Add the FamilyPreRegistration Obsidian Block EntityType
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.FamilyPreRegistration", "Family Pre Registration", "Rock.Blocks.Crm.FamilyPreRegistration, Rock.Blocks, Version=1.15.1.1, Culture=neutral, PublicKeyToken=null", false, false, "C03CE9ED-8572-4BE5-AB2A-FF7498494905" );

            // Add/Update Obsidian Block Type
            //   Name: Family Pre Registration
            //   Category: Obsidian > CRM
            //   EntityType:Rock.Blocks.Crm.FamilyPreRegistration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Family Pre Registration", "Provides a way to allow people to pre-register their families for weekend check-in.", "Rock.Blocks.Crm.FamilyPreRegistration", "Obsidian > CRM", "1D6794F5-876B-47B9-9C9B-5C2C2CC81074" );
        }

        private void ReplaceWebFormFamilyPreRegistrationWithObsidianFamilyPreRegistration()
        {
            // Webforms 463A454A-6370-4B4A-BCA1-415F2D9B0CB7
            // Obsidian 1D6794F5-876B-47B9-9C9B-5C2C2CC81074

            var FullyQualifiedJobClassName = "Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks";

            // Configure run-once job by modifying these variables.
            var commandTimeout = 14000;
            var blockTypeReplacements = new Dictionary<string, string>
            {
                { "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1D6794F5-876B-47B9-9C9B-5C2C2CC81074" }
            };

            Sql( $@"
                IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{FullyQualifiedJobClassName}' AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}' )
                BEGIN
                    INSERT INTO [ServiceJob] (
                          [IsSystem]
                        , [IsActive]
                        , [Name]
                        , [Description]
                        , [Class]
                        , [CronExpression]
                        , [NotificationStatus]
                        , [Guid] )
                    VALUES ( 
                          0
                        , 1
                        , 'Rock Update Helper v15.2 - Replace WebForms Blocks with Obsidian Blocks'
                        , 'This job will replace WebForms blocks with their Obsidian blocks on all sites, pages, and layouts.'
                        , '{FullyQualifiedJobClassName}'
                        , '0 0 21 1/1 * ? *'
                        , 1
                        , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}');
                END" );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Command Timeout
            var commandTimeoutAttributeGuid = "4F15F5A5-F83C-4CA6-B7F9-9DCDAC0B4DEF";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.INTEGER, "Class", FullyQualifiedJobClassName, "Command Timeout", "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.", 0, "14000", commandTimeoutAttributeGuid, "CommandTimeout" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, commandTimeoutAttributeGuid, commandTimeout.ToString() );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Block Type Guid Replacement Pairs
            var blockTypeReplacementsAttributeGuid = "CDDB8075-E559-499F-B12F-B8DC8CCD73B5";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", FullyQualifiedJobClassName, "Block Type Guid Replacement Pairs", "Block Type Guid Replacement Pairs", "The key-value pairs of replacement BlockType.Guid values, where the key is the existing BlockType.Guid and the value is the new BlockType.Guid. Blocks of BlockType.Guid == key will be replaced by blocks of BlockType.Guid == value in all sites, pages, and layouts.", 1, "", blockTypeReplacementsAttributeGuid, "BlockTypeGuidReplacementPairs" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, blockTypeReplacementsAttributeGuid, SerializeDictionary( blockTypeReplacements ) );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Migration Strategy
            var migrationStrategyAttributeGuid = "FA99E828-2388-4CDF-B69B-DBC36332D6A4";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.SINGLE_SELECT, "Class", FullyQualifiedJobClassName, "Migration Strategy", "Migration Strategy", "Determines if the blocks should be chopped instead of swapped. By default,the old blocks are swapped with the new ones.", 2, "Swap", migrationStrategyAttributeGuid, "MigrationStrategy" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, migrationStrategyAttributeGuid, "Chop" );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Should Keep Old Blocks
            var shouldKeepOldBlocksAttributeGuid = "768BDB5D-BA09-4246-AA25-F1C37097CC7D";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.BOOLEAN, "Class", FullyQualifiedJobClassName, "Should Keep Old Blocks", "Should Keep Old Blocks", "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted.", 3, "False", shouldKeepOldBlocksAttributeGuid, "ShouldKeepOldBlocks" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, shouldKeepOldBlocksAttributeGuid, "False" );
        }

        private string SerializeDictionary( Dictionary<string, string> dictionary )
        {
            const string keyValueSeparator = "^";

            if ( dictionary?.Any() != true )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var first = dictionary.First();
            sb.Append( $"{first.Key}{keyValueSeparator}{first.Value}" );

            foreach ( var kvp in dictionary.Skip( 1 ) )
            {
                sb.Append( $"|{kvp.Key}{keyValueSeparator}{kvp.Value}" );
            }

            return sb.ToString();
        }
    }
}
