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
    public partial class CodeGenerated_20241205 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LogSettings
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LogSettings", "Log Settings", "Rock.Blocks.Cms.LogSettings, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "E5F272D4-E63F-46E7-9429-0D62CB458FD1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.TagList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.TagList", "Tag List", "Rock.Blocks.Core.TagList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "9A396390-842F-4408-AEFD-FB4793F9EF7E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.BadgeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.BadgeList", "Badge List", "Rock.Blocks.Crm.BadgeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "A42A7EAC-C24C-4B6E-8870-762B4C64A97C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupMemberScheduleTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.Scheduling.GroupMemberScheduleTemplateList", "Group Member Schedule Template List", "Rock.Blocks.Group.Scheduling.GroupMemberScheduleTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "9FAAC3E9-01DD-4FED-AF85-01817CDEBF83" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StreakTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakTypeList", "Streak Type List", "Rock.Blocks.Engagement.StreakTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "FB234106-94FD-4206-AA85-4377F1D2C512" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduledJobHistoryList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduledJobHistoryList", "Scheduled Job History List", "Rock.Blocks.Core.ScheduledJobHistoryList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "4B46834F-C9D3-43F3-9DE2-8990D3A232C2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.PersonSuggestionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.PersonSuggestionList", "Person Suggestion List", "Rock.Blocks.Core.PersonSuggestionList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "FD10B5B8-494C-4665-8F1F-8F92F2194F7E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelItemList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelItemList", "Content Channel Item List", "Rock.Blocks.Cms.ContentChannelItemList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "5597BADD-BB0E-4BCD-BE1F-5ACF230CF428" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.MetricValueDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.MetricValueDetail", "Metric Value Detail", "Rock.Blocks.Reporting.MetricValueDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "AF69AA1A-3EEE-4F25-8014-1A02BA82AC32" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StepParticipantList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StepParticipantList", "Step Participant List", "Rock.Blocks.Engagement.StepParticipantList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "E7EB8F39-AE85-4F9C-8AFB-18B3E3C6C570" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AttributeMatrixTemplateList", "Attribute Matrix Template List", "Rock.Blocks.Core.AttributeMatrixTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "A1D4E3E2-60A6-4815-9984-F87DD4741AAF" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.BlockList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.BlockList", "Block List", "Rock.Blocks.Cms.BlockList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "9CF1AA10-24E4-4530-A345-57DA4CFE9595" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialStatementTemplateList", "Financial Statement Template List", "Rock.Blocks.Finance.FinancialStatementTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "F46CD5A7-BAF5-4EEB-8154-A4F4AC886264" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceActiveList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationInstanceActiveList", "Registration Instance Active List", "Rock.Blocks.Event.RegistrationInstanceActiveList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "3951453C-E9FC-4F43-8B7B-794C5ACFCABE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.Disc
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.Disc", "Disc", "Rock.Blocks.Crm.Disc, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "5D8108B4-4877-4214-819F-78CA058A82E0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageList", "Page List", "Rock.Blocks.Cms.PageList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "B49F5C5B-95D4-448D-8A82-BE7661E4FF1D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageShortLinkList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageShortLinkList", "Page Short Link List", "Rock.Blocks.Cms.PageShortLinkList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "B9825E53-D074-4280-A1A3-E20771E34625" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationEntry", "Communication Entry", "Rock.Blocks.Communication.CommunicationEntry, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "26C0C9A1-1383-48D5-A062-E05622A1CBF2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelList", "Content Channel List", "Rock.Blocks.Cms.ContentChannelList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "DE1AB18E-C973-4333-832E-A8B4754F0571" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.BinaryFileList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.BinaryFileList", "Binary File List", "Rock.Blocks.Core.BinaryFileList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "67D1CC46-C871-46E7-AFFD-1B1B23EEEA84" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.BinaryFileTypeDetail", "Binary File Type Detail", "Rock.Blocks.Core.BinaryFileTypeDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "B2C1F7F4-4810-4B34-9FB6-9E6D6DEBE4C9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.EventCalendarItemList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.EventCalendarItemList", "Event Calendar Item List", "Rock.Blocks.Event.EventCalendarItemList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "CA712211-3076-48BD-9321-2B7CEE1D5961" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementTypeDetail", "Achievement Type Detail", "Rock.Blocks.Engagement.AchievementTypeDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "8B22D387-C8F3-41FF-99EF-EE4F088610A1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StepProgramList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StepProgramList", "Step Program List", "Rock.Blocks.Engagement.StepProgramList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "EF0D9904-48BE-4BA5-9950-E77D318A4CFA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.AssessmentTypeList", "Assessment Type List", "Rock.Blocks.Crm.AssessmentTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "26DD8B62-5826-44A9-82B1-C6E4E4AB61D0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StepTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StepTypeList", "Step Type List", "Rock.Blocks.Engagement.StepTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "F3A7B501-61C4-4784-8F73-958E2F1FC353" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AssetStorageProviderList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AssetStorageProviderList", "Asset Storage Provider List", "Rock.Blocks.Core.AssetStorageProviderList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "172E0874-E30F-4FD1-A340-99A8134D9779" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.MediaFolderList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.MediaFolderList", "Media Folder List", "Rock.Blocks.Cms.MediaFolderList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "AF4FA9D1-C8E7-47A6-A522-D40A7370517C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.BlockTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.BlockTypeList", "Block Type List", "Rock.Blocks.Cms.BlockTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "8FCEE05F-6757-4B16-8718-63CD80FF07D6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.PersonFollowingList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.PersonFollowingList", "Person Following List", "Rock.Blocks.Core.PersonFollowingList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "030B944D-66B5-4EDB-AA38-10081E2ACFB6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.AppleTvPageList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.AppleTvPageList", "Apple Tv Page List", "Rock.Blocks.Tv.AppleTvPageList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "4E89A96E-88A2-4CA4-A86B-B9FFDCACF49F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialPersonBankAccountList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPersonBankAccountList", "Financial Person Bank Account List", "Rock.Blocks.Finance.FinancialPersonBankAccountList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "30150FA5-A4E9-4767-A320-C9092B8FFD61" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DeviceList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DeviceList", "Device List", "Rock.Blocks.Core.DeviceList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "DC43AE74-09D8-4080-9074-2CA91B6119D2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialScheduledTransactionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialScheduledTransactionList", "Financial Scheduled Transaction List", "Rock.Blocks.Finance.FinancialScheduledTransactionList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "946127EC-ADEC-46C9-8181-A405C137A8A3" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationList", "Communication List", "Rock.Blocks.Communication.CommunicationList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "E4BD5CAD-579E-476D-87EC-989DE975BB60" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementAttemptList", "Achievement Attempt List", "Rock.Blocks.Engagement.AchievementAttemptList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "039C87AE-0835-4844-AC9B-A66AE1D19530" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupTypeList", "Group Type List", "Rock.Blocks.Group.GroupTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "562ED873-BD66-4287-AE9F-D7C43FECD7A8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialGatewayList", "Financial Gateway List", "Rock.Blocks.Finance.FinancialGatewayList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "9158F560-4EAE-4E1D-80FF-DA24C351E241" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageShortLinkClickList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageShortLinkClickList", "Page Short Link Click List", "Rock.Blocks.Cms.PageShortLinkClickList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "AA860DC7-D590-4D0E-BBB3-16990F2CD680" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.NamelessPersonList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.NamelessPersonList", "Nameless Person List", "Rock.Blocks.Crm.NamelessPersonList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "911EA779-AC00-4A93-B706-B6A642C727CB" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.MediaAccountList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.MediaAccountList", "Media Account List", "Rock.Blocks.Cms.MediaAccountList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "4B445E33-8AE3-4831-A5DC-88ED46D1CCEA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.BinaryFileTypeList", "Binary File Type List", "Rock.Blocks.Core.BinaryFileTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "94AC60CE-B192-4559-88A0-AF0CC143F631" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignalTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignalTypeList", "Signal Type List", "Rock.Blocks.Core.SignalTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "2D9562D6-D28D-4515-8CA6-A2955E0ACE23" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelDetail", "Content Channel Detail", "Rock.Blocks.Cms.ContentChannelDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "C7C776C4-F1DB-477D-87E3-62F8F82BA773" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupMemberScheduleTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupMemberScheduleTemplateDetail", "Group Member Schedule Template Detail", "Rock.Blocks.Group.GroupMemberScheduleTemplateDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "611BAAB0-FEF9-4E01-A0EA-688C7D4549CE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StepTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StepTypeDetail", "Step Type Detail", "Rock.Blocks.Engagement.StepTypeDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "458B0A6C-73D6-456A-9A94-56B5AE3F0592" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.FileAssetManager
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.FileAssetManager", "File Asset Manager", "Rock.Blocks.Cms.FileAssetManager, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "E357AD54-1725-48B8-997C-23C2587800FB" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SmsPipelineList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SmsPipelineList", "Sms Pipeline List", "Rock.Blocks.Communication.SmsPipelineList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "8141535C-4EBB-490F-875F-C62C1F7F4D00" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DefinedTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DefinedTypeList", "Defined Type List", "Rock.Blocks.Core.DefinedTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "6508DCC1-ADA8-4299-9147-DC37095C2AFF" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.InteractionComponentDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.InteractionComponentDetail", "Interaction Component Detail", "Rock.Blocks.Reporting.InteractionComponentDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "29E5A6BF-FE7F-406E-AFC1-64EAB506DDB0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentTemplateList", "Signature Document Template List", "Rock.Blocks.Core.SignatureDocumentTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "8FAE9715-89F1-4FAA-A35F-18CB55E269C0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelTypeList", "Content Channel Type List", "Rock.Blocks.Cms.ContentChannelTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "AFB54433-A564-4E77-A10C-8946FF9D9EC6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.PersistedDataViewList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.PersistedDataViewList", "Persisted Data View List", "Rock.Blocks.Reporting.PersistedDataViewList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "E1C5FBEB-7E0A-496F-97B2-38E6EC8D5B84" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.ConnectionOpportunityList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.ConnectionOpportunityList", "Connection Opportunity List", "Rock.Blocks.Engagement.ConnectionOpportunityList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "02713F10-E574-45E0-9178-A02F7957B3A4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BusinessList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BusinessList", "Business List", "Rock.Blocks.Finance.BusinessList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "1214E9D9-3D0C-49AD-BD99-58C427A8A7D2" );

            // Add/Update Obsidian Block Type
            //   Name:Block List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.BlockList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Block List", "Displays a list of blocks.", "Rock.Blocks.Cms.BlockList", "CMS", "EA8BE085-D420-4D1B-A538-2C0D4D116E0A" );

            // Add/Update Obsidian Block Type
            //   Name:Block Type List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.BlockTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Block Type List", "Displays a list of block types.", "Rock.Blocks.Cms.BlockTypeList", "CMS", "1C3D7F3D-E8C7-4F27-871C-7EC20483B416" );

            // Add/Update Obsidian Block Type
            //   Name:Content Channel Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Channel Detail", "Displays the details for a content channel.", "Rock.Blocks.Cms.ContentChannelDetail", "CMS", "2BAD2AB9-86AD-480E-BF38-C54F2C5C03A8" );

            // Add/Update Obsidian Block Type
            //   Name:Content Item List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelItemList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Item List", "Displays a list of content channel items.", "Rock.Blocks.Cms.ContentChannelItemList", "CMS", "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1" );

            // Add/Update Obsidian Block Type
            //   Name:Content Channel List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Channel List", "Displays a list of content channels.", "Rock.Blocks.Cms.ContentChannelList", "CMS", "F381936B-0D8C-43F0-8DA5-401383E40883" );

            // Add/Update Obsidian Block Type
            //   Name:Content Channel Type List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Channel Type List", "Displays a list of content channel types.", "Rock.Blocks.Cms.ContentChannelTypeList", "CMS", "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B" );

            // Add/Update Obsidian Block Type
            //   Name:File Asset Manager
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.FileAssetManager
            RockMigrationHelper.AddOrUpdateEntityBlockType( "File Asset Manager", "Browse and manage files on the web server or stored on a remote server or 3rd party cloud storage", "Rock.Blocks.Cms.FileAssetManager", "CMS", "535500A7-967F-4DA3-8FCA-CB844203CB3D" );

            // Add/Update Obsidian Block Type
            //   Name:Log Settings
            //   Category:Administration
            //   EntityType:Rock.Blocks.Cms.LogSettings
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Log Settings", "Block to edit rock log settings.", "Rock.Blocks.Cms.LogSettings", "Administration", "FA01630C-18FB-472F-8BF1-013AF257DE3F" );

            // Add/Update Obsidian Block Type
            //   Name:Media Account List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.MediaAccountList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Media Account List", "Displays a list of media accounts.", "Rock.Blocks.Cms.MediaAccountList", "CMS", "BAF39B55-C4E5-4EB4-A834-B4F820DD2F42" );

            // Add/Update Obsidian Block Type
            //   Name:Media Folder List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.MediaFolderList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Media Folder List", "Displays a list of media folders.", "Rock.Blocks.Cms.MediaFolderList", "CMS", "75133C37-547F-47FA-991C-6D957B2EA92D" );

            // Add/Update Obsidian Block Type
            //   Name:Page List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PageList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page List", "Displays a list of pages.", "Rock.Blocks.Cms.PageList", "CMS", "39B02B93-B1AF-4F9B-A535-33F470D91106" );

            // Add/Update Obsidian Block Type
            //   Name:Page Short Link Click List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PageShortLinkClickList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link Click List", "Lists clicks for a particular short link.", "Rock.Blocks.Cms.PageShortLinkClickList", "CMS", "E44CAC85-346F-41A4-884B-A6FB5FC64DE1" );

            // Add/Update Obsidian Block Type
            //   Name:Page Short Link List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PageShortLinkList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link List", "Displays a list of page short links.", "Rock.Blocks.Cms.PageShortLinkList", "CMS", "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Entry
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationEntry
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Entry", "Used for creating and sending a new communications such as email, SMS, etc. to recipients.", "Rock.Blocks.Communication.CommunicationEntry", "Communication", "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" );

            // Add/Update Obsidian Block Type
            //   Name:Communication List
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication List", "Lists the status of all previously created communications.", "Rock.Blocks.Communication.CommunicationList", "Communication", "C3544F53-8E2D-43D6-B165-8FEFC541A4EB" );

            // Add/Update Obsidian Block Type
            //   Name:Sms Pipeline List
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SmsPipelineList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Sms Pipeline List", "Lists the SMS Pipelines currently in the system.", "Rock.Blocks.Communication.SmsPipelineList", "Communication", "DA937CFD-F20E-4619-8CB8-D1A2738D2FF2" );

            // Add/Update Obsidian Block Type
            //   Name:Asset Storage Provider List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AssetStorageProviderList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Asset Storage Provider List", "Displays a list of asset storage providers.", "Rock.Blocks.Core.AssetStorageProviderList", "Core", "2663E57E-ED73-49FE-BA16-69B4B829C488" );

            // Add/Update Obsidian Block Type
            //   Name:Attribute Matrix Template List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attribute Matrix Template List", "Shows a list of all attribute matrix templates.", "Rock.Blocks.Core.AttributeMatrixTemplateList", "Core", "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD" );

            // Add/Update Obsidian Block Type
            //   Name:Binary File List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.BinaryFileList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Binary File List", "Displays a list of all binary files.", "Rock.Blocks.Core.BinaryFileList", "Core", "69A45481-467B-47EF-9838-4462E5615216" );

            // Add/Update Obsidian Block Type
            //   Name:Binary File Type Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Binary File Type Detail", "Displays all details of a binary file type.", "Rock.Blocks.Core.BinaryFileTypeDetail", "Core", "DABF690B-BE17-4821-A13E-44C7C8D587CD" );

            // Add/Update Obsidian Block Type
            //   Name:Binary File Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Binary File Type List", "Displays a list of binary file types.", "Rock.Blocks.Core.BinaryFileTypeList", "Core", "000CA534-6164-485E-B405-BA0FA6AE92F9" );

            // Add/Update Obsidian Block Type
            //   Name:Defined Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DefinedTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Defined Type List", "Displays a list of defined types.", "Rock.Blocks.Core.DefinedTypeList", "Core", "7FAF32D3-C577-462A-BC0B-D34DE3316A5B" );

            // Add/Update Obsidian Block Type
            //   Name:Device List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DeviceList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Device List", "Displays a list of devices.", "Rock.Blocks.Core.DeviceList", "Core", "7686A42F-A2C4-4C15-9331-8B364F24BD0F" );

            // Add/Update Obsidian Block Type
            //   Name:Person Following List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.PersonFollowingList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Following List", "Block for displaying people that current person follows.", "Rock.Blocks.Core.PersonFollowingList", "Follow", "18FA879F-1466-413B-8623-834D728F677B" );

            // Add/Update Obsidian Block Type
            //   Name:Person Suggestion List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.PersonSuggestionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Suggestion List", "Block for displaying people that have been suggested to current person to follow.", "Rock.Blocks.Core.PersonSuggestionList", "Follow", "D29619D6-2FFE-4EF7-ADAF-14DB588944EA" );

            // Add/Update Obsidian Block Type
            //   Name:Scheduled Job History
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduledJobHistoryList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Job History", "Lists all scheduled job's History.", "Rock.Blocks.Core.ScheduledJobHistoryList", "Core", "2306068D-3551-4C10-8DB8-133C030FA4FA" );

            // Add/Update Obsidian Block Type
            //   Name:Signal Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignalTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signal Type List", "Displays a list of signal types.", "Rock.Blocks.Core.SignalTypeList", "Core", "770D3039-3F07-4D6F-A64E-C164ACCE93E1" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document Template List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document Template List", "Lists all the signature document templates and allows for managing them.", "Rock.Blocks.Core.SignatureDocumentTemplateList", "Core", "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7" );

            // Add/Update Obsidian Block Type
            //   Name:Tag List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.TagList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Tag List", "Block for viewing a list of tags.", "Rock.Blocks.Core.TagList", "Core", "0ACF764F-5F60-4985-9D10-029CB042DA0D" );

            // Add/Update Obsidian Block Type
            //   Name:Assessment Type List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Assessment Type List", "Displays a list of assessment types.", "Rock.Blocks.Crm.AssessmentTypeList", "CRM", "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4" );

            // Add/Update Obsidian Block Type
            //   Name:Badge List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.BadgeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Badge List", "Displays a list of badges.", "Rock.Blocks.Crm.BadgeList", "CRM", "559978D5-A392-4BD1-8E04-055C2833F347" );

            // Add/Update Obsidian Block Type
            //   Name:Disc
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.Disc
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Disc", "Allows you to take a DISC test and saves your DISC score.", "Rock.Blocks.Crm.Disc", "CRM", "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E" );

            // Add/Update Obsidian Block Type
            //   Name:Nameless Person List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.NamelessPersonList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Nameless Person List", "List unmatched phone numbers with an option to link to a person that has the same phone number.", "Rock.Blocks.Crm.NamelessPersonList", "CRM", "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Attempt List
            //   Category:Achievements
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Attempt List", "Lists all the people that have made an attempt at earning an achievement.", "Rock.Blocks.Engagement.AchievementAttemptList", "Achievements", "B294C1B9-8368-422C-8054-9672C7F41477" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Type Detail
            //   Category:Achievements
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Type Detail", "Displays the details of the given Achievement Type for editing.", "Rock.Blocks.Engagement.AchievementTypeDetail", "Achievements", "EDDFCAFF-70AA-4791-B051-6567B37518C4" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Opportunity List
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.ConnectionOpportunityList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Opportunity List", "Displays a list of connection opportunities.", "Rock.Blocks.Engagement.ConnectionOpportunityList", "Engagement", "8EB82E1E-C0BD-4591-9D7A-F120A871FEC3" );

            // Add/Update Obsidian Block Type
            //   Name:Step Participant List
            //   Category:Steps
            //   EntityType:Rock.Blocks.Engagement.StepParticipantList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Step Participant List", "Lists all the participants in a Step.", "Rock.Blocks.Engagement.StepParticipantList", "Steps", "272B2236-FCCC-49B4-B914-20893F5E746D" );

            // Add/Update Obsidian Block Type
            //   Name:Step Program List
            //   Category:Steps
            //   EntityType:Rock.Blocks.Engagement.StepProgramList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Step Program List", "Displays a list of step programs.", "Rock.Blocks.Engagement.StepProgramList", "Steps", "5284B259-A9EC-431C-B949-661780BFCD68" );

            // Add/Update Obsidian Block Type
            //   Name:Step Type Detail
            //   Category:Steps
            //   EntityType:Rock.Blocks.Engagement.StepTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Step Type Detail", "Displays the details of the given Step Type for editing.", "Rock.Blocks.Engagement.StepTypeDetail", "Steps", "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1" );

            // Add/Update Obsidian Block Type
            //   Name:Step Type List
            //   Category:Steps
            //   EntityType:Rock.Blocks.Engagement.StepTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Step Type List", "Shows a list of all step types for a program.", "Rock.Blocks.Engagement.StepTypeList", "Steps", "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F" );

            // Add/Update Obsidian Block Type
            //   Name:Streak Type List
            //   Category:Streaks
            //   EntityType:Rock.Blocks.Engagement.StreakTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak Type List", "Shows a list of all streak types.", "Rock.Blocks.Engagement.StreakTypeList", "Streaks", "6F0F3AD2-4989-4F50-B394-0DE3C7AF35AD" );

            // Add/Update Obsidian Block Type
            //   Name:Calendar Event Item List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.EventCalendarItemList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Calendar Event Item List", "Lists all the event items in the given calendar.", "Rock.Blocks.Event.EventCalendarItemList", "Event", "20C68613-F253-4D2F-A465-62AFBB01DCD6" );

            // Add/Update Obsidian Block Type
            //   Name:Registration Instance Active List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceActiveList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Registration Instance Active List", "Displays a list of registration instances.", "Rock.Blocks.Event.RegistrationInstanceActiveList", "Event", "5E899CCB-3C24-4F7D-9843-2F1CB00AED8F" );

            // Add/Update Obsidian Block Type
            //   Name:Business List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BusinessList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Business List", "Displays a list of businesses.", "Rock.Blocks.Finance.BusinessList", "Finance", "1E60C390-98C4-404D-AEE8-F9E3E9C69705" );

            // Add/Update Obsidian Block Type
            //   Name:Gateway List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Gateway List", "Block for viewing list of financial gateways.", "Rock.Blocks.Finance.FinancialGatewayList", "Finance", "0F99866A-7FAB-462D-96EB-9F9534322C57" );

            // Add/Update Obsidian Block Type
            //   Name:Bank Account List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialPersonBankAccountList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Bank Account List", "Lists bank accounts for a person.", "Rock.Blocks.Finance.FinancialPersonBankAccountList", "Finance", "E1DCE349-2F5B-46ED-9F3D-8812AF857F69" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Scheduled Transaction List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialScheduledTransactionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Scheduled Transaction List", "Displays a list of financial scheduled transactions.", "Rock.Blocks.Finance.FinancialScheduledTransactionList", "Finance", "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Statement Template List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Statement Template List", "Displays a list of financial statement templates.", "Rock.Blocks.Finance.FinancialStatementTemplateList", "Finance", "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F" );

            // Add/Update Obsidian Block Type
            //   Name:Group Member Schedule Template Detail
            //   Category:Group Scheduling
            //   EntityType:Rock.Blocks.Group.GroupMemberScheduleTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Member Schedule Template Detail", "Displays the details of a group member schedule template.", "Rock.Blocks.Group.GroupMemberScheduleTemplateDetail", "Group Scheduling", "07BCB48D-746E-4364-80F3-C5BEB9075FC6" );

            // Add/Update Obsidian Block Type
            //   Name:Group Type List
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Type List", "Displays a list of group types.", "Rock.Blocks.Group.GroupTypeList", "Group", "8885F47D-9262-48B0-B969-9BEE003370EB" );

            // Add/Update Obsidian Block Type
            //   Name:Group Member Schedule Template List
            //   Category:Group Scheduling
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupMemberScheduleTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Member Schedule Template List", "Lists group member schedule templates.", "Rock.Blocks.Group.Scheduling.GroupMemberScheduleTemplateList", "Group Scheduling", "2B8A5A3D-BF9D-4319-B7E5-06757FA44759" );

            // Add/Update Obsidian Block Type
            //   Name:Interaction Component Detail
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.InteractionComponentDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Interaction Component Detail", "Presents the details of a interaction channel using Lava", "Rock.Blocks.Reporting.InteractionComponentDetail", "Reporting", "BC2034D1-416B-4FB4-9FFF-E202FA666203" );

            // Add/Update Obsidian Block Type
            //   Name:Metric Value Detail
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.MetricValueDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Metric Value Detail", "Displays the details of a particular metric value.", "Rock.Blocks.Reporting.MetricValueDetail", "Reporting", "B52E7CAE-C5CC-41CB-A5EC-1CF027074A2C" );

            // Add/Update Obsidian Block Type
            //   Name:Persisted Data View List
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.PersistedDataViewList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Persisted Data View List", "Shows a list of Data Views that have persistence enabled.", "Rock.Blocks.Reporting.PersistedDataViewList", "Reporting", "1A46CC61-6110-4022-8ACE-EFE188A6AB5A" );

            // Add/Update Obsidian Block Type
            //   Name:Apple TV Page List
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.AppleTvPageList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Page List", "Lists pages for TV apps (Apple or other).", "Rock.Blocks.Tv.AppleTvPageList", "TV > TV Apps", "A759218B-1C72-446C-8994-8559BA72941E" );

            // Add Block 
            //  Block Name: Add Prayer Request
            //  Page Name: Prayer
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "7019736A-8F30-4402-8A48-CE5308218618".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "5AA30F53-1B7D-4CA9-89B6-C10592968870".AsGuid(), "Add Prayer Request", "Main", @"", @"", 1, "E554B5F3-442E-4E55-8CD2-49D33DD8DC24" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Prayer,  Zone: Main,  Block: Add Prayer Request
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'E554B5F3-442E-4E55-8CD2-49D33DD8DC24'" );

            // Update Order for Page: Prayer,  Zone: Main,  Block: Content
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '186D89D6-8366-401D-82CD-4367355AA2D6'" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Enable AI Disclaimer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable AI Disclaimer", "EnableAIDisclaimer", "Enable AI Disclaimer", @"If enabled and the PrayerRequest Text was sent to an AI automation the configured AI Disclaimer will be shown.", 7, @"True", "67F4CDB7-AF0D-45B2-B51C-AD5F8EBCD8EB" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: AI Disclaimer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "AI Disclaimer", "AIDisclaimer", "AI Disclaimer", @"The message to display indicating the Prayer Request text may have been modified by an AI automation.", 8, @"This request may have been modified by an AI for formatting and privacy. Please be aware that errors may be present.", "FAF647E9-283F-4DF6-AEF4-1925B18EB060" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Duplicate Prevention Option", "ShowDuplicatePreventionOption", "Show Duplicate Prevention Option", @"Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.", 15, @"False", "1003FD5F-E678-49AE-A7EA-76B5F01A1283" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Prayer
            //   Attribute: Enable AI Disclaimer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable AI Disclaimer", "EnableAIDisclaimer", "Enable AI Disclaimer", @"If enabled and the PrayerRequest Text was sent to an AI automation the configured AI Disclaimer will be shown.", 9, @"False", "A6B4F3B1-BC23-4C00-A33D-7D894E5B5F38" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Prayer
            //   Attribute: AI Disclaimer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "9C204CD0-1233-41C5-818A-C5DA439445AA", "AI Disclaimer", "AIDisclaimer", "AI Disclaimer", @"The message to display indicating the Prayer Request text may have been modified by an AI automation.", 10, @"This request may have been modified by an AI for formatting and privacy. Please be aware that errors may be present.", "A1703115-B2E2-4874-B95B-CFF0313799CD" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable End Date", "EnableEndDate", "Enable End Date", @"When enabled, this setting allows an individual to specify an optional end date for their recurring scheduled gifts.", 14, @"False", "4E465E0E-84A5-407B-903A-5DE247A0F7CA" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable End Date", "EnableEndDate", "Enable End Date", @"When enabled, this setting allows an individual to specify an optional end date for their recurring scheduled gifts.", 8, @"False", "F46250E9-C7C0-4908-A8DF-914DEB5EEFC4" );

            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable End Date", "EnableEndDate", "Enable End Date", @"When enabled, this setting allows an individual to specify an optional end date for their recurring scheduled gifts.", 30, @"False", "16F2D574-8075-483F-8107-E66BA9377390" );

            // Attribute for BlockType
            //   BlockType: Profile Details
            //   Category: Mobile > Cms
            //   Attribute: Display Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Types", "DisplayCampusTypes", "Display Campus Types", @"The campus types that will be included in the list of campuses for the user to choose from. If the user's assigned campus is not part of the filtered list, it will be automatically added.", 13, @"5A61507B-79CB-4DA2-AF43-6F82260203B3", "9A495075-BCF8-42BC-A89B-D85777D1E5EB" );

            // Attribute for BlockType
            //   BlockType: Profile Details
            //   Category: Mobile > Cms
            //   Attribute: Display Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Statuses", "DisplayCampusStatuses", "Display Campus Statuses", @"The campus types that will be included in the list of campuses for the user to choose from.", 14, @"10696FD8-D0C7-486F-B736-5FB3F5D69F1A", "B6F2794B-A305-49B4-9808-22D01ACBA421" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 17, @"False", "9149656D-ADAA-4B88-BD4D-2275BAE444E1" );

            // Attribute for BlockType
            //   BlockType: Control Gallery
            //   Category: Obsidian > Example
            //   Attribute: Show Reflection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reflection", "ShowReflection", "Show Reflection", @"When enabled, a Show Reflection option will be enabled that will add a second control to demonstrate two-way databinding.  This is typically only useful to developers when they are developing a new control.", 0, @"false", "CF0FD9E2-FCC1-40D6-97DC-2520A6B88504" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable End Date", "EnableEndDate", "Enable End Date", @"When enabled, this setting allows an individual to specify an optional end date for their recurring scheduled gifts.", 29, @"False", "B1858793-2A64-4B6E-B976-DDC7A96F5E47" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Address", "Address", "Address", @"How should Address be displayed.", 15, @"Optional", "B28BB6D7-B297-49A1-A3BC-3530892CFD84" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "Gender", "Gender", @"How should Gender be displayed.", 28, @"Optional", "443179DA-CD2D-4119-BAD0-5A2E79C37BCA" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show Additional Email Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Additional Email Recipients", "ShowAdditionalEmailRecipients", "Show Additional Email Recipients", @"Allow additional email recipients to be entered for email communications?", 11, @"False", "049B5763-868C-4F5B-8CBE-CF14100D6C0B" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 19, @"False", "D603C3C6-508A-47EC-913F-9CD5DFF4BACB" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Adult Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Adult Label", "AdultLabel", "Adult Label", @"The label that should be used when referring to adults on the form. Please provide this in the singular form.", 15, @"Adult", "E54979CE-6004-434A-AFBF-FAC47C6E2CAE" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Child Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Child Label", "ChildLabel", "Child Label", @"The label that should be used when referring to children on the form. Please provide this in the singular form.", 11, @"Child", "51E3BB2D-B5D8-4821-AD7A-407380492E31" );

            // Attribute for BlockType
            //   BlockType: Site Detail
            //   Category: CMS
            //   Attribute: Default File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3E935E45-4796-4389-AB1C-98D2403FAEDF", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Default File Type", "DefaultFileType", "Default File Type", @"The default file type to use while uploading Favicon", 0, @"C1142570-8CD6-4A20-83B1-ACB47C1CD377", "79A18B03-89A9-4889-BBDB-000D452E4084" );

            // Attribute for BlockType
            //   BlockType: Site List
            //   Category: CMS
            //   Attribute: Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title", "BlockTitle", "Block Title", @"The title to display for this block.", 2, @"", "584E49DA-D61F-4BDC-A79C-0A11A5B45EAD" );

            // Attribute for BlockType
            //   BlockType: Site List
            //   Category: CMS
            //   Attribute: Show Site Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Site Icon", "ShowSiteIcon", "Show Site Icon", @"Determines if the site icon should be shown.", 4, @"True", "BBC6692A-4439-436E-ADE9-AC48DF4A6A93" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Finance
            //   Attribute: Hide Account Totals Section
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BE58680-8795-46A0-8BFA-434A01FEB4C8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Account Totals Section", "IsAccountTotalsHidden", "Hide Account Totals Section", @"When enabled the Account Totals section of the Financial Batch Detail block will be hidden.", 4, @"False", "8C44DD40-29BE-457E-9DEF-77BF849B266D" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB756565-8A35-42E2-BC79-8D11F57E4004", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the chart.", 0, @"", "7C1A7B8F-D3C0-4686-96E1-DD26C0C03A58" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB756565-8A35-42E2-BC79-8D11F57E4004", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the chart.", 1, @"", "93B67CB7-9967-43EC-BA29-40706A157FEA" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "CourseDetailTemplate", "Lava Template", @"The Lava template to use to render the page. Merge fields include: Course, Program, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variable Assignments
{% assign requirementTypes = Course.CourseRequirements | Distinct:'RequirementType' %}
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' |
Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}


