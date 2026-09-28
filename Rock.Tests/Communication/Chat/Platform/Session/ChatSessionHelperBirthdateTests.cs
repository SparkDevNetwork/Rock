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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

using static Rock.Tests.Communication.Chat.Platform.Session.ChatSessionFixture;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// What happens when a person gives chat the birthdate it asked for. The save may only
    /// fill in what Rock does not know, and only when chat is asking, so no refusal here may
    /// leave any part of a birthdate changed.
    /// </summary>
    [TestClass]
    public class ChatSessionHelperBirthdateTests
    {
        private RockMock<RockContext> _rockContextMock;
        private RockContext _rockContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _rockContextMock = CreateRockContextMock();
            _rockContext = _rockContextMock.Object;
        }

        #region Saved

        [TestMethod]
        public void SaveBirthdate_BlankBirthdateOfAnAdult_WritesItAndOpensChat()
        {
            var person = Stored( null, null, null );
            var year = RockDateTime.Now.Year - 30;

            var result = ChatSessionHelper.SaveBirthdate( PersonId, year, 4, 12, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatBirthdateCode.Saved, result.Code );
            Assert.AreEqual( year, person.BirthYear );
            Assert.AreEqual( 4, person.BirthMonth );
            Assert.AreEqual( 12, person.BirthDay );
            Assert.AreEqual( "ok", result.Session.Gate );
            Assert.AreEqual( ValidConfig().Configuration.ProjectUrl, result.Session.ProjectUrl );
            Assert.AreEqual( person.PrimaryAliasGuid, result.Session.PersonAliasGuid );
            Assert.AreEqual( 1, MarkerCount( _rockContext, PersonId ), "a person let into chat by their birthdate was not enrolled" );
        }

        [TestMethod]
        public void SaveBirthdate_BlankBirthdateUnderTheMinimumAge_WritesItAndSaysAgeRestricted()
        {
            // The date is the person's own word, as it was on the shipped chat block, and staff
            // correct it on the profile. Recording it is what keeps them out next time.
            var person = Stored( null, null, null );
            var year = RockDateTime.Now.Year - 8;

            var result = ChatSessionHelper.SaveBirthdate( PersonId, year, 4, 12, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatBirthdateCode.Saved, result.Code );
            Assert.AreEqual( year, person.BirthYear );
            Assert.AreEqual( "age_restricted", result.Session.Gate );
            Assert.IsNull( result.Session.ProjectUrl, "a person under age was told where the platform is" );
            Assert.IsNull( result.Session.PersonAliasGuid );
            Assert.AreEqual( 0, MarkerCount( _rockContext, PersonId ), "a person under age was enrolled" );

            // Nothing else saves for a person the gate refuses, so without this the date is
            // lost and the next open asks again, when a different date could be tried.
            _rockContextMock.Verify( m => m.SaveChanges(), Times.AtLeastOnce(), "the birthdate was never saved" );
        }

        [TestMethod]
        public void SaveBirthdate_MonthAndDayRecordedWithoutAYear_WritesTheYearWhenTheyAgree()
        {
            var person = Stored( null, 4, 12 );
            var year = RockDateTime.Now.Year - 30;

            var result = ChatSessionHelper.SaveBirthdate( PersonId, year, 4, 12, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatBirthdateCode.Saved, result.Code );
            Assert.AreEqual( year, person.BirthYear );
            Assert.AreEqual( 4, person.BirthMonth );
            Assert.AreEqual( 12, person.BirthDay );
            Assert.AreEqual( "ok", result.Session.Gate );
        }

        #endregion Saved

        #region Refused, nothing written

        [TestMethod]
        public void SaveBirthdate_BirthdateAlreadyRecorded_ChangesNothing()
        {
            // One recorded date lets the person in and the other keeps them out. Neither may
            // be replaced from here, or this is a way to change a recorded age.
            var adultYear = RockDateTime.Now.Year - 30;
            var childYear = RockDateTime.Now.Year - 8;

            foreach ( var recordedYear in new[] { adultYear, childYear } )
            {
                TestInitialize();
                var person = Stored( recordedYear, 1, 1 );

                var result = ChatSessionHelper.SaveBirthdate( PersonId, RockDateTime.Now.Year - 40, 6, 6, ValidConfig(), _rockContext );

                Assert.AreEqual( ChatBirthdateCode.BirthdateRecorded, result.Code, $"recorded year {recordedYear}" );
                AssertUnchanged( person, recordedYear, 1, 1 );
            }
        }

        [TestMethod]
        public void SaveBirthdate_MonthAndDayRecordedWithoutAYear_RefusesADateThatDisagrees()
        {
            var person = Stored( null, 4, 12 );

            var result = ChatSessionHelper.SaveBirthdate( PersonId, RockDateTime.Now.Year - 30, 5, 12, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatBirthdateCode.BirthdateRecorded, result.Code );
            AssertUnchanged( person, null, 4, 12 );
            AssertNothingSaved();
            Assert.AreEqual( "age_verification_required", result.Session.Gate );
        }

        [TestMethod]
        public void SaveBirthdate_WhenChatIsNotAskingForABirthdate_ChangesNothing()
        {
            var noMinimumAge = ValidConfig();
            noMinimumAge.Configuration.MinimumAge = null;

            var person = Stored( null, null, null );
            var result = ChatSessionHelper.SaveBirthdate( PersonId, RockDateTime.Now.Year - 30, 4, 12, noMinimumAge, _rockContext );

            Assert.AreEqual( ChatBirthdateCode.NotAsked, result.Code, "a church with no minimum age" );
            AssertUnchanged( person, null, null, null );

            // A banned person's gate refuses before the age gate is reached.
            TestInitialize();
            var banned = Stored( null, null, null );
            AddBanListMember( _rockContext, PersonId, GroupMemberStatus.Active, isArchived: false );

            var bannedResult = ChatSessionHelper.SaveBirthdate( PersonId, RockDateTime.Now.Year - 30, 4, 12, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatBirthdateCode.NotAsked, bannedResult.Code, "a banned person" );
            Assert.AreEqual( "banned", bannedResult.Session.Gate );
            AssertUnchanged( banned, null, null, null );
            AssertNothingSaved();
        }

        [TestMethod]
        public void SaveBirthdate_DateThatIsIncompleteImpossibleOrInTheFuture_ChangesNothing()
        {
            var tomorrow = RockDateTime.Today.AddDays( 1 );
            var cases = new[]
            {
                (Year: 0, Month: 4, Day: 12, Name: "no year"),
                (Year: RockDateTime.Now.Year - 30, Month: 0, Day: 12, Name: "no month"),
                (Year: RockDateTime.Now.Year - 30, Month: 4, Day: 0, Name: "no day"),
                (Year: RockDateTime.Now.Year - 30, Month: 2, Day: 30, Name: "30 February"),
                (Year: RockDateTime.Now.Year - 30, Month: 13, Day: 1, Name: "a thirteenth month"),
                (Year: 1, Month: 4, Day: 12, Name: "the year Rock stores as no year"),
                (Year: tomorrow.Year, Month: tomorrow.Month, Day: tomorrow.Day, Name: "tomorrow")
            };

            foreach ( var c in cases )
            {
                TestInitialize();
                var person = Stored( null, null, null );

                var result = ChatSessionHelper.SaveBirthdate( PersonId, c.Year, c.Month, c.Day, ValidConfig(), _rockContext );

                Assert.AreEqual( ChatBirthdateCode.InvalidDate, result.Code, c.Name );
                AssertUnchanged( person, null, null, null );
                AssertNothingSaved();
                Assert.AreEqual( "age_verification_required", result.Session.Gate, c.Name );
            }
        }

        [TestMethod]
        public void SaveBirthdate_NobodySignedIn_IsRefused()
        {
            var result = ChatSessionHelper.SaveBirthdate( null, RockDateTime.Now.Year - 30, 4, 12, ValidConfig(), _rockContext );

            Assert.AreEqual( ChatBirthdateCode.SignInRequired, result.Code );
            Assert.AreEqual( "sign_in_required", result.Session.Gate );
        }

        #endregion Refused, nothing written

        #region Helpers

        private Person Stored( int? year, int? month, int? day )
        {
            var person = Adult();
            person.BirthYear = year;
            person.BirthMonth = month;
            person.BirthDay = day;

            _rockContext.Set<Person>().Add( person );

            return person;
        }

        private static void AssertUnchanged( Person person, int? year, int? month, int? day )
        {
            Assert.AreEqual( year, person.BirthYear, "the birth year changed" );
            Assert.AreEqual( month, person.BirthMonth, "the birth month changed" );
            Assert.AreEqual( day, person.BirthDay, "the birth day changed" );
        }

        private void AssertNothingSaved()
        {
            _rockContextMock.Verify( m => m.SaveChanges(), Times.Never(), "a refusal saved" );
            _rockContextMock.Verify( m => m.SaveChanges( It.IsAny<bool>() ), Times.Never(), "a refusal saved" );
            _rockContextMock.Verify( m => m.SaveChanges( It.IsAny<SaveChangesArgs>() ), Times.Never(), "a refusal saved" );
        }

        #endregion Helpers
    }
}
