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

using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
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

        #endregion
    }
}
