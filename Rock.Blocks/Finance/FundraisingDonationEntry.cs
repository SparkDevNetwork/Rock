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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FundraisingDonationEntry;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Prompts the donor for a fundraising opportunity and participant, then navigates to the
    /// configured transaction entry page so the gift can be attributed to that participant.
    /// </summary>
    [DisplayName( "Fundraising Donation Entry" )]
    [Category( "Fundraising" )]
    [Description( "Block that starts out a Fundraising Donation by prompting for information prior to going to a TransactionEntry block" )]
    [IconCssClass( "ti ti-cash" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Transaction Entry Page",
        Description = "The Transaction Entry page to navigate to after prompting for the Fundraising Specific inputs",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.TransactionEntryPage )]

    [BooleanField( "Show First Name Only",
        Description = "Only show the First Name of each participant instead of Full Name",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.ShowFirstNameOnly )]

    [BooleanField( "Allow Automatic Selection",
        Description = "If enabled and there is only one participant and registrations are not enabled then that participant will automatically be selected and this page will get bypassed.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.AllowAutomaticSelection )]

    [GroupField( "Root Group",
        Description = "Select the group that will be used as the base of the list.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.RootGroup )]

    [SystemGuid.EntityTypeGuid( "B98F1E6E-0ACB-4BB8-9BE9-652975879916" )]
    // was [SystemGuid.BlockTypeGuid( "087D8634-EDC7-44AC-91DF-13A70FB36385" )]
    [Rock.SystemGuid.BlockTypeGuid( "A24D68F2-C58B-4322-AED8-6556DBED1B76" )]
    public class FundraisingDonationEntry : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TransactionEntryPage = "TransactionEntryPage";
            public const string ShowFirstNameOnly = "ShowFirstNameOnly";
            public const string AllowAutomaticSelection = "AllowAutomaticSelection";
            public const string RootGroup = "RootGroup";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
        }

        /// <summary>
        /// Query string keys passed to the configured transaction entry page.
        /// </summary>
        private static class TargetPageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string ParticipationMode = "ParticipationMode";
            public const string AccountIds = "AccountIds";
            public const string AmountLimit = "AmountLimit";
        }

        /// <summary>
        /// Attribute keys for the fundraising opportunity group and its members.
        /// </summary>
        private static class FundraisingAttributeKey
        {
            public const string OpportunityTitle = "OpportunityTitle";
            public const string OpportunityDateRange = "OpportunityDateRange";
            public const string AllowDonationsUntil = "AllowDonationsUntil";
            public const string ParticipationType = "ParticipationType";
            public const string FinancialAccount = "FinancialAccount";
            public const string CapFundraisingAmount = "CapFundraisingAmount";
            public const string RegistrationInstance = "RegistrationInstance";
            public const string IndividualFundraisingGoal = "IndividualFundraisingGoal";
            public const string DisablePublicContributionRequests = "DisablePublicContributionRequests";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<FundraisingDonationEntryBag, FundraisingDonationEntryOptionsBag>();
            var bag = new FundraisingDonationEntryBag();

            box.Options.IsTransactionEntryPageConfigured = GetAttributeValue( AttributeKey.TransactionEntryPage ).IsNotNullOrWhiteSpace();

            var group = GetGroupFromParameters( out var groupMember );

            // When the participant is already known, skip the form and go straight to payment.
            if ( groupMember != null )
            {
                bag.RedirectUrl = GetNextPageUrl( groupMember );
                box.Bag = bag;
                return box;
            }

            if ( group != null )
            {
                group.LoadAttributes( RockContext );

                // Single-participant projects can bypass this page entirely when configured to do so.
                if ( GetAttributeValue( AttributeKey.AllowAutomaticSelection ).AsBoolean() )
                {
                    var autoSelectedMember = GetAutoSelectedMember( group );
                    if ( autoSelectedMember != null )
                    {
                        bag.RedirectUrl = GetNextPageUrl( autoSelectedMember );
                        box.Bag = bag;
                        return box;
                    }
                }

                // Reflect the chosen opportunity in the page and browser titles.
                var opportunityTitle = group.GetAttributeValue( FundraisingAttributeKey.OpportunityTitle );
                var pageTitle = $"Donate to {opportunityTitle}";
                ResponseContext.SetPageTitle( pageTitle );
                ResponseContext.SetBrowserTitle( pageTitle );

                bag.IsOpportunityLocked = true;
                bag.OpportunityTitle = opportunityTitle;
                bag.SelectedOpportunityValue = group.IdKey;
                bag.ParticipantOptions = BuildParticipantOptions( group );
            }
            else
            {
                bag.OpportunityOptions = BuildOpportunityOptions();
            }

            box.Bag = bag;
            return box;
        }

        /// <summary>
        /// Resolves the fundraising opportunity (and participant, when supplied) from the query string.
        /// </summary>
        /// <param name="groupMember">The resolved participant, or <c>null</c> when one was not supplied.</param>
        /// <returns>The resolved fundraising opportunity group, or <c>null</c>.</returns>
        private Rock.Model.Group GetGroupFromParameters( out GroupMember groupMember )
        {
            groupMember = null;

            // Integer ids are still accepted here so existing links into this page keep working.
            var groupMemberKey = PageParameter( PageParameterKey.GroupMemberId );
            if ( groupMemberKey.IsNotNullOrWhiteSpace() )
            {
                groupMember = new GroupMemberService( RockContext ).Get( groupMemberKey, true );
                return groupMember?.Group;
            }

            var groupKey = PageParameter( PageParameterKey.GroupId );
            if ( groupKey.IsNotNullOrWhiteSpace() )
            {
                return new GroupService( RockContext ).Get( groupKey, true );
            }

            return null;
        }

        /// <summary>
        /// Gets the only eligible participant when the opportunity qualifies for automatic selection.
        /// </summary>
        /// <param name="group">The fundraising opportunity group (attributes already loaded).</param>
        /// <returns>The single active, non-leader participant, or <c>null</c> when the opportunity does not qualify.</returns>
        private GroupMember GetAutoSelectedMember( Rock.Model.Group group )
        {
            // Automatic selection does not apply when a registration instance gates participation.
            if ( group.GetAttributeValue( FundraisingAttributeKey.RegistrationInstance ).IsNotNullOrWhiteSpace() )
            {
                return null;
            }

            var nonLeaderMembers = new GroupMemberService( RockContext ).Queryable()
                .Where( m => m.GroupId == group.Id && !m.GroupRole.IsLeader )
                .ToList();

            if ( nonLeaderMembers.Count == 1 && nonLeaderMembers[0].GroupMemberStatus == GroupMemberStatus.Active )
            {
                return nonLeaderMembers[0];
            }

            return null;
        }

        /// <summary>
        /// Builds the list of fundraising opportunities that are active, have members, and are still
        /// accepting donations.
        /// </summary>
        /// <returns>The selectable fundraising opportunities.</returns>
        private List<ListItemBag> BuildOpportunityOptions()
        {
            var fundraisingGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;

            var groupQuery = new GroupService( RockContext ).Queryable()
                .Where( g => ( g.GroupTypeId == fundraisingGroupTypeId || g.GroupType.InheritedGroupTypeId == fundraisingGroupTypeId )
                    && g.IsActive
                    && g.Members.Any() );

            var rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
            if ( rootGroupGuid.HasValue )
            {
                var rootGroupIds = GetRootGroupIds( rootGroupGuid.Value );
                groupQuery = groupQuery.Where( g => rootGroupIds.Contains( g.Id ) );
            }

            var opportunities = groupQuery.OrderBy( g => g.Order ).ThenBy( g => g.Name ).ToList();
            opportunities.LoadAttributes<Rock.Model.Group>( RockContext );

            var options = new List<ListItemBag>();
            foreach ( var opportunity in opportunities )
            {
                var allowDonationsUntil = opportunity.GetAttributeValue( FundraisingAttributeKey.AllowDonationsUntil ).AsDateTime();
                var dateRangeEnd = DateRange.FromDelimitedValues( opportunity.GetAttributeValue( FundraisingAttributeKey.OpportunityDateRange ) ).End;
                var donationWindowEnd = allowDonationsUntil ?? dateRangeEnd ?? DateTime.MaxValue;

                if ( RockDateTime.Now > donationWindowEnd )
                {
                    continue;
                }

                var title = opportunity.GetAttributeValue( FundraisingAttributeKey.OpportunityTitle );
                options.Add( new ListItemBag
                {
                    Value = opportunity.IdKey,
                    Text = title.IsNotNullOrWhiteSpace() ? title : opportunity.Name
                } );
            }

            return options;
        }

        /// <summary>
        /// Gets the root group and all of its descendant group ids used to scope the opportunity list.
        /// </summary>
        /// <param name="rootGroupGuid">The configured root group unique identifier.</param>
        /// <returns>The root group id together with every descendant group id.</returns>
        private List<int> GetRootGroupIds( Guid rootGroupGuid )
        {
            var groupService = new GroupService( RockContext );
            var rootGroup = groupService.Get( rootGroupGuid );

            var ids = new List<int>();
            if ( rootGroup != null )
            {
                ids.Add( rootGroup.Id );
                ids.AddRange( groupService.GetAllDescendentGroupIds( rootGroup.Id, false ) );
            }

            return ids;
        }

        /// <summary>
        /// Builds the participant list for a fundraising opportunity, honoring its participation mode.
        /// </summary>
        /// <param name="group">The fundraising opportunity group.</param>
        /// <returns>The selectable participants.</returns>
        private List<ListItemBag> BuildParticipantOptions( Rock.Model.Group group )
        {
            if ( group.Attributes == null )
            {
                group.LoadAttributes( RockContext );
            }

            var participationMode = group.GetAttributeValue( FundraisingAttributeKey.ParticipationType ).ConvertToEnumOrNull<ParticipationType>() ?? ParticipationType.Individual;

            var members = new GroupMemberService( RockContext ).Queryable()
                .Where( m => m.GroupId == group.Id && m.GroupMemberStatus == GroupMemberStatus.Active )
                .Include( m => m.Person )
                .OrderBy( m => m.Person.NickName )
                .ThenBy( m => m.Person.LastName )
                .ToList();

            members.LoadAttributes<GroupMember>( RockContext );

            if ( participationMode == ParticipationType.Family )
            {
                return BuildFamilyParticipantOptions( members );
            }

            // Individual participation excludes members who opted out of public contribution requests.
            var eligibleMembers = members
                .Where( m => !m.GetAttributeValue( FundraisingAttributeKey.DisablePublicContributionRequests ).AsBoolean() )
                .ToList();

            return BuildIndividualParticipantOptions( eligibleMembers );
        }

        /// <summary>
        /// Builds one participant option per eligible member.
        /// </summary>
        /// <param name="members">The eligible group members.</param>
        /// <returns>The participant options.</returns>
        private List<ListItemBag> BuildIndividualParticipantOptions( List<GroupMember> members )
        {
            var showOnlyFirstName = GetAttributeValue( AttributeKey.ShowFirstNameOnly ).AsBoolean();

            return members.Select( m => new ListItemBag
            {
                Value = m.IdKey,
                Text = showOnlyFirstName ? m.Person.NickName : m.Person.FullName
            } ).ToList();
        }

        /// <summary>
        /// Builds one participant option per family, grouping the active members by their primary family.
        /// </summary>
        /// <param name="members">The active group members, including any who opted out of public contribution requests.</param>
        /// <returns>The participant options.</returns>
        private List<ListItemBag> BuildFamilyParticipantOptions( List<GroupMember> members )
        {
            // Group by the scalar primary-family key to avoid lazy-loading the family navigation per member.
            var membersByFamily = members
                .Where( m => m.Person.PrimaryFamilyId.HasValue )
                .GroupBy( m => m.Person.PrimaryFamilyId.Value )
                .ToList();

            // Resolve all family names in a single query.
            var familyIds = membersByFamily.Select( fm => fm.Key ).ToList();
            var familyNames = new GroupService( RockContext ).Queryable()
                .Where( g => familyIds.Contains( g.Id ) )
                .Select( g => new { g.Id, g.Name } )
                .ToDictionary( g => g.Id, g => g.Name );

            var options = new List<ListItemBag>();
            foreach ( var familyGroup in membersByFamily )
            {
                var sortedMembers = familyGroup
                    .OrderBy( m => m.Person.AgeClassification )
                    .ThenBy( m => m.Person.Gender )
                    .ToList();

                var representative = sortedMembers.First();

                string text;
                if ( sortedMembers.Count == 1 )
                {
                    text = representative.Person.FullName;
                }
                else
                {
                    familyNames.TryGetValue( familyGroup.Key, out var familyName );
                    var nickNames = sortedMembers.Select( m => m.Person.NickName )
                        .JoinStringsWithRepeatAndFinalDelimiterWithMaxLength( ", ", " & ", 36 );
                    text = $"{familyName} ({nickNames})";
                }

                options.Add( new ListItemBag
                {
                    Value = representative.IdKey,
                    Text = text
                } );
            }

            return options;
        }

        /// <summary>
        /// Builds the transaction entry page URL for the selected participant, including the participation
        /// mode, financial account, and (when the opportunity caps giving) the remaining amount.
        /// </summary>
        /// <param name="groupMember">The selected participant.</param>
        /// <returns>The transaction entry page URL, or <c>null</c>/empty when the page is not configured.</returns>
        private string GetNextPageUrl( GroupMember groupMember )
        {
            if ( groupMember == null )
            {
                return null;
            }

            groupMember.LoadAttributes( RockContext );
            var group = groupMember.Group;
            group.LoadAttributes( RockContext );

            var participationMode = group.GetAttributeValue( FundraisingAttributeKey.ParticipationType ).ConvertToEnumOrNull<ParticipationType>() ?? ParticipationType.Individual;

            var queryParams = new Dictionary<string, string>
            {
                [TargetPageParameterKey.GroupId] = group.IdKey,
                [TargetPageParameterKey.GroupMemberId] = groupMember.IdKey,
                [TargetPageParameterKey.ParticipationMode] = ( ( int ) participationMode ).ToString()
            };

            var financialAccountGuid = group.GetAttributeValue( FundraisingAttributeKey.FinancialAccount ).AsGuidOrNull();
            if ( financialAccountGuid.HasValue )
            {
                var financialAccount = FinancialAccountCache.Get( financialAccountGuid.Value );
                if ( financialAccount != null )
                {
                    queryParams[TargetPageParameterKey.AccountIds] = financialAccount.Id.ToString();
                }
            }

            if ( group.GetAttributeValue( FundraisingAttributeKey.CapFundraisingAmount ).AsBoolean() )
            {
                var amountLeft = GetRemainingFundraisingAmount( groupMember, group, participationMode );
                queryParams[TargetPageParameterKey.AmountLimit] = amountLeft.ToString();
            }

            return this.GetLinkedPageUrl( AttributeKey.TransactionEntryPage, queryParams );
        }

        /// <summary>
        /// Calculates the remaining fundraising amount (goal minus contributions) for the participant,
        /// summing across the family when the opportunity uses family participation.
        /// </summary>
        /// <param name="groupMember">The selected participant.</param>
        /// <param name="group">The fundraising opportunity group (attributes already loaded).</param>
        /// <param name="participationMode">The opportunity's participation mode.</param>
        /// <returns>The remaining fundraising amount.</returns>
        private decimal GetRemainingFundraisingAmount( GroupMember groupMember, Rock.Model.Group group, ParticipationType participationMode )
        {
            var groupMemberEntityTypeId = EntityTypeCache.GetId<GroupMember>();

            if ( participationMode == ParticipationType.Family )
            {
                var familyMembers = new GroupService( RockContext )
                    .GroupMembersInAnotherGroup( groupMember.Person.GetFamily(), group )
                    .ToList();

                familyMembers.LoadAttributes<GroupMember>( RockContext );

                decimal fundraisingGoal = 0;
                foreach ( var member in familyMembers )
                {
                    fundraisingGoal += member.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                        ?? group.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                        ?? 0;
                }

                var contributionTotal = new FinancialTransactionDetailService( RockContext )
                    .GetContributionsForGroupMemberList( groupMemberEntityTypeId, familyMembers.Select( m => m.Id ).ToList() );

                return fundraisingGoal - contributionTotal;
            }

            var memberFundraisingGoal = groupMember.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                ?? group.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                ?? 0;

            var individualContributionTotal = new FinancialTransactionDetailService( RockContext ).Queryable()
                .Where( d => d.EntityTypeId == groupMemberEntityTypeId && d.EntityId == groupMember.Id )
                .Sum( a => ( decimal? ) a.Amount ) ?? 0.00M;

            return memberFundraisingGoal - individualContributionTotal;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the participants available for the selected fundraising opportunity.
        /// </summary>
        /// <param name="opportunityKey">The identifier of the selected fundraising opportunity.</param>
        /// <returns>The participant options.</returns>
        [BlockAction]
        public BlockActionResult GetParticipants( string opportunityKey )
        {
            var group = new GroupService( RockContext ).Get( opportunityKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( group == null )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            return ActionOk( BuildParticipantOptions( group ) );
        }

        /// <summary>
        /// Gets the transaction entry page URL for the selected participant.
        /// </summary>
        /// <param name="participantKey">The identifier of the selected participant.</param>
        /// <returns>The URL the donor should be navigated to.</returns>
        [BlockAction]
        public BlockActionResult GetNextUrl( string participantKey )
        {
            var groupMember = new GroupMemberService( RockContext ).Get( participantKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( groupMember == null )
            {
                return ActionBadRequest( "The selected participant could not be found." );
            }

            var url = GetNextPageUrl( groupMember );
            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The Transaction Entry page has not been configured." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions
    }
}
