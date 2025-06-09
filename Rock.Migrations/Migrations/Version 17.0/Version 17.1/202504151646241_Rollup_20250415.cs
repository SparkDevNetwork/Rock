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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250415 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddChatConfigurationAdminUIUp();
            UpdateExternalSiteSeriesAndMessagePagesUp();
            ChatAdminImprovementsUp();
            JPH_AddChatSharedChannelDefaultGroupRole_20250324_Up();
            AddLoginHistoryPageAndBlocksUp();
            JMH_FixBlankPreviewCommunicationTemplateVersionUp();
            AddCommunicationRecipientIndexUp();
            JPH_UpdateCommunicationRecipientIndex_20250401_Up();
            JMH_DeleteStarterEmailSections();
            ChopBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddChatConfigurationAdminUIDown();
            UpdateExternalSiteSeriesAndMessagePagesDown();
            ChatAdminImprovementsDown();
            AddLoginHistoryPageAndBlocksDown();
        }

        #region 235_AddChatConfigurationAdminUI Plugin Migration

        private void AddChatConfigurationAdminUIUp()
        {
            JPH_AddChatConfigurationPageAndBlocks_Up();
            JPH_UpdateChatGroups_20250320_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddChatConfigurationAdminUIDown()
        {
            JPH_AddChatConfigurationPageAndBlocks_Down();
            JPH_UpdateChatGroups_20250320_Down();
        }

        /// <summary>
        /// JPH: Add Chat Configuration page and blocks - up.
        /// </summary>
        private void JPH_AddChatConfigurationPageAndBlocks_Up()
        {
            // Add Page 
            //  Internal Name: Chat Configuration
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Chat Configuration", "", "03FA9A18-545E-4158-AEA8-2C78AE582C50", "fa fa-comments-o" );

            // Add Page Route
            //   Page:Chat Configuration
            //   Route:communications/chat-configuration
            RockMigrationHelper.AddOrUpdatePageRoute( "03FA9A18-545E-4158-AEA8-2C78AE582C50", "communications/chat-configuration", "05FF6DDF-B980-478A-977C-9A2A1C0786AB" );

            // Add Block 
            //  Block Name: Chat Configuration
            //  Page Name: Chat Configuration
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03FA9A18-545E-4158-AEA8-2C78AE582C50".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D5BE6AAE-70A2-4021-93F7-DD66A09B08CB".AsGuid(), "Chat Configuration", "Main", @"", @"", 0, "86E90886-01F5-4DA3-8162-0811A5A38F41" );

            // Add Block 
            //  Block Name: Chat Ban List
            //  Page Name: Chat Configuration
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03FA9A18-545E-4158-AEA8-2C78AE582C50".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "88B7EFA9-7419-4D05-9F88-38B936E61EDD".AsGuid(), "Chat Ban List", "Main", @"", @"", 1, "2C4B26B2-D16A-4536-B1E5-192C536C4AA8" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Person Profile Page
            /*   Attribute Value: 08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52 */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 3905c63f-4d57-40f0-9721-c60a2f681911,3b7e7c82-c4a3-45c2-ba56-85f6970a9be2 */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"3905c63f-4d57-40f0-9721-c60a2f681911,3b7e7c82-c4a3-45c2-ba56-85f6970a9be2" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Group
            /*   Attribute Value: c9e3a59f-3b5e-43b1-9d97-191ef82d73c4 */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A", @"c9e3a59f-3b5e-43b1-9d97-191ef82d73c4" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show Date Added
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "F281090E-A05D-4F81-AD80-A3599FB8E2CD", @"True" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show Campus Filter
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "65B9EA6C-D904-4105-8B51-CCA784DDAAFA", @"False" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show First/Last Attendance
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664", @"False" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show Note Column
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "5F54C068-1418-44FA-B215-FBF70072F6A5", @"True" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Block Title
            /*   Attribute Value: Chat Ban List */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "EB6292BE-96EA-4E08-A8CA-7245ACAA151D", @"Chat Ban List" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "2053BCCE-A4CA-49F0-BC94-4EA0FFA04E91", @"True" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "2924EC20-7A18-4DA6-98C5-6EF3E56A4B93", @"False" );
        }

        /// <summary>
        /// JPH: Add Chat Configuration page and blocks - down.
        /// </summary>
        private void JPH_AddChatConfigurationPageAndBlocks_Down()
        {
            // Remove Block
            //  Name: Chat Ban List, from Page: Chat Configuration, Site: Rock RMS
            //  from Page: Chat Configuration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8" );

            // Remove Block
            //  Name: Chat Configuration, from Page: Chat Configuration, Site: Rock RMS
            //  from Page: Chat Configuration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "86E90886-01F5-4DA3-8162-0811A5A38F41" );

            // Delete Page 
            //  Internal Name: Chat Configuration
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "03FA9A18-545E-4158-AEA8-2C78AE582C50" );

            // The [PageRoute] record will cascade-delete.
        }

        /// <summary>
        /// JPH: Update Chat groups - up.
        /// </summary>
        private void JPH_UpdateChatGroups_20250320_Up()
        {
            // Update previously-seeded "Chat Shared Channel" group type.
            // We're changing the description and making it visible in lists and nav.
            RockMigrationHelper.AddGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat shared channel is created within Rock.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: true,
                showInNavigation: true,
                iconCssClass: "fa fa-comments-o",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true );

            // Add a new "Chat Shared Channels" group to be the parent for all chat shared channels created within Rock.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                name: "Chat Shared Channels",
                description: "Parent group for all chat shared channels.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Add a new "Chat Direct Messages" group to be the parent for all chat direct messages created within the
            // external chat system.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE,
                name: "Chat Direct Messages",
                description: "Parent group for all chat direct messages.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Move the previously-created "Chat Ban List" group to live under the newly-created "Chat Shared Channels" parent group.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP,
                name: "Chat Ban List",
                description: "Used to identify individuals who are globally banned from chat. Anyone who belongs to this group will be unable to access Rock chat, even if they belong to chat-enabled groups.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Disable chat for the following system groups, as they should never by synced as a chat group.
            Sql( $@"
UPDATE [Group]
SET [IsChatEnabledOverride] = 0
WHERE [Guid] IN (
    '{Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST}'
    , '{Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS}'
    , '{Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES}'
);" );
        }

        /// <summary>
        /// JPH: Update Chat groups - down.
        /// </summary>
        private void JPH_UpdateChatGroups_20250320_Down()
        {
            // Revert changes to previously-seeded "Chat Shared Channel" group type.
            RockMigrationHelper.AddGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat channel is created from inside the external chat application.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: false,
                showInNavigation: false,
                iconCssClass: "fa fa-comments-o",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true );

            // Re-orphan the previously-created "Chat Ban List" group.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP,
                name: "Chat Ban List",
                description: "Used to identify individuals who are globally banned from chat. Anyone who belongs to this group will be unable to access Rock chat, even if they belong to chat-enabled groups.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Delete "Chat Shared Channels" group.
            RockMigrationHelper.DeleteGroup( Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS );

            // Delete "Chat Direct Messages" group.
            RockMigrationHelper.DeleteGroup( Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES );
        }

        #endregion

        #region 236_MigrationRollupsForV17_1_0 Plugin Migration (DH: Update external site series and message pages and routes.)

        /// <summary>
        /// Updates the external site message watch pages and routes.
        /// </summary>
        private void UpdateExternalSiteSeriesAndMessagePagesUp()
        {
            // Update the Series Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '7669a501-4075-431a-9828-565c47fd21c8'" );
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "39cc148d-b905-4560-96dd-c5151dc344de", "Series" );
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "47c56661-fb70-4703-9781-8651b8b49485", @"<style>
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
	<div class=""series-banner"" style=""background-image: url('/GetImage.ashx?Guid={{ seriesImageGuid }}');"">
	</div>
	<h1 class=""series-title"">
		{{ Item.Title }}
	</h1>
	<p class=""series-dates"">
		<strong>
			{{ Item.StartDateTime | Date:'M/d/yyyy' }}
			{% if Item.StartDateTime != Item.ExpireDateTime %}
			- {{ Item.ExpireDateTime | Date:'M/d/yyyy' }}
			{% endif %}
		</strong>
	</p>
	<script>
		function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }
	</script>
	<script>
		function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT'); var element = document.createElement('a'); element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text)); element.setAttribute('download', '{{ Event.Name }}.ics'); element.style.display = 'none'; document.body.appendChild(element); element.click(); document.body.removeChild(element); }
	</script>
	<ul class=""socialsharing"">
		<li>
			<a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
				<i class=""fa fa-fw fa-facebook"">
				</i>
			</a>
		</li>
		<li>
			<a href=""http://twitter.com/home?status={{ 'Global' | Page:'Url' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
				<i class=""fa fa-fw fa-twitter"">
				</i>
			</a>
		</li>
		<li>
			<a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ 'Global' | Page:'Url' }}"" class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
				<i class=""fa fa-fw fa-envelope-o"">
				</i>
			</a>
		</li>
	</ul>
	<div class=""margin-t-lg"">
		{{ Item.Content }}
	</div>
	<h4 class=""messages-title margin-t-lg"">
		In This Series
	</h4>
	<ol class=""messages"">
		{% for message in Item.ChildItems %}
		<li>
			<a href=""/watch/{{ Item.Id }}/{{ message.ChildContentChannelItem.Id }}"">
				{{ message.ChildContentChannelItem.Title }}
			</a>
		</li>
		{% endfor %}
	</ol>
