﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Integration.Core
{
    [TestClass]
    public class SundayDateTest
    {
        [TestMethod]
        public void TestPerformance()
        {
            // warm up
            var warmup = RockDateTime.FirstDayOfWeek;
            int testCount = 1000000;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for ( int i = 0; i < testCount; i++ )
            {
                var firstDayOfWeek = RockDateTime.FirstDayOfWeek;
            }

            stopwatch.Stop();
            Debug.WriteLine( $"{stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalMilliseconds / testCount}ms/count  RockDateTime.FirstDayOfWeek" );

            stopwatch.Restart();
            for ( int i = 0; i < testCount; i++ )
            {
                var firstDayOfWeek = RockDateTime.DefaultFirstDayOfWeek;
            }

            stopwatch.Stop();
            Debug.WriteLine( $"{stopwatch.Elapsed.TotalMilliseconds}ms RockDateTime.DefaultFirstDayOfWeek" );
        }


        [TestMethod]
        public void TestFirstDateOfWeekMonday()
        {
            var sundayDate20190825 = new DateTime( 2019, 8, 25 );
            var sundayDate20190901 = new DateTime( 2019, 9, 1 );
            var sundayDate20190908 = new DateTime( 2019, 9, 8 );
            var sundayDate20190915 = new DateTime( 2019, 9, 15 );
            var sundayDate20190922 = new DateTime( 2019, 9, 22 );

            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK, DayOfWeek.Monday.ConvertToInt().ToString() );
            RockDateTime.UpdateSundayDateData();

            Assert.IsTrue( new DateTime( 2019, 8, 23 ).SundayDate() == sundayDate20190825, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 24 ).SundayDate() == sundayDate20190825, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 8, 25 ).SundayDate() == sundayDate20190825, "Incorrect Sunday Date" );
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

            var rockContext = new RockContext();

            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 23 )}')" ).FirstOrDefault() == sundayDate20190825, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 24 )}')" ).FirstOrDefault() == sundayDate20190825, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 25 )}')" ).FirstOrDefault() == sundayDate20190825, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 26 )}')" ).FirstOrDefault() == sundayDate20190901, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 27 )}')" ).FirstOrDefault() == sundayDate20190901, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 28 )}')" ).FirstOrDefault() == sundayDate20190901, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 29 )}')" ).FirstOrDefault() == sundayDate20190901, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 8, 30 )}')" ).FirstOrDefault() == sundayDate20190901, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 9, 5 )}')" ).FirstOrDefault() == sundayDate20190908, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 9, 9 )}')" ).FirstOrDefault() == sundayDate20190915, "Incorrect Sunday Date (SQL)" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 9, 17 )}')" ).FirstOrDefault() == sundayDate20190922, "Incorrect Sunday Date (SQL)" );

        }

        [TestMethod]
        public void TestFirstDateOfWeekTuesday()
        {
            var sundayDate20191006 = new DateTime( 2019, 10, 6 );
            var sundayDate20191013 = new DateTime( 2019, 10, 13 );
            var sundayDate20191020 = new DateTime( 2019, 10, 20 );
            var sundayDate20191027 = new DateTime( 2019, 10, 27 );

            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK, DayOfWeek.Tuesday.ConvertToInt().ToString() );
            RockDateTime.UpdateSundayDateData();

            Assert.IsTrue( new DateTime( 2019, 10, 1 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 5 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 6 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 7 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 8 ).SundayDate() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 14 ).SundayDate() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 13 ).SundayDate() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 21 ).SundayDate() == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 22 ).SundayDate() != sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 22 ).SundayDate() == sundayDate20191027, "Incorrect Sunday Date" );

            var rockContext = new RockContext();

            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 1 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 5 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 6 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 7 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 8 )}')" ).FirstOrDefault() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 14 )}')" ).FirstOrDefault() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 13 )}')" ).FirstOrDefault() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 21 )}')" ).FirstOrDefault() == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 22 )}')" ).FirstOrDefault() != sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 22 )}')" ).FirstOrDefault() == sundayDate20191027, "Incorrect Sunday Date" );

        }

        [TestMethod]
        public void TestFirstDateOfWeekSunday()
        {
            var sundayDate20191006 = new DateTime( 2019, 10, 6 );
            var sundayDate20191013 = new DateTime( 2019, 10, 13 );
            var sundayDate20191020 = new DateTime( 2019, 10, 20 );
            var sundayDate20191027 = new DateTime( 2019, 10, 27 );

            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK, DayOfWeek.Sunday.ConvertToInt().ToString() );
            RockDateTime.UpdateSundayDateData();


            Assert.IsTrue( new DateTime( 2019, 10, 6 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 8 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 9 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 12 ).SundayDate() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 13 ).SundayDate() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 14 ).SundayDate() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 19 ).SundayDate() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 20 ).SundayDate() == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 25 ).SundayDate() == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( new DateTime( 2019, 10, 28 ).SundayDate() == sundayDate20191027, "Incorrect Sunday Date" );

            var rockContext = new RockContext();

            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 6 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 8 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 9 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 12 )}')" ).FirstOrDefault() == sundayDate20191006, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 13 )}')" ).FirstOrDefault() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 14 )}')" ).FirstOrDefault() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 19 )}')" ).FirstOrDefault() == sundayDate20191013, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 20 )}')" ).FirstOrDefault() == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 25 )}')" ).FirstOrDefault() == sundayDate20191020, "Incorrect Sunday Date" );
            Assert.IsTrue( rockContext.Database.SqlQuery<DateTime>( $@"SELECT dbo.ufnUtility_GetSundayDate('{new DateTime( 2019, 10, 28 )}')" ).FirstOrDefault() == sundayDate20191027, "Incorrect Sunday Date" );
        }
    }
}
