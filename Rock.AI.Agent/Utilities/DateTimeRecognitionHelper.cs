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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Rock.AI.Agent.Utilities
{
    /// <summary>
    /// Provides helpers to extract dates and date ranges using Microsoft Recognizers.Text.
    /// </summary>
    internal static class DateTimeRecognitionHelper
    {
        /// <summary>
        /// Recognizes a date range (start and end) or a single date from a natural language query.
        /// If only a single date is found, StartDate is set and EndDate is null.
        /// If a range is found, both are set. The lower value is inclusive but the
        /// upper value is exclusive (e.g. "before June 1" would set EndDate to June 1
        /// and StartDate would be null).
        /// </summary>
        public static DateRangeResult RecognizeDateRange( string query, DateTime referenceDate )
        {
            var results = DateTimeRecognizer.RecognizeDateTime( query, Culture.English, refTime: referenceDate );

            if ( !results.Any() )
            {
                return null;
            }

            if ( !results[0].Resolution.TryGetValue( "values", out var values ) || !( values is IList<Dictionary<string, string>> resolution ) || resolution.Count == 0 )
            {
                return null;
            }

            var dict = resolution[0];

            // If "start" and "end" present, assume range
            if ( dict.TryGetValue( "start", out var start ) && dict.TryGetValue( "end", out var end ) )
            {
                var range = new DateRangeResult
                {
                    StartDate = DateTime.Parse( start ),
                    EndDate = DateTime.Parse( end )
                };

                var type = dict.GetValueOrNull( "type" );

                if ( type == "timerange" || type == "datetimerange" )
                {
                    range.EndDate = range.EndDate.Value.AddMilliseconds( -1 );
                }
                else if ( type == "daterange" )
                {
                    AdjustTimexEndDate( range, dict );
                }

                return range;
            }

            // Otherwise, look for a single "value"
            if ( dict.TryGetValue( "value", out var value ) )
            {
                var range = new DateRangeResult
                {
                    StartDate = DateTime.Parse( value ),
                };

                range.EndDate = range.StartDate.Value.AddDays( 1 ).AddMilliseconds( -1 );

                return range;
            }

            return null;
        }

        private static void AdjustTimexEndDate( DateRangeResult range, Dictionary<string, string> dict )
        {
            var isoDuration = new Regex( @"P(?:(?<Years>\d+)Y)?(?:(?<Months>\d+)M)?(?:(?<Weeks>\d+)W)?(?:(?<Days>\d+)D)?(?:T(?:(?<Hours>\d+)H)?(?:(?<Minutes>\d+)M)?(?:(?<Seconds>\d+(?:\.\d+)?)S)?)?", RegexOptions.Compiled | RegexOptions.IgnoreCase );

            if ( dict.TryGetValue( "timex", out var timex ) )
            {
                var match = isoDuration.Match( timex );

                if ( match.Success )
                {
                    var newEndDate = range.StartDate.Value;
                    var canAddDay = true;

                    if ( int.TryParse( match.Groups["Years"].Value, out var years ) )
                    {
                        newEndDate = newEndDate.AddYears( years );
                    }

                    if ( int.TryParse( match.Groups["Months"].Value, out var months ) )
                    {
                        newEndDate = newEndDate.AddMonths( months );
                        canAddDay = !timex.Contains( "XXXX" );
                    }

                    if ( int.TryParse( match.Groups["Weeks"].Value, out var weeks ) )
                    {
                        newEndDate = newEndDate.AddDays( weeks * 7 );
                    }

                    if ( int.TryParse( match.Groups["Days"].Value, out var days ) )
                    {
                        newEndDate = newEndDate.AddDays( days );
                    }

                    if ( int.TryParse( match.Groups["Hours"].Value, out var hours ) )
                    {
                        newEndDate = newEndDate.AddHours( hours );
                    }

                    if ( int.TryParse( match.Groups["Minutes"].Value, out var minutes ) )
                    {
                        newEndDate = newEndDate.AddMinutes( minutes );
                    }

                    if ( int.TryParse( match.Groups["Seconds"].Value, out var seconds ) )
                    {
                        newEndDate = newEndDate.AddSeconds( seconds );
                    }

                    if ( range.StartDate.Value.Date != newEndDate.Date && canAddDay )
                    {
                        newEndDate = newEndDate.AddDays( 1 );
                    }

                    range.EndDate = newEndDate.AddMilliseconds( -1 );

                    return;
                }
                else
                {
                }
            }

            if ( range.StartDate.Value != range.EndDate.Value )
            {
                range.EndDate = range.EndDate.Value.AddMilliseconds( -1 );
            }
        }
    }

    internal class DateRangeResult
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
