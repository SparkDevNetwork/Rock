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

using System;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill.Mediums;
using Rock.Communication;
using Rock.Data;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Centralized skill for drafting and sending communications (email and SMS) in Rock.
/// Provides LLM prompts for drafting messages and tool functions for sending them.
/// </summary>
[Description( "This skill helps author and send communications, and track their impact." )]
[AgentSkillGuid( "37DF3637-9775-4A89-9A77-BF6744232991" )]
[EntityTypeGuid( "F67D0B02-B59F-475F-A005-8F2A5CCCA91C" )]
internal sealed partial class CommunicationSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    public CommunicationSkill( ILogger<CommunicationSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Shared Helpers

    /// <summary>
    /// Gets the current person's default SMS phone number, if any.
    /// </summary>
    /// <returns></returns>
    private SystemPhoneNumberCache GetDefaultSmsPhoneNumber()
    {
        var currentPerson = AgentRequestContext.CurrentPerson;
        if ( currentPerson == null )
        {
            return null;
        }

        var prefs = PersonPreferenceCache.GetPersonPreferenceCollection( currentPerson );
        var savedId = prefs.GetValue( PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER ).AsIntegerOrNull();

        // If a saved default exists, use it—unless it's gone or inactive, then fall back.
        if ( savedId.HasValue && savedId.Value > 0 )
        {
            var saved = SystemPhoneNumberCache.Get( savedId.Value );
            if ( saved != null && saved.IsActive && saved.IsSmsEnabled )
            {
                return saved;
            }
        }

        // No valid saved default: pick the first active number assigned to this person.
        var aliasId = currentPerson.PrimaryAliasId;
        if ( !aliasId.HasValue )
        {
            return null;
        }

        var fallback = SystemPhoneNumberCache.All()
            .Where( spn =>
                spn.IsActive
                && spn.AssignedToPersonAliasId == aliasId.Value
                && spn.IsSmsEnabled
            )
            .OrderByDescending( spn => spn.Id )
            .FirstOrDefault();

        return fallback;
    }

    /// <summary>
    /// Returns the specified medium based on the communication type.
    /// </summary>
    /// <param name="communicationType">The communication type to build a medium for.</param>
    /// <param name="rockContext">The rock context passed through to mediums that need it.</param>
    /// <param name="fromNumberId">The system phone number id to use for SMS communications, if any.</param>
    /// <returns>An <see cref="IAgentCommunicationMedium"/> instance, or <c>null</c> if the medium is unsupported or has no active transport.</returns>
    private IAgentCommunicationMedium GetCommunicationMedium( AgentCommunicationType communicationType, RockContext rockContext, int? fromNumberId = null )
    {
        IAgentCommunicationMedium medium;

        if ( communicationType == AgentCommunicationType.Email )
        {
            if ( !MediumContainer.HasActiveEmailTransport() )
            {
                return null;
            }

            medium = new EmailMedium();
        }
        else if ( communicationType == AgentCommunicationType.Sms )
        {
            if ( !fromNumberId.HasValue )
            {
                return null;
            }

            if ( !MediumContainer.HasActiveSmsTransport() )
            {
                return null;
            }

            medium = new SmsMedium( fromNumberId.Value );
        }
        else if ( communicationType == AgentCommunicationType.Push )
        {
            if ( !MediumContainer.HasActivePushTransport() )
            {
                return null;
            }

            medium = new PushNotificationMedium( rockContext );
        }
        else
        {
            return null;
        }

        return medium;
    }

    /// <summary>
    /// Get the system phone number identifier to use when creating an SMS
    /// communication.
    /// </summary>
    /// <param name="helper">The tool helper used to record any errors encountered.</param>
    /// <param name="communicationType">The type of communication being processed.</param>
    /// <param name="fromNumberIdKey">The identifier key of the system phone number that was specified.</param>
    /// <returns>The integer identifier of a system phone number or <c>null</c> if it could not be determined.</returns>
    private int? GetFromNumberId( AgentToolHelper helper, AgentCommunicationType communicationType, string fromNumberIdKey )
    {
        if ( communicationType != AgentCommunicationType.Sms )
        {
            return null;
        }

        if ( fromNumberIdKey.IsNotNullOrWhiteSpace() )
        {
            var fromNumber = SystemPhoneNumberCache.Get( fromNumberIdKey, false );

            if ( fromNumber != null && fromNumber.IsActive && fromNumber.IsSmsEnabled )
            {
                return fromNumber.Id;
            }

            helper.AddError( "The provided fromNumberIdKey does not correspond to a valid active SMS-enabled system phone number." );
        }
        else
        {
            var fromNumberId = GetDefaultSmsPhoneNumber()?.Id;

            if ( fromNumberId.HasValue )
            {
                return fromNumberId.Value;
            }

            helper.AddError( "No valid default SMS 'from' number could be determined for the current person. Please provide a fromNumberIdKey." );
            helper.AddInstructions( "Call the LookupSystemPhoneNumbers function, and prompt the user to pick from the list." );
        }

        return null;
    }

    #endregion
}
