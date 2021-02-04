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
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class AttributeFilterTests : LavaIntegrationTestBase
    {
        #region Filter Tests: Attribute

        /// <summary>
        /// Applying the Attribute filter to a known entity returns the Attribute value.
        /// </summary>
        [TestMethod]
        public void AttributeFilter_ForEntityDefaultAttribute_ReturnsCorrectValue()
        {
            var personDecker = TestHelper.GetTestPersonTedDecker();

            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = new RockContext();

            var tedDeckerPerson = new PersonService( rockContext ).Queryable().First( x => x.Guid == tedDeckerGuid );

            var values = new LavaDataDictionary { { "Person", tedDeckerPerson } };

            TestHelper.AssertTemplateOutput("2001-09-13",
                "{{ Person | Attribute:'BaptismDate' | Date:'yyyy-MM-dd' }}",
                values );
        }

        #endregion
    }
}
