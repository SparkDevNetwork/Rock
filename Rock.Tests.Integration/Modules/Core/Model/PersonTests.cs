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
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class PersonTests : DatabaseTestsBase
    {
        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
        }

        [TestMethod]
        public void GraduatesThisYear()
        {
            DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
            SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

            int thisYear = RockDateTime.Now.Year;
            var Person = new Person();

            // set the GraduationYear to this year using GradeOffset
            Person.GradeOffset = 0;

            // Grade Transition isn't until tomorrow, so if their GradeOffset is 0,they should graduation day should be this year
            Assert.That.IsTrue( Person.GraduationYear == thisYear );
        }

        [TestMethod]
        public void GraduatesNextYear()
        {
            DateTime yesterday = RockDateTime.Now.AddDays( -1 );
            SetGradeTransitionDateGlobalAttribute( yesterday.Month, yesterday.Day );

            int nextYear = RockDateTime.Now.Year + 1;
            var Person = new Person();

            // set the GraduationYear to this year using GradeOffset
            Person.GradeOffset = 0;

            // Grade Transition was yesterday, so if their GradeOffset is 0, they should graduate next year
            Assert.That.IsTrue( Person.GraduationYear == nextYear );
        }
        
        private static void SetGradeTransitionDateGlobalAttribute( int month, int day )
        {
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", month, day ), false );
        }
    }
}
