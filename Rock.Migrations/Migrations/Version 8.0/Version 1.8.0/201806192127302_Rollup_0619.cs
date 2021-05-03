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
    public partial class Rollup_0619 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenBlocksAndPagesUp();
            NoteWatchNotificationEmail();
            NoteApprovalEmail();
            StarkMigration();
            UsePersonPageIcon();
            BadLavaInSystemEmail();
            EmailPreferenceText();
            FixPendingGroupMembersNotificationEmail();
            ForgotUserName();
            UnattendedCheckInLogging();
            MySettingsNavFix();
            HomepageIconsFix();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenBlocksAndPagesDown();
        }

        private void CodeGenBlocksAndPagesUp()
        {
            // Add Block to Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlock( true, "56F1DC05-3D7D-49B6-9A30-5CF271C687F4", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Content Channel Dynamic", "Main", @"", @"", 0, "0C828414-8AEF-4B43-AAEF-B200544A2197" );
            // Add Block to Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlock( true, "2D0D0FB0-68C4-47E1-8BC6-98F931497F5E", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Blog Details", "Main", @"", @"", 0, "70DCBB50-0978-4B24-9382-CDD9BEED5ADB" );
            // Add Block to Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlock( true, "7669A501-4075-431A-9828-565C47FD21C8", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Content Channel View Detail", "Main", @"", @"", 0, "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7" );
            // Add Block to Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlock( true, "BB83C51D-65C7-4F6C-BA24-A496167C9B11", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Content Channel View Detail", "Main", @"", @"", 0, "71D998C7-9F27-4B8A-937A-64C5EFC4783A" );
            // Attrib for BlockType: Content Channel View Detail:Item Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Item Cache Duration", "ItemCacheDuration", "", @"Number of seconds to cache the content item specified by the parameter.", 0, @"3600", "38351F1F-CE8B-458B-B5F4-8ECFA2DEE857" );
            // Attrib for BlockType: Public Profile Edit:Default Connection Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "", @"The connection status that should be set by default", 0, @"", "A819F717-D052-4F9C-AC49-2FD14380CC83" );
            // Attrib for BlockType: Communication Entry:Simple Communications Are Bulk
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Simple Communications Are Bulk", "IsBulk", "", @"Should simple mode communications be sent as a bulk communication?", 10, @"True", "C2AEEF4C-25E4-4BC7-B225-82C197E0CAD4" );
            // Attrib for BlockType: Family Pre Registration:Require Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "", @"Require that a campus be selected", 10, @"True", "A8368E19-A762-4C3D-9969-FD1D9029D5FF" );
            // Attrib for BlockType: Connection Requests:Hide Inactive Connection Requests
            RockMigrationHelper.UpdateBlockTypeAttribute( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Inactive Connection Requests", "HideInactive", "", @"Show only connection requests that are active?", 0, @"False", "259854C4-C01B-455E-B462-009E6DE430EE" );
            // Attrib for BlockType: Fundraising Donation Entry:Root Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "A24D68F2-C58B-4322-AED8-6556DBED1B76", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Root Group", "RootGroup", "", @"Select the group that will be used as the bease of the list.", 4, @"", "C253E0D3-C390-49AC-9F2E-83A6A6669BD9" );
            // Attrib for BlockType: Group Attendance Detail:Show Notes
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Notes", "ShowNotes", "", @"Should the notes field be displayed?", 9, @"True", "382744F8-035D-427D-A4A3-7686459B0CB0" );
            // Attrib for BlockType: Group Attendance Detail:Attendance Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Attendance Email", "AttendanceEmailTemplate", "", @"The System Email to use to send the attendance", 13, @"CA794BD8-25C5-46D9-B7C2-AD8190AC27E6", "64813B79-72AE-418A-BF14-2F035B5B9351" );
            // Attrib for BlockType: Group Attendance Detail:Send Summary Email To
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Send Summary Email To", "SendSummaryEmailTo", "", @"", 12, @"0", "42B5D362-48C5-46D0-8109-4179D717814A" );
            // Attrib for BlockType: Group Attendance Detail:Show Anonymous Count
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Anonymous Count", "ShowAnonymousCount", "", @"Should the anonymous count be displayed?", 11, @"True", "E374C60D-319C-49C4-9EF3-89CD64E337A0" );
            // Attrib for BlockType: Group Attendance Detail:Attendance Note Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attendance Note Label", "AttendanceNoteLabel", "", @"The text to use to describe the notes", 10, @"Notes", "B944ED54-514A-4BB8-9E49-94F8F4567026" );
            // Attrib for BlockType: Group Finder:CampusLabel
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CampusLabel", "CampusLabel", "", @"", 0, @"Campuses", "579FA5ED-19DA-4541-BC90-6489C7779125" );
            // Attrib for BlockType: Group Finder:TimeOfDayLabel
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "TimeOfDayLabel", "TimeOfDayLabel", "", @"", 0, @"Time of Day", "11DE5946-AB06-4C74-ADFD-5B04D367D368" );
            // Attrib for BlockType: Group Finder:DayOfWeekLabel
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "DayOfWeekLabel", "DayOfWeekLabel", "", @"", 0, @"Day of Week", "A9F9626C-9E62-4FE5-A875-EC9C42066B0B" );
            // Attrib for BlockType: Prayer Request Detail:Require Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "", @"Require that a campus be selected", 6, @"False", "54DC2244-7765-47F4-A2D2-4F7EB3C3F2FE" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Workflow Type Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Title Attribute Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Description Attribute Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Card Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Image Attribute Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Enabled Lava Commands Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Content Channel Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"8e213bb1-9e6f-40c1-b468-b3f8a60d5d24" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Content Channel Query Parameter Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Lava Template Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "47C56661-FB70-4703-9781-8651B8B49485", @"{% assign detailImageGuid = Item | Attribute:'DetailImage','RawValue' %}
{% if detailImageGuid != '' %}
  <img alt=""{{ Item.Title }}"" src=""/GetImage.ashx?Guid={{ detailImageGuid }}"" class=""title-image img-responsive"">
{% endif %}
<h1>{{ Item.Title }}</h1>{{ Item.Content }}" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Output Cache Duration Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Set Page Title Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Write Interaction Only If Individual Logged In Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Log Interactions Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Launch Workflow Only If Individual Logged In Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Launch Workflow Condition Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Blog Details, Attribute:Launch Workflow Only If Individual Logged In Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Blog Details, Attribute:Launch Workflow Condition Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Blog Details, Attribute:Log Interactions Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Blog Details, Attribute:Write Interaction Only If Individual Logged In Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Blog Details, Attribute:Output Cache Duration Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Set Page Title Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"True" );
            // Attrib Value for Block:Blog Details, Attribute:Lava Template Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "47C56661-FB70-4703-9781-8651B8B49485", @"<h1>{{ Item.Title }}</h1>
{{ Item.Content }}" );
            // Attrib Value for Block:Blog Details, Attribute:Content Channel Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"2b408da7-bdd1-4e71-b6ac-f22d786b605f" );
            // Attrib Value for Block:Blog Details, Attribute:Enabled Lava Commands Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Content Channel Query Parameter Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Image Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Title Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Card Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Description Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Workflow Type Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Workflow Type Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Description Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Card Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Title Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Image Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Query Parameter Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Enabled Lava Commands Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"0a63a427-e6b5-2284-45b3-789b293c02ea" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Lava Template Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "47C56661-FB70-4703-9781-8651B8B49485", @"	{% assign videoLink = Item | Attribute:'VideoLink','RawValue' %}
	{% assign videoEmbed = Item | Attribute:'VideoEmbed' %}
	{% assign audioLink = Item | Attribute:'AudioLink','RawValue' %}

	<article class=""message-detail"">

		{% if videoEmbed != '' %}
			{{ videoEmbed }}
		{% endif %}

		<h1>{{ Item.Title }}</h1>

		<p>
			<strong> {{ item | Attribute:'Speaker' }} - {{ Item.StartDateTime | Date:'M/d/yyyy' }}</strong>
		</p>

		<div class=""row"">
			<div class=""col-md-8"">
				{{ Item.Content }}
			</div>
			<div class=""col-md-4"">
				{% if videoLink != '' or audioLink != '' %}
					<div class=""panel panel-default"">
						<div class=""panel-heading"">Downloads &amp; Resources</div>
						<div class=""list-group"">

							{% if videoLink != '' %}
								<a href=""{{ videoLink }}"" class=""list-group-item""><i class=""fa fa-film""></i> Video Download</a>
							{% endif %}

							{% if audioLink != '' %}
								<a href=""{{ audioLink }}"" class=""list-group-item""><i class=""fa fa-volume-up""></i> Audio Download</a>
							{% endif %}

						</div>
					</div>
				{% endif %}
			</div>
		</row>
	</article>" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Set Page Title Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"True" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Log Interactions Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Output Cache Duration Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Write Interaction Only If Individual Logged In Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Condition Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Only If Individual Logged In Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Condition Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Write Interaction Only If Individual Logged In Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Only If Individual Logged In Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Output Cache Duration Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Set Page Title Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"True" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Log Interactions Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Lava Template Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "47C56661-FB70-4703-9781-8651B8B49485", @"<style>
	.series-banner {
		height: 220px;
		background-size: cover;
		background-position: center center;
		background-repeat: no-repeat;
	}

	@media (min-width: 992px) {
		.series-banner {
			height: 420px;
		}
	}

	.series-title{
		margin-bottom: 4px;
	}

	.series-dates {
		opacity: .6;
	}

	.messages-title {
		font-size: 24px;
	}

	.messages {
		font-size: 18px;
	}
