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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Opportunity View" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity" )]

    [CodeEditorField( "Summary Lava Template", "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingOpportunitySummary.lava' %}", order: 1 )]

    [CodeEditorField( "Sidebar Lava Template", "Lava template for what to display on the left side bar. Usually used to show event registration or other info.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingOpportunitySidebar.lava' %}", order: 2 )]

    [CodeEditorField( "Updates Lava Template", "Lava template for the Updates (Content Channel Items)", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingOpportunityUpdates.lava' %}", order: 3 )]


    [CodeEditorField( "Participant Lava Template", "Lava template for how the participant actions and progress bar should be displayed", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingOpportunityParticipant.lava' %}", order: 4 )]

    [NoteTypeField( "Note Type", "Note Type to use for comments", false, "Rock.Model.Group", defaultValue: "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", order: 5 )]
    [LinkedPage( "Donation Page", "The page where a person can donate to the fundraising opportunity", required: false, order: 6 )]
    [LinkedPage( "Leader Toolbox Page", "The toolbox page for a leader of this fundraising opportunity", required: false, order: 7 )]
    [LinkedPage( "Participant Page", "The participant page for a participant of this fundraising opportunity", required: false, order: 8 )]

    [BooleanField( "Set Page Title to Opportunity Title", "", true, order: 9 )]
    [LinkedPage( "Registration Page", "The page to use for registrations.", required: false, order: 10 )]
    [TextField( "Image CSS Class", "CSS class to apply to the image.", false, "img-thumbnail", key: "ImageCssClass", order: 11 )]
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

                imgOpportunityPhoto.CssClass = GetAttributeValue( "ImageCssClass" );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupId = PageParameter( pageReference, "GroupId" ).AsIntegerOrNull();
            if ( groupId != null )
            {
                Group group = new GroupService( new RockContext() ).Get( groupId.Value );
                if ( group != null )
                {
                    group.LoadAttributes();
                    breadCrumbs.Add( new BreadCrumb( group.GetAttributeValue( "OpportunityTitle" ), pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
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
            var opportunityType = DefinedValueCache.Get( group.GetAttributeValue( "OpportunityType" ).AsGuid() );

            if ( this.GetAttributeValue( "SetPageTitletoOpportunityTitle" ).AsBoolean() )
            {
                RockPage.Title = group.GetAttributeValue( "OpportunityTitle" );
                RockPage.BrowserTitle = group.GetAttributeValue( "OpportunityTitle" );
                RockPage.Header.Title = group.GetAttributeValue( "OpportunityTitle" );
            }

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Block", this.BlockCache );
            mergeFields.Add( "Group", group );

            // Left Sidebar
            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" ).AsGuidOrNull();
            imgOpportunityPhoto.Visible = photoGuid.HasValue;
            imgOpportunityPhoto.ImageUrl = string.Format( "~/GetImage.ashx?Guid={0}", photoGuid );

            var groupMembers = group.Members.ToList();
            foreach ( var gm in groupMembers )
            {
                gm.LoadAttributes( rockContext );
            }

            // only show the 'Donate to a Participant' button if there are participants that are taking contribution requests
            btnDonateToParticipant.Visible = groupMembers.Where( a => !a.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean() ).Any();
            if ( !string.IsNullOrWhiteSpace( opportunityType.GetAttributeValue( "core_DonateButtonText" ) ) )
            {
                btnDonateToParticipant.Text = opportunityType.GetAttributeValue( "core_DonateButtonText" );
            }

            RegistrationInstance registrationInstance = null;
            var registrationInstanceId = group.GetAttributeValue( "RegistrationInstance" ).AsIntegerOrNull();
            if ( registrationInstanceId.HasValue )
            {
                registrationInstance = new RegistrationInstanceService( rockContext ).Get( registrationInstanceId.Value );
            }

            mergeFields.Add( "RegistrationPage", LinkedPageRoute( "RegistrationPage" ) );

            if ( registrationInstance != null )
            {
                mergeFields.Add( "RegistrationInstance", registrationInstance );
                mergeFields.Add( "RegistrationInstanceLinkages", registrationInstance.Linkages );

                // populate merge fields for Registration Counts
                var maxRegistrantCount = 0;
                var currentRegistrationCount = 0;

                if ( registrationInstance.MaxAttendees != 0 )
                {
                    maxRegistrantCount = registrationInstance.MaxAttendees;
                }
               
                currentRegistrationCount = new RegistrationRegistrantService( rockContext ).Queryable().AsNoTracking()
                                                .Where( r =>
                                                    r.Registration.RegistrationInstanceId == registrationInstance.Id
                                                    && r.OnWaitList == false )
                                                .Count();

                mergeFields.Add( "CurrentRegistrationCount", currentRegistrationCount );
                if ( maxRegistrantCount != 0 )
                {
                    mergeFields.Add( "MaxRegistrantCount", maxRegistrantCount );
                    mergeFields.Add( "RegistrationSpotsAvailable", maxRegistrantCount - currentRegistrationCount );
                }
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
            }
            else
            {
                hfGroupMemberId.Value = null;
                pnlParticipantActions.Visible = false;
            }

            mergeFields.Add( "GroupMember", groupMember );

            // Progress
            if ( groupMember != null && pnlParticipantActions.Visible )
            {
                var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

                var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                            .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                    && d.EntityId == groupMember.Id )
                            .Sum( a => (decimal?)a.Amount ) ?? 0.00M;

                var individualFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                if ( !individualFundraisingGoal.HasValue )
                {
                    individualFundraisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                }

                var amountLeft = individualFundraisingGoal - contributionTotal;
                var percentMet = individualFundraisingGoal > 0 ? contributionTotal * 100 / individualFundraisingGoal : 100;

                mergeFields.Add( "AmountLeft", amountLeft );
                mergeFields.Add( "PercentMet", percentMet );

                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "GroupId", hfGroupId.Value );
                queryParams.Add( "GroupMemberId", hfGroupMemberId.Value );
                mergeFields.Add( "MakeDonationUrl", LinkedPageUrl( "DonationPage", queryParams ) );
                mergeFields.Add( "ParticipantPageUrl", LinkedPageUrl( "ParticipantPage", queryParams ) );

                string makeDonationButtonText = null;
                if ( groupMember.PersonId == this.CurrentPersonId )
                {
                    makeDonationButtonText = "Make Payment";
                }
                else
                {
                    makeDonationButtonText = string.Format( "Contribute to {0} {1}", RockFilters.Possessive( groupMember.Person.NickName ), opportunityType );
                }

                mergeFields.Add( "MakeDonationButtonText", makeDonationButtonText );

                var participantLavaTemplate = this.GetAttributeValue( "ParticipantLavaTemplate" );
                lParticipantActionsHtml.Text = participantLavaTemplate.ResolveMergeFields( mergeFields );
            }

            // Tab:Details
            lDetailsHtml.Text = group.GetAttributeValue( "OpportunityDetails" );
            btnDetailsTab.Text = string.Format( "{0} Details", opportunityType );

            // Tab:Updates
            liUpdatesTab.Visible = false;
            var updatesContentChannelGuid = group.GetAttributeValue( "UpdateContentChannel" ).AsGuidOrNull();
            if ( updatesContentChannelGuid.HasValue )
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( updatesContentChannelGuid.Value );
                if ( contentChannel != null )
                {
                    liUpdatesTab.Visible = true;
                    string updatesLavaTemplate = this.GetAttributeValue( "UpdatesLavaTemplate" );
                    var contentChannelItems = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).AsNoTracking().ToList();

                    mergeFields.Add( "ContentChannelItems", contentChannelItems );
                    lUpdatesContentItemsHtml.Text = updatesLavaTemplate.ResolveMergeFields( mergeFields );

                    btnUpdatesTab.Text = string.Format( "{0} Updates ({1})", opportunityType, contentChannelItems.Count() );
                }
            }

            // Tab:Comments
            var noteType = NoteTypeCache.Get( this.GetAttributeValue( "NoteType" ).AsGuid() );
            if ( noteType != null )
            {
                notesCommentsTimeline.NoteOptions.SetNoteTypes( new List<NoteTypeCache> { noteType } );
            }

            notesCommentsTimeline.NoteOptions.EntityId = groupId;

            // show the Add button on comments for any logged in person
            notesCommentsTimeline.AddAllowed = true;

            var enableCommenting = group.GetAttributeValue( "EnableCommenting" ).AsBoolean();
            btnCommentsTab.Text = string.Format( "Comments ({0})", notesCommentsTimeline.NoteCount );

            if ( CurrentPerson == null )
            {
                notesCommentsTimeline.Visible = enableCommenting && ( notesCommentsTimeline.NoteCount > 0 );
                lNoLoginNoCommentsYet.Visible = notesCommentsTimeline.NoteCount == 0;
                liCommentsTab.Visible = enableCommenting;
                btnLoginToComment.Visible = enableCommenting;
            }
            else
            {
                lNoLoginNoCommentsYet.Visible = false;
                notesCommentsTimeline.Visible = enableCommenting;
                liCommentsTab.Visible = enableCommenting;
                btnLoginToComment.Visible = false;
            }

            // if btnDetailsTab is the only visible tab, hide the tab since there is nothing else to tab to
            if ( !liCommentsTab.Visible && !liUpdatesTab.Visible )
            {
                tlTabList.Visible = false;
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

            if ( tabName == "Details" )
            {
                liUpdatesTab.RemoveCssClass( "active" );
                liDetailsTab.AddCssClass( "active" );
                liCommentsTab.RemoveCssClass( "active" );
            }
            else if ( tabName == "Updates" )
            {
                liUpdatesTab.AddCssClass( "active" );
                liDetailsTab.RemoveCssClass( "active" );
                liCommentsTab.RemoveCssClass( "active" );
            }
            else if ( tabName == "Comments" )
            {
                liUpdatesTab.RemoveCssClass( "active" );
                liDetailsTab.RemoveCssClass( "active" );
                liCommentsTab.AddCssClass( "active" );
            }
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

        /// <summary>
        /// Handles the Click event of the btnLoginToComment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLoginToComment_Click( object sender, EventArgs e )
        {
            var site = RockPage.Layout.Site;
            if ( site.LoginPageId.HasValue )
            {
                site.RedirectToLoginPage( true );
            }
            else
            {
                System.Web.Security.FormsAuthentication.RedirectToLoginPage();
            }
        }

        #endregion
    }
}