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

    /// <summary>
    ///
    /// </summary>
    public partial class RefreshEmailPreferenceEntryBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_RenameExistingEmailPreferenceEntryBlock_202500904_Up();
            JPH_AddRefreshedEmailPreferenceEntryBlockType_20250904_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddRefreshedEmailPreferenceEntryBlockType_20250904_Down();
            JPH_RenameExistingEmailPreferenceEntryBlock_20250904_Down();
        }

        /// <summary>
        /// JPH: Renames the existing EmailPreferenceEntry [Obsidian] block to EmailPreferenceEntryLegacy - up.
        /// </summary>
        /// <remarks>
        /// Although it's unconventional to add a "Legacy" suffix to an Obsidian block, this is necessary as we need to
        /// introduce a refreshed version of the already-chopped block without removing this existing one.
        /// </remarks>
        private void JPH_RenameExistingEmailPreferenceEntryBlock_202500904_Up()
        {
            Sql( $@"
UPDATE [BlockType]
SET [Name] = 'Email Preference Entry (Legacy)'
WHERE [Guid] = '476FBA19-005C-4FF4-996B-CA1B165E5BC8';

UPDATE [EntityType]
SET [Name] = 'Rock.Blocks.Communication.EmailPreferenceEntryLegacy'
    , [AssemblyName] = 'Rock.Blocks.Communication.EmailPreferenceEntryLegacy' -- This will get overwritten with the correct assembly name on Rock startup.
    , [FriendlyName] = 'Email Preference Entry (Legacy)'
WHERE [Guid] = '28265232-B692-4099-9533-4D7646BDA2C1';" );
        }

        /// <summary>
        /// JPH: Renames the existing EmailPreferenceEntry [Obsidian] block to EmailPreferenceEntryLegacy - down.
        /// </summary>
        private void JPH_RenameExistingEmailPreferenceEntryBlock_20250904_Down()
        {
            Sql( $@"
UPDATE [BlockType]
SET [Name] = 'Email Preference Entry'
WHERE [Guid] = '476FBA19-005C-4FF4-996B-CA1B165E5BC8';

UPDATE [EntityType]
SET [Name] = 'Rock.Blocks.Communication.EmailPreferenceEntry'
    , [AssemblyName] = 'Rock.Blocks.Communication.EmailPreferenceEntry' -- This will get overwritten with the correct assembly name on Rock startup.
    , [FriendlyName] = 'Email Preference Entry'
WHERE [Guid] = '28265232-B692-4099-9533-4D7646BDA2C1';" );
        }

        /// <summary>
        /// JPH: Add refreshed Email Preference Entry block type and settings - up.
        /// </summary>
        private void JPH_AddRefreshedEmailPreferenceEntryBlockType_20250904_Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.EmailPreferenceEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.EmailPreferenceEntry", "Email Preference Entry", "Rock.Blocks.Communication.EmailPreferenceEntry, Rock.Blocks, Version=18.0.11.0, Culture=neutral, PublicKeyToken=null", false, false, "E80E5BA7-40C8-4E99-B2F7-0B668012521F" );

            // Add/Update Obsidian Block Type
            //   Name:Email Preference Entry
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.EmailPreferenceEntry
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Email Preference Entry", "Allows a person to set their email preferences and manage their subscriptions to communication channels.", "Rock.Blocks.Communication.EmailPreferenceEntry", "Communication", "FF88B243-4580-4BE0-97B1-BA9895C3FB8F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Configuration Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Communication Configuration Options", "AvailableOptions", "Communication Configuration Options", @"Select the options that should be available to a person when they are updating their communication preferences.", 0, @"Update Email Address,Unsubscribe,Not Involved", "6A463D8E-DA08-48A3-A6E3-331F81D94F02" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Show Header Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Header Icon", "ShowHeaderIcon", "Show Header Icon", @"When enabled, a default icon will be shown beside the header title.", 1, @"True", "4F09BE35-8D30-46D1-8191-83D185FC215A" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Exclude Communication Flows
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Communication Flows", "ExcludeCommunicationFlows", "Exclude Communication Flows", @"When enabled, communication flows will not be displayed in the Current or Available Communication Channels sections.", 2, @"False", "1F915CAE-2247-4C14-8C11-AEF74374A3BD" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Filter Communication Lists by Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Communication Lists by Campus Context", "FilterGroupsByCampusContext", "Filter Communication Lists by Campus Context", @"When enabled, all communication lists will be filtered by the campus context of the page. Lists with no campus will always be shown.", 3, @"False", "96E9F09B-BD1B-4333-8770-1C112D090154" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Always Include Subscribed Communication Lists
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Include Subscribed Communication Lists", "AlwaysIncludeSubscribedLists", "Always Include Subscribed Communication Lists", @"If campus context filtering is enabled, always include lists the person is subscribed to, even if they don't match the current campus context. Note: Category filtering still applies — lists outside the selected categories will not be shown.", 4, @"True", "BB84A676-737B-43FB-90F9-370E7731E200" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Message Count Window (In Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Message Count Window (In Days)", "MessageCountWindowDays", "Message Count Window (In Days)", @"Set the number of days to look back when calculating how many messages a person has received for the Current Communication Channels section. Set to 0 to disable feature.", 5, @"30", "D7824600-814B-477E-8D0F-CCB0B78498F8" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Manage My Account Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage My Account Page", "ManageMyAccountPage", "Manage My Account Page", @"Select the page that the Manage My Account button should open for a person's account. This button will be shown if a person's account has been deactivated and also for Unsubscribe Mode. Leave blank to hide the button.", 6, @"", "0AB5FAA0-0C6C-449C-A0C1-5D3189B066D6" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribe from Communication List Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Unsubscribe from Communication List Workflow", "UnsubscribeWorkflow", "Unsubscribe from Communication List Workflow", @"Select the workflow type to launch when a person unsubscribes from a communication list. The person will be passed in as the Entity and the communication list ID will be passed to the workflow 'CommunicationListIds' attribute if it exists.", 7, @"", "F17C4C35-06E7-4FE5-96A1-9CBC2A2F17B4" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Available Communication Channels
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Available Communication Channels", "AvailableCommunicationChannels", "Available Communication Channels", @"Choose which communication channels to display in the Available Communication Channels section. Previously Subscribed Channels: Shows all lists where the person was once subscribed, even if currently inactive. All Channels Available: Shows all lists the person has permission to view.", 0, @"All", "DAEF5CD3-0538-4B5F-94A9-692F499CCAE1" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories_PreferenceMode", "Communication List Categories", @"Select which categories of communication lists to display for Preference Mode. If no categories are selected, all lists the person is authorized to view will be shown. Note: category permissions override communication list permissions.", 1, @"A0889E77-67D9-418C-B301-1B3924692058", "66522AF4-22F9-413C-AB1C-3CFCA486D50D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Flow Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication Flow Categories", "CommunicationFlowCategories_PreferenceMode", "Communication Flow Categories", @"Select which categories of communication flows to display in the Current Communication Channels section for Preference Mode. If no categories are selected, all flows the person is in will be shown.", 2, @"", "6DDFAF5D-330B-4E71-9584-57CB66B992AA" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Show Medium Preference On Current Channels
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Medium Preference On Current Channels", "ShowMediumPreference", "Show Medium Preference On Current Channels", @"When enabled, will display the person's current communication medium preference for each list and allow changes.", 3, @"True", "5BFA9C97-EEA5-4C26-9A4C-14E080A1ACEF" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories_UnsubscribeMode", "Communication List Categories", @"Select which categories of communication lists to display for Unsubscribe Mode. If no categories are selected, all lists the person is authorized to view will be shown. Note: category permissions override communication list permissions.", 0, @"A0889E77-67D9-418C-B301-1B3924692058", "10169034-2583-4FB7-AB69-61C8FB698CBC" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Flow Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication Flow Categories", "CommunicationFlowCategories_UnsubscribeMode", "Communication Flow Categories", @"Select which categories of communication flows to display for Unsubscribe Mode. If no categories are selected, all flows the person is in will be shown.", 1, @"", "5E53FE43-ABB9-4BCF-9F3C-5837049BE905" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Allow Deactivating Family
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Deactivating Family", "AllowInactivatingFamily", "Allow Deactivating Family", @"When enabled, if the person chooses to remove their involvement, show the option to deactivate records for the whole family. This will not show if the person is a member of more than one family or is not an adult.", 2, @"True", "304152F9-B4C7-4925-AD87-F24433E77F9F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Inactive Reasons to Exclude
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Inactive Reasons to Exclude", "ReasonstoExclude", "Inactive Reasons to Exclude", @"The inactive reasons to exclude from the list when a person is removing themselves from church involvement.", 3, @"64014FE6-943D-4ACF-8014-FED9F9169AE8,05D35BC4-5816-4210-965F-1BF44F35A16A", "C04A6AC0-6BAC-4750-9F46-B3017026C569" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Opt-Out Options Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Opt-Out Options Description", "OptOutOptionsDescription", "Opt-Out Options Description", @"The text to display for the Opt-Out Options section description. <span class='tip tip-lava'></span>", 0, @"", "693ADE07-3D91-405F-9031-D646D6408D9E" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Current Communication Channels Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Current Communication Channels Description", "CurrentCommunicationChannelsDescription", "Current Communication Channels Description", @"The text to display for the Current Communication Channels section description. <span class='tip tip-lava'></span>", 1, @"These are the communication lists you've subscribed to. You may unsubscribe or update your preferred communication method.", "5E2C4BC7-107B-4886-A686-BED7FDAA16E2" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Available Communication Channels Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Available Communication Channels Description", "AvailableCommunicationChannelsDescription", "Available Communication Channels Description", @"The text to display for the Available Communication Channels section description. <span class='tip tip-lava'></span>", 2, @"These are the communication lists you've subscribed to before, or that you might be interested in subscribing to.", "8D14BAF8-072F-4187-9CC5-AD004DC894BD" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Email Preference Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Email Preference Title", "EmailPreferenceTitle", "Email Preference Title", @"The title text to display for the Email Preference option.", 0, @"Email Preference", "BF45C021-741C-4C86-903F-9CB75A6DD928" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Email Preference Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Email Preference Description", "EmailPreferenceDescription", "Email Preference Description", @"The description text to display for the Email Preference option.", 1, @"Choose how you'd like to receive emails from us. You can update this anytime.", "E14114F1-B9E4-4773-9C07-FD8AB8DA14C0" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Remove Involvement Title", "RemoveInvolvementTitle", "Remove Involvement Title", @"The title text to display for the Remove Involvement option.", 2, @"Remove Me From All Church Involvement", "D40E3A3B-A7BF-48E1-BC60-AA4A0B3F3A1D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Remove Involvement Description", "RemoveInvolvementDescription", "Remove Involvement Description", @"The description text to display for the Remove Involvement option.", 3, @"I'm No Longer Involved With This Church, Please Deactivate My Account", "D4652742-BE32-4016-A6B9-660C7B63ABA9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Preference Mode Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Preference Mode Header Title", "PreferenceModeHeaderTitle", "Preference Mode Header Title", @"The text that is displayed for the header title when navigating to this block from the website.", 0, @"Communication Preferences", "AE2485D1-56B1-4BAB-B0C2-72685A97FB83" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Preference Mode Header Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Preference Mode Header Description", "PreferenceModeHeaderDescription", "Preference Mode Header Description", @"The description text that is displayed under the header title when navigating to this block from the website. <span class='tip tip-lava'></span>", 1, @"Configure below which communications you want to receive from {{ 'Global' | Attribute:'OrganizationName' }}.", "F9D0ECDC-09A3-4A75-860F-3D46769B6A7D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Channel Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unsubscribed From Channel Confirmation Title", "UnsubscribedFromChannelConfirmationTitle", "Unsubscribed From Channel Confirmation Title", @"The text that is displayed for the header title when someone successfully unsubscribes from an email link.", 0, @"You've Been Unsubscribed", "5F8B2A86-8453-40B4-BE06-7F8AADBF893F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Channel Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribed From Channel Confirmation Description", "UnsubscribedFromChannelConfirmationDescription", "Unsubscribed From Channel Confirmation Description", @"The description text that is displayed under the header title when someone successfully unsubscribes from an email link. <span class='tip tip-lava'></span>", 1, @"You've successfully unsubscribed from {{ Channel.Name }} emails. Manage your other email preferences below.", "0248EC77-BD9E-4BCE-A7F7-29AF9EFAAA4F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Bulk Email Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unsubscribed From Bulk Email Confirmation Title", "UnsubscribedFromBulkEmailConfirmationTitle", "Unsubscribed From Bulk Email Confirmation Title", @"The text that is displayed for the header title when someone successfully unsubscribes from all bulk lists. Note: This is triggered when someone unsubscribes from an email link without being a part of any channels.", 2, @"You've Been Unsubscribed From All Bulk Emails", "A0785609-6A08-4828-928A-B99676FCA164" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Bulk Email Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribed From Bulk Email Confirmation Description", "UnsubscribedFromBulkEmailConfirmationDescription", "Unsubscribed From Bulk Email Confirmation Description", @"The description text that is displayed under the header title when someone successfully unsubscribes from all bulk lists. Note: This is triggered when someone unsubscribes from an email link without being a part of any channels. <span class='tip tip-lava'></span>", 3, @"You will no longer receive bulk emails from {{ 'Global' | Attribute:'OrganizationName' }}, but you may still receive individual messages when necessary.", "001B1F86-18B3-4BE4-9997-B94B575D78F2" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From All Emails Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unsubscribed From All Emails Confirmation Title", "UnsubscribedFromAllEmailsConfirmationTitle", "Unsubscribed From All Emails Confirmation Title", @"The text that is displayed for the header title when someone successfully unsubscribes from all emails. Note: This is triggered when someone unsubscribes from an email link who has already unsubscribed from all bulk email lists.", 4, @"You've Been Unsubscribed From All Emails", "1D734638-AD9F-49CA-B1C4-5A308355FDD1" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From All Emails Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribed From All Emails Confirmation Description", "UnsubscribedFromAllEmailsConfirmationDescription", "Unsubscribed From All Emails Confirmation Description", @"The description text that is displayed under the header title when someone successfully unsubscribes from all emails. Note: This is triggered when someone unsubscribes from an email link who has already unsubscribed from all bulk email lists. <span class='tip tip-lava'></span>", 5, @"You will no longer receive any emails from {{ 'Global' | Attribute:'OrganizationName' }}, including announcements, updates, and personal correspondence.", "0876EE81-DEB9-4464-A35E-D15BFB0410F9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Remove Involvement Confirmation Title", "RemoveInvolvementConfirmationTitle", "Remove Involvement Confirmation Title", @"The text that is displayed for the header title when someone requests to be removed from involvement.", 0, @"Confirm Your Request", "7DF63369-967E-483B-A2B3-5D3FC62021A9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Remove Involvement Confirmation Description", "RemoveInvolvementConfirmationDescription", "Remove Involvement Confirmation Description", @"The description text that is displayed under the header title when someone requests to be removed from involvement. <span class='tip tip-lava'></span>", 1, @"Once you confirm, we will update our records, and you will no longer receive official church communications. If there's anything we can do to support you, please let us know.", "51066133-096F-4AFD-BE78-6E3C4444048D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Person Removed Involvement Success Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Removed Involvement Success Title", "PersonRemovedInvolvementSuccessTitle", "Person Removed Involvement Success Title", @"The text that is displayed for the header title when a person has been removed from involvement.", 2, @"Your Request Has Been Completed", "1ADD361B-E030-4CA6-BABD-02AE9E553419" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Person Removed Involvement Success Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Person Removed Involvement Success Description", "PersonRemovedInvolvementSuccessDescription", "Person Removed Involvement Success Description", @"The description text that is displayed under the header title when a person has been removed from involvement. <span class='tip tip-lava'></span>", 3, @"Your personal record at {{ 'Global' | Attribute:'OrganizationName' }} has been deactivated, and you will no longer receive official communications from us. If there's anything else we can do for you, please let us know.", "712AF829-BA21-4985-9C1A-B4285A5C1F45" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Family Removed Involvement Success Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Removed Involvement Success Title", "FamilyRemovedInvolvementSuccessTitle", "Family Removed Involvement Success Title", @"The text that is displayed for the header title when a person and their family have been removed from involvement.", 4, @"Your Request Has Been Completed", "8FEE16B4-F46B-4508-AC91-E4B01629A33B" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Family Removed Involvement Success Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Family Removed Involvement Success Description", "FamilyRemovedInvolvementSuccessDescription", "Family Removed Involvement Success Description", @"The description text that is displayed under the header title when a person and their family have been removed from involvement. <span class='tip tip-lava'></span>", 5, @"Your personal record, along with your family's records, has been deactivated at {{ 'Global' | Attribute:'OrganizationName' }}. You will no longer receive official communications from us. If there's anything else we can do for you, please let us know.", "D1C1CA17-B9C4-416D-A039-D225F931214C" );
        }

        /// <summary>
        /// JPH: Add refreshed Email Preference Entry block type and settings - down.
        /// </summary>
        private void JPH_AddRefreshedEmailPreferenceEntryBlockType_20250904_Down()
        {
            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Family Removed Involvement Success Description
            RockMigrationHelper.DeleteAttribute( "D1C1CA17-B9C4-416D-A039-D225F931214C" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Family Removed Involvement Success Title
            RockMigrationHelper.DeleteAttribute( "8FEE16B4-F46B-4508-AC91-E4B01629A33B" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Person Removed Involvement Success Description
            RockMigrationHelper.DeleteAttribute( "712AF829-BA21-4985-9C1A-B4285A5C1F45" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Person Removed Involvement Success Title
            RockMigrationHelper.DeleteAttribute( "1ADD361B-E030-4CA6-BABD-02AE9E553419" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Confirmation Description
            RockMigrationHelper.DeleteAttribute( "51066133-096F-4AFD-BE78-6E3C4444048D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Confirmation Title
            RockMigrationHelper.DeleteAttribute( "7DF63369-967E-483B-A2B3-5D3FC62021A9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From All Emails Confirmation Description
            RockMigrationHelper.DeleteAttribute( "0876EE81-DEB9-4464-A35E-D15BFB0410F9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From All Emails Confirmation Title
            RockMigrationHelper.DeleteAttribute( "1D734638-AD9F-49CA-B1C4-5A308355FDD1" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Bulk Email Confirmation Description
            RockMigrationHelper.DeleteAttribute( "001B1F86-18B3-4BE4-9997-B94B575D78F2" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Bulk Email Confirmation Title
            RockMigrationHelper.DeleteAttribute( "A0785609-6A08-4828-928A-B99676FCA164" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Channel Confirmation Description
            RockMigrationHelper.DeleteAttribute( "0248EC77-BD9E-4BCE-A7F7-29AF9EFAAA4F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Channel Confirmation Title
            RockMigrationHelper.DeleteAttribute( "5F8B2A86-8453-40B4-BE06-7F8AADBF893F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Preference Mode Header Description
            RockMigrationHelper.DeleteAttribute( "F9D0ECDC-09A3-4A75-860F-3D46769B6A7D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Preference Mode Header Title
            RockMigrationHelper.DeleteAttribute( "AE2485D1-56B1-4BAB-B0C2-72685A97FB83" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Description
            RockMigrationHelper.DeleteAttribute( "D4652742-BE32-4016-A6B9-660C7B63ABA9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Title
            RockMigrationHelper.DeleteAttribute( "D40E3A3B-A7BF-48E1-BC60-AA4A0B3F3A1D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Email Preference Description
            RockMigrationHelper.DeleteAttribute( "E14114F1-B9E4-4773-9C07-FD8AB8DA14C0" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Email Preference Title
            RockMigrationHelper.DeleteAttribute( "BF45C021-741C-4C86-903F-9CB75A6DD928" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Available Communication Channels Description
            RockMigrationHelper.DeleteAttribute( "8D14BAF8-072F-4187-9CC5-AD004DC894BD" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Current Communication Channels Description
            RockMigrationHelper.DeleteAttribute( "5E2C4BC7-107B-4886-A686-BED7FDAA16E2" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Opt-Out Options Description
            RockMigrationHelper.DeleteAttribute( "693ADE07-3D91-405F-9031-D646D6408D9E" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Inactive Reasons to Exclude
            RockMigrationHelper.DeleteAttribute( "C04A6AC0-6BAC-4750-9F46-B3017026C569" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Allow Deactivating Family
            RockMigrationHelper.DeleteAttribute( "304152F9-B4C7-4925-AD87-F24433E77F9F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Flow Categories
            RockMigrationHelper.DeleteAttribute( "5E53FE43-ABB9-4BCF-9F3C-5837049BE905" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication List Categories
            RockMigrationHelper.DeleteAttribute( "10169034-2583-4FB7-AB69-61C8FB698CBC" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Show Medium Preference On Current Channels
            RockMigrationHelper.DeleteAttribute( "5BFA9C97-EEA5-4C26-9A4C-14E080A1ACEF" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Flow Categories
            RockMigrationHelper.DeleteAttribute( "6DDFAF5D-330B-4E71-9584-57CB66B992AA" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication List Categories
            RockMigrationHelper.DeleteAttribute( "66522AF4-22F9-413C-AB1C-3CFCA486D50D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Available Communication Channels
            RockMigrationHelper.DeleteAttribute( "DAEF5CD3-0538-4B5F-94A9-692F499CCAE1" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribe from Communication List Workflow
            RockMigrationHelper.DeleteAttribute( "F17C4C35-06E7-4FE5-96A1-9CBC2A2F17B4" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Manage My Account Page
            RockMigrationHelper.DeleteAttribute( "0AB5FAA0-0C6C-449C-A0C1-5D3189B066D6" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Message Count Window (In Days)
            RockMigrationHelper.DeleteAttribute( "D7824600-814B-477E-8D0F-CCB0B78498F8" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Always Include Subscribed Communication Lists
            RockMigrationHelper.DeleteAttribute( "BB84A676-737B-43FB-90F9-370E7731E200" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Filter Communication Lists by Campus Context
            RockMigrationHelper.DeleteAttribute( "96E9F09B-BD1B-4333-8770-1C112D090154" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Exclude Communication Flows
            RockMigrationHelper.DeleteAttribute( "1F915CAE-2247-4C14-8C11-AEF74374A3BD" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Show Header Icon
            RockMigrationHelper.DeleteAttribute( "4F09BE35-8D30-46D1-8191-83D185FC215A" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Configuration Options
            RockMigrationHelper.DeleteAttribute( "6A463D8E-DA08-48A3-A6E3-331F81D94F02" );

            // Delete BlockType 
            //   Name: Email Preference Entry
            //   Category: Communication
            //   Path: -
            //   EntityType: Email Preference Entry
            RockMigrationHelper.DeleteBlockType( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F" );

            // Delete Block EntityType: Rock.Blocks.Communication.EmailPreferenceEntry
            RockMigrationHelper.DeleteEntityType( "E80E5BA7-40C8-4E99-B2F7-0B668012521F" );
        }
    }
}
