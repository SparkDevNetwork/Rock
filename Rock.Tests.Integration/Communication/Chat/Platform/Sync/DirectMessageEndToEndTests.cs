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
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
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

        #region Support

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