</article>
{% else %}
<h1>
	Could not find series.
</h1>
{% endif %}
" );

            // Update the Message Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11'" );
            RockMigrationHelper.AddBlockAttributeValue( "71d998c7-9f27-4b8a-937a-64c5efc4783a", "39cc148d-b905-4560-96dd-c5151dc344de", "Message" );

            // Update the Blog Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '2d0d0fb0-68c4-47e1-8bc6-98f931497f5e'" );

            // Add the new routes, but only if there are not any routes
            // that might conflict.
            Sql( @"
DECLARE @SeriesPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7669a501-4075-431a-9828-565c47fd21c8')
DECLARE @MessagePageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11')
IF NOT EXISTS (SELECT 1 FROM [PageRoute] WHERE [Route] LIKE 'watch/%') AND @SeriesPageId IS NOT NULL AND @MessagePageId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [PageRoute] WHERE [Guid] IN ('bef2bf79-7cc0-49eb-93ab-2dcb7b1ce859', '8f2735a3-c434-4e3a-b7b6-a943a4254c5e'))
    BEGIN
        INSERT INTO [PageRoute] ([IsSystem], [PageId], [Route], [IsGlobal], [Guid]) 
            VALUES
            (1, @SeriesPageId, 'watch/{Series}', 0, 'bef2bf79-7cc0-49eb-93ab-2dcb7b1ce859'),
            (1, @MessagePageId, 'watch/{Series}/{Message}', 0, '8f2735a3-c434-4e3a-b7b6-a943a4254c5e')
    END
