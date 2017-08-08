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
        "The interaction component name. <span class='tip tip-lava'></span>", 
        true, "", "", 1, "ComponentName", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Component EntityId", "Component EntityId",
        "The interaction component entityId. <span class='tip tip-lava'></span>",
        false, "", "", 2, "ComponentEntityId", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    /*
    [WorkflowTextOrAttribute( "Target Url", "Target Url",
        "The url that the short link will redirect to. <span class='tip tip-lava'></span>",
        true, "", "", 2, "Url", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType" } )]
    [WorkflowAttribute( "Attribute", "The attribute to store the generated short link's url to.", 
        false, "", "", 3, "Attribute", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType" } )]
    [IntegerField( "Random Token Length", "The number of characters to use when generating a random unique token.", false, 7, "", 4 )]
    [BooleanField( "Allow Token Re-use", "If a short link already exists with the same token, should it be updated to the new URL? If this is not allowed, this action will fail due to existing short link.", true, "", 5, "Overwrite" )]
    */
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

            var interactionComponentService = new InteractionComponentService( rockContext );
            interactionComponentService.GetComponentByEntityId()

            // Get the token
            string token = GetAttributeValue( action, "Token", true ).ResolveMergeFields( mergeFields );
            if ( token.IsNullOrWhiteSpace() )
            {
                int tokenLen = GetAttributeValue( action, "RandomTokenLength" ).AsIntegerOrNull() ?? 7;
                token = service.GetUniqueToken( site.Id, tokenLen );
            } 

            // Get the target url
            string url = GetAttributeValue( action, "Url", true ).ResolveMergeFields( mergeFields );
            if ( url.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "A valid Target Url was not specified." );
                return false;
            }

            // Save the short link
            var link = service.GetByToken( token, site.Id );
            if ( link != null )
            {
                if ( !GetAttributeValue( action, "Overwrite" ).AsBoolean() )
                {
                    errorMessages.Add( string.Format( "The selected token ('{0}') already exists. Please specify a unique token, or configure action to allow token re-use.", token ) );
                    return false;
                }
                else
                {
                    link.Url = url;
                }
            }
            else
            {
                link = new PageShortLink();
                link.SiteId = site.Id;
                link.Token = token;
                link.Url = url;
                service.Add( link );
            }
            rockContext.SaveChanges();

            // Save the resulting short link url
            var attribute = AttributeCache.Read( GetAttributeValue( action, "Attribute" ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                string domain = new SiteService( rockContext ).GetDefaultDomainUri( site.Id ).ToString();
                string shortLink = domain.EnsureTrailingForwardslash() + token;

                SetWorkflowAttributeValue( action, attribute.Guid, shortLink );
                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, shortLink ) );
            }

            return true;
        }

    }
}