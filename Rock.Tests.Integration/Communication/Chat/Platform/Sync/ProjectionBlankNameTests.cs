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

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// A name Rock holds as blank never reaches the far side as blank.
    /// </summary>
    /// <remarks>
    /// The far side refuses an empty nick name, last name or badge name, and one refused row fails
    /// the whole church's submission on every cycle. Rock's own person save can store an empty nick
    /// name (a nick name that was only emoji is stripped after the blank one is filled), and an import
    /// or a direct edit can store any of the three. Each value here is written straight to its
    /// column, because that is how the rows this protects against get there.
    /// </remarks>
    [TestClass]
    public class ProjectionBlankNameTests : DatabaseTestsBase
    {
        #region Person names

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void ABlankNickName_TravelsAsNothing( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var personId = EnrolledPerson( fixture, "BlankNick" );
                fixture.SetNickNameDirectly( personId, blank );

                var payload = fixture.Project();
                var row = PrimaryRow( payload, personId );

                Assert.AreEqual( JTokenType.Null, payload.Value( "aliases", row, "nick_name" ).Type,
                    "a blank nick name was sent, and the far side refuses the whole church over it" );
                Assert.AreEqual( "BlankNick", ( string ) payload.Value( "aliases", row, "last_name" ),
                    "the last name was lost along with the blank nick name" );
            }
        }

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void ABlankLastName_TravelsAsNothing( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var personId = EnrolledPerson( fixture, "BlankLast" );
                fixture.SetLastNameDirectly( personId, blank );

                var payload = fixture.Project();
                var row = PrimaryRow( payload, personId );

                Assert.AreEqual( JTokenType.Null, payload.Value( "aliases", row, "last_name" ).Type,
                    "a blank last name was sent, and the far side refuses the whole church over it" );
                Assert.AreNotEqual( JTokenType.Null, payload.Value( "aliases", row, "nick_name" ).Type,
                    "the nick name was lost along with the blank last name" );
            }
        }

        /// <summary>
        /// Only a blank name is changed. Anything else is sent exactly as Rock holds it, spaces and all.
        /// </summary>
        [TestMethod]
        public void ANameWithSpacesAroundIt_TravelsAsRockHoldsIt()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var personId = EnrolledPerson( fixture, "Spaced" );
                fixture.SetNickNameDirectly( personId, " Ann " );

                var payload = fixture.Project();
                var row = PrimaryRow( payload, personId );

                Assert.AreEqual( " Ann ", ( string ) payload.Value( "aliases", row, "nick_name" ) );
            }
        }

        #endregion Person names

        #region Badge names

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void ABadgeWithABlankName_IsProjectedWithAName( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddBadge( "Named on the way in" );
                fixture.SetBadgeNameDirectly( badgeGuid, blank );

                var payload = fixture.Project();
                var row = payload.Row( "badges", "badge_key", badgeGuid );

                Assert.IsNotNull( row, "the badge was not projected at all" );
                Assert.IsFalse( string.IsNullOrWhiteSpace( ( string ) payload.Value( "badges", row, "name" ) ),
                    "a badge with no name fails the whole submission rather than that one row" );
            }
        }

        #endregion Badge names

        #region Support

        private static int EnrolledPerson( ChatSyncProjectionFixture fixture, string lastName )
        {
            var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, lastName + " channel" );
            var personId = fixture.AddPerson( lastName );
            fixture.AddMember( channelGuid, personId );

            return personId;
        }

        private static JArray PrimaryRow( ProjectedPayload payload, int personId )
        {
            Guid primaryAliasGuid;

            using ( var rockContext = new RockContext() )
            {
                primaryAliasGuid = new PersonService( rockContext ).Queryable()
                    .Where( p => p.Id == personId )
                    .Select( p => p.Aliases.FirstOrDefault( a => a.Id == p.PrimaryAliasId ).Guid )
                    .First();
            }

            var row = payload.Row( "aliases", "person_alias_guid", primaryAliasGuid );

            Assert.IsNotNull( row, "the person's primary alias was not projected" );

            return row;
        }

        #endregion Support
    }
}
