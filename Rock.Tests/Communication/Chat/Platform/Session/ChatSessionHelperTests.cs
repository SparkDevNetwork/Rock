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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Enums.Crm;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// Session gates, church-token minting, and Chat People enrolment.
    /// </summary>
    [TestClass]
    public class ChatSessionHelperTests
    {
        private const int InactiveRecordStatusValueId = 3;
        private const int BanListGroupId = 40;
        private const int ChatPeopleGroupId = 50;
        private const int ChatPeopleRoleId = 7;
        private const string Kid = "kid-test-1";

        private RockContext _rockContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _rockContext = BuildContext( withInactiveRecordStatus: true, withBanListGroup: true );
        }

        /// <summary>
        /// A context seeded the way a healthy Rock is. The two flags leave out the rows the
        /// record-status gate and the ban gate each read, which is how a database missing its
        /// own seed data is reproduced.
        /// </summary>
        private static RockContext BuildContext( bool withInactiveRecordStatus, bool withBanListGroup )
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

            return rockContext;
        }

        #region Gates

        [TestMethod]
        public void Evaluate_NullPerson_IsSignInRequired()
        {
            var result = ChatSessionHelper.Evaluate( null, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.SignInRequired, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_PersonWithIdZero_IsSignInRequired()
        {
            var person = Adult();
            person.Id = 0;

            var result = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.SignInRequired, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_MissingTenant_IsNotConfigured()
        {
            var config = ValidConfig();
            config.TenantId = null;

            var result = ChatSessionHelper.Evaluate( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.NotConfigured, result.Gate );
        }

        [TestMethod]
        public void Evaluate_MissingPrivateKey_IsNotConfigured()
        {
            var config = ValidConfig();
            config.PrivateKey = null;

            var result = ChatSessionHelper.Evaluate( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.NotConfigured, result.Gate );
        }

        [TestMethod]
        public void Evaluate_MissingProjectUrl_IsNotConfigured()
        {
            var config = ValidConfig();
            config.ProjectUrl = " ";

            var result = ChatSessionHelper.Evaluate( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.NotConfigured, result.Gate );
        }

        [TestMethod]
        public void Evaluate_MissingPublishableKey_IsNotConfigured()
        {
            var config = ValidConfig();
            config.PublishableKey = null;

            var result = ChatSessionHelper.Evaluate( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.NotConfigured, result.Gate );
        }

        [TestMethod]
        public void Evaluate_DeceasedPerson_IsDeceased()
        {
            var person = Adult();
            person.IsDeceased = true;

            var result = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Deceased, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_InactiveRecordStatus_IsInactive()
        {
            var person = Adult();
            person.RecordStatusValueId = InactiveRecordStatusValueId;

            var result = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Inactive, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_ActiveBanListMember_IsBanned()
        {
            var person = Adult();
            AddBanListMember( person.Id, GroupMemberStatus.Active, isArchived: false );

            var result = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Banned, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_ArchivedOrInactiveBanListMember_IsNotBanned()
        {
            var person = Adult();
            AddBanListMember( person.Id, GroupMemberStatus.Active, isArchived: true );
            AddBanListMember( person.Id, GroupMemberStatus.Inactive, isArchived: false );

            var result = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
        }

        [TestMethod]
        public void Evaluate_PersonNotOnBanList_IsNotBanned()
        {
            var result = ChatSessionHelper.Evaluate( Adult(), ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
        }

        [TestMethod]
        public void Evaluate_MinimumAgeWithoutBirthdate_IsAgeVerificationRequired()
        {
            var person = Adult();
            person.BirthYear = null;
            person.BirthMonth = null;
            person.BirthDay = null;

            var config = ValidConfig();
            config.MinimumAge = 13;

            var result = ChatSessionHelper.Evaluate( person, config, _rockContext );

            Assert.AreEqual( ChatMintGate.AgeVerificationRequired, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_UnderMinimumAge_IsAgeRestricted()
        {
            var person = Adult();
            person.BirthYear = RockDateTime.Now.Year - 10;
            person.BirthMonth = 1;
            person.BirthDay = 1;

            var config = ValidConfig();
            config.MinimumAge = 13;

            var result = ChatSessionHelper.Evaluate( person, config, _rockContext );

            Assert.AreEqual( ChatMintGate.AgeRestricted, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_MissingPrimaryAlias_IsNoPrimaryAlias()
        {
            var person = Adult();
            person.PrimaryAliasGuid = null;

            var result = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.NoPrimaryAlias, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_NullRockContext_ThrowsRatherThanSkippingTheGatesThatNeedIt()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () =>
                ChatSessionHelper.Evaluate( Adult(), ValidConfig(), null ) );
        }

        [TestMethod]
        public void TryMintChurchToken_NullRockContext_ThrowsRatherThanSigningUngatedToken()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () =>
                ChatSessionHelper.TryMintChurchToken( Adult(), SigningConfig(), null ) );
        }

        [TestMethod]
        public void EnsureEnrollment_NullRockContext_ThrowsRatherThanEnrollingUngated()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () =>
                ChatSessionHelper.EnsureEnrollment( Adult(), ValidConfig(), null ) );
        }

        [TestMethod]
        public void Evaluate_MissingBanListGroup_IsGateUnavailable()
        {
            var context = BuildContext( withInactiveRecordStatus: true, withBanListGroup: false );

            var result = ChatSessionHelper.Evaluate( Adult(), ValidConfig(), context );

            Assert.AreEqual( ChatMintGate.GateUnavailable, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_MissingInactiveRecordStatusValue_IsGateUnavailable()
        {
            var context = BuildContext( withInactiveRecordStatus: false, withBanListGroup: true );

            var result = ChatSessionHelper.Evaluate( Adult(), ValidConfig(), context );

            Assert.AreEqual( ChatMintGate.GateUnavailable, result.Gate );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_PersonBannedBetweenMints_IsBannedOnTheSecond()
        {
            var person = Adult();

            var first = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );
            AddBanListMember( person.Id, GroupMemberStatus.Active, isArchived: false );
            var second = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, first.Gate );
            Assert.AreEqual( ChatMintGate.Banned, second.Gate );
        }

        [TestMethod]
        public void Evaluate_PersonInactivatedBetweenMints_IsInactiveOnTheSecond()
        {
            var person = Adult();

            var first = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );
            person.RecordStatusValueId = InactiveRecordStatusValueId;
            var second = ChatSessionHelper.Evaluate( person, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, first.Gate );
            Assert.AreEqual( ChatMintGate.Inactive, second.Gate );
        }

        [TestMethod]
        public void Evaluate_ValidPerson_IsOkWithAliasAndTenantAndNoToken()
        {
            var person = Adult();
            var config = ValidConfig();

            var result = ChatSessionHelper.Evaluate( person, config, _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
            Assert.AreEqual( person.PrimaryAliasGuid, result.PersonAliasGuid );
            Assert.AreEqual( config.TenantId, result.TenantId );
            Assert.IsNull( result.ChurchToken );
        }

        [TestMethod]
        public void Evaluate_BlankDirectMessageAccessDataView_CanStartDm()
        {
            var config = ValidConfig();
            config.DirectMessageAccessDataViewGuid = null;

            var result = ChatSessionHelper.Evaluate( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
            Assert.IsTrue( result.CanStartDm );
        }

        [TestMethod]
        public void Evaluate_PersonInDirectMessageAccessDataView_CanStartDm()
        {
            var person = Adult();
            var config = ValidConfig();
            config.DirectMessageAccessDataViewGuid = Guid.Parse( "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb" );
            config.DirectMessageAccessPersonIds = new HashSet<int> { person.Id };

            var result = ChatSessionHelper.Evaluate( person, config, _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
            Assert.IsTrue( result.CanStartDm );
        }

        [TestMethod]
        public void Evaluate_PersonNotInDirectMessageAccessDataView_CannotStartDmButStillMints()
        {
            var person = Adult();
            var config = ValidConfig();
            config.DirectMessageAccessDataViewGuid = Guid.Parse( "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb" );
            config.DirectMessageAccessPersonIds = new HashSet<int> { person.Id + 1 };

            var result = ChatSessionHelper.Evaluate( person, config, _rockContext );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
            Assert.IsFalse( result.CanStartDm );
            Assert.IsNull( result.ChurchToken );
        }

        #endregion

        #region Mint

        [TestMethod]
        public void TryMintChurchToken_ValidPerson_SignsEs256SessionTokenWithFiveMinuteExpiry()
        {
            var person = Adult();
            var config = SigningConfig();
            var beforeUtc = DateTime.UtcNow;

            var result = ChatSessionHelper.TryMintChurchToken( person, config, _rockContext );

            var afterUtc = DateTime.UtcNow;
            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
            Assert.IsFalse( string.IsNullOrWhiteSpace( result.ChurchToken ) );
            Assert.IsNull( result.ErrorMessage );

            var token = new JwtSecurityTokenHandler().ReadJwtToken( result.ChurchToken );
            Assert.AreEqual( "ES256", token.Header.Alg );
            Assert.AreEqual( Kid, token.Header.Kid );
            Assert.AreEqual( person.PrimaryAliasGuid.ToString(), token.Subject );
            Assert.AreEqual( config.TenantId.ToString(), token.Payload["tid"].ToString() );
            Assert.AreEqual( Kid, token.Payload["kid"].ToString() );
            Assert.AreEqual( "chat.session", token.Payload["scp"].ToString() );
            Assert.IsTrue( token.Payload.ContainsKey( "exp" ), "a church token always expires" );
            Assert.IsTrue( token.ValidTo >= beforeUtc.AddMinutes( 5 ).AddSeconds( -2 ) );
            Assert.IsTrue( token.ValidTo <= afterUtc.AddMinutes( 5 ).AddSeconds( 2 ) );
            Assert.AreEqual( result.ExpiresAtUtc?.UtcDateTime, token.ValidTo );
        }

        [TestMethod]
        public void TryMintChurchToken_FailedGate_ReturnsGateWithNoTokenAndNoExceptionText()
        {
            var person = Adult();
            person.IsDeceased = true;

            var result = ChatSessionHelper.TryMintChurchToken( person, SigningConfig(), _rockContext );

            Assert.AreEqual( ChatMintGate.Deceased, result.Gate );
            Assert.IsNull( result.ChurchToken );
            Assert.IsNull( result.ErrorMessage );
        }

        [TestMethod]
        public void TryMintChurchToken_JwkMissingPrivatePart_IsInvalidKeyWithoutExceptionText()
        {
            var config = SigningConfig();
            config.PrivateKey = StripJwkProperty( config.PrivateKey, "d" );

            var result = ChatSessionHelper.TryMintChurchToken( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.InvalidKey, result.Gate );
            Assert.IsNull( result.ChurchToken );
            Assert.IsNull( result.ErrorMessage );
        }

        [TestMethod]
        public void TryMintChurchToken_JwkMissingKid_IsInvalidKeyWithoutExceptionText()
        {
            var config = SigningConfig();
            config.PrivateKey = StripJwkProperty( config.PrivateKey, "kid" );
            config.Kid = null;

            var result = ChatSessionHelper.TryMintChurchToken( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.InvalidKey, result.Gate );
            Assert.IsNull( result.ChurchToken );
            Assert.IsNull( result.ErrorMessage );
        }

        [TestMethod]
        public void TryMintChurchToken_JwkNotEcP256_IsInvalidKeyWithoutExceptionText()
        {
            var config = SigningConfig();
            config.PrivateKey = SetJwkProperty( config.PrivateKey, "kty", "RSA" );

            var result = ChatSessionHelper.TryMintChurchToken( Adult(), config, _rockContext );

            Assert.AreEqual( ChatMintGate.InvalidKey, result.Gate );
            Assert.IsNull( result.ChurchToken );
            Assert.IsNull( result.ErrorMessage );
        }

        [TestMethod]
        public void TryMintSyncToken_ValidConfig_SignsSyncScopeWithTenantAndKid()
        {
            var config = SigningConfig();

            var result = ChatSessionHelper.TryMintSyncToken( config );

            Assert.AreEqual( ChatMintGate.Ok, result.Gate );
            Assert.IsFalse( string.IsNullOrWhiteSpace( result.ChurchToken ) );
            Assert.AreEqual( config.TenantId, result.TenantId );
            Assert.AreEqual( Kid, result.Kid );

            var token = new JwtSecurityTokenHandler().ReadJwtToken( result.ChurchToken );
            Assert.AreEqual( "sync", token.Payload["scp"].ToString() );
            Assert.AreEqual( config.TenantId.ToString(), token.Payload["tid"].ToString() );
            Assert.AreEqual( Kid, token.Header.Kid );
            Assert.IsTrue( token.Payload.ContainsKey( "exp" ), "a church token always expires" );
        }

        #endregion

        #region Enrolment

        [TestMethod]
        public void EnsureEnrollment_FirstOpen_WritesExactlyOneMarkerRow()
        {
            var person = Adult();

            ChatSessionHelper.EnsureEnrollment( person, ValidConfig(), _rockContext );

            Assert.AreEqual( 1, MarkerCount( person.Id ) );
        }

        [TestMethod]
        public void EnsureEnrollment_SecondOpen_WritesNoAdditionalMarkerRow()
        {
            var person = Adult();

            ChatSessionHelper.EnsureEnrollment( person, ValidConfig(), _rockContext );
            ChatSessionHelper.EnsureEnrollment( person, ValidConfig(), _rockContext );

            Assert.AreEqual( 1, MarkerCount( person.Id ) );
        }

        [TestMethod]
        public void TryMintChurchToken_CalledTwice_WritesNoMarkerRow()
        {
            var person = Adult();

            ChatSessionHelper.TryMintChurchToken( person, SigningConfig(), _rockContext );
            ChatSessionHelper.TryMintChurchToken( person, SigningConfig(), _rockContext );

            Assert.AreEqual( 0, MarkerCount( person.Id ) );
        }

        [TestMethod]
        public void EnsureEnrollment_FailedGate_WritesNoMarkerRow()
        {
            var person = Adult();
            person.IsDeceased = true;

            ChatSessionHelper.EnsureEnrollment( person, ValidConfig(), _rockContext );

            Assert.AreEqual( 0, MarkerCount( person.Id ) );
        }

        #endregion

        #region Helpers

        private void AddBanListMember( int personId, GroupMemberStatus status, bool isArchived )
        {
            _rockContext.Set<GroupMember>().Add( new GroupMember
            {
                GroupId = BanListGroupId,
                PersonId = personId,
                GroupMemberStatus = status,
                IsArchived = isArchived
            } );
        }

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

        private static ChatSessionConfiguration ValidConfig()
        {
            return new ChatSessionConfiguration
            {
                TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                PrivateKey = "{\"kty\":\"EC\"}",
                ProjectUrl = "http://127.0.0.1:54321",
                PublishableKey = "sb_publishable_test",
                Kid = "kid-1",
                MinimumAge = 13
            };
        }

        private static ChatSessionConfiguration SigningConfig()
        {
            var config = ValidConfig();
            config.PrivateKey = CreatePrivateJwk( Kid );
            config.Kid = Kid;
            return config;
        }

        private static string StripJwkProperty( string jwkJson, string name )
        {
            var jwk = JObject.Parse( jwkJson );
            var match = jwk.Properties().FirstOrDefault( p =>
                string.Equals( p.Name, name, StringComparison.OrdinalIgnoreCase ) );
            match?.Remove();
            return jwk.ToString();
        }

        private static string SetJwkProperty( string jwkJson, string name, string value )
        {
            var jwk = JObject.Parse( jwkJson );
            var match = jwk.Properties().FirstOrDefault( p =>
                string.Equals( p.Name, name, StringComparison.OrdinalIgnoreCase ) );
            if ( match != null )
            {
                match.Value = value;
            }
            else
            {
                jwk[name] = value;
            }

            return jwk.ToString();
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
