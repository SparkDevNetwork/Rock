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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v16 to perform additional steps for moving Person Preferences.
    /// </summary>
    [DisplayName( "Rock Update Helper v16.0 - Move Person Preferences" )]
    [Description( "This job will initialize all person preferences from attribute values." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV16DataMigrationsMovePersonPreferences : RockJob
    {
        /// <summary>
        /// A list of any errors that occurred that should prevent us from
        /// showing a successful completion.
        /// </summary>
        private readonly List<(string Message, Exception Exception)> _migrationErrors = new List<(string Message, Exception Exception)>();

        /// <summary>
        /// List of all prefix and suffix pairs that have already been converted.
        /// </summary>
        private readonly List<(string Prefix, string Suffix)> _convertedKeys = new List<(string Prefix, string Suffix)>();

        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );

            CopyPreferences( "block-", string.Empty, CopyStandardBlockPreferences );
            CopyPreferences( "grid-page-size-preference_", string.Empty, CopyGridPageSizePreferences );
            CopyPreferences( "grid-filter-", string.Empty, CopyGridFilterPreferences );
            CopyPreferences( "attendance-reporting-", string.Empty, CopyAttendanceReportingPreferences );
            CopyPreferences( "sms-conversations-", string.Empty, CopySmsConversationsPreferences );
            CopyPreferences( "giving-analytics-", string.Empty, CopyGivingAnalyticsPreferences );
            CopyPreferences( "transaction-matching-", string.Empty, CopyTransactionMatchingPreferences );
            CopyPreferences( "pledge-analytics-", string.Empty, CopyPledgeAnalyticsPreferences );
            CopyPreferences( "prayer-session-", string.Empty, CopyPrayerSessionPreferences );
            CopyPreferences( "prayer-categories-", string.Empty, CopyPrayerCategoriesPreferences );
            CopyPreferences( "report-show-results", string.Empty, CopyReportShowResultsPreferences );
            CopyPreferences( "reportdata-filter-", string.Empty, CopyReportDataFilterPreferences );
            CopyPreferences( "form-analytics-", string.Empty, CopyFormAnalyticsPreferences );
            CopyPreferences( "Rock.KeyAttributes.", string.Empty, CopyKeyAttributesPreferences );
            CopyPreferences( string.Empty, "_showInactive", CopyCheckinShowInactivePreferences );
            CopyPreferences( "CurrentNavPath", string.Empty, CopyCurrentNavPathPreferences );
            CopyPreferences( "ContentChannelNavigation_StatusFilter", string.Empty, CopyContentChannelNavigationStatusFilterPreferences );
            CopyPreferences( "ContentChannelNavigation_SelectedChannelId", string.Empty, CopyContentChannelNavigationSelectedChannelIdPreferences );
            CopyPreferences( "ContentChannelNavigation_CategoryFilter", string.Empty, CopyContentChannelNavigationCategoryFilterPreferences );
            CopyPreferences( "MyConnectionOpportunities_Toggle", string.Empty, CopyMyConnectionOpportunitiesTogglePreferences );
            CopyPreferences( "MyConnectionOpportunities_ToggleShowActive", string.Empty, CopyMyConnectionOpportunitiesToggleShowActivePreferences );
            CopyPreferences( "MyConnectionOpportunities_SelectedOpportunity", string.Empty, CopyMyConnectionOpportunitiesSelectedOpportunityPreferences );
            CopyPreferences( "MyConnectionOpportunities_SelectedCampus", string.Empty, CopyMyConnectionOpportunitiesSelectedCampusPreferences );
            CopyPreferences( "ConnectionRequestDetail_Campus", string.Empty, CopyConnectionRequestDetailCampusPreferences );
            CopyPreferences( "MyActiveOpportunitiesChecked", string.Empty, CopyMyActiveOpportunitiesCheckedPreferences );
            CopyPreferences( "ConnectionOpportunitiesSelectedCampus", string.Empty, CopyConnectionOpportunitiesSelectedCampusPreferences );
            CopyPreferences( "context-date-range", string.Empty, CopyContextDateRangePreferences );
            CopyPreferences( "HideInactiveItems", string.Empty, CopyHideInactiveItemsPreferences );
            CopyPreferences( "HideInactiveAccounts", string.Empty, CopyHideInactiveAccountsPreferences );
            CopyPreferences( "Attendance_List_Sorting_Toggle", string.Empty, CopyAttendanceListSortingTogglePreferences );
            CopyPreferences( "HideInactiveGroups", string.Empty, CopyHideInactiveGroupsPreferences );
            CopyPreferences( "LimitPublicGroups", string.Empty, CopyLimitPublicGroupsPreferences );
            CopyPreferences( "CountsType", string.Empty, CopyCountsTypePreferences );
            CopyPreferences( "CampusFilter", string.Empty, CopyCampusFilterPreferences );
            CopyPreferences( "IncludeNoCampus", string.Empty, CopyIncludeNoCampusPreferences );
            CopyPreferences( "PrayerCardView_SelectedCampus", string.Empty, CopyPrayerCardViewSelectedCampusPreferences );
            CopyPreferences( "PersonProgramStepList.IsCardView", string.Empty, CopyPersonProgramStepListIsCardViewPreferences );
            CopyPreferences( "StreakMapEditorDateRange", string.Empty, CopyStreakMapEditorDateRangePreferences );
            CopyPreferences( "MyWorkflows_DisplayToggle", string.Empty, CopyMyWorkflowsDisplayTogglePreferences );
            CopyPreferences( "MyWorkflows_RoleToggle", string.Empty, CopyMyWorkflowsRoleTogglePreferences );

            CopyRemainingGlobalPreferences();

            PersonPreferenceCache.Clear();

            if ( _migrationErrors.Any() )
            {
                foreach ( var error in _migrationErrors )
                {
                    var exception = new Exception( error.Message, error.Exception );

                    ExceptionLogService.LogException( exception );
                }

                throw new Exception( "One or more person preferences failed to migrate completely, see exception log for details." );
            }

            DeleteJob();
        }

        #region Preference Type Processors

        private void CopyStandardBlockPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^block-([0-9]+)-(.+)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyGridPageSizePreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^grid-page-size-preference_([0-9]+)$" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-grid-page-size-preference", existingValues );
        }

        private void CopyGridFilterPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^grid-filter-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-grid-filter-{preferenceKey}", existingValues );
        }

        private void CopyAttendanceReportingPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^attendance-reporting-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopySmsConversationsPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^sms-conversations-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyGivingAnalyticsPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^giving-analytics-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyTransactionMatchingPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^transaction-matching-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyPledgeAnalyticsPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^pledge-analytics-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyPrayerSessionPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^prayer-session-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyPrayerCategoriesPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^prayer-categories-([0-9]+)-$" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-categories", existingValues );
        }

        private void CopyReportShowResultsPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^report-show-results-([0-9]+)$" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-show-results", existingValues );
        }

        private void CopyReportDataFilterPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^reportdata-filter-([0-9]+)-_(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyFormAnalyticsPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^form-analytics-([0-9]+)-(.*)" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();
            var preferenceKey = match.Groups[2].Value;

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue || preferenceKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-{preferenceKey}", existingValues );
        }

        private void CopyKeyAttributesPreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^Rock.KeyAttributes.([0-9]+)$" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockId = match.Groups[1].Value.AsIntegerOrNull();

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-selected-attributes", existingValues );
        }

        private void CopyCheckinShowInactivePreferences( int attributeId, string attributeKey )
        {
            var regex = new Regex( "^([0-9a-fA-F-]+)_showInactive$" );
            var match = regex.Match( attributeKey );

            // Invalid match, we processed it by mistake.
            if ( match == null || !match.Success )
            {
                return;
            }

            var blockGuid = match.Groups[1].Value.AsGuid();
            var blockId = BlockCache.GetId( blockGuid );

            // Invalid match, we processed it by mistake.
            if ( !blockId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, blockId.Value, $"block-{blockId}-show-inactive", existingValues );
        }

        private void CopyCurrentNavPathPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "CurrentNavPath" )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( null, null, "global-0-checkin-manager-current-nav-path", existingValues );
        }

        private void CopyContentChannelNavigationStatusFilterPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "ContentChannelNavigation_StatusFilter" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( SystemGuid.BlockType.CONTENT_CHANNEL_NAVIGATION.AsGuid() );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-status-filter", existingValues );
            }
        }

        private void CopyContentChannelNavigationSelectedChannelIdPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "ContentChannelNavigation_SelectedChannelId" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( SystemGuid.BlockType.CONTENT_CHANNEL_NAVIGATION.AsGuid() );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-selected-channel", existingValues );
            }
        }

        private void CopyContentChannelNavigationCategoryFilterPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "ContentChannelNavigation_CategoryFilter" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( SystemGuid.BlockType.CONTENT_CHANNEL_NAVIGATION.AsGuid() );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-category-filter", existingValues );
            }
        }

        private void CopyMyConnectionOpportunitiesTogglePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyConnectionOpportunities_Toggle" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-my-opportunities", existingValues );
            }
        }

        private void CopyMyConnectionOpportunitiesToggleShowActivePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyConnectionOpportunities_ToggleShowActive" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-show-active", existingValues );
            }
        }

        private void CopyMyConnectionOpportunitiesSelectedOpportunityPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyConnectionOpportunities_SelectedOpportunity" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-selected-opportunity", existingValues );
            }
        }

        private void CopyMyConnectionOpportunitiesSelectedCampusPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyConnectionOpportunities_SelectedCampus" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-selected-campus", existingValues );
            }
        }

        private void CopyConnectionRequestDetailCampusPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "ConnectionRequestDetail_Campus" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-default-campus", existingValues );
            }
        }

        private void CopyMyActiveOpportunitiesCheckedPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyActiveOpportunitiesChecked" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-my-active-opportunities", existingValues );
            }
        }

        private void CopyConnectionOpportunitiesSelectedCampusPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "ConnectionOpportunitiesSelectedCampus" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-selected-campus", existingValues );
            }
        }

        private void CopyContextDateRangePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "context-date-range" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "ABC4A04E-6FA8-4817-8113-A653251A16B3" ) );

            // Ehh, shouldn't happen, but lets be safe.
            if ( !blockTypeId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<BlockType>().Id, blockTypeId, $"block-type-{blockTypeId}-date-range", existingValues );
        }

        private void CopyHideInactiveItemsPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "HideInactiveItems" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "ADE003C7-649B-466A-872B-B8AC952E7841" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-hide-inactive-items", existingValues );
            }
        }

        private void CopyHideInactiveAccountsPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "HideInactiveAccounts" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6" ) );

            // Ehh, shouldn't happen, but lets be safe.
            if ( !blockTypeId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<BlockType>().Id, blockTypeId, $"block-type-{blockTypeId}-hide-inactive-accounts", existingValues );
        }

        private void CopyAttendanceListSortingTogglePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "Attendance_List_Sorting_Toggle" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( Rock.SystemGuid.BlockType.GROUP_ATTENDANCE_DETAIL ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-sort-by-last-name", existingValues );
            }
        }

        private void CopyHideInactiveGroupsPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "HideInactiveGroups" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-hide-inactive-groups", existingValues );
            }
        }

        private void CopyLimitPublicGroupsPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "LimitPublicGroups" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-limit-to-public", existingValues );
            }
        }

        private void CopyCountsTypePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "CountsType" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-counts-type", existingValues );
            }
        }

        private void CopyCampusFilterPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "CampusFilter" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" ) );

            // Ehh, shouldn't happen, but lets be safe.
            if ( !blockTypeId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<BlockType>().Id, blockTypeId, $"block-type-{blockTypeId}-campus-filter", existingValues );
        }

        private void CopyIncludeNoCampusPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "IncludeNoCampus" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" ) );

            // Ehh, shouldn't happen, but lets be safe.
            if ( !blockTypeId.HasValue )
            {
                return;
            }

            var existingValues = GetExistingAttributeValues( attributeId );

            CreateOrIgnorePersonPreferences( EntityTypeCache.Get<BlockType>().Id, blockTypeId, $"block-type-{blockTypeId}-include-no-campus", existingValues );
        }

        private void CopyPrayerCardViewSelectedCampusPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "PrayerCardView_SelectedCampus" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-selected-campus", existingValues );
            }
        }

        private void CopyPersonProgramStepListIsCardViewPreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "PersonProgramStepList.IsCardView" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-is-card-view", existingValues );
            }
        }

        private void CopyStreakMapEditorDateRangePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "StreakMapEditorDateRange" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "4DB69FBA-32C7-448A-B322-EDFBCEF2D124" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-date-range", existingValues );
            }
        }

        private void CopyMyWorkflowsDisplayTogglePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyWorkflows_DisplayToggle" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "689B434F-DD2D-464A-8DA3-21F8768BB5BF" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-display-toggle", existingValues );
            }
        }

        private void CopyMyWorkflowsRoleTogglePreferences( int attributeId, string attributeKey )
        {
            // Invalid match, we processed it by mistake.
            if ( attributeKey != "MyWorkflows_RoleToggle" )
            {
                return;
            }

            var blockTypeId = BlockTypeCache.GetId( new Guid( "689B434F-DD2D-464A-8DA3-21F8768BB5BF" ) );
            var blocks = BlockCache.All().Where( b => b.BlockTypeId == blockTypeId ).ToList();

            var existingValues = GetExistingAttributeValues( attributeId );

            foreach ( var block in blocks )
            {
                CreateOrIgnorePersonPreferences( EntityTypeCache.Get<Block>().Id, block.Id, $"block-{block.Id}-role-toggle", existingValues );
            }
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Helper method that initiates a copy operation for any attributes
        /// that match the prefix and suffix.
        /// </summary>
        /// <param name="prefix">The attribute key prefix.</param>
        /// <param name="suffix">The attribute key suffix.</param>
        /// <param name="processor">The processor to process each attribute individually.</param>
        private void CopyPreferences( string prefix, string suffix, Action<int, string> processor )
        {
            var personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;
            List<(int Id, string Key)> attributes;

            _convertedKeys.Add( (prefix, suffix) );

            // Load all the attributes that match the query.
            using ( var rockContext = CreateRockContext() )
            {
                var attributesQry = new AttributeService( rockContext )
                    .Queryable()
                    .Where( a => a.EntityTypeId == personEntityTypeId
                        && ( a.EntityTypeQualifierColumn == null || a.EntityTypeQualifierColumn == string.Empty )
                        && ( a.EntityTypeQualifierValue == null || a.EntityTypeQualifierValue == string.Empty ) );

                if ( prefix.IsNotNullOrWhiteSpace() )
                {
                    attributesQry = attributesQry.Where( a => a.Key.StartsWith( prefix ) );
                }

                if ( suffix.IsNotNullOrWhiteSpace() )
                {
                    attributesQry = attributesQry.Where( a => a.Key.EndsWith( suffix ) );
                }

                attributes = attributesQry
                    .Select( a => new
                    {
                        a.Id,
                        a.Key
                    } )
                    .ToList()
                    .Select( a => (a.Id, a.Key) )
                    .ToList();
            }

            // Process each attribute one at a time.
            foreach ( var (attributeId, attributeKey) in attributes )
            {
                try
                {
                    processor( attributeId, attributeKey );
                }
                catch ( Exception ex )
                {
                    _migrationErrors.Add( ($"Failed to migrate person preference for attribute #{attributeId}", ex) );
                }
            }
        }

        /// <summary>
        /// Copies any remaining preferences that have not already been copied.
        /// They are created as global preferences.
        /// </summary>
        private void CopyRemainingGlobalPreferences()
        {
            var personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;
            List<(int Id, string Key)> attributes;

            // Load all the attributes that match the query.
            using ( var rockContext = CreateRockContext() )
            {
                var attributesQry = new AttributeService( rockContext )
                    .Queryable()
                    .Where( a => a.EntityTypeId == personEntityTypeId
                        && ( a.EntityTypeQualifierColumn == null || a.EntityTypeQualifierColumn == string.Empty )
                        && ( a.EntityTypeQualifierValue == null || a.EntityTypeQualifierValue == string.Empty ) );

                foreach ( var (prefix, suffix) in _convertedKeys )
                {
                    if ( prefix.IsNotNullOrWhiteSpace() && suffix.IsNotNullOrWhiteSpace() )
                    {
                        attributesQry = attributesQry.Where( a => !( a.Key.StartsWith( prefix ) && a.Key.EndsWith( suffix ) ) );
                    }
                    else if ( prefix.IsNotNullOrWhiteSpace() )
                    {
                        attributesQry = attributesQry.Where( a => !a.Key.StartsWith( prefix ) );
                    }
                    else if ( suffix.IsNotNullOrWhiteSpace() )
                    {
                        attributesQry = attributesQry.Where( a => !a.Key.EndsWith( suffix ) );
                    }
                }

                attributes = attributesQry
                    .Select( a => new
                    {
                        a.Id,
                        a.Key
                    } )
                    .ToList()
                    .Select( a => (a.Id, a.Key) )
                    .ToList();
            }

            // Process each attribute one at a time.
            foreach ( var (attributeId, attributeKey) in attributes )
            {
                try
                {
                    // We only copy global preferences if the total size is 2KB or less.
                    var existingValues = GetExistingAttributeValues( attributeId )
                        .Where( v => v.Value.Length <= 2048 )
                        .ToList();

                    CreateOrIgnorePersonPreferences( null, null, $"global-0-{attributeKey}", existingValues );
                }
                catch ( Exception ex )
                {
                    _migrationErrors.Add( ($"Failed to migrate person preference for attribute #{attributeId}", ex) );
                }
            }
        }

        /// <summary>
        /// Gets the existing attribute values for the attribute identifier.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns>A list of <see cref="PreferenceValue"/> objects.</returns>
        private List<PreferenceValue> GetExistingAttributeValues( int attributeId )
        {
            using ( var rockContext = CreateRockContext() )
            {
                var personAliasQry = new PersonAliasService( rockContext ).Queryable();

                // Find all Attribute Values for this attribute that have a
                // non-null and non-empty value. Join to the PersonAlias
                // table and then filter to just the primary alias.
                return new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( av => av.AttributeId == attributeId
                        && !string.IsNullOrEmpty( av.Value ) )
                    .Join( personAliasQry, av => av.EntityId, pa => pa.PersonId, ( av, pa ) => new
                    {
                        AttributeValue = av,
                        PersonAlias = pa
                    } )
                    .Where( j => j.PersonAlias.AliasPersonId == j.PersonAlias.PersonId )
                    .Select( j => new PreferenceValue
                    {
                        PersonId = j.PersonAlias.PersonId,
                        PersonAliasId = j.PersonAlias.Id,
                        Value = j.AttributeValue.Value
                    } )
                    .ToList();
            }
        }

        /// <summary>
        /// Creates (or ignores if they already exist) the person preferences
        /// specified by the parameters.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="key">The key for the preferences.</param>
        /// <param name="preferenceValues">The preference values.</param>
        private void CreateOrIgnorePersonPreferences( int? entityTypeId, int? entityId, string key, List<PreferenceValue> preferenceValues )
        {
            while ( preferenceValues.Any() )
            {
                var preferenceValueChunk = preferenceValues.Take( 1_000 ).ToList();
                preferenceValues = preferenceValues.Skip( 1_000 ).ToList();

                var personIds = preferenceValueChunk.Select( pv => pv.PersonId ).ToList();

                // Intentionally bypass any methods in the service layer so that
                // we get exact filtering we desire during the migration.
                using ( var rockContext = CreateRockContext() )
                {
                    var personPreferenceService = new PersonPreferenceService( rockContext );
                    var existingPreferencesQry = personPreferenceService.Queryable();

                    if ( !entityTypeId.HasValue || !entityId.HasValue )
                    {
                        existingPreferencesQry = existingPreferencesQry
                            .Where( pp => !pp.EntityTypeId.HasValue && !pp.EntityId.HasValue );
                    }
                    else
                    {
                        existingPreferencesQry = existingPreferencesQry
                            .Where( pp => pp.EntityTypeId == entityTypeId && pp.EntityId == entityId );
                    }

                    var existingPreferencePersonIds = existingPreferencesQry
                        .Where( pp => personIds.Contains( pp.PersonAlias.PersonId )
                            && pp.Key == key )
                        .Select( pp => pp.PersonAlias.PersonId )
                        .ToList();

                    var preferencesToAdd = new List<PersonPreference>();

                    // Create all the person preference objects that don't
                    // already exist.
                    foreach ( var preferenceValue in preferenceValueChunk )
                    {
                        if ( existingPreferencePersonIds.Contains( preferenceValue.PersonId ) )
                        {
                            continue;
                        }

                        var personPreference = new PersonPreference
                        {
                            PersonAliasId = preferenceValue.PersonAliasId,
                            Key = key,
                            EntityTypeId = entityTypeId,
                            EntityId = entityId,
                            Value = preferenceValue.Value,
                            LastAccessedDateTime = RockDateTime.Now,
                        };

                        preferencesToAdd.Add( personPreference );
                    }

                    if ( preferencesToAdd.Any() )
                    {
                        // Try to save the changes in bulk. If something fails it
                        // is probably a conflict with a single preference, so if
                        // that happens try one at a time skipping the failures.
                        try
                        {
                            personPreferenceService.AddRange( preferencesToAdd );

                            // Disable pre-post processing so we don't flood
                            // the bus with cache invalidated messages. We will
                            // clear cache when we are all done.
                            rockContext.SaveChanges( new SaveChangesArgs
                            {
                                DisablePrePostProcessing = true
                            } );
                        }
                        catch
                        {
                            CreateOrIgnorePersonPreferencesOneByOne( preferencesToAdd );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates (or ignores if they already exist) the person preference
        /// records specified in the parameters. These are created one at a
        /// time so that any errors during the save operation can be ignored.
        /// </summary>
        /// <param name="personPreferences">The person preferences.</param>
        private void CreateOrIgnorePersonPreferencesOneByOne( List<PersonPreference> personPreferences )
        {
            foreach ( var personPreference in personPreferences )
            {
                try
                {
                    using ( var rockContext = CreateRockContext() )
                    {
                        var personPreferenceService = new PersonPreferenceService( rockContext );

                        var newPreference = new PersonPreference
                        {
                            PersonAliasId = personPreference.PersonAliasId,
                            Key = personPreference.Key,
                            EntityTypeId = personPreference.EntityTypeId,
                            EntityId = personPreference.EntityId,
                            Value = personPreference.Value,
                            LastAccessedDateTime = personPreference.LastAccessedDateTime
                        };

                        personPreferenceService.Add( newPreference );

                        // Disable pre-post processing so we don't flood
                        // the bus with cache invalidated messages. We will
                        // clear cache when we are all done.
                        rockContext.SaveChanges( new SaveChangesArgs {
                            DisablePrePostProcessing = true
                        } );
                    }
                }
                catch
                {
                    // Ignore individual errors as that likely means a duplicate
                    // record already exists, in which case we ignore it.
                }
            }
        }

        /// <summary>
        /// Creates the rock context with the correct timeout.
        /// </summary>
        /// <returns>A new instance of RockContext.</returns>
        private RockContext CreateRockContext()
        {
            var rockContext = new RockContext();

            rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            return rockContext;
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }

        #endregion

        private class PreferenceValue
        {
            public int PersonId { get; set; }

            public int PersonAliasId { get; set; }

            public string Value { get; set; }
        }
    }
}
