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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Humanizer;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that calculates Rock's peer networks for individuals.
    /// </summary>
    [DisplayName( "Calculate Peer Network" )]
    [Description( "Job that calculates Rock's peer networks for individuals." )]

    [BooleanField( "Calculate Peer Network for Following",
        Key = AttributeKey.CalculatePeerNetworkForFollowing,
        Description = "Determines if peer networks should be calculated for followed individuals.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 0 )]

    [BooleanField( "Calculate Peer Network for Groups",
        Key = AttributeKey.CalculatePeerNetworkForGroups,
        Description = "Determines if peer networks should be calculated for individuals in groups.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 1 )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60,
        Category = "General",
        Order = 2 )]

    [RockLoggingCategory]
    public class CalculatePeerNetwork : RockJob
    {
        /// <summary>
        /// Attribute Keys for the <see cref="CalculatePeerNetwork"/> job.
        /// </summary>
        private static class AttributeKey
        {
            public const string CalculatePeerNetworkForFollowing = "CalculatePeerNetworkForFollowing";
            public const string CalculatePeerNetworkForGroups = "CalculatePeerNetworkForGroups";
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Friendly peer network labels to use in status messages.
        /// </summary>
        private static class PeerNetworkLabels
        {
            public const string Following = "followed individuals";
            public const string Groups = "individuals in groups";
        }

        /// <summary>
        /// Empty constructor for job initialization.
        /// <para>
        /// Jobs require a public empty constructor so that the scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculatePeerNetwork()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var peerNetworksCalculated = new List<string>();

            using ( var rockContext = new RockContext() )
            {
                var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;
                rockContext.Database.CommandTimeout = commandTimeout;

                if ( CalculatePeerNetworkForFollowing( rockContext ) )
                {
                    peerNetworksCalculated.Add( PeerNetworkLabels.Following );
                }

                if ( CalculatePeerNetworkForGroups( rockContext ) )
                {
                    peerNetworksCalculated.Add( PeerNetworkLabels.Groups );
                }
            }

            if ( peerNetworksCalculated.Any() )
            {
                var resultSb = new StringBuilder();

                foreach ( var peerNetwork in peerNetworksCalculated )
                {
                    resultSb.AppendLine( $"<i class='fa fa-circle text-success'></i> {peerNetwork.Titleize()}" );
                }

                UpdateLastStatusMessage( resultSb.ToString() );
            }
            else
            {
                UpdateLastStatusMessage( "<i class='fa fa-circle text-warning'></i> No peer network calculations enabled." );
            }
        }

        /// <summary>
        /// Calculates peer networks for followed individuals.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private bool CalculatePeerNetworkForFollowing( RockContext rockContext )
        {
            if ( !GetAttributeValue( AttributeKey.CalculatePeerNetworkForFollowing ).AsBoolean() )
            {
                return false;
            }

            UpdateLastStatusMessage( $"Calculating peer networks for {PeerNetworkLabels.Following}." );
            rockContext.Database.ExecuteSqlCommand( "spPeerNetwork_UpdateFollowing" );

            return true;
        }

        /// <summary>
        /// Calculates peer networks for individuals in groups.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private bool CalculatePeerNetworkForGroups( RockContext rockContext )
        {
            if ( !GetAttributeValue( AttributeKey.CalculatePeerNetworkForGroups ).AsBoolean() )
            {
                return false;
            }

            UpdateLastStatusMessage( $"Calculating peer networks for {PeerNetworkLabels.Groups}." );
            rockContext.Database.ExecuteSqlCommand( "spPeerNetwork_UpdateGroupConnections" );

            return true;
        }
    }
}
