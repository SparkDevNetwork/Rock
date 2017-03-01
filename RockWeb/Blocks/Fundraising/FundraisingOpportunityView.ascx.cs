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

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Opportunity View" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity" )]

    [CodeEditorField( "Summary Lava Template", "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"
<h1>{{ Group | Attribute:'OpportunityTitle' }}</h1>
{{ Group | Attribute:'OpportunitySummary' }}
", order: 1 )]

    [CodeEditorField( "Sidebar Lava Template", "Lava template for what to display on the left side bar. Usually used to show event registration or other info.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"
{% if (RegistrationInstance.ContactPersonAlias.Person.FullName | Trim != '') or RegistrationInstance.ContactEmail != '' or RegistrationInstance.ContactPhone != '' %}
  <p>
	<strong>Contact</strong><br />
	{% if RegistrationInstance.ContactPersonAlias.Person.FullName | Trim != '' %}
	{{ RegistrationInstance.ContactPersonAlias.Person.FullName }} <br />
	{% endif %}

	{% if RegistrationInstance.ContactEmail != '' %}
	{{ RegistrationInstance.ContactEmail }} <br />
	{% endif %}

	{{ RegistrationInstance.ContactPhone }}
  </p>
{% endif %}
", order: 2 )]

    [CodeEditorField( "Updates Lava Template", "Lava template for the Updates (Content Channel Items)", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"
{% for item in ContentChannelItems %}
<article class='margin-b-lg'>
  <h3>{{ item.Title }}</h3>
  {{ item | Attribute:'Image' }}
  <div>
    {{ item.Content }}
  </div>

</article>
{% endfor %}", order: 3 )]
    [NoteTypeField( "Note Type", "Note Type to use for comments", false, "Rock.Model.Group", defaultValue: "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", order: 4 )]
    [LinkedPage( "Donation Page", "The page where a person can donate to the fundraising opportunity", required: false, order: 5 )]
    [LinkedPage( "Leader Toolbox Page", "The toolbox page for a leader of this fundraising opportunity", required: false, order: 6 )]
    [LinkedPage( "Participant Page", "The partipant page for a participant of this fundraising opportunity", required: false, order: 7 )]
    [BooleanField( "Enable Debug", "Show Lava Debug Help", false, order: 8 )]
    public partial class FundraisingOpportunityView : RockBlock
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

            group.LoadAttributes( rockContext );
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Group", group );

            // Left Sidebar
            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" );
            imgOpportunityPhoto.ImageUrl = string.Format( "~/GetImage.ashx?Guid={0}", photoGuid );

            var registrationInstanceId = group.GetAttributeValue( "RegistrationInstance" ).AsIntegerOrNull();
            if ( registrationInstanceId.HasValue )
            {
                RegistrationInstance registrationInstance = new RegistrationInstanceService( rockContext ).Get( registrationInstanceId.Value );
                mergeFields.Add( "RegistrationInstance", registrationInstance );
            }

            string sidebarLavaTemplate = this.GetAttributeValue( "SidebarLavaTemplate" );
            lSidebarHtml.Text = sidebarLavaTemplate.ResolveMergeFields( mergeFields );

            SetActiveTab( "Details" );

            // Top Main
            string summaryLavaTemplate = this.GetAttributeValue( "SummaryLavaTemplate" );
            lMainTopContentHtml.Text = summaryLavaTemplate.ResolveMergeFields( mergeFields );

            // only show the leader toolbox link of the currentperson has a leader role in the group
            btnLeaderToolbox.Visible = group.Members.Any( a => a.PersonId == this.CurrentPersonId && a.GroupRole.IsLeader );

            //// Participant Actions 
            // only show if the current person is a group member
            var groupMember = group.Members.FirstOrDefault( a => a.PersonId == this.CurrentPersonId );
            if ( groupMember != null )
            {
                hfGroupMemberId.Value = groupMember.Id.ToString();
                pnlParticipantActions.Visible = true;
                imgParticipant.ImageUrl = Person.GetPersonPhotoUrl( groupMember.Person, 100, 100 );
            }
            else
            {
                hfGroupMemberId.Value = null;
                pnlParticipantActions.Visible = false;
                imgParticipant.ImageUrl = null;
            }

            // Progress
            if ( groupMember != null && pnlParticipantActions.Visible )
            {
                var transactionTypeFundraisingValueId = DefinedValueCache.Read( "142EA7C8-04E5-4708-9E29-9C89127061C7" ).Id;
                var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

                var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                            .Where( d => d.Transaction.TransactionTypeValueId == transactionTypeFundraisingValueId
                                    && d.EntityTypeId == entityTypeIdGroupMember
                                    && d.EntityId == groupMember.Id )
                            .Sum( a => (decimal?)a.Amount ) ?? 0.00M;

                var individualFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                if ( !individualFundraisingGoal.HasValue )
                {
                    individualFundraisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                }

                var amountLeft = individualFundraisingGoal - contributionTotal;
                var percentMet = individualFundraisingGoal > 0 ? contributionTotal * 100 / individualFundraisingGoal : 100;
                if ( amountLeft >= 0 )
                {
                    lFundraisingAmountLeftText.Text = string.Format( "{0} left", amountLeft.FormatAsCurrency() );
                }
                else
                {
                    // over 100% of the goal, so display percent
                    lFundraisingAmountLeftText.Text = string.Format( "{0}%", Math.Round( percentMet ?? 0 ) );
                }

                lFundraisingProgressTitle.Text = "Fundraising Progress";
                lFundraisingProgressBar.Text = string.Format(
                    @"<div class='progress'>
                    <div class='progress-bar' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {1}%;'>
                    <span class='sr-only'>{0}% Complete</span>
                    </div>
                 </div>",
                    Math.Round( percentMet ?? 0, 2 ), percentMet > 100 ? 100 : percentMet );
            }

            // Tab:Details
            lDetailsHtml.Text = group.GetAttributeValue( "OpportunityDetails" );

            // Tab:Updates
            btnUpdatesTab.Visible = false;
            var updatesContentChannelGuid = group.GetAttributeValue( "UpdateContentChannel" ).AsGuidOrNull();
            if ( updatesContentChannelGuid.HasValue )
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( updatesContentChannelGuid.Value );
                if ( contentChannel != null )
                {
                    btnUpdatesTab.Visible = true;
                    string updatesLavaTemplate = this.GetAttributeValue( "UpdatesLavaTemplate" );
                    var contentChannelItems = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).AsNoTracking().ToList();

                    mergeFields.Add( "ContentChannelItems", contentChannelItems );
                    lUpdatesContentItemsHtml.Text = updatesLavaTemplate.ResolveMergeFields( mergeFields );
                }
            }

            // Tab:Comments
            var noteType = NoteTypeCache.Read( this.GetAttributeValue( "NoteType" ).AsGuid() );
            if ( noteType != null )
            {
                notesCommentsTimeline.NoteTypes = new List<NoteTypeCache> { noteType };
            }

            notesCommentsTimeline.EntityId = groupId;

            // show the Add button on comments if the current person is a member of the Fundraising Group
            notesCommentsTimeline.AddAllowed = group.Members.Any( a => a.PersonId == this.CurrentPersonId );

            notesCommentsTimeline.RebuildNotes( true );

            if ( this.GetAttributeValue( "EnableDebug" ).AsBoolean() )
            {
                lLavaHelp.Text = mergeFields.lavaDebugInfo( rockContext );
            }
            else
            {
                lLavaHelp.Text = string.Empty;
            }
        }

        /// <summary>
        /// Sets the active tab.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        protected void SetActiveTab( string tabName )
        {
            hfActiveTab.Value = tabName;
            pnlDetails.Visible = tabName == "Details";
            pnlUpdates.Visible = tabName == "Updates";
            pnlComments.Visible = tabName == "Comments";
            btnDetailsTab.CssClass = tabName == "Details" ? "btn btn-primary" : "btn btn-default";
            btnUpdatesTab.CssClass = tabName == "Updates" ? "btn btn-primary" : "btn btn-default";
            btnCommentsTab.CssClass = tabName == "Comments" ? "btn btn-primary" : "btn btn-default";
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView( hfGroupId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnDetailsTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDetailsTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Details" );
        }

        /// <summary>
        /// Handles the Click event of the btnUpdatesTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdatesTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Updates" );
        }

        /// <summary>
        /// Handles the Click event of the btnCommentsTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCommentsTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Comments" );
        }

        /// <summary>
        /// Handles the Click event of the btnParticipantPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnParticipantPage_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            queryParams.Add( "GroupMemberId", hfGroupMemberId.Value );
            NavigateToLinkedPage( "ParticipantPage", queryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnMakePayment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMakePayment_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            queryParams.Add( "GroupMemberId", hfGroupMemberId.Value );
            NavigateToLinkedPage( "DonationPage", queryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnDonateToParticipant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDonateToParticipant_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            NavigateToLinkedPage( "DonationPage", queryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnLeaderToolbox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLeaderToolbox_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            NavigateToLinkedPage( "LeaderToolboxPage", queryParams );
        }

        #endregion
    }
}