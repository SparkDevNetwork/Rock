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
using System.ComponentModel;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    [TestCategory( "Core.Crm.Person" )]
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

        #region Graduation

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

        #endregion

        #region Anniversaries

        [TestMethod]
        public void DaysToBirthday_ForCurrentDate_ReturnsExpectedNumberOfDays()
        {
            const int daysOffset = 101;

            var person = new Person();
            person.SetBirthDate( RockDateTime.Now.AddDays( daysOffset ).AddYears( -10 ) );

            Assert.AreEqual( daysOffset, person.DaysToBirthdayOrNull );
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual( daysOffset, person.DaysToBirthday );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        public void DaysToBirthday_ForUndefinedDate_ReturnsNull()
        {
            var person = new Person();
            person.SetBirthDate( null );

            Assert.AreEqual( null, person.DaysToBirthdayOrNull );
        }

        [TestMethod]
        public void DaysToAnniversary_ForCurrentDate_ReturnsExpectedNumberOfDays()
        {
            const int daysOffset = 102;

            var person = new Person();
            person.AnniversaryDate = RockDateTime.Now.AddDays( daysOffset ).AddYears( -10 );

            Assert.AreEqual( daysOffset, person.DaysToAnniversaryOrNull );
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual( daysOffset, person.DaysToAnniversary );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        public void DaysToAnniversary_ForUndefinedDate_ReturnsNull()
        {
            var person = new Person();
            person.AnniversaryDate = null;

            Assert.AreEqual( null, person.DaysToAnniversaryOrNull );
        }

        [TestMethod]
        public void DaysToAnniversary_RangeTestForUndefinedDate_DoesNotPass()
        {
            var person = new Person();
            person.AnniversaryDate = null;

            var anniversaryThisWeek = person.DaysToAnniversaryOrNull < 7;
            var anniversaryNextWeek = person.DaysToAnniversaryOrNull > 7 && person.DaysToAnniversaryOrNull < 15;

            Assert.IsFalse( anniversaryThisWeek || anniversaryNextWeek );
        }

        [DataTestMethod]
        [DataRow( "30-06", "2020-06-29", 1, DisplayName = "Anniversary is the next day." )]
        [DataRow( "30-06", "2020-06-30", 0, DisplayName = "Anniversary is the same day." )]
        [DataRow( "30-06", "2020-07-01", 364, DisplayName = "Anniversary is the previous day." )]
        [DataRow( "29-02", "2020-03-01", 364, DisplayName = "Anniversary has no leap day for next occurrence." )]
        [DataRow( "01-03", "2020-02-28", 2, DisplayName = "Period to next date includes a leap day." )]
        [DataRow( "31-06", "2020-06-30", 0, DisplayName = "Invalid Day." )]
        [DataRow( "01-13", "2020-07-07", null, DisplayName = "Invalid Month." )]
        public void GetDaysToAnniversary_ForSpecificDates_ReturnsExpectedValue( string anniversaryDayMonth, string asAtDate, int? expectedNumberOfDays )
        {
            var dateParts = anniversaryDayMonth.Split( '-' );
            var daysUntil = Person.GetDaysToNextAnnualDate( dateParts[0].AsInteger(), dateParts[1].AsInteger(), asAtDate.AsDateTime().Value );

            Assert.AreEqual( expectedNumberOfDays, daysUntil );
        }

        #endregion
    }
}
