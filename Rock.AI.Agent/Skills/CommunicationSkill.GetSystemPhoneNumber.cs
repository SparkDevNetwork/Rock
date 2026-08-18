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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single system phone number in full detail.
    /// </summary>
    /// <remarks>
    /// This is the partner Get for <see cref="LookupSystemPhoneNumbers"/>. The lookup
    /// returns a compact reference set without the unique identifier, so this tool is
    /// the only route to a system phone number's Guid. That matters because the places
    /// that reference a phone number, workflow action settings among them, store it as
    /// a Guid rather than an id.
    /// </remarks>
    [Description( "Gets one system phone number in full, including its unique identifier and its SMS configuration." )]
    [AgentPurpose( "Provides the complete detail of a system phone number, including the unique identifier that other configuration stores it by." )]
    [AgentToolPrerequisite( "Call LookupSystemPhoneNumbers to determine the systemPhoneNumberIdKey." )]
    [AgentToolGuid( "7B4E6C15-9D2A-4F83-A0E1-3C5B8D2F41A6" )]
    public AgentToolResult GetSystemPhoneNumber( string systemPhoneNumberIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var systemPhoneNumber = helper.GetRequiredEntity<SystemPhoneNumber>( systemPhoneNumberIdKey );

        if ( systemPhoneNumber == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( LookupSystemPhoneNumbers )} function to determine the available system phone numbers." );
        }

        var result = new SystemPhoneNumberResult
        {
            Id = systemPhoneNumber.Id,
            Guid = systemPhoneNumber.Guid,
            Name = systemPhoneNumber.Name,
            Description = systemPhoneNumber.Description,
            Number = systemPhoneNumber.Number,

            // Returned even though the lookup only surfaces active numbers. A caller
            // that arrived with a key from somewhere else should be told the number is
            // inactive rather than left to infer it.
            IsActive = systemPhoneNumber.IsActive,
            Order = systemPhoneNumber.Order,
            IsSmsEnabled = systemPhoneNumber.IsSmsEnabled,
            IsSmsForwardingEnabled = systemPhoneNumber.IsSmsForwardingEnabled,
            SuppressSmsOptInOutAutoReplies = systemPhoneNumber.SuppressSmsOptInOutAutoReplies,
            DisableSmsOptInOutTracking = systemPhoneNumber.DisableSmsOptInOutTracking,
            CreatedDateTime = systemPhoneNumber.CreatedDateTime,
            ModifiedDateTime = systemPhoneNumber.ModifiedDateTime,
            AttributeValues = systemPhoneNumber.GetAttributeValueResults( AgentRequestContext ).ToList()
        };

        PopulateReferences( result, systemPhoneNumber );

        // Success does not sanitize, so the per-attribute view check has to be made
        // here. Without it the result can carry attribute values the current person is
        // not authorized to see.
        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this system phone number." );
        }

        // A reference rather than the whole result, matching the other Get tools.
        // The detail answers the current turn; repeating it in history costs tokens
        // on every later message for something reachable again by key.
        return Success( result )
            .WithHistoryContent( new KeyNameResult( systemPhoneNumber.Id, systemPhoneNumber.Guid, systemPhoneNumber.Name ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Populates the related entities of the result as key and name references. Each
    /// reference is left null when the phone number does not configure it, which is the
    /// common case for all four.
    /// </summary>
    /// <param name="result">The result to be populated.</param>
    /// <param name="systemPhoneNumber">The system phone number being described.</param>
    private void PopulateReferences( SystemPhoneNumberResult result, SystemPhoneNumber systemPhoneNumber )
    {
        var rockContext = AgentRequestContext.RockContext;

        if ( systemPhoneNumber.AssignedToPersonAliasId.HasValue )
        {
            var person = new PersonAliasService( rockContext ).GetPerson( systemPhoneNumber.AssignedToPersonAliasId.Value );

            result.AssignedToPerson = PersonResult.NameOnly( person );
        }

        if ( systemPhoneNumber.SmsReceivedWorkflowTypeId.HasValue )
        {
            var workflowType = WorkflowTypeCache.Get( systemPhoneNumber.SmsReceivedWorkflowTypeId.Value, rockContext );

            if ( workflowType != null )
            {
                result.SmsReceivedWorkflowType = new KeyNameResult
                {
                    Id = workflowType.Id,
                    Guid = workflowType.Guid,
                    Name = workflowType.Name
                };
            }
        }

        // A group has no cache of its own, so it is read through its service.
        if ( systemPhoneNumber.SmsNotificationGroupId.HasValue )
        {
            var group = new GroupService( rockContext ).Get( systemPhoneNumber.SmsNotificationGroupId.Value );

            if ( group != null )
            {
                result.SmsNotificationGroup = new KeyNameResult
                {
                    Id = group.Id,
                    Guid = group.Guid,
                    Name = group.Name
                };
            }
        }

        if ( systemPhoneNumber.MobileApplicationSiteId.HasValue )
        {
            var site = SiteCache.Get( systemPhoneNumber.MobileApplicationSiteId.Value, rockContext );

            if ( site != null )
            {
                result.MobileApplicationSite = new KeyNameResult
                {
                    Id = site.Id,
                    Guid = site.Guid,
                    Name = site.Name
                };
            }
        }
    }

    #endregion
}
