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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// What the chat shell tells the browser when it opens and when it is asked for a token.
    /// The gates themselves are proven in ChatSessionHelperTests; these prove the shell passes
    /// every outcome through, never hands the browser a key, and asks the gates again each time.
    /// </summary>
    [TestClass]
    public class ChatShellSessionTests
    {
        private const int InactiveRecordStatusValueId = 3;
        private const int BanListGroupId = 40;
        private const int ChatPeopleGroupId = 50;
        private const int ChatPeopleRoleId = 7;
        private const string Kid = "kid-shell-1";

        private RockContext _rockContext;

        [TestInitialize]
        public void TestInitialize()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;

            rockContext.Set<DefinedValue>().Add( new DefinedValue
            {
                Id = InactiveRecordStatusValueId,
                Guid = Guid.Parse( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE )
            } );

            rockContext.Set<Group>().Add( new Group
            {
                Id = BanListGroupId,
                Guid = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST ),
                Name = "Chat Ban List",
                GroupTypeId = 1
            } );

            rockContext.Set<Group>().Add( new Group
            {
                Id = ChatPeopleGroupId,
                Guid = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE ),
                Name = "Chat People",
                GroupTypeId = 1,
                GroupType = new GroupType { Id = 1, DefaultGroupRoleId = ChatPeopleRoleId }
            } );

            _rockContext = rockContext;
        }

        #region Gate codes

        [TestMethod]
        public void ToGateCode_EveryGate_HasItsOwnSnakeCaseCode()
        {
            var gates = Enum.GetValues( typeof( ChatMintGate ) ).Cast<ChatMintGate>().ToList();
            var codes = gates.Select( ChatShellSession.ToGateCode ).ToList();

            Assert.AreEqual( gates.Count, codes.Distinct().Count(), "two gates share a code, so the browser cannot tell them apart" );
            foreach ( var code in codes )
            {
                Assert.IsTrue( System.Text.RegularExpressions.Regex.IsMatch( code, "^[a-z]+(_[a-z]+)*$" ), $"'{code}' is not snake_case" );
            }

            Assert.AreEqual( "ok", ChatShellSession.ToGateCode( ChatMintGate.Ok ) );
            Assert.AreEqual( "age_verification_required", ChatShellSession.ToGateCode( ChatMintGate.AgeVerificationRequired ) );
            Assert.AreEqual( "age_restricted", ChatShellSession.ToGateCode( ChatMintGate.AgeRestricted ) );
        }

        #endregion

        #region Open

        [TestMethod]
        public void Open_Ok_CarriesThePublicSettingsAndTheDirectMessageRight()
        {
            var person = Adult();
            var config = SigningConfig();

            var bag = ChatShellSession.Open( person, config, _rockContext );

            Assert.AreEqual( "ok", bag.Gate );
            Assert.AreEqual( config.Configuration.ProjectUrl, bag.ProjectUrl );
            Assert.AreEqual( config.Configuration.PublishableKey, bag.PublishableKey );
            Assert.AreEqual( config.Configuration.TenantId, bag.TenantId );
            Assert.AreEqual( person.PrimaryAliasGuid, bag.PersonAliasGuid );
            Assert.IsTrue( bag.CanStartDm, "no Direct Message Access data view means anyone may start one" );
        }

        [TestMethod]
        public void Open_PersonOutsideTheDirectMessageDataView_MayNotStartOne()
        {
            // A church that names a Direct Message Access data view which resolved to someone else.
            var config = SigningConfig();
            config.Configuration.DirectMessageAccessDataViewGuid = Guid.Parse( "dddddddd-dddd-4ddd-8ddd-dddddddddddd" );
            config.DirectMessageAccessPersonIds = new System.Collections.Generic.HashSet<int> { 999 };

            var bag = ChatShellSession.Open( Adult(), config, _rockContext );

            Assert.AreEqual( "ok", bag.Gate );
            Assert.IsFalse( bag.CanStartDm );
        }

        [TestMethod]
        public void Open_RefusedGates_CarryTheGateAndNoPlatformSettings()
        {
            var deceased = Adult();
            deceased.IsDeceased = true;

            var noBirthdate = Adult();
            noBirthdate.BirthYear = null;
            noBirthdate.BirthMonth = null;
            noBirthdate.BirthDay = null;

            var child = Adult();
            child.BirthYear = RockDateTime.Now.Year - 8;

            var notConfigured = SigningConfig();
            notConfigured.Configuration.TenantId = null;

            var cases = new[]
            {
                (Person: (Person) null, Config: SigningConfig(), Gate: "sign_in_required"),
                (Person: Adult(), Config: notConfigured, Gate: "not_configured"),
                (Person: deceased, Config: SigningConfig(), Gate: "deceased"),
                (Person: noBirthdate, Config: SigningConfig(), Gate: "age_verification_required"),
                (Person: child, Config: SigningConfig(), Gate: "age_restricted")
            };

            foreach ( var c in cases )
            {
                var bag = ChatShellSession.Open( c.Person, c.Config, _rockContext );

                Assert.AreEqual( c.Gate, bag.Gate );
                Assert.IsNull( bag.ProjectUrl, $"{c.Gate} was told where the platform is" );
                Assert.IsNull( bag.PublishableKey, $"{c.Gate} was given the publishable key" );
                Assert.IsNull( bag.TenantId, $"{c.Gate} was told the church's platform id" );
                Assert.IsNull( bag.PersonAliasGuid, $"{c.Gate} was told their platform identity" );
                Assert.IsFalse( bag.CanStartDm, $"{c.Gate} was offered direct messages" );
            }
        }

        [TestMethod]
        public void Open_Ok_SerialisesNoTokenAndNoPartOfTheSigningKey()
        {
            var config = SigningConfig();
            var privatePart = Newtonsoft.Json.Linq.JObject.Parse( config.Configuration.PrivateKey )
                .GetValue( "d", StringComparison.OrdinalIgnoreCase )
                .ToString();
            Assert.IsFalse( string.IsNullOrEmpty( privatePart ), "the fixture key has no private part to look for" );

            var box = new ChatShellInitializationBox { Session = ChatShellSession.Open( Adult(), config, _rockContext ) };
            var json = JsonConvert.SerializeObject( box );

            Assert.IsFalse( json.Contains( privatePart ), "the private key reached the browser" );
            Assert.IsFalse( json.Contains( Kid ), "the key id reached the browser" );
            // The box's own SecurityGrantToken is Rock's and is not ours to judge, so the session
            // bag alone is read for a token.
            var sessionJson = JsonConvert.SerializeObject( box.Session );
            Assert.IsFalse( sessionJson.IndexOf( "token", StringComparison.OrdinalIgnoreCase ) >= 0, "the session bag carries a token" );
        }

        [TestMethod]
        public void Open_Ok_EnrolsThePersonOnceAcrossTwoOpens()
        {
            var person = Adult();

            ChatShellSession.Open( person, SigningConfig(), _rockContext );
            ChatShellSession.Open( person, SigningConfig(), _rockContext );

            Assert.AreEqual( 1, MarkerCount( person.Id ) );
        }

        [TestMethod]
        public void Open_RefusedGate_EnrolsNobody()
        {
            var person = Adult();
            person.IsDeceased = true;

            ChatShellSession.Open( person, SigningConfig(), _rockContext );

            Assert.AreEqual( 0, MarkerCount( person.Id ) );
        }

        #endregion

        #region Token

        [TestMethod]
        public void MintToken_Ok_ReturnsASignedSessionTokenAndItsExpiry()
        {
            var person = Adult();

            var bag = ChatShellSession.MintToken( person, SigningConfig(), _rockContext );

            Assert.AreEqual( "ok", bag.Gate );
            Assert.IsFalse( string.IsNullOrWhiteSpace( bag.ChurchToken ) );
            var token = new JwtSecurityTokenHandler().ReadJwtToken( bag.ChurchToken );
            Assert.AreEqual( person.PrimaryAliasGuid.ToString(), token.Subject );
            Assert.AreEqual( "chat.session", token.Payload["scp"].ToString() );
            Assert.AreEqual( token.ValidTo, bag.ExpiresAt?.UtcDateTime );
        }

        [TestMethod]
        public void MintToken_PersonBannedBetweenTwoAsks_IsRefusedOnTheSecond()
        {
            var person = Adult();

            var first = ChatShellSession.MintToken( person, SigningConfig(), _rockContext );
            _rockContext.Set<GroupMember>().Add( new GroupMember
            {
                GroupId = BanListGroupId,
                PersonId = person.Id,
                GroupMemberStatus = GroupMemberStatus.Active,
                IsArchived = false
            } );
            var second = ChatShellSession.MintToken( person, SigningConfig(), _rockContext );

            Assert.AreEqual( "ok", first.Gate );
            Assert.AreEqual( "banned", second.Gate );
            Assert.IsNull( second.ChurchToken );
            Assert.IsNull( second.ExpiresAt );
        }

        [TestMethod]
        public void MintToken_UnusableKey_CarriesTheGateAndNoExceptionText()
        {
            var config = SigningConfig();
            config.Configuration.PrivateKey = "{\"kty\":\"EC\"}";

            var bag = ChatShellSession.MintToken( Adult(), config, _rockContext );
            var json = JsonConvert.SerializeObject( bag );

            Assert.AreEqual( "invalid_key", bag.Gate );
            Assert.IsNull( bag.ChurchToken );
            Assert.IsFalse( json.IndexOf( "exception", StringComparison.OrdinalIgnoreCase ) >= 0 );
        }

        #endregion

        #region Helpers

        private int MarkerCount( int personId )
        {
            return _rockContext.Set<GroupMember>()
                .Count( m => m.GroupId == ChatPeopleGroupId && m.PersonId == personId );
        }

        private static Person Adult()
        {
            return new Person
            {
                Id = 10,
                Gender = Gender.Unknown,
                RecordStatusValueId = 1,
                PrimaryAliasGuid = Guid.Parse( "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa" ),
                BirthYear = RockDateTime.Now.Year - 30,
                BirthMonth = 1,
                BirthDay = 1
            };
        }

        private static ChatSessionContext SigningConfig()
        {
            return new ChatSessionContext
            {
                Configuration = new ChatPlatformConfiguration
                {
                    TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                    PrivateKey = CreatePrivateJwk( Kid ),
                    ProjectUrl = "http://127.0.0.1:54321",
                    PublishableKey = "sb_publishable_test",
                    Kid = Kid,
                    MinimumAge = 13
                }
            };
        }

        private static string CreatePrivateJwk( string kid )
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

        #endregion
    }
}
