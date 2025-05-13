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
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.InteractionChannel"/> entity objects.
    /// </summary>
    public partial class InteractionChannelService
    {
        /// <summary>
        /// Returns a queryable of Interaction Channels that are tied to Rock Sites with Geo Tracking enabled.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.17" )]
        [Obsolete( "Geolocation lookups are now performed on all interactions, regardless of a Site's EnablePageViewGeoTracking setting." )]
        public IQueryable<InteractionChannel> QueryBySitesWithGeoTracking()
        {
            var channelMediumTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;

            // Get the ids of sites with geo tracking enabled. Since this will be a short list we'll pre-fetch this
            // list and do a contains later in the queryable. 
            var siteIdsWithGeoTrackingEnabled = SiteCache.All()
                .Where( s => s.EnablePageViewGeoTracking == true )
                .Select( s => s.Id )
                .ToList();

            return this.Queryable().Where( ic =>
                    ic.ChannelTypeMediumValueId == channelMediumTypeValueId &&
                    siteIdsWithGeoTrackingEnabled.Contains( ic.ChannelEntityId.Value ) );

        }
    }
}
