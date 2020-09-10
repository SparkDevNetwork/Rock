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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Model
{
    public class GroupTests
    {
        /// <summary>
        /// Used for testing anything regarding GroupMember.
        /// </summary>
        [TestClass]
        public class GroupMemberTests
        {
            #region Setup

            /// <summary>
            /// Runs before any tests in this class are executed.
            /// </summary>
            [ClassInitialize]
            public static void ClassInitialize( TestContext testContext )
            {
                DatabaseTests.ResetDatabase();
            }

            /// <summary>
            /// Runs after all tests in this class is executed.
            /// </summary>
            [ClassCleanup]
            public static void ClassCleanup()
            {
                DatabaseTests.DeleteDatabase();
            }

            /// <summary>
            /// Runs after each test in this class is executed.
            /// Deletes the test data added to the database for each tests.
            /// </summary>
            [TestCleanup]
            public void Cleanup()
            {


            }

            #endregion

            #region Group Member Tests

            /// <summary>
            /// Ensure the Exclude Archived Group Member
            /// </summary>
            [TestMethod]
            public void GetGroupMembersExcludeArchived()
            {
                List<GroupMember> deceasedList = new List<GroupMember>();

                using ( var rockContext = new RockContext() )
                {
                    deceasedList = new GroupMemberService( rockContext ).Queryable( true ).ToList();
                }

                var areAnyArchived = deceasedList.Any( x => x.IsArchived );
                Assert.That.IsFalse( areAnyArchived );
            }

            /// <summary>
            /// Ensure the Deceased Group Member is included
            /// Depends on at least one Group Member being in a group that is deceased
            /// </summary>
            [TestMethod]
            public void GetGroupMembersIncludeDeceased()
            {
                List<GroupMember> deceasedList = new List<GroupMember>();
                List<Person> deceasedpersonList = new List<Person>();
                using ( var rockContext = new RockContext() )
                {

                    deceasedList = new GroupMemberService( rockContext ).Queryable( true ).ToList();

                    foreach ( GroupMember deceasedgMember in deceasedList )
                    {
                        if ( deceasedgMember.Person.IsDeceased )
                        {
                            Assert.That.IsTrue( deceasedgMember.Person.IsDeceased );
                        }
                    }
                    Assert.That.Fail();
                }
            }

            #endregion



        }
    }
}
