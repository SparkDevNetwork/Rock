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
    
    /// <summary>
    ///
    /// </summary>
    public partial class ChopEmailPreferenceEntry : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChopBlocksForV18Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.EmailPreferenceEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.EmailPreferenceEntry", "Email Preference Entry", "Rock.Blocks.Communication.EmailPreferenceEntry, Rock.Blocks, Version=18.0.12.0, Culture=neutral, PublicKeyToken=null", false, false, "E80E5BA7-40C8-4E99-B2F7-0B668012521F" );

            // Add/Update Obsidian Block Type
            //   Name:Email Preference Entry
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.EmailPreferenceEntry
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Email Preference Entry", "Allows a person to set their email preferences and manage their subscriptions to communication channels.", "Rock.Blocks.Communication.EmailPreferenceEntry", "Communication", "FF88B243-4580-4BE0-97B1-BA9895C3FB8F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Allow Deactivating Family
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Deactivating Family", "AllowInactivatingFamily", "Allow Deactivating Family", @"When enabled, if the person chooses to remove their involvement, show the option to deactivate records for the whole family. This will not show if the person is a member of more than one family or is not an adult.", 2, @"True", "304152F9-B4C7-4925-AD87-F24433E77F9F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Always Include Subscribed Communication Lists
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Include Subscribed Communication Lists", "AlwaysIncludeSubscribedLists", "Always Include Subscribed Communication Lists", @"If campus context filtering is enabled, always include lists the person is subscribed to, even if they don't match the current campus context. Note: Category filtering still applies — lists outside the selected categories will not be shown.", 4, @"True", "BB84A676-737B-43FB-90F9-370E7731E200" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Available Communication Channels Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Available Communication Channels Description", "AvailableCommunicationChannelsDescription", "Available Communication Channels Description", @"The text to display for the Available Communication Channels section description. <span class='tip tip-lava'></span>", 2, @"These are the communication lists you've subscribed to before, or that you might be interested in subscribing to.", "8D14BAF8-072F-4187-9CC5-AD004DC894BD" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Available Communication Channels
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Available Communication Channels", "AvailableCommunicationChannels", "Available Communication Channels", @"Choose which communication channels to display in the Available Communication Channels section. Previously Subscribed Channels: Shows all lists where the person was once subscribed, even if currently inactive. All Channels Available: Shows all lists the person has permission to view.", 0, @"All", "DAEF5CD3-0538-4B5F-94A9-692F499CCAE1" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Configuration Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Communication Configuration Options", "AvailableOptions", "Communication Configuration Options", @"Select the options that should be available to a person when they are updating their communication preferences.", 0, @"Update Email Address,Unsubscribe,Not Involved", "6A463D8E-DA08-48A3-A6E3-331F81D94F02" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Flow Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication Flow Categories", "CommunicationFlowCategories_PreferenceMode", "Communication Flow Categories", @"Select which categories of communication flows to display in the Current Communication Channels section for Preference Mode. If no categories are selected, all flows the person is in will be shown.", 2, @"", "6DDFAF5D-330B-4E71-9584-57CB66B992AA" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication Flow Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication Flow Categories", "CommunicationFlowCategories_UnsubscribeMode", "Communication Flow Categories", @"Select which categories of communication flows to display for Unsubscribe Mode. If no categories are selected, all flows the person is in will be shown.", 1, @"", "5E53FE43-ABB9-4BCF-9F3C-5837049BE905" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories_PreferenceMode", "Communication List Categories", @"Select which categories of communication lists to display for Preference Mode. If no categories are selected, all lists the person is authorized to view will be shown. Note: category permissions override communication list permissions.", 1, @"A0889E77-67D9-418C-B301-1B3924692058", "66522AF4-22F9-413C-AB1C-3CFCA486D50D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories_UnsubscribeMode", "Communication List Categories", @"Select which categories of communication lists to display for Unsubscribe Mode. If no categories are selected, all lists the person is authorized to view will be shown. Note: category permissions override communication list permissions.", 0, @"A0889E77-67D9-418C-B301-1B3924692058", "10169034-2583-4FB7-AB69-61C8FB698CBC" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Current Communication Channels Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Current Communication Channels Description", "CurrentCommunicationChannelsDescription", "Current Communication Channels Description", @"The text to display for the Current Communication Channels section description. <span class='tip tip-lava'></span>", 1, @"These are the communication lists you've subscribed to. You may unsubscribe or update your preferred communication method.", "5E2C4BC7-107B-4886-A686-BED7FDAA16E2" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Email Preference Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Email Preference Description", "EmailPreferenceDescription", "Email Preference Description", @"The description text to display for the Email Preference option.", 1, @"Choose how you'd like to receive emails from us. You can update this anytime.", "E14114F1-B9E4-4773-9C07-FD8AB8DA14C0" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Email Preference Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Email Preference Title", "EmailPreferenceTitle", "Email Preference Title", @"The title text to display for the Email Preference option.", 0, @"Email Preference", "BF45C021-741C-4C86-903F-9CB75A6DD928" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Exclude Communication Flows
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Communication Flows", "ExcludeCommunicationFlows", "Exclude Communication Flows", @"When enabled, communication flows will not be displayed in the Current or Available Communication Channels sections.", 2, @"False", "1F915CAE-2247-4C14-8C11-AEF74374A3BD" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Family Removed Involvement Success Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Family Removed Involvement Success Description", "FamilyRemovedInvolvementSuccessDescription", "Family Removed Involvement Success Description", @"The description text that is displayed under the header title when a person and their family have been removed from involvement. <span class='tip tip-lava'></span>", 5, @"Your personal record, along with your family's records, has been deactivated at {{ 'Global' | Attribute:'OrganizationName' }}. You will no longer receive official communications from us. If there's anything else we can do for you, please let us know.", "D1C1CA17-B9C4-416D-A039-D225F931214C" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Family Removed Involvement Success Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Removed Involvement Success Title", "FamilyRemovedInvolvementSuccessTitle", "Family Removed Involvement Success Title", @"The text that is displayed for the header title when a person and their family have been removed from involvement.", 4, @"Your Request Has Been Completed", "8FEE16B4-F46B-4508-AC91-E4B01629A33B" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Filter Communication Lists by Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Communication Lists by Campus Context", "FilterGroupsByCampusContext", "Filter Communication Lists by Campus Context", @"When enabled, all communication lists will be filtered by the campus context of the page. Lists with no campus will always be shown.", 3, @"False", "96E9F09B-BD1B-4333-8770-1C112D090154" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Inactive Reasons to Exclude
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Inactive Reasons to Exclude", "ReasonstoExclude", "Inactive Reasons to Exclude", @"The inactive reasons to exclude from the list when a person is removing themselves from church involvement.", 3, @"64014FE6-943D-4ACF-8014-FED9F9169AE8,05D35BC4-5816-4210-965F-1BF44F35A16A", "C04A6AC0-6BAC-4750-9F46-B3017026C569" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Manage My Account Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage My Account Page", "ManageMyAccountPage", "Manage My Account Page", @"Select the page that the Manage My Account button should open for a person's account. This button will be shown if a person's account has been deactivated and also for Unsubscribe Mode. Leave blank to hide the button.", 6, @"", "0AB5FAA0-0C6C-449C-A0C1-5D3189B066D6" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Message Count Window (In Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Message Count Window (In Days)", "MessageCountWindowDays", "Message Count Window (In Days)", @"Set the number of days to look back when calculating how many messages a person has received for the Current Communication Channels section. Set to 0 to disable feature.", 5, @"30", "D7824600-814B-477E-8D0F-CCB0B78498F8" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Opt-Out Options Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Opt-Out Options Description", "OptOutOptionsDescription", "Opt-Out Options Description", @"The text to display for the Opt-Out Options section description. <span class='tip tip-lava'></span>", 0, @"", "693ADE07-3D91-405F-9031-D646D6408D9E" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Person Removed Involvement Success Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Person Removed Involvement Success Description", "PersonRemovedInvolvementSuccessDescription", "Person Removed Involvement Success Description", @"The description text that is displayed under the header title when a person has been removed from involvement. <span class='tip tip-lava'></span>", 3, @"Your personal record at {{ 'Global' | Attribute:'OrganizationName' }} has been deactivated, and you will no longer receive official communications from us. If there's anything else we can do for you, please let us know.", "712AF829-BA21-4985-9C1A-B4285A5C1F45" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Person Removed Involvement Success Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Removed Involvement Success Title", "PersonRemovedInvolvementSuccessTitle", "Person Removed Involvement Success Title", @"The text that is displayed for the header title when a person has been removed from involvement.", 2, @"Your Request Has Been Completed", "1ADD361B-E030-4CA6-BABD-02AE9E553419" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Preference Mode Header Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Preference Mode Header Description", "PreferenceModeHeaderDescription", "Preference Mode Header Description", @"The description text that is displayed under the header title when navigating to this block from the website. <span class='tip tip-lava'></span>", 1, @"Configure below which communications you want to receive from {{ 'Global' | Attribute:'OrganizationName' }}.", "F9D0ECDC-09A3-4A75-860F-3D46769B6A7D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Preference Mode Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Preference Mode Header Title", "PreferenceModeHeaderTitle", "Preference Mode Header Title", @"The text that is displayed for the header title when navigating to this block from the website.", 0, @"Communication Preferences", "AE2485D1-56B1-4BAB-B0C2-72685A97FB83" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Remove Involvement Confirmation Description", "RemoveInvolvementConfirmationDescription", "Remove Involvement Confirmation Description", @"The description text that is displayed under the header title when someone requests to be removed from involvement. <span class='tip tip-lava'></span>", 1, @"Once you confirm, we will update our records, and you will no longer receive official church communications. If there's anything we can do to support you, please let us know.", "51066133-096F-4AFD-BE78-6E3C4444048D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Remove Involvement Confirmation Title", "RemoveInvolvementConfirmationTitle", "Remove Involvement Confirmation Title", @"The text that is displayed for the header title when someone requests to be removed from involvement.", 0, @"Confirm Your Request", "7DF63369-967E-483B-A2B3-5D3FC62021A9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Remove Involvement Description", "RemoveInvolvementDescription", "Remove Involvement Description", @"The description text to display for the Remove Involvement option.", 3, @"I'm No Longer Involved With This Church, Please Deactivate My Account", "D4652742-BE32-4016-A6B9-660C7B63ABA9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Remove Involvement Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Remove Involvement Title", "RemoveInvolvementTitle", "Remove Involvement Title", @"The title text to display for the Remove Involvement option.", 2, @"Remove Me From All Church Involvement", "D40E3A3B-A7BF-48E1-BC60-AA4A0B3F3A1D" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Show Header Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Header Icon", "ShowHeaderIcon", "Show Header Icon", @"When enabled, a default icon will be shown beside the header title.", 1, @"True", "4F09BE35-8D30-46D1-8191-83D185FC215A" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Show Medium Preference On Current Channels
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Medium Preference On Current Channels", "ShowMediumPreference", "Show Medium Preference On Current Channels", @"When enabled, will display the person's current communication medium preference for each list and allow changes.", 3, @"True", "5BFA9C97-EEA5-4C26-9A4C-14E080A1ACEF" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribe from Communication List Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Unsubscribe from Communication List Workflow", "UnsubscribeWorkflow", "Unsubscribe from Communication List Workflow", @"Select the workflow type to launch when a person unsubscribes from a communication list. The person will be passed in as the Entity and the communication list ID will be passed to the workflow 'CommunicationListIds' attribute if it exists.", 7, @"", "F17C4C35-06E7-4FE5-96A1-9CBC2A2F17B4" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From All Emails Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribed From All Emails Confirmation Description", "UnsubscribedFromAllEmailsConfirmationDescription", "Unsubscribed From All Emails Confirmation Description", @"The description text that is displayed under the header title when someone successfully unsubscribes from all emails. Note: This is triggered when someone unsubscribes from an email link who has already unsubscribed from all bulk email lists. <span class='tip tip-lava'></span>", 5, @"You will no longer receive any emails from {{ 'Global' | Attribute:'OrganizationName' }}, including announcements, updates, and personal correspondence.", "0876EE81-DEB9-4464-A35E-D15BFB0410F9" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From All Emails Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unsubscribed From All Emails Confirmation Title", "UnsubscribedFromAllEmailsConfirmationTitle", "Unsubscribed From All Emails Confirmation Title", @"The text that is displayed for the header title when someone successfully unsubscribes from all emails. Note: This is triggered when someone unsubscribes from an email link who has already unsubscribed from all bulk email lists.", 4, @"You've Been Unsubscribed From All Emails", "1D734638-AD9F-49CA-B1C4-5A308355FDD1" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Bulk Email Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribed From Bulk Email Confirmation Description", "UnsubscribedFromBulkEmailConfirmationDescription", "Unsubscribed From Bulk Email Confirmation Description", @"The description text that is displayed under the header title when someone successfully unsubscribes from all bulk lists. Note: This is triggered when someone unsubscribes from an email link without being a part of any channels. <span class='tip tip-lava'></span>", 3, @"You will no longer receive bulk emails from {{ 'Global' | Attribute:'OrganizationName' }}, but you may still receive individual messages when necessary.", "001B1F86-18B3-4BE4-9997-B94B575D78F2" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Bulk Email Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unsubscribed From Bulk Email Confirmation Title", "UnsubscribedFromBulkEmailConfirmationTitle", "Unsubscribed From Bulk Email Confirmation Title", @"The text that is displayed for the header title when someone successfully unsubscribes from all bulk lists. Note: This is triggered when someone unsubscribes from an email link without being a part of any channels.", 2, @"You've Been Unsubscribed From All Bulk Emails", "A0785609-6A08-4828-928A-B99676FCA164" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Channel Confirmation Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribed From Channel Confirmation Description", "UnsubscribedFromChannelConfirmationDescription", "Unsubscribed From Channel Confirmation Description", @"The description text that is displayed under the header title when someone successfully unsubscribes from an email link. <span class='tip tip-lava'></span>", 1, @"You've successfully unsubscribed from {{ Channel.Name }} emails. Manage your other email preferences below.", "0248EC77-BD9E-4BCE-A7F7-29AF9EFAAA4F" );

            // Attribute for BlockType
            //   BlockType: Email Preference Entry
            //   Category: Communication
            //   Attribute: Unsubscribed From Channel Confirmation Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FF88B243-4580-4BE0-97B1-BA9895C3FB8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unsubscribed From Channel Confirmation Title", "UnsubscribedFromChannelConfirmationTitle", "Unsubscribed From Channel Confirmation Title", @"The text that is displayed for the header title when someone successfully unsubscribes from an email link.", 0, @"You've Been Unsubscribed", "5F8B2A86-8453-40B4-BE06-7F8AADBF893F" );
        }

        private void ChopBlockTypesv18_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 18.0 (18.0.13)",
                blockTypeReplacements: new Dictionary<string, string> {
                // blocks chopped in v18.0.13
{ "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "FF88B243-4580-4BE0-97B1-BA9895C3FB8F" }, // Email Preference Entry ( Communication )
                // blocks chopped in v18.0.11
{ "0926B82C-CBA2-4943-962E-F788C8A80037", "000ca534-6164-485e-b405-ba0fa6ae92f9" }, // Binary File Type List ( Core )
{ "508DA252-F94C-4641-8579-458D8FCE14B2", "b52e7cae-c5cc-41cb-a5ec-1cf027074a2c" }, // Metric Value Detail ( Reporting )
{ "56ABBD0F-8F62-4094-88B3-161E71F21419", "c3544f53-8e2d-43d6-b165-8fefc541a4eb" }, // Communication List ( Communication )
{ "ACF84335-34A1-4DD6-B242-20119B8D0967", "6bc7da76-1a19-4685-b50a-dfd7eaa5ce33" }, // Schedule Category Exclusion List ( Core )
{ "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F" }, // Connection Opportunity Signup ( Connection )
{ "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "2B63C6ED-20D5-467E-9A6A-C608E1D953E5" }, // Communication Detail ( Communication )
{ "D9302E4A-C498-4CD7-8D3B-0E9DA9802DD5", "66f5882f-163c-4616-9b39-2f063611db22" }, // Bulk Import ( Bulk Import )
{ "DA102F02-6DBB-42E6-BFEE-360E137B1411", "740f7de3-d5f5-4eeb-beee-99c3bfb23b52" }, // Merge Template List ( Core )
{ "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "3f997da7-ac42-41c9-97f1-2069bb9d9e5c" }, // Prayer Comment List ( Core )
                // blocks chopped in v18.0.10
{ "3131C55A-8753-435F-85F3-DF777EFBD1C8", "8adb5c0d-9a4f-4396-ab0f-deb552c094e1" }, // Benevolence Request List ( Finance )
{ "41AE0574-BE1E-4656-B45D-2CB734D1BE30", "6e9672e6-ee42-4aac-b0a9-b041c3b8368c" }, // Nameless Person List ( CRM )
{ "63ADDB5A-75D6-4E86-A031-98B3451C49A3", "6c824483-6624-460b-9dd8-e127b25ca65d" }, // Web Farm Node Log List ( WebFarm )
{ "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "1c3d7f3d-e8c7-4f27-871c-7ec20483b416" }, // Block Type List ( CMS )
{ "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" }, // Group Schedule Toolbox ( Group Scheduling )
{ "95F38562-6CEF-4798-8A4F-05EBCDFB07E0", "6bba1fc0-ac56-4e58-9e99-eb20da7aa415" }, // Web Farm Node Detail ( WebFarm )
{ "A3E648CC-0F19-455F-AF1D-B70A8205802D", "6c329001-9c04-4090-bed0-12e3f6b88fb6" }, // Block Type Detail ( CMS )
{ "C4191011-0391-43DF-9A9D-BE4987C679A4", "e1dce349-2f5b-46ed-9f3d-8812af857f69" }, // Bank Account List ( Finance )
{ "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "52df00e5-bc19-43f2-8533-a386db53c74f" }, // Campus List ( Core )
{ "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "e20b2fe2-2708-4e9a-b9fb-b370e8b0e702" }, // Saved Account List ( Finance )
                    // blocks chopped in v18.0.8
{ "250FF1C3-B2AE-4AFD-BEFA-29C45BEB30D2", "770d3039-3f07-4d6f-a64e-c164acce93e1" }, // Signal Type List ( Core )
{ "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "633a75a7-7186-4cfd-ab80-6f2237f0bdd8" }, // AI Provider List ( Core )
{ "88820905-1B5A-4B82-8E56-F9A0736A0E98", "13f49f94-d9bc-434a-bb20-a6ba87bbe81f" }, // AI Provider Detail ( Core )
{ "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "120552e2-5c36-4220-9a73-fbbbd75b0964" }, // Audit List ( Core )
                    // blocks chopped in v18.0
{ "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "e44cac85-346f-41a4-884b-a6fb5fc64de1" }, // Page Short Link Click List ( CMS )
{ "4C4A46CD-1622-4642-A655-11585C5D3D31", "eddfcaff-70aa-4791-b051-6567b37518c4" }, // Achievement Type Detail ( Achievements )
{ "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "fbe75c18-7f71-4d23-a546-7a17cf944ba6" }, // Achievement Attempt Detail ( Engagement )
{ "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "b294c1b9-8368-422c-8054-9672c7f41477" }, // Achievement Attempt List ( Achievements )
{ "C26C7979-81C1-4A20-A167-35415CD7FED3", "09FD3746-48D1-4B94-AAA9-6896443AA43E" }, // Lava Shortcode List ( CMS )
{ "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "4acfbf3f-3d49-4ae3-b468-529f79da9898" }, // Achievement Type List ( Streaks )
{ "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "d25ff675-07c8-4e2d-a3fa-38ba3468b4ae" }, // Page Short Link List ( CMS )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_180_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "SeriesColors" }, // Communication Detail ( Communication )
                { "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "FutureWeeksToShow,SignupInstructions,EnableSignup,DeclineReasonPage" }, // Group Schedule Toolbox    
                { "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "SetPageTitle,EnableDebug,2E6540EA-63F0-40FE-BE50-F2A84735E600,8522BADD-2871-45A5-81DD-C76DA07E2E7E" }, // Connection Opportunity Signup
                { "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "GroupCategoryId" }, // Prayer Comment List ( Core )
                { "3131C55A-8753-435F-85F3-DF777EFBD1C8", "DetailPage,CaseWorkerGroup" }, // Benevolence Request List ( Finance )
                { "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "StreakPage" }, // Achievement Attempt Detail ( Engagement )
                { "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "UnsubscribefromListsText,UpdateEmailAddressText,EmailsAllowedText,NoMassEmailsText,NoEmailsText,NotInvolvedText,SuccessText,UnsubscribeSuccessText,CommunicationListCategories" } // Email Preference Entry ( Communication )
            } );
        }

        private void ChopBlocksForV18Up()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv18_0();
        }
    }
}
