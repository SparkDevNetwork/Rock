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

namespace RockWeb.Plugins.cc_newspring.Blocks.Fundraising
{
    [DisplayName( "Fundraising Opportunity Participant" )]
    [Category( "NewSpring" )]
    [Description( "Public facing block that shows a fundraising opportunity participant" )]

    [CodeEditorField( "Profile Lava Template", "Lava template for what to display at the top of the main panel. Usually used to display information about the participant such as photo, name, etc.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingParticipantProfile.lava' %}", order: 1 )]

    [CodeEditorField( "Progress Lava Template", "Lava template for how the progress bar should be displayed ", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingParticipantProgress.lava' %}", order: 2)]

    [CodeEditorField( "Updates Lava Template", "Lava template for the Updates (Content Channel Items)", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
        @"{% include '~~/Assets/Lava/FundraisingOpportunityUpdates.lava' %}", order: 3 )]

    [NoteTypeField( "Note Type", "Note Type to use for participant comments", false, "Rock.Model.GroupMember", defaultValue: "FFFC3644-60CD-4D14-A714-E8DCC202A0E1", order: 5 )]
    [LinkedPage( "Donation Page", "The page where a person can donate to the fundraising opportunity", required: false, order: 6 )]
    [LinkedPage( "Main Page", "The main page for the fundraising opportunity", required: false, order: 7 )]
    [BooleanField( "Show Clipboard Icon", "Show a clipboard icon which will copy the page url to the users clipboard", true, order:8)]
    [TextField( "Image CSS Class", "CSS class to apply to the image.", false, "img-thumbnail", key: "ImageCssClass", order: 9 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "PersonAttributes", "The Person Attributes that the participant can edit", false, true, order: 7 )]
    public partial class FundraisingParticipant : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( this.GetAttributeValue( "ShowClipboardIcon" ).AsBoolean() )
            {
                // Setup for being able to copy text to clipboard
                RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
                string script = string.Format( @"
    new ClipboardJS('#{0}');
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
                ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

                Uri uri = new Uri( Request.Url.ToString() );
                btnCopyToClipboard.Attributes["data-clipboard-text"] = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + CurrentPageReference.BuildUrl();
                btnCopyToClipboard.Visible = true;
            }
            else
            {
                btnCopyToClipboard.Visible = false;
            }

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
                int? groupMemberId = this.PageParameter( "GroupMemberId" ).AsIntegerOrNull();

                if ( groupId.HasValue && groupMemberId.HasValue )
                {
                    ShowView( groupId.Value, groupMemberId.Value );
                }
                else
                {
                    pnlView.Visible = false;
                }

                imgOpportunityPhoto.CssClass = GetAttributeValue( "ImageCssClass" );
            }
            else
            {
                var groupMember = new GroupMemberService( new RockContext() ).Get( hfGroupMemberId.Value.AsInteger() );
                if ( groupMember != null )
                {
                    CreateDynamicControls( groupMember );
                }
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
            ShowView( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsInteger() );
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
        /// Handles the Click event of the btnUpdatesTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdatesTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Updates" );
        }

        /// <summary>
        /// Handles the Click event of the btnContributionsTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnContributionsTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Contributions" );
        }

        #endregion

        #region Edit Preferences

        /// <summary>
        /// Handles the Click event of the btnEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEditProfile_Click( object sender, EventArgs e )
        {
            pnlMain.Visible = false;
            pnlEditPreferences.Visible = true;

            var rockContext = new RockContext();

            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember != null )
            {
                var person = groupMember.Person;
                imgProfilePhoto.BinaryFileId = person.PhotoId;
                imgProfilePhoto.NoPictureUrl = Person.GetPersonNoPictureUrl( person, 200, 200 );

                groupMember.LoadAttributes( rockContext );
                groupMember.Group.LoadAttributes( rockContext );
                var opportunityType = DefinedValueCache.Get( groupMember.Group.GetAttributeValue( "OpportunityType" ).AsGuid() );

                lProfileTitle.Text = string.Format(
                    "{0} Profile for {1}",
                    RockFilters.Possessive( groupMember.Person.FullName ),
                    groupMember.Group.GetAttributeValue( "OpportunityTitle" ) );

                var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( groupMember.Group.GetAttributeValue( "OpportunityDateRange" ) );

                lDateRange.Text = dateRange.ToString( "MMMM dd, yyyy" );
                CreateDynamicControls( groupMember );
            }
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        private void CreateDynamicControls( GroupMember groupMember )
        {
            groupMember.LoadAttributes();
            groupMember.Group.LoadAttributes();
            // GroupMember Attributes (all of them)
            phGroupMemberAttributes.Controls.Clear();

            List<string> excludes = new List<string>();
            if ( !groupMember.Group.GetAttributeValue( "AllowIndividualDisablingofContributionRequests" ).AsBoolean() )
            {
                excludes.Add( "DisablePublicContributionRequests" );
            }

            if ( !groupMember.Group.GetAttributeValue( "AllowIndividualEditingofFundraisingGoal" ).AsBoolean() )
            {
                excludes.Add( "IndividualFundraisingGoal" );
            }

            Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, "vgProfileEdit", excludes, true );

            // Person Attributes (the ones they picked in the Block Settings)
            phPersonAttributes.Controls.Clear();

            var personAttributes = this.GetAttributeValue( "PersonAttributes" ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Get( a ) );
            if ( personAttributes.Any() )
            {
                var person = groupMember.Person;
                person.LoadAttributes();
                foreach ( var personAttribute in personAttributes.OrderBy( a => a.Order ) )
                {
                    personAttribute.AddControl( phPersonAttributes.Controls, person.GetAttributeValue( personAttribute.Key ), "vgProfileEdit", true, true );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveEditPreferences_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            var personService = new PersonService( rockContext );
            var person = personService.Get( groupMember.PersonId );

            int? orphanedPhotoId = null;
            if ( person.PhotoId != imgProfilePhoto.BinaryFileId )
            {
                orphanedPhotoId = person.PhotoId;
                person.PhotoId = imgProfilePhoto.BinaryFileId;

                // add or update the Photo Verify group to have this person as Pending since the photo was changed or deleted
                using ( var photoRequestRockContext = new RockContext() )
                {
                    GroupMemberService groupMemberService = new GroupMemberService( photoRequestRockContext );
                    Group photoRequestGroup = new GroupService( photoRequestRockContext ).Get( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );

                    var photoRequestGroupMember = groupMemberService.Queryable().Where( a => a.GroupId == photoRequestGroup.Id && a.PersonId == person.Id ).FirstOrDefault();
                    if ( photoRequestGroupMember == null )
                    {
                        photoRequestGroupMember = new GroupMember();
                        photoRequestGroupMember.GroupId = photoRequestGroup.Id;
                        photoRequestGroupMember.PersonId = person.Id;
                        photoRequestGroupMember.GroupRoleId = photoRequestGroup.GroupType.DefaultGroupRoleId ?? -1;
                        groupMemberService.Add( photoRequestGroupMember );
                    }

                    photoRequestGroupMember.GroupMemberStatus = GroupMemberStatus.Pending;

                    photoRequestRockContext.SaveChanges();
                }
            }

            // Save the GroupMember Attributes
            groupMember.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phGroupMemberAttributes, groupMember );

            // Save selected Person Attributes (The ones picked in Block Settings)
            var personAttributes = this.GetAttributeValue( "PersonAttributes" ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Get( a ) );
            if ( personAttributes.Any() )
            {
                person.LoadAttributes( rockContext );
                foreach ( var personAttribute in personAttributes )
                {
                    Control attributeControl = phPersonAttributes.FindControl( string.Format( "attribute_field_{0}", personAttribute.Id ) );
                    if ( attributeControl != null )
                    {
                        // Save Attribute value to the database
                        string newValue = personAttribute.FieldType.Field.GetEditValue( attributeControl, personAttribute.QualifierValues );
                        Rock.Attribute.Helper.SaveAttributeValue( person, personAttribute, newValue, rockContext );
                    }
                }
            }

            // Save everything else to the database in a db transaction
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                groupMember.SaveAttributeValues( rockContext );

                if ( orphanedPhotoId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        rockContext.SaveChanges();
                    }
                }

                // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
                if ( imgProfilePhoto.CropBinaryFileId.HasValue )
                {
                    if ( imgProfilePhoto.CropBinaryFileId != person.PhotoId )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( imgProfilePhoto.CropBinaryFileId.Value );
                        if ( binaryFile != null && binaryFile.IsTemporary )
                        {
                            string errorMessage;
                            if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                            {
                                binaryFileService.Delete( binaryFile );
                                rockContext.SaveChanges();
                            }
                        }
                    }
                }
            } );

            ShowView( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelEditPreferences_Click( object sender, EventArgs e )
        {
            ShowView( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberId">The group member identifier.</param>
        protected void ShowView( int groupId, int groupMemberId )
        {
            pnlView.Visible = true;
            pnlMain.Visible = true;
            pnlEditPreferences.Visible = false;
            hfGroupId.Value = groupId.ToString();
            hfGroupMemberId.Value = groupMemberId.ToString();
            var rockContext = new RockContext();

            var group = new GroupService( rockContext ).Get( groupId );
            if ( group == null )
            {
                pnlView.Visible = false;
                return;
            }

            var groupMember = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == groupId && a.Id == groupMemberId ).FirstOrDefault();
            if ( groupMember == null )
            {
                pnlView.Visible = false;
                return;
            }

            group.LoadAttributes( rockContext );

            // set page title to the trip name
            RockPage.Title = group.GetAttributeValue( "OpportunityTitle" );
            RockPage.BrowserTitle = group.GetAttributeValue( "OpportunityTitle" );
            RockPage.Header.Title = group.GetAttributeValue( "OpportunityTitle" );

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Group", group );

            groupMember.LoadAttributes( rockContext );
            mergeFields.Add( "GroupMember", groupMember );

            // Left Top Sidebar
            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" );
            imgOpportunityPhoto.ImageUrl = string.Format( "~/GetImage.ashx?Guid={0}", photoGuid );

            // Top Main
            string profileLavaTemplate = this.GetAttributeValue( "ProfileLavaTemplate" );
            if ( groupMember.PersonId == this.CurrentPersonId )
            {
                // show a warning about missing Photo or Intro if the current person is viewing their own profile
                var warningItems = new List<string>();
                if ( !groupMember.Person.PhotoId.HasValue )
                {
                    warningItems.Add( "photo" );
                }
                if ( groupMember.GetAttributeValue( "PersonalOpportunityIntroduction" ).IsNullOrWhiteSpace())
                {
                    warningItems.Add( "personal opportunity introduction" );
                }

                nbProfileWarning.Text = "<stong>Tip!</strong> Edit your profile to add a " + warningItems.AsDelimited( ", ", " and " ) + ".";
                nbProfileWarning.Visible = warningItems.Any();
            }
            else
            {
                nbProfileWarning.Visible = false;
            }

            btnEditProfile.Visible = groupMember.PersonId == this.CurrentPersonId;

            lMainTopContentHtml.Text = profileLavaTemplate.ResolveMergeFields( mergeFields );

            bool disablePublicContributionRequests = groupMember.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean();

            // only show Contribution stuff if the current person is the participant and contribution requests haven't been disabled
            bool showContributions = !disablePublicContributionRequests && ( groupMember.PersonId == this.CurrentPersonId );
            btnContributionsTab.Visible = showContributions;

            // Progress
            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

            var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                        .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                && d.EntityId == groupMemberId )
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
            mergeFields.Add( "MakeDonationUrl", LinkedPageUrl( "DonationPage", queryParams ));

            var opportunityType = DefinedValueCache.Get( group.GetAttributeValue( "OpportunityType" ).AsGuid() );

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

            var progressLavaTemplate = this.GetAttributeValue( "ProgressLavaTemplate" );
            lProgressHtml.Text = progressLavaTemplate.ResolveMergeFields( mergeFields );

            // set text on the return button
            btnMainPage.Text = opportunityType.Value + " Page";

            // Tab:Updates
            btnUpdatesTab.Visible = false;
            bool showContentChannelUpdates = false;
            var updatesContentChannelGuid = group.GetAttributeValue( "UpdateContentChannel" ).AsGuidOrNull();
            if ( updatesContentChannelGuid.HasValue )
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( updatesContentChannelGuid.Value );
                if ( contentChannel != null )
                {
                    showContentChannelUpdates = true;

                    // only show the UpdatesTab if there is another Tab option
                    btnUpdatesTab.Visible = btnContributionsTab.Visible;

                    string updatesLavaTemplate = this.GetAttributeValue( "UpdatesLavaTemplate" );
                    var contentChannelItems = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).AsNoTracking().ToList();

                    mergeFields.Add( "ContentChannelItems", contentChannelItems );
                    lUpdatesContentItemsHtml.Text = updatesLavaTemplate.ResolveMergeFields( mergeFields );
                    btnUpdatesTab.Text = string.Format( "{0} Updates ({1})", opportunityType, contentChannelItems.Count() );
                }
            }

            if ( showContentChannelUpdates )
            {
                SetActiveTab( "Updates" );
            }
            else if (showContributions)
            {
                SetActiveTab( "Contributions" );
            }
            else
            {
                SetActiveTab( "" );
            }

            // Tab: Contributions
            BindContributionsGrid();

            // Tab:Comments
            var noteType = NoteTypeCache.Get( this.GetAttributeValue( "NoteType" ).AsGuid() );
            if ( noteType != null )
            {
                notesCommentsTimeline.NoteOptions.SetNoteTypes( new List<NoteTypeCache> { noteType } );
            }

            notesCommentsTimeline.NoteOptions.EntityId = groupMember.Id;

            // show the Add button on comments for any logged in person
            notesCommentsTimeline.AddAllowed = true;

            var enableCommenting = group.GetAttributeValue( "EnableCommenting" ).AsBoolean();

            if ( CurrentPerson == null )
            {
                notesCommentsTimeline.Visible = enableCommenting && ( notesCommentsTimeline.NoteCount > 0 );
                lNoLoginNoCommentsYet.Visible = notesCommentsTimeline.NoteCount == 0;
                pnlComments.Visible = enableCommenting;
                btnLoginToComment.Visible = enableCommenting;
            }
            else
            {
                lNoLoginNoCommentsYet.Visible = false;
                notesCommentsTimeline.Visible = enableCommenting;
                pnlComments.Visible = enableCommenting;
                btnLoginToComment.Visible = false;
            }

            // if btnContributionsTab is the only visible tab, hide the tab since there is nothing else to tab to
            if ( !btnUpdatesTab.Visible && btnContributionsTab.Visible )
            {
                SetActiveTab( "Contributions" );
                btnContributionsTab.Visible = false;
            }
        }

        /// <summary>
        /// Binds the contributions grid.
        /// </summary>
        protected void BindContributionsGrid()
        {
            var rockContext = new RockContext();
            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();
            int groupMemberId = hfGroupMemberId.Value.AsInteger();

            var financialTransactionQry = new FinancialTransactionService( rockContext ).Queryable();

            financialTransactionQry = financialTransactionQry.Where( a =>
                a.TransactionDetails.Any( d => d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMemberId ) );

            financialTransactionQry = financialTransactionQry.OrderByDescending( a => a.TransactionDateTime );

            gContributions.SetLinqDataSource( financialTransactionQry );
            gContributions.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gContributions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gContributions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
			var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();
            int groupMemberId = hfGroupMemberId.Value.AsInteger();
			
            FinancialTransaction financialTransaction = e.Row.DataItem as FinancialTransaction;
            if ( financialTransaction != null &&
                financialTransaction.AuthorizedPersonAlias != null &&
                financialTransaction.AuthorizedPersonAlias.Person != null )
            {
	            Literal lAddress = e.Row.FindControl( "lAddress" ) as Literal;
	            if ( lAddress != null )
	            {
	                var location = financialTransaction.AuthorizedPersonAlias.Person.GetMailingLocation();
                    string streetAddress = location != null ? location.GetFullStreetAddress() : string.Empty;
                    lAddress.Text = financialTransaction.ShowAsAnonymous ? string.Empty : streetAddress;
                }

	            Literal lPersonName = e.Row.FindControl( "lPersonName" ) as Literal;
	            if ( lPersonName != null )
	            {
	                lPersonName.Text = financialTransaction.ShowAsAnonymous ? "Anonymous" : financialTransaction.AuthorizedPersonAlias.Person.FullName;
	            }
				
				Literal lTransactionDetailAmount = e.Row.FindControl( "lTransactionDetailAmount" ) as Literal;
                if ( lTransactionDetailAmount != null )
                {
					var amount = financialTransaction.TransactionDetails
                        .Where( d => d.EntityTypeId.HasValue && d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMemberId )
                        .Sum( d => ( decimal? ) d.Amount )
                        .ToString();
                    lTransactionDetailAmount.Text = string.Format( "${0}", amount );
                }
	        }
        }

        /// <summary>
        /// Sets the active tab.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        protected void SetActiveTab( string tabName )
        {
            hfActiveTab.Value = tabName;
            pnlUpdatesComments.Visible = tabName == "Updates";
            pnlContributions.Visible = tabName == "Contributions";
            if (tabName == "Updates")
            {
                liUpdatesTab.AddCssClass( "active" );
                liContributionsTab.RemoveCssClass( "active" );
            }
            else if (tabName == "Contributions")
            {
                liUpdatesTab.RemoveCssClass( "active" );
                liContributionsTab.AddCssClass( "active" );
            }
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