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
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.Bus.Locking;
using Rock.Communication.Chat.Platform.Session;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// Serializes member removals with the leader decision and replacement they may require.
    /// </summary>
    internal static partial class ChatDoorHelper
    {
        // How many of a channel's other leaders a leader's removal asks the projection about. Rock's
        // own read has already applied every part of the listed rule Rock can see, so nearly every
        // one it returns is live in the room; a few cover the rare one that is not, and keep the
        // read the size of a small room however many lead. Should every one asked fail, the door
        // only asks who leads next, which adds a leader and never leaves the channel with none.
        internal const int MaxOtherLeadersConfirmed = 10;

        /// <summary>
        /// Removes a person from a shared channel the caller manages, or from a group conversation
        /// the caller is in.
        /// </summary>
        /// <param name="caller">The person removing.</param>
        /// <param name="channelGuid">The channel, as the browser names it.</param>
        /// <param name="personAliasGuid">The person removed, by any of their aliases.</param>
        /// <param name="replacementPersonAliasGuid">Who leads next when the person removed is a channel's last leader, by any of their aliases.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the membership is removed in.</param>
        /// <returns>Whether they were removed, and whether the platform had taken it yet.</returns>
        internal static async Task<ChatMembershipResultBag> RemoveMemberAsync( Person caller, Guid channelGuid, Guid personAliasGuid, Guid? replacementPersonAliasGuid, ChatSessionContext context, RockContext rockContext )
        {
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return Membership( ChatSessionHelper.ToGateCode( gate.Gate ), "Chat is not available to you right now." );
            }

            var targetId = new PersonAliasService( rockContext ).Queryable()
                .Where( a => a.Guid == personAliasGuid )
                .Select( a => ( int? ) a.PersonId )
                .FirstOrDefault();
            if ( !targetId.HasValue )
            {
                return Membership( "door.not_found", "That person could not be found." );
            }

            // Leaving has its own door and its own line.
            if ( targetId.Value == caller.Id )
            {
                return Membership( "door.bad_request", "Leave the conversation instead." );
            }

            string line;
            var lockProvider = RockApp.Current.GetRequiredService<IDistributedLockProvider>();

            // The leader decision and removal share the room's lock across every Rock server.
            // Separate saves of different leaders otherwise both see someone else still leading.
            using ( var removalLock = await lockProvider.TryAcquireAsync( typeof( ChatDoorHelper ), channelGuid.ToString( "N" ), TimeSpan.FromSeconds( 2 ) ).ConfigureAwait( false ) )
            {
                if ( !removalLock.IsAcquired )
                {
                    return Membership( "door.not_allowed", "This channel is being changed. Try again." );
                }

                var leaders = ReadLeaders( rockContext, channelGuid, targetId.Value );
                int? replacementId = null;
                if ( replacementPersonAliasGuid.HasValue )
                {
                    replacementId = new PersonAliasService( rockContext ).Queryable()
                        .Where( a => a.Guid == replacementPersonAliasGuid.Value )
                        .Select( a => ( int? ) a.PersonId )
                        .FirstOrDefault();
                    if ( !replacementId.HasValue )
                    {
                        return Membership( "door.not_found", "The person chosen to lead next could not be found." );
                    }
                }

                // One projection call says which of these people chat still shows in the room.
                var room = ReadRoom( rockContext, context.Configuration, channelGuid, PeopleToReadForRemoval( caller.Id, targetId.Value, replacementId, leaders ) );
                if ( room == null )
                {
                    return Membership( "door.not_found", "That channel could not be found." );
                }

                var refusal = RefuseManaging( rockContext, room, caller, "You can't remove people from this channel." );
                if ( refusal != null )
                {
                    return refusal;
                }

                // Removing a banned row would lift the ban, which is a moderator's act and not this one.
                if ( room.BannedPersonIds.Contains( targetId.Value ) || HoldsBannedRow( rockContext, room.GroupId, targetId.Value ) )
                {
                    return Membership( "door.not_allowed", "That person is banned here; lift the ban instead." );
                }

                if ( !room.LivePersonIds.Contains( targetId.Value ) )
                {
                    return Membership( "door.not_member", "That person isn't in this conversation." );
                }

                // Only a shared channel has leaders; a conversation's members are all alike.
                var isPromoting = false;
                if ( room.Channel.ChannelType == SharedChannelType )
                {
                    var heldRoleId = leaders.TargetRoleId;
                    var leaderIds = heldRoleId.HasValue ? leaders.OtherLeaderIds.Concat( new[] { targetId.Value } ) : leaders.OtherLeaderIds;
                    var isLastLeader = IsLastLeader( targetId.Value, leaderIds, room.LivePersonIds, room.UnlistedPersonIds );

                    var leaderRefusal = RefuseLeaderRemoval( heldRoleId.HasValue, ChatSessionHelper.IsChatAdministrator( caller.Id, rockContext ), isLastLeader, replacementId.HasValue );
                    if ( leaderRefusal != null )
                    {
                        return leaderRefusal;
                    }

                    // A replacement is acted on only where it is needed: if someone else came to lead
                    // the channel since the screen asked, the removal goes ahead without it.
                    if ( isLastLeader )
                    {
                        var promotion = PromoteToLeader( rockContext, room, targetId.Value, replacementId.Value, heldRoleId );
                        if ( promotion != null )
                        {
                            return promotion;
                        }

                        isPromoting = true;
                    }
                }

                // The removal and any new leader are saved together, so the one push carries both and
                // the platform never sees the channel with nobody leading it.
                var targetName = new PersonService( rockContext ).Get( targetId.Value ).FullName;
                RemoveMember( rockContext, room.GroupId, targetId.Value );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( GroupMemberValidationException ) when ( isPromoting )
                {
                    // The church's own rules for the role, such as its most members or a requirement
                    // the person does not meet, are the group's call, and nothing was saved.
                    return Membership( "door.not_allowed", "That person can't lead this channel." );
                }

                var isDirect = room.Channel.ChannelType == DirectChannelType;
                line = isDirect ? caller.FullName + " removed " + targetName + "." : null;
            }

            // The database change is complete; a slow platform must not hold the room's lock.
            return await FinishAsync( context.Configuration, rockContext, channelGuid, line, SystemKind.Removed ).ConfigureAwait( false );
        }

        /// <summary>
        /// Why a channel's leader may not be removed: only a Chat Administrator removes one, and
        /// the last one only with someone named to lead next.
        /// </summary>
        /// <param name="targetLeads">Whether the person removed holds a live leader membership of the channel.</param>
        /// <param name="callerIsChatAdministrator">Whether the caller is an active Chat Administrator.</param>
        /// <param name="isLastLeader">Whether nobody else leads the channel.</param>
        /// <param name="hasReplacement">Whether the caller named someone to lead next.</param>
        /// <returns>The refusal, or null where the removal may go ahead.</returns>
        internal static ChatMembershipResultBag RefuseLeaderRemoval( bool targetLeads, bool callerIsChatAdministrator, bool isLastLeader, bool hasReplacement )
        {
            if ( !targetLeads )
            {
                return null;
            }

            // Managing a room's members is not enough: who leads a room is the church's call.
            if ( !callerIsChatAdministrator )
            {
                return Membership( "door.not_allowed", "Only Chat Administrators can remove a leader." );
            }

            return isLastLeader && !hasReplacement
                ? Membership( "door.bad_request", "Choose who leads this channel next before removing its last leader." )
                : null;
        }

        /// <summary>
        /// Whether the person removed is the channel's last leader: they lead it, and nobody else
        /// leads it whom chat still shows in the room. A leader row in Rock held by someone on the
        /// Chat Ban List, inactive or no longer in the room on the platform leads nobody in chat,
        /// so it does not spare the room a replacement.
        /// </summary>
        /// <param name="targetPersonId">The person removed.</param>
        /// <param name="leaderPersonIds">Everyone holding a live leader membership in Rock.</param>
        /// <param name="live">Who the platform has in the room.</param>
        /// <param name="unlisted">Who chat does not list.</param>
        /// <returns>True when the person leads and nobody else does.</returns>
        internal static bool IsLastLeader( int targetPersonId, IEnumerable<int> leaderPersonIds, ISet<int> live, ISet<int> unlisted )
        {
            var leaders = leaderPersonIds.ToList();
            return leaders.Contains( targetPersonId )
                && !leaders.Any( id => id != targetPersonId && live.Contains( id ) && !unlisted.Contains( id ) );
        }

        /// <summary>
        /// The leader the remove door would ask a successor for: the one leader of the channel chat
        /// still shows in the room, by the rule <see cref="IsLastLeader"/> applies to a removal.
        /// </summary>
        /// <param name="leaderPersonIds">Everyone holding a live leader membership in Rock.</param>
        /// <param name="live">Who the platform has in the room.</param>
        /// <param name="unlisted">Who chat does not list.</param>
        /// <returns>The last leader, or null where none leads or more than one still does.</returns>
        internal static int? LastLeaderOf( IEnumerable<int> leaderPersonIds, ISet<int> live, ISet<int> unlisted )
        {
            var leaders = leaderPersonIds.ToList();

            // Someone not shown in the room is nobody the remove door takes out, so never the answer.
            return leaders
                .Where( id => live.Contains( id ) && !unlisted.Contains( id ) )
                .Select( id => ( int? ) id )
                .FirstOrDefault( id => IsLastLeader( id.Value, leaders, live, unlisted ) );
        }

        /// <summary>
        /// The people a removal asks the projection about: the caller, the person removed and
        /// whoever was named to lead next, and, only when the person removed leads the channel,
        /// a few of its other leaders, so the projection can say whether one of them still leads
        /// anyone in chat.
        /// </summary>
        /// <param name="callerId">The person removing.</param>
        /// <param name="targetId">The person removed.</param>
        /// <param name="replacementId">Who leads next, when someone was named.</param>
        /// <param name="leaders">What Rock read of the channel's leaders.</param>
        /// <returns>Each person once.</returns>
        internal static List<int> PeopleToReadForRemoval( int callerId, int targetId, int? replacementId, ChannelLeaders leaders )
        {
            var people = new List<int> { callerId, targetId };

            if ( replacementId.HasValue )
            {
                people.Add( replacementId.Value );
            }

            // Someone who does not lead leaves the channel led as it was, so nobody else is asked.
            if ( leaders.TargetRoleId.HasValue )
            {
                people.AddRange( leaders.OtherLeaderIds.Take( MaxOtherLeadersConfirmed ) );
            }

            return people.Distinct().ToList();
        }

        /// <summary>
        /// The role the person named to lead next is given: the removed leader's own role when it
        /// is a leader role of the channel's type, so a church's role name carries over, otherwise
        /// the type's first leader role by order.
        /// </summary>
        /// <param name="heldRoleId">The role the removed leader held, if known.</param>
        /// <param name="typeRoles">The roles of the channel's group type.</param>
        /// <returns>The role, or null when the type has no leader role.</returns>
        internal static int? PickLeaderRoleId( int? heldRoleId, IEnumerable<(int Id, bool IsLeader, int Order)> typeRoles )
        {
            var leaderRoles = typeRoles.Where( r => r.IsLeader ).ToList();
            if ( heldRoleId.HasValue && leaderRoles.Any( r => r.Id == heldRoleId.Value ) )
            {
                return heldRoleId;
            }

            return leaderRoles.OrderBy( r => r.Order ).ThenBy( r => r.Id ).Select( r => ( int? ) r.Id ).FirstOrDefault();
        }

        /// <summary>
        /// What Rock reads of a channel's leaders for a removal.
        /// </summary>
        internal sealed class ChannelLeaders
        {
            /// <summary>
            /// The leader role the person removed holds, or null when they do not lead the channel.
            /// </summary>
            public int? TargetRoleId { get; set; }

            /// <summary>
            /// A few of the channel's other leaders whom Rock still counts as listed.
            /// </summary>
            public List<int> OtherLeaderIds { get; set; } = new List<int>();
        }

        /// <summary>
        /// The leader role the person removed holds in a group, and who else may still lead it, in
        /// one read. A leader is an active, unarchived row in a leader role under no ban in force;
        /// another leader also needs what Rock itself knows of chat's listed rule, an active
        /// record and no active place on the Chat Ban List. No more others are read than the
        /// projection is then asked about.
        /// </summary>
        private static ChannelLeaders ReadLeaders( RockContext rockContext, Guid groupGuid, int targetPersonId )
        {
            var now = RockDateTime.Now;
            var activeRecordStatusId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var banListGuid = Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid();
            var banned = new GroupMemberService( rockContext ).Queryable()
                .Where( b => b.Group.Guid == banListGuid && b.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( b => b.PersonId );

            // The person removed is ordered first, so the bound never cuts their own row off.
            var rows = new GroupMemberService( rockContext ).Queryable()
                .Where( m => m.Group.Guid == groupGuid
                    && m.GroupMemberStatus == GroupMemberStatus.Active
                    && m.GroupRole.IsLeader
                    && !( m.IsChatBanned && ( !m.ChatBannedUntil.HasValue || m.ChatBannedUntil > now ) )
                    && ( m.PersonId == targetPersonId
                        || ( m.Person.RecordStatusValueId == activeRecordStatusId && !banned.Contains( m.PersonId ) ) ) )
                .GroupBy( m => m.PersonId )
                .Select( g => new { PersonId = g.Key, RoleId = g.Min( m => m.GroupRoleId ) } )
                .OrderBy( l => l.PersonId == targetPersonId ? 0 : 1 )
                .ThenBy( l => l.PersonId )
                .Take( MaxOtherLeadersConfirmed + 1 )
                .ToList();

            return new ChannelLeaders
            {
                TargetRoleId = rows.Where( l => l.PersonId == targetPersonId ).Select( l => ( int? ) l.RoleId ).FirstOrDefault(),
                OtherLeaderIds = rows.Where( l => l.PersonId != targetPersonId ).Select( l => l.PersonId ).ToList()
            };
        }

        /// <summary>
        /// Moves the person named to lead a channel next into a leader role, in place of the leader
        /// removed. Unsaved, so it is saved with the removal.
        /// </summary>
        /// <returns>The refusal when they cannot lead it, or null once the change is made.</returns>
        private static ChatMembershipResultBag PromoteToLeader( RockContext rockContext, RoomRead room, int removedPersonId, int replacementPersonId, int? heldRoleId )
        {
            // The same standing a roster row needs: in the room now, listed, and not banned on any row.
            var isEligible = replacementPersonId != removedPersonId
                && room.LivePersonIds.Contains( replacementPersonId )
                && !room.UnlistedPersonIds.Contains( replacementPersonId )
                && !room.BannedPersonIds.Contains( replacementPersonId )
                && !HoldsBannedRow( rockContext, room.GroupId, replacementPersonId );
            if ( !isEligible )
            {
                return Membership( "door.bad_request", "Choose someone in this channel to lead it next." );
            }

            var groupTypeId = new GroupService( rockContext ).GetSelect( room.GroupId, g => g.GroupTypeId );
            var roleId = PickLeaderRoleId( heldRoleId, GroupTypeCache.Get( groupTypeId ).Roles.Select( r => (r.Id, r.IsLeader, r.Order) ) );
            if ( !roleId.HasValue )
            {
                return Membership( "door.bad_request", "This channel's group type has no leader role to give." );
            }

            var service = new GroupMemberService( rockContext );
            var rows = service.AsNoFilter().Where( m => m.GroupId == room.GroupId && m.PersonId == replacementPersonId ).ToList();

            // Rock holds one row per person and role, so a row they already hold in that role,
            // archived or inactive, is brought back rather than a second one made.
            var row = rows.FirstOrDefault( m => m.GroupRoleId == roleId.Value )
                ?? rows.Where( m => !m.IsArchived && m.GroupMemberStatus == GroupMemberStatus.Active ).OrderBy( m => m.Id ).FirstOrDefault();
            if ( row == null )
            {
                return Membership( "door.bad_request", "Choose someone in this channel to lead it next." );
            }

            if ( row.IsArchived )
            {
                service.Restore( row );
            }

            row.GroupRoleId = roleId.Value;
            row.GroupMemberStatus = GroupMemberStatus.Active;

            return null;
        }
    }
}
