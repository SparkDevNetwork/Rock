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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.Reporting;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReportingSkill
{
    #region Constants

    /// <summary>
    /// The number of report rows returned per page.
    /// </summary>
    private const int ReportPageSize = 50;

    #endregion

    #region Tool(s)

    /// <summary>
    /// Runs a report and returns its rows.
    /// </summary>
    /// <remarks>
    /// This runs through an internal Rock core shim (<c>AgentReportRunner</c>) so the
    /// report engine's System.Web dependency never enters this assembly. Rows are
    /// paged in memory and, unless a sort field is given, returned in the report's
    /// saved order. Values are the raw query values, not display-formatted markup;
    /// attribute columns the caller may not view are masked.
    /// </remarks>
    [Description( "Runs a report and returns its rows, paged, optionally sorted by one field. Values are the report's raw column values; columns the caller cannot view are masked." )]
    [AgentPurpose( "Gets the rows a report produces." )]
    [AgentUsage( "Pass sortByFieldIdKey (a field's IdKey from GetReport) with isDescending to sort by that field; omit it to use the report's saved sort. Use pageNumber to page through large reports." )]
    [AgentToolPrerequisite( "Call GetReport to determine the reportIdKey and which fields can be sorted on." )]
    [AgentToolGuid( "0481F45E-98DA-403E-8709-A132960F9107" )]
    public AgentToolResult GetReportItems( string reportIdKey, string sortByFieldIdKey = null, bool isDescending = false, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        // GetRequiredEntity enforces view permission on the report itself.
        var report = helper.GetRequiredEntity<Rock.Model.Report>( reportIdKey );

        if ( report == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListReports function to determine the available reports." );
        }

        // Resolve and validate the optional sort field against the report's fields so
        // the caller gets a clear error rather than a generic run failure.
        int? sortByFieldId = null;

        if ( sortByFieldIdKey.IsNotNullOrWhiteSpace() )
        {
            var fieldId = IdHasher.Instance.GetId( sortByFieldIdKey );
            var sortField = fieldId.HasValue ? report.ReportFields.FirstOrDefault( f => f.Id == fieldId.Value ) : null;

            if ( sortField == null )
            {
                return Error( $"'{sortByFieldIdKey}' is not a field of this report." )
                    .WithInstructions( "Call the GetReport function to determine the report's fields." );
            }

            if ( !ReportQueryBuilder.IsFieldSortable( sortField ) )
            {
                return Error( $"The field '{( sortField.ColumnHeaderText.IsNotNullOrWhiteSpace() ? sortField.ColumnHeaderText : sortField.Selection )}' cannot be sorted on." )
                    .WithInstructions( "Call the GetReport function to see which fields can be sorted on." );
            }

            sortByFieldId = sortField.Id;
        }

        AgentReportResult output;

        try
        {
            output = AgentReportRunner.GetReportData( report, AgentRequestContext.CurrentPerson, AgentRequestContext.RockContext, sortByFieldId, isDescending );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to run report {ReportId} for the agent.", report.Id );

            return Error( "The report could not be run. It may include a column that cannot be queried directly." );
        }

        if ( output?.Rows == null )
        {
            return NoData();
        }

        // The shim returns the full result set; page it in memory.
        var pageIndex = Math.Max( 0, pageNumber - 1 );
        var totalRowCount = output.Rows.Count;

        /*
            9/1/26 - CLAUDE

            Each row carries only the record Id (surfaced as IdKey), not its Guid.
            The report engine projects a runtime dynamic type that includes the Id
            but not the Guid, so the Guid is not available here without either adding
            it to the shared Report.GetQueryable projection (which would affect every
            report consumer, including the grid) or issuing a second per-page lookup
            through the entity service. Neither is worth it for this tool, so results
            are keyed by IdKey only.

            Reason: Report rows expose Id but not Guid; returning Guid is not free.
        */
        var rows = output.Rows
            .Skip( pageIndex * ReportPageSize )
            .Take( ReportPageSize )
            .Select( row => new ReportItemResult { Id = row.Id, Values = ShapeValues( row.Values ) } )
            .ToList();

        var hasMore = ( ( long ) pageNumber * ReportPageSize ) < totalRowCount;

        var page = new PaginatedResult<ReportItemResult>
        {
            Items = rows,
            PageNumber = pageNumber,
            PageSize = ReportPageSize,
            ReturnedItemCount = rows.Count,
            HasMoreItems = hasMore
        };

        // No history. Report rows are re-fetchable and can be large.
        return helper.GetPaginatedResult( page )
            .WithoutHistoryContent();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Shapes a row's raw values for output: DateTime values become an unambiguous
    /// organization-time round-trip string; every other value passes through as-is
    /// (native value types stay typed, strings and masked values stay strings).
    /// </summary>
    /// <param name="rawValues">The row's raw values keyed by column header.</param>
    /// <returns>The shaped values keyed by the same headers.</returns>
    private static Dictionary<string, object> ShapeValues( Dictionary<string, object> rawValues )
    {
        var values = new Dictionary<string, object>( rawValues.Count );

        foreach ( var kvp in rawValues )
        {
            if ( kvp.Value is DateTime dateTime )
            {
                // Emit dates as an unambiguous organization-time round-trip string.
                values[kvp.Key] = FormatOrganizationDateTime( dateTime );
            }
            else
            {
                values[kvp.Key] = kvp.Value;
            }
        }

        return values;
    }

    /// <summary>
    /// Formats a report DateTime value as an unambiguous round-trip string in the
    /// organization's time zone.
    /// </summary>
    /// <remarks>
    /// Report DateTime values are stored in organization time with an unspecified
    /// kind. This attaches the organization's UTC offset for that date (honoring
    /// daylight saving) and emits the round-trip ("o") form, e.g.
    /// <c>2026-08-31T13:45:00.0000000-05:00</c>, so the value carries its own time
    /// zone rather than relying on the reader to assume one.
    /// </remarks>
    /// <param name="value">The DateTime value from the report.</param>
    /// <returns>The round-trip organization-time string.</returns>
    private static string FormatOrganizationDateTime( DateTime value )
    {
        // ToRockDateTimeOffset treats the value as organization time and attaches the
        // organization's UTC offset (DST-aware); "o" gives an unambiguous round-trip.
        return value.ToRockDateTimeOffset().ToString( "o", CultureInfo.InvariantCulture );
    }

    #endregion
}
