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
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Entry" )]
    [Category( "Steps" )]
    [Description( "Displays a form to add or edit a step." )]

    #region Block Attributes

    [IntegerField(
        name: "Step Type Id",
        description: "The step type to use to add a new step. Leave blank to use the query string: StepTypeId. The type of the step, if step id is specified, overrides this setting.",
        required: false,
        order: 1,
        key: AttributeKey.StepType )]

    [LinkedPage(
        name: "Success Page",
        description: "The page to navigate to once the add or edit has completed. Leave blank to navigate to the parent page.",
        required: false,
        order: 2,
        key: AttributeKey.SuccessPage )]

    [LinkedPage(
        name: "Workflow Entry Page",
        description: "Page used to launch a new workflow of the selected type.",
        required: false,
        order: 3,
        key: AttributeKey.WorkflowEntryPage )]

    #endregion Block Attributes

    public partial class StepEntry : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The step type
            /// </summary>
            public const string StepType = "StepType";

            /// <summary>
            /// The success page
            /// </summary>
            public const string SuccessPage = "SuccessPage";

            /// <summary>
            /// The workflow entry page
            /// </summary>
            public const string WorkflowEntryPage = "WorkflowEntryPage";
        }

        /// <summary>
        /// Keys for the page parameters
        /// </summary>
        private static class ParameterKey
        {
            /// <summary>
            /// The step type identifier
            /// </summary>
            public const string StepTypeId = "StepTypeId";

            /// <summary>
            /// The step identifier
            /// </summary>
            public const string StepId = "StepId";

            /// <summary>
            /// The person identifier
            /// </summary>
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.InitializeWorkflowControls();

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Step.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack && !ValidateRequiredModels() )
            {
                pnlEditDetails.Visible = false;
                return;
            }

            if ( !IsPostBack )
            {
                ShowDetails();
            }
            else
            {
                BindWorkflows();
            }
        }

        #endregion

        #region Workflows

        /// <summary>
        /// Initialize the workflows control.
        /// </summary>
        private void InitializeWorkflowControls()
        {
            rptWorkflows.ItemCommand += rptWorkflows_ItemCommand;
        }

        /// <summary>
        /// Bind the set of available workflows to the repeater control.
        /// </summary>
        private void BindWorkflows()
        {
            var stepType = GetStepType();

            if ( stepType == null )
            {
                return;
            }

            var workflows = stepType.StepWorkflowTriggers
                .Union( stepType.StepProgram.StepWorkflowTriggers )
                .Where( x => x.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.Manual
                        && x.WorkflowType != null
                        && ( x.WorkflowType.IsActive ?? false ) )
                .OrderBy( w => w.WorkflowType.Name )
                .ToList();

            var authorizedWorkflows = workflows.Where( x => x.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) );

            bool hasWorkflows = authorizedWorkflows.Any();

            lblWorkflows.Visible = hasWorkflows;
            rptWorkflows.Visible = hasWorkflows;

            if ( hasWorkflows )
            {
                rptWorkflows.DataSource = authorizedWorkflows.ToList();
                rptWorkflows.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptRequestWorkflows control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptWorkflows_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "LaunchWorkflow" )
            {
                var triggerId = e.CommandArgument.ToString().AsInteger();
                var targetId = hfStepId.ValueAsInt();

                this.LaunchWorkflow( triggerId, targetId );
            }
        }

        /// <summary>
        /// Launch a specific workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="triggerId">The connection workflow.</param>
        /// <param name="targetId">The name.</param>
        private void LaunchWorkflow( int triggerId, int targetId )
        {
            var rockContext = this.GetRockContext();

            var target = new StepService( rockContext ).Get( targetId );

            var workflowTrigger = new StepWorkflowTriggerService( rockContext ).Get( triggerId );

            bool success = this.LaunchWorkflow( rockContext, target, workflowTrigger );

            if ( success )
            {
                ShowReadonlyDetails();
            }
        }

        /// <summary>
        /// Launch a specific workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowTrigger">The connection workflow.</param>
        /// <param name="target">The name.</param>
        private bool LaunchWorkflow( RockContext rockContext, Step target, StepWorkflowTrigger workflowTrigger )
        {
            if ( target == null
                 || workflowTrigger == null )
            {
                mdWorkflowResult.Show( "Workflow Processing Failed:<ul><li>The workflow parameters are invalid.</li></ul>", ModalAlertType.Information );
                return false;
            }

            var workflowType = workflowTrigger.WorkflowType;

            if ( workflowType == null || !( workflowType.IsActive ?? true ) )
            {
                mdWorkflowResult.Show( "Workflow Processing Failed:<ul><li>This workflow is unavailable.</li></ul>", ModalAlertType.Information );
                return false;
            }

            if ( !workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                mdWorkflowResult.Show( "Workflow Processing Failed:<ul><li>You are not authorized to access this workflow.</li></ul>", ModalAlertType.Information );
                return false;
            }

            var workflowTypeCache = WorkflowTypeCache.Get( workflowType );

            var workflow = Rock.Model.Workflow.Activate( workflowTypeCache, workflowTrigger.WorkflowType.WorkTerm, rockContext );

            if ( workflow == null )
            {
                mdWorkflowResult.Show( "Workflow Processing Failed:<ul><li>The workflow could not be activated.</li></ul>", ModalAlertType.Information );
                return false;
            }

            var workflowService = new Rock.Model.WorkflowService( rockContext );

            List<string> workflowErrors;

            var processed = workflowService.Process( workflow, target, out workflowErrors );

            if ( processed )
            {
                if ( workflow.HasActiveEntryForm( CurrentPerson ) )
                {
                    // If the workflow has a user entry form that can be displayed for the current user, show it now.
                    // Note that for a non-persisted workflow, a new instance of the workflow will be created after the user form is saved.
                    var qryParam = new Dictionary<string, string>();

                    qryParam.Add( "WorkflowTypeId", workflowType.Id.ToString() );

                    if ( workflow.Id != 0 )
                    {
                        qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                    }

                    var entryPage = this.GetAttributeValue( AttributeKey.WorkflowEntryPage );

                    if ( string.IsNullOrWhiteSpace( entryPage ) )
                    {
                        mdWorkflowResult.Show( "A Workflow Entry Page has not been configured for this block.", ModalAlertType.Alert );
                        return false;
                    }

                    NavigateToLinkedPage( AttributeKey.WorkflowEntryPage, qryParam );
                    return false;
                }
                else if ( workflow.Id != 0 )
                {
                    // The workflow has been started and persisted, but it has no requirement for user interaction.
                    mdWorkflowResult.Show( string.Format( "A '{0}' workflow has been started.", workflowType.Name ), ModalAlertType.Information );
                    return true;
                }
                else
                {
                    // The workflow has run to completion, and it has no requirement for user interaction.
                    mdWorkflowResult.Show( string.Format( "A '{0}' workflow was processed.", workflowType.Name ), ModalAlertType.Information );

                    return true;
                }
            }
            else
            {
                mdWorkflowResult.Show( "Workflow Processing Failed:<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
            }

            return false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = GetRockContext();
            var service = new StepService( rockContext );
            var step = GetStep();
            var stepType = GetStepType();
            var person = GetPerson();
            var isPersonSelectable = IsPersonSelectable();

            // If the person is allowed to be selected and the person is missing, query for it
            if ( isPersonSelectable && ppPerson.PersonId.HasValue && person == null )
            {
                var personService = new PersonService( rockContext );
                person = personService.Get( ppPerson.PersonId.Value );
            }

            // Person is the only required field for the step
            if ( person == null )
            {
                ShowError( "The person is required to save a step record." );
            }

            // If the step is null, then the aim is to create a new step
            var isAdd = step == null;

            if ( isAdd )
            {
                step = new Step
                {
                    StepTypeId = stepType.Id,
                    PersonAliasId = person.PrimaryAliasId.Value
                };
            }

            // Update the step properties. Person cannot be changed (only set when the step is added)
            step.CampusId = cpCampus.SelectedCampusId;
            step.StartDateTime = rdpStartDate.SelectedDate;
            step.EndDateTime = stepType.HasEndDate ? rdpEndDate.SelectedDate : null;
            step.StepStatusId = rsspStatus.SelectedValueAsId();

            step.CampusId = cpCampus.SelectedCampusId;

            // Update the completed date time, which is based on the start, end, and status
            if ( !step.StepStatusId.HasValue )
            {
                step.CompletedDateTime = null;
            }
            else
            {
                var stepStatusService = new StepStatusService( rockContext );
                var stepStatus = stepStatusService.Get( step.StepStatusId.Value );

                if ( stepStatus == null || !stepStatus.IsCompleteStatus )
                {
                    step.CompletedDateTime = null;
                }
                else
                {
                    step.CompletedDateTime = step.EndDateTime ?? step.StartDateTime;
                }
            }

            if ( !step.IsValid )
            {
                ShowError( step.ValidationResults.Select( vr => vr.ErrorMessage ).ToList().AsDelimited( "<br />" ) );
                return;
            }

            if ( isAdd )
            {
                var errorMessage = string.Empty;
                var canAdd = service.CanAdd( step, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    ShowError( errorMessage );
                    return;
                }

                if ( !canAdd )
                {
                    ShowError( "The step cannot be added for an unspecified reason" );
                    return;
                }

                service.Add( step );
            }

            // Save the step record
            rockContext.SaveChanges();

            // Save the step attributes from the attribute controls
            step.LoadAttributes( rockContext );
            avcAttributes.GetEditValues( step );
            step.SaveAttributeValues( rockContext );

            GoToSuccessPage( step.Id );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfStepId.Value.Equals( "0" ) )
            {
                GoToSuccessPage( null );
            }
            else
            {
                ShowReadonlyDetails();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            DeleteStep();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets a flag indicating if the user can edit the current record.
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && _step != null;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Validate that the models required to add or to edit are present
        /// </summary>
        private bool ValidateRequiredModels()
        {
            var stepType = GetStepType();

            if ( stepType == null )
            {
                ShowError( "A step type is required to add a step" );
                return false;
            }

            if ( !stepType.AllowManualEditing && !UserCanEdit )
            {
                ShowError( "You are not authorized to add or edit a step of this type" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Display an error in the browser window
        /// </summary>
        /// <param name="message"></param>
        private void ShowError( string message )
        {
            nbMessage.Text = message;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the detail panel containing the main content of the block.
        /// </summary>
        /// <param name="stepTypeId">The entity id of the item to be shown.</param>
        public void ShowDetails()
        {
            pnlDetails.Visible = false;

            // Get the Step data model
            var step = GetStep();

            int stepId = 0;

            if ( step != null )
            {
                stepId = step.Id;
            }

            pnlDetails.Visible = true;

            hfStepId.Value = stepId.ToString();

            btnEdit.Visible = true;
            btnDelete.Visible = true;

            if ( stepId == 0 )
            {
                ShowEditDetails();
            }
            else
            {
                ShowReadonlyDetails();
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        private void ShowEditDetails()
        {
            var stepType = GetStepType();

            if ( stepType == null )
            {
                return;
            }

            SetEditMode( true );

            rsspStatus.StepProgramId = stepType.StepProgramId;

            lStepTypeTitle.Text = string.Format( "{0} {1}",
                stepType.IconCssClass.IsNotNullOrWhiteSpace() ?
                    string.Format( @"<i class=""{0}""></i>", stepType.IconCssClass ) :
                    string.Empty,
                stepType.Name );

            rdpEndDate.Visible = stepType.HasEndDate;
            rdpStartDate.Label = stepType.HasEndDate ? "Start Date" : "Date";

            var step = GetStep();
            if ( step != null )
            {
                cpCampus.SelectedCampusId = step.CampusId;
                rdpStartDate.SelectedDate = step.StartDateTime;
                rdpEndDate.SelectedDate = step.EndDateTime;
                rsspStatus.SelectedValue = step.StepStatusId.ToStringSafe();
                cpCampus.SelectedCampusId = step.CampusId;
            }

            BuildDynamicControls( true );
            InitializePersonPicker();

            InitializeWorkflowControls();

            BindWorkflows();
        }

        /// <summary>
        /// Build the dynamic controls based on the attributes
        /// </summary>
        private void BuildDynamicControls( bool editMode )
        {
            var stepEntityTypeId = EntityTypeCache.GetId( typeof( Step ) );
            var excludedAttributes = AttributeCache.All()
                .Where( a => a.EntityTypeId == stepEntityTypeId )
                .Where( a => a.Key == "Order" || a.Key == "Active" );
            avcAttributes.ExcludedAttributes = excludedAttributes.ToArray();

            var stepType = GetStepType();
            var step = GetStep() ?? new Step { StepTypeId = stepType.Id };

            step.LoadAttributes();

            if ( editMode )
            {
                avcAttributes.AddEditControls( step );
            }
            else
            {
                avcAttributesView.AddDisplayControls( step );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="stepType">The entity instance to be displayed.</param>
        private void ShowReadonlyDetails()
        {
            SetEditMode( false );

            var step = GetStep();
            var stepType = GetStepType();

            lStepTypeTitle.Text = string.Format( "{0} {1}",
                                                 stepType.IconCssClass.IsNotNullOrWhiteSpace() ?
                                                 string.Format( @"<i class=""{0}""></i>", stepType.IconCssClass ) :
                                                 string.Empty,
                                                 stepType.Name );

            // Create the read-only description text.
            var descriptionListMain = new DescriptionList();

            descriptionListMain.Add( "Person", step.PersonAlias.Person.FullName );

            var campusCount = CampusCache.All().Count;
            if ( campusCount > 1 )
            {
                descriptionListMain.Add( "Campus", step.Campus == null ? string.Empty : step.Campus.Name );
            }

            if ( stepType.HasEndDate )
            {
                descriptionListMain.Add( "Start Date", step.StartDateTime, "d" );
                descriptionListMain.Add( "End Date", step.EndDateTime, "d" );
                descriptionListMain.Add( "Completed Date", step.EndDateTime, "d" );
            }
            else
            {
                descriptionListMain.Add( "Date", step.StartDateTime, "d" );
            }

            descriptionListMain.Add( "Status", step.StepStatus == null ? string.Empty : step.StepStatus.Name );

            lStepDescription.Text = descriptionListMain.Html;

            BuildDynamicControls( false );

            BindWorkflows();

            // Set the available actions according to current user permissions.
            var canEdit = CanEdit();

            btnEdit.Visible = canEdit;
            btnDelete.Visible = canEdit;
        }

        /// <summary>
        /// Delete the current Step.
        /// </summary>
        private void DeleteStep()
        {
            var step = this.GetStep();

            if ( step == null )
            {
                return;
            }

            var dataContext = GetRockContext();

            var stepService = new StepService( dataContext );

            string errorMessage;

            if ( !stepService.CanDelete( step, out errorMessage ) )
            {
                mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            stepService.Delete( step );

            dataContext.SaveChanges();

            GoToSuccessPage( null );
        }

        #endregion

        #region Attribute Helpers

        /// <summary>
        /// Redirect to the success page, or if it is not set, then go to the parent page
        /// </summary>
        private void GoToSuccessPage( int? newStepId )
        {
            var page = GetAttributeValue( AttributeKey.SuccessPage );
            var parameters = new Dictionary<string, string>();
            var stepTypeIdParam = PageParameter( ParameterKey.StepTypeId ).AsIntegerOrNull();
            var personIdParam = PageParameter( ParameterKey.PersonId ).AsIntegerOrNull();

            if ( personIdParam.HasValue )
            {
                parameters.Add( ParameterKey.PersonId, personIdParam.Value.ToString() );
            }
            else if ( stepTypeIdParam.HasValue )
            {
                parameters.Add( ParameterKey.StepTypeId, stepTypeIdParam.Value.ToString() );
            }

            if ( page.IsNullOrWhiteSpace() )
            {
                NavigateToParentPage( parameters );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.SuccessPage, parameters );
            }
        }

        #endregion Attribute Helpers

        #region Model Getters

        /// <summary>
        /// Get the step model
        /// </summary>
        /// <returns></returns>
        private Step GetStep()
        {
            if ( _step == null )
            {
                var stepId = PageParameter( ParameterKey.StepId ).AsIntegerOrNull();

                if ( stepId.HasValue )
                {
                    var rockContext = GetRockContext();
                    var service = new StepService( rockContext );
                    _step = service.Get( stepId.Value );
                }

                if ( _step != null )
                {
                    hfStepId.Value = _step.Id.ToString();
                }
            }

            return _step;
        }
        private Step _step = null;

        /// <summary>
        /// Get the step type model
        /// </summary>
        /// <returns></returns>
        private StepType GetStepType()
        {
            if ( _stepType == null )
            {
                var step = GetStep();

                if ( step != null )
                {
                    _stepType = step.StepType;
                }
                else
                {
                    var stepTypeId = GetAttributeValue( AttributeKey.StepType ).AsIntegerOrNull() ??
                        PageParameter( ParameterKey.StepTypeId ).AsIntegerOrNull();

                    if ( stepTypeId.HasValue )
                    {
                        var rockContext = GetRockContext();
                        var service = new StepTypeService( rockContext );

                        _stepType = service.Queryable()
                            .AsNoTracking()
                            .FirstOrDefault( st => st.Id == stepTypeId.Value && st.IsActive );
                    }
                }
            }

            return _stepType;
        }
        private StepType _stepType = null;

        /// <summary>
        /// Get the person. 1st source is the step, 2nd is the query param, 3rd is the context
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            if ( _person == null )
            {
                var step = GetStep();

                if ( step != null && step.PersonAlias != null && step.PersonAlias.Person != null )
                {
                    _person = step.PersonAlias.Person;
                }
                else
                {
                    var personId = PageParameter( ParameterKey.PersonId ).AsIntegerOrNull();

                    if ( personId.HasValue )
                    {
                        var rockContext = GetRockContext();
                        var service = new PersonService( rockContext );
                        _person = service.Get( personId.Value );
                    }
                    else
                    {
                        _person = ContextEntity() as Person;
                    }
                }
            }

            return _person;
        }
        private Person _person = null;

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
        private RockContext _rockContext = null;

        #endregion Model Getters

        #region Control Helpers

        /// <summary>
        /// Initialize the person picker. Show or hide it based on if the person is selectable
        /// </summary>
        private void InitializePersonPicker()
        {
            var isSelectable = IsPersonSelectable();
            ppPerson.Enabled = isSelectable;
            ppPerson.Required = isSelectable;

            var person = GetPerson();
            ppPerson.SetValue( person );
        }

        /// <summary>
        /// Returns true if this block is adding a new step and the Person is not set by context or page parameter
        /// </summary>
        /// <returns></returns>
        private bool IsPersonSelectable()
        {
            var person = GetPerson();
            var step = GetStep();
            return step == null && person == null;
        }

        #endregion Control Helpers
    }
}
