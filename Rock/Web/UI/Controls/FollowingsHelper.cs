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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    ///  Helper class that allows you to configure a control (Image, Panel, etc) as a "Following" control
    /// </summary>
    public static class FollowingsHelper
    {
        /// <summary>
        /// Configures a control to display and toggle following for the specified entity
        /// </summary>
        /// <param name="followEntity">The follow entity. NOTE: Make sure to use PersonAlias instead of Person when following a Person</param>
        /// <param name="followControl">The follow control.</param>
        /// <param name="follower">The follower.</param>
        public static void SetFollowing( IEntity followEntity, WebControl followControl, Person follower )
        {
            if ( followEntity == null )
            {
                return;
            }

            var followingEntityType = EntityTypeCache.Get( followEntity.GetType() );
            if ( follower != null && follower.PrimaryAliasId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var followingService = new FollowingService( rockContext );

                    var followingQry = followingService.Queryable()
                        .Where( f =>
                            f.EntityTypeId == followingEntityType.Id &&
                            f.PersonAlias.PersonId == follower.Id );

                    followingQry = followingQry.Where( f => f.EntityId == followEntity.Id );

                    if ( followingQry.Any() )
                    {
                        followControl.AddCssClass( "following" );
                    }
                    else
                    {
                        followControl.RemoveCssClass( "following" );
                    }
                }

                int entityId = followEntity.Id;

                // only show the following control if the entity has been saved to the database
                followControl.Visible = entityId > 0;

                string script = string.Format(
                    @"Rock.controls.followingsToggler.initialize($('#{0}'), {1}, {2}, {3}, {4});",
                        followControl.ClientID,
                        followingEntityType.Id,
                        entityId,
                        follower.Id,
                        follower.PrimaryAliasId );

                ScriptManager.RegisterStartupScript( followControl, followControl.GetType(), followControl.ClientID + "_following", script, true );
            }
        }
    }
}
