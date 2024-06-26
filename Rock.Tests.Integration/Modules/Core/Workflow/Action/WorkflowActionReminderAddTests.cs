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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Tests.Integration.TestData.Core;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action;

namespace Rock.Tests.Integration.Modules.Core.Workflow
{
    [TestClass]
    [TestCategory( "Core.Workflow.Actions" )]
    public class WorkflowActionReminderAddTests : DatabaseTestsBase
    {
        private const string _reminderAddWorkflowType1Guid = "63BB1CBD-BAFB-474E-9A0C-D32E19A6ADCB";
        private const string _reminderAddWorkflowType2Guid = "3C99124E-6D9E-4C9D-96B9-633420077832";
        private const string _reminderDateWorkflowAttributeKey = "ReminderDate1";
        private const string _reminderDateActivityAttributeKey = "ReminderDate2";
        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            CreateWorkflowTypeWithAddReminderAction( $"Reminder Add Test (WorkflowReminderDateAttribute)",
                _reminderAddWorkflowType1Guid.AsGuid(),
                reminderDateIsActivityAttribute: false );

            CreateWorkflowTypeWithAddReminderAction( $"Reminder Add Test (ActivityReminderDateAttribute)",
                _reminderAddWorkflowType2Guid.AsGuid(),
                reminderDateIsActivityAttribute: true );
        }

        [TestMethod]
        public void ReminderAdd_WithInvalidPersonToRemind_FailsWithErrorMessage()
        {
            var rockContext = new RockContext();

            var newWorkflow = CreateDefaultTestWorkflowInstance( _reminderAddWorkflowType1Guid.AsGuid(), null, null );

            SetWorkflowAttribute( newWorkflow, "CurrentPerson", "invalid_person_ref", rockContext );

            newWorkflow.SaveAttributeValues( rockContext );

            var messages = ExecuteTestWorkflow( newWorkflow, throwOnFailure: false );

            Assert.That.Contains( messages, "No person was provided for the reminder." );
        }

        [TestMethod]
        public void ReminderAdd_WithInvalidReminderDate_FailsWithErrorMessage()
        {
            var rockContext = new RockContext();

            var newWorkflow = CreateDefaultTestWorkflowInstance( _reminderAddWorkflowType1Guid.AsGuid(), null, null );

            SetWorkflowAttribute( newWorkflow, _reminderDateWorkflowAttributeKey, "invalid_date", rockContext );

            newWorkflow.SaveAttributeValues( rockContext );

            var messages = ExecuteTestWorkflow( newWorkflow, throwOnFailure: false );

            Assert.That.Contains( messages, "Reminder Date value is not a valid date ('invalid_date')" );
        }

        [TestMethod]
        public void ReminderAdd_WithUnspecifiedReminderDate_DefaultsToNow()
        {
            var rockContext = new RockContext();

            // Assign a unique note value to identify the reminder.
            var reminderCode = Guid.NewGuid().ToString();
            var note = $"Reminder Code: {reminderCode}";

            var now = RockDateTime.Now.Date;
            var newWorkflow = CreateDefaultTestWorkflowInstance( _reminderAddWorkflowType1Guid.AsGuid(), now, note );

            // Execute the Workflow
            var workflowService = new WorkflowService( rockContext );

            List<string> messages;
            var success = workflowService.Process( newWorkflow, out messages );

            Assert.That.IsTrue( success, "Workflow execution failed.\n" + messages.AsDelimited( "\n" ) );

            // Verify the Reminder details.
            var reminderService = new ReminderService( rockContext );
            var reminder = reminderService.Queryable().FirstOrDefault( r => r.Note == note );

            Assert.That.AreEqualDate( now, reminder.ReminderDate, "Reminder Date is invalid." );
        }

        [TestMethod]
        public void ReminderAdd_ReminderDateAsWorkflowAttribute_ReturnsDateFromWorkflowAttribute()
        {
            var rockContext = new RockContext();

            // Assign a unique note value to identify the reminder.
            var reminderCode = Guid.NewGuid().ToString();
            var note = $"Reminder Code: {reminderCode}";

            var reminderDate = RockDateTime.Now.AddDays( 7 );
            var newWorkflow = CreateDefaultTestWorkflowInstance( _reminderAddWorkflowType1Guid.AsGuid(), reminderDate, note );

            // Execute the Workflow
            var workflowService = new WorkflowService( rockContext );

            List<string> messages;
            var success = workflowService.Process( newWorkflow, out messages );

            Assert.That.IsTrue( success, "Workflow execution failed.\n" + messages.AsDelimited( "\n" ) );

            // Verify the Reminder details.
            var reminderService = new ReminderService( rockContext );
            var reminder = reminderService.Queryable().FirstOrDefault( r => r.Note == note );

            Assert.That.AreEqualDate( reminderDate, reminder.ReminderDate, "Reminder Date is invalid." );
        }

