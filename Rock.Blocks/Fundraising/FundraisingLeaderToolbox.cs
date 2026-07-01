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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Fundraising.FundraisingLeaderToolbox;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Fundraising
{
    /// <summary>
    /// The Leader Toolbox for a fundraising opportunity. Displays opportunity summary
    /// details and the list of active participants to a leader of the opportunity group.
    /// </summary>
    [DisplayName( "Fundraising Leader Toolbox" )]
    [Category( "Fundraising" )]
    [Description( "The Leader Toolbox for a fundraising opportunity" )]
    [IconCssClass( "ti ti-briefcase" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Summary Lava Template",
        Key = AttributeKey.SummaryLavaTemplate,
        Description = "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"
<h1>{{ Group | Attribute:'OpportunityTitle' }}</h1>
{% assign dateRangeParts = Group | Attribute:'OpportunityDateRange','RawValue' | Split:',' %}
{% assign dateRangePartsSize = dateRangeParts | Size %}
{% if dateRangePartsSize == 2 %}
    {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }} to {{ dateRangeParts[1] | Date:'MMMM dd, yyyy' }}<br/>
{% elsif dateRangePartsSize == 1 %}
    {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }}
{% endif %}
{{ Group | Attribute:'OpportunityLocation' }}

<br />
<br />
<p>
{{ Group | Attribute:'OpportunitySummary' }}
</p>
",
        Order = 1 )]

    [LinkedPage( "Participant Page",
        Key = AttributeKey.ParticipantPage,
        Description = "The participant page for a participant of this fundraising opportunity",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Main Page",
        Key = AttributeKey.MainPage,
        Description = "The main page for the fundraising opportunity",
        IsRequired = false,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "D6901D38-D13D-49CF-A8D1-EB3F68110EFF" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "D1491C02-B860-4933-B2FC-BA8E96CF2639" )]
     [Rock.SystemGuid.BlockTypeGuid( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3" )]
    [CustomizedGrid]
    public class FundraisingLeaderToolbox : RockListBlockType<FundraisingLeaderToolbox.ParticipantRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SummaryLavaTemplate = "SummaryLavaTemplate";
            public const string ParticipantPage = "ParticipantPage";
            public const string MainPage = "MainPage";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
        }

        private static class NavigationUrlKey
        {
            public const string ParticipantPage = "ParticipantPage";
            public const string MainPage = "MainPage";
        }

        /// <summary>
        /// Attribute keys for the fundraising opportunity group and its members.
        /// </summary>
        private static class FundraisingAttributeKey
        {
            public const string OpportunityPhoto = "OpportunityPhoto";
            public const string IndividualFundraisingGoal = "IndividualFundraisingGoal";
            public const string DisablePublicContributionRequests = "DisablePublicContributionRequests";
        }

        #endregion Keys

        #region Fields

        private Rock.Model.Group _group;
        private bool _isGroupLoaded;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the fundraising opportunity group identifier, resolved from the loaded group.
        /// Returns <c>0</c> when no group could be found for the page parameter.
        /// </summary>
        private int GroupId => GetGroup()?.Id ?? 0;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FundraisingLeaderToolboxOptionsBag>();
            var builder = GetGridBuilder();

            // This block neither adds nor deletes participants from the grid.
            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FundraisingLeaderToolboxOptionsBag GetBoxOptions()
        {
            var options = new FundraisingLeaderToolboxOptionsBag();

            var group = GetGroup();

            // The opportunity group is required; without it there is nothing to display.
            if ( group == null )
            {
                options.ErrorMessage = "No fundraising opportunity was specified.";
                return options;
            }

            // Only a leader of the opportunity group may view the toolbox. When the current
            // person is not a leader the toolbox content is hidden and a message is shown.
            if ( !IsCurrentPersonLeader() )
            {
                options.ErrorMessage = "You are not a leader of this fundraising opportunity.";
                return options;
            }

            options.GroupName = group.Name;

            // Build the left-sidebar photo URL from the opportunity photo attribute.
            var photoGuid = group.GetAttributeValue( FundraisingAttributeKey.OpportunityPhoto ).AsGuidOrNull();
            if ( photoGuid.HasValue )
            {
                options.PhotoUrl = FileUrlHelper.GetImageUrl( photoGuid.Value );
            }

            // Resolve the configured Summary Lava Template against the opportunity group.
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            options.SummaryHtml = GetAttributeValue( AttributeKey.SummaryLavaTemplate ).ResolveMergeFields( mergeFields );

            return options;
        }

        /// <summary>
        /// Gets the URLs the block navigates to for linked pages.
        /// </summary>
        /// <returns>A dictionary of navigation keys to URLs.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParticipantPage] = this.GetLinkedPageUrl( AttributeKey.ParticipantPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, GroupId.ToString() },
                    { PageParameterKey.GroupMemberId, "((Key))" }
                } ),
                [NavigationUrlKey.MainPage] = this.GetLinkedPageUrl( AttributeKey.MainPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, GroupId.ToString() }
                } )
            };
        }

        /// <summary>
        /// Gets the fundraising opportunity group, loading it (and its attributes) once from
        /// the page parameter. The parameter may be supplied as an Id, IdKey, or Guid.
        /// </summary>
        /// <returns>The group, or <c>null</c> when it could not be found.</returns>
        private Rock.Model.Group GetGroup()
        {
            if ( !_isGroupLoaded )
            {
                _isGroupLoaded = true;

                _group = new GroupService( RockContext )
                    .Get( PageParameter( PageParameterKey.GroupId ), !PageCache.Layout.Site.DisablePredictableIds );
                _group?.LoadAttributes( RockContext );
            }

            return _group;
        }

        /// <summary>
        /// Determines whether the current person is a leader of the opportunity group, which
        /// is required to view the toolbox.
        /// </summary>
        /// <returns><c>true</c> when the current person is a leader of the group.</returns>
        private bool IsCurrentPersonLeader()
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;
            if ( !currentPersonId.HasValue )
            {
                return false;
            }

            return new GroupMemberService( RockContext )
                .Queryable()
                .Any( m => m.GroupId == GroupId
                    && m.PersonId == currentPersonId.Value
                    && m.GroupRole.IsLeader );
        }

        /// <inheritdoc/>
        protected override IQueryable<ParticipantRow> GetListQueryable( RockContext rockContext )
        {
            var group = GetGroup();

            // Only an existing opportunity viewed by one of its leaders returns rows. This
            // server-side guard is what gates the grid; the client is never trusted.
            if ( group == null || !IsCurrentPersonLeader() )
            {
                return new List<ParticipantRow>().AsQueryable();
            }

            var groupMemberEntityTypeId = EntityTypeCache.GetId<GroupMember>();

            var groupMemberQuery = new GroupMemberService( RockContext )
                .Queryable()
                .Where( m => m.GroupId == group.Id && m.GroupMemberStatus == GroupMemberStatus.Active );

            // grab the total fundraised for each memmber of the group, so we can calculate the remaining amount to fundraise for each member
            var contributionTotalByMemberId = new FinancialTransactionDetailService( RockContext )
                .Queryable()
                .Where( d => d.EntityTypeId == groupMemberEntityTypeId
                    && d.EntityId.HasValue
                    && groupMemberQuery.Any( m => m.Id == d.EntityId.Value ) )
                .GroupBy( d => d.EntityId.Value )
                .Select( g => new
                {
                    GroupMemberId = g.Key,
                    Total = g.Sum( d => ( decimal? ) d.Amount )
                } )
                .ToDictionary( a => a.GroupMemberId, a => a.Total ?? 0.00M );

            var groupMembers = groupMemberQuery
                .Include( m => m.Person )
                .Include( m => m.GroupRole )
                .OrderBy( m => m.Person.LastName )
                .ThenBy( m => m.Person.NickName )
                .AsNoTracking()
                .ToList();

            var rows = new List<ParticipantRow>();

            foreach ( var groupMember in groupMembers )
            {
                contributionTotalByMemberId.TryGetValue( groupMember.Id, out var contributionTotal );

                rows.Add( new ParticipantRow
                {
                    IdKey = groupMember.IdKey,
                    PersonIdKey = groupMember.Person.IdKey,
                    DateTimeAdded = groupMember.DateTimeAdded,
                    Gender = groupMember.Person.Gender,
                    TotalContributed = contributionTotal,
                    RoleName = groupMember.GroupRole?.Name,
                    GroupMember = groupMember
                } );
            }

            return rows.AsQueryable();
        }

        protected override List<ParticipantRow> GetListItems( IQueryable<ParticipantRow> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            if ( items.Count == 0 )
            {
                return items;
            }

            // Bulk load the individual fundraising goal and toggle for public contribution requests
            Helper.LoadFilteredAttributes( typeof( GroupMember ), items.Select( i => i.GroupMember ).Cast<IHasAttributes>().ToList(), RockContext,
                a => a.Key == FundraisingAttributeKey.IndividualFundraisingGoal
                    || a.Key == FundraisingAttributeKey.DisablePublicContributionRequests );

            // A member's goal falls back to the group-level goal when the member has none.
            var groupFundraisingGoal = GetGroup().GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull();

            foreach ( var item in items )
            {
                // if no indiviudal goal, set the amount remaining to the group goal
                var individualFundraisingGoal = item.GroupMember.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                    ?? groupFundraisingGoal;

                var disablePublicContributionRequests = item.GroupMember.GetAttributeValue( FundraisingAttributeKey.DisablePublicContributionRequests ).AsBoolean();

                // Funding remaining is hidden when the member has opted out of public
                // contribution requests, and is never shown as a negative amount.
                decimal? fundingRemaining = individualFundraisingGoal - item.TotalContributed;
                if ( disablePublicContributionRequests )
                {
                    fundingRemaining = null;
                }
                else if ( fundingRemaining < 0 )
                {
                    fundingRemaining = 0.00M;
                }

                item.FundingRemaining = fundingRemaining;
            }

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<ParticipantRow> GetGridBuilder()
        {
            return new GridBuilder<ParticipantRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "personIdKey", a => a.PersonIdKey )
                .AddPersonField( "person", a => a.GroupMember.Person )
                .AddDateTimeField( "dateTimeAdded", a => a.DateTimeAdded )
                .AddTextField( "gender", a => a.Gender.ConvertToString() )
                .AddField( "fundingRemaining", a => a.FundingRemaining )
                .AddTextField( "roleName", a => a.RoleName );
        }

        #endregion Methods

        #region Helper Classes

        /// <summary>
        /// A single fundraising participant row displayed on the grid.
        /// </summary>
        public class ParticipantRow
        {
            /// <summary>
            /// Gets or sets the group member's hashed identifier (the grid key).
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the participant person's hashed identifier (used for communications).
            /// </summary>
            public string PersonIdKey { get; set; }

            /// <summary>
            /// Gets or sets the date and time the participant was added to the opportunity.
            /// </summary>
            public System.DateTime? DateTimeAdded { get; set; }

            /// <summary>
            /// Gets or sets the participant's gender.
            /// </summary>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the amount of funding still needed to reach the participant's goal.
            /// <c>null</c> when public contribution requests are disabled for the participant.
            /// Computed in <see cref="GetListItems"/> from the participant's attributes.
            /// </summary>
            public decimal? FundingRemaining { get; set; }

            /// <summary>
            /// Gets or sets the total amount contributed toward this participant's fundraising.
            /// </summary>
            public decimal TotalContributed { get; set; }

            /// <summary>
            /// Gets or sets the participant's group role name.
            /// </summary>
            public string RoleName { get; set; }

            /// <summary>
            /// Gets or sets the underlying group member. Carried so the attribute-dependent
            /// funding remaining can be computed in <see cref="GetListItems"/>; this is not
            /// emitted as a grid field.
            /// </summary>
            internal GroupMember GroupMember { get; set; }
        }

        #endregion Helper Classes
    }
}
