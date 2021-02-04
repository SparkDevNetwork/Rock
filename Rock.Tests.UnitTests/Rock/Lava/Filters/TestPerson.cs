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
using Rock.Lava;

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// A representation of a Person used for testing purposes.
    /// </summary>
    public class TestPerson : LavaDataObject
    {
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
    /// A representation of a Campus used for testing purposes.
    /// </summary>
    public class TestCampus : LavaDataObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
