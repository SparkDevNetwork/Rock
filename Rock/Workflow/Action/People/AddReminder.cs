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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Adds a reminder to a person's list.
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Adds a reminder to a person's list of reminders." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reminder Add" )]

    #region Workflow Attributes

    [ReminderTypeField( "Reminder Type",
    Description = "The type of reminder to add.",
    Key = AttributeKey.ReminderType,
    IsRequired = true,
    Order = 1 )]

    [WorkflowAttribute( "Person to Remind",
    Description = "The person attribute that contains the person the reminder will be for.",
    Key = AttributeKey.Person,
    IsRequired = true,
    FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
    Order = 2 )]

    [WorkflowTextOrAttribute( "Reminder Entity Id or Guid",
    "Attribute Value",
    Description = "The id or guid of the entity to associate the reminder with.  Typically this would be the subject of the reminder.  <span class='tip tip-lava'></span>",
    IsRequired = true,
    DefaultValue = "",
    Category = "",
    Order = 3,
    Key = AttributeKey.ReminderEntityIdOrGuid )]

    [WorkflowTextOrAttribute( "Note",
    "Attribute Value",
    Description = "The text to add to the reminder.  <span class='tip tip-lava'></span>",
    IsRequired = true,
    DefaultValue = "",
    Category = "",
    Order = 4,
    Key = AttributeKey.Note,
    FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowTextOrAttribute( "Reminder Date",
    "Attribute Value",
    Description = "The date to set the reminder for.  <span class='tip tip-lava'></span>",
    IsRequired = true,
    Order = 5,
    Key = AttributeKey.ReminderDate,
    FieldTypeClassNames = new string[] { "Rock.Field.Types.DateFieldType", "Rock.Field.Types.DateTimeFieldType", "Rock.Field.Types.TextFieldType" } )]

    [WorkflowTextOrAttribute( "Repeat Every (days)",
    "Attribute Value",
    Description = "Setting this value (in days) will repeat the reminder that number of days after completion.",
    IsRequired = false,
    Order = 6,
    Key = AttributeKey.RepeatEvery,
    FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.TextFieldType" } )]

    [WorkflowTextOrAttribute( "Number of Times to Repeat",
    "Attribute Value",
    Description = "The number of times to repeat. Leave blank to repeat indefinitely.",
    IsRequired = false,
    Order = 7,
    Key = AttributeKey.NumberOfTimesToRepeat,
    FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.TextFieldType" } )]

    #endregion Workflow Attributes

    [Rock.SystemGuid.EntityTypeGuid( "3AC6CAF2-6827-44E2-B405-ED943C0CC580" )]
    public class ReminderAdd : ActionComponent
    {
        /// <summary>
        /// Keys for the attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string ReminderType = "ReminderType";
            public const string ReminderEntityIdOrGuid = "ReminderEntityIdOrGuid";
            public const string Note = "Note";
            public const string ReminderDate = "ReminderDate";
            public const string RepeatEvery = "RepeatEvery";
            public const string NumberOfTimesToRepeat = "NumberOfTimesToRepeat";
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var person = GetPersonFromActionAttribute( AttributeKey.Person, rockContext, action, errorMessages );
            if ( person == null )
            {
                errorMessages.Add( "No person was provided for the reminder." );
            }
            else if ( !person.PrimaryAliasId.HasValue )
            {
                errorMessages.Add( $"{person.FullName} does not have a primary alias identifier." );
            }

            ReminderTypeService reminderTypeService = new ReminderTypeService( rockContext );
            var reminderType = reminderTypeService.Get( GetAttributeValue( action, AttributeKey.ReminderType ).AsGuid() );

            if ( reminderType == null )
            {
                errorMessages.Add( $"{action.ActionType.Name} does not have a valid reminder type." );
            }

            var mergeFields = GetMergeFields( action );

            // Get the entity from the reminder.
            EntityTypeService entityTypeService = new EntityTypeService( rockContext );
            IEntity entityObject = null;
            string entityIdGuidString = GetAttributeValue( action, AttributeKey.ReminderEntityIdOrGuid, true ).ResolveMergeFields( mergeFields ).Trim();
            var entityId = entityIdGuidString.AsIntegerOrNull();
            if ( entityId.HasValue )
            {
                entityObject = entityTypeService.GetEntity( reminderType.EntityTypeId, entityId.Value );
            }
            else
            {
                var entityGuid = entityIdGuidString.AsGuidOrNull();
                if ( entityGuid.HasValue )
                {
                    entityObject = entityTypeService.GetEntity( reminderType.EntityTypeId, entityGuid.Value );
                }
            }

            if ( entityObject == null )
            {
                var value = GetActionAttributeValue( action, AttributeKey.ReminderEntityIdOrGuid );
                entityObject = action.GetEntityFromAttributeValue( value, rockContext );
            }

            if ( entityObject == null )
            {
                errorMessages.Add( string.Format( "Entity could not be found for selected value ('{0}')!", entityIdGuidString ) );
            }

            // Get the Reminder Date.
            DateTime? reminderDateTime;

            var reminderDate = GetAttributeValueFromWorkflowTextOrAttribute( action, AttributeKey.ReminderDate )
                .ResolveMergeFields( mergeFields );
            if ( reminderDate.IsNullOrWhiteSpace() )
            {
                // If no reminder date is specified, activate it immediately.
                reminderDateTime = RockDateTime.Now;
            }
            else
            {
                reminderDateTime = reminderDate.AsDateTime();

                if ( reminderDateTime == null )
                {
                    errorMessages.Add( $"Reminder Date value is not a valid date ('{reminderDate}')" );
                }
            }

            // If any parameters are invalid, log the error messages and fail the action.
            if ( errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            ReminderService reminderService = new ReminderService( rockContext );

            Reminder reminder = new Reminder
            {
                ReminderTypeId = reminderType.Id,
                PersonAliasId = person.PrimaryAliasId.Value
            };

            reminder.EntityId = entityObject.Id;

            reminder.Note = GetAttributeValueFromWorkflowTextOrAttribute( action, AttributeKey.Note ).ResolveMergeFields( mergeFields );
            reminder.ReminderDate = reminderDateTime.Value;
            reminder.RenewPeriodDays = GetAttributeValue( action, AttributeKey.RepeatEvery ).AsIntegerOrNull();
            reminder.RenewMaxCount = GetAttributeValue( action, AttributeKey.NumberOfTimesToRepeat ).AsIntegerOrNull();

            reminderService.Add( reminder );
            rockContext.SaveChanges();

            return true;
        }

        private Person GetPersonFromActionAttribute( string key, RockContext rockContext, WorkflowAction action, List<string> errorMessages )
        {
            string value = GetAttributeValue( action, key );
            Guid guidPersonAttribute = value.AsGuid();
            if ( guidPersonAttribute.IsEmpty() )
            {
                return null;
            }

            var attributePerson = AttributeCache.Get( guidPersonAttribute, rockContext );
            if ( attributePerson == null )
            {
                return null;
            }

            string attributePersonValue = action.GetWorkflowAttributeValue( guidPersonAttribute );
            if ( string.IsNullOrWhiteSpace( attributePersonValue ) )
            {
                return null;
            }

            if ( attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType" )
            {
                errorMessages.Add( $"The attribute used for {key} to provide the person was not of type 'Person'." );
                return null;
            }

            Guid personAliasGuid = attributePersonValue.AsGuid();
            if ( personAliasGuid.IsEmpty() )
            {
                errorMessages.Add( $"Person could not be found for selected value ('{guidPersonAttribute}')!" );
                return null;
            }

            return new PersonAliasService( rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Guid.Equals( personAliasGuid ) )
                .Select( a => a.Person ).FirstOrDefault();
        }
    }
}
