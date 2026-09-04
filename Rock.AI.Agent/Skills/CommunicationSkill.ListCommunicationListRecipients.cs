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
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the active members of a communication list with their resolved
    /// deliverability.
    /// </summary>
    /// <remarks>
    /// Only active members are returned, matching the recipients a communication
    /// sent to the list would resolve to. Access is authorized at the list level;
    /// once a caller can view the list they can page its members.
    /// </remarks>
    [Description( "Lists the active members of a communication list, one page at a time, along with the channels each member can actually be reached on." )]
    [AgentPurpose( "Retrieves who is on a communication list and whether they can receive email or SMS." )]
    [AgentToolPrerequisite( "Call LookupCommunicationLists to determine the communicationListIdKey." )]
    [AgentToolGuid( "8EF3A0ED-71FB-4533-96F8-F2766B4A1AC9" )]
    public AgentToolResult ListCommunicationListRecipients( string communicationListIdKey, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var group = helper.GetRequiredEntity<Model.Group>( communicationListIdKey, checkSecurity: true );

        if ( group == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupCommunicationLists function to determine the available communication lists." );
        }

        var communicationListGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ) ?? 0;

        if ( group.GroupTypeId != communicationListGroupTypeId )
        {
            return Error( "That group is not a communication list." )
                .WithInstructions( "Call the LookupCommunicationLists function to determine the available communication lists." );
        }

        // GetCommunicationListMembers resolves the same active membership a
        // communication to this list would target. No segments are applied here.
        var memberQuery = Rock.Model.Communication.GetCommunicationListMembers( rockContext, group.Id, SegmentCriteria.All, new List<int>() )
            .Include( gm => gm.Person )
            .OrderBy( gm => gm.Person.LastName )
            .ThenBy( gm => gm.Person.NickName )
            .ThenBy( gm => gm.Id );

        var page = helper.GetPaginatedItems( memberQuery, pageNumber );

        // Resolve which of the people on this page have an SMS enabled number in a
        // single query rather than per member.
        var personIds = page.Items.Select( gm => gm.PersonId ).ToList();
        var smsCapablePersonIds = new HashSet<int>( new PhoneNumberService( rockContext ).Queryable()
            .Where( pn => personIds.Contains( pn.PersonId ) && pn.IsMessagingEnabled )
            .Select( pn => pn.PersonId )
            .Distinct()
            .ToList() );

        var resultPage = page.WithItems( page.Items
            .Select( gm =>
            {
                // A member with no preference of their own falls back to the
                // person's own preference.
                var preference = gm.CommunicationPreference != CommunicationType.RecipientPreference
                    ? gm.CommunicationPreference
                    : gm.Person.CommunicationPreference;

                return new CommunicationListRecipientResult
                {
                    Id = gm.Id,
                    Guid = gm.Guid,
                    PersonId = gm.Person.Id,
                    PersonGuid = gm.Person.Guid,
                    Name = gm.Person.FullName,
                    Email = gm.Person.Email,
                    CommunicationPreference = preference.ToString(),
                    CanReceiveEmail = gm.Person.CanReceiveEmail( true ),
                    CanReceiveSms = smsCapablePersonIds.Contains( gm.Person.Id )
                };
            } )
            .ToList() );

        var historyPage = page.WithItems( page.Items
            .Select( gm => new CommunicationListRecipientResult
            {
                Id = gm.Id,
                Guid = gm.Guid,
                PersonId = gm.Person.Id,
                PersonGuid = gm.Person.Guid,
                Name = gm.Person.FullName
            } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
