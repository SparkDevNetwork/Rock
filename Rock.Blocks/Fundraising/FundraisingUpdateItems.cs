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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Fundraising
{
    /// <summary>
    /// Loads the items shown on the Updates tab of the fundraising blocks. Shared by the
    /// Fundraising Opportunity View and Fundraising Participant blocks so both list the
    /// same items.
    /// </summary>
    internal static class FundraisingUpdateItems
    {
        /// <summary>
        /// Gets the items in the opportunity's update content channel that are published
        /// (started, not expired and approved when the channel uses approval) and that the
        /// person is allowed to view.
        /// </summary>
        /// <param name="rockContext">The Rock context to query with.</param>
        /// <param name="contentChannelId">The identifier of the update content channel.</param>
        /// <param name="currentPerson">The person viewing the page, or <c>null</c> when nobody is signed in.</param>
        /// <returns>The items to list, in no particular order.</returns>
        internal static List<ContentChannelItem> GetViewableItems( RockContext rockContext, int contentChannelId, Person currentPerson )
        {
            /*
                11/18/2026 - MSE

                Only list items that are published: started, not expired and approved (unless the channel
                does not use approval). Item security can not be evaluated in SQL, so items the current
                person can not View are removed after the query. The channel and its type are included
                because the item's security falls back to them.

                Reason: The Updates tab listed unpublished and restricted update items.
            */
            var now = RockDateTime.Now;

            return new ContentChannelItemService( rockContext ).Queryable()
                .Include( a => a.ContentChannel.ContentChannelType )
                .Where( a => a.ContentChannelId == contentChannelId
                    && a.StartDateTime <= now
                    && ( !a.ExpireDateTime.HasValue || a.ExpireDateTime.Value > now )
                    && ( a.Status == ContentChannelItemStatus.Approved || !a.ContentChannel.RequiresApproval || a.ContentChannel.ContentChannelType.DisableStatus ) )
                .AsNoTracking()
                .ToList()
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();
        }
    }
}
