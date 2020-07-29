using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Utility;
using Rock.Tests.Shared;
using System.Collections.Generic;

namespace Rock.Tests.UnitTests.Rock.Utility
{
    [TestClass]
    public class TimeIntervalSettingTests
    {
        [TestMethod]
        public void TimeIntervalSettingShouldDefaultToCorrectValues()
        {
            var actualValue = new TimeIntervalSetting( null, null );

            Assert.That.AreEqual( 0, actualValue.IntervalValue );
            Assert.That.AreEqual( IntervalTimeUnit.Hour, actualValue.IntervalUnit );
        }

        [TestMethod]
        public void TimeIntervalSettingShouldGetSetToCorrectValues()
        {
            var expectedIntervalValue = 30;
            var expectedIntervalUnit = IntervalTimeUnit.Minute;

            var actualValue = new TimeIntervalSetting( expectedIntervalValue, expectedIntervalUnit );

            Assert.That.AreEqual( expectedIntervalValue, actualValue.IntervalValue );
            Assert.That.AreEqual( expectedIntervalUnit, actualValue.IntervalUnit );
        }

        [TestMethod]
        public void TimeIntervalSettingShouldDefaultTo59MinutesIfPassedHigherValue()
        {
            var expectedIntervalValue = 59;
            var expectedIntervalUnit = IntervalTimeUnit.Minute;

            var actualValue = new TimeIntervalSetting( 60, expectedIntervalUnit );

            Assert.That.AreEqual( expectedIntervalValue, actualValue.IntervalValue );
            Assert.That.AreEqual( expectedIntervalUnit, actualValue.IntervalUnit );
        }

        [TestMethod]
        public void TimeIntervalSettingShouldDefaultToDefaultValueIfNullPassedIn()
        {
            var expectedIntervalValue = 15;
            var expectedIntervalUnit = IntervalTimeUnit.Minute;

            var actualValue = new TimeIntervalSetting( null, expectedIntervalUnit, 15 );

            Assert.That.AreEqual( expectedIntervalValue, actualValue.IntervalValue );
            Assert.That.AreEqual( expectedIntervalUnit, actualValue.IntervalUnit );
        }

        [TestMethod]
        public void TimeIntervalSettingShouldDefaultTo23HoursIfPassedHigherValue()
        {
            var expectedIntervalValue = 23;
            var expectedIntervalUnit = IntervalTimeUnit.Hour;

            var actualValue = new TimeIntervalSetting( 24, expectedIntervalUnit );

            Assert.That.AreEqual( expectedIntervalValue, actualValue.IntervalValue );
            Assert.That.AreEqual( expectedIntervalUnit, actualValue.IntervalUnit );
        }

        [TestMethod]
        public void TimeIntervalSettingShouldDefaultTo31DaysIfPassedHigherValue()
        {
            var expectedIntervalValue = 31;
            var expectedIntervalUnit = IntervalTimeUnit.Day;

            var actualValue = new TimeIntervalSetting( 32, expectedIntervalUnit );

            Assert.That.AreEqual( expectedIntervalValue, actualValue.IntervalValue );
            Assert.That.AreEqual( expectedIntervalUnit, actualValue.IntervalUnit );
        }

        [TestMethod]
        public void TimeIntervalSettingShouldCorrectlyDetermineIntervalUnit()
        {
            var testValues = new Dictionary<int, IntervalTimeUnit>
            {
                {1, IntervalTimeUnit.Minute },
                {59, IntervalTimeUnit.Minute },
                {60, IntervalTimeUnit.Hour },
                {1439, IntervalTimeUnit.Hour },
                {1440, IntervalTimeUnit.Day },
            };

            foreach(var key in testValues.Keys )
            {
                var expectedIntervalUnit = testValues[key];

                var actualValue = new TimeIntervalSetting( key, null );

                Assert.That.AreEqual( expectedIntervalUnit, actualValue.IntervalUnit );
            }
        }

        [TestMethod]
        public void TimeIntervalSettingShouldCorrectlyDetermineIntervalValue()
        {
            var testValues = new Dictionary<int, int>
            {
                {1, 1 },
                {59, 59 },
                {60, 1 },
                {1439, 23 },
                {1440, 1 },
            };

            foreach ( var key in testValues.Keys )
            {
                var expectedIntervalValue = testValues[key];

                var actualValue = new TimeIntervalSetting( key, null );

                Assert.That.AreEqual( expectedIntervalValue, actualValue.IntervalValue );
            }
        }

        [TestMethod]
        public void TimeIntervalSettingShouldCorrectlyDetermineIntervalMinutes()
        {
            var testValues = new Dictionary<int, int>
            {
                {1, 1 },
                {59, 59 },
                {60, 60 },
                {1439, 1380 },
                {1440, 1440 },
                {2980, 2880 },
            };

            foreach ( var key in testValues.Keys )
            {
                var expectedIntervalMinutes = testValues[key];

                var actualValue = new TimeIntervalSetting( key, null );

                Assert.That.AreEqual( expectedIntervalMinutes, actualValue.IntervalMinutes );
            }
        }

        [TestMethod]
        public void TimeIntervalSettingShouldHaveCorrectToStringValues()
        {
            var testValues = new Dictionary<int, string>
            {
                {0, "" },
                {1, "1 minute" },
                {59, "59 minutes" },
                {60, "1 hour" },
                {1439, "23 hours" },
                {1440, "1 day" },
                {2880, "2 days" },
            };

            foreach ( var key in testValues.Keys )
            {
                var expectedIntervalValue = testValues[key];

                var actualValue = new TimeIntervalSetting( key, null );

                Assert.That.AreEqual( expectedIntervalValue, actualValue.ToString() );
            }
        }

        [TestMethod]
        public void TimeIntervalSettingShouldCorrectlyDetermineMaxValue()
        {
            var testValues = new Dictionary<int, int>
            {
                {1, 59 },
                {59, 59 },
                {60, 23 },
                {1439, 23 },
                {1440, 31 },
            };

            foreach ( var key in testValues.Keys )
            {
                var expectedMaxValue = testValues[key];

                var actualValue = new TimeIntervalSetting( key, null );

                Assert.That.AreEqual( expectedMaxValue, actualValue.MaxValue );
            }
        }

        [TestMethod]
        public void TimeIntervalSettingIntervalValueShouldUpdateCorrectly()
        {
            var testValues = new Dictionary<int, int>
            {
                {1, 1 * 60 },
                {15, 15 * 60 },
                {59, 23 * 60 },
            };

            var actualValue = new TimeIntervalSetting( null, null );

            foreach ( var key in testValues.Keys )
            {
                var expectedIntervalMinutes = testValues[key];

                actualValue.IntervalValue = key;    

                Assert.That.AreEqual( expectedIntervalMinutes, actualValue.IntervalMinutes );
            }
        }

        [TestMethod]
        public void TimeIntervalSettingIntervalUnitShouldUpdateCorrectly()
        {
            var testValues = new Dictionary<IntervalTimeUnit, int>
            {
                {IntervalTimeUnit.Minute, 12 },
                {IntervalTimeUnit.Hour, 12 * 60 },
                {IntervalTimeUnit.Day, 12* 1440 },
            };

            var actualValue = new TimeIntervalSetting( null, null );

            foreach ( var key in testValues.Keys )
            {
                var expectedIntervalMinutes = testValues[key];

                actualValue.IntervalUnit = key;

                Assert.That.AreEqual( expectedIntervalMinutes, actualValue.IntervalMinutes );
            }
        }
    }
}
