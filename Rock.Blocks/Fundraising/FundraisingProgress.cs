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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Fundraising.FundraisingProgress;
using Rock.Web.Cache;

namespace Rock.Blocks.Fundraising
{
    /// <summary>
    /// Displays the fundraising progress for all participants (or families) in a fundraising opportunity.
    /// </summary>
    [DisplayName( "Fundraising Progress" )]
    [Category( "Fundraising" )]
    [Description( "Progress for all people in a fundraising opportunity" )]
    [IconCssClass( "ti ti-certificate" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "2306D5F4-24FF-4B12-AB9C-598C884737B0" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "D8F3F3D7-EA50-4037-BCBC-6D0FF00FFBA1" )]
    [Rock.SystemGuid.BlockTypeGuid( "75D2BC14-34DF-42EA-8DBB-3F5294B290A9" )]
    public class FundraisingProgress : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
        }

        private static class FundraisingAttributeKey
        {
            public const string IndividualFundraisingGoal = "IndividualFundraisingGoal";
            public const string ParticipationType = "ParticipationType";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <summary>
        /// Builds the initialization box (the view payload) for the block.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private FundraisingProgressInitializationBox GetInitializationBox()
        {
            var box = new FundraisingProgressInitializationBox();

            var groupTypeFundraisingId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;

            Rock.Model.Group group = null;
            GroupMember groupMember = null;

            var groupParam = PageParameter( PageParameterKey.GroupId );

            if ( groupParam.IsNotNullOrWhiteSpace() )
            {
                group = new GroupService( RockContext ).Get( groupParam, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                var groupMemberParam = PageParameter( PageParameterKey.GroupMemberId );

                if ( groupMemberParam.IsNotNullOrWhiteSpace() )
                {
                    groupMember = new GroupMemberService( RockContext ).GetInclude( groupMemberParam, gm => gm.Group, !PageCache.Layout.Site.DisablePredictableIds );
                    group = groupMember?.Group;
                }
            }

            if ( group == null || ( group.GroupTypeId != groupTypeFundraisingId && group.GroupType.InheritedGroupTypeId != groupTypeFundraisingId ) )
            {
                box.ErrorMessage = "No Fundraising Opportunity Group found";
                return box;
            }

            box.IsGroupTotalVisible = groupMember == null;
            box.Title = group.Name;
            box.ProgressItems = GetProgressItems( group, groupMember );

            if ( box.IsGroupTotalVisible )
            {
                box.GroupFundraisingGoal = box.ProgressItems.Sum( p => p.FundraisingGoal );
                box.GroupContributionTotal = box.ProgressItems.Sum( p => p.ContributionTotal );
                // The group goal is a non-null sum, so this always resolves to a value; the ?? 0
                // only satisfies the nullable return type of GetPercentComplete.
                box.PercentComplete = GetPercentComplete( box.GroupFundraisingGoal, box.GroupContributionTotal ) ?? 0;
            }

            return box;
        }

        /// <summary>
        /// Determines the participation mode and dispatches to the appropriate progress builder.
        /// When a <paramref name="groupMember"/> is provided the query is narrowed to that
        /// member (individual mode) or that member's family (family mode) before dispatch.
        /// </summary>
        private List<FundraisingProgressBag> GetProgressItems( Rock.Model.Group group, GroupMember groupMember )
        {
            group.LoadAttributes( RockContext );

            var participationMode = group.GetAttributeValue( FundraisingAttributeKey.ParticipationType ).ConvertToEnumOrNull<ParticipationType>() ?? ParticipationType.Individual;
            var defaultGoal = group.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull();

            var groupMemberQry = new GroupMemberService( RockContext ).Queryable().Where( gm => gm.GroupId == group.Id );

            if ( participationMode == ParticipationType.Individual )
            {
                if ( groupMember != null )
                {
                    groupMemberQry = groupMemberQry.Where( gm => gm.Id == groupMember.Id );
                }

                return GetIndividualProgressItems( groupMemberQry, defaultGoal );
            }
            else
            {
                if ( groupMember != null )
                {
                    groupMemberQry = groupMemberQry.Where( gm => gm.Person.PrimaryFamilyId == groupMember.Person.PrimaryFamilyId );
                }

                return GetFamilyProgressItems( groupMemberQry, defaultGoal );
            }
        }

        /// <summary>
        /// Builds the progress bags for individual participation mode. Members are sorted by
        /// last name then nick name to match the legacy WebForms ordering. Attributes and
        /// contribution totals are loaded in bulk to avoid per-member queries.
        /// </summary>
        /// <param name="groupMemberQry">
        /// A queryable scoped to the relevant group members (all members, or a single member
        /// when a <c>GroupMemberId</c> page parameter was provided).
        /// </param>
        /// <param name="defaultGoal">The group-level fundraising goal used when a member has no individual goal set. Null when no group-level goal is configured.</param>
        private List<FundraisingProgressBag> GetIndividualProgressItems( IQueryable<GroupMember> groupMemberQry, decimal? defaultGoal )
        {
            var progressItems = new List<FundraisingProgressBag>();

            var groupMembers = groupMemberQry
                .Include( gm => gm.Person )
                .ToList()
                .OrderBy( gm => gm.Person.LastName )
                .ThenBy( gm => gm.Person.NickName )
                .ToList();

            if ( !groupMembers.Any() )
            {
                return progressItems;
            }

            var groupMemberIds = groupMembers.Select( gm => gm.Id ).ToList();

            groupMembers.LoadFilteredAttributes( a => a.Key == FundraisingAttributeKey.IndividualFundraisingGoal );

            var contributionTotalsByMemberId = GetContributionTotals( groupMemberIds );

            foreach ( var member in groupMembers )
            {
                progressItems.Add( BuildMemberProgressBag( member, defaultGoal, contributionTotalsByMemberId ) );
            }

            return progressItems;
        }

        /// <summary>
        /// Builds the progress bags for family participation mode. Members are grouped by
        /// primary family in memory using the pre-scoped query, so no per-family re-queries
        /// are needed. Contribution totals are fetched in a single bulk query.
        /// </summary>
        /// <param name="groupMemberQry">
        /// A queryable scoped to the relevant group members (all members, or a single family
        /// when a <c>GroupMemberId</c> page parameter was provided).
        /// </param>
        /// <param name="defaultGoal">The group-level fundraising goal used when a member has no individual goal set. Null when no group-level goal is configured.</param>
        private List<FundraisingProgressBag> GetFamilyProgressItems( IQueryable<GroupMember> groupMemberQry, decimal? defaultGoal )
        {
            var progressItems = new List<FundraisingProgressBag>();

            var groupMembers = groupMemberQry
                .Include( gm => gm.Person )
                .Include( gm => gm.Person.PrimaryFamily )
                .ToList();

            if ( !groupMembers.Any() )
            {
                return progressItems;
            }

            var groupMemberIds = groupMembers.Select( gm => gm.Id ).ToList();

            groupMembers.LoadFilteredAttributes( a => a.Key == FundraisingAttributeKey.IndividualFundraisingGoal );

            var contributionTotalsByMemberId = GetContributionTotals( groupMemberIds );

            // Order families alphabetically by name to match the legacy WebForms ordering
            // ( .Select( PrimaryFamily ).Distinct().OrderBy( Name ) ).
            var families = groupMembers
                .GroupBy( gm => gm.Person.PrimaryFamilyId )
                .OrderBy( family => family.First().Person.PrimaryFamily?.Name );

            foreach ( var family in families )
            {
                var orderedFamilyMembers = family
                    .OrderBy( gm => gm.Person.AgeClassification )
                    .ThenBy( gm => gm.Person.Gender )
                    .ToList();

                // Build a child row for each member so the family row can be expanded in the UI.
                var childItems = orderedFamilyMembers
                    .Select( m => BuildMemberProgressBag( m, defaultGoal, contributionTotalsByMemberId ) )
                    .ToList();

                // Derive the family totals from the children so the parent and child rows always agree.
                var familyGoal = childItems.Sum( c => c.FundraisingGoal );
                var contributionTotal = childItems.Sum( c => c.ContributionTotal );

                var familyName = orderedFamilyMembers.First().Person.PrimaryFamily?.Name ?? string.Empty;

                var isSingleMemberFamily = orderedFamilyMembers.Count == 1;

                var progressTitle = isSingleMemberFamily
                    ? orderedFamilyMembers.First().Person.FullName
                    : familyName;

                progressItems.Add( new FundraisingProgressBag
                {
                    ProgressTitle = progressTitle,
                    FundraisingGoal = familyGoal,
                    ContributionTotal = contributionTotal,
                    PercentComplete = GetPercentComplete( familyGoal, contributionTotal ),

                    // A single-member family row represents one person, so surface that person's
                    // photo on the parent row rather than falling back to the family icon.
                    PhotoUrl = isSingleMemberFamily ? childItems.First().PhotoUrl : null,
                    ChildItems = childItems
                } );
            }

            return progressItems;
        }

        /// <summary>
        /// Builds a single progress bag for one group member, resolving the member's individual
        /// fundraising goal (falling back to the group default) and contribution total.
        /// </summary>
        /// <param name="member">The group member. Attributes must already be loaded.</param>
        /// <param name="defaultGoal">The group-level fundraising goal used when the member has no individual goal set. Null when no group-level goal is configured.</param>
        /// <param name="contributionTotalsByMemberId">The bulk-loaded contribution totals keyed by group member ID.</param>
        private FundraisingProgressBag BuildMemberProgressBag( GroupMember member, decimal? defaultGoal, Dictionary<int, decimal> contributionTotalsByMemberId )
        {
            // Fall back to the group-level goal when the member has no individual goal. This can
            // still be null when neither is configured, which is intentionally distinct from a
            // goal of zero (see GetPercentComplete).
            var memberGoal = member.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull() ?? defaultGoal;
            var contributionTotal = contributionTotalsByMemberId.TryGetValue( member.Id, out var total ) ? total : 0;

            var photoUrl = Person.GetPersonPhotoUrl( member.Person );

            return new FundraisingProgressBag
            {
                ProgressTitle = member.Person.FullName,
                FundraisingGoal = memberGoal ?? 0,
                ContributionTotal = contributionTotal,
                PercentComplete = GetPercentComplete( memberGoal, contributionTotal ),
                PhotoUrl = photoUrl
            };
        }

        /// <summary>
        /// Returns a dictionary mapping GroupMember ID to total contribution amount for
        /// the given member IDs. Members with no contributions are absent from the dictionary.
        /// </summary>
        private Dictionary<int, decimal> GetContributionTotals( List<int> memberIds )
        {
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();

            return new FinancialTransactionDetailService( RockContext ).Queryable()
                .Where( t => t.EntityTypeId == entityTypeIdGroupMember && t.EntityId != null && memberIds.Contains( t.EntityId.Value ) )
                .GroupBy( d => d.EntityId.Value )
                .Select( g => new { GroupMemberId = g.Key, Total = g.Sum( d => d.Amount ) } )
                .ToDictionary( x => x.GroupMemberId, x => x.Total );
        }

        /// <summary>
        /// Returns the percentage of the goal achieved.
        /// <para>
        /// A <c>null</c> goal means no goal was ever configured (neither an individual nor a
        /// group-level goal); this returns <c>null</c> so the UI can omit the percentage entirely
        /// rather than treating an unconfigured member as 0% or fully funded. A configured goal of
        /// zero returns 100 (fully funded by definition), matching WebForms.
        /// </para>
        /// </summary>
        private decimal? GetPercentComplete( decimal? goal, decimal contributionTotal )
        {
            if ( goal == null )
            {
                return null;
            }

            return goal.Value > 0 ? decimal.Round( ( contributionTotal / goal.Value ) * 100, 2 ) : 100;
        }

        #endregion Methods
    }
}
