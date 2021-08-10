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

            SetFollowing( followEntity.TypeId, followEntity.Id, followControl, follower, string.Empty, string.Empty );
        }

        /// <summary>
        /// Configures a control to display and toggle following for the specified entity
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="followControl">The follow control.</param>
        /// <param name="follower">The follower.</param>
        /// <param name="callbackScript">The callback script.</param>
        public static void SetFollowing( int entityTypeId, int entityId, WebControl followControl, Person follower, string callbackScript = "" )
        {
            SetFollowing( entityTypeId, entityId, followControl, follower, callbackScript, string.Empty );
        }

        /// <summary>
        /// Configures a control to display and toggle following for the specified entity
        /// </summary>
        /// <param name="followEntity">The follow entity. NOTE: Make sure to use PersonAlias instead of Person when following a Person</param>
        /// <param name="followControl">The follow control.</param>
        /// <param name="follower">The follower.</param>
        /// <param name="purposeKey">A purpose that defines how this following will be used.</param>
        public static void SetFollowing( IEntity followEntity, WebControl followControl, Person follower, string purposeKey )
        {
            if ( followEntity == null )
            {
                return;
            }

            SetFollowing( followEntity.TypeId, followEntity.Id, followControl, follower, string.Empty, purposeKey );
        }

        /// <summary>
        /// Configures a control to display and toggle following for the specified entity
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="followControl">The follow control.</param>
        /// <param name="follower">The follower.</param>
        /// <param name="callbackScript">The callback script.</param>
        /// <param name="purposeKey">A purpose that defines how this following will be used.</param>
        public static void SetFollowing( int entityTypeId, int entityId, WebControl followControl, Person follower, string callbackScript = "", string purposeKey = "" )
        {
            if ( follower != null && follower.PrimaryAliasId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var followingService = new FollowingService( rockContext );

                    // If PurposeKey is null then the provided purposeKey must be
                    // empty.
                    // If PurposeKey is not null then it must match the provided
                    // purposeKey.
                    var followingQry = followingService.Queryable()
                        .Where( f =>
                            f.EntityTypeId == entityTypeId &&
                            f.EntityId == entityId &&
                            f.PersonAlias.PersonId == follower.Id &&
                            ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                    if ( followingQry.Any() )
                    {
                        followControl.AddCssClass( "following" );
                    }
                    else
                    {
                        followControl.RemoveCssClass( "following" );
                    }
                }

                // only show the following control if the entity has been saved to the database
                followControl.Visible = entityId > 0;

                string script = string.Format(
                    @"Rock.controls.followingsToggler.initialize($('#{0}'), {1}, {2}, '{3}', {4}, {5}, {6});",
                        followControl.ClientID,
                        entityTypeId,
                        entityId,
                        purposeKey.Replace( "'", "\\'" ),
                        follower.Id,
                        follower.PrimaryAliasId,
                        callbackScript.IsNullOrWhiteSpace() ? "null" : callbackScript );

                ScriptManager.RegisterStartupScript( followControl, followControl.GetType(), followControl.ClientID + "_following", script, true );
            }
        }
    }
}
