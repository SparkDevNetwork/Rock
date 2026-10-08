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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Direct messages and workflow posts against a real chat platform: the conversation Rock makes,
    /// on the platform before the door answers, and the message a workflow posts, read back as the
    /// platform stored it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every other test of these paths stops at a stand-in, which can agree with Rock about a
    ///         call the real platform refuses. These make the real calls: the push the door awaits,
    ///         and the post under the church's sync credential. Each test provisions its own church.
    ///     </para>
    ///     <para>
    ///         They run only when a chat platform is named in the environment, as
    ///         <see cref="LocalChatPlatform"/> describes.
    ///     </para>
    /// </remarks>
    [TestClass]
    [TestCategory( "ChatPlatformEndToEnd" )]
    public class DirectMessageEndToEndTests : DatabaseTestsBase
    {
        #region The door

        [TestMethod]
        public void TheDoorsConversationsAreOnThePlatformWhenItAnswers()
        {
            using ( var scene = new Scene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo" );
                var cy = scene.AddChatPerson( "Cy" );

                foreach ( var people in new[] { new[] { bo }, new[] { bo, cy } } )
                {
                    ChatDirectMessageResultBag result;

                    using ( ChatSyncProjectionFixture.InsideRequest() )
                    using ( var rockContext = new RockContext() )
                    {
                        var caller = new PersonService( rockContext ).Get( ada );
                        var context = ChatSessionHelper.BuildSessionContext( caller, scene.Configuration, rockContext );
                        result = ChatDoorHelper.StartDirectMessageAsync( caller, people.Select( scene.Fixture.PrimaryAliasGuid ), context, rockContext )
                            .GetAwaiter().GetResult();
                    }

                    scene.Made( result.ChannelGuid );
                    Assert.AreEqual( "ok", result.Code, result.Message );
                    Assert.IsFalse( result.IsPending, "the push landed within the door's budget" );

                    // Read at once, not waited for: the door's answer is the promise that it is there.
                    var channel = scene.Platform.WaitForChannel( scene.TenantId, result.ChannelGuid.Value, r => true );
                    Assert.IsNotNull( channel, "the conversation is on the platform when the door answers" );
                    Assert.AreEqual( "dm", ( string ) channel["channel_type"] );

                    foreach ( var personId in people.Concat( new[] { ada } ) )
                    {
                        var member = scene.Platform.WaitForMember( scene.TenantId, result.ChannelGuid.Value, scene.Fixture.PrimaryAliasGuid( personId ), r => true );
                        Assert.IsNotNull( member, "every person in it is a member there, person " + personId );
                    }
                }
            }
        }

        #endregion The door

        #region The membership doors

        [TestMethod]
        public void AJoinThroughTheDoorIsOnThePlatformWithItsLineWhenItAnswers()
        {
            using ( var scene = new Scene() )
            {
                scene.Platform.AddSystemAuthor( scene.TenantId );
                var ada = scene.AddChatPerson( "Ada" );
                var pip = scene.AddChatPerson( "Pip" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Push( room, pip );
                Assert.IsNotNull( scene.Platform.WaitForChannel( scene.TenantId, room, r => r != null ), "the room is on the platform" );

                var result = scene.Join( ada, room );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.IsFalse( result.IsPending, "the push landed within the door's budget" );

                // Read at once, not waited for: the door's answer is the promise that it is there.
                var member = scene.Platform.WaitForMember( scene.TenantId, room, scene.Fixture.PrimaryAliasGuid( ada ), r => true );
                Assert.IsNotNull( member, "the person is a member on the platform when the door answers" );
                Assert.IsTrue( member["absent_since"].Type == Newtonsoft.Json.Linq.JTokenType.Null );

                var name = new PersonService( new RockContext() ).Get( ada ).FullName;
                var line = scene.Platform.WaitForSystemLine( scene.TenantId, room, name + " joined the channel." );
                Assert.IsNotNull( line, "the join line is in the room" );
                Assert.IsTrue( line["person_alias_guid"].Type == Newtonsoft.Json.Linq.JTokenType.Null || ( Guid ) line["person_alias_guid"] == Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid(),
                    "under nobody but chat itself" );

                // How long a person waits on a join: twenty more, each after a leave, timed from
                // the call to its answer, the awaited push and the line included.
                var elapsed = new List<double>();
                var pending = 0;
                for ( var i = 0; i < 20; i++ )
                {
                    Assert.AreEqual( "ok", scene.Leave( ada, room ).Code );

                    var stopwatch = Stopwatch.StartNew();
                    var again = scene.Join( ada, room );
                    elapsed.Add( stopwatch.Elapsed.TotalMilliseconds );

                    Assert.AreEqual( "ok", again.Code, again.Message );
                    pending += again.IsPending ? 1 : 0;
                }

                elapsed.Sort();
                TestContext.WriteLine( string.Join( Environment.NewLine,
                    $"joins timed: {elapsed.Count}, from the call to its answer, the awaited push and the line included; pending: {pending}",
                    $"p50 ms: {Percentile( elapsed, 0.50 ).ToString( "F1", CultureInfo.InvariantCulture )}",
                    $"p95 ms: {Percentile( elapsed, 0.95 ).ToString( "F1", CultureInfo.InvariantCulture )}",
                    $"max ms: {elapsed.Last().ToString( "F1", CultureInfo.InvariantCulture )}" ) );
            }
        }

        [TestMethod]
        public void AGroupConversationsNewNameIsOnThePlatformWhenTheDoorAnswers()
        {
            using ( var scene = new Scene() )
            {
                scene.Platform.AddSystemAuthor( scene.TenantId );
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo" );
                var cy = scene.AddChatPerson( "Cy" );
                var group = scene.Start( ada, bo, cy );

                var result = scene.Rename( bo, group, "Trip planning" );

                Assert.AreEqual( "ok", result.Code, result.Message );
                var channel = scene.Platform.WaitForChannel( scene.TenantId, group, r => true );
                Assert.AreEqual( "Trip planning", ( string ) channel["name"], "the name is on the platform when the door answers" );
            }
        }

        #endregion The membership doors

        #region Workflow posts

        [TestMethod]
        public void AWorkflowsDirectMessageEnrolsARecipientWhoNeverOpenedChatAndPostsAsTheSender()
        {
            using ( var scene = new Scene() )
            {
                var sender = scene.Fixture.AddPerson( "Sender" );
                var recipient = scene.Fixture.AddPerson( "Recipient" );

                var outcome = ChatDoorHelper.SendWorkflowDirectMessage( sender, recipient, "Welcome to the church", scene.Configuration );
                scene.Made( outcome.ChannelGuid );

                Assert.AreEqual( "ok", outcome.Code, outcome.Message );
                var message = scene.Platform.ReadMessage( scene.TenantId, outcome.MessageId.Value );
                Assert.IsNotNull( message, "the message the platform answered with is stored" );
                Assert.AreEqual( "text", ( string ) message["message_type"], "a message under a person's name is an ordinary message" );
                Assert.AreEqual( outcome.ChannelGuid.Value, ( Guid ) message["channel_id"] );
                Assert.AreEqual( scene.Fixture.PrimaryAliasGuid( sender ), ( Guid ) message["person_alias_guid"] );
                Assert.AreEqual( "Welcome to the church", ( string ) message["body"] );
                Assert.IsNotNull( scene.Platform.WaitForMember( scene.TenantId, outcome.ChannelGuid.Value, scene.Fixture.PrimaryAliasGuid( recipient ), r => r != null ),
                    "the recipient, enrolled on the way, is in the conversation" );
            }
        }

        [TestMethod]
        public void AWorkflowsChannelPostIsASystemLineWithNoSenderAndAnOrdinaryMessageWithOne()
        {
            using ( var scene = new Scene() )
            {
                scene.Platform.AddSystemAuthor( scene.TenantId );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Workflow room", g => g.IsChatChannelPublicOverride = false );
                var member = scene.Fixture.AddPerson( "Member" );

                // The room reaches the platform the ordinary way, through a save inside a request.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == room );
                    new GroupMemberService( rockContext ).Add( new GroupMember
                    {
                        GroupId = group.Id,
                        GroupTypeId = group.GroupTypeId,
                        PersonId = member,
                        GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,
                        GroupMemberStatus = GroupMemberStatus.Active
                    } );
                    rockContext.SaveChanges();
                    ChatPlatformSyncHelper.FlushAsync( rockContext ).GetAwaiter().GetResult();
                }

                Assert.IsNotNull( scene.Platform.WaitForChannel( scene.TenantId, room, r => r != null ), "the room is on the platform" );

                var line = ChatDoorHelper.SendWorkflowChannelMessage( room, null, "The service starts at ten", scene.Configuration );
                Assert.AreEqual( "ok", line.Code, line.Message );
                Assert.AreEqual( "system", ( string ) scene.Platform.ReadMessage( scene.TenantId, line.MessageId.Value )["message_type"] );

                // A staff member who is not in the room and has never opened chat.
                var staff = scene.Fixture.AddPerson( "Staff" );
                var post = ChatDoorHelper.SendWorkflowChannelMessage( room, staff, "Reminder from the office", scene.Configuration );
                Assert.AreEqual( "ok", post.Code, post.Message );

                var stored = scene.Platform.ReadMessage( scene.TenantId, post.MessageId.Value );
                Assert.AreEqual( "text", ( string ) stored["message_type"] );
                Assert.AreEqual( scene.Fixture.PrimaryAliasGuid( staff ), ( Guid ) stored["person_alias_guid"] );
            }
        }

        [TestMethod]
        public void AWorkflowPostToAGroupThePlatformHasNeverSeenIsRefusedWithItsCode()
        {
            using ( var scene = new Scene() )
            {
                scene.Platform.AddSystemAuthor( scene.TenantId );

                // Made outside a request, so no push carried it.
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Never synced room" );

                var outcome = ChatDoorHelper.SendWorkflowChannelMessage( room, null, "Anyone there?", scene.Configuration );

                Assert.AreEqual( "rpc.channel_not_found", outcome.Code );
                Assert.IsFalse( string.IsNullOrWhiteSpace( outcome.Message ), "with the platform's sentence for the workflow log" );
                Assert.IsNull( outcome.MessageId );
            }
        }

        #endregion Workflow posts

        #region Person merge

        [TestMethod]
        public void APersonMergeLeavesOneConversationWithAllItsHistoryAndKeepsTheBan()
        {
            using ( var scene = new Scene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var lou = scene.AddChatPerson( "Lou" );
                var cal = scene.AddChatPerson( "Cal" );
                var adaAlias = scene.Fixture.PrimaryAliasGuid( ada );
                var louAlias = scene.Fixture.PrimaryAliasGuid( lou );

                // A room Lou is banned from and Ada is not, both members.
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Merge room" );
                scene.Push( room, ada );
                scene.Push( room, lou, m => m.IsChatBanned = true );

                // Cal's conversation with Lou first, so it is the oldest, then with Ada; one
                // message in each.
                var oldest = scene.Start( cal, lou );
                var twin = scene.Start( cal, ada );
                var first = ChatDoorHelper.SendWorkflowDirectMessage( lou, cal, "Before the merge, from Lou", scene.Configuration );
                var second = ChatDoorHelper.SendWorkflowDirectMessage( ada, cal, "Before the merge, from Ada", scene.Configuration );
                Assert.AreEqual( oldest, first.ChannelGuid, "the workflow reused Lou's conversation" );
                Assert.AreEqual( twin, second.ChannelGuid, "and Ada's" );

                // The Person Merge block's path, then the full sync that restates the church.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.Database.ExecuteSqlCommand( "EXEC dbo.spCrm_PersonMerge @p0, @p1", lou, ada );
                        ChatPlatformSyncHelper.RecordPersonMerge( rockContext, ada );
                    } );
                }

                Assert.AreEqual( 2, scene.Platform.WaitForMessageCount( scene.TenantId, oldest, n => n == 2 ),
                    "the kept conversation holds both conversations' messages once the merge's push lands" );

                using ( var rockContext = new RockContext() )
                {
                    var run = ChatPlatformSync.Run( rockContext, scene.Configuration, true );
                    Assert.IsFalse( run.IsFailure, run.Message );
                }

                Assert.AreEqual( 2, scene.Platform.WaitForMessageCount( scene.TenantId, oldest, n => n == 2 ),
                    "and still does after the full sync restates the church" );
                Assert.AreEqual( 0, scene.Platform.WaitForMessageCount( scene.TenantId, twin, n => n == 0 ), "the twin holds none" );
                // Rock archived it, which keeps its channel and takes everyone out of it.
                foreach ( var personAlias in new[] { adaAlias, scene.Fixture.PrimaryAliasGuid( cal ) } )
                {
                    Assert.IsNotNull( scene.Platform.WaitForMember( scene.TenantId, twin, personAlias, r => r != null && r["absent_since"].Type != Newtonsoft.Json.Linq.JTokenType.Null ),
                        "and nobody is in it" );
                }

                var member = scene.Platform.WaitForMember( scene.TenantId, room, adaAlias, r => r != null && ( bool ) r["is_banned"] );
                Assert.IsNotNull( member, "the one membership left in the room carries the ban Lou held" );
                Assert.IsNotNull( scene.Platform.WaitForMember( scene.TenantId, room, louAlias, r => r != null && r["absent_since"].Type != Newtonsoft.Json.Linq.JTokenType.Null ),
                    "and Lou's old membership is gone" );

                var stamp = scene.Platform.ReadMessage( scene.TenantId, first.MessageId.Value );
                Assert.AreEqual( louAlias, ( Guid ) stamp["person_alias_guid"], "Lou's message keeps the alias it was written under" );
                Assert.IsNotNull( scene.Platform.WaitForAlias( scene.TenantId, louAlias, r => r != null && ( Guid ) r["primary_person_alias_guid"] == adaAlias ),
                    "and that alias now names Ada, so the message reads as hers" );
            }
        }

        #endregion Person merge

        #region Support

        /// <summary>
        /// The nearest-rank percentile of values already sorted.
        /// </summary>
        private static double Percentile( IList<double> sorted, double fraction )
        {
            var rank = ( int ) Math.Ceiling( fraction * sorted.Count );

            return sorted[Math.Max( 0, rank - 1 )];
        }

        /// <summary>
        /// A church provisioned on the platform, its settings stored in Rock, a public lobby that
        /// enrols people, and the immediate sync sending to the real platform.
        /// </summary>
        private sealed class Scene : IDisposable
        {
            private readonly IDisposable _override;

            private readonly Guid _lobby;

            private readonly List<Guid> _made = new List<Guid>();

            public Scene()
            {
                Platform = LocalChatPlatform.FromEnvironment();
                Fixture = new ChatSyncProjectionFixture();
                Configuration = Platform.ProvisionChurch();
                Fixture.StoreConfiguration( Configuration );
                Fixture.EnsureChatPeopleGroup();
                _override = ChatPlatformSyncHelper.OverrideImmediateSync( new HttpClientHandler() );
                _lobby = Fixture.AddChannel( Fixture.SharedGroupTypeId, "Lobby" );
            }

            public LocalChatPlatform Platform { get; }

            public ChatSyncProjectionFixture Fixture { get; }

            public ChatPlatformConfiguration Configuration { get; }

            public Guid TenantId => Configuration.TenantId.Value;

            /// <summary>
            /// A person enrolled in chat through the lobby, with Open DM on so any of them can be
            /// put in a conversation.
            /// </summary>
            public int AddChatPerson( string lastName )
            {
                var personId = Fixture.AddPerson( lastName );
                Fixture.AddMember( _lobby, personId );

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( "UPDATE [Person] SET [IsChatOpenDirectMessageAllowed] = 1 WHERE [Id] = @p0", personId );
                }

                return personId;
            }

            /// <summary>
            /// Puts a person in a room inside a request and waits for the push, so the room is on
            /// the platform the ordinary way.
            /// </summary>
            public void Push( Guid groupGuid, int personId, Action<GroupMember> edit = null )
            {
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == groupGuid );
                    var member = new GroupMember
                    {
                        GroupId = group.Id,
                        GroupTypeId = group.GroupTypeId,
                        PersonId = personId,
                        GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };
                    edit?.Invoke( member );
                    new GroupMemberService( rockContext ).Add( member );
                    rockContext.SaveChanges();
                    ChatPlatformSyncHelper.FlushAsync( rockContext ).GetAwaiter().GetResult();
                }
            }

            /// <summary>
            /// Starts a group conversation through its door and answers its Guid.
            /// </summary>
            public Guid Start( int callerId, params int[] others )
            {
                var result = AsCaller( callerId, ( caller, context, rockContext ) =>
                    ChatDoorHelper.StartDirectMessageAsync( caller, others.Select( Fixture.PrimaryAliasGuid ), context, rockContext ) );
                Made( result.ChannelGuid );
                Assert.AreEqual( "ok", result.Code, result.Message );

                return result.ChannelGuid.Value;
            }

            public ChatMembershipResultBag Join( int callerId, Guid channelGuid )
            {
                return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.JoinChannelAsync( caller, channelGuid, context, rockContext ) );
            }

            public ChatMembershipResultBag Leave( int callerId, Guid channelGuid )
            {
                return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.LeaveChannelAsync( caller, channelGuid, context, rockContext ) );
            }

            public ChatMembershipResultBag Rename( int callerId, Guid channelGuid, string name )
            {
                return AsCaller( callerId, ( caller, context, rockContext ) => ChatDoorHelper.RenameConversationAsync( caller, channelGuid, name, context, rockContext ) );
            }

            /// <summary>
            /// Runs a door as the caller inside a request of its own, as the block does.
            /// </summary>
            private T AsCaller<T>( int callerId, Func<Person, ChatSessionContext, RockContext, System.Threading.Tasks.Task<T>> door )
            {
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var caller = new PersonService( rockContext ).Get( callerId );
                    var context = ChatSessionHelper.BuildSessionContext( caller, Configuration, rockContext );

                    return door( caller, context, rockContext ).GetAwaiter().GetResult();
                }
            }

            public void Made( Guid? channelGuid )
            {
                if ( channelGuid.HasValue )
                {
                    _made.Add( channelGuid.Value );
                }
            }

            public void Dispose()
            {
                _override.Dispose();

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

        #endregion Support
    }
}