        [TestMethod]
        public void ReminderAdd_ReminderDateAsActivityAttribute_ReturnsDateFromActivityAttribute()
        {
            var rockContext = new RockContext();

            var reminderCode = Guid.NewGuid().ToString();
            var note = $"Reminder Code: {reminderCode}";
            var reminderDate = RockDateTime.Now.AddDays( 7 );

            var newWorkflow = CreateDefaultTestWorkflowInstance( _reminderAddWorkflowType2Guid.AsGuid(), reminderDate, note );

            // Execute the Workflow
            var workflowService = new WorkflowService( rockContext );

            List<string> messages;
            var success = workflowService.Process( newWorkflow, out messages );

            Assert.That.IsTrue( success, "Workflow execution failed.\n" + messages.AsDelimited( "\n" ) );

            // Verify the Reminder details.
            var reminderService = new ReminderService( rockContext );
            var reminder = reminderService.Queryable().FirstOrDefault( r => r.Note == note );

            Assert.That.AreEqualDate( reminderDate, reminder.ReminderDate, "Reminder Date is invalid." );
        }

        /// <summary>
        /// Execute a workflow instance and return the output messages.
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="throwOnFailure"></param>
        /// <returns></returns>
        private List<string> ExecuteTestWorkflow( Rock.Model.Workflow workflow, bool throwOnFailure )
        {
            // Execute the Workflow
            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            List<string> messages;
            var success = workflowService.Process( workflow, out messages );

            if ( !success && throwOnFailure )
            {
                Assert.That.Fail( "Workflow execution failed.\n" + messages.AsDelimited( "\n" ) );
            }

            return messages;
        }

        /// <summary>
        /// Create an instance of the specified test Workflow Type.
        /// </summary>
        /// <param name="workflowTypeGuid"></param>
        /// <param name="reminderDate"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        private Rock.Model.Workflow CreateDefaultTestWorkflowInstance( Guid workflowTypeGuid, DateTime? reminderDate, string note )
        {
            var rockContext = new RockContext();

            // Create a new Workflow instance and set its attributes.
            var workflowTypeCache = WorkflowTypeCache.Get( workflowTypeGuid );

            var newWorkflow = Rock.Model.Workflow.Activate( workflowTypeCache, "New Test", rockContext );

            rockContext.SaveChanges();

            newWorkflow.LoadAttributes( rockContext );

            SetWorkflowPersonAttribute( newWorkflow, "CurrentPerson", TestGuids.TestPeople.TedDecker.AsGuid(), rockContext );
            SetWorkflowPersonAttribute( newWorkflow, "TargetPerson", TestGuids.TestPeople.BillMarble.AsGuid(), rockContext );
            SetWorkflowAttribute( newWorkflow, "ReminderText", note, rockContext );

            if ( workflowTypeGuid == _reminderAddWorkflowType1Guid.AsGuid() )
            {
                SetWorkflowAttribute( newWorkflow, _reminderDateWorkflowAttributeKey, reminderDate, rockContext );
            }

            newWorkflow.SaveAttributeValues( rockContext );

            var reminderAddActionEntityType = EntityTypeCache.Get( typeof( ReminderAdd ) );

            // Get the Start Activity and set its attributes.
            var startActivityType = workflowTypeCache.ActivityTypes.FirstOrDefault( a => a.Name == "Start" );
            var reminderAddActionType = startActivityType.ActionTypes.FirstOrDefault( a => a.WorkflowAction.EntityType.Id == reminderAddActionEntityType.Id );
            var startActivity = newWorkflow.Activities.FirstOrDefault( a => a.ActivityTypeId == startActivityType.Id );

            startActivity.LoadAttributes( rockContext );

            if ( workflowTypeGuid == _reminderAddWorkflowType2Guid.AsGuid() )
            {
                startActivity.SetAttributeValue( _reminderDateActivityAttributeKey, reminderDate );
            }

            startActivity.SaveAttributeValues( rockContext );

            return newWorkflow;
        }

