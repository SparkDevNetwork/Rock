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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Model
{
    [TestClass]
    public class EntityExtensionMethodTests
    {
        [TestMethod]
        public void IsRockEntityCollection_ForEntityCollectionType_ReturnsTrue()
        {
            bool result;
            // Test a null reference to a strongly-typed collection.
            List<Person> personList = null;
            result = personList.IsRockEntityCollection();
            Assert.That.IsTrue( result );

            // Test a reference to a concrete object.
            personList = new List<Person>();
            result = personList.IsRockEntityCollection();
            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void IsRockEntityCollection_ForNonEntityCollectionType_ReturnsFalse()
        {
            // Test a null reference to a collection of non-entities.
            List<string> stringListNull = null;
            var result = stringListNull.IsRockEntityCollection();
            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void IsRockEntityCollection_ForNonCollectionTypes_ReturnsFalse()
        {
            bool result;

            // Test a reference to a single Entity.
            var person = new Person();
            result = person.IsRockEntityCollection();
            Assert.That.IsFalse( result );

            // Test a reference to a non-Entity.
            var sb = new StringBuilder();
            result = sb.IsRockEntityCollection();
            Assert.That.IsFalse( result );
        }
    }
}