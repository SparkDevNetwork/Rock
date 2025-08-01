using System;
using System.Collections.Generic;
using System.Linq;

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
        /// If a range is found, both are set.
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
                return new DateRangeResult
                {
                    StartDate = DateTime.Parse( start ),
                    EndDate = DateTime.Parse( end )
                };
            }

            // Otherwise, look for a single "value"
            if ( dict.TryGetValue( "value", out var value ) )
            {
                return new DateRangeResult
                {
                    StartDate = DateTime.Parse( value ),
                    EndDate = null
                };
            }

            return null;
        }
    }

    public class DateRangeResult
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