END" );
        }

        /// <summary>
        /// Revert the changes made to the external site series and message pages.
        /// </summary>
        private void UpdateExternalSiteSeriesAndMessagePagesDown()
        {
            // Revert the page routes.
            Sql( "DELETE FROM [PageRoute] WHERE [Guid] IN ('bef2bf79-7cc0-49eb-93ab-2dcb7b1ce859', '8f2735a3-c434-4e3a-b7b6-a943a4254c5e')" );

            // Revert the Blog Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = '2d0d0fb0-68c4-47e1-8bc6-98f931497f5e'" );

            // Revert the Message Detail page.
            RockMigrationHelper.AddBlockAttributeValue( "71d998c7-9f27-4b8a-937a-64c5efc4783a", "39cc148d-b905-4560-96dd-c5151dc344de", "Message" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11'" );

            // Revert the Series Detail page.
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "47c56661-fb70-4703-9781-8651b8b49485", @"<style>
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
	<div class=""series-banner"" style=""background-image: url('/GetImage.ashx?Guid={{ seriesImageGuid }}');"">
	</div>
	<h1 class=""series-title"">
		{{ Item.Title }}
	</h1>
	<p class=""series-dates"">
		<strong>
			{{ Item.StartDateTime | Date:'M/d/yyyy' }}
			{% if Item.StartDateTime != Item.ExpireDateTime %}
			- {{ Item.ExpireDateTime | Date:'M/d/yyyy' }}
			{% endif %}
		</strong>
	</p>
	<script>
		function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }
	</script>
	<script>
		function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT'); var element = document.createElement('a'); element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text)); element.setAttribute('download', '{{ Event.Name }}.ics'); element.style.display = 'none'; document.body.appendChild(element); element.click(); document.body.removeChild(element); }
	</script>
	<ul class=""socialsharing"">
		<li>
			<a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
				<i class=""fa fa-fw fa-facebook"">
				</i>
			</a>
		</li>
		<li>
			<a href=""http://twitter.com/home?status={{ 'Global' | Page:'Url' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
				<i class=""fa fa-fw fa-twitter"">
				</i>
			</a>
		</li>
		<li>
			<a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ 'Global' | Page:'Url' }}"" class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
				<i class=""fa fa-fw fa-envelope-o"">
				</i>
			</a>
		</li>
	</ul>
	<div class=""margin-t-lg"">
		{{ Item.Content }}
	</div>
	<h4 class=""messages-title margin-t-lg"">
		In This Series
	</h4>
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
<h1>
	Could not find series.
</h1>
{% endif %}
" );
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "39cc148d-b905-4560-96dd-c5151dc344de", "" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11'" );
        }

        #endregion

        #region 237_ChatAdminImprovements

        private void ChatAdminImprovementsUp()
        {
            JPH_AddMissingChatGroupTypeAssociations_20250321_Up();
            JPH_MoveChatBanListGroup_20250321_Up();
            JPH_SeedChatSyncJob_20250321_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ChatAdminImprovementsDown()
        {
            JPH_MoveChatBanListGroup_20250321_Down();
            JPH_SeedChatSyncJob_20250321_Down();
        }

        /// <summary>
        /// JPH: Add missing chat group type associations to the chat system groups - up.
        /// </summary>
        private void JPH_AddMissingChatGroupTypeAssociations_20250321_Up()
        {
            RockMigrationHelper.AddGroupTypeAssociation( SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE, SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE );
            RockMigrationHelper.AddGroupTypeAssociation( SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL, SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL );
        }

        /// <summary>
        /// JPH: Move "Chat Ban List" group - up.
        /// </summary>
        private void JPH_MoveChatBanListGroup_20250321_Up()
        {
            // Add a new "Hidden Application Group" group type.
            RockMigrationHelper.AddGroupType(
                name: "Hidden Application Group",
                description: "A generic group type used by specific features in Rock. Groups of this type are not shown in nav or in lists and have a single 'Member' role. For example, the Chat Ban List group is of this type.",
                groupTerm: "Group",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: false,
                showInNavigation: false,
                iconCssClass: "fa fa-gears",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP,
                isSystem: true );

            RockMigrationHelper.AddGroupTypeAssociation( Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP, Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP );

            // Add default "Member" group type role to the "Hidden Application Group" group type.
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP, "Member", "Member of a group", order: 0, maxCount: null, minCount: null, guid: Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER, isSystem: true, isLeader: false, isDefaultGroupTypeRole: true );

            // Change any existing, "Chat Ban List" members to have this new role.
            Sql( $@"
DECLARE @OldGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_APPLICATION_GROUP_MEMBER}');
DECLARE @NewGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER}');

