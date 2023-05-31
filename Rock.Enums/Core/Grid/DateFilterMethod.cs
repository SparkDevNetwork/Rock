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

namespace Rock.Enums.Core.Grid
{
    /// <summary>
    /// The filtering method used for a date filter column filter.
    /// </summary>
    public enum DateFilterMethod
    {
        /// <summary>
        /// The date portion of the cell value must match the filter value.
        /// </summary>
        Equals = 0,

        /// <summary>
        /// The date portion of the cell value must not match the filter value.
        /// </summary>
        DoesNotEqual = 1,

        /// <summary>
        /// The date portion of the cell value must be less than the filter value.
        /// </summary>
        Before = 2,

        /// <summary>
        /// The date portion of the cell value must be greater than the filter value.
        /// </summary>
        After = 3,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the lower filter value and less than or equal to the upper filter value.
        /// </summary>
        Between = 4,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of this week and less than or equal to the last day of
        /// this week.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The start of the week defaults to Monday but can be changed in the
        /// System Configuration.
        /// </para>
        /// <para>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 3/13/2023 &amp;&amp; cellDate &lt;= 3/19/2023</code>
        /// </para>
        /// </remarks>
        ThisWeek = 5,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of the previous week and less than or equal to the last
        /// day of the previous week.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The start of the week defaults to Monday but can be changed in the
        /// System Configuration.
        /// </para>
        /// <para>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 3/6/2023 &amp;&amp; cellDate &lt;= 3/12/2023</code>
        /// </para>
        /// </remarks>
        LastWeek = 6,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of the next week and less than or equal to the last
        /// day of the next week.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The start of the week defaults to Monday but can be changed in the
        /// System Configuration.
        /// </para>
        /// <para>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 3/20/2023 &amp;&amp; cellDate &lt;= 3/26/2023</code>
        /// </para>
        /// </remarks>
        NextWeek = 7,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of this month and less than or equal to the last day of
        /// this month.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 3/1/2023 &amp;&amp; cellDate &lt;= 3/31/2023</code>
        /// </remarks>
        ThisMonth = 8,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of the previous month and less than or equal to the last
        /// day of the previous month.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 2/1/2023 &amp;&amp; cellDate &lt;= 2/28/2023</code>
        /// </remarks>
        LastMonth = 9,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of the next month and less than or equal to the last
        /// day of the next month.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 4/1/2023 &amp;&amp; cellDate &lt;= 4/30/2023</code>
        /// </remarks>
        NextMonth = 10,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of this year and less than or equal to the last day of
        /// this year.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 1/1/2023 &amp;&amp; cellDate &lt;= 12/31/2023</code>
        /// </remarks>
        ThisYear = 11,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of the previous year and less than or equal to the last
        /// day of the previous year.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 1/1/2022 &amp;&amp; cellDate &lt;= 12/31/2022</code>
        /// </remarks>
        LastYear = 12,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of the next year and less than or equal to the last
        /// day of the next year.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 1/1/2024 &amp;&amp; cellDate &lt;= 12/31/2024</code>
        /// </remarks>
        NextYear = 13,

        /// <summary>
        /// The date portion of the cell value must be greater than or equal to
        /// the first day of this year and less than or equal to today.
        /// </summary>
        /// <remarks>
        /// If today is Wednesday 3/15/2023, then this check would be:
        /// <code>cellDate &gt;= 1/1/2023 &amp;&amp; cellDate &lt;= 3/15/2023</code>
        /// </remarks>
        YearToDate = 14
    }
}