//- Styles

<style>

    @media (max-width: 991px) {
        .course-side-panel {
            padding-left: 0;
        }
        .card {
            margin-bottom: 24px;
        }
        
    }
    
    @media (max-width: 767px) {
        h1 {
            font-size: 28px;
        }
        .card {
            margin-bottom: 24px;
        }
    }

</style>


<div class=""d-flex flex-column gap-4"">
    
    <div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ Course.Entity.ImageBinaryFile.Guid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ Course.Entity.PublicName }} </h1>
            <p class=""hero-section-description""> {{ Course.Entity.Summary }} </p>
        </div>
    </div>

    <div>

        <div class=""row"">

            <div class=""col-xs-12 col-sm-12 col-md-8""> //- LEFT CONTAINER

                <div class=""card rounded-lg""> //-COURSE DESCRIPTION

                    <div class=""card-body"">
                        <div class=""card-title"">
                            <h4 class=""m-0"">Course Description</h4>
                        </div>
                        <div class=""card-text"">
                            {% if Course.Entity.CourseCode != empty %}
                            <div class=""text-gray-600"">
                                <span class=""text-bold"">Course Code: </span>
                                <span>{{Course.Entity.CourseCode}}</span>
                            </div>
                            {% endif %}

                            <div class=""text-gray-600"">
                                <span class=""text-bold"">Credits: </span>
                                <span>{{Course.Entity.Credits}}</span>
                            </div>

                            <div class=""pb-3 text-gray-600"""">
                                <span class=""text-bold"">Prerequisites: </span>
                                <span>
                                    {{ prerequisitesText }}
                                </span>
                            </div>

                            <div class=""pt-3 border-top border-gray-200"">
                                <span>{{Course.DescriptionAsHtml}}</span>
                            </div>
                        </div>
                    </div>

                </div>

            </div>


            <div class=""col-xs-12 col-sm-12 col-md-4""> //- RIGHT CONTAINER

                <div>

                    {% case Course.LearningCompletionStatus %}

                        {% when 'Incomplete' %}
                        <div class=""card rounded-lg"">
                            <div class=""card-body"">
                                <div class=""card-title d-flex align-items-center"">
                                    <i class=""fa fa-user-check mr-2""></i>
                                    <h4 class=""m-0"">Currently Enrolled</h4>
                                </div>
                                <div class=""card-text text-muted"">
                                    <p>You are currently enrolled in this course.</p>
                                </div>
                                <div class=""mt-3"">
                                    <a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course Workspace</a>
                                </div>
                            </div>
                            
                        </div>


                        {% when 'Pass' or 'Fail' %}
                            {% if CanShowHistoricalAccess == true %}

                            <div class=""card rounded-lg"">
                                
                                <div class=""card-body"">
                                    <div class=""card-title d-flex align-items-center"">
                                        <i class=""fa fa-rotate-left mr-2""></i>
                                        <h4 class=""m-0"">History</h4>
                                    </div>
                                    <div class=""text-muted"">You completed this class on {{
                                        Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
            
                                    <div class=""mt-3"">
                                        <a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
                                    </div>
                                </div>
                                
                            </div>
                            
                            {% else %}
                            
                            <div class=""card rounded-lg"">
                                
                                <div class=""card-body"">
                                    <div class=""card-title d-flex align-items-center"">
                                        <i class=""fa fa-rotate-left mr-2""></i>
                                        <h4 class=""m-0"">History</h4>
                                    </div>
                                    <div class=""text-muted"">You completed this class on {{
                                        Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
            
                                    <div class=""mt-3"">
                                        <div class=""text-muted"">
                                            <p class=""text-bold mb-0"">Grade</p>
                                            <p class=""mb-0"">{{ Course.MostRecentParticipation.LearningGradingSystemScale.Name }}</p>
                                        </div>
                                    </div>
                                </div>
                                
                            </div>
                            
                            
                            {% endif %}

                        {% else %}

                        <div class=""card rounded-lg"">

                            <div class=""card-body"">
                                <div class=""card-title d-flex align-items-center"">
                                    <i class=""fa fa-calendar-alt mr-2""></i>
                                    <h4 class=""m-0"">Upcoming Schedule</h4>
                                </div>

                                <div class=""card-text d-flex flex-column gap-1"">
                                    {% if Course.Program.ConfigurationMode == ""AcademicCalendar"" %}
                                    <div class=""text-muted"">
                                        <p class=""text-bold mb-0"">Next Session Semester: </p>
                                        {% if Course.NextSemester.Name %}
                                        <p>{{ Course.NextSemester.Name }}</p>
                                        {% else %}
                                        <p>TBD</p>
                                        {% endif %}
                                    </div>
                                    {% endif %}

                                    <div class=""text-muted"">
                                        <p class=""text-bold mb-0"">{{ 'Facilitator' | PluralizeForQuantity:facilitatorCount
                                            }}:</p>
                                        <p>{{ facilitators }}</p>
                                    </div>

                                    {% for requirementType in requirementTypes %}

                                    {% assign requirementsText = Course.CourseRequirements |
                                    Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' |
                                    Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                                    <div class=""text-muted"">
                                        <div class=""d-flex align-items-center"">
                                            <p class=""text-bold mb-0"">{{ requirementType | Pluralize }}</p>
                                            {% if Course.UnmetPrerequisites %}
                                            <i class=""fa fa-check-circle text-success ml-2""></i>
                                            {% else %}
                                            <i class=""fa fa-exclamation-circle text-danger ml-2""></i>
                                            {% endif %}
                                        </div>
                                        <p>{{ requirementsText }}</p>
                                    </div>

                                    {% endfor %}

                                    <div class=""mt-3"">

                                        {% if ErrorKey == 'enrollment_closed' or ErrorKey == 'class_full' %}
                                            <p class=""text-danger"">Enrollment is closed for this class.</p>
                                            {% else %}
                                                <a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
                                        {% endif %}

                                    </div>

                                </div>
                            </div>
                        </div>

                    {% endcase %}

                </div>
            </div>
        </div>
    </div>
</div>
", "DA6C3170-5264-427D-AC22-8D50D2F6D2F6" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public classes will be excluded.", 5, @"True", "774CABEA-2BB5-4799-80ED-42D7123B06B7" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public courses will be excluded.", 4, @"True", "20CDB819-CBBF-44AC-9068-D89024FF11D1" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2FC656DA-7F5D-41B3-AD18-BFE692CFCA57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Title", "PageTitle", "Page Title", @"Provide a clear, welcoming title for the Learning Hub homepage. Example: 'Grow Together in Faith.'", 1, @"Learning Hub", "248E1752-E6FB-4C8A-B90D-D7E6EF5267D8" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2FC656DA-7F5D-41B3-AD18-BFE692CFCA57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Description", "PageDescription", "Page Description", @"Enter a brief description for the homepage to introduce users to the LMS. Example: 'Explore resources to deepen your faith and connect with our community.'", 2, @"Explore courses and trainings designed to deepen your faith, help you grow in spiritual knowledge, and prepare you for serving and volunteering.", "E19A078B-939E-4F38-AA75-C89B0E823BD9" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Banner Image
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2FC656DA-7F5D-41B3-AD18-BFE692CFCA57", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Banner Image", "BannerImage", "Banner Image", @"Add a welcoming banner image to visually enhance the homepage. Ideal size: 1200x400 pixels; use high-quality images.", 0, @"605FD4B7-2DCA-4782-8826-95AAC6C6BAB6", "B6260CB0-9B7F-4D6A-ACED-D52B22FCA5EE" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2FC656DA-7F5D-41B3-AD18-BFE692CFCA57", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public programs will be excluded.", 5, @"True", "4A34980C-32D4-4E1E-882B-2EF36D532DD4" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "87FFA598-39F1-4413-ABD3-44670769EF7E" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "671F05CC-571C-45AF-9C2F-2463035EAA6D" );

            // Attribute for BlockType
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Attribute: Watched Note Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B1F65833-CECA-4054-BCC3-2DE5692741ED", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Watched Note Lava Template", "WatchedNoteLavaTemplate", "Watched Note Lava Template", @"The Lava template to use to show the watched note type. <span class='tip tip-lava'></span>", 0, @"", "0E6D23E7-D4E8-43D1-962B-4FB28A46E133" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "HeaderLavaTemplate", "Header Lava Template", @"The Lava template to use to show a header above the various state templates. Merge fields include: LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
<div class=""hero-section"">
    <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ LearningClass.LearningCourse.ImageBinaryFile.Guid }}')""></div>
    <div class=""hero-section-content"">
        <h1 class=""hero-section-title""> {{ LearningClass.LearningCourse.PublicName }} </h1>
        <p class=""hero-section-description""> {{ LearningClass.LearningCourse.Summary }} </p>
    </div>
</div>
", "09A07907-D776-4F37-805E-C1A81A1CBA1A" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Confirmation Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Lava Template", "ConfirmationLavaTemplate", "Confirmation Lava Template", @"The Lava template to use when displaying the confirmation messaging to the individual. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled'), UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 2, @"
//-Variable Assignments
{% assign facilitatorCount = Facilitators | Size %}
{% assign facilitatorsText = '' %}

{% for f in Facilitators %}
    {%- capture name -%}
        {{f | Property:'Name'}}{%- unless forloop.last -%}, {% endunless %}
    {%- endcapture -%}
    {% assign facilitatorsText = facilitatorsText | Append:name %}
{% endfor %}

{% if facilitatorsText == empty %}
    {% assign facilitatorsText = 'TBD' %}
{% endif %}
{% assign credits = LearningClass.LearningCourse.Credits | AsInteger %}
{% assign location = LearningClass.GroupLocations | First %}
{% assign locationNameLength = location.Name | Size %}
{% assign schedule = LearningClass.Schedule %}
{% assign scheduleNameLength = schedule.Name | Size %}
{% assign hasLocation = locationNameLength > 0 %}
{% assign hasSchedule = scheduleNameLength > 0 %}


{% stylesheet %}
    .confirmation-details {
        width: 100%;
        max-width: 545px;
    }
    
    .detail-table {
        border: 1px solid var(--color-interface-softer);
        width: 100%;
    }
    
    .detail-row {
        border-bottom: 1px solid var(--color-interface-softer);
        padding: 8px;
        width: 100%;
    }
    
    .detail-row:last-child {
        border-bottom: none;
    }

    
{% endstylesheet %}



<div class=""d-flex flex-column w-100 justify-content-center gap-4 my-4"">
    
    //- 1 REVIEW HEADING
    <div>
        <h3 class=""text-center"">Enrollment Review</h3>
        <div class=""text-center"">Please review class details before confirming enrollment:</div>
    </div>
    
    //- 2 TABLE
    <div class=""d-flex flex-column mt-3 gap-3"">
        <div>
            <h5>Participant Details</h5>
            <div class=""detail-table"">
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Name
                    </div>
                    <div class=""field-value"">
                        {{Registrant.FullName}}
                    </div>
                </div>
                <div class=""detail-row participant-email d-flex justify-content-between"">
                    <div class=""field-title"">
                        Email
                    </div>
                    <div class=""field-value"">
                        {{Registrant.Email}}
                    </div>
                </div>
            </div>
        </div>
        
        <div>
            <h5>Class Details</h5>
            <div class=""detail-table"">
                <div class=""detail-row  d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Name
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningCourse.PublicName}}
                    </div>
                </div>
                {% if LearningClass.LearningCourse.CourseCode != empty %}
                <div class=""detail-row  d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Code
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningCourse.CourseCode}}
                    </div>
                </div>
                {% endif %}
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Configuration
                    </div>
                    <div class=""field-value "">
                        {% if LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 0 %}
                            Academic Calendar
                        {% elseif LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 1 %}
                            On-Demand
                        {% endif %}
                    </div>
                </div>
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        {{ 'Facilitator' | PluralizeForQuantity:facilitatorCount }}:
                    </div>
                    <div class=""field-value"">
                        {{facilitatorsText}}
                    </div>
                </div>
        
                {% if credits > 0 %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Credits
                        </div>
                        <div class=""field-value"">
                            {{LearningClass.LearningCourse.Credits}}
                        </div>
                    </div>
                {% endif %}
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Grading System
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningGradingSystem.Name}}
                    </div>
                </div>
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Semester
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningSemester.Name}}
                    </div>
                </div>
        
                {% if hasLocation %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Location
                        </div>
                        <div class=""field-value"">
                            {{location.Name}}
                        </div>
                    </div>
                {% endif %}
        
                {% if hasSchedule %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Schedule
                        </div>
                        <div class=""field-value"">
                            {{schedule.Name}}
                        </div>
                    </div>
                {% endif %}
        
                {% if LearningClass.LearningSemester.StartDate %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Starts
                        </div>
                        <div class=""field-value"">
                            {{LearningClass.LearningSemester.StartDate |  Date:'sd' }}
                        </div>
                    </div>
                {% endif %}
            </div>
        </div>
    </div>

</div>

", "09002AE5-51E4-483E-9EB5-5767C6180E55" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Completion Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Lava Template", "CompletionLavaTemplate", "Completion Lava Template", @"The Lava template to use to show the completed message. Merge fields include: UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 3, @"
<div class=""completion-container d-flex flex-column justify-content-center my-5"">
    <i class=""fa fa-check-circle fa-4x text-success text-center""></i>
    <h3 class=""completion-header text-center"">Successfully Enrolled!</h3>
    <div class=""completion-sub-header text-center"">
        You are now enrolled in this class.
        Check your email for a confirmation with your enrollment details.
        Click “Go to Class Workspace” to begin your learning experience.
    </div>
</div>
", "FEC8565A-7565-4310-8BB1-5CB5B42233B6" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Enrollment Error Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Enrollment Error Lava Template", "EnrollmentErrorLavaTemplate", "Enrollment Error Lava Template", @"The Lava template to use when the individual is not able to enroll. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled'), UnmetRequirements, Facilitators, LearningClass, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 4, @"
<div class=""error-container d-flex flex-column justify-content-center my-5"">
    <i class=""fa fa-exclamation-triangle fa-4x text-danger text-center""></i>
    <h3 class=""error-header text-center"">Cannot Enroll in Class</h3>
    <div class=""error-sub-header text-center"">
        {% case ErrorKey %}
        {% when 'unmet_course_requirements' %}
            You have not completed the following 
            {{ 'prerequisite' | PluralizeForQuantity:UnmetRequirements }} for this course:
            <ul class=""d-inline-block"">
                {% for requirement in UnmetRequirements %}
                <li>{{requirement.RequiredLearningCourse.Name}}</li>
                {% endfor %}
            </ul>
        {% when 'class_full' %}
            This class has reached it's capacity. Please go back to the Course Detail and try again.
        {% when 'enrollment_closed' %}
            Enrollment is closed for this class.
        {% when 'already_enrolled' %}
            You're already enrolled in this class.
        {% else %}
            Something went wrong with your enrollment. Please contact the facilitator for further support.
        {% endcase %}
    </div>
</div>
", "72C1EC84-8BC9-434D-BDC2-DEDF1869CAF3" );

            // Attribute for BlockType
            //   BlockType: Block List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA8BE085-D420-4D1B-A538-2C0D4D116E0A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6042A95E-DC10-4231-AECC-ABD5B1549767" );

            // Attribute for BlockType
            //   BlockType: Block List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA8BE085-D420-4D1B-A538-2C0D4D116E0A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D64A6E5E-A48F-445D-A48F-5EC092689B33" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the block type details.", 0, @"", "1E67AA59-6189-425F-8C07-D930CB5A479B" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "38E12A82-1D83-4777-A80D-3E6E3B778973" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6987D90D-CB37-4873-8BBC-5C1186B53353" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "EA661287-2A10-40F6-B652-DF42FF5CBE6C" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "F579201B-BCB5-4358-AD3D-AEDCBCC91E36" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Filter Items For Current User
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Items For Current User", "FilterItemsForCurrentUser", "Filter Items For Current User", @"Filters the items by those created by the current logged in user.", 1, @"False", "B915402B-EA86-4DEC-8BB8-906DCE0175E9" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filters", "ShowFilters", "Show Filters", @"Allows you to show/hide the grids filters.", 2, @"True", "B4D61CA8-8E77-4423-A523-C99F17D5CBEC" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Event Occurrences Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Event Occurrences Column", "ShowEventOccurrencesColumn", "Show Event Occurrences Column", @"Determines if the column that lists event occurrences should be shown if any of the items has an event occurrence.", 3, @"True", "CEA81A11-8C6B-4C15-B910-C61CD317EA66" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Priority Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Priority Column", "ShowPriorityColumn", "Show Priority Column", @"Determines if the column that displays priority should be shown for content channels that have Priority enabled.", 4, @"True", "474DA16D-0B92-4A8F-9394-F0E7F41E290F" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Security Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Column", "ShowSecurityColumn", "Show Security Column", @"Determines if the security column should be shown.", 5, @"True", "3E0D56FD-E7D2-4163-9C74-5F655E5310DF" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Expire Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Expire Column", "ShowExpireColumn", "Show Expire Column", @"Determines if the expire column should be shown.", 6, @"True", "51B3E0D6-3D38-40E5-8ABA-B713653A4284" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"If set the block will ignore content channel query parameters", 0, @"", "A0E5C403-EB0E-4BB5-B5FB-F8FF74BAE701" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "CED2BD43-E0BF-46EE-A438-A8B22CA05546" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DEB89A0B-A1B2-405D-9F75-FC36FCF60C2B" );

            // Attribute for BlockType
            //   BlockType: Content Channel List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F381936B-0D8C-43F0-8DA5-401383E40883", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the content channel details.", 0, @"", "9EF8744F-C102-4784-B5A5-A73FCEB987D4" );

            // Attribute for BlockType
            //   BlockType: Content Channel List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F381936B-0D8C-43F0-8DA5-401383E40883", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "47246985-31E2-467F-82F8-70A40ACFEBE5" );

            // Attribute for BlockType
            //   BlockType: Content Channel List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F381936B-0D8C-43F0-8DA5-401383E40883", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D4A0AD46-1ACD-42C5-88E2-2F30ED3668CC" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the content channel type details.", 0, @"", "AEA0D9E0-F558-42F8-A0E2-2AAE93B9F2FD" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "46F93E91-03AB-4C75-A8BF-420CC3952AC0" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B44FFDA2-22CD-4FE8-AE07-D2CFBF638794" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Asset Storage Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Asset Storage Providers", "EnableAssetProviders", "Enable Asset Storage Providers", @"Set this to true to enable showing folders and files from your configured asset storage providers.", 0, @"True", "7750F7BB-DC53-41C6-987B-5FD2B02674C2" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable File Manager
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable File Manager", "EnableFileManager", "Enable File Manager", @"Set this to true to enable showing folders and files your server's local file system.", 1, @"False", "FCBB90A6-965F-4237-9B0F-4384E3FFC991" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Use Static Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Static Height", "IsStaticHeight", "Use Static Height", @"Set this to true to be able to set a CSS height value dictating how tall the block will be. Otherwise, it will grow with the content.", 2, @"False", "5D1524EA-E1F0-472B-AE24-4D30DA6672F8" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Height", "Height", "Height", @"If you've selected Yes for ""Use Static Height"", this will be the CSS length value that dictates how tall the block will be.", 3, @"400px", "48C3DFAD-2168-4F6C-8DEC-167E49C379B7" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Root Folder", "RootFolder", "Root Folder", @"The root file manager folder to browse", 4, @"~/Content", "14684245-5768-442D-9BFB-C80E1383775A" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Browse Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Browse Mode", "BrowseMode", "Browse Mode", @"Select 'image' to show only image files. Select 'doc' to show all files.", 5, @"doc", "C872B6A7-36F6-4771-807A-7B4A7E8BAD2C" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: File Editor Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "File Editor Page", "FileEditorPage", "File Editor Page", @"Page used to edit the contents of a file.", 6, @"", "80D4544B-563A-4109-9F61-F4E019580B3A" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Zip Upload
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Zip Upload", "ZipUploaderEnabled", "Enable Zip Upload", @"Set this to true to enable the Zip File uploader.", 7, @"False", "BD031ADA-5D23-4237-A332-468FAC7282E9" );

            // Attribute for BlockType
            //   BlockType: Media Account List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAF39B55-C4E5-4EB4-A834-B4F820DD2F42", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the media account details.", 0, @"", "64E612A9-EBC5-4BD6-8A5F-9851D00248C3" );

            // Attribute for BlockType
            //   BlockType: Media Account List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAF39B55-C4E5-4EB4-A834-B4F820DD2F42", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0D6A12F5-80D4-4577-892F-AA8C03CEDDE8" );

            // Attribute for BlockType
            //   BlockType: Media Account List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAF39B55-C4E5-4EB4-A834-B4F820DD2F42", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1C7F5A05-88BB-4F34-8C8A-E9E8E906C2EB" );

            // Attribute for BlockType
            //   BlockType: Media Folder List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "75133C37-547F-47FA-991C-6D957B2EA92D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the media folder details.", 0, @"", "EBA09B93-0D35-425D-9DE0-1095B465A253" );

            // Attribute for BlockType
            //   BlockType: Media Folder List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "75133C37-547F-47FA-991C-6D957B2EA92D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "976C6535-929F-4BE8-9E4D-B0AF54628615" );

            // Attribute for BlockType
            //   BlockType: Media Folder List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "75133C37-547F-47FA-991C-6D957B2EA92D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "01005A24-256A-4556-B0D9-366CC4CFABB7" );

            // Attribute for BlockType
            //   BlockType: Page List
            //   Category: CMS
            //   Attribute: Show Page Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39B02B93-B1AF-4F9B-A535-33F470D91106", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Page Id", "ShowPageId", "Show Page Id", @"Enables the hiding of the page id column.", 0, @"True", "A441C9A7-781D-4E24-BD5D-E7A490804620" );

            // Attribute for BlockType
            //   BlockType: Page List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39B02B93-B1AF-4F9B-A535-33F470D91106", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6958A295-286E-461E-9F4D-2298C022328D" );

            // Attribute for BlockType
            //   BlockType: Page List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39B02B93-B1AF-4F9B-A535-33F470D91106", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F24590B5-965B-4868-A136-75C42630503F" );

            // Attribute for BlockType
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E44CAC85-346F-41A4-884B-A6FB5FC64DE1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "AB7E8098-ED68-4C0F-BC65-06B116F5A2D3" );

            // Attribute for BlockType
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E44CAC85-346F-41A4-884B-A6FB5FC64DE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6CB70101-5745-4DBB-9162-7D1C55E007CC" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the page short link details.", 0, @"", "04A2FCF6-DE8F-4973-A9F9-055A860E7AEB" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "54AB7012-8515-42EC-9440-854DAFF40E0C" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0F77028D-6D77-4E75-897E-642F34F342EE" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Lava", "EnableLava", "Enable Lava", @"When enabled, allows lava in the message. When disabled, lava is removed from the message without resolving it.", 0, @"False", "C1A95B9F-9003-4DD3-9BA6-AE51FC05BAF7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block if Enable Lava is checked.", 1, @"", "C19DF45C-152A-4FF3-B784-A610B7878398" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Person Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.", 2, @"False", "5C95D6FF-9736-4FBD-87BF-7CF739610C34" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Mediums
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "039E2E97-3682-4B29-8748-7132287A2059", "Mediums", "Mediums", "Mediums", @"The Mediums that should be available to user to send through (If none are selected, all active mediums will be available).", 3, @"", "11290B87-BFDF-4A1B-9976-E16DB1AA9132" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Default Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9", "Default Template", "DefaultTemplate", "Default Template", @"The default template to use for a new communication. (Note: This will only be used if the template is for the same medium as the communication.)", 4, @"", "8B9A5D60-A7DF-4E5F-813E-AF1DBD0A58C2" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Maximum Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "Maximum Recipients", @"The maximum number of recipients allowed before communication will need to be approved.", 5, @"0", "C369DFC4-CFFA-4183-B7F2-090D2C7E604A" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Send When Approved
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send When Approved", "SendWhenApproved", "Send When Approved", @"Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", 6, @"True", "57A5010D-69F3-47D7-9FD0-B6510F17958C" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "Mode", @"The mode to use ('Simple' mode will prevent users from searching/adding new people to communication).", 7, @"Full", "235CEBCB-8216-452C-A50C-C1EDA8F52C80" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Allow CC/Bcc
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow CC/Bcc", "AllowCcBcc", "Allow CC/Bcc", @"Allow CC and BCC addresses to be entered for email communications?", 8, @"False", "BA82B409-2F29-4447-9910-45B03BEF5FA5" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Attachment Uploader
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Attachment Uploader", "ShowAttachmentUploader", "Show Attachment Uploader", @"Should the attachment uploader be shown for email communications?", 9, @"True", "BCB16360-06CF-416E-A91C-E197FC4F4240" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "B8C35BA7-85E9-4512-B99C-12DE697DE14E", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included).", 10, @"", "FCF43ED8-A2B8-426F-A9CE-5F44A4F189E7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Simple Communications Are Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Simple Communications Are Bulk", "IsBulk", "Simple Communications Are Bulk", @"Should simple mode communications be sent as a bulk communication?", 11, @"True", "06B03251-CD7B-4E8E-8B72-B5D484DAE6E1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "Attachment Binary File Type", @"The FileType to use for files that are attached to an sms or email communication.", 12, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "99D2674D-803F-45FE-B082-F604104ABD8D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Default As Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default As Bulk", "DefaultAsBulk", "Default As Bulk", @"Should new entries be flagged as bulk communication by default?", 13, @"False", "3BBCB9A5-523D-4DB7-BACF-693A17EAEF94" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Email Metrics Reminder Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Metrics Reminder Options", "ShowEmailMetricsReminderOptions", "Show Email Metrics Reminder Options", @"Should the email metrics reminder options be shown after a communication is sent?", 14, @"False", "AF5AD05E-A7A9-4784-B88C-FA82A88E8A65" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Additional Email Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Additional Email Recipients", "ShowAdditionalEmailRecipients", "Show Additional Email Recipients", @"Allow additional email recipients to be entered for email communications?", 15, @"False", "71EF060D-E70B-4733-822C-89D3AE6FD069" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Duplicate Prevention Option", "ShowDuplicatePreventionOption", "Show Duplicate Prevention Option", @"Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.", 16, @"False", "75545130-5EAA-4619-9D2D-CBE33FC4EA9D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "Document Root Folder", @"The folder to use as the root when browsing or uploading documents.", 16, @"~/Content", "2796C859-AC47-40FA-B698-7DB0A8B25255" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "Image Root Folder", @"The folder to use as the root when browsing or uploading images.", 17, @"~/Content", "7FF5F073-1F7A-48A6-9511-75F7D8463028" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "User Specific Folders", @"Should the root folders be specific to current user?", 18, @"False", "CA6F5B1C-8385-44D4-BED4-0D3710D0FDF7" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the communication details.", 0, @"", "5209A318-9C53-43E4-9511-AAC595FC3684" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B3DD2485-725A-44B2-9B30-CA47F8ADDF03" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5192EB07-ED42-4D0E-8BA4-0081711227FC" );

            // Attribute for BlockType
            //   BlockType: Sms Pipeline List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA937CFD-F20E-4619-8CB8-D1A2738D2FF2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "SMSPipelineDetail", "Detail Page", @"The page that will show the sms pipeline details.", 0, @"", "0DD51B5F-E16D-40D8-B934-6598015227A1" );

            // Attribute for BlockType
            //   BlockType: Sms Pipeline List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA937CFD-F20E-4619-8CB8-D1A2738D2FF2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E4FDD860-4316-4589-A8A2-B2ABDE51F372" );

            // Attribute for BlockType
            //   BlockType: Sms Pipeline List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA937CFD-F20E-4619-8CB8-D1A2738D2FF2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8728839A-83C9-45E3-A776-3EC2B5D1E370" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2663E57E-ED73-49FE-BA16-69B4B829C488", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the asset storage provider details.", 0, @"", "10FA745A-A70D-45F4-AFFB-EF3B6FB52345" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2663E57E-ED73-49FE-BA16-69B4B829C488", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DFFA9B37-1332-4B51-A6A3-A5FED0BF8939" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2663E57E-ED73-49FE-BA16-69B4B829C488", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8E2B6DEF-D02A-41FF-BFC3-CEFF5F5BBE69" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the attribute matrix template details.", 0, @"", "6032EF50-8F86-4093-87EE-4F02AADAF2D0" );

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
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "69A45481-467B-47EF-9838-4462E5615216", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the binary file details.", 0, @"", "7CBF6711-D34C-4090-A06B-59C07DDF54CA" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "69A45481-467B-47EF-9838-4462E5615216", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Binary File Type", "BinaryFileType", "Binary File Type", @"", 0, @"", "E4B2DBD3-5812-4B32-B455-26B49B5BB995" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "69A45481-467B-47EF-9838-4462E5615216", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A0273D2A-082B-4806-97A6-1BE4E18D0615" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "69A45481-467B-47EF-9838-4462E5615216", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3714D9CD-58C5-4B7D-85D8-63A9C7D8FEBA" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "000CA534-6164-485E-B405-BA0FA6AE92F9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the binary file type details.", 0, @"", "657F5CDE-0727-4EE2-81A4-3052FF6E39AE" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "000CA534-6164-485E-B405-BA0FA6AE92F9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9788BE43-E6FD-4AC3-8E46-1B514871DD84" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "000CA534-6164-485E-B405-BA0FA6AE92F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "462EEE93-F09F-49C2-A6E9-82700D2DDC03" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7FAF32D3-C577-462A-BC0B-D34DE3316A5B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the defined type details.", 0, @"", "96E6B468-4D42-4D7F-9660-7154A44177DD" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7FAF32D3-C577-462A-BC0B-D34DE3316A5B", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Categories", "Categories", "Categories", @"If block should only display Defined Types from specific categories, select the categories here.", 1, @"", "FC4ABBCD-CE55-4E0C-82D7-15A3BC267AE9" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7FAF32D3-C577-462A-BC0B-D34DE3316A5B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "92DC50F2-3FCB-448B-9140-9ADDF6607134" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7FAF32D3-C577-462A-BC0B-D34DE3316A5B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4839CE2D-D104-4312-872A-B1BF206C57C1" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7686A42F-A2C4-4C15-9331-8B364F24BD0F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the device details.", 0, @"", "E58A1D17-6664-4F0C-A8CA-1727C52F1191" );

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
            //   BlockType: Person Following List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18FA879F-1466-413B-8623-834D728F677B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "60AAFCAC-553B-430D-9F33-1B5BBF214F3D" );

            // Attribute for BlockType
            //   BlockType: Person Following List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18FA879F-1466-413B-8623-834D728F677B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FF99A423-17FC-4430-A968-7FF8AE755D41" );

            // Attribute for BlockType
            //   BlockType: Person Suggestion List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D29619D6-2FFE-4EF7-ADAF-14DB588944EA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4721D4D4-5C67-4D48-8899-718F61030587" );

            // Attribute for BlockType
            //   BlockType: Person Suggestion List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D29619D6-2FFE-4EF7-ADAF-14DB588944EA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "9FC141F9-EEBE-4F10-AA01-E1C87D8D483E" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2306068D-3551-4C10-8DB8-133C030FA4FA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "21075366-2E56-44B5-A32B-024CFED9EC59" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2306068D-3551-4C10-8DB8-133C030FA4FA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "25C5C011-3910-4803-B23E-215664088D36" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "770D3039-3F07-4D6F-A64E-C164ACCE93E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the signal type details.", 0, @"", "28E98675-BA87-4541-96F8-D58D557809C7" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "770D3039-3F07-4D6F-A64E-C164ACCE93E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DD991CEC-C1C3-4FE8-86F2-E27296A76D80" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "770D3039-3F07-4D6F-A64E-C164ACCE93E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2A28086F-F2D2-49AB-93E6-0ED5CCF584AF" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the signature document template details.", 0, @"", "C5246895-FFEF-41FF-9E26-8F5AB5783C79" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7CD92079-5E56-4FB3-914F-7DF6247BD1A2" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "93F34441-1109-49F8-81E8-521656A00DD1" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: Show Qualifier Columns
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ACF764F-5F60-4985-9D10-029CB042DA0D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Qualifier Columns", "ShowQualifierColumns", "Show Qualifier Columns", @"Should the 'Qualifier Column' and 'Qualifier Value' fields be displayed in the grid?", 0, @"false", "55DA3A81-0A73-494B-A4FD-35C1CE5C9EB7" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ACF764F-5F60-4985-9D10-029CB042DA0D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the tag details.", 0, @"", "76783565-690B-41B7-9E50-20102A44549E" );

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
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the assessment type details.", 0, @"", "100D6999-8F0C-4EB7-92D1-537DBD385AEB" );

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
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "559978D5-A392-4BD1-8E04-055C2833F347", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the badge details.", 0, @"", "79D2AFB7-824F-479C-96F5-540A4255749E" );

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
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Instructions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "Instructions", @"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"
<h2>Welcome!</h2>
<p>
    {{ Person.NickName }}, our behaviors are influenced by our natural personality wiring. This assessment
    evaluates your essential approach to the world around you and how that drives your behavior.
</p>
<p>
    For best results with this assessment, picture a setting such as the workplace, at home or with friends,
    and keep that same setting in mind as you answer all the questions. Your responses may be different in
    different circumstances.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most natural.
    Since there are no right or wrong answers, just go with your instinct.
</p>", "F7052876-99FC-4A1B-8056-EBBE849FD1D4" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Set Page Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Title", "SetPageTitle", "Set Page Title", @"The text to display as the heading.", 1, @"DISC Assessment", "297DCD1D-943C-4463-ACC2-332F661A103E" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Set Page Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "Set Page Icon", @"The css class name to use for the heading icon.", 2, @"fa fa-chart-bar", "2CD8C9D5-4278-489B-9822-1E336313A779" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Number of Questions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "Number of Questions", @"The number of questions to show per page while taking the test", 3, @"5", "D25A1036-E619-417E-8AE6-36F128A0B1EA" );

            // Attribute for BlockType
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E9DDF9B9-839B-4F2D-AA5A-51F507AB0734" );

            // Attribute for BlockType
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0CC87BD8-FD68-4B86-B65B-77FC3F7105E7" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B294C1B9-8368-422C-8054-9672C7F41477", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the achievement attempt details.", 0, @"", "AC750FE6-33DF-4617-B9C1-38EEEEB781E2" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B294C1B9-8368-422C-8054-9672C7F41477", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5E15CC7B-190E-4547-A4EE-20BA78219B96" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B294C1B9-8368-422C-8054-9672C7F41477", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FEE6722A-32E5-43B5-BBED-49D6EE6366C4" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8EB82E1E-C0BD-4591-9D7A-F120A871FEC3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the connection opportunity details.", 0, @"", "87F2BA25-37AF-4A50-A1B1-61CE8905026C" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8EB82E1E-C0BD-4591-9D7A-F120A871FEC3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C1F8D81F-D59C-4DF0-AF19-D776898FA8C1" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8EB82E1E-C0BD-4591-9D7A-F120A871FEC3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F5211493-99C2-494C-84DD-D8DE602BC27E" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "272B2236-FCCC-49B4-B914-20893F5E746D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "E8F10140-A221-45AD-81EC-55D57C51D5B9" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "272B2236-FCCC-49B4-B914-20893F5E746D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set a view profile button will show for each participant.", 2, @"", "B034203A-F007-4F7B-8E0A-DAC09781D354" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: Show Note Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "272B2236-FCCC-49B4-B914-20893F5E746D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Note Column", "ShowNoteColumn", "Show Note Column", @"Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?", 3, @"False", "B9B99603-9FFD-4449-8845-9151C7638861" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "272B2236-FCCC-49B4-B914-20893F5E746D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5DF54F82-B920-4DC8-A98F-90EEEB489F25" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "272B2236-FCCC-49B4-B914-20893F5E746D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "18953D12-97A1-4C5B-9862-FBEEF66B8A21" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5284B259-A9EC-431C-B949-661780BFCD68", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Categories", "Categories", "Categories", @"If block should only display Step Programs from specific categories, select the categories here.", 1, @"", "0E83A4B2-A87F-46BC-AD0B-11DA7AB4AF2D" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5284B259-A9EC-431C-B949-661780BFCD68", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the step program details.", 0, @"", "92338117-0F02-42A1-84D3-A24B675D789D" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5284B259-A9EC-431C-B949-661780BFCD68", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5C018E03-A98A-43E6-9579-8B738B2D6681" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5284B259-A9EC-431C-B949-661780BFCD68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "9CD01960-832A-4332-AB57-0B532E555F28" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Show Chart
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "ShowChart", "Show Chart", @"", 0, @"true", "4DA044A3-45E5-4EC4-8B45-2E2C752A8A6B" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Chart Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "Chart Style", @"", 1, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "0652C0A0-DD68-45C7-A83B-F5630076A473" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Default Chart Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Chart Date Range", "SlidingDateRange", "Default Chart Date Range", @"", 2, @"Current||Year||", "00484295-F1E0-4409-894B-EBC2C7DAB6BC" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Data View Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Data View Categories", "DataViewCategories", "Data View Categories", @"The categories from which the Audience and Autocomplete data view options can be selected. If empty, all data views will be available.", 7, @"", "44C01B07-6288-4EC8-82BC-8263D53F90BF" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Bulk Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Bulk Entry Page", "BulkEntryPage", "Bulk Entry Page", @"The page to use for bulk entry of steps data", 8, @"", "FE76CB51-678E-4269-B4E2-1D5A70C70136" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Key Performance Indicator Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Key Performance Indicator Lava", "KpiLava", "Key Performance Indicator Lava", @"The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>", 9, @"{[kpis style:'card' iconbackground:'true' columncount:'4']}
    [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing' color:'blue-700']][[ endkpi ]]
    {% if StepType.HasEndDate %}
        [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete' color:'green-600']][[ endkpi ]]
        [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
    {% endif %}
    [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}", "AAF9EFAC-F590-4A6D-8A02-F6312456AFAD" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: Step Program
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "Programs", "Step Program", @"Display Step Types from a specified program. If none selected, the block will display the program from the current context.", 1, @"", "9F023BD3-7DB0-478F-821D-0F6139A2F843" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 2, @"", "F9383E15-12D6-462D-A9BB-5BD58AD59E9D" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: Bulk Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Bulk Entry", "BulkEntryPage", "Bulk Entry", @"Linked page that allows for bulk entry of steps for a step type.", 3, @"", "8D1F81C4-4160-430C-AC09-F5D09F03DB77" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "68F01095-D1CD-454C-92F4-A315E9F75F63" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5A6F2720-0583-4D15-8270-2327C18DD421" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F0F3AD2-4989-4F50-B394-0DE3C7AF35AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the streak type details.", 0, @"", "EA8F9F3F-37A7-457A-AEFC-DCA775E5703E" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F0F3AD2-4989-4F50-B394-0DE3C7AF35AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "778F5AD0-26F1-42F6-83ED-ADF27DC6F4A7" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F0F3AD2-4989-4F50-B394-0DE3C7AF35AD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A83E3D2C-7E32-4EAC-A3AD-21BFB274261F" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "20C68613-F253-4D2F-A465-62AFBB01DCD6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the event calendar item details.", 0, @"", "8759A23A-7888-4511-89F9-FA7F93FBFD42" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "20C68613-F253-4D2F-A465-62AFBB01DCD6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9FF9F371-554C-49CF-9897-D486A8E23F44" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "20C68613-F253-4D2F-A465-62AFBB01DCD6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DF997F14-BEF3-4102-8D04-9966EC61F1C9" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Active List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E899CCB-3C24-4F7D-9843-2F1CB00AED8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the registration instance details.", 0, @"", "3B08322A-E85F-4979-BC27-B6945639F1C2" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Active List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E899CCB-3C24-4F7D-9843-2F1CB00AED8F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2146BE9A-5DDD-45B5-8543-619A4CD9A121" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Active List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E899CCB-3C24-4F7D-9843-2F1CB00AED8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "ACE58FE1-EEB9-4BE9-8948-55AADAD63E0D" );

            // Attribute for BlockType
            //   BlockType: Business List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1E60C390-98C4-404D-AEE8-F9E3E9C69705", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the business details.", 0, @"", "394D47F0-7211-4AC0-B633-0DB4CEBB5C40" );

            // Attribute for BlockType
            //   BlockType: Business List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1E60C390-98C4-404D-AEE8-F9E3E9C69705", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "38447B9B-C45A-43E6-91C6-63CF892F9BA9" );

            // Attribute for BlockType
            //   BlockType: Business List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1E60C390-98C4-404D-AEE8-F9E3E9C69705", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "9FDEEF99-ED8C-4335-B6A3-2F0A920CEE5D" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0F99866A-7FAB-462D-96EB-9F9534322C57", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the financial gateway details.", 0, @"", "9922A104-9184-45BE-BF98-35014E3B9D65" );

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
            //   BlockType: Bank Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1DCE349-2F5B-46ED-9F3D-8812AF857F69", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "449AB58C-668B-439F-A38C-1872E0962F1B" );

            // Attribute for BlockType
            //   BlockType: Bank Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1DCE349-2F5B-46ED-9F3D-8812AF857F69", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F7272148-6D24-44C8-8748-2DBB4B8A49B6" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "CB0C4FAE-72C8-4DFC-A219-A2D5431AAF4A" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: View Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "View Page", "ViewPage", "View Page", @"", 0, @"", "63FBC2CC-31CC-4F21-B76D-3CC38318F377" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Add Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Page", "AddPage", "Add Page", @"", 0, @"", "1250735C-D1AA-4613-A88E-53FF267D8181" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"Limit the results to scheduled transactions that match the selected accounts.", 2, @"", "F530482A-DB36-4E5E-BEA4-3BE9574D2A78" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Person Token Expire Minutes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "Person Token Expire Minutes", @"When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.", 3, @"60", "1078698A-34A5-42AC-BC80-0C265AF72789" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Person Token Usage Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "Person Token Usage Limit", @"When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.", 4, @"1", "6A1E9CB8-01DA-4BD8-82E0-D2324852752A" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Show Transaction Type Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Transaction Type Column", "ShowTransactionTypeColumn", "Show Transaction Type Column", @"Show the Transaction Type column.", 5, @"False", "0CD8D004-1EFB-4E28-A972-B38656491B83" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E369A0B1-79D9-4B46-B0EF-7419F0BD37E5" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3F758072-F32A-4C20-BDF0-2A22EC5C1F9A" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the financial statement template details.", 0, @"", "55E63235-32AA-4F6E-92F8-3D8883B09DB1" );

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
            //   BlockType: Group Type List
            //   Category: Group
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8885F47D-9262-48B0-B969-9BEE003370EB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the group type details.", 0, @"", "8698480F-2097-45D6-88AE-4BEEEA190415" );

            // Attribute for BlockType
            //   BlockType: Group Type List
            //   Category: Group
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8885F47D-9262-48B0-B969-9BEE003370EB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F7CBEFCD-4988-4BDF-B000-6B6D3E2E7F0B" );

            // Attribute for BlockType
            //   BlockType: Group Type List
            //   Category: Group
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8885F47D-9262-48B0-B969-9BEE003370EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "55B989F7-7CE6-4760-AFFB-6E4F41A28188" );

            // Attribute for BlockType
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B8A5A3D-BF9D-4319-B7E5-06757FA44759", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the group member schedule template details.", 0, @"", "39573DA7-9216-4077-B411-EEB80C4EA93B" );

            // Attribute for BlockType
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B8A5A3D-BF9D-4319-B7E5-06757FA44759", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "54AC4812-1986-40AB-801D-D270E3C1EEDF" );

            // Attribute for BlockType
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B8A5A3D-BF9D-4319-B7E5-06757FA44759", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "23701862-5CDE-43BF-BE3B-290AC0CDE74D" );

            // Attribute for BlockType
            //   BlockType: Interaction Component Detail
            //   Category: Reporting
            //   Attribute: Default Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BC2034D1-416B-4FB4-9FFF-E202FA666203", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Template", "DefaultTemplate", "Default Template", @"The Lava template to use as default.", 0, @"<div class='row'>
        <div class='col-md-6'>
            <dl><dt>Name</dt><dd>{{ InteractionComponent.Name }}<dd/></dl>
        </div>
        {% if InteractionComponentEntity != '' %}
            <div class='col-md-6'>
                <dl>
                    <dt>Entity Name</dt><dd>{{ InteractionComponentEntity }}<dd/>
                </dl>
            </div>
        {% endif %}
    </div>", "AD5CF382-AC7F-4966-A54C-8C11FA98DF6D" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A46CC61-6110-4022-8ACE-EFE188A6AB5A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the data view details.", 0, @"", "CCFC79CF-E93B-41EB-BDDA-13B1BD2933B3" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A46CC61-6110-4022-8ACE-EFE188A6AB5A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "3DD95DE4-4ECD-4FE4-8B36-F11B6C0F7757" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A46CC61-6110-4022-8ACE-EFE188A6AB5A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FD808661-3013-4BA4-8721-B7E59CB9AB70" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A759218B-1C72-446C-8994-8559BA72941E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the page details.", 0, @"", "D400AC7B-A227-4768-A8B2-7B403A0CAF17" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A759218B-1C72-446C-8994-8559BA72941E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "887084AE-416A-4389-916A-99466BDEBEAC" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A759218B-1C72-446C-8994-8559BA72941E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "84D1AB87-B286-47F4-8851-6D25A65EACF2" );

            // Add Block Attribute Value
            //   Block: Attendance History
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Block Location: Page=History, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "56FD2E7F-E928-4945-9483-FC7A705A5A22", "76CE6D5B-4C1D-4E6B-A242-E35F967EC0EF", @"" );

            // Add Block Attribute Value
            //   Block: Attendance History
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Block Location: Page=History, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "56FD2E7F-E928-4945-9483-FC7A705A5A22", "2DE2C714-2A79-4671-98CB-7B70381808C1", @"False" );

            // Add Block Attribute Value
            //   Block: Attendance History
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Block Location: Page=History V1, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "161EEB4F-8642-4D65-8CB4-1CA821A4F9AE", "2DE2C714-2A79-4671-98CB-7B70381808C1", @"False" );

            // Add Block Attribute Value
            //   Block: Attendance History
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Block Location: Page=History V1, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "161EEB4F-8642-4D65-8CB4-1CA821A4F9AE", "76CE6D5B-4C1D-4E6B-A242-E35F967EC0EF", @"" );

            // Add Block Attribute Value
            //   Block: Pledge List
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Block Location: Page=Pledges, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "ADC6FE06-2898-46D0-AA0F-DECB82EA6560", "6E75CF0E-61F7-4D1D-ABB4-D19E94D4EA20", @"" );

            // Add Block Attribute Value
            //   Block: Pledge List
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Block Location: Page=Pledges, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "ADC6FE06-2898-46D0-AA0F-DECB82EA6560", "71ED8220-483D-4511-BC46-44085FADFD45", @"False" );

            // Add Block Attribute Value
            //   Block: Note Watch List
            //   BlockType: Note Watch List
            //   Category: Core
            //   Block Location: Page=Note Watches, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "F454492A-DEA8-449D-9098-5D92C6FDF81C", "D957C5D0-5BA3-44A3-B61C-0CF3BB766EA3", @"" );

            // Add Block Attribute Value
            //   Block: Note Watch List
            //   BlockType: Note Watch List
            //   Category: Core
            //   Block Location: Page=Note Watches, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F454492A-DEA8-449D-9098-5D92C6FDF81C", "B8B0249A-39CE-48D3-B9EF-57967AD105DB", @"False" );

            // Add Block Attribute Value
            //   Block: Note Watch Detail
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Block Location: Page=Note Watch Detail, Site=Rock RMS
            //   Attribute: Watched Note Lava Template
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "4C9B2479-9111-48BE-9F33-3C5196C7A1C1", "0E6D23E7-D4E8-43D1-962B-4FB28A46E133", @"" );

            // Add Block Attribute Value
            //   Block: Rest Controller List
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Block Location: Page=REST Controllers, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "CCCDC039-2A14-49AC-8A42-75DF24BEC027", "903215E0-2070-4563-9E72-04550881A73D", @"" );

            // Add Block Attribute Value
            //   Block: Rest Controller List
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Block Location: Page=REST Controllers, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CCCDC039-2A14-49AC-8A42-75DF24BEC027", "D63530B0-69F2-4025-8A38-6EEF5A3EC6A1", @"False" );

            // Add Block Attribute Value
            //   Block: Rest Controller Detail
            //   BlockType: Rest Action List
            //   Category: Core
            //   Block Location: Page=REST Controller Actions, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "B9EEC112-B2BA-43CF-A2A2-554D93770635", "82DC480F-F111-4E6B-9FF1-00B844F0B97D", @"" );

            // Add Block Attribute Value
            //   Block: Rest Controller Detail
            //   BlockType: Rest Action List
            //   Category: Core
            //   Block Location: Page=REST Controller Actions, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B9EEC112-B2BA-43CF-A2A2-554D93770635", "2ABDE16A-8D8F-4600-8266-19EA1E4F6718", @"False" );

            // Add Block Attribute Value
            //   Block: Account List
            //   BlockType: Account List
            //   Category: Finance
            //   Block Location: Page=Accounts, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "033DA2BD-A8F1-4A0C-A7E2-75C2C08507B7", "3507887E-92C0-49FD-9510-0376E32F1AD7", @"" );

            // Add Block Attribute Value
            //   Block: Account List
            //   BlockType: Account List
            //   Category: Finance
            //   Block Location: Page=Accounts, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "033DA2BD-A8F1-4A0C-A7E2-75C2C08507B7", "F5668EDF-C68D-42B9-A577-17D5C007C9F0", @"False" );

            // Add Block Attribute Value
            //   Block: Top-Level Account List
            //   BlockType: Account List
            //   Category: Finance
            //   Block Location: Page=Order Top-Level Accounts, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "FAB9425E-0350-42E1-A7DC-58884ACD9D08", "F5668EDF-C68D-42B9-A577-17D5C007C9F0", @"False" );

            // Add Block Attribute Value
            //   Block: Top-Level Account List
            //   BlockType: Account List
            //   Category: Finance
            //   Block Location: Page=Order Top-Level Accounts, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "FAB9425E-0350-42E1-A7DC-58884ACD9D08", "3507887E-92C0-49FD-9510-0376E32F1AD7", @"" );

            // Add Block Attribute Value
            //   Block: Group Archived List
            //   BlockType: Group Archived List
            //   Category: Utility
            //   Block Location: Page=Archived Groups, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "CE6978FF-603A-43E4-A3D6-ACD795256782", "E4D77D88-059D-4FB7-A052-3F8FC192A0CC", @"" );

            // Add Block Attribute Value
            //   Block: Group Archived List
            //   BlockType: Group Archived List
            //   Category: Utility
            //   Block Location: Page=Archived Groups, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CE6978FF-603A-43E4-A3D6-ACD795256782", "5D072230-25F0-4271-BCB5-91F505B96063", @"False" );

            // Add Block Attribute Value
            //   Block: Schedule List
            //   BlockType: Schedule List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "7D94B359-2E51-40C5-A859-3BCFC0A879BB", "437A9BF3-32C7-4E83-B523-8A5C1AE3E5BA", @"" );

            // Add Block Attribute Value
            //   Block: Schedule List
            //   BlockType: Schedule List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "7D94B359-2E51-40C5-A859-3BCFC0A879BB", "D2D6B41C-095F-4E01-84F8-07E7C98F84AB", @"False" );

            // Add Block Attribute Value
            //   Block: Schedule List
            //   BlockType: Schedule List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "1F45DFF9-8293-4751-ACB8-AD7019F97040", "D2D6B41C-095F-4E01-84F8-07E7C98F84AB", @"False" );

            // Add Block Attribute Value
            //   Block: Schedule List
            //   BlockType: Schedule List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "1F45DFF9-8293-4751-ACB8-AD7019F97040", "437A9BF3-32C7-4E83-B523-8A5C1AE3E5BA", @"" );

            // Add Block Attribute Value
            //   Block: Attendance List
            //   BlockType: Attendance List
            //   Category: Check-in
            //   Block Location: Page=Attendance List, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "2FE2C1A6-5507-4134-9062-DFD1CCC5268E", "2CB2EC8F-172E-4F86-BD0F-9B828FAA868D", @"" );

            // Add Block Attribute Value
            //   Block: Attendance List
            //   BlockType: Attendance List
            //   Category: Check-in
            //   Block Location: Page=Attendance List, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2FE2C1A6-5507-4134-9062-DFD1CCC5268E", "D9039B97-28B8-4C33-98F6-91925F9FE04F", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Category Selection
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "4896146D-3BB3-445E-A98C-D712AAC68189", @"" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Default Category
            /*   Attribute Value: 4B2D88F5-6E45-4B4B-8776-11118C8E8269 */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "17A383DC-D8A7-461E-A1F5-708A74655820", @"4B2D88F5-6E45-4B4B-8776-11118C8E8269" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Enable Auto Approve
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "5B097154-B1D7-48B1-93CA-E2BF9D140ED5", @"True" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Expires After (Days)
            /*   Attribute Value: 14 */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "D5B048C5-EA2E-4541-ADED-73E839B0CFAC", @"14" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Enable Urgent Flag
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "0675AEF0-594F-4A1C-8002-5C33A6B3BA37", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Enable Comments Flag
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "EBE5D230-7205-4DE7-9AC2-41846449CE76", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Default Allow Comments Setting
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "3843DC95-9B14-4023-AA8A-B1DE3E90F3CF", @"True" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Enable Public Display Flag
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "667900B2-3A1E-49CF-A830-339C9FDE8E2B", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Default To Public
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "B67F84DE-B25D-4319-A4D2-102187D23CB3", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Character Limit
            /*   Attribute Value: 250 */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "5BE3E0B1-82D1-4D7A-8AE8-3F98DAEEE83F", @"250" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Require Last Name
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "AFD6A3D7-7051-47AB-A1F1-9C2FF54BD981", @"True" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Show Campus
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "A6290F64-029E-4A34-8873-0B28965CA0B3", @"True" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Require Campus
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "6B48F492-145D-498F-A529-26CF9FBEFCE1", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Enable Person Matching
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "F4795052-F971-438B-8D40-2AED4EE02DF9", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Create Person If No Match Found
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "170FCF6F-ABA2-4504-8D89-36BE06E299EC", @"True" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Connection Status
            /*   Attribute Value: 8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061 */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "A41486C2-A14B-4E8F-B4CE-3B9B032B30B7", @"8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Record Status
            /*   Attribute Value: 283999EC-7346-42E3-B807-BCE9B2BABB49 */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "15C53879-3325-4F43-BD4D-FCCC0CE83CEB", @"283999EC-7346-42E3-B807-BCE9B2BABB49" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Navigate To Parent On Save
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "6567BC62-2DAB-4309-8BA8-ED052DEB8A3B", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Refresh Page On Save
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "9D12468F-F9DB-43EC-A9AF-30211A475A03", @"False" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Save Success Text
            /*   Attribute Value: <p>Thank you for allowing us to pray for you.</p> */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "10670B9B-9452-4292-9AE1-90CB4E3174A4", @"<p>Thank you for allowing us to pray for you.</p>" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Workflow
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "061A0909-2CB7-4C1D-B87F-90876198194A", @"" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Campus Types
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "F8B92BB5-6A32-4090-A4BB-BC6E4127C53E", @"" );

            // Add Block Attribute Value
            //   Block: Add Prayer Request
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Block Location: Page=Prayer, Site=External Website
            //   Attribute: Campus Statuses
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24", "47C232B1-45A0-435A-A522-6AD07AADE1FD", @"" );

            // Add Block Attribute Value
            //   Block: Event List
            //   BlockType: Event List
            //   Category: Follow
            //   Block Location: Page=Following Events, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "FDAC05B9-9488-44F4-8958-FA0995EC8F6C", "5AED54CE-EE09-4B87-9244-1BC3DD6DD212", @"" );

            // Add Block Attribute Value
            //   Block: Event List
            //   BlockType: Event List
            //   Category: Follow
            //   Block Location: Page=Following Events, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "FDAC05B9-9488-44F4-8958-FA0995EC8F6C", "CB07072C-89D5-4952-B237-A2AEE821952F", @"False" );

            // Add Block Attribute Value
            //   Block: Personal Link Section List
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Block Location: Page=Shared Links, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "53D889D5-2973-47E4-9254-00A4FA0002FB", "0035FB03-5320-4961-8A92-4C1CE6E59037", @"" );

            // Add Block Attribute Value
            //   Block: Personal Link Section List
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Block Location: Page=Shared Links, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "53D889D5-2973-47E4-9254-00A4FA0002FB", "B56E4B9A-C4EB-4E34-B4AA-346B937D9A2D", @"False" );

            // Add Block Attribute Value
            //   Block: Personal Link Section List
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Block Location: Page=Personal Links, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B27F9437-2EDB-43B7-8BD7-038A2C531222", "B56E4B9A-C4EB-4E34-B4AA-346B937D9A2D", @"False" );

            // Add Block Attribute Value
            //   Block: Personal Link Section List
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Block Location: Page=Personal Links, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "B27F9437-2EDB-43B7-8BD7-038A2C531222", "0035FB03-5320-4961-8A92-4C1CE6E59037", @"" );

            // Add Block Attribute Value
            //   Block: User Logins
            //   BlockType: User Login List
            //   Category: Security
            //   Block Location: Page=User Accounts, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "8139EF3B-CEA2-4681-962F-9C71DC2DA121", "E5331B78-AEE5-474F-93F1-4A979A742432", @"" );

            // Add Block Attribute Value
            //   Block: User Logins
            //   BlockType: User Login List
            //   Category: Security
            //   Block Location: Page=User Accounts, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "8139EF3B-CEA2-4681-962F-9C71DC2DA121", "A071F2C3-348D-452B-81F6-6420971BE032", @"False" );

            // Add Block Attribute Value
            //   Block: User Logins
            //   BlockType: User Login List
            //   Category: Security
            //   Block Location: Page=Security, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "3B2BADBB-9725-4BBF-8D83-B64ACB18A7EB", "A071F2C3-348D-452B-81F6-6420971BE032", @"False" );

            // Add Block Attribute Value
            //   Block: User Logins
            //   BlockType: User Login List
            //   Category: Security
            //   Block Location: Page=Security, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "3B2BADBB-9725-4BBF-8D83-B64ACB18A7EB", "E5331B78-AEE5-474F-93F1-4A979A742432", @"" );

            // Add Block Attribute Value
            //   Block: OpenID Connect Clients
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Block Location: Page=OpenID Connect Clients, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "79E7A836-2D54-4D29-8B10-3DB543AB9E71", "4B1443B2-C316-4CBD-AE40-1C9C92738C91", @"" );

            // Add Block Attribute Value
            //   Block: OpenID Connect Clients
            //   BlockType: OpenID Connect Clients
            //   Category: Security > OIDC
            //   Block Location: Page=OpenID Connect Clients, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "79E7A836-2D54-4D29-8B10-3DB543AB9E71", "4C973144-E2D9-4F30-88E4-7E51276A37FD", @"False" );

            // Add Block Attribute Value
            //   Block: Personal Link List
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Block Location: Page=Section Detail, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "50F5D213-67A2-4762-947E-EDAE94BD34EB", "B05051A9-BDF6-4C56-B4F5-DE2BEA71661C", @"" );

            // Add Block Attribute Value
            //   Block: Personal Link List
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Block Location: Page=Section Detail, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "50F5D213-67A2-4762-947E-EDAE94BD34EB", "170438D9-2B7C-4128-8D27-11C4D5022F0C", @"False" );

            // Add Block Attribute Value
            //   Block: Personal Link List
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Block Location: Page=Section Detail, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "CC8F9C06-FB4F-4C98-B17D-AF2827BE3D53", "B05051A9-BDF6-4C56-B4F5-DE2BEA71661C", @"" );

            // Add Block Attribute Value
            //   Block: Personal Link List
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Block Location: Page=Section Detail, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CC8F9C06-FB4F-4C98-B17D-AF2827BE3D53", "170438D9-2B7C-4128-8D27-11C4D5022F0C", @"False" );

            // Add Block Attribute Value
            //   Block: Location List
            //   BlockType: Location List
            //   Category: Core
            //   Block Location: Page=Location Editor, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "4DAA030A-67DA-4298-89E3-E9524D2C97AF", "AB9E967F-1166-418C-99E5-16D774006602", @"" );

            // Add Block Attribute Value
            //   Block: Location List
            //   BlockType: Location List
            //   Category: Core
            //   Block Location: Page=Location Editor, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "4DAA030A-67DA-4298-89E3-E9524D2C97AF", "A1108432-C93D-4569-B9CE-172EE4871668", @"False" );

            // Add Block Attribute Value
            //   Block: Group Requirement Type List
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Block Location: Page=Group Requirement Types, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "4E1DCCD7-A1CE-4B87-B3D3-2AD4987FA3D5", "B4C1A31A-A8A8-4DA7-A00F-BAE7A0515B2E", @"" );

            // Add Block Attribute Value
            //   Block: Group Requirement Type List
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Block Location: Page=Group Requirement Types, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "4E1DCCD7-A1CE-4B87-B3D3-2AD4987FA3D5", "BA3CBA61-F4DC-487F-B7CA-F609336505D6", @"False" );

            // Add Block Attribute Value
            //   Block: Suggestion List
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Block Location: Page=Following Suggestions, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "55D9C593-C399-4E4B-98A4-C3913C0A7694", "B3F4D9E2-1A2A-4E3A-BA7F-D0ADA36721E7", @"" );

            // Add Block Attribute Value
            //   Block: Suggestion List
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Block Location: Page=Following Suggestions, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "55D9C593-C399-4E4B-98A4-C3913C0A7694", "50B25D69-0D2F-4D29-88D3-27DD830BDDDC", @"False" );
            RockMigrationHelper.UpdateFieldType( "Open AI Model Picker", "", "Rock", "Rock.Field.Types.OpenAIModelPickerFieldType", "216F0510-90ED-4CCB-BC8B-2F5F1470DC2C" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show Additional Email Recipients
            RockMigrationHelper.DeleteAttribute( "049B5763-868C-4F5B-8CBE-CF14100D6C0B" );

            // Attribute for BlockType
            //   BlockType: Profile Details
            //   Category: Mobile > Cms
            //   Attribute: Display Campus Statuses
            RockMigrationHelper.DeleteAttribute( "B6F2794B-A305-49B4-9808-22D01ACBA421" );

            // Attribute for BlockType
            //   BlockType: Profile Details
            //   Category: Mobile > Cms
            //   Attribute: Display Campus Types
            RockMigrationHelper.DeleteAttribute( "9A495075-BCF8-42BC-A89B-D85777D1E5EB" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "9149656D-ADAA-4B88-BD4D-2275BAE444E1" );

            // Attribute for BlockType
            //   BlockType: Site List
            //   Category: CMS
            //   Attribute: Show Site Icon
            RockMigrationHelper.DeleteAttribute( "BBC6692A-4439-436E-ADE9-AC48DF4A6A93" );

            // Attribute for BlockType
            //   BlockType: Site List
            //   Category: CMS
            //   Attribute: Block Title
            RockMigrationHelper.DeleteAttribute( "584E49DA-D61F-4BDC-A79C-0A11A5B45EAD" );

            // Attribute for BlockType
            //   BlockType: Site Detail
            //   Category: CMS
            //   Attribute: Default File Type
            RockMigrationHelper.DeleteAttribute( "79A18B03-89A9-4889-BBDB-000D452E4084" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "84D1AB87-B286-47F4-8851-6D25A65EACF2" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "887084AE-416A-4389-916A-99466BDEBEAC" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "D400AC7B-A227-4768-A8B2-7B403A0CAF17" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "FD808661-3013-4BA4-8721-B7E59CB9AB70" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "3DD95DE4-4ECD-4FE4-8B36-F11B6C0F7757" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "CCFC79CF-E93B-41EB-BDDA-13B1BD2933B3" );

            // Attribute for BlockType
            //   BlockType: Interaction Component Detail
            //   Category: Reporting
            //   Attribute: Default Template
            RockMigrationHelper.DeleteAttribute( "AD5CF382-AC7F-4966-A54C-8C11FA98DF6D" );

            // Attribute for BlockType
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "23701862-5CDE-43BF-BE3B-290AC0CDE74D" );

            // Attribute for BlockType
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "54AC4812-1986-40AB-801D-D270E3C1EEDF" );

            // Attribute for BlockType
            //   BlockType: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "39573DA7-9216-4077-B411-EEB80C4EA93B" );

            // Attribute for BlockType
            //   BlockType: Group Type List
            //   Category: Group
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "55B989F7-7CE6-4760-AFFB-6E4F41A28188" );

            // Attribute for BlockType
            //   BlockType: Group Type List
            //   Category: Group
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F7CBEFCD-4988-4BDF-B000-6B6D3E2E7F0B" );

            // Attribute for BlockType
            //   BlockType: Group Type List
            //   Category: Group
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "8698480F-2097-45D6-88AE-4BEEEA190415" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "9F1F79D1-6190-4EE5-9EF8-F244116EE472" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "7FF05AB4-C1B4-449D-9D6F-823C98041772" );

            // Attribute for BlockType
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "55E63235-32AA-4F6E-92F8-3D8883B09DB1" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "3F758072-F32A-4C20-BDF0-2A22EC5C1F9A" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E369A0B1-79D9-4B46-B0EF-7419F0BD37E5" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Show Transaction Type Column
            RockMigrationHelper.DeleteAttribute( "0CD8D004-1EFB-4E28-A972-B38656491B83" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Person Token Usage Limit
            RockMigrationHelper.DeleteAttribute( "6A1E9CB8-01DA-4BD8-82E0-D2324852752A" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Person Token Expire Minutes
            RockMigrationHelper.DeleteAttribute( "1078698A-34A5-42AC-BC80-0C265AF72789" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute( "F530482A-DB36-4E5E-BEA4-3BE9574D2A78" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Add Page
            RockMigrationHelper.DeleteAttribute( "1250735C-D1AA-4613-A88E-53FF267D8181" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: View Page
            RockMigrationHelper.DeleteAttribute( "63FBC2CC-31CC-4F21-B76D-3CC38318F377" );

            // Attribute for BlockType
            //   BlockType: Financial Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "CB0C4FAE-72C8-4DFC-A219-A2D5431AAF4A" );

            // Attribute for BlockType
            //   BlockType: Bank Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F7272148-6D24-44C8-8748-2DBB4B8A49B6" );

            // Attribute for BlockType
            //   BlockType: Bank Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "449AB58C-668B-439F-A38C-1872E0962F1B" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "30DBBC5F-D162-4A45-828F-FB5B9790909C" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "78F010B3-C8BB-46CB-92EE-7EB6547C962D" );

            // Attribute for BlockType
            //   BlockType: Gateway List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "9922A104-9184-45BE-BF98-35014E3B9D65" );

            // Attribute for BlockType
            //   BlockType: Business List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "9FDEEF99-ED8C-4335-B6A3-2F0A920CEE5D" );

            // Attribute for BlockType
            //   BlockType: Business List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "38447B9B-C45A-43E6-91C6-63CF892F9BA9" );

            // Attribute for BlockType
            //   BlockType: Business List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "394D47F0-7211-4AC0-B633-0DB4CEBB5C40" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Active List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "ACE58FE1-EEB9-4BE9-8948-55AADAD63E0D" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Active List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "2146BE9A-5DDD-45B5-8543-619A4CD9A121" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Active List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "3B08322A-E85F-4979-BC27-B6945639F1C2" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "DF997F14-BEF3-4102-8D04-9966EC61F1C9" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9FF9F371-554C-49CF-9897-D486A8E23F44" );

            // Attribute for BlockType
            //   BlockType: Calendar Event Item List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "8759A23A-7888-4511-89F9-FA7F93FBFD42" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "A83E3D2C-7E32-4EAC-A3AD-21BFB274261F" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "778F5AD0-26F1-42F6-83ED-ADF27DC6F4A7" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "EA8F9F3F-37A7-457A-AEFC-DCA775E5703E" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5A6F2720-0583-4D15-8270-2327C18DD421" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "68F01095-D1CD-454C-92F4-A315E9F75F63" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: Bulk Entry
            RockMigrationHelper.DeleteAttribute( "8D1F81C4-4160-430C-AC09-F5D09F03DB77" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "F9383E15-12D6-462D-A9BB-5BD58AD59E9D" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: Step Program
            RockMigrationHelper.DeleteAttribute( "9F023BD3-7DB0-478F-821D-0F6139A2F843" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Key Performance Indicator Lava
            RockMigrationHelper.DeleteAttribute( "AAF9EFAC-F590-4A6D-8A02-F6312456AFAD" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Bulk Entry Page
            RockMigrationHelper.DeleteAttribute( "FE76CB51-678E-4269-B4E2-1D5A70C70136" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Data View Categories
            RockMigrationHelper.DeleteAttribute( "44C01B07-6288-4EC8-82BC-8263D53F90BF" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Default Chart Date Range
            RockMigrationHelper.DeleteAttribute( "00484295-F1E0-4409-894B-EBC2C7DAB6BC" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Chart Style
            RockMigrationHelper.DeleteAttribute( "0652C0A0-DD68-45C7-A83B-F5630076A473" );

            // Attribute for BlockType
            //   BlockType: Step Type Detail
            //   Category: Steps
            //   Attribute: Show Chart
            RockMigrationHelper.DeleteAttribute( "4DA044A3-45E5-4EC4-8B45-2E2C752A8A6B" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "9CD01960-832A-4332-AB57-0B532E555F28" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "5C018E03-A98A-43E6-9579-8B738B2D6681" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "92338117-0F02-42A1-84D3-A24B675D789D" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: Categories
            RockMigrationHelper.DeleteAttribute( "0E83A4B2-A87F-46BC-AD0B-11DA7AB4AF2D" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "18953D12-97A1-4C5B-9862-FBEEF66B8A21" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "5DF54F82-B920-4DC8-A98F-90EEEB489F25" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: Show Note Column
            RockMigrationHelper.DeleteAttribute( "B9B99603-9FFD-4449-8845-9151C7638861" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute( "B034203A-F007-4F7B-8E0A-DAC09781D354" );

            // Attribute for BlockType
            //   BlockType: Step Participant List
            //   Category: Steps
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E8F10140-A221-45AD-81EC-55D57C51D5B9" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F5211493-99C2-494C-84DD-D8DE602BC27E" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C1F8D81F-D59C-4DF0-AF19-D776898FA8C1" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "87F2BA25-37AF-4A50-A1B1-61CE8905026C" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "FEE6722A-32E5-43B5-BBED-49D6EE6366C4" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "5E15CC7B-190E-4547-A4EE-20BA78219B96" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "AC750FE6-33DF-4617-B9C1-38EEEEB781E2" );

            // Attribute for BlockType
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "0CC87BD8-FD68-4B86-B65B-77FC3F7105E7" );

            // Attribute for BlockType
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E9DDF9B9-839B-4F2D-AA5A-51F507AB0734" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Number of Questions
            RockMigrationHelper.DeleteAttribute( "D25A1036-E619-417E-8AE6-36F128A0B1EA" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Set Page Icon
            RockMigrationHelper.DeleteAttribute( "2CD8C9D5-4278-489B-9822-1E336313A779" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Set Page Title
            RockMigrationHelper.DeleteAttribute( "297DCD1D-943C-4463-ACC2-332F661A103E" );

            // Attribute for BlockType
            //   BlockType: Disc
            //   Category: CRM
            //   Attribute: Instructions
            RockMigrationHelper.DeleteAttribute( "F7052876-99FC-4A1B-8056-EBBE849FD1D4" );

            // Attribute for BlockType
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "E39490AF-EAFA-4D53-9DE9-9081A5499D47" );

            // Attribute for BlockType
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E55B952E-A492-47EA-83F8-F5DB313C059C" );

            // Attribute for BlockType
            //   BlockType: Badge List
            //   Category: CRM
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "79D2AFB7-824F-479C-96F5-540A4255749E" );

            // Attribute for BlockType
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "AD70FEE2-736A-4D90-BA54-B16CAFD9CE94" );

            // Attribute for BlockType
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "723C3BEA-F72D-4B86-A0DA-92FAB1CBC973" );

            // Attribute for BlockType
            //   BlockType: Assessment Type List
            //   Category: CRM
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "100D6999-8F0C-4EB7-92D1-537DBD385AEB" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D56C9E63-3366-4030-9517-479A842AAA01" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "4D82E702-7520-4BCD-A731-787A453E6F48" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "76783565-690B-41B7-9E50-20102A44549E" );

            // Attribute for BlockType
            //   BlockType: Tag List
            //   Category: Core
            //   Attribute: Show Qualifier Columns
            RockMigrationHelper.DeleteAttribute( "55DA3A81-0A73-494B-A4FD-35C1CE5C9EB7" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "93F34441-1109-49F8-81E8-521656A00DD1" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "7CD92079-5E56-4FB3-914F-7DF6247BD1A2" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "C5246895-FFEF-41FF-9E26-8F5AB5783C79" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "2A28086F-F2D2-49AB-93E6-0ED5CCF584AF" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "DD991CEC-C1C3-4FE8-86F2-E27296A76D80" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "28E98675-BA87-4541-96F8-D58D557809C7" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "25C5C011-3910-4803-B23E-215664088D36" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "21075366-2E56-44B5-A32B-024CFED9EC59" );

            // Attribute for BlockType
            //   BlockType: Person Suggestion List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "9FC141F9-EEBE-4F10-AA01-E1C87D8D483E" );

            // Attribute for BlockType
            //   BlockType: Person Suggestion List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "4721D4D4-5C67-4D48-8899-718F61030587" );

            // Attribute for BlockType
            //   BlockType: Person Following List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "FF99A423-17FC-4430-A968-7FF8AE755D41" );

            // Attribute for BlockType
            //   BlockType: Person Following List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "60AAFCAC-553B-430D-9F33-1B5BBF214F3D" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "FB060D89-3347-4D87-8426-6653E5FD51B3" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "23CDA7C6-A3E5-461A-87AF-73A688117AED" );

            // Attribute for BlockType
            //   BlockType: Device List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E58A1D17-6664-4F0C-A8CA-1727C52F1191" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "4839CE2D-D104-4312-872A-B1BF206C57C1" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "92DC50F2-3FCB-448B-9140-9ADDF6607134" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: Categories
            RockMigrationHelper.DeleteAttribute( "FC4ABBCD-CE55-4E0C-82D7-15A3BC267AE9" );

            // Attribute for BlockType
            //   BlockType: Defined Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "96E6B468-4D42-4D7F-9660-7154A44177DD" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "462EEE93-F09F-49C2-A6E9-82700D2DDC03" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9788BE43-E6FD-4AC3-8E46-1B514871DD84" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "657F5CDE-0727-4EE2-81A4-3052FF6E39AE" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "3714D9CD-58C5-4B7D-85D8-63A9C7D8FEBA" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "A0273D2A-082B-4806-97A6-1BE4E18D0615" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: Binary File Type
            RockMigrationHelper.DeleteAttribute( "E4B2DBD3-5812-4B32-B455-26B49B5BB995" );

            // Attribute for BlockType
            //   BlockType: Binary File List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "7CBF6711-D34C-4090-A06B-59C07DDF54CA" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "2BF4400C-19B0-4C1C-80F2-C96559584FC1" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "272E9A4A-0515-46A9-819C-3B1E5B161F2B" );

            // Attribute for BlockType
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "6032EF50-8F86-4093-87EE-4F02AADAF2D0" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "8E2B6DEF-D02A-41FF-BFC3-CEFF5F5BBE69" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "DFFA9B37-1332-4B51-A6A3-A5FED0BF8939" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "10FA745A-A70D-45F4-AFFB-EF3B6FB52345" );

            // Attribute for BlockType
            //   BlockType: Sms Pipeline List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "8728839A-83C9-45E3-A776-3EC2B5D1E370" );

            // Attribute for BlockType
            //   BlockType: Sms Pipeline List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E4FDD860-4316-4589-A8A2-B2ABDE51F372" );

            // Attribute for BlockType
            //   BlockType: Sms Pipeline List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0DD51B5F-E16D-40D8-B934-6598015227A1" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5192EB07-ED42-4D0E-8BA4-0081711227FC" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B3DD2485-725A-44B2-9B30-CA47F8ADDF03" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5209A318-9C53-43E4-9511-AAC595FC3684" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: User Specific Folders
            RockMigrationHelper.DeleteAttribute( "CA6F5B1C-8385-44D4-BED4-0D3710D0FDF7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Image Root Folder
            RockMigrationHelper.DeleteAttribute( "7FF5F073-1F7A-48A6-9511-75F7D8463028" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Document Root Folder
            RockMigrationHelper.DeleteAttribute( "2796C859-AC47-40FA-B698-7DB0A8B25255" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.DeleteAttribute( "75545130-5EAA-4619-9D2D-CBE33FC4EA9D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Additional Email Recipients
            RockMigrationHelper.DeleteAttribute( "71EF060D-E70B-4733-822C-89D3AE6FD069" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Email Metrics Reminder Options
            RockMigrationHelper.DeleteAttribute( "AF5AD05E-A7A9-4784-B88C-FA82A88E8A65" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Default As Bulk
            RockMigrationHelper.DeleteAttribute( "3BBCB9A5-523D-4DB7-BACF-693A17EAEF94" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.DeleteAttribute( "99D2674D-803F-45FE-B082-F604104ABD8D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Simple Communications Are Bulk
            RockMigrationHelper.DeleteAttribute( "06B03251-CD7B-4E8E-8B72-B5D484DAE6E1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "FCF43ED8-A2B8-426F-A9CE-5F44A4F189E7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Attachment Uploader
            RockMigrationHelper.DeleteAttribute( "BCB16360-06CF-416E-A91C-E197FC4F4240" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Allow CC/Bcc
            RockMigrationHelper.DeleteAttribute( "BA82B409-2F29-4447-9910-45B03BEF5FA5" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Mode
            RockMigrationHelper.DeleteAttribute( "235CEBCB-8216-452C-A50C-C1EDA8F52C80" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Send When Approved
            RockMigrationHelper.DeleteAttribute( "57A5010D-69F3-47D7-9FD0-B6510F17958C" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Maximum Recipients
            RockMigrationHelper.DeleteAttribute( "C369DFC4-CFFA-4183-B7F2-090D2C7E604A" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Default Template
            RockMigrationHelper.DeleteAttribute( "8B9A5D60-A7DF-4E5F-813E-AF1DBD0A58C2" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Mediums
            RockMigrationHelper.DeleteAttribute( "11290B87-BFDF-4A1B-9976-E16DB1AA9132" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Person Parameter
            RockMigrationHelper.DeleteAttribute( "5C95D6FF-9736-4FBD-87BF-7CF739610C34" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "C19DF45C-152A-4FF3-B784-A610B7878398" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Lava
            RockMigrationHelper.DeleteAttribute( "C1A95B9F-9003-4DD3-9BA6-AE51FC05BAF7" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "0F77028D-6D77-4E75-897E-642F34F342EE" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "54AB7012-8515-42EC-9440-854DAFF40E0C" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "04A2FCF6-DE8F-4973-A9F9-055A860E7AEB" );

            // Attribute for BlockType
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6CB70101-5745-4DBB-9162-7D1C55E007CC" );

            // Attribute for BlockType
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "AB7E8098-ED68-4C0F-BC65-06B116F5A2D3" );

            // Attribute for BlockType
            //   BlockType: Page List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F24590B5-965B-4868-A136-75C42630503F" );

            // Attribute for BlockType
            //   BlockType: Page List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "6958A295-286E-461E-9F4D-2298C022328D" );

            // Attribute for BlockType
            //   BlockType: Page List
            //   Category: CMS
            //   Attribute: Show Page Id
            RockMigrationHelper.DeleteAttribute( "A441C9A7-781D-4E24-BD5D-E7A490804620" );

            // Attribute for BlockType
            //   BlockType: Media Folder List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "01005A24-256A-4556-B0D9-366CC4CFABB7" );

            // Attribute for BlockType
            //   BlockType: Media Folder List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "976C6535-929F-4BE8-9E4D-B0AF54628615" );

            // Attribute for BlockType
            //   BlockType: Media Folder List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "EBA09B93-0D35-425D-9DE0-1095B465A253" );

            // Attribute for BlockType
            //   BlockType: Media Account List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "1C7F5A05-88BB-4F34-8C8A-E9E8E906C2EB" );

            // Attribute for BlockType
            //   BlockType: Media Account List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "0D6A12F5-80D4-4577-892F-AA8C03CEDDE8" );

            // Attribute for BlockType
            //   BlockType: Media Account List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "64E612A9-EBC5-4BD6-8A5F-9851D00248C3" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Zip Upload
            RockMigrationHelper.DeleteAttribute( "BD031ADA-5D23-4237-A332-468FAC7282E9" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: File Editor Page
            RockMigrationHelper.DeleteAttribute( "80D4544B-563A-4109-9F61-F4E019580B3A" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Browse Mode
            RockMigrationHelper.DeleteAttribute( "C872B6A7-36F6-4771-807A-7B4A7E8BAD2C" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Root Folder
            RockMigrationHelper.DeleteAttribute( "14684245-5768-442D-9BFB-C80E1383775A" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height
            RockMigrationHelper.DeleteAttribute( "48C3DFAD-2168-4F6C-8DEC-167E49C379B7" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Use Static Height
            RockMigrationHelper.DeleteAttribute( "5D1524EA-E1F0-472B-AE24-4D30DA6672F8" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable File Manager
            RockMigrationHelper.DeleteAttribute( "FCBB90A6-965F-4237-9B0F-4384E3FFC991" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Asset Storage Providers
            RockMigrationHelper.DeleteAttribute( "7750F7BB-DC53-41C6-987B-5FD2B02674C2" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B44FFDA2-22CD-4FE8-AE07-D2CFBF638794" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "46F93E91-03AB-4C75-A8BF-420CC3952AC0" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "AEA0D9E0-F558-42F8-A0E2-2AAE93B9F2FD" );

            // Attribute for BlockType
            //   BlockType: Content Channel List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D4A0AD46-1ACD-42C5-88E2-2F30ED3668CC" );

            // Attribute for BlockType
            //   BlockType: Content Channel List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "47246985-31E2-467F-82F8-70A40ACFEBE5" );

            // Attribute for BlockType
            //   BlockType: Content Channel List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "9EF8744F-C102-4784-B5A5-A73FCEB987D4" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "DEB89A0B-A1B2-405D-9F75-FC36FCF60C2B" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "CED2BD43-E0BF-46EE-A438-A8B22CA05546" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Content Channel
            RockMigrationHelper.DeleteAttribute( "A0E5C403-EB0E-4BB5-B5FB-F8FF74BAE701" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Expire Column
            RockMigrationHelper.DeleteAttribute( "51B3E0D6-3D38-40E5-8ABA-B713653A4284" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Security Column
            RockMigrationHelper.DeleteAttribute( "3E0D56FD-E7D2-4163-9C74-5F655E5310DF" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Priority Column
            RockMigrationHelper.DeleteAttribute( "474DA16D-0B92-4A8F-9394-F0E7F41E290F" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Event Occurrences Column
            RockMigrationHelper.DeleteAttribute( "CEA81A11-8C6B-4C15-B910-C61CD317EA66" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Filters
            RockMigrationHelper.DeleteAttribute( "B4D61CA8-8E77-4423-A523-C99F17D5CBEC" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Filter Items For Current User
            RockMigrationHelper.DeleteAttribute( "B915402B-EA86-4DEC-8BB8-906DCE0175E9" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "F579201B-BCB5-4358-AD3D-AEDCBCC91E36" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "EA661287-2A10-40F6-B652-DF42FF5CBE6C" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6987D90D-CB37-4873-8BBC-5C1186B53353" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "38E12A82-1D83-4777-A80D-3E6E3B778973" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "1E67AA59-6189-425F-8C07-D930CB5A479B" );

            // Attribute for BlockType
            //   BlockType: Block List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D64A6E5E-A48F-445D-A48F-5EC092689B33" );

            // Attribute for BlockType
            //   BlockType: Block List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "6042A95E-DC10-4231-AECC-ABD5B1549767" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "93B67CB7-9967-43EC-BA29-40706A157FEA" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "7C1A7B8F-D3C0-4686-96E1-DD26C0C03A58" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: AI Disclaimer
            RockMigrationHelper.DeleteAttribute( "FAF647E9-283F-4DF6-AEF4-1925B18EB060" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Enable AI Disclaimer
            RockMigrationHelper.DeleteAttribute( "67F4CDB7-AF0D-45B2-B51C-AD5F8EBCD8EB" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Prayer
            //   Attribute: AI Disclaimer
            RockMigrationHelper.DeleteAttribute( "A1703115-B2E2-4874-B95B-CFF0313799CD" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Prayer
            //   Attribute: Enable AI Disclaimer
            RockMigrationHelper.DeleteAttribute( "A6B4F3B1-BC23-4C00-A33D-7D894E5B5F38" );

            // Attribute for BlockType
            //   BlockType: Control Gallery
            //   Category: Obsidian > Example
            //   Attribute: Show Reflection
            RockMigrationHelper.DeleteAttribute( "CF0FD9E2-FCC1-40D6-97DC-2520A6B88504" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Enrollment Error Lava Template
            RockMigrationHelper.DeleteAttribute( "72C1EC84-8BC9-434D-BDC2-DEDF1869CAF3" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Completion Lava Template
            RockMigrationHelper.DeleteAttribute( "FEC8565A-7565-4310-8BB1-5CB5B42233B6" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Confirmation Lava Template
            RockMigrationHelper.DeleteAttribute( "09002AE5-51E4-483E-9EB5-5767C6180E55" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Header Lava Template
            RockMigrationHelper.DeleteAttribute( "09A07907-D776-4F37-805E-C1A81A1CBA1A" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute( "4A34980C-32D4-4E1E-882B-2EF36D532DD4" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Banner Image
            RockMigrationHelper.DeleteAttribute( "B6260CB0-9B7F-4D6A-ACED-D52B22FCA5EE" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Description
            RockMigrationHelper.DeleteAttribute( "E19A078B-939E-4F38-AA75-C89B0E823BD9" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Page Title
            RockMigrationHelper.DeleteAttribute( "248E1752-E6FB-4C8A-B90D-D7E6EF5267D8" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute( "20CDB819-CBBF-44AC-9068-D89024FF11D1" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute( "774CABEA-2BB5-4799-80ED-42D7123B06B7" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "DA6C3170-5264-427D-AC22-8D50D2F6D2F6" );

            // Attribute for BlockType
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Attribute: Watched Note Lava Template
            RockMigrationHelper.DeleteAttribute( "0E6D23E7-D4E8-43D1-962B-4FB28A46E133" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.DeleteAttribute( "4E465E0E-84A5-407B-903A-5DE247A0F7CA" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.DeleteAttribute( "B1858793-2A64-4B6E-B976-DDC7A96F5E47" );

            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.DeleteAttribute( "16F2D574-8075-483F-8107-E66BA9377390" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Enable End Date
            RockMigrationHelper.DeleteAttribute( "F46250E9-C7C0-4908-A8DF-914DEB5EEFC4" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "671F05CC-571C-45AF-9C2F-2463035EAA6D" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "87FFA598-39F1-4413-ABD3-44670769EF7E" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.DeleteAttribute( "1003FD5F-E678-49AE-A7EA-76B5F01A1283" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Gender
            RockMigrationHelper.DeleteAttribute( "443179DA-CD2D-4119-BAD0-5A2E79C37BCA" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Address
            RockMigrationHelper.DeleteAttribute( "B28BB6D7-B297-49A1-A3BC-3530892CFD84" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Finance
            //   Attribute: Hide Account Totals Section
            RockMigrationHelper.DeleteAttribute( "8C44DD40-29BE-457E-9DEF-77BF849B266D" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Child Label
            RockMigrationHelper.DeleteAttribute( "51E3BB2D-B5D8-4821-AD7A-407380492E31" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Adult Label
            RockMigrationHelper.DeleteAttribute( "E54979CE-6004-434A-AFBF-FAC47C6E2CAE" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "D603C3C6-508A-47EC-913F-9CD5DFF4BACB" );

            // Remove Block
            //  Name: Add Prayer Request, from Page: Prayer, Site: External Website
            //  from Page: Prayer, Site: External Website
            RockMigrationHelper.DeleteBlock( "E554B5F3-442E-4E55-8CD2-49D33DD8DC24" );

            // Delete BlockType 
            //   Name: Apple TV Page List
            //   Category: TV > TV Apps
            //   Path: -
            //   EntityType: Apple Tv Page List
            RockMigrationHelper.DeleteBlockType( "A759218B-1C72-446C-8994-8559BA72941E" );

            // Delete BlockType 
            //   Name: Persisted Data View List
            //   Category: Reporting
            //   Path: -
            //   EntityType: Persisted Data View List
            RockMigrationHelper.DeleteBlockType( "1A46CC61-6110-4022-8ACE-EFE188A6AB5A" );

            // Delete BlockType 
            //   Name: Metric Value Detail
            //   Category: Reporting
            //   Path: -
            //   EntityType: Metric Value Detail
            RockMigrationHelper.DeleteBlockType( "B52E7CAE-C5CC-41CB-A5EC-1CF027074A2C" );

            // Delete BlockType 
            //   Name: Interaction Component Detail
            //   Category: Reporting
            //   Path: -
            //   EntityType: Interaction Component Detail
            RockMigrationHelper.DeleteBlockType( "BC2034D1-416B-4FB4-9FFF-E202FA666203" );

            // Delete BlockType 
            //   Name: Group Member Schedule Template List
            //   Category: Group Scheduling
            //   Path: -
            //   EntityType: Group Member Schedule Template List
            RockMigrationHelper.DeleteBlockType( "2B8A5A3D-BF9D-4319-B7E5-06757FA44759" );

            // Delete BlockType 
            //   Name: Group Type List
            //   Category: Group
            //   Path: -
            //   EntityType: Group Type List
            RockMigrationHelper.DeleteBlockType( "8885F47D-9262-48B0-B969-9BEE003370EB" );

            // Delete BlockType 
            //   Name: Group Member Schedule Template Detail
            //   Category: Group Scheduling
            //   Path: -
            //   EntityType: Group Member Schedule Template Detail
            RockMigrationHelper.DeleteBlockType( "07BCB48D-746E-4364-80F3-C5BEB9075FC6" );

            // Delete BlockType 
            //   Name: Financial Statement Template List
            //   Category: Finance
            //   Path: -
            //   EntityType: Financial Statement Template List
            RockMigrationHelper.DeleteBlockType( "2EAF9E5A-F47D-4C58-9AA4-2D340547A35F" );

            // Delete BlockType 
            //   Name: Financial Scheduled Transaction List
            //   Category: Finance
            //   Path: -
            //   EntityType: Financial Scheduled Transaction List
            RockMigrationHelper.DeleteBlockType( "2DB92EA3-F3B3-496E-A1F0-8EEBD8DC928A" );

            // Delete BlockType 
            //   Name: Bank Account List
            //   Category: Finance
            //   Path: -
            //   EntityType: Financial Person Bank Account List
            RockMigrationHelper.DeleteBlockType( "E1DCE349-2F5B-46ED-9F3D-8812AF857F69" );

            // Delete BlockType 
            //   Name: Gateway List
            //   Category: Finance
            //   Path: -
            //   EntityType: Financial Gateway List
            RockMigrationHelper.DeleteBlockType( "0F99866A-7FAB-462D-96EB-9F9534322C57" );

            // Delete BlockType 
            //   Name: Business List
            //   Category: Finance
            //   Path: -
            //   EntityType: Business List
            RockMigrationHelper.DeleteBlockType( "1E60C390-98C4-404D-AEE8-F9E3E9C69705" );

            // Delete BlockType 
            //   Name: Registration Instance Active List
            //   Category: Event
            //   Path: -
            //   EntityType: Registration Instance Active List
            RockMigrationHelper.DeleteBlockType( "5E899CCB-3C24-4F7D-9843-2F1CB00AED8F" );

            // Delete BlockType 
            //   Name: Calendar Event Item List
            //   Category: Event
            //   Path: -
            //   EntityType: Event Calendar Item List
            RockMigrationHelper.DeleteBlockType( "20C68613-F253-4D2F-A465-62AFBB01DCD6" );

            // Delete BlockType 
            //   Name: Streak Type List
            //   Category: Streaks
            //   Path: -
            //   EntityType: Streak Type List
            RockMigrationHelper.DeleteBlockType( "6F0F3AD2-4989-4F50-B394-0DE3C7AF35AD" );

            // Delete BlockType 
            //   Name: Step Type List
            //   Category: Steps
            //   Path: -
            //   EntityType: Step Type List
            RockMigrationHelper.DeleteBlockType( "6A7C7C71-4760-4E6C-9D6F-6926C81CAF8F" );

            // Delete BlockType 
            //   Name: Step Type Detail
            //   Category: Steps
            //   Path: -
            //   EntityType: Step Type Detail
            RockMigrationHelper.DeleteBlockType( "487ECB63-BDF3-41A1-BE67-C5FAAB5F27C1" );

            // Delete BlockType 
            //   Name: Step Program List
            //   Category: Steps
            //   Path: -
            //   EntityType: Step Program List
            RockMigrationHelper.DeleteBlockType( "5284B259-A9EC-431C-B949-661780BFCD68" );

            // Delete BlockType 
            //   Name: Step Participant List
            //   Category: Steps
            //   Path: -
            //   EntityType: Step Participant List
            RockMigrationHelper.DeleteBlockType( "272B2236-FCCC-49B4-B914-20893F5E746D" );

            // Delete BlockType 
            //   Name: Connection Opportunity List
            //   Category: Engagement
            //   Path: -
            //   EntityType: Connection Opportunity List
            RockMigrationHelper.DeleteBlockType( "8EB82E1E-C0BD-4591-9D7A-F120A871FEC3" );

            // Delete BlockType 
            //   Name: Achievement Type Detail
            //   Category: Achievements
            //   Path: -
            //   EntityType: Achievement Type Detail
            RockMigrationHelper.DeleteBlockType( "EDDFCAFF-70AA-4791-B051-6567B37518C4" );

            // Delete BlockType 
            //   Name: Achievement Attempt List
            //   Category: Achievements
            //   Path: -
            //   EntityType: Achievement Attempt List
            RockMigrationHelper.DeleteBlockType( "B294C1B9-8368-422C-8054-9672C7F41477" );

            // Delete BlockType 
            //   Name: Nameless Person List
            //   Category: CRM
            //   Path: -
            //   EntityType: Nameless Person List
            RockMigrationHelper.DeleteBlockType( "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C" );

            // Delete BlockType 
            //   Name: Disc
            //   Category: CRM
            //   Path: -
            //   EntityType: Disc
            RockMigrationHelper.DeleteBlockType( "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E" );

            // Delete BlockType 
            //   Name: Badge List
            //   Category: CRM
            //   Path: -
            //   EntityType: Badge List
            RockMigrationHelper.DeleteBlockType( "559978D5-A392-4BD1-8E04-055C2833F347" );

            // Delete BlockType 
            //   Name: Assessment Type List
            //   Category: CRM
            //   Path: -
            //   EntityType: Assessment Type List
            RockMigrationHelper.DeleteBlockType( "1FDE6D4F-390A-4FF6-AD42-668EC8CC62C4" );

            // Delete BlockType 
            //   Name: Tag List
            //   Category: Core
            //   Path: -
            //   EntityType: Tag List
            RockMigrationHelper.DeleteBlockType( "0ACF764F-5F60-4985-9D10-029CB042DA0D" );

            // Delete BlockType 
            //   Name: Signature Document Template List
            //   Category: Core
            //   Path: -
            //   EntityType: Signature Document Template List
            RockMigrationHelper.DeleteBlockType( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7" );

            // Delete BlockType 
            //   Name: Signal Type List
            //   Category: Core
            //   Path: -
            //   EntityType: Signal Type List
            RockMigrationHelper.DeleteBlockType( "770D3039-3F07-4D6F-A64E-C164ACCE93E1" );

            // Delete BlockType 
            //   Name: Scheduled Job History
            //   Category: Core
            //   Path: -
            //   EntityType: Scheduled Job History List
            RockMigrationHelper.DeleteBlockType( "2306068D-3551-4C10-8DB8-133C030FA4FA" );

            // Delete BlockType 
            //   Name: Person Suggestion List
            //   Category: Follow
            //   Path: -
            //   EntityType: Person Suggestion List
            RockMigrationHelper.DeleteBlockType( "D29619D6-2FFE-4EF7-ADAF-14DB588944EA" );

            // Delete BlockType 
            //   Name: Person Following List
            //   Category: Follow
            //   Path: -
            //   EntityType: Person Following List
            RockMigrationHelper.DeleteBlockType( "18FA879F-1466-413B-8623-834D728F677B" );

            // Delete BlockType 
            //   Name: Device List
            //   Category: Core
            //   Path: -
            //   EntityType: Device List
            RockMigrationHelper.DeleteBlockType( "7686A42F-A2C4-4C15-9331-8B364F24BD0F" );

            // Delete BlockType 
            //   Name: Defined Type List
            //   Category: Core
            //   Path: -
            //   EntityType: Defined Type List
            RockMigrationHelper.DeleteBlockType( "7FAF32D3-C577-462A-BC0B-D34DE3316A5B" );

            // Delete BlockType 
            //   Name: Binary File Type List
            //   Category: Core
            //   Path: -
            //   EntityType: Binary File Type List
            RockMigrationHelper.DeleteBlockType( "000CA534-6164-485E-B405-BA0FA6AE92F9" );

            // Delete BlockType 
            //   Name: Binary File Type Detail
            //   Category: Core
            //   Path: -
            //   EntityType: Binary File Type Detail
            RockMigrationHelper.DeleteBlockType( "DABF690B-BE17-4821-A13E-44C7C8D587CD" );

            // Delete BlockType 
            //   Name: Binary File List
            //   Category: Core
            //   Path: -
            //   EntityType: Binary File List
            RockMigrationHelper.DeleteBlockType( "69A45481-467B-47EF-9838-4462E5615216" );

            // Delete BlockType 
            //   Name: Attribute Matrix Template List
            //   Category: Core
            //   Path: -
            //   EntityType: Attribute Matrix Template List
            RockMigrationHelper.DeleteBlockType( "47F619C2-F66D-45EC-ADBB-22CA23B4F3AD" );

            // Delete BlockType 
            //   Name: Asset Storage Provider List
            //   Category: Core
            //   Path: -
            //   EntityType: Asset Storage Provider List
            RockMigrationHelper.DeleteBlockType( "2663E57E-ED73-49FE-BA16-69B4B829C488" );

            // Delete BlockType 
            //   Name: Sms Pipeline List
            //   Category: Communication
            //   Path: -
            //   EntityType: Sms Pipeline List
            RockMigrationHelper.DeleteBlockType( "DA937CFD-F20E-4619-8CB8-D1A2738D2FF2" );

            // Delete BlockType 
            //   Name: Communication List
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication List
            RockMigrationHelper.DeleteBlockType( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB" );

            // Delete BlockType 
            //   Name: Communication Entry
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Entry
            RockMigrationHelper.DeleteBlockType( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" );

            // Delete BlockType 
            //   Name: Page Short Link List
            //   Category: CMS
            //   Path: -
            //   EntityType: Page Short Link List
            RockMigrationHelper.DeleteBlockType( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE" );

            // Delete BlockType 
            //   Name: Page Short Link Click List
            //   Category: CMS
            //   Path: -
            //   EntityType: Page Short Link Click List
            RockMigrationHelper.DeleteBlockType( "E44CAC85-346F-41A4-884B-A6FB5FC64DE1" );

            // Delete BlockType 
            //   Name: Page List
            //   Category: CMS
            //   Path: -
            //   EntityType: Page List
            RockMigrationHelper.DeleteBlockType( "39B02B93-B1AF-4F9B-A535-33F470D91106" );

            // Delete BlockType 
            //   Name: Media Folder List
            //   Category: CMS
            //   Path: -
            //   EntityType: Media Folder List
            RockMigrationHelper.DeleteBlockType( "75133C37-547F-47FA-991C-6D957B2EA92D" );

            // Delete BlockType 
            //   Name: Media Account List
            //   Category: CMS
            //   Path: -
            //   EntityType: Media Account List
            RockMigrationHelper.DeleteBlockType( "BAF39B55-C4E5-4EB4-A834-B4F820DD2F42" );

            // Delete BlockType 
            //   Name: Log Settings
            //   Category: Administration
            //   Path: -
            //   EntityType: Log Settings
            RockMigrationHelper.DeleteBlockType( "FA01630C-18FB-472F-8BF1-013AF257DE3F" );

            // Delete BlockType 
            //   Name: File Asset Manager
            //   Category: CMS
            //   Path: -
            //   EntityType: File Asset Manager
            RockMigrationHelper.DeleteBlockType( "535500A7-967F-4DA3-8FCA-CB844203CB3D" );

            // Delete BlockType 
            //   Name: Content Channel Type List
            //   Category: CMS
            //   Path: -
            //   EntityType: Content Channel Type List
            RockMigrationHelper.DeleteBlockType( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B" );

            // Delete BlockType 
            //   Name: Content Channel List
            //   Category: CMS
            //   Path: -
            //   EntityType: Content Channel List
            RockMigrationHelper.DeleteBlockType( "F381936B-0D8C-43F0-8DA5-401383E40883" );

            // Delete BlockType 
            //   Name: Content Item List
            //   Category: CMS
            //   Path: -
            //   EntityType: Content Channel Item List
            RockMigrationHelper.DeleteBlockType( "93DC73C4-545D-40B9-BFEA-1CEC04C07EB1" );

            // Delete BlockType 
            //   Name: Content Channel Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Content Channel Detail
            RockMigrationHelper.DeleteBlockType( "2BAD2AB9-86AD-480E-BF38-C54F2C5C03A8" );

            // Delete BlockType 
            //   Name: Block Type List
            //   Category: CMS
            //   Path: -
            //   EntityType: Block Type List
            RockMigrationHelper.DeleteBlockType( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416" );

            // Delete BlockType 
            //   Name: Block List
            //   Category: CMS
            //   Path: -
            //   EntityType: Block List
            RockMigrationHelper.DeleteBlockType( "EA8BE085-D420-4D1B-A538-2C0D4D116E0A" );
        }
    }
}
