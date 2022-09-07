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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Get Group Type Attendance Information for Person.
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Get Group Type Attendance Information for Person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Get Group Type Attendance" )]

    #region Attributes

    [WorkflowAttribute(
        "Person",
        Description = "The attribute that contains the person to mark the requirement met.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Person,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute(
        "Group Type",
        Description = "The workflow attribute that contains the group type that should be used to check for attendance.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.GroupType,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupTypeFieldType" } )]

    [WorkflowAttribute(
        "Attended Boolean",
        Description = "The workflow attribute to store whether the person had attended the group.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.AttendedBoolean,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.BooleanFieldType" } )]

    [WorkflowAttribute(
        "Last Attended Date",
        Description = "The workflow attribute to store the last attended date.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.LastAttendedDate,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.DateTimeFieldType", "Rock.Field.Types.DateFieldType" } )]

    #endregion
    [Rock.SystemGuid.EntityTypeGuid( "1B87EC7B-E52D-4C82-833A-01951DAE2CAB" )]
    public class PersonGetGroupTypeAttendance : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string GroupType = "GroupType";
            public const string AttendedBoolean = "AttendedBoolean";
            public const string LastAttendedDate = "LastAttendedDate";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var person = GetPersonFromWorkflowAttribute( rockContext, action, errorMessages );

            var groupType = GetGroupTypeFromWorkflowAttribute( rockContext, action, errorMessages );

            if ( groupType == null || person == null )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            var attendedBooleanGuid = GetAttributeValue( action, AttributeKey.AttendedBoolean ).AsGuid();
            var lastAttendedDateGuid = GetAttributeValue( action, AttributeKey.LastAttendedDate ).AsGuid();
            var lastAttendedDate = new AttendanceService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group.GroupTypeId == groupType.Id &&
                    a.PersonAlias.PersonId == person.Id &&
                    a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .FirstOrDefault();

            var isAttended = lastAttendedDate != null;
            if ( !attendedBooleanGuid.IsEmpty() )
            {
                SetWorkflowAttributeValue( action, attendedBooleanGuid, isAttended.ToTrueFalse() );
            }
            else
            {
                action.AddLogEntry( "No Attended Boolean Attribute was found." );
            }

            if ( lastAttendedDate != null )
            {
                if ( !lastAttendedDateGuid.IsEmpty() )
                {
                    SetWorkflowAttributeValue( action, lastAttendedDateGuid, lastAttendedDate.StartDateTime.ToString() );
                }
                else
                {
                    action.AddLogEntry( $"No Last Attended Date Attribute was found." );
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the group type from workflow attribute.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private GroupType GetGroupTypeFromWorkflowAttribute( RockContext rockContext, WorkflowAction action, List<string> errorMessages )
        {
            GroupType groupType = null;

            var guidGroupTypeAttribute = GetAttributeValue( action, AttributeKey.GroupType ).AsGuidOrNull();

            if ( guidGroupTypeAttribute.HasValue )
            {
                var attributeGroupType = AttributeCache.Get( guidGroupTypeAttribute.Value, rockContext );
                if ( attributeGroupType != null )
                {
                    var groupTypeGuid = action.GetWorkflowAttributeValue( guidGroupTypeAttribute.Value ).AsGuidOrNull();

                    if ( groupTypeGuid.HasValue )
                    {
                        groupType = new GroupTypeService( rockContext ).Get( groupTypeGuid.Value );
                    }
                }
            }

            if ( groupType == null )
            {
                errorMessages.Add( "No group type was provided." );
            }


            return groupType;
        }

        /// <summary>
        /// Gets the person from workflow attribute.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private Person GetPersonFromWorkflowAttribute( RockContext rockContext, WorkflowAction action, List<string> errorMessages )
        {
            // Get the Person entity from Attribute.
            Person person = null;
            var guidPersonAttribute = GetAttributeValue( action, AttributeKey.Person ).AsGuidOrNull();
            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Get( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null )
                {
                    string attributePersonValue = action.GetWorkflowAttributeValue( guidPersonAttribute.Value );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        if ( attributePerson.FieldType.Class == typeof( Rock.Field.Types.PersonFieldType ).FullName )
                        {
                            Guid personAliasGuid = attributePersonValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( a => a.Guid.Equals( personAliasGuid ) )
                                    .Select( a => a.Person )
                                    .FirstOrDefault();
                            }
                        }
                    }
                }
            }

            if ( person == null )
            {
                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
            }

            return person;
        }
    }
}