</style>

{% if Item  %}

	<article class=""series-detail"">
		{% assign seriesImageGuid = Item | Attribute:'SeriesImage','RawValue' %}
		<div class=""series-banner"" style=""background-image: url('/GetImage.ashx?Guid={{ seriesImageGuid }}');"" ></div>

		<h1 class=""series-title"">{{ Item.Title }}</h1>
		<p class=""series-dates"">
			<strong>{{ Item.StartDateTime | Date:'M/d/yyyy' }}
				{% if Item.StartDateTime != Item.ExpireDateTime %}
					- {{ Item.ExpireDateTime | Date:'M/d/yyyy' }}
				{% endif %}
			</strong>
		</p>


		<script>function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }</script>
		<script>function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT'); var element = document.createElement('a'); element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text)); element.setAttribute('download', '{{ Event.Name }}.ics'); element.style.display = 'none'; document.body.appendChild(element); element.click(); document.body.removeChild(element); }</script>
		<ul class=""socialsharing"">
			<li>
				<a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
					<i class=""fa fa-fw fa-facebook""></i>
				</a>
			</li>
			<li>
				<a href=""http://twitter.com/home?status={{ 'Global' | Page:'Url' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
					<i class=""fa fa-fw fa-twitter""></i>
				</a>
			</li>
			<li>
				<a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ 'Global' | Page:'Url' }}""  class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
					<i class=""fa fa-fw fa-envelope-o""></i>
				</a>
			</li>
		</ul>

		<div class=""margin-t-lg"">
			{{ Item.Content }}
		</div>

		<h4 class=""messages-title margin-t-lg"">In This Series</h4>
		<ol class=""messages"">
			{% for message in Item.ChildItems %}
				<li>
					<a href=""/page/461?Item={{ message.ChildContentChannelItem.Id }}"">
						{{ message.ChildContentChannelItem.Title }}
					</a>
        </li>
			{% endfor %}
		</ol>

	</article>

