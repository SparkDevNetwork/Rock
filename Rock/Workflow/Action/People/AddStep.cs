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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Creates a step for the person
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Adds a step for a person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Step Add" )]

    #region Attributes

    [WorkflowAttribute(
        name: "Person",
        description: "Workflow attribute that contains the person that is taking the step.",
        required: true,
        order: 0,
        key: AttributeKey.Person,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowTextOrAttribute(
        textLabel: "Step Type Id",
        attributeLabel: "Attribute Value",
        description: "The step type id in which the step will be created.",
        required: true,
        order: 1,
        key: AttributeKey.StepProgramStepType,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.StepProgramStepTypeFieldType",
            "Rock.Field.Types.TextFieldType"
        } )]

    [WorkflowTextOrAttribute(
        textLabel: "Step Status Id",
        attributeLabel: "Attribute Value",
        description: "The step status id in which the step will be created.",
        required: true,
        order: 2,
        key: AttributeKey.StepProgramStepStatus,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.StepProgramStepStatusFieldType",
            "Rock.Field.Types.TextFieldType"
        } )]

    [WorkflowTextOrAttribute(
        textLabel: "Start Date",
        attributeLabel: "Attribute Value",
        description: "The date that the step was started.",
        required: false,
        order: 3,
        key: AttributeKey.StartDate,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.DateFieldType",
            "Rock.Field.Types.TextFieldType"
        } )]

    [WorkflowTextOrAttribute(
        textLabel: "End Date",
        attributeLabel: "Attribute Value",
        description: "The date that the step was ended. This will also be set as the step 'completed' date.",
        required: false,
        order: 4,
        key: AttributeKey.EndDate,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.DateFieldType",
            "Rock.Field.Types.TextFieldType"
        } )]

    [WorkflowAttribute(
        name: "Step Attribute",
        description: "An optional step attribute to store the item that is created.",
        required: false,
        order: 5,
        key: AttributeKey.StepAttribute,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.StepFieldType" } )]

    [WorkflowTextOrAttribute(
        textLabel: "Campus",
        attributeLabel: "Attribute Value",
        description: "The campus where the step was completed.",
        required: false,
        order: 6,
        key: AttributeKey.Campus,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.CampusFieldType",
            "Rock.Field.Types.TextFieldType" } )]

    #endregion Attributes

    public class AddStep : ActionComponent
    {
        /// <summary>
        /// Keys for the attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string StepProgramStepType = "StepProgramStepType";
            public const string StepProgramStepStatus = "StepProgramStepStatus";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string StepAttribute = "StepAttribute";
            public const string Campus = "Campus";
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var mergeFields = GetMergeFields( action );

            // Validate the person exists
            var personGuid = GetAttributeValue( action, AttributeKey.Person, true ).AsGuidOrNull();

            if ( !personGuid.HasValue )
            {
                errorMessages.Add( "The person guid is required but was missing" );
                return LogMessagesForExit( action, errorMessages );
            }

            var personService = new PersonService( rockContext );
            var person = personService.Queryable( "Aliases" ).AsNoTracking()
                .FirstOrDefault( p => p.Guid == personGuid.Value || p.Aliases.Any( pa => pa.Guid == personGuid.Value ) );

            if ( person == null )
            {
                errorMessages.Add( $"The person with the guid '{personGuid.Value}' was not found" );
                return LogMessagesForExit( action, errorMessages );
            }

            if ( !person.PrimaryAliasId.HasValue )
            {
                errorMessages.Add( $"{person.FullName} does not have a primary alias identifier" );
                return LogMessagesForExit( action, errorMessages );
            }

            // Validate the step type exists. Could be a step type id or a guid
            var stepType = GetStepType( rockContext, action, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( errorMessage );
                return LogMessagesForExit( action, errorMessages );
            }

            if ( stepType == null )
            {
                errorMessages.Add( "The step type could not be found" );
                return LogMessagesForExit( action, errorMessages );
            }

            // Validate the step status exists and is in the same program as the step type
            var stepStatus = GetStepStatus( stepType, action, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( errorMessage );
                return LogMessagesForExit( action, errorMessages );
            }

            if ( stepStatus == null )
            {
                errorMessages.Add( "The step status could not be found" );
                return LogMessagesForExit( action, errorMessages );
            }

            // Get the start and end dates
            var startDate = GetLavaAttributeValue( action, AttributeKey.StartDate ).AsDateTime() ?? RockDateTime.Now;
            var endDate = GetLavaAttributeValue( action, AttributeKey.EndDate ).AsDateTime();

            var campusAttributeValue = GetLavaAttributeValue( action, AttributeKey.Campus );
            var campusId = campusAttributeValue.AsIntegerOrNull();
            var campusGuid = campusAttributeValue.AsGuidOrNull();

            if ( campusGuid != null )
            {
                var campus = CampusCache.Get( campusGuid.Value );
                if ( campus != null )
                {
                    campusId = campus.Id;
                }
            }

            // The completed date is today or the end date if the status is a completed status
            var completedDate = stepStatus.IsCompleteStatus ? ( endDate ?? RockDateTime.Now ) : ( DateTime? ) null;

            // Create the step object
            var step = new Step
            {
                StepTypeId = stepType.Id,
                PersonAliasId = person.PrimaryAliasId.Value,
                StartDateTime = startDate,
                EndDateTime = endDate,
                CompletedDateTime = completedDate,
                StepStatusId = stepStatus.Id,
                CampusId = campusId
            };

            // Validate the step
            if ( !step.IsValid )
            {
                errorMessages.AddRange( step.ValidationResults.Select( a => a.ErrorMessage ) );
                return LogMessagesForExit( action, errorMessages );
            }

            // Check if the step can be created because of Allow Multiple rules on the step type and also prerequisite requirements
            var stepService = new StepService( rockContext );
            var canAdd = stepService.CanAdd( step, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( errorMessage );
            }
            else if ( !canAdd )
            {
                errorMessages.Add( "Cannot add the step for an unspecified reason" );
            }
            else
            {
                try
                {
                    stepService.Add( step );
                    rockContext.SaveChanges();

                    SetCreatedItemAttribute( action, AttributeKey.StepAttribute, step, rockContext );
                }
                catch ( Exception exception )
                {
                    errorMessages.Add( $"Exception thrown: {exception.Message}" );
                    ExceptionLogService.LogException( exception );
                }
            }

            return LogMessagesForExit( action, errorMessages );
        }

        /// <summary>
        /// Gets the step type.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="action">The action.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private StepType GetStepType( RockContext rockContext, WorkflowAction action, out string errorMessage )
        {
            errorMessage = string.Empty;
            var stepTypeIncludes = "StepProgram.StepStatuses";
            var stepTypeService = new StepTypeService( rockContext );
            StepType stepType = null;
            var stepTypeValue = GetLavaAttributeValue( action, AttributeKey.StepProgramStepType );

            // Check if the value is a guid. This method works for stepProgram|stepType or simply just step type guids
            StepProgramStepTypeFieldType.ParseDelimitedGuids( stepTypeValue, out var unused1, out var stepTypeGuid );

            if ( stepTypeGuid.HasValue )
            {
                stepType = stepTypeService.Queryable( stepTypeIncludes ).AsNoTracking().FirstOrDefault( st => st.Guid == stepTypeGuid.Value );

                if ( stepType == null )
                {
                    errorMessage = $"The step type with the guid '{stepTypeGuid.Value}' was not found";
                    return null;
                }
            }

            // Try to get a step type with a step type id
            if ( stepType == null )
            {
                var stepTypeId = stepTypeValue.AsIntegerOrNull();

                if ( !stepTypeId.HasValue )
                {
                    errorMessage = "The step type identifier is required but was missing";
                    return null;
                }

                stepType = stepTypeService.Queryable( stepTypeIncludes ).AsNoTracking().FirstOrDefault( st => st.Id == stepTypeId.Value );

                if ( stepType == null )
                {
                    errorMessage = $"The step type with the id '{stepTypeId.Value}' was not found";
                    return null;
                }
            }

            if ( stepType.StepProgram == null )
            {
                errorMessage = $"The step type '{stepType.Name}' program is missing";
                return null;
            }

            if ( stepType.StepProgram.StepStatuses == null || !stepType.StepProgram.StepStatuses.Any() )
            {
                errorMessage = $"The step program '{stepType.StepProgram.Name}' does not have any statuses";
                return null;
            }

            return stepType;
        }

        /// <summary>
        /// Gets the step status.
        /// </summary>
        /// <param name="stepType">Type of the step.</param>
        /// <param name="action">The action.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private StepStatus GetStepStatus( StepType stepType, WorkflowAction action, out string errorMessage )
        {
            errorMessage = string.Empty;
            StepStatus stepStatus = null;
            var stepStatusValue = GetLavaAttributeValue( action, AttributeKey.StepProgramStepStatus );

            if ( stepType == null || stepType.StepProgram == null || stepType.StepProgram.StepStatuses == null )
            {
                errorMessage = "The step type is required to include the step program and statuses";
                return null;
            }

            // Check if the value is a guid. This method works for stepProgram|stepStatus or simply just step status guid
            StepProgramStepStatusFieldType.ParseDelimitedGuids( stepStatusValue, out var unused2, out var stepStatusGuid );

            if ( stepStatusGuid.HasValue )
            {
                stepStatus = stepType.StepProgram.StepStatuses.FirstOrDefault( ss => ss.Guid == stepStatusGuid.Value );

                if ( stepStatus == null )
                {
                    errorMessage = $"The step status with the guid '{stepStatusGuid.Value}' was not found in '{stepType.StepProgram.Name}'";
                    return null;
                }
            }

            // Try to get a step status with a step status id
            if ( stepStatus == null )
            {
                var stepStatusId = stepStatusValue.AsIntegerOrNull();

                if ( !stepStatusId.HasValue )
                {
                    errorMessage = "The step status identifier is required but was missing";
                    return null;
                }

                stepStatus = stepType.StepProgram.StepStatuses.FirstOrDefault( ss => ss.Id == stepStatusId.Value );

                if ( stepStatus == null )
                {
                    errorMessage = $"The step status with the id '{stepStatusId.Value}' was not found in '{stepType.StepProgram.Name}'";
                    return null;
                }
            }

            return stepStatus;
        }

        /// <summary>
        /// Logs the messages for exit.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private bool LogMessagesForExit( WorkflowAction action, List<string> errorMessages )
        {
            errorMessages.ForEach( m => action.AddLogEntry( m ) );
            return !errorMessages.Any();
        }

        /// <summary>
        /// Gets the lava attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private string GetLavaAttributeValue( WorkflowAction action, string attributeKey )
        {
            if ( _mergeFields == null )
            {
                _mergeFields = GetMergeFields( action );
            }

            var value = GetAttributeValue( action, attributeKey, true );
            return value.ResolveMergeFields( _mergeFields );
        }
        private Dictionary<string, object> _mergeFields = null;

        /// <summary>
        /// Sets the Guid of the created item as the Workflow value of the attribute specified by the attributeKey.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The <see cref="WorkflowAction"/>.</param>
        /// <param name="attributeKey">The key of the attribute.</param>
        /// <param name="entity">Any Rock entity.</param>
        /// <param name="rockContext">The DB context.</param>
        private void SetCreatedItemAttribute<T>( WorkflowAction action, string attributeKey, T entity, RockContext rockContext ) where T : Entity<T>, new()
        {
            // If request attribute was specified, requery the request and set the attribute's value
            Guid? attributeGuid = GetAttributeValue( action, attributeKey ).AsGuidOrNull();
            if ( attributeGuid.HasValue )
            {
                // Ensure the entity has been added to the database before setting the attribute value.
                var entityService = new Service<T>( rockContext );
                entity = entityService.Get( entity.Id );
                if ( entity != null )
                {
                    SetWorkflowAttributeValue( action, attributeGuid.Value, entity.Guid.ToString() );
                }
            }
        }

    }
}