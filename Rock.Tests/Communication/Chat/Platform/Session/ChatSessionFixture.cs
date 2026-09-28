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
using System.Linq;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// The church, the person and the seed rows every session test starts from, so the gate,
    /// session and birthdate tests all read the same Rock.
    /// </summary>
    internal static class ChatSessionFixture
    {
        public const int PersonId = 10;
        public const int InactiveRecordStatusValueId = 3;
        public const int BanListGroupId = 40;
        public const int ChatPeopleGroupId = 50;
        public const int ChatPeopleRoleId = 7;
        public const string Kid = "kid-test-1";

        /// <summary>
        /// A context seeded the way a healthy Rock is. The two flags leave out the rows the
        /// record-status gate and the ban gate each read, which is how a database missing its
        /// own seed data is reproduced.
        /// </summary>
        /// <param name="withInactiveRecordStatus">Whether the Inactive record status exists.</param>
        /// <param name="withBanListGroup">Whether the Chat Ban List group exists.</param>
        /// <returns>The mock, so a test can verify what was saved.</returns>
        public static RockMock<RockContext> CreateRockContextMock( bool withInactiveRecordStatus = true, bool withBanListGroup = true )
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContext = rockContextMock.Object;

            if ( withInactiveRecordStatus )
            {
                rockContext.Set<DefinedValue>().Add( new DefinedValue
                {
                    Id = InactiveRecordStatusValueId,
                    Guid = Guid.Parse( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE )
                } );
            }

            if ( withBanListGroup )
            {
                rockContext.Set<Group>().Add( new Group
                {
                    Id = BanListGroupId,
                    Guid = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST ),
                    Name = "Chat Ban List",
                    GroupTypeId = 1
                } );
            }

            rockContext.Set<Group>().Add( new Group
            {
                Id = ChatPeopleGroupId,
                Guid = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE ),
                Name = "Chat People",
                GroupTypeId = 1,
                GroupType = new GroupType { Id = 1, DefaultGroupRoleId = ChatPeopleRoleId }
            } );

            return rockContextMock;
        }

        /// <summary>
        /// An active adult with a full birthdate and a primary alias, whom every gate lets in.
        /// </summary>
        /// <returns>A new person each call, so a test may change it freely.</returns>
        public static Person Adult()
        {
            return new Person
            {
                Id = PersonId,
                Gender = Gender.Unknown,
                RecordStatusValueId = 1,
                PrimaryAliasGuid = Guid.Parse( "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa" ),
                BirthYear = RockDateTime.Now.Year - 30,
                BirthMonth = 1,
                BirthDay = 1
            };
        }

        /// <summary>
        /// A configured church with a minimum age of 13. The key is not one that signs, so it
        /// only makes the church count as configured; a test that mints uses
        /// <see cref="SigningConfig"/>.
        /// </summary>
        /// <returns>A new context each call, so a test may change it freely.</returns>
        public static ChatSessionContext ValidConfig()
        {
            return new ChatSessionContext
            {
                Configuration = new ChatPlatformConfiguration
                {
                    TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                    PrivateKey = "{\"kty\":\"EC\"}",
                    ProjectUrl = "http://127.0.0.1:54321",
                    PublishableKey = "sb_publishable_test",
                    Kid = "kid-1",
                    MinimumAge = 13
                }
            };
        }

        /// <summary>
        /// The configured church of <see cref="ValidConfig"/> with a fresh P-256 key that signs,
        /// under <see cref="Kid"/>.
        /// </summary>
        /// <returns>A new context each call.</returns>
        public static ChatSessionContext SigningConfig()
        {
            var context = ValidConfig();
            context.Configuration.PrivateKey = CreatePrivateJwk( Kid );
            context.Configuration.Kid = Kid;
            return context;
        }

        /// <summary>
        /// A new ES256 private key as the JWK the church settings hold.
        /// </summary>
        /// <param name="kid">The key id to stamp on it.</param>
        /// <returns>The key as JSON.</returns>
        public static string CreatePrivateJwk( string kid )
        {
            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var key = new ECDsaSecurityKey( ecdsa ) { KeyId = kid };
                var jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey( key );
                jwk.Kid = kid;
                jwk.Use = "sig";
                jwk.Alg = "ES256";
                return JsonConvert.SerializeObject( jwk );
            }
        }

        /// <summary>
        /// Puts the person on the Chat Ban List.
        /// </summary>
        /// <param name="rockContext">The seeded context.</param>
        /// <param name="personId">The person to ban.</param>
        /// <param name="status">The membership status.</param>
        /// <param name="isArchived">Whether the membership is archived.</param>
        public static void AddBanListMember( RockContext rockContext, int personId, GroupMemberStatus status = GroupMemberStatus.Active, bool isArchived = false )
        {
            rockContext.Set<GroupMember>().Add( new GroupMember
            {
                GroupId = BanListGroupId,
                PersonId = personId,
                GroupMemberStatus = status,
                IsArchived = isArchived
            } );
        }

        /// <summary>
        /// How many Chat People markers the person has.
        /// </summary>
        /// <param name="rockContext">The seeded context.</param>
        /// <param name="personId">The person.</param>
        /// <returns>The count, which enrolment keeps at one at most.</returns>
        public static int MarkerCount( RockContext rockContext, int personId )
        {
            return rockContext.Set<GroupMember>()
                .Count( m => m.GroupId == ChatPeopleGroupId && m.PersonId == personId );
        }
    }
}