{% else %}
	<h1>Could not find series.</h1>
{% endif %}" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"e2c598f1-d299-1baa-4873-8b679e3c1998" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Query Parameter Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Image Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Enabled Lava Commands Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Description Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Card Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Workflow Type Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Title Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );

        }

        private void CodeGenBlocksAndPagesDown()
        {
            // Attrib for BlockType: Prayer Request Detail:Require Campus
            RockMigrationHelper.DeleteAttribute( "54DC2244-7765-47F4-A2D2-4F7EB3C3F2FE" );
            // Attrib for BlockType: Group Finder:DayOfWeekLabel
            RockMigrationHelper.DeleteAttribute( "A9F9626C-9E62-4FE5-A875-EC9C42066B0B" );
            // Attrib for BlockType: Group Finder:TimeOfDayLabel
            RockMigrationHelper.DeleteAttribute( "11DE5946-AB06-4C74-ADFD-5B04D367D368" );
            // Attrib for BlockType: Group Finder:CampusLabel
            RockMigrationHelper.DeleteAttribute( "579FA5ED-19DA-4541-BC90-6489C7779125" );
            // Attrib for BlockType: Group Attendance Detail:Attendance Note Label
            RockMigrationHelper.DeleteAttribute( "B944ED54-514A-4BB8-9E49-94F8F4567026" );
            // Attrib for BlockType: Group Attendance Detail:Show Anonymous Count
            RockMigrationHelper.DeleteAttribute( "E374C60D-319C-49C4-9EF3-89CD64E337A0" );
            // Attrib for BlockType: Group Attendance Detail:Send Summary Email To
            RockMigrationHelper.DeleteAttribute( "42B5D362-48C5-46D0-8109-4179D717814A" );
            // Attrib for BlockType: Group Attendance Detail:Attendance Email
            RockMigrationHelper.DeleteAttribute( "64813B79-72AE-418A-BF14-2F035B5B9351" );
            // Attrib for BlockType: Group Attendance Detail:Show Notes
            RockMigrationHelper.DeleteAttribute( "382744F8-035D-427D-A4A3-7686459B0CB0" );
            // Attrib for BlockType: Fundraising Donation Entry:Root Group
            RockMigrationHelper.DeleteAttribute( "C253E0D3-C390-49AC-9F2E-83A6A6669BD9" );
            // Attrib for BlockType: Connection Requests:Hide Inactive Connection Requests
            RockMigrationHelper.DeleteAttribute( "259854C4-C01B-455E-B462-009E6DE430EE" );
            // Attrib for BlockType: Family Pre Registration:Require Campus
            RockMigrationHelper.DeleteAttribute( "A8368E19-A762-4C3D-9969-FD1D9029D5FF" );
            // Attrib for BlockType: Communication Entry:Simple Communications Are Bulk
            RockMigrationHelper.DeleteAttribute( "C2AEEF4C-25E4-4BC7-B225-82C197E0CAD4" );
            // Attrib for BlockType: Public Profile Edit:Default Connection Status
            RockMigrationHelper.DeleteAttribute( "A819F717-D052-4F9C-AC49-2FD14380CC83" );
            // Attrib for BlockType: Content Channel View Detail:Item Cache Duration
            RockMigrationHelper.DeleteAttribute( "38351F1F-CE8B-458B-B5F4-8ECFA2DEE857" );
            // Remove Block: Content Channel View Detail, from Page: Series Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7" );
            // Remove Block: Content Channel View Detail, from Page: Message Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "71D998C7-9F27-4B8A-937A-64C5EFC4783A" );
            // Remove Block: Blog Details, from Page: Blog Details, Site: External Website
            RockMigrationHelper.DeleteBlock( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB" );
            // Remove Block: Content Channel Dynamic, from Page: Item Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "0C828414-8AEF-4B43-AAEF-B200544A2197" );
        }

        /// <summary>
        /// GJ: Note Watch Notification Email
        /// </summary>
        private void NoteWatchNotificationEmail()
        {
            // Note Watch Notification
            RockMigrationHelper.UpdateSystemEmail( "System", "Note Watch Notification", "", "", "", "", "", "Note Watch Digest | {{ 'Global' | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}
<p>
    {{ Person.NickName }},
</p>

<p>
    Here are some updates on some notes you're watching:
</p>

<table style=""border: 1px solid #ffffff; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">
{%- for note in NoteList -%}
    {%- if note.EditedDateTime > note.CreatedDateTime -%}
        {%- assign noteAction = 'edited' -%}
    {%- else -%}
      {%- if note.ParentNoteId != null -%}
        {%- assign noteAction = 'replied to' -%}
      {%- else -%}
        {%- assign noteAction = 'left' -%}
      {%- endif -%}
    {%- endif -%}
    <tr>
        <td bgcolor=""#ffffff"" align=""left"" style=""color: #484848; padding: 4px 8px;{% unless forloop.last %} border-bottom: 1px solid #dbdbdb;{% endunless %}"">
            <h4 style=""color: #484848; line-height: 1.2em;font-weight: bold;"">
                {{ note.EditedByPersonName }} {{ noteAction }} a {{ note.NoteType.Name }} on {{ note.EntityName }}
            </h4>
            {%- assign noteText = note.Text | Escape | Linkify | FromMarkdown -%}
            {{ noteText }}
            <p>
                <a href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}{{ note.NoteUrl }}#{{ note.NoteAnchorId }}"">View Note</a>
            </p>
        </td>
    </tr>
{% endfor %}
</table>

<p>&nbsp;</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "21B92DE2-6825-45F3-BD27-43B47FE490D8" );
        }

        /// <summary>
        /// GJ: Note Approval Email
        /// </summary>
        private void NoteApprovalEmail()
        {
            // Note Approval Email

            RockMigrationHelper.UpdateSystemEmail( "System", "Note Approval Email", "", "", "", "", "", "Note Approvals Request | {{ 'Global' | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}



<p>

    {{ Person.NickName }},

</p>



<p>

    Here are some notes that require your approval:

</p>



<table style=""border: 1px solid #ffffff; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">

{% for note in NoteList %}

    {%- if note.EditedDateTime > note.CreatedDateTime -%}

        {%- assign noteAction = 'edited' -%}

    {%- else -%}

      {%- if note.ParentNoteId != null -%}

        {%- assign noteAction = 'replied to' -%}

      {%- else -%}

        {%- assign noteAction = 'left' -%}

      {%- endif -%}

    {%- endif -%}

    <tr>

      <td bgcolor=""#ffffff"" align=""left"" style=""color: #484848; padding: 4px 8px;{% unless forloop.last %} border-bottom: 1px solid #dbdbdb;{% endunless %}"">

        <h4 style=""color: #484848; line-height: 1.2em;font-weight: bold;"">

          {{ note.EditedByPersonName }} {{ noteAction }} a {{ note.NoteType.Name }} on {{ note.EntityName }}

        </h4>

        {%- assign noteText = note.Text | Escape | Linkify | FromMarkdown -%}

        {{ noteText }}

        <p>

        <a href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}{{ note.NoteUrl }}#{{ note.NoteAnchorId }}"">View Note</a>

        {% if note.NoteType.ApprovalUrlTemplate != '' %}

        | <a href=""{{ note.ApprovalUrl }}"">Note Approval</a>

        {% endif %}

        </p>

      </td>

    </tr>

{% endfor %}

</table>



<p>&nbsp;</p>



{{ 'Global' | Attribute:'EmailFooter' }}", "B2E3D75F-681E-430F-82C9-D0D681040FAF" );
        }

        /// <summary>
        /// GJ: Stark Migration
        /// </summary>
        private void StarkMigration()
        {
            Sql( @"UPDATE[HtmlContent]
SET[Content] = '<a class=""navbar-brand"" href=""~/page/1"">
{%- if CurrentPage.Layout.Site.SiteLogoBinaryFileId -%}
    <img src=""/GetImage.ashx?id={{ CurrentPage.Layout.Site.SiteLogoBinaryFileId }}"" alt=""{{ ''Global'' | Attribute:''OrganizationName'' }}"">
{%- else -%}
            {{ ''Global'' | Attribute:''OrganizationName'' }}
            {%- endif -%}
</a>
'
WHERE [BlockId] = ( SELECT [ID] FROM [Block] WHERE [Guid] = 'CABFB331-8878-46BF-AAE0-65E28560AEBB')
AND [Content] = '<a class=""navbar-brand"" href=""~/page/1"">{{ ''Global'' | Attribute:''OrganizationName'' }}</a>
'");

        }

        /// <summary>
        /// GJ: Person Timeline use person icons
        /// </summary>
        private void UsePersonPageIcon()
        {
            // Attrib Value for Block:TimeLine, Attribute:Use Person Icon Page: Person Profile, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"True" );

        }

        /// <summary>
        /// ED: Bad Lava in System Email (Fixes #3044)
        /// </summary>
        private void BadLavaInSystemEmail()
        {
            RockMigrationHelper.UpdateSystemEmail( "System", "Communication Queue Notice", "", "", "", "", "", "Alert: Your Rock Communications Are Queuing Up", @"
The following communications have been queued to send for longer than 2 hours...<br/>
<br/>
{% for comm in Communications %}
    <a href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}Communication/{{ comm.Id }}"">{{ comm.Subject }}</a> from {{ comm.SenderPersonAlias.Person.FullName }}.<br/>
{% endfor %}
", "2FC7D3E3-D85B-4265-8983-970345215DEA" );
        }

        /// <summary>
        /// Fix Email Preference Success Text
        /// </summary>
        private void EmailPreferenceText()
        {
            Sql( @"UPDATE[Attribute]
                SET [DefaultValue] = '<h4>Thank You</h4>We have saved your email preference.'
                WHERE [Guid] = '46309218-6CDF-427D-BE45-B3DE6FAC1FE1'
                AND [DefaultValue] = '<h4>Thank You</h4>We have saved your email preference.}'");
        }

        /// <summary>
        /// Remove email hyphenation
        /// </summary>
        private void FixPendingGroupMembersNotificationEmail()
        {
            Sql( @"UPDATE [SystemEmail]
SET [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    We wanted to make you aware of additional individuals who have taken the next step to connect with 
    group. The individuals'' names and contact information can be found below. Our 
    goal is to contact new members within 24-48 hours of receiving this email.
</p>

<table cellpadding=""25"">
{% for pendingIndividual in PendingIndividuals %}
    <tr><td>
        <strong>{{ pendingIndividual.FullName }}</strong><br />
        {%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' -%}
        {%- assign homePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' -%}
        {%- assign homeAddress = pendingIndividual | Address:''Home'' -%}
        
        {%- if mobilePhone != empty -%}
            Mobile Phone: {{ mobilePhone }}<br />
        {%- endif -%}
        
        {%- if homePhone != empty -%}
            Home Phone: {{ homePhone }}<br />
        {%- endif -%}
        
        {%- if pendingIndividual.Email != empty -%}
            {{ pendingIndividual.Email }}<br />
        {%- endif -%}
        
        <p>
        {%- if homeAddress != empty -%}
            Home Address <br />
            {{ homeAddress }}
        {%- endif -%}
        </p>
        
    </td></tr>
{% endfor %}
</table>


<p>
    Once you have connected with these individuals, please mark them as active.
</p>

<p>
    Thank you for your ongoing commitment to {{ ''Global'' | Attribute:''OrganizationName'' }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986'
AND [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    We wanted to make you aware of additional individuals who have taken the next step to connect with 
    group. The individuals'' names and contact information can be found below. Our 
    goal is to contact new members within 24-48 hours of receiving this e-mail.
</p>

<table cellpadding=""25"">
{% for pendingIndividual in PendingIndividuals %}
    <tr><td>
        <strong>{{ pendingIndividual.FullName }}</strong><br />
        {% assign mobilePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% assign homePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% assign homeAddress = pendingIndividual | Address:''Home'' %}
        
        {% if mobilePhone != empty %}
            Mobile Phone: {{ mobilePhone }}<br />
        {% endif %}
        
        {% if homePhone != empty %}
            Home Phone: {{ homePhone }}<br />
        {% endif %}
        
        {% if pendingIndividual.Email != empty %}
            {{ pendingIndividual.Email }}<br />
        {% endif %}
        
        <p>
        {% if homeAddress != empty %}
            Home Address <br />
            {{ homeAddress }}
        {% endif %}
        </p>
        
    </td></tr>
{% endfor %}
</table>


<p>
    Once you have connected with these individuals, please mark them as active.
</p>

<p>
    Thank you for your ongoing commitment to {{ ''Global'' | Attribute:''OrganizationName'' }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'" );
        }

        /// <summary>
        /// Update Forgot Username
        /// </summary>
        private void ForgotUserName()
        {
            Sql( @"UPDATE[PAGE]
                SET[PageTitle] = 'Forgot Username', [BrowserTitle] = 'Forgot Username', [InternalName] = 'Forgot Username'
                WHERE[Guid] = 'C6628FBD-F297-4C23-852E-40F1369C23A8' AND[PageTitle] = 'Forgot User Name'" );
        }

        /// <summary>
        /// ED: Unattended Check-in has a logging level of Activity instead of none.
        /// </summary>
        private void UnattendedCheckInLogging()
        {
            // Change the LoggingLevel on Unattended Check-in to 0 since it won't be saved anyway
            // and this will help to keep memory overhead down.
            Sql( @"UPDATE [WorkflowType]
                SET [LoggingLevel] = 0
                WHERE [Guid] = '011E9F5A-60D4-4FF5-912A-290881E37EAF'" );
        }

        /// <summary>
        /// JE: Fixed invalid DisplayInNavWhen setting on the My Settings page
        /// </summary>
        private void MySettingsNavFix()
        {
            Sql( @"UPDATE[Page]
                SET [DisplayInNavWhen] = 2
                WHERE [Guid] = 'CF54E680-2E02-4F16-B54B-A2F2D29CD932'" );
        }

        /// <summary>
        /// GJ: Fixed Icons on homepage
        /// </summary>
        private void HomepageIconsFix()
        {
            Sql( @"UPDATE [AttributeValue] SET [Value] = N'{% stylesheet id:''home-feature'' %}

.feature-image {
    width: 100%;
    height: 450px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}


.communicationview h1 {
    font-size: 28px;
    margin-top: 12px;
}

.homepage-article .photo {
    width: 100%;
    height: 140px;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
}

.metric {
    border: 1px solid #ccc;
    padding: 12px;
    margin-bottom: 12px;
}

.metric h5 {
    font-size: 24px;
    margin-top: 0;
    margin-bottom: 0;
    width: 100%;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.metric .value {
    font-size: 48px;
    font-weight: 800;
    line-height: 1em;
}

.metric .value small{
    display: block;
    font-weight: 300;
    font-size: 14px;
    line-height: 1em;
}

.metric .icon {
    float: right;
    opacity: .3;
    font-size: 65px;
    border-radius: 0;
    width: 85px;
    height: 65px;
}

{% endstylesheet %}

<div class=""communicationview"">
    {% assign featureLink = Item | Attribute:''FeatureLink'' -%}

    <div class=""feature"">

        <div class=""feature-image"" style="" background-image: url(''/GetImage.ashx?Guid={{ Item | Attribute:''FeatureImage'',''RawValue'' }}&w=2400&h=2400'');""></div>
        <h1 class=""feature-title"">{{ Item | Attribute:''FeatureTitle'' }}</h1>
        <p>
            {{ Item | Attribute:''FeatureText'' }}
        </p>

        {% if featureLink != empty -%}
            <a class=""btn btn-xs btn-link"" href=""{{ featureLink }}"">More Info</a>
        {% endif -%}
    </div>

    <hr class=""margin-v-lg"" />

    <div class=""margin-b-lg"">
        {{ Item | Attribute:''Articles'' }}
    </div>

    {% assign metricCount = Metrics | Size -%}

    {% if metricCount > 0 -%}
        <h1>Metrics</h1>

        <div class=""row"">
        {% for metric in Metrics -%}
            <div class=""col-lg-4"">
                <div class=""metric"">
                    <h5>{{ metric.Title }}</h5>
                    <span class=""date"">{{ metric.LastRunDateTime | Date:''sd'' }}</span>
                    <i class=""icon {{ metric.IconCssClass  }}""></i>

                    <div class=""value"">
                        {{ metric.LastValue | AsInteger }}
                        <small>{{ metric.UnitsLabel }}</small>
                    </div>
                </div>
            </div>

            {% cycle '''', '''', ''</div><div class=""row"">'' %}
        {% endfor -%}
        </div>
    {% endif %}

</div>' WHERE [Guid] = 'EEB490B4-A68E-4712-AD71-2239C844DBB0';" );
        }


    }
}
