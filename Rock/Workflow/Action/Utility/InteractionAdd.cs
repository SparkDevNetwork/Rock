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

    [WorkflowTextOrAttribute( "InteractionChannel Name, Id, or Guid", "Interaction Channel",
        "The interaction channel to use for writing to interactions. If a Name is entered here, the channel will be automatically created if it doesn't exist. <span class='tip tip-lava'></span>",
        true, "", "", 0, "InteractionChannel", new string[] { "Rock.Field.Types.InteractionChannelFieldType" } )]
    [WorkflowTextOrAttribute( "Component Name, Id, or Guid", "Component Name",
        "The interaction component Identifier. A value must be included here, or the Component Entity Id must be specified. If a Name is entered here, the component will be automatically created if it doesn't exist. <span class='tip tip-lava'></span>",
        true, "", "", 1, "ComponentName", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Component Entity Id", "Component Entity Id",
        "The interaction component entityId. This is optional. If Component EntityId is known, it will be used to determine the component. Otherwise, it can be looked up using Component Name. <span class='tip tip-lava'></span>",
        false, "", "", 2, "ComponentEntityId", new string[] { "Rock.Field.Types.IntegerFieldType" } )]
    [WorkflowTextOrAttribute( "Person Alias Id or Guid", "Person Attribute", "The person for the interaction. Use a Person Alias Id for best performance. <span class='tip tip-lava'></span>", 
        false, "", "", 3, "PersonAttribute", new string[] { "Rock.Field.Types.PersonFieldType" } )]
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

            // Get Interaction Channel
            string channelValue = GetAttributeValue( action, "InteractionChannel", true ).ResolveMergeFields( mergeFields );
            var interactionChannel = GetChannel( rockContext, channelValue );
            if ( interactionChannel == null )
            {
                errorMessages.Add( $"InteractionChannel could not be found for value of '{channelValue}'." );
                return false;
            }

            // Get Interaction Component
            var componentEntityId = GetAttributeValue( action, "ComponentEntityId", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
            var componentValue = GetAttributeValue( action, "ComponentName", true ).ResolveMergeFields( mergeFields );
            var interactionComponent = GetComponent( rockContext, interactionChannel, componentEntityId, componentValue );
            if ( interactionComponent == null )
            {
                errorMessages.Add( $"InteractionComponent could not be found for EntityId of '{componentEntityId}' or Value of '{componentValue}'." );
                return false;
            }

            // Get Person
            int? personAliasId = GetPersonAliasId( rockContext, GetAttributeValue( action, "PersonAttribute", true ).ResolveMergeFields( mergeFields ) );

            // Get Operation
            var operation = GetAttributeValue( action, "Operation", true ).ResolveMergeFields( mergeFields );

            // Get InteractionSummary
            var interactionSummary = GetAttributeValue( action, "InteractionSummary", true ).ResolveMergeFields( mergeFields );

            // Get InteractionData
            var interactionData = GetAttributeValue( action, "InteractionData", true ).ResolveMergeFields( mergeFields );

            // Get InteractionEntityId
            var interactionEntityId = GetAttributeValue( action, "InteractionEntityId", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();

            // Write the interaction record
            var interaction = new InteractionService( rockContext )
                .AddInteraction( interactionComponent.Id, interactionEntityId, operation, interactionData, personAliasId, RockDateTime.Now, null, null, null, null, null, null );
            interaction.InteractionSummary = interactionSummary;
            rockContext.SaveChanges();

            return true;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        private InteractionChannelCache GetChannel( RockContext rockContext, string identifier )
        {
            if ( identifier.IsNotNullOrWhiteSpace() )
            {
                // Find by Id
                int? id = identifier.AsIntegerOrNull();
                if ( id.HasValue )
                {
                    var channel = InteractionChannelCache.Get( id.Value );
                    if ( channel != null )
                    {
                        return channel;
                    }
                }

                // Find by Guid
                Guid? guid = identifier.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var channel = InteractionChannelCache.Get( guid.Value );
                    if ( channel != null )
                    {
                        return channel;
                    }
                }

                if ( !id.HasValue && !guid.HasValue )
                {
                    // Find by Name
                    int? interactionChannelId = new InteractionChannelService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( c => c.Name == identifier )
                        .Select( c => c.Id )
                        .Cast<int?>()
                        .FirstOrDefault();

                    if ( interactionChannelId != null )
                    {
                        return InteractionChannelCache.Get( interactionChannelId.Value );
                    }

                    // If still no match, and we have a name, create a new channel
                    using ( var newRockContext = new RockContext() )
                    {
                        InteractionChannel interactionChannel = new InteractionChannel();
                        interactionChannel.Name = identifier;
                        new InteractionChannelService( newRockContext ).Add( interactionChannel );
                        newRockContext.SaveChanges();
                        return InteractionChannelCache.Get( interactionChannel.Id );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        private InteractionComponentCache GetComponent( RockContext rockContext, InteractionChannelCache channel, int? entityId, string identifier )
        {
            if ( channel != null )
            {
                if ( entityId.HasValue )
                {
                    // Find by the Entity Id
                    int? interactionComponentId = new InteractionComponentService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( c => c.EntityId.HasValue && c.EntityId.Value == entityId.Value )
                        .Select( c => c.Id )
                        .Cast<int?>()
                        .FirstOrDefault();

                    if ( interactionComponentId != null )
                    {
                        return InteractionComponentCache.Get( interactionComponentId.Value );
                    }
                }

                if ( identifier.IsNotNullOrWhiteSpace() )
                {
                    // Find by Id
                    int? id = identifier.AsIntegerOrNull();
                    if ( id.HasValue )
                    {
                        var component = InteractionComponentCache.Get( id.Value );
                        if ( component != null && component.ChannelId == channel.Id )
                        {
                            return component;
                        }
                    }

                    // Find by Guid
                    Guid? guid = identifier.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        var component = InteractionComponentCache.Get( guid.Value );
                        if ( component != null && component.ChannelId == channel.Id )
                        {
                            return component;
                        }
                    }

                    if ( !id.HasValue && !guid.HasValue )
                    {
                        // Find by Name
                        int? interactionComponentId = new InteractionComponentService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Where( c => c.Name.Equals( identifier, StringComparison.OrdinalIgnoreCase ) )
                            .Select( c => c.Id )
                            .Cast<int?>()
                            .FirstOrDefault();

                        if ( interactionComponentId != null )
                        {
                            return InteractionComponentCache.Get( interactionComponentId.Value );
                        }

                        // If still no match, and we have a name, create a new channel
                        using ( var newRockContext = new RockContext() )
                        {
                            var interactionComponent = new InteractionComponent();
                            interactionComponent.Name = identifier;
                            interactionComponent.ChannelId = channel.Id;
                            new InteractionComponentService( newRockContext ).Add( interactionComponent );
                            newRockContext.SaveChanges();

                            return InteractionComponentCache.Get( interactionComponent.Id );
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the person alias identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        private int? GetPersonAliasId( RockContext rockContext, string identifier )
        {
            int? personAliasId = identifier.AsIntegerOrNull();
            if ( personAliasId.HasValue )
            {
                return personAliasId.Value;
            }

            Guid? personAliasGuid = identifier.AsGuidOrNull();
            if ( personAliasGuid.HasValue )
            {
                personAliasId = new PersonAliasService( rockContext ).GetId( personAliasGuid.Value );
                if ( personAliasId.HasValue )
                {
                    return personAliasId.Value;
                }
            }

            return null;
        }
    }
}