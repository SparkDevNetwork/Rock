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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// What happens when a person gives chat the birthdate it asked for. The door may only
    /// fill in what Rock does not know, and only when chat is asking, so no refusal here may
    /// leave any part of a birthdate changed.
    /// </summary>
    [TestClass]
    public class ChatBirthdateDoorTests
    {
        private const int PersonId = 10;
        private const int InactiveRecordStatusValueId = 3;
        private const int BanListGroupId = 40;
        private const int ChatPeopleGroupId = 50;
        private const int ChatPeopleRoleId = 7;

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

        #region Saved

        [TestMethod]
        public void Save_BlankBirthdateOfAnAdult_WritesItAndOpensChat()
        {
            var person = Stored( null, null, null );
            var year = RockDateTime.Now.Year - 30;

            var result = ChatBirthdateDoor.Save( PersonId, year, 4, 12, Config(), _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.Saved, result.Code );
            Assert.AreEqual( year, person.BirthYear );
            Assert.AreEqual( 4, person.BirthMonth );
            Assert.AreEqual( 12, person.BirthDay );
            Assert.AreEqual( "ok", result.Session.Gate );
            Assert.AreEqual( Config().Configuration.ProjectUrl, result.Session.ProjectUrl );
            Assert.AreEqual( person.PrimaryAliasGuid, result.Session.PersonAliasGuid );
            Assert.AreEqual( 1, MarkerCount(), "a person let into chat by their birthdate was not enrolled" );
        }

        [TestMethod]
        public void Save_BlankBirthdateUnderTheMinimumAge_WritesItAndSaysAgeRestricted()
        {
            // The date is the person's own word, as it was on the shipped chat block, and staff
            // correct it on the profile. Recording it is what keeps them out next time.
            var person = Stored( null, null, null );
            var year = RockDateTime.Now.Year - 8;

            var result = ChatBirthdateDoor.Save( PersonId, year, 4, 12, Config(), _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.Saved, result.Code );
            Assert.AreEqual( year, person.BirthYear );
            Assert.AreEqual( "age_restricted", result.Session.Gate );
            Assert.IsNull( result.Session.ProjectUrl, "a person under age was told where the platform is" );
            Assert.IsNull( result.Session.PersonAliasGuid );
            Assert.AreEqual( 0, MarkerCount(), "a person under age was enrolled" );
        }

        [TestMethod]
        public void Save_MonthAndDayRecordedWithoutAYear_WritesTheYearWhenTheyAgree()
        {
            var person = Stored( null, 4, 12 );
            var year = RockDateTime.Now.Year - 30;

            var result = ChatBirthdateDoor.Save( PersonId, year, 4, 12, Config(), _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.Saved, result.Code );
            Assert.AreEqual( year, person.BirthYear );
            Assert.AreEqual( 4, person.BirthMonth );
            Assert.AreEqual( 12, person.BirthDay );
            Assert.AreEqual( "ok", result.Session.Gate );
        }

        #endregion Saved

        #region Refused, nothing written

        [TestMethod]
        public void Save_BirthdateAlreadyRecorded_ChangesNothing()
        {
            // One recorded date lets the person in and the other keeps them out. Neither may
            // be replaced from here, or this is a way to change a recorded age.
            var adultYear = RockDateTime.Now.Year - 30;
            var childYear = RockDateTime.Now.Year - 8;

            foreach ( var recordedYear in new[] { adultYear, childYear } )
            {
                TestInitialize();
                var person = Stored( recordedYear, 1, 1 );

                var result = ChatBirthdateDoor.Save( PersonId, RockDateTime.Now.Year - 40, 6, 6, Config(), _rockContext );

                Assert.AreEqual( ChatBirthdateDoor.BirthdateRecorded, result.Code, $"recorded year {recordedYear}" );
                AssertUnchanged( person, recordedYear, 1, 1 );
            }
        }

        [TestMethod]
        public void Save_MonthAndDayRecordedWithoutAYear_RefusesADateThatDisagrees()
        {
            var person = Stored( null, 4, 12 );

            var result = ChatBirthdateDoor.Save( PersonId, RockDateTime.Now.Year - 30, 5, 12, Config(), _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.BirthdateRecorded, result.Code );
            AssertUnchanged( person, null, 4, 12 );
            Assert.AreEqual( "age_verification_required", result.Session.Gate );
        }

        [TestMethod]
        public void Save_WhenChatIsNotAskingForABirthdate_ChangesNothing()
        {
            var noMinimumAge = Config();
            noMinimumAge.Configuration.MinimumAge = null;

            var person = Stored( null, null, null );
            var result = ChatBirthdateDoor.Save( PersonId, RockDateTime.Now.Year - 30, 4, 12, noMinimumAge, _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.NotAsked, result.Code, "a church with no minimum age" );
            AssertUnchanged( person, null, null, null );

            // A banned person's gate refuses before the age gate is reached.
            TestInitialize();
            var banned = Stored( null, null, null );
            _rockContext.Set<GroupMember>().Add( new GroupMember
            {
                GroupId = BanListGroupId,
                PersonId = PersonId,
                GroupMemberStatus = GroupMemberStatus.Active,
                IsArchived = false
            } );

            var bannedResult = ChatBirthdateDoor.Save( PersonId, RockDateTime.Now.Year - 30, 4, 12, Config(), _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.NotAsked, bannedResult.Code, "a banned person" );
            Assert.AreEqual( "banned", bannedResult.Session.Gate );
            AssertUnchanged( banned, null, null, null );
        }

        [TestMethod]
        public void Save_DateThatIsIncompleteImpossibleOrInTheFuture_ChangesNothing()
        {
            var tomorrow = RockDateTime.Today.AddDays( 1 );
            var cases = new[]
            {
                (Year: 0, Month: 4, Day: 12, Name: "no year"),
                (Year: RockDateTime.Now.Year - 30, Month: 0, Day: 12, Name: "no month"),
                (Year: RockDateTime.Now.Year - 30, Month: 4, Day: 0, Name: "no day"),
                (Year: RockDateTime.Now.Year - 30, Month: 2, Day: 30, Name: "30 February"),
                (Year: RockDateTime.Now.Year - 30, Month: 13, Day: 1, Name: "a thirteenth month"),
                (Year: tomorrow.Year, Month: tomorrow.Month, Day: tomorrow.Day, Name: "tomorrow")
            };

            foreach ( var c in cases )
            {
                TestInitialize();
                var person = Stored( null, null, null );

                var result = ChatBirthdateDoor.Save( PersonId, c.Year, c.Month, c.Day, Config(), _rockContext );

                Assert.AreEqual( ChatBirthdateDoor.InvalidDate, result.Code, c.Name );
                AssertUnchanged( person, null, null, null );
                Assert.AreEqual( "age_verification_required", result.Session.Gate, c.Name );
            }
        }

        [TestMethod]
        public void Save_NobodySignedIn_IsRefused()
        {
            var result = ChatBirthdateDoor.Save( null, RockDateTime.Now.Year - 30, 4, 12, Config(), _rockContext );

            Assert.AreEqual( ChatBirthdateDoor.SignInRequired, result.Code );
            Assert.AreEqual( "sign_in_required", result.Session.Gate );
        }

        #endregion Refused, nothing written

        #region Helpers

        private Person Stored( int? year, int? month, int? day )
        {
            var person = new Person
            {
                Id = PersonId,
                Gender = Gender.Unknown,
                RecordStatusValueId = 1,
                PrimaryAliasGuid = Guid.Parse( "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa" ),
                BirthYear = year,
                BirthMonth = month,
                BirthDay = day
            };

            _rockContext.Set<Person>().Add( person );

            return person;
        }

        private static void AssertUnchanged( Person person, int? year, int? month, int? day )
        {
            Assert.AreEqual( year, person.BirthYear, "the birth year changed" );
            Assert.AreEqual( month, person.BirthMonth, "the birth month changed" );
            Assert.AreEqual( day, person.BirthDay, "the birth day changed" );
        }

        private int MarkerCount()
        {
            return _rockContext.Set<GroupMember>()
                .Count( m => m.GroupId == ChatPeopleGroupId && m.PersonId == PersonId );
        }

        private static ChatSessionContext Config()
        {
            // The door opens a session and never mints a token, so the key is only there to
            // make the church count as configured.
            return new ChatSessionContext
            {
                Configuration = new ChatPlatformConfiguration
                {
                    TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                    PrivateKey = "{\"kty\":\"EC\"}",
                    ProjectUrl = "http://127.0.0.1:54321",
                    PublishableKey = "sb_publishable_test",
                    Kid = "kid-door-1",
                    MinimumAge = 13
                }
            };
        }

        #endregion Helpers
    }
}
