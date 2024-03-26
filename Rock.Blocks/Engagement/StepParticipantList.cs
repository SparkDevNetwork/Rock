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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StepParticipantList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of steps.
    /// </summary>
    [DisplayName( "Step Participant List" )]
    [Category( "Steps" )]
    [Description( "Lists all the participants in a Step." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]
    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each participant.",
        Key = AttributeKey.ProfilePage,
        IsRequired = false,
        Order = 2 )]
    [BooleanField(
        "Show Note Column",
        Key = AttributeKey.ShowNoteColumn,
        Description = "Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?",
        IsRequired = false,
        Order = 3 )]
    [Rock.SystemGuid.EntityTypeGuid( "e7eb8f39-ae85-4f9c-8afb-18b3e3c6c570" )]
    [Rock.SystemGuid.BlockTypeGuid( "272b2236-fccc-49b4-b914-20893f5e746d" )]
    [CustomizedGrid]
    public class StepParticipantList : RockEntityListBlockType<Step>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ProfilePage = "PersonProfilePage";
            public const string ShowNoteColumn = "ShowNoteColumn";
            public const string StepType = "StepType";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string StepTypeId = "StepTypeId";
            public const string StepId = "StepId";
            public const string PersonId = "PersonId";
        }

        private static class PreferenceKey
        {
            public const string FilterFirstName = "filter-first-name";
            public const string FilterLastName = "filter-last-name";
            public const string FilterStepStatus = "filter-step-status";
            public const string FilterDateStartedUpper = "filter-date-started-upper";
            public const string FilterDateStartedLower = "filter-date-started-lower";
            public const string FilterDateCompletedUpper = "filter-date-completed-upper";
            public const string FilterDateCompletedLower = "filter-date-completed-lower";
            public const string FilterNote = "filter-note";
            public const string FilterCampus = "filter-campus";
        }

        #endregion Keys

        #region Fields

        private StepType _stepType = null;
        private RockContext _dataContext;

        #endregion Fields

        #region Properties

        protected string FilterFirstName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterFirstName );

        protected string FilterLastName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterLastName );

        protected List<Guid> FilterStepStatus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterStepStatus )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        protected DateTime? FilterDateStartedUpper => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateStartedUpper )
            .AsDateTime();

        protected DateTime? FilterDateStartedLower => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateStartedLower )
            .AsDateTime();

        protected DateTime? FilterDateCompletedUpper => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateCompletedUpper )
            .AsDateTime();

        protected DateTime? FilterDateCompletedLower => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateCompletedLower )
            .AsDateTime();

        protected string FilterNote => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterNote );

        protected Guid? FilterCampus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StepParticipantListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private StepParticipantListOptionsBag GetBoxOptions()
        {
            var stepType = GetStepType();
            var options = new StepParticipantListOptionsBag()
            {
                IsCampusColumnVisible = CampusCache.All( false ).Count > 1,
                StepType = stepType.ToListItemBag(),
                IsNoteColumnVisible = GetAttributeValue( AttributeKey.ShowNoteColumn ).AsBoolean(),
                IsDateStartedColumnVisible = stepType?.HasEndDate == true,
                PersonProfilePageUrl = this.GetLinkedPageUrl( AttributeKey.ProfilePage, new Dictionary<string, string> { { PageParameterKey.PersonId, "((Key))" } } ),
                StepStatusItems = stepType?.StepProgram?.StepStatuses?.OrderBy( x => x.Order ).ToListItemBagList(),
                StepStatusBackgroundColors = GetStepStatusBackgroundColors( stepType )
            };
            return options;
        }

        /// <summary>
        /// Gets the step status background colors.
        /// </summary>
        /// <param name="stepType">The Step Type.</param>
        /// <returns></returns>
        private Dictionary<string, string> GetStepStatusBackgroundColors( StepType stepType )
        {
            if ( stepType == null )
            {
                return new Dictionary<string, string>();
            }

            var stepStatusService = new StepStatusService( GetDataContext() );
            return stepStatusService.Queryable()
                .AsNoTracking()
                .Where( ss => ss.StepProgram.StepTypes.Any( st => st.Id == stepType.Id ) )
                .ToDictionary( ss => ss.Name, ss => ss.StatusColorOrDefault );
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var stepType = GetStepType();
            var currentPerson = GetCurrentPerson();
            return stepType?.IsAuthorized( Authorization.EDIT, currentPerson ) == true || stepType?.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>()
                {
                    { PageParameterKey.StepId, "((Key))" },
                    { PageParameterKey.StepTypeId, GetStepType()?.IdKey }
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Step> GetListQueryable( RockContext rockContext )
        {
            var stepType = GetStepType();
            if ( stepType == null )
            {
                return new List<Step>().AsQueryable();
            }

            var stepService = new StepService( rockContext );

            var queryable = stepService.Queryable()
                .Include( x => x.StepStatus )
                .Include( x => x.PersonAlias.Person )
                .AsNoTracking()
                .Where( x => x.StepTypeId == stepType.Id );

            // Filter by First Name
            if ( !string.IsNullOrWhiteSpace( FilterFirstName ) )
            {
                queryable = queryable.Where( m =>
                    m.PersonAlias.Person.FirstName.StartsWith( FilterFirstName ) ||
                    m.PersonAlias.Person.NickName.StartsWith( FilterFirstName ) );
            }

            // Filter by Last Name
            if ( !string.IsNullOrWhiteSpace( FilterLastName ) )
            {
                queryable = queryable.Where( m => m.PersonAlias.Person.LastName.StartsWith( FilterLastName ) );
            }

            // Filter by Step Status
            var validStatusGuids = stepType.StepProgram.StepStatuses.Select( r => r.Guid ).ToList();
            var statusGuids = FilterStepStatus.Where( statusId => validStatusGuids.Contains( statusId ) ).ToList();

            if ( statusGuids.Any() )
            {
                queryable = queryable.Where( m => statusGuids.Contains( m.StepStatus.Guid ) );
            }

            // Filter By Start Date
            if ( FilterDateStartedLower.HasValue )
            {
                var startDate = FilterDateStartedLower.Value.Date;
                queryable = queryable.Where( m => m.StartDateTime >= startDate );
            }

            if ( FilterDateStartedUpper.HasValue )
            {
                var exclusiveEndDate = FilterDateStartedUpper.Value.Date.AddDays( 1 ).Date;
                queryable = queryable.Where( m => m.StartDateTime < exclusiveEndDate );
            }

            // Filter by Date Completed
            if ( FilterDateCompletedLower.HasValue )
            {
                var startDate = FilterDateCompletedLower.Value.Date;
                queryable = queryable.Where( m => m.CompletedDateTime >= startDate );
            }

            if ( FilterDateCompletedUpper.HasValue )
            {
                var exclusiveEndDate = FilterDateCompletedUpper.Value.Date.AddDays( 1 ).Date;
                queryable = queryable.Where( m => m.CompletedDateTime < exclusiveEndDate );
            }

            // Filter by Note
            if ( !string.IsNullOrWhiteSpace( FilterNote ) )
            {
                queryable = queryable.Where( m => m.Note.Contains( FilterNote ) );
            }

            var campusContext = GetCampusContextOrNull();
            var campusGuid = campusContext == null ? FilterCampus : campusContext.Guid;
            if ( campusGuid.HasValue )
            {
                queryable = queryable.Where( m => m.Campus.Guid == campusGuid );
            }

            return queryable;
        }

        protected override IQueryable<Step> GetOrderedListQueryable( IQueryable<Step> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Step> GetGridBuilder()
        {
            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
            return new GridBuilder<Step>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "person", a => a.PersonAlias.Person )
                .AddTextField( "fullName", a => a.PersonAlias.Person.FullName )
                .AddTextField( "personIdKey", a => a.PersonAlias.Person.Id.ToString() )
                .AddTextField( "stepStatus", a => a.StepStatus?.Name )
                .AddDateTimeField( "dateStarted", a => a.StartDateTime )
                .AddDateTimeField( "dateCompleted", a => a.CompletedDateTime )
                .AddTextField( "campus", a => a.Campus?.Name )
                .AddTextField( "note", a => a.Note )
                .AddTextField( "signalMarkup", a => a.PersonAlias.Person.GetSignalMarkup() )
                .AddField( "isDeceased", a => a.PersonAlias.Person.IsDeceased )
                .AddField( "isInactive", a => a.PersonAlias.Person.RecordStatusValueId == inactiveStatus.Id )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            // Parse the attribute filters
            var availableAttributes = new List<AttributeCache>();
            var stepType = GetStepType();

            if ( stepType != null )
            {
                int entityTypeId = new Step().TypeId;

                string entityTypeQualifier = stepType.Id.ToString();
                availableAttributes.AddRange( AttributeCache.GetOrderedGridAttributes( entityTypeId, "StepTypeId", entityTypeQualifier ).Where( attribute => attribute.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) ) );
            }

            return availableAttributes;
        }

        /// <summary>
        /// Gets the current step type.
        /// </summary>
        /// <returns></returns>
        private StepType GetStepType()
        {
            if ( _stepType == null )
            {
                // if this block has a specific StepTypeId set, use that, otherwise, determine it from the PageParameters
                var stepTypeGuid = GetAttributeValue( AttributeKey.StepType ).AsGuid();

                int stepTypeId = 0;

                if ( stepTypeGuid == Guid.Empty )
                {
                    stepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsInteger();
                }

                if ( !( stepTypeId == 0 && stepTypeGuid == Guid.Empty ) )
                {
                    var dataContext = GetDataContext();
                    _stepType = new StepTypeService( dataContext ).Queryable()
                                        .Where( g => g.Id == stepTypeId || g.Guid == stepTypeGuid )
                                        .FirstOrDefault();
                }
            }

            return _stepType;
        }

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            return _dataContext ?? ( _dataContext = new RockContext() );
        }

        /// <summary>
        /// Gets the campus context, returns null if there is only no more than one active campus.
        /// This is to prevent to filtering out of Steps that are associated with currently inactive
        /// campuses or no campus at all.
        /// </summary>
        /// <returns></returns>
        private Campus GetCampusContextOrNull()
        {
            return ( CampusCache.All( false ).Count > 1 ) ? RequestContext.GetContextEntity<Campus>() : null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StepService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Step.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${Step.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion Block Actions
    }
}