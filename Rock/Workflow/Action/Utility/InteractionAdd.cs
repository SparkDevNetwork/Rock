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
    /// 
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Writes to the interactions table" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Write to Interactions" )]

    [WorkflowTextOrAttribute( "InteractionChannel Id or Guid", "Interaction Channel",
        "The interaction channel to use for writing to interactions. <span class='tip tip-lava'></span>",
        true, "", "", 0, "InteractionChannel", new string[] { "Rock.Field.Types.InteractionChannelFieldType" } )]
    [WorkflowTextOrAttribute( "Component Name", "Component Name",
        "The interaction component name. Either Component Name or EntityId must be specified. If only Component Name is specified, the component will be automatically created if it doesn't exist. <span class='tip tip-lava'></span>",
        true, "", "", 1, "ComponentName", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Component Entity Id", "Component Entity Id",
        "The interaction component entityId. This is optional. If Component EntityId is known, it will be used to determine the component. Otherwise, it can be looked up using Component Name. <span class='tip tip-lava'></span>",
        false, "", "", 2, "ComponentEntityId", new string[] { "Rock.Field.Types.IntegerFieldType" } )]
    [WorkflowAttribute( "Person Attribute", "The person for the interaction.", true, "", "", 3, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [TextField( "Operation", "The name of the operation.", true, "", "", 4, "Operation" )]
    [WorkflowTextOrAttribute( "Interaction Summary", "Interaction Summary",
        "The interaction summary. <span class='tip tip-lava'></span>",
        true, "", "", 5, "InteractionSummary", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" }, rows: 3 )]
    [WorkflowTextOrAttribute( "Interaction Data", "Interaction Data",
        "The interaction data. <span class='tip tip-lava'></span>",
        true, "", "", 6, "InteractionData", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" }, rows: 3 )]
    [WorkflowTextOrAttribute( "Interaction Entity Id", "Interaction Entity Id",
        "The interaction entityId. Optional. The EntityId that the Interaction record should be populated with.<span class='tip tip-lava'></span>",
        false, "", "", 7, "InteractionEntityId", new string[] { "Rock.Field.Types.IntegerFieldType" } )]
    public class InteractionAdd : ActionComponent
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

            // Get the merge fields
            var mergeFields = GetMergeFields( action );

            // Get the InteractionChannel
            string interactionChannelValue = GetAttributeValue( action, "InteractionChannel", true ).ResolveMergeFields( mergeFields );
            var interactionChannelService = new InteractionChannelService( rockContext );
            InteractionChannel interactionChannel = interactionChannelService.Get( interactionChannelValue.AsGuid() ) ?? interactionChannelService.Get( interactionChannelValue.AsInteger() );
            if ( interactionChannel == null )
            {
                errorMessages.Add( string.Format( "InteractionChannel could not be found for selected value ('{0}')!", interactionChannelValue ) );
                return false;
            }

            // Get the InteractionChannel using the supplied ComponentEntityId or ComponentName
            var componentEntityId = GetAttributeValue( action, "ComponentEntityId", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
            var componentName = GetAttributeValue( action, "ComponentName", true ).ResolveMergeFields( mergeFields );

            var interactionComponentService = new InteractionComponentService( rockContext );
            InteractionComponent interactionComponent = null;

            if ( componentEntityId.HasValue )
            {
                interactionComponent = interactionComponentService.GetComponentByEntityId( interactionChannel.Id, componentEntityId.Value, componentName );
            }
            else
            {
                if ( !string.IsNullOrEmpty( componentName ) )
                {
                    interactionComponent = interactionComponentService.GetComponentByComponentName( interactionChannel.Id, componentName );
                }
                else
                {
                    errorMessages.Add( "InteractionComponent requires that either ComponentEntityId or ComponentName is specified." );
                    return false;
                }
            }

            if ( interactionComponent == null )
            {
                // shouldn't happen
                errorMessages.Add( "Error getting InteractionComponent." );
                return false;
            }
            else if ( interactionComponent.Id == 0 )
            {
                // a new component got created, so save changes to get our Id
                rockContext.SaveChanges();
            }

            // Get Person
            int? personAliasId = null;
            Guid? personAliasGuid = GetAttributeValue( action, "PersonAttribute", true ).AsGuidOrNull();
            if ( personAliasGuid.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
                if ( personAlias != null )
                {
                    personAliasId = personAlias.Id;
                }
            }

            // Get Operation
            var operation = GetAttributeValue( action, "Operation", true ).ResolveMergeFields( mergeFields );

            // Get InteractionSummary
            var interactionSummary = GetAttributeValue( action, "InteractionSummary", true ).ResolveMergeFields( mergeFields );

            // Get InteractionData
            var interactionData = GetAttributeValue( action, "InteractionData", true ).ResolveMergeFields( mergeFields );

            var interactionEntityId = GetAttributeValue( action, "InteractionEntityId", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();

            // Write the interaction record
            var interaction = new InteractionService( rockContext ).AddInteraction( interactionComponent.Id, interactionEntityId, operation, interactionData, personAliasId, RockDateTime.Now, null, null, null, null, null, null );
            interaction.InteractionSummary = interactionSummary;
            rockContext.SaveChanges();

            return true;
        }

    }
}