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

using System;
using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Utilities;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class SystemUtilitySkill
{
    #region Tool(s)

    [Description( "Determines a date range from a natural language string." )]
    [AgentToolPreamble( "Determining Date Range for Query." )]
    [AgentPurpose( "Determines a date range from a natural language string." )]
    [AgentUsage( "This function is useful in cases where you need to determine a date range from a query that does not include specific time units." )]
    [AgentToolGuid( "87756092-9D52-448E-82EE-556A780DF7CF" )]
    public AgentToolResult DetermineDateRange(
        [Description( "A natural language string, such as 'first quarter', 'yesterday' 'tomorrow', 'year to date'.")]
        string query )
    {
        var dateRange = DateTimeRecognitionHelper.RecognizeDateRange( query, DateTime.Now );

        if ( dateRange == null )
        {
            return Error( "A date range could not be determined from the query." )
                .WithInstructions( $"Today is {DateTime.Now}. Using today as a reference date, infer the date range yourself." );
        }

        return Success( dateRange );
    }

    // This was added as an attempt to make the date range queries match
    // the UI sliding date range slider, but it proved to be difficult
    // since the way we talk about dates doesn't actually match how the
    // sliding date range picker works.

    //[AgentToolPreamble( "Calculating Date Range" )]
    //[AgentPurpose( "This tool should be used when the request includes enough data to fill in the arguments because it will match date conversion logic for the system. Otherwise the DetermineDateRange tool should be used.")]
    //[AgentUsage( "Previous and Upcoming are anchored on natural calendar boundaries (whole hours, days, weeks, etc) while Last and Next are anchored to the current date and time." )]
    //[AgentUsage( "These time units have specific meaning in this system and are used in UI, so if the query specifies one of these terms it should be used without inferring intent." )]
    //[AgentUsage( "Whole weeks are typically defined as Monday - Sunday, though the organization can override that." )]
    //[AgentToolGuid( "376bdaa2-a947-4e9f-baad-30f09f7c8f64" )]
    //public AgentToolResult CalculateSlidingDateRange(
    //    DateRangeType dateRangeType,
    //    int numberOfUnits,
    //    TimeUnitType timeUnit )
    //{
    //    SlidingDateRangeType slidingDateRangeType;

    //    if ( dateRangeType == DateRangeType.Last )
    //    {
    //        slidingDateRangeType = SlidingDateRangeType.Last;
    //    }
    //    else if ( dateRangeType == DateRangeType.Previous )
    //    {
    //        slidingDateRangeType = SlidingDateRangeType.Previous;
    //    }
    //    else if ( dateRangeType == DateRangeType.Current )
    //    {
    //        slidingDateRangeType = SlidingDateRangeType.Current;
    //    }
    //    else if ( dateRangeType == DateRangeType.Next )
    //    {
    //        slidingDateRangeType = SlidingDateRangeType.Next;
    //    }
    //    else if ( dateRangeType == DateRangeType.Upcoming )
    //    {
    //        slidingDateRangeType = SlidingDateRangeType.Upcoming;
    //    }
    //    else
    //    {
    //        return Error( "Invalid DateRangeType." );
    //    }

    //    var range = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( slidingDateRangeType, numberOfUnits, timeUnit, null, null );

    //    if ( !range.Start.HasValue && !range.End.HasValue )
    //    {
    //        return Error( $"A date range could not be determined from the provided arguments. Considering trying ${nameof( DetermineDateRange )} instead." );
    //    }

    //    return Success( new DateRangeResult
    //    {
    //        StartDate = range.Start,
    //        EndDate = range.End,
    //    } );
    //}

    #endregion

    //public enum DateRangeType
    //{
    //    Last = 0,
    //    Previous = 1,
    //    Current = 2,
    //    Next = 3,
    //    Upcoming = 4,
    //}
}
