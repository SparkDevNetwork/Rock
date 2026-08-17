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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StepEntry;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a form to add or edit a step.
    /// </summary>

    [DisplayName( "Step Entry" )]
    [Category( "Steps" )]
    [Description( "Displays a form to add or edit a step." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        name: "Step Type Id",
        Description = "The step type to use to add a new step. Leave blank to use the query string: StepTypeId. The type of the step, if step id is specified, overrides this setting.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.StepType )]

    [LinkedPage(
        "Success Page",
        Description = "The page to navigate to once the add or edit has completed. Leave blank to navigate to the parent page.",
        Key = AttributeKey.SuccessPage,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Key = AttributeKey.WorkflowEntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "58CFCCAA-8E7F-43BD-BE01-E46EF96D4AD6" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "6EDB27CF-6DCB-424C-8639-47D8BA975B1E" )]
    [Rock.SystemGuid.BlockTypeGuid( "8D78BC55-6E67-40AB-B453-994D69503838" )]
    public class StepEntry : RockEntityDetailBlockType<Step, StepEntryBag>
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The step type identifier block setting.
            /// </summary>
            public const string StepType = "StepType";

            /// <summary>
            /// The success page linked page.
            /// </summary>
            public const string SuccessPage = "SuccessPage";

            /// <summary>
            /// The workflow entry page linked page.
            /// </summary>
            public const string WorkflowEntryPage = "WorkflowEntryPage";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The step identifier page parameter.
            /// </summary>
            public const string StepId = "StepId";

            /// <summary>
            /// The step type identifier page parameter.
            /// </summary>
            public const string StepTypeId = "StepTypeId";

            /// <summary>
            /// The person identifier page parameter (legacy, integer-based).
            /// </summary>
            public const string PersonId = "PersonId";

            /// <summary>
            /// The general person identifier page parameter which can be the Id, IdKey, or Guid.
            /// </summary>
            public const string Person = "Person";
        }

        /// <summary>
        /// Keys for navigation URLs sent to the client.
        /// </summary>
        private static class NavigationUrlKey
        {
            /// <summary>
            /// The success page URL.
            /// </summary>
            public const string SuccessPage = "SuccessPage";
        }

        #endregion Keys

        #region Fields

        private StepType _stepType;
        private Person _person;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<StepEntryBag, StepEntryOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StepEntryBag, StepEntryOptionsBag> box )
        {
            var entity = GetInitialEntity();
            var stepType = GetStepType( entity );

            // Validate that a step type was resolved.
            if ( stepType == null )
            {
                box.ErrorMessage = "A step type is required to add a step.";
                return;
            }

            if ( !stepType.AllowManualEditing )
            {
                box.ErrorMessage = "You are not authorized to add or edit a step of this type.";
                return;
            }

            var canEdit = CanEdit( entity, stepType );
            box.IsEditable = canEdit;

            // Set StepTypeId so that LoadAttributes resolves the correct
            // step-type-qualified attributes for new steps.
            if ( entity.Id == 0 )
            {
                entity.StepTypeId = stepType.Id;
            }

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity — prepare for view mode by default.
                var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );

                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Step.FriendlyTypeName );
                }
            }
            else
            {
                // New entity — prepare for edit mode by default.
                if ( canEdit )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Step.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the options bag for the block.
        /// </summary>
        /// <param name="isEditable">Whether the entity is editable.</param>
        /// <returns>The options bag.</returns>
        private StepEntryOptionsBag GetBoxOptions( bool isEditable )
        {
            var entity = GetInitialEntity();
            var stepType = GetStepType( entity );

            var options = new StepEntryOptionsBag
            {
                StepProgramGuid = stepType?.StepProgram?.Guid,
                HasEndDate = stepType?.HasEndDate ?? false,
                IsDateRequired = stepType?.IsDateRequired ?? false,
                IsPersonSelectable = entity.Id == 0 && GetPerson( entity ) == null,
                IsSingleCampus = CampusCache.SingleCampus != null,
                StepTypeName = stepType?.Name,
                StepTypeIconCssClass = stepType?.IconCssClass,
                AvailableWorkflows = entity.Id != 0 ? GetAvailableWorkflows( stepType ) : new List<StepEntryWorkflowBag>()
            };

            return options;
        }

        /// <summary>
        /// Gets the navigation URLs for the box.
        /// </summary>
        /// <returns>A dictionary of navigation URL keys and values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.SuccessPage] = GetSuccessPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override StepEntryBag GetEntityBagForView( Step entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            // CompletedDateTime is server-computed and only needed for view display.
            bag.CompletedDateTime = entity.CompletedDateTime?.ToString( "s" );

            // Person info for view panel.
            var person = GetPerson( entity );

            if ( person != null )
            {
                bag.PersonPhotoUrl = person.PhotoUrl;
                bag.PersonConnectionStatus = person.ConnectionStatusValue?.Value;
            }

            // Status color for the header label.
            if ( entity.StepStatus != null )
            {
                bag.StepStatusColor = entity.StepStatus.StatusColorOrDefault;
            }

            // Sanitize the note for safe HTML display.
            bag.NoteHtml = entity.Note?.ScrubHtmlAndConvertCrLfToBr();

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override StepEntryBag GetEntityBagForEdit( Step entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Gets the entity bag properties that are common between view and edit modes.
        /// </summary>
        /// <param name="entity">The step entity.</param>
        /// <returns>A populated <see cref="StepEntryBag"/>.</returns>
        private StepEntryBag GetCommonEntityBag( Step entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var person = GetPerson( entity );

            var bag = new StepEntryBag
            {
                IdKey = entity.IdKey,
                Campus = entity.CampusId.HasValue ? CampusCache.Get( entity.CampusId.Value )?.ToListItemBag() : null,
                StartDateTime = entity.StartDateTime?.ToString( "s" ),
                EndDateTime = entity.EndDateTime?.ToString( "s" ),
                Note = entity.Note
            };

            // Set the step status list item.
            if ( entity.StepStatusId.HasValue && entity.StepStatus != null )
            {
                bag.StepStatus = new ListItemBag
                {
                    Value = entity.StepStatus.Guid.ToString(),
                    Text = entity.StepStatus.Name
                };
            }

            // Set person alias for the person picker.
            if ( person != null )
            {
                bag.PersonAlias = new ListItemBag
                {
                    Value = person.PrimaryAlias?.Guid.ToString(),
                    Text = person.FullName
                };
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Step entity, ValidPropertiesBox<StepEntryBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Campus ),
                () => entity.CampusId = box.Bag.Campus?.GetEntityId<Campus>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.StartDateTime ),
                () => entity.StartDateTime = box.Bag.StartDateTime.AsDateTime() );

            box.IfValidProperty( nameof( box.Bag.EndDateTime ),
                () =>
                {
                    var stepType = GetStepType( entity );
                    entity.EndDateTime = stepType?.HasEndDate == true ? box.Bag.EndDateTime.AsDateTime() : null;
                } );

            box.IfValidProperty( nameof( box.Bag.StepStatus ),
                () =>
                {
                    if ( box.Bag.StepStatus?.Value != null )
                    {
                        var statusGuid = box.Bag.StepStatus.Value.AsGuid();
                        var status = new StepStatusService( RockContext ).Get( statusGuid );
                        entity.StepStatusId = status?.Id;
                    }
                    else
                    {
                        entity.StepStatusId = null;
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.Note ),
                () => entity.Note = box.Bag.Note );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );
                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override Step GetInitialEntity()
        {
            var stepKey = RequestContext.GetPageParameter( PageParameterKey.StepId );
            var step = new StepService( RockContext ).Get( stepKey, !PageCache.Layout.Site.DisablePredictableIds );

            return step ?? new Step { Id = 0, Guid = Guid.Empty };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Step entity, out BlockActionResult error )
        {
            var entityService = new StepService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                var stepType = GetStepType();
                var person = GetPerson();

                entity = new Step
                {
                    StepTypeId = stepType?.Id ?? 0
                };

                // Set the person alias for a new step.
                if ( person?.PrimaryAliasId != null )
                {
                    entity.PersonAliasId = person.PrimaryAliasId.Value;
                }
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Step.FriendlyTypeName} not found." );
                return false;
            }

            if ( !CanEdit( entity, GetStepType( entity ) ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {Step.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Determines whether the current person can edit the step, checking
        /// both EDIT and MANAGE_STEPS authorization on the step or step type.
        /// </summary>
        /// <param name="step">The step entity.</param>
        /// <param name="stepType">The step type.</param>
        /// <returns><c>true</c> if the current person can edit; otherwise <c>false</c>.</returns>
        private bool CanEdit( Step step, StepType stepType )
        {
            var currentPerson = RequestContext.CurrentPerson;

            /*
                4/1/26 - MSE

                For existing steps (Id != 0) we check authorization on the step
                entity itself. For new steps (Id == 0) we fall back to the step
                type, matching the original WebForms behavior where _step was
                null for the add case and non-null only when loaded from the DB.

                Reason: Ensure entity-level security is honored for existing
                records while still allowing creation based on step type auth.
            */
            if ( step != null && step.Id != 0 )
            {
                return step.IsAuthorized( Authorization.EDIT, currentPerson )
                    || step.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson );
            }

            if ( stepType != null )
            {
                return stepType.IsAuthorized( Authorization.EDIT, currentPerson )
                    || stepType.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson );
            }

            return false;
        }

        /// <summary>
        /// Resolves the step type from the step entity, the block attribute, or the page parameter.
        /// </summary>
        /// <param name="step">The optional step entity to get the type from.</param>
        /// <returns>The resolved step type, or <c>null</c> if not found.</returns>
        private StepType GetStepType( Step step = null )
        {
            if ( _stepType != null )
            {
                return _stepType;
            }

            // First try the step's own type.
            if ( step?.StepType != null )
            {
                _stepType = step.StepType;
                return _stepType;
            }

            var service = new StepTypeService( RockContext );

            // Then try the block attribute.
            var stepTypeId = GetAttributeValue( AttributeKey.StepType ).AsIntegerOrNull();

            if ( stepTypeId.HasValue )
            {
                _stepType = service.Queryable()
                    .AsNoTracking()
                    .Include( st => st.StepProgram )
                    .FirstOrDefault( st => st.Id == stepTypeId.Value && st.IsActive );

                return _stepType;
            }

            // Finally try the page parameter.
            var stepTypeKeyParam = RequestContext.GetPageParameter( PageParameterKey.StepTypeId );

            if ( stepTypeKeyParam.IsNotNullOrWhiteSpace() )
            {
                var stepType = service.Get( stepTypeKeyParam, !PageCache.Layout.Site.DisablePredictableIds );
                _stepType = stepType != null && stepType.IsActive ? stepType : null;

                // Eager load the step program.
                if ( _stepType != null && _stepType.StepProgram == null )
                {
                    _stepType.StepProgram = new StepProgramService( RockContext ).Get( _stepType.StepProgramId );
                }

                return _stepType;
            }

            return null;
        }

        /// <summary>
        /// Resolves the person from the step entity, the page parameters, or the context entity.
        /// </summary>
        /// <param name="step">The optional step entity to get the person from.</param>
        /// <returns>The resolved person, or <c>null</c> if not found.</returns>
        private Person GetPerson( Step step = null )
        {
            if ( _person != null )
            {
                return _person;
            }

            if ( step != null && step.PersonAliasId > 0 )
            {
                _person = new PersonAliasService( RockContext ).GetPerson( step.PersonAliasId );
                return _person;
            }

            // Try the Person page parameter (preferred).
            var personKey = RequestContext.GetPageParameter( PageParameterKey.Person );

            // Fall back to the PersonId page parameter.
            if ( personKey.IsNullOrWhiteSpace() )
            {
                personKey = RequestContext.GetPageParameter( PageParameterKey.PersonId );
            }

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                _person = new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
                return _person;
            }

            // Try the context entity.
            _person = RequestContext.GetContextEntity<Person>();

            return _person;
        }

        /// <summary>
        /// Gets the available manual workflow triggers for the step type, authorized for the current person.
        /// </summary>
        /// <param name="stepType">The step type.</param>
        /// <returns>A list of workflow trigger bags.</returns>
        private List<StepEntryWorkflowBag> GetAvailableWorkflows( StepType stepType )
        {
            if ( stepType == null )
            {
                return new List<StepEntryWorkflowBag>();
            }

            var currentPerson = RequestContext.CurrentPerson;

            // Get triggers from both the step type and the step program.
            var triggers = new StepWorkflowTriggerService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( t => t.WorkflowType )
                .Where( t =>
                    ( t.StepTypeId == stepType.Id || ( !t.StepTypeId.HasValue && t.StepProgramId == stepType.StepProgramId ) )
                    && t.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.Manual
                    && t.WorkflowType != null
                    && ( t.WorkflowType.IsActive ?? false ) )
                .OrderBy( t => t.WorkflowType.Name )
                .ToList();

            return triggers
                .Where( t => t.WorkflowType.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( t => new StepEntryWorkflowBag
                {
                    Guid = t.Guid,
                    WorkflowTypeName = t.WorkflowType.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the success page URL using the linked page attribute or the parent page.
        /// Includes relevant page parameters for context preservation.
        /// </summary>
        /// <returns>The success page URL.</returns>
        private string GetSuccessPageUrl()
        {
            var parameters = new Dictionary<string, string>();
            var personKey = RequestContext.GetPageParameter( PageParameterKey.Person );
            var personIdParam = RequestContext.GetPageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            var stepTypeIdParam = RequestContext.GetPageParameter( PageParameterKey.StepTypeId );

            var stepType = GetStepType();

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                parameters.Add( PageParameterKey.Person, personKey );
            }
            else if ( personIdParam.HasValue )
            {
                parameters.Add( PageParameterKey.PersonId, personIdParam.Value.ToString() );
            }
            else if ( stepTypeIdParam.IsNotNullOrWhiteSpace() )
            {
                parameters.Add( PageParameterKey.StepTypeId, stepTypeIdParam );
            }
            else if ( stepType != null )
            {
                parameters.Add( PageParameterKey.StepTypeId, stepType.IdKey );
            }

            if ( stepType != null )
            {
                parameters["ProgramId"] = stepType.StepProgramId.ToString();
            }

            var successPage = GetAttributeValue( AttributeKey.SuccessPage );

            if ( successPage.IsNullOrWhiteSpace() )
            {
                return this.GetParentPageUrl( parameters );
            }

            return this.GetLinkedPageUrl( AttributeKey.SuccessPage, parameters );
        }

        /// <summary>
        /// Updates the CompletedDateTime on the step based on the current status, start date, and end date.
        /// </summary>
        /// <param name="step">The step to update.</param>
        private void UpdateCompletedDateTime( Step step )
        {
            if ( !step.StepStatusId.HasValue )
            {
                step.CompletedDateTime = null;
                return;
            }

            var stepStatus = new StepStatusService( RockContext ).Get( step.StepStatusId.Value );

            if ( stepStatus == null || !stepStatus.IsCompleteStatus )
            {
                step.CompletedDateTime = null;
            }
            else
            {
                step.CompletedDateTime = step.EndDateTime ?? step.StartDateTime;
            }
        }

        #endregion Helper Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<StepEntryBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A URL to redirect to after saving.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<StepEntryBag> box )
        {
            var entityService = new StepService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isAdd = entity.Id == 0;

            // If the person is selectable and provided in the bag, set it on the new entity.
            if ( isAdd && box.Bag.PersonAlias?.Value != null )
            {
                var personAliasGuid = box.Bag.PersonAlias.Value.AsGuid();
                var personAlias = new PersonAliasService( RockContext ).Get( personAliasGuid );

                if ( personAlias != null )
                {
                    entity.PersonAliasId = personAlias.Id;
                }
            }

            if ( isAdd && entity.PersonAliasId == 0 )
            {
                return ActionBadRequest( "A person is required to save a step." );
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Update the computed CompletedDateTime.
            UpdateCompletedDateTime( entity );

            // Validate the entity.
            if ( !entity.IsValid )
            {
                return ActionBadRequest( entity.ValidationResults.Select( vr => vr.ErrorMessage ).JoinStrings( ", " ) );
            }

            /*
                4/7/26 - MSE

                Add is deferred to here instead of TryGetEntityForEditAction()
                because StepService.Add() internally calls CanAdd(), which
                validates AllowMultiple and prerequisite rules and throws an
                ArgumentException on failure. In TryGetEntityForEditAction() the
                entity is only partially built (no dates, status, or person
                from the form), so that validation would run against incomplete
                data and throw before Save() ever gets a chance to handle it.

                By adding here the entity is fully populated, and the catch
                converts the exception into a friendly ActionBadRequest response.

                Reason: StepService overrides Add() with throwing validation,
                unlike other services where Add() simply tracks the entity.
            */
            if ( isAdd )
            {
                try
                {
                    entityService.Add( entity );
                }
                catch ( ArgumentException ex )
                {
                    return ActionBadRequest( ex.Message );
                }
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            // Step Entry always navigates to the success page after save.
            return ActionOk( GetSuccessPageUrl() );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new StepService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( GetSuccessPageUrl() );
        }

        /// <summary>
        /// Launches a manual workflow trigger for the current step.
        /// </summary>
        /// <param name="guid">The Guid of the workflow trigger to launch.</param>
        /// <returns>An object with a message and optional redirect URL.</returns>
        [BlockAction]
        public BlockActionResult LaunchWorkflow( Guid guid )
        {
            var step = GetInitialEntity();

            if ( step == null || step.Id == 0 )
            {
                return ActionBadRequest( "The step must be saved before launching a workflow." );
            }

            var workflowTrigger = new StepWorkflowTriggerService( RockContext ).Get( guid );

            if ( workflowTrigger == null )
            {
                return ActionBadRequest( "The workflow trigger was not found." );
            }

            var workflowType = workflowTrigger.WorkflowType;

            if ( workflowType == null || !( workflowType.IsActive ?? true ) )
            {
                return ActionBadRequest( "This workflow is unavailable." );
            }

            if ( !workflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to access this workflow." );
            }

            var workflowTypeCache = WorkflowTypeCache.Get( workflowType );
            var workflow = Rock.Model.Workflow.Activate( workflowTypeCache, workflowType.WorkTerm, RockContext );

            if ( workflow == null )
            {
                return ActionBadRequest( "The workflow could not be activated." );
            }

            var workflowService = new WorkflowService( RockContext );
            List<string> workflowErrors;

            var processed = workflowService.Process( workflow, step, out workflowErrors );

            if ( !processed )
            {
                return ActionBadRequest( "Workflow processing failed: " + workflowErrors.AsDelimited( ", " ) );
            }

            if ( workflow.HasActiveEntryForm( RequestContext.CurrentPerson ) )
            {
                // The workflow has an entry form — return a redirect URL for the workflow entry page.
                var entryPage = GetAttributeValue( AttributeKey.WorkflowEntryPage );

                if ( entryPage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "A Workflow Entry Page has not been configured for this block." );
                }

                var qryParam = new Dictionary<string, string>
                {
                    { "WorkflowTypeId", workflowType.Id.ToString() }
                };

                if ( workflow.Id != 0 )
                {
                    qryParam.Add( "WorkflowGuid", workflow.Guid.ToString() );
                }

                var redirectUrl = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, qryParam );

                return ActionOk( new StepEntryLaunchWorkflowResultBag
                {
                    RedirectUrl = redirectUrl
                } );
            }

            // The workflow completed without requiring user interaction.
            return ActionOk( new StepEntryLaunchWorkflowResultBag
            {
                Message = workflow.Id != 0
                    ? $"A '{workflowType.Name}' workflow has been started."
                    : $"A '{workflowType.Name}' workflow was started."
            } );
        }

        #endregion Block Actions
    }
}
