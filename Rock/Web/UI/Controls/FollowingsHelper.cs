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
        /// <param name="followEntity">The follow entity.</param>
        /// <param name="followControl">The follow control.</param>
        /// <param name="follower">The follower.</param>
        public static void SetFollowing( IEntity followEntity, WebControl followControl, Person follower )
        {
            var followingEntityType = EntityTypeCache.Read( followEntity.GetType() );
            if ( follower.PrimaryAliasId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var followingService = new FollowingService( rockContext );

                    var followingQry = followingService.Queryable()
                        .Where( f =>
                            f.EntityTypeId == followingEntityType.Id &&

                            f.PersonAlias.PersonId == follower.Id );

                    if ( followEntity is Person )
                    {
                        var paQry = personAliasService.Queryable()
                        .Where( p => p.PersonId == followEntity.Id )
                        .Select( p => p.Id );
                        followingQry = followingQry.Where( f => paQry.Contains( f.EntityId ) );
                    }
                    else
                    {
                        followingQry = followingQry.Where( f => f.EntityId == followEntity.Id );
                    }

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
                if ( followEntity is Person )
                {
                    entityId = ( followEntity as Person ).PrimaryAliasId.Value;
                }

                string script = string.Format(
                    @"Rock.controls.followingsToggler.initialize($('#{0}'), {1}, {2}, {3}, {4});",
                        followControl.ClientID,
                        followingEntityType.Id,
                        entityId,
                        follower.Id,
                        follower.PrimaryAliasId );

                ScriptManager.RegisterStartupScript( followControl, followControl.GetType(), "following", script, true );
            }
        }
    }
}
