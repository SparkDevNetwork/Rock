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
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.RockTests.Model
{
    [TestClass]
    [Ignore( "Need a mock for Global Attributes" )]
    public class PersonTests
    {
        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
        }

        /// <summary>
        /// Runs before each test in this class is executed.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod]
        public void GraduatesThisYear()
        {
            InitGlobalAttributesCache();
            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.That.IsTrue( Person.GraduationYear == RockDateTime.Now.AddYears( 1 ).Year );
        }

        [TestMethod]
        [Ignore( "Need a mock for Global Attributes" )]
        public void OffsetGraduatesToday()
        {
            InitGlobalAttributesCache();
            var person = new Person();
            person.GraduationYear = RockDateTime.Now.Year; // the "year" the person graduates.

            Assert.That.IsTrue( 0 == person.GradeOffset );
        }

        [TestMethod]
        [Ignore( "Need a mock for Global Attributes" )]
        public void OffsetGraduatesTomorrow()
        {
            InitGlobalAttributesCache();

            DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
            SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

            var Person = new Person();
            Person.GraduationYear = RockDateTime.Now.Year; // the "year" the person graduates.

            Assert.That.IsTrue( 1 == Person.GradeOffset );
        }

        [TestMethod]

        public void GraduatesNextYear()
        {
            InitGlobalAttributesCache();

            DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
            SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.That.IsTrue( Person.GraduationYear == RockDateTime.Now.Year );
        }

        private static void InitGlobalAttributesCache()
        {
            DateTime today = RockDateTime.Now;
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", today.Month, today.Day ), false );
        }

        private static void SetGradeTransitionDateGlobalAttribute( int month, int day )
        {
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", month, day ), false );
        }
    }
}
