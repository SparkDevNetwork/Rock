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

using Rock.Attribute;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.PersonProgramStepList;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays step records for a person in a step program.
    /// </summary>

    [DisplayName( "Personal Step List" )]
    [Category( "Steps" )]
    [Description( "Displays step records for a person in a step program." )]
    [IconCssClass( "ti ti-list-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Person ) )]

    #region Block Attributes

    [StepProgramField(
        "Step Program",
        Description = "The Step Program to display. This value can also be a page parameter: StepProgramId. Leave this attribute blank to use the page parameter.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.StepProgram )]

    [LinkedPage(
        "Step Entry Page",
        Description = "The page where step records can be edited or added",
        Order = 2,
        Key = AttributeKey.StepPage )]

    [IntegerField(
        "Steps Per Row",
        Description = "The number of step cards that should be shown on a row",
        Order = 3,
        IsRequired = true,
        Key = AttributeKey.StepsPerRow,
        DefaultIntegerValue = AttributeDefault.StepsPerRow )]

    [IntegerField(
        "Steps Per Row Mobile",
        Description = "The number of step cards that should be shown on a row on a mobile screen size",
        Order = 4,
        IsRequired = true,
        Key = AttributeKey.StepsPerRowMobile,
        DefaultIntegerValue = AttributeDefault.StepsPerRowMobile )]

    [BooleanField(
        "Show Campus Column",
        Description = "Should the campus should be shown on the grid and card display?",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.ShowCampusColumn )]

    [BooleanField(
        "Show Start Date Column",
        Description = "Should the step start date be shown on the grid and card display?",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.ShowStartedDateColumn )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "2A6E839D-B37D-43F1-9159-A386F7BF932E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4" )]
    [Rock.SystemGuid.BlockTypeGuid( "1774616D-2577-4A73-B330-D3EFCD6D8BCF" )]
    public class PersonProgramStepList : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string StepProgram = "StepProgram";
            public const string StepPage = "StepPage";
            public const string StepsPerRow = "StepsPerRow";
            public const string StepsPerRowMobile = "StepsPerRowMobile";
            public const string ShowCampusColumn = "ShowCampusColumn";
            public const string ShowStartedDateColumn = "ShowStartedDateColumn";
        }

        /// <summary>
        /// Default values for block attributes.
        /// </summary>
        private static class AttributeDefault
        {
            public const int StepsPerRow = 5;
            public const int StepsPerRowMobile = 1;
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The original step program page parameter used by this block.
            /// </summary>
            public const string StepProgramId = "StepProgramId";

            /// <summary>
            /// The step program page parameter used by the other Steps blocks.
            /// </summary>
            public const string ProgramId = "ProgramId";

            /// <summary>
            /// The legacy, integer-based person page parameter.
            /// </summary>
            public const string PersonId = "PersonId";

            /// <summary>
            /// The person page parameter which can be an Id, Guid, or IdKey.
            /// </summary>
            public const string Person = "Person";

            public const string StepTypeId = "StepTypeId";
            public const string StepId = "StepId";
        }

        /// <summary>
        /// Keys for block person preferences.
        /// </summary>
        private static class PersonPreferenceKey
        {
            public const string IsCardView = "is-card-view";
        }

        #endregion Keys

        #region Fields

        private StepProgramCache _stepProgram;
        private Person _person;
        private List<StepType> _stepTypes;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PersonProgramStepListBag, PersonProgramStepListOptionsBag>();

            var program = GetStepProgram();
            var person = GetPerson();

            box.Options = GetBoxOptions( program, person );
            box.Bag = GetBag( program, person );

            return box;
        }

        /// <summary>
        /// Builds the per-load bag with the cards, grid rows, and view mode.
        /// Populates only the error message when a required model is missing.
        /// </summary>
        /// <param name="program">The resolved step program, or null.</param>
        /// <param name="person">The resolved person, or null.</param>
        /// <returns>The populated bag.</returns>
        private PersonProgramStepListBag GetBag( StepProgramCache program, Person person )
        {
            var bag = new PersonProgramStepListBag
            {
                StepTypes = new List<PersonProgramStepTypeBag>()
            };

            if ( program == null )
            {
                bag.ErrorMessage = $"The step program was not found. Please set the block attribute or ensure a query parameter `{PageParameterKey.StepProgramId}` or `{PageParameterKey.ProgramId}` is set.";
                return bag;
            }

            if ( person == null )
            {
                bag.ErrorMessage = "The person was not found";
                return bag;
            }

            bag.IsCardView = GetIsCardView( program );

            var stepTypes = GetStepTypes( program );
            var personSteps = GetPersonSteps( person, stepTypes );

            bag.GridData = GetGridData( stepTypes, personSteps );
            bag.StepStatusColors = GetStepStatusColors( personSteps );
            bag.StepTypes = stepTypes
                .Select( stepType =>
                {
                    var steps = personSteps[stepType.Id];
                    var prerequisites = GetPrerequisiteStepTypes( stepType );
                    var hasMetPrerequisites = HasMetPrerequisites( prerequisites, personSteps );
                    var isComplete = steps.Any( s => s.IsComplete );
                    var isAddEnabled = CanAddStep( stepType, steps, hasMetPrerequisites );

                    return new PersonProgramStepTypeBag
                    {
                        Id = stepType.Id,
                        IdKey = stepType.IdKey,
                        Name = stepType.Name,
                        IconCssClass = stepType.IconCssClass,
                        HighlightColor = stepType.HighlightColor,
                        HasSteps = steps.Any(),
                        IsComplete = isComplete,
                        HasMetPrerequisites = hasMetPrerequisites,
                        IsAddEnabled = isAddEnabled,
                        Prerequisites = prerequisites
                            .Select( p => new PersonProgramPrerequisiteBag
                            {
                                Name = p.Name,
                                IsComplete = IsPrerequisiteComplete( p, personSteps )
                            } )
                            .ToList(),
                        Steps = steps.Select( GetStepBag ).ToList()
                    };
                } )
                .ToList();

            return bag;
        }

        /// <summary>
        /// Determines whether campus should be shown. Requires the block
        /// setting and more than one active campus.
        /// </summary>
        /// <returns>True when campus should be shown.</returns>
        private bool GetIsCampusVisible()
        {
            return GetAttributeValue( AttributeKey.ShowCampusColumn ).AsBoolean() && CampusCache.All( false ).Count > 1;
        }

        /// <summary>
        /// Gets the active step types that must be completed before a step of
        /// the given type can be added.
        /// </summary>
        /// <param name="stepType">The step type.</param>
        /// <returns>The prerequisite step types.</returns>
        private List<StepType> GetPrerequisiteStepTypes( StepType stepType )
        {
            return stepType.StepTypePrerequisites
                .Select( p => p.PrerequisiteStepType )
                .Where( p => p != null && p.IsActive )
                .ToList();
        }

        /// <summary>
        /// Determines whether the person has a completed step for every
        /// prerequisite step type.
        /// </summary>
        /// <param name="prerequisites">The prerequisite step types.</param>
        /// <param name="personSteps">The person's steps keyed by step type identifier.</param>
        /// <returns>True when there are no prerequisites or all are complete.</returns>
        private bool HasMetPrerequisites( List<StepType> prerequisites, Dictionary<int, List<Step>> personSteps )
        {
            return prerequisites.All( prerequisite => IsPrerequisiteComplete( prerequisite, personSteps ) );
        }

        /// <summary>
        /// Determines whether the person has a completed step of the prerequisite type.
        /// </summary>
        /// <param name="prerequisite">The prerequisite step type.</param>
        /// <param name="personSteps">The person's steps keyed by step type identifier.</param>
        /// <returns>True when a completed step of the type exists.</returns>
        private bool IsPrerequisiteComplete( StepType prerequisite, Dictionary<int, List<Step>> personSteps )
        {
            return personSteps.TryGetValue( prerequisite.Id, out var steps ) && steps.Any( s => s.IsComplete );
        }

        /// <summary>
        /// Determines whether a step of the given type can be added for the
        /// person. Requires manual editing, EDIT or MANAGE_STEPS on the step
        /// type, met prerequisites, and either allow multiple or no existing step.
        /// </summary>
        /// <param name="stepType">The step type.</param>
        /// <param name="steps">The person's existing steps of this type.</param>
        /// <param name="hasMetPrerequisites">Whether the prerequisites are met.</param>
        /// <returns>True when a step can be added.</returns>
        private bool CanAddStep( StepType stepType, List<Step> steps, bool hasMetPrerequisites )
        {
            if ( !stepType.AllowManualEditing || !stepType.IsActive || !hasMetPrerequisites )
            {
                return false;
            }

            var currentPerson = RequestContext.CurrentPerson;
            var canEdit = stepType.IsAuthorized( Authorization.EDIT, currentPerson ) || stepType.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson );

            if ( !canEdit )
            {
                return false;
            }

            return stepType.AllowMultiple || !steps.Any();
        }

        /// <summary>
        /// Gets the person's steps for the given step types, keyed by step type
        /// identifier. Every step type has an entry even when the person has no
        /// steps of that type. Steps are ordered oldest to newest so the last
        /// entry is the latest step.
        /// </summary>
        /// <param name="person">The person whose steps are loaded.</param>
        /// <param name="stepTypes">The step types to load steps for.</param>
        /// <returns>The steps grouped by step type identifier.</returns>
        private Dictionary<int, List<Step>> GetPersonSteps( Person person, List<StepType> stepTypes )
        {
            var stepTypesById = stepTypes.ToDictionary( st => st.Id );
            var stepTypeIds = stepTypesById.Keys.ToList();

            var steps = new StepService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( s => s.StepStatus )
                .Include( s => s.Campus )
                .Where( s => s.PersonAlias.PersonId == person.Id && stepTypeIds.Contains( s.StepTypeId ) )
                .ToList();

            // Step authorization walks StepType then StepProgram, so attach the
            // already-loaded types to avoid two lazy loads per step.
            foreach ( var step in steps )
            {
                step.StepType = stepTypesById[step.StepTypeId];
            }

            return stepTypeIds.ToDictionary(
                stepTypeId => stepTypeId,
                stepTypeId => steps
                    .Where( s => s.StepTypeId == stepTypeId )
                    .OrderBy( s => s.CompletedDateTime ?? s.EndDateTime ?? s.StartDateTime ?? s.CreatedDateTime ?? System.DateTime.MinValue )
                    .ToList() );
        }

        /// <summary>
        /// Builds the bag for one existing step shown in a card's hover table.
        /// </summary>
        /// <param name="step">The step.</param>
        /// <returns>The populated step bag.</returns>
        private PersonProgramStepBag GetStepBag( Step step )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var canEdit = step.IsAuthorized( Authorization.EDIT, currentPerson ) || step.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson );

            return new PersonProgramStepBag
            {
                Id = step.Id,
                IdKey = step.IdKey,
                StatusName = step.StepStatus?.Name,
                StartDateTime = step.StartDateTime?.ToString( "s" ),
                CompletedDateTime = step.CompletedDateTime?.ToString( "s" ),
                CanEdit = canEdit,
                CanDelete = canEdit
            };
        }

        /// <summary>
        /// Gets the active step types of the program ordered for display. The
        /// model is loaded with its program so it can be attached to each step
        /// and satisfy the step's parent authority chain without lazy loads.
        /// </summary>
        /// <param name="program">The step program.</param>
        /// <returns>The ordered list of active step types.</returns>
        private List<StepType> GetStepTypes( StepProgramCache program )
        {
            if ( _stepTypes != null )
            {
                return _stepTypes;
            }

            _stepTypes = new StepTypeService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( st => st.StepProgram )
                .Include( st => st.StepTypePrerequisites.Select( p => p.PrerequisiteStepType ) )
                .Where( st => st.StepProgramId == program.Id && st.IsActive )
                .OrderBy( st => st.Order )
                .ThenBy( st => st.Name )
                .ToList();

            return _stepTypes;
        }

        /// <summary>
        /// Builds the configuration options that do not change between loads.
        /// </summary>
        /// <param name="program">The resolved step program, or null.</param>
        /// <param name="person">The resolved person, or null.</param>
        /// <returns>The populated options bag.</returns>
        private PersonProgramStepListOptionsBag GetBoxOptions( StepProgramCache program, Person person )
        {
            var options = new PersonProgramStepListOptionsBag
            {
                ProgramName = program?.Name,
                StepTerm = program?.StepTerm ?? "Step",
                StepsPerRow = GetAttributeValue( AttributeKey.StepsPerRow ).AsIntegerOrNull() ?? AttributeDefault.StepsPerRow,
                StepsPerRowMobile = GetAttributeValue( AttributeKey.StepsPerRowMobile ).AsIntegerOrNull() ?? AttributeDefault.StepsPerRowMobile,
                IsCampusColumnVisible = GetIsCampusVisible(),
                IsStartDateColumnVisible = GetAttributeValue( AttributeKey.ShowStartedDateColumn ).AsBoolean(),
                IsBlockEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ),
                GridDefinition = GetGridBuilder().BuildDefinition(),
                StepEntryUrlTemplate = GetStepEntryUrlTemplate( person )
            };

            return options;
        }

        /// <summary>
        /// Gets the step program to display. Order: block attribute, then the
        /// StepProgramId page parameter, then the ProgramId page parameter,
        /// then the first active program.
        /// </summary>
        /// <returns>The step program, or null when none is active.</returns>
        private StepProgramCache GetStepProgram()
        {
            if ( _stepProgram != null )
            {
                return _stepProgram;
            }

            var programGuid = GetAttributeValue( AttributeKey.StepProgram ).AsGuidOrNull();
            var allowIntegerIdentifier = !PageCache.Layout.Site.DisablePredictableIds;

            if ( programGuid.HasValue )
            {
                _stepProgram = StepProgramCache.Get( programGuid.Value );
            }
            else if ( PageParameter( PageParameterKey.StepProgramId ).IsNotNullOrWhiteSpace() )
            {
                _stepProgram = StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), allowIntegerIdentifier );
            }
            else if ( PageParameter( PageParameterKey.ProgramId ).IsNotNullOrWhiteSpace() )
            {
                _stepProgram = StepProgramCache.Get( PageParameter( PageParameterKey.ProgramId ), allowIntegerIdentifier );
            }
            else
            {
                _stepProgram = StepProgramCache.All()
                    .Where( p => p.IsActive )
                    .OrderBy( p => p.Id )
                    .FirstOrDefault();
            }

            if ( _stepProgram != null && !_stepProgram.IsActive )
            {
                _stepProgram = null;
            }

            return _stepProgram;
        }

        /// <summary>
        /// Gets the person whose steps are displayed. Order: Person context
        /// entity, then the Person page parameter, then the PersonId page
        /// parameter, then the current person.
        /// </summary>
        /// <returns>The person, or null.</returns>
        private Person GetPerson()
        {
            if ( _person != null )
            {
                return _person;
            }

            _person = RequestContext.GetContextEntity<Person>();

            if ( _person != null )
            {
                return _person;
            }

            var personKey = PageParameter( PageParameterKey.Person );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                personKey = PageParameter( PageParameterKey.PersonId );
            }

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                _person = new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                _person = RequestContext.CurrentPerson;
            }

            return _person;
        }

        /// <summary>
        /// Gets whether the card view should be shown, from the person
        /// preference when set, otherwise from the program's default view.
        /// </summary>
        /// <param name="program">The step program.</param>
        /// <returns>True for card view, false for grid view.</returns>
        private bool GetIsCardView( StepProgramCache program )
        {
            var preference = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.IsCardView ).AsBooleanOrNull();

            return preference ?? program.DefaultListView == StepProgram.ViewMode.Cards;
        }

        /// <summary>
        /// Builds the Step Entry page URL with placeholders for the step type
        /// and step identifiers. Retains the Person key when one was passed in.
        /// </summary>
        /// <param name="person">The person, or null.</param>
        /// <returns>The URL template, or null when no page is configured or no person was resolved.</returns>
        private string GetStepEntryUrlTemplate( Person person )
        {
            if ( person == null || GetAttributeValue( AttributeKey.StepPage ).IsNullOrWhiteSpace() )
            {
                // TODO: Match WebForms for now (silent no-op when no Step Entry
                // page is configured). Revisit at the end of the conversion.
                return null;
            }

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.PersonId, person.Id.ToString() },
                { PageParameterKey.StepTypeId, "((StepTypeId))" },
                { PageParameterKey.StepId, "((StepId))" }
            };

            var personKey = PageParameter( PageParameterKey.Person );

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                queryParams.Add( PageParameterKey.Person, personKey );
            }

            return this.GetLinkedPageUrl( AttributeKey.StepPage, queryParams );
        }

        /// <summary>
        /// Builds the grid rows for every step the person has in the program,
        /// ordered by step type order then name to match the card order.
        /// </summary>
        /// <param name="stepTypes">The program's active step types.</param>
        /// <param name="personSteps">The person's steps keyed by step type identifier.</param>
        /// <returns>The grid data.</returns>
        private GridDataBag GetGridData( List<StepType> stepTypes, Dictionary<int, List<Step>> personSteps )
        {
            var steps = stepTypes.SelectMany( stepType => personSteps[stepType.Id] ).ToList();

            // Load once for every step so the summary column does not query per row.
            steps.LoadAttributes( RockContext );

            var currentPerson = RequestContext.CurrentPerson;

            var rows = steps
                .Select( step => new StepRow
                {
                    Step = step,
                    StepTypeName = step.StepType.Name,
                    StepTypeIconCssClass = step.StepType.IconCssClass,
                    StepTypeOrder = step.StepType.Order,
                    CampusName = step.Campus?.Name ?? string.Empty,
                    StatusName = step.StepStatus?.Name ?? string.Empty,
                    SummaryHtml = GetSummaryHtml( step ),
                    CanDelete = step.IsAuthorized( Authorization.EDIT, currentPerson ) || step.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson )
                } )
                .OrderBy( r => r.StepTypeOrder )
                .ThenBy( r => r.StepTypeName )
                .ToList();

            return GetGridBuilder().Build( rows );
        }

        /// <summary>
        /// Gets the colors of the statuses in use by the person's steps, keyed
        /// by status name. Built from the already-loaded steps so no query is needed.
        /// </summary>
        /// <param name="personSteps">The person's steps keyed by step type identifier.</param>
        /// <returns>The status name to color dictionary.</returns>
        private Dictionary<string, string> GetStepStatusColors( Dictionary<int, List<Step>> personSteps )
        {
            return personSteps.Values
                .SelectMany( steps => steps )
                .Where( s => s.StepStatus != null )
                .GroupBy( s => s.StepStatus.Name )
                .ToDictionary( g => g.Key, g => g.First().StepStatus.StatusColorOrDefault );
        }

        /// <summary>
        /// Builds the summary column HTML from the step's attributes that are
        /// marked to show in grids, one "Name: Value" line per attribute.
        /// Boolean values are shown in full; everything else is condensed.
        /// </summary>
        /// <param name="step">The step with attributes loaded.</param>
        /// <returns>The summary HTML, or an empty string.</returns>
        private string GetSummaryHtml( Step step )
        {
            if ( step.Attributes == null )
            {
                return string.Empty;
            }

            var booleanFieldTypeGuid = Rock.SystemGuid.FieldType.BOOLEAN.AsGuid();
            var lines = new List<string>();

            foreach ( var attribute in step.Attributes.Values.Where( a => a.IsGridColumn ) )
            {
                var rawValue = step.GetAttributeValue( attribute.Key );
                var field = attribute.FieldType.Field;

                var formattedValue = attribute.FieldType.Guid == booleanFieldTypeGuid
                    ? field.GetHtmlValue( rawValue, attribute.ConfigurationValues )
                    : field.GetCondensedHtmlValue( rawValue, attribute.ConfigurationValues );

                lines.Add( $"{attribute.Name}: {formattedValue}" );
            }

            return string.Join( "<br />", lines );
        }

        /// <summary>
        /// Gets the grid builder that defines the columns for the grid view.
        /// </summary>
        /// <returns>The grid builder.</returns>
        private GridBuilder<StepRow> GetGridBuilder()
        {
            return new GridBuilder<StepRow>()
                .AddTextField( "idKey", r => r.Step.IdKey )
                .AddField( "id", r => r.Step.Id )
                .AddField( "stepTypeId", r => r.Step.StepTypeId )
                .AddTextField( "stepType", r => r.StepTypeName )
                .AddTextField( "stepTypeIconCssClass", r => r.StepTypeIconCssClass )
                .AddTextField( "campus", r => r.CampusName )
                .AddDateTimeField( "startDateTime", r => r.Step.StartDateTime )
                .AddDateTimeField( "completedDateTime", r => r.Step.CompletedDateTime )
                .AddTextField( "summary", r => r.SummaryHtml )
                .AddTextField( "status", r => r.StatusName )
                .AddField( "canDelete", r => r.CanDelete )
                .AddField( "stepTypeOrder", r => r.StepTypeOrder );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified step and returns the refreshed block data.
        /// </summary>
        /// <param name="key">The identifier key of the step to delete.</param>
        /// <returns>The refreshed bag, or an error result.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var stepService = new StepService( RockContext );
            var step = stepService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( step == null )
            {
                return ActionBadRequest( $"{Step.FriendlyTypeName} not found." );
            }

            var currentPerson = RequestContext.CurrentPerson;

            if ( !step.IsAuthorized( Authorization.EDIT, currentPerson ) && !step.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Step.FriendlyTypeName}." );
            }

            if ( !stepService.CanDelete( step, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            stepService.Delete( step );
            RockContext.SaveChanges();

            return ActionOk( GetBag( GetStepProgram(), GetPerson() ) );
        }

        #endregion Block Actions

        #region Helper Classes

        /// <summary>
        /// Projection of a step used to build the grid rows.
        /// </summary>
        private class StepRow
        {
            public Step Step { get; set; }

            public string StepTypeName { get; set; }

            public string StepTypeIconCssClass { get; set; }

            public int StepTypeOrder { get; set; }

            public string CampusName { get; set; }

            public string StatusName { get; set; }

            public string SummaryHtml { get; set; }

            public bool CanDelete { get; set; }
        }

        #endregion Helper Classes
    }
}