        /// <summary>
        /// Create a Workflow Type suitable for testing the Reminder Add action.
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="workflowGuid"></param>
        /// <param name="reminderDateIsActivityAttribute"></param>
        private static void CreateWorkflowTypeWithAddReminderAction( string workflowName, Guid workflowGuid, bool reminderDateIsActivityAttribute )
        {
            var rockContext = new RockContext();

            //
            // Create New Workflow Type.
            //
            var workflowType = new Rock.Model.WorkflowType
            {
                Guid = workflowGuid,
                Name = workflowName,
                IsActive = true,
                IsPersisted = true,
                WorkTerm = "Reminder Add Test",
                CategoryId = CategoryCache.GetId( SystemGuid.Category.WORKFLOW_TYPE_SAMPLES.AsGuid() )
            };

            var workflowService = new WorkflowTypeService( rockContext );
            workflowService.Add( workflowType );

            rockContext.SaveChanges();

            // Add Workflow Attributes
            var workflowEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.Workflow ) ).ToString();
            var workflowTypeId = workflowType.Id.ToString();

            var _reminderAddCurrentPersonAttributeGuid = Guid.NewGuid();
            var _reminderAddTargetPersonAttributeGuid = Guid.NewGuid();
            var _reminderAddNoteGuid = Guid.NewGuid();

            var workflowAttributes = new List<AddEntityAttributeArgs>()
            {
                 AddEntityAttributeArgs.New("CurrentPerson", SystemGuid.FieldType.PERSON, workflowEntityTypeId )
                    .WithIdentifier( _reminderAddCurrentPersonAttributeGuid )
                    .WithEntityTypeQualifier("WorkflowTypeId", workflowTypeId ),
                 AddEntityAttributeArgs.New("TargetPerson", SystemGuid.FieldType.PERSON, workflowEntityTypeId )
                    .WithIdentifier( _reminderAddTargetPersonAttributeGuid )
                    .WithEntityTypeQualifier("WorkflowTypeId", workflowTypeId ),
                 AddEntityAttributeArgs.New("ReminderText", SystemGuid.FieldType.TEXT, workflowEntityTypeId )
                    .WithIdentifier( _reminderAddNoteGuid )
                    .WithEntityTypeQualifier("WorkflowTypeId", workflowTypeId ),
            };

            var reminderAddDateGuid = Guid.NewGuid();

            if ( !reminderDateIsActivityAttribute )
            {
                workflowAttributes.Add( AddEntityAttributeArgs.New( _reminderDateWorkflowAttributeKey, SystemGuid.FieldType.DATE, workflowEntityTypeId )
                   .WithIdentifier( reminderAddDateGuid )
                   .WithEntityTypeQualifier( "WorkflowTypeId", workflowTypeId ) );
            }

            EntityAttributeDataManager.Instance.AddEntityAttributes( workflowAttributes, rockContext );

            //
            // Add Activity: Start
            //
            var startActivityType = new Rock.Model.WorkflowActivityType
            {
                Name = "Start",
                IsActivatedWithWorkflow = true,
            };

            workflowType.ActivityTypes.Add( startActivityType );

            rockContext.SaveChanges();

            var activityTypeId = startActivityType.Id.ToString();

            // Add Activity Attributes
            if ( reminderDateIsActivityAttribute )
            {
                var activityEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.WorkflowActivity ) ).ToString();

                var activityAttributes = new List<AddEntityAttributeArgs>()
                {
                     AddEntityAttributeArgs.New( _reminderDateActivityAttributeKey, SystemGuid.FieldType.DATE, activityEntityTypeId )
                        .WithIdentifier( reminderAddDateGuid )
                        .WithEntityTypeQualifier( "ActivityTypeId", activityTypeId )
                };

                EntityAttributeDataManager.Instance.AddEntityAttributes( activityAttributes, rockContext );

                rockContext.SaveChanges();
            }

            //
            // Add Action: Reminder Add
            //
            var actionComponentEntityTypeId = EntityTypeCache.GetId( typeof( ReminderAdd ) ).GetValueOrDefault();

            var actionType = new WorkflowActionType()
            {
                Name = "Add Reminder",
                IsActionCompletedOnSuccess = true,
                EntityTypeId = actionComponentEntityTypeId
            };

            startActivityType.ActionTypes.Add( actionType );

            rockContext.SaveChanges();

            var actionTypeId = actionType.Id.ToString();

            // Reminder Add Action: Set Attribute Values
            var actionEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.WorkflowActionType ) ).Value;

            var actionAttributes = new List<AddEntityAttributeArgs>();

            CreateAttributesForWorkflowAction( typeof( ReminderAdd ), rockContext );

            actionType.LoadAttributes( rockContext );

            // Create a personal reminder.
            var reminderTypes = GetListOfValuesForFieldType( typeof( ReminderTypeFieldType ) );

            var reminderType = reminderTypes.Where( r => r.Value == "Personal Reminder" )
                .Select( r => r.Key )
                .FirstOrDefault();

            // Set these Attributes to reference the Workflow Attributes that will be supplied when
            // an instance of this workflow is created.
            actionType.SetAttributeValue( "Person", _reminderAddCurrentPersonAttributeGuid );
            actionType.SetAttributeValue( "ReminderEntityIdOrGuid", _reminderAddTargetPersonAttributeGuid );
            actionType.SetAttributeValue( "ReminderDate", reminderAddDateGuid );
            actionType.SetAttributeValue( "Note", _reminderAddNoteGuid );

            actionType.SetAttributeValue( "ReminderType", reminderType );
            actionType.SetAttributeValue( "RepeatEvery", "30" );
            actionType.SetAttributeValue( "NumberOfTimesToRepeat", "3" );

            actionType.SaveAttributeValues( rockContext );

            rockContext.SaveChanges();
        }

        #region Helper Methods

        /// <summary>
        /// Creates the set of Attributes needed for a WorkflowAction based on the Workflow Action Type.
        /// </summary>
        /// <param name="workflowActionType"></param>
        /// <param name="rockContext"></param>
        private static void CreateAttributesForWorkflowAction( System.Type workflowActionType, RockContext rockContext )
        {
            var actionComponentEntityTypeId = EntityTypeCache.GetId( workflowActionType ).GetValueOrDefault();

            Rock.Attribute.Helper.UpdateAttributes( workflowActionType, EntityTypeCache.GetId( workflowActionType.FullName ), rockContext );

            // Get an instance of the component that executes the Workflow Action.
            var actionComponent = ActionContainer.GetComponent( typeof( ReminderAdd ).FullName );

            // Set Action Attribute Values.
            var actionEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.WorkflowActionType ) ).Value;

            var attributes = AttributeCache.AllForEntityType( actionComponentEntityTypeId )
                .Where( a => a.EntityTypeId == actionComponentEntityTypeId )
                .ToList();

            // Copy the Attributes defined for the ActionComponent to this ActionType.
            var actionAttributes = new List<AddEntityAttributeArgs>();
            foreach ( var attribute in attributes )
            {
                var args = AddEntityAttributeArgs.New( attribute.Key, attribute.FieldTypeId.ToString(), actionEntityTypeId.ToString() ) // attribute.EntityTypeId.ToString() )
                    .WithEntityTypeQualifier( "EntityTypeId", actionComponentEntityTypeId.ToString() );

                actionAttributes.Add( args );
            }

            EntityAttributeDataManager.Instance.AddEntityAttributes( actionAttributes, rockContext );

            rockContext.SaveChanges();
        }

        // Get the list of values associated with a Field Type.
        private static Dictionary<string, string> GetListOfValuesForFieldType( System.Type fieldType )
        {
            var fieldTypeInstance = Activator.CreateInstance( fieldType ) as Rock.Field.IFieldType;

            var configValues = fieldTypeInstance.GetPublicConfigurationValues( new Dictionary<string, string>(), Rock.Field.ConfigurationValueUsage.Edit, null );

            var listValues = new Dictionary<string, string>();

            if ( configValues.ContainsKey( "values" ) )
            {
                var valuesList = JsonConvert.DeserializeObject<IEnumerable<JObject>>( configValues["values"] );

                var firstValue = valuesList?.FirstOrDefault();
                if ( firstValue != null
                     && firstValue.ContainsKey( "guid" )
                     && firstValue.ContainsKey( "name" ) )
                {
                    foreach ( var value in valuesList )
                    {
                        var guid = value.GetValue( "guid" ).Value<string>();
                        var name = value.GetValue( "name" ).Value<string>();
                        listValues.Add( guid, name );
                    }

                    return listValues;
                }
            }

            return new Dictionary<string, string>();
        }

        private static void SetWorkflowPersonAttribute( Rock.Model.Workflow workflow, string attributeKey, Guid personGuid, RockContext rockContext )
        {
            // A reference to a Person is stored as the PersonAlias identifier.
            var aliasService = new PersonAliasService( rockContext );
            var primaryAliasGuid = aliasService.GetPrimaryAliasGuid( personGuid );

            SetWorkflowAttribute( workflow, attributeKey, primaryAliasGuid.ToString(), rockContext );
        }

        private static void SetWorkflowAttribute( Rock.Model.Workflow workflow, string attributeKey, object value, RockContext rockContext )
        {
            // Load the Attributes if they do not exist.
            if ( workflow.Attributes == null )
            {
                workflow.LoadAttributes( rockContext );
            }

            SetWorkflowAttribute( workflow, attributeKey, value );
        }

        private static void SetWorkflowAttribute( IHasAttributes entity, string attributeKey, object value )
        {
            if ( value is DateTime dtValue )
            {
                entity.SetAttributeValue( attributeKey, dtValue );
            }
            else if ( value is Guid guidValue )
            {
                entity.SetAttributeValue( attributeKey, guidValue );
            }
            else if ( value is decimal decimalValue )
            {
                entity.SetAttributeValue( attributeKey, decimalValue );
            }
            else if ( value is int intValue )
            {
                entity.SetAttributeValue( attributeKey, intValue );
            }
            else if ( value == null )
            {
                entity.SetAttributeValue( attributeKey, null );
            }
            else
            {
                entity.SetAttributeValue( attributeKey, value.ToString() );
            }
        }

        #endregion
    }
}