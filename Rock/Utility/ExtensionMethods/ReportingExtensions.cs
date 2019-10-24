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
using System.Text.RegularExpressions;

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
            // Evaluate compare types that are not type specific
            switch ( compareType )
            {
                case ComparisonType.Contains: return value.Contains( compareValue );
                case ComparisonType.DoesNotContain: return !value.Contains( compareValue );
                case ComparisonType.StartsWith: return value.StartsWith( compareValue, StringComparison.OrdinalIgnoreCase );
                case ComparisonType.EndsWith: return value.EndsWith( compareValue, StringComparison.OrdinalIgnoreCase );
                case ComparisonType.IsBlank: return string.IsNullOrWhiteSpace( value );
                case ComparisonType.IsNotBlank: return !string.IsNullOrWhiteSpace( value );
                case ComparisonType.RegularExpression: try { return  Regex.IsMatch( value, compareValue ); } catch { return false; }
            }

            // numeric compares
            decimal? decimalValue = value.AsDecimalOrNull();
            decimal? decimalCompareValue = compareValue.AsDecimalOrNull();
            if ( decimalValue.HasValue && decimalCompareValue.HasValue )
            { 
                switch ( compareType )
                {
                    case ComparisonType.EqualTo: return decimalValue == decimalCompareValue;
                    case ComparisonType.GreaterThan: return decimalValue.Value > decimalCompareValue.Value;
                    case ComparisonType.GreaterThanOrEqualTo: return decimalValue.Value >= decimalCompareValue.Value;
                    case ComparisonType.LessThan: return decimalValue.Value < decimalCompareValue.Value;
                    case ComparisonType.LessThanOrEqualTo: return decimalValue.Value <= decimalCompareValue.Value;
                    case ComparisonType.NotEqualTo: return decimalValue.Value != decimalCompareValue.Value;
                }
            }

            // date time compares
            DateTime? datetimeValue = value.AsDateTime();
            DateTime? datetimeCompareValue = compareValue.AsDateTime();
            if ( datetimeValue.HasValue && datetimeCompareValue.HasValue )
            {
                switch ( compareType )
                {
                    case ComparisonType.EqualTo: return datetimeValue == datetimeCompareValue;
                    case ComparisonType.GreaterThan: return datetimeValue.Value > datetimeCompareValue.Value;
                    case ComparisonType.GreaterThanOrEqualTo: return datetimeValue.Value >= datetimeCompareValue.Value;
                    case ComparisonType.LessThan: return datetimeValue.Value < datetimeCompareValue.Value;
                    case ComparisonType.LessThanOrEqualTo: return datetimeValue.Value <= datetimeCompareValue.Value;
                    case ComparisonType.NotEqualTo: return datetimeValue.Value != datetimeCompareValue.Value;
                }
            }
            
            // string compares
            switch ( compareType )
            {
                case ComparisonType.EqualTo: return value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
                case ComparisonType.GreaterThan: return value.CompareTo( compareValue ) > 0;
                case ComparisonType.GreaterThanOrEqualTo: return value.CompareTo( compareValue ) >= 0;
                case ComparisonType.LessThan: return value.CompareTo( compareValue ) < 0;
                case ComparisonType.LessThanOrEqualTo: return value.CompareTo( compareValue ) <= 0;
                case ComparisonType.NotEqualTo: return !value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }
    }
}
