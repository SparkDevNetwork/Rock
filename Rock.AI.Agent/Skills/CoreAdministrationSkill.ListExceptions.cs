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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Constants

    /// <summary>
    /// How many days of exceptions to summarize when the caller supplies no date
    /// range. One week, which is the common "what is breaking lately" question.
    /// </summary>
    private const int DefaultExceptionRangeDays = 7;

    /// <summary>
    /// The widest date range the exception tools will search. Exception volume is
    /// high on a busy instance, so an unbounded range is how a single call scans
    /// the whole table.
    /// </summary>
    private const int MaxExceptionRangeDays = 30;

    #endregion

    #region Tool(s)

    /// <summary>
    /// Lists exceptions grouped by the error they represent.
    /// </summary>
    /// <remarks>
    /// This mirrors Rock's own Exception List: only outermost exceptions are
    /// counted, and those sharing an exception type and the first 95 characters of
    /// their description are grouped into one row with a count. The grouping is
    /// done in memory, following the core block, because a description-prefix
    /// grouping does not translate cleanly to SQL.
    /// </remarks>
    [Description( "Lists logged exceptions grouped by the error they represent, with a count of how many times each occurred, over a date range." )]
    [AgentPurpose( "Finds what has been failing recently and how often." )]
    [AgentUsage( "The date range defaults to the last 7 days and cannot exceed 30 days. Use ListExceptionInstances to see the individual occurrences of one group." )]
    [AgentToolGuid( "B7783D9B-E062-46A9-A47F-14D741F7B868" )]
    public AgentToolResult ListExceptions(
        DateTime? startDateTime = null,
        DateTime? endDateTime = null,
        string partialExceptionType = null,
        string siteIdKey = null,
        string pageIdKey = null,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        if ( !TryResolveExceptionDateRange( helper, startDateTime, endDateTime, out var start, out var end ) )
        {
            return helper.ErrorResult;
        }

        var siteId = ResolveOptionalEntityId<Rock.Model.Site>( helper, siteIdKey );
        var pageId = ResolveOptionalEntityId<Rock.Model.Page>( helper, pageIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var exceptionLogService = new ExceptionLogService( AgentRequestContext.RockContext );

        var query = exceptionLogService.Queryable()
            .Where( e => e.CreatedDateTime >= start && e.CreatedDateTime < end );

        query = exceptionLogService.FilterByOutermost( query );

        if ( siteId.HasValue )
        {
            query = query.Where( e => e.SiteId == siteId.Value );
        }

        if ( pageId.HasValue )
        {
            query = query.Where( e => e.PageId == pageId.Value );
        }

        if ( partialExceptionType.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( e => e.ExceptionType.Contains( partialExceptionType ) );
        }

        // Materialize the minimal columns and group in memory. The description
        // prefix grouping does not translate cleanly to SQL, which is why the core
        // Exception List block pulls the rows down before summarizing them.
        var occurrences = query
            .Select( e => new
            {
                e.Id,
                e.Guid,
                e.ExceptionType,
                e.Description,
                e.CreatedDateTime
            } )
            .ToList();

        var groups = occurrences
            .GroupBy( e => new
            {
                e.ExceptionType,
                DescriptionPrefix = ( e.Description ?? string.Empty ).Truncate( ExceptionLogService.DescriptionGroupingPrefixLength, false )
            } )
            .Select( g =>
            {
                var mostRecent = g.OrderByDescending( e => e.CreatedDateTime ).First();

                return new ExceptionSummaryResult
                {
                    ExceptionType = g.Key.ExceptionType,
                    Description = g.Key.DescriptionPrefix,
                    Count = g.Count(),
                    FirstOccurredDateTime = g.Min( e => e.CreatedDateTime ),
                    LastOccurredDateTime = g.Max( e => e.CreatedDateTime ),
                    SampleException = new KeyNameResult { Id = mostRecent.Id, Guid = mostRecent.Guid, Name = g.Key.ExceptionType }
                };
            } )
            .OrderByDescending( s => s.LastOccurredDateTime )
            .ThenBy( s => s.ExceptionType )
            .AsQueryable();

        var page = helper.GetPaginatedItems( groups, pageNumber );

        // No history. The grouped summary is reference data the agent can re-fetch,
        // and it is expected to be large on a busy instance.
        return helper.GetPaginatedResult( page )
            .WithoutHistoryContent();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resolves the date range shared by the exception tools, applying the default
    /// window and enforcing the maximum span. Any problem is reported as an error
    /// on <paramref name="helper"/>.
    /// </summary>
    /// <param name="helper">The tool helper errors are accumulated on.</param>
    /// <param name="startDateTime">The requested start, or <c>null</c> for the default.</param>
    /// <param name="endDateTime">The requested end, or <c>null</c> for now.</param>
    /// <param name="start">On success, the resolved start.</param>
    /// <param name="end">On success, the resolved end.</param>
    /// <returns><c>true</c> when the range is valid; otherwise <c>false</c>.</returns>
    private bool TryResolveExceptionDateRange( AgentToolHelper helper, DateTime? startDateTime, DateTime? endDateTime, out DateTime start, out DateTime end )
    {
        end = endDateTime ?? RockDateTime.Now;
        start = startDateTime ?? end.AddDays( -DefaultExceptionRangeDays );

        if ( start >= end )
        {
            helper.AddError( "startDateTime must be before endDateTime." );
            return false;
        }

        if ( ( end - start ).TotalDays > MaxExceptionRangeDays )
        {
            helper.AddError( $"The date range cannot exceed {MaxExceptionRangeDays} days. Narrow startDateTime and endDateTime." );
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resolves an optional entity IdKey to its integer Id, recording an error when
    /// a value is supplied but does not resolve.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to resolve.</typeparam>
    /// <param name="helper">The tool helper errors are accumulated on.</param>
    /// <param name="idKey">The IdKey to resolve, or <c>null</c> to skip.</param>
    /// <returns>The resolved Id, or <c>null</c> when no value was supplied.</returns>
    private int? ResolveOptionalEntityId<TEntity>( AgentToolHelper helper, string idKey )
        where TEntity : class, Rock.Data.IEntity, new()
    {
        if ( idKey.IsNullOrWhiteSpace() )
        {
            return null;
        }

        var entity = helper.GetOptionalEntity<TEntity>( idKey, checkSecurity: false );

        return entity?.Id;
    }

    #endregion
}
