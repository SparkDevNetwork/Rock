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
using System.Linq;
using Rock.Data;
using Rock.IpAddress.Classes;
using Rock.Model;

namespace Rock.IpAddress
{
    /// <summary>
    /// Utilities to help work with IP Address locations
    /// </summary>
    [RockObsolete( "1.17" )]
    [Obsolete( "Use IpGeoLookup instead." )]
    public static class IpLocationUtilities
    {
        #region Public Methods
        /// <summary>
        /// Updates a list of interaction session locations with their sessions.
        /// </summary>
        /// <param name="ipLocations">The ip locations.</param>
        /// <param name="ipSessions">The ip sessions.</param>
        public static void UpdateInteractionSessionLocations( List<IpLocation> ipLocations, Dictionary<string, List<int>> ipSessions )
        {
            // Process each valid address and invalid address.  Invalid (or 'bad') addresses will be 
            // handled in the UpdateInteractionSessionLocation method so they do not reprocess each time
            // the PopulateInteractionSessionData job runs.
            foreach ( var location in ipLocations )
            {
                // Process location if sessions exist for it
                if ( ipSessions.ContainsKey( location.IpAddress ) )
                {
                    UpdateInteractionSessionLocation( location, ipSessions[location.IpAddress] );
                }
            }
        }
        #endregion 

        #region Private Methods
        /// <summary>
        /// Updates a single interaction session location and links in their sessions.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="sessionIds">The session ids.</param>
        private static void UpdateInteractionSessionLocation( IpLocation location, List<int> sessionIds )
        {
            var rockContext = new RockContext();
            var interactionSessionLocationService = new InteractionSessionLocationService( rockContext );
            var interactionSessionService = new InteractionSessionService( rockContext );

            // Create the Interaction Session Location record we'll use in processing
            InteractionSessionLocation interactionSessionLocation;

            // If the result record was null that means the address is not correct. We'll link this
            // to a bad address location to prevent re-process on future runs
            if ( !location.IsValid )
            {
                interactionSessionLocation = GetBadAddressLocation( interactionSessionLocationService, location );
            }
            else
            {
                // Check if a matching session location already exists. This is possible as we are rechecking
                // IP address that have not been looked up in a while in case the data has changed in the providers
                // database. When these changes take place we'll keep the original data and create a new location
                // record with new new data.
                interactionSessionLocation = interactionSessionLocationService.Queryable()
                    .Where( l => l.IpAddress == location.IpAddress )
                    .Where( l => l.ISP == location.ISP
                        && l.RegionCode == location.RegionCode
                        && l.CountryCode == location.CountryCode
                        && l.PostalCode == location.PostalCode )
                    .FirstOrDefault();

                // No match was found so let's create a new one
                if ( interactionSessionLocation == null )
                {
                    interactionSessionLocation = new InteractionSessionLocation
                    {
                        IpAddress = location.IpAddress,
                    };

                    interactionSessionLocationService.Add( interactionSessionLocation );
                }

                // Update the properties (doing this in all cases in case one of our defined values changed)
                interactionSessionLocation.PostalCode = location.PostalCode;
                interactionSessionLocation.Location = location.Location;
                interactionSessionLocation.RegionCode = location.RegionCode;
                interactionSessionLocation.RegionValueId = location.RegionValueId;
                interactionSessionLocation.CountryCode = location.CountryCode;
                interactionSessionLocation.CountryValueId = location.CountryValueId;
                interactionSessionLocation.GeoPoint = Rock.Model.Location.GetGeoPoint( location.Latitude, location.Longitude );
                interactionSessionLocation.ISP = location.ISP;
            }

            interactionSessionLocation.LookupDateTime = RockDateTime.Now;
            rockContext.SaveChanges();

            // Save the location to the sessions
            BulkUpdateSessions( sessionIds, interactionSessionLocation.Id );
        }

        /// <summary>
        /// Bulks updates the sessions for the location.
        /// </summary>
        /// <param name="sessionIds">The session ids.</param>
        /// <param name="interactionSessionLocationId">The interaction session location identifier.</param>
        private static void BulkUpdateSessions( List<int> sessionIds, int interactionSessionLocationId )
        {
            // We create our own context and service to be self-contained
            var rockContext = new RockContext();
            var interactionSessionService = new InteractionSessionService( rockContext );

            // Bulk update the sessions with the new interaction session location
            // We'll need to chunk this up as some IPs can have tens of thousands of sessions (monitoring apps)
            var pagingIndex = 0;
            var batchSize = 1000;

            while ( true )
            {
                var batchSessionIds = sessionIds.Skip( pagingIndex ).Take( batchSize ).ToList();
                var interactionSessions = interactionSessionService.GetByIds( batchSessionIds );

                rockContext.BulkUpdate( interactionSessions,
                    a => new InteractionSession { InteractionSessionLocationId = interactionSessionLocationId } );

                // Update the cursor
                pagingIndex += batchSize;

                // Check if we're out of sessions to process
                if ( batchSessionIds.Count() < batchSize )
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the bad address location that is used for all IP address that we're not found.
        /// </summary>
        /// <returns></returns>
        private static InteractionSessionLocation GetBadAddressLocation( InteractionSessionLocationService interactionSessionLocationService, IpLocation location )
        {
            /* 
             5/31/2022 JME
             To prevent checking addresses that were not found on every future run we tie these sessions to a 'bad address' Interaction
             Session Location. We create a location for each type of bad address (invalid and reserved). 
            */

            var badAddressIp = location.IpLocationErrorCode.ToString();

            // Get the record used for all bad addresses
            var badAddressRecord = interactionSessionLocationService.Queryable()
                  .Where( l => l.IpAddress == badAddressIp )
                  .FirstOrDefault();

            if ( badAddressRecord != null )
            {
                return badAddressRecord;
            }

            // Bad IP address record does not exist so we'll make it
            badAddressRecord = new InteractionSessionLocation
            {
                IpAddress = badAddressIp
            };

            interactionSessionLocationService.Add( badAddressRecord );

            return badAddressRecord;
        }
        #endregion
    }
}
