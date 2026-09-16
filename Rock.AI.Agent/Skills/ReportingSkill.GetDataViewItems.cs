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
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.Data;
using Rock.Reporting;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReportingSkill
{
    #region Tool(s)

    /// <summary>
    /// Runs a data view and returns the records it selects.
    /// </summary>
    /// <remarks>
    /// Results are identity only; the fields of each record are read through that
    /// record's own skill. The optional entityIdKeys restricts the run to a set of
    /// records, intersected with the data view's own filters in SQL. Only the data
    /// view's own view permission is enforced, matching Rock's data view results
    /// screen, which applies no per-row filter.
    /// </remarks>
    [Description( "Runs a data view and returns the records it selects, optionally restricted to a given set of records." )]
    [AgentPurpose( "Gets the set of records a saved data view selects, such as the people in a segment." )]
    [AgentUsage( "Pass entityIdKeys to restrict the run to specific records; the data view's filters still apply." )]
    [AgentToolPrerequisite( "Call ListDataViews to determine the dataViewIdKey." )]
    [AgentToolGuid( "1D97B2CD-7B22-4673-9112-99BF74050D0B" )]
    public AgentToolResult GetDataViewItems( string dataViewIdKey, List<string> entityIdKeys = null, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        // GetRequiredEntity enforces view permission on the data view itself.
        var dataView = helper.GetRequiredEntity<Rock.Model.DataView>( dataViewIdKey );

        if ( dataView == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListDataViews function to determine the available data views." );
        }

        var dataViewCache = DataViewCache.Get( dataView.Id, rockContext );

        if ( dataViewCache == null )
        {
            return Error( "The data view could not be loaded." );
        }

        IQueryable<IEntity> query;

        try
        {
            query = dataViewCache.GetQuery( new GetQueryableOptions
            {
                DbContext = rockContext,
                DatabaseTimeoutSeconds = 180,
                IsQueryTaggingDisabled = IsQueryTaggingDisabled
            } );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to run data view {DataViewId} for the agent.", dataView.Id );

            return Error( "The data view could not be run." );
        }

        // GetQuery can return null rather than throwing (see the TagWith note in
        // DataViewQueryBuilder). Guard it here so a null query surfaces as a graceful
        // error instead of a null reference on the query operations below.
        if ( query == null )
        {
            _logger.LogError( "Data view {DataViewId} produced a null query for the agent.", dataView.Id );

            return Error( "The data view could not be run." );
        }

        if ( entityIdKeys != null && entityIdKeys.Any() )
        {
            var ids = new HashSet<int>( entityIdKeys
                .Select( k => IdHasher.Instance.GetId( k ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value ) );

            // Intersected in SQL with the data view's own filters.
            query = query.Where( e => ids.Contains( e.Id ) );
        }

        // Order by Id so paging is stable; Skip/Take then run in SQL.
        var orderedQuery = query.OrderBy( e => e.Id );

        var page = helper.GetPaginatedItems( orderedQuery, pageNumber );

        var resultPage = page.WithItems( page.Items
            .Select( e => new DataViewItemResult { Id = e.Id, Guid = e.Guid, Name = e.ToString() } )
            .ToList() );

        var historyPage = page.WithItems( page.Items
            .Select( e => new KeyNameResult { Id = e.Id, Guid = e.Guid, Name = e.ToString() } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
