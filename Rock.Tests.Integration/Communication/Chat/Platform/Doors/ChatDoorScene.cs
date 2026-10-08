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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Communication.Chat.Platform.Sync;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// A church with chat set up, a lobby that enrols each person in chat without sharing a
    /// private room, and the immediate sync sending to a stand-in.
    /// </summary>
    internal sealed class ChatDoorScene : IDisposable
    {
        internal const string ProjectUrl = "https://example.supabase.co";

        internal const string PushPath = "/rest/v1/rpc/sync_push";

        internal const string ExchangePath = "/functions/v1/token-exchange";

        internal const string SystemPostPath = "/rest/v1/rpc/chat_send_system_message";

        internal static readonly TimeSpan PushWait = TimeSpan.FromSeconds( 30 );

        private readonly Guid _lobby;

        private readonly List<Guid> _made = new List<Guid>();

        private readonly object _sync = new object();

        public ChatDoorScene( Guid? directMessageAccess = null, int? minimumAge = null )
        {
            Fixture = new ChatSyncProjectionFixture();

            const string kid = "kid-door-test";
            Configuration = new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = ChatDoorScene.ProjectUrl,
                PublishableKey = "sb_publishable_test",
                Kid = kid,
                PrivateKey = ChatSyncProjectionFixture.CreateSigningKey( kid ).PrivateJwk,
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = false,
                DirectMessageAccessDataViewGuid = directMessageAccess,
                MinimumAge = minimumAge,
                ChatBadgeDataViewGuids = new List<Guid>()
            };
            Fixture.StoreConfiguration( Configuration );

            Platform = new PlatformStandIn();
            Override = ChatPlatformSyncHelper.OverrideImmediateSync( Platform );

            // The fixture's shared type is public, so a lobby of it enrols without sharing.
            _lobby = Fixture.AddChannel( Fixture.SharedGroupTypeId, "Lobby" );
        }

        public ChatSyncProjectionFixture Fixture { get; }

        public ChatPlatformConfiguration Configuration { get; }

        public Guid TenantId => Configuration.TenantId.Value;

        public PlatformStandIn Platform { get; }

        public ChatPlatformSyncHelper.ImmediateSyncOverride Override { get; }

        /// <summary>
        /// A person enrolled in chat through the public lobby, with their Open DM setting.
        /// </summary>
        public int AddChatPerson( string lastName, bool? isOpenDmAllowed = null )
        {
            var personId = Fixture.AddPerson( lastName );
            Fixture.AddMember( _lobby, personId );

            if ( isOpenDmAllowed.HasValue )
            {
                SetOpenDm( personId, isOpenDmAllowed.Value );
            }

            return personId;
        }

        public void SetOpenDm( int personId, bool isOpenDmAllowed )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand(
                    "UPDATE [Person] SET [IsChatOpenDirectMessageAllowed] = @p1 WHERE [Id] = @p0", personId, isOpenDmAllowed );
            }
        }

        public void AddToBanList( int personId )
        {
            Fixture.AddMember( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid(), personId );
        }

        public void MakeDeceased( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [Person] SET [IsDeceased] = 1 WHERE [Id] = @p0", personId );
            }
        }

        public void MakeInactive( int personId )
        {
            var inactive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [Person] SET [RecordStatusValueId] = @p1 WHERE [Id] = @p0", personId, inactive );
            }
        }

        /// <summary>
        /// Gives a person a birthdate that makes them exactly this many years old today.
        /// </summary>
        public void SetAge( int personId, int years )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( personId );
                person.SetBirthDate( RockDateTime.Today.AddYears( -years ) );
                rockContext.SaveChanges();
            }
        }

        public Guid Alias( int personId )
        {
            return Fixture.PrimaryAliasGuid( personId );
        }

        public Person Person( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new PersonService( rockContext ).Queryable().AsNoTracking().Single( p => p.Id == personId );
            }
        }

        /// <summary>
        /// Saves one of the caller's own settings inside a request of its own, as the block does.
        /// </summary>
        public ChatPersonSettingResultBag SaveSetting( int callerId, string setting, bool value )
        {
            using ( ChatSyncProjectionFixture.InsideRequest() )
            using ( var rockContext = new RockContext() )
            {
                var caller = new PersonService( rockContext ).Get( callerId );
                var context = ChatSessionHelper.BuildSessionContext( caller, Configuration, rockContext );

                return ChatDoorHelper.SavePersonSettingAsync( caller, setting, value, context, rockContext ).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Starts a conversation as the caller, inside a request of its own, as the block does.
        /// </summary>
        public ChatDirectMessageResultBag Start( int callerId, params Guid[] personAliasGuids )
        {
            ChatDirectMessageResultBag result;

            using ( ChatSyncProjectionFixture.InsideRequest() )
            using ( var rockContext = new RockContext() )
            {
                var caller = new PersonService( rockContext ).Get( callerId );
                var context = ChatSessionHelper.BuildSessionContext( caller, Configuration, rockContext );

                result = ChatDoorHelper.StartDirectMessageAsync( caller, personAliasGuids, context, rockContext ).GetAwaiter().GetResult();
            }

            if ( result.ChannelGuid.HasValue )
            {
                lock ( _sync )
                {
                    _made.Add( result.ChannelGuid.Value );
                }
            }

            return result;
        }

        /// <summary>
        /// Sends a workflow's direct message outside any request, as the workflow engine does.
        /// </summary>
        public ChatDoorOutcome SendWorkflowDirectMessage( int senderId, int recipientId, string body )
        {
            var outcome = ChatDoorHelper.SendWorkflowDirectMessage( senderId, recipientId, body, Configuration );

            if ( outcome.ChannelGuid.HasValue )
            {
                lock ( _sync )
                {
                    _made.Add( outcome.ChannelGuid.Value );
                }
            }

            return outcome;
        }

        /// <summary>
        /// Posts a workflow's message into a channel outside any request, as the workflow engine does.
        /// </summary>
        public ChatDoorOutcome SendWorkflowChannelMessage( Guid groupGuid, int? senderId, string body )
        {
            return ChatDoorHelper.SendWorkflowChannelMessage( groupGuid, senderId, body, Configuration );
        }

        /// <summary>
        /// Joins a channel as the caller, inside a request of its own, as the block does.
        /// </summary>
        public ChatMembershipResultBag Join( int callerId, Guid channelGuid )
        {
            return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.JoinChannelAsync( caller, channelGuid, context, rockContext ) );
        }

        /// <summary>
        /// Leaves a channel or a conversation as the caller, inside a request of its own.
        /// </summary>
        public ChatMembershipResultBag Leave( int callerId, Guid channelGuid )
        {
            return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.LeaveChannelAsync( caller, channelGuid, context, rockContext ) );
        }

        /// <summary>
        /// Adds people to a channel or a conversation as the caller, inside a request of its own.
        /// </summary>
        public ChatMembershipResultBag Add( int callerId, Guid channelGuid, params Guid[] personAliasGuids )
        {
            return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.AddMembersAsync( caller, channelGuid, personAliasGuids, context, rockContext ) );
        }

        /// <summary>
        /// Removes a person from a channel or a conversation as the caller, inside a request of its own.
        /// </summary>
        public ChatMembershipResultBag Remove( int callerId, Guid channelGuid, Guid personAliasGuid )
        {
            return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.RemoveMemberAsync( caller, channelGuid, personAliasGuid, context, rockContext ) );
        }

        /// <summary>
        /// Renames a conversation as the caller, inside a request of its own.
        /// </summary>
        public ChatMembershipResultBag Rename( int callerId, Guid channelGuid, string name )
        {
            return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.RenameConversationAsync( caller, channelGuid, name, context, rockContext ) );
        }

        /// <summary>
        /// Runs a door as the caller inside a request of its own, with the caller's session context.
        /// </summary>
        private ChatMembershipResultBag AsCaller( int callerId, Func<Person, ChatSessionContext, RockContext, Task<ChatMembershipResultBag>> door )
        {
            using ( ChatSyncProjectionFixture.InsideRequest() )
            using ( var rockContext = new RockContext() )
            {
                var caller = new PersonService( rockContext ).Get( callerId );
                var context = ChatSessionHelper.BuildSessionContext( caller, Configuration, rockContext );

                return door( caller, context, rockContext ).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// A role on the fixture's shared type whose holders may manage a group's members.
        /// </summary>
        public int AddManagerRole()
        {
            var roleId = Fixture.AddRole( Fixture.SharedGroupTypeId, "Manager", isLeader: false );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [GroupTypeRole] SET [CanManageMembers] = 1 WHERE [Id] = @p0", roleId );
            }

            return roleId;
        }

        /// <summary>
        /// Every membership a person holds in a group, archived ones included.
        /// </summary>
        public List<GroupMember> Memberships( Guid groupGuid, int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupMemberService( rockContext ).AsNoFilter().AsNoTracking()
                    .Where( m => m.Group.Guid == groupGuid && m.PersonId == personId )
                    .ToList();
            }
        }

        /// <summary>
        /// The person's name as a system line writes it.
        /// </summary>
        public string Name( int personId )
        {
            return Person( personId ).FullName;
        }

        public Group Group( Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupService( rockContext ).Queryable().AsNoTracking().Single( g => g.Guid == groupGuid );
            }
        }

        /// <summary>
        /// The group by its Guid, archived or not.
        /// </summary>
        public Group AnyGroup( Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupService( rockContext ).AsNoFilter().AsNoTracking().Single( g => g.Guid == groupGuid );
            }
        }

        public List<int> ActiveMembers( Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.Group.Guid == groupGuid && m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived )
                    .Select( m => m.PersonId )
                    .ToList();
            }
        }

        public int ActiveMemberCount( Guid groupGuid )
        {
            return ActiveMembers( groupGuid ).Count;
        }

        /// <summary>
        /// The direct message groups whose active members are exactly these people.
        /// </summary>
        public List<Guid> DirectMessagesWithExactly( params int[] personIds )
        {
            var directMessageTypeId = Fixture.DirectMessageGroupTypeId;
            var personCount = personIds.Length;

            using ( var rockContext = new RockContext() )
            {
                return new GroupService( rockContext ).Queryable()
                    .Where( g => g.GroupTypeId == directMessageTypeId )
                    .Select( g => new
                    {
                        g.Guid,
                        People = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active && !m.IsArchived ).Select( m => m.PersonId )
                    } )
                    .Where( g => g.People.Count() == personCount && g.People.All( p => personIds.Contains( p ) ) )
                    .Select( g => g.Guid )
                    .ToList();
            }
        }

        public List<RecordedPush> WaitForPushes()
        {
            Assert.IsTrue( Override.WaitForPushes( ChatDoorScene.PushWait ), "a push was still running when its wait ran out" );

            return Platform.Pushes;
        }

        public void Dispose()
        {
            Override.WaitForPushes( ChatDoorScene.PushWait );
            Override.Dispose();
            Platform.Dispose();

            // The groups the door made carry no fixture mark, so they are taken away by Guid.
            using ( var rockContext = new RockContext() )
            {
                foreach ( var guid in _made.Distinct() )
                {
                    rockContext.Database.ExecuteSqlCommand(
                        "DELETE FROM [GroupMember] WHERE [GroupId] IN ( SELECT [Id] FROM [Group] WHERE [Guid] = @p0 );"
                        + "DELETE FROM [Group] WHERE [Guid] = @p0;",
                        guid );
                }
            }

            Fixture.Dispose();
        }
    }

    /// <summary>
    /// One push as the stand-in received it: the channels and the people it carried.
    /// </summary>
    internal sealed class RecordedPush
    {
        public List<Guid> ChannelGuids { get; set; }

        public List<Newtonsoft.Json.Linq.JArray> Aliases { get; set; }

        public List<Newtonsoft.Json.Linq.JArray> Channels { get; set; }

        public List<Newtonsoft.Json.Linq.JArray> Members { get; set; }

        /// <summary>
        /// The memberships the push stamped absent, each as its channel and primary alias.
        /// </summary>
        public List<(Guid ChannelGuid, Guid AliasGuid)> AbsentMembers { get; set; }

        /// <summary>
        /// The channels the push stamped absent.
        /// </summary>
        public List<Guid> AbsentChannels { get; set; }
    }

    /// <summary>
    /// One system line as the stand-in received it.
    /// </summary>
    internal sealed class PostedLine
    {
        public Guid ChannelGuid { get; set; }

        public string Body { get; set; }

        public Guid? SenderAliasGuid { get; set; }
    }

    /// <summary>
    /// Answers the exchange and the push as the platform would, after a delay a test may set.
    /// </summary>
    internal sealed class PlatformStandIn : HttpMessageHandler
    {
        private readonly object _sync = new object();

        private readonly List<RecordedPush> _pushes = new List<RecordedPush>();

        private readonly List<PostedLine> _lines = new List<PostedLine>();

        public TimeSpan PushDelay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// How many posts Rock has made through the system post.
        /// </summary>
        public int Posts { get; private set; }

        /// <summary>
        /// Whether the system post fails, as a platform that cannot take it would.
        /// </summary>
        public bool IsPostFailing { get; set; }

        /// <summary>
        /// Each system line Rock has posted, in order.
        /// </summary>
        public List<PostedLine> Lines
        {
            get
            {
                lock ( _sync )
                {
                    return _lines.ToList();
                }
            }
        }

        public List<RecordedPush> Pushes
        {
            get
            {
                lock ( _sync )
                {
                    return _pushes.ToList();
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            var path = request.RequestUri.AbsolutePath;

            if ( path == ChatDoorScene.ExchangePath )
            {
                return Answer( "{\"access_token\":\"exchanged.platform.token\",\"token_type\":\"bearer\",\"expires_in\":300}" );
            }

            if ( path == ChatDoorScene.SystemPostPath )
            {
                if ( IsPostFailing )
                {
                    return new HttpResponseMessage( HttpStatusCode.ServiceUnavailable );
                }

                var post = Newtonsoft.Json.Linq.JObject.Parse( await request.Content.ReadAsStringAsync() );
                lock ( _sync )
                {
                    Posts++;
                    _lines.Add( new PostedLine
                    {
                        ChannelGuid = ( Guid ) post["p_channel_id"],
                        Body = ( string ) post["p_body"],
                        SenderAliasGuid = ( Guid? ) post["p_sender_alias"]
                    } );
                }

                return Answer( "{\"id\":1,\"created_at\":\"2026-10-06T00:00:00.000000+00:00\"}" );
            }

            if ( path != ChatDoorScene.PushPath )
            {
                return new HttpResponseMessage( HttpStatusCode.NotFound );
            }

            var body = Newtonsoft.Json.Linq.JObject.Parse( await request.Content.ReadAsStringAsync() );
            lock ( _sync )
            {
                _pushes.Add( new RecordedPush
                {
                    ChannelGuids = body["channels"].Select( r => ( Guid ) r[0] ).ToList(),
                    Aliases = body["aliases"].Cast<Newtonsoft.Json.Linq.JArray>().ToList(),
                    Channels = body["channels"].Cast<Newtonsoft.Json.Linq.JArray>().ToList(),
                    Members = body["members"].Cast<Newtonsoft.Json.Linq.JArray>().ToList(),
                    AbsentMembers = ( body["absent"]?["members"] ?? new Newtonsoft.Json.Linq.JArray() )
                        .Select( k => ( ( Guid ) k[0], ( Guid ) k[1] ) )
                        .ToList(),
                    AbsentChannels = ( body["absent"]?["channels"] ?? new Newtonsoft.Json.Linq.JArray() )
                        .Select( k => ( Guid ) k )
                        .ToList()
                } );
            }

            if ( PushDelay > TimeSpan.Zero )
            {
                await Task.Delay( PushDelay, cancellationToken );
            }

            var counters = "{\"inserted\":0,\"updated\":0,\"suppressed\":0,\"skipped_stale\":0,\"absent\":0}";

            return Answer( "{\"aliases\":" + counters + ",\"channels\":" + counters + ",\"members\":" + counters + "}" );
        }

        private static HttpResponseMessage Answer( string json )
        {
            return new HttpResponseMessage( HttpStatusCode.OK )
            {
                Content = new StringContent( json, Encoding.UTF8, "application/json" )
            };
        }
    }
}
