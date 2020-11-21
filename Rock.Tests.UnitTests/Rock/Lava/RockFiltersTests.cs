using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;

using Ical.Net;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Lava
{
    [TestClass]
    public class RockFiltersTest
    {
        // A fake web-root Content folder for any tests that use the HTTP Context simulator
        private static string webContentFolder = string.Empty;

        private static readonly Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
        private static CalendarSerializer serializer = new CalendarSerializer();
        private static RecurrencePattern weeklyRecurrence = new RecurrencePattern( "RRULE:FREQ=WEEKLY;BYDAY=SA" );
        private static RecurrencePattern monthlyRecurrence = new RecurrencePattern( "RRULE:FREQ=MONTHLY;BYDAY=1SA" );

        private static readonly DateTime today = RockDateTime.Today;

        private static readonly Calendar weeklySaturday430 = new Calendar()
        {
            Events =
            {
                new Event
                    {
                        DtStart = new CalDateTime( today.Year, today.Month, today.Day + DayOfWeek.Saturday - today.DayOfWeek, 16, 30, 0 ),
                        DtEnd = new CalDateTime( today.Year, today.Month, today.Day + DayOfWeek.Saturday - today.DayOfWeek, 17, 30, 0 ),
                        DtStamp = new CalDateTime( today.Year, today.Month, today.Day ),
                        RecurrenceRules = new List<IRecurrencePattern> { weeklyRecurrence },
                        Sequence = 0,
                        Uid = @"d74561ac-c0f9-4dce-a610-c39ca14b0d6e"
                    }
                }
        };

        private static readonly Calendar monthlyFirstSaturday = new Calendar()
        {
            Events =
            {
                new Event
                    {
                        DtStart = new CalDateTime( today.Year, today.Month, today.Day, 8, 0, 0 ),
                        DtEnd = new CalDateTime( today.Year, today.Month, today.Day, 10, 0, 0 ),
                        DtStamp = new CalDateTime( today.Year, today.Month, today.Day ),
                        RecurrenceRules = new List<IRecurrencePattern> { monthlyRecurrence },
                        Sequence = 0,
                        Uid = @"517d77dd-6fe8-493b-925f-f266aa2d852c"
                    }
                }
        };

        private static readonly string iCalStringSaturday430 = serializer.SerializeToString( weeklySaturday430 );
        private static readonly string iCalStringFirstSaturdayOfMonth = serializer.SerializeToString( monthlyFirstSaturday );

        #region Text Filters

        /// <summary>
        /// For use in Lava -- should match the pattern in the string.
        /// </summary>
        [TestMethod]
        public void Text_RegExMatch_ShouldMatchSimpleString()
        {
            var output = RockFilters.RegExMatch( "Group 12345 has 5 members", @"\d\d\d\d\d" );
            Assert.That.IsTrue( output );

            output = RockFilters.RegExMatch( "Group Decker has 5 members", @"\d\d\d\d\d" );
            Assert.That.IsFalse( output );
        }

        /// <summary>
        /// For use in Lava -- should match a valid email address pattern in the string.
        /// </summary>
        [TestMethod]
        public void Text_RegExMatch_ShouldMatchValidEmailString()
        {
            var regexEmail = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            var output = RockFilters.RegExMatch( "ted@rocksolidchurchdemo.com", regexEmail );
            Assert.That.IsTrue( output );

            output = RockFilters.RegExMatch( "ted@rocksolidchurchdemo. com", regexEmail );
            Assert.That.IsFalse( output );

            output = RockFilters.RegExMatch( "ted(AT)rocksolidchurchdemo.com", regexEmail );
            Assert.That.IsFalse( output );
        }

        /// <summary>
        /// For use in Lava -- should return the first matching pattern in the string.
        /// </summary>
        [TestMethod]
        public void Text_RegExMatchValue_ShouldReturnMatchValue()
        {
            var output = RockFilters.RegExMatchValue( "Group 12345 has 54321 members", @"\d+" );
            Assert.That.AreEqual( "12345", output );
        }

        /// <summary>
        /// For use in Lava -- should not match and should return nothing.
        /// </summary>
        [TestMethod]
        public void Text_RegExMatchValue_ShouldNotMatchValue()
        {
            var output = RockFilters.RegExMatchValue( "Group Decker has no members", @"\d+" );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should return all matching patterns in the string.
        /// </summary>
        [TestMethod]
        public void Text_RegExMatchValues_ShouldReturnMatchValues()
        {
            var output = RockFilters.RegExMatchValues( "Group 12345 has 54321 members", @"\d+" );
            Assert.That.AreEqual( new List<string> { "12345", "54321" }, output );
        }

        /// <summary>
        /// For use in Lava -- should not match and should return nothing.
        /// </summary>
        [TestMethod]
        public void Text_RegExMatchValues_ShouldNotMatchValues()
        {
            var output = RockFilters.RegExMatchValues( "Group Decker has no members", @"\d+" );
            Assert.That.AreEqual( new List<string>(), output );
        }

        /// <summary>
        /// Verfies that the StripHtml filter handles standard HTML tags.
        /// </summary>
        [TestMethod]
        public void StripHtml_ShouldStripStandardTags()
        {
            var html = "<p>Lorem <a href=\"#\">ipsum</a> <b>dolor</b> sit amet, <strong>consectetur</strong> adipiscing <t>elit</t>.</p>";
            var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

            Assert.That.AreEqual( text, DotLiquid.StandardFilters.StripHtml( html ) );
        }

        /// <summary>
        /// Verifies that the StripHtml filter handles multi-line HTML comments.
        /// </summary>
        [TestMethod]
        public void StripHtml_ShouldStripHtmlComments()
        {
            var html = @"<p>Lorem ipsum <!-- this is
a comment --> sit amet</p>";
            var text = @"Lorem ipsum  sit amet";

            Assert.That.AreEqual( text, StandardFilters.StripHtml( html ) );
        }

        #endregion

        #region Numeric Filters

        #region Minus

        /// <summary>
        /// For use in Lava -- should subtract two integers and return an integer.
        /// </summary>
        [TestMethod]
        public void MinusTwoInts()
        {
            // I'd like to test via Lava Resolve/MergeFields but can't get that to work.
            //string lava = "{{ 3 | Minus: 2 | ToJSON }}";
            //var person = new Person();
            //var o = lava.ResolveMergeFields( mergeObjects, person, "" );
            //Assert.That.AreEqual( "1", o);
            var output = RockFilters.Minus( 3, 2 );
            Assert.That.AreEqual( 1, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two decimals and return a decimal.
        /// </summary>
        [TestMethod]
        public void MinusTwoDecimals()
        {
            var output = RockFilters.Minus( 3.0M, 2.0M );
            Assert.That.AreEqual( 1.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two strings (containing integers) and return an int.
        /// </summary>
        [TestMethod]
        public void MinusTwoStringInts()
        {
            var output = RockFilters.Minus( "3", "2" );
            Assert.That.AreEqual( 1, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two strings (containing decimals) and return a decimal.
        /// </summary>
        [TestMethod]
        public void MinusTwoStringDecimals()
        {
            var output = RockFilters.Minus( "3.0", "2.0" );
            Assert.That.AreEqual( 1.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [TestMethod]
        public void MinusIntAndDecimal()
        {
            var output = RockFilters.Minus( 3, "2.0" );
            Assert.That.AreEqual( 1.0M, output );
        }

        #endregion

        #region Plus

        /// <summary>
        /// For use in Lava -- should add two integers and return an integer.
        /// </summary>
        [TestMethod]
        public void PlusTwoInts()
        {
            var output = RockFilters.Plus( 3, 2 );
            Assert.That.AreEqual( 5, output );
        }

        /// <summary>
        /// For use in Lava -- should add two decimals and return a decimal.
        /// </summary>
        [TestMethod]
        public void PlusTwoDecimals()
        {
            var output = RockFilters.Plus( 3.0M, 2.0M );
            Assert.That.AreEqual( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should add two strings (containing integers) and return an int.
        /// </summary>
        [TestMethod]
        public void PlusTwoStringInts()
        {
            var output = RockFilters.Plus( "3", "2" );
            Assert.That.AreEqual( 5, output );
        }

        /// <summary>
        /// For use in Lava -- should add two strings (containing decimals) and return a decimal.
        /// </summary>
        [TestMethod]
        public void PlusTwoStringDecimals()
        {
            var output = RockFilters.Plus( "3.0", "2.0" );
            Assert.That.AreEqual( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should add an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [TestMethod]
        public void PlusIntAndDecimal()
        {
            var output = RockFilters.Plus( 3, "2.0" );
            Assert.That.AreEqual( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should concatenate two strings.
        /// </summary>
        [TestMethod]
        public void PlusStrings()
        {
            var output = RockFilters.Plus( "Food", "Bar" );
            Assert.That.AreEqual( "FoodBar", output );
        }

        #endregion

        #region Times

        /// <summary>
        /// For use in Lava -- should multiply two integers and return an integer.
        /// </summary>
        [TestMethod]
        public void TimesTwoInts()
        {
            var output = RockFilters.Times( 3, 2 );
            Assert.That.AreEqual( 6, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two decimals and return a decimal.
        /// </summary>
        [TestMethod]
        public void TimesTwoDecimals()
        {
            var output = RockFilters.Times( 3.0M, 2.0M );
            Assert.That.AreEqual( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two strings (containing integers) and return an int.
        /// </summary>
        [TestMethod]
        public void TimesTwoStringInts()
        {
            var output = RockFilters.Times( "3", "2" );
            Assert.That.AreEqual( 6, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two strings (containing decimals) and return a decimal.
        /// </summary>
        [TestMethod]
        public void TimesTwoStringDecimals()
        {
            var output = RockFilters.Times( "3.0", "2.0" );
            Assert.That.AreEqual( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [TestMethod]
        public void TimesIntAndDecimal()
        {
            var output = RockFilters.Times( 3, "2.0" );
            Assert.That.AreEqual( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should repeat the string (containing a decimal) and return a decimal.
        /// </summary>
        [TestMethod]
        public void TimesStringAndInt()
        {
            var expectedOutput = Enumerable.Repeat( "Food", 2 ).ToList();
            var output = RockFilters.Times( "Food", 2 ) as IEnumerable<string>;

            Assert.That.AreEqual( expectedOutput, output );
        }

        #endregion

        #endregion

        #region "Other" category filters

        #region AsInteger

        /// <summary>
        /// For use in Lava -- should not cast the null to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_Null()
        {
            var output = RockFilters.AsInteger( null );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the true boolean to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_InvalidBoolean()
        {
            var output = RockFilters.AsInteger( true );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_ValidInteger()
        {
            var output = RockFilters.AsInteger( 3 );
            Assert.That.AreEqual( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_ValidDecimal()
        {
            var output = RockFilters.AsInteger( ( decimal ) 3.0d );
            Assert.That.AreEqual( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_ValidDouble()
        {
            var output = RockFilters.AsInteger( 3.0d );
            Assert.That.AreEqual( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_ValidString()
        {
            var output = RockFilters.AsInteger( "3" );
            Assert.That.AreEqual( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_InvalidString()
        {
            var output = RockFilters.AsInteger( "a" );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal string to an integer.
        /// </summary>
        [TestMethod]
        public void AsInteger_ValidDecimalString()
        {
            var output = RockFilters.AsInteger( "3.2" );
            Assert.That.AreEqual( 3, output );
        }

        #endregion

        #region AsDecimal

        /// <summary>
        /// For use in Lava -- should not cast the null to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_Null()
        {
            var output = RockFilters.AsDecimal( null );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the true boolean to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_InvalidBoolean()
        {
            var output = RockFilters.AsDecimal( true );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_ValidInteger()
        {
            var output = RockFilters.AsDecimal( 3 );
            Assert.That.AreEqual( output, ( decimal ) 3.0d );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_ValidDecimal()
        {
            var output = RockFilters.AsDecimal( ( decimal ) 3.2d );
            Assert.That.AreEqual( output, ( decimal ) 3.2d );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_ValidDouble()
        {
            var output = RockFilters.AsDecimal( 3.141592d );
            Assert.That.AreEqual( output, ( decimal ) 3.141592d );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_ValidString()
        {
            var output = RockFilters.AsDecimal( "3.14" );
            Assert.That.AreEqual( output, ( decimal ) 3.14d );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_InvalidString()
        {
            var output = RockFilters.AsDecimal( "a" );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal string to a decimal.
        /// </summary>
        [TestMethod]
        public void AsDecimal_InvalidDecimalString()
        {
            var output = RockFilters.AsInteger( "3.0.2" );
            Assert.That.IsNull( output );
        }

        #endregion

        #region AsDouble

        /// <summary>
        /// For use in Lava -- should not cast the null to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_Null()
        {
            var output = RockFilters.AsDouble( null );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the true boolean to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidBoolean()
        {
            var output = RockFilters.AsDouble( true );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidInteger()
        {
            var output = RockFilters.AsDouble( 3 );
            Assert.That.AreEqual( 3.0d, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidDecimal()
        {
            var output = RockFilters.AsDouble( ( decimal ) 3.2d );
            Assert.That.AreEqual( ( double ) 3.2d, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidDouble()
        {
            var output = RockFilters.AsDouble( 3.141592d );
            Assert.That.AreEqual( ( double ) 3.141592d, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidString()
        {
            var output = RockFilters.AsDouble( "3.14" );
            Assert.That.AreEqual( ( double ) 3.14d, output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidString()
        {
            var output = RockFilters.AsDouble( "a" );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidDecimalString()
        {
            var output = RockFilters.AsDouble( "3.0.2" );
            Assert.That.IsNull( output );
        }

        #endregion

        #region AsString

        /// <summary>
        /// For use in Lava -- should not cast the null to a string.
        /// </summary>
        [TestMethod]
        public void AsString_Null()
        {
            var output = RockFilters.AsString( null );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the false boolean to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidFalseBoolean()
        {
            var output = RockFilters.AsString( false );
            Assert.That.AreEqual( "False", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the true boolean to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidTrueBoolean()
        {
            var output = RockFilters.AsString( true );
            Assert.That.AreEqual( "True", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidInteger()
        {
            var output = RockFilters.AsString( 3 );
            Assert.That.AreEqual( "3", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidDecimal()
        {
            var output = RockFilters.AsString( ( decimal ) 3.2d );
            Assert.That.AreEqual( "3.2", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidDouble()
        {
            var output = RockFilters.AsString( 3.141592d );
            Assert.That.AreEqual( "3.141592", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidDoubleString()
        {
            var output = RockFilters.AsString( "3.14" );
            Assert.That.AreEqual( "3.14", output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a string.
        /// </summary>
        [TestMethod]
        public void AsString_ValidString()
        {
            var output = RockFilters.AsString( "abc" );
            Assert.That.AreEqual( "abc", output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the datetime to a string.
        /// </summary>
        [TestMethod]
        public void AsString_DateTime()
        {
            DateTime dt = new DateTime( 2017, 3, 7, 15, 4, 33 );
            var output = RockFilters.AsString( dt );
            Assert.That.AreEqual( output, dt.ToString() );
        }

        #endregion

        #region AsDateTime

        /// <summary>
        /// For use in Lava -- should not cast the null to an datetime.
        /// </summary>
        [TestMethod]
        public void AsDateTime_Null()
        {
            var output = RockFilters.AsDateTime( null );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to an datetime.
        /// </summary>
        [TestMethod]
        public void AsDateTime_InvalidString()
        {
            var output = RockFilters.AsDateTime( "1/1/1 50:00" );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to an datetime.
        /// </summary>
        [TestMethod]
        public void AsDateTime_ValidString()
        {
            DateTime dt = new DateTime( 2017, 3, 7, 15, 4, 33 );
            var output = RockFilters.AsDateTime( dt.ToString() );
            Assert.That.AreEqual( dt, output );
        }

        #endregion

        /// <summary>
        /// For use in Lava -- should return the IP address of the Client
        /// </summary>
        [TestMethod]
        public void Client_IP()
        {
            InitWebContentFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var output = RockFilters.Client( "Global", "ip" );
                Assert.That.AreEqual( "127.0.0.1", output );
            }
        }

        /// <summary>
        /// For use in Lava -- should return the IP address of the Client using the "x-forwarded-for" header value
        /// </summary>
        [TestMethod]
        public void Client_IP_ForwardedFor()
        {
            InitWebContentFolder();

            NameValueCollection headers = new NameValueCollection();
            headers.Add( "x-forwarded-for", "77.7.7.77" );

            using ( var simulator = new HttpSimulator( "/", webContentFolder ) )
            {
                simulator.SimulateRequest( new Uri( "http://localhost/" ), new NameValueCollection(), headers );

                var output = RockFilters.Client( "Global", "ip" );
                Assert.That.AreEqual( "77.7.7.77", output );
            }
        }

        /// <summary>
        /// For use in Lava -- should return the user agent of the client (which is setup in the fake/mock HttpSimulator)
        /// </summary>
        [TestMethod]
        public void Client_Browser()
        {
            InitWebContentFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                dynamic output = RockFilters.Client( "Global", "browser" );
                Assert.That.AreEqual( "Chrome", output.UserAgent.Family as string );
                Assert.That.AreEqual( "68", output.UserAgent.Major as string );
                Assert.That.AreEqual( "Windows", output.OS.Family as string );
            }
        }

        #endregion

        #region Index

        /// <summary>
        /// For use in Lava -- should extract a single element from the array.
        /// </summary>
        [TestMethod]
        public void Index_ArrayAndInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, 1 );
            Assert.That.AreEqual( "value2", output );
        }

        /// <summary>
        /// For use in Lava -- should extract a single element from the array.
        /// </summary>
        [TestMethod]
        public void Index_ArrayAndString()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, "1" );
            Assert.That.AreEqual( "value2", output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [TestMethod]
        public void Index_ArrayAndInvalidString()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, "a" );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [TestMethod]
        public void Index_ArrayAndNegativeInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, -1 );
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [TestMethod]
        public void Index_ArrayAndHugeInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, int.MaxValue );
            Assert.That.IsNull( output );
        }

        #endregion

        #region Sort

        /// <summary>
        /// For use in Lava -- sort objects (from JSON) by an int property
        /// </summary>
        [TestMethod]
        public void Sort_FromJson_Int()
        {
            var expected = new List<string>() { "A", "B", "C", "D" };

            var json = @"[{
		""Title"": ""D"",
        ""Order"": 4
    }, {
		""Title"": ""A"",
		""Order"": 1
    }, {
		""Title"": ""C"",
		""Order"": 3
    }, {
		""Title"": ""B"",
		""Order"": 2
    }]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) StandardFilters.Sort( input, "Order" );
            var sortedTitles = output.Cast<dynamic>().Select( x => x.Title );
            Assert.That.AreEqual( expected, sortedTitles );
        }

        /// <summary>
        /// For use in Lava -- sort from JSON. NOTE: Dates really should be in ISO 8601 for guaranteed sort-ability.
        /// </summary>
        [TestMethod]
        public void Sort_FromJson()
        {
            var json = @"[{
		""Title"": ""Hallelujah!( 6 / 12 / 16 )"",
        ""StartDateTime"": ""2016-06-12T12:00:00""
    }, {
		""Title"": ""Are You Dealing With Insecurity? (6/19/16)"",
		""StartDateTime"": ""2016-06-19T12:00:00""
    }, {
		""Title"": ""Woman's Infirmity Healed (6/5/16)"",
		""StartDateTime"": ""2016-06-05T12:00:00""
    }, {
		""Title"": ""Test new sermon (5/29/16)"",
		""StartDateTime"": ""2016-05-29T12:00:00""
    }, {
		""Title"": ""Test new sermon (7/3/16)"",
		""StartDateTime"": ""2016-07-03T12:00:00""
    }]";
            var converter = new ExpandoObjectConverter();
            object input = null;
            input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) StandardFilters.Sort( input, "StartDateTime" );
            Assert.That.AreEqual( "Test new sermon (5/29/16)", ( ( IDictionary<string, object> ) output.First() )["Title"] );
        }

        /// <summary>
        /// For use in Lava -- sort from JSON. NOTE: Dates must be in ISO 8601 for guaranteed sort-ability.
        /// </summary>
        [TestMethod]
        public void Sort_FromJsonDesc()
        {
            var json = @"[{
		""Title"": ""Hallelujah!(6/12/16 )"",
        ""StartDateTime"": ""2016-06-12T12:00:00""
    }, {
		""Title"": ""Are You Dealing With Insecurity? (6/19/16)"",
		""StartDateTime"": ""2016-06-19T12:00:00""
    }, {
		""Title"": ""Woman's Infirmity Healed (6/5/16)"",
		""StartDateTime"": ""2016-06-05T12:00:00""
    }, {
		""Title"": ""Test new sermon (5/29/16)"",
		""StartDateTime"": ""2016-05-29T12:00:00""
    }, {
		""Title"": ""Test new sermon (7/3/16)"",
		""StartDateTime"": ""2016-07-03T12:00:00""
    }]";
            var converter = new ExpandoObjectConverter();
            object input = null;
            input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) StandardFilters.Sort( input, "StartDateTime", "desc" );
            Assert.That.AreEqual( "Test new sermon (7/3/16)", ( ( IDictionary<string, object> ) output.First() )["Title"] );
        }

        /// <summary>
        /// For use in Lava -- sort by DateTime
        /// </summary>
        [TestMethod]
        public void Sort_DateTime()
        {
            var input = new List<DateTime>
            {
                new DateTime().AddDays(1),
                new DateTime()
            };
            var output = ( List<object> ) StandardFilters.Sort( input, null );
            Assert.That.AreEqual( new DateTime(), output[0] );
        }

        /// <summary>
        /// For use in Lava -- sort by DateTime desc
        /// </summary>
        [TestMethod]
        public void Sort_DateTimeDesc()
        {
            var input = new List<DateTime>
            {
                new DateTime(),
                new DateTime().AddDays(1),
            };
            var output = ( List<object> ) StandardFilters.Sort( input, null, "desc" );
            Assert.That.AreEqual( new DateTime().AddDays( 1 ), output[0] );
        }

        /// <summary>
        /// For use in Lava -- sort by int
        /// </summary>
        [TestMethod]
        public void Sort_Int()
        {
            var input = new List<int> { 2, 1 };
            var output = ( List<object> ) StandardFilters.Sort( input, null );
            Assert.That.AreEqual( 1, output[0] );
        }

        /// <summary>
        /// For use in Lava -- sort by int
        /// </summary>
        [TestMethod]
        public void Sort_IntDesc()
        {
            var input = new List<int> { 1, 2 };
            var output = ( List<object> ) StandardFilters.Sort( input, null, "desc" );
            Assert.That.AreEqual( 2, output[0] );
        }


        /// <summary>
        /// For use in Lava -- sort by string
        /// </summary>
        [TestMethod]
        public void Sort_StringDesc()
        {
            var input = new List<string> { "A", "B" };
            var output = ( List<object> ) StandardFilters.Sort( input, null, "desc" );
            Assert.That.AreEqual( "B", output[0] );
        }

        /// <summary>
        /// For use in Lava -- sort by string
        /// </summary>
        [TestMethod]
        public void Sort_String()
        {
            var input = new List<string> { "B", "A" };
            var output = ( List<object> ) StandardFilters.Sort( input, null );
            Assert.That.AreEqual( "A", output[0] );
        }

        /// <summary>
        /// For use in Lava -- sort arbitrary by date
        /// </summary>
        [TestMethod]
        public void Sort_ArbitraryDateTime()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", new DateTime().AddDays(1) }, { "Value", "2" } },
               new Dictionary<string, object> { { "Id", new DateTime() }, { "Value", "1" } }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id" );
            Assert.That.AreEqual( "1", ( ( Dictionary<string, object> ) output[0] )["Value"] );
        }

        /// <summary>
        /// For use in Lava -- sort arbitrary by date desc
        /// </summary>
        [TestMethod]
        public void Sort_ArbitraryDateTimeDesc()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", new DateTime() }, { "Value", "1" } },
               new Dictionary<string, object> { { "Id", new DateTime().AddDays(1) }, { "Value", "2" } }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id", "desc" );
            Assert.That.AreEqual( "2", ( ( Dictionary<string, object> ) output[0] )["Value"] );
        }

        /// <summary>
        /// For use in Lava -- sort arbitrary by int
        /// </summary>
        [TestMethod]
        public void Sort_ArbitraryInt()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", 2 }, { "Value", "2" } },
               new Dictionary<string, object> { { "Id", 1 }, { "Value", "1" } }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id" );
            Assert.That.AreEqual( "1", ( ( Dictionary<string, object> ) output[0] )["Value"] );
        }

        /// <summary>
        /// For use in Lava -- sort arbitrary by int desc
        /// </summary>
        [TestMethod]
        public void Sort_ArbitraryIntDesc()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", 1 }, { "Value", "1" } },
               new Dictionary<string, object> { { "Id", 2 }, { "Value", "2" } }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id", "desc" );
            Assert.That.AreEqual( "2", ( ( Dictionary<string, object> ) output[0] )["Value"] );
        }

        /// <summary>
        /// For use in Lava -- sort arbitrary by string
        /// </summary>
        [TestMethod]
        public void Sort_ArbitraryString()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", "B"}, { "Value", "2" } },
               new Dictionary<string, object> { { "Id", "A" }, { "Value", "1" } }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id" );
            Assert.That.AreEqual( "1", ( ( Dictionary<string, object> ) output[0] )["Value"] );
        }

        /// <summary>
        /// For use in Lava -- sort arbitrary by string desc
        /// </summary>
        [TestMethod]
        public void Sort_ArbitraryStringDesc()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", "A" }, { "Value", "1" } },
               new Dictionary<string, object> { { "Id", "B"}, { "Value", "2" } },
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id", "desc" );
            Assert.That.AreEqual( "2", ( ( Dictionary<string, object> ) output[0] )["Value"] );
        }

        /// <summary>
        /// For use in Lava -- sort ILiquidizable by date
        /// </summary>
        [TestMethod]
        public void Sort_ILiquidizableDateTime()
        {
            var input = new List<object>
            {
               new Person(){ AnniversaryDate = new DateTime().AddDays(1), NickName="2" },
                new Person(){ AnniversaryDate = new DateTime(), NickName="1" }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "AnniversaryDate" );
            Assert.That.AreEqual( "1", ( ( Person ) output[0] ).NickName );
        }

        /// <summary>
        /// For use in Lava -- sort ILiquidizable by date desc
        /// </summary>
        [TestMethod]
        public void Sort_ILiquidizableDateTimeDesc()
        {
            var input = new List<object>
            {
                new Person(){ AnniversaryDate = new DateTime(), NickName="1" },
               new Person(){ AnniversaryDate = new DateTime().AddDays(1), NickName="2" }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "AnniversaryDate", "desc" );
            Assert.That.AreEqual( "2", ( ( Person ) output[0] ).NickName );
        }

        /// <summary>
        /// For use in Lava -- sort ILiquidizable by int
        /// </summary>
        [TestMethod]
        public void Sort_ILiquidizableInt()
        {
            var input = new List<object>
            {
               new Person(){ Id = 2, NickName="2" },
                new Person(){ Id = 1, NickName="1" }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id" );
            Assert.That.AreEqual( "1", ( ( Person ) output[0] ).NickName );
        }


        /// <summary>
        /// For use in Lava -- sort ILiquidizable by int desc
        /// </summary>
        [TestMethod]
        public void Sort_ILiquidizableIntDesc()
        {
            var input = new List<object>
            {
                new Person(){ Id = 1, NickName="1" },
               new Person(){ Id = 2, NickName="2" }
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "Id", "desc" );
            Assert.That.AreEqual( "2", ( ( Person ) output[0] ).NickName );
        }

        /// <summary>
        /// For use in Lava -- sort ILiquidizable by string
        /// </summary>
        [TestMethod]
        public void Sort_ILiquidizableString()
        {
            var input = new List<object>
            {
               new Person(){ NickName="2" },
                new Person(){ NickName="1" },
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "NickName" );
            Assert.That.AreEqual( "1", ( ( Person ) output[0] ).NickName );
        }

        /// <summary>
        /// For use in Lava -- sort ILiquidizable by string desc
        /// </summary>
        [TestMethod]
        public void Sort_ILiquidizableStringDesc()
        {
            var input = new List<object>
            {
               new Person(){ NickName="1" },
                new Person(){ NickName="2" },
            };
            var output = ( List<object> ) StandardFilters.Sort( input, "NickName", "desc" );
            Assert.That.AreEqual( "2", ( ( Person ) output[0] ).NickName );
        }

        #region Where

        /// <summary>
        /// For use in Lava -- should extract a single element form array
        /// </summary>
        [TestMethod]
        public void Where_ByInt()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", (int)1 } },
               new Dictionary<string, object> { { "Id", (int)2 } }
            };
            var output = RockFilters.Where( input, "Id", 1 );
            Assert.That.IsTrue( ( ( List<object> ) output ).Count() == 1 );
        }

        /// <summary>
        /// For use in Lava -- should extract a single element form array. Simulates a | FromJSON input.
        /// </summary>
        [TestMethod]
        public void Where_ByLong()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", (long)1 } },
               new Dictionary<string, object> { { "Id", (long)2 } }
            };
            var output = RockFilters.Where( input, "Id", ( int ) 1 );
            Assert.That.IsTrue( ( ( List<object> ) output ).Count == 1 );
        }

        /// <summary>
        /// For use in Lava -- should extract a single element form array
        /// </summary>
        [TestMethod]
        public void Where_ByString()
        {
            var input = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", "1" } },
               new Dictionary<string, object> { { "Id", "2" } }
            };

            var output = RockFilters.Where( input, "Id", "1" );
            Assert.That.IsTrue( ( ( List<object> ) output ).Count == 1 );
        }

        #endregion

        #endregion

        #region OrderBy

        /// <summary>
        /// For use in Lava -- sort objects (from JSON) by a single int property
        /// using the default ordering (ascending).
        /// </summary>
        [TestMethod]
        public void OrderBy_FromJson_Int()
        {
            var expected = new List<string>() { "A", "B", "C", "D" };

            var json = @"[
    {""Title"": ""D"", ""Order"": 4},
    {""Title"": ""A"", ""Order"": 1},
    {""Title"": ""C"", ""Order"": 3},
    {""Title"": ""B"", ""Order"": 2}
]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) RockFilters.OrderBy( input, "Order" );
            var sortedTitles = output.Cast<dynamic>().Select( x => x.Title ).Cast<string>().ToList();

            CollectionAssert.AreEquivalent( expected, sortedTitles );
        }

        /// <summary>
        /// For use in Lava -- sort objects (from JSON) by a single int property
        /// using the explicit ordering descending.
        /// </summary>
        [TestMethod]
        public void OrderBy_FromJson_IntDescending()
        {
            var expected = new List<string>() { "D", "C", "B", "A" };

            var json = @"[
    { ""Title"": ""D"", ""Order"": 4 },
    { ""Title"": ""A"", ""Order"": 1 },
    { ""Title"": ""C"", ""Order"": 3 },
    { ""Title"": ""B"", ""Order"": 2 }
]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) RockFilters.OrderBy( input, "Order desc" );
            var sortedTitles = output.Cast<dynamic>().Select( x => x.Title ).Cast<string>().ToList();

            CollectionAssert.AreEquivalent( expected, sortedTitles );
        }

        /// <summary>
        /// For use in Lava -- sort objects (from JSON) by a two int properties
        /// using the ascending on the first and descending on the second.
        /// </summary>
        [TestMethod]
        public void OrderBy_FromJson_IntInt()
        {
            var expected = new List<string>() { "A", "B", "C", "D" };

            var json = @"[
    { ""Title"": ""D"", ""Order"": 2, ""SecondOrder"": 1 },
    { ""Title"": ""A"", ""Order"": 1, ""SecondOrder"": 2 },
    { ""Title"": ""C"", ""Order"": 2, ""SecondOrder"": 2 },
    { ""Title"": ""B"", ""Order"": 1, ""SecondOrder"": 1 }
]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) RockFilters.OrderBy( input, "Order,SecondOrder desc" );
            var sortedTitles = output.Cast<dynamic>().Select( x => x.Title ).Cast<string>().ToList();

            CollectionAssert.AreEquivalent( expected, sortedTitles );
        }

        /// <summary>
        /// For use in Lava -- sort objects (from JSON) by a two int properties
        /// using the ascending on the first and descending on the second.
        /// </summary>
        [TestMethod]
        public void OrderBy_FromJson_IntNestedInt()
        {
            var expected = new List<string>() { "A", "B", "C", "D" };

            var json = @"[
    { ""Title"": ""D"", ""Order"": 2, ""Nested"": { ""Order"": 1 } },
    { ""Title"": ""A"", ""Order"": 1, ""Nested"": { ""Order"": 2 } },
    { ""Title"": ""C"", ""Order"": 2, ""Nested"": { ""Order"": 2 } },
    { ""Title"": ""B"", ""Order"": 1, ""Nested"": { ""Order"": 1 } }
]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );
            var output = ( List<object> ) RockFilters.OrderBy( input, "Order, Nested.Order desc" );
            var sortedTitles = output.Cast<dynamic>().Select( x => x.Title ).Cast<string>().ToList();

            CollectionAssert.AreEquivalent( expected, sortedTitles );
        }

        /// <summary>
        /// For use in Lava -- sort collection of group members by person name.
        /// </summary>
        [TestMethod]
        public void OrderBy_FromObject_GroupMemberPersonName()
        {
            var expected = new List<int>() { 1, 2, 3, 4 };

            var members = new List<GroupMember>
            {
                new GroupMember
                {
                    Id = 2,
                    Person = new Person { FirstName = "Zippey", LastName = "Jones" }
                },
                new GroupMember
                {
                    Id = 4,
                    Person = new Person { FirstName = "Nancy", LastName = "Smith" }
                },
                new GroupMember
                {
                    Id = 1,
                    Person = new Person { FirstName = "Adele", LastName = "Jones" }
                },
                new GroupMember
                {
                    Id = 3,
                    Person = new Person { FirstName = "Fred", LastName = "Smith" }
                },
            };

            var output = ( List<object> ) RockFilters.OrderBy( members, "Person.LastName, Person.FirstName" );
            var sortedIds = output.Cast<dynamic>().Select( x => x.Id ).Cast<int>().ToList();

            CollectionAssert.AreEquivalent( expected, sortedIds );
        }

        #endregion

        #region Tags (if/else)

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void Liquid_IfElse_ShouldIf()
        {
            AssertTemplateResult( " CORRECT ", "{% if true %} CORRECT {% else %} NO {% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void Liquid_IfElse_ShouldElse()
        {
            AssertTemplateResult( " CORRECT ", "{% if false %} NO {% else %} CORRECT {% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / elsif / else
        /// </summary>
        [TestMethod]
        public void Liquid_IfElsIf_ShouldIf()
        {
            AssertTemplateResult( "CORRECT", "{% if 1 == 1 %}CORRECT{% elsif 1 == 1%}1{% else %}2{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / elsif / else
        /// </summary>
        [TestMethod]
        public void Liquid_IfElsIf_ShouldElsIf()
        {
            AssertTemplateResult( "CORRECT", "{% if 1 == 0 %}0{% elsif 1 == 1%}CORRECT{% else %}2{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / elsif / else
        /// </summary>
        [TestMethod]
        public void Liquid_IfElsIf_ShouldElse()
        {
            AssertTemplateResult( "CORRECT", "{% if 2 == 0 %}0{% elsif 2 == 1%}1{% else %}CORRECT{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void LiquidCustom_IfElseIf_ShouldElseIf()
        {
            AssertTemplateResult( "CORRECT", "{% if 1 == 0 %}0{% elseif 1 == 1%}CORRECT{% else %}2{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void LiquidCustom_IfElseIf_ShouldElse()
        {
            AssertTemplateResult( "CORRECT", "{% if 1 == 0 %}0{% elseif 1 == 2%}1{% else %}CORRECT{% endif %}" );
        }

        #endregion

        #region Date Filters

        /// <summary>
        /// For use in Lava -- adding default days to 'Now' should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddDaysDefaultViaNow()
        {
            var output = RockFilters.DateAdd( "Now", 5 );
            DateTimeAssert.AreEqual( output, RockDateTime.Now.AddDays( 5 ) );
        }

        /// <summary>
        /// For use in Lava -- adding days (default) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddDaysDefaultToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1", 3 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-4" ) );
        }

        /// <summary>
        /// For use in Lava -- adding days (d parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddDaysIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1", 3, "d" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-4" ) );
        }

        /// <summary>
        /// For use in Lava -- adding hours (h parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddHoursIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1 3:00 PM", 1, "h" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-1 4:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding minutes (m parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddMinutesIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1 3:00 PM", 120, "m" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-1 5:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding seconds (s parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddSecondsIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1 3:00 PM", 300, "s" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-1 3:05 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding years (y parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddYearsIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1 3:00 PM", 2, "y" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2020-5-1 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding years (y parameter) to a given leap-year date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddYearsIntervalToGivenLeapDate()
        {
            var output = RockFilters.DateAdd( "2016-2-29 3:00 PM", 1, "y" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2017-2-28 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding months (M parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddMonthsIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1 3:00 PM", 1, "M" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-6-1 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding months (M parameter) to a given date with more days in the month
        /// should be equal to the month's last day.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddMonthsIntervalToGivenLongerMonthDate()
        {
            var output = RockFilters.DateAdd( "2018-5-31 3:00 PM", 1, "M" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-6-30 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding weeks (w parameter) to a given date should be equal.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddWeeksIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "2018-5-1 3:00 PM", 2, "w" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-15 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week using the simplest format.
        /// Uses May 1, 2018 which was a Tuesday.
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_NextWeekdate()
        {
            // Since we're not including the current day, we advance to next week's Tuesday, 5/8
            var output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Tuesday" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-8 3:00 PM" ) );

            // Since Wednesday has not happened, we advance to it -- which is Wed, 5/2
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Wednesday" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-2 3:00 PM" ) );

            // Since Monday has passed, we advance to next week's Monday, 5/7
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Monday" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-7 3:00 PM" ) );

            // From the Lava documentation
            output = RockFilters.NextDayOfTheWeek( "2011-2-9 3:00 PM", "Friday" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2011-2-11 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week including the current day.
        /// Uses May 1, 2018 which was a Tuesday.
        /// 
        ///        May 2018        
        /// Su Mo Tu We Th Fr Sa  
        ///        1  2  3  4  5  
        ///  6  7  8  9 10 11 12  
        /// 13 14 15 16 17 18 19  
        /// 20 21 22 23 24 25 26  
        /// 27 28 29 30 31
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_NextWeekdateIncludeCurrentDay()
        {
            var output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Tuesday", true );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-1 3:00 PM" ) );

            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Wednesday", true );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-2 3:00 PM" ) );

            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Monday", true );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-7 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week in two weeks.
        /// Uses May 1, 2018 which was a Tuesday.
        /// 
        ///        May 2018
        /// Su Mo Tu We Th Fr Sa
        ///        1  2  3  4  5
        ///  6  7  8  9 10 11 12
        /// 13 14 15 16 17 18 19
        /// 20 21 22 23 24 25 26
        /// 27 28 29 30 31
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_NextWeekdateTwoWeeks()
        {
            // Since we're not including the current day, we advance to next two week's out to Tuesday, 5/15
            var output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Tuesday", false, 2 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-15 3:00 PM" ) );

            // Since Wednesday has not happened, we advance two Wednesdays -- which is Wed, 5/9
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Wednesday", false, 2 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-9 3:00 PM" ) );

            // Since Monday has passed, we advance to two week's out Monday, 5/14
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Monday", false, 2 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-14 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week with minus one week.
        /// 
        ///      April 2018
        /// Su Mo Tu We Th Fr Sa
        ///  1  2  3  4  5  6  7
        ///  8  9 10 11 12 13 14
        /// 15 16 17 18 19 20 21
        /// 22 23 24 25 26 27 28
        /// 29 30
        ///
        ///        May 2018
        /// Su Mo Tu We Th Fr Sa
        ///        1  2  3  4  5
        ///  6  7  8  9 10 11 12
        /// 13 14 15 16 17 18 19
        /// 20 21 22 23 24 25 26
        /// 27 28 29 30 31
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_NextWeekdateBackOneWeek()
        {
            // In this case, since it's Tuesday (and we're not including current day), then
            // the current day counts as the *previous* week's Tuesday.
            var output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Tuesday", false, -1 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-1 3:00 PM" ) );

            // If we include the current day (so it counts as *this* week), then one week ago would be
            // last Tuesday, April 24.
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Tuesday", true, -1 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-4-24 3:00 PM" ) );

            // Get previous week's Wednesday, 4/25
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Wednesday", false, -1 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-4-25 3:00 PM" ) );

            // Since Monday has just passed, we get this past Monday, 4/30
            output = RockFilters.NextDayOfTheWeek( "2018-5-1 3:00 PM", "Monday", false, -1 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-4-30 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the To Midnight using a string.
        /// </summary>
        [TestMethod]
        public void ToMidnight_TextString()
        {
            var output = RockFilters.ToMidnight( "2018-5-1 3:00 PM" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2018-5-1 12:00 AM" ) );
        }

        /// <summary>
        /// Tests the To Midnight using a string of "Now".
        /// </summary>
        [TestMethod]
        public void ToMidnight_Now()
        {
            var output = RockFilters.ToMidnight( "Now" );
            DateTimeAssert.AreEqual( output, RockDateTime.Now.Date );
        }

        /// <summary>
        /// Tests the To Midnight using a datetime.
        /// </summary>
        [TestMethod]
        public void ToMidnight_DateTime()
        {
            var output = RockFilters.ToMidnight( RockDateTime.Now );
            DateTimeAssert.AreEqual( output, RockDateTime.Now.Date );
        }

        #region DatesFromICal

        /// <summary>
        /// For use in Lava -- should return next occurrence for Rock's standard Saturday 4:30PM service datetime.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_OneNextSaturday()
        {
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime nextSaturday = today.AddDays( daysUntilSaturday );

            List<DateTime> expected = new List<DateTime>() { nextSaturday.AddHours( 16.5 ) }; // 4:30:00 PM

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, 1 );

            Assert.That.AreEqual( expected, output );
        }

        /// <summary>
        /// For use in Lava -- should return the current Saturday for next year's occurrence for Rock's standard Saturday 4:30PM service datetime.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_NextYearSaturday()
        {
            // Get the previous Saturday from today's date, one year on.
            // The DatesFromICal function only returns occurrences for a maximum of 1 year, so this ensures that our test date will be in the sequence.
            DateTime today = RockDateTime.Today;

            int daysAfterSaturday = ( int ) today.DayOfWeek + 1;
            DateTime previousSaturday = today.AddDays( -daysAfterSaturday );
            DateTime nextYearSaturday = previousSaturday.AddDays( 7 * 52 );

            DateTime expected = nextYearSaturday.AddHours( 16.5 ); // 4:30:00 PM

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, "all" );

            // Test if the output exists in the list of dates, because it may be entry 52 or 53 in the sequence.
            Assert.That.IsTrue( output.Contains( expected ) );
        }

        /// <summary>
        /// For use in Lava -- should return the end datetime for the next occurrence for Rock's standard Saturday 4:30PM service datetime (which ends at 5:30PM).
        /// </summary>
        [TestMethod]
        public void DatesFromICal_NextEndOccurrenceSaturday()
        {
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime nextSaturday = today.AddDays( daysUntilSaturday );

            List<DateTime> expected = new List<DateTime>() { DateTime.Parse( nextSaturday.ToShortDateString() + " 5:30:00 PM" ) };

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, null, "enddatetime" );
            CollectionAssert.AreEquivalent( expected, output );
        }

        /// <summary>
        /// For use in Lava -- should find the end datetime (10 AM) occurrence for the fictitious, first Saturday of the month event for Saturday a year from today.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_NextYearsEndOccurrenceSaturday()
        {
            // Next year's Saturday (from right now)
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime firstSaturdayThisMonth = today.AddDays( daysUntilSaturday - ( ( ( today.Day - 1 ) / 7 ) * 7 ) );
            DateTime nextYearSaturday = firstSaturdayThisMonth.AddDays( 7 * 52 );

            DateTime expected = nextYearSaturday.AddHours( 10 );

            // Get the end datetime of the 13th event in the "First Saturday of the Month" schedule.
            var output = RockFilters.DatesFromICal( iCalStringFirstSaturdayOfMonth, 13, "enddatetime" ).LastOrDefault();
            Assert.That.AreEqual( expected, output );
        }

        #endregion

        #endregion

        #region Url

        private string _urlValidHttps = "https://www.rockrms.com/WorkflowEntry/35?PersonId=2";
        private string _urlValidHttpsPort = "https://www.rockrms.com:443/WorkflowEntry/35?PersonId=2";
        private string _urlValidHttpNonStdPort = "http://www.rockrms.com:8000/WorkflowEntry/35?PersonId=2";
        private string _urlInvalid = "thequickbrownfoxjumpsoverthelazydog";

        /// <summary>
        /// Should extract the host name from the URL.
        /// </summary>
        [TestMethod]
        public void Url_Host()
        {
            var output = RockFilters.Url( _urlValidHttps, "host" );
            Assert.That.AreEqual( "www.rockrms.com", output );
        }

        /// <summary>
        /// Should extract the port number as an integer from the URL.
        /// </summary>
        [TestMethod]
        public void Url_Port()
        {
            var output = RockFilters.Url( _urlValidHttps, "port" );
            Assert.That.AreEqual( 443, output );
        }

        /// <summary>
        /// Should extract all the segments from the URL.
        /// </summary>
        [TestMethod]
        public void Url_Segments()
        {
            var output = RockFilters.Url( _urlValidHttps, "segments" ) as string[];
            Assert.That.IsNotNull( output );
            Assert.That.AreEqual( 3, output.Length );
            Assert.That.AreEqual( "/", output[0] );
            Assert.That.AreEqual( "WorkflowEntry/", output[1] );
            Assert.That.AreEqual( "35", output[2] );
        }

        /// <summary>
        /// Should extract the protocol/scheme from the URL.
        /// </summary>
        [TestMethod]
        public void Url_Scheme()
        {
            var output = RockFilters.Url( _urlValidHttps, "scheme" );
            Assert.That.AreEqual( "https", output );
        }

        /// <summary>
        /// Should extract the protocol/scheme from the URL.
        /// </summary>
        [TestMethod]
        public void Url_Protocol()
        {
            var output = RockFilters.Url( _urlValidHttps, "protocol" );
            Assert.That.AreEqual( "https", output );
        }

        /// <summary>
        /// Should extract the request path from the URL.
        /// </summary>
        [TestMethod]
        public void Url_LocalPath()
        {
            var output = RockFilters.Url( _urlValidHttps, "localpath" );
            Assert.That.AreEqual( "/WorkflowEntry/35", output );
        }

        /// <summary>
        /// Should extract the request path and the query string from the URL.
        /// </summary>
        [TestMethod]
        public void Url_PathAndQuery()
        {
            var output = RockFilters.Url( _urlValidHttps, "pathandquery" );
            Assert.That.AreEqual( "/WorkflowEntry/35?PersonId=2", output );
        }

        /// <summary>
        /// Should extract a single query parameter from the URL.
        /// </summary>
        [TestMethod]
        public void Url_QueryParameter()
        {
            var output = RockFilters.Url( _urlValidHttps, "queryparameter", "PersonId" );
            Assert.That.AreEqual( "2", output );
        }

        /// <summary>
        /// Should extract the full URL from the URL.
        /// </summary>
        [TestMethod]
        public void Url_Url()
        {
            var output = RockFilters.Url( _urlValidHttps, "url" );
            Assert.That.AreEqual( output, _urlValidHttps );
        }

        /// <summary>
        /// Should extract the full URL, trimming standard port numbers, from the URL.
        /// </summary>
        [TestMethod]
        public void Url_UrlStdPort()
        {
            var output = RockFilters.Url( _urlValidHttpsPort, "url" );
            Assert.That.AreEqual( output, _urlValidHttpsPort.Replace( ":443", string.Empty ) );
        }

        /// <summary>
        /// Should extract the full URL, including the non-standard port number, from the URL.
        /// </summary>
        [TestMethod]
        public void Url_UrlNonStdPort()
        {
            var output = RockFilters.Url( _urlValidHttpNonStdPort, "url" );
            Assert.That.AreEqual( output, _urlValidHttpNonStdPort );
        }

        /// <summary>
        /// Should fail to extract the host from an invalid URL.
        /// </summary>
        [TestMethod]
        public void Url_InvalidUrl()
        {
            var output = RockFilters.Url( _urlInvalid, "host" );
            Assert.That.AreEqual( output, string.Empty );
        }

        #endregion

        #region Helper methods to build web content folder for HttpSimulator

        /// <summary>
        /// Initializes the web content folder.
        /// </summary>
        private void InitWebContentFolder()
        {
            var codeBaseUrl = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase );
            var codeBasePath = Uri.UnescapeDataString( codeBaseUrl.AbsolutePath );
            var dirPath = System.IO.Path.GetDirectoryName( codeBasePath );
            webContentFolder = System.IO.Path.Combine( dirPath, "Content" );
        }

        #endregion

        #region Lava Test helper methods
        private static void AssertTemplateResult( string expected, string template )
        {
            AssertTemplateResult( expected, template, null );
        }

        private static void AssertTemplateResult( string expected, string template, Hash localVariables )
        {
            Assert.That.AreEqual( expected, Template.Parse( template ).Render( localVariables ) );
        }
        #endregion
    }
    #region Helper class to deal with comparing inexact dates (that are otherwise equal).

    public static class DateTimeAssert
    {
        /// <summary>
        /// Asserts that the two dates are equal within a timespan of 500 milliseconds.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <exception cref="Xunit.Sdk.EqualException">Thrown if the two dates are not equal enough.</exception>
        public static void AreEqual( DateTime? expectedDate, DateTime? actualDate )
        {
            AreEqual( expectedDate, actualDate, TimeSpan.FromMilliseconds( 500 ) );
        }

        /// <summary>
        /// Asserts that the two dates are equal within what ever timespan you deem is sufficient.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <param name="maximumDelta">The maximum delta.</param>
        /// <exception cref="Xunit.Sdk.EqualException">Thrown if the two dates are not equal enough.</exception>
        public static void AreEqual( DateTime? expectedDate, DateTime? actualDate, TimeSpan maximumDelta )
        {
            if ( expectedDate == null && actualDate == null )
            {
                return;
            }
            else if ( expectedDate == null )
            {
                throw new NullReferenceException( "The expected date was null" );
            }
            else if ( actualDate == null )
            {
                throw new NullReferenceException( "The actual date was null" );
            }

            double totalSecondsDifference = Math.Abs( ( ( DateTime ) actualDate - ( DateTime ) expectedDate ).TotalSeconds );

            if ( totalSecondsDifference > maximumDelta.TotalSeconds )
            {
                //throw new Xunit.Sdk.EqualException( string.Format( "{0}", expectedDate ), string.Format( "{0}", actualDate ) );
                throw new Exception( string.Format( "\nExpected Date: {0}\nActual Date: {1}\nExpected Delta: {2}\nActual Delta in seconds: {3}",
                                                expectedDate,
                                                actualDate,
                                                maximumDelta,
                                                totalSecondsDifference ) );
            }
        }
    }
    #endregion
}
