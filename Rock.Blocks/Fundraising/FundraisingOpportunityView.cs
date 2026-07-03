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
using Rock.ClientService.Core.Note;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Fundraising.FundraisingOpportunityView;
using Rock.ViewModels.Controls;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Fundraising
{
    /// <summary>
    /// Public facing block that shows a fundraising opportunity.
    /// </summary>
    [DisplayName( "Fundraising Opportunity View" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity" )]
    [IconCssClass( "ti ti-world" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField(
        "Summary Lava Template",
        Key = AttributeKey.SummaryLavaTemplate,
        Description = "Lava template for what to display at the top of the main panel. Usually used to display title and other details about the fundraising opportunity.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingOpportunitySummary.lava' %}",
        Order = 1 )]

    [CodeEditorField(
        "Sidebar Lava Template",
        Key = AttributeKey.SidebarLavaTemplate,
        Description = "Lava template for what to display on the left side bar. Usually used to show event registration or other info.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingOpportunitySidebar.lava' %}",
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
        "Participant Lava Template",
        Key = AttributeKey.ParticipantLavaTemplate,
        Description = "Lava template for how the participant actions and progress bar should be displayed",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingOpportunityParticipant.lava' %}",
        Order = 4 )]

    [NoteTypeField(
        "Note Type",
        Key = AttributeKey.NoteType,
        Description = "Note Type to use for comments",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Group",
        DefaultValue = "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95",
        IsRequired = true,
        Order = 5 )]

    [LinkedPage(
        "Donation Page",
        Key = AttributeKey.DonationPage,
        Description = "The page where a person can donate to the fundraising opportunity",
        IsRequired = false,
        Order = 6 )]

    [LinkedPage(
        "Leader Toolbox Page",
        Key = AttributeKey.LeaderToolboxPage,
        Description = "The toolbox page for a leader of this fundraising opportunity",
        IsRequired = false,
        Order = 7 )]

    [LinkedPage(
        "Participant Page",
        Key = AttributeKey.ParticipantPage,
        Description = "The participant page for a participant of this fundraising opportunity",
        IsRequired = false,
        Order = 8 )]

    [BooleanField(
        "Set Page Title to Opportunity Title",
        Key = AttributeKey.SetPageTitleToOpportunityTitle,
        DefaultBooleanValue = true,
        Order = 9 )]

    [LinkedPage(
        "Registration Page",
        Key = AttributeKey.RegistrationPage,
        Description = "The page to use for registrations.",
        IsRequired = false,
        Order = 10 )]

    [TextField(
        "Image CSS Class",
        Key = AttributeKey.ImageCssClass,
        Description = "CSS class to apply to the image.",
        IsRequired = false,
        DefaultValue = "img-thumbnail",
        Order = 11 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "968FEB66-796C-4207-A17D-7B91C9214375" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "7DAF8CEC-7A8D-4BEC-BCA1-F2C8E883E3A7" )]
     [Rock.SystemGuid.BlockTypeGuid( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241" )]
    public class FundraisingOpportunityView : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SummaryLavaTemplate = "SummaryLavaTemplate";
            public const string SidebarLavaTemplate = "SidebarLavaTemplate";
            public const string UpdatesLavaTemplate = "UpdatesLavaTemplate";
            public const string ParticipantLavaTemplate = "ParticipantLavaTemplate";
            public const string NoteType = "NoteType";
            public const string DonationPage = "DonationPage";
            public const string LeaderToolboxPage = "LeaderToolboxPage";
            public const string ParticipantPage = "ParticipantPage";
            public const string SetPageTitleToOpportunityTitle = "SetPageTitletoOpportunityTitle";
            public const string RegistrationPage = "RegistrationPage";
            public const string ImageCssClass = "ImageCssClass";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string ParticipationMode = "ParticipationMode";
        }

        private static class NavigationUrlKey
        {
            public const string DonationPage = "DonationPage";
            public const string LeaderToolboxPage = "LeaderToolboxPage";
        }

        /// <summary>
        /// Attribute keys for the fundraising opportunity group and its members.
        /// </summary>
        private static class FundraisingAttributeKey
        {
            public const string OpportunityTitle = "OpportunityTitle";
            public const string OpportunityType = "OpportunityType";
            public const string OpportunityPhoto = "OpportunityPhoto";
            public const string OpportunityDetails = "OpportunityDetails";
            public const string ParticipationType = "ParticipationType";
            public const string IndividualFundraisingGoal = "IndividualFundraisingGoal";
            public const string UpdateContentChannel = "UpdateContentChannel";
            public const string EnableCommenting = "EnableCommenting";
            public const string DisablePublicContributionRequests = "DisablePublicContributionRequests";
            public const string AllowDonationsUntil = "AllowDonationsUntil";
            public const string RegistrationInstance = "RegistrationInstance";
        }

        /// <summary>
        /// Attribute keys for the opportunity type (a DefinedValue).
        /// </summary>
        private static class OpportunityTypeAttributeKey
        {
            public const string DonateButtonText = "core_DonateButtonText";
        }

        #endregion Keys

        #region Fields

        private Rock.Model.Group _group;

        private List<GroupMember> _groupMembers;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new FundraisingOpportunityViewInitializationBox
            {
                NavigationUrls = GetBoxNavigationUrls()
            };

            GetFundraisingOpportunityBox( box );

            return box;

        }

        /// <summary>
        /// Populates the view payload (resolved Lava, photo, tab labels, and visibility flags)
        /// on the initialization box for the current opportunity.
        /// </summary>
        /// <param name="box">The initialization box to populate.</param>
        private void GetFundraisingOpportunityBox( FundraisingOpportunityViewInitializationBox box )
        {
            var group = GetGroup();
            if ( group == null )
            {
                box.ErrorMessage = "The fundraising opportunity could not be found.";
                return;
            }

            var opportunityType = DefinedValueCache.Get( group.GetAttributeValue( FundraisingAttributeKey.OpportunityType ).AsGuid() );
            var opportunityTitle = group.GetAttributeValue( FundraisingAttributeKey.OpportunityTitle );
            var opportunityPhotoGuid = group.GetAttributeValue( FundraisingAttributeKey.OpportunityPhoto ).AsGuidOrNull();
            var opportunityDetails = group.GetAttributeValue( FundraisingAttributeKey.OpportunityDetails );
            var participationMode = group.GetAttributeValue( FundraisingAttributeKey.ParticipationType ).ConvertToEnum<ParticipationType>( ParticipationType.Individual );
            var isCommentingEnabled = group.GetAttributeValue( FundraisingAttributeKey.EnableCommenting ).AsBoolean();
            var allowDonationsUntil = group.GetAttributeValue( FundraisingAttributeKey.AllowDonationsUntil ).AsDateTime() ?? DateTime.MaxValue;

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            mergeFields.Add( "Block", BlockCache );

            // Set the page and browser title to the opportunity title when configured to do so.
            if ( GetAttributeValue( AttributeKey.SetPageTitleToOpportunityTitle ).AsBoolean() )
            {
                RequestContext.Response.SetPageTitle( opportunityTitle );
                RequestContext.Response.SetBrowserTitle( opportunityTitle );
            }

            // Left-sidebar opportunity photo.
            if ( opportunityPhotoGuid.HasValue )
            {
                box.PhotoUrl = FileUrlHelper.GetImageUrl( opportunityPhotoGuid.Value );
            }

            box.ImageCssClass = GetAttributeValue( AttributeKey.ImageCssClass );

            AddRegistrationMergeFields( group, mergeFields );
            box.SidebarHtml = GetAttributeValue( AttributeKey.SidebarLavaTemplate ).ResolveMergeFields( mergeFields );
            box.IsLeaderToolboxVisible = IsCurrentPersonGroupLeader( group );

            box.SummaryHtml = GetAttributeValue( AttributeKey.SummaryLavaTemplate ).ResolveMergeFields( mergeFields );

            var currentGroupMember = GetCurrentPersonGroupMember( group );

            mergeFields.Add( "GroupMember", currentGroupMember );
            mergeFields.Add( "ParticipationMode", participationMode.ToString( "D" ) );

            if ( currentGroupMember != null )
            {
                box.IsParticipantActionsVisible = true;
                box.ParticipantActionsHtml = GetParticipantActionsHtml( group, currentGroupMember, participationMode, mergeFields );
            }


            // Tab: Details.
            box.DetailsHtml = opportunityDetails;
            box.DetailsTabLabel = $"{opportunityType?.Value} Details";

            // Tab: Updates. Shown when the opportunity has an update content channel configured.
            box.UpdatesHtml = GetUpdatesHtml( group, mergeFields, out var updatesItemCount );
            box.IsUpdatesTabVisible = box.UpdatesHtml != null;

            if ( box.IsUpdatesTabVisible )
            {
                box.UpdatesTabLabel = $"{opportunityType?.Value} Updates ({updatesItemCount})";
            }

            box.IsCommentsTabVisible = isCommentingEnabled;

            // Match the legacy label ("Comments (N)"); the count is only meaningful (and only
            // queried) when the tab is actually shown.
            box.CommentsTabLabel = isCommentingEnabled
                ? $"Comments ({GetCommentCount( group )})"
                : "Comments";

            var donationsAllowed = RockDateTime.Now <= allowDonationsUntil;
            box.IsDonateToParticipantVisible = donationsAllowed && DoesAnyMemberAllowDonations( group );

            var donateButtonText = opportunityType?.GetAttributeValue( OpportunityTypeAttributeKey.DonateButtonText );
            box.DonateToParticipantButtonText = donateButtonText.IsNotNullOrWhiteSpace() ? donateButtonText : "Donate to a Participant";

        }

        /// <summary>
        /// Resolves the Updates Lava template from the opportunity's update content channel.
        /// Returns <c>null</c> when no update content channel is configured (or it no longer
        /// exists), which also hides the Updates tab.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <param name="mergeFields">The shared merge fields to resolve the template with.</param>
        /// <param name="itemCount">The number of content channel items found, or 0 when null is returned.</param>
        /// <returns>The resolved updates HTML, or <c>null</c> when there is no content channel.</returns>
        private string GetUpdatesHtml( Rock.Model.Group group, Dictionary<string, object> mergeFields, out int itemCount )
        {
            itemCount = 0;

            var updatesContentChannelGuid = group.GetAttributeValue( FundraisingAttributeKey.UpdateContentChannel ).AsGuidOrNull();
            if ( !updatesContentChannelGuid.HasValue )
            {
                return null;
            }

            var contentChannel = ContentChannelCache.Get( updatesContentChannelGuid.Value );
            if ( contentChannel == null )
            {
                return null;
            }

            var contentChannelItems = new ContentChannelItemService( RockContext ).Queryable()
                .Where( a => a.ContentChannelId == contentChannel.Id )
                .AsNoTracking()
                .ToList();

            itemCount = contentChannelItems.Count;

            mergeFields.Add( "ContentChannelItems", contentChannelItems );

            return GetAttributeValue( AttributeKey.UpdatesLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Adds the event-registration merge fields consumed by the sidebar Lava template. The
        /// <c>RegistrationPage</c> link is always added (the template appends its own query
        /// string). The remaining fields are only added when the opportunity is tied to an
        /// existing registration instance via its <c>RegistrationInstance</c> attribute.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <param name="mergeFields">The shared merge fields to add the registration values to.</param>
        private void AddRegistrationMergeFields( Rock.Model.Group group, Dictionary<string, object> mergeFields )
        {
            mergeFields.Add( "RegistrationPage", this.GetLinkedPageUrl( AttributeKey.RegistrationPage ) );

            var registrationInstanceId = group.GetAttributeValue( FundraisingAttributeKey.RegistrationInstance ).AsIntegerOrNull();
            if ( !registrationInstanceId.HasValue )
            {
                return;
            }

            var registrationInstance = new RegistrationInstanceService( RockContext ).Queryable()
                .Include( ri => ri.RegistrationTemplate )
                .Include( ri => ri.ContactPersonAlias.Person )
                .Include( ri => ri.Linkages )
                .FirstOrDefault( ri => ri.Id == registrationInstanceId.Value );

            if ( registrationInstance == null )
            {
                return;
            }

            mergeFields.Add( "RegistrationInstance", registrationInstance );
            mergeFields.Add( "RegistrationInstanceLinkages", registrationInstance.Linkages );

            // The number of registrants who are not on the wait list (i.e. spots taken).
            var currentRegistrationCount = new RegistrationRegistrantService( RockContext ).Queryable().AsNoTracking()
                .Where( r => r.Registration.RegistrationInstanceId == registrationInstance.Id && r.OnWaitList == false )
                .Count();

            mergeFields.Add( "CurrentRegistrationCount", currentRegistrationCount );

            // The remaining capacity fields are only meaningful when a cap is configured.
            var maxRegistrantCount = registrationInstance.MaxAttendees;
            if ( maxRegistrantCount.HasValue )
            {
                mergeFields.Add( "MaxRegistrantCount", maxRegistrantCount );
                mergeFields.Add( "RegistrationSpotsAvailable", maxRegistrantCount - currentRegistrationCount );
            }
        }

        /// <summary>
        /// Determines whether at least one participant in the opportunity is accepting public
        /// contribution requests. A member is considered to accept them unless they have
        /// explicitly disabled them.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <returns><c>true</c> when at least one member accepts public contribution requests.</returns>
        private bool DoesAnyMemberAllowDonations( Rock.Model.Group group )
        {
            return GetGroupMembers( group.Id )
                .Any( m => !m.GetAttributeValue( FundraisingAttributeKey.DisablePublicContributionRequests ).AsBoolean() );
        }

        /// <summary>
        /// Determines whether the current person is a leader of the opportunity group.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <returns><c>true</c> when the current person has a leader role in the group.</returns>
        private bool IsCurrentPersonGroupLeader( Rock.Model.Group group )
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( !currentPersonId.HasValue )
            {
                return false;
            }

            return GetGroupMembers( group.Id )
                .Any( m => m.PersonId == currentPersonId.Value && m.GroupRole.IsLeader );
        }

        /// <summary>
        /// Gets the current person's membership in the opportunity group, loading its attributes.
        /// The participant actions panel is only shown for a person who is a participant.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <returns>The current person's group member, or <c>null</c> when they are not a participant.</returns>
        private GroupMember GetCurrentPersonGroupMember( Rock.Model.Group group )
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( !currentPersonId.HasValue )
            {
                return null;
            }

            // Attributes are already loaded by GetGroupMembers; no per-member LoadAttributes needed.
            return GetGroupMembers( group.Id ).FirstOrDefault( m => m.PersonId == currentPersonId.Value );
        }

        /// <summary>
        /// Resolves the Participant Lava template (participant actions and progress bar) for the
        /// current person. Adds the progress merge fields (title, totals, donation links, and the
        /// per-family-member list for Family participation) before resolving.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <param name="currentPerson">The current person's group member.</param>
        /// <param name="participationMode">The opportunity participation type.</param>
        /// <param name="mergeFields">The shared merge fields to add the progress values to.</param>
        /// <returns>The resolved participant actions HTML.</returns>
        private string GetParticipantActionsHtml( Rock.Model.Group group, GroupMember currentPerson, ParticipationType participationMode, Dictionary<string, object> mergeFields )
        {
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();

            // The progress title is the participant's name (Individual) or the family name (Family).
            var progressTitle = participationMode == ParticipationType.Individual
                ? currentPerson.Person.FullName
                : currentPerson.Person.PrimaryFamily?.Name;

            mergeFields.Add( "ProgressTitle", progressTitle );

            // The group-level goal is the fallback when a member has no individual goal set.
            var groupGoal = group.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull();

            decimal contributionTotal;
            decimal? fundraisingGoal;

            if ( participationMode == ParticipationType.Family )
            {
                // Get the family member ids of people in the current person's family
                var familyMemberPersonIds = new GroupMemberService( RockContext ).Queryable().AsNoTracking()
                    .Where( m => m.GroupId == currentPerson.Person.PrimaryFamilyId )
                    .Select( m => m.PersonId )
                    .ToList();

                // Now check if any group members are in current person's family
                var familyGroupMembers = GetGroupMembers( group.Id )
                    .Where( m => familyMemberPersonIds.Contains( m.PersonId ) )
                    .OrderBy( m => m.Person.AgeClassification )
                    .ThenBy( m => m.Person.Gender )
                    .ToList();

                contributionTotal = new FinancialTransactionDetailService( RockContext )
                    .GetContributionsForGroupMemberList( entityTypeIdGroupMember, familyGroupMembers.Select( m => m.Id ).ToList() );

                decimal familyGoal = 0;
                var familyMemberGroupMembers = new List<object>();

                foreach ( var member in familyGroupMembers )
                {
                    var familyMemberQueryParams = new Dictionary<string, string>
                    {
                        { PageParameterKey.GroupId, group.Id.ToString() },
                        { PageParameterKey.GroupMemberId, member.Id.ToString() },
                        { PageParameterKey.ParticipationMode, participationMode.ToString( "D" ) }
                    };

                    familyMemberGroupMembers.Add( new Dictionary<string, object>
                    {
                        { "MakeDonationUrl", this.GetLinkedPageUrl( AttributeKey.DonationPage, familyMemberQueryParams ) },
                        { "ParticipantPageUrl", this.GetLinkedPageUrl( AttributeKey.ParticipantPage, familyMemberQueryParams ) },
                        { "FullName", member.Person.FullName },
                        { "PhotoUrl", member.Person.PhotoUrl }
                    } );

                    familyGoal += member.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull() ?? groupGoal ?? 0;
                }

                fundraisingGoal = familyGoal;
                mergeFields.Add( "FamilyMemberGroupMembers", familyMemberGroupMembers );
            }
            else
            {
                contributionTotal = new FinancialTransactionDetailService( RockContext ).Queryable()
                    .Where( d => d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == currentPerson.Id )
                    .Sum( a => ( decimal? ) a.Amount ) ?? 0.00M;

                fundraisingGoal = currentPerson.GetAttributeValue( FundraisingAttributeKey.IndividualFundraisingGoal ).AsDecimalOrNull()
                    ?? groupGoal;
            }

            var amountLeft = fundraisingGoal - contributionTotal;
            var percentMet = fundraisingGoal > 0 ? contributionTotal * 100 / fundraisingGoal : 100;

            mergeFields.Add( "AmountLeft", amountLeft );
            mergeFields.Add( "PercentMet", percentMet );

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, group.Id.ToString() },
                { PageParameterKey.GroupMemberId, currentPerson.Id.ToString() },
                { PageParameterKey.ParticipationMode, participationMode.ToString( "D" ) }
            };
            mergeFields.Add( "MakeDonationUrl", this.GetLinkedPageUrl( AttributeKey.DonationPage, queryParams ) );
            mergeFields.Add( "ParticipantPageUrl", this.GetLinkedPageUrl( AttributeKey.ParticipantPage, queryParams ) );

            mergeFields.Add( "MakeDonationButtonText", "Make Payment" );

            return GetAttributeValue( AttributeKey.ParticipantLavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the fundraising opportunity group, loading it (and its attributes) once from the
        /// page parameter. The parameter may be supplied as an Id, IdKey, or Guid.
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
        /// Gets the opportunity group's members, materialized and cached once per request with the
        /// person, primary family, group role, and all member attributes loaded. Every consumer
        /// (the donation check, the leader check, and the participant panel) reads from this single
        /// list so the members are queried and their attributes loaded only once.
        /// </summary>
        /// <param name="groupId">The opportunity group identifier.</param>
        /// <returns>The group's members with attributes loaded.</returns>
        private List<GroupMember> GetGroupMembers( int groupId )
        {
            if ( _groupMembers == null )
            {
                _groupMembers = new GroupMemberService( RockContext ).Queryable()
                    .Where( m => m.GroupId == groupId )
                    .Include( m => m.Person.PrimaryFamily )
                    .Include( m => m.GroupRole )
                    .AsNoTracking()
                    .ToList();

                // Load the member attributes once, here, so every consumer (the donation check and
                // the participant Lava, which receives GroupMember and may reference any member
                // attribute) reads from the same fully-loaded instances. Loading them in a single
                // place means no later filtered load can re-initialize and clobber these instances.
                Helper.LoadFilteredAttributes( typeof( GroupMember ), _groupMembers.Cast<IHasAttributes>().ToList(), RockContext, a => true );
            }

            return _groupMembers;
        }

        /// <summary>
        /// Gets the URLs the block navigates to for linked pages.
        /// </summary>
        /// <returns>A dictionary of navigation keys to URLs.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var group = GetGroup();
            var groupId = group?.Id.ToString() ?? string.Empty;

            var groupQueryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, groupId }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DonationPage] = this.GetLinkedPageUrl( AttributeKey.DonationPage, groupQueryParams ),
                [NavigationUrlKey.LeaderToolboxPage] = this.GetLinkedPageUrl( AttributeKey.LeaderToolboxPage, groupQueryParams )
            };
        }

        /// <summary>
        /// Gets the note types used for opportunity comments (the single Note Type configured in
        /// the block setting).
        /// </summary>
        /// <returns>The configured comment note types.</returns>
        private List<NoteTypeCache> GetCommentNoteTypes()
        {
            var noteType = NoteTypeCache.Get( GetAttributeValue( AttributeKey.NoteType ).AsGuid() );
            return noteType != null ? new List<NoteTypeCache> { noteType } : new List<NoteTypeCache>();
        }

        /// <summary>
        /// Gets the number of top-level comments on the opportunity that are viewable by the
        /// current person. Only root notes are counted (replies are nested beneath their parent),
        /// matching the count shown in the legacy comments tab label.
        /// </summary>
        /// <param name="group">The opportunity group.</param>
        /// <returns>The number of viewable root comments.</returns>
        private int GetCommentCount( Rock.Model.Group group )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson )
            {
                AllowedNoteTypes = GetCommentNoteTypes()
            };

            return noteClientService.GetViewableNotes( group ).Count( n => !n.ParentNoteId.HasValue );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the comments (notes) for the opportunity. Available to any viewer when the
        /// opportunity has commenting enabled; per-note view security is still applied.
        /// </summary>
        /// <returns>The comments data.</returns>
        [BlockAction]
        public BlockActionResult GetComments()
        {
            var group = GetGroup();
            if ( group == null )
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

            var noteCollection = noteClientService.GetViewableNotes( group );
            var notes = noteClientService.OrderNotes( noteCollection, descending: true ).ToList();
            var watchedNoteIds = noteClientService.GetWatchedNoteIds( notes );
            notes.LoadAttributes( RockContext );

            var isLoggedIn = RequestContext.CurrentPerson != null;

            var bag = new FundraisingOpportunityViewCommentsBag
            {
                Notes = notes.Select( n => noteClientService.GetNoteBag( n, watchedNoteIds ) ).ToList(),
                NoteTypes = noteTypes.Select( nt => noteClientService.GetNoteTypeBag( nt ) ).ToList(),
                IsAddAllowed = isLoggedIn,

                LoginUrl = isLoggedIn ? null : this.GetLoginPageUrl( this.GetCurrentPageUrl() )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Saves an opportunity comment (a new note or changes to an existing one).
        /// </summary>
        /// <param name="request">The note to save.</param>
        /// <returns>The saved note for display.</returns>
        [BlockAction]
        public BlockActionResult SaveComment( SaveNoteRequestBag request )
        {
            var group = GetGroup();
            if ( group == null )
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

            var noteBag = noteClientService.SaveNote( request, group, PageCache.Id, this.GetCurrentPageUrl(), RequestContext, out var errorMessage );

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
        /// Deletes an opportunity comment.
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
        /// Sets the watched state of an opportunity comment for the current person.
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

        #region IBreadCrumbBlock

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var result = new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
            };

            var key = pageReference.GetPageParameter( PageParameterKey.GroupId );
            if ( key.IsNullOrWhiteSpace() )
            {
                // Don't show a breadcrumb if we don't have a page parameter to work with.
                return result;
            }

            var group = new GroupService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            if ( group == null )
            {
                return result;
            }

            group.LoadAttributes( RockContext );
            var opportunityTitle = group.GetAttributeValue( FundraisingAttributeKey.OpportunityTitle );

            result.BreadCrumbs.Add( new BreadCrumbLink( opportunityTitle, pageReference ) );

            return result;
        }

        #endregion IBreadCrumbBlock
    }
}
