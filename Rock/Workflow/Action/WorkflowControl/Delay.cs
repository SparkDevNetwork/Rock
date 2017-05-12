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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Delays successful execution of action until a specified number of minutes have passed
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Delays successful execution of action until a specified number of minutes have passed" )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Delay" )]

    [IntegerField( "Minutes To Delay", "The number of minutes to delay successful execution of action", false, order: 0 )]
    [WorkflowAttribute( "Date In Attribute", "The date or date/time attribute value to use for the delay.", false, "", "", 1, null, new string[] { "Rock.Field.Types.DateFieldType", "Rock.Field.Types.DateTimeFieldType" } )]
    [CustomDropdownListField("Next Weekday", "The next day of the week to wait till.", "0^Sunday,1^Monday,2^Tuesday,3^Wednesday,4^Thursday,5^Friday,6^Saturday,7^Sunday", false, order: 2)]
    class Delay : Rock.Workflow.ActionComponent
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var now = RockDateTime.Now;

            // Save/Get the datetime this action was first activated
            var activatedDateTime = GetDateTimeActivated( action );

            // First check if number of minutes to delay
            int? minutes = GetAttributeValue( action, "MinutesToDelay" ).AsIntegerOrNull();
            if ( minutes.HasValue )
            {
                if ( activatedDateTime.AddMinutes( minutes.Value ).CompareTo( now ) <= 0 )
                {
                    action.AddLogEntry( string.Format( "{0:N0} Minute Delay Completed.", minutes.Value ), true );
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Then check for specific date/time
            Guid dateAttributeGuid = GetAttributeValue( action, "DateInAttribute" ).AsGuid();
            if ( !dateAttributeGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Read( dateAttributeGuid, rockContext );
                if ( attribute != null )
                {
                    DateTime? attributeDate = action.GetWorklowAttributeValue( dateAttributeGuid ).AsDateTime();
                    if ( attributeDate.HasValue )
                    {
                        if ( attributeDate.Value.CompareTo( now ) <= 0 )
                        {
                            action.AddLogEntry( string.Format( "Delay until {0} Completed.", attributeDate.Value ), true );
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            // Finally check weekday
            int? dayOfTheWeek = GetAttributeValue( action, "NextWeekday" ).AsIntegerOrNull();
            if ( dayOfTheWeek.HasValue )
            {
                if ( (int)now.DayOfWeek == dayOfTheWeek.Value )
                { 
                    action.AddLogEntry( string.Format( "Delay until {0} Completed.", now.DayOfWeek.ConvertToString() ), true );
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // If delay has not elapsed, return false so that processing of activity stops
            return false;
        }

        private DateTime GetDateTimeActivated( WorkflowAction action )
        {
            var dateActivated = RockDateTime.Now;

            // Use the current action type' guid as the key for a 'Delay Activated' attribute 
            string AttrKey = action.ActionTypeCache.Guid.ToString();

            // Check to see if the action's activity does not yet have the the 'Delay Activated' attribute.
            // The first time this action runs on any workflow instance using this action instance, the 
            // attribute will not exist and need to be created
            if ( !action.Activity.Attributes.ContainsKey( AttrKey ) )
            {
                var attribute = new Rock.Model.Attribute();
                attribute.EntityTypeId = action.Activity.TypeId;
                attribute.EntityTypeQualifierColumn = "ActivityTypeId";
                attribute.EntityTypeQualifierValue = action.Activity.ActivityTypeId.ToString();
                attribute.Name = "Delay Activated";
                attribute.Key = AttrKey;
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT.AsGuid() ).Id;

                // Need to save the attribute now (using different context) so that an attribute id is returned.
                using ( var newRockContext = new RockContext() )
                {
                    new AttributeService( newRockContext ).Add( attribute );
                    newRockContext.SaveChanges();
                    AttributeCache.FlushEntityAttributes();
                    WorkflowActivityTypeCache.Flush( action.Activity.ActivityTypeId );
                }

                action.Activity.Attributes.Add( AttrKey, AttributeCache.Read( attribute ) );
                var attributeValue = new AttributeValueCache();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.Value = dateActivated.ToString( "o" );
                action.Activity.AttributeValues.Add( AttrKey, attributeValue );

                action.AddLogEntry( string.Format( "Delay Activated at {0}", dateActivated), true );
            }
            else
            {
                // Check to see if this action instance has a value for the 'Delay Activated' attrbute
                DateTime? activated = action.Activity.GetAttributeValue( AttrKey ).AsDateTime();
                if ( activated.HasValue )
                {
                    return activated.Value;
                }
                else
                { 
                    // If no value exists, set the value to the current time
                    action.Activity.SetAttributeValue( AttrKey, dateActivated.ToString( "o" ) );
                    action.AddLogEntry( string.Format( "Delay Activated at {0}", dateActivated ), true );
                }
            }

            return dateActivated;
        }
    }
}