UPDATE [GroupMember]
SET [GroupRoleId] = @NewGroupRoleId
WHERE [GroupRoleId] = @OldGroupRoleId;" );

            // Change the previously-created "Chat Ban List" group be of type "Hidden Application Group".
            // Also clear out its parent group (previously "Chat Shared Channels").
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP,
                name: "Chat Ban List",
                description: "Used to identify individuals who are globally banned from chat. Anyone who belongs to this group will be unable to access Rock chat, even if they belong to chat-enabled groups.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );
        }

        /// <summary>
        /// JPH: Move "Chat Ban List" group - down.
        /// </summary>
        private void JPH_MoveChatBanListGroup_20250321_Down()
        {
            // Revert changes to existing, "Chat Ban List" members.
            Sql( $@"
DECLARE @OldGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER}');
DECLARE @NewGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_APPLICATION_GROUP_MEMBER}');

UPDATE [GroupMember]
SET [GroupRoleId] = @NewGroupRoleId
WHERE [GroupRoleId] = @OldGroupRoleId;" );

            // Revert changes made to the "Chat Ban List" group.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP,
                name: "Chat Ban List",
                description: "Used to identify individuals who are globally banned from chat. Anyone who belongs to this group will be unable to access Rock chat, even if they belong to chat-enabled groups.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Delete the "Hidden Application Group" group type and role.
            RockMigrationHelper.DeleteGroupTypeRole( Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER );
            RockMigrationHelper.DeleteGroupType( Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP );
        }

        /// <summary>
        /// JPH: Seed an instance of the "Chat Sync" job - up.
        /// </summary>
        private void JPH_SeedChatSyncJob_20250321_Up()
        {
            Sql( $@"
DECLARE @Now DATETIME = (SELECT GETDATE());

IF EXISTS
(
    SELECT [Id]
    FROM [ServiceJob]
    WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}'
)
BEGIN
    UPDATE [ServiceJob]
    SET [Name] = 'Chat Sync'
        , [Description] = 'Job that performs synchronization tasks between Rock and the external chat system.'
        , [Class] = 'Rock.Jobs.ChatSync'
        , [CronExpression] = '0 0 5 1/1 * ? *'
        , [ModifiedDateTime] = @Now
    WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}';
END
ELSE
BEGIN
    INSERT INTO [ServiceJob]
    (
        [IsSystem]
        , [IsActive]
        , [Name]
        , [Description]
        , [Class]
        , [CronExpression]
        , [NotificationStatus]
        , [Guid]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [HistoryCount]
    )
    VALUES
    (
        0
        , 1
        , 'Chat Sync'
        , 'Job that performs synchronization tasks between Rock and the external chat system.'
        , 'Rock.Jobs.ChatSync'
        , '0 0 5 1/1 * ? *'
        , 1
        , '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}'
        , @Now
        , @Now
        , 500
    );
