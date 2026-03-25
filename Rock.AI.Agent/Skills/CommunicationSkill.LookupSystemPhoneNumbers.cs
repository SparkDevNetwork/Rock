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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class CommunicationSkill
{
    #region Tool(s)

    [Description( "Looks up system phone numbers." )]
    [AgentToolGuid( "FD3F160F-ABCA-4A18-B69F-0E21D61B6874" )]
    public IAgentToolResult LookupSystemPhoneNumbers( bool? smsEnabled = null )
    {
        var spnResults = GetSystemPhoneNumbers( smsEnabled );

        // Trim down for history
        var trimmedSpns = spnResults.Select( spn => new KeyNameResult
        {
            Id = spn.Id,
            Name = spn.Name
        } );

        var historyKey = smsEnabled.HasValue ? $"system-phone-numbers-sms-{smsEnabled.Value}" : "system-phone-numbers-all";

        return Success( spnResults )
            .WithHistoryContent( trimmedSpns, historyKey );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the system phone numbers, optionally filtering to only SMS-enabled numbers.
    /// </summary>
    /// <param name="smsEnabled"></param>
    /// <returns></returns>
    private List<SystemPhoneNumberResult> GetSystemPhoneNumbers( bool? smsEnabled = null )
    {
        return SystemPhoneNumberCache.All( AgentRequestContext.RockContext )
            .Where( spn => spn.IsActive )
            .Where( spn => !smsEnabled.HasValue || spn.IsSmsEnabled == smsEnabled.Value )
            .Where( spn => spn.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( spn =>
            {
                var spnResult = new SystemPhoneNumberResult
                {
                    Id = spn.Id,
                    Name = spn.Name,
                    Description = spn.Description,
                    Number = spn.Number,
                    IsSmsEnabled = spn.IsSmsEnabled,
                };

                if ( spn.AssignedToPersonAliasId.HasValue )
                {
                    var person = new PersonAliasService( AgentRequestContext.RockContext ).GetPerson( spn.AssignedToPersonAliasId.Value );

                    spnResult.AssignedToPerson = PersonResult.NameOnly( person );
                }

                return spnResult;
            } )
            .ToList();
    }

    #endregion
}
