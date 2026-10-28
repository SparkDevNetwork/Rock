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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Doors;

namespace Rock.Tests.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The rules behind a profile card's details that need no database: whose details show, the
    /// address a phone dials, and how many people one viewer may read in a minute.
    /// </summary>
    [TestClass]
    public class ChatDoorProfileRuleTests
    {
        #region Whose details show

        [TestMethod]
        public void APersonsOwnChoiceWinsEitherWay()
        {
            Assert.IsTrue( ChatDoorHelper.IsShowingProfileDetails( true, churchDefault: false ) );
            Assert.IsFalse( ChatDoorHelper.IsShowingProfileDetails( false, churchDefault: true ) );
        }

        [TestMethod]
        public void SomeoneWhoNeverChoseFollowsTheChurch()
        {
            Assert.IsTrue( ChatDoorHelper.IsShowingProfileDetails( null, churchDefault: true ) );
            Assert.IsFalse( ChatDoorHelper.IsShowingProfileDetails( null, churchDefault: false ) );
        }

        #endregion Whose details show

        #region The address a phone dials

        [TestMethod]
        public void ANumberWithItsCountryDialsInternationally()
        {
            Assert.AreEqual( "tel:+16025550142", ChatDoorHelper.ToTelUri( "1", "6025550142" ) );
            Assert.AreEqual( "tel:+447700900123", ChatDoorHelper.ToTelUri( "+44", "7700 900123" ) );
        }

        [TestMethod]
        public void ANumberWithNoCountryDialsAsStored()
        {
            Assert.AreEqual( "tel:6025550142", ChatDoorHelper.ToTelUri( null, "(602) 555-0142" ) );
            Assert.AreEqual( "tel:6025550142", ChatDoorHelper.ToTelUri( " ", "6025550142" ) );
        }

        [TestMethod]
        public void ANumberWithNoDigitsDialsNothing()
        {
            Assert.IsNull( ChatDoorHelper.ToTelUri( "1", null ) );
            Assert.IsNull( ChatDoorHelper.ToTelUri( "1", "ext" ) );
        }

        [TestMethod]
        public void NothingButDigitsReachesTheAddress()
        {
            // A number is typed by people, so a stray character must not become part of a link.
            Assert.AreEqual( "tel:+16025550142", ChatDoorHelper.ToTelUri( "1\"", "602\"><x>5550142" ) );
        }

        #endregion The address a phone dials

        #region How many people one viewer reads

        private static readonly DateTime Start = new DateTime( 2026, 10, 27, 9, 0, 0 );

        [TestMethod]
        public void AViewerReadsUpToTheLimitOfDifferentPeople()
        {
            var window = new ChatDoorHelper.ProfileReadWindow( maxPeople: 3, maxReads: 100, Start );

            for ( var i = 0; i < 3; i++ )
            {
                Assert.IsTrue( window.TryAdmit( Guid.NewGuid(), Start.AddSeconds( i ) ), $"read {i + 1}" );
            }

            Assert.IsFalse( window.TryAdmit( Guid.NewGuid(), Start.AddSeconds( 3 ) ), "a fourth person in the same window" );
        }

        [TestMethod]
        public void ReopeningSomeoneAlreadyReadIsAdmittedAtThePeopleLimit()
        {
            var window = new ChatDoorHelper.ProfileReadWindow( maxPeople: 2, maxReads: 100, Start );
            var first = Guid.NewGuid();

            Assert.IsTrue( window.TryAdmit( first, Start ) );
            Assert.IsTrue( window.TryAdmit( Guid.NewGuid(), Start ) );

            Assert.IsTrue( window.TryAdmit( first, Start ), "the first person again, at the limit" );
            Assert.IsFalse( window.TryAdmit( Guid.NewGuid(), Start ) );
        }

        [TestMethod]
        public void ReadingTheSamePersonOverAndOverIsBoundedToo()
        {
            var window = new ChatDoorHelper.ProfileReadWindow( maxPeople: 2, maxReads: 5, Start );
            var person = Guid.NewGuid();

            for ( var i = 0; i < 5; i++ )
            {
                Assert.IsTrue( window.TryAdmit( person, Start ), $"read {i + 1}" );
            }

            Assert.IsFalse( window.TryAdmit( person, Start ), "a sixth read of the same person" );
            Assert.IsFalse( window.TryAdmit( Guid.NewGuid(), Start ), "nor anyone else, once every read is spent" );
        }

        [TestMethod]
        public void TheWindowEndingAdmitsAgain()
        {
            var window = new ChatDoorHelper.ProfileReadWindow( maxPeople: 1, maxReads: 1, Start );
            Assert.IsTrue( window.TryAdmit( Guid.NewGuid(), Start ) );
            Assert.IsFalse( window.TryAdmit( Guid.NewGuid(), Start.AddSeconds( 59 ) ), "still in the minute" );

            Assert.IsTrue( window.TryAdmit( Guid.NewGuid(), Start.AddMinutes( 1 ) ), "the next minute" );
            Assert.IsFalse( window.TryAdmit( Guid.NewGuid(), Start.AddMinutes( 1 ) ), "which has its own limit" );
        }

        #endregion How many people one viewer reads
    }
}
