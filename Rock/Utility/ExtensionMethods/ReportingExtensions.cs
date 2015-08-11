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
