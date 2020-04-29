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
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Personal Step List" )]
    [Category( "Steps" )]
    [Description( "Displays step records for a person in a step program." )]
    [ContextAware( typeof( Person ) )]

    #region Attributes

    [StepProgramField(
        name: "Step Program",
        description: "The Step Program to display. This value can also be a page parameter: StepProgramId. Leave this attribute blank to use the page parameter.",
        required: false,
        order: 1,
        key: AttributeKey.StepProgram )]

    [LinkedPage(
        name: "Step Entry Page",
        description: "The page where step records can be edited or added",
        order: 2,
        key: AttributeKey.StepPage )]

    [IntegerField(
        name: "Steps Per Row",
        description: "The number of step cards that should be shown on a row",
        order: 3,
        required: true,
        key: AttributeKey.StepsPerRow,
        defaultValue: AttributeDefault.StepsPerRow )]

    [IntegerField(
        name: "Steps Per Row Mobile",
        description: "The number of step cards that should be shown on a row on a mobile screen size",
        order: 4,
        required: true,
        key: AttributeKey.StepsPerRowMobile,
        defaultValue: AttributeDefault.StepsPerRowMobile )]

    #endregion Attributes

    public partial class PersonProgramStepList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Attribute keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The step program attribute key
            /// </summary>
            public const string StepProgram = "StepProgram";

            /// <summary>
            /// The step page attribute key
            /// </summary>
            public const string StepPage = "StepPage";

            /// <summary>
            /// The steps per row attribute key
            /// </summary>
            public const string StepsPerRow = "StepsPerRow";

            /// <summary>
            /// The steps per row on mobile attribute key
            /// </summary>
            public const string StepsPerRowMobile = "StepsPerRowMobile";
        }

        /// <summary>
        /// Attribute Default Values
        /// </summary>
        private static class AttributeDefault
        {
            /// <summary>
            /// The steps per row attribute default value
            /// </summary>
            public const int StepsPerRow = 5;

            /// <summary>
            /// The steps per row on mobile attribute default value
            /// </summary>
            public const int StepsPerRowMobile = 1;
        }

        /// <summary>
        /// Filter keys
        /// </summary>
        private static class FilterKey
        {
            /// <summary>
            /// The step type name filter key
            /// </summary>
            public const string StepTypeName = "StepTypeName";

            /// <summary>
            /// The step status name filter key
            /// </summary>
            public const string StepStatusName = "StepStatusName";
        }

        /// <summary>
        /// Query string or other page parameter keys
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The step program id page parameter
            /// </summary>
            public const string StepProgramId = "StepProgramId";
        }

        /// <summary>
        /// User preference keys
        /// </summary>
        private static class PreferenceKey
        {
            /// <summary>
            /// The is card view user preference key
            /// </summary>
            public const string IsCardView = "PersonProgramStepList.IsCardView";
        }

        #endregion Keys

        #region Events

        /// <summary>
        /// Handles the OnInit event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ClearError();
            BlockUpdated += PersonProgramStepList_BlockUpdated;

            gfGridFilter.ApplyFilterClick += gfGridFilter_ApplyFilterClick;
            gfGridFilter.ClearFilterClick += gfGridFilter_ClearFilterClick;

            gStepList.DataKeyNames = new[] { "id" };
            gStepList.GridRebind += gStepList_GridRebind;

            if ( !IsPostBack )
            {
                SetProgramDetailsOnBlock();

                var isCardViewPref = GetUserPreference( PreferenceKey.IsCardView ).AsBooleanOrNull();

                if ( isCardViewPref.HasValue )
                {
                    hfIsCardView.Value = isCardViewPref.ToString();
                }
                else
                {
                    // use the program's default view setting
                    var program = GetStepProgram();
                    hfIsCardView.Value = ( program.DefaultListView == StepProgram.ViewMode.Cards ).ToString();
                }
            }

            RenderStepsPerRow();
            DisplayStepTerm();
            RenderViewMode();
        }

        /// <summary>
        /// Handle the event where the block was updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PersonProgramStepList_BlockUpdated( object sender, EventArgs e )
        {
            ClearError();
            SetProgramDetailsOnBlock();
            RenderViewMode();
        }

        /// <summary>
        /// Handle the rebind event for the step list grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gStepList_GridRebind( object sender, GridRebindEventArgs e )
        {
            RenderGridView();
        }

        /// <summary>
        /// Show the grid view and save the user's preference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ShowGrid( object sender, EventArgs e )
        {
            hfIsCardView.Value = false.ToString();
            RenderViewMode();
            SetUserPreference( PreferenceKey.IsCardView, hfIsCardView.Value, true );
        }

        /// <summary>
        /// Show the card view and save the user's preference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ShowCards( object sender, EventArgs e )
        {
            hfIsCardView.Value = true.ToString();
            RenderViewMode();
            SetUserPreference( PreferenceKey.IsCardView, hfIsCardView.Value, true );
        }

        /// <summary>
        /// Display either the card or the grid view
        /// </summary>
        private void RenderViewMode()
        {
            var isCardView = hfIsCardView.Value.AsBoolean();

            if ( isCardView )
            {
                RenderCardView();
            }
            else
            {
                RenderGridView();
            }

            pnlCardView.Visible = isCardView;
            pnlGridView.Visible = !isCardView;
        }

        /// <summary>
        /// Generate the contents of the step type column of the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lStepType_DataBound( object sender, RowEventArgs e )
        {
            var lStepType = sender as Literal;
            var stepGridRow = e.Row.DataItem as StepGridRowViewModel;

            lStepType.Text = string.Format( "<i class=\"{0}\"></i> {1}", stepGridRow.StepTypeIconCssClass, stepGridRow.StepTypeName );
        }

        /// <summary>
        /// Generate the contents of the step status column of the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lStepStatus_DataBound( object sender, RowEventArgs e )
        {
            var lStepStatus = sender as Literal;
            var stepGridRow = e.Row.DataItem as StepGridRowViewModel;
            var classAttribute = string.Empty;

            if ( !stepGridRow.StepStatusColor.IsNullOrWhiteSpace() )
            {
                classAttribute = string.Format( @" class=""label label-default"" style=""background-color: {0};"" ", stepGridRow.StepStatusColor );
            }

            lStepStatus.Text = string.Format( "<span{0}>{1}</span>",
                classAttribute,
                stepGridRow.StepStatusName.IsNullOrWhiteSpace() ? "-" : stepGridRow.StepStatusName );
        }

        protected void lSummary_DataBound( object sender, RowEventArgs e )
        {
            var lStepStatus = sender as Literal;
            if(lStepStatus == null )
            {
                return;
            }

            var stepGridRowViewModel = e.Row.DataItem as StepGridRowViewModel;
            if(stepGridRowViewModel == null )
            {
                return;
            }

            var step = stepGridRowViewModel.Step;
            if(step == null )
            {
                return;
            }

            var classAttribute = string.Empty;
            var itemSummary = new StringBuilder();

            if ( step.Attributes == null )
            {
                step.LoadAttributes();
            }

            var formatString = "{0}: {1}<br />";
            var attributesToDisplay = step.Attributes.Where( a => a.Value.IsGridColumn );
            foreach ( var attributeCache in attributesToDisplay )
            {
                var attribute = step.Attributes[attributeCache.Key];
                var rawValue = step.GetAttributeValue( attributeCache.Key );
                
                var showCondensed = !( attribute.FieldType.Field is BooleanFieldType );

                var formattedValue = attribute.FieldType.Field.FormatValue( null, attribute.EntityTypeId, step.Id, rawValue, attribute.QualifierValues, showCondensed );

                itemSummary.AppendLine( string.Format( formatString, attribute.Name, formattedValue ) );
            }
        
            lStepStatus.Text = itemSummary.ToString();
        }

        /// <summary>
        /// The click event of the add step buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddStep( object sender, CommandEventArgs e )
        {
            var stepTypeId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();
            AddStep( stepTypeId );
        }

        /// <summary>
        /// The click event of the add step buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bAddStep_ServerClick( object sender, EventArgs e )
        {
            var stepTypeId = ( ( HtmlButton ) sender ).Attributes["data-step-type-id"].AsIntegerOrNull();
            AddStep( stepTypeId );
        }

        /// <summary>
        /// Add a step with the given step type
        /// </summary>
        /// <param name="stepTypeId"></param>
        private void AddStep( int? stepTypeId )
        {
            if ( stepTypeId.HasValue )
            {
                var stepType = GetStepTypes().FirstOrDefault( st => st.Id == stepTypeId );

                if ( stepType != null && CanAddStep( stepType ) )
                {
                    GoToStepPage( stepTypeId.Value );
                }
            }
        }

        /// <summary>
        /// Event when the user clicks to edit a step from the card view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rSteps_Edit( object sender, CommandEventArgs e )
        {
            var stepId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();

            if ( stepId.HasValue )
            {
                var stepTypeId = GetStepTypeId( stepId.Value );

                if ( stepTypeId.HasValue )
                {
                    GoToStepPage( stepTypeId.Value, stepId );

                }
            }
        }

        /// <summary>
        /// Event when the user clicks to delete a step from the card view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rSteps_Delete( object sender, CommandEventArgs e )
        {
            var stepId = e.CommandArgument.ToStringSafe().AsInteger();
            DeleteStep( stepId );
            RenderCardView();
        }

        /// <summary>
        /// Handle the event where the user wants to delete a step from the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gStepList_Delete( object sender, RowEventArgs e )
        {
            var stepId = e.RowKeyId;
            DeleteStep( stepId );
            RenderGridView();
        }

        /// <summary>
        /// A row in the grid view grid is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gStepList_RowSelected( object sender, RowEventArgs e )
        {
            var stepId = e.RowKeyId;
            var stepTypeId = GetStepTypeId( stepId );

            if ( stepTypeId.HasValue )
            {
                GoToStepPage( stepTypeId.Value, stepId );
            }
        }

        /// <summary>
        /// Event called for each step in the add steps repeater
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void rAddStepButtons_ItemDataBound( Object Sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            var viewModel = e.Item.DataItem as AddStepButtonViewModel;
            var addStepButton = e.Item.DataItem as AddStepButtonViewModel;
            var bAddStep = e.Item.FindControl( "bAddStep" ) as HtmlButton;

            bAddStep.Attributes["data-step-type-id"] = viewModel.StepTypeId.ToString();

            if ( !addStepButton.IsEnabled )
            {
                bAddStep.Attributes["disabled"] = "disabled";
            }
        }

        /// <summary>
        /// When the card data is bound within the repeater, this method is called and allows manipulation of that card
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void rStepTypeCards_ItemDataBound( Object Sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            var cardData = e.Item.DataItem as CardViewModel;
            var stepTypeId = cardData.StepType.Id;

            var pnlPrereqs = e.Item.FindControl( "pnlPrereqs" ) as Panel;
            var lbCardAddStep = e.Item.FindControl( "lbCardAddStep" ) as LinkButton;

            lbCardAddStep.Visible = cardData.CanAddStep;
            pnlPrereqs.Visible = UserCanEdit && !cardData.HasMetPrerequisites;

            // Existing step records panel
            var steps = GetPersonStepsOfType( stepTypeId );
            var canEdit = cardData.StepType.AllowManualEditing && UserCanEdit;
            var canDelete = cardData.StepType.AllowManualEditing && UserCanEdit;

            var data = steps.Select( s => new CardStepViewModel
            {
                StepId = s.Id,
                CanEdit = canEdit,
                CanDelete = canDelete,
                StatusHtml = string.Format( "{0}<br /><small>{1}</small>",
                    s.StepStatus != null ? s.StepStatus.Name : string.Empty,
                    s.CompletedDateTime.HasValue ? s.CompletedDateTime.Value.ToShortDateString() : string.Empty )
            } );

            var rSteps = e.Item.FindControl( "rSteps" ) as Repeater;
            rSteps.DataSource = data;
            rSteps.DataBind();

            // Prereqs panel
            if ( !cardData.HasMetPrerequisites )
            {
                var prereqs = GetPrerequisiteStepTypes( stepTypeId );

                var rPrereqs = e.Item.FindControl( "rPrereqs" ) as Repeater;
                rPrereqs.DataSource = prereqs;
                rPrereqs.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rSteps control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rSteps_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            var cardStepViewModel = e.Item.DataItem as CardStepViewModel;
            var tdEdit = e.Item.FindControl( "tdEdit" ) as HtmlTableCell;
            var tdDelete = e.Item.FindControl( "tdDelete" ) as HtmlTableCell;

            tdEdit.Visible = cardStepViewModel.CanEdit;
            tdDelete.Visible = cardStepViewModel.CanDelete;
        }

        #endregion Events

        #region GridFilter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfGridFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfGridFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfGridFilter.SaveUserPreference( FilterKey.StepTypeName, tbStepTypeName.Text );
            gfGridFilter.SaveUserPreference( FilterKey.StepStatusName, tbStepStatus.Text );
            RenderGridView();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfGridFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfGridFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfGridFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var stepTypeNameFilter = gfGridFilter.GetUserPreference( FilterKey.StepTypeName );
            tbStepTypeName.Text = !string.IsNullOrWhiteSpace( stepTypeNameFilter ) ? stepTypeNameFilter : string.Empty;

            var stepStatusNameFilter = gfGridFilter.GetUserPreference( FilterKey.StepStatusName );
            tbStepStatus.Text = !string.IsNullOrWhiteSpace( stepStatusNameFilter ) ? stepStatusNameFilter : string.Empty;
        }

        #endregion GridFilter Events

        #region Model Helpers

        /// <summary>
        /// Gets the step program (model) that should be displayed in the block
        /// 1.) Use the block setting
        /// 2.) Use the page parameter
        /// 3.) Use the first active program
        /// </summary>
        /// <returns></returns>
        private StepProgram GetStepProgram()
        {
            if ( _stepProgram == null )
            {
                var programGuid = GetAttributeValue( AttributeKey.StepProgram ).AsGuidOrNull();
                var programId = PageParameter( PageParameterKey.StepProgramId ).AsIntegerOrNull();
                var rockContext = GetRockContext();
                var service = new StepProgramService( rockContext );
                var query = service.Queryable()
                    .Include( sp => sp.StepTypes )
                    .AsNoTracking();

                if ( programGuid.HasValue )
                {
                    // 1.) Use the block setting
                    _stepProgram = query.FirstOrDefault( sp => sp.Guid == programGuid.Value && sp.IsActive );
                }
                else if ( programId.HasValue )
                {
                    // 2.) Use the page parameter
                    _stepProgram = query.FirstOrDefault( sp => sp.Id == programId.Value && sp.IsActive );
                }
                else
                {
                    // 3.) Just use the first active program
                    _stepProgram = query.FirstOrDefault( sp => sp.IsActive );
                }
            }

            return _stepProgram;
        }
        private StepProgram _stepProgram;

        /// <summary>
        /// Get the term for the steps in the program
        /// </summary>
        /// <returns></returns>
        public string GetStepTerm()
        {
            if ( _stepTerm == null )
            {
                var defaultValue = "Step";
                var program = GetStepProgram();

                if ( program == null )
                {
                    _stepTerm = defaultValue;
                }
                else
                {
                    var term = program.StepTerm;
                    _stepTerm = term.IsNullOrWhiteSpace() ? defaultValue : term;
                }
            }

            return _stepTerm;
        }
        private string _stepTerm;

        /// <summary>
        /// Gets the step types (model) for this program
        /// </summary>
        /// <returns></returns>
        private List<StepType> GetStepTypes()
        {
            if ( _stepTypes == null )
            {
                var program = GetStepProgram();

                if ( program != null )
                {
                    _stepTypes = OrderStepTypes( program.StepTypes.Where( st => st.IsActive ) ).ToList();
                }
            }

            return _stepTypes;
        }
        private List<StepType> _stepTypes;

        /// <summary>
        /// Apply standard ordering for step types to a step type query
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        private IOrderedEnumerable<StepType> OrderStepTypes( IEnumerable<StepType> qry )
        {
            return qry.OrderBy( st => st.Order ).ThenBy( st => st.Name );
        }

        /// <summary>
        /// Gets the person model that should be displayed in the block
        /// 1.) Context Entity
        /// 2.) PersonId parameter
        /// 3.) Current person
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            if ( _person == null )
            {
                // 1.) Context Entity
                _person = ContextEntity<Person>();

                if ( _person == null )
                {
                    // 2.) PersonId parameter
                    var personId = PageParameter( "PersonId" ).AsIntegerOrNull();

                    if ( personId.HasValue )
                    {
                        var rockContext = GetRockContext();
                        _person = new PersonService( rockContext ).Get( personId.Value );
                    }
                    else
                    {
                        // 3.) Current person
                        _person = CurrentPerson;
                    }
                }
            }

            return _person;
        }
        private Person _person;

        /// <summary>
        /// Get a list of the steps that the person has taken within the given step type
        /// </summary>
        /// <param name="stepTypeId"></param>
        /// <returns></returns>
        private List<Step> GetPersonStepsOfType( int stepTypeId )
        {
            var defaultValue = new List<Step>();
            var personStepsMap = GetStepTypeToPersonStepMap();

            if ( personStepsMap == null )
            {
                return defaultValue;
            }

            List<Step> personStepsOfType = null;
            personStepsMap.TryGetValue( stepTypeId, out personStepsOfType );
            return personStepsOfType ?? defaultValue;
        }

        /// <summary>
        /// Get the person's steps for this program
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<Step>> GetStepTypeToPersonStepMap()
        {
            if ( _personStepsMap == null )
            {
                var person = GetPerson();
                var program = GetStepProgram();

                if ( person != null && program != null )
                {
                    var rockContext = GetRockContext();
                    var stepService = new StepService( rockContext );

                    var activeStepTypeIds = program.StepTypes.Where( st => st.IsActive ).Select( st => st.Id );

                    var personSteps = stepService.Queryable()
                        .Where( s =>
                            activeStepTypeIds.Contains( s.StepTypeId ) &&
                            s.PersonAlias.PersonId == person.Id )
                        .ToList();

                    _personStepsMap = activeStepTypeIds.ToDictionary(
                        stId => stId,
                        stId => personSteps
                            .Where( s => s.StepTypeId == stId )
                            .OrderBy( s => s.CompletedDateTime ?? s.EndDateTime ?? s.StartDateTime ?? s.CreatedDateTime ?? DateTime.MinValue )
                            .ToList() );
                }
            }

            return _personStepsMap;
        }
        private Dictionary<int, List<Step>> _personStepsMap;

        /// <summary>
        /// Given a step Id, get the step type Id
        /// </summary>
        /// <param name="stepId"></param>
        /// <returns></returns>
        private int? GetStepTypeId( int stepId )
        {
            var stepMap = GetStepTypeToPersonStepMap();

            foreach ( var kvp in stepMap )
            {
                if ( kvp.Value.Any( s => s.Id == stepId ) )
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        /// <summary>
        /// Get a list of the prerequisites for the given step type id
        /// </summary>
        /// <param name="stepTypeId"></param>
        /// <returns></returns>
        private List<StepType> GetPrerequisiteStepTypes( int stepTypeId )
        {
            var rockContext = GetRockContext();
            var service = new StepTypePrerequisiteService( rockContext );

            return service.Queryable()
                .AsNoTracking()
                .Include( stp => stp.PrerequisiteStepType )
                .Where( stp => stp.StepTypeId == stepTypeId && stp.PrerequisiteStepType.IsActive )
                .Select( stp => stp.PrerequisiteStepType )
                .ToList();
        }

        /// <summary>
        /// Has the person met the prereqs for the given step type
        /// </summary>
        /// <param name="stepTypeId"></param>
        /// <returns></returns>
        private bool HasMetPrerequisites( int stepTypeId )
        {
            var preReqs = GetPrerequisiteStepTypes( stepTypeId );

            if ( !preReqs.Any() )
            {
                return true;
            }

            foreach ( var preReq in preReqs )
            {
                var steps = GetPersonStepsOfType( preReq.Id );

                if ( !steps.Any() || steps.All( s => !s.IsComplete ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Can a step of the given step type be added for the person. Checks for active, prereqs, and if the step
        /// type allows more than one step record
        /// </summary>
        /// <param name="stepType"></param>
        /// <returns></returns>
        private bool CanAddStep( StepType stepType )
        {
            if ( !stepType.AllowManualEditing || !UserCanEdit )
            {
                return false;
            }

            if ( !stepType.IsActive || !HasMetPrerequisites( stepType.Id ) )
            {
                return false;
            }

            if ( stepType.AllowMultiple )
            {
                return true;
            }

            var exisitingSteps = GetPersonStepsOfType( stepType.Id );
            return !exisitingSteps.Any();
        }

        /// <summary>
        /// Delete the step with the given Id and then re-render the lists of steps
        /// </summary>
        /// <param name="stepId"></param>
        private void DeleteStep( int stepId )
        {
            var rockContext = GetRockContext();
            var service = new StepService( rockContext );
            var step = service.Get( stepId );
            string errorMessage;

            if ( step == null )
            {
                return;
            }

            if ( !service.CanDelete( step, out errorMessage ) )
            {
                ShowError( errorMessage );
                return;
            }

            service.Delete( step );
            rockContext.SaveChanges();
            ClearBlockData();
        }

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext;

        /// <summary>
        /// Clears the block data.
        /// </summary>
        private void ClearBlockData()
        {
            _rockContext = null;
            _personStepsMap = null;
            _person = null;
            _stepProgram = null;
            _stepTerm = null;
            _stepTypes = null;
        }

        #endregion Model Helpers

        #region Control Helpers

        /// <summary>
        /// Render the steps per row css literal control
        /// </summary>
        private void RenderStepsPerRow()
        {
            var stepsPerRow = GetAttributeValue( AttributeKey.StepsPerRow ).AsIntegerOrNull() ?? AttributeDefault.StepsPerRow;
            var stepsPerRowMobile = GetAttributeValue( AttributeKey.StepsPerRowMobile ).AsIntegerOrNull() ?? AttributeDefault.StepsPerRow;

            lStepsPerRowCss.Text =
@"<style>
    #" + upContent.ClientID + @" {
        --stepsPerRow: " + stepsPerRow + @";
        --stepsPerRowMobile: " + stepsPerRowMobile + @";
        }
</style>";
        }

        /// <summary>
        /// Display the step term in all the appropriate places
        /// </summary>
        private void DisplayStepTerm()
        {
            var stepTerm = GetStepTerm();
            tbStepTypeName.Label = string.Format( "{0} Type Name", stepTerm );
            tbStepStatus.Label = string.Format( "{0} Status", stepTerm );
            lStepType.HeaderText = string.Format( "{0} Type", stepTerm );
            lAddStepButtonsLabel.Text = string.Format( "Add {0}:", stepTerm );
        }

        /// <summary>
        /// Show an error in the notification box
        /// </summary>
        /// <param name="message"></param>
        private void ShowError( string message )
        {
            nbNotificationBox.NotificationBoxType = NotificationBoxType.Danger;
            nbNotificationBox.Title = "Uh oh...";
            nbNotificationBox.Text = message;
            nbNotificationBox.Visible = true;
        }

        /// <summary>
        /// Clear the displayed error
        /// </summary>
        private void ClearError()
        {
            nbNotificationBox.NotificationBoxType = NotificationBoxType.Danger;
            nbNotificationBox.Title = string.Empty;
            nbNotificationBox.Text = string.Empty;
            nbNotificationBox.Visible = false;
        }

        /// <summary>
        /// Set details on the block that come from the program
        /// </summary>
        private void SetProgramDetailsOnBlock()
        {
            if ( !ValidateRequiredModels() )
            {
                return;
            }

            var program = GetStepProgram();
            lStepProgramName.Text = program.Name;
            iIcon.Attributes["class"] = program.IconCssClass;
        }

        /// <summary>
        /// Render the step cards
        /// </summary>
        private void RenderCardView()
        {
            if ( !ValidateRequiredModels() )
            {
                return;
            }

            var program = GetStepProgram();
            var stepTerm = GetStepTerm();
            var person = GetPerson();
            var orderedStepTypes = OrderStepTypes( GetStepTypes() );
            var cardsData = new List<CardViewModel>();

            foreach ( var stepType in orderedStepTypes )
            {
                var cardCssClasses = new List<string>();
                var personStepsOfType = GetPersonStepsOfType( stepType.Id );

                var latestStep = personStepsOfType.LastOrDefault();
                var latestStepStatus = latestStep == null ? null : latestStep.StepStatus;
                var isComplete = personStepsOfType.Any( s => s.IsComplete );
                var canAddStep = CanAddStep( stepType );
                var hasMetPrerequisites = HasMetPrerequisites( stepType.Id );

                var rendered = stepType.CardLavaTemplate.ResolveMergeFields( new Dictionary<string, object> {
                    { "StepType", stepType },
                    { "Steps", personStepsOfType },
                    { "Person", person },
                    { "Program", program },
                    { "IsComplete", isComplete },
                    { "CompletedDateTime", personStepsOfType.Where( s => s.CompletedDateTime.HasValue ).Max( s => s.CompletedDateTime ) },
                    { "StepCount", personStepsOfType.Count },
                    { "CanAddStep", canAddStep },
                    { "LatestStep", latestStep },
                    { "LatestStepStatus", latestStepStatus },
                } );

                if ( isComplete )
                {
                    cardCssClasses.Add( "is-complete" );
                }

                if ( personStepsOfType.Any() )
                {
                    cardCssClasses.Add( "has-steps" );
                }
                else
                {
                    cardCssClasses.Add( "no-steps" );
                }

                if ( canAddStep )
                {
                    cardCssClasses.Add( "has-add" );
                }

                if ( UserCanEdit && !hasMetPrerequisites )
                {
                    cardCssClasses.Add( "has-prerequisite" );
                }

                cardsData.Add( new CardViewModel
                {
                    StepType = stepType,
                    RenderedLava = rendered,
                    StepTerm = stepTerm,
                    CardCssClass = cardCssClasses.JoinStrings( " " ),
                    CanAddStep = canAddStep,
                    HasMetPrerequisites = hasMetPrerequisites
                } );
            }

            rStepTypeCards.DataSource = cardsData;
            rStepTypeCards.DataBind();
        }

        /// <summary>
        /// Get data and bind it to the grid to display step records for the given person
        /// </summary>
        private void RenderGridView()
        {
            if ( !ValidateRequiredModels() )
            {
                return;
            }

            RenderStepGrid();
            RenderAddStepButtons();
        }

        /// <summary>
        /// Render the grid view's grid
        /// </summary>
        private void RenderStepGrid()
        {
            if ( !ValidateRequiredModels() )
            {
                return;
            }

            // Get the initial query
            var stepTypes = GetStepTypes();

            // Get filter values
            var stepTypeNameFilter = gfGridFilter.GetUserPreference( FilterKey.StepTypeName );
            var stepStatusNameFilter = gfGridFilter.GetUserPreference( FilterKey.StepStatusName );

            // Apply step type filters
            if ( !string.IsNullOrEmpty( stepTypeNameFilter ) )
            {
                stepTypes = stepTypes.Where( st => st.Name.Contains( stepTypeNameFilter ) ).ToList();
            }

            // Get the step type Ids
            var stepTypeIds = stepTypes.Select( st => st.Id ).ToList();

            // Query for the steps
            var rockContext = GetRockContext();
            var stepService = new StepService( rockContext );
            var person = GetPerson();

            var stepsQuery = stepService.Queryable()
                .AsNoTracking()
                .Include( s => s.StepType )
                .Include( s => s.StepStatus )
                .Where( s =>
                    s.PersonAlias.PersonId == person.Id &&
                    stepTypeIds.Contains( s.StepTypeId ) );

            // Apply step filters
            if ( !string.IsNullOrEmpty( stepStatusNameFilter ) )
            {
                stepsQuery = stepsQuery.Where( s => s.StepStatus != null && s.StepStatus.Name.Contains( stepStatusNameFilter ) );
            }

            // Create a view model for each step
            var viewModels = stepsQuery.Select( s => new StepGridRowViewModel
            {
                Id = s.Id,
                StepTypeName = s.StepType.Name,
                CompletedDateTime = s.CompletedDateTime,
                StepStatusColor = s.StepStatus == null ? string.Empty : s.StepStatus.StatusColor,
                StepStatusName = s.StepStatus == null ? string.Empty : s.StepStatus.Name,
                StepTypeIconCssClass = s.StepType.IconCssClass,
                StepTypeOrder = s.StepType.Order,
                Summary = string.Empty,
                Step = s
            } );

            // Sort the view models
            if ( gStepList.SortProperty != null )
            {
                viewModels = viewModels.Sort( gStepList.SortProperty );
            }
            else
            {
                viewModels = viewModels.OrderBy( vm => vm.StepTypeOrder ).ThenBy( vm => vm.StepTypeName );
            }

            // Bind the grid for the steps
            gStepList.SetLinqDataSource( viewModels );
            gStepList.DataBind();
        }

        /// <summary>
        /// Render the grid view's buttons
        /// </summary>
        private void RenderAddStepButtons()
        {
            if ( !ValidateRequiredModels() )
            {
                return;
            }

            var stepTypes = GetStepTypes();
            var addButtons = new List<AddStepButtonViewModel>();
            var stepTerm = GetStepTerm();

            foreach ( var stepType in stepTypes )
            {
                // Add a view model for the add button for this step type
                var addButtonIsEnabled = CanAddStep( stepType );

                addButtons.Add( new AddStepButtonViewModel
                {
                    StepTypeId = stepType.Id,
                    IsEnabled = CanAddStep( stepType ),
                    ButtonContents = string.Format( "<i class=\"{0}\"></i> &nbsp; {1}", stepType.IconCssClass, stepType.Name ),
                    StepTerm = stepTerm
                } );
            }

            // Bind the repeater for the add buttons
            rAddStepButtons.DataSource = addButtons;
            rAddStepButtons.DataBind();
        }

        /// <summary>
        /// Determine if the required models for rendering this block are available. If not, display a validation error.
        /// </summary>
        /// <returns></returns>
        private bool ValidateRequiredModels()
        {
            var program = GetStepProgram();

            if ( program == null )
            {
                ShowError( string.Format(
                    "The step program was not found. Please set the block attribute or ensure a query parameter `{0}` is set.",
                    PageParameterKey.StepProgramId ) );
                return false;
            }

            var person = GetPerson();

            if ( person == null )
            {
                ShowError( "The person was not found" );
                return false;
            }

            var stepTypes = GetStepTypes();

            if ( stepTypes == null )
            {
                ShowError( "The step types were not found" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Navigate to the step page. StepTypeId is required. StepId can be omitted for add, or set for edit.
        /// </summary>
        /// <param name="stepTypeId"></param>
        /// <param name="stepId"></param>
        private void GoToStepPage( int stepTypeId, int? stepId = null )
        {
            var person = GetPerson();

            if ( person != null )
            {
                NavigateToLinkedPage( AttributeKey.StepPage, new Dictionary<string, string> {
                    { "PersonId", person.Id.ToString() },
                    { "StepTypeId", stepTypeId.ToString() },
                    { "StepId", (stepId ?? 0).ToString() }
                } );
            }
        }

        #endregion Control Helpers

        #region View Models

        /// <summary>
        /// View model for data for a grid row
        /// </summary>
        private class StepGridRowViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the step type.
            /// </summary>
            /// <value>
            /// The name of the step type.
            /// </value>
            public string StepTypeName { get; set; }

            /// <summary>
            /// Gets or sets the completed date time.
            /// </summary>
            /// <value>
            /// The completed date time.
            /// </value>
            public DateTime? CompletedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the color of the step status.
            /// </summary>
            /// <value>
            /// The color of the step status.
            /// </value>
            public string StepStatusColor { get; set; }

            /// <summary>
            /// Gets or sets the name of the step status.
            /// </summary>
            /// <value>
            /// The name of the step status.
            /// </value>
            public string StepStatusName { get; set; }

            /// <summary>
            /// Gets or sets the step type icon CSS class.
            /// </summary>
            /// <value>
            /// The step type icon CSS class.
            /// </value>
            public string StepTypeIconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the summary.
            /// </summary>
            /// <value>
            /// The summary.
            /// </value>
            public string Summary { get; set; }

            /// <summary>
            /// Gets the step type order.
            /// </summary>
            /// <value>
            /// The step type order.
            /// </value>
            public int StepTypeOrder { get; internal set; }
            public Step Step { get; set; }
        }

        /// <summary>
        /// View model for the add step buttons above the grid
        /// </summary>
        private class AddStepButtonViewModel
        {
            /// <summary>
            /// Gets or sets the step type identifier.
            /// </summary>
            /// <value>
            /// The step type identifier.
            /// </value>
            public int StepTypeId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is enabled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// Gets or sets the button contents.
            /// </summary>
            /// <value>
            /// The button contents.
            /// </value>
            public string ButtonContents { get; set; }

            /// <summary>
            /// Gets or sets the step term.
            /// </summary>
            /// <value>
            /// The step term.
            /// </value>
            public string StepTerm { get; set; }
        }

        /// <summary>
        /// View model for the data show on a card
        /// </summary>
        private class CardViewModel
        {
            /// <summary>
            /// Gets or sets the type of the step.
            /// </summary>
            /// <value>
            /// The type of the step.
            /// </value>
            public StepType StepType { get; set; }

            /// <summary>
            /// Gets or sets the rendered lava.
            /// </summary>
            /// <value>
            /// The rendered lava.
            /// </value>
            public string RenderedLava { get; set; }

            /// <summary>
            /// Gets or sets the step term.
            /// </summary>
            /// <value>
            /// The step term.
            /// </value>
            public string StepTerm { get; set; }

            /// <summary>
            /// Gets or sets the card CSS class.
            /// </summary>
            /// <value>
            /// The card CSS class.
            /// </value>
            public string CardCssClass { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can add step.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can add step; otherwise, <c>false</c>.
            /// </value>
            public bool CanAddStep { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance has met prerequisites.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has met prerequisites; otherwise, <c>false</c>.
            /// </value>
            public bool HasMetPrerequisites { get; set; }
        }

        /// <summary>
        /// View model for a single step shown on the hover state of the card
        /// </summary>
        private class CardStepViewModel
        {
            /// <summary>
            /// Gets or sets the step identifier.
            /// </summary>
            /// <value>
            /// The step identifier.
            /// </value>
            public int StepId { get; set; }

            /// <summary>
            /// Gets or sets the status HTML.
            /// </summary>
            /// <value>
            /// The status HTML.
            /// </value>
            public string StatusHtml { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can edit.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
            /// </value>
            public bool CanEdit { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can delete.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can delete; otherwise, <c>false</c>.
            /// </value>
            public bool CanDelete { get; set; }
        }

        #endregion View Models
    }
}