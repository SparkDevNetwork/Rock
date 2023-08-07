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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.Groups
{
    /// <summary>
    /// Adds a new sign-up opportunity to a provided project.
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Adds a new sign-up opportunity to a provided project." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Sign-Up Opportunity Add" )]

    #region Attributes

    [WorkflowAttribute( "Sign-Up Project",
        Description = "The sign-up project group that the opportunity should be added to.",
        Key = AttributeKey.SignUpProject,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupFieldType" },
        Order = 0 )]

    [WorkflowAttribute( "Location",
        Description = "The location for the opportunity.",
        Key = AttributeKey.Location,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.LocationFieldType" },
        Order = 1 )]

    [WorkflowAttribute( "Schedule",
        Description = "The schedule for the opportunity.",
        Key = AttributeKey.Schedule,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.ScheduleFieldType" },
        Order = 2 )]

    [WorkflowTextOrAttribute( "Minimum Capacity",
        "Attribute Value",
        Description = "The minimum capacity for the opportunity.",
        Key = AttributeKey.MinimumCapacity,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 3 )]

    [WorkflowTextOrAttribute( "Desired Capacity",
        "Attribute Value",
        Description = "The desired capacity for the opportunity.",
        Key = AttributeKey.DesiredCapacity,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 4 )]

    [WorkflowTextOrAttribute( "Maximum Capacity",
        "Attribute Value",
        Description = "The maximum capacity for the opportunity.",
        Key = AttributeKey.MaximumCapacity,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 5 )]

    [WorkflowTextOrAttribute( "Reminder Details",
        "Attribute Value",
        Description = "The reminder details for the opportunity. <span class='tip tip-lava'></span>",
        Key = AttributeKey.ReminderDetails,
        IsRequired = false,
        FieldTypeClassNames = new string[]
        {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.MemoFieldType",
            "Rock.Field.Types.HtmlFieldType",
            "Rock.Field.Types.StructuredContentEditorFieldType"
        },
        Order = 6 )]

    [WorkflowTextOrAttribute( "Confirmation Details",
        "Attribute Value",
        Description = "The confirmation details for the opportunity. <span class='tip tip-lava'></span>",
        Key = AttributeKey.ConfirmationDetails,
        IsRequired = false,
        FieldTypeClassNames = new string[]
        {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.MemoFieldType",
            "Rock.Field.Types.HtmlFieldType",
            "Rock.Field.Types.StructuredContentEditorFieldType"
        },
        Order = 7 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "A917A5D4-76D2-42ED-A2C3-7B72A2F0A12A" )]
    public class AddSignUpOpportunity : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string SignUpProject = "SignUpProject";
            public const string Location = "Location";
            public const string Schedule = "Schedule";
            public const string MinimumCapacity = "MinimumCapacity";
            public const string DesiredCapacity = "DesiredCapacity";
            public const string MaximumCapacity = "MaximumCapacity";
            public const string ReminderDetails = "ReminderDetails";
            public const string ConfirmationDetails = "ConfirmationDetails";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the sign-up project group.
            var groupGuid = GetAttributeValue( action, AttributeKey.SignUpProject, true ).AsGuidOrNull();
            if ( !groupGuid.HasValue )
            {
                errorMessages.Add( "Invalid sign-up project provided." );
                return false;
            }

            var group = new GroupService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( g => g.GroupType )
                .FirstOrDefault( g => g.Guid == groupGuid.Value );

            if ( group == null )
            {
                errorMessages.Add( "The sign-up project provided does not exist." );
                return false;
            }

            // Get the location.
            var locationGuid = GetAttributeValue( action, AttributeKey.Location, true ).AsGuidOrNull();
            if ( !locationGuid.HasValue )
            {
                errorMessages.Add( "Invalid location provided." );
                return false;
            }

            var location = new LocationService( rockContext ).GetNoTracking( locationGuid.Value );
            if ( location == null )
            {
                errorMessages.Add( "The location provided does not exist." );
                return false;
            }

            var groupLocationPickerMode = GroupType.GetGroupLocationPickerMode( location );
            if ( !group.GroupType.LocationSelectionMode.HasFlag( groupLocationPickerMode ) )
            {
                errorMessages.Add( $@"{group.GroupType.Name} projects do not allow ""{groupLocationPickerMode:G}"" locations." );
                return false;
            }

            // Get the schedule.
            var scheduleGuid = GetAttributeValue( action, AttributeKey.Schedule, true ).AsGuidOrNull();
            if ( !scheduleGuid.HasValue )
            {
                errorMessages.Add( "Invalid schedule provided." );
                return false;
            }

            var schedule = new ScheduleService( rockContext ).Get( scheduleGuid.Value );
            if ( schedule == null )
            {
                errorMessages.Add( "The schedule provided does not exist." );
                return false;
            }

            if ( !group.GroupType.AllowedScheduleTypes.HasFlag( schedule.ScheduleType ) )
            {
                errorMessages.Add( $@"{group.GroupType.Name} projects do not allow ""{schedule.ScheduleType:G}"" schedules." );
                return false;
            }

            /*
             * 06/09/2023 - JPH
             * 
             * For the first release of this feature, we are allowing at most "Custom" and/or "Named" schedule types.
             * 
             * Reason: Sign-Ups / short term serving projects - simplified UI (minus weekly schedule types).
             */
            if ( schedule.ScheduleType == ScheduleType.Weekly )
            {
                errorMessages.Add( $@"Sign-up projects do not allow ""{ScheduleType.Weekly:G}"" schedules." );
                return false;
            }

            var anyChangesSaved = false;

            // Create a GroupLocation record if one doesn't already exist.
            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocation = groupLocationService
                .Queryable()
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationScheduleConfigs )
                .FirstOrDefault( gl => gl.GroupId == group.Id && gl.LocationId == location.Id );

            if ( groupLocation == null )
            {
                groupLocation = new GroupLocation
                {
                    GroupId = group.Id,
                    LocationId = location.Id
                };

                groupLocationService.Add( groupLocation );

                // Initial save so we can use the new GroupLocation ID below.
                rockContext.SaveChanges();
                anyChangesSaved = true;

                action.AddLogEntry( "Added a new GroupLocation record." );
            }
            else
            {
                action.AddLogEntry( "GroupLocation already exists." );
            }

            // Create a GroupLocationSchedule record if one doesn't already exist.
            var shouldSaveChanges = false;
            if ( !groupLocation.Schedules.Any( s => s.Id == schedule.Id ) )
            {
                groupLocation.Schedules.Add( schedule );

                shouldSaveChanges = true;

                action.AddLogEntry( "Added a new GroupLocationSchedule record." );
            }
            else
            {
                action.AddLogEntry( "GroupLocationSchedule already exists." );
            }

            // Create a GroupLocationScheduleConfig record if one doesn't already exist.
            if ( !groupLocation.GroupLocationScheduleConfigs.Any( c => c.ScheduleId == schedule.Id ) )
            {
                // Get the merge fields
                var mergeFields = GetMergeFields( action );

                groupLocation.GroupLocationScheduleConfigs.Add( new GroupLocationScheduleConfig
                {
                    GroupLocationId = groupLocation.Id,
                    ScheduleId = schedule.Id,
                    MinimumCapacity = GetAttributeValue( action, AttributeKey.MinimumCapacity, true ).AsIntegerOrNull(),
                    DesiredCapacity = GetAttributeValue( action, AttributeKey.DesiredCapacity, true ).AsIntegerOrNull(),
                    MaximumCapacity = GetAttributeValue( action, AttributeKey.MaximumCapacity, true ).AsIntegerOrNull(),
                    ReminderAdditionalDetails = GetAttributeValue( action, AttributeKey.ReminderDetails, true ).ResolveMergeFields( mergeFields ),
                    ConfirmationAdditionalDetails = GetAttributeValue( action, AttributeKey.ConfirmationDetails, true ).ResolveMergeFields( mergeFields )
                } );

                shouldSaveChanges = true;

                action.AddLogEntry( "Added a new GroupLocationScheduleConfig record." );
            }
            else
            {
                action.AddLogEntry( "GroupLocationScheduleConfig already exists." );
            }

            if ( shouldSaveChanges )
            {
                rockContext.SaveChanges();
                anyChangesSaved = true;
            }

            if ( anyChangesSaved )
            {
                action.AddLogEntry( "Sign-Up Project opportunity created." );
            }
            else
            {
                action.AddLogEntry( "Sign-Up Project opportunity already exists; no changes have been made." );
            }

            return true;
        }
    }
}
