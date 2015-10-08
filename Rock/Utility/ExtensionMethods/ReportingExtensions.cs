// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Extensions related to Rock Reports, Dataviews and DataFilters
    /// </summary>
    public static partial class ExtensionMethods
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

            if ( compareType == ComparisonType.StartsWith )
            {
                return value.StartsWith( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.EndsWith )
            {
                return value.EndsWith( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.IsBlank )
            {
                return string.IsNullOrWhiteSpace( value );
            }

            if ( compareType == ComparisonType.IsNotBlank )
            {
                return !string.IsNullOrWhiteSpace( value );
            }

            // Following compares could be numeric
            decimal? decimalValue = value.AsDecimalOrNull();
            decimal? decimalCompareValue = compareValue.AsDecimalOrNull();

            if ( compareType == ComparisonType.EqualTo )
            {
                if ( decimalValue.HasValue && decimalCompareValue.HasValue )
                {
                    return decimalValue.Value == decimalCompareValue.Value;
                }
                return value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.GreaterThan )
            {
                if ( decimalValue.HasValue && decimalCompareValue.HasValue )
                {
                    return decimalValue.Value > decimalCompareValue.Value;
                }
                return value.CompareTo( compareValue ) > 0;
            }

            if ( compareType == ComparisonType.GreaterThanOrEqualTo )
            {
                if ( decimalValue.HasValue && decimalCompareValue.HasValue )
                {
                    return decimalValue.Value >= decimalCompareValue.Value;
                }
                return value.CompareTo( compareValue ) >= 0;
            }

            if ( compareType == ComparisonType.LessThan )
            {
                if ( decimalValue.HasValue && decimalCompareValue.HasValue )
                {
                    return decimalValue.Value < decimalCompareValue.Value;
                }
                return value.CompareTo( compareValue ) < 0;
            }

            if ( compareType == ComparisonType.LessThanOrEqualTo )
            {
                if ( decimalValue.HasValue && decimalCompareValue.HasValue )
                {
                    return decimalValue.Value >= decimalCompareValue.Value;
                }
                return value.CompareTo( compareValue ) <= 0;
            }

            if ( compareType == ComparisonType.NotEqualTo )
            {
                if ( decimalValue.HasValue && decimalCompareValue.HasValue )
                {
                    return decimalValue.Value != decimalCompareValue.Value;
                }
                return !value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }
    }
}
