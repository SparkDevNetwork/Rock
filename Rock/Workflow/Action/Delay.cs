// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    [Description( "Delays successful execution of action until a specified number of minutes have passed" )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Delay" )]

    [IntegerField( "Minutes To Delay", "The number of minuts to delay successful execution of action" )]
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

            // Get the number of minutes to delay
            int? minutes = GetAttributeValue( action, "MinutesToDelay" ).AsIntegerOrNull();
            if ( !minutes.HasValue || minutes.Value <= 0 )
            {
                return true;
            }

            // Use the current action type' guid as the key for a 'Delay Activated' attribute 
            string AttrKey = action.ActionType.Guid.ToString();

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
                var newRockContext = new RockContext();
                new AttributeService( newRockContext ).Add( attribute );
                newRockContext.SaveChanges();

                action.Activity.Attributes.Add( AttrKey, AttributeCache.Read( attribute ) );
                var attributeValue = new Rock.Model.AttributeValue();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.Value = RockDateTime.Now.ToString( "o" );
                action.Activity.AttributeValues.Add( AttrKey, attributeValue );

                action.AddLogEntry( string.Format( "{0:N0} Minute Delay Activated.", minutes.Value ), true );
            }
            else
            {
                // Check to see if this action instance has a value for the 'Delay Activated' attrbute
                DateTime? activated = action.Activity.GetAttributeValue( AttrKey ).AsDateTime();
                if ( !activated.HasValue )
                {
                    // If no value exists, set the value to the current time
                    action.Activity.SetAttributeValue( AttrKey, RockDateTime.Now.ToString( "o" ) );
                    action.AddLogEntry( string.Format( "{0:N0} Minute Delay Activated.", minutes.Value ), true );
                }
                else
                {
                    // If a value does exist, check to see if the number of minutes to delay has passed
                    // since the value was saved
                    if ( activated.Value.AddMinutes( minutes.Value ).CompareTo( RockDateTime.Now ) < 0 )
                    {

                        // If delay has elapsed, return True ( so that processing of activity will continue )
                        action.AddLogEntry( string.Format( "{0:N0} Minute Delay Completed.", minutes.Value ), true );
                        return true;
                    }
                }
            }

            // If delay has not elapsed, return false so that processing of activity stops
            return false;

        }
    }
}
