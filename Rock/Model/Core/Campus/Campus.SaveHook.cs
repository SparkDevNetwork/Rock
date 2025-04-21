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
using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Campus
    {
        /// <summary>
        /// Save hook implementation for <see cref="Campus"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Campus>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var campus = this.Entity as Campus;

                /*
                * 1/15/2020 - JPH
                * Upon saving a Campus, ensure it has a TeamGroup defined (GroupType = 'Team Group',
                * IsSystem = true). We are creating this Campus-to-Group relationship behind the scenes
                * so that we can assign GroupRoles to a Campus, and place people into those roles.
                *
                * Reason: Campus Team Feature
                */
                var campusTeamGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM.AsGuid() );
                if ( State != EntityContextState.Deleted && campusTeamGroupTypeId.HasValue )
                {
                    if ( campus.TeamGroup == null || campus.TeamGroup.GroupTypeId != campusTeamGroupTypeId.Value )
                    {
                        // this Campus does not yet have a Group of the correct GroupType: create one and assign it
                        var teamGroup = new Group
                        {
                            IsSystem = true,
                            GroupTypeId = campusTeamGroupTypeId.Value,
                            Name = string.Format( "{0} Team", campus.Name ),
                            Description = "Are responsible for leading and administering the Campus."
                        };

                        new GroupService( this.RockContext ).Add( teamGroup );
                        campus.TeamGroup = teamGroup;
                    }

                    if ( !campus.TeamGroup.IsSystem )
                    {
                        // this Campus already had a Group of the correct GroupType, but the IsSystem value was incorrect
                        campus.TeamGroup.IsSystem = true;
                    }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( PreSaveState == EntityContextState.Modified )
                {
                    // If a campus is modified, then it can only effect kiosk
                    // configuration if the LocationId or TimeZoneId are modified.
                    var oldLocationId = OriginalValues[nameof( Entity.LocationId )] as int?;
                    var oldTimeZoneId = OriginalValues[nameof( Entity.TimeZoneId )] as string;

                    if ( oldLocationId != Entity.LocationId || oldTimeZoneId != Entity.TimeZoneId )
                    {
                        CheckInDirector.SendRefreshKioskConfiguration();
                    }
                }
                else if ( PreSaveState == EntityContextState.Added || PreSaveState == EntityContextState.Deleted )
                {
                    // If a campus is added or deleted then it could effect
                    // kiosk configuration no matter what the values were/are.
                    CheckInDirector.SendRefreshKioskConfiguration();
                }

                base.PostSave();
            }
        }
    }
}
