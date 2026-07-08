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
using Rock.ClientService.Core.Note;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks.Fundraising.FundraisingParticipant;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Fundraising
{
    /// <summary>
    /// Public facing block that shows a fundraising opportunity participant.
    /// </summary>
    [DisplayName( "Fundraising Opportunity Participant" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity participant" )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField(
        "Profile Lava Template",
        Key = AttributeKey.ProfileLavaTemplate,
        Description = "Lava template for what to display at the top of the main panel. Usually used to display information about the participant such as photo, name, etc.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingParticipantProfile.lava' %}",
        Order = 1 )]

    [CodeEditorField(
        "Progress Lava Template",
        Key = AttributeKey.ProgressLavaTemplate,
        Description = "Lava template for how the progress bar should be displayed ",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingParticipantProgress.lava' %}",
        Order = 2 )]

    [CodeEditorField(
        "Updates Lava Template",
        Key = AttributeKey.UpdatesLavaTemplate,
        Description = "Lava template for the Updates (Content Channel Items)",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingOpportunityUpdates.lava' %}",
        Order = 3 )]

    [CodeEditorField(
        "Requirements Header Lava Template",
        Key = AttributeKey.RequirementsHeaderLavaTemplate,
        Description = "Lava template for requirements header.",
        EditorMode = CodeEditorMode.Lava,
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

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "640CF4F0-F192-4F92-9E20-4D4DE9EAFB0F" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "2DE09992-8E0F-4282-85AE-BD6ED546C815" )]
    [Rock.SystemGuid.BlockTypeGuid( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1" )]
    [CustomizedGrid]
    public class FundraisingParticipant : RockBlockType
    {
        #region Keys

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

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
        }

        private static class NavigationUrlKey
        {
            public const string DonationPage = "DonationPage";
            public const string MainPage = "MainPage";
        }

        /// <summary>
        /// Attribute keys for the fundraising opportunity group and its members.
        /// </summary>
        private static class FundraisingAttributeKey
        {
            public const string OpportunityTitle = "OpportunityTitle";
            public const string OpportunityType = "OpportunityType";
            public const string OpportunityPhoto = "OpportunityPhoto";
            public const string OpportunityDateRange = "OpportunityDateRange";
            public const string ParticipationType = "ParticipationType";
            public const string IndividualFundraisingGoal = "IndividualFundraisingGoal";
            public const string UpdateContentChannel = "UpdateContentChannel";
            public const string EnableCommenting = "EnableCommenting";
            public const string DisablePublicContributionRequests = "DisablePublicContributionRequests";
            public const string PersonalOpportunityIntroduction = "PersonalOpportunityIntroduction";
            public const string AllowIndividualDisablingofContributionRequests = "AllowIndividualDisablingofContributionRequests";
            public const string AllowIndividualEditingofFundraisingGoal = "AllowIndividualEditingofFundraisingGoal";
        }

        #endregion Keys

        #region Fields

        private Rock.Model.Group _group;

        private GroupMember _groupMember;

        private List<int> _participantFamilyMemberPersonIds;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <summary>
        /// Builds the initialization box for the block (the view payload, grid definition, and
        /// navigation URLs). Used for the initial render and to return the refreshed view after
        /// a save.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private FundraisingParticipantInitializationBox GetInitializationBox()
        {
            var box = new FundraisingParticipantInitializationBox
            {
                NavigationUrls = GetBoxNavigationUrls(),
                GridDefinition = GetGridBuilder().BuildDefinition()
            };

            GetFundraisingParticipantBox( box );

            return box;
        }

        /// <summary>
        /// Populates the view payload (resolved Lava, photo, button text, and visibility
        /// flags) on the initialization box for the current participant.
        /// </summary>
        /// <param name="box">The initialization box to populate.</param>
        private void GetFundraisingParticipantBox( FundraisingParticipantInitializationBox box )
        {
            var group = GetGroup();
            var groupMember = GetGroupMember();

            if ( group == null || groupMember == null )
            {
                box.ErrorMessage = "The fundraising participant could not be found.";
                return;
            }

            var currentPerson = RequestContext.CurrentPerson;

            // Set the page and browser title to the opportunity title.
            var opportunityTitle = group.GetAttributeValue( FundraisingAttributeKey.OpportunityTitle );
            RequestContext.Response.SetPageTitle( opportunityTitle );
            RequestContext.Response.SetBrowserTitle( opportunityTitle );

            var participationMode = group.GetAttributeValue( FundraisingAttributeKey.ParticipationType ).ConvertToEnum<ParticipationType>( ParticipationType.Individual );

            var isViewerTheParticipant = currentPerson?.Id == groupMember.PersonId;
            var isViewerAuthorized = IsViewerAuthorized();

            if ( isViewerAuthorized )
            {
                // The Edit Profile button is only available to the participant themselves,
                // not to other authorized family members.
                box.IsEditProfileVisible = isViewerTheParticipant;

                // Contributions are shown to authorized viewers unless the participant has
                // opted out of public contribution requests.
                var disablePublicContributionRequests = groupMember.GetAttributeValue( FundraisingAttributeKey.DisablePublicContributionRequests ).AsBoolean();
                box.IsContributionsTabVisible = !disablePublicContributionRequests;

                // Tip shown to the participant when their profile is missing a photo or a
                // personal introduction.
                box.ProfileWarningText = GetProfileWarningText( groupMember );

                // The requirements container is only rendered when the group (or its group
                // type) actually has requirements.
                var hasRequirements = new GroupRequirementService( RockContext ).Queryable()
                    .Any( r => r.GroupId == group.Id || r.GroupTypeId == group.GroupTypeId );
                if ( hasRequirements )
                {
                    var mergeFields = RequestContext.GetCommonMergeFields( currentPerson );
                    mergeFields.Add( "Group", group );
                    mergeFields.Add( "GroupMember", groupMember );

                    box.RequirementsHeaderHtml = GetAttributeValue( AttributeKey.RequirementsHeaderLavaTemplate ).ResolveMergeFields( mergeFields );
                    box.Requirements = GetRequirementsBag( group, groupMember );
                }
            }

            // Left-sidebar opportunity photo
            var photoGuid = group.GetAttributeValue( FundraisingAttributeKey.OpportunityPhoto ).AsGuidOrNull();
            if ( photoGuid.HasValue )
            {
                box.PhotoUrl = FileUrlHelper.GetImageUrl( photoGuid.Value );
            }

            box.ImageCssClass = GetAttributeValue( AttributeKey.ImageCssClass );

            // The main page button label uses the opportunity type (e.g. "Trip Page").
            var opportunityType = DefinedValueCache.Get( group.GetAttributeValue( FundraisingAttributeKey.OpportunityType ).AsGuid() );
            box.MainPageButtonText = $"{opportunityType?.Value} Page";

            box.ProfileHtml = GetProfileHtml( group, groupMember );
            box.ProgressHtml = GetProgressHtml( group, groupMember, participationMode, isViewerAuthorized, isViewerTheParticipant, opportunityType );

            // Updates are shown when the opportunity has an update content channel configured.
            box.UpdatesHtml = GetUpdatesHtml( group, groupMember, out var updatesItemCount );
            box.IsUpdatesTabVisible = box.UpdatesHtml != null;

            if ( box.IsUpdatesTabVisible )
            {
                box.UpdatesTabLabel = $"{opportunityType?.Value} Updates ({updatesItemCount})";
            }

            // Settings-driven display values.
            box.ContributionsHeader = GetAttributeValue( AttributeKey.ContributionsHeader );
            box.IsAmountColumnVisible = GetAttributeValue( AttributeKey.ShowAmount ).AsBoolean();
            box.IsClipboardIconVisible = GetAttributeValue( AttributeKey.ShowClipboardIcon ).AsBoolean();

            // Comments gating. The notes themselves are loaded by the comments control.
            box.IsCommentingEnabled = group.GetAttributeValue( FundraisingAttributeKey.EnableCommenting ).AsBoolean();
        }

        /// <summary>
        /// Resolves the Profile Lava template displayed at the top of the main panel. Shown to
        /// every viewer (not gated by authorization).
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <param name="groupMember">The participant group member.</param>
        /// <returns>The resolved profile HTML.</returns>
        private string GetProfileHtml( Rock.Model.Group group, GroupMember groupMember )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            mergeFields.Add( "GroupMember", groupMember );

            return GetAttributeValue( AttributeKey.ProfileLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Builds the profile-completeness tip for the participant, listing the missing
        /// pieces (a photo and/or a personal introduction).
        /// </summary>
        /// <param name="groupMember">The participant group member.</param>
        /// <returns>The tip text, or <c>null</c> when nothing is missing.</returns>
        private string GetProfileWarningText( GroupMember groupMember )
        {
            var missingItems = new List<string>();

            if ( !groupMember.Person.PhotoId.HasValue )
            {
                missingItems.Add( "photo" );
            }

            if ( groupMember.GetAttributeValue( FundraisingAttributeKey.PersonalOpportunityIntroduction ).IsNullOrWhiteSpace() )
            {
                missingItems.Add( "personal opportunity introduction" );
            }

            if ( !missingItems.Any() )
            {
                return null;
            }

            return $"Edit your profile to add a {missingItems.AsDelimited( ", ", " and " )}.";
        }

        /// <summary>
        /// Builds the configuration the group member requirements container needs to load and
        /// display the participant's requirement statuses.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <param name="groupMember">The participant group member.</param>
        /// <returns>The requirements container configuration.</returns>
        private FundraisingParticipantRequirementsBag GetRequirementsBag( Rock.Model.Group group, GroupMember groupMember )
        {
            // The group role guid comes from the cached group type to avoid a query.
            var groupRoleGuid = GroupTypeCache.Get( group.GroupTypeId )?.Roles
                .FirstOrDefault( r => r.Id == groupMember.GroupRoleId )?.Guid;

            return new FundraisingParticipantRequirementsBag
            {
                GroupGuid = group.Guid,
                GroupRoleGuid = groupRoleGuid,
                GroupMemberGuid = groupMember.Guid,
                PersonGuid = groupMember.Person.Guid,
                WorkflowEntryLinkedPageValue = GetAttributeValue( AttributeKey.WorkflowEntryPage )
            };
        }

        /// <summary>
        /// Gets the URLs the block navigates to for linked pages.
        /// </summary>
        /// <returns>A dictionary of navigation keys to URLs.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var group = GetGroup();
            var groupId = group?.Id.ToString() ?? string.Empty;

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.MainPage] = this.GetLinkedPageUrl( AttributeKey.MainPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, groupId }
                } ),
                [NavigationUrlKey.DonationPage] = this.GetLinkedPageUrl( AttributeKey.DonationPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, groupId }
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
            if ( _group == null )
            {
                _group = new GroupService( RockContext )
                    .Get( PageParameter( PageParameterKey.GroupId ), !PageCache.Layout.Site.DisablePredictableIds );
                _group?.LoadAttributes( RockContext );
            }

            return _group;
        }

        /// <summary>
        /// Gets the participant group member, loading it (and its attributes) once from the
        /// page parameter. The parameter may be supplied as an Id, IdKey, or Guid. The member
        /// must belong to the opportunity group.
        /// </summary>
        /// <returns>The group member, or <c>null</c> when it could not be found.</returns>
        private GroupMember GetGroupMember()
        {
            if ( _groupMember == null )
            {
                var group = GetGroup();
                if ( group != null )
                {
                    // Resolve the key to an integer Id so we can use a Queryable with
                    // eager-loaded navigation properties (avoiding lazy-load round trips
                    // for Person and Person.PrimaryFamily later in the request).
                    var key = PageParameter( PageParameterKey.GroupMemberId );
                    var allowIntId = !PageCache.Layout.Site.DisablePredictableIds;
                    var id = ( allowIntId ? key.AsIntegerOrNull() : null )
                        ?? Rock.Utility.IdHasher.Instance.GetId( key );

                    GroupMember groupMember = null;

                    if ( id.HasValue )
                    {
                        groupMember = new GroupMemberService( RockContext )
                            .Queryable()
                            .Include( m => m.Person.PrimaryFamily )
                            .FirstOrDefault( m => m.Id == id.Value );
                    }
                    else
                    {
                        var guid = key.AsGuidOrNull();
                        if ( guid.HasValue )
                        {
                            groupMember = new GroupMemberService( RockContext )
                                .Queryable()
                                .Include( m => m.Person.PrimaryFamily )
                                .FirstOrDefault( m => m.Guid == guid.Value );
                        }
                    }

                    // The member must belong to the opportunity group.
                    if ( groupMember != null && groupMember.GroupId == group.Id )
                    {
                        groupMember.LoadAttributes( RockContext );
                        _groupMember = groupMember;
                    }
                }
            }

            return _groupMember;
        }

        private bool IsViewerAuthorized()
        {
            var groupMember = GetGroupMember();
            if ( groupMember == null )
            {
                return false;
            }

            if ( RequestContext.CurrentPerson?.Id == groupMember.PersonId )
            {
                return true;
            }

            var participationMode = GetGroup().GetAttributeValue( FundraisingAttributeKey.ParticipationType ).ConvertToEnum<ParticipationType>( ParticipationType.Individual );
            return IsViewerAnAuthorizedFamilyMember( participationMode );
        }

        /// <summary>
        /// Determines whether the current person is an authorized family member of the
        /// participant. This is only true when the opportunity uses Family participation and
        /// the current person is in the participant's family.
        /// </summary>
        /// <param name="participationMode">The opportunity participation type.</param>
        /// <returns><c>true</c> when the current person is an authorized family member.</returns>
        private bool IsViewerAnAuthorizedFamilyMember( ParticipationType participationMode )
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;
            if ( participationMode != ParticipationType.Family || !currentPersonId.HasValue )
            {
                return false;
            }

            return GetParticipantFamilyMemberPersonIds().Contains( currentPersonId.Value );
        }

        /// <summary>
        /// Gets the person identifiers of everyone in the participant's family, loading them
        /// once per request. Used both to authorize family viewers and to aggregate Family
        /// participation totals, so the underlying lookup runs at most once.
        /// </summary>
        /// <returns>The participant's family member person identifiers.</returns>
        private List<int> GetParticipantFamilyMemberPersonIds()
        {
            if ( _participantFamilyMemberPersonIds == null )
            {
                var groupMember = GetGroupMember();
                _participantFamilyMemberPersonIds = groupMember?.Person.GetFamilyMembers( true ).Select( m => m.PersonId ).ToList()
                    ?? new List<int>();
            }

            return _participantFamilyMemberPersonIds;
        }

        /// <summary>
        /// Resolves the Progress Lava template (the progress bar). Shown to every viewer, but
        /// the progress title is only included for the participant or an authorized family
        /// member, matching the original behavior.
        /// </summary>
        /// <returns>The resolved progress HTML.</returns>
        private string GetProgressHtml( Rock.Model.Group group, GroupMember groupMember, ParticipationType participationMode, bool isViewerAuthorized, bool isViewerTheParticipant, DefinedValueCache opportunityType )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            mergeFields.Add( "GroupMember", groupMember );

            // The progress title is only shown to the participant or an authorized family member.
            if ( isViewerAuthorized )
            {
                var progressTitle = participationMode == ParticipationType.Individual
                    ? groupMember.Person.FullName
                    : groupMember.Person.PrimaryFamily?.Name;
                mergeFields.Add( "ProgressTitle", progressTitle );
            }

            GetFundraisingProgress( group, groupMember, participationMode, out var contributionTotal, out var fundraisingGoal );

            var amountLeft = fundraisingGoal - contributionTotal;

            var percentMet = fundraisingGoal > 0 ? contributionTotal * 100 / fundraisingGoal : 100;

            mergeFields.Add( "AmountLeft", amountLeft );
            mergeFields.Add( "PercentMet", percentMet );

            // The donation page link carries the participation mode so the donation block can
            // attribute the gift correctly.
            var donationQueryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, group.Id.ToString() },
                { PageParameterKey.GroupMemberId, groupMember.Id.ToString() },
                { "ParticipationMode", participationMode.ToString( "D" ) }
            };
            mergeFields.Add( "MakeDonationUrl", this.GetLinkedPageUrl( AttributeKey.DonationPage, donationQueryParams ) );

            // The participant sees "Make Payment"; everyone else sees a contribute prompt.
            mergeFields.Add( "MakeDonationButtonText", isViewerTheParticipant
                ? "Make Payment"
                : $"Contribute to {groupMember.Person.NickName?.ToPossessive()} {opportunityType?.Value}" );

            return GetAttributeValue( AttributeKey.ProgressLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Resolves the Updates Lava template from the opportunity's update content channel.
        /// Returns <c>null</c> when no update content channel is configured (or it no longer
        /// exists), which also hides the Updates tab.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <param name="groupMember">The participant group member.</param>
        /// <param name="itemCount">The number of content channel items found, or 0 when null is returned.</param>
        /// <returns>The resolved updates HTML, or <c>null</c> when there is no content channel.</returns>
        private string GetUpdatesHtml( Rock.Model.Group group, GroupMember groupMember, out int itemCount )
        {
            itemCount = 0;

            var updatesContentChannelGuid = group.GetAttributeValue( FundraisingAttributeKey.UpdateContentChannel ).AsGuidOrNull();
            if ( !updatesContentChannelGuid.HasValue )
            {
                return null;
            }

            var contentChannel = new ContentChannelService( RockContext ).Get( updatesContentChannelGuid.Value );
            if ( contentChannel == null )
            {
                return null;
            }

            var contentChannelItems = new ContentChannelItemService( RockContext ).Queryable()
                .Where( a => a.ContentChannelId == contentChannel.Id )
                .AsNoTracking()
                .ToList();

            itemCount = contentChannelItems.Count;

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            mergeFields.Add( "GroupMember", groupMember );
            mergeFields.Add( "ContentChannelItems", contentChannelItems );

            return GetAttributeValue( AttributeKey.UpdatesLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Computes the participant's contribution total and fundraising goal. For Family
        /// participation, the totals are summed across all family members on the team; the
        /// goal falls back from the member's individual goal to the group's goal.
        /// </summary>
        private void GetFundraisingProgress( Rock.Model.Group group, GroupMember groupMember, ParticipationType participationMode, out decimal contributionTotal, out decimal? fundraisingGoal )
        {
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();

            if ( participationMode == ParticipationType.Family )
            {
                var familyMemberPersonIds = GetParticipantFamilyMemberPersonIds();

                // Query only the family members in this group rather than lazy-loading all
                // group members and filtering in memory.
                var familyGroupMembers = new GroupMemberService( RockContext )
                    .Queryable()
                    .Where( m => m.GroupId == group.Id && familyMemberPersonIds.Contains( m.PersonId ) )
                    .ToList();

                contributionTotal = new FinancialTransactionDetailService( RockContext )
                    .GetContributionsForGroupMemberList( entityTypeIdGroupMember, familyGroupMembers.Select( m => m.Id ).ToList() );

                // Bulk-load the individual fundraising goal for every family member in a single
                // query rather than loading each member's attributes one at a time in the loop.
                Helper.LoadFilteredAttributes( typeof( GroupMember ), familyGroupMembers.Cast<IHasAttributes>().ToList(), RockContext,
                    a => a.Key == FundraisingAttributeKey.IndividualFundraisingGoal );

                // Sum each family member's individual goal, falling back to the group goal.
                var groupGoal = group.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull() ?? 0;

                fundraisingGoal = 0;

                foreach ( var member in familyGroupMembers )
                {
                    fundraisingGoal += member.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull() ?? groupGoal;
                }
            }
            else
            {
                contributionTotal = new FinancialTransactionDetailService( RockContext ).Queryable()
                    .Where( d => d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMember.Id )
                    .Sum( a => ( decimal? ) a.Amount ) ?? 0.00M;

                fundraisingGoal = groupMember.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                    ?? group.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull();
            }
        }

        /// <summary>
        /// Gets the grid builder for the contributions grid. The row values are computed
        /// server-side (including anonymization and the per-member amount), so the builder
        /// simply maps the already-prepared <see cref="ContributionRow"/> fields.
        /// </summary>
        /// <returns>The grid builder for the contributions grid.</returns>
        private GridBuilder<ContributionRow> GetGridBuilder()
        {
            return new GridBuilder<ContributionRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddDateTimeField( "transactionDateTime", a => a.TransactionDateTime )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "address", a => a.Address )
                .AddField( "amount", a => a.Amount );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupMember"></param>
        /// <param name="group"></param>
        /// <param name="currentPerson"></param>
        /// <returns></returns>
        private HashSet<string> GetEditableGroupMemberAttributeKeys( GroupMember groupMember, Rock.Model.Group group, Person currentPerson )
        {
            var allowEditGoal = group.GetAttributeValue( FundraisingAttributeKey.AllowIndividualEditingofFundraisingGoal ).AsBoolean();
            var allowDisableContributionRequests = group.GetAttributeValue( FundraisingAttributeKey.AllowIndividualDisablingofContributionRequests ).AsBoolean();

            var editableKeys = new HashSet<string>();

            foreach ( var attribute in groupMember.Attributes.Values )
            {
                bool isEditable;

                switch ( attribute.Key )
                {
                    case FundraisingAttributeKey.IndividualFundraisingGoal:
                        isEditable = allowEditGoal;
                        break;
                    case FundraisingAttributeKey.DisablePublicContributionRequests:
                        isEditable = allowDisableContributionRequests;
                        break;
                    case FundraisingAttributeKey.PersonalOpportunityIntroduction:
                        isEditable = true;
                        break;
                    default:
                        isEditable = attribute.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson );
                        break;
                }

                if ( isEditable )
                {
                    editableKeys.Add( attribute.Key );
                }
            }

            return editableKeys;
        }

        /// <summary>
        /// Gets the note types used for participant comments (the single Note Type configured
        /// in the block setting).
        /// </summary>
        /// <returns>The configured comment note types.</returns>
        private List<NoteTypeCache> GetCommentNoteTypes()
        {
            var noteType = NoteTypeCache.Get( GetAttributeValue( AttributeKey.NoteType ).AsGuid() );
            return noteType != null ? new List<NoteTypeCache> { noteType } : new List<NoteTypeCache>();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the contributions grid data for the participant. The viewer must be authorized
        /// and the participant must not have disabled public contribution requests.
        /// </summary>
        /// <returns>The grid data bag for the contributions grid.</returns>
        [BlockAction]
        public BlockActionResult GetContributionsGridData()
        {
            var groupMember = GetGroupMember();

            // Re-enforce the same gate the view uses; never trust the client visibility flag.
            if ( groupMember == null || !IsViewerAuthorized() )
            {
                return ActionForbidden();
            }

            if ( groupMember.GetAttributeValue( FundraisingAttributeKey.DisablePublicContributionRequests ).AsBoolean() )
            {
                return ActionForbidden();
            }

            var showAmount = GetAttributeValue( AttributeKey.ShowAmount ).AsBoolean();
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();
            var groupMemberId = groupMember.Id;

            // Transactions whose details are credited to this participant. Include the details
            // and the donor's mailing location so neither is lazy-loaded per row.
            var transactions = new FinancialTransactionService( RockContext ).Queryable()
                .Include( t => t.TransactionDetails )
                .Include( t => t.AuthorizedPersonAlias.Person.PrimaryFamily.GroupLocations.Select( gl => gl.Location ) )
                .Where( t => t.TransactionDetails.Any( d => d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMemberId ) )
                .OrderByDescending( t => t.TransactionDateTime )
                .ToList();

            var rows = new List<ContributionRow>();

            foreach ( var transaction in transactions )
            {
                var person = transaction.AuthorizedPersonAlias?.Person;

                var row = new ContributionRow
                {
                    IdKey = transaction.IdKey,
                    TransactionDateTime = transaction.TransactionDateTime,
                    Name = transaction.ShowAsAnonymous ? "Anonymous" : person?.FullName,
                    Address = transaction.ShowAsAnonymous ? string.Empty : person?.GetMailingLocation()?.GetFullStreetAddress()
                };

                // The amount is only computed (and exposed) when the block is set to show it.
                // A transaction may be split, so sum only the details credited to this member.
                if ( showAmount )
                {
                    row.Amount = transaction.TransactionDetails
                        .Where( d => d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMemberId )
                        .Sum( d => ( decimal? ) d.Amount );
                }

                rows.Add( row );
            }

            return ActionOk( GetGridBuilder().Build( rows ) );
        }

        /// <summary>
        /// Gets the details needed to edit the participant's profile. Editing is restricted to
        /// the participant themselves (not other family members).
        /// </summary>
        /// <returns>The edit details for the participant.</returns>
        [BlockAction]
        public BlockActionResult GetEditDetails()
        {
            var group = GetGroup();
            var groupMember = GetGroupMember();
            var currentPerson = RequestContext.CurrentPerson;

            // Editing the profile is owner-only.
            if ( group == null || groupMember == null || currentPerson == null || currentPerson.Id != groupMember.PersonId )
            {
                return ActionForbidden();
            }

            var person = groupMember.Person;
            person.LoadAttributes( RockContext );

            // Group member attributes are limited to the ones the participant may edit.
            var editableGroupMemberAttributeKeys = GetEditableGroupMemberAttributeKeys( groupMember, group, currentPerson );
            bool isGroupMemberAttributeEditable( AttributeCache attribute ) => editableGroupMemberAttributeKeys.Contains( attribute.Key );

            // Person attributes are limited to those selected in the block setting.
            var selectedPersonAttributeGuids = GetAttributeValue( AttributeKey.PersonAttributes ).SplitDelimitedValues().AsGuidList();
            bool isPersonAttributeEditable( AttributeCache attribute ) => selectedPersonAttributeGuids.Contains( attribute.Guid );

            var bag = new FundraisingParticipantEditBag
            {
                ProfileTitle = $"{person.FullName?.ToPossessive()} Profile for {group.GetAttributeValue( FundraisingAttributeKey.OpportunityTitle )}",
                DateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( group.GetAttributeValue( FundraisingAttributeKey.OpportunityDateRange ) ).ToString( "MMMM d, yyyy" ),
                NoPictureUrl = Person.GetPersonNoPictureUrl( person, 200, 200 ),
                PhotoBinaryFile = GetPhotoListItem( person ),
                GroupMemberAttributes = groupMember.GetPublicAttributesForEdit( currentPerson, enforceSecurity: false, attributeFilter: isGroupMemberAttributeEditable ),
                GroupMemberAttributeValues = groupMember.GetPublicAttributeValuesForEdit( currentPerson, enforceSecurity: false, attributeFilter: isGroupMemberAttributeEditable ),
                PersonAttributes = person.GetPublicAttributesForEdit( currentPerson, enforceSecurity: false, attributeFilter: isPersonAttributeEditable ),
                PersonAttributeValues = person.GetPublicAttributeValuesForEdit( currentPerson, enforceSecurity: false, attributeFilter: isPersonAttributeEditable )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Gets the participant's current photo as a list item (binary file guid + name), or
        /// <c>null</c> when they have no photo.
        /// </summary>
        /// <param name="person">The participant person.</param>
        /// <returns>The photo list item, or <c>null</c>.</returns>
        private ListItemBag GetPhotoListItem( Person person )
        {
            if ( !person.PhotoId.HasValue )
            {
                return null;
            }

            var photo = new BinaryFileService( RockContext ).Get( person.PhotoId.Value );
            if ( photo == null )
            {
                return null;
            }

            return new ListItemBag
            {
                Value = photo.Guid.ToString(),
                Text = photo.FileName
            };
        }

        /// <summary>
        /// Saves the participant's edited profile (photo, group member attribute values, and
        /// selected person attribute values). Editing is restricted to the participant.
        /// </summary>
        /// <param name="bag">The details to save.</param>
        /// <returns>The result of the save operation.</returns>
        [BlockAction]
        public BlockActionResult SaveEditDetails( FundraisingParticipantSaveBag bag )
        {
            var group = GetGroup();
            var groupMember = GetGroupMember();
            var currentPerson = RequestContext.CurrentPerson;

            // Editing the profile is owner-only.
            if ( group == null || groupMember == null || currentPerson == null || currentPerson.Id != groupMember.PersonId )
            {
                return ActionForbidden();
            }

            var person = new PersonService( RockContext ).Get( groupMember.PersonId );
            if ( person == null )
            {
                return ActionNotFound();
            }

            var binaryFileService = new BinaryFileService( RockContext );

            // Resolve the newly selected photo (if any) and detect a change.
            var newPhotoGuid = bag.PhotoBinaryFile?.Value.AsGuidOrNull();
            var newPhotoBinaryFile = newPhotoGuid.HasValue ? binaryFileService.Get( newPhotoGuid.Value ) : null;
            var newPhotoId = newPhotoBinaryFile?.Id;

            int? orphanedPhotoId = null;
            if ( person.PhotoId != newPhotoId )
            {
                orphanedPhotoId = person.PhotoId;
                person.PhotoId = newPhotoId;

                // A newly uploaded photo starts out temporary; keep it now that it is in use.
                if ( newPhotoBinaryFile != null )
                {
                    newPhotoBinaryFile.IsTemporary = false;
                }

                // The photo was changed or removed, so flag the person as pending in the Photo
                // Request group for re-verification.
                AddOrUpdatePhotoVerifyPending( person );
            }

            // Save the group member attribute values, re-enforcing the editable filter so a
            // client cannot save attributes it was not allowed to edit. (Group member
            // attributes are already loaded by GetGroupMember.)
            var editableGroupMemberAttributeKeys = GetEditableGroupMemberAttributeKeys( groupMember, group, currentPerson );
            groupMember.SetPublicAttributeValues( bag.GroupMemberAttributeValues, currentPerson, enforceSecurity: false, attributeFilter: a => editableGroupMemberAttributeKeys.Contains( a.Key ) );

            // Save the selected person attribute values (limited to those in the block setting).
            person.LoadAttributes( RockContext );
            var selectedPersonAttributeGuids = GetAttributeValue( AttributeKey.PersonAttributes ).SplitDelimitedValues().AsGuidList();
            person.SetPublicAttributeValues( bag.PersonAttributeValues, currentPerson, enforceSecurity: false, attributeFilter: a => selectedPersonAttributeGuids.Contains( a.Guid ) );

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                groupMember.SaveAttributeValues( RockContext );
                person.SaveAttributeValues( RockContext );

                // Mark the previous photo as temporary so it gets cleaned up later.
                if ( orphanedPhotoId.HasValue )
                {
                    var orphanedBinaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                    if ( orphanedBinaryFile != null )
                    {
                        orphanedBinaryFile.IsTemporary = true;
                        RockContext.SaveChanges();
                    }
                }
            } );

            // Return the refreshed view box so the client can update in place without a reload.
            return ActionOk( GetInitializationBox() );
        }

        /// <summary>
        /// Flags the person as pending in the Photo Request group so a changed or removed photo
        /// is re-verified. Uses a separate context to mirror the original block's behavior.
        /// </summary>
        /// <param name="person">The person whose photo changed.</param>
        private void AddOrUpdatePhotoVerifyPending( Person person )
        {
            using ( var photoRequestRockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( photoRequestRockContext );
                var photoRequestGroup = new GroupService( photoRequestRockContext ).Get( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );

                var photoRequestGroupMember = groupMemberService.Queryable()
                    .FirstOrDefault( a => a.GroupId == photoRequestGroup.Id && a.PersonId == person.Id );

                if ( photoRequestGroupMember == null )
                {
                    photoRequestGroupMember = new GroupMember
                    {
                        GroupId = photoRequestGroup.Id,
                        PersonId = person.Id,
                        GroupRoleId = photoRequestGroup.GroupType.DefaultGroupRoleId ?? -1
                    };
                    groupMemberService.Add( photoRequestGroupMember );
                }

                photoRequestGroupMember.GroupMemberStatus = GroupMemberStatus.Pending;

                photoRequestRockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the comments (notes) for the participant. Available to any viewer when the
        /// opportunity has commenting enabled; per-note view security is still applied.
        /// </summary>
        /// <returns>The comments data.</returns>
        [BlockAction]
        public BlockActionResult GetComments()
        {
            var group = GetGroup();
            var groupMember = GetGroupMember();

            if ( group == null || groupMember == null )
            {
                return ActionNotFound();
            }

            if ( !group.GetAttributeValue( FundraisingAttributeKey.EnableCommenting ).AsBoolean() )
            {
                return ActionForbidden();
            }

            var noteTypes = GetCommentNoteTypes();
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson )
            {
                AllowedNoteTypes = noteTypes
            };

            var noteCollection = noteClientService.GetViewableNotes( groupMember );
            var notes = noteClientService.OrderNotes( noteCollection, descending: true ).ToList();
            var watchedNoteIds = noteClientService.GetWatchedNoteIds( notes );
            notes.LoadAttributes( RockContext );

            var isLoggedIn = RequestContext.CurrentPerson != null;

            var bag = new FundraisingParticipantCommentsBag
            {
                Notes = notes.Select( n => noteClientService.GetNoteBag( n, watchedNoteIds ) ).ToList(),
                NoteTypes = noteTypes.Select( nt => noteClientService.GetNoteTypeBag( nt ) ).ToList(),
                IsAddAllowed = isLoggedIn,

                // Anonymous viewers can read comments but must log in to add one; surface the
                // login URL so the client can show a "Log In to Comment" link.
                LoginUrl = isLoggedIn ? null : this.GetLoginPageUrl( this.GetCurrentPageUrl() )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Saves a participant comment (a new note or changes to an existing one).
        /// </summary>
        /// <param name="request">The note to save.</param>
        /// <returns>The saved note for display.</returns>
        [BlockAction]
        public BlockActionResult SaveComment( SaveNoteRequestBag request )
        {
            var group = GetGroup();
            var groupMember = GetGroupMember();

            if ( group == null || groupMember == null )
            {
                return ActionNotFound();
            }

            // Commenting must be enabled and the person must be logged in to add or edit.
            if ( !group.GetAttributeValue( FundraisingAttributeKey.EnableCommenting ).AsBoolean() || RequestContext.CurrentPerson == null )
            {
                return ActionForbidden();
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson )
            {
                AllowedNoteTypes = GetCommentNoteTypes()
            };

            var noteBag = noteClientService.SaveNote( request, groupMember, PageCache.Id, this.GetCurrentPageUrl(), RequestContext, out var errorMessage );

            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Gets the editable details of an existing comment.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the note.</param>
        /// <returns>The editable note details.</returns>
        [BlockAction]
        public BlockActionResult EditComment( string idKey )
        {
            var note = new NoteService( RockContext ).Get( idKey, false );
            if ( note == null )
            {
                return ActionNotFound( "Comment not found." );
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var noteBag = noteClientService.EditNote( note, out var errorMessage );

            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Deletes a participant comment.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the note.</param>
        /// <returns>An empty result when the comment is deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteComment( string idKey )
        {
            var note = new NoteService( RockContext ).Get( idKey, false );
            if ( note == null )
            {
                return ActionNotFound( "Comment not found." );
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );

            if ( !noteClientService.DeleteNote( note, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        /// <summary>
        /// Sets the watched state of a participant comment for the current person.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the note.</param>
        /// <param name="isWatching">Whether the note should be watched.</param>
        /// <returns>An empty result when the watch state is updated.</returns>
        [BlockAction]
        public BlockActionResult WatchComment( string idKey, bool isWatching )
        {
            var note = new NoteService( RockContext ).Get( idKey, false );
            if ( note == null )
            {
                return ActionNotFound( "Comment not found." );
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );

            if ( !noteClientService.WatchNote( note, isWatching, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        #endregion Block Actions

        #region Helper Classes

        /// <summary>
        /// A single contribution row displayed on the contributions grid. All values are
        /// computed server-side, including anonymization (anonymous gifts show "Anonymous"
        /// and a blank address) and the amount summed across the details that belong to this
        /// participant.
        /// </summary>
        private class ContributionRow
        {
            /// <summary>
            /// Gets or sets the financial transaction's hashed identifier (the grid key).
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the date and time of the transaction.
            /// </summary>
            public System.DateTime? TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the donor's name, or "Anonymous" for anonymous gifts.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the donor's mailing address, or an empty string for anonymous gifts.
            /// </summary>
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets the amount of the gift credited to this participant.
            /// </summary>
            public decimal? Amount { get; set; }
        }

        #endregion Helper Classes
    }
}
