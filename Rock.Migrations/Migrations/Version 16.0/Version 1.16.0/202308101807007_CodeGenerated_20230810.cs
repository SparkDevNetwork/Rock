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
    public partial class CodeGenerated_20230810 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Bus.QueueDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Bus.QueueDetail", "Queue Detail", "Rock.Blocks.Bus.QueueDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "6C61FFC3-B37C-4D90-8B99-CC3C53150EE3" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Cms.BlockTypeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.BlockTypeDetail", "Block Type Detail", "Rock.Blocks.Cms.BlockTypeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "81B9BFD5-621D-4E82-84F6-38CAE1810332" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Cms.LavaShortcodeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LavaShortcodeDetail", "Lava Shortcode Detail", "Rock.Blocks.Cms.LavaShortcodeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "76DE9139-63C8-4E38-ADFE-F38C1EF1021A" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Cms.LayoutDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LayoutDetail", "Layout Detail", "Rock.Blocks.Cms.LayoutDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "B85C080A-F645-430A-B0D4-8EEE689F4265" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Cms.PageRouteDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageRouteDetail", "Page Route Detail", "Rock.Blocks.Cms.PageRouteDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "3ED3C7C7-70B8-416B-819C-353011204F51" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.CMS.PageShortLinkDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CMS.PageShortLinkDetail", "Page Short Link Detail", "Rock.Blocks.CMS.PageShortLinkDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "AD614123-C7CA-40EE-B5D5-64D0D1C91378" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Cms.SiteDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.SiteDetail", "Site Detail", "Rock.Blocks.Cms.SiteDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "88CE8A0B-35B6-4427-817F-2FDF485D0241" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Cms.SiteList              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.SiteList", "Site List", "Rock.Blocks.Cms.SiteList, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "12A8C8AE-7BBE-41D2-9448-8D7EAE298099" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Communication.EmailPreferenceEntry              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.EmailPreferenceEntry", "Email Preference Entry", "Rock.Blocks.Communication.EmailPreferenceEntry, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "28265232-B692-4099-9533-4D7646BDA2C1" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.AssetStorageProviderDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AssetStorageProviderDetail", "Asset Storage Provider Detail", "Rock.Blocks.Core.AssetStorageProviderDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "D18E8437-9452-441A-BE17-D03F793F6B47" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.CampusList              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.CampusList", "Campus List", "Rock.Blocks.Core.CampusList, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "A21A13C9-9429-4BA1-85B2-D2FA4E3D5081" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.CategoryDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.CategoryDetail", "Category Detail", "Rock.Blocks.Core.CategoryDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "2889352C-52BA-45F6-8EE1-9AFA61211582" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.FollowingEventTypeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.FollowingEventTypeDetail", "Following Event Type Detail", "Rock.Blocks.Core.FollowingEventTypeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "D9C32C98-434F-4975-8EBC-E64C628F02DB" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.LocationDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.LocationDetail", "Location Detail", "Rock.Blocks.Core.LocationDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "862067B0-8764-452E-9B4F-DC3E0CF5F876" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.Notes              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.Notes", "Notes", "Rock.Blocks.Core.Notes, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "33566B2B-D74F-4148-B962-1897D418C6DF" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.NoteTypeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.NoteTypeDetail", "Note Type Detail", "Rock.Blocks.Core.NoteTypeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "A664C469-985C-4747-80CD-E07501D13F43" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.NotificationMessageList              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.NotificationMessageList", "Notification Message List", "Rock.Blocks.Core.NotificationMessageList, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "5F6BB4E3-94B2-41FA-94D5-AF49A97B21CB" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.ScheduleDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduleDetail", "Schedule Detail", "Rock.Blocks.Core.ScheduleDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "CE4859A1-3E47-442F-8442-2671A89A5656" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.ServiceJobDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ServiceJobDetail", "Service Job Detail", "Rock.Blocks.Core.ServiceJobDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "B50B6B68-D327-4A73-B2A8-57EF9E151182" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentTemplateDetail", "Signature Document Template Detail", "Rock.Blocks.Core.SignatureDocumentTemplateDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "525B6687-964E-4051-94A5-4B20D4575041" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.SuggestionDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SuggestionDetail", "Suggestion Detail", "Rock.Blocks.Core.SuggestionDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "64E05A6C-90AD-45A8-8CA2-3E4FE29CBFDB" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Core.TagDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.TagDetail", "Tag Detail", "Rock.Blocks.Core.TagDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "919345D6-6E20-4501-B956-EBCB35D0B16E" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Crm.AssessmentList              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.AssessmentList", "Assessment List", "Rock.Blocks.Crm.AssessmentList, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "3509289A-2E12-443F-87BF-2179BC36FECD" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Crm.BadgeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.BadgeDetail", "Badge Detail", "Rock.Blocks.Crm.BadgeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "5B57BD74-416D-4FD0-A36B-C74955F4C691" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Crm.DocumentTypeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.DocumentTypeDetail", "Document Type Detail", "Rock.Blocks.Crm.DocumentTypeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "EE4F6524-C311-4F73-BA4F-18152148297E" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Crm.PhotoOptOutDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PhotoOptOutDetail", "Photo Opt Out Detail", "Rock.Blocks.Crm.PhotoOptOutDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "63F1D46A-EB78-4B0F-B398-099A83E058E8" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementAttemptDetail", "Achievement Attempt Detail", "Rock.Blocks.Engagement.AchievementAttemptDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "A80564D5-701B-4F3A-8BA1-20BAA2304DA6" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Engagement.StreakDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakDetail", "Streak Detail", "Rock.Blocks.Engagement.StreakDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "867ABCE8-47A9-46FA-8A35-47EBBC60C4FE" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Engagement.StreakTypeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakTypeDetail", "Streak Type Detail", "Rock.Blocks.Engagement.StreakTypeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "8A8C5BEA-6293-4AC0-8C2E-D89F541043AA" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Event.EventCalendarDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.EventCalendarDetail", "Event Calendar Detail", "Rock.Blocks.Event.EventCalendarDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "B033F86D-C166-4642-B999-0677F2CA2DAF" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Event.EventItemDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.EventItemDetail", "Event Item Detail", "Rock.Blocks.Event.EventItemDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "E09743B1-CC81-4D00-B3E1-5825A178A473" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Event.RegistrationListLava              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationListLava", "Registration List Lava", "Rock.Blocks.Event.RegistrationListLava, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "52C84E33-FE5F-4023-8365-A5FE1F71C93B" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Finance.BusinessDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BusinessDetail", "Business Detail", "Rock.Blocks.Finance.BusinessDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "D54D7307-40F2-4BEB-819D-8112DFBFBB12" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Finance.FinancialBatchDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialBatchDetail", "Financial Batch Detail", "Rock.Blocks.Finance.FinancialBatchDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "B5976E12-A3E4-4FAF-95B5-3D54F25405DA" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPledgeDetail", "Financial Pledge Detail", "Rock.Blocks.Finance.FinancialPledgeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "C7862196-7312-4370-B2D7-05B631429071" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeEntry              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPledgeEntry", "Financial Pledge Entry", "Rock.Blocks.Finance.FinancialPledgeEntry, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "FAD28407-5128-4DDB-9C1C-A0C2233F3E73" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Groups.GroupRegistration              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Groups.GroupRegistration", "Group Registration", "Rock.Blocks.Groups.GroupRegistration, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "BBCE9C47-B14D-4122-86A0-08441DEE2759" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Prayer.PrayerRequestDetail", "Prayer Request Detail", "Rock.Blocks.Prayer.PrayerRequestDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "D1E21128-C831-4535-B8DF-0EC928DCBBA4" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestEntry              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Prayer.PrayerRequestEntry", "Prayer Request Entry", "Rock.Blocks.Prayer.PrayerRequestEntry, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "EC1DB1C6-17C2-43A0-8968-10A1DFF7AA03" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.MergeTemplateDetail", "Merge Template Detail", "Rock.Blocks.Reporting.MergeTemplateDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "3338D32F-20E0-4F6F-9ABC-DD21558649C8" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Security.ConfirmAccount              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.ConfirmAccount", "Confirm Account", "Rock.Blocks.Security.ConfirmAccount, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "49098480-A041-4404-964C-10EFF41B7DCA" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Security.ForgotUserName              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.ForgotUserName", "Forgot User Name", "Rock.Blocks.Security.ForgotUserName, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "5BBEE600-781E-4480-8144-36F8D01C7F09" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Security.RestKeyDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.RestKeyDetail", "Rest Key Detail", "Rock.Blocks.Security.RestKeyDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "AED330CA-40A4-407A-B2DC-A0C1310FDC39" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Tv.AppleTvAppDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.AppleTvAppDetail", "Apple Tv App Detail", "Rock.Blocks.Tv.AppleTvAppDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "E66D1530-8E39-4C00-8FA4-078482E56080" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Tv.AppleTvPageDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.AppleTvPageDetail", "Apple Tv Page Detail", "Rock.Blocks.Tv.AppleTvPageDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "D8419B3C-EDA1-46FC-9810-B1D81FB37CB3" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.WebFarm.WebFarmNodeDetail", "Web Farm Node Detail", "Rock.Blocks.WebFarm.WebFarmNodeDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "8471BF7F-6D0D-411B-899F-CD853F496BB9" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.WebFarm.WebFarmSettings              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.WebFarm.WebFarmSettings", "Web Farm Settings", "Rock.Blocks.WebFarm.WebFarmSettings, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "3AA0CC1E-3C16-4AB2-BF03-9EA2FD3239E9" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Workflow.WorkflowTriggerDetail              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Workflow.WorkflowTriggerDetail", "Workflow Trigger Detail", "Rock.Blocks.Workflow.WorkflowTriggerDetail, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "3F0D9D0F-A739-4C92-94A7-70B2BBE03F46" );

            // Add/Update Obsidian Block Type              
            //   Name:Queue Detail              
            //   Category:Bus              
            //   EntityType:Rock.Blocks.Bus.QueueDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Queue Detail", "Displays the details of a queue detail.", "Rock.Blocks.Bus.QueueDetail", "Bus", "DB19D24E-B0C8-4686-8582-7B84DAE33EE8" );

            // Add/Update Obsidian Block Type              
            //   Name:Block Type Detail              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.Cms.BlockTypeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Block Type Detail", "Displays the details of a particular block type.", "Rock.Blocks.Cms.BlockTypeDetail", "CMS", "6C329001-9C04-4090-BED0-12E3F6B88FB6" );

            // Add/Update Obsidian Block Type              
            //   Name:Lava Shortcode Detail              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.Cms.LavaShortcodeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Shortcode Detail", "Displays the details of a particular lava shortcode.", "Rock.Blocks.Cms.LavaShortcodeDetail", "CMS", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" );

            // Add/Update Obsidian Block Type              
            //   Name:Layout Detail              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.Cms.LayoutDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Layout Detail", "Displays the details of a particular layout.", "Rock.Blocks.Cms.LayoutDetail", "CMS", "64C3B64A-CDB3-4E5F-BC54-0E3D50AAC564" );

            // Add/Update Obsidian Block Type              
            //   Name:Route Detail              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.Cms.PageRouteDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Route Detail", "Displays the details of a particular page route.", "Rock.Blocks.Cms.PageRouteDetail", "CMS", "E4302549-0BB8-4DDE-9B9A-70F0B665E76F" );

            // Add/Update Obsidian Block Type              
            //   Name:Page Short Link Detail              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.CMS.PageShortLinkDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link Detail", "Displays the details of a particular page short link.", "Rock.Blocks.CMS.PageShortLinkDetail", "CMS", "72EDDF3D-625E-40A9-A68B-76236E77A3F3" );

            // Add/Update Obsidian Block Type              
            //   Name:Site Detail              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.Cms.SiteDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Site Detail", "Displays the details of a particular site.", "Rock.Blocks.Cms.SiteDetail", "CMS", "3E935E45-4796-4389-AB1C-98D2403FAEDF" );

            // Add/Update Obsidian Block Type              
            //   Name:Site List              
            //   Category:CMS              
            //   EntityType:Rock.Blocks.Cms.SiteList              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Site List", "Displays a list of sites.", "Rock.Blocks.Cms.SiteList", "CMS", "D27A9C0D-E118-4172-8F8E-368C973F5486" );

            // Add/Update Obsidian Block Type              
            //   Name:Email Preference Entry              
            //   Category:Communication              
            //   EntityType:Rock.Blocks.Communication.EmailPreferenceEntry              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Email Preference Entry", "Allows user to set their email preference or unsubscribe from a communication list.", "Rock.Blocks.Communication.EmailPreferenceEntry", "Communication", "476FBA19-005C-4FF4-996B-CA1B165E5BC8" );

            // Add/Update Obsidian Block Type              
            //   Name:Asset Storage Provider Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.AssetStorageProviderDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Asset Storage Provider Detail", "Displays the details of a particular asset storage provider.", "Rock.Blocks.Core.AssetStorageProviderDetail", "Core", "4B50E08A-A805-4213-A5AF-BCA570FCB528" );

            // Add/Update Obsidian Block Type              
            //   Name:Campus List              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.CampusList              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Campus List", "Displays a list of campuses.", "Rock.Blocks.Core.CampusList", "Core", "52DF00E5-BC19-43F2-8533-A386DB53C74F" );

            // Add/Update Obsidian Block Type              
            //   Name:Category Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.CategoryDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Category Detail", "Displays the details of a particular category.", "Rock.Blocks.Core.CategoryDetail", "Core", "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE" );

            // Add/Update Obsidian Block Type              
            //   Name:Following Event Type Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.FollowingEventTypeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Following Event Type Detail", "Displays the details of a particular following event type.", "Rock.Blocks.Core.FollowingEventTypeDetail", "Core", "78F27537-C05F-44E0-AF84-2329C8B5D71D" );

            // Add/Update Obsidian Block Type              
            //   Name:Location Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.LocationDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Location Detail", "Displays the details of a particular location.", "Rock.Blocks.Core.LocationDetail", "Core", "D0203B97-5856-437E-8700-8846309F8EED" );

            // Add/Update Obsidian Block Type              
            //   Name:Notes              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.Notes              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Notes", "Context aware block for adding notes to an entity.", "Rock.Blocks.Core.Notes", "Core", "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8" );

            // Add/Update Obsidian Block Type              
            //   Name:Note Type Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.NoteTypeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Note Type Detail", "Displays the details of a particular note type.", "Rock.Blocks.Core.NoteTypeDetail", "Core", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" );

            // Add/Update Obsidian Block Type              
            //   Name:Notification Messages              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.NotificationMessageList              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Notification Messages", "Displays notification messages for the current individual.", "Rock.Blocks.Core.NotificationMessageList", "Core", "2E4292B9-CD68-41E9-86BD-356ACCD54F36" );

            // Add/Update Obsidian Block Type              
            //   Name:Schedule Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.ScheduleDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Schedule Detail", "Displays the details of a particular schedule.", "Rock.Blocks.Core.ScheduleDetail", "Core", "7C10240A-7EE5-4720-AAC9-5C162E9F5AAC" );

            // Add/Update Obsidian Block Type              
            //   Name:Scheduled Job Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.ServiceJobDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Job Detail", "Displays the details of a particular service job.", "Rock.Blocks.Core.ServiceJobDetail", "Core", "762F09EA-0A11-4BC7-9A68-13F0E44217C1" );

            // Add/Update Obsidian Block Type              
            //   Name:Signature Document Template Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document Template Detail", "Displays the details of a particular signature document template.", "Rock.Blocks.Core.SignatureDocumentTemplateDetail", "Core", "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5" );

            // Add/Update Obsidian Block Type              
            //   Name:Suggestion Detail              
            //   Category:Follow              
            //   EntityType:Rock.Blocks.Core.SuggestionDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Suggestion Detail", "Block for editing the following suggestion types.", "Rock.Blocks.Core.SuggestionDetail", "Follow", "E18AB976-6665-48A5-B418-8FAC8F374135" );

            // Add/Update Obsidian Block Type              
            //   Name:Tag Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Core.TagDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Tag Detail", "Displays the details of a particular tag.", "Rock.Blocks.Core.TagDetail", "Core", "B150E767-E964-460C-9ED1-B293474C5F5D" );

            // Add/Update Obsidian Block Type              
            //   Name:Assessment List              
            //   Category:CRM              
            //   EntityType:Rock.Blocks.Crm.AssessmentList              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Assessment List", "Displays the details of a particular assessment.", "Rock.Blocks.Crm.AssessmentList", "CRM", "5ECCA4FB-F8FB-49DB-96B7-082BB4E4C170" );

            // Add/Update Obsidian Block Type              
            //   Name:Badge Detail              
            //   Category:CRM              
            //   EntityType:Rock.Blocks.Crm.BadgeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Badge Detail", "Displays the details of a particular badge.", "Rock.Blocks.Crm.BadgeDetail", "CRM", "5BD4CD27-C1C1-4E12-8756-9C93E4EDB28E" );

            // Add/Update Obsidian Block Type              
            //   Name:Document Type Detail              
            //   Category:CRM              
            //   EntityType:Rock.Blocks.Crm.DocumentTypeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Document Type Detail", "Displays the details of a particular document type.", "Rock.Blocks.Crm.DocumentTypeDetail", "CRM", "FD3EB724-1AFA-4507-8850-C3AEE170C83B" );

            // Add/Update Obsidian Block Type              
            //   Name:Photo Opt-Out              
            //   Category:CRM              
            //   EntityType:Rock.Blocks.Crm.PhotoOptOutDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Photo Opt-Out", "Allows a person to opt-out of future photo requests.", "Rock.Blocks.Crm.PhotoOptOutDetail", "CRM", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" );

            // Add/Update Obsidian Block Type              
            //   Name:Achievement Attempt Detail              
            //   Category:Engagement              
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Attempt Detail", "Displays the details of a particular achievement attempt.", "Rock.Blocks.Engagement.AchievementAttemptDetail", "Engagement", "FBE75C18-7F71-4D23-A546-7A17CF944BA6" );

            // Add/Update Obsidian Block Type              
            //   Name:Streak Detail              
            //   Category:Engagement              
            //   EntityType:Rock.Blocks.Engagement.StreakDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak Detail", "Displays the details of a particular streak.", "Rock.Blocks.Engagement.StreakDetail", "Engagement", "1C98107F-DFBF-44BD-A860-0C9DF2E6C495" );

            // Add/Update Obsidian Block Type              
            //   Name:Streak Type Detail              
            //   Category:Engagement              
            //   EntityType:Rock.Blocks.Engagement.StreakTypeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak Type Detail", "Displays the details of a particular streak type.", "Rock.Blocks.Engagement.StreakTypeDetail", "Engagement", "A83A1F49-10A6-4362-ACC3-8027224A2120" );

            // Add/Update Obsidian Block Type              
            //   Name:Calendar Detail              
            //   Category:Event              
            //   EntityType:Rock.Blocks.Event.EventCalendarDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Calendar Detail", "Displays the details of the given Event Calendar.", "Rock.Blocks.Event.EventCalendarDetail", "Event", "2DC334AC-C2C2-4031-9E1C-6A5B6FBCAE9C" );

            // Add/Update Obsidian Block Type              
            //   Name:Calendar Event Item Detail              
            //   Category:Event              
            //   EntityType:Rock.Blocks.Event.EventItemDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Calendar Event Item Detail", "Displays the details of the given calendar event item.", "Rock.Blocks.Event.EventItemDetail", "Event", "63D0DFB8-1F9E-464A-A603-2252034BC6AF" );

            // Add/Update Obsidian Block Type              
            //   Name:Registration List Lava              
            //   Category:Event              
            //   EntityType:Rock.Blocks.Event.RegistrationListLava              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Registration List Lava", "List recent registrations using a Lava template.", "Rock.Blocks.Event.RegistrationListLava", "Event", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" );

            // Add/Update Obsidian Block Type              
            //   Name:Business Detail              
            //   Category:Finance              
            //   EntityType:Rock.Blocks.Finance.BusinessDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Business Detail", "Displays the details of the given business.", "Rock.Blocks.Finance.BusinessDetail", "Finance", "729E1953-4CFF-46F0-8715-9D7892BADB4E" );

            // Add/Update Obsidian Block Type              
            //   Name:Financial Batch Detail              
            //   Category:Finance              
            //   EntityType:Rock.Blocks.Finance.FinancialBatchDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Batch Detail", "Displays the details of a particular financial batch.", "Rock.Blocks.Finance.FinancialBatchDetail", "Finance", "6BE58680-8795-46A0-8BFA-434A01FEB4C8" );

            // Add/Update Obsidian Block Type              
            //   Name:Pledge Detail              
            //   Category:Finance              
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Pledge Detail", "Allows the details of a given pledge to be edited.", "Rock.Blocks.Finance.FinancialPledgeDetail", "Finance", "2A5AE27F-F536-4ACC-B5EB-9263C4B92EF5" );

            // Add/Update Obsidian Block Type              
            //   Name:Pledge Entry              
            //   Category:Finance              
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeEntry              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Pledge Entry", "Allows a website visitor to create pledge for the configured accounts, start and end date. This block also creates a new person record if a matching person could not be found.", "Rock.Blocks.Finance.FinancialPledgeEntry", "Finance", "0455ECBD-D54D-4485-BF4D-F469048AE10F" );

            // Add/Update Obsidian Block Type              
            //   Name:Group Registration              
            //   Category:Group              
            //   EntityType:Rock.Blocks.Groups.GroupRegistration              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Registration", "Allows a person to register for a group.", "Rock.Blocks.Groups.GroupRegistration", "Group", "5E000376-FF90-4962-A053-EC1473DA5C45" );

            // Add/Update Obsidian Block Type              
            //   Name:Prayer Request Detail              
            //   Category:Prayer              
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Prayer Request Detail", "Displays the details of a particular prayer request.", "Rock.Blocks.Prayer.PrayerRequestDetail", "Prayer", "E120F06F-6DB7-464A-A797-C3C90B92EF40" );

            // Add/Update Obsidian Block Type              
            //   Name:Prayer Request Entry              
            //   Category:Prayer              
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestEntry              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Prayer Request Entry", "Allows prayer requests to be added via visitors on the website.", "Rock.Blocks.Prayer.PrayerRequestEntry", "Prayer", "5AA30F53-1B7D-4CA9-89B6-C10592968870" );

            // Add/Update Obsidian Block Type              
            //   Name:Merge Template Detail              
            //   Category:Core              
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Merge Template Detail", "Displays the details of a particular merge template.", "Rock.Blocks.Reporting.MergeTemplateDetail", "Core", "B852DB84-0CDF-4862-9EC7-CDBBBD5BB77A" );

            // Add/Update Obsidian Block Type              
            //   Name:Confirm Account              
            //   Category:Security              
            //   EntityType:Rock.Blocks.Security.ConfirmAccount              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Confirm Account", "Block for user to confirm a newly created login account, usually from an email that was sent to them.", "Rock.Blocks.Security.ConfirmAccount", "Security", "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A" );

            // Add/Update Obsidian Block Type              
            //   Name:Forgot Username              
            //   Category:Security              
            //   EntityType:Rock.Blocks.Security.ForgotUserName              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Forgot Username", "Allows a user to get their forgotten username information emailed to them.", "Rock.Blocks.Security.ForgotUserName", "Security", "16CD7562-BE31-4823-9C4D-F365AB0AA5C4" );

            // Add/Update Obsidian Block Type              
            //   Name:Rest Key Detail              
            //   Category:Security              
            //   EntityType:Rock.Blocks.Security.RestKeyDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Key Detail", "Displays the details of a particular user login.", "Rock.Blocks.Security.RestKeyDetail", "Security", "28A34F1C-80F4-496F-A598-180974ADEE61" );

            // Add/Update Obsidian Block Type              
            //   Name:Apple TV Application Detail              
            //   Category:TV > TV Apps              
            //   EntityType:Rock.Blocks.Tv.AppleTvAppDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Application Detail", "Allows a person to edit an Apple TV application..", "Rock.Blocks.Tv.AppleTvAppDetail", "TV > TV Apps", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" );

            // Add/Update Obsidian Block Type              
            //   Name:Apple TV Page Detail              
            //   Category:TV > TV Apps              
            //   EntityType:Rock.Blocks.Tv.AppleTvPageDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Page Detail", "Allows a person to edit an Apple TV page.", "Rock.Blocks.Tv.AppleTvPageDetail", "TV > TV Apps", "ADBF3377-A491-4016-9375-346496A25FB4" );

            // Add/Update Obsidian Block Type              
            //   Name:Web Farm Node Detail              
            //   Category:WebFarm              
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Web Farm Node Detail", "Displays the details of a particular web farm node.", "Rock.Blocks.WebFarm.WebFarmNodeDetail", "WebFarm", "6BBA1FC0-AC56-4E58-9E99-EB20DA7AA415" );

            // Add/Update Obsidian Block Type              
            //   Name:Web Farm Settings              
            //   Category:WebFarm              
            //   EntityType:Rock.Blocks.WebFarm.WebFarmSettings              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Web Farm Settings", "Displays the details of the Web Farm.", "Rock.Blocks.WebFarm.WebFarmSettings", "WebFarm", "D9510038-0547-45F3-9ECA-C2CA85E64416" );

            // Add/Update Obsidian Block Type              
            //   Name:Workflow Trigger Detail              
            //   Category:WorkFlow              
            //   EntityType:Rock.Blocks.Workflow.WorkflowTriggerDetail              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Workflow Trigger Detail", "Displays the details of a particular workflow trigger.", "Rock.Blocks.Workflow.WorkflowTriggerDetail", "WorkFlow", "A8062FE5-5BCD-48AC-8C37-2124462656A7" );

            // Add Block               
            //  Block Name: Membership              
            //  Page Name: Extended Attributes V1              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "B3197A5B-1B81-4E3F-BB65-955CCC8056BF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "064125E2-0069-4899-9B8C-300F736878A1" );

            // Attribute for BlockType              
            //   BlockType: Page Short Link Detail              
            //   Category: CMS              
            //   Attribute: Minimum Token Length              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72EDDF3D-625E-40A9-A68B-76236E77A3F3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Token Length", "MinimumTokenLength", "Minimum Token Length", @"The minimum number of characters for the token.", 0, @"7", "606DB351-90C7-4936-B7DC-8D1306672503" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the site details.", 0, @"", "B8DA40CC-D2BD-4E31-8B9F-A68536A2408A" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: Site Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Site Type", "SiteType", "Site Type", @"Includes Items with the following Type.", 1, @"", "4A70389B-5D70-475A-B9EA-A67B482662DC" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: Show Delete Column              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Delete Column", "ShowDeleteColumn", "Show Delete Column", @"Determines if the delete column should be shown.", 2, @"False", "820FDB69-D1BB-4F98-9A2C-434F55C5211B" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6B9B6F69-CEA2-4877-BCA0-FAA2F6CE4C31" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D27A9C0D-E118-4172-8F8E-368C973F5486", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1B061454-9E69-4CD5-BCEF-A6627FF73F74" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Unsubscribe from Lists Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribe from Lists Text", "UnsubscribefromListsText", "Unsubscribe from Lists Text", @"Text to display for the 'Unsubscribe me from the following lists:' option.", 0, @"Only unsubscribe me from the following lists", "EE4318BA-9AAE-4BFB-A60C-956708D4A882" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Update Email Address Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Update Email Address Text", "UpdateEmailAddressText", "Update Email Address Text", @"Text to display for the 'Update Email Address' option.", 1, @"Update my email address.", "5158AB09-F748-40D4-97CD-12F9265E34EE" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Emails Allowed Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Emails Allowed Text", "EmailsAllowedText", "Emails Allowed Text", @"Text to display for the 'Emails Allowed' option.", 2, @"I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, and wish to receive all emails.", "003C4E88-4916-4848-BE8C-39376B7AB047" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: No Mass Emails Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "No Mass Emails Text", "NoMassEmailsText", "No Mass Emails Text", @"Text to display for the 'No Mass Emails' option.", 3, @"I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not wish to receive mass emails (personal emails are fine).", "5032BCF9-FC48-45D8-945A-77B38B41A4A7" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: No Emails Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "No Emails Text", "NoEmailsText", "No Emails Text", @"Text to display for the 'No Emails' option.", 4, @"I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not want to receive emails of ANY kind.", "C613D25E-C3BF-4AEE-999B-90BD7F8F5F7D" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Not Involved Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Not Involved Text", "NotInvolvedText", "Not Involved Text", @"Text to display for the 'Not Involved' option.", 5, @" I am no longer involved with {{ 'Global' | Attribute:'OrganizationName' }}.", "B27B2A5F-CB1D-48D9-B3A5-992D55767551" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Unsubscribe from List Workflow              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Unsubscribe from List Workflow", "UnsubscribeWorkflow", "Unsubscribe from List Workflow", @"The workflow type to launch for person who wants to unsubscribe from one or more Communication Lists. The person will be passed in as the Entity and the communication list Ids will be passed as a comma delimited string to the workflow 'CommunicationListIds' attribute if it exists.", 0, @"", "B4F1EFBD-EA10-4379-82D1-AEB8C1941DB9" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Success Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Success Text", "SuccessText", "Success Text", @"Text to display after user submits selection.", 6, @"<h4>Thank You</h4>We have saved your email preference.", "C9624E15-ABF9-48BA-8AAA-5B25CAA43FDC" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Unsubscribe Success Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unsubscribe Success Text", "UnsubscribeSuccessText", "Unsubscribe Success Text", @"Text to display after user unsubscribes from communication lists.", 7, @"<h4>Thank You</h4>  We have unsubscribed you from the following lists:  <ul>  {% for unsubscribedGroup in UnsubscribedGroups %}    <li>{{ unsubscribedGroup | Attribute:'PublicName' | Default:unsubscribedGroup.Name }}</li>  {% endfor %}  </ul>", "6D0F9A62-9AA0-43BF-ACFF-5383D98CD2E0" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Reasons to Exclude              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Reasons to Exclude", "ReasonstoExclude", "Reasons to Exclude", @"A delimited list of the Inactive Reasons to exclude from Reason list", 8, @"No Activity,Deceased", "21EC6F56-D0E2-4A76-BB1F-B04A5395EB18" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Communication List Categories              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories", "Communication List Categories", @"Select the categories of the communication lists to display for unsubscribe, or select none to show all that the user is authorized to view.", 9, @"A0889E77-67D9-418C-B301-1B3924692058", "EBDA7DC7-E1A6-46CF-A4BE-B7142F28FEE9" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Available Options              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Available Options", "AvailableOptions", "Available Options", @"Select the options that should be available to a user when they are updating their email preference.", 10, @"Unsubscribe,Update Email Address,Emails Allowed,No Mass Emails,No Emails,Not Involved", "328ACD62-8AD1-404F-895F-526AC59CBF17" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Allow Inactivating Family              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "476FBA19-005C-4FF4-996B-CA1B165E5BC8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Inactivating Family", "AllowInactivatingFamily", "Allow Inactivating Family", @"If the person chooses the 'Not Involved' choice show the option of inactivating the whole family. This will not show if the person is a member of more than one family or is not an adult.", 11, @"True", "AE618C57-DD77-4A9F-ACC4-5E5AB1903E8E" );

            // Attribute for BlockType              
            //   BlockType: Campus List              
            //   Category: Core              
            //   Attribute: Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the campus details.", 0, @"", "BC6A44F6-218C-454C-9988-66C9E44C5124" );

            // Attribute for BlockType              
            //   BlockType: Campus List              
            //   Category: Core              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4A927AA1-F071-4FB9-8D1F-17310BB4FCDD" );

            // Attribute for BlockType              
            //   BlockType: Campus List              
            //   Category: Core              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7AC8E1D6-8728-4866-978A-E13D52946851" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Entity Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "Entity Type", @"The type of entity to associate category with", 0, @"", "3C6E056B-5087-4E02-B9FD-853B658E3C85" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Entity Type Qualifier Property              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Type Qualifier Property", "EntityTypeQualifierProperty", "Entity Type Qualifier Property", @"", 0, @"", "DF059E85-8F9C-4897-BFDE-AAD297921E0A" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Entity Type Qualifier Value              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Type Qualifier Value", "EntityTypeQualifierValue", "Entity Type Qualifier Value", @"", 0, @"", "59E9EE72-C903-4466-861F-665CB8585539" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Root Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Root Category", "RootCategory", "Root Category", @"Select the root category to use as a starting point for the parent category picker.", 0, @"", "414D4558-6F4A-4431-8B77-C84A6EC1392D" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Exclude Categories              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Exclude Categories", "ExcludeCategories", "Exclude Categories", @"Select any category that you need to exclude from the parent category picker", 0, @"", "19BD3904-11D0-465E-8E73-C6064681AAB2" );

            // Attribute for BlockType              
            //   BlockType: Location Detail              
            //   Category: Core              
            //   Attribute: Map HTML              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0203B97-5856-437E-8700-8846309F8EED", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map HTML", "MapHTML", "Map HTML", @"The HTML to use for displaying group location maps. Lava syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", 0, @"{% if point or polygon %}      <div class='group-location-map'>          <img class='img-thumbnail' src='
              //maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>      </div>  {% endif %}", "F3A9192B-E8BB-4988-B298-0B9A5A954093" );

            // Attribute for BlockType              
            //   BlockType: Location Detail              
            //   Category: Core              
            //   Attribute: Map Style              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0203B97-5856-437E-8700-8846309F8EED", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "Map Style", @"The map theme that should be used for styling the GeoPicker map.", 0, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "1C42E196-FFC8-4B95-B182-7BC3A4C6447F" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Entity Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "DD8E419E-DAD9-492C-891F-BC92FB8FCA41" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Heading              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "Heading", @"The text to display as the heading.", 1, @"", "76147D4C-C73F-4AA3-9AF4-F03A0533F981" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Heading Icon CSS Class              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Icon CSS Class", "HeadingIcon", "Heading Icon CSS Class", @"The css class name to use for the heading icon. ", 2, @"", "99441A07-1DED-454A-ABD2-925F02FF40B8" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Note Term              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Term", "NoteTerm", "Note Term", @"The term to use for note (i.e. 'Note', 'Comment').", 3, @"Note", "9F9E94EB-8781-4CEE-986E-365DD483C789" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Display Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Type", "DisplayType", "Display Type", @"The format to use for displaying notes.", 4, @"Full", "AA931DC4-5C35-4071-B115-585263B29076" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Use Person Icon              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Icon", "UsePersonIcon", "Use Person Icon", @"", 5, @"false", "559CB810-2113-4F47-B58F-3AEFFE88909A" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Show Alert Checkbox              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Alert Checkbox", "ShowAlertCheckbox", "Show Alert Checkbox", @"", 6, @"true", "92074941-B3AC-4C42-BC71-050E49E08CDD" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Show Private Checkbox              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Private Checkbox", "ShowPrivateCheckbox", "Show Private Checkbox", @"", 7, @"true", "C2F55DE4-A76F-4D5A-B439-DB3A3D9B8FF7" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Show Security Button              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Button", "ShowSecurityButton", "Show Security Button", @"", 8, @"true", "DC501107-06FE-4031-8A14-F57BB9E93220" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Allow Anonymous              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Anonymous", "AllowAnonymous", "Allow Anonymous", @"", 9, @"false", "488A58A4-4DB9-4CC1-AAB3-7135FD73C0DA" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Add Always Visible              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Always Visible", "AddAlwaysVisible", "Add Always Visible", @"Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).", 10, @"false", "8A95C729-D9C6-496A-9DEA-500BE3B5A9E2" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Display Order              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Order", "DisplayOrder", "Display Order", @"Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option", 11, @"Descending", "F9F9CC06-B84B-46BB-80BB-1DF1FE6F3076" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Allow Backdated Notes              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Backdated Notes", "AllowBackdatedNotes", "Allow Backdated Notes", @"", 12, @"false", "D4284680-2C9F-4F45-9AA6-CD233193F38C" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Note Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "Note Types", @"Optional list of note types to limit display to", 12, @"", "AE02734B-D078-49A4-816B-EA109CCFA87E" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Display Note Type Heading              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Note Type Heading", "DisplayNoteTypeHeading", "Display Note Type Heading", @"Should each note's Note Type be displayed as a heading above each note?", 13, @"false", "88776CFD-6CC7-4684-9DA8-556F8F65EE93" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Expand Replies              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Expand Replies", "ExpandReplies", "Expand Replies", @"Should replies be automatically expanded?", 14, @"false", "E9C57E56-F2B1-4088-9D0F-9D3E5A65447A" );

            // Attribute for BlockType              
            //   BlockType: Signature Document Template Detail              
            //   Category: Core              
            //   Attribute: Default File Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Default File Type", "DefaultFileType", "Default File Type", @"The default file type to use when creating new documents.", 0, @"8C9C5A97-005A-46E5-AF7B-AC2F359B738A", "10896922-ABD0-4FFD-9D8A-5609023C3907" );

            // Attribute for BlockType              
            //   BlockType: Signature Document Template Detail              
            //   Category: Core              
            //   Attribute: Show Legacy Signature Providers              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legacy Signature Providers", "ShowLegacyExternalProviders", "Show Legacy Signature Providers", @"Enable this setting to see the configuration for legacy signature providers. Note that support for these providers will be fully removed in the next full release.", 1, @"False", "EC663120-AEE3-42E8-9C31-BC1FA5E53FC6" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Only Show Requested              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5ECCA4FB-F8FB-49DB-96B7-082BB4E4C170", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Show Requested", "OnlyShowRequested", "Only Show Requested", @"If enabled, limits the list to show only assessments that have been requested or completed.", 0, @"True", "1ECCC6B4-FCB6-4958-AC46-E0E8DC928DFE" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Hide If No Active Requests              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5ECCA4FB-F8FB-49DB-96B7-082BB4E4C170", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide If No Active Requests", "HideIfNoActiveRequests", "Hide If No Active Requests", @"If enabled, nothing will be shown if there are not pending (waiting to be taken) assessment requests.", 1, @"False", "868A34A4-0D7A-4563-BBF8-A8512921EC28" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Hide If No Requests              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5ECCA4FB-F8FB-49DB-96B7-082BB4E4C170", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide If No Requests", "HideIfNoRequests", "Hide If No Requests", @"If enabled, nothing will be shown where there are no requests (pending or completed).", 2, @"False", "E4BC905E-37A3-4059-BE3B-FE7DAB03FAF7" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Lava Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5ECCA4FB-F8FB-49DB-96B7-082BB4E4C170", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"<div class='panel panel-default'>      <div class='panel-heading'>Assessments</div>      <div class='panel-body'>              {% for assessmenttype in AssessmentTypes %}                  {% if assessmenttype.LastRequestObject %}                      {% if assessmenttype.LastRequestObject.Status == 'Complete' %}                          <div class='panel panel-success'>                              <div class='panel-heading'>{{ assessmenttype.Title }}<br />                                  Completed: {{ assessmenttype.LastRequestObject.CompletedDate | Date:'M/d/yyyy'}} <br />                                  <a href='{{ assessmenttype.AssessmentResultsPath}}'>View Results</a>                                  &nbsp;&nbsp;{{ assessmenttype.AssessmentRetakeLinkButton }}                              </div>                          </div>                      {% elseif assessmenttype.LastRequestObject.Status == 'Pending' %}                          <div class='panel panel-warning'>                              <div class='panel-heading'> {{ assessmenttype.Title }}<br />                                  Requested: {{assessmenttype.LastRequestObject.Requester}} ({{ assessmenttype.LastRequestObject.RequestedDate | Date:'M/d/yyyy'}})<br />                                  <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>                              </div>                          </div>                      {% endif %}                      {% else %}                          <div class='panel panel-default'>                              <div class='panel-heading'> {{ assessmenttype.Title }}<br />                                  Available<br />                                  <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>                              </div>                          </div>                  {% endif %}              {% endfor %}      </div>  </div>", "2AF36F01-CC5D-40DB-BDFF-FA83E001498E" );

            // Attribute for BlockType              
            //   BlockType: Photo Opt-Out              
            //   Category: CRM              
            //   Attribute: Success Message              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Message", "SuccessMessage", "Success Message", @"Message to show after a successful opt out.", 0, @"You've been opted out and should no longer receive photo requests from us.", "4B8D559A-1639-4E27-82BC-2FEBC777C5BA" );

            // Attribute for BlockType              
            //   BlockType: Photo Opt-Out              
            //   Category: CRM              
            //   Attribute: Not Found Message              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Found Message", "NotFoundMessage", "Not Found Message", @"Message to show if the account is not found.", 1, @"We could not find your record in our system. Please contact our office at the number below.", "C7C2458B-579C-4C49-804E-6BC75654BF4A" );

            // Attribute for BlockType              
            //   BlockType: Achievement Attempt Detail              
            //   Category: Engagement              
            //   Attribute: Achievement Type Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FBE75C18-7F71-4D23-A546-7A17CF944BA6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievement Type Page", "AchievementPage", "Achievement Type Page", @"Page used for viewing the achievement type that this attempt is toward.", 2, @"", "4DC7AAC2-6F4E-4264-9142-166DE331F9F9" );

            // Attribute for BlockType              
            //   BlockType: Streak Type Detail              
            //   Category: Engagement              
            //   Attribute: Map Editor Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A83A1F49-10A6-4362-ACC3-8027224A2120", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Map Editor Page", "MapEditorPage", "Map Editor Page", @"Page used for editing the streak type map.", 1, @"", "45F80703-15E2-49E7-A7D7-C9569E07187E" );

            // Attribute for BlockType              
            //   BlockType: Streak Type Detail              
            //   Category: Engagement              
            //   Attribute: Exclusions Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A83A1F49-10A6-4362-ACC3-8027224A2120", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Exclusions Page", "ExclusionsPage", "Exclusions Page", @"Page used for viewing a list of streak type exclusions.", 2, @"", "4181756F-81D6-4A2D-84EA-D62376A5F0BF" );

            // Attribute for BlockType              
            //   BlockType: Streak Type Detail              
            //   Category: Engagement              
            //   Attribute: Achievements Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A83A1F49-10A6-4362-ACC3-8027224A2120", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievements Page", "AchievementsPage", "Achievements Page", @"Page used for viewing a list of streak type achievement types.", 3, @"", "030C802C-6881-47B0-9DA8-5AF4C7111345" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Lava Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C0CFDAB7-BB29-499E-BD0A-468B0856C037", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"Lava template to use to display content", 2, @"{% include '~~/Assets/Lava/RegistrationListSidebar.lava' %}", "D01411BE-F45A-466D-8AEF-0FA82BA2FB66" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Registrations to Display              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C0CFDAB7-BB29-499E-BD0A-468B0856C037", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Registrations to Display", "RegistrationsToDisplay", "Registrations to Display", @"The items you select here will control which registrations will be shown. When Balance Due is selected, only items with a balance will be shown", 3, @"", "D52744A4-6C6D-41D9-A32E-A643A7FB917E" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Recent Registrations Date Range              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C0CFDAB7-BB29-499E-BD0A-468B0856C037", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Recent Registrations Date Range", "RecentRegistrations", "Recent Registrations Date Range", @"If Recent Registrations is selected above, this sliding date range controls which registrations should be displayed.", 6, @"Last|3|Month||", "993C3E05-5A09-43F4-A5EE-F9F2701EFD2F" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Future Events Date Range              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C0CFDAB7-BB29-499E-BD0A-468B0856C037", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Future Events Date Range", "DateRange", "Future Events Date Range", @"If Future Events is selected above, this sliding date range controls which registrations for future events should be displayed.", 7, @",", "F39FBC00-CE79-4BFE-89F3-D219A3DF6EEE" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Communication Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "Communication Page", @"The communication page to use for when the business email address is clicked. Leave this blank to use the default.", 0, @"", "0C685921-EBE8-41DD-B863-424CA9B15D57" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Badges              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The label badges to display in this block.", 1, @"", "D57F3EBE-BB3C-47A2-9983-D7AC25347849" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Display Tags              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Tags", "DisplayTags", "Display Tags", @"Should tags be displayed?", 2, @"True", "D0EF03C0-D339-4426-973B-9061DC3358A7" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Tag Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Tag Category", "TagCategory", "Tag Category", @"Optional category to limit the tags to. If specified all new personal tags will be added with this category.", 3, @"", "EC602BD3-54FF-44E8-A7BB-8A0BB56AC3F9" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Search Key Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Search Key Types", "SearchKeyTypes", "Search Key Types", @"Optional list of search key types to limit the display in search keys grid. No selection will show all.", 4, @"", "7815684F-C864-4B07-B99C-9F51C438C61B" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Business Attributes              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Business Attributes", "BusinessAttributes", "Business Attributes", @"The person attributes that should be displayed / edited for adults.", 5, @"", "D39871FE-0F42-4E27-98DF-C31140A0270E" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Workflow Actions              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Actions", "WorkflowActions", "Workflow Actions", @"The workflows to make available as actions.", 6, @"", "2B65FA38-3B8E-46A1-9A0B-C18F70AC3F5D" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Additional Custom Actions              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Custom Actions", "AdditionalCustomActions", "Additional Custom Actions", @"  Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current business's id.  Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an   &lt;li&gt; element for each available action.  Example:  <pre>      &lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;  </pre>", 7, @"", "A323F427-4843-410B-83CF-145C5EF62B8E" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch Detail              
            //   Category: Finance              
            //   Attribute: Transaction Matching Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BE58680-8795-46A0-8BFA-434A01FEB4C8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Matching Page", "TransactionMatchingPage", "Transaction Matching Page", @"Page used to match transactions for a batch.", 1, @"", "6248E860-A438-4E0F-A06F-51816CEEF463" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch Detail              
            //   Category: Finance              
            //   Attribute: Audit Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BE58680-8795-46A0-8BFA-434A01FEB4C8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Audit Page", "AuditPage", "Audit Page", @"Page used to display the history of changes to a batch.", 2, @"", "9E65A4E2-B4FE-4AF0-A0B6-48A458964BC9" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch Detail              
            //   Category: Finance              
            //   Attribute: Batch Names              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BE58680-8795-46A0-8BFA-434A01FEB4C8", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Batch Names", "BatchNames", "Batch Names", @"The Defined Type that contains a predefined list of batch names to choose from instead of entering it in manually when adding a new batch. Leave this blank to hide this option and let them edit the batch name manually.", 3, @"", "CD682BEC-37FA-4366-BD90-067B84F26B96" );

            // Attribute for BlockType              
            //   BlockType: Pledge Detail              
            //   Category: Finance              
            //   Attribute: Select Group Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A5AE27F-F536-4ACC-B5EB-9263C4B92EF5", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Select Group Type", "SelectGroupType", "Select Group Type", @"Optional Group Type that if selected will display a list of groups that pledge can be associated to for selected user", 1, @"", "568893F2-2903-4E75-86DE-653E672FC82D" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Enable Smart Names              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Smart Names", "EnableSmartNames", "Enable Smart Names", @"Check the first name for 'and' and '&' and split it to just use the first name provided.", 1, @"True", "3C76B7D1-9998-4728-8153-2E27885422B4" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Account              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Account", "Account", "Account", @"The account that new pledges will be allocated toward.", 2, @"4410306F-3FB5-4A57-9A80-09A3F9D40D0C", "90DA8610-F272-4D1C-A97A-BFACC52630B6" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: New Connection Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New Connection Status", "NewConnectionStatus", "New Connection Status", @"Person connection status to assign to a new user.", 3, @"8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061", "D56D41AF-13E8-4373-9B32-C6D2C2BA3BA9" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Pledge Date Range              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "9C7D431C-875C-4792-9E76-93F3A32BB850", "Pledge Date Range", "PledgeDateRange", "Pledge Date Range", @"Date range of the pledge.", 4, @",", "84819AA1-F85E-4BEF-B5A6-E766137836E0" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Show Pledge Frequency              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Pledge Frequency", "ShowPledgeFrequency", "Show Pledge Frequency", @"Show the pledge frequency option to the user.", 5, @"false", "69523CEB-F479-47A6-BAA7-B5A727C9BA46" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Require Pledge Frequency              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Pledge Frequency", "RequirePledgeFrequency", "Require Pledge Frequency", @"Require that a user select a specific pledge frequency (when pledge frequency is shown).", 6, @"false", "BF2696B5-B22A-4ECA-B49C-E12BD343470F" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Save Button Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Button Text", "SaveButtonText", "Save Button Text", @"The Text to shown on the Save button", 7, @"Save", "B4FFAE5E-C1A5-49EB-A332-AAB7B153BB0B" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Note Message              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Message", "NoteMessage", "Note Message", @"Message to show at the bottom of the create pledge block.", 8, @"Note: This commitment is a statement of intent and may be changed as your circumstances change.", "C070D2A7-750C-416A-99D2-F00AA7CA50C6" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Receipt Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Receipt Text", "ReceiptText", "Receipt Text", @"The text (or HTML) to display as the pledge receipt. <span class='tip tip-lava'></span> <span class='tip tip-html'>", 9, @"  <h1>Thank You!</h1>  <p>  {{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.  </p>  <p>      <a href='~/page/186?PledgeId={{ FinancialPledge.Id }}' class='btn btn-default' >Setup a Giving Profile</a>  </p>  ", "9A60B235-2D4D-4CE7-9BD7-9BBA98554A77" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Confirmation Email Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirmation Email Template", "ConfirmationEmailTemplate", "Confirmation Email Template", @"Email template to use after submitting a new pledge. Leave blank to not send an email.", 10, @"", "966D8BF5-5DAD-4D11-A53D-9014C1D394AC" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Select Group Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Select Group Type", "SelectGroupType", "Select Group Type", @"Optional Group Type that if selected will display a selection of groups that current user belongs to that can then be associated with the pledge.", 11, @"", "D6567FF0-27B7-4280-83A3-51ED3BA77464" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Pledge Term              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Pledge Term", "PledgeTerm", "Pledge Term", @"The Text to display as the pledge term on the pledge amount input label.", 12, @"Pledge", "592B9471-88F1-4BDB-B20A-9225CECAB7AD" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Allowed Group Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Allowed Group Types", "AllowedGroupTypes", "Allowed Group Types", @"This setting restricts which types of groups a person can be added to, however selecting a specific group via the Group setting will override this restriction.", 0, @"50FCFB30-F51A-49DF-86F4-2B176EA1820B", "69161BDD-85FB-4349-A3E5-9887217A9486" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Group              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "Group", @"Optional group to add person to. If omitted, the group's Guid should be passed via the Query string (GroupGuid=).", 0, @"", "D91C47CF-1C8B-411F-991C-0CB83B5B04C8" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Enable Passing Group Id              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Passing Group Id", "EnablePassingGroupId", "Enable Passing Group Id", @"If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.", 0, @"True", "752F5F84-BB23-4559-AC46-F9F75E74E1B6" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Mode              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "Mode", @"The mode to use when displaying registration details.", 1, @"Simple", "1FA71B5C-8473-478E-98C1-FBD586A58C29" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Group Member Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "Group Member Status", @"The group member status to use when adding person to group (default: 'Pending'.)", 2, @"2", "B544F1FA-E374-416D-BB41-36E422AB72E0" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Connection Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Prospect'.)", 3, @"368DD475-242C-49C4-A42C-7278BE690CC2", "5C28101C-717A-442E-A517-CC6167C15235" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Record Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 4, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "CED00945-F9A5-4E3D-A3BC-61A3C00171BB" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Workflow              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 5, @"", "B1982C98-3A1B-4D9C-835E-66A0515D4281" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Lava Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to format the group details.", 7, @"  <div class='alert alert-info'>      Please complete the form below to register for {{ Group.Name }}.   </div>", "CDB47FE2-047E-4D5F-900C-5543F32CD6B0" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Result Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Result Page", "ResultPage", "Result Page", @"An optional page to redirect user to after they have been registered for the group.", 8, @"", "59BF3CF0-0B61-4E39-AB99-148D9FF79E0D" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Result Lava Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Result Lava Template", "ResultLavaTemplate", "Result Lava Template", @"The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).", 9, @"  <div class='alert alert-success'>      You have been registered for {{ Group.Name }}. You should be hearing from the leader soon.  </div>", "FF5E8A0A-35CB-4FE1-BD62-C7D6F8221B70" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Auto Fill Form              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Auto Fill Form", "AutoFillForm", "Auto Fill Form", @"If set to FALSE then the form will not load the context of the logged in user (default: 'True'.)", 10, @"true", "8BF9AD36-1DEC-4D7D-8880-EC4065C56CB6" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Register Button Alt Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Register Button Alt Text", "RegisterButtonAltText", "Register Button Alt Text", @"Alternate text to use for the Register button (default is 'Register').", 11, @"Register", "ED132D7E-2E46-4336-904E-AC7E64B3692A" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Prevent Overcapacity Registrations              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prevent Overcapacity Registrations", "PreventOvercapacityRegistrations", "Prevent Overcapacity Registrations", @"When set to true, user cannot register for groups that are at capacity or whose default GroupTypeRole are at capacity. If only one spot is available, no spouses can be registered.", 12, @"True", "DAAB0EDE-59D3-4C4B-8D5E-C8461B57D1ED" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Require Email              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Email", "RequireEmail", "Require Email", @"Should email be required for registration?", 13, @"True", "8AAC3A4C-254F-4840-81C6-E987A0BA602D" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Require Mobile Phone              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Mobile Phone", "RequireMobilePhone", "Require Mobile Phone", @"Should mobile phone numbers be required (when visible) for registration?  NOTE: Certain fields such as phone numbers and address are not shown when the block is configured for 'Simple' mode.", 14, @"False", "6C589547-9460-4E9A-B4F0-14A8D517277A" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Expires After (days)              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (days)", "ExpireDays", "Expires After (days)", @"Default number of days until the request will expire.", 0, @"14", "5F0853C9-BF37-4CDC-B1B1-3A3B1802B720" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Default Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"If a category is not selected, choose a default category to use for all new prayer requests.", 1, @"4B2D88F5-6E45-4B4B-8776-11118C8E8269", "A1DB8ABB-F749-4E7E-8454-55CDF6776FEB" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Set Current Person To Requester              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Current Person To Requester", "SetCurrentPersonToRequester", "Set Current Person To Requester", @"Will set the current person as the requester. This is useful in self-entry situations.", 2, @"False", "48887E66-3791-4A4A-9660-571072704C07" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Require Last Name              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered", 3, @"True", "2EB1BB05-8ABC-445E-8934-C233396D319D" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Default To Public              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default", 4, @"False", "D4773CA1-BA82-4D73-856F-CCE5099387A4" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Default Allow Comments Checked              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Allow Comments Checked", "DefaultAllowCommentsChecked", "Default Allow Comments Checked", @"If true, the Allow Comments checkbox will be pre-checked for all new requests by default.", 5, @"True", "3529583E-1E1A-4A23-97F9-00154CF2223B" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Require Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 6, @"False", "90457B3A-BDBD-44FF-B1EC-778F3B3DF564" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Category Selection              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category Selection", "GroupCategoryId", "Category Selection", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 0, @"", "4896146D-3BB3-445E-A98C-D712AAC68189" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Default Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"If categories are not being shown, choose a default category to use for all new prayer requests.", 1, @"4B2D88F5-6E45-4B4B-8776-11118C8E8269", "17A383DC-D8A7-461E-A1F5-708A74655820" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Auto Approve              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 2, @"True", "5B097154-B1D7-48B1-93CA-E2BF9D140ED5" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Expires After (Days)              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpireDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approve is enabled).", 3, @"14", "D5B048C5-EA2E-4541-ADED-73E839B0CFAC" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Urgent Flag              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Urgent Flag", "EnableUrgentFlag", "Enable Urgent Flag", @"If enabled, requesters will be able to flag prayer requests as urgent.", 4, @"False", "0675AEF0-594F-4A1C-8002-5C33A6B3BA37" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Comments Flag              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Comments Flag", "EnableCommentsFlag", "Enable Comments Flag", @"If enabled, requesters will be able to set whether or not they want to allow comments on their requests.", 5, @"False", "EBE5D230-7205-4DE7-9AC2-41846449CE76" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Default Allow Comments Setting              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Allow Comments Setting", "DefaultAllowCommentsSetting", "Default Allow Comments Setting", @"This is the default setting for 'Allow Comments' on prayer requests. If the 'Enable Comments Flag' setting is enabled, the requester can override this default setting.", 6, @"True", "3843DC95-9B14-4023-AA8A-B1DE3E90F3CF" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Public Display Flag              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Display Flag", "EnablePublicDisplayFlag", "Enable Public Display Flag", @"If enabled, requesters will be able set whether or not they want their request displayed on the public website.", 7, @"False", "667900B2-3A1E-49CF-A830-339C9FDE8E2B" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Default To Public              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default", 8, @"False", "B67F84DE-B25D-4319-A4D2-102187D23CB3" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Character Limit              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 9, @"250", "5BE3E0B1-82D1-4D7A-8AE8-3F98DAEEE83F" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Require Last Name              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered.", 10, @"True", "AFD6A3D7-7051-47AB-A1F1-9C2FF54BD981" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Show Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 11, @"True", "A6290F64-029E-4A34-8873-0B28965CA0B3" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Require Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 12, @"False", "6B48F492-145D-498F-A529-26CF9FBEFCE1" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Person Matching              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 13, @"False", "F4795052-F971-438B-8D40-2AED4EE02DF9" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Create Person If No Match Found              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Create Person If No Match Found", "CreatePersonIfNoMatchFound", "Create Person If No Match Found", @"When person matching is enabled this setting determines if a person should be created if a matched record is not found. This setting has no impact if person matching is disabled.", 14, @"True", "170FCF6F-ABA2-4504-8D89-36BE06E299EC" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Connection Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use when creating new person records.", 15, @"8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061", "A41486C2-A14B-4E8F-B4CE-3B9B032B30B7" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Record Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use when creating new person records.", 16, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "15C53879-3325-4F43-BD4D-FCCC0CE83CEB" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Navigate To Parent On Save              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Navigate To Parent On Save", "NavigateToParentOnSave", "Navigate To Parent On Save", @"If enabled, on successful save control will redirect back to the parent page.", 17, @"False", "6567BC62-2DAB-4309-8BA8-ED052DEB8A3B" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Refresh Page On Save              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Refresh Page On Save", "RefreshPageOnSave", "Refresh Page On Save", @"If enabled, on successful save control will reload the current page. NOTE: This is ignored if 'Navigate to Parent On Save' is enabled.", 18, @"False", "9D12468F-F9DB-43EC-A9AF-30211A475A03" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Save Success Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Save Success Text", "SaveSuccessText", "Save Success Text", @"Text to display upon successful save. The 'PrayerRequest' merge field will contain the saved PrayerRequest. (Only applies if not navigating to parent page on save.) <span class='tip tip-lava'></span><span class='tip tip-html'></span>", 19, @"<p>Thank you for allowing us to pray for you.</p>", "10670B9B-9452-4292-9AE1-90CB4E3174A4" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Workflow              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 20, @"", "061A0909-2CB7-4C1D-B87F-90876198194A" );

            // Attribute for BlockType              
            //   BlockType: Merge Template Detail              
            //   Category: Core              
            //   Attribute: Merge Templates Ownership              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B852DB84-0CDF-4862-9EC7-CDBBBD5BB77A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Merge Templates Ownership", "MergeTemplatesOwnership", "Merge Templates Ownership", @"Set this to restrict if the merge template must be a Personal or Global merge template. Note: If the user has EDIT authorization to this block, both Global and Personal templates can be edited regardless of this setting.", 0, @"Global", "F7A6F6E7-9A65-42EF-B144-9D5D5BCBDFA4" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Confirmed Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmed Caption", "ConfirmedCaption", "Confirmed Caption", @"", 0, @"{0}, your account has been confirmed. Thank you for creating the account.", "80287931-23EC-493B-ACAA-6BEFA3B773AE" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Reset Password Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Reset Password Caption", "ResetPasswordCaption", "Reset Password Caption", @"", 1, @"{0}, enter a new password for your '{1}' account.", "894D0618-0441-4133-A5C3-81A6FDC88808" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Password Reset Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Password Reset Caption", "PasswordResetCaption", "Password Reset Caption", @"", 2, @"{0}, the password for your '{1}' account has been changed.", "D400CEAB-C299-4D0D-B268-AEEDAC25C802" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Delete Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Delete Caption", "DeleteCaption", "Delete Caption", @"", 3, @"Are you sure you want to delete the '{0}' account?", "30EF4F5D-BE40-4592-97D3-FBC31984C4FE" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Deleted Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Deleted Caption", "DeletedCaption", "Deleted Caption", @"", 4, @"The account has been deleted.", "69857D95-1C86-4ACB-BF30-EC67D6D10970" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Invalid Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Caption", "InvalidCaption", "Invalid Caption", @"", 5, @"The confirmation code you've entered is not valid.  Please enter a valid confirmation code or <a href='{0}'>create a new account</a>.", "97A29925-BFBF-4360-8A82-3C12F2B41679" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Password Reset Unavailable Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Password Reset Unavailable Caption", "PasswordResetUnavailableCaption", "Password Reset Unavailable Caption", @"", 6, @"This type of account does not allow passwords to be changed.  Please contact your system administrator for assistance changing your password.", "6AEA34EC-FB47-478A-AECA-EFDA87F5C47C" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: New Account Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "New Account Page", "NewAccountPage", "New Account Page", @"Page to navigate to when user selects 'Create New Account' option (if blank will use 'NewAccount' page route)", 7, @"", "0A450F3F-FB5F-4007-94B1-48391EE78E66" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Heading Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Heading Caption", "HeadingCaption", "Heading Caption", @"", 0, @"<h5 class='text-center'>Can't log in?</h5>", "ABA3EC57-E97F-4D78-8F08-D26956D5E3B1" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Invalid Email Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid Email Caption", "InvalidEmailCaption", "Invalid Email Caption", @"", 1, @"Sorry, we didn't recognize that email address. Want to try another?", "83C80E67-0EEA-49A5-9E7F-31E72C88C4D4" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Success Caption              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Caption", "SuccessCaption", "Success Caption", @"", 2, @"We've emailed you instructions for logging in.", "519A41A8-A2CB-40BD-BEBC-4747B95FE0DC" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Confirmation Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationPage", "Confirmation Page", @"Page for user to confirm their account (if blank will use 'ConfirmAccount' page route).", 3, @"", "E7784BE1-AC2B-4739-8B4B-5BB77B8866A6" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Forgot Username Email Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Forgot Username Email Template", "EmailTemplate", "Forgot Username Email Template", @"The email template to use when sending the forgot username (and password) email.  The following merge fields are available for use in the template: Person, Users, and SupportsChangePassword (an array of the usernames that support password changes).", 4, @"113593ff-620e-4870-86b1-7a0ec0409208", "1532ACF4-2074-47D2-94EB-C34F4A579A7B" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Save Communication History              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "CreateCommunicationRecord", "Save Communication History", @"Should a record of communication from this block be saved to the recipient's profile?", 5, @"False", "B8919D22-E7E4-4DF0-A65C-BC2A26AE7422" );

            // Attribute for BlockType              
            //   BlockType: Web Farm Node Detail              
            //   Category: WebFarm              
            //   Attribute: Node CPU Chart Hours              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BBA1FC0-AC56-4E58-9E99-EB20DA7AA415", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node CPU Chart Hours", "CpuChartHours", "Node CPU Chart Hours", @"The amount of hours represented by the width of the Node CPU chart.", 2, @"24", "4D88CC76-B7F1-43A9-860F-97C75F730A2A" );

            // Attribute for BlockType              
            //   BlockType: Web Farm Settings              
            //   Category: WebFarm              
            //   Attribute: Farm Node Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9510038-0547-45F3-9ECA-C2CA85E64416", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Farm Node Detail Page", "NodeDetailPage", "Farm Node Detail Page", @"The page where the node details can be seen", 1, @"63698D5C-7C73-44A4-A27D-A7EB777EB2A2", "E0AA8E65-B2A6-420F-8B8F-E9C680B4CF93" );

            // Attribute for BlockType              
            //   BlockType: Web Farm Settings              
            //   Category: WebFarm              
            //   Attribute: Node CPU Chart Hours              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9510038-0547-45F3-9ECA-C2CA85E64416", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node CPU Chart Hours", "CpuChartHours", "Node CPU Chart Hours", @"The amount of hours represented by the width of the Node CPU charts.", 2, @"4", "AFCE829D-1A35-43F9-B785-684846548C98" );

            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Disallow Group Selection If Specified              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disallow Group Selection If Specified", "DisallowGroupSelectionIfSpecified", "Disallow Group Selection If Specified", @"When enabled, will hide the group picker if there is a GroupId in the query string.", 4, @"False", "9ED9616F-188E-43BA-B64C-E0A0645C829F" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "F7370E5F-04EE-48E5-ABD2-669FF4BAD122" );

            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */              
            RockMigrationHelper.AddBlockAttributeValue( "064125E2-0069-4899-9B8C-300F736878A1", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );

            RockMigrationHelper.UpdateFieldType( "Media Selector", "", "Rock", "Rock.Field.Types.MediaSelectorFieldType", "243E40FC-04D0-48AD-B379-25A400CB0CAC" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "F7370E5F-04EE-48E5-ABD2-669FF4BAD122" );

            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Disallow Group Selection If Specified              
            RockMigrationHelper.DeleteAttribute( "9ED9616F-188E-43BA-B64C-E0A0645C829F" );

            // Attribute for BlockType              
            //   BlockType: Web Farm Settings              
            //   Category: WebFarm              
            //   Attribute: Node CPU Chart Hours              
            RockMigrationHelper.DeleteAttribute( "AFCE829D-1A35-43F9-B785-684846548C98" );

            // Attribute for BlockType              
            //   BlockType: Web Farm Settings              
            //   Category: WebFarm              
            //   Attribute: Farm Node Detail Page              
            RockMigrationHelper.DeleteAttribute( "E0AA8E65-B2A6-420F-8B8F-E9C680B4CF93" );

            // Attribute for BlockType              
            //   BlockType: Web Farm Node Detail              
            //   Category: WebFarm              
            //   Attribute: Node CPU Chart Hours              
            RockMigrationHelper.DeleteAttribute( "4D88CC76-B7F1-43A9-860F-97C75F730A2A" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Save Communication History              
            RockMigrationHelper.DeleteAttribute( "B8919D22-E7E4-4DF0-A65C-BC2A26AE7422" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Forgot Username Email Template              
            RockMigrationHelper.DeleteAttribute( "1532ACF4-2074-47D2-94EB-C34F4A579A7B" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Confirmation Page              
            RockMigrationHelper.DeleteAttribute( "E7784BE1-AC2B-4739-8B4B-5BB77B8866A6" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Success Caption              
            RockMigrationHelper.DeleteAttribute( "519A41A8-A2CB-40BD-BEBC-4747B95FE0DC" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Invalid Email Caption              
            RockMigrationHelper.DeleteAttribute( "83C80E67-0EEA-49A5-9E7F-31E72C88C4D4" );

            // Attribute for BlockType              
            //   BlockType: Forgot Username              
            //   Category: Security              
            //   Attribute: Heading Caption              
            RockMigrationHelper.DeleteAttribute( "ABA3EC57-E97F-4D78-8F08-D26956D5E3B1" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: New Account Page              
            RockMigrationHelper.DeleteAttribute( "0A450F3F-FB5F-4007-94B1-48391EE78E66" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Password Reset Unavailable Caption              
            RockMigrationHelper.DeleteAttribute( "6AEA34EC-FB47-478A-AECA-EFDA87F5C47C" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Invalid Caption              
            RockMigrationHelper.DeleteAttribute( "97A29925-BFBF-4360-8A82-3C12F2B41679" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Deleted Caption              
            RockMigrationHelper.DeleteAttribute( "69857D95-1C86-4ACB-BF30-EC67D6D10970" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Delete Caption              
            RockMigrationHelper.DeleteAttribute( "30EF4F5D-BE40-4592-97D3-FBC31984C4FE" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Password Reset Caption              
            RockMigrationHelper.DeleteAttribute( "D400CEAB-C299-4D0D-B268-AEEDAC25C802" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Reset Password Caption              
            RockMigrationHelper.DeleteAttribute( "894D0618-0441-4133-A5C3-81A6FDC88808" );

            // Attribute for BlockType              
            //   BlockType: Confirm Account              
            //   Category: Security              
            //   Attribute: Confirmed Caption              
            RockMigrationHelper.DeleteAttribute( "80287931-23EC-493B-ACAA-6BEFA3B773AE" );

            // Attribute for BlockType              
            //   BlockType: Merge Template Detail              
            //   Category: Core              
            //   Attribute: Merge Templates Ownership              
            RockMigrationHelper.DeleteAttribute( "F7A6F6E7-9A65-42EF-B144-9D5D5BCBDFA4" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Workflow              
            RockMigrationHelper.DeleteAttribute( "061A0909-2CB7-4C1D-B87F-90876198194A" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Save Success Text              
            RockMigrationHelper.DeleteAttribute( "10670B9B-9452-4292-9AE1-90CB4E3174A4" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Refresh Page On Save              
            RockMigrationHelper.DeleteAttribute( "9D12468F-F9DB-43EC-A9AF-30211A475A03" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Navigate To Parent On Save              
            RockMigrationHelper.DeleteAttribute( "6567BC62-2DAB-4309-8BA8-ED052DEB8A3B" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Record Status              
            RockMigrationHelper.DeleteAttribute( "15C53879-3325-4F43-BD4D-FCCC0CE83CEB" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Connection Status              
            RockMigrationHelper.DeleteAttribute( "A41486C2-A14B-4E8F-B4CE-3B9B032B30B7" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Create Person If No Match Found              
            RockMigrationHelper.DeleteAttribute( "170FCF6F-ABA2-4504-8D89-36BE06E299EC" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Person Matching              
            RockMigrationHelper.DeleteAttribute( "F4795052-F971-438B-8D40-2AED4EE02DF9" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Require Campus              
            RockMigrationHelper.DeleteAttribute( "6B48F492-145D-498F-A529-26CF9FBEFCE1" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Show Campus              
            RockMigrationHelper.DeleteAttribute( "A6290F64-029E-4A34-8873-0B28965CA0B3" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Require Last Name              
            RockMigrationHelper.DeleteAttribute( "AFD6A3D7-7051-47AB-A1F1-9C2FF54BD981" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Character Limit              
            RockMigrationHelper.DeleteAttribute( "5BE3E0B1-82D1-4D7A-8AE8-3F98DAEEE83F" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Default To Public              
            RockMigrationHelper.DeleteAttribute( "B67F84DE-B25D-4319-A4D2-102187D23CB3" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Public Display Flag              
            RockMigrationHelper.DeleteAttribute( "667900B2-3A1E-49CF-A830-339C9FDE8E2B" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Default Allow Comments Setting              
            RockMigrationHelper.DeleteAttribute( "3843DC95-9B14-4023-AA8A-B1DE3E90F3CF" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Comments Flag              
            RockMigrationHelper.DeleteAttribute( "EBE5D230-7205-4DE7-9AC2-41846449CE76" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Urgent Flag              
            RockMigrationHelper.DeleteAttribute( "0675AEF0-594F-4A1C-8002-5C33A6B3BA37" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Expires After (Days)              
            RockMigrationHelper.DeleteAttribute( "D5B048C5-EA2E-4541-ADED-73E839B0CFAC" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Enable Auto Approve              
            RockMigrationHelper.DeleteAttribute( "5B097154-B1D7-48B1-93CA-E2BF9D140ED5" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Default Category              
            RockMigrationHelper.DeleteAttribute( "17A383DC-D8A7-461E-A1F5-708A74655820" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Entry              
            //   Category: Prayer              
            //   Attribute: Category Selection              
            RockMigrationHelper.DeleteAttribute( "4896146D-3BB3-445E-A98C-D712AAC68189" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Require Campus              
            RockMigrationHelper.DeleteAttribute( "90457B3A-BDBD-44FF-B1EC-778F3B3DF564" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Default Allow Comments Checked              
            RockMigrationHelper.DeleteAttribute( "3529583E-1E1A-4A23-97F9-00154CF2223B" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Default To Public              
            RockMigrationHelper.DeleteAttribute( "D4773CA1-BA82-4D73-856F-CCE5099387A4" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Require Last Name              
            RockMigrationHelper.DeleteAttribute( "2EB1BB05-8ABC-445E-8934-C233396D319D" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Set Current Person To Requester              
            RockMigrationHelper.DeleteAttribute( "48887E66-3791-4A4A-9660-571072704C07" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Default Category              
            RockMigrationHelper.DeleteAttribute( "A1DB8ABB-F749-4E7E-8454-55CDF6776FEB" );

            // Attribute for BlockType              
            //   BlockType: Prayer Request Detail              
            //   Category: Prayer              
            //   Attribute: Expires After (days)              
            RockMigrationHelper.DeleteAttribute( "5F0853C9-BF37-4CDC-B1B1-3A3B1802B720" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Require Mobile Phone              
            RockMigrationHelper.DeleteAttribute( "6C589547-9460-4E9A-B4F0-14A8D517277A" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Require Email              
            RockMigrationHelper.DeleteAttribute( "8AAC3A4C-254F-4840-81C6-E987A0BA602D" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Prevent Overcapacity Registrations              
            RockMigrationHelper.DeleteAttribute( "DAAB0EDE-59D3-4C4B-8D5E-C8461B57D1ED" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Register Button Alt Text              
            RockMigrationHelper.DeleteAttribute( "ED132D7E-2E46-4336-904E-AC7E64B3692A" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Auto Fill Form              
            RockMigrationHelper.DeleteAttribute( "8BF9AD36-1DEC-4D7D-8880-EC4065C56CB6" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Result Lava Template              
            RockMigrationHelper.DeleteAttribute( "FF5E8A0A-35CB-4FE1-BD62-C7D6F8221B70" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Result Page              
            RockMigrationHelper.DeleteAttribute( "59BF3CF0-0B61-4E39-AB99-148D9FF79E0D" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Lava Template              
            RockMigrationHelper.DeleteAttribute( "CDB47FE2-047E-4D5F-900C-5543F32CD6B0" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Workflow              
            RockMigrationHelper.DeleteAttribute( "B1982C98-3A1B-4D9C-835E-66A0515D4281" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Record Status              
            RockMigrationHelper.DeleteAttribute( "CED00945-F9A5-4E3D-A3BC-61A3C00171BB" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Connection Status              
            RockMigrationHelper.DeleteAttribute( "5C28101C-717A-442E-A517-CC6167C15235" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Group Member Status              
            RockMigrationHelper.DeleteAttribute( "B544F1FA-E374-416D-BB41-36E422AB72E0" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Mode              
            RockMigrationHelper.DeleteAttribute( "1FA71B5C-8473-478E-98C1-FBD586A58C29" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Enable Passing Group Id              
            RockMigrationHelper.DeleteAttribute( "752F5F84-BB23-4559-AC46-F9F75E74E1B6" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Group              
            RockMigrationHelper.DeleteAttribute( "D91C47CF-1C8B-411F-991C-0CB83B5B04C8" );

            // Attribute for BlockType              
            //   BlockType: Group Registration              
            //   Category: Group              
            //   Attribute: Allowed Group Types              
            RockMigrationHelper.DeleteAttribute( "69161BDD-85FB-4349-A3E5-9887217A9486" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Pledge Term              
            RockMigrationHelper.DeleteAttribute( "592B9471-88F1-4BDB-B20A-9225CECAB7AD" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Select Group Type              
            RockMigrationHelper.DeleteAttribute( "D6567FF0-27B7-4280-83A3-51ED3BA77464" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Confirmation Email Template              
            RockMigrationHelper.DeleteAttribute( "966D8BF5-5DAD-4D11-A53D-9014C1D394AC" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Receipt Text              
            RockMigrationHelper.DeleteAttribute( "9A60B235-2D4D-4CE7-9BD7-9BBA98554A77" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Note Message              
            RockMigrationHelper.DeleteAttribute( "C070D2A7-750C-416A-99D2-F00AA7CA50C6" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Save Button Text              
            RockMigrationHelper.DeleteAttribute( "B4FFAE5E-C1A5-49EB-A332-AAB7B153BB0B" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Require Pledge Frequency              
            RockMigrationHelper.DeleteAttribute( "BF2696B5-B22A-4ECA-B49C-E12BD343470F" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Show Pledge Frequency              
            RockMigrationHelper.DeleteAttribute( "69523CEB-F479-47A6-BAA7-B5A727C9BA46" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Pledge Date Range              
            RockMigrationHelper.DeleteAttribute( "84819AA1-F85E-4BEF-B5A6-E766137836E0" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: New Connection Status              
            RockMigrationHelper.DeleteAttribute( "D56D41AF-13E8-4373-9B32-C6D2C2BA3BA9" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Account              
            RockMigrationHelper.DeleteAttribute( "90DA8610-F272-4D1C-A97A-BFACC52630B6" );

            // Attribute for BlockType              
            //   BlockType: Pledge Entry              
            //   Category: Finance              
            //   Attribute: Enable Smart Names              
            RockMigrationHelper.DeleteAttribute( "3C76B7D1-9998-4728-8153-2E27885422B4" );

            // Attribute for BlockType              
            //   BlockType: Pledge Detail              
            //   Category: Finance              
            //   Attribute: Select Group Type              
            RockMigrationHelper.DeleteAttribute( "568893F2-2903-4E75-86DE-653E672FC82D" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch Detail              
            //   Category: Finance              
            //   Attribute: Batch Names              
            RockMigrationHelper.DeleteAttribute( "CD682BEC-37FA-4366-BD90-067B84F26B96" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch Detail              
            //   Category: Finance              
            //   Attribute: Audit Page              
            RockMigrationHelper.DeleteAttribute( "9E65A4E2-B4FE-4AF0-A0B6-48A458964BC9" );

            // Attribute for BlockType              
            //   BlockType: Financial Batch Detail              
            //   Category: Finance              
            //   Attribute: Transaction Matching Page              
            RockMigrationHelper.DeleteAttribute( "6248E860-A438-4E0F-A06F-51816CEEF463" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Additional Custom Actions              
            RockMigrationHelper.DeleteAttribute( "A323F427-4843-410B-83CF-145C5EF62B8E" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Workflow Actions              
            RockMigrationHelper.DeleteAttribute( "2B65FA38-3B8E-46A1-9A0B-C18F70AC3F5D" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Business Attributes              
            RockMigrationHelper.DeleteAttribute( "D39871FE-0F42-4E27-98DF-C31140A0270E" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Search Key Types              
            RockMigrationHelper.DeleteAttribute( "7815684F-C864-4B07-B99C-9F51C438C61B" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Tag Category              
            RockMigrationHelper.DeleteAttribute( "EC602BD3-54FF-44E8-A7BB-8A0BB56AC3F9" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Display Tags              
            RockMigrationHelper.DeleteAttribute( "D0EF03C0-D339-4426-973B-9061DC3358A7" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Badges              
            RockMigrationHelper.DeleteAttribute( "D57F3EBE-BB3C-47A2-9983-D7AC25347849" );

            // Attribute for BlockType              
            //   BlockType: Business Detail              
            //   Category: Finance              
            //   Attribute: Communication Page              
            RockMigrationHelper.DeleteAttribute( "0C685921-EBE8-41DD-B863-424CA9B15D57" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Future Events Date Range              
            RockMigrationHelper.DeleteAttribute( "F39FBC00-CE79-4BFE-89F3-D219A3DF6EEE" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Recent Registrations Date Range              
            RockMigrationHelper.DeleteAttribute( "993C3E05-5A09-43F4-A5EE-F9F2701EFD2F" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Registrations to Display              
            RockMigrationHelper.DeleteAttribute( "D52744A4-6C6D-41D9-A32E-A643A7FB917E" );

            // Attribute for BlockType              
            //   BlockType: Registration List Lava              
            //   Category: Event              
            //   Attribute: Lava Template              
            RockMigrationHelper.DeleteAttribute( "D01411BE-F45A-466D-8AEF-0FA82BA2FB66" );

            // Attribute for BlockType              
            //   BlockType: Streak Type Detail              
            //   Category: Engagement              
            //   Attribute: Achievements Page              
            RockMigrationHelper.DeleteAttribute( "030C802C-6881-47B0-9DA8-5AF4C7111345" );

            // Attribute for BlockType              
            //   BlockType: Streak Type Detail              
            //   Category: Engagement              
            //   Attribute: Exclusions Page              
            RockMigrationHelper.DeleteAttribute( "4181756F-81D6-4A2D-84EA-D62376A5F0BF" );

            // Attribute for BlockType              
            //   BlockType: Streak Type Detail              
            //   Category: Engagement              
            //   Attribute: Map Editor Page              
            RockMigrationHelper.DeleteAttribute( "45F80703-15E2-49E7-A7D7-C9569E07187E" );

            // Attribute for BlockType              
            //   BlockType: Achievement Attempt Detail              
            //   Category: Engagement              
            //   Attribute: Achievement Type Page              
            RockMigrationHelper.DeleteAttribute( "4DC7AAC2-6F4E-4264-9142-166DE331F9F9" );

            // Attribute for BlockType              
            //   BlockType: Photo Opt-Out              
            //   Category: CRM              
            //   Attribute: Not Found Message              
            RockMigrationHelper.DeleteAttribute( "C7C2458B-579C-4C49-804E-6BC75654BF4A" );

            // Attribute for BlockType              
            //   BlockType: Photo Opt-Out              
            //   Category: CRM              
            //   Attribute: Success Message              
            RockMigrationHelper.DeleteAttribute( "4B8D559A-1639-4E27-82BC-2FEBC777C5BA" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Lava Template              
            RockMigrationHelper.DeleteAttribute( "2AF36F01-CC5D-40DB-BDFF-FA83E001498E" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Hide If No Requests              
            RockMigrationHelper.DeleteAttribute( "E4BC905E-37A3-4059-BE3B-FE7DAB03FAF7" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Hide If No Active Requests              
            RockMigrationHelper.DeleteAttribute( "868A34A4-0D7A-4563-BBF8-A8512921EC28" );

            // Attribute for BlockType              
            //   BlockType: Assessment List              
            //   Category: CRM              
            //   Attribute: Only Show Requested              
            RockMigrationHelper.DeleteAttribute( "1ECCC6B4-FCB6-4958-AC46-E0E8DC928DFE" );

            // Attribute for BlockType              
            //   BlockType: Signature Document Template Detail              
            //   Category: Core              
            //   Attribute: Show Legacy Signature Providers              
            RockMigrationHelper.DeleteAttribute( "EC663120-AEE3-42E8-9C31-BC1FA5E53FC6" );

            // Attribute for BlockType              
            //   BlockType: Signature Document Template Detail              
            //   Category: Core              
            //   Attribute: Default File Type              
            RockMigrationHelper.DeleteAttribute( "10896922-ABD0-4FFD-9D8A-5609023C3907" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Expand Replies              
            RockMigrationHelper.DeleteAttribute( "E9C57E56-F2B1-4088-9D0F-9D3E5A65447A" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Display Note Type Heading              
            RockMigrationHelper.DeleteAttribute( "88776CFD-6CC7-4684-9DA8-556F8F65EE93" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Note Types              
            RockMigrationHelper.DeleteAttribute( "AE02734B-D078-49A4-816B-EA109CCFA87E" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Allow Backdated Notes              
            RockMigrationHelper.DeleteAttribute( "D4284680-2C9F-4F45-9AA6-CD233193F38C" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Display Order              
            RockMigrationHelper.DeleteAttribute( "F9F9CC06-B84B-46BB-80BB-1DF1FE6F3076" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Add Always Visible              
            RockMigrationHelper.DeleteAttribute( "8A95C729-D9C6-496A-9DEA-500BE3B5A9E2" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Allow Anonymous              
            RockMigrationHelper.DeleteAttribute( "488A58A4-4DB9-4CC1-AAB3-7135FD73C0DA" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Show Security Button              
            RockMigrationHelper.DeleteAttribute( "DC501107-06FE-4031-8A14-F57BB9E93220" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Show Private Checkbox              
            RockMigrationHelper.DeleteAttribute( "C2F55DE4-A76F-4D5A-B439-DB3A3D9B8FF7" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Show Alert Checkbox              
            RockMigrationHelper.DeleteAttribute( "92074941-B3AC-4C42-BC71-050E49E08CDD" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Use Person Icon              
            RockMigrationHelper.DeleteAttribute( "559CB810-2113-4F47-B58F-3AEFFE88909A" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Display Type              
            RockMigrationHelper.DeleteAttribute( "AA931DC4-5C35-4071-B115-585263B29076" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Note Term              
            RockMigrationHelper.DeleteAttribute( "9F9E94EB-8781-4CEE-986E-365DD483C789" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Heading Icon CSS Class              
            RockMigrationHelper.DeleteAttribute( "99441A07-1DED-454A-ABD2-925F02FF40B8" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Heading              
            RockMigrationHelper.DeleteAttribute( "76147D4C-C73F-4AA3-9AF4-F03A0533F981" );

            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Core              
            //   Attribute: Entity Type              
            RockMigrationHelper.DeleteAttribute( "DD8E419E-DAD9-492C-891F-BC92FB8FCA41" );

            // Attribute for BlockType              
            //   BlockType: Location Detail              
            //   Category: Core              
            //   Attribute: Map Style              
            RockMigrationHelper.DeleteAttribute( "1C42E196-FFC8-4B95-B182-7BC3A4C6447F" );

            // Attribute for BlockType              
            //   BlockType: Location Detail              
            //   Category: Core              
            //   Attribute: Map HTML              
            RockMigrationHelper.DeleteAttribute( "F3A9192B-E8BB-4988-B298-0B9A5A954093" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Exclude Categories              
            RockMigrationHelper.DeleteAttribute( "19BD3904-11D0-465E-8E73-C6064681AAB2" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Root Category              
            RockMigrationHelper.DeleteAttribute( "414D4558-6F4A-4431-8B77-C84A6EC1392D" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Entity Type Qualifier Value              
            RockMigrationHelper.DeleteAttribute( "59E9EE72-C903-4466-861F-665CB8585539" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Entity Type Qualifier Property              
            RockMigrationHelper.DeleteAttribute( "DF059E85-8F9C-4897-BFDE-AAD297921E0A" );

            // Attribute for BlockType              
            //   BlockType: Category Detail              
            //   Category: Core              
            //   Attribute: Entity Type              
            RockMigrationHelper.DeleteAttribute( "3C6E056B-5087-4E02-B9FD-853B658E3C85" );

            // Attribute for BlockType              
            //   BlockType: Campus List              
            //   Category: Core              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.DeleteAttribute( "7AC8E1D6-8728-4866-978A-E13D52946851" );

            // Attribute for BlockType              
            //   BlockType: Campus List              
            //   Category: Core              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.DeleteAttribute( "4A927AA1-F071-4FB9-8D1F-17310BB4FCDD" );

            // Attribute for BlockType              
            //   BlockType: Campus List              
            //   Category: Core              
            //   Attribute: Detail Page              
            RockMigrationHelper.DeleteAttribute( "BC6A44F6-218C-454C-9988-66C9E44C5124" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Allow Inactivating Family              
            RockMigrationHelper.DeleteAttribute( "AE618C57-DD77-4A9F-ACC4-5E5AB1903E8E" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Available Options              
            RockMigrationHelper.DeleteAttribute( "328ACD62-8AD1-404F-895F-526AC59CBF17" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Communication List Categories              
            RockMigrationHelper.DeleteAttribute( "EBDA7DC7-E1A6-46CF-A4BE-B7142F28FEE9" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Reasons to Exclude              
            RockMigrationHelper.DeleteAttribute( "21EC6F56-D0E2-4A76-BB1F-B04A5395EB18" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Unsubscribe Success Text              
            RockMigrationHelper.DeleteAttribute( "6D0F9A62-9AA0-43BF-ACFF-5383D98CD2E0" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Success Text              
            RockMigrationHelper.DeleteAttribute( "C9624E15-ABF9-48BA-8AAA-5B25CAA43FDC" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Unsubscribe from List Workflow              
            RockMigrationHelper.DeleteAttribute( "B4F1EFBD-EA10-4379-82D1-AEB8C1941DB9" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Not Involved Text              
            RockMigrationHelper.DeleteAttribute( "B27B2A5F-CB1D-48D9-B3A5-992D55767551" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: No Emails Text              
            RockMigrationHelper.DeleteAttribute( "C613D25E-C3BF-4AEE-999B-90BD7F8F5F7D" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: No Mass Emails Text              
            RockMigrationHelper.DeleteAttribute( "5032BCF9-FC48-45D8-945A-77B38B41A4A7" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Emails Allowed Text              
            RockMigrationHelper.DeleteAttribute( "003C4E88-4916-4848-BE8C-39376B7AB047" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Update Email Address Text              
            RockMigrationHelper.DeleteAttribute( "5158AB09-F748-40D4-97CD-12F9265E34EE" );

            // Attribute for BlockType              
            //   BlockType: Email Preference Entry              
            //   Category: Communication              
            //   Attribute: Unsubscribe from Lists Text              
            RockMigrationHelper.DeleteAttribute( "EE4318BA-9AAE-4BFB-A60C-956708D4A882" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.DeleteAttribute( "1B061454-9E69-4CD5-BCEF-A6627FF73F74" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.DeleteAttribute( "6B9B6F69-CEA2-4877-BCA0-FAA2F6CE4C31" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: Show Delete Column              
            RockMigrationHelper.DeleteAttribute( "820FDB69-D1BB-4F98-9A2C-434F55C5211B" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: Site Type              
            RockMigrationHelper.DeleteAttribute( "4A70389B-5D70-475A-B9EA-A67B482662DC" );

            // Attribute for BlockType              
            //   BlockType: Site List              
            //   Category: CMS              
            //   Attribute: Detail Page              
            RockMigrationHelper.DeleteAttribute( "B8DA40CC-D2BD-4E31-8B9F-A68536A2408A" );

            // Attribute for BlockType              
            //   BlockType: Page Short Link Detail              
            //   Category: CMS              
            //   Attribute: Minimum Token Length              
            RockMigrationHelper.DeleteAttribute( "606DB351-90C7-4936-B7DC-8D1306672503" );

            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "064125E2-0069-4899-9B8C-300F736878A1" );

            // Delete BlockType               
            //   Name: Workflow Trigger Detail              
            //   Category: WorkFlow              
            //   Path: -              
            //   EntityType: Workflow Trigger Detail              
            RockMigrationHelper.DeleteBlockType( "A8062FE5-5BCD-48AC-8C37-2124462656A7" );

            // Delete BlockType               
            //   Name: Web Farm Settings              
            //   Category: WebFarm              
            //   Path: -              
            //   EntityType: Web Farm Settings              
            RockMigrationHelper.DeleteBlockType( "D9510038-0547-45F3-9ECA-C2CA85E64416" );

            // Delete BlockType               
            //   Name: Web Farm Node Detail              
            //   Category: WebFarm              
            //   Path: -              
            //   EntityType: Web Farm Node Detail              
            RockMigrationHelper.DeleteBlockType( "6BBA1FC0-AC56-4E58-9E99-EB20DA7AA415" );

            // Delete BlockType               
            //   Name: Apple TV Page Detail              
            //   Category: TV > TV Apps              
            //   Path: -              
            //   EntityType: Apple Tv Page Detail              
            RockMigrationHelper.DeleteBlockType( "ADBF3377-A491-4016-9375-346496A25FB4" );

            // Delete BlockType               
            //   Name: Apple TV Application Detail              
            //   Category: TV > TV Apps              
            //   Path: -              
            //   EntityType: Apple Tv App Detail              
            RockMigrationHelper.DeleteBlockType( "CDAB601D-1369-44CB-A146-4E80C7D66BCD" );

            // Delete BlockType               
            //   Name: Rest Key Detail              
            //   Category: Security              
            //   Path: -              
            //   EntityType: Rest Key Detail              
            RockMigrationHelper.DeleteBlockType( "28A34F1C-80F4-496F-A598-180974ADEE61" );

            // Delete BlockType               
            //   Name: Forgot Username              
            //   Category: Security              
            //   Path: -              
            //   EntityType: Forgot User Name              
            RockMigrationHelper.DeleteBlockType( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4" );

            // Delete BlockType               
            //   Name: Confirm Account              
            //   Category: Security              
            //   Path: -              
            //   EntityType: Confirm Account              
            RockMigrationHelper.DeleteBlockType( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A" );

            // Delete BlockType               
            //   Name: Merge Template Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Merge Template Detail              
            RockMigrationHelper.DeleteBlockType( "B852DB84-0CDF-4862-9EC7-CDBBBD5BB77A" );

            // Delete BlockType               
            //   Name: Prayer Request Entry              
            //   Category: Prayer              
            //   Path: -              
            //   EntityType: Prayer Request Entry              
            RockMigrationHelper.DeleteBlockType( "5AA30F53-1B7D-4CA9-89B6-C10592968870" );

            // Delete BlockType               
            //   Name: Prayer Request Detail              
            //   Category: Prayer              
            //   Path: -              
            //   EntityType: Prayer Request Detail              
            RockMigrationHelper.DeleteBlockType( "E120F06F-6DB7-464A-A797-C3C90B92EF40" );

            // Delete BlockType               
            //   Name: Group Registration              
            //   Category: Group              
            //   Path: -              
            //   EntityType: Group Registration              
            RockMigrationHelper.DeleteBlockType( "5E000376-FF90-4962-A053-EC1473DA5C45" );

            // Delete BlockType               
            //   Name: Pledge Entry              
            //   Category: Finance              
            //   Path: -              
            //   EntityType: Financial Pledge Entry              
            RockMigrationHelper.DeleteBlockType( "0455ECBD-D54D-4485-BF4D-F469048AE10F" );

            // Delete BlockType               
            //   Name: Pledge Detail              
            //   Category: Finance              
            //   Path: -              
            //   EntityType: Financial Pledge Detail              
            RockMigrationHelper.DeleteBlockType( "2A5AE27F-F536-4ACC-B5EB-9263C4B92EF5" );

            // Delete BlockType               
            //   Name: Financial Batch Detail              
            //   Category: Finance              
            //   Path: -              
            //   EntityType: Financial Batch Detail              
            RockMigrationHelper.DeleteBlockType( "6BE58680-8795-46A0-8BFA-434A01FEB4C8" );

            // Delete BlockType               
            //   Name: Business Detail              
            //   Category: Finance              
            //   Path: -              
            //   EntityType: Business Detail              
            RockMigrationHelper.DeleteBlockType( "729E1953-4CFF-46F0-8715-9D7892BADB4E" );

            // Delete BlockType               
            //   Name: Registration List Lava              
            //   Category: Event              
            //   Path: -              
            //   EntityType: Registration List Lava              
            RockMigrationHelper.DeleteBlockType( "C0CFDAB7-BB29-499E-BD0A-468B0856C037" );

            // Delete BlockType               
            //   Name: Calendar Event Item Detail              
            //   Category: Event              
            //   Path: -              
            //   EntityType: Event Item Detail              
            RockMigrationHelper.DeleteBlockType( "63D0DFB8-1F9E-464A-A603-2252034BC6AF" );

            // Delete BlockType               
            //   Name: Calendar Detail              
            //   Category: Event              
            //   Path: -              
            //   EntityType: Event Calendar Detail              
            RockMigrationHelper.DeleteBlockType( "2DC334AC-C2C2-4031-9E1C-6A5B6FBCAE9C" );

            // Delete BlockType               
            //   Name: Streak Type Detail              
            //   Category: Engagement              
            //   Path: -              
            //   EntityType: Streak Type Detail              
            RockMigrationHelper.DeleteBlockType( "A83A1F49-10A6-4362-ACC3-8027224A2120" );

            // Delete BlockType               
            //   Name: Streak Detail              
            //   Category: Engagement              
            //   Path: -              
            //   EntityType: Streak Detail              
            RockMigrationHelper.DeleteBlockType( "1C98107F-DFBF-44BD-A860-0C9DF2E6C495" );

            // Delete BlockType               
            //   Name: Achievement Attempt Detail              
            //   Category: Engagement              
            //   Path: -              
            //   EntityType: Achievement Attempt Detail              
            RockMigrationHelper.DeleteBlockType( "FBE75C18-7F71-4D23-A546-7A17CF944BA6" );

            // Delete BlockType               
            //   Name: Photo Opt-Out              
            //   Category: CRM              
            //   Path: -              
            //   EntityType: Photo Opt Out Detail              
            RockMigrationHelper.DeleteBlockType( "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" );

            // Delete BlockType               
            //   Name: Document Type Detail              
            //   Category: CRM              
            //   Path: -              
            //   EntityType: Document Type Detail              
            RockMigrationHelper.DeleteBlockType( "FD3EB724-1AFA-4507-8850-C3AEE170C83B" );

            // Delete BlockType               
            //   Name: Badge Detail              
            //   Category: CRM              
            //   Path: -              
            //   EntityType: Badge Detail              
            RockMigrationHelper.DeleteBlockType( "5BD4CD27-C1C1-4E12-8756-9C93E4EDB28E" );

            // Delete BlockType               
            //   Name: Assessment List              
            //   Category: CRM              
            //   Path: -              
            //   EntityType: Assessment List              
            RockMigrationHelper.DeleteBlockType( "5ECCA4FB-F8FB-49DB-96B7-082BB4E4C170" );

            // Delete BlockType               
            //   Name: Tag Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Tag Detail              
            RockMigrationHelper.DeleteBlockType( "B150E767-E964-460C-9ED1-B293474C5F5D" );

            // Delete BlockType               
            //   Name: Suggestion Detail              
            //   Category: Follow              
            //   Path: -              
            //   EntityType: Suggestion Detail              
            RockMigrationHelper.DeleteBlockType( "E18AB976-6665-48A5-B418-8FAC8F374135" );

            // Delete BlockType               
            //   Name: Signature Document Template Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Signature Document Template Detail              
            RockMigrationHelper.DeleteBlockType( "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5" );

            // Delete BlockType               
            //   Name: Scheduled Job Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Service Job Detail              
            RockMigrationHelper.DeleteBlockType( "762F09EA-0A11-4BC7-9A68-13F0E44217C1" );

            // Delete BlockType               
            //   Name: Schedule Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Schedule Detail              
            RockMigrationHelper.DeleteBlockType( "7C10240A-7EE5-4720-AAC9-5C162E9F5AAC" );

            // Delete BlockType               
            //   Name: Notification Messages              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Notification Message List              
            RockMigrationHelper.DeleteBlockType( "2E4292B9-CD68-41E9-86BD-356ACCD54F36" );

            // Delete BlockType               
            //   Name: Note Type Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Note Type Detail              
            RockMigrationHelper.DeleteBlockType( "9E901A5A-82C2-4788-9623-3720FFC4DAEC" );

            // Delete BlockType               
            //   Name: Notes              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Notes              
            RockMigrationHelper.DeleteBlockType( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8" );

            // Delete BlockType               
            //   Name: Location Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Location Detail              
            RockMigrationHelper.DeleteBlockType( "D0203B97-5856-437E-8700-8846309F8EED" );

            // Delete BlockType               
            //   Name: Following Event Type Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Following Event Type Detail              
            RockMigrationHelper.DeleteBlockType( "78F27537-C05F-44E0-AF84-2329C8B5D71D" );

            // Delete BlockType               
            //   Name: Category Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Category Detail              
            RockMigrationHelper.DeleteBlockType( "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE" );

            // Delete BlockType               
            //   Name: Campus List              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Campus List              
            RockMigrationHelper.DeleteBlockType( "52DF00E5-BC19-43F2-8533-A386DB53C74F" );

            // Delete BlockType               
            //   Name: Asset Storage Provider Detail              
            //   Category: Core              
            //   Path: -              
            //   EntityType: Asset Storage Provider Detail              
            RockMigrationHelper.DeleteBlockType( "4B50E08A-A805-4213-A5AF-BCA570FCB528" );

            // Delete BlockType               
            //   Name: Email Preference Entry              
            //   Category: Communication              
            //   Path: -              
            //   EntityType: Email Preference Entry              
            RockMigrationHelper.DeleteBlockType( "476FBA19-005C-4FF4-996B-CA1B165E5BC8" );

            // Delete BlockType               
            //   Name: Site List              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Site List              
            RockMigrationHelper.DeleteBlockType( "D27A9C0D-E118-4172-8F8E-368C973F5486" );

            // Delete BlockType               
            //   Name: Site Detail              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Site Detail              
            RockMigrationHelper.DeleteBlockType( "3E935E45-4796-4389-AB1C-98D2403FAEDF" );

            // Delete BlockType               
            //   Name: Page Short Link Detail              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Page Short Link Detail              
            RockMigrationHelper.DeleteBlockType( "72EDDF3D-625E-40A9-A68B-76236E77A3F3" );

            // Delete BlockType               
            //   Name: Route Detail              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Page Route Detail              
            RockMigrationHelper.DeleteBlockType( "E4302549-0BB8-4DDE-9B9A-70F0B665E76F" );

            // Delete BlockType               
            //   Name: Layout Detail              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Layout Detail              
            RockMigrationHelper.DeleteBlockType( "64C3B64A-CDB3-4E5F-BC54-0E3D50AAC564" );

            // Delete BlockType               
            //   Name: Lava Shortcode Detail              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Lava Shortcode Detail              
            RockMigrationHelper.DeleteBlockType( "3852E96A-9270-4C0E-A0D0-3CD9601F183E" );

            // Delete BlockType               
            //   Name: Block Type Detail              
            //   Category: CMS              
            //   Path: -              
            //   EntityType: Block Type Detail              
            RockMigrationHelper.DeleteBlockType( "6C329001-9C04-4090-BED0-12E3F6B88FB6" );

            // Delete BlockType               
            //   Name: Queue Detail              
            //   Category: Bus              
            //   Path: -              
            //   EntityType: Queue Detail              
            RockMigrationHelper.DeleteBlockType( "DB19D24E-B0C8-4686-8582-7B84DAE33EE8" );
        }
    }
}
