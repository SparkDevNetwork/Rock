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
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Opportunity Participant" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity participant" )]

    [CodeEditorField(
        "Profile Lava Template",
        Key = AttributeKey.ProfileLavaTemplate,
        Description = "Lava template for what to display at the top of the main panel. Usually used to display information about the participant such as photo, name, etc.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingParticipantProfile.lava' %}",
        Order = 1 )]

    [CodeEditorField(
        "Progress Lava Template",
        Key = AttributeKey.ProgressLavaTemplate,
        Description = "Lava template for how the progress bar should be displayed ",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingParticipantProgress.lava' %}",
        Order = 2 )]

    [CodeEditorField(
        "Updates Lava Template",
        Key = AttributeKey.UpdatesLavaTemplate,
        Description = "Lava template for the Updates (Content Channel Items)",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingOpportunityUpdates.lava' %}",
        Order = 3 )]

    [CodeEditorField(
        "Requirements Header Lava Template",
        Key = AttributeKey.RequirementsHeaderLavaTemplate,
        Description = "Lava template for requirements header.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingParticipantRequirementsHeader.lava' %}",
        Order = 4 )]

    [NoteTypeField(
        "Note Type",
        Key = AttributeKey.NoteType,
        Description = "Note Type to use for participant comments",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.GroupMember",
        DefaultValue = Rock.SystemGuid.NoteType.GROUPMEMBER_NOTE,
        Order = 5 )]

    [LinkedPage(
        "Donation Page",
        Key = AttributeKey.DonationPage,
        Description = "The page where a person can donate to the fundraising opportunity",
        IsRequired = false,
        Order = 6 )]

    [LinkedPage(
        "Main Page",
        Key = AttributeKey.MainPage,
        Description = "The main page for the fundraising opportunity",
        IsRequired = false,
        Order = 7 )]

    [BooleanField(
        "Show Clipboard Icon",
        Key = AttributeKey.ShowClipboardIcon,
        Description = "Show a clipboard icon which will copy the page url to the users clipboard",
        IsRequired = true,
        Order = 8 )]

    [TextField(
        "Image CSS Class",
        Description = "CSS class to apply to the image.",
        IsRequired = false,
        DefaultValue = "img-thumbnail",
        Key = AttributeKey.ImageCssClass,
        Order = 9 )]

    [TextField(
        "Contributions Header",
        Description = "The title for the Contributions header.",
        IsRequired = false,
        DefaultValue = "Contributions",
        Key = AttributeKey.ContributionsHeader,
        Order = 10 )]

    [AttributeField(
        "PersonAttributes",
        Key = AttributeKey.PersonAttributes,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Description = "The Person Attributes that the participant can edit",
        IsRequired = false,
        AllowMultiple = true,
        Order = 11 )]

    [BooleanField(
        "Show Amount",
        Key = AttributeKey.ShowAmount,
        Description = "Determines if the Amount column should be displayed in the Contributions List.",
        DefaultBooleanValue = false,
        Order = 12 )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Key = AttributeKey.WorkflowEntryPage,
        DefaultValue = Rock.SystemGuid.Page.EXTERNAL_WORKFLOW_ENTRY,
        Order = 13 )]

    [Rock.SystemGuid.BlockTypeGuid( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1" )]
    public partial class FundraisingParticipant : RockBlock
    {
        private static class AttributeKey
        {
            public const string ProfileLavaTemplate = "ProfileLavaTemplate";
            public const string ProgressLavaTemplate = "ProgressLavaTemplate";
            public const string UpdatesLavaTemplate = "UpdatesLavaTemplate";
            public const string RequirementsHeaderLavaTemplate = "RequirementsHeaderLavaTemplate";
            public const string NoteType = "NoteType";
            public const string DonationPage = "DonationPage";
            public const string MainPage = "MainPage";
            public const string ShowClipboardIcon = "ShowClipboardIcon";
            public const string ImageCssClass = "ImageCssClass";
            public const string ContributionsHeader = "ContributionsHeader";
            public const string PersonAttributes = "PersonAttributes";
            public const string ShowAmount = "ShowAmount";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
        }
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( this.GetAttributeValue( AttributeKey.ShowClipboardIcon ).AsBoolean() )
            {
                // Setup for being able to copy text to clipboard
                RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
                string script = string.Format( @"
    new ClipboardJS('#{0}');
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
                ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

                Uri uri = new Uri( Request.UrlProxySafe().ToString() );
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

                imgOpportunityPhoto.CssClass = GetAttributeValue( AttributeKey.ImageCssClass );
            }
            else
            {
                var rockContext = new RockContext();
                var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );

                if ( groupMember != null )
                {
                    // Set the requirements values only if there are requirements for this group / group type.
                    if ( groupMember.Group.GroupRequirements.Any() || groupMember.Group.GroupType.GroupRequirements.Any() )
                    {
                        gmrcRequirements.WorkflowEntryLinkedPageValue = this.GetAttributeValue( AttributeKey.WorkflowEntryPage );
                        gmrcRequirements.Visible = true;
                        SetRequirementStatuses( rockContext );
                    }

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
            NavigateToLinkedPage( AttributeKey.MainPage, queryParams );
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

                lDateRange.Text = dateRange.ToString( "MMMM d, yyyy" );
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

            // GroupMember Attributes (all of them).
            phGroupMemberAttributes.Controls.Clear();

            // Exclude any attributes for which the current person has NO EDIT access.
            // But skip these three special member attributes since they are handled in a special way.
            List<string> excludes = groupMember.Attributes.Where(
                a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) &&
                a.Key != "IndividualFundraisingGoal" &&
                a.Key != "DisablePublicContributionRequests" &&
                a.Key != "PersonalOpportunityIntroduction" )
                .Select( a => a.Key ).ToList();

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

            var personAttributes = this.GetAttributeValue( AttributeKey.PersonAttributes ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Get( a ) );
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
            var personAttributes = this.GetAttributeValue( AttributeKey.PersonAttributes ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Get( a ) );
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
            var participationMode = group.GetAttributeValue( "ParticipationType" ).ConvertToEnumOrNull<ParticipationType>() ?? ParticipationType.Individual;

            // set page title to the trip name
            RockPage.Title = group.GetAttributeValue( "OpportunityTitle" );
            RockPage.BrowserTitle = group.GetAttributeValue( "OpportunityTitle" );
            RockPage.Header.Title = group.GetAttributeValue( "OpportunityTitle" );

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions() );
            mergeFields.Add( "Group", group );

            groupMember.LoadAttributes( rockContext );
            mergeFields.Add( "GroupMember", groupMember );

            // Left Top Sidebar
            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" );
            if ( !string.IsNullOrWhiteSpace( photoGuid ) )
            {
                imgOpportunityPhoto.ImageUrl = FileUrlHelper.GetImageUrl( photoGuid.AsGuid() );
            }
            else
            {
                imgOpportunityPhoto.Visible = false;
            }

            // Top Main
            string profileLavaTemplate = this.GetAttributeValue( AttributeKey.ProfileLavaTemplate );

            // Create a list of group member Ids that are all the family members in the current group.
            var familyMembers = groupMember.Person.GetFamilyMembers( true ).Select( m => m.PersonId ).ToList();

            // This variable sets up whether the current person is logged in, is in the same family as the block's group member, and whether this group's participation type is "Family".
            bool isCurrentPersonAFamilyMemberOfGroupMemberAndGroupParticipationTypeIsFamily =
                participationMode == ParticipationType.Family && this.CurrentPersonId.HasValue && familyMembers.Contains( this.CurrentPersonId.Value );

            if ( groupMember.PersonId == this.CurrentPersonId || isCurrentPersonAFamilyMemberOfGroupMemberAndGroupParticipationTypeIsFamily )
            {
                // show a warning about missing Photo or Intro if the current person is viewing their own profile
                string progressTitle = participationMode == ParticipationType.Individual ? groupMember.Person.FullName : groupMember.Person.PrimaryFamily.Name;
                mergeFields.Add( "ProgressTitle", progressTitle );
                var warningItems = new List<string>();
                if ( !groupMember.Person.PhotoId.HasValue )
                {
                    warningItems.Add( "photo" );
                }

                if ( groupMember.GetAttributeValue( "PersonalOpportunityIntroduction" ).IsNullOrWhiteSpace() )
                {
                    warningItems.Add( "personal opportunity introduction" );
                }

                nbProfileWarning.Text = "<strong>Tip!</strong> Edit your profile to add a " + warningItems.AsDelimited( ", ", " and " ) + ".";
                nbProfileWarning.Visible = warningItems.Any();

                // Set the requirements values only if there are requirements for this group / group type.
                if ( group.GroupRequirements.Any() || group.GroupType.GroupRequirements.Any() )
                {
                    gmrcRequirements.WorkflowEntryLinkedPageValue = this.GetAttributeValue( AttributeKey.WorkflowEntryPage );
                    gmrcRequirements.Visible = true;
                    SetRequirementStatuses( rockContext );

                    var participantLavaTemplate = this.GetAttributeValue( AttributeKey.RequirementsHeaderLavaTemplate );
                    lParticipantHtml.Text = participantLavaTemplate.ResolveMergeFields( mergeFields );
                }
            }
            else
            {
                nbProfileWarning.Visible = false;
                gmrcRequirements.Visible = false;
            }

            btnEditProfile.Visible = groupMember.PersonId == this.CurrentPersonId;

            lMainTopContentHtml.Text = profileLavaTemplate.ResolveMergeFields( mergeFields );

            bool disablePublicContributionRequests = groupMember.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean();

            // only show Contribution stuff if the participant is the current person or an allowed family member, and contribution requests haven't been disabled
            bool showContributions = !disablePublicContributionRequests && ( groupMember.PersonId == this.CurrentPersonId || isCurrentPersonAFamilyMemberOfGroupMemberAndGroupParticipationTypeIsFamily );
            btnContributionsTab.Visible = showContributions;

            // Progress
            // Create the total and the goal variables before setting them.
            decimal contributionTotal;
            decimal? fundraisingGoal;
            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

            // If this is a Family participation type, collect the number of family members that are on the team.
            if ( participationMode == ParticipationType.Family )
            {
                var groupMembers = group.Members.ToList();

                // Create a list of group member Ids that are all the family members in the current group.
                var familyMemberGroupMembersInCurrentGroup = groupMembers.Where( m => familyMembers.Contains( m.PersonId ) );

                contributionTotal = new FinancialTransactionDetailService( rockContext )
                       .GetContributionsForGroupMemberList( entityTypeIdGroupMember, familyMemberGroupMembersInCurrentGroup.Select( m => m.Id ).ToList() );

                // Sum the family members' individual fundraising goals or the goals from the group.
                fundraisingGoal = 0;
                foreach ( var member in familyMemberGroupMembersInCurrentGroup )
                {
                    member.LoadAttributes( rockContext );
                    fundraisingGoal += member.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? 0;
                }
            }
            else
            {
                contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                            .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                    && d.EntityId == groupMemberId )
                            .Sum( a => ( decimal? ) a.Amount ) ?? 0.00M;

                fundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                if ( !fundraisingGoal.HasValue )
                {
                    fundraisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                }
            }

            var amountLeft = fundraisingGoal - contributionTotal;
            var percentMet = fundraisingGoal > 0 ? contributionTotal * 100 / fundraisingGoal : 100;

            mergeFields.Add( "AmountLeft", amountLeft );
            mergeFields.Add( "PercentMet", percentMet );

            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            queryParams.Add( "GroupMemberId", hfGroupMemberId.Value );
            queryParams.Add( "ParticipationMode", participationMode.ToString( "D" ) );
            mergeFields.Add( "MakeDonationUrl", LinkedPageUrl( AttributeKey.DonationPage, queryParams ) );

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

            var progressLavaTemplate = this.GetAttributeValue( AttributeKey.ProgressLavaTemplate );
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

                    string updatesLavaTemplate = this.GetAttributeValue( AttributeKey.UpdatesLavaTemplate );
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
            else if ( showContributions )
            {
                SetActiveTab( "Contributions" );
            }
            else
            {
                SetActiveTab( string.Empty );
            }

            // Tab: Contributions
            BindContributionsGrid();
            lContributionsHeader.Text = this.GetAttributeValue( AttributeKey.ContributionsHeader );

            // Tab:Comments
            var noteType = NoteTypeCache.Get( this.GetAttributeValue( AttributeKey.NoteType ).AsGuid() );
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
        /// Sets the Requirement Statuses.
        /// </summary>
        /// <param name="rockContext"></param>
        private void SetRequirementStatuses( RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMember = groupMemberService.Get( hfGroupMemberId.ValueAsInt() );

            gmrcRequirements.RequirementStatuses = groupMember.Group.PersonMeetsGroupRequirements( rockContext, groupMember.PersonId, groupMember.GroupRoleId );
            gmrcRequirements.SelectedGroupRoleId = groupMember.GroupRoleId;
            var currentPersonIsLeaderOfCurrentGroup = this.CurrentPerson != null ?
                groupMember.Group.Members.Where( m => m.GroupRole.IsLeader ).Select( m => m.PersonId ).Contains( this.CurrentPerson.Id ) : false;
            gmrcRequirements.CreateRequirementStatusControls( groupMember.Id, currentPersonIsLeaderOfCurrentGroup, false );
        }

        /// <summary>
        /// Binds the contributions grid.
        /// </summary>
        protected void BindContributionsGrid()
        {
            // Hide the whole Amount column if the block setting is set to hide
            var showAmount = GetAttributeValue( AttributeKey.ShowAmount ).AsBoolean();
            var amountCol = gContributions.ColumnsOfType<RockLiteralField>()
                .FirstOrDefault( c => c.ID == "lTransactionDetailAmount" );
            if ( amountCol != null )
            {
                amountCol.Visible = showAmount;
            }

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

                // The transaction may have been split with details for one contribution going to the person
                // and the other details going elsewhere.  We only want to show details that match this group member.
                Literal lTransactionDetailAmount = e.Row.FindControl( "lTransactionDetailAmount" ) as Literal;
                var showAmount = GetAttributeValue( AttributeKey.ShowAmount ).AsBoolean();

                if ( lTransactionDetailAmount != null && showAmount )
                {
                    var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();
                    int groupMemberId = hfGroupMemberId.Value.AsInteger();
                    var amount = financialTransaction.TransactionDetails
                        .Where( d => d.EntityTypeId.HasValue && d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMemberId )
                        .Sum( d => ( decimal? ) d.Amount );
                    lTransactionDetailAmount.Text = amount.FormatAsCurrency();
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
            if ( tabName == "Updates" )
            {
                liUpdatesTab.AddCssClass( "active" );
                liContributionsTab.RemoveCssClass( "active" );
            }
            else if ( tabName == "Contributions" )
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