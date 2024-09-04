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
using System.Collections.Generic;
using System.Data;

using Rock.Data;

namespace Rock.Migrations
{
    /// <summary>
    /// This migration will update the XAML templates for all mobile blocks to follow the new style guide.
    /// </summary>
    public partial class MobileStyleGuide : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// The standard icon to use for new templates.
        /// </summary>
        private const string _standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MobileSitesUp();
            MobileTemplatesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MobileTemplatesDown();
        }

        /// <summary>
        /// Load all of the mobile sites and ensure they are configured to use the new style guide.
        /// </summary>
        private void MobileSitesUp()
        {
            // Get mobile sites, and their additional settings.
            string sql = "SELECT [Id], [AdditionalSettings] FROM [Site] WHERE [SiteType] = 1";
            var siteData = DbService.GetDataTable( sql, CommandType.Text, new Dictionary<string, object>() );

            if ( siteData == null || siteData.Rows == null || siteData.Rows.Count == 0 )
            {
                return;
            }

            // Loop through each site and ensure compatibility settings are set.
            foreach ( DataRow row in siteData.Rows )
            {
                var additionalSettingsJson = row["AdditionalSettings"].ToString();
                var siteId = row["Id"].ToStringSafe().AsIntegerOrNull();

                if ( additionalSettingsJson.IsNullOrWhiteSpace() || siteId == null )
                {
                    continue;
                }

                try
                {
                    dynamic additionalSettings = additionalSettingsJson.FromJsonDynamicOrNull();

                    if ( additionalSettings == null )
                    {
                        continue;
                    }

                    // Ensure the mobile style framework is set to "Legacy" for existing sites.
                    additionalSettings.DownhillSettings.MobileStyleFramework = 0;
                    var newJson = JsonExtensions.ToJson( additionalSettings );

                    // Update the site with the new settings.
                    sql = "UPDATE [Site] SET [AdditionalSettings] = @AdditionalSettings WHERE [Id] = @SiteId";
                    DbService.ExecuteCommand( sql, CommandType.Text, new Dictionary<string, object>
                    {
                        { "AdditionalSettings", newJson },
                        { "SiteId", siteId }
                    } );
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Updates the XAML templates for the mobile blocks to follow the new style guide.
        /// </summary>
        private void MobileTemplatesUp()
        {
            // Daily Challenge Entry
            UpdateTemplate( "3DA15C4B-BD5B-44AF-97CD-E9F5FD97B55A", "Template", "B702FF5B-2488-42C7-AAE8-2DD99E82326D", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_DAILY_CHALLENGE_ENTRY, _dailyChallengeEntryTemplate, "08009450-92A5-4D4A-8E31-FCC1E4CBDF16", _dailyChallengeEntryLegacyTemplate );

            // Communication View
            UpdateTemplate( "528EA8BC-4E9D-4F17-9920-9E11F3A4FC5E", "Template", "863E5638-B310-407E-A54E-2C069979881D", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW, _communicationViewTemplate, "3DA15C4B-BD5B-44AF-97CD-E9F5FD97B55A", _communicationViewLegacyTemplate );

            // Connection Type List
            UpdateTemplate( "F9F29166-A080-4179-A210-AE42CC473D6F", "TypeTemplate", "31E1FCCF-C4B1-4D84-992C-DEACAF3697CF", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST, _connectionTypeListTemplate, "E0D00422-7895-4081-9C06-16DE9BF48E1A", _connectionTypeListLegacyTemplate );

            // Connection Opportunity List
            UpdateTemplate( "A7D8FB47-A779-4427-B41D-2C0F0E6DB0FF", "OpportunityTemplate", "0015A574-C10A-4530-897C-F7B7C3D9393E", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST, _connectionOpportunityListTemplate, "1FB8E236-DF34-4BA2-B5C6-CA8B542ABC7A", _connectionOpportunityListLegacyTemplate );

            // Conection Request List
            UpdateTemplate( "2E36BC98-A18A-4524-8AC1-F14A1AC9DE2F", "RequestTemplate", "612E9E13-434F-4E47-958D-37E1C3EEF304", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST, _connectionRequestListTemplate, "787BFAA8-FF61-49BA-80DD-67074DC362C2", _connectionRequestListLegacyTemplate );

            // Schedule Toolbox
            UpdateTemplate( "F04B6154-1543-4632-89A2-1792F6CED9D6", "ToolboxTemplate", "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX, _scheduleToolboxTemplate, "CD2629E5-8EB0-4D52-ACAB-8EDF9AF84814", _scheduleToolboxLegacyTemplate );
            UpdateTemplate( "DE3E57AC-E12B-4249-BB15-64C7A7780AC8", "ConfirmDeclineTemplate", "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL, _scheduleToolboxDeclineReasonTemplate, "92D39913-7D69-4B73-8FF9-72AC161BE381", _scheduleToolboxDeclineReasonLegacyTemplate );

            // Schedule Sign-up
            UpdateTemplate( "2204D103-A145-4003-9DA0-1C9461D6BAA1", "LandingTemplate", "CA27CB14-22FD-4DE6-9C3B-0EAA0AA84708", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE, _scheduleSignUpLandingTemplate, "C4BFED3A-C2A1-4A68-A646-44C3B499C75A", _scheduleSignUpLandingLegacyTemplate );

            // Schedule Preference
            UpdateTemplate( "8DF04E4B-9ABF-477D-8CD2-D36FF06DBDB8", "LandingTemplate", "F6D0A258-F97E-4561-B881-ACBF985F89DC", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE, _schedulePreferenceLandingTemplate, "C3A98DBE-E977-499C-B823-0B3676731E48", _schedulePreferenceLandingLegacyTemplate );

            // Schedule Unavailability
            UpdateTemplate( "FCFB9F90-9C94-4405-BBF9-DF62DC85DEFD", "TypeTemplate", "AEFF246D-A514-4D46-801E-D717E1D1D209", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_UNAVAILABILITY, _scheduleUnavailabilityTemplate, "1A699B18-AB29-4CD5-BC02-AF55159D5DA6", _scheduleUnavailabilityLegacyTemplate );

            // Group Member List
            UpdateTemplate( "A57595B6-3F19-43B7-B3A5-D5E7BB041C66", "Template", "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST, _groupMemberListTemplate, "674CF1E3-561C-430D-B4A8-39957AC1BCF1", _groupMemberListLegacyTemplate );

            // Group Member View
            UpdateTemplate( "74760472-3516-480D-B96B-1F77AAEF0862", "Template", "6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW, _groupMemberViewTemplate, "F093A516-6D95-429E-8EEB-1DFB0303DF71", _groupMemberViewLegacyTemplate );

            // Group Members
            UpdateTemplate( "493F4ED9-11B9-4E9B-90FE-AD2BF207367B", "MembersTemplate", "1F1E7598-8D51-4750-8D61-E5791A226FDB", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBERS, _groupMembersTemplate, "13470DDB-5F8C-4EA2-93FD-B738F37C9AFC", _groupMembersLegacyTemplate );

            // Group View
            UpdateTemplate( "95FF4A7D-6512-4C5F-9A01-523E42CA10D6", "Template", "3F34AE03-9378-4363-A232-0318139C3BD3", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW, _groupViewTemplate, "6207AF10-B6C9-40B5-8AA5-4C11FA6D0966", _groupViewLegacyTemplate );

            // Group Finder
            UpdateTemplate( "4EA2A456-8164-48DA-851A-1F8979EB8B8E", "Template", "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER, _groupFinderTemplate, "CC117DBB-5C3C-4A32-8ABA-88A7493C7F70", _groupFinderLegacyTemplate );

            // Prayer Session 
            UpdateTemplate( "C0FCA573-D341-4B33-B097-3FB7028B3816", "Template", "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION, _prayerSessionTemplate, "2B0F4548-8DA7-4236-9BF9-5FA3C07D762F", _prayerSessionLegacyTemplate );

            // Answer to Prayer
            UpdateTemplate( "D0B57F68-DD15-4C91-84B9-A9D421937980", "Template", "324D5295-72E6-42DF-B111-E428E811B786", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER, _answerToPrayerTemplate, "91C29610-1D77-49A8-A46B-5A35EC67C551", _answerToPrayerLegacyTemplate );

            // My Prayer Requests
            UpdateTemplate( "2E867AB7-700D-41A7-85D0-7FA1E6FD4662", "Template", "C095B269-36E2-446A-B73E-2C8CC4B7BF37", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS, _myPrayerRequestsTemplate, "DED26289-4746-4233-A5BD-D4095248023D", _myPrayerRequestsLegacyTemplate );

            // Prayer Card View
            UpdateTemplate( "5A2574BB-ED27-4EFB-B5DA-675F73AEDD29", "Template", "CA75C558-9345-47E7-99AF-D8191D31D00D", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW, _prayerCardViewTemplate, "757935E7-AB6D-47B6-A6C4-1CA5920C922E", _prayerCardViewLegacyTemplate );

            // Notes
            UpdateTemplate( "1EB6DCE4-69F1-44B9-A674-E1C67E216B2A", "NoteTemplate", "5B337D89-A298-4620-A0BE-078A41BC054B", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES, _notesTemplate, "C9134085-D433-444D-9803-8E5CE1B053DE", _notesLegacyTemplate );

            // Search 
            UpdateTemplate( "351A873D-B82E-4700-99ED-5A6D5B3E4149", "ResultItemTemplate", "41174BEA-6567-430C-AAD4-A89A5CF70FB0", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH, _searchTemplate, "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3", _searchLegacyTemplate );

            // Live Experience Occurrences
            UpdateTemplate( "B26B0C69-8B11-4C5B-B24A-AB57A433FC33", "Template", "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", SystemGuid.DefinedValue.BLOCK_TEMPLATE_LIVE_EXPERIENCE_OCCURRENCES, _liveExperienceOccurrencesTemplate, "0C75B833-E710-45AE-B3B2-3FAC97A79BB2", _liveExperienceOccurrencesLegacyTemplate );

            // Calendar Event List
            UpdateTemplate( "4482BB6D-9C02-48D0-8475-4BA0948131A0", "EventTemplate", "A9149623-6A82-4F25-8F4D-0961557BE78C", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST, _calendarEventListTemplate, "E3A4AA4E-2A61-4E63-B636-93B86E493D95", _calendarEventListLegacyTemplate );

            // Calendar Event Item Occurrence
            UpdateTemplate( "5114DB6E-40CC-4455-99B8-C109B7BB52D1", "Template", "15DD270A-A0BB-45BF-AA36-FE37856C60DE", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW, _calendarEventItemOccurrenceTemplate, "6593D4EB-2B7A-4C24-8D30-A02991D26BC0", _calendarEventItemOccurrenceLegacyTemplate );

            // Event Item Occurrence List by Audience
            UpdateTemplate( "EA866D37-4E8F-4956-AB80-055F72151302", "LavaTemplate", "FC2879AC-5967-43E7-8759-6888BF21CE21", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE, _eventItemOccurrenceListByAudienceLavaTemplate, "323D1996-C27F-4B48-B0C7-82FDA440D950", _eventItemOccurrenceListByAudienceLavaLegacyTemplate );
        }

        /// <summary>
        /// Downgrades the XAML templates for the mobile blocks to follow the legacy style guide.
        /// </summary>
        private void MobileTemplatesDown()
        {
            // Daily Challenge Entry
            DowngradeTemplate( "3DA15C4B-BD5B-44AF-97CD-E9F5FD97B55A", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_DAILY_CHALLENGE_ENTRY, "08009450-92A5-4D4A-8E31-FCC1E4CBDF16", _dailyChallengeEntryLegacyTemplate );

            // Communication View
            DowngradeTemplate( "528EA8BC-4E9D-4F17-9920-9E11F3A4FC5E", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW, "3DA15C4B-BD5B-44AF-97CD-E9F5FD97B55A", _communicationViewLegacyTemplate );

            // Connection Type List
            DowngradeTemplate( "F9F29166-A080-4179-A210-AE42CC473D6F", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST, "E0D00422-7895-4081-9C06-16DE9BF48E1A", _connectionTypeListLegacyTemplate );

            // Connection Opportunity List
            DowngradeTemplate( "A7D8FB47-A779-4427-B41D-2C0F0E6DB0FF", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST, "1FB8E236-DF34-4BA2-B5C6-CA8B542ABC7A", _connectionOpportunityListLegacyTemplate );

            // Conection Request List
            DowngradeTemplate( "2E36BC98-A18A-4524-8AC1-F14A1AC9DE2F", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST, "787BFAA8-FF61-49BA-80DD-67074DC362C2", _connectionRequestListLegacyTemplate );

            // Schedule Toolbox
            DowngradeTemplate( "F04B6154-1543-4632-89A2-1792F6CED9D6", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX, "CD2629E5-8EB0-4D52-ACAB-8EDF9AF84814", _scheduleToolboxLegacyTemplate );
            DowngradeTemplate( "DE3E57AC-E12B-4249-BB15-64C7A7780AC8", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL, "92D39913-7D69-4B73-8FF9-72AC161BE381", _scheduleToolboxDeclineReasonLegacyTemplate );

            // Schedule Sign-up
            DowngradeTemplate( "2204D103-A145-4003-9DA0-1C9461D6BAA1", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE, "C4BFED3A-C2A1-4A68-A646-44C3B499C75A", _scheduleSignUpLandingLegacyTemplate );

            // Schedule Preference
            DowngradeTemplate( "8DF04E4B-9ABF-477D-8CD2-D36FF06DBDB8", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE, "C3A98DBE-E977-499C-B823-0B3676731E48", _schedulePreferenceLandingLegacyTemplate );

            // Schedule Unavailability
            DowngradeTemplate( "FCFB9F90-9C94-4405-BBF9-DF62DC85DEFD", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_UNAVAILABILITY, "1A699B18-AB29-4CD5-BC02-AF55159D5DA6", _scheduleUnavailabilityLegacyTemplate );

            // Group Member List
            DowngradeTemplate( "A57595B6-3F19-43B7-B3A5-D5E7BB041C66", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST, "674CF1E3-561C-430D-B4A8-39957AC1BCF1", _groupMemberListLegacyTemplate );

            // Group Member View
            DowngradeTemplate( "74760472-3516-480D-B96B-1F77AAEF0862", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW, "F093A516-6D95-429E-8EEB-1DFB0303DF71", _groupMemberViewLegacyTemplate );

            // Group Members
            DowngradeTemplate( "493F4ED9-11B9-4E9B-90FE-AD2BF207367B", Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBERS, "13470DDB-5F8C-4EA2-93FD-B738F37C9AFC", _groupMembersLegacyTemplate );

            // Group View
            DowngradeTemplate( "95FF4A7D-6512-4C5F-9A01-523E42CA10D6", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW, "6207AF10-B6C9-40B5-8AA5-4C11FA6D0966", _groupViewLegacyTemplate );

            // Group Finder
            DowngradeTemplate( "4EA2A456-8164-48DA-851A-1F8979EB8B8E", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER, "CC117DBB-5C3C-4A32-8ABA-88A7493C7F70", _groupFinderLegacyTemplate );

            // Prayer Session Startup
            DowngradeTemplate( "C0FCA573-D341-4B33-B097-3FB7028B3816", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION, "2B0F4548-8DA7-4236-9BF9-5FA3C07D762F", _prayerSessionLegacyTemplate );

            // Answer to Prayer
            DowngradeTemplate( "D0B57F68-DD15-4C91-84B9-A9D421937980", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER, "91C29610-1D77-49A8-A46B-5A35EC67C551", _answerToPrayerLegacyTemplate );

            // My Prayer Requests
            DowngradeTemplate( "2E867AB7-700D-41A7-85D0-7FA1E6FD4662", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS, "DED26289-4746-4233-A5BD-D4095248023D", _myPrayerRequestsLegacyTemplate );

            // Prayer Card View
            DowngradeTemplate( "5A2574BB-ED27-4EFB-B5DA-675F73AEDD29", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW, "757935E7-AB6D-47B6-A6C4-1CA5920C922E", _prayerCardViewLegacyTemplate );

            // Notes
            DowngradeTemplate( "1EB6DCE4-69F1-44B9-A674-E1C67E216B2A", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES, "C9134085-D433-444D-9803-8E5CE1B053DE", _notesLegacyTemplate );

            // Search 
            DowngradeTemplate( "351A873D-B82E-4700-99ED-5A6D5B3E4149", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH, "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3", _searchLegacyTemplate );

            // Live Experience Occurrences
            DowngradeTemplate( "B26B0C69-8B11-4C5B-B24A-AB57A433FC33", SystemGuid.DefinedValue.BLOCK_TEMPLATE_LIVE_EXPERIENCE_OCCURRENCES, "0C75B833-E710-45AE-B3B2-3FAC97A79BB2", _liveExperienceOccurrencesLegacyTemplate );

            // Calendar Event List
            DowngradeTemplate( "4482BB6D-9C02-48D0-8475-4BA0948131A0", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST, "E3A4AA4E-2A61-4E63-B636-93B86E493D95", _calendarEventListLegacyTemplate );

            // Calendar Event Item Occurrence
            DowngradeTemplate( "5114DB6E-40CC-4455-99B8-C109B7BB52D1", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW, "6593D4EB-2B7A-4C24-8D30-A02991D26BC0", _calendarEventItemOccurrenceLegacyTemplate );

            // Event Item Occurrence List by Audience
            DowngradeTemplate( "EA866D37-4E8F-4956-AB80-055F72151302", SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE, "323D1996-C27F-4B48-B0C7-82FDA440D950", _eventItemOccurrenceListByAudienceLavaLegacyTemplate );
        }

        #region CMS Blocks

        private string _dailyChallengeEntryTemplate = @"{% assign MissedDatesSize = MissedDates | Size %}
{% if CompletedChallenge != null or Challenge.IsComplete == true %}
    {% assign challenge = Challenge %}
    {% if CompletedChallenge != null %}
        {% assign challenge = CompletedChallenge %}
    {% endif %}
    <StackLayout StyleClass=""spacing-4"">
        {% if challenge.HeaderContent != '' %}
            {{ challenge.HeaderContent }}
        {% endif %}
        
        <Rock:StyledBorder StyleClass=""border, border-interface-soft, rounded, bg-interface-softest, p-16"">
            <StackLayout>
                {% for item in challenge.ChallengeItems %}
                    <StackLayout>
                        <StackLayout StyleClass=""spacing-4"">
                            <Label Text=""{{ item.Title | Escape }}""
                                   StyleClass=""text-interface-stronger, body, bold"" />
                    
                            {% if item.Content != '' %}
                                {{ item.Content }}
                            {% endif %}
            
                            {% if item.InputType == 'Text' %}
                                <Rock:FieldContainer>
                                    <Rock:TextBox IsReadOnly=""true"">
                                        <Rock:TextBox.Text>
                                            {{ challenge.ChallengeItemValues[item.Guid].Value | XamlWrap }}
                                        </Rock:TextBox.Text>
                                    </Rock:TextBox>
                                </Rock:FieldContainer>
                            {% elsif item.InputType == 'Memo' %}
                                <Rock:FieldContainer>
                                    <Rock:TextEditor IsReadOnly=""true"">
                                        <Rock:TextEditor.Text>
                                            {{ challenge.ChallengeItemValues[item.Guid].Value | XamlWrap }}
                                        </Rock:TextEditor.Text>
                                    </Rock:TextEditor>
                                </Rock:FieldContainer>
                            {% endif %}
                        </StackLayout>
                        {% unless forloop.last %}
                            <Rock:Divider StyleClass=""my-8"" />
                        {% endunless %}
                    </StackLayout>
                {% endfor %}
            </StackLayout>
        </Rock:StyledBorder>
    </StackLayout>
{% elsif MissedDatesSize > 0 %}
    <StackLayout StyleClass=""spacing-24"">
        <StackLayout>
            <Label Text=""Missed Day""
                   StyleClass=""title1, bold, text-interface-strongest"" />
            <Label Text=""Looks like you missed a day. Do you want to continue your previous challenge or start over?""
                   StyleClass=""bold, text-interface-stronger"" />
        </StackLayout>
        
        <StackLayout StyleClass=""spacing-8"">
            <Button StyleClass=""btn, btn-primary"" Text=""Continue Challenge""
                    Command=""{Binding Challenge.ShowChallengeForDate}""
                    CommandParameter=""{{ MissedDates[0] }}"" />
                
            <Button StyleClass=""btn, btn-danger"" Text=""Start Over""
                    Command=""{Binding Challenge.ShowCurrentChallenge}"" />
        </StackLayout>
    </StackLayout>
{% else %}
    <StackLayout StyleClass=""spacing-4"">
        {% if Challenge.HeaderContent != '' %}
            {{ Challenge.HeaderContent }}
        {% endif %}
        
        <Rock:StyledBorder StyleClass=""border, border-interface-soft, rounded, bg-interface-softest, p-16"">
            <StackLayout>
                {% for item in Challenge.ChallengeItems %}
                    <StackLayout>
                        <StackLayout StyleClass=""spacing-8"">
                            <StackLayout Orientation=""Horizontal""
                                         StyleClass=""spacing-16"">
                                <Rock:Icon IconClass=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete, Converter={Rock:BooleanValueConverter True=check-circle, False=circle}}""
                                           TextColor=""{Rock:PaletteColor App-Primary-Strong}""
                                           VerticalOptions=""Center""
                                           VerticalTextAlignment=""Center""
                                           Command=""{Binding Challenge.ToggleChallengeItem}""
                                           StyleClass=""body""
                                           CommandParameter=""{{ item.Guid }}"" />
            
                                <Label Text=""{{ item.Title | Escape }}""
                                       VerticalTextAlignment=""Center""
                                       VerticalOptions=""Center""
                                       StyleClass=""body, text-interface-strong"">
                                    <Label.GestureRecognizers>
                                        <TapGestureRecognizer Command=""{Binding Challenge.ToggleChallengeItem}"" CommandParameter=""{{ item.Guid }}"" />
                                    </Label.GestureRecognizers>
                                </Label>
                            </StackLayout>
                            
                            {% if item.Content != '' %}
                                {{ item.Content }}
                            {% endif %}
                            
                            {% if item.InputType == 'Text' %}
                                <Rock:FieldContainer>
                                    <Rock:TextBox Text=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].Value}""
                                                  IsReadOnly=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete}"" />
                                </Rock:FieldContainer>
                            {% elsif item.InputType == 'Memo' %}
                                <Rock:FieldContainer>
                                    <Rock:TextEditor Text=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].Value}""
                                                     IsReadOnly=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete}"" />
                                </Rock:FieldContainer>
                            {% endif %}
                        </StackLayout>
                        {% unless forloop.last %}
                            <Rock:Divider StyleClass=""my-8"" />
                        {% endunless %}
                    </StackLayout>
                {% endfor %}
            </StackLayout>
        </Rock:StyledBorder>
    </StackLayout>
{% endif %}";

        private string _dailyChallengeEntryLegacyTemplate = @"{% assign MissedDatesSize = MissedDates | Size %}
{% if CompletedChallenge != null or Challenge.IsComplete == true %}
    {% assign challenge = Challenge %}
    {% if CompletedChallenge != null %}
        {% assign challenge = CompletedChallenge %}
    {% endif %}
    <StackLayout StyleClass=""challenge-view"">
        {% if challenge.HeaderContent != '' %}
            {{ challenge.HeaderContent }}
        {% endif %}
        
        {% for item in challenge.ChallengeItems %}
            <StackLayout StyleClass=""challenge-item"">
                <Label Text=""{{ item.Title | Escape }}""
                    VerticalTextAlignment=""Center""
                    VerticalOptions=""Center""
                    StyleClass=""challenge-title"" />
                {% if item.Content != '' %}
                    {{ item.Content }}
                {% endif %}
                
                {% if item.InputType == 'Text' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextBox StyleClass=""text-field"" IsReadOnly=""true"">
                            <Rock:TextBox.Text>
                                {{ challenge.ChallengeItemValues[item.Guid].Value | XamlWrap }}
                            </Rock:TextBox.Text>
                        </Rock:TextBox>
                    </Rock:FieldContainer>
                {% elsif item.InputType == 'Memo' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextEditor StyleClass=""memo-field"" IsReadOnly=""true"">
                            <Rock:TextEditor.Text>
                                {{ challenge.ChallengeItemValues[item.Guid].Value | XamlWrap }}
                            </Rock:TextEditor.Text>
                        </Rock:TextEditor>
                    </Rock:FieldContainer>
                {% endif %}
            </StackLayout>
            
            <Rock:Divider />
        {% endfor %}
    </StackLayout>
{% elsif MissedDatesSize > 0 %}
    <StackLayout StyleClass=""challenge-missed"">
        <Label Text=""Looks like you missed a day. Do you want to continue your previous challenge or start over?"" />
        
        <Button StyleClass=""btn,btn-primary"" Text=""Continue Challenge""
            Command=""{Binding Challenge.ShowChallengeForDate}""
            CommandParameter=""{{ MissedDates[0] }}"" />
            
        <Button StyleClass=""btn,btn-primary"" Text=""Start Over""
            Command=""{Binding Challenge.ShowCurrentChallenge}"" />
    </StackLayout>
{% else %}
    <StackLayout StyleClass=""challenge"">
        {% if Challenge.HeaderContent != '' %}
            {{ Challenge.HeaderContent }}
        {% endif %}
        
        {% for item in Challenge.ChallengeItems %}
            <StackLayout StyleClass=""challenge-item"">
                <StackLayout Orientation=""Horizontal"">
                    <Rock:Icon IconClass=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete, Converter={Rock:BooleanValueConverter True=check-circle, False=circle}}""
                        TextColor=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete, Converter={Rock:BooleanValueConverter True=#ee7725, False=#7c7c7c}}""
                        VerticalOptions=""Center""
                        Command=""{Binding Challenge.ToggleChallengeItem}""
                        CommandParameter=""{{ item.Guid }}"" />
                    <Label Text=""{{ item.Title | Escape }}""
                        VerticalTextAlignment=""Center""
                        VerticalOptions=""Center""
                        StyleClass=""challenge-title"">
                        <Label.GestureRecognizers>
                            <TapGestureRecognizer Command=""{Binding Challenge.ToggleChallengeItem}"" CommandParameter=""{{ item.Guid }}"" />
                        </Label.GestureRecognizers>
                    </Label>
                </StackLayout>
                
                {% if item.Content != '' %}
                    {{ item.Content }}
                {% endif %}
                
                {% if item.InputType == 'Text' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextBox Text=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].Value}""
                            StyleClass=""text-field""
                            IsReadOnly=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete}"" />
                    </Rock:FieldContainer>
                {% elsif item.InputType == 'Memo' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextEditor Text=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].Value}""
                            StyleClass=""memo-field""
                            IsReadOnly=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete}"" />
                    </Rock:FieldContainer>
                {% endif %}
            </StackLayout>
            
            <Rock:Divider />
        {% endfor %}
    </StackLayout>
{% endif %}
";

        #endregion

        #region Communication Blocks

        private string _communicationViewTemplate = @"<StackLayout StyleClass=""spacing-4"">
    <Label Text=""{{ Communication.PushTitle | Escape }}"" StyleClass=""text-interface-strongest, title1, bold"" />
    <Rock:Html>
        <![CDATA[{{ Content }}]]>
    </Rock:Html>
</StackLayout>";

        private string _communicationViewLegacyTemplate = @"<StackLayout>
    <Label Text=""{{ Communication.PushTitle | Escape }}"" StyleClass=""h1"" />
    <Rock:Html>
        <![CDATA[{{ Content }}]]>
    </Rock:Html>
</StackLayout>";

        #endregion

        #region Connection Blocks 

        private const string _connectionTypeListTemplate = @"<Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"">
    <VerticalStackLayout>
        {% assign size = ConnectionTypes | Size %}
        {% if size == 0 %}
            <Label Text=""No Connection Types Found""
                StyleClass=""body, text-interface-stronger"" />
        {% endif %}
        {% for type in ConnectionTypes %}    
            <Grid RowDefinitions=""64, Auto""
                ColumnDefinitions=""Auto, *, 20""
                StyleClass=""gap-column-8"">
    
                <!-- Icon -->
                <Rock:Icon StyleClass=""text-interface-strong""
                    IconClass=""{{ type.IconCssClass | Replace:'fa fa-','' }}""
                    FontSize=""32""
                    Grid.Row=""0""
                    Grid.Column=""0""
                    VerticalOptions=""Center""
                    HorizontalOptions=""Center"" />
                <!-- Name and Description -->
                <VerticalStackLayout Grid.Row=""0""
                    Grid.Column=""1""
                    VerticalOptions=""Center"">
                    <Label Text=""{{ type.Name | Escape }}""
                        StyleClass=""body, bold, text-interface-stronger""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                    <Label Text=""{{ type.Description | Escape }}""
                        StyleClass=""footnote, text-interface-strong""
                        MaxLines=""2""
                        LineBreakMode=""TailTruncation"" />
                </VerticalStackLayout>
                <!-- Count -->              
                <Border HeightRequest=""20""
                    WidthRequest=""20""
                    StrokeShape=""RoundRectangle 10""
                    Grid.Row=""0""
                    Grid.Column=""2""
                    VerticalOptions=""Center""
                    StyleClass=""bg-info-strong"">
                    <Label Text=""{{ ConnectionRequestCounts[type.Id].AssignedToYouCount }}""
                        StyleClass=""text-interface-softer, caption2""
                        HorizontalOptions=""Center""
                        VerticalOptions=""Center"" />
                </Border>
                
                <!-- Divider -->
                {% unless forloop.last %}
                    <Rock:Divider Grid.Row=""1"" 
                        Grid.Column=""0"" 
                        Grid.ColumnSpan=""3""
                        VerticalOptions=""Center""
                        StyleClass=""my-8"" />
                {% endunless %}
                {% if DetailPage %}
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionTypeGuid={{ type.Guid }}"" />
                    </Grid.GestureRecognizers>
                {% endif %}
            </Grid>
        {% endfor %}
    </VerticalStackLayout>    
</Rock:StyledBorder>";
        private const string _connectionTypeListLegacyTemplate = @"<StackLayout Spacing=""0"">
    {% for type in ConnectionTypes %}    
        <Frame StyleClass=""connection-type""
            HasShadow=""false"">
            <Grid ColumnDefinitions=""50,*,Auto""
                RowDefinitions=""Auto,Auto""
                RowSpacing=""0"">
                {% if type.IconCssClass != null and type.IconCssClass != '' %}
                    <Rock:Icon StyleClass=""connection-type-icon""
                        IconClass=""{{ type.IconCssClass | Replace:'fa fa-','' }}""
                        HorizontalOptions=""Center""
                        VerticalOptions=""Center""
                        Grid.RowSpan=""2"" />
                {% endif %}
                
                <Label StyleClass=""connection-type-name""
                    Text=""{{ type.Name | Escape }}""
                    Grid.Column=""1"" />
                <Label StyleClass=""connection-type-description""
                    Text=""{{ type.Description | Escape }}""
                    MaxLines=""2""
                    LineBreakMode=""TailTruncation""
                    Grid.Column=""1""
                    Grid.Row=""1""
                    Grid.ColumnSpan=""2"" />
                <Rock:Tag StyleClass=""connection-type-count""
                    Text=""{{ ConnectionRequestCounts[type.Id].AssignedToYouCount }}""
                    Type=""info""
                    Grid.Column=""2"" />
            </Grid>
            {% if DetailPage != null %}            
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionTypeGuid={{ type.Guid }}"" />
                </Frame.GestureRecognizers>
            {% endif %}
        </Frame>
    {% endfor %}
</StackLayout>";
        private const string _connectionOpportunityListTemplate = @"<Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"">
    <VerticalStackLayout>
        {% assign size = ConnectionOpportunities | Size %}
        {% if size == 0 %}
            <Label Text=""No Opportunities Found""
                StyleClass=""body, text-interface-stronger"" />
        {% endif %}
        {% for opportunity in ConnectionOpportunities %}    
            <Grid RowDefinitions=""64, Auto""
                ColumnDefinitions=""Auto, *, 20""
                StyleClass=""gap-column-8"">
    
                <!-- Icon -->
                <Rock:Icon StyleClass=""text-interface-strong""
                    IconClass=""{{ opportunity.IconCssClass | Replace:'fa fa-','' }}""
                    FontSize=""32""
                    Grid.Row=""0""
                    Grid.Column=""0""
                    VerticalOptions=""Center""
                    HorizontalOptions=""Center"" />
                <!-- Name and Description -->
                <VerticalStackLayout Grid.Row=""0""
                    Grid.Column=""1""
                    VerticalOptions=""Center"">
                    <Label Text=""{{ opportunity.Name | Escape }}""
                        StyleClass=""body, bold, text-interface-stronger""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                    <Label Text=""{{ opportunity.Summary | StripHtml | Escape }}""
                        StyleClass=""footnote, text-interface-strong""
                        MaxLines=""2""
                        LineBreakMode=""TailTruncation"" />
                </VerticalStackLayout>
                <!-- Count -->              
                <Border HeightRequest=""20""
                    WidthRequest=""20""
                    StrokeShape=""RoundRectangle 10""
                    Grid.Row=""0""
                    Grid.Column=""2""
                    VerticalOptions=""Center""
                    StyleClass=""bg-info-strong"">
                    <Label Text=""{{ ConnectionRequestCounts[opportunity.Id].AssignedToYouCount }}""
                        StyleClass=""text-interface-softer, caption2""
                        HorizontalOptions=""Center""
                        VerticalOptions=""Center"" />
                </Border>
                
                <!-- Divider -->
                {% unless forloop.last %}
                    <Rock:Divider Grid.Row=""1"" 
                        Grid.Column=""0"" 
                        Grid.ColumnSpan=""3""
                        VerticalOptions=""Center""
                        StyleClass=""my-8"" />
                {% endunless %}
                {% if DetailPage %}
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionOpportunityGuid={{ opportunity.Guid }}"" />
                    </Grid.GestureRecognizers>
                {% endif %}
            </Grid>
        {% endfor %}
    </VerticalStackLayout>    
</Rock:StyledBorder>";
        private const string _connectionOpportunityListLegacyTemplate = @"<StackLayout Spacing=""0"">
    {% for opportunity in ConnectionOpportunities %}    
        <Frame StyleClass=""connection-opportunity""
            HasShadow=""false"">
            <Grid ColumnDefinitions=""50,*,Auto""
                RowDefinitions=""Auto,Auto""
                RowSpacing=""0"">
                {% if opportunity.IconCssClass != null and opportunity.IconCssClass != '' %}
                    <Rock:Icon StyleClass=""connection-opportunity-icon""
                        IconClass=""{{ opportunity.IconCssClass | Replace:'fa fa-','' }}""
                        HorizontalOptions=""Center""
                        VerticalOptions=""Center""
                        Grid.RowSpan=""2"" />
                {% endif %}
                
                <Label StyleClass=""connection-opportunity-name""
                    Text=""{{ opportunity.Name | Escape }}""
                    Grid.Column=""1"" />
                <Label StyleClass=""connection-opportunity-description""
                    Text=""{{ opportunity.Summary | StripHtml | Escape }}""
                    MaxLines=""2""
                    LineBreakMode=""TailTruncation""
                    Grid.Column=""1""
                    Grid.Row=""1""
                    Grid.ColumnSpan=""2"" />
                <Rock:Tag StyleClass=""connection-opportunity-count""
                    Text=""{{ ConnectionRequestCounts[opportunity.Id].AssignedToYouCount }}""
                    Type=""info""
                    Grid.Column=""2"" />
            </Grid>
            {% if DetailPage != null %}            
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionOpportunityGuid={{ opportunity.Guid }}"" />
                </Frame.GestureRecognizers>
            {% endif %}
        </Frame>
    {% endfor %}
</StackLayout>";
        private const string _connectionRequestListTemplate = @"<Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"">
    <VerticalStackLayout>
        {% assign size = ConnectionRequests | Size %}
        {% if size == 0 %}
            <Label Text=""No Connection Requests Found""
                StyleClass=""body, text-interface-stronger"" />
        {% endif %}
        {% for request in ConnectionRequests %}
            {% assign person = request.PersonAlias.Person %}
            <Grid RowDefinitions=""64, Auto""
                ColumnDefinitions=""Auto, *""
                StyleClass=""gap-column-8"">
    
                <!-- Avatar -->
                <Rock:Avatar Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ person.PhotoUrl | Append:'&width=200' | Escape }}""
                    Grid.Row=""0""
                    Grid.Column=""0""
                    VerticalOptions=""Center""
                    ShowStroke=""false""
                    HorizontalOptions=""Center"" />
                <!-- Name and date are inline -->
                <Grid ColumnDefinitions=""*, Auto""
                    RowDefinitions=""Auto, *""
                    Grid.Row=""0""
                    Grid.Column=""1""
                    VerticalOptions=""Center"">
                    <Label Text=""{{ person.FullName | Escape }}""
                        StyleClass=""body, bold, text-interface-stronger""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                    <!-- Date -->
                    <HorizontalStackLayout Grid.Column=""1""
                        Spacing=""4""
                        VerticalOptions=""Start"">
                        <Label StyleClass=""connection-request-date, caption2, text-interface-medium""
                            Text=""{{ request.CreatedDateTime | Date:'sd' }}""
                            VerticalOptions=""Start"" />
                        <Rock:Icon IconClass=""chevron-right""
                            StyleClass=""text-interface-medium, caption2""
                            VerticalOptions=""Start"" />
                    </HorizontalStackLayout>
                    
                    <Label Text=""{{ request.Comments | Default:'' | Escape }}""
                        Grid.Row=""1""
                        Grid.ColumnSpan=""2""
                        StyleClass=""footnote, text-interface-strong""
                        MaxLines=""2""
                        LineBreakMode=""TailTruncation"" />
                </Grid>
                <!-- Divider -->
                {% unless forloop.last %}
                    <Rock:Divider Grid.Row=""1"" 
                        Grid.Column=""0"" 
                        Grid.ColumnSpan=""3""
                        VerticalOptions=""Center""
                        StyleClass=""my-8"" />
                {% endunless %}
                {% if DetailPage %}
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionRequestGuid={{ request.Guid }}"" />
                    </Grid.GestureRecognizers>
                {% endif %}
            </Grid>
        {% endfor %}
    </VerticalStackLayout>
</Rock:StyledBorder>";
        private const string _connectionRequestListLegacyTemplate = @"<StackLayout Spacing=""0"">
    {% for request in ConnectionRequests %}
        {% assign person = request.PersonAlias.Person %}
        <Frame StyleClass=""connection-request""
            HasShadow=""false"">
            <Grid ColumnDefinitions=""50,*,Auto""
                RowDefinitions=""Auto,Auto""
                RowSpacing=""0"">
                <Rock:Image StyleClass=""connection-request-image""
                    Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if person.PhotoId != null %}{{ person.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ person.PhotoUrl | Escape }}{% endif %}""
                    HorizontalOptions=""Center""
                    VerticalOptions=""Center""
                    Grid.RowSpan=""2"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <Label StyleClass=""connection-request-name""
                    Text=""{{ person.FullName | Escape }}""
                    Grid.Column=""1"" />
                <Label Text=""{{ request.Comments | Default:'' | Escape }}""
                    MaxLines=""2""
                    LineBreakMode=""TailTruncation""
                    StyleClass=""connection-request-description""
                    Grid.Column=""1""
                    Grid.Row=""1""
                    Grid.ColumnSpan=""2"" />
                <Label StyleClass=""connection-request-date""
                    Text=""{{ request.CreatedDateTime | Date:'sd' }}""
                    Grid.Column=""2"" />
            </Grid>
            {% if DetailPage != null %}
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?ConnectionRequestGuid={{ request.Guid }}"" />
                </Frame.GestureRecognizers>
            {% endif %}
        </Frame>
    {% endfor %}
</StackLayout>";

        #endregion

        #region Core Blocks

        private string _searchTemplate = @"{% assign photoUrl = null %}
{% assign itemName = null %}
{% assign itemText = null %}

//- Person is the only entity type supported out of the box. Otherwise just show the entity name.
{% if ItemType == 'Person' %}
    {% capture photoUrl %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ Item.PhotoUrl | Append:'&width=200' | Escape }}{% endcapture %}
    {% assign itemName = Item.FullName %}
    {% assign itemText = Item.Email %}
{% else %}
    {% assign itemName = Item | AsString %}
{% endif %}

//- Render the result item.
<Grid RowDefinitions=""40""
    ColumnDefinitions=""32, *, Auto""
    StyleClass=""gap-column-8"">
    <Rock:Avatar Source=""{{ photoUrl | Escape }}""
        Grid.Row=""0""
        Grid.Column=""0""
        VerticalOptions=""Center""
        ShowStroke=""false""
        HorizontalOptions=""Center""
        HeightRequest=""32""
        WidthRequest=""32"" />
    
    <VerticalStackLayout Grid.Row=""0""
        Grid.Column=""1""
        VerticalOptions=""Center"">
        <Label Text=""{{ itemName | Escape }}""
            StyleClass=""body, bold, text-interface-stronger""
            MaxLines=""1""
            VerticalTextAlignment=""Center""
            LineBreakMode=""TailTruncation"" />

        {% if itemText != null and itemText != """" %}
            <Label StyleClass=""footnote, text-interface-strong"" 
                MaxLines=""1"" 
                LineBreakMode=""TailTruncation"">{{ itemText |  XamlWrap  }}</Label>
        {% endif %}
    </VerticalStackLayout>

    <Rock:Icon IconClass=""chevron-right""
        VerticalTextAlignment=""Center""
        Grid.Column=""2"" 
        StyleClass=""text-interface-medium, body"" />

    <Grid.GestureRecognizers>
        <TapGestureRecognizer Command=""{Binding AggregateCommand}"">
            <TapGestureRecognizer.CommandParameter>
                <Rock:AggregateCommandParameters>
                    //- First command navigates to the configured page.
                    <Rock:CommandReference Command=""{Binding {{ DetailNavigationActionType }}}"" CommandParameter=""{{ DetailNavigationActionPage }}?{{ ItemType | Append:'Guid' }}={{ Item.Guid }}"" />

                    /- Second command appends the item to the search history.
                    / The parameters stored here become available to the 
                    / Historical Result Item Template. -/
                    <Rock:CommandReference Command=""{Binding AppendSearchHistory}"">
                        <Rock:CommandReference.CommandParameter>
                            <Rock:AppendToSearchHistoryParameters Guid=""{{ Item.Guid }}""
                                DetailKey=""{{ ItemType | Append:'Guid' }}"" >
                                <Rock:Parameter Name=""Name"" Value=""{{ itemName }}"" />
                                
                                {% if photoUrl != null and photoUrl != """" %}
                                <Rock:Parameter Name=""PhotoUrl"" Value=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl | Escape }}"" />
                                {% endif %}
                                <Rock:Parameter Name=""Text"" Value=""{{ itemText | Escape  }}"" />
                            </Rock:AppendToSearchHistoryParameters>
                        </Rock:CommandReference.CommandParameter>
                    </Rock:CommandReference>
                </Rock:AggregateCommandParameters>
            </TapGestureRecognizer.CommandParameter>
        </TapGestureRecognizer>
    </Grid.GestureRecognizers>
</Grid>";
        private string _searchLegacyTemplate = @"{% assign photoUrl = null %}
{% assign itemName = null %}
{% assign itemText = null %}

{% if ItemType == 'Person' %}
    {% capture photoUrl %}{% if Item.PhotoId != null %}{{ Item.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ Item.PhotoUrl | Escape }}{% endif %}{% endcapture %}
    {% assign itemName = Item.FullName %}
    {% assign itemText = Item.Email %}
{% else %}
    {% assign itemName = Item | AsString %}
{% endif %}

<StackLayout
    Spacing=""0"">
    <Frame StyleClass=""search-item-container"" 
        Margin=""0""
        BackgroundColor=""White""
        HasShadow=""false""
        HeightRequest=""40"">
            <StackLayout Orientation=""Horizontal""
                Spacing=""0""
                VerticalOptions=""Center"">
                <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl }}""
                    StyleClass=""search-image""
                    VerticalOptions=""Start""
                    Aspect=""AspectFit""
                    Margin=""0, 4, 8, 0""
                    BackgroundColor=""#e4e4e4"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <StackLayout Spacing=""0"" 
                    HorizontalOptions=""FillAndExpand"">
                    <StackLayout Orientation=""Horizontal""
                    VerticalOptions=""Center"">
                        <Label StyleClass=""search-item-name""
                            Text=""{{ itemName }}""
                            LineBreakMode=""TailTruncation""
                            HorizontalOptions=""FillAndExpand"" />

                        <Grid ColumnSpacing=""4"" 
                            RowSpacing=""0""
                            ColumnDefinitions=""*, Auto""
                            VerticalOptions=""Start"">

                            <Rock:Icon IconClass=""chevron-right""
                                VerticalTextAlignment=""Start""
                                Grid.Column=""1"" 
                                StyleClass=""note-read-more-icon""
                                />
                        </Grid>
                    </StackLayout>
                    {% if itemText == null or itemText == """" %}
                        {% assign itemText = ""No Email"" %}
                    {% endif %}
                        <Label StyleClass=""search-item-text""
                            Grid.Column=""0""
                            MaxLines=""2""
                            LineBreakMode=""TailTruncation"">{{ itemText | XamlWrap }}</Label> 
                </StackLayout>
            </StackLayout>
        </Frame>
    <BoxView HorizontalOptions=""FillAndExpand""
        HeightRequest=""1""
        Color=""#cccccc"" />

    <StackLayout.GestureRecognizers>
        <TapGestureRecognizer Command=""{Binding AggregateCommand}"">
            <TapGestureRecognizer.CommandParameter>
                <Rock:AggregateCommandParameters>
                    <Rock:CommandReference Command=""{Binding {{ DetailNavigationActionType }}}"" CommandParameter=""{{ DetailNavigationActionPage }}?{{ ItemType | Append:'Guid' }}={{ Item.Guid }}"" />
                    <Rock:CommandReference Command=""{Binding AppendSearchHistory}"">
                        <Rock:CommandReference.CommandParameter>
                            <Rock:AppendToSearchHistoryParameters Guid=""{{ Item.Guid }}""
                                DetailKey=""{{ ItemType | Append:'Guid' }}"" >
                                <Rock:Parameter Name=""Name"" Value=""{{ itemName }}"" />
                                
                                {% if photoUrl != null and photoUrl != """" %}
                                <Rock:Parameter Name=""PhotoUrl"" Value=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl }}"" />
                                {% endif %}
                                <Rock:Parameter Name=""Text"" Value=""{{ itemText }}"" />
                            </Rock:AppendToSearchHistoryParameters>
                        </Rock:CommandReference.CommandParameter>
                    </Rock:CommandReference>
                </Rock:AggregateCommandParameters>
            </TapGestureRecognizer.CommandParameter>
        </TapGestureRecognizer>
    </StackLayout.GestureRecognizers>
</StackLayout>";

        private string _notesTemplate = @"<StackLayout StyleClass=""spacing-8"">
    //- Header
    <Grid ColumnDefinitions=""*, Auto"">
        <Label Text=""Notes""
            StyleClass=""title2, bold, text-interface-strongest""
            Grid.Column=""0"" />

        <Rock:IconButton Grid.Column=""1""
            IconClass=""fa fa-ellipsis-h""
            FontSize=""14""
            CornerRadius=""12""
            IconFamily=""FontAwesome""
            HeightRequest=""24""
            WidthRequest=""24""
            Rock:ContextMenu.ShowMenuOnClick=""True""
            StyleClass=""btn, btn-outline-primary, bg-interface-softer, border-2, p-0"">
            <Rock:ContextMenu.Menu>
                <DataTemplate>
                    <Rock:Menu>
                        <Rock:MenuGroup Title=""Note Actions"">
                            <Rock:MenuAction Title=""View All"" 
                                Command=""{Binding ShowAllNotes}""
                                SystemIcon=""{OnPlatform iOS=square.grid.2x2}"" />

                            <Rock:MenuAction Title=""Add Note"" 
                                Command=""{Binding AddNote}""
                                SystemIcon=""{OnPlatform iOS=plus}"" />
                        </Rock:MenuGroup>
                    </Rock:Menu>
                </DataTemplate>
            </Rock:ContextMenu.Menu>
        </Rock:IconButton>
    </Grid>

    //- Content
    <Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"" 
        VerticalOptions=""Start"">
        <VerticalStackLayout>
            {% assign size = Notes | Size %}
            {% if size == 0 %}
                <Label Text=""No Notes Found""
                    StyleClass=""body, text-interface-stronger"" />
            {% endif %}
            {% for note in Notes %}
                <Grid RowDefinitions=""64, Auto""
                    ColumnDefinitions=""Auto, *""
                    StyleClass=""gap-column-8"">
        
                    <!-- Avatar -->
                    <Rock:Avatar Source=""{{ note.PhotoUrl | Escape }}""
                        Grid.Row=""0""
                        Grid.Column=""0""
                        VerticalOptions=""Center""
                        ShowStroke=""true""
                        HorizontalOptions=""Center"" />
                    <!-- Name and date are inline -->
                    <Grid ColumnDefinitions=""*, Auto""
                        RowDefinitions=""Auto, *""
                        Grid.Row=""0""
                        Grid.Column=""1""
                        VerticalOptions=""Center"">
                        <Label Text=""{{ note.Name | Escape }}""
                            StyleClass=""body, bold, text-interface-stronger""
                            MaxLines=""1""
                            LineBreakMode=""TailTruncation"" />
                        <!-- Date -->
                        <HorizontalStackLayout Grid.Column=""1""
                            Spacing=""4""
                            VerticalOptions=""Start"">
                            <Label StyleClass=""caption2, text-interface-medium""
                                Text=""{{ note.Date | Date:'sd' }}""
                                VerticalOptions=""Start"" />
                            <Rock:Icon IconClass=""chevron-right""
                                StyleClass=""text-interface-medium, caption2""
                                VerticalOptions=""Start"" />
                        </HorizontalStackLayout>
                        
                        <Label Text=""{{ note.Text | StripHtml | Escape }}""
                            Grid.Row=""1""
                            Grid.ColumnSpan=""2""
                            StyleClass=""footnote, text-interface-strong""
                            MaxLines=""2""
                            LineBreakMode=""TailTruncation"" />
                    </Grid>
                    <!-- Divider -->
                    {% unless forloop.last %}
                        <Rock:Divider Grid.Row=""1"" 
                            Grid.Column=""0"" 
                            Grid.ColumnSpan=""3""
                            VerticalOptions=""Center""
                            StyleClass=""my-8"" />
                    {% endunless %}
                    {% if DetailPage %}
                        <Grid.GestureRecognizers>
                            <TapGestureRecognizer Command=""{Binding EditNote}"" CommandParameter=""{{ note.Guid }}"" />
                        </Grid.GestureRecognizers>
                    {% endif %}
                </Grid>
            {% endfor %}
        </VerticalStackLayout>
    </Rock:StyledBorder>
</StackLayout>";
        private string _notesLegacyTemplate = @"<StackLayout StyleClass=""notes-container"" Spacing=""0"">
    <FlexLayout JustifyContent=""SpaceBetween""
        StyleClass=""px-12"">
        <Label StyleClass=""h3""
            Text=""Notes""
        />
        {% if ListPage != null %}
            <Button StyleClass=""btn,btn-link"" 
                Text=""See All""
                HorizontalOptions=""End""
                Padding=""0""
                Command=""{Binding PushPage}""
                CommandParameter=""{{ ListPage }}?PersonGuid={{ PageParameter.PersonGuid | Escape }}"">
            </Button>
        {% else %}
            <Button StyleClass=""btn,btn-link"" 
                Text=""See All""
                HorizontalOptions=""End""
                Padding=""0""
                Command=""{Binding ShowAllNotes}"">
            </Button>
        {% endif %}
    </FlexLayout>

    {% for note in Notes %}
            <Frame Margin=""0""
                BackgroundColor=""White""
                HasShadow=""false""
                Padding=""12""
                HeightRequest=""64""
                StyleClass=""note-container"">
                <StackLayout Orientation=""Horizontal""
                    Spacing=""0"">
                    <Rock:Image StyleClass=""note-author-image""
                        VerticalOptions=""Start""
                        Aspect=""AspectFit""
                        Margin=""0, 4, 14, 0""
                        BackgroundColor=""#e4e4e4""
                        Source=""{{ note.PhotoUrl | Escape}}"">
                        <Rock:CircleTransformation />
                    </Rock:Image>
                    
                    <StackLayout Spacing=""0"" 
                        HorizontalOptions=""FillAndExpand"">
                        <StackLayout Orientation=""Horizontal"">
                            <Label StyleClass=""note-author""
                                Text=""{{ note.Name | Escape }}""
                                LineBreakMode=""TailTruncation""
                                HorizontalOptions=""FillAndExpand"" />

                            <Grid ColumnSpacing=""4"" 
                                RowSpacing=""0""
                                ColumnDefinitions=""*, Auto""
                            >
                            {% if note.Date != null %}
                                <Label StyleClass=""note-date""
                                    Text=""{{ note.Date | HumanizeDateTime }}""
                                    HorizontalTextAlignment=""End"" 
                                    HorizontalOptions=""End""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""0""  />
                            {% endif %}
					{% if note.CanEdit %}
                                <Rock:Icon IconClass=""chevron-right""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""1"" 
                                    StyleClass=""note-read-more-icon""
                                    />
					{% endif %}
                            </Grid>
                        </StackLayout>
                            <Label StyleClass=""note-text""
                                Grid.Column=""0""
                                MaxLines=""2""
                                LineBreakMode=""TailTruncation"" 
                                Text=""{{ note.Text | StripHtml | Escape }}"" /> 

                    </StackLayout>
                </StackLayout>
                {% if note.CanEdit %}
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding EditNote}"" 
                     CommandParameter=""{{ note.Guid }}"" />
                </Frame.GestureRecognizers>
                {% endif %}
            </Frame>
        <BoxView HorizontalOptions=""FillAndExpand""
            HeightRequest=""1""
            Color=""#cccccc""
        />
    {% endfor %}
    
    {% if CurrentPerson != null %}
        <FlexLayout JustifyContent=""SpaceBetween""
            StyleClass=""px-12"">
            <ContentView />
            <Rock:Icon 
                HorizontalOptions=""End""
                IconClass=""plus""
                HorizontalTextAlignment=""End""
                StyleClass=""btn,btn-link""
                Padding=""0, 4, 0, 0""
                VerticalTextAlignment=""Center"">
                    <Rock:Icon.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding AddNote}"" />
                    </Rock:Icon.GestureRecognizers>
            </Rock:Icon>
        </FlexLayout>
    {% endif %}
</StackLayout>";

        #endregion

        #region Crm Blocks

        private const string _groupMembersTemplate = @"<StackLayout StyleClass=""spacing-24"">
    {% for group in Groups %}
        <StackLayout StyleClass=""spacing-8"">
            <Label Text=""{{ group.Group.Name | Escape }}"" 
                StyleClass=""h3, title2, bold, text-interface-stronger"" />
            <Rock:StyledBorder StyleClass=""border, border-interface-soft, rounded, bg-interface-softest, p-16"">
                <Grid ColumnDefinitions=""*, Auto"">
                    <StackLayout Orientation=""Horizontal"" StyleClass=""spacing-8"">
                        {% for member in group.Group.Members %}
                            {% if Person.Id != member.Person.Id %}
                                <VerticalStackLayout>
                                    {% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}
                                    {% assign photoUrl = publicApplicationRoot | Append:member.Person.PhotoUrl %}
                                    
                                    <Rock:Avatar Source=""{{ photoUrl | Escape }}"" 
                                        StyleClass=""h-48"" />
        
                                    <Label Text=""{{ member.Person.NickName }}""
                                        HorizontalTextAlignment=""Center""
                                        StyleClass=""body, text-interface-strong"" />
                                </VerticalStackLayout>
                            {% else %}
                                <ContentView />
                            {% endif %}
                        {% endfor %}
                    </StackLayout>
                    {% if EditPage and group.CanEdit %}
                        <Rock:Icon Grid.Column=""1""
                            IconClass=""chevron-right"" 
                            VerticalOptions=""Center""
                            StyleClass=""caption1, text-interface-medium"" /> 
                    {% endif %}
                </Grid>
                {% if EditPage and group.CanEdit %}
                    <Rock:StyledBorder.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding PushPage}""
                            CommandParameter=""{{ EditPage }}?GroupGuid={{ group.Group.Guid }}"" />
                    </Rock:StyledBorder.GestureRecognizers>
                {% endif %}
            </Rock:StyledBorder>
        </StackLayout>
    {% endfor %}
</StackLayout>";

        private const string _groupMembersLegacyTemplate = @"{% for group in Groups %}
    <StackLayout>
        <Label Text=""{{ group.Group.Name }}"" 
            StyleClass=""h3"" />
        
        <Frame StyleClass=""p-0""
            HasShadow=""False"">
            <Grid
                ColumnDefinitions=""*, 32"">
                <ScrollView Orientation=""Horizontal""
                    Grid.Column=""0""
                    HorizontalScrollBarVisibility=""Never"">
                    <StackLayout Orientation=""Horizontal"">
                      {% for member in group.Group.Members %}
                        <!-- We want to exclude the actual person from this list -->
                        {% if Person.Id != member.Person.Id %}
                            <StackLayout>
                            {% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}
                            {% assign photoUrl = publicApplicationRoot | Append:member.Person.PhotoUrl %}
                            <Rock:Image Source=""{{ photoUrl | Escape }}""
                                WidthRequest=""48""
                                HeightRequest=""48"">
                                <Rock:CircleTransformation />
                            </Rock:Image>
                            <Label Text=""{{ member.Person.NickName }}""
                                HorizontalTextAlignment=""Center"" />
                        </StackLayout>
                        {% else %}
                            <ContentView />
                        {% endif %}
                      {% endfor %}
                    {% if EditPage and group.CanEdit %}
                    <StackLayout.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding PushPage}""
                            CommandParameter=""{{ EditPage }}?GroupGuid={{ group.Group.Guid }}"" />
                    </StackLayout.GestureRecognizers>
                    {% endif %}
                    </StackLayout>
                </ScrollView>
                {% if EditPage and group.CanEdit %}
                    <Rock:Icon Grid.Column=""1""
                        IconClass=""chevron-right""
                        WidthRequest=""32""
                        HeightRequest=""32"" 
                        VerticalTextAlignment=""Center""
                        HorizontalTextAlignment=""End"" /> 
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding PushPage}""
                            CommandParameter=""{{ EditPage }}?GroupGuid={{ group.Group.Guid }}"" />
                    </Grid.GestureRecognizers>
                {% endif %}
            </Grid>
        </Frame>
    </StackLayout>
{% endfor %}";

        #endregion 

        #region Group Blocks

        private const string _scheduleToolboxTemplate = @"<Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"">
    <VerticalStackLayout>
        {% if ScheduleList == empty %}
            <Label Text=""You currently have no pending or confirmed schedules. Reach out to a church administrator if you are interested!""
                StyleClass=""body, text-interface-stronger"" />
        {% endif %}
        {% for attendance in ScheduleList %}
            {% assign status = attendance.GroupScheduleType %}
            <Grid RowDefinitions=""64, Auto""
                ColumnDefinitions=""*, Auto"">
                <VerticalStackLayout VerticalOptions=""Center"">
                    <Label StyleClass=""body, bold, text-interface-stronger""
                        Text=""{{ attendance.OccurrenceStartDate | Date:'sd' }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                    {% capture title %}{{ attendance.Group.Name }}{% if attendance.Location.Name and attendance.Location.Name != '' %} - {{ attendance.Location.Name }}{% endif %}{% endcapture %}
                    <Label StyleClass=""footnote, text-interface-strong""
                        Text=""{{ title | Escape }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                    <Label StyleClass=""footnote, text-interface-strong""
                        Text=""{{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                </VerticalStackLayout>
                {% if status == 'Upcoming' %}
                    <StackLayout Spacing=""8"" 
                        Grid.Column=""1"" 
                        StyleClass=""spacing-8""
                        Orientation=""Horizontal"">
        
                        <Label StyleClass=""footnote, text-success-strong""
                            VerticalOptions=""Center""
                            Text=""Confirmed"" />
                        <Rock:Icon IconClass=""ellipsis-v"" 
                            VerticalOptions=""Center""
                            StyleClass=""text-interface-stronger, footnote""
                            Command=""{Binding ShowActionPanel}"">
                            <Rock:Icon.CommandParameter>
                                <Rock:ShowActionPanelParameters 
                                    Title=""{{ attendance.OccurrenceStartDate | Date:'sd' }} - {{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                                    CancelTitle=""Exit"">
                                    <Rock:ActionPanelButton Title=""Cancel Confirmation"" 
                                        Command=""{Binding ScheduleToolbox.SetPending}"" 
                                        CommandParameter=""{{ attendance.Guid }}"" />
                                </Rock:ShowActionPanelParameters>
                            </Rock:Icon.CommandParameter>
                        </Rock:Icon>
                    </StackLayout>
                {% elseif status == 'Pending' %}
                    <StackLayout Spacing=""8"" 
                        Grid.Column=""1"" 
                        StyleClass=""spacing-8""
                        HorizontalOptions=""Center""
                        Orientation=""Horizontal"">
                        <Button StyleClass=""btn, btn-success"" 
                            Text=""Accept""
                            Command=""{Binding ScheduleToolbox.ConfirmAttend}""
                            HorizontalOptions=""Center""
                            CommandParameter=""{{ attendance.Guid }}"" />
    
                        <Button StyleClass=""btn, btn-outline-danger"" 
                            Text=""Decline""              
                            Command=""{Binding ScheduleToolbox.PushScheduleConfirmModal}""
                            HorizontalOptions=""Center""
                            CommandParameter=""{{ attendance.Guid }}"" />
                    </StackLayout>
                {% elseif status == ""Unavailable"" %}
                    <StackLayout Spacing=""8"" 
                        Grid.Column=""1"" 
                        StyleClass=""spacing-8""
                        Orientation=""Horizontal"">
        
                        <Label StyleClass=""footnote, text-danger-strong""
                            VerticalOptions=""Center""
                            Text=""Declined"" />
                        <Rock:Icon IconClass=""ellipsis-v"" 
                            VerticalOptions=""Center""
                            StyleClass=""text-interface-stronger, footnote""
                            Command=""{Binding ShowActionPanel}"">
                            <Rock:Icon.CommandParameter>
                                <Rock:ShowActionPanelParameters 
                                    Title=""{{ attendance.OccurrenceStartDate | Date:'sd' }} - {{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                                    CancelTitle=""Exit"">
                                    <Rock:ActionPanelButton Title=""Cancel Declination"" 
                                        Command=""{Binding ScheduleToolbox.SetPending}"" 
                                        CommandParameter=""{{ attendance.Guid }}"" />
                                </Rock:ShowActionPanelParameters>
                            </Rock:Icon.CommandParameter>
                        </Rock:Icon>
                    </StackLayout>
                {% endif %}
                <!-- Divider -->
                {% unless forloop.last %}
                    <Rock:Divider Grid.Row=""1"" 
                        Grid.Column=""0"" 
                        Grid.ColumnSpan=""2""
                        VerticalOptions=""Center""
                        StyleClass=""my-8"" />
                {% endunless %}
            </Grid>
        {% endfor %}
    </VerticalStackLayout>
</Rock:StyledBorder>";
        private const string _scheduleToolboxLegacyTemplate = @"<StackLayout StyleClass=""schedule-toolbox"">
        {% if ScheduleList == empty %}
        
        <Rock:NotificationBox Text=""You currently have no pending or confirmed schedules. Reach out to a church administrator if you are interested!"" />
    
        {% endif %}
        {% for attendance in ScheduleList %}
        {% assign status = attendance.GroupScheduleType %}
        
        <Grid ColumnSpacing=""12"" Padding=""8"">
            <StackLayout Spacing=""4"" Grid.Column=""0""  StyleClass=""schedule-toolbox-container"">
                <Label  StyleClass=""detail-title""
                        Text=""{{ attendance.OccurrenceStartDate | Date:'sd' }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
            <Label StyleClass=""detail""
                        Text=""{{ attendance.Group.Name | Escape }} - {{ attendance.Location.Name }}""
                        MaxLines=""2""
                        LineBreakMode=""TailTruncation"" />
            <Label StyleClass=""detail""
                        Text=""{{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
            </StackLayout>
        {% if status == 'Upcoming' %}
            <StackLayout Spacing=""8"" Grid.Column=""1"" StyleClass=""schedule-toolbox-confirmations-container""
                    VerticalOptions=""Center""
                    HorizontalOptions=""End""
                    Orientation=""Horizontal"">
            <Label StyleClass=""confirmed-text""
                    HorizontalOptions=""Start"" VerticalOptions=""Center""
                    Padding=""8""
                    Text=""Confirmed"" />
            <Rock:Icon IconClass=""Ellipsis-v"" 
                    HorizontalOptions=""End""
                    VerticalOptions=""Center""
                    Padding=""8""
                    Command=""{Binding ShowActionPanel}"">
                    <Rock:Icon.CommandParameter>
                        <Rock:ShowActionPanelParameters 
                            Title=""{{ attendance.OccurrenceStartDate | | Date:'sd' }} - {{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                            CancelTitle=""Exit"">
                            <Rock:ActionPanelButton Title=""Cancel Confirmation"" 
                                Command=""{Binding ScheduleToolbox.SetPending}"" 
                                CommandParameter=""{{ attendance.Guid }}"" />
                        </Rock:ShowActionPanelParameters>
                    </Rock:Icon.CommandParameter>
                </Rock:Icon>
            </StackLayout>
        {% endif %}
        {% if status == 'Unavailable' %}
            <StackLayout Spacing=""8"" Grid.Column=""1"" StyleClass=""schedule-toolbox-confirmations-container""
                    VerticalOptions=""Center""
                    HorizontalOptions=""End""
                    Orientation=""Horizontal"">
            <Label StyleClass=""declined-text""
                    HorizontalOptions=""Start"" VerticalOptions=""Center""
                    Padding=""8""
                    Text=""Declined"" />
            <Rock:Icon IconClass=""Ellipsis-v"" 
                    HorizontalOptions=""End""
                    VerticalOptions=""Center""
                    Padding=""8""
                    Command=""{Binding ShowActionPanel}"">
                    <Rock:Icon.CommandParameter>
                        <Rock:ShowActionPanelParameters 
                            Title=""{{ attendance.OccurrenceStartDate | | Date:'sd' }} - {{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                            CancelTitle=""Exit"">
                            <Rock:ActionPanelButton Title=""Cancel Declination"" 
                                Command=""{Binding ScheduleToolbox.SetPending}"" 
                                CommandParameter=""{{ attendance.Guid }}"" />
                        </Rock:ShowActionPanelParameters>
                    </Rock:Icon.CommandParameter>
                </Rock:Icon>
            </StackLayout>
        {% endif %}
        {% if status == 'Pending' %}
                
            <StackLayout Spacing=""8"" Grid.Column=""1"" StyleClass=""schedule-toolbox-pending-container""
                    VerticalOptions=""Center""
                    HorizontalOptions=""End""
                    Orientation=""Horizontal"">
                    <Button StyleClass=""btn,btn-success,accept-button"" 
                        Text=""Accept""
                        Command=""{Binding ScheduleToolbox.ConfirmAttend}""
                        CommandParameter=""{{ attendance.Guid }}""
                        HorizontalOptions=""Fill"" />
                    <Button StyleClass=""btn,btn-outline-danger,decline-button"" 
                        Text=""Decline""              
                        Command=""{Binding ScheduleToolbox.PushScheduleConfirmModal}""
                        CommandParameter=""{{ attendance.Guid }}""
                        HorizontalOptions=""Fill""  />
            </StackLayout>
        {% endif %}
        </Grid>
        <Rock:Divider />
        {% endfor %}
</StackLayout>";

        private const string _scheduleToolboxDeclineReasonTemplate = @"<VerticalStackLayout>
    <Label StyleClass=""title2, bold, text-interface-strongest""
        Text=""Can't make it, {{ Attendance.PersonAlias.Person.NickName }}? "" />
    <Label StyleClass=""body, text-interface-stronger"" Grid.Row=""1""
        Text=""Thanks for letting us know. We’ll try to schedule another person for {{ Attendance.StartDateTime | Date:'dddd @ h:mmtt'}}."" />
</VerticalStackLayout>";
        private const string _scheduleToolboxDeclineReasonLegacyTemplate = @"<StackLayout Spacing=""0"">
    <Frame HasShadow=""False"">
        <Grid RowDefinitions=""1*, 1*, 1*"">
            <Label StyleClass=""h2, schedule-toolbox-confirm-title""
                Text=""Can't make it, {{ Attendance.PersonAlias.Person.NickName }}? "" />
            <Label StyleClass=""schedule-toolbox-confirm-title"" Grid.Row=""1""
                Text=""Thanks for letting us know. We’ll try to schedule another person for {{ Attendance.StartDateTime | Date:'dddd @ h:mmtt'}}."" />
        </Grid>
    </Frame>
</StackLayout>";

        private const string _schedulePreferenceLandingTemplate = @"<StackLayout StyleClass=""spacing-24"">
    <VerticalStackLayout>
        <Label StyleClass=""title1, bold, text-interface-strongest""
            Text=""Serving Areas"" />
    
        <Label StyleClass=""footnote, text-interface-strong""
            Text=""You are registered to serve in the following areas. Please select an area to update your scheduling preferences."" />
    </VerticalStackLayout>
    <StackLayout StyleClass=""spacing-8"">
        {% for group in SchedulingGroupList %}
            <Button StyleClass=""btn, btn-primary""
                Text=""{{ group.Name | Escape }}""
                Command=""{Binding SchedulePreference.ShowPreferences}"" 
                CommandParameter=""{{ group.Guid }}"" /> 
        {% endfor %} 
    </StackLayout>
</StackLayout>";
        private const string _schedulePreferenceLandingLegacyTemplate = @"<StackLayout>
<StackLayout Padding=""16"" HorizontalOptions=""Center"" VerticalOptions=""Center"">
    
    <Label StyleClass=""h3"" 
        Text=""Serving Areas""
        />
    <Label StyleClass=""""
        Text=""You are registered to serve in the following areas. Please select an area to update your scheduling preferences."" />
    {% if SchedulingGroupList == empty %}
        <Rock:NotificationBox Text=""You are currently not enrolled in any groups with scheduling options. Contact a church administrator if you are interested!"" />
    {% endif %}
    
    {% for group in SchedulingGroupList %}
    <Grid> 
        <Button StyleClass=""btn,btn-primary,group-selection-button"" Text=""{{ group.Name }}"" HorizontalOptions=""FillAndExpand""
            Command=""{Binding SchedulePreference.PushToPreferencePage}"" 
            CommandParameter=""{{ group.Guid }}"" /> 
    </Grid> 
    {% endfor %} 
</StackLayout>
</StackLayout>";

        private const string _scheduleSignUpLandingTemplate = @"<StackLayout StyleClass=""spacing-24"">
    <VerticalStackLayout>
        <Label StyleClass=""title1, bold, text-interface-strongest""
            Text=""Serving Areas"" />
    
        <Label StyleClass=""footnote, text-interface-strong""
            Text=""You are registered to serve in the following areas. Please select an area to update your scheduling preferences."" />
    </VerticalStackLayout>
    <StackLayout StyleClass=""spacing-8"">
        {% for group in SchedulingGroupList %}
            <Button StyleClass=""btn, btn-primary""
                Text=""{{ group.Name | Escape }}""
                Command=""{Binding ScheduleSignup.PushToSignupPage}""
                CommandParameter=""{{ group.Guid }}"" /> 
        {% endfor %} 
    </StackLayout>
</StackLayout>";
        private const string _scheduleSignUpLandingLegacyTemplate = @"<StackLayout>
<StackLayout Padding=""16"" HorizontalOptions=""Center"" VerticalOptions=""Center"">
    <Label StyleClass=""h3"" 
        Text=""Serving Areas""
        />
    <Label StyleClass=""""
        Text=""You are registered to serve in the following areas. Please select an area to update your scheduling preferences."" />
    {% if SchedulingGroupList == empty %}
        <Rock:NotificationBox Text=""You are currently not enrolled in any groups with scheduling options. Contact a church administrator if you are interested!"" />
    {% endif %}
    
    {% for group in SchedulingGroupList %}
    <Grid> 
        <Button StyleClass=""btn,btn-primary,group-selection-button"" Text=""{{ group.Name }}"" HorizontalOptions=""FillAndExpand""

            Command=""{Binding ScheduleSignup.PushToSignupPage}""
            CommandParameter=""{{ group.Guid }}"" />
    </Grid> 
    {% endfor %} 

</StackLayout>
</StackLayout>";

        private const string _scheduleUnavailabilityTemplate = @"<VerticalStackLayout Spacing=""8"">
    <Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"">
        <VerticalStackLayout>
            {% if ScheduleExclusionsList == empty %}
                <Label Text=""You currently have no dates excluded from your schedule.""
                    StyleClass=""body, text-interface-stronger"" />
            {% endif %}
    
            {% for exclusion in ScheduleExclusionsList %}
                {% if exclusion.Group %}
                    {% assign GroupName = exclusion.Group.Name %}
                {% else %} 
                    {% assign GroupName = ""All Groups"" %}
                {% endif %}
    
                <Grid RowDefinitions=""80, Auto""
                    ColumnDefinitions=""*, Auto"">
    
                    <VerticalStackLayout VerticalOptions=""Center"">
                        <Label StyleClass=""body, bold, text-interface-stronger""
                            Text=""{{ exclusion.OccurrenceStartDate | Date:'sd' }} - {{ exclusion.OccurrenceEndDate | Date:'sd' }}""
                            MaxLines=""1""
                            LineBreakMode=""TailTruncation"" />
    
                        <Label StyleClass=""footnote, text-interface-strong""
                            Text=""{{ exclusion.PersonAlias.Person.FullName }}""
                            MaxLines=""1""
                            LineBreakMode=""TailTruncation"" />
        
                        <Label StyleClass=""footnote, text-interface-strong""
                            Text=""{{ GroupName | Escape }}"" 
                            MaxLines=""1""
                            LineBreakMode=""TailTruncation"" />
    
                        {% if exclusion.Title %}
                            <Label StyleClass=""footnote, text-interface-strong""
                                Text=""&quot;{{ exclusion.Title | Escape }}&quot;"" 
                                MaxLines=""1""
                                LineBreakMode=""TailTruncation"" />
                        {% endif %}
                    </VerticalStackLayout>
    
                    
                    <Rock:Icon IconClass=""times"" 
                        StyleClass=""text-interface-stronger, body""
                        VerticalOptions=""Center""
                        Grid.Column=""1""
                        Command=""{Binding ShowActionPanel}"">
                        <Rock:Icon.CommandParameter>
                            <Rock:ShowActionPanelParameters 
                                Title=""{{ exclusion.OccurrenceStartDate | Date:'sd' }} - {{ exclusion.OccurrenceEndDate | Date:'sd' }}"" 
                                CancelTitle=""Exit"">
    
                                <Rock:ShowActionPanelParameters.DestructiveButton>
                                    <Rock:ActionPanelButton Title=""Cancel My Request"" 
                                        Command=""{Binding ScheduleUnavailability.DeleteScheduledUnavailability}""
                                        CommandParameter=""{{ exclusion.Guid }}"" />
                                </Rock:ShowActionPanelParameters.DestructiveButton>
                            </Rock:ShowActionPanelParameters>
                        </Rock:Icon.CommandParameter>
                    </Rock:Icon>
    
                    {% unless forloop.last %}
                        <Rock:Divider Grid.Row=""1"" 
                            Grid.Column=""0"" 
                            Grid.ColumnSpan=""2""
                            StyleClass=""my-8"" />
                    {% endunless %}
                </Grid>
            {% endfor %}
        </VerticalStackLayout>  
    </Rock:StyledBorder>
    <Button Text=""Schedule Unavailability""
        HorizontalOptions=""End"" 
        StyleClass=""btn, btn-primary, schedule-unavailabilty-button""       
        Command=""{Binding ScheduleUnavailability.PushScheduleUnavailabilityModal}"" />
</VerticalStackLayout>";
        private const string _scheduleUnavailabilityLegacyTemplate = @"<StackLayout StyleClass=""schedule-toolbox"">
    {% if ScheduleExclusionsList == empty %}
        
    <Rock:NotificationBox Text=""You currently have no blackout dates inputted."" />
    
    {% endif %}
    
    {% for attendance in ScheduleExclusionsList %}
        {% if attendance.Group %}
            {% assign GroupName = attendance.Group.Name %}
        {% else %} 
            {% assign GroupName = ""All Groups"" %}
        {% endif %}
        
        <Grid ColumnSpacing=""12"" Padding=""4"" Margin=""8"">
            <StackLayout Spacing=""4"" Grid.Column=""0""  StyleClass=""schedule-toolbox-container"">
                <Label  StyleClass=""detail-title""
                        Text=""{{ attendance.OccurrenceStartDate | Date:'sd' }} - {{ attendance.OccurrenceEndDate | Date:'sd' }} ""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                <Label StyleClass=""detail""
                        Text=""{{ attendance.PersonAlias.Person.FullName }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                <Label StyleClass=""detail""
                        Text=""{{ GroupName }}"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                {% if attendance.Title %}
                <Label StyleClass=""detail""
                        Text=""&quot;{{ attendance.Title }}&quot;"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                {% endif %}
            </StackLayout>
            <Rock:Icon IconClass=""times"" 
                    StyleClass=""""
                    HorizontalOptions=""End""
                    VerticalOptions=""Center""
                    Padding=""8""
                    Command=""{Binding ShowActionPanel}"">
                    <Rock:Icon.CommandParameter>
                        <Rock:ShowActionPanelParameters 
                            Title=""{{ attendance.OccurrenceStartDate | Date:'sd' }} - {{ attendance.OccurrenceEndDate | Date:'sd' }}"" 
                            CancelTitle=""Exit"">
                            <Rock:ShowActionPanelParameters.DestructiveButton>
                                <Rock:ActionPanelButton Title=""Cancel My Request"" 
                                    Command=""{Binding ScheduleUnavailability.DeleteScheduledUnavailability}""
                                    CommandParameter=""{{ attendance.Guid }}""
                                    />
                            </Rock:ShowActionPanelParameters.DestructiveButton>
                        </Rock:ShowActionPanelParameters>
                    </Rock:Icon.CommandParameter>
                </Rock:Icon>
        </Grid>
        <Rock:Divider />
        {% endfor %}
    <Button Margin=""8"" HorizontalOptions=""End"" StyleClass=""btn,btn-primary,schedule-unavailabilty-button""
                  Text=""Schedule Unavailability""
                  Command=""{Binding ScheduleUnavailability.PushScheduleUnavailabilityModal}"">
    </Button>
</StackLayout>";

        private const string _groupMemberListTemplate = @"<Rock:StyledBorder StyleClass=""bg-interface-softest, border, border-interface-soft, rounded, p-16"">
    <VerticalStackLayout>
        {% for member in Members %}
            <Grid RowDefinitions=""48, Auto""
                ColumnDefinitions=""Auto, *, Auto""
                StyleClass=""gap-column-8"">
                
                <Rock:Avatar Source=""{{ member.PhotoUrl | Escape }}""
                    HeightRequest=""48""
                    ShowStroke=""false""
                    Grid.Row=""0""
                    Grid.Column=""0""
                    VerticalOptions=""Center"" />
                <StackLayout Grid.Column=""1""
                    VerticalOptions=""Center"">
                    <Label StyleClass=""body, bold, text-interface-stronger""
                        Text=""{{ member.FullName }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                    <Label StyleClass=""footnote, text-interface-strong""
                        Grid.Column=""0""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" 
                        Text=""{{ member.GroupRole | Escape }}"" /> 
                </StackLayout>
                <Rock:Icon Grid.Column=""2""
                    IconClass=""chevron-right"" 
                    VerticalOptions=""Center""
                    StyleClass=""caption1, text-interface-medium"" /> 
                {% unless forloop.last %}
                    <Rock:Divider Grid.Row=""1"" 
                        Grid.ColumnSpan=""3"" 
                        StyleClass=""my-8""/>
                {% endunless %}
                <Grid.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" 
                     CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
                </Grid.GestureRecognizers>
            </Grid>
        {% endfor %}
    </VerticalStackLayout>
</Rock:StyledBorder>";
        private const string _groupMemberListLegacyTemplate = @"<StackLayout StyleClass=""members-container"" 
    Spacing=""0"">
    {% for member in Members %}
        <Frame StyleClass=""member-container"" 
            Margin=""0""
            BackgroundColor=""White""
            HasShadow=""false""
            HeightRequest=""40"">
                <StackLayout Orientation=""Horizontal""
                    Spacing=""0""
                    VerticalOptions=""Center"">
                    <Rock:Image Source=""{{ member.PhotoUrl | Escape }}""
                        StyleClass=""member-person-image""
                        VerticalOptions=""Start""
                        Aspect=""AspectFit""
                        Margin=""0, 4, 14, 0""
                        BackgroundColor=""#e4e4e4"">
                        <Rock:CircleTransformation />
                    </Rock:Image>
                    
                    <StackLayout Spacing=""0"" 
                        HorizontalOptions=""FillAndExpand"">
                        <StackLayout Orientation=""Horizontal""
                        VerticalOptions=""Center"">
                            <Label StyleClass=""member-name""
                                Text=""{{ member.FullName }}""
                                LineBreakMode=""TailTruncation""
                                HorizontalOptions=""FillAndExpand"" />
                            <Grid ColumnSpacing=""4"" 
                                RowSpacing=""0""
                                ColumnDefinitions=""*, Auto""
                                VerticalOptions=""Start"">
                                <Rock:Icon IconClass=""chevron-right""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""1"" 
                                    StyleClass=""note-read-more-icon""
                                    />
                            </Grid>
                        </StackLayout>
                            <Label StyleClass=""member-text""
                                Grid.Column=""0""
                                MaxLines=""2""
                                LineBreakMode=""TailTruncation"" 
                                Text=""{{ member.GroupRole | Escape }}"" /> 
                    </StackLayout>
                </StackLayout>
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" 
                     CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
                </Frame.GestureRecognizers>
            </Frame>
        <BoxView HorizontalOptions=""FillAndExpand""
            HeightRequest=""1""
            Color=""#cccccc"" />
    {% endfor %}
</StackLayout>";

        private const string _groupMemberViewTemplate = @"
<StackLayout StyleClass=""spacing-24"">
    <VerticalStackLayout>
        {% assign groupMemberCount = Member.Group.Members | Size %}
    
        <Label StyleClass=""title1, text-interface-strongest, bold"" 
            Text=""{{ Member.Group.Name | Escape }} Group"" />
    
        <Label StyleClass=""body, text-interface-strong"" 
            Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" /> 
    </VerticalStackLayout>
    <Grid ColumnDefinitions=""80, *""
        StyleClass=""gap-column-24"">
        {% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}
        {% assign photoUrl = publicApplicationRoot | Append:Member.Person.PhotoUrl %}
        <Rock:Avatar WidthRequest=""80""
            HeightRequest=""80""
            Source=""{{ photoUrl | Escape }}""
            ShowStroke=""false""
            VerticalOptions=""Center"" />
        <VerticalStackLayout VerticalOptions=""Center""
            Grid.Column=""1"">
            <Label StyleClass=""body, bold, text-interface-stronger""
                Text=""{{ Member.Person.FullName | Escape }}"" />
            {% if Member.Person.BirthDate != null %}
                <Label StyleClass=""body, text-interface-strong"">
                    <Label.FormattedText>
                        <FormattedString>
                                <Span Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                                <Span Text=""&#x0A;"" />
                                <Span Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                        </FormattedString>
                    </Label.FormattedText>
                </Label>
            {% endif %}
        </VerticalStackLayout>
    </Grid>
    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout ColumnSpacing=""0""
            RowSpacing=""8"">
            {% for attribute in VisibleAttributes %}
                {% if attribute.FormattedValue != '' %}
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Rock:FieldContainer>
                            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                        </Rock:FieldContainer>
                    </Rock:ResponsiveColumn>
                {% endif %}
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}
    <Rock:StyledBorder StyleClass=""bg-interface-softest, p-16, border, border-interface-soft, rounded"">
        <VerticalStackLayout>
            <!-- Email -->
            <Grid ColumnDefinitions=""*, Auto""
                RowDefinitions=""48, Auto"">
                {% assign hasEmail = false %}
                {% if Member.Person.Email != '' %}
                    {% assign hasEmail = true %}
                {% endif %}
                <VerticalStackLayout VerticalOptions=""Center"">
                    <Label Text=""Email""
                        StyleClass=""footnote, text-interface-strong"" />
    
                    <Label Text=""{% if hasEmail %}{{ Member.Person.Email | Escape }}{% else %}None{% endif %}"" 
                        StyleClass=""body, text-interface-stronger"" />
                </VerticalStackLayout>
    
                <Rock:Icon IconClass=""chevron-right""
                    StyleClass=""text-interface-medium, caption1""
                    Grid.Column=""1""
                    Grid.Row=""0""
                    VerticalOptions=""Center"" />
                {% if hasEmail %}
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding SendEmail}""
                            CommandParameter=""{{ Member.Person.Email | Escape }}""/>
                    </Grid.GestureRecognizers>
                {% endif %}
                <Rock:Divider Grid.Column=""0""
                    Grid.ColumnSpan=""2""
                    Grid.Row=""1""
                    StyleClass=""my-8"" />
            </Grid>
            <!-- Mobile -->
            <Grid ColumnDefinitions=""*, Auto""
                RowDefinitions=""48, Auto"">
                {% assign hasPhoneNumber = false %}
                {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
                {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
                {% if phoneNumber != '' and phoneNumber != null %}
                    {% assign hasPhoneNumber = true %}
                {% endif %}
                <VerticalStackLayout VerticalOptions=""Center"">
                    <Label Text=""Mobile""
                        StyleClass=""footnote, text-interface-strong"" />
    
                    <Label Text=""{% if hasPhoneNumber %}{{ phoneNumber }}{% else %}None{% endif %}"" 
                        StyleClass=""body, text-interface-stronger"" />
                </VerticalStackLayout>
    
                <Rock:Icon IconClass=""chevron-right""
                    StyleClass=""text-interface-medium, caption1""
                    Grid.Column=""1""
                    Grid.Row=""0""
                    VerticalOptions=""Center"" />
                {% if hasPhoneNumber %}
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding ShowActionPanel}"">
                            <TapGestureRecognizer.CommandParameter>
                                <Rock:ShowActionPanelParameters Title=""Mobile Number""
                                    CancelTitle=""Cancel"">
                                    <Rock:ActionPanelButton Title=""Call""
                                        Command=""{Binding CallPhoneNumber}""
                                        CommandParameter=""{{ phoneNumberLong }}"" />
                                    <Rock:ActionPanelButton Title=""Text""
                                        Command=""{Binding SendSms}""
                                        CommandParameter=""{{ phoneNumberLong }}"" />
                                </Rock:ShowActionPanelParameters>
                            </TapGestureRecognizer.CommandParameter>
                        </TapGestureRecognizer>
                    </Grid.GestureRecognizers>
                {% endif %}
                <Rock:Divider Grid.Column=""0""
                    Grid.ColumnSpan=""2""
                    Grid.Row=""1""
                    StyleClass=""my-8"" />
            </Grid>
            <!-- Home -->
            <Grid ColumnDefinitions=""*, Auto""
                RowDefinitions=""48, Auto"">
                {% assign hasPhoneNumber = false %}
                {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
                {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
                {% if phoneNumber != '' and phoneNumber != null %}
                    {% assign hasPhoneNumber = true %}
                {% endif %}
                <VerticalStackLayout VerticalOptions=""Center"">
                    <Label Text=""Home""
                        StyleClass=""footnote, text-interface-strong"" />
    
                    <Label Text=""{% if hasPhoneNumber %}{{ phoneNumber }}{% else %}None{% endif %}"" 
                        StyleClass=""body, text-interface-stronger"" />
                </VerticalStackLayout>
    
                <Rock:Icon IconClass=""chevron-right""
                    StyleClass=""text-interface-medium, caption1""
                    Grid.Column=""1""
                    Grid.Row=""0""
                    VerticalOptions=""Center"" />
                {% if hasPhoneNumber %}
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding ShowActionPanel}"">
                            <TapGestureRecognizer.CommandParameter>
                                <Rock:ShowActionPanelParameters Title=""Mobile Number""
                                    CancelTitle=""Cancel"">
                                    <Rock:ActionPanelButton Title=""Call""
                                        Command=""{Binding CallPhoneNumber}""
                                        CommandParameter=""{{ phoneNumberLong }}"" />
                                    <Rock:ActionPanelButton Title=""Text""
                                        Command=""{Binding SendSms}""
                                        CommandParameter=""{{ phoneNumberLong }}"" />
                                </Rock:ShowActionPanelParameters>
                            </TapGestureRecognizer.CommandParameter>
                        </TapGestureRecognizer>
                    </Grid.GestureRecognizers>
                {% endif %}
            </Grid>
        </VerticalStackLayout>
    </Rock:StyledBorder>
</StackLayout>";
        private const string _groupMemberViewLegacyTemplate = @"<StackLayout Spacing=""0"">
    {% assign groupMemberCount = Member.Group.Members | Size %}
    
    <Label StyleClass=""h1"" Text=""{{ Member.Group.Name | Escape }} Group"" />
    <Label StyleClass=""text"" Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" />
    <StackLayout Orientation=""Horizontal"" Spacing=""20"" Margin=""0, 20, 0, 40"">
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if Member.Person.PhotoId != null %}{{ Member.Person.PhotoUrl | Append:'&width=300' | Escape }}{% else %}{{ Member.Person.PhotoUrl | Escape }}{% endif %}"" WidthRequest=""80"">
                <Rock:CircleTransformation />
            </Rock:Image>
            <StackLayout Spacing=""0"" VerticalOptions=""Center"">
                <Label StyleClass=""h4"" Text=""{{ Member.Person.FullName | Escape }}"" />
                {% if Member.Person.BirthDate != null %}
                    <Label StyleClass=""text, o-60"" Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                    <Label StyleClass=""text, o-60"" Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                {% endif %}
            </StackLayout>
    </StackLayout>
    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout ColumnSpacing=""0"">
            {% for attribute in VisibleAttributes %}
                {% if attribute.FormattedValue != '' %}
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Rock:FieldContainer Margin=""0, 0, 0, {% if forloop.last %}40{% else %}10{% endif %}"">
                            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                        </Rock:FieldContainer>
                    </Rock:ResponsiveColumn>
                {% endif %}
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}
    <!-- Contact options -->
    {% assign hasContact = false %}
    {% if Member.Person.Email != '' %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ Member.Person.Email | Escape }}"" />
                <Label Text=""Email"" />
            </StackLayout>
            <Rock:Icon IconClass=""envelope"" FontSize=""36"" Command=""{Binding SendEmail}"" CommandParameter=""{{ Member.Person.Email | Escape }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}
    {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Mobile"" />
            </StackLayout>
            <Rock:Icon IconClass=""comment"" FontSize=""36"" Command=""{Binding SendSms}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
            <Rock:Icon IconClass=""phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}
    {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Home"" />
            </StackLayout>
            <Rock:Icon IconClass=""phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}
    {% if hasContact == true %}
        <Rock:Divider />
    {% endif %}
    {% if GroupMemberEditPage != '' %}
        <Button StyleClass=""btn,btn-primary,mt-32"" Text=""Edit"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupMemberEditPage }}?GroupMemberGuid={{ Member.Guid }}"" />
    {% endif %}
</StackLayout>";

        private const string _groupViewTemplate = @"<StackLayout StyleClass=""spacing-24"">
    <Grid RowDefinitions=""Auto, Auto""
        ColumnDefinitions=""*, Auto"">
        {% assign groupMemberCount = Group.Members | Size %}
    
        <Label StyleClass=""title1, text-interface-strongest, bold"" 
            Text=""{{ Group.Name | Escape }} Group"" />
    
        <Label StyleClass=""body, text-interface-strong"" 
            Text=""{{ 'member' | ToQuantity:groupMemberCount }}""
            Grid.Row=""1"" /> 
        {% if GroupEditPage != '' and AllowedActions.Edit == true %}
            <Rock:StyledBorder HeightRequest=""28""
                WidthRequest=""28""
                CornerRadius=""14""
                StyleClass=""border-2, border-primary-strong""
                Grid.Column=""1""
                Grid.RowSpan=""2""
                VerticalOptions=""Center"">
                <Rock:Icon IconClass=""fa fa-ellipsis-h""
                    FontSize=""14""
                    StyleClass=""text-primary-strong""
                    VerticalOptions=""Center""
                    HorizontalOptions=""Center"" />
                <Rock:StyledBorder.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding ShowActionPanel}"">
                        <TapGestureRecognizer.CommandParameter>
                            <Rock:ShowActionPanelParameters Title=""Group Actions"" CancelTitle=""Cancel"">
                                <Rock:ActionPanelButton Title=""Edit Group"" 
                                    Command=""{Binding PushPage}"" CommandParameter=""{{ GroupEditPage }}?GroupGuid={{ Group.Guid }}"" />
                            </Rock:ShowActionPanelParameters>
                        </TapGestureRecognizer.CommandParameter>
                    </TapGestureRecognizer>
                </Rock:StyledBorder.GestureRecognizers>
            </Rock:StyledBorder>
        {% endif %}
    </Grid>
    <!-- Handle Group Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout ColumnSpacing=""0""
            RowSpacing=""8"">
            {% for attribute in VisibleAttributes %}
                {% if attribute.FormattedValue != '' %}
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Rock:FieldContainer>
                            <Rock:Literal Label=""{{ attribute.Name | Escape }}""
                                Text=""{{ attribute.FormattedValue }}"" />
                        </Rock:FieldContainer>
                    </Rock:ResponsiveColumn>
                {% endif %}
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}
    <!-- Handle displaying of leaders -->
    {% if ShowLeaderList == true %}
    <StackLayout StyleClass=""spacing-8"">
        <Label StyleClass=""title2, bold, text-interface-strongest"" 
            Text=""Leaders"" />
        {% assign members = Group.Members | OrderBy:'Person.FullName' %}
        <Rock:StyledBorder StyleClass=""bg-interface-softest, border, border-interface-soft, p-16, rounded"">
            <VerticalStackLayout>
            {% for member in members %}
                {% if member.GroupRole.IsLeader == false %}{% continue %}{% endif %}
                <Grid RowDefinitions=""48, Auto""
                    ColumnDefinitions=""Auto, *""
                    StyleClass=""gap-column-8"">
                    
                    <Rock:Avatar Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ member.Person.PhotoUrl | Escape }}""
                        HeightRequest=""48""
                        ShowStroke=""false""
                        Grid.Row=""0""
                        Grid.Column=""0""
                        VerticalOptions=""Center"" />
                    <StackLayout Grid.Column=""1""
                        VerticalOptions=""Center"">
                        <Label StyleClass=""body, bold, text-interface-stronger""
                            Text=""{{ member.Person.FullName }}""
                            MaxLines=""1""
                            LineBreakMode=""TailTruncation"" />
                        <Label StyleClass=""footnote, text-interface-strong""
                            Grid.Column=""0""
                            MaxLines=""1""
                            LineBreakMode=""TailTruncation"" 
                            Text=""{{ member.GroupRole.Name | Escape }}"" /> 
                    </StackLayout>
                    {% unless forloop.last %}
                        <Rock:Divider Grid.Row=""1"" 
                            Grid.ColumnSpan=""3"" 
                            StyleClass=""my-8""/>
                    {% endunless %}
                </Grid>
            {% endfor %}
            </VerticalStackLayout>
        </Rock:StyledBorder>
    </StackLayout>
    {% endif %}
</StackLayout>";
        private const string _groupViewLegacyTemplate = @"<StackLayout Spacing=""0"">
    {% assign groupMemberCount = Member.Group.Members | Size %}
    
    <Label StyleClass=""h1"" Text=""{{ Member.Group.Name | Escape }} Group"" />
    <Label StyleClass=""text"" Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" />
    <StackLayout Orientation=""Horizontal"" Spacing=""20"" Margin=""0, 20, 0, 40"">
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if Member.Person.PhotoId != null %}{{ Member.Person.PhotoUrl | Append:'&width=300' | Escape }}{% else %}{{ Member.Person.PhotoUrl | Escape }}{% endif %}"" WidthRequest=""80"">
                <Rock:CircleTransformation />
            </Rock:Image>
            <StackLayout Spacing=""0"" VerticalOptions=""Center"">
                <Label StyleClass=""h4"" Text=""{{ Member.Person.FullName | Escape }}"" />
                {% if Member.Person.BirthDate != null %}
                    <Label StyleClass=""text, o-60"" Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                    <Label StyleClass=""text, o-60"" Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                {% endif %}
            </StackLayout>
    </StackLayout>
    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout ColumnSpacing=""0"">
            {% for attribute in VisibleAttributes %}
                {% if attribute.FormattedValue != '' %}
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Rock:FieldContainer Margin=""0, 0, 0, {% if forloop.last %}40{% else %}10{% endif %}"">
                            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                        </Rock:FieldContainer>
                    </Rock:ResponsiveColumn>
                {% endif %}
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}
    <!-- Contact options -->
    {% assign hasContact = false %}
    {% if Member.Person.Email != '' %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ Member.Person.Email | Escape }}"" />
                <Label Text=""Email"" />
            </StackLayout>
            <Rock:Icon IconClass=""envelope"" FontSize=""36"" Command=""{Binding SendEmail}"" CommandParameter=""{{ Member.Person.Email | Escape }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}
    {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Mobile"" />
            </StackLayout>
            <Rock:Icon IconClass=""comment"" FontSize=""36"" Command=""{Binding SendSms}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
            <Rock:Icon IconClass=""phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}
    {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Home"" />
            </StackLayout>
            <Rock:Icon IconClass=""phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}
    {% if hasContact == true %}
        <Rock:Divider />
    {% endif %}
    {% if GroupMemberEditPage != '' %}
        <Button StyleClass=""btn,btn-primary,mt-32"" Text=""Edit"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupMemberEditPage }}?GroupMemberGuid={{ Member.Guid }}"" />
    {% endif %}
</StackLayout>";

        private const string _groupFinderTemplate = @"{% if Groups == empty %}
    <Rock:NotificationBox NotificationType=""Warning"" Text=""No groups match your search criteria."" />
{% else %}
    <Rock:StyledBorder StyleClass=""bg-interface-softest, border, border-interface-soft, rounded, p-16"">
        <StackLayout>
            {% for group in Groups %}
                <Grid RowDefinitions=""64, Auto""
                    ColumnDefinitions=""*, Auto""
                    StyleClass=""gap-column-8""
                    VerticalOptions=""Center"">
                    
                    <VerticalStackLayout VerticalOptions=""Center"">
                        {% if group.Schedule.WeeklyDayOfWeek != null %}
                            <Label Text=""{{ group.Schedule.WeeklyDayOfWeek }}"" 
                                StyleClass=""text-info-strong, footnote"" />
                        {% endif %}
    
                        <Label Text=""{{ group.Name | Escape }}""
                            StyleClass=""body, bold, text-interface-stronger""
                            VerticalOptions=""Center"" />
                        <StackLayout Orientation=""Horizontal""
                            StyleClass=""spacing-8"">
                            {% if group.Schedule.WeeklyTimeOfDay != null %}
                                <Label Text=""Weekly at {{ group.Schedule.WeeklyTimeOfDayText }}"" 
                                    StyleClass=""text-interface-strong, footnote"" />
                            {% elsif group.Schedule != null %}
                                <Label Text=""{{ group.Schedule.FriendlyScheduleText }}"" 
                                    StyleClass=""text-interface-strong, footnote"" />
                            {% endif %}
                            {% assign topic = group | Attribute:'Topic' %}
                            {% if topic != empty %}
                                <Label Text=""{{ topic | Escape }}"" 
                                    StyleClass=""text-interface-strong, footnote"" />
                            {% endif %}
                        </StackLayout>
                    </VerticalStackLayout>
    
                    {% if DetailPage != null %}
                        <Grid.GestureRecognizers>
                            <TapGestureRecognizer Command=""{Binding PushPage}""
                                CommandParameter=""{{ DetailPage }}?GroupGuid={{ group.Guid }}"" />
                        </Grid.GestureRecognizers>
                        
                        <Rock:Icon Grid.Column=""1""
                            IconClass=""chevron-right"" 
                            VerticalOptions=""Center""
                            StyleClass=""caption1, text-interface-medium"" /> 
                    {% endif %}
    
                    {% unless forloop.last %}
                        <Rock:Divider Grid.Row=""1""
                            Grid.Column=""0"" 
                            Grid.ColumnSpan=""2""
                            VerticalOptions=""Center""
                            StyleClass=""my-8"" />
                    {% endunless %}
                </Grid>
            {% endfor %}
        </StackLayout>
    </Rock:StyledBorder>
{% endif %}";
        private const string _groupFinderLegacyTemplate = @"{% if Groups == empty %}
    <Rock:NotificationBox NotificationType=""Warning"" Text=""No groups match your search criteria."" />
{% else %}
    <StackLayout>
        <Rock:Divider />
        {% for group in Groups %}
        {% assign distance = Distances[group.Id] %}
        <Grid ColumnDefinitions=""1*, 15"" ColumnSpacing=""12"" StyleClass=""group-content"">
            {% if DetailPage != null %}
                <Grid.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?GroupGuid={{ group.Guid }}"" />
                </Grid.GestureRecognizers>
            {% endif %}
            <StackLayout Grid.Column=""0"" StyleClass=""group-primary-content"">
                {% if group.Schedule.WeeklyDayOfWeek != null %}
                    <Label Text=""{{ group.Schedule.WeeklyDayOfWeek }}"" StyleClass=""group-meeting-day"" />
                {% endif %}
                <Label Text=""{{ group.Name | Escape }}"" StyleClass=""group-name"" />
                <StackLayout Orientation=""Horizontal"">
                    {% if group.Schedule.WeeklyTimeOfDay != null %}
                        <Label Text=""Weekly at {{ group.Schedule.WeeklyTimeOfDayText }}"" HorizontalOptions=""Start"" StyleClass=""group-meeting-time"" />
                    {% elsif group.Schedule != null %}
                        <Label Text=""{{ group.Schedule.FriendlyScheduleText }}"" HorizontalOptions=""Start"" StyleClass=""group-meeting-time"" />
                    {% endif %}
                    {% assign topic = group | Attribute:'Topic' %}
                    {% if topic != empty %}
                        <Label Text=""{{ topic | Escape }}"" HorizontalTextAlignment=""End"" HorizontalOptions=""EndAndExpand"" StyleClass=""group-topic"" />
                    {% endif %}
                </StackLayout>
                {% if distance != null %}
                    <Label Text=""{{ distance | Format:'#,##0.0' }} mi"" StyleClass=""group-distance"" />
                {% endif %}
            </StackLayout>
            <Rock:Icon IconClass=""chevron-right"" Grid.Column=""1"" HorizontalOptions=""End"" VerticalOptions=""Center"" StyleClass=""group-more-icon"" />
        </Grid>
        <Rock:Divider />
        {% endfor %}
    </StackLayout>
{% endif %}";

        #endregion

        #region Prayer Blocks

        private const string _prayerSessionTemplate = @"{% if Request != null %}
    {% if Request.RequestedByPersonAlias != null %}
        {% assign photoUrl = Request.RequestedByPersonAlias.Person.PhotoUrl %}
    {% else %}
        {% assign photoUrl = 'Assets/Images/person-no-photo-unknown.svg' %}
    {% endif %}
    <Grid RowDefinitions=""Auto, Auto, Auto, Auto, Auto""
        ColumnDefinitions=""*, Auto""
        StyleClass=""gap-row-8"">
        
        <Label Text=""{{ Request.Category.Name | Escape }}""
            StyleClass=""body, text-interface-medium""
            Grid.Column=""0""
            Grid.Row=""0"" />
        <Rock:Icon IconFamily=""FontAwesomeRegular""
            IconClass=""Times-Circle""
            FontSize=""24""
            StyleClass=""text-interface-medium""
            Grid.Column=""1""
            Grid.Row=""0""
            Command=""{Binding PopPage}"" />
        <Rock:Avatar Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' | Append:photoUrl | Escape }}""
            HeightRequest=""80""
            Grid.Column=""0""
            WidthRequest=""80""
            HorizontalOptions=""Start""
            Grid.Row=""1"" />
        <Rock:FollowingIcon VerticalOptions=""Start""
            EntityTypeId=""{{ Request.TypeId }}""
            EntityId=""{{ Request.Id }}""
            FontSize=""24""
            IsFollowed=""{{ Request | IsFollowed }}""
            FollowingIconClass=""Heart""
            FollowingIconFamily=""FontAwesomeSolid""
            FollowingIconColor=""#b91c1c""
            NotFollowingIconClass=""Heart""
            NotFollowingIconFamily=""FontAwesomeRegular""
            NotFollowingIconColor=""#9aa7b3""
            Grid.Column=""1"" 
            Grid.Row=""1"" />
        <VerticalStackLayout Grid.Column=""0""
            Grid.Row=""2"">
            <Label StyleClass=""title1, bold, text-interface-strongest""
                Text=""Pray for {{ Request.FirstName | Escape }}"" />
            {% if Request.Campus and Request.Campus.Name != '' %}
                <Label Text=""{{ Request.Campus.Name | Escape }}""
                    StyleClass=""body, text-interface-strong"" />
            {% endif %}
        </VerticalStackLayout>
        <Label StyleClass=""mt-16, body, text-interface-strong""
            Grid.Row=""3""
            Grid.ColumnSpan=""2"">{{ Request.Text | XamlWrap }}</Label>
        <VerticalStackLayout Grid.Row=""4""
            Grid.ColumnSpan=""2""
            StyleClass=""mt-16"">
            <Button StyleClass=""btn, btn-primary""
                Text=""{{ PrayedButtonText | Escape }}""
                Command=""{Binding Callback}""
                CommandParameter=""{Rock:CallbackParameters Name=':NextRequest', Parameters={Rock:Parameter Name='SessionContext', Value='{{ SessionContext }}'}}"" />
            {% if ShowInappropriateButton %}
                <Button StyleClass=""btn, btn-link""
                    Text=""Report prayer as inappropriate""
                    Command=""{Binding ShowActionPanel}"">
                    <Button.CommandParameter>
                        <Rock:ShowActionPanelParameters Title=""Report Prayer Request""
                                                        CancelTitle=""Cancel"">
                            <Rock:ShowActionPanelParameters.DestructiveButton>
                                <Rock:ActionPanelButton Title=""Report""
                                                        Command=""{Binding Callback}""
                                                        CommandParameter=""{Rock:CallbackParameters Name=':FlagRequest', Parameters={Rock:Parameter Name='SessionContext', Value='{{ SessionContext }}'}}"" />
                            </Rock:ShowActionPanelParameters.DestructiveButton>
                        </Rock:ShowActionPanelParameters>
                    </Button.CommandParameter>
                </Button>
            {% endif %}
        </VerticalStackLayout>
    </Grid>
{% else %}
    <StackLayout StyleClass=""spacing-8"">
        <Label Text=""You have completed your prayer session."" 
            StyleClass=""body, text-interface-stronger""
            HorizontalOptions=""Center"" />
        <Button StyleClass=""btn, btn-primary""
            Text=""Done""
            Command=""{Binding PopPage}"" />
    </StackLayout>
{% endif %}";
        private const string _prayerSessionLegacyTemplate = @"{% if Request != null %}
<StackLayout Spacing=""0"">
    {% if Request.RequestedByPersonAlias != null %}
        {% if Request.RequestedByPersonAlias.Person.PhotoId != null %}
            {% assign photoUrl = Request.RequestedByPersonAlias.Person.PhotoUrl | Append:'&width=120' | Escape %}
        {% else %}
            {% assign photoUrl = Request.RequestedByPersonAlias.Person.PhotoUrl %}
        {% endif %}
    {% else %}
        {% assign photoUrl = 'Assets/Images/person-no-photo-unknown.svg' %}
    {% endif %}
    <StackLayout Orientation=""Horizontal"">
        <Label Text=""{{ Request.Category.Name | Escape }}""
                    HorizontalOptions=""StartAndExpand"" />
        <Rock:Icon IconFamily=""FontAwesomeRegular""
                           IconClass=""Times-Circle""
                          FontSize=""28""
                          TextColor=""#afafaf""
                          Command=""{Binding PopPage}"" />
    </StackLayout>
    <StackLayout Orientation=""Horizontal"">
        <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' | Append:photoUrl | Escape}}""
                              HeightRequest=""80""
                              BackgroundColor=""#afafaf""
                              HorizontalOptions=""StartAndExpand"">
            <Rock:CircleTransformation />
        </Rock:Image>
        <Rock:FollowingIcon HorizontalOptions=""End""
                                          VerticalOptions=""Center""
                                          EntityTypeId=""{{ Request.TypeId }}""
                                          EntityId=""{{ Request.Id }}""
                                          FontSize=""28""
                                          IsFollowed=""{{ Request | IsFollowed }}""
                                          FollowingIconClass=""Heart""
                                          FollowingIconFamily=""FontAwesomeSolid""
                                          FollowingIconColor=""#ff3434""
                                          NotFollowingIconClass=""Heart""
                                          NotFollowingIconFamily=""FontAwesomeRegular""
                                          NotFollowingIconColor=""#afafaf"" />
    </StackLayout>
    <Label StyleClass=""h1""
                Text=""Pray for {{ Request.FirstName | Escape }}"" />
    <Label Text=""{{ Request.Campus.Name }}"" />
    <Label Margin=""0,30,0,60"">{{ Request.Text | XamlWrap }}</Label>
    <Button StyleClass=""btn,btn-primary""
                  Text=""{{ PrayedButtonText | Escape }}""
                  Command=""{Binding Callback}""
                  CommandParameter=""{Rock:CallbackParameters Name=':NextRequest', Parameters={Rock:Parameter Name='SessionContext', Value='{{ SessionContext }}'}}"" />
    {% if ShowInappropriateButton %}
    <Button StyleClass=""btn,btn-link""
                  Text=""Report prayer as inappropriate""
                  Command=""{Binding ShowActionPanel}"">
        <Button.CommandParameter>
            <Rock:ShowActionPanelParameters Title=""Report Prayer Request""
                                            CancelTitle=""Cancel"">
                <Rock:ShowActionPanelParameters.DestructiveButton>
                    <Rock:ActionPanelButton Title=""Report""
                                            Command=""{Binding Callback}""
                                            CommandParameter=""{Rock:CallbackParameters Name=':FlagRequest', Parameters={Rock:Parameter Name='SessionContext', Value='{{ SessionContext }}'}}"" />
                </Rock:ShowActionPanelParameters.DestructiveButton>
            </Rock:ShowActionPanelParameters>
        </Button.CommandParameter>
    </Button>
    {% endif %}
</StackLayout>
{% else %}
<StackLayout Spacing=""0"">
    <Label Text=""You have completed your prayer session."" />
    <Button StyleClass=""btn,btn-primary""
                  Text=""Ok""
                  Command=""{Binding PopPage}"" />
</StackLayout>
{% endif %}";

        private const string _answerToPrayerTemplate = @"<Grid RowDefinitions=""Auto, Auto, Auto""
    ColumnDefinitions=""*, Auto"">
    <Label StyleClass=""body, text-interface-medium""
        Text=""{{ PrayerRequest.Category.Name | Escape }}""
        Grid.Row=""0""
        Grid.Column=""0"" />
    <Label StyleClass=""body, text-interface-medium""
        Text=""{{ PrayerRequest.EnteredDateTime | Date:'MMM d, yyyy' }}""
        Grid.Row=""0""
        Grid.Column=""1"" />
    <Label StyleClass=""body, text-interface-stronger, mt-24""
        Grid.ColumnSpan=""2"">
        <Label.Text><![CDATA[{{ PrayerRequest.Text }}]]></Label.Text>
    </Label>
</Grid>";
        private const string _answerToPrayerLegacyTemplate = @"<StackLayout StyleClass=""prayer-request"">
    <StackLayout StyleClass=""prayer-header""
        Orientation=""Horizontal"">
        <Label StyleClass=""prayer-category,text-gray-500""
            Text=""{{ PrayerRequest.Category.Name | Escape }}""
            HorizontalOptions=""StartAndExpand"" />
        <Label StyleClass=""prayer-date,text-gray-500""
            Text=""{{ PrayerRequest.EnteredDateTime | Date:'MMM d, yyyy' }}"" />
    </StackLayout>
    <Label StyleClass=""prayer-text"">
        <Label.Text><![CDATA[{{ PrayerRequest.Text }}]]></Label.Text>
    </Label>
</StackLayout>";

        private const string _myPrayerRequestsTemplate = @"{% if PrayerRequestItems == empty %}
    <Rock:NotificationBox NotificationType=""Information"" 
        Text=""Looks like you don't have any prayer requests."" />
{% else %}
    <Rock:StyledBorder StyleClass=""bg-interface-softest, border, border-interface-soft, rounded, p-16"">
        <VerticalStackLayout>
            {% for PrayerRequest in PrayerRequestItems %}
                <Grid RowDefinitions=""Auto, Auto, Auto, Auto, Auto""
                    ColumnDefinitions=""*, Auto"">
                    
                    <Label StyleClass=""body, text-interface-medium""
                        Text=""{{ PrayerRequest.Category.Name | Escape }}""
                        Grid.Row=""0""
                        Grid.Column=""0"" />
                    <Label StyleClass=""body, text-interface-medium""
                        Text=""{{ PrayerRequest.EnteredDateTime | Date:'MMM d, yyyy' }}""
                        Grid.Row=""0""
                        Grid.Column=""1"" /> 
                    <Label StyleClass=""body, text-interface-stronger, mt-24""
                        Grid.Row=""1""
                        Grid.ColumnSpan=""2"">
                        <Label.Text><![CDATA[{{ PrayerRequest.Text }}]]></Label.Text>
                    </Label>
                    {% if PrayerRequest.Answer != null and PrayerRequest.Answer != '' %}
                        <VerticalStackLayout StyleClass=""mt-8""
                            Grid.Row=""2"">
                            <Label Text=""Answer:"" StyleClass=""answer-header, body, text-interface-stronger"" />
                            <Label StyleClass=""answer-text, body, text-interface-strong"">
                                <Label.Text><![CDATA[{{ PrayerRequest.Answer }}]]></Label.Text>
                            </Label>
                        </VerticalStackLayout>
                    {% endif %}
                    <Grid ColumnDefinitions=""Auto, *, Auto, Auto""
                        Grid.Row=""3""
                        Grid.ColumnSpan=""2""
                        StyleClass=""mt-24"">
                        {% if AnswerPage != null %}
                            <Button StyleClass=""btn, btn-primary, btn-sm, add-answer-button""
                                Text=""{% if PrayerRequest.Answer != null and PrayerRequest.Answer != '' %}Edit Answer{% else %}Answer{% endif %}""
                                Command=""{Binding PushPage}""
                                CommandParameter=""{{ AnswerPage }}?RequestGuid={{ PrayerRequest.Guid}}"" />
                        {% endif %}
                        
                        {% if EditPage != null %}
                            <Button StyleClass=""btn, btn-link, btn-sm, edit-button""
                                Text=""Edit""
                                Command=""{Binding PushPage}""
                                Grid.Column=""2""
                                CommandParameter=""{{ EditPage }}?RequestGuid={{ PrayerRequest.Guid}}"" />
                        {% endif %}
                        <Button StyleClass=""btn, btn-link, btn-sm, delete-button""
                            Text=""Delete""
                            Command=""{Binding ShowActionPanel}""
                            Grid.Column=""3"">
                            <Button.CommandParameter>
                                <Rock:ShowActionPanelParameters Title=""Delete Prayer?"" CancelTitle=""Cancel"">
                                    <Rock:ShowActionPanelParameters.DestructiveButton>
                                        <Rock:ActionPanelButton Title=""Delete""
                                            Command=""{Binding Callback}""
                                            CommandParameter="":Delete?requestGuid={{ PrayerRequest.Guid }}"" />
                                    </Rock:ShowActionPanelParameters.DestructiveButton>
                                </Rock:ShowActionPanelParameters>
                            </Button.CommandParameter>
                        </Button>
                    </Grid>
                    {% unless forloop.last %}
                        <Rock:Divider Grid.Row=""4""
                            StyleClass=""my-8""
                            Grid.ColumnSpan=""2"" />
                    {% endunless %}
                </Grid>
            {% endfor %}
        </VerticalStackLayout>
    </Rock:StyledBorder>
{% endif %}";
        private const string _myPrayerRequestsLegacyTemplate = @"{% if PrayerRequestItems == empty %}
    <Rock:NotificationBox NotificationType=""Information"" Text=""Looks like you don't have any prayer requests."" />
{% else %}
    <StackLayout StyleClass=""prayer-request-list"">
        {% for PrayerRequest in PrayerRequestItems %}
            {% if forloop.index > 1 %}<Rock:Divider />{% endif %}
            <StackLayout StyleClass=""prayer-request"">
                <StackLayout StyleClass=""prayer-header""
                    Orientation=""Horizontal"">
                    <Label StyleClass=""prayer-category,text-gray-500""
                        Text=""{{ PrayerRequest.Category.Name | Escape }}""
                        HorizontalOptions=""StartAndExpand"" />
                    <Label StyleClass=""prayer-date,text-gray-500""
                        Text=""{{ PrayerRequest.EnteredDateTime | Date:'MMM d, yyyy' }}"" />
                </StackLayout>
                <Label StyleClass=""prayer-text"">
                    <Label.Text><![CDATA[{{ PrayerRequest.Text }}]]></Label.Text>
                </Label>
                
                {% if PrayerRequest.Answer != null and PrayerRequest.Answer != '' %}
                    <Label Text=""Answer:"" StyleClass=""answer-header,text-gray-500"" />
                    <Label StyleClass=""answer-text,text-gray-500"">
                        <Label.Text><![CDATA[{{ PrayerRequest.Answer }}]]></Label.Text>
                    </Label>
                {% endif %}
                <StackLayout Orientation=""Horizontal"" StyleClass=""actions"">
                    {% if AnswerPage != null %}
                        <Button StyleClass=""btn,btn-primary,btn-sm,add-answer-button""
                            Text=""{% if PrayerRequest.Answer != null and PrayerRequest.Answer != '' %}Edit Answer{% else %}Add an Answer{% endif %}""
                            Command=""{Binding PushPage}""
                            CommandParameter=""{{ AnswerPage }}?requestGuid={{ PrayerRequest.Guid}}"" />
                    {% endif %}
                    
                    <ContentView HorizontalOptions=""FillAndExpand"" />
                    {% if EditPage != null %}
                        <Button StyleClass=""btn,btn-link,btn-sm,edit-button""
                            Text=""Edit Request""
                            Command=""{Binding PushPage}""
                            CommandParameter=""{{ EditPage }}?requestGuid={{ PrayerRequest.Guid}}"" />
                    {% endif %}
                    
                    <Button StyleClass=""btn,btn-link,btn-sm,delete-button""
                        Text=""Delete""
                        Command=""{Binding ShowActionPanel}"">
                        <Button.CommandParameter>
                            <Rock:ShowActionPanelParameters Title=""Delete Prayer?"" CancelTitle=""Cancel"">
                                <Rock:ShowActionPanelParameters.DestructiveButton>
                                    <Rock:ActionPanelButton Title=""Delete""
                                        Command=""{Binding Callback}""
                                        CommandParameter="":Delete?requestGuid={{ PrayerRequest.Guid }}"" />
                                </Rock:ShowActionPanelParameters.DestructiveButton>
                            </Rock:ShowActionPanelParameters>
                        </Button.CommandParameter>
                    </Button>
                </StackLayout>
            </StackLayout>
        {% endfor %}
    </StackLayout>
{% endif %}";

        private const string _prayerCardViewTemplate = @"<VerticalStackLayout Spacing=""16"">
    {% assign size = PrayerRequestItems | Size %}
    {% if size == 0 %}
    <Label Text=""No Prayer Requests Found""
        StyleClass=""body, text-interface-stronger"" />
    {% else %}
        {% for item in PrayerRequestItems %}
            <Rock:StyledBorder StyleClass=""border, border-interface-soft, bg-interface-softest, rounded, p-16"">
                <VerticalStackLayout Spacing=""16"">
                    <VerticalStackLayout Spacing=""4"">
                        <Label Text=""{{ item.FirstName | Escape }} {{ item.LastName | Escape }}"" 
                            StyleClass=""title2, bold, text-interface-stronger"" />

                        <Rock:Tag Text=""{{ item.Category.Name | Escape }}"" 
                            Type=""Info"" />
                    </VerticalStackLayout>

                    <Label StyleClass=""prayer-card-text"">{{ item.Text | XamlWrap }}</Label>

                    <Button x:Name=""PrayedBtn{{ forloop.index }}""
                        IsVisible=""false""
                        StyleClass=""btn,btn-primary,prayer-card-prayed-button""
                        HorizontalOptions=""End""
                        Text=""Prayed""
                        IsEnabled=""false"" />

                    <Button x:Name=""PrayBtn{{ forloop.index }}""
                        StyleClass=""btn,btn-primary,prayer-card-pray-button""
                        HorizontalOptions=""End""
                        Text=""Pray""
                        Command=""{Binding AggregateCommand}"">
                        <Button.CommandParameter>
                            <Rock:AggregateCommandParameters>
                                <Rock:CommandReference Command=""{Binding PrayForRequest}""
                                    CommandParameter=""{Rock:PrayForRequestParameters Guid={{ item.Guid }}, WorkflowTypeGuid='{{ PrayedWorkflowType }}'}"" />

                                <Rock:CommandReference Command=""{Binding SetViewProperty}""
                                    CommandParameter=""{Rock:SetViewPropertyParameters View={x:Reference PrayedBtn{{ forloop.index }}}, Name=IsVisible, Value=true}"" />

                                <Rock:CommandReference Command=""{Binding SetViewProperty}""
                                    CommandParameter=""{Rock:SetViewPropertyParameters View={x:Reference PrayBtn{{ forloop.index }}}, Name=IsVisible, Value=false}"" />
                            </Rock:AggregateCommandParameters>
                        </Button.CommandParameter>
                    </Button>
                </VerticalStackLayout>
            </Rock:StyledBorder>
        {% endfor %}
    {% endif %}
</VerticalStackLayout>";
        private const string _prayerCardViewLegacyTemplate = @"<Rock:ResponsiveLayout>
{% for item in PrayerRequestItems %}
    <Rock:ResponsiveColumn Medium=""6"">
        <Frame StyleClass=""prayer-card-container"" HasShadow=""false"">
            <StackLayout>
                <Label Text=""{{ item.FirstName | Escape }} {{ item.LastName | Escape }}"" StyleClass=""prayer-card-name"" />

                <ContentView StyleClass=""prayer-card-category"" HorizontalOptions=""Start"">
                    <Label Text=""{{ item.Category.Name | Escape }}"" />
                </ContentView>

                <Label StyleClass=""prayer-card-text"">{{ item.Text | XamlWrap }}</Label>

                <Button x:Name=""PrayedBtn{{ forloop.index }}""
                    IsVisible=""false""
                    StyleClass=""btn,btn-primary,prayer-card-prayed-button""
                    HorizontalOptions=""End""
                    Text=""Prayed""
                    IsEnabled=""false"" />
                <Button x:Name=""PrayBtn{{ forloop.index }}""
                    StyleClass=""btn,btn-primary,prayer-card-pray-button""
                    HorizontalOptions=""End""
                    Text=""Pray""
                    Command=""{Binding AggregateCommand}"">
                    <Button.CommandParameter>
                        <Rock:AggregateCommandParameters>
                            <Rock:CommandReference Command=""{Binding PrayForRequest}""
                                CommandParameter=""{Rock:PrayForRequestParameters Guid={{ item.Guid }}, WorkflowTypeGuid='{{ PrayedWorkflowType }}'}"" />

                            <Rock:CommandReference Command=""{Binding SetViewProperty}""
                                CommandParameter=""{Rock:SetViewPropertyParameters View={x:Reference PrayedBtn{{ forloop.index }}}, Name=IsVisible, Value=true}"" />

                            <Rock:CommandReference Command=""{Binding SetViewProperty}""
                                CommandParameter=""{Rock:SetViewPropertyParameters View={x:Reference PrayBtn{{ forloop.index }}}, Name=IsVisible, Value=false}"" />
                        </Rock:AggregateCommandParameters>
                    </Button.CommandParameter>
                </Button>
            </StackLayout>
        </Frame>
    </Rock:ResponsiveColumn>
{% endfor %}
</Rock:ResponsiveLayout>";

        #endregion

        #region Event Blocks

        private const string _liveExperienceOccurrencesTemplate = @"<StackLayout Spacing=""8"">
    {% for occurrence in Occurrences %}
        {% if occurrence.Campus != null %}
            {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }} at {{ occurrence.Campus.Name }}{% endcapture %}
        {% else %}
            {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }}{% endcapture %}
        {% endif %}

        <Button Text=""{{ occurrenceName | Escape }}"" 
            StyleClass=""btn, btn-primary"" 
            Command=""{Binding PushPage}"" 
            CommandParameter=""{{ DestinationPageGuid }}?InteractiveExperienceOccurrenceKey={{ occurrence.Guid }}"" />
    {% endfor %}

    {% if LoginRecommended == true %}
        <Button Text=""Login"" 
            StyleClass=""btn, btn-outline-primary"" 
            Command=""{Binding InteractiveExperienceOccurrences.Login}"" />
    {% endif %}

    {% if GeoLocationRecommended == true %}
        <Button Text=""Provide Location"" StyleClass=""btn, btn-outline-primary""
            Command=""{Binding InteractiveExperienceOccurrences.ProvideLocation}"" />
    {% endif %}
</StackLayout>";
        private const string _liveExperienceOccurrencesLegacyTemplate = @"<StackLayout>
    {% for occurrence in Occurrences %}
        {% if occurrence.Campus != null %}
            {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }} at {{ occurrence.Campus.Name }}{% endcapture %}
        {% else %}
            {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }}{% endcapture %}
        {% endif %}

        <Button Text=""{{ occurrenceName | Escape }}"" StyleClass=""btn,btn-primary"" Command=""{Binding PushPage}"" CommandParameter=""{{ DestinationPageGuid }}?InteractiveExperienceOccurrenceKey={{ occurrence.Guid }}"" />
    {% endfor %}

    {% if LoginRecommended == true %}
        <Button Text=""Login"" StyleClass=""btn,btn-secondary"" Command=""{Binding InteractiveExperienceOccurrences.Login}"" />
    {% endif %}

    {% if GeoLocationRecommended == true %}
        <Button Text=""Provide Location"" StyleClass=""btn,btn-secondary"" />
    {% endif %}
</StackLayout>";

        private const string _calendarEventListTemplate = @"<Rock:StyledBorder>
    <StackLayout>
        <Label StyleClass=""body, bold, text-interface-stronger""
            Text=""{Binding Name}"" />
        
        {% if Item.EndDateTime == null %}
            <Label StyleClass=""footnote, text-interface-strong"" 
                Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" 
                LineBreakMode=""NoWrap"" />
        {% else %}
            <Label StyleClass=""footnote, text-interface-strong"" 
                Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" 
                LineBreakMode=""NoWrap"" />
        {% endif %}

        <Grid ColumnDefinitions=""*, Auto""
            StyleClass=""mt-16"">
            <Label StyleClass=""caption1, text-interface-strong"" 
                Text=""{{ Item.Audiences | Select:'Name' | Join:', ' | Escape }}"" />

            <Label StyleClass=""caption1, text-interface-strong""
                Text=""{{ Item.Campus | Escape }}""
                HorizontalTextAlignment=""End"" 
                LineBreakMode=""NoWrap"" />
        </Grid>
    </StackLayout>
</Rock:StyledBorder>";
        private const string _calendarEventListLegacyTemplate = @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
    <StackLayout Spacing=""0"">
        <Label StyleClass=""calendar-event-title"" Text=""{Binding Name}"" />
        {% if Item.EndDateTime == null %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% else %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% endif %}
        <StackLayout Orientation=""Horizontal"">
            <Label HorizontalOptions=""FillAndExpand"" StyleClass=""calendar-event-audience"" Text=""{{ Item.Audiences | Select:'Name' | Join:', ' | Escape }}"" />
            <Label StyleClass=""calendar-event-campus"" Text=""{{ Item.Campus | Escape }}"" HorizontalTextAlignment=""End"" LineBreakMode=""NoWrap"" />
        </StackLayout>
    </StackLayout>
</Frame>";

        private const string _calendarEventItemOccurrenceTemplate = @"<VerticalStackLayout Spacing=""24"">
    <VerticalStackLayout Spacing=""8"">
        {% if Event.Photo.Guid %}
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}/GetImage.ashx?Guid={{ Event.Photo.Guid }}"" 
                Aspect=""AspectFill"" 
                Ratio=""4:2"">
                <Rock:RoundedTransformation CornerRadius=""12""  />
            </Rock:Image>    
        {% endif %}

        <Label StyleClass=""title1, text-interface-strongest, bold"" 
            Text=""{{ Event.Name | Escape }}""  />    

        
        <Rock:Html StyleClass=""body, text-interface-stronger"">
            {{ Event.Description | Escape }}
        </Rock:Html>
    </VerticalStackLayout>

    <Rock:FieldContainer FieldLayout=""Individual"">
        {% assign scheduledDates = EventItemOccurrence.Schedule.iCalendarContent | DatesFromICal:'all' %}
        {% assign scheduleListing = '' %}
        {% for scheduledDate in scheduledDates %}
            {% if forloop.index <= 5 %}
                {% assign scheduleDateTime = scheduledDate | Date:'dddd, MMMM d, yyyy @ h:mm tt' %}
                {% assign scheduleListing = scheduleListing | Append:scheduleDateTime | Append:'&#xa;'  %}
            {% endif %}

        {% endfor %}
        
        <Rock:Literal Label=""Date / Time"" Text=""{{ scheduleListing | ReplaceLast:'&#xa;', '' }}"" />
    
        {% if EventItemOccurrence.Location != '' %}
            <Rock:Literal Label=""Location"" Text=""{{ EventItemOccurrence.Location }}"" />
        {% endif %}

        {% if EventItemOccurrence.ContactPersonAliasId != null or EventItemOccurrence.ContactEmail != '' or EventItemOccurrence.ContactPhone != '' %}
            {% if EventItemOccurrence.ContactPersonAliasId != null %}
                <Rock:Literal Label=""Contact"" Text=""{{ EventItemOccurrence.ContactPersonAlias.Person.FullName | Escape }}"" />
            {% endif %}
            {% if EventItemOccurrence.ContactEmail != '' %}
                <Rock:Literal Label=""Contact Email"" Text=""{{ EventItemOccurrence.ContactEmail | Escape }}"" />
            {% endif %}
            {% if EventItemOccurrence.ContactPhone != '' %}
                <Rock:Literal Label=""Contact Phone"" Text=""{{ EventItemOccurrence.ContactPhone | Escape }}"" />
            {% endif %}
        {% endif %}
    </Rock:FieldContainer>
            
    {% assign showRegistration = false %}
    {% assign eventItemOccurrenceLinkages = EventItemOccurrence.Linkages %}
    
    {% assign eventItemOccurrenceLinkagesCount = eventItemOccurrenceLinkages | Size %}
    {% if eventItemOccurrenceLinkagesCount > 0 %}
        {% for eventItemOccurrenceLinkage in eventItemOccurrenceLinkages %}
            {% assign daysTillStartDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime,'m' %}
            {% assign daysTillEndDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime,'m' %}
            {% assign showRegistration = true %}
            {% assign registrationMessage = '' %}
    
            {% if daysTillStartDate and daysTillStartDate > 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if daysTillEndDate and daysTillEndDate < 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if showRegistration == true %}
                {% assign statusLabel = RegistrationStatusLabels[eventItemOccurrenceLinkage.RegistrationInstanceId] %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% assign registrationButtonText = statusLabel %}
                {% else %}
                  {% assign registrationButtonText = statusLabel | Plus:' for ' | Plus:eventItemOccurrenceLinkage.PublicName %}
                {% endif %}
    
                {% if statusLabel == 'Full' %}
                    {% if eventItemOccurrenceLinkagesCount == 1 %}
                      {% assign registrationButtonText = 'Registration Full' %}
                    {% else %}
                      {% assign registrationButtonText = eventItemOccurrenceLinkage.PublicName | Plus: ' (Registration Full) ' %}
                    {% endif %}
                    <Label StyleClass=""body, bold, text-interface-stronger"">{{ registrationButtonText }}</Label>
                {% else %}
                    {% if eventItemOccurrenceLinkage.UrlSlug != '' %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""Slug"" Value=""{{eventItemOccurrenceLinkage.UrlSlug}}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% else %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""EventOccurrenceId"" Value=""{{ eventItemOccurrenceLinkage.EventItemOccurrenceId }}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% endif %}
                {% endif %}
            {% else %}
              <Label StyleClass=""body, bold, text-interface-stronger"" Text=""Registration Information"" />
              <Label StyleClass=""body, text-interface-strong"" Text=""{{ registrationMessage | Escape }}"" />
            {% endif %}
        {% endfor %}
    {% endif %}
</VerticalStackLayout>";
        private const string _calendarEventItemOccurrenceLegacyTemplate = @"<StackLayout Spacing=""0"">
    
    {% if Event.Photo.Guid %}
        <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}/GetImage.ashx?Guid={{ Event.Photo.Guid }}"" 
            Aspect=""AspectFill"" 
            Ratio=""4:2""
            Margin=""0,0,0,16"">
            <Rock:RoundedTransformation CornerRadius=""12""  />
        </Rock:Image>
    {% endif %}
    
    <Label StyleClass=""h1"" 
        Text=""{{ Event.Name | Escape }}""  />
    
    <Rock:FieldContainer FieldLayout=""Individual"">
        {% assign scheduledDates = EventItemOccurrence.Schedule.iCalendarContent | DatesFromICal:'all' %}
        {% assign scheduleListing = '' %}
        {% for scheduledDate in scheduledDates %}
            {% if forloop.index <= 5 %}
                {% assign scheduleDateTime = scheduledDate | Date:'dddd, MMMM d, yyyy @ h:mm tt' %}
                {% assign scheduleListing = scheduleListing | Append:scheduleDateTime | Append:'&#xa;'  %}
            {% endif %}

        {% endfor %}
        
        <Rock:Literal Label=""Date / Time"" Text=""{{ scheduleListing | ReplaceLast:'&#xa;', '' }}"" />
    
        {% if EventItemOccurrence.Location != '' %}
            <Rock:Literal Label=""Location"" Text=""{{ EventItemOccurrence.Location }}"" />
        {% endif %}
    </Rock:FieldContainer>
    
    <Rock:Html StyleClass=""text"">
        {{ Event.Description | Escape }}
    </Rock:Html>
    
    {% if EventItemOccurrence.Note != '' %}
        <Label Text=""Note"" StyleClass=""text, font-weight-bold"" />
        <Rock:Html StyleClass=""text"">{{ EventItemOccurrence.Note | Escape }}</Rock:Html>
    {% endif %}

    
    {% if EventItemOccurrence.ContactPersonAliasId != null or EventItemOccurrence.ContactEmail != '' or EventItemOccurrence.ContactPhone != '' %}
        {% if EventItemOccurrence.ContactPersonAliasId != null %}
            <Label Text=""Contact"" StyleClass=""title"" />
            <Label Text=""{{ EventItemOccurrence.ContactPersonAlias.Person.FullName }}"" />
        {% endif %}
        {% if EventItemOccurrence.ContactEmail != '' %}
            <Label Text=""{{ EventItemOccurrence.ContactEmail }}"" />
        {% endif %}
        {% if EventItemOccurrence.ContactPhone != '' %}
            <Label Text=""{{ EventItemOccurrence.ContactPhone }}"" />
        {% endif %}
    {% endif %}
    
    
    {% assign showRegistration = false %}
    {% assign eventItemOccurrenceLinkages = EventItemOccurrence.Linkages %}
    
    {% assign eventItemOccurrenceLinkagesCount = eventItemOccurrenceLinkages | Size %}
    {% if eventItemOccurrenceLinkagesCount > 0 %}
        {% for eventItemOccurrenceLinkage in eventItemOccurrenceLinkages %}
            {% assign daysTillStartDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime,'m' %}
            {% assign daysTillEndDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime,'m' %}
            {% assign showRegistration = true %}
            {% assign registrationMessage = '' %}
    
            {% if daysTillStartDate and daysTillStartDate > 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if daysTillEndDate and daysTillEndDate < 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if showRegistration == true %}
                {% assign statusLabel = RegistrationStatusLabels[eventItemOccurrenceLinkage.RegistrationInstanceId] %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% assign registrationButtonText = statusLabel %}
                {% else %}
                  {% assign registrationButtonText = statusLabel | Plus:' for ' | Plus:eventItemOccurrenceLinkage.PublicName %}
                {% endif %}
    
                {% if statusLabel == 'Full' %}
                    {% if eventItemOccurrenceLinkagesCount == 1 %}
                      {% assign registrationButtonText = 'Registration Full' %}
                    {% else %}
                      {% assign registrationButtonText = eventItemOccurrenceLinkage.PublicName | Plus: ' (Registration Full) ' %}
                    {% endif %}
                    <Label StyleClass=""text"">{{ registrationButtonText }}</Label>
                {% else %}
                    {% if eventItemOccurrenceLinkage.UrlSlug != '' %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary, mt-24"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""Slug"" Value=""{{eventItemOccurrenceLinkage.UrlSlug}}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% else %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary, mt-24"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""EventOccurrenceId"" Value=""{{ eventItemOccurrenceLinkage.EventItemOccurrenceId }}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% endif %}
                {% endif %}
            {% else %}
              <Label StyleClass=""font-weight-bold"" Text=""Registration Information"" />
              <Label StyleClass=""text"" Text=""{{ registrationMessage | Escape }}"" />
            {% endif %}
        {% endfor %}
    {% endif %}
</StackLayout>";

        private const string _eventItemOccurrenceListByAudienceLavaTemplate = @"<StackLayout>
    <Label StyleClass=""title1, bold, text-interface-strongest"" Text=""{{ ListTitle | Escape }}"" />

    {% for occurrence in EventItemOccurrences %}
    <Rock:StyledBorder StyleClass=""bg-interface-softest, border, border-interface-soft, rounded, p-16, my-4"">
        <Grid ColumnDefinitions=""*, Auto"">
            <StackLayout Spacing=""0"">
                <Label StyleClass=""body, bold, text-interface-stronger"" Text=""{{ occurrence.EventItem.Name | Escape }}"" />
                <Label StyleClass=""footnote, text-interface-strong"" Text=""({{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'M/d ' }})"" LineBreakMode=""NoWrap"" />
            </StackLayout>

            <Rock:Icon IconClass=""chevron-right""
                StyleClass=""text-interface-medium, caption2""
                VerticalOptions=""Center""
                Grid.Column=""1"" />
        </Grid>
    
        {% if occurrence.EventItem.DetailsUrl != '' %}
            <Rock:StyledBorder.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding OpenExternalBrowser}"" CommandParameter=""{{ occurrence.EventItem.DetailsUrl | Escape }}"" />
            </Rock:StyledBorder.GestureRecognizers>
        {% elseif EventDetailPage != null %}
            <Rock:StyledBorder.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ EventDetailPage }}?EventOccurrenceId={{ occurrence.Id }}&amp;EventOccurrenceGuid={{ occurrence.Guid }}"" />
            </Rock:StyledBorder.GestureRecognizers>
        {% endif %}
    </Rock:StyledBorder>
    {% endfor %}
</StackLayout>";
        private const string _eventItemOccurrenceListByAudienceLavaLegacyTemplate = @"<StackLayout>
    <Label StyleClass=""h1,mb-4"" Text=""{{ ListTitle | Escape }}"" />
    {% for occurrence in EventItemOccurrences %}
    <Frame HasShadow=""false"" StyleClass=""calendar-event"">
        <StackLayout Spacing=""0"">
            <Label StyleClass=""calendar-event-title"" Text=""{{ occurrence.EventItem.Name | Escape }}"" />
            <Label StyleClass=""calendar-event-text text-sm"" Text=""({{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'M/d ' }})"" LineBreakMode=""NoWrap"" />
        </StackLayout>
    
        {% if occurrence.EventItem.DetailsUrl != '' %}
            <Frame.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding OpenExternalBrowser}"" CommandParameter=""{{ occurrence.EventItem.DetailsUrl | Escape }}"" />
            </Frame.GestureRecognizers>
        {% elseif EventDetailPage != null %}
            <Frame.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ EventDetailPage }}?EventOccurrenceId={{ occurrence.Id }}&amp;EventOccurrenceGuid={{ occurrence.Guid }}"" />
            </Frame.GestureRecognizers>
        {% endif %}
    </Frame>
    {% endfor %}
</StackLayout>";

        #endregion

        #region Helpers

        /// <summary>
        /// This method adds a new block template with the specified values, and then renames the provided
        /// legacy template to have a new name.
        /// </summary>
        /// <param name="templateGuid">The GUID of the new template being generated.</param>
        /// <param name="templateBlockGuid">The GUID of the block template.</param>
        /// <param name="templateXaml">The XAML to use in the new template.</param>
        /// <param name="legacyTemplateGuid">The GUID of the legacy template.</param>
        /// <param name="legacyTemplateXaml">The old legacy template value.</param>
        private void UpdateTemplate( string templateGuid, string attributeKey, string blockTypeGuid, string templateBlockGuid, string templateXaml, string legacyTemplateGuid, string legacyTemplateXaml )
        {
            // Add the new default template.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                templateGuid,
                templateBlockGuid,
                "Default",
                templateXaml,
                _standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // Rename the old template to have a new name.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                legacyTemplateGuid,
                templateBlockGuid,
                "Legacy",
                legacyTemplateXaml,
                _standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // If someone has dragged the block on to a page and not saved the settings,
            // the template will now have a wrong default value. So we need to update the
            // block settings to use the new default template.
            string sql = $@"DECLARE @AttributeKey NVARCHAR( 1000 ) = '{SanitizeSql( attributeKey )}'
DECLARE @AttributeId INT = (
    SELECT TOP 1 attr.[Id] 
    FROM [Attribute] attr
    INNER JOIN [EntityType] et ON et.[Id] = attr.[EntityTypeId]
    WHERE et.[Name] = 'Rock.Model.Block'
    AND attr.[Key] = @AttributeKey
);
DECLARE @Value NVARCHAR(MAX) = '{SanitizeSql( legacyTemplateGuid )}'
DECLARE @BlockTypeGuid UNIQUEIDENTIFIER = '{SanitizeSql( blockTypeGuid )}'

IF @AttributeId IS NOT NULL
BEGIN
    INSERT INTO [AttributeValue] ([AttributeId], [EntityId], [Value], [Guid], [IsSystem])
    SELECT 
        @AttributeId,         -- Use the AttributeId from the variable
        b.[Id],               -- EntityId is the Block Id
        @Value,  
        NEWID(),
        0
    FROM [BlockType] bt 
    INNER JOIN [Block] b ON bt.[Id] = b.[BlockTypeId]
    WHERE bt.[Guid] = @BlockTypeGuid
    AND b.[Id] NOT IN (
        SELECT b.[Id] 
        FROM [BlockType] bt 
        INNER JOIN [Block] b ON bt.[Id] = b.[BlockTypeId]
        INNER JOIN [AttributeValue] av ON av.[EntityId] = b.[Id]
        WHERE bt.[Guid] = @BlockTypeGuid
        AND av.[AttributeId] = @AttributeId 
    );
END";
            Sql( sql );
        }

        /// <summary>
        /// Sanitizes a SQL string by replacing any occurrences of "'" with "''".
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Sanitized string that can be used in a SQL statement.</returns>
        /// <example>{% sql %}SELECT * FROM [Person] WHERE [LastName] = '{{ Name | SanitizeSql }}'{% endsql %}</example>
        private string SanitizeSql( string input )
        {
            if ( input == null )
            {
                return input;
            }

            return input.Replace( "'", "''" );
        }

        /// <summary>
        /// Downgrades a template by deleting the new template and legacy template, and then adding the legacy template back in.
        /// </summary>
        /// <param name="newTemplateGuid">The GUID of the new template being generated.</param>
        /// <param name="templateBlockGuid">The GUID of the block template.</param>
        /// <param name="legacyTemplateGuid">The GUID of the legacy template.</param>
        /// <param name="legacyTemplateXaml">The XAML of the legacy template.</param>
        private void DowngradeTemplate( string newTemplateGuid, string templateBlockGuid, string legacyTemplateGuid, string legacyTemplateXaml )
        {
            // Delete the new default and legacy template.
            RockMigrationHelper.DeleteTemplateBlockTemplate( newTemplateGuid );
            RockMigrationHelper.DeleteTemplateBlockTemplate( legacyTemplateGuid );

            // Add the new "default" template.. Which is really the legacy template.. because
            // down methods are super weird.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                newTemplateGuid,
                templateBlockGuid,
                "Default",
                legacyTemplateXaml,
                _standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );
        }

        #endregion
    }
}
