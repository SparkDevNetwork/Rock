using System;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Extensions related to Rock Reports, Dataviews and DataFilters
    /// </summary>
    public static class ReportingExtensions
    {
        /// <summary>
        /// Compares to the given value returning true if comparable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <param name="compareType">Type of the compare.</param>
        /// <returns></returns>
        public static bool CompareTo( this string value, string compareValue, ComparisonType compareType )
        {
            if ( compareType == ComparisonType.Contains )
            {
                return value.Contains( compareValue );
            }

            if ( compareType == ComparisonType.DoesNotContain )
            {
                return !value.Contains( compareValue );
            }

            if ( compareType == ComparisonType.EndsWith )
            {
                return value.EndsWith( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.EqualTo )
            {
                return value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.GreaterThan )
            {
                return value.CompareTo( compareValue ) > 0;
            }

            if ( compareType == ComparisonType.GreaterThanOrEqualTo )
            {
                return value.CompareTo( compareValue ) >= 0;
            }

            if ( compareType == ComparisonType.IsBlank )
            {
                return string.IsNullOrWhiteSpace( value );
            }

            if ( compareType == ComparisonType.IsNotBlank )
            {
                return !string.IsNullOrWhiteSpace( value );
            }

            if ( compareType == ComparisonType.LessThan )
            {
                return value.CompareTo( compareValue ) < 0;
            }

            if ( compareType == ComparisonType.LessThanOrEqualTo )
            {
                return value.CompareTo( compareValue ) <= 0;
            }

            if ( compareType == ComparisonType.NotEqualTo )
            {
                return !value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.StartsWith )
            {
                return value.StartsWith( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }
    }
}
