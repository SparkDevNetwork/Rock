using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;

using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)
        [Description( "Lists people in the provided person's peer network." )]
        [AgentToolGuid( "39244A1E-57BF-476B-AF88-65EBC205F25D" )]
        public RockToolResult ListPeerNetworkForPerson( string personIdKey )
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
                return RockToolResult.NoData();
            }

            return RockToolResult.Success( results );
        }

        #endregion
    }
}
