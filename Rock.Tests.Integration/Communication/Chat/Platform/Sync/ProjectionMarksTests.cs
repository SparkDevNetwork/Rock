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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The high-water marks a submission carries about the tables it read.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The platform keeps the highest it has seen of each and refuses a submission whose
    ///         marks went backwards, which is what a database restored from a backup looks like and
    ///         what ordinary operation never does.
    ///     </para>
    ///     <para>
    ///         That only works if the mark is the identity seed. The largest id in a table drops the
    ///         moment its newest rows are deleted, which happens in every church every week, so a
    ///         mark built that way would refuse a healthy church instead of a restored one. The cell
    ///         below deletes the newest row of three tables and requires the largest id to fall and
    ///         the mark not to.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ProjectionMarksTests : DatabaseTestsBase
    {
        [TestMethod]
        public void DeletingTheNewestRowsLeavesTheMarksWhereTheyWere()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Marks channel" );
                var personId = fixture.AddPerson( "Marks" );
                fixture.AddMember( channelGuid, personId );

                var before = fixture.Project().Result.Marks;

                Assert.IsTrue( before.Person > 0, "no mark was taken for the person table" );
                Assert.IsTrue( before.PersonAlias > 0, "no mark was taken for the person alias table" );
                Assert.IsTrue( before.Group > 0, "no mark was taken for the group table" );
                Assert.IsTrue( before.GroupMember > 0, "no mark was taken for the group membership table" );

                var largestPerson = LargestId( "Person" );
                var largestAlias = LargestId( "PersonAlias" );
                var largestMember = LargestId( "GroupMember" );

                // Everything this fixture made, which holds the newest row of each of these three
                // tables. The group table is left out of the comparison because adding a person
                // creates that person's family, and the family group is not this fixture's to delete.
                DeleteOurRows( fixture.ForeignKey );

                Assert.IsTrue( LargestId( "Person" ) < largestPerson, "the newest person was not deleted, so this proves nothing" );
                Assert.IsTrue( LargestId( "PersonAlias" ) < largestAlias, "the newest alias was not deleted, so this proves nothing" );
                Assert.IsTrue( LargestId( "GroupMember" ) < largestMember, "the newest membership was not deleted, so this proves nothing" );

                var after = fixture.Project().Result.Marks;

                Assert.AreEqual( before.Person, after.Person,
                    "the person mark followed a delete, so it is the largest id rather than the seed and this church is refused as soon as it deletes a person" );
                Assert.AreEqual( before.PersonAlias, after.PersonAlias, "the person alias mark followed a delete" );
                Assert.AreEqual( before.GroupMember, after.GroupMember, "the group membership mark followed a delete" );
                Assert.AreEqual( before.Group, after.Group, "the group mark moved while nothing was added" );
            }
        }

        #region Support

        private static long LargestId( string table )
        {
            using ( var rockContext = new RockContext() )
            {
                return rockContext.Database
                    .SqlQuery<long>( string.Format( "SELECT CAST( ISNULL( MAX( [Id] ), 0 ) AS BIGINT ) FROM [{0}]", table ) )
                    .First();
            }
        }

        private static void DeleteOurRows( string foreignKey )
        {
            using ( var rockContext = new RockContext() )
            {
                ChatSyncProjectionFixture.DeletePeopleAndChannels( rockContext, foreignKey );
            }
        }

        #endregion Support
    }
}
