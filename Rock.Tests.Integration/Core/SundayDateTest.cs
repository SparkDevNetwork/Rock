using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;

namespace Rock.Tests.Integration.RockTests.Core
{
    [TestClass]
    public class SundayDateTest
    {
        [TestMethod]
        public void TestFirstDateOfWeekMonday()
        {
            var sundayDate20190825 = new DateTime( 2019, 8, 25 );
            var sundayDate20190901 = new DateTime( 2019, 9, 1 );
            var sundayDate20190908 = new DateTime( 2019, 9, 8 );
            var sundayDate20190915 = new DateTime( 2019, 9, 15 );
            var sundayDate20190922 = new DateTime( 2019, 9, 22);


            Assert.IsTrue( new DateTime( 2019, 8, 23 ).SundayDate() == sundayDate20190825, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 24 ).SundayDate() == sundayDate20190825, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 25 ).SundayDate() == sundayDate20190825,  "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 8, 26 ).SundayDate() == sundayDate20190901, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 27 ).SundayDate() == sundayDate20190901, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 28 ).SundayDate() == sundayDate20190901, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 29 ).SundayDate() == sundayDate20190901, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 30 ).SundayDate() == sundayDate20190901, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 9, 5 ).SundayDate() == sundayDate20190908, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 9, 9 ).SundayDate() == sundayDate20190915, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 9, 17 ).SundayDate() == sundayDate20190922, "Incorrect Sunday Date" );

            var attendanceOccurrence = new AttendanceOccurrence() { OccurrenceDate = new DateTime( 2019, 9, 17 ) };
            Assert.IsTrue( attendanceOccurrence.SundayDate == new DateTime( 2019, 9, 17 ).SundayDate() );


            attendanceOccurrence = new AttendanceOccurrence() { OccurrenceDate = new DateTime( 2019, 9, 19 ) };
            Assert.IsTrue( attendanceOccurrence.SundayDate == new DateTime( 2019, 9, 19 ).SundayDate() );
        }

        [TestMethod]
        public void TestFirstDateOfWeekTuesday()
        {
            var sundayDate20191006 = new DateTime( 2019, 10, 6 );
            var sundayDate20191013 = new DateTime( 2019, 10, 13 );
            var sundayDate20191020 = new DateTime( 2019, 10, 20 );
            var sundayDate20191027 = new DateTime( 2019, 10, 27 );


            Assert.IsTrue( new DateTime( 2019, 10, 1 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 5 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 6 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 7 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191006, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 10, 8 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 14 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 13 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191013, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 10, 21 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191020, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 10, 22 ).SundayDate( DayOfWeek.Tuesday ) != sundayDate20191020, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 10, 22 ).SundayDate( DayOfWeek.Tuesday ) == sundayDate20191027, "Incorrect Sunday Date" );

        }

        [TestMethod]
        public void TestFirstDateOfWeekSunday()
        {
            var sundayDate20191006 = new DateTime( 2019, 10, 6 );
            var sundayDate20191013 = new DateTime( 2019, 10, 13 );
            var sundayDate20191020 = new DateTime( 2019, 10, 20 );
            var sundayDate20191027 = new DateTime( 2019, 10, 27 );


            Assert.IsTrue( new DateTime( 2019, 10, 6 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 8 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 9 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 12 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191006, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 10, 13 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 14 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 19 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191013, "Incorrect Sunday Date" );

            Assert.IsTrue( new DateTime( 2019, 10, 20 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 25 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 28 ).SundayDate( DayOfWeek.Sunday ) == sundayDate20191027, "Incorrect Sunday Date" );

            

        }
    }
}
