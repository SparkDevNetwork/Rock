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
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Event.RegistrationEntry;
using Rock.ViewModels.Group.GroupMember;
using Rock.Web.Cache;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic for client devices to use when monitoring for changes in
    /// Group Placements.
    /// </summary>
    [RealTimeTopic]
    internal sealed class GroupPlacementTopic : Topic<IGroupPlacement>
    {
        /// <summary>
        /// Joins group guids to respective channels to begin receiving notifications
        /// </summary>
        /// <param name="groupGuids">The list of group unique identifiers.</param>
        public async Task JoinGroupChannels( List<Guid> groupGuids )
        {
            foreach( var groupGuid in groupGuids )
            {
                var group = GroupCache.Get( groupGuid );

                if ( group == null )
                {
                    throw new RealTimeException( "Group was not found." );
                }

                using ( var rockContext = new RockContext() )
                {
                    var person = Context.CurrentPersonId.HasValue
                        ? new PersonService( rockContext ).Get( Context.CurrentPersonId.Value )
                        : null;

                    if ( !group.IsAuthorized( Security.Authorization.VIEW, person ) )
                    {
                        throw new RealTimeException( "You are not authorized for this group." );
                    }

                    await Channels.AddToChannelAsync( Context.ConnectionId, GetGroupMemberChannelForGroup( groupGuid ) );
                }
            }
        }

        /// <summary>
        /// Leaves an group channel to indicate that the client no longer wishes
        /// to receive notifications.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        public async Task LeaveGroupChannel( Guid groupGuid )
        {
            var group = GroupCache.Get( groupGuid );

            if ( group == null )
            {
                throw new RealTimeException( "Group was not found." );
            }

            await Channels.RemoveFromChannelAsync( Context.ConnectionId, GetGroupMemberChannelForGroup( groupGuid ) );
        }

        #region Registrant Methods

        /// <summary>
        /// Gets the registrant channels that should be used when sending
        /// notifications for this bag.
        /// </summary>
        /// <param name="bag">The bag that contains the registrant data.</param>
        /// <returns>A list of strings that contain the channel names.</returns>
        public static List<string> GetRegistrantChannelsForBag( RegistrationRegistrantUpdatedMessageBag bag )
        {
            var channels = new List<string>();

            if ( bag.RegistrationInstanceGuid.HasValue )
            {
                channels.Add( GetRegistrantChannelForRegistrationInstance( bag.RegistrationInstanceGuid.Value ) );
            }

            if ( bag.RegistrationTemplateGuid.HasValue )
            {
                channels.Add( GetRegistrantChannelForRegistrationTemplate( bag.RegistrationTemplateGuid.Value ) );
            }

            return channels;
        }

        /// <summary>
        /// Gets the channel for monitoring registrant notifications for the
        /// specified registration instance.
        /// </summary>
        /// <param name="guid">The registration instance unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetRegistrantChannelForRegistrationInstance( Guid guid )
        {
            return $"Registrant:RegistrationInstance:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring registrant notifications for the
        /// specified registration template.
        /// </summary>
        /// <param name="guid">The registration template unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetRegistrantChannelForRegistrationTemplate( Guid guid )
        {
            return $"Registrant:RegistrationInstance:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring registrant deleted notifications.
        /// </summary>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetRegistrantDeletedChannel()
        {
            return "Registrant:Deleted";
        }

        #endregion

        #region Group Member Methods

        /// <summary>
        /// Gets the group member channels that should be used when sending
        /// notifications for this bag.
        /// </summary>
        /// <param name="bag">The bag that contains the group member data.</param>
        /// <returns>A list of strings that contain the channel names.</returns>
        public static List<string> GetGroupMemberChannelsForBag( GroupMemberUpdatedMessageBag bag )
        {
            var channels = new List<string>();

            if ( bag.GroupGuid.HasValue )
            {
                channels.Add( GetGroupMemberChannelForGroup( bag.GroupGuid.Value ) );
            }

            return channels;
        }

        /// <summary>
        /// Gets the channel for monitoring group member notifications for the
        /// specified group.
        /// </summary>
        /// <param name="guid">The group unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetGroupMemberChannelForGroup( Guid guid )
        {
            return $"GroupMember:Group:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring group member deleted notifications.
        /// </summary>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetGroupMemberDeletedChannel()
        {
            return "GroupMember:Deleted";
        }

        #endregion
    }
}