END" );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Synchronize Data",
                description: "Determines if data synchronization should be performed between Rock and the external chat system. If enabled, this will ensure that all chat-related data in Rock is in sync with the corresponding data in the external chat system.",
                order: 1,
                defaultValue: true.ToString(),
                guid: "B63C2896-C0EE-4396-9A3E-24F6F0E3C1AB",
                key: "SynchronizeData",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Create Interactions",
                description: "Determines if chat interaction records should be created. If enabled, this will create a daily interaction for each person who posted one or more messages within a given chat channel, and will include how many messages that person posted within that channel for that day. Will only look back up to 5 days when determining the interactions to create.",
                order: 2,
                defaultValue: true.ToString(),
                guid: "716A7180-26BA-4C99-9BE8-E66B65A60458",
                key: "CreateInteractions",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Delete Merged Chat Individuals",
                description: "Determines if non-prevailing, merged chat individuals should be deleted in the external chat system. If enabled, when two people in Rock have been merged, and both had an associated chat individual, the non-prevailing chat individual will be deleted from the external chat system to ensure other people can send future messages to only the prevailing chat individual.",
                order: 3,
                defaultValue: true.ToString(),
                guid: "E91B3F20-BA58-4710-8E50-9F3A28B7DC8F",
                key: "DeleteMergedChatUsers",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Enforce Default Grants Per Role",
                description: "This is an experimental setting that might be removed in a future version of Rock. If enabled, will overwrite all permission grants (per role) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these permissions.",
                order: 4,
                defaultValue: true.ToString(),
                guid: "91FC0EF3-2D63-40D2-9FD0-31E5D8605319",
                key: "EnforceDefaultGrantsPerRole",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Enforce Default Sync Settings",
                description: "This is an experimental setting that might be removed in a future version of Rock. If enabled, will overwrite all settings (e.g. channel type and channel settings) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these settings.",
                order: 5,
                defaultValue: true.ToString(),
                guid: "1D4AA3DE-06C1-44DE-97FC-BC04F1AEC800",
                key: "EnforceDefaultSyncSettings",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Command Timeout",
                description: "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher.",
                order: 6,
                defaultValue: 3600.ToString(),
                guid: "6DA10AE8-CF9B-4393-824A-F45AF96DDCB0",
                key: "CommandTimeout",
                isRequired: false
            );
        }

        /// <summary>
        /// JPH: Seed an instance of the "Chat Sync" job - down.
        /// </summary>
        private void JPH_SeedChatSyncJob_20250321_Down()
        {
            RockMigrationHelper.DeleteAttribute( "B63C2896-C0EE-4396-9A3E-24F6F0E3C1AB" );
            RockMigrationHelper.DeleteAttribute( "716A7180-26BA-4C99-9BE8-E66B65A60458" );
            RockMigrationHelper.DeleteAttribute( "E91B3F20-BA58-4710-8E50-9F3A28B7DC8F" );
            RockMigrationHelper.DeleteAttribute( "91FC0EF3-2D63-40D2-9FD0-31E5D8605319" );
            RockMigrationHelper.DeleteAttribute( "1D4AA3DE-06C1-44DE-97FC-BC04F1AEC800" );
            RockMigrationHelper.DeleteAttribute( "6DA10AE8-CF9B-4393-824A-F45AF96DDCB0" );

            Sql( $@"
DELETE FROM [ServiceJob]
WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}';" );
        }

        #endregion

        #region 238_ChatSharedChannelDefaultGroupRole

        /// <summary>
        /// JPH: Add "Chat Shared Channel" default group role - up.
        /// </summary>
        private void JPH_AddChatSharedChannelDefaultGroupRole_20250324_Up()
        {
            // Update previously-seeded "Chat Shared Channel" group type.
            // We're reassigning the default group role that was wiped out in a recent migration.
            RockMigrationHelper.UpdateGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat shared channel is created within Rock.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                defaultGroupRoleGuid: "E133E458-9785-4C59-B844-35E0C3B686D9",
                allowMultipleLocations: false,
                showInGroupList: true,
                showInNavigation: true,
                iconCssClass: "fa fa-comments-o",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true
            );
        }

        #endregion

        #region 239_AddLoginHistoryPageAndBlocks

        private void AddLoginHistoryPageAndBlocksUp()
        {
            JPH_UnsecureOidcGivePermissionPage_20250326_Up();
            JPH_AddLoginHistoryPageAndBlocks_20250326_Up();
            //JPH_MigrateLoginHistory_20250326_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddLoginHistoryPageAndBlocksDown()
        {
            JPH_UnsecureOidcGivePermissionPage_20250326_Down();
            JPH_AddLoginHistoryPageAndBlocks_20250326_Down();
        }

        /// <summary>
        /// JPH: Unsecure OIDC Give Permission page - up.
        /// </summary>
        private void JPH_UnsecureOidcGivePermissionPage_20250326_Up()
        {
            // This OIDC page will handle authentication manually, as we need to first load the page unsecured and check
            // some OIDC-specific stuff. We'll redirect to the login page if needed.
            RockMigrationHelper.DeleteSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, 0, "View", true, null, 1, "ED4B5C98-8442-4CCA-8C17-2CD17AF10016" );
        }

        /// <summary>
        /// JPH: Unsecure OIDC Give Permission page - down.
        /// </summary>
        private void JPH_UnsecureOidcGivePermissionPage_20250326_Down()
        {
            // Reset the security back to only allow authenticated individuals.
            RockMigrationHelper.DeleteSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, 0, "View", true, null, 2, "A9705A6C-A339-4BE3-835A-1A0CE3CBE194" );
        }

        /// <summary>
        /// JPH: Add Login History page and blocks - up.
        /// </summary>
        private void JPH_AddLoginHistoryPageAndBlocks_20250326_Up()
        {
            // Add Page 
            //  Internal Name: Login History
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Login History", "", "A2495383-5717-451D-95C2-0F14B6787B3A", "fa fa-history" );

            // Add Page Route
            //   Page:Login History
            //   Route:admin/security/login-history
            RockMigrationHelper.AddOrUpdatePageRoute( "A2495383-5717-451D-95C2-0F14B6787B3A", "admin/security/login-history", "7F103C07-FF6F-422C-B202-67E3B6FA8C09" );

            // ----------------------------------

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.LoginHistory
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.LoginHistory", "User Login Activity", "Rock.Blocks.Security.LoginHistory, Rock.Blocks, Version=17.0.36.0, Culture=neutral, PublicKeyToken=null", false, false, "63507646-F14D-4F2C-A5C4-FA28B3DEB8F0" );

            // Add/Update Obsidian Block Type
            //   Name:Login History
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.LoginHistory
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Login History", "A block for viewing login activity for all or a single person.", "Rock.Blocks.Security.LoginHistory", "Security", "6C02377F-DD74-4B2C-9BAD-1A010A12A714" );

            // Attribute for BlockType
            //   BlockType: Login History
            //   Category: Security
            //   Attribute: Enable Person Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C02377F-DD74-4B2C-9BAD-1A010A12A714", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Context", "EnablePersonContext", "Enable Person Context", @"If enabled and the page has a person context, its value will be used to limit the grid results to only this person, and the ""Person"" column will be hidden.", 0, @"False", "6C4AD3C4-8635-422D-9F02-8F20E8E64EC6" );

            // ----------------------------------

            // Add Block 
            //  Block Name: Login History
            //  Page Name: Login History
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A2495383-5717-451D-95C2-0F14B6787B3A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C02377F-DD74-4B2C-9BAD-1A010A12A714".AsGuid(), "Login History", "Main", @"", @"", 0, "38F796C4-EA54-4B51-A16D-2845DDD82263" );

            // ----------------------------------

            // Add Block 
            //  Block Name: Login History
            //  Page Name: History
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C02377F-DD74-4B2C-9BAD-1A010A12A714".AsGuid(), "Login History", "SectionC1", @"", @"", 4, "427BC41B-E803-41FD-B09A-91EA85F492F1" );

            // Add Block Attribute Value
            //   Block: Login History
            //   BlockType: Login History
            //   Category: Security
            //   Block Location: Page=History, Site=Rock RMS
            //   Attribute: Enable Person Context
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "427BC41B-E803-41FD-B09A-91EA85F492F1", "6C4AD3C4-8635-422D-9F02-8F20E8E64EC6", @"True" );
        }

        /// <summary>
        /// JPH: Add Login History page and blocks - down.
        /// </summary>
        private void JPH_AddLoginHistoryPageAndBlocks_20250326_Down()
        {
            // Remove Block
            //  Name: Login History, from Page: History, Site: Rock RMS
            //  from Page: History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "427BC41B-E803-41FD-B09A-91EA85F492F1" );

            // ----------------------------------

            // Remove Block
            //  Name: Login History, from Page: Login History, Site: Rock RMS
            //  from Page: Login History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "38F796C4-EA54-4B51-A16D-2845DDD82263" );

            // ----------------------------------

            // Attribute for BlockType
            //   BlockType: Login History
            //   Category: Security
            //   Attribute: Enable Person Context
            RockMigrationHelper.DeleteAttribute( "6C4AD3C4-8635-422D-9F02-8F20E8E64EC6" );

            // Delete BlockType 
            //   Name: Login History
            //   Category: Security
            //   Path: -
            //   EntityType: User Login Activity
            RockMigrationHelper.DeleteBlockType( "6C02377F-DD74-4B2C-9BAD-1A010A12A714" );

            // Delete Block EntityType: Rock.Blocks.Security.LoginHistory
            RockMigrationHelper.DeleteEntityType( "63507646-F14D-4F2C-A5C4-FA28B3DEB8F0" );

            // ----------------------------------

            // Delete Page 
            //  Internal Name: Login History
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "A2495383-5717-451D-95C2-0F14B6787B3A" );
        }

        /// <summary>
        /// JPH: Add a post update job to migrate login history.
        /// </summary>
        private void JPH_MigrateLoginHistory_20250326_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.1 - Migrate Login History",
                description: "This job will migrate login history from the History table to the HistoryLogin table.",
                jobType: "Rock.Jobs.PostV171MigrateLoginHistory",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_171_MIGRATE_LOGIN_HISTORY );
        }

        #endregion

        #region 240_FixBlankPreviewCommunicationTemplateVersion

        private void JMH_FixBlankPreviewCommunicationTemplateVersionUp()
        {
            // Update the Blank (Preview) communication template to have the new Beta version.
            Sql( $@"UPDATE [CommunicationTemplate]
SET [Version] = {( int ) CommunicationTemplateVersion.Beta}
WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'" );
        }

        #endregion

        #region KH: Add Communication Recipient Index

        private void AddCommunicationRecipientIndexUp()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.1 - Add Communication Recipient Index",
                description: "This job will add a new index for the Communication Recipient table.",
                jobType: "Rock.Jobs.PostV171AddCommunicationRecipientIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_171_ADD_COMMUNICATIONRECIPIENT_INDEX );
        }

        #endregion

        #region JPH: Update CommunicationRecipient Index

        /// <summary>
        /// JPH: Add a post update job to update an existing index on the CommunicationRecipient table.
        /// </summary>
        private void JPH_UpdateCommunicationRecipientIndex_20250401_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.1 - Update CommunicationRecipient Index",
                description: "This job will update an existing index on the CommunicationRecipient table.",
                jobType: "Rock.Jobs.PostV171UpdateCommunicationRecipientIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_171_UPDATE_COMMUNICATIONRECIPIENT_INDEX );
        }

        #endregion

        #region JMH: Delete Starter Email Sections

        private void JMH_DeleteStarterEmailSections()
        {
            // Delete the starter Email Sections used by the Obsidian Communication Entry Wizard.
            // They will be recreated when the user creates a new communication.
            Sql( $@"DELETE FROM [EmailSection]
WHERE [Guid] IN ('ACAE542B-51E3-4BB2-99B3-FF420A85D019', '6CBE0906-9A9A-4B67-91AF-FABD4936DEC9', '63C1EBF8-0398-4039-9FBA-A99886ED7106')" );
        }

        #endregion

        #region KH: Register block attributes for chop job in v17.1 (18.0.4)

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            //ChopBlockTypesv17_1();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelTypeList", "Content Channel Type List", "Rock.Blocks.Cms.ContentChannelTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "AFB54433-A564-4E77-A10C-8946FF9D9EC6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersistedDatasetList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersistedDatasetList", "Persisted Dataset List", "Rock.Blocks.Cms.PersistedDatasetList, Rock.Blocks, Version=1.16.7.5, Culture=neutral, PublicKeyToken=null", false, false, "DC11E26E-7E4A-4550-AF2D-2C9B94BEED4E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AttributeMatrixTemplateList", "Attribute Matrix Template List", "Rock.Blocks.Core.AttributeMatrixTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "A1D4E3E2-60A6-4815-9984-F87DD4741AAF" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DeviceList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DeviceList", "Device List", "Rock.Blocks.Core.DeviceList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "DC43AE74-09D8-4080-9074-2CA91B6119D2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.TagList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.TagList", "Tag List", "Rock.Blocks.Core.TagList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "9A396390-842F-4408-AEFD-FB4793F9EF7E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.AssessmentTypeList", "Assessment Type List", "Rock.Blocks.Crm.AssessmentTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "26DD8B62-5826-44A9-82B1-C6E4E4AB61D0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.BadgeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.BadgeList", "Badge List", "Rock.Blocks.Crm.BadgeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "A42A7EAC-C24C-4B6E-8870-762B4C64A97C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialGatewayList", "Financial Gateway List", "Rock.Blocks.Finance.FinancialGatewayList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "9158F560-4EAE-4E1D-80FF-DA24C351E241" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialStatementTemplateList", "Financial Statement Template List", "Rock.Blocks.Finance.FinancialStatementTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "F46CD5A7-BAF5-4EEB-8154-A4F4AC886264" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.RestKeyList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.RestKeyList", "Rest Key List", "Rock.Blocks.Security.RestKeyList, Rock.Blocks, Version=1.16.7.5, Culture=neutral, PublicKeyToken=null", false, false, "55E010D1-152A-4745-8E1D-2DB2195F2B36" );

            // Add/Update Obsidian Block Type
            //   Name:Assessment Type List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Assessment Type List", "Displays a list of assessment types.", "Rock.Blocks.Crm.AssessmentTypeList", "CRM", "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4" );

            // Add/Update Obsidian Block Type
            //   Name:Attribute Matrix Template List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attribute Matrix Template List", "Shows a list of all attribute matrix templates.", "Rock.Blocks.Core.AttributeMatrixTemplateList", "Core", "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD" );

            // Add/Update Obsidian Block Type
            //   Name:Badge List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.BadgeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Badge List", "Displays a list of badges.", "Rock.Blocks.Crm.BadgeList", "CRM", "559978D5-A392-4BD1-8E04-055C2833F347" );

            // Add/Update Obsidian Block Type
            //   Name:Content Channel Type List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Channel Type List", "Displays a list of content channel types.", "Rock.Blocks.Cms.ContentChannelTypeList", "CMS", "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B" );

            // Add/Update Obsidian Block Type
            //   Name:Device List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DeviceList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Device List", "Displays a list of devices.", "Rock.Blocks.Core.DeviceList", "Core", "7686A42F-A2C4-4C15-9331-8B364F24BD0F" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Statement Template List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Statement Template List", "Displays a list of financial statement templates.", "Rock.Blocks.Finance.FinancialStatementTemplateList", "Finance", "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F" );

            // Add/Update Obsidian Block Type
            //   Name:Gateway List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Gateway List", "Block for viewing list of financial gateways.", "Rock.Blocks.Finance.FinancialGatewayList", "Finance", "0F99866A-7FAB-462D-96EB-9F9534322C57" );

            // Add/Update Obsidian Block Type
            //   Name:Persisted Dataset List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersistedDatasetList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Persisted Dataset List", "Displays a list of persisted datasets.", "Rock.Blocks.Cms.PersistedDatasetList", "CMS", "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E" );

            // Add/Update Obsidian Block Type
            //   Name:Rest Key List
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.RestKeyList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Key List", "Lists all the REST API Keys", "Rock.Blocks.Security.RestKeyList", "Security", "40B6AF94-5FFC-4EE3-ADD9-C76818992274" );

            // Add/Update Obsidian Block Type
            //   Name:Tag List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.TagList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Tag List", "Block for viewing a list of tags.", "Rock.Blocks.Core.TagList", "Core", "0ACF764F-5F60-4985-9D10-029CB042DA0D" );

            // Attribute for BlockType
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "723C3BEA-F72D-4B86-A0DA-92FAB1CBC973" );

            // Attribute for BlockType
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AD70FEE2-736A-4D90-BA54-B16CAFD9CE94" );

            // Attribute for BlockType
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the assessment type details.", 0, @"", "100D6999-8F0C-4EB7-92D1-537DBD385AEB" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "272E9A4A-0515-46A9-819C-3B1E5B161F2B" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2BF4400C-19B0-4C1C-80F2-C96559584FC1" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the attribute matrix template details.", 0, @"", "6032EF50-8F86-4093-87EE-4F02AADAF2D0" );

            // Attribute for BlockType
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "559978D5-A392-4BD1-8E04-055C2833F347", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E55B952E-A492-47EA-83F8-F5DB313C059C" );

            // Attribute for BlockType
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "559978D5-A392-4BD1-8E04-055C2833F347", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E39490AF-EAFA-4D53-9DE9-9081A5499D47" );

            // Attribute for BlockType
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "559978D5-A392-4BD1-8E04-055C2833F347", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the badge details.", 0, @"", "79D2AFB7-824F-479C-96F5-540A4255749E" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "569A371C-30DA-4588-9368-4A5F72CC8335" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AA12725C-136A-4535-A61B-EAEE7163B009" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the content channel type details.", 0, @"", "1FE6EC0B-F714-45BD-AD00-A5E1F1DAF27E" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7686A42F-A2C4-4C15-9331-8B364F24BD0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "23CDA7C6-A3E5-461A-87AF-73A688117AED" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7686A42F-A2C4-4C15-9331-8B364F24BD0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FB060D89-3347-4D87-8426-6653E5FD51B3" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7686A42F-A2C4-4C15-9331-8B364F24BD0F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the device details.", 0, @"", "E58A1D17-6664-4F0C-A8CA-1727C52F1191" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7FF05AB4-C1B4-449D-9D6F-823C98041772" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "9F1F79D1-6190-4EE5-9EF8-F244116EE472" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the financial statement template details.", 0, @"", "55E63235-32AA-4F6E-92F8-3D8883B09DB1" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0F99866A-7FAB-462D-96EB-9F9534322C57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "78F010B3-C8BB-46CB-92EE-7EB6547C962D" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0F99866A-7FAB-462D-96EB-9F9534322C57", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "30DBBC5F-D162-4A45-828F-FB5B9790909C" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0F99866A-7FAB-462D-96EB-9F9534322C57", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the financial gateway details.", 0, @"", "9922A104-9184-45BE-BF98-35014E3B9D65" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "23E8236F-7BDD-4F08-ACF1-4BC534B907B5" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "42A40F23-C51E-4311-88F3-3C7E524E6713" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the persisted dataset details.", 0, @"", "616EAA22-3C6B-46E6-81D7-8FDB471CBCA9" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: Max Preview Size (MB)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Max Preview Size (MB)", "MaxPreviewSizeMB", "Max Preview Size (MB)", @"If the JSON data is large, it could cause the browser to timeout.", 2, @"1", "18BAE77E-2A11-44F8-B275-81C001784332" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "40B6AF94-5FFC-4EE3-ADD9-C76818992274", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "865EF130-C08A-452C-8DDB-590474C1CEF9" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "40B6AF94-5FFC-4EE3-ADD9-C76818992274", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0CE31EED-E496-4E4C-8953-FD3E81E9F7BB" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "40B6AF94-5FFC-4EE3-ADD9-C76818992274", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the person details.", 0, @"", "66785D25-9671-40BE-AC37-B9440376C117" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ACF764F-5F60-4985-9D10-029CB042DA0D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4D82E702-7520-4BCD-A731-787A453E6F48" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ACF764F-5F60-4985-9D10-029CB042DA0D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D56C9E63-3366-4030-9517-479A842AAA01" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ACF764F-5F60-4985-9D10-029CB042DA0D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the tag details.", 0, @"", "76783565-690B-41B7-9E50-20102A44549E" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: Show Qualifier Columns
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ACF764F-5F60-4985-9D10-029CB042DA0D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Qualifier Columns", "ShowQualifierColumns", "Show Qualifier Columns", @"Should the 'Qualifier Column' and 'Qualifier Value' fields be displayed in the grid?", 0, @"false", "55DA3A81-0A73-494B-A4FD-35C1CE5C9EB7" );
        }

        private void ChopBlockTypesv17_1()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 17.1 (18.0.4)",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v17.1 (Pre-Alpha: 18.0.4)
{ "00A86827-1E0C-4F47-8A6F-82581FA75CED", "1fde6d4f-390a-4ff6-ad42-668ec8cc62c4" }, // Assessment Type List ( CRM )
{ "069554B7-983E-4653-9A28-BA39659C6D63", "47f619c2-f66d-45ec-adbb-22ca23b4f3ad" }, // Attribute Matrix Template List ( Core )
{ "32183AD6-01CB-4533-858B-1BDA5120AAD5", "7686a42f-a2c4-4c15-9331-8b364f24bd0f" }, // Device List ( Core )
{ "32E89BAE-C085-40B3-B872-B62E25A62BDB", "0f99866a-7fab-462d-96eb-9f9534322c57" }, // Gateway List ( Finance )
{ "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6", "cfbb4daf-1aeb-4095-8098-e3a82e30fa7e" }, // Persisted Dataset List ( CMS )
{ "65057F07-85D5-4795-91A1-86D8F67A65DC", "2eaf9e5a-f47d-4c58-9aa4-2d340547a35f" }, // Financial Statement Template List ( Finance )
{ "A580027F-56DB-43B0-AAD6-7C2B8A952012", "29227fc7-8f24-44b1-a0fb-e6a8694f1c3b" }, // Content Channel Type List ( CMS )
{ "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC", "40b6af94-5ffc-4ee3-add9-c76818992274" }, // Rest Key List ( Security )
{ "C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53", "0acf764f-5f60-4985-9d10-029cb042da0d" }, // Tag List ( Core )
{ "D8CCD577-2200-44C5-9073-FD16F174D364", "559978d5-a392-4bd1-8e04-055c2833f347" }, // Badge List ( CRM )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_171_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> { } );
        }

        #endregion
    }
}
