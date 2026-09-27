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

using Rock.Lava;
using Rock.Utility;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Test subjects for Lava tests, expressed as plain Lava data objects.
    /// </summary>
    /// <remarks>
    /// These are the people the Lava documentation examples talk about. Nothing
    /// here touches a database - they exist so that a test can put a familiar,
    /// predictable object into a template's merge fields.
    /// </remarks>
    public static class LavaTestData
    {
        #region Methods

        /// <summary>
        /// Returns the test subject Ted Decker.
        /// </summary>
        /// <returns>An initialized <see cref="TestPerson"/>.</returns>
        public static TestPerson GetTestPersonTedDecker()
        {
            var campus = new TestCampus { Name = "North Campus", Id = 1 };

            return new TestPerson { FirstName = "Edward", NickName = "Ted", LastName = "Decker", Campus = campus, Id = 1 };
        }

        /// <summary>
        /// Returns the test subject Alisha Marble.
        /// </summary>
        /// <returns>An initialized <see cref="TestPerson"/>.</returns>
        public static TestPerson GetTestPersonAlishaMarble()
        {
            var campus = new TestCampus { Name = "South Campus", Id = 102 };

            return new TestPerson { FirstName = "Alisha", NickName = "Alisha", LastName = "Marble", Campus = campus, Id = 2 };
        }

        /// <summary>
        /// Returns the test subject Bill Marble.
        /// </summary>
        /// <returns>An initialized <see cref="TestPerson"/>.</returns>
        public static TestPerson GetTestPersonBillMarble()
        {
            var campus = new TestCampus { Name = "South Campus", Id = 101 };

            return new TestPerson { FirstName = "William", NickName = "Bill", LastName = "Marble", Campus = campus, Id = 2 };
        }

        /// <summary>
        /// Returns the Decker family.
        /// </summary>
        /// <returns>A collection of initialized <see cref="TestPerson"/> objects.</returns>
        public static List<TestPerson> GetTestPersonCollectionForDecker()
        {
            return new List<TestPerson>
            {
                GetTestPersonTedDecker(),
                new TestPerson { FirstName = "Cindy", NickName = "Cindy", LastName = "Decker", Id = 2 },
                new TestPerson { FirstName = "Noah", NickName = "Noah", LastName = "Decker", Id = 3 },
                new TestPerson { FirstName = "Alex", NickName = "Alex", LastName = "Decker", Id = 4 }
            };
        }

        /// <summary>
        /// Returns the Decker family followed by the Marbles.
        /// </summary>
        /// <returns>A collection of initialized <see cref="TestPerson"/> objects.</returns>
        public static List<TestPerson> GetTestPersonCollectionForDeckerAndMarble()
        {
            var personList = GetTestPersonCollectionForDecker();

            personList.Add( GetTestPersonBillMarble() );
            personList.Add( GetTestPersonAlishaMarble() );

            return personList;
        }

        #endregion Methods
    }

    /// <summary>
    /// A test subject whose Id is the only member marked visible to Lava, used to
    /// verify that an unmarked member of a <see cref="RockDynamic"/> stays hidden.
    /// </summary>
    public class TestSecuredRockDynamicObject : RockDynamic
    {
        [LavaVisible]
        public int Id { get; set; }

        public string NickName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public TestCampus Campus { get; set; }

        public override string ToString()
        {
            return $"{NickName} {LastName}";
        }
    }

    /// <summary>
    /// A test subject whose Id is the only member marked visible to Lava, used to
    /// verify that an unmarked member of a <see cref="LavaDataObject"/> stays hidden.
    /// </summary>
    public class TestSecuredLavaDataObject : LavaDataObject
    {
        [LavaVisible]
        public int Id { get; set; }

        public string NickName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public TestCampus Campus { get; set; }

        public override string ToString()
        {
            return $"{NickName} {LastName}";
        }
    }
}
