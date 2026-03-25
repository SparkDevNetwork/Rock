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
using System.Linq.Dynamic.Core;

using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Lists people in the provided person's peer network." )]
    [AgentToolGuid( "39244A1E-57BF-476B-AF88-65EBC205F25D" )]
    public IAgentToolResult ListPeerNetworkForPerson( string personIdKey )
    {
        var rockContext = AgentRequestContext.RockContext;

        var peerNetworkService = rockContext.Set<PeerNetwork>();
        var personId = IdHasher.Instance.GetId( personIdKey );

        var results = peerNetworkService
            .Where( pn => pn.SourcePersonId == personId )
            .Join(
                rockContext.Set<Rock.Model.Person>(),
                pn => pn.TargetPersonId,
                tp => tp.Id,
                ( pn, tp ) => new { pn, tp }
            )
            .GroupBy( x => new { x.tp.NickName, x.tp.LastName, x.tp.Id } )
            .Select( g => new
            {
                TargetName = g.Key.NickName + " " + g.Key.LastName,
                TargetPersonId = g.Key.Id,
                RelationshipScore = ( int ) Math.Round( g.Sum( x => x.pn.RelationshipScore ), 0 ),
                PointDifference = g.Sum( x => x.pn.RelationshipScore ) - g.Sum( x => x.pn.RelationshipScoreLastUpdateValue )
            } )
            .OrderByDescending( x => x.RelationshipScore )
            .ThenBy( x => x.TargetName.Split( ' ' )[1] ) // LastName
            .ThenBy( x => x.TargetName.Split( ' ' )[0] ) // NickName
            .ToList();

        if ( !results.Any() )
        {
            return NoData();
        }

        return Success( results );
    }

    #endregion
}
