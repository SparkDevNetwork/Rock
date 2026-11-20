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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Communication.Chat;
using Rock.Communication.Chat.DTO;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.Constants;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Communication.Chat
{
    /// <summary>
    /// This suite checks <see cref="ChatHelper.GetChatUserAuthenticationAsync(int, bool)"/>.
    /// </summary>
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class ChatHelperUserAuthenticationTests
    {
        private const int BannedPersonId = 20;
        private const int AllowedPersonId = 21;
        private const int ChatBanListGroupId = 101;

        [TestCleanup]
        public void TestCleanup()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                RockCache.ClearAllCachedItems( false );
            }
        }

        [TestMethod]
        public async Task GetChatUserAuthentication_WithBannedPerson_ReturnsNullWithoutContactingChatProvider()
        {
            var chatProviderMock = new Mock<IChatProvider>( MockBehavior.Loose );
            var rockContextMock = CreateRockContextMock();

            ChatUserAuthentication auth;

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( chatProviderMock.Object ) ) )
            using ( var chatHelper = new ChatHelper( rockContextMock.Object ) )
            {
                auth = await chatHelper.GetChatUserAuthenticationAsync( BannedPersonId, false );
            }

            Assert.IsNull( auth );
            chatProviderMock.Verify( m => m.GetChatUsersAsync( It.IsAny<List<string>>() ), Times.Never );
            chatProviderMock.Verify( m => m.GetChatUserTokenAsync( It.IsAny<string>() ), Times.Never );
        }

        [TestMethod]
        public async Task GetChatUserAuthentication_WithAllowedPerson_ContactsChatProvider()
        {
            var chatProviderMock = new Mock<IChatProvider>( MockBehavior.Loose );
            var rockContextMock = CreateRockContextMock();

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( chatProviderMock.Object ) ) )
            using ( var chatHelper = new ChatHelper( rockContextMock.Object ) )
            {
                await chatHelper.GetChatUserAuthenticationAsync( AllowedPersonId, false );
            }

            chatProviderMock.Verify( m => m.GetChatUsersAsync( It.IsAny<List<string>>() ), Times.Once );
        }

        /// <summary>
        /// Creates a mocked context with chat turned on, the chat system groups, one banned person and one allowed
        /// person, each of whom already has a chat user.
        /// </summary>
        /// <returns>The mocked context.</returns>
        private static Mock<RockContext> CreateRockContextMock()
        {
            var rockContextMock = new Mock<RockContext>( MockBehavior.Strict, "invalidConnectionString" );

            EnableChat();

            var chatGroups = new List<Group>
            {
                CreateEntity<Group>( 100, SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS ),
                CreateEntity<Group>( ChatBanListGroupId, SystemGuid.Group.GROUP_CHAT_BAN_LIST ),
                CreateEntity<Group>( 102, SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES ),
                CreateEntity<Group>( 103, SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS )
            };

            var recordTypes = new List<DefinedValue>
            {
                CreateEntity<DefinedValue>( 1, SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ),
                CreateEntity<DefinedValue>( 2, SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS ),
                CreateEntity<DefinedValue>( 3, SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS ),
                CreateEntity<DefinedValue>( 4, SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER )
            };

            rockContextMock.SetupDbSet( chatGroups );
            rockContextMock.SetupDbSet( recordTypes );

            chatGroups.ForEach( g => GroupCache.Get( g.Guid, rockContextMock.Object ) );
            recordTypes.ForEach( dv => DefinedValueCache.Get( dv.Guid, rockContextMock.Object ) );

            var bannedPerson = new Person { Id = BannedPersonId, Guid = Guid.NewGuid(), NickName = "Banned", LastName = "Person", RecordTypeValueId = 1 };
            var allowedPerson = new Person { Id = AllowedPersonId, Guid = Guid.NewGuid(), NickName = "Allowed", LastName = "Person", RecordTypeValueId = 1 };

            rockContextMock.SetupDbSet( new List<Person> { bannedPerson, allowedPerson } );

            rockContextMock.SetupDbSet( new List<PersonAlias>
            {
                CreateChatPersonAlias( 200, BannedPersonId ),
                CreateChatPersonAlias( 201, AllowedPersonId )
            } );

            rockContextMock.SetupDbSet( new List<GroupMember>
            {
                new GroupMember { Id = 300, GroupId = ChatBanListGroupId, PersonId = BannedPersonId, Person = bannedPerson, GroupMemberStatus = GroupMemberStatus.Active }
            } );

            return rockContextMock;
        }

        /// <summary>
        /// Creates an entity with no attributes that can be loaded into cache without a database.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="guid">The entity unique identifier.</param>
        /// <returns>The entity.</returns>
        private static TEntity CreateEntity<TEntity>( int id, string guid )
            where TEntity : Model<TEntity>, new()
        {
            var entityMock = new Mock<TEntity>( MockBehavior.Loose )
            {
                CallBase = true
            };

            entityMock.Setup( m => m.TypeId ).Returns( 0 );

            var entity = entityMock.Object;
            entity.Id = id;
            entity.Guid = guid.AsGuid();
            entity.Attributes = new Dictionary<string, AttributeCache>();
            entity.AttributeValues = new Dictionary<string, AttributeValueCache>();

            return entity;
        }

        /// <summary>
        /// Saves a chat configuration with an API key and secret into the cached system settings.
        /// </summary>
        private static void EnableChat()
        {
            var chatConfiguration = new ChatConfiguration
            {
                ApiKey = "test-api-key",
                ApiSecret = Encryption.EncryptString( "test-api-secret" )
            };

            var systemSettings = new SystemSettings();
            var valuesProperty = typeof( SystemSettings ).GetProperty( "SystemSettingsValues", BindingFlags.Instance | BindingFlags.NonPublic );
            var values = ( ConcurrentDictionary<string, string> ) valuesProperty.GetValue( systemSettings );

            values[SystemKey.SystemSetting.CHAT_CONFIGURATION] = chatConfiguration.ToJson();

            RockCache.AddOrUpdate( "Rock:SystemSettings", systemSettings );
        }

        /// <summary>
        /// Creates a chat-specific person alias for the person.
        /// </summary>
        /// <param name="id">The person alias identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns>The chat-specific person alias.</returns>
        private static PersonAlias CreateChatPersonAlias( int id, int personId )
        {
            var guid = Guid.NewGuid();

            return new PersonAlias
            {
                Id = id,
                Guid = guid,
                PersonId = personId,
                Name = ChatHelper.ChatPersonAliasName,
                ForeignKey = $"rock-user-{guid}"
            };
        }
    }
}
