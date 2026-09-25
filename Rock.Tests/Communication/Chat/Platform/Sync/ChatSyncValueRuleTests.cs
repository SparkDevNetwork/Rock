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
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Tests the rules for turning a value Rock holds into the one the wire carries.
    /// </summary>
    /// <remarks>
    /// Every value here would be accepted by the far side if it were sent as Rock holds it, and be
    /// wrong. That is what makes these worth their own tests: none of these failures raises
    /// anything anywhere.
    /// </remarks>
    [TestClass]
    public class ChatSyncValueRuleTests
    {
        #region Writing a value

        /// <summary>
        /// Guids are written lowercase and hyphenated, which is the one form that parses as a
        /// Postgres uuid and compares equal to the same value stored there.
        /// </summary>
        [TestMethod]
        public void Guids_AreWrittenLowercaseAndHyphenated()
        {
            var text = WriteOneValue( new Guid( "DFDC14A3-D1DC-4342-A012-5CE9E8994B5E" ) );

            StringAssert.Contains( text, "dfdc14a3-d1dc-4342-a012-5ce9e8994b5e", "the guid was not written lowercase and hyphenated" );
        }

        /// <summary>
        /// A collection of guids is written as a JSON array.
        /// </summary>
        /// <remarks>
        /// The badge column on the far side is a uuid array, and the drain reads anything that is
        /// not a JSON array as an empty one. A joined string would therefore give every person no
        /// badges, on a submission the platform accepts, with no error raised anywhere.
        /// </remarks>
        [TestMethod]
        public void CollectionsOfGuids_AreWrittenAsArraysRatherThanJoinedText()
        {
            var badges = new List<Guid>
            {
                new Guid( "C1000000-0000-4000-8000-000000000001" ),
                new Guid( "C1000000-0000-4000-8000-000000000002" )
            };

            var element = JToken.Parse( WriteOneValue( badges ) );

            Assert.AreEqual( JTokenType.Array, element.Type, "the badge keys were not written as an array, so every person arrives with none and nothing reports it" );
            CollectionAssert.AreEqual(
                new[] { "c1000000-0000-4000-8000-000000000001", "c1000000-0000-4000-8000-000000000002" },
                element.Select( e => e.Value<string>() ).ToArray(),
                "the badge keys are not lowercase hyphenated uuids" );
        }

        /// <summary>
        /// A time that is not UTC stops the submission rather than going out to be read in the
        /// platform's own zone.
        /// </summary>
        /// <remarks>
        /// Converting here silently would hide which times were already right, so the caller
        /// converts and this refuses anything it cannot vouch for.
        /// </remarks>
        [TestMethod]
        public void TimesThatAreNotUtc_StopTheSubmissionHere()
        {
            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => WriteOneValue( new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Unspecified ) ),
                "a time with no zone was written, so the platform reads it in its own zone and the value is wrong by the church's offset" );

            StringAssert.Contains( thrown.Message, "UTC", "the failure does not say what is wrong with the time" );
        }

        /// <summary>
        /// A UTC time is written with an explicit offset.
        /// </summary>
        [TestMethod]
        public void UtcTimes_AreWrittenWithAnExplicitOffset()
        {
            var text = WriteOneValue( new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Utc ) );

            StringAssert.Contains( text, "2026-09-21T18:00:00", "the time was not written in a form the platform parses" );
            StringAssert.Contains( text, "Z", "the time carries no offset, so it is read in the receiving session's zone" );
        }

        /// <summary>
        /// A value of a type with no agreed form on the wire stops the submission rather than being
        /// serialized however Json.NET decides.
        /// </summary>
        [TestMethod]
        public void Values_WithNoAgreedForm_StopTheSubmissionHere()
        {
            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => WriteOneValue( new byte[] { 1, 2 } ),
                "a value with no agreed form was written in a shape nobody chose" );

            StringAssert.Contains( thrown.Message, "members", "the failure does not name the section" );
        }

        /// <summary>
        /// Writes one value as the body writer would.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The JSON text.</returns>
        private static string WriteOneValue( object value )
        {
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                ChatPlatformSyncHelper.WriteValue( json, value, "members" );
            }

            return text.ToString();
        }

        #endregion

        #region Badge keys

        /// <summary>
        /// The joined keys become a list, in the order the church configured them.
        /// </summary>
        [TestMethod]
        public void BadgeKeys_BecomeAListRatherThanStayingOneString()
        {
            var keys = ChatPlatformSyncHelper.ReadBadgeKeys( "c1000000-0000-4000-8000-000000000001,c1000000-0000-4000-8000-000000000002" );

            CollectionAssert.AreEqual(
                new[]
                {
                    new Guid( "c1000000-0000-4000-8000-000000000001" ),
                    new Guid( "c1000000-0000-4000-8000-000000000002" )
                },
                keys.ToArray(),
                "the badge keys did not come out as a list in the order they were given" );
        }

        /// <summary>
        /// A person with no badges has an empty list, never a missing one.
        /// </summary>
        /// <remarks>
        /// The column on the far side cannot hold null, and it is the difference between a person
        /// who holds no badge and a row that did not say.
        /// </remarks>
        [TestMethod]
        public void BadgeKeys_ForAPersonWithNone_AreAnEmptyListRatherThanNothing()
        {
            foreach ( var nothing in new object[] { null, "", "   ", DBNull.Value } )
            {
                var keys = ChatPlatformSyncHelper.ReadBadgeKeys( nothing );

                Assert.IsNotNull( keys, "a person with no badges produced no list at all" );
                Assert.AreEqual( 0, keys.Count, "a person with no badges produced a list with something in it" );
            }
        }

        /// <summary>
        /// A key that is not an identifier stops the submission rather than being dropped.
        /// </summary>
        [TestMethod]
        public void BadgeKeys_ThatAreNotIdentifiers_StopTheSubmissionHere()
        {
            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatPlatformSyncHelper.ReadBadgeKeys( "c1000000-0000-4000-8000-000000000001,not-an-identifier" ),
                "a badge key that is not an identifier was dropped, so the badge disappears with the submission still accepted" );

            StringAssert.Contains( thrown.Message, "not-an-identifier", "the failure does not name the value that could not be read" );
        }

        #endregion

        #region Ban expiry

        /// <summary>
        /// The ban expiry is converted out of the organisation's zone.
        /// </summary>
        /// <remarks>
        /// The zone here is seven hours behind UTC and does not observe daylight saving, so six in
        /// the evening on the church's clock is one in the morning the next day in UTC. Sent
        /// unchanged it would be read as six in the evening UTC, and the ban would lift seven hours
        /// early.
        /// </remarks>
        [TestMethod]
        public void BanExpiry_IsConvertedFromTheOrganisationsZoneToUtc()
        {
            var converted = ChatPlatformSyncHelper.ToUtc( new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Unspecified ), ChatSyncTestBody.FixedOffsetZone() );

            Assert.IsTrue( converted.HasValue, "the ban expiry did not come out as a time" );
            Assert.AreEqual( DateTimeKind.Utc, converted.Value.Kind, "the ban expiry is not marked as UTC, so the writer cannot vouch for it" );
            Assert.AreEqual( new DateTime( 2026, 9, 22, 1, 0, 0, DateTimeKind.Utc ), converted.Value, "the ban expiry was not moved by the church's offset" );
        }

        /// <summary>
        /// A ban with no expiry is given none.
        /// </summary>
        [TestMethod]
        public void BanExpiry_WhenThereIsNone_StaysAbsent()
        {
            foreach ( var nothing in new object[] { null, DBNull.Value } )
            {
                Assert.IsNull( ChatPlatformSyncHelper.ToUtc( nothing, ChatSyncTestBody.FixedOffsetZone() ), "a ban that does not expire was given an expiry" );
            }
        }

        #endregion

        #region Badge colours

        /// <summary>
        /// A dark badge is written on in white and a light one in black.
        /// </summary>
        [TestMethod]
        public void BadgeColours_PickTheForegroundThatReadsAgainstTheBackground()
        {
            var dark = ChatPlatformSyncHelper.ReadBadgeColors( "#1B4D3E" );

            Assert.AreEqual( "#1b4d3e", dark.Item1, "the background is not the configured colour" );
            Assert.AreEqual( "#ffffff", dark.Item2, "a dark badge is not written on in white" );

            var light = ChatPlatformSyncHelper.ReadBadgeColors( "#FFE08A" );

            Assert.AreEqual( "#ffe08a", light.Item1, "the background is not the configured colour" );
            Assert.AreEqual( "#000000", light.Item2, "a light badge is not written on in black" );
        }

        /// <summary>
        /// The three digit form is expanded, because the far side accepts only the six digit one.
        /// </summary>
        [TestMethod]
        public void BadgeColours_InTheShortForm_AreExpanded()
        {
            var colors = ChatPlatformSyncHelper.ReadBadgeColors( "#ABC" );

            Assert.AreEqual( "#aabbcc", colors.Item1, "the three digit form was not expanded, so the far side refuses it" );
        }

        /// <summary>
        /// A colour this cannot read leaves the badge uncoloured rather than failing the church.
        /// </summary>
        /// <remarks>
        /// The field is free text in Rock. A badge with no colour still renders; a submission
        /// refused over one badge takes the whole church down for that cycle.
        /// </remarks>
        [TestMethod]
        public void BadgeColours_ThatCannotBeRead_LeaveTheBadgeUncoloured()
        {
            foreach ( var unreadable in new object[] { null, DBNull.Value, "", "cornflowerblue", "rgb(1,2,3)", "#12345", "#GGGGGG" } )
            {
                var colors = ChatPlatformSyncHelper.ReadBadgeColors( unreadable );

                Assert.IsNull( colors.Item1, string.Format( "a colour that cannot be read produced a background: {0}", unreadable ?? "(null)" ) );
                Assert.IsNull( colors.Item2, string.Format( "a colour that cannot be read produced a foreground: {0}", unreadable ?? "(null)" ) );
            }
        }

        #endregion
    }
}
