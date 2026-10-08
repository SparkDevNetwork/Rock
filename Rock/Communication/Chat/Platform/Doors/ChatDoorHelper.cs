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
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The doors that change who is in a conversation: starting a direct message from the Chat
    /// block; joining, leaving, adding and removing people and renaming a group conversation; and
    /// the direct message and channel post a workflow sends.
    /// </summary>
    /// <remarks>
    /// Everything a door decides is read from Rock. Whether a group is a chat channel, who is
    /// enrolled and what each person's Open DM resolves to are read through the one projection,
    /// so no second copy of those rules exists here.
    /// </remarks>
    internal static class ChatDoorHelper
    {
        #region Constants

        /// <summary>
        /// The code every door answers with when it did what was asked.
        /// </summary>
        internal const string OkCode = "ok";

        // A conversation holds nine people, the one starting it included.
        internal const int MaxOthers = 8;

        // The platform's names for the kinds of channel a door tells apart.
        private const string SharedChannelType = "shared";

        private const string DirectChannelType = "dm";

        // The longest name a conversation carries, Rock's own width for a group's name.
        private const int MaxNameLength = 100;

        // Rock requires a group name, and the platform treats this one as no name at all.
        private const string DirectMessageName = "Chat Direct Message";

        // The namespace every derived direct message Guid is hashed under. Fixed for good: a new
        // value would give every existing set of people a second Guid.
        private static readonly Guid DirectMessageNamespace = new Guid( "6F3B2C1A-8D4E-4B7F-9A21-5C0E7D3F4B18" );

        // The immediate sync's own background timeout, so a workflow waits as long as a push may take.
        private static readonly TimeSpan WorkflowWait = TimeSpan.FromSeconds( 10 );

        #endregion Constants

        #region The door

        /// <summary>
        /// Starts a direct message with the people chosen, or reopens the one those exact people
        /// already have, as the first message sent to a draft asks.
        /// </summary>
        /// <param name="caller">The person starting it.</param>
        /// <param name="personAliasGuids">The other people, one to eight, by any of their aliases.</param>
        /// <param name="context">The church's settings and the caller's direct message access.</param>
        /// <param name="rockContext">The context the conversation is created in.</param>
        /// <returns>The conversation, or why there is none.</returns>
        internal static async Task<ChatDirectMessageResultBag> StartDirectMessageAsync( Person caller, IEnumerable<Guid> personAliasGuids, ChatSessionContext context, RockContext rockContext )
        {
            // The same gates a session obeys, so the door is never a way into chat around them.
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return Refuse( ChatSessionHelper.ToGateCode( gate.Gate ), "Chat is not available to you right now." );
            }

            // The button is hidden without the right, but a button is not a control.
            if ( !gate.CanStartDm )
            {
                return Refuse( "door.not_allowed", "You are not able to start new conversations." );
            }

            var requested = ( personAliasGuids ?? Enumerable.Empty<Guid>() ).Distinct().ToList();
            if ( requested.Count == 0 || requested.Count > MaxOthers )
            {
                return Refuse( "door.bad_request", "Choose between one and eight people." );
            }

            var chosen = new PersonAliasService( rockContext ).Queryable()
                .Where( a => requested.Contains( a.Guid ) )
                .Select( a => new { a.Guid, a.PersonId, a.Person.NickName, a.Person.LastName } )
                .ToList();

            // A Guid of anything else, a group's included, names nobody.
            if ( chosen.Count != requested.Count )
            {
                return Refuse( "door.not_found", "One of the people chosen could not be found." );
            }

            if ( chosen.Any( a => a.PersonId == caller.Id ) )
            {
                return Refuse( "door.self", "You are already in every conversation you start." );
            }

            // Two aliases of one person are one person, and the request names each person by the
            // first alias it gave for them.
            var others = chosen.GroupBy( a => a.PersonId ).Select( g => g.First() ).ToList();
            var personIds = others.Select( a => a.PersonId ).Concat( new[] { caller.Id } ).ToList();
            var read = ReadDirectMessage( rockContext, context.Configuration, caller.Id, personIds, true );

            foreach ( var other in others )
            {
                // Someone the projection leaves out has no row on the platform to put in a
                // conversation, and chat's own author is not a person to talk to.
                var person = read.People[other.PersonId];
                var isOutOfReach = person == null || person.IsGloballyBanned || person.IsInactive || other.Guid == Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid();

                if ( isOutOfReach )
                {
                    return NotEligible( other.Guid, other.NickName, other.LastName );
                }
            }

            // An existing conversation is reopened even after someone turned Open DM off, but a
            // ban in it is not stepped around by starting again.
            if ( read.Existing.HasValue )
            {
                var banned = others.FirstOrDefault( o => read.BannedPersonIds.Contains( o.PersonId ) );
                if ( banned != null || read.BannedPersonIds.Contains( caller.Id ) )
                {
                    return NotEligible( banned?.Guid, banned?.NickName, banned?.LastName );
                }

                return new ChatDirectMessageResultBag { Code = OkCode, ChannelGuid = read.Existing };
            }

            // A new conversation needs each person's Open DM, or a private room they share.
            var unreachable = others.FirstOrDefault( o => !read.People[o.PersonId].IsOpenDmAllowed && !read.SharedPersonIds.Contains( o.PersonId ) );
            if ( unreachable != null )
            {
                return NotEligible( unreachable.Guid, unreachable.NickName, unreachable.LastName );
            }

            // This door enrols nobody, so no person's own row needs pushing with the conversation.
            var channelGuid = CreateDirectMessage( rockContext, context.Configuration.TenantId.Value, personIds, Enumerable.Empty<int>() );
            var push = await ChatPlatformSyncHelper.FlushAsync( rockContext ).ConfigureAwait( false );

            // A push still on its way is no failure: the client waits for the conversation to arrive.
            return new ChatDirectMessageResultBag
            {
                Code = OkCode,
                ChannelGuid = channelGuid,
                IsPending = push == ChatPlatformSyncHelper.PushOutcome.Pending
            };
        }

        /// <summary>
        /// The Guid a new direct message of these people in this church is given: a name-based
        /// UUID (version 5, SHA-1) over the church and the people, so the same set always gives the
        /// same Guid and Rock's unique index on a group's Guid refuses a second create.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="personIds">The people, in any order, the one starting it included.</param>
        /// <returns>The Guid.</returns>
        internal static Guid DirectMessageGuid( Guid tenantId, IEnumerable<int> personIds )
        {
            // The separator keeps ids 1 and 23 apart from ids 12 and 3.
            var name = tenantId.ToString( "D" ) + ":" + string.Join( ",", personIds.Distinct().OrderBy( id => id ) );

            // RFC 4122 hashes the namespace in network order, which is not how .NET lays out a Guid.
            var namespaceBytes = ToNetworkOrder( DirectMessageNamespace.ToByteArray() );
            var nameBytes = Encoding.UTF8.GetBytes( name );

            byte[] hash;
            using ( var sha1 = SHA1.Create() )
            {
                hash = sha1.ComputeHash( namespaceBytes.Concat( nameBytes ).ToArray() );
            }

            var bytes = new byte[16];
            Array.Copy( hash, bytes, 16 );

            // The version in the high nibble of byte 6 and the RFC 4122 variant in byte 8.
            bytes[6] = ( byte ) ( ( bytes[6] & 0x0F ) | 0x50 );
            bytes[8] = ( byte ) ( ( bytes[8] & 0x3F ) | 0x80 );

            return new Guid( ToNetworkOrder( bytes ) );
        }

        /// <summary>
        /// Saves one of the caller's own chat settings, show my profile details or let anyone
        /// message me, and pushes it before answering.
        /// </summary>
        /// <param name="caller">The person changing their own setting.</param>
        /// <param name="setting">"profile_details" or "open_dm".</param>
        /// <param name="value">On or off.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the person is saved in.</param>
        /// <returns>Whether it was saved, and whether the platform had taken it yet.</returns>
        internal static async Task<ChatPersonSettingResultBag> SavePersonSettingAsync( Person caller, string setting, bool value, ChatSessionContext context, RockContext rockContext )
        {
            // The same gates a session obeys, so nobody chat refuses can change how chat shows them.
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return new ChatPersonSettingResultBag { Code = ChatSessionHelper.ToGateCode( gate.Gate ), Message = "Chat is not available to you right now." };
            }

            var isProfileDetails = setting == "profile_details";
            if ( !isProfileDetails && setting != "open_dm" )
            {
                return new ChatPersonSettingResultBag { Code = "door.bad_request", Message = "That setting can't be changed here." };
            }

            // Only the setting named is written: the other may be null, meaning it follows the
            // church's default, and writing both would turn that into a fixed value.
            var person = new PersonService( rockContext ).Get( caller.Id );
            if ( isProfileDetails )
            {
                person.IsChatProfilePublic = value;
            }
            else
            {
                person.IsChatOpenDirectMessageAllowed = value;
            }

            // The person's save hook records the change for the immediate sync, and this waits for it.
            rockContext.SaveChanges();
            var push = await ChatPlatformSyncHelper.FlushAsync( rockContext ).ConfigureAwait( false );

            return new ChatPersonSettingResultBag
            {
                Code = OkCode,
                IsPending = push == ChatPlatformSyncHelper.PushOutcome.Pending
            };
        }

        #endregion The door

        #region The membership doors

        /// <summary>
        /// Joins the caller to a public or pinned channel, as the Join button or a first send into
        /// it asks.
        /// </summary>
        /// <param name="caller">The person joining.</param>
        /// <param name="channelGuid">The channel, as the browser names it.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the membership is saved in.</param>
        /// <returns>Whether the caller is in the channel now, and whether the platform had taken it yet.</returns>
        internal static async Task<ChatMembershipResultBag> JoinChannelAsync( Person caller, Guid channelGuid, ChatSessionContext context, RockContext rockContext )
        {
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return Membership( ChatSessionHelper.ToGateCode( gate.Gate ), "Chat is not available to you right now." );
            }

            var room = ReadRoom( rockContext, context.Configuration, channelGuid, new[] { caller.Id } );
            if ( room == null )
            {
                return Membership( "door.not_found", "That channel could not be found." );
            }

            // Only a shared room everyone can read is joined, and a ban is never stepped around by
            // joining again.
            var isJoinable = room.Channel.ChannelType == SharedChannelType && ( room.Channel.IsPublic || room.Channel.IsAlwaysShown );
            if ( !isJoinable || room.BannedPersonIds.Contains( caller.Id ) )
            {
                return Membership( "door.not_allowed", "You can't join this channel." );
            }

            // A join sent twice is done once, and the second posts no second line.
            if ( room.LivePersonIds.Contains( caller.Id ) )
            {
                return Membership( OkCode, null );
            }

            if ( HoldsBannedRow( rockContext, room.GroupId, caller.Id ) )
            {
                return Membership( "door.not_allowed", "You can't join this channel." );
            }

            AddOrRestoreMember( rockContext, room.GroupId, caller.Id );
            rockContext.SaveChanges();

            return await FinishAsync( context.Configuration, rockContext, channelGuid, caller.FullName + " joined the channel." ).ConfigureAwait( false );
        }

        /// <summary>
        /// Takes the caller out of a channel that lets them leave, or a group conversation, and out
        /// of the Rock group with it.
        /// </summary>
        /// <param name="caller">The person leaving.</param>
        /// <param name="channelGuid">The channel, as the browser names it.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the membership is removed in.</param>
        /// <returns>Whether the caller has left, and whether the platform had taken it yet.</returns>
        internal static async Task<ChatMembershipResultBag> LeaveChannelAsync( Person caller, Guid channelGuid, ChatSessionContext context, RockContext rockContext )
        {
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return Membership( ChatSessionHelper.ToGateCode( gate.Gate ), "Chat is not available to you right now." );
            }

            var room = ReadRoom( rockContext, context.Configuration, channelGuid, new[] { caller.Id } );
            if ( room == null )
            {
                return Membership( "door.not_found", "That channel could not be found." );
            }

            // Leaving deletes every row the person holds here that is not archived, a banned one with
            // the ban on it; an archived banned row refuses too, which staff can clear in Rock.
            if ( room.BannedPersonIds.Contains( caller.Id ) || HoldsBannedRow( rockContext, room.GroupId, caller.Id ) )
            {
                return Membership( "door.not_allowed", "You can't leave this channel." );
            }

            if ( !room.LivePersonIds.Contains( caller.Id ) )
            {
                return Membership( "door.not_member", "You're not in this conversation." );
            }

            // A conversation of two is hidden rather than left, so the other person is never left
            // talking to nobody.
            var isDirect = room.Channel.ChannelType == DirectChannelType;
            var mayLeave = isDirect
                ? room.PersonCount > 2
                : room.Channel.ChannelType == SharedChannelType && room.Channel.IsLeaveAllowed;
            if ( !mayLeave )
            {
                return Membership( "door.not_allowed", isDirect ? "A conversation of two can be hidden but not left." : "This channel can't be left." );
            }

            RemoveMember( rockContext, room.GroupId, caller.Id );
            rockContext.SaveChanges();

            return await FinishAsync( context.Configuration, rockContext, channelGuid, isDirect ? caller.FullName + " left the conversation." : null ).ConfigureAwait( false );
        }

        /// <summary>
        /// Adds people to a shared channel the caller manages, or to a group conversation the caller
        /// is in.
        /// </summary>
        /// <param name="caller">The person adding.</param>
        /// <param name="channelGuid">The channel, as the browser names it.</param>
        /// <param name="personAliasGuids">The people, one to eight, by any of their aliases.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the memberships are saved in.</param>
        /// <returns>Whether they were added, or who could not be.</returns>
        internal static async Task<ChatMembershipResultBag> AddMembersAsync( Person caller, Guid channelGuid, IEnumerable<Guid> personAliasGuids, ChatSessionContext context, RockContext rockContext )
        {
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return Membership( ChatSessionHelper.ToGateCode( gate.Gate ), "Chat is not available to you right now." );
            }

            var requested = ( personAliasGuids ?? Enumerable.Empty<Guid>() ).Distinct().ToList();
            if ( requested.Count == 0 || requested.Count > MaxOthers )
            {
                return Membership( "door.bad_request", "Choose between one and eight people." );
            }

            var chosen = new PersonAliasService( rockContext ).Queryable()
                .Where( a => requested.Contains( a.Guid ) )
                .Select( a => new { a.Guid, a.PersonId } )
                .ToList();
            if ( chosen.Count != requested.Count )
            {
                return Membership( "door.not_found", "One of the people chosen could not be found." );
            }

            // Two aliases of one person are one person, named by the first alias given for them.
            var others = chosen.GroupBy( a => a.PersonId ).Select( g => g.First() ).ToList();
            if ( others.Any( o => o.PersonId == caller.Id ) )
            {
                return Membership( "door.bad_request", "You're already in it." );
            }

            var room = ReadRoom( rockContext, context.Configuration, channelGuid, others.Select( o => o.PersonId ).Concat( new[] { caller.Id } ) );
            if ( room == null )
            {
                return Membership( "door.not_found", "That channel could not be found." );
            }

            var refusal = RefuseManaging( rockContext, room, caller, "You can't add people to this channel." );
            if ( refusal != null )
            {
                return refusal;
            }

            var isDirect = room.Channel.ChannelType == DirectChannelType;
            var newcomers = others.Where( o => !room.LivePersonIds.Contains( o.PersonId ) ).ToList();
            if ( isDirect && room.PersonCount + newcomers.Count > MaxOthers + 1 )
            {
                return Membership( "door.bad_request", "A conversation holds at most nine people." );
            }

            // Everyone is checked before anyone is added, so a refusal adds nobody.
            // A list, because Entity Framework translates a list's Contains and not a projection's.
            var newcomerIds = newcomers.Select( n => n.PersonId ).ToList();
            // Deceased people are read too, so the session gates refuse them rather than the read missing them.
            var people = new PersonService( rockContext ).Queryable( true ).Where( p => newcomerIds.Contains( p.Id ) ).ToList();
            var gateContext = new ChatSessionContext { Configuration = context.Configuration };
            foreach ( var newcomer in newcomers )
            {
                var person = people.First( p => p.Id == newcomer.PersonId );
                var isKeptOut = room.BannedPersonIds.Contains( newcomer.PersonId )
                    || HoldsBannedRow( rockContext, room.GroupId, newcomer.PersonId )
                    || ChatSessionHelper.Evaluate( person, gateContext, rockContext ).Gate != ChatMintGate.Ok;

                if ( isKeptOut )
                {
                    return NotAddable( newcomer.Guid, person.FullName );
                }
            }

            // A conversation takes only someone the caller could start one with: their Open DM is
            // on, or the two share a private room.
            if ( isDirect && newcomers.Any() )
            {
                var read = ReadDirectMessage( rockContext, context.Configuration, caller.Id, newcomers.Select( n => n.PersonId ).Concat( new[] { caller.Id } ).ToList(), true );
                var unreachable = newcomers.FirstOrDefault( n =>
                {
                    var projected = read.People[n.PersonId];
                    return projected == null || projected.IsGloballyBanned || projected.IsInactive
                        || ( !projected.IsOpenDmAllowed && !read.SharedPersonIds.Contains( n.PersonId ) );
                } );

                if ( unreachable != null )
                {
                    return NotAddable( unreachable.Guid, people.First( p => p.Id == unreachable.PersonId ).FullName );
                }
            }

            if ( !newcomers.Any() )
            {
                return Membership( OkCode, null );
            }

            // Someone who has never opened chat is enrolled, and their own row goes with the push,
            // since a membership needs its person on the platform.
            foreach ( var person in people )
            {
                ChatSessionHelper.EnsureEnrollment( person, gateContext, rockContext );
                ChatPlatformSyncHelper.RecordPersonChange( rockContext, person.Id );
                AddOrRestoreMember( rockContext, room.GroupId, person.Id );
            }

            rockContext.SaveChanges();

            var names = newcomers.Select( n => people.First( p => p.Id == n.PersonId ).FullName ).ToList();
            return await FinishAsync( context.Configuration, rockContext, channelGuid, caller.FullName + " added " + JoinNames( names ) + "." ).ConfigureAwait( false );
        }

        /// <summary>
        /// Removes a person from a shared channel the caller manages, or from a group conversation
        /// the caller is in.
        /// </summary>
        /// <param name="caller">The person removing.</param>
        /// <param name="channelGuid">The channel, as the browser names it.</param>
        /// <param name="personAliasGuid">The person removed, by any of their aliases.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the membership is removed in.</param>
        /// <returns>Whether they were removed, and whether the platform had taken it yet.</returns>
        internal static async Task<ChatMembershipResultBag> RemoveMemberAsync( Person caller, Guid channelGuid, Guid personAliasGuid, ChatSessionContext context, RockContext rockContext )
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

            var room = ReadRoom( rockContext, context.Configuration, channelGuid, new[] { caller.Id, targetId.Value } );
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

            var targetName = new PersonService( rockContext ).Get( targetId.Value ).FullName;
            RemoveMember( rockContext, room.GroupId, targetId.Value );
            rockContext.SaveChanges();

            var isDirect = room.Channel.ChannelType == DirectChannelType;
            return await FinishAsync( context.Configuration, rockContext, channelGuid, isDirect ? caller.FullName + " removed " + targetName + "." : null ).ConfigureAwait( false );
        }

        /// <summary>
        /// Renames a group conversation the caller is in, or gives it back its members' names when
        /// the name is blank.
        /// </summary>
        /// <param name="caller">The person renaming.</param>
        /// <param name="channelGuid">The conversation, as the browser names it.</param>
        /// <param name="name">The new name, or blank for none.</param>
        /// <param name="context">The church's settings, for the session gates.</param>
        /// <param name="rockContext">The context the group is saved in.</param>
        /// <returns>Whether it was renamed, and whether the platform had taken it yet.</returns>
        internal static async Task<ChatMembershipResultBag> RenameConversationAsync( Person caller, Guid channelGuid, string name, ChatSessionContext context, RockContext rockContext )
        {
            var gate = ChatSessionHelper.Evaluate( caller, context, rockContext );
            if ( !gate.Success )
            {
                return Membership( ChatSessionHelper.ToGateCode( gate.Gate ), "Chat is not available to you right now." );
            }

            var room = ReadRoom( rockContext, context.Configuration, channelGuid, new[] { caller.Id } );
            if ( room == null )
            {
                return Membership( "door.not_found", "That conversation could not be found." );
            }

            // A channel's name is staff's, set in Rock.
            if ( room.Channel.ChannelType != DirectChannelType )
            {
                return Membership( "door.not_allowed", "Only a group conversation can be renamed here." );
            }

            if ( !room.LivePersonIds.Contains( caller.Id ) )
            {
                return Membership( "door.not_member", "You're not in this conversation." );
            }

            if ( room.PersonCount <= 2 )
            {
                return Membership( "door.not_allowed", "A conversation of two is named by the other person." );
            }

            var trimmed = ( name ?? string.Empty ).Trim();
            if ( trimmed.Length > MaxNameLength )
            {
                return Membership( "door.bad_request", "A name is at most 100 characters." );
            }

            // Rock requires a name, and chat reads its placeholder as none, showing the members' names.
            var group = new GroupService( rockContext ).Get( room.GroupId );
            group.Name = trimmed.Length == 0 ? DirectMessageName : trimmed;
            ChatPlatformSyncHelper.RecordGroupChange( rockContext, channelGuid );
            rockContext.SaveChanges();

            var line = trimmed.Length == 0
                ? caller.FullName + " removed the conversation's name."
                : caller.FullName + " named the conversation " + trimmed + ".";

            return await FinishAsync( context.Configuration, rockContext, channelGuid, line ).ConfigureAwait( false );
        }

        #endregion The membership doors

        #region Workflow posts

        /// <summary>
        /// Sends a workflow's direct message from one person to another, the administrator's act:
        /// neither the recipient's Open DM nor the sender's direct message access is asked, both
        /// people are enrolled, and the conversation is made when they have none.
        /// </summary>
        /// <param name="senderPersonId">The person the message is from.</param>
        /// <param name="recipientPersonId">The person it is to.</param>
        /// <param name="body">The message.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The message posted, or the code and text of why it was not.</returns>
        internal static ChatDoorOutcome SendWorkflowDirectMessage( int senderPersonId, int recipientPersonId, string body, ChatPlatformConfiguration configuration )
        {
            return RunWorkflowPost( configuration, async stopwatch =>
            {
                if ( senderPersonId == recipientPersonId )
                {
                    return Outcome( "door.self", "A direct message needs a sender and a different recipient." );
                }

                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var people = new[] { personService.Get( senderPersonId ), personService.Get( recipientPersonId ) };

                    if ( people.Any( p => p == null ) )
                    {
                        return Outcome( "door.not_found", "The sender or the recipient could not be found." );
                    }

                    var refused = RefusedByGates( people, configuration, rockContext );
                    if ( refused != null )
                    {
                        return refused;
                    }

                    // Open DM and shared rooms are not asked, so the sharing read is skipped.
                    var personIds = people.Select( p => p.Id ).ToList();
                    var read = ReadDirectMessage( rockContext, configuration, senderPersonId, personIds, false );

                    // The administrator's act still does not step around a ban in the conversation.
                    if ( read.Existing.HasValue && read.BannedPersonIds.Any() )
                    {
                        return Outcome( "door.target_not_eligible", "Someone in this conversation is banned from it." );
                    }

                    // The workflow saves outside any request, which the save hooks do not push, so the
                    // conversation and both people are named for this context's commit.
                    Guid channelGuid;
                    if ( read.Existing.HasValue )
                    {
                        channelGuid = read.Existing.Value;
                        ChatPlatformSyncHelper.RecordGroupChange( rockContext, channelGuid );
                        personIds.ForEach( id => ChatPlatformSyncHelper.RecordPersonChange( rockContext, id ) );
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        channelGuid = CreateDirectMessage( rockContext, configuration.TenantId.Value, personIds, personIds );
                    }

                    // A push still pending is not checked: the post that follows names the reason
                    // the platform gives if the conversation has not reached it.
                    await ChatPlatformSyncHelper.FlushAsync( rockContext, Remaining( stopwatch ) ).ConfigureAwait( false );

                    return await PostAsync( configuration, channelGuid, body, people[0].PrimaryAliasGuid, stopwatch ).ConfigureAwait( false );
                }
            } );
        }

        /// <summary>
        /// Posts a workflow's message into a channel: under a person's name when a sender is given,
        /// enrolling them first, and as a system line when none is.
        /// </summary>
        /// <param name="groupGuid">The channel's group.</param>
        /// <param name="senderPersonId">The person the message is from, or null for a system line.</param>
        /// <param name="body">The message.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The message posted, or the code and text of why it was not.</returns>
        internal static ChatDoorOutcome SendWorkflowChannelMessage( Guid groupGuid, int? senderPersonId, string body, ChatPlatformConfiguration configuration )
        {
            return RunWorkflowPost( configuration, async stopwatch =>
            {
                // A system line is under nobody, so there is no one to gate or enrol.
                if ( !senderPersonId.HasValue )
                {
                    return await PostAsync( configuration, groupGuid, body, null, stopwatch ).ConfigureAwait( false );
                }

                using ( var rockContext = new RockContext() )
                {
                    var sender = new PersonService( rockContext ).Get( senderPersonId.Value );
                    if ( sender == null )
                    {
                        return Outcome( "door.not_found", "The sender could not be found." );
                    }

                    var refused = RefusedByGates( new[] { sender }, configuration, rockContext );
                    if ( refused != null )
                    {
                        return refused;
                    }

                    // A sender who has never opened chat has no row on the platform to post under.
                    ChatPlatformSyncHelper.RecordPersonChange( rockContext, sender.Id );
                    rockContext.SaveChanges();
                    await ChatPlatformSyncHelper.FlushAsync( rockContext, Remaining( stopwatch ) ).ConfigureAwait( false );

                    return await PostAsync( configuration, groupGuid, body, sender.PrimaryAliasGuid, stopwatch ).ConfigureAwait( false );
                }
            } );
        }

        /// <summary>
        /// Runs a workflow post once chat is set up, under one wait for the whole of it.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="post">The post, given the stopwatch its wait is measured on.</param>
        /// <returns>The post's outcome, or why chat could not take it.</returns>
        private static ChatDoorOutcome RunWorkflowPost( ChatPlatformConfiguration configuration, Func<Stopwatch, Task<ChatDoorOutcome>> post )
        {
            if ( configuration == null || !configuration.IsConfigured )
            {
                return Outcome( ChatSessionHelper.ToGateCode( ChatMintGate.NotConfigured ), "Chat is not set up for this church." );
            }

            // A workflow runs on a thread of its own with no request around it, so the awaits run on
            // the pool rather than deadlocking on a context nobody pumps.
            return Task.Run( () => post( Stopwatch.StartNew() ) ).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refuses a sender or recipient chat's gates keep out of a session, and enrols the rest.
        /// </summary>
        /// <returns>The refusal, or null where everyone may be messaged.</returns>
        private static ChatDoorOutcome RefusedByGates( IList<Person> people, ChatPlatformConfiguration configuration, RockContext rockContext )
        {
            // A workflow has no session of its own, so only the church's settings frame the gates.
            var context = new ChatSessionContext { Configuration = configuration };

            // Every person is checked before anyone is enrolled, so a refusal enrols nobody.
            foreach ( var person in people )
            {
                var gate = ChatSessionHelper.Evaluate( person, context, rockContext ).Gate;

                // Every gate a session obeys, the minimum age included, or a workflow becomes the
                // way around them: a person chat keeps out is neither written to nor written as.
                if ( gate != ChatMintGate.Ok )
                {
                    return Outcome( "door.target_not_eligible", string.Format( "{0} cannot be messaged in chat ({1}).", person.FullName, ChatSessionHelper.ToGateCode( gate ) ) );
                }
            }

            foreach ( var person in people )
            {
                ChatSessionHelper.EnsureEnrollment( person, context, rockContext );
            }

            return null;
        }

        /// <summary>
        /// Posts through the platform under the church's sync credential, within what is left of
        /// the workflow's wait.
        /// </summary>
        private static async Task<ChatDoorOutcome> PostAsync( ChatPlatformConfiguration configuration, Guid channelGuid, string body, Guid? senderAliasGuid, Stopwatch stopwatch )
        {
            using ( var timeout = new CancellationTokenSource( Remaining( stopwatch ) ) )
            {
                var answer = await ChatPlatformSyncHelper.SendSystemMessageAsync( configuration, channelGuid, body, senderAliasGuid, timeout.Token ).ConfigureAwait( false );

                // The platform's own code and sentence pass through, so the workflow log says why.
                if ( !answer.Id.HasValue )
                {
                    return new ChatDoorOutcome { Code = answer.Code, Message = answer.Message, ChannelGuid = channelGuid };
                }

                return new ChatDoorOutcome { Code = OkCode, ChannelGuid = channelGuid, MessageId = answer.Id };
            }
        }

        #endregion Workflow posts

        #region The read and the create

        /// <summary>
        /// What one projection read says about a set of people: each person's alias row, the live
        /// direct message of exactly them, and who shares a private room with the caller.
        /// </summary>
        private sealed class DirectMessageRead
        {
            public Dictionary<int, ChatPlatformSyncHelper.ProjectedAlias> People { get; set; }

            public Guid? Existing { get; set; }

            public HashSet<int> BannedPersonIds { get; } = new HashSet<int>();

            public HashSet<int> SharedPersonIds { get; } = new HashSet<int>();
        }

        /// <summary>
        /// Reads the people, the candidate conversations and the rooms they share through one
        /// scoped projection call.
        /// </summary>
        /// <param name="rockContext">The context read in.</param>
        /// <param name="configuration">The church's chat settings, which resolve the Open DM default.</param>
        /// <param name="callerId">The person starting the conversation.</param>
        /// <param name="personIds">Everyone in it, the caller included.</param>
        /// <param name="isSharingRead">Whether to read the rooms the caller shares with each person.</param>
        /// <returns>What the projection says.</returns>
        private static DirectMessageRead ReadDirectMessage( RockContext rockContext, ChatPlatformConfiguration configuration, int callerId, IList<int> personIds, bool isSharingRead )
        {
            var directMessageTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() ) ?? 0;
            var activeMembers = new GroupMemberService( rockContext ).Queryable()
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived );
            // A list, because Entity Framework translates a list's Contains and not every collection's.
            var people = personIds.ToList();
            var count = people.Count;

            // The caller's direct messages whose active members are exactly these people. Stream-era
            // twins are read too, and the lowest id among the live ones wins.
            var candidates = new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == directMessageTypeId && g.IsActive && !g.IsArchived )
                .Where( g => g.Members.Any( m => m.PersonId == callerId && m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived ) )
                .Where( g => g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived ).Select( m => m.PersonId ).Distinct().Count() == count
                    && !g.Members.Any( m => m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived && !people.Contains( m.PersonId ) ) )
                .OrderBy( g => g.Id )
                .Select( g => g.Guid )
                .ToList();

            // Rock's membership only proposes; the projection decides which candidate is live and
            // who in it is banned, as the platform will see them.
            var changes = new ChatPlatformSyncHelper.ImmediateChanges();
            changes.PersonIds.UnionWith( personIds );
            changes.GroupGuids.UnionWith( candidates );

            // Only the caller's and each person's own memberships of the groups they share are read,
            // so a room of thousands costs two rows, not its roster. The chat-allowed filter only
            // narrows the candidates; the projection decides which of them is a chat channel.
            if ( isSharingRead )
            {
                var callerGroupIds = activeMembers.Where( m => m.PersonId == callerId ).Select( m => m.GroupId );
                var shared = activeMembers
                    .Where( m => m.PersonId != callerId && people.Contains( m.PersonId ) && callerGroupIds.Contains( m.GroupId ) )
                    .Select( m => new { m.Group.GroupTypeId, m.Group.Guid, m.PersonId } )
                    .Distinct()
                    .ToList()
                    .Where( m => GroupTypeCache.Get( m.GroupTypeId )?.IsChatAllowed == true );

                foreach ( var membership in shared )
                {
                    changes.MemberKeys.Add( (membership.Guid, callerId) );
                    changes.MemberKeys.Add( (membership.Guid, membership.PersonId) );
                }
            }

            var projection = ChatPlatformSyncHelper.ReadScopedProjection( rockContext, configuration, changes );
            var read = new DirectMessageRead { People = projection.People };

            foreach ( var candidate in candidates )
            {
                // Members come only from live channels, and a member who is not one of these people
                // makes it a different conversation.
                var inIt = projection.Members.Where( m => m.ChannelGuid == candidate ).ToList();
                var isExact = projection.Channels.ContainsKey( candidate )
                    && inIt.All( m => m.PersonId.HasValue )
                    && new HashSet<int>( inIt.Select( m => m.PersonId.Value ) ).SetEquals( personIds );

                if ( isExact )
                {
                    read.Existing = candidate;
                    read.BannedPersonIds.UnionWith( inIt.Where( m => m.IsBanned ).Select( m => m.PersonId.Value ) );
                    break;
                }
            }

            // A private room counts when the caller and the person are both unbanned members of it.
            var privateRooms = projection.Channels.Where( c => !c.Value.IsPublic ).Select( c => c.Key ).ToList();
            foreach ( var room in privateRooms )
            {
                var unbanned = projection.Members.Where( m => m.ChannelGuid == room && !m.IsBanned && m.PersonId.HasValue ).Select( m => m.PersonId.Value ).ToList();
                if ( unbanned.Contains( callerId ) )
                {
                    read.SharedPersonIds.UnionWith( unbanned.Where( id => id != callerId ) );
                }
            }

            return read;
        }

        /// <summary>
        /// Creates the direct message group of these people under their derived Guid, falling back
        /// as a race or a changed group requires, and names it for the context's next commit.
        /// </summary>
        /// <param name="rockContext">The context the group is saved in, whose push the caller awaits.</param>
        /// <param name="tenantId">The church.</param>
        /// <param name="personIds">Everyone in it.</param>
        /// <param name="enrolledPersonIds">People whose own rows the push carries too, having just been enrolled.</param>
        /// <returns>The conversation's Guid.</returns>
        private static Guid CreateDirectMessage( RockContext rockContext, Guid tenantId, IList<int> personIds, IEnumerable<int> enrolledPersonIds )
        {
            var derived = DirectMessageGuid( tenantId, personIds );

            // Named before the save, so the push follows the commit whether or not a request made it.
            ChatPlatformSyncHelper.RecordGroupChange( rockContext, derived );
            foreach ( var personId in enrolledPersonIds )
            {
                ChatPlatformSyncHelper.RecordPersonChange( rockContext, personId );
            }

            var group = AddDirectMessageGroup( rockContext, derived, personIds );

            // The unique index on a group's Guid is the lock: two people starting the same
            // conversation at once both try, and exactly one save wins.
            try
            {
                rockContext.SaveChanges();
                return derived;
            }
            catch ( Exception exception ) when ( Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( exception, DirectMessageName ) != null )
            {
                // Taken back out of the context, so the next save does not try them again.
                foreach ( var member in group.Members.ToList() )
                {
                    rockContext.Entry( member ).State = EntityState.Detached;
                }

                rockContext.Entry( group ).State = EntityState.Detached;
            }

            // Read without the archived filter, so an archived holder is seen and passed over rather
            // than missed and mistaken for one with nobody in it.
            var directMessageTypeId = group.GroupTypeId;
            var holder = new GroupService( rockContext ).AsNoFilter().AsNoTracking()
                .Where( g => g.Guid == derived )
                .Select( g => new
                {
                    IsLive = g.IsActive && !g.IsArchived && g.GroupTypeId == directMessageTypeId,
                    PersonIds = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived ).Select( m => m.PersonId )
                } )
                .FirstOrDefault();

            // The other half of a race made it: the empty save pushes it again, so this caller is
            // answered only once the platform holds it too. An ended holder is never handed back,
            // because the platform holds it as gone and every send to it would fail.
            var isRaceWon = holder != null && holder.IsLive && new HashSet<int>( holder.PersonIds ).SetEquals( personIds );
            if ( isRaceWon )
            {
                rockContext.SaveChanges();
                return derived;
            }

            // The group holding the Guid has since gained or lost people, or has ended, so it is a
            // different conversation and this one gets a Guid of its own.
            var fallback = Guid.NewGuid();
            ChatPlatformSyncHelper.RecordGroupChange( rockContext, fallback );
            AddDirectMessageGroup( rockContext, fallback, personIds );
            rockContext.SaveChanges();

            return fallback;
        }

        /// <summary>
        /// Adds a direct message group and one active membership per person, unsaved.
        /// </summary>
        private static Group AddDirectMessageGroup( RockContext rockContext, Guid guid, IList<int> personIds )
        {
            var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() );

            var group = new Group
            {
                Guid = guid,
                GroupTypeId = groupType.Id,
                ParentGroupId = GroupCache.GetId( Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES.AsGuid() ),
                Name = DirectMessageName,
                IsActive = true
            };

            foreach ( var personId in personIds )
            {
                group.Members.Add( new GroupMember
                {
                    Group = group,
                    GroupTypeId = groupType.Id,
                    PersonId = personId,
                    GroupRoleId = groupType.DefaultGroupRoleId ?? 0,
                    GroupMemberStatus = GroupMemberStatus.Active
                } );
            }

            new GroupService( rockContext ).Add( group );

            return group;
        }

        #endregion The read and the create

        #region The membership read and writes

        /// <summary>
        /// What one projection read says about a channel and some of its people: the channel's
        /// own flags, which of them are in it and which are banned from it.
        /// </summary>
        private sealed class RoomRead
        {
            public int GroupId { get; set; }

            public ChatPlatformSyncHelper.ProjectedChannel Channel { get; set; }

            public HashSet<int> LivePersonIds { get; } = new HashSet<int>();

            public HashSet<int> BannedPersonIds { get; } = new HashSet<int>();

            /// <summary>
            /// How many people a conversation holds; zero for a channel, which is never counted.
            /// </summary>
            public int PersonCount { get; set; }
        }

        /// <summary>
        /// Reads a channel the browser named, and these people's memberships of it, through one
        /// scoped projection call.
        /// </summary>
        /// <param name="rockContext">The context read in.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="channelGuid">The channel.</param>
        /// <param name="personIds">The people whose memberships matter, the caller included.</param>
        /// <returns>The channel, or null when it is not a live chat channel.</returns>
        private static RoomRead ReadRoom( RockContext rockContext, ChatPlatformConfiguration configuration, Guid channelGuid, IEnumerable<int> personIds )
        {
            // An ended group keeps its row on the platform for its history, but nobody joins or is
            // added to it.
            var groupId = new GroupService( rockContext ).Queryable()
                .Where( g => g.Guid == channelGuid && g.IsActive )
                .Select( g => ( int? ) g.Id )
                .FirstOrDefault();
            if ( !groupId.HasValue )
            {
                return null;
            }

            // The channel's own row and these people's memberships of it, never its whole roster,
            // so a room of thousands costs what a room of two does. Whether it is a chat channel at
            // all is the projection's answer, the same one the platform is sent.
            var changes = new ChatPlatformSyncHelper.ImmediateChanges();
            changes.ChannelGuids.Add( channelGuid );
            foreach ( var personId in personIds )
            {
                changes.PersonIds.Add( personId );
                changes.MemberKeys.Add( (channelGuid, personId) );
            }

            var projection = ChatPlatformSyncHelper.ReadScopedProjection( rockContext, configuration, changes );
            if ( !projection.Channels.TryGetValue( channelGuid, out var channel ) )
            {
                return null;
            }

            var room = new RoomRead { GroupId = groupId.Value, Channel = channel };
            foreach ( var member in projection.Members.Where( m => m.ChannelGuid == channelGuid && m.PersonId.HasValue ) )
            {
                if ( member.IsBanned )
                {
                    room.BannedPersonIds.Add( member.PersonId.Value );
                }
                else
                {
                    room.LivePersonIds.Add( member.PersonId.Value );
                }
            }

            // A conversation's size decides what may be done to it; it holds nine at most, so
            // Rock's own count costs nothing.
            if ( channel.ChannelType == DirectChannelType )
            {
                room.PersonCount = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.GroupId == groupId.Value && m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( m => m.PersonId )
                    .Distinct()
                    .Count();
            }

            return room;
        }

        /// <summary>
        /// Why the caller may not add or remove people here: in a shared channel they need to
        /// manage its members, in a conversation of more than two they need to be in it.
        /// </summary>
        /// <returns>The refusal, or null where they may.</returns>
        private static ChatMembershipResultBag RefuseManaging( RockContext rockContext, RoomRead room, Person caller, string channelRefusal )
        {
            if ( room.Channel.ChannelType == DirectChannelType )
            {
                if ( !room.LivePersonIds.Contains( caller.Id ) )
                {
                    return Membership( "door.not_member", "You're not in this conversation." );
                }

                // A conversation of two stays private; adding someone starts a new one instead.
                return room.PersonCount <= 2
                    ? Membership( "door.not_allowed", "A conversation of two stays as it is; start a new conversation instead." )
                    : null;
            }

            // Rock's own rule for who manages a group's members: security, or a group role that
            // may manage members or edit.
            var group = new GroupService( rockContext ).Get( room.GroupId );
            var isManager = room.Channel.ChannelType == SharedChannelType
                && group.IsAuthorized( Rock.Security.Authorization.MANAGE_MEMBERS, caller );

            return isManager ? null : Membership( "door.not_allowed", channelRefusal );
        }

        /// <summary>
        /// Puts a person in a group, using a row they already hold, archived or inactive, so a
        /// return never leaves two side by side. Unsaved.
        /// </summary>
        private static void AddOrRestoreMember( RockContext rockContext, int groupId, int personId )
        {
            var service = new GroupMemberService( rockContext );
            var existing = service.AsNoFilter()
                .Where( m => m.GroupId == groupId && m.PersonId == personId )
                .OrderBy( m => m.IsArchived )
                .ThenBy( m => m.Id )
                .FirstOrDefault();

            if ( existing != null )
            {
                if ( existing.IsArchived )
                {
                    service.Restore( existing );
                }

                existing.GroupMemberStatus = GroupMemberStatus.Active;
                return;
            }

            var groupTypeId = new GroupService( rockContext ).GetSelect( groupId, g => g.GroupTypeId );
            service.Add( new GroupMember
            {
                GroupId = groupId,
                GroupTypeId = groupTypeId,
                PersonId = personId,
                GroupRoleId = GroupTypeCache.Get( groupTypeId ).DefaultGroupRoleId ?? 0,
                GroupMemberStatus = GroupMemberStatus.Active
            } );
        }

        /// <summary>
        /// Whether the person holds a row in the group, active or not, under a ban still in force.
        /// The projection reads only active rows, so a ban left on an inactive or archived row is
        /// asked here, before that row is brought back.
        /// </summary>
        private static bool HoldsBannedRow( RockContext rockContext, int groupId, int personId )
        {
            var now = RockDateTime.Now;

            return new GroupMemberService( rockContext ).AsNoFilter()
                .Any( m => m.GroupId == groupId && m.PersonId == personId && m.IsChatBanned
                    && ( !m.ChatBannedUntil.HasValue || m.ChatBannedUntil > now ) );
        }

        /// <summary>
        /// Takes a person out of a group, every row they hold in it. Rock decides whether each is
        /// deleted or archived, as it does for any removal from a group. Unsaved.
        /// </summary>
        private static void RemoveMember( RockContext rockContext, int groupId, int personId )
        {
            var service = new GroupMemberService( rockContext );
            foreach ( var member in service.Queryable().Where( m => m.GroupId == groupId && m.PersonId == personId ).ToList() )
            {
                service.Delete( member );
            }
        }

        /// <summary>
        /// Waits for the saved change to reach the platform, then posts the room's line for it.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="rockContext">The context the change was saved in, whose push is awaited.</param>
        /// <param name="channelGuid">The channel the line goes to.</param>
        /// <param name="line">The line, or null where this change posts none.</param>
        /// <returns>Done, and whether the platform had taken the change yet.</returns>
        private static async Task<ChatMembershipResultBag> FinishAsync( ChatPlatformConfiguration configuration, RockContext rockContext, Guid channelGuid, string line )
        {
            var push = await ChatPlatformSyncHelper.FlushAsync( rockContext ).ConfigureAwait( false );

            if ( line != null )
            {
                // What the person asked for was the change, and it is made; a line the platform
                // cannot take is logged and the door still answers that it is done.
                using ( var timeout = new CancellationTokenSource( ChatPlatformSyncHelper.AwaitedPushBudget ) )
                {
                    var answer = await ChatPlatformSyncHelper.SendSystemMessageAsync( configuration, channelGuid, line, null, timeout.Token ).ConfigureAwait( false );
                    if ( !answer.Id.HasValue )
                    {
                        RockLogger.LoggerFactory.CreateLogger( typeof( ChatDoorHelper ).FullName )
                            .LogWarning( "A chat line was not posted to {ChannelGuid}: {Code} {Message}", channelGuid, answer.Code, answer.Message );
                    }
                }
            }

            return new ChatMembershipResultBag
            {
                Code = OkCode,
                IsPending = push == ChatPlatformSyncHelper.PushOutcome.Pending
            };
        }

        /// <summary>
        /// Names the people a line lists: "Ada", "Ada and Bo", "Ada, Bo and Cy".
        /// </summary>
        private static string JoinNames( IList<string> names )
        {
            return names.Count == 1
                ? names[0]
                : string.Join( ", ", names.Take( names.Count - 1 ) ) + " and " + names.Last();
        }

        #endregion The membership read and writes

        #region Person merge

        /// <summary>
        /// Archives each live direct message of a person whose active members are exactly those of
        /// an older live one, keeping the oldest of each set.
        /// </summary>
        /// <param name="rockContext">The context the merge runs in, read after the merge moved the memberships.</param>
        /// <param name="personId">The person the merge kept.</param>
        /// <returns>The conversations archived, not yet saved.</returns>
        /// <remarks>
        /// When each of two merged people had a conversation with the same third person, both hold
        /// the same people once they are one. The oldest is the one the door already reopens, so it
        /// is kept, and the chat platform moves the history of each archived one into it.
        /// </remarks>
        internal static List<Group> ArchiveDirectMessageTwins( RockContext rockContext, int personId )
        {
            var directMessageTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() ) ?? 0;
            var groupService = new GroupService( rockContext );

            var conversations = groupService.Queryable()
                .Where( g => g.GroupTypeId == directMessageTypeId && g.IsActive && !g.IsArchived )
                .Where( g => g.Members.Any( m => m.PersonId == personId && m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived ) )
                .Select( g => new
                {
                    Group = g,
                    People = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived ).Select( m => m.PersonId ).Distinct()
                } )
                .ToList();

            var twins = conversations
                .GroupBy( c => string.Join( ",", c.People.OrderBy( id => id ) ) )
                .SelectMany( set => set.OrderBy( c => c.Group.Id ).Skip( 1 ) )
                .Select( c => c.Group )
                .ToList();

            foreach ( var twin in twins )
            {
                groupService.Archive( twin, null, false );
            }

            return twins;
        }

        #endregion Person merge

        #region Support

        /// <summary>
        /// Swaps a Guid's first three fields between .NET's little-endian layout and the network
        /// order RFC 4122 hashes and writes, which is the same swap both ways.
        /// </summary>
        private static byte[] ToNetworkOrder( byte[] bytes )
        {
            var swapped = ( byte[] ) bytes.Clone();
            Array.Reverse( swapped, 0, 4 );
            Array.Reverse( swapped, 4, 2 );
            Array.Reverse( swapped, 6, 2 );

            return swapped;
        }

        /// <summary>
        /// What is left of a workflow's wait, never below zero.
        /// </summary>
        private static TimeSpan Remaining( Stopwatch stopwatch )
        {
            var remaining = WorkflowWait - stopwatch.Elapsed;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// A door refusal with the sentence the person reads.
        /// </summary>
        private static ChatDirectMessageResultBag Refuse( string code, string message )
        {
            return new ChatDirectMessageResultBag { Code = code, Message = message };
        }

        /// <summary>
        /// The refusal that names the person who cannot be put in the conversation.
        /// </summary>
        private static ChatDirectMessageResultBag NotEligible( Guid? personAliasGuid, string nickName, string lastName )
        {
            var name = $"{nickName} {lastName}".Trim();

            return new ChatDirectMessageResultBag
            {
                Code = "door.target_not_eligible",
                PersonAliasGuid = personAliasGuid,
                Message = name.IsNullOrWhiteSpace() ? "Someone chosen cannot be messaged." : name + " cannot be messaged."
            };
        }

        /// <summary>
        /// A membership door's answer with the sentence the person reads.
        /// </summary>
        private static ChatMembershipResultBag Membership( string code, string message )
        {
            return new ChatMembershipResultBag { Code = code, Message = message };
        }

        /// <summary>
        /// The refusal that names the person who cannot be added.
        /// </summary>
        private static ChatMembershipResultBag NotAddable( Guid personAliasGuid, string name )
        {
            return new ChatMembershipResultBag
            {
                Code = "door.target_not_eligible",
                PersonAliasGuid = personAliasGuid,
                Message = name.IsNullOrWhiteSpace() ? "Someone chosen cannot be added." : name + " cannot be added."
            };
        }

        /// <summary>
        /// A workflow refusal with the sentence for its log.
        /// </summary>
        private static ChatDoorOutcome Outcome( string code, string message )
        {
            return new ChatDoorOutcome { Code = code, Message = message };
        }

        #endregion Support
    }

    /// <summary>
    /// What a workflow's post came to: the platform's code and sentence when it was refused, and
    /// the conversation and message when it was made.
    /// </summary>
    internal sealed class ChatDoorOutcome
    {
        /// <summary>
        /// "ok", or the refusal's code: a door's own, or the platform's, such as rpc.channel_not_found.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The sentence for the workflow log when it was refused.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The conversation posted to, when there is one.
        /// </summary>
        public Guid? ChannelGuid { get; set; }

        /// <summary>
        /// The message's id on the platform, when it was posted.
        /// </summary>
        public long? MessageId { get; set; }
    }
}
