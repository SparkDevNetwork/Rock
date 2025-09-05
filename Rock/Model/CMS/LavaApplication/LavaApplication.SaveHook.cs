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

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LavaApplication
    {

        /// <summary>
        /// Save hook implementation for <see cref="LavaApplication"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<LavaApplication>
        {
            private History.HistoryChangeList HistoryChanges { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                base.PostSave();

                // Add base security to the application
                if ( State == EntityContextState.Added )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var authService = new AuthService( rockContext );

                        // Add administrator access
                       
                        var adminRoleGroupId = RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id;

                        authService.Add( GetAuth( adminRoleGroupId, Authorization.VIEW ) );
                        authService.Add( GetAuth( adminRoleGroupId, Authorization.EDIT ) );
                        authService.Add( GetAuth( adminRoleGroupId, Authorization.ADMINISTRATE ) );

                        var lavaApplicationDevelopers = RoleCache.Get( SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() ).Id;
                        authService.Add( GetAuth( lavaApplicationDevelopers, Authorization.VIEW ) );
                        authService.Add( GetAuth( lavaApplicationDevelopers, Authorization.EDIT ) );
                        authService.Add( GetAuth( lavaApplicationDevelopers, Authorization.ADMINISTRATE ) );

                        rockContext.SaveChanges();
                    }
                }
            }

            /// <summary>
            /// Creates an auth record for the group.
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="action"></param>
            /// <returns></returns>
            private Auth GetAuth( int groupId, string action )
            {
                return new Auth
                {
                    EntityTypeId = this.Entity.TypeId,
                    EntityId = this.Entity.Id,
                    Action = action,
                    AllowOrDeny = "A",
                    GroupId = groupId,
                    Order = 0,
                    PersonAliasId = null,
                    SpecialRole = SpecialRole.None
                };
            }
        }

    }
}
