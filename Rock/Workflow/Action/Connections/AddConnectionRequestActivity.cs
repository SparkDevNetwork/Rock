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
    /// Adds a new connection request activity.
    /// </summary>
    [ActionCategory( "Connections" )]
    [Description( "Adds a new connection request activity." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Request Activity Add" )]

    [WorkflowAttribute( "Connection Request Attribute", "The attribute that contains the connection request to add an activity to.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]
    [WorkflowAttribute( "Connection Activity Type Attribute", "The attribute that contains the activity type to add.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.ConnectionActivityTypeFieldType" } )]
    [WorkflowTextOrAttribute( "Note", "Attribute Value", "The note or an attribute that contains the note for the new activity. <span class='tip tip-lava'></span>", false, "", "", 2, "Note",
        new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowAttribute( "Person Attribute", "An optional Person attribute that contains the person who is adding the activity.", false, "", "", 3, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]

    public class AddConnectionRequestActivity : ActionComponent
    {
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

            // Get the connection request
            ConnectionRequest request = null;
            Guid connectionRequestGuid = action.GetWorklowAttributeValue(GetAttributeValue( action, "ConnectionRequestAttribute" ).AsGuid()).AsGuid();
            request = new ConnectionRequestService( rockContext ).Get( connectionRequestGuid );
            if ( request == null )
            {
                errorMessages.Add( "Invalid Connection Request Attribute or Value!" );
                return false;
            }

            // Get the activity type
            ConnectionActivityType activityType = null;
            Guid activityTypeGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "ConnectionActivityTypeAttribute" ).AsGuid() ).AsGuid();
            activityType = new ConnectionActivityTypeService( rockContext ).Get( activityTypeGuid );
            if ( activityType == null )
            {
                errorMessages.Add( "Invalid Connection Activity Type Attribute or Value!" );
                return false;
            }

            // Get the note
            string noteValue = GetAttributeValue( action, "Note", true );
            string note = string.Empty;
            Guid? noteGuid = noteValue.AsGuidOrNull();
            if ( noteGuid.HasValue )
            {
                var attribute = AttributeCache.Get( noteGuid.Value, rockContext );
                if ( attribute != null )
                {
                    note  = action.GetWorklowAttributeValue( noteGuid.Value );
                }
            }
            else
            {
                note = noteValue;
            }

            // Get the connector
            int? personAliasId = null;
            Guid? personAttributeGuid = GetAttributeValue( action, "PersonAttribute" ).AsGuidOrNull();
            if ( personAttributeGuid.HasValue )
            {
                Guid? personAliasGuid = action.GetWorklowAttributeValue( personAttributeGuid.Value ).AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
                    if ( personAlias != null )
                    {
                        personAliasId = personAlias.Id;
                    }
                }
            }

            // Add the activity
            var activity = new ConnectionRequestActivity();
            activity.ConnectionRequestId = request.Id;
            activity.ConnectionActivityTypeId = activityType.Id;
            activity.ConnectionOpportunityId = request.ConnectionOpportunityId;
            activity.ConnectorPersonAliasId = personAliasId;
            activity.Note = note.ResolveMergeFields( mergeFields );
            new ConnectionRequestActivityService( rockContext ).Add( activity );
            rockContext.SaveChanges();

            return true;
        }
    }
}