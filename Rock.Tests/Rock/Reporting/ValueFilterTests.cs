using Xunit;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Tests.Rock.Reporting
{
    public class ValueFilterTests
    {
        private readonly SampleData Sample = new SampleData
        {
            Text = "The quick brown fox jumps over the lazy dog",
            EmptyText = string.Empty,
            NullText = null,
            Integer = 42,
            Double = 42.42
        };

        #region String Value Comparisons

        /// <summary>
        /// Filter Test: String contains value
        /// </summary>
        [Fact]
        public void StringContains()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Contains,
                Value = "brown"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String contains value of differing case
        /// </summary>
        [Fact]
        public void StringContainsInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Contains,
                Value = "BROWN"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String does not contain value
        /// </summary>
        [Fact]
        public void StringDoesNotContain()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.DoesNotContain,
                Value = "jason"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String does not contain value of differing case
        /// </summary>
        [Fact]
        public void StringDoesNotContainInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.DoesNotContain,
                Value = "JASON"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String ends with value
        /// </summary>
        [Fact]
        public void StringEndsWith()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EndsWith,
                Value = "dog"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String ends with value of differing case
        /// </summary>
        [Fact]
        public void StringEndsWithInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EndsWith,
                Value = "DOG"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String equal to value
        /// </summary>
        [Fact]
        public void StringEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = Sample.Text
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String equal to value of differing case
        /// </summary>
        [Fact]
        public void StringEqualToInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = Sample.Text.ToUpper()
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String is blank
        /// </summary>
        [Fact]
        public void StringIsBlank()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.IsBlank,
                Value = string.Empty
            };

            Assert.True( filter.Evaluate( Sample, "EmptyText" ) );
        }

        /// <summary>
        /// Filter Test: String is blank (null string check)
        /// </summary>
        [Fact]
        public void StringIsBlankNull()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.IsBlank,
                Value = string.Empty
            };

            Assert.True( filter.Evaluate( Sample, "NullText" ) );
        }

        /// <summary>
        /// Filter Test: String is not blank
        /// </summary>
        [Fact]
        public void StringIsNotBlank()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.IsNotBlank,
                Value = string.Empty
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String is not equal to value
        /// </summary>
        [Fact]
        public void StringNotEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = "Jim Bob"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String is not equal to value of differing case
        /// </summary>
        [Fact]
        public void StringNotEqualToInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = "JIM BOB"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String matches regular expression
        /// </summary>
        [Fact]
        public void StringRegularExpression()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.RegularExpression,
                Value = "^The.*dog$"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String matches regular expression of differing case
        /// </summary>
        [Fact]
        public void StringRegularExpressionInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.RegularExpression,
                Value = "^THE.*DOG$"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String starts with value
        /// </summary>
        [Fact]
        public void StringStartsWith()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.StartsWith,
                Value = "The quick"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// Filter Test: String starts with value of differing case
        /// </summary>
        [Fact]
        public void StringStartsWithInsensitive()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.StartsWith,
                Value = "THE QUICK"
            };

            Assert.True( filter.Evaluate( Sample, "Text" ) );
        }

        #endregion

        #region String Value Comparisons (False)

        /// <summary>
        /// False Filter Test: String NOT contains value
        /// </summary>
        [Fact]
        public void StringContainsFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Contains,
                Value = "invaliddata"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT contains value of differing case
        /// </summary>
        [Fact]
        public void StringContainsInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Contains,
                Value = "INVALIDDATA"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String DOES contain value
        /// </summary>
        [Fact]
        public void StringDoesNotContainFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.DoesNotContain,
                Value = "quick"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String DOES contain value of differing case
        /// </summary>
        [Fact]
        public void StringDoesNotContainInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.DoesNotContain,
                Value = "QUICK"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT ends with value
        /// </summary>
        [Fact]
        public void StringEndsWithFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EndsWith,
                Value = "invaliddata"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT ends with value of differing case
        /// </summary>
        [Fact]
        public void StringEndsWithInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EndsWith,
                Value = "INVALIDDATA"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT equal to value
        /// </summary>
        [Fact]
        public void StringEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = "invaliddata"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT equal to value of differing case
        /// </summary>
        [Fact]
        public void StringEqualToInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = "invaliddata"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String is NOT blank
        /// </summary>
        [Fact]
        public void StringIsBlankFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.IsBlank,
                Value = string.Empty
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String IS blank
        /// </summary>
        [Fact]
        public void StringIsNotBlankFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.IsNotBlank,
                Value = string.Empty
            };

            Assert.False( filter.Evaluate( Sample, "EmptyText" ) );
        }

        /// <summary>
        /// False Filter Test: String IS blank (null string check)
        /// </summary>
        [Fact]
        public void StringIsNotBlankNullFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.IsNotBlank,
                Value = string.Empty
            };

            Assert.False( filter.Evaluate( Sample, "NullText" ) );
        }

        /// <summary>
        /// False Filter Test: String IS equal to value
        /// </summary>
        [Fact]
        public void StringNotEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = Sample.Text
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String IS equal to value of differing case
        /// </summary>
        [Fact]
        public void StringNotEqualToInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = Sample.Text.ToUpper()
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT matches regular expression
        /// </summary>
        [Fact]
        public void StringRegularExpressionFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.RegularExpression,
                Value = "^Dog.*the$"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT matches regular expression of differing case
        /// </summary>
        [Fact]
        public void StringRegularExpressionInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.RegularExpression,
                Value = "^DOG.*THE$"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT starts with value
        /// </summary>
        [Fact]
        public void StringStartsWithFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.StartsWith,
                Value = "brown fox"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        /// <summary>
        /// False Filter Test: String NOT starts with value of differing case
        /// </summary>
        [Fact]
        public void StringStartsWithInsensitiveFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.StartsWith,
                Value = "BROWN FOX"
            };

            Assert.False( filter.Evaluate( Sample, "Text" ) );
        }

        #endregion

        #region Integer Value Comparisons

        /// <summary>
        /// Filter Test: Integer is between two values
        /// </summary>
        [Fact]
        public void IntegerBetween()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Between,
                Value = "30",
                Value2 = "50"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is equal to value
        /// </summary>
        [Fact]
        public void IntegerEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = "42"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is greater than value
        /// </summary>
        [Fact]
        public void IntegerGreaterThan()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThan,
                Value = "30"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is greater than or equal to value
        /// </summary>
        [Fact]
        public void IntegerGreaterThanOrEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThanOrEqualTo,
                Value = "30"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is greater than or equal to value (value == integer)
        /// </summary>
        [Fact]
        public void IntegerGreaterThanOrEqualToEquals()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThanOrEqualTo,
                Value = "42"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is less than value
        /// </summary>
        [Fact]
        public void IntegerLessThan()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThan,
                Value = "50"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is less than or equal to value
        /// </summary>
        [Fact]
        public void IntegerLessThanOrEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThanOrEqualTo,
                Value = "50"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is less than or equal to value (value == integer)
        /// </summary>
        [Fact]
        public void IntegerLessThanOrEqualToEquals()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThanOrEqualTo,
                Value = "42"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// Filter Test: Integer is not equal to value
        /// </summary>
        [Fact]
        public void IntegerNotEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = "30"
            };

            Assert.True( filter.Evaluate( Sample, "Integer" ) );
        }

        #endregion

        #region Integer Value Comparisons (False)

        /// <summary>
        /// False Filter Test: Integer is NOT between two values
        /// </summary>
        [Fact]
        public void IntegerBetweenFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Between,
                Value = "45",
                Value2 = "50"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// False Filter Test: Integer is NOT equal to value
        /// </summary>
        [Fact]
        public void IntegerEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = "30"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// False Filter Test: Integer is NOT greater than value
        /// </summary>
        [Fact]
        public void IntegerGreaterThanFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThan,
                Value = "50"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// False Filter Test: Integer is NOT greater than or equal to value
        /// </summary>
        [Fact]
        public void IntegerGreaterThanOrEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThanOrEqualTo,
                Value = "50"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// False Filter Test: Integer is NOT less than value
        /// </summary>
        [Fact]
        public void IntegerLessThanFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThan,
                Value = "30"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// False Filter Test: Integer is NOT less than or equal to value
        /// </summary>
        [Fact]
        public void IntegerLessThanOrEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThanOrEqualTo,
                Value = "30"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        /// <summary>
        /// False Filter Test: Integer IS equal to value
        /// </summary>
        [Fact]
        public void IntegerNotEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = "42"
            };

            Assert.False( filter.Evaluate( Sample, "Integer" ) );
        }

        #endregion

        #region Double Value Comparisons

        /// <summary>
        /// Filter Test: Double is between two values
        /// </summary>
        [Fact]
        public void DoubleBetween()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Between,
                Value = "42.1",
                Value2 = "42.6"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is equal to value
        /// </summary>
        [Fact]
        public void DoubleEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = "42.42"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is greater than value
        /// </summary>
        [Fact]
        public void DoubleGreaterThan()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThan,
                Value = "42.1"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is greater than or equal to value
        /// </summary>
        [Fact]
        public void DoubleGreaterThanOrEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThanOrEqualTo,
                Value = "42.1"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is greater than or equal to value (value == Double)
        /// </summary>
        [Fact]
        public void DoubleGreaterThanOrEqualToEquals()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThanOrEqualTo,
                Value = "42.42"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is less than value
        /// </summary>
        [Fact]
        public void DoubleLessThan()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThan,
                Value = "42.6"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is less than or equal to value
        /// </summary>
        [Fact]
        public void DoubleLessThanOrEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThanOrEqualTo,
                Value = "42.6"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is less than or equal to value (value == Double)
        /// </summary>
        [Fact]
        public void DoubleLessThanOrEqualToEquals()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThanOrEqualTo,
                Value = "42.42"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// Filter Test: Double is not equal to value
        /// </summary>
        [Fact]
        public void DoubleNotEqualTo()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = "42.1"
            };

            Assert.True( filter.Evaluate( Sample, "Double" ) );
        }

        #endregion

        #region Double Value Comparisons (False)

        /// <summary>
        /// False Filter Test: Double is NOT between two values
        /// </summary>
        [Fact]
        public void DoubleBetweenFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.Between,
                Value = "45.1",
                Value2 = "48.6"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// False Filter Test: Double is NOT equal to value
        /// </summary>
        [Fact]
        public void DoubleEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.EqualTo,
                Value = "45.1"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// False Filter Test: Double is NOT greater than value
        /// </summary>
        [Fact]
        public void DoubleGreaterThanFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThan,
                Value = "45.1"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// False Filter Test: Double is NOT greater than or equal to value
        /// </summary>
        [Fact]
        public void DoubleGreaterThanOrEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.GreaterThanOrEqualTo,
                Value = "45.1"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// False Filter Test: Double is NOT less than value
        /// </summary>
        [Fact]
        public void DoubleLessThanFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThan,
                Value = "41.4"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// False Filter Test: Double is NOT less than or equal to value
        /// </summary>
        [Fact]
        public void DoubleLessThanOrEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.LessThanOrEqualTo,
                Value = "41.4"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        /// <summary>
        /// False Filter Test: Double IS equal to value
        /// </summary>
        [Fact]
        public void DoubleNotEqualToFalse()
        {
            var filter = new ComparisonFilterExpression
            {
                Comparison = ComparisonType.NotEqualTo,
                Value = "42.42"
            };

            Assert.False( filter.Evaluate( Sample, "Double" ) );
        }

        #endregion

        #region Support Classes

        private class SampleData
        {
            public string Text { get; set; }

            public string EmptyText { get; set; }

            public string NullText { get; set; }

            public int Integer { get; set; }

            public double Double { get; set; }
        }

        #endregion
    }
}
