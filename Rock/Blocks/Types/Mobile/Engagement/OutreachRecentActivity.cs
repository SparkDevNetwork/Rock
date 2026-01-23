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

using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Engagement.OutreachRecentActivity;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// Recent Activity allows you to view recent touchpoints.
    /// </summary>
    [DisplayName( "Outreach Recent Activity" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-list" )]
    [Description( "Recent Activity allows you to view recent touchpoints." )]
    [SupportedSiteTypes( SiteType.Mobile )]

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_OUTREACH_RECENT_ACTIVITY_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_OUTREACH_RECENT_ACTIVITY )]
    public class OutreachRecentActivity : RockBlockType
    {
        /// <summary>
        /// Get the recent activity.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetRecentActivity()
        {
            var person = RequestContext.CurrentPerson;

            if ( person == null )
            {
                return ActionBadRequest( "Current person not found." );
            }

            ContactTouchpointService touchpointService = new ContactTouchpointService( RockContext );
            ContactService contactService = new ContactService( RockContext );

            var personContactIds = contactService
                .Queryable()
                .Where( c => c.OwnerPersonAliasId == person.PrimaryAliasId )
                .Select( c => c.Id );

            var recentActivities = touchpointService
                .Queryable()
                .Where( tp => personContactIds.Contains( tp.ContactId ) )
                .Where( tp => tp.CompletedDateTime != null )
                .OrderByDescending( tp => tp.CompletedDateTime )
                .Take( 5 )
                .Select( tp => new
                {
                    tp.Contact.PhotoId,
                    tp.Contact.FirstName,
                    tp.Contact.LastName,
                    tp.Type,
                    tp.CompletedDateTime,
                    tp.Contact.Gender
                } )
                .ToList()
                .Select( tp =>
                {
                    var profileURL = tp.PhotoId.HasValue
                        ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( tp.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) )
                        : "";
                    return new RecentActivity
                    {
                        ProfileURL = profileURL,
                        contactName = tp.FirstName,
                        LastName = tp.LastName,
                        Gender = tp.Gender.ToMobile(),
                        TouchpointType = tp.Type.ToMobile(),
                        CompletedDate = tp.CompletedDateTime.Value
                    };
                } ).ToList();

            return ActionOk( recentActivities );
        }
    }
}
