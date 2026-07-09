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

using Rock;
using Rock.Attribute;
using Rock.ClientService.Core.Note;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.Notes;
using Rock.ViewModels.Blocks.Prayer.PrayerSession;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Prayer
{
    /// <summary>
    /// Allows a user to start a session to pray for active, approved prayer requests.
    /// </summary>
    [DisplayName( "Prayer Session" )]
    [Category( "Prayer" )]
    [Description( "Allows a user to start a session to pray for active, approved prayer requests." )]
    [IconCssClass( "ti ti-heart-handshake" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Welcome Introduction Text",
        Description = "Some text (or HTML) to display on the first step.",
        Key = AttributeKey.WelcomeIntroductionText,
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<h2>Let’s get ready to pray...</h2>",
        Order = 0 )]

    [CategoryField( "Category",
        Description = "A top level category. This controls which categories are shown when starting a prayer session.",
        Key = AttributeKey.CategoryGuid,
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 1 )]

    [BooleanField( "Enable Prayer Team Flagging",
        Description = "If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.",
        Key = AttributeKey.EnableCommunityFlagging,
        DefaultBooleanValue = false,
        Category = AttributeCategory.Flagging,
        Order = 2 )]

    [IntegerField( "Flag Limit",
        Description = "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.",
        Key = AttributeKey.FlagLimit,
        DefaultIntegerValue = 1,
        IsRequired = false,
        Category = AttributeCategory.Flagging,
        Order = 3 )]

    [CodeEditorField( "Prayer Person Lava",
        Description = "The Lava Template for how the person details are shown in the header",
        Key = AttributeKey.PrayerPersonLava,
        DefaultValue = PrayerPersonLavaDefaultValue,
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = true,
        Order = 4 )]

    [CodeEditorField( "Prayer Display Lava",
        Description = "The Lava Template which will show the details of the Prayer Request",
        Key = AttributeKey.PrayerDisplayLava,
        DefaultValue = PrayerDisplayLavaDefaultValue,
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = true,
        Order = 5 )]

    [BooleanField( "Display Campus",
        Description = "Should the campus field be displayed? If there is only one active campus then the campus field will not show.",
        Key = AttributeKey.DisplayCampus,
        DefaultBooleanValue = true,
        Category = AttributeCategory.Filtering,
        Order = 6 )]

    [BooleanField( "Public Only",
        Description = "If selected, all non-public prayer request will be excluded.",
        Key = AttributeKey.PublicOnly,
        DefaultBooleanValue = false,
        Order = 7 )]

    [BooleanField( "Create Interactions for Prayers",
        Description = "If enabled then this block will record an Interaction whenever somebody prays for a prayer request.",
        Key = AttributeKey.CreateInteractionsForPrayers,
        DefaultBooleanValue = true,
        IsRequired = true,
        Order = 8 )]

    [BooleanField( "Enable AI Disclaimer",
        Description = "If enabled and the PrayerRequest Text was sent to an AI automation the configured AI Disclaimer will be shown.",
        DefaultBooleanValue = false,
        Key = AttributeKey.EnableAIDisclaimer,
        Category = AttributeCategory.AIAutomations,
        Order = 9 )]

    [TextField( "AI Disclaimer",
        Description = "The message to display indicating the Prayer Request text may have been modified by an AI automation.",
        IsRequired = false,
        DefaultValue = "This request may have been modified by an AI for formatting and privacy. Please be aware that errors may be present.",
        Key = AttributeKey.AIDisclaimer,
        Category = AttributeCategory.AIAutomations,
        Order = 10 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7D904804-0EB1-423A-939D-F73DE04CAD21" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "D776E7FB-D73B-494E-939D-C7A9F5216C9C" )]
    [Rock.SystemGuid.BlockTypeGuid( "FD294789-3B72-4D83-8006-FA50B5087D06" )]
    public class PrayerSession : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string WelcomeIntroductionText = "WelcomeIntroductionText";
            public const string CategoryGuid = "CategoryGuid";
            public const string EnableCommunityFlagging = "EnableCommunityFlagging";
            public const string FlagLimit = "FlagLimit";
            public const string PrayerPersonLava = "PrayerPersonLava";
            public const string PrayerDisplayLava = "PrayerDisplayLava";
            public const string DisplayCampus = "DisplayCampus";
            public const string PublicOnly = "PublicOnly";
            public const string CreateInteractionsForPrayers = "CreateInteractionsForPrayers";
            public const string EnableAIDisclaimer = "EnableAIDisclaimer";
            public const string AIDisclaimer = "AIDisclaimer";
        }

        private static class AttributeCategory
        {
            public const string AIAutomations = "AI Automations";
            public const string Filtering = "Filtering";
            public const string Flagging = "Flagging";
        }

        private static class PageParameterKey
        {
            public const string GroupGuid = "GroupGuid";
        }

        private static class PersonPreferenceKey
        {
            public const string Campus = "campus";
            public const string Categories = "categories";
        }

        #endregion

        #region Attribute Default Values

        private const string PrayerDisplayLavaDefaultValue = @"
<div class='row'>
    <div class='col-md-6'>
        <strong>Prayer Request</strong>
    </div>
    <div class='col-md-6 text-right'>
      {% if PrayerRequest.EnteredDateTime %}
          Date Entered: {{ PrayerRequest.EnteredDateTime | Date:'M/d/yyyy' }}
      {% endif %}
    </div>
</div>

{{ PrayerRequest.Text | NewlineToBr }}

<div class='attributes margin-t-md'>
{% for prayerRequestAttribute in PrayerRequest.AttributeValues %}
    {% if prayerRequestAttribute.Value != '' %}
    <strong>{{ prayerRequestAttribute.AttributeName }}</strong>
    <p>{{ prayerRequestAttribute.ValueFormatted }}</p>
    {% endif %}
{% endfor %}
</div>

{% if PrayerRequest.Answer %}
<div class='margin-t-lg'>
    <strong>Update</strong>
    <br />
    {{ PrayerRequest.Answer | Escape | NewlineToBr }}
</div>
{% endif %}

";

        private const string PrayerPersonLavaDefaultValue = @"
{% if PrayerRequest.RequestedByPersonAlias %}
<img src='{{ PrayerRequest.RequestedByPersonAlias.Person.PhotoUrl }}' class='pull-left margin-r-md img-thumbnail' width=50 />
{% endif %}
<span class='first-word'>{{ PrayerRequest.FirstName }}</span> {{ PrayerRequest.LastName }}
";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PrayerSessionBag, PrayerSessionOptionsBag>();

            box.Options = new PrayerSessionOptionsBag
            {
                WelcomeIntroductionText = GetAttributeValue( AttributeKey.WelcomeIntroductionText ),
                IsCampusPickerVisible = GetAttributeValue( AttributeKey.DisplayCampus ).AsBoolean(),
                IsCommunityFlaggingEnabled = GetAttributeValue( AttributeKey.EnableCommunityFlagging ).AsBoolean(),
                PersonAvatarUrl = RequestContext.CurrentPerson?.PhotoUrl,
                CanAddComments = RequestContext.CurrentPerson != null
            };

            box.Bag = GetWelcomeBag();

            return box;
        }

        /// <summary>
        /// Builds the welcome-step data: the active prayer categories (with their
        /// request counts) available for selection and the person's saved category
        /// and campus selections. Shared by the initial block load and the
        /// "start again" refresh so both present identical, up-to-date data.
        /// </summary>
        /// <returns>The populated welcome bag.</returns>
        private PrayerSessionBag GetWelcomeBag()
        {
            var categoryData = GetActiveCategoryData( RockContext );
            var categoryItems = categoryData
                .Select( c => new ListItemBag { Value = c.Guid.ToString(), Text = $"{c.Name} ({c.Count})" } )
                .ToList();

            var preferences = GetBlockPersonPreferences();

            /*
                6/30/26 - MSE

                Category and campus selections are persisted as entity Ids to stay
                compatible with the values saved by the legacy WebForms block. The
                Obsidian controls work in Guids, so the saved Ids are translated to
                Guids here for pre-selection and translated back to Ids on save.

                Reason: Preserve previously-saved prayer session preferences.
            */
            var savedCategoryValues = preferences.GetValue( PersonPreferenceKey.Categories )
                .SplitDelimitedValues()
                .Select( v => v.AsIntegerOrNull() )
                .Where( id => id.HasValue )
                .Select( id => CategoryCache.Get( id.Value )?.Guid )
                .Where( guid => guid.HasValue )
                .Select( guid => guid.Value.ToString() )
                .ToList();

            ListItemBag selectedCampus = null;
            var savedCampusId = preferences.GetValue( PersonPreferenceKey.Campus ).AsIntegerOrNull();
            if ( savedCampusId.HasValue )
            {
                selectedCampus = CampusCache.Get( savedCampusId.Value )?.ToListItemBag();
            }

            return new PrayerSessionBag
            {
                Categories = categoryItems,
                SelectedCategoryValues = savedCategoryValues,
                SelectedCampus = selectedCampus,
                HasActiveCategories = categoryItems.Any()
            };
        }

        /// <summary>
        /// Builds the base query of active, approved, unexpired prayer requests that
        /// fall within the block's configured top-level category, group, and
        /// public-only scope. Shared by the category-data and category-id queries.
        /// </summary>
        /// <param name="rockContext">The data context.</param>
        /// <returns>The scoped prayer request query.</returns>
        private IQueryable<PrayerRequest> GetScopedActiveRequestQuery( RockContext rockContext )
        {
            var service = new PrayerRequestService( rockContext );
            IQueryable<PrayerRequest> qry = service.GetActiveApprovedUnexpired();

            var topLevelCategory = GetConfiguredTopLevelCategory();
            if ( topLevelCategory != null )
            {
                qry = qry.Where( p => p.Category.ParentCategoryId == topLevelCategory.Id );
            }

            qry = ApplyGroupScope( qry, rockContext );

            if ( GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean() )
            {
                qry = qry.Where( p => p.IsPublic == true );
            }

            return qry;
        }

        /// <summary>
        /// Gets the active prayer categories (with their request counts) that are
        /// available for selection on the welcome step.
        /// </summary>
        /// <param name="rockContext">The data context.</param>
        /// <returns>A list of category descriptors ordered by name.</returns>
        private List<CategoryCountInfo> GetActiveCategoryData( RockContext rockContext )
        {
            // Count active requests per category in the database, then resolve the
            // category names from cache and order them for display.
            var counts = GetScopedActiveRequestQuery( rockContext )
                .Where( p => p.CategoryId.HasValue )
                .GroupBy( p => p.CategoryId.Value )
                .Select( g => new { CategoryId = g.Key, Count = g.Count() } )
                .ToList();

            return counts
                .Select( c => new { Category = CategoryCache.Get( c.CategoryId ), c.Count } )
                .Where( x => x.Category != null )
                .OrderBy( x => x.Category.Name )
                .Select( x => new CategoryCountInfo
                {
                    Guid = x.Category.Guid,
                    Name = x.Category.Name,
                    Count = x.Count
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the distinct Ids of the categories that currently have active,
        /// approved, unexpired requests within the block's scope. Returns the same
        /// set of categories as <see cref="GetActiveCategoryData"/> (including the
        /// "resolves in cache" filter) but without the per-category counts, so it is
        /// a lighter query for callers that only need the Ids, such as preference
        /// retention.
        /// </summary>
        /// <param name="rockContext">The data context.</param>
        /// <returns>The active category Ids.</returns>
        private List<int> GetActiveCategoryIds( RockContext rockContext )
        {
            var activeCategoryIds = GetScopedActiveRequestQuery( rockContext )
                .Where( p => p.CategoryId.HasValue )
                .Select( p => p.CategoryId.Value )
                .Distinct()
                .ToList();

            // Keep only categories that resolve in the cache so this matches the set
            // GetActiveCategoryData returns (which the retention logic depends on).
            return activeCategoryIds.Where( id => CategoryCache.Get( id ) != null ).ToList();
        }

        /// <summary>
        /// Gets the configured top-level category whose child categories drive the
        /// prayer session, or <c>null</c> when none is configured.
        /// </summary>
        /// <returns>The configured category, or <c>null</c>.</returns>
        private CategoryCache GetConfiguredTopLevelCategory()
        {
            var categoryGuid = GetAttributeValue( AttributeKey.CategoryGuid ).AsGuidOrNull();
            return categoryGuid.HasValue ? CategoryCache.Get( categoryGuid.Value ) : null;
        }

        /// <summary>
        /// Gets the category Ids that fall within the block's configured category
        /// scope: the direct children of the configured top-level category and their
        /// immediate children (which a selection expands to include). Returns
        /// <c>null</c> when no top-level category is configured and therefore no
        /// category scoping applies.
        /// </summary>
        /// <returns>The in-scope category Ids, or <c>null</c> when unscoped.</returns>
        private HashSet<int> GetScopedCategoryIds()
        {
            var topLevelCategory = GetConfiguredTopLevelCategory();
            if ( topLevelCategory == null )
            {
                return null;
            }

            var scopedCategoryIds = new HashSet<int>();
            foreach ( var childCategory in topLevelCategory.Categories )
            {
                scopedCategoryIds.Add( childCategory.Id );
                foreach ( var grandchildCategory in childCategory.Categories )
                {
                    scopedCategoryIds.Add( grandchildCategory.Id );
                }
            }

            return scopedCategoryIds;
        }

        /// <summary>
        /// Applies the group page-parameter scope to a prayer request query. When a
        /// group is specified the session is limited to that group; otherwise only
        /// non-group (general) requests are included.
        /// </summary>
        /// <param name="qry">The query to scope.</param>
        /// <param name="rockContext">The data context.</param>
        /// <returns>The scoped query.</returns>
        private IQueryable<PrayerRequest> ApplyGroupScope( IQueryable<PrayerRequest> qry, RockContext rockContext )
        {
            var groupKey = PageParameter( PageParameterKey.GroupGuid );

            if ( groupKey.IsNullOrWhiteSpace() )
            {
                return qry.Where( p => !p.GroupId.HasValue );
            }

            var group = new GroupService( rockContext ).Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( group == null )
            {
                // A group was requested but could not be resolved. Match nothing
                // rather than leak general requests into a group-scoped session.
                return qry.Where( p => false );
            }

            return qry.Where( p => p.GroupId == group.Id );
        }

        /// <summary>
        /// Saves the person's category and campus selections for use during their
        /// next prayer session. Previously-saved categories that are not currently
        /// active are retained so they stay selected if they become active again.
        /// </summary>
        /// <param name="rockContext">The data context.</param>
        /// <param name="selectedCategoryValues">The category values the person selected.</param>
        /// <param name="campusValue">The campus value the person selected, if any.</param>
        private void SaveSessionPreferences( RockContext rockContext, List<string> selectedCategoryValues, string campusValue )
        {
            var preferences = GetBlockPersonPreferences();

            // Translate the selected category Guids back to Ids so the persisted
            // format matches the legacy WebForms block.
            var selectedCategoryIds = selectedCategoryValues
                .Select( v => v.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => CategoryCache.Get( guid.Value )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .ToList();

            // Previously-saved Ids that are no longer active are retained so they
            // stay selected if their category becomes active again.
            var previouslySavedIds = preferences.GetValue( PersonPreferenceKey.Categories ).SplitDelimitedValues();
            var availableIds = GetActiveCategoryIds( rockContext ).Select( id => id.ToString() ).ToList();
            var retainedIds = previouslySavedIds.Where( id => !availableIds.Contains( id ) );

            var categoryValues = selectedCategoryIds
                .Concat( retainedIds )
                .Distinct()
                .ToList()
                .AsDelimited( "," );

            // Translate the campus Guid to an Id for persistence (legacy-compatible).
            var campusId = string.Empty;
            var campusGuid = campusValue.AsGuidOrNull();
            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.Get( campusGuid.Value )?.Id.ToString() ?? string.Empty;
            }

            preferences.SetValue( PersonPreferenceKey.Categories, categoryValues );
            preferences.SetValue( PersonPreferenceKey.Campus, campusId );
            preferences.Save();
        }

        /// <summary>
        /// Builds the display details for a single prayer request, resolving its
        /// Lava templates, labels, AI disclaimer, and comments.
        /// </summary>
        /// <param name="prayerRequest">The prayer request to describe.</param>
        /// <param name="rockContext">The data context.</param>
        /// <returns>The populated request bag.</returns>
        private PrayerSessionRequestBag BuildRequestBag( PrayerRequest prayerRequest, RockContext rockContext )
        {
            // The campus label is only meaningful when more than one active campus exists.
            string campusName = null;
            if ( CampusCache.All( false ).Count > 1 && prayerRequest.CampusId.HasValue )
            {
                campusName = CampusCache.Get( prayerRequest.CampusId.Value )?.Name;
            }

            // Load attributes, hiding any the person isn't authorized to view, so the
            // Lava template only renders permitted values.
            prayerRequest.LoadAttributes( rockContext );
            var unauthorizedKeys = prayerRequest.Attributes
                .Where( a => !a.Value.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( a => a.Key )
                .ToList();
            foreach ( var key in unauthorizedKeys )
            {
                prayerRequest.Attributes.Remove( key );
                prayerRequest.AttributeValues.Remove( key );
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "PrayerRequest", prayerRequest );

            var bag = new PrayerSessionRequestBag
            {
                IdKey = prayerRequest.IdKey,
                PersonHtml = GetAttributeValue( AttributeKey.PrayerPersonLava ).ResolveMergeFields( mergeFields ),
                PrayerHtml = GetAttributeValue( AttributeKey.PrayerDisplayLava ).ResolveMergeFields( mergeFields ),
                CampusName = campusName,
                CategoryName = prayerRequest.CategoryId.HasValue ? CategoryCache.Get( prayerRequest.CategoryId.Value )?.Name : null,
                IsUrgent = prayerRequest.IsUrgent ?? false,
                PrayerCountText = $"{prayerRequest.PrayerCount ?? 0} team prayers",
                AllowComments = prayerRequest.AllowComments ?? false
            };

            // Show the AI disclaimer when the request text may have been modified by
            // an AI automation and the disclaimer is enabled.
            if ( prayerRequest.OriginalRequest.IsNotNullOrWhiteSpace() && GetAttributeValue( AttributeKey.EnableAIDisclaimer ).AsBoolean() )
            {
                bag.AiDisclaimer = GetAttributeValue( AttributeKey.AIDisclaimer );
            }

            if ( bag.AllowComments )
            {
                PopulateComments( bag, prayerRequest, rockContext );
            }

            return bag;
        }

        /// <summary>
        /// Populates the comment thread for a prayer request, limited to the prayer
        /// comment note type and the notes the person is authorized to view.
        /// </summary>
        /// <param name="bag">The request bag to populate.</param>
        /// <param name="prayerRequest">The prayer request whose comments are loaded.</param>
        /// <param name="rockContext">The data context.</param>
        private void PopulateComments( PrayerSessionRequestBag bag, PrayerRequest prayerRequest, RockContext rockContext )
        {
            bag.Notes = new List<NoteBag>();
            bag.NoteTypes = new List<NoteTypeBag>();

            var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
            if ( noteType == null )
            {
                return;
            }

            var noteTypes = new List<NoteTypeCache> { noteType };
            var noteClientService = new NoteClientService( rockContext, RequestContext.CurrentPerson )
            {
                AllowedNoteTypes = noteTypes
            };

            var noteCollection = noteClientService.GetViewableNotes( prayerRequest );
            var notes = noteClientService.OrderNotes( noteCollection, false ).ToList();
            var watchedNoteIds = noteClientService.GetWatchedNoteIds( notes );

            notes.LoadAttributes( rockContext );

            bag.Notes = notes.Select( n => noteClientService.GetNoteBag( n, watchedNoteIds ) ).ToList();
            bag.NoteTypes = noteTypes.Select( nt => noteClientService.GetNoteTypeBag( nt ) ).ToList();
        }

        /// <summary>
        /// Determines whether a prayer request is still eligible to be displayed in
        /// a session given the current block settings and group scope.
        /// </summary>
        /// <param name="prayerRequest">The prayer request to validate.</param>
        /// <param name="rockContext">The data context.</param>
        /// <returns><c>true</c> if the request may be displayed; otherwise <c>false</c>.</returns>
        private bool IsRequestAvailable( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var isActiveApprovedUnexpired = prayerRequest.IsActive == true
                && prayerRequest.IsApproved == true
                && prayerRequest.ExpirationDate.HasValue
                && RockDateTime.Today <= prayerRequest.ExpirationDate.Value;

            if ( !isActiveApprovedUnexpired )
            {
                return false;
            }

            if ( GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean() && !( prayerRequest.IsPublic ?? false ) )
            {
                return false;
            }

            // Enforce the block's configured category scope so a crafted identifier
            // cannot reach a request outside the categories this block exposes.
            var scopedCategoryIds = GetScopedCategoryIds();
            if ( scopedCategoryIds != null
                && ( !prayerRequest.CategoryId.HasValue || !scopedCategoryIds.Contains( prayerRequest.CategoryId.Value ) ) )
            {
                return false;
            }

            var groupKey = PageParameter( PageParameterKey.GroupGuid );
            if ( groupKey.IsNullOrWhiteSpace() )
            {
                return !prayerRequest.GroupId.HasValue;
            }

            var group = new GroupService( rockContext ).Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds );
            return group != null && prayerRequest.GroupId == group.Id;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the refreshed welcome-step data so a new session can be started with
        /// an up-to-date category list and the person's latest saved selections.
        /// </summary>
        /// <returns>The welcome-step data.</returns>
        [BlockAction]
        public BlockActionResult GetWelcomeData()
        {
            return ActionOk( GetWelcomeBag() );
        }

        /// <summary>
        /// Starts a prayer session for the selected categories and campus, saving the
        /// person's selections and returning the ordered prayer requests to pray for.
        /// </summary>
        /// <param name="request">The session start request.</param>
        /// <returns>The ordered identifiers of the prayer requests in the session.</returns>
        [BlockAction]
        public BlockActionResult StartSession( PrayerSessionStartRequestBag request )
        {
            if ( request?.CategoryValues == null || !request.CategoryValues.Any() )
            {
                return ActionBadRequest( "Please select at least one prayer category." );
            }

            var rockContext = RockContext;

            // Expand the selected categories to include their direct child categories
            // so a request in a sub-category of a selection is still included.
            var categoryIds = new HashSet<int>();
            foreach ( var value in request.CategoryValues )
            {
                var categoryGuid = value.AsGuidOrNull();
                if ( !categoryGuid.HasValue )
                {
                    continue;
                }

                var category = CategoryCache.Get( categoryGuid.Value );
                if ( category == null )
                {
                    continue;
                }

                categoryIds.Add( category.Id );
                foreach ( var childCategory in category.Categories )
                {
                    categoryIds.Add( childCategory.Id );
                }
            }

            // Constrain the requested categories to the block's configured category
            // scope so a crafted request cannot start a session for categories
            // outside that scope.
            var scopedCategoryIds = GetScopedCategoryIds();
            if ( scopedCategoryIds != null )
            {
                categoryIds.IntersectWith( scopedCategoryIds );
            }

            // Save the selections before building the session so they persist even
            // when the current selection yields no requests.
            SaveSessionPreferences( rockContext, request.CategoryValues, request.CampusValue );

            if ( !categoryIds.Any() )
            {
                return ActionOk( new PrayerSessionStartResponseBag { PrayerRequestKeys = new List<string>() } );
            }

            var service = new PrayerRequestService( rockContext );
            IQueryable<PrayerRequest> qry = service.GetActiveApprovedUnexpired()
                .Where( p => p.CategoryId.HasValue && categoryIds.Contains( p.CategoryId.Value ) );

            var campusGuid = request.CampusValue.AsGuidOrNull();
            if ( campusGuid.HasValue )
            {
                var campusId = CampusCache.Get( campusGuid.Value )?.Id;
                if ( campusId.HasValue )
                {
                    qry = qry.Where( p => p.CampusId == campusId.Value );
                }
            }

            if ( GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean() )
            {
                qry = qry.Where( p => p.IsPublic == true );
            }

            qry = ApplyGroupScope( qry, rockContext );

            var orderedIds = qry
                .OrderByDescending( p => p.IsUrgent )
                .ThenBy( p => p.PrayerCount )
                .Select( p => p.Id )
                .ToList();

            var keys = orderedIds.Select( id => IdHasher.Instance.GetHash( id ) ).ToList();

            return ActionOk( new PrayerSessionStartResponseBag { PrayerRequestKeys = keys } );
        }

        /// <summary>
        /// Gets the display details for a single prayer request, records that a prayer
        /// was offered, and enqueues an interaction when configured to do so.
        /// </summary>
        /// <param name="idKey">The identifier of the prayer request to display.</param>
        /// <returns>The prayer request details.</returns>
        [BlockAction]
        public BlockActionResult GetPrayerRequest( string idKey )
        {
            var rockContext = RockContext;
            var service = new PrayerRequestService( rockContext );

            var prayerRequest = service.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( prayerRequest == null )
            {
                return ActionNotFound( "Prayer request not found." );
            }

            if ( !IsRequestAvailable( prayerRequest, rockContext ) )
            {
                return ActionBadRequest( "This prayer request is no longer available." );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            var bag = BuildRequestBag( prayerRequest, rockContext );

            if ( GetAttributeValue( AttributeKey.CreateInteractionsForPrayers ).AsBoolean() )
            {
                PrayerRequestService.EnqueuePrayerInteraction(
                    prayerRequest,
                    RequestContext.CurrentPerson,
                    PageCache?.Layout?.Site?.Name,
                    RequestContext.ClientInformation?.UserAgent,
                    RequestContext.ClientInformation?.IpAddress,
                    RequestContext.SessionGuid );
            }

            try
            {
                // Persist the incremented prayer count.
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return ActionOk( bag );
        }

        /// <summary>
        /// Flags a prayer request as inappropriate, automatically unapproving it once
        /// it reaches the configured flag limit.
        /// </summary>
        /// <param name="idKey">The identifier of the prayer request to flag.</param>
        /// <returns>An empty 200-OK response when the request was flagged.</returns>
        [BlockAction]
        public BlockActionResult FlagPrayerRequest( string idKey )
        {
            if ( !GetAttributeValue( AttributeKey.EnableCommunityFlagging ).AsBoolean() )
            {
                return ActionBadRequest( "Flagging is not enabled." );
            }

            var rockContext = RockContext;
            var prayerRequest = new PrayerRequestService( rockContext ).Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( prayerRequest == null )
            {
                return ActionNotFound( "Prayer request not found." );
            }

            if ( !IsRequestAvailable( prayerRequest, rockContext ) )
            {
                return ActionBadRequest( "This prayer request is no longer available." );
            }

            var flagLimit = GetAttributeValue( AttributeKey.FlagLimit ).AsIntegerOrNull();
            prayerRequest.FlagCount = ( prayerRequest.FlagCount ?? 0 ) + 1;
            if ( flagLimit.HasValue && prayerRequest.FlagCount >= flagLimit.Value )
            {
                prayerRequest.IsApproved = false;
            }

            rockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Begins the edit process of a comment by returning the editable details.
        /// </summary>
        /// <param name="request">The request describing which comment to edit.</param>
        /// <returns>The editable details of the comment.</returns>
        [BlockAction]
        public BlockActionResult EditNote( EditNoteRequestBag request )
        {
            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            var note = new NoteService( RockContext ).Get( request.IdKey, false );
            if ( note == null )
            {
                return ActionNotFound( "Note not found." );
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
        /// Saves a new or edited comment against the specified prayer request.
        /// </summary>
        /// <param name="request">The request describing the comment and its changes.</param>
        /// <param name="prayerRequestKey">The identifier of the prayer request being commented on.</param>
        /// <returns>The saved comment for display purposes.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( SaveNoteRequestBag request, string prayerRequestKey )
        {
            if ( request == null || !request.IsValidProperty( nameof( NoteEditBag.IdKey ) ) )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            if ( RequestContext.CurrentPerson == null )
            {
                return ActionBadRequest( "You must be signed in to comment." );
            }

            var prayerRequest = new PrayerRequestService( RockContext ).Get( prayerRequestKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( prayerRequest == null )
            {
                return ActionBadRequest( "Prayer request not found." );
            }

            if ( !IsRequestAvailable( prayerRequest, RockContext ) )
            {
                return ActionBadRequest( "This prayer request is no longer available." );
            }

            if ( !( prayerRequest.AllowComments ?? false ) )
            {
                return ActionBadRequest( "Comments are not allowed on this request." );
            }

            var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson )
            {
                AllowedNoteTypes = noteType != null ? new List<NoteTypeCache> { noteType } : new List<NoteTypeCache>()
            };

            var noteBag = noteClientService.SaveNote( request, prayerRequest, PageCache.Id, this.GetCurrentPageUrl(), RequestContext, out var errorMessage );
            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Deletes the requested comment.
        /// </summary>
        /// <param name="request">The request describing which comment to delete.</param>
        /// <returns>An empty 200-OK response when the comment was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( DeleteNoteRequestBag request )
        {
            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            var note = new NoteService( RockContext ).Get( request.IdKey, false );
            if ( note == null )
            {
                return ActionNotFound( "Note not found." );
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            if ( !noteClientService.DeleteNote( note, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        /// <summary>
        /// Sets the watched state of a specific comment.
        /// </summary>
        /// <param name="request">The request describing which comment and whether to watch it.</param>
        /// <returns>An empty 200-OK response when the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult WatchNote( WatchNoteRequestBag request )
        {
            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            var note = new NoteService( RockContext ).Get( request.IdKey, false );
            if ( note == null )
            {
                return ActionNotFound( "Note not found." );
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            if ( !noteClientService.WatchNote( note, request.IsWatching, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Describes an active prayer category and its current request count.
        /// </summary>
        private class CategoryCountInfo
        {
            /// <summary>
            /// Gets or sets the unique identifier of the category.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the name of the category.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the number of active requests in the category.
            /// </summary>
            public int Count { get; set; }
        }

        #endregion
    }
}
