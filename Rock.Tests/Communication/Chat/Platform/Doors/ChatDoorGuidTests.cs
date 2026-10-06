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
    /// The Guid a new direct message group is given, worked out from the church and its people.
    /// </summary>
    /// <remarks>
    /// Two people starting the same conversation at the same moment must end up in one group, and
    /// Rock's rule is a database constraint rather than a lock, since a lock in one web server's
    /// memory means nothing to the next. So the Guid is not random: the same church and the same
    /// people always give the same one, whichever order they were chosen in, and the unique index
    /// on a group's Guid refuses the second create.
    /// </remarks>
    [TestClass]
    public class ChatDoorGuidTests
    {
        private static readonly Guid ChurchOne = new Guid( "10000000-0000-4000-8000-000000000001" );

        private static readonly Guid ChurchTwo = new Guid( "20000000-0000-4000-8000-000000000002" );

        [TestMethod]
        public void TheSamePeopleInAnyOrderGiveTheSameGuid()
        {
            var first = ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 12, 7, 31 } );

            Assert.AreEqual( first, ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 31, 12, 7 } ) );
            Assert.AreEqual( first, ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 7, 31, 12 } ) );
        }

        [TestMethod]
        public void APersonListedTwiceCountsOnce()
        {
            Assert.AreEqual(
                ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 7, 12 } ),
                ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 12, 7, 12 } ) );
        }

        [TestMethod]
        public void AnotherChurchOrAnotherSetOfPeopleGivesAnotherGuid()
        {
            var pair = ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 7, 12 } );

            Assert.AreNotEqual( pair, ChatDoorHelper.DirectMessageGuid( ChurchTwo, new[] { 7, 12 } ),
                "the platform keys every church's channels apart, but a Guid shared across churches would still be one coincidence away from confusing them" );
            Assert.AreNotEqual( pair, ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 7, 13 } ) );
            Assert.AreNotEqual( pair, ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 7, 12, 13 } ),
                "a group with one more person is a different conversation" );

            // Joined as text, ids 1 and 23 and ids 12 and 3 would read alike without a separator.
            Assert.AreNotEqual(
                ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 1, 23 } ),
                ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 12, 3 } ) );
        }

        [TestMethod]
        public void TheGuidIsANameBasedUuid()
        {
            var text = ChatDoorHelper.DirectMessageGuid( ChurchOne, new[] { 7, 12 } ).ToString( "D" );

            Assert.AreEqual( '5', text[14], "version 5, a name hashed with SHA-1" );
            StringAssert.Contains( "89ab", text[19].ToString(), "the RFC 4122 variant" );
        }
    }
}
