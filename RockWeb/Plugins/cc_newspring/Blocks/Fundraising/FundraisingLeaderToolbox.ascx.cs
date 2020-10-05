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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.Fundraising
{
    [DisplayName( "Fundraising Leader Toolbox" )]
    [Category( "NewSpring" )]
    [Description( "The Leader Toolbox for a fundraising opportunity" )]

    [CodeEditorField( "Summary Lava Template", "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
         @"
<h1>{{ Group | Attribute:'OpportunityTitle' }}</h1>
{% assign dateRangeParts = Group | Attribute:'OpportunityDateRange','RawValue' | Split:',' %}
{% assign dateRangePartsSize = dateRangeParts | Size %}
{% if dateRangePartsSize == 2 %}
    {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }} to {{ dateRangeParts[1] | Date:'MMMM dd, yyyy' }}<br/>
{% elsif dateRangePartsSize == 1  %}      
    {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }}
{% endif %}
{{ Group | Attribute:'OpportunityLocation' }}

<br />
<br />
<p>
{{ Group | Attribute:'OpportunitySummary' }}
</p>
", order: 1 )]

    [LinkedPage( "Participant Page", "The participant page for a participant of this fundraising opportunity", required: false, order: 2 )]
    [LinkedPage( "Main Page", "The main page for the fundraising opportunity", required: false, order: 3 )]
    public partial class FundraisingLeaderToolbox : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gGroupMembers.Actions.ShowBulkUpdate = false;
            gGroupMembers.Actions.ShowCommunicate = true;
            gGroupMembers.Actions.ShowMergePerson = false;
            gGroupMembers.Actions.ShowMergeTemplate = false;

            gGroupMembers.GridRebind += gGroupMembers_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();

                if ( groupId.HasValue )
                {
                    ShowView( groupId.Value );
                }
                else
                {
                    pnlView.Visible = false;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        protected void ShowView( int groupId )
        {
            pnlView.Visible = true;
            hfGroupId.Value = groupId.ToString();
            var rockContext = new RockContext();

            var group = new GroupService( rockContext ).Get( groupId );
            if ( group == null )
            {
                pnlView.Visible = false;
                return;
            }

            // only show if the current person is a Leader in the Group
            if ( !group.Members.Any( a => a.PersonId == this.CurrentPersonId && a.GroupRole.IsLeader ) )
            {
                pnlView.Visible = false;
                return;
            }

            group.LoadAttributes( rockContext );
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Group", group );

            // Left Top Sidebar
            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" );
            imgOpportunityPhoto.ImageUrl = string.Format( "~/GetImage.ashx?Guid={0}", photoGuid );

            // Top Main
            string summaryLavaTemplate = this.GetAttributeValue( "SummaryLavaTemplate" );
            lMainTopContentHtml.Text = summaryLavaTemplate.ResolveMergeFields( mergeFields );

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid()
        {
            var rockContext = new RockContext();

            int groupId = hfGroupId.Value.AsInteger();
            var groupMembersQuery = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == groupId );
            var group = new GroupService( rockContext ).Get( groupId );
            group.LoadAttributes( rockContext );
            var defaultIndividualFundRaisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();

            groupMembersQuery = groupMembersQuery.Sort( gGroupMembers.SortProperty ?? new SortProperty { Property = "Person.LastName, Person.NickName" } );

            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

            var groupMemberList = groupMembersQuery.ToList().Select( a =>
            {
                var groupMember = a;
                groupMember.LoadAttributes( rockContext );

                var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                            .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                    && d.EntityId == groupMember.Id )
                            .Sum( d => (decimal?)d.Amount ) ?? 0.00M;

                var individualFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                bool disablePublicContributionRequests = groupMember.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean();
                if ( !individualFundraisingGoal.HasValue )
                {
                    individualFundraisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                }

                var fundingRemaining = individualFundraisingGoal - contributionTotal;
                if ( disablePublicContributionRequests )
                {
                    fundingRemaining = null;
                }
                else if ( fundingRemaining < 0 )
                {
                    fundingRemaining = 0.00M;
                }

                return new
                {
                    groupMember.Id,
                    PersonId = groupMember.PersonId,
                    DateTimeAdded = groupMember.DateTimeAdded,
                    groupMember.Person.FullName,
                    groupMember.Person.Gender,
                    FundingRemaining = fundingRemaining,
                    GroupRoleName = a.GroupRole.Name
                };
            } ).ToList();

            gGroupMembers.DataSource = groupMemberList;
            gGroupMembers.DataBind();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gGroupMembers_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGroupMembersGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView( hfGroupId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnMainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMainPage_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            NavigateToLinkedPage( "MainPage", queryParams );
        }

        /// <summary>
        /// Handles the RowSelected event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupMembers_RowSelected( object sender, RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            queryParams.Add( "GroupMemberId", e.RowKeyId.ToString() );
            NavigateToLinkedPage( "ParticipantPage", queryParams );
        }

        #endregion
    }
}