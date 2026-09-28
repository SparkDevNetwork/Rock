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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;

using static Rock.Tests.Communication.Chat.Platform.Session.ChatSessionFixture;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// What a chat block tells the browser when it opens and when it is asked for a token.
    /// The gates themselves are proven in ChatSessionHelperTests; these prove the helper passes
    /// every outcome through, never hands the browser a key, and asks the gates again each time.
    /// </summary>
    [TestClass]
    public class ChatSessionHelperBagTests
    {
        private RockContext _rockContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _rockContext = CreateRockContextMock().Object;
        }

        #region Gate codes

        [TestMethod]
        public void ToGateCode_EveryGate_HasItsOwnSnakeCaseCode()
        {
            var gates = Enum.GetValues( typeof( ChatMintGate ) ).Cast<ChatMintGate>().ToList();
            var codes = gates.Select( ChatSessionHelper.ToGateCode ).ToList();

            Assert.AreEqual( gates.Count, codes.Distinct().Count(), "two gates share a code, so the browser cannot tell them apart" );
            foreach ( var code in codes )
            {
                Assert.IsTrue( System.Text.RegularExpressions.Regex.IsMatch( code, "^[a-z]+(_[a-z]+)*$" ), $"'{code}' is not snake_case" );
            }

            Assert.AreEqual( "ok", ChatSessionHelper.ToGateCode( ChatMintGate.Ok ) );
            Assert.AreEqual( "age_verification_required", ChatSessionHelper.ToGateCode( ChatMintGate.AgeVerificationRequired ) );
            Assert.AreEqual( "age_restricted", ChatSessionHelper.ToGateCode( ChatMintGate.AgeRestricted ) );
        }

        #endregion

        #region OpenSession

        [TestMethod]
        public void OpenSession_Ok_CarriesThePublicSettingsAndTheDirectMessageRight()
        {
            var person = Adult();
            var config = SigningConfig();

            var bag = ChatSessionHelper.OpenSession( person, config, _rockContext );

            Assert.AreEqual( "ok", bag.Gate );
            Assert.AreEqual( config.Configuration.ProjectUrl, bag.ProjectUrl );
            Assert.AreEqual( config.Configuration.PublishableKey, bag.PublishableKey );
            Assert.AreEqual( config.Configuration.TenantId, bag.TenantId );
            Assert.AreEqual( person.PrimaryAliasGuid, bag.PersonAliasGuid );
            Assert.IsTrue( bag.CanStartDm, "no Direct Message Access data view means anyone may start one" );
        }

        [TestMethod]
        public void OpenSession_PersonOutsideTheDirectMessageDataView_MayNotStartOne()
        {
            // A church that names a Direct Message Access data view which resolved to someone else.
            var config = SigningConfig();
            config.Configuration.DirectMessageAccessDataViewGuid = Guid.Parse( "dddddddd-dddd-4ddd-8ddd-dddddddddddd" );
            config.DirectMessageAccessPersonIds = new System.Collections.Generic.HashSet<int> { 999 };

            var bag = ChatSessionHelper.OpenSession( Adult(), config, _rockContext );

            Assert.AreEqual( "ok", bag.Gate );
            Assert.IsFalse( bag.CanStartDm );
        }

        [TestMethod]
        public void OpenSession_RefusedGates_CarryTheGateAndNoPlatformSettings()
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
                var bag = ChatSessionHelper.OpenSession( c.Person, c.Config, _rockContext );

                Assert.AreEqual( c.Gate, bag.Gate );
                Assert.IsNull( bag.ProjectUrl, $"{c.Gate} was told where the platform is" );
                Assert.IsNull( bag.PublishableKey, $"{c.Gate} was given the publishable key" );
                Assert.IsNull( bag.TenantId, $"{c.Gate} was told the church's platform id" );
                Assert.IsNull( bag.PersonAliasGuid, $"{c.Gate} was told their platform identity" );
                Assert.IsFalse( bag.CanStartDm, $"{c.Gate} was offered direct messages" );
            }
        }

        [TestMethod]
        public void OpenSession_Ok_SerialisesNoTokenAndNoPartOfTheSigningKey()
        {
            var config = SigningConfig();
            var privatePart = Newtonsoft.Json.Linq.JObject.Parse( config.Configuration.PrivateKey )
                .GetValue( "d", StringComparison.OrdinalIgnoreCase )
                .ToString();
            Assert.IsFalse( string.IsNullOrEmpty( privatePart ), "the fixture key has no private part to look for" );

            var box = new ChatShellInitializationBox { Session = ChatSessionHelper.OpenSession( Adult(), config, _rockContext ) };
            var json = JsonConvert.SerializeObject( box );

            Assert.IsFalse( json.Contains( privatePart ), "the private key reached the browser" );
            Assert.IsFalse( json.Contains( Kid ), "the key id reached the browser" );
            // The box's own SecurityGrantToken is Rock's and is not ours to judge, so the session
            // bag alone is read for a token.
            var sessionJson = JsonConvert.SerializeObject( box.Session );
            Assert.IsFalse( sessionJson.IndexOf( "token", StringComparison.OrdinalIgnoreCase ) >= 0, "the session bag carries a token" );
        }

        [TestMethod]
        public void OpenSession_Ok_EnrolsThePersonOnceAcrossTwoOpens()
        {
            var person = Adult();

            ChatSessionHelper.OpenSession( person, SigningConfig(), _rockContext );
            ChatSessionHelper.OpenSession( person, SigningConfig(), _rockContext );

            Assert.AreEqual( 1, MarkerCount( _rockContext, person.Id ) );
        }

        [TestMethod]
        public void OpenSession_RefusedGate_EnrolsNobody()
        {
            var person = Adult();
            person.IsDeceased = true;

            ChatSessionHelper.OpenSession( person, SigningConfig(), _rockContext );

            Assert.AreEqual( 0, MarkerCount( _rockContext, person.Id ) );
        }

        #endregion

        #region Token

        [TestMethod]
        public void MintToken_Ok_ReturnsASignedSessionTokenAndItsExpiry()
        {
            var person = Adult();

            var bag = ChatSessionHelper.MintToken( person, SigningConfig(), _rockContext );

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

            var first = ChatSessionHelper.MintToken( person, SigningConfig(), _rockContext );
            AddBanListMember( _rockContext, person.Id, GroupMemberStatus.Active, isArchived: false );
            var second = ChatSessionHelper.MintToken( person, SigningConfig(), _rockContext );

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

            var bag = ChatSessionHelper.MintToken( Adult(), config, _rockContext );
            var json = JsonConvert.SerializeObject( bag );

            Assert.AreEqual( "invalid_key", bag.Gate );
            Assert.IsNull( bag.ChurchToken );
            Assert.IsFalse( json.IndexOf( "exception", StringComparison.OrdinalIgnoreCase ) >= 0 );
        }

        #endregion
    }
}
