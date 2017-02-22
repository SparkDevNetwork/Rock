using Rock;
using Rock.Lava;
using Rock.Model;
using System.Collections.Generic;
using Xunit;
using System.Linq;

namespace Rock.Tests.Rock.Lava
{
    public class RockFiltersTest
    {
        static readonly Dictionary<string, object> mergeObjects = new Dictionary<string, object>();

        #region Minus
        /// <summary>
        /// For use in Lava -- should subtract two integers and return an integer.
        /// </summary>
        [Fact]
        public void MinusTwoInts()
        {
            // I'd like to test via Lava Resolve/MergeFields but can't get that to work.
            //string lava = "{{ 3 | Minus: 2 | ToJSON }}";
            //var person = new Person();
            //var o = lava.ResolveMergeFields( mergeObjects, person, "" );
            //Assert.Equal( "1", o);
            var output = RockFilters.Minus( 3, 2 );
            Assert.Equal( 1, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two decimals and return a decimal.
        /// </summary>
        [Fact]
        public void MinusTwoDecimals()
        {
            var output = RockFilters.Minus( 3.0M, 2.0M );
            Assert.Equal( 1.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two strings (containing integers) and return an int.
        /// </summary>
        [Fact]
        public void MinusTwoStringInts()
        {
            var output = RockFilters.Minus( "3", "2" );
            Assert.Equal( 1, output ); 
        }

        /// <summary>
        /// For use in Lava -- should subtract two strings (containing decimals) and return a decimal.
        /// </summary>
        [Fact]
        public void MinusTwoStringDecimals()
        {
            var output = RockFilters.Minus( "3.0", "2.0" );
            Assert.Equal( 1.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void MinusIntAndDecimal()
        {
            var output = RockFilters.Minus( 3, "2.0" );
            Assert.Equal( 1.0M, output );
        }
        #endregion

        #region Plus
        /// <summary>
        /// For use in Lava -- should add two integers and return an integer.
        /// </summary>
        [Fact]
        public void PlusTwoInts()
        {
            var output = RockFilters.Plus( 3, 2 );
            Assert.Equal( 5, output );
        }

        /// <summary>
        /// For use in Lava -- should add two decimals and return a decimal.
        /// </summary>
        [Fact]
        public void PlusTwoDecimals()
        {
            var output = RockFilters.Plus( 3.0M, 2.0M );
            Assert.Equal( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should add two strings (containing integers) and return an int.
        /// </summary>
        [Fact]
        public void PlusTwoStringInts()
        {
            var output = RockFilters.Plus( "3", "2" );
            Assert.Equal( 5, output );
        }

        /// <summary>
        /// For use in Lava -- should add two strings (containing decimals) and return a decimal.
        /// </summary>
        [Fact]
        public void PlusTwoStringDecimals()
        {
            var output = RockFilters.Plus( "3.0", "2.0" );
            Assert.Equal( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should add an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void PlusIntAndDecimal()
        {
            var output = RockFilters.Plus( 3, "2.0" );
            Assert.Equal( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should concat two strings.
        /// </summary>
        [Fact]
        public void PlusStrings()
        {
            var output = RockFilters.Plus( "Foo", "Bar" );
            Assert.Equal( "FooBar", output );
        }

        #endregion

        #region Times
        /// <summary>
        /// For use in Lava -- should multiply two integers and return an integer.
        /// </summary>
        [Fact]
        public void TimesTwoInts()
        {
            var output = RockFilters.Times( 3, 2 );
            Assert.Equal( 6, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two decimals and return a decimal.
        /// </summary>
        [Fact]
        public void TimesTwoDecimals()
        {
            var output = RockFilters.Times( 3.0M, 2.0M );
            Assert.Equal( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two strings (containing integers) and return an int.
        /// </summary>
        [Fact]
        public void TimesTwoStringInts()
        {
            var output = RockFilters.Times( "3", "2" );
            Assert.Equal( 6, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two strings (containing decimals) and return a decimal.
        /// </summary>
        [Fact]
        public void TimesTwoStringDecimals()
        {
            var output = RockFilters.Times( "3.0", "2.0" );
            Assert.Equal( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void TimesIntAndDecimal()
        {
            var output = RockFilters.Times( 3, "2.0" );
            Assert.Equal( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should repeat the string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void TimesStringAndInt()
        {
            var expectedOutput = Enumerable.Repeat( "Foo", 2 );
            var output = RockFilters.Times( "Foo", 2 );
            Assert.Equal( expectedOutput, output );
        }

        #endregion
    }
}
