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
using System.Data.Entity.Spatial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Newtonsoft.Json;
using Rock.Model;
using Assert = NUnit.Framework.Assert;
using Rock.Web.Cache;

namespace Rock.Tests.Model
{
    [TestFixture]
    public class PersonTests
    {
        /// <summary>
        /// Note: Using these Microsoft.VisualStudio.TestTools.UnitTesting Attribute markup
        /// allows the tests to be grouped by "trait" (ie, the TestCategory defined below).
        /// </summary>
        [TestClass]
        public class TheGradeProperty
        {
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShoulBeGrade13GraduateToday()
            {
                InitGlobalAttributesCache();
                var Person = new Person();
                Person.GraduationDate = RockDateTime.Now; // the "year" the person graduates.

                Assert.True( 13 == Person.Grade );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShoulBeGrade12GraduateTomorrow()
            {
                InitGlobalAttributesCache();

                DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
                SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

                var Person = new Person();
                Person.GraduationDate = RockDateTime.Now; // the "year" the person graduates.

                Assert.True( 12 == Person.Grade );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShouldSetGraduationToNextYearIfGradeGraduationToday()
            {
                InitGlobalAttributesCache();
                var Person = new Person();
                Person.Grade = 12;
                
                Assert.True( Person.GraduationDate.Value.Year == RockDateTime.Now.AddYears(1).Year );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShouldSetGraduationToThisYearIfGrade12GradTomorrow()
            {
                InitGlobalAttributesCache();

                DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
                SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

                var Person = new Person();
                Person.Grade = 12;

                Assert.True( Person.GraduationDate.Value.Year == RockDateTime.Now.Year );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShouldSetGraduationDateToGradeTransitionDate()
            {
                InitGlobalAttributesCache();

                DateTime lastMonth = RockDateTime.Now.AddMonths( -1 ).AddDays( -RockDateTime.Now.Day+1 );
                SetGradeTransitionDateGlobalAttribute( lastMonth.Month, lastMonth.Day );

                var Person = new Person();
                Person.Grade = 11;
                System.Diagnostics.Debug.WriteLine( "Person's Graduation Date: " + Person.GraduationDate.Value.ToShortDateString() );
                System.Diagnostics.Debug.WriteLine( "GradeTransitionDate: " + lastMonth.ToShortDateString() );
                Assert.True( Person.GraduationDate.Value.Month == lastMonth.Month );
                Assert.True( Person.GraduationDate.Value.Day == lastMonth.Day );
            }

            private static void InitGlobalAttributesCache()
            {
                DateTime today = RockDateTime.Now;
                GlobalAttributesCache globalAttributes = GlobalAttributesCache.Read();
                globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", today.Month, today.Day ), false );
            }

            private static void SetGradeTransitionDateGlobalAttribute( int month, int day )
            {
                GlobalAttributesCache globalAttributes = GlobalAttributesCache.Read();
                globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", month, day ), false );
            }


        }
    }
}
