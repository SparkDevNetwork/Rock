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
using Rock.Enums.Controls;
using Rock.Lava;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StepParticipantList;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of steps.
    /// </summary>
    [DisplayName( "Step Participant List" )]
    [Category( "Steps" )]
    [Description( "Lists all the participants in a Step." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "e7eb8f39-ae85-4f9c-8afb-18b3e3c6c570" )]
    // Was [Rock.SystemGuid.BlockTypeGuid( "272b2236-fccc-49b4-b914-20893f5e746d" )]
    [Rock.SystemGuid.BlockTypeGuid( "2E4A1578-145E-4052-9B56-1739F7366827" )]
    [CustomizedGrid]
    public class StepParticipantList : RockListBlockType<StepParticipantList.StepParticipantRow>
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
            public const string StepProgramId = "ProgramId";
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
            public const string FilterCreatedDateRange = "filter-created-date-range";
        }

        #endregion Keys

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        #endregion Fields

        #region Properties

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        private SlidingDateRangeBag FilterCreatedDateRange => PersonPreferences
            .GetValue( PreferenceKey.FilterCreatedDateRange )
            .ToSlidingDateRangeBagOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StepParticipantListOptionsBag>();
            var builder = GetGridBuilder();

            if ( FilterCreatedDateRange == null )
            {
                var defaultSlidingDateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Last,
                    TimeUnit = TimeUnitType.Month,
                    TimeValue = 6
                };

                this.PersonPreferences.SetValue( PreferenceKey.FilterCreatedDateRange, defaultSlidingDateRange.ToDelimitedSlidingDateRangeOrNull() );
                this.PersonPreferences.Save();
            }

            var canEdit = GetCanEdit();
            box.IsAddEnabled = canEdit;
            box.IsDeleteEnabled = canEdit;
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
                StepType = stepType?.ToListItemBag(),
                IsNoteColumnVisible = GetAttributeValue( AttributeKey.ShowNoteColumn ).AsBoolean(),
                IsDateStartedColumnVisible = stepType?.HasEndDate == true,
                PersonProfilePageUrl = this.GetLinkedPageUrl( AttributeKey.ProfilePage, new Dictionary<string, string> { { PageParameterKey.PersonId, "((Key))" } } ),
                StepStatusItems = stepType != null ? GetStepTypeStatus( stepType )?.ToListItemBagList() : null,
                StepStatusBackgroundColors = GetStepStatusBackgroundColors( stepType )
            };
            return options;
        }

        /// <summary>
        /// Gets the step status background colors.
        /// </summary>
        /// <param name="stepType">The Step Type.</param>
        /// <returns></returns>
        private Dictionary<string, string> GetStepStatusBackgroundColors( StepTypeCache stepType )
        {
            if ( stepType == null )
            {
                return new Dictionary<string, string>();
            }

            var stepStatusService = new StepStatusService( RockContext );
            return stepStatusService.Queryable()
                .AsNoTracking()
                .Where( ss => ss.StepProgram.StepTypes.Any( st => st.Id == stepType.Id ) )
                .ToDictionary( ss => ss.Name, ss => ss.StatusColorOrDefault );
        }

        /// <summary>
        /// Determines if the current person can edit the Steps
        /// <summary>
        /// <returns>A boolean value that indicates if the current person can edit.</returns>
        private bool GetCanEdit()
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
                    { PageParameterKey.StepTypeId, GetStepType()?.IdKey },
                    { PageParameterKey.StepProgramId, GetStepProgram()?.IdKey }
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StepParticipantRow> GetListQueryable( RockContext rockContext )
        {
            var stepType = GetStepType();
            if ( stepType == null )
            {
                return new List<StepParticipantRow>().AsQueryable();
            }

            var stepService = new StepService( rockContext );

            var queryable = stepService.Queryable()
                .AsNoTracking()
                .Where( x => x.StepTypeId == stepType.Id )
                .Select( x => new StepParticipantRow
                {
                    Step = x,
                    StepStatusName = x.StepStatus != null ? x.StepStatus.Name : string.Empty,
                    CampusName = x.Campus != null ? x.Campus.Name : string.Empty,
                    IsCompleted = x.StepStatus != null ? x.StepStatus.IsCompleteStatus : false,
                    Person = new PersonProjection
                    {
                        NickName = x.PersonAlias.Person.NickName,
                        LastName = x.PersonAlias.Person.LastName,
                        SuffixValueId = x.PersonAlias.Person.SuffixValueId,
                        PhotoId = x.PersonAlias.Person.PhotoId,
                        Age = x.PersonAlias.Person.Age,
                        Gender = x.PersonAlias.Person.Gender,
                        RecordTypeValueId = x.PersonAlias.Person.RecordTypeValueId,
                        AgeClassification = x.PersonAlias.Person.AgeClassification,
                        Id = x.PersonAlias.Person.Id,
                        RecordStatusValueId = x.PersonAlias.Person.RecordStatusValueId,
                        TopSignalColor = x.PersonAlias.Person.TopSignalColor,
                        TopSignalIconCssClass = x.PersonAlias.Person.TopSignalIconCssClass,
                        IsDeceased = x.PersonAlias.Person.IsDeceased
                    }
                } );

            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                queryable = queryable.Where( s => s.Step.CampusId == campusContext.Id );
            }

            queryable = FilterByDate( queryable );

            return queryable;
        }

        protected override IQueryable<StepParticipantRow> GetOrderedListQueryable( IQueryable<StepParticipantRow> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Person.LastName ).ThenBy( a => a.Person.NickName );
        }

        protected override List<StepParticipantRow> GetListItems( IQueryable<StepParticipantRow> queryable, RockContext rockContext )
        {
            var stepParticipantData = queryable.ToList();

            // Load attribute values for the grid-selected attributes.
            GridAttributeLoader.LoadFor( stepParticipantData, a => a.Step, GetGridAttributes(), rockContext );

            foreach ( var participant in stepParticipantData )
            {
                participant.Person.IdKey = IdHasher.Instance.GetHash( participant.Person.Id );
                participant.Person.Initials = $"{participant.Person.NickName.Truncate( 1, false )}{participant.Person.LastName.Truncate( 1, false )}";
                participant.Person.FullName = Rock.Model.Person.FormatFullName(
                    participant.Person.NickName,
                    participant.Person.LastName,
                    participant.Person.SuffixValueId,
                    participant.Person.RecordTypeValueId
                );
                participant.Person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                    participant.Person.Initials,
                    participant.Person.PhotoId,
                    participant.Person.Age,
                    participant.Person.Gender,
                    participant.Person.RecordTypeValueId,
                    participant.Person.AgeClassification
                );
            }

            return stepParticipantData;
        }

        /// <inheritdoc/>
        protected override GridBuilder<StepParticipantRow> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<StepParticipantRow>
            {
                LavaObject = row => row.Step
            };

            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

            return new GridBuilder<StepParticipantRow>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.Step.IdKey )
                .AddField( "person", a => a.Person )
                .AddTextField( "fullName", a => a.Person.FullName )
                .AddTextField( "personIdKey", a => a.Person.IdKey )
                .AddTextField( "stepStatus", a => a.StepStatusName )
                .AddDateTimeField( "dateStarted", a => a.Step.StartDateTime )
                .AddDateTimeField( "dateCompleted", a => a.Step.CompletedDateTime )
                .AddTextField( "campus", a => a.CampusName )
                .AddTextField( "note", a => a.Step.Note )
                .AddTextField( "signalMarkup", a => Rock.Model.Person.GetSignalMarkup( a.Person.TopSignalColor, a.Person.TopSignalIconCssClass ) )
                .AddField( "isDeceased", a => a.Person.IsDeceased )
                .AddField( "isActive", a => a.Person.RecordStatusValueId != inactiveStatus.Id )
                .AddField( "id", a => a.Step.Id )
                .AddField( "personId", a => a.Person.Id )
                .AddField( "stepStatusId", a => a.Step.StepStatusId )
                .AddField( "exportPerson", a => a.Person.FullName )
                .AddField( "isCompleted", a => a.IsCompleted.ToTrueFalse() )
                .AddAttributeFieldsFrom( a => a.Step, GetGridAttributes() );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private List<AttributeCache> GetGridAttributes()
        {
            if ( _gridAttributes == null )
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

                _gridAttributes = availableAttributes;
            }

            return _gridAttributes;
        }

        private List<AttributeCache> _gridAttributes = null;

        /// <summary>
        /// Filters the queryable by the Created Date
        /// </summary>
        /// <param name="queryable">The <see cref="StepParticipantRow"/> queryable</param>
        /// <returns></returns>
        private IQueryable<StepParticipantRow> FilterByDate( IQueryable<StepParticipantRow> queryable )
        {
            // Default to the last 180 days if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var dateRange = FilterCreatedDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            queryable = queryable
                .Where( c =>
                    (
                        c.Step.CreatedDateTime ??
                        c.Step.StartDateTime ??
                        c.Step.CompletedDateTime ??
                        c.Step.EndDateTime
                    ) >= dateTimeStart &&
                    (
                        c.Step.CreatedDateTime ??
                        c.Step.StartDateTime ??
                        c.Step.CompletedDateTime ??
                        c.Step.EndDateTime
                    ) <= dateTimeEnd );


            return queryable;
        }

        /// <summary>
        /// Gets the current step type.
        /// </summary>
        /// <returns></returns>
        private StepTypeCache GetStepType()
        {
            // if this block has a specific StepTypeId set, use that, otherwise, determine it from the PageParameters
            var stepTypeGuid = GetAttributeValue( AttributeKey.StepType ).AsGuidOrNull();

            if ( stepTypeGuid.HasValue )
            {
                return StepTypeCache.Get( stepTypeGuid.Value );
            }

            return StepTypeCache.Get( PageParameter( PageParameterKey.StepTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the current step program.
        /// </summary>
        /// <returns></returns>
        private StepProgramCache GetStepProgram()
        {
            return StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds );
        }

        private List<StepStatus> GetStepTypeStatus( StepTypeCache stepType )
        {
            if (stepType == null)
            {
                return new List<StepStatus>();
            }
            
            using (var rockContext = new RockContext())
            {
                var stepStatusService = new StepStatusService( rockContext );
                return stepStatusService.Queryable()
                    .AsNoTracking()
                    .Where( ss => ss.StepProgramId == stepType.StepProgramId )
                    .ToList();
            }
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

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !entity.IsAuthorized( Authorization.MANAGE_STEPS, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Step.FriendlyTypeName}." );
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

        #region Helper Classes

        public class StepParticipantRow
        {
            public Step Step { get; set; }

            public string StepStatusName { get; set; }

            public string CampusName { get; set; }

            public bool IsCompleted { get; set; }

            public PersonProjection Person { get; set; }
        }

        public class PersonProjection
        {
            public string Initials { get; set; }

            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? SuffixValueId { get; set; }

            public int? PhotoId { get; set; }

            public int? Age { get; set; }

            public Gender Gender { get; set; }

            public int? RecordTypeValueId { get; set; }

            public AgeClassification AgeClassification { get; set; }

            public string PhotoUrl { get; set; }

            public string IdKey { get; set; }

            public int Id { get; set; }

            public string FullName { get; set; }

            public int? RecordStatusValueId { get; set; }

            public string TopSignalColor { get; set; }

            public string TopSignalIconCssClass { get; set; }

            public bool IsDeceased { get; set; }
        }

        #endregion
    }
}