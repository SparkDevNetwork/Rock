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
using System.Data.SqlClient;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Lists page visits for a specific person." )]
    [AgentPurpose( "Retrieves page visits for a specific person, optionally filtered by date and/or site." )]
    [AgentPurpose( "Results include the site type (web, mobile, tv), visited pages and visit counts." )]
    [AgentUsage( "The results are paginated (and the 'PageNumber' parameter is required.)" )]
    [AgentUsage( "Do not call this function multiple times per site, unless necessary. It supports all-site aggregation when `siteId` is null." )]
    [AgentToolPrerequisite( "This function depends on context set by `LookupSites`. Ensure it has been called first to set the site list." )]
    [AgentToolExample( "has Ted Decker been active on any of our mobile applications in the last 2 years" )]
    [AgentToolExample( "has Alisha Marble visted the giving page in the past 30 days" )]
    [AgentToolExample( "has Pete been active on our platform?" )]
    [AgentToolGuid( "EFDBC338-CC1C-46D2-A7F6-7AE5081147AE" )]
    public AgentToolResult GetPageVisitSummaryForPerson(
        string personIdKey,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string siteIdKey = "",
        int pageNumber = 1 )
    {
        var errors = new List<string>();


        var personId = IdHasher.Instance.GetId( personIdKey );
        if ( !personId.HasValue || personId <= 0 )
        {
            errors.Add( "There was an invalid key provided for the person." );
        }

        var siteId = IdHasher.Instance.GetId( siteIdKey );
        if ( siteId.HasValue && siteId.Value <= 0 )
        {
            errors.Add( "There was an invalid site key provided." );
        }

        if ( startDate.HasValue && endDate.HasValue && startDate > endDate )
        {
            errors.Add( "Invalid date range. Start date cannot be after end date." );
        }


        if ( errors.Count > 0 )
        {
            return Error( errors );
        }

        // Defaults: past year → now (safety in case nothing was provided)
        if ( !startDate.HasValue && !endDate.HasValue )
        {
            endDate = RockDateTime.Now;
            startDate = endDate.Value.AddYears( -1 );
        }
        else if ( startDate.HasValue && !endDate.HasValue )
        {
            endDate = RockDateTime.Now;
        }

        // Paging
        var pgNumber = Math.Max( 1, pageNumber );
        var basePageSize = 100;
        var offset = ( pgNumber - 1 ) * basePageSize;
        var take = basePageSize + 1; // N+1 to compute hasMore

        try
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@PersonId", personId),
                GetParameterValueOrDbNull("@SiteId", siteId),
                GetParameterValueOrDbNull("@StartDate", startDate),
                GetParameterValueOrDbNull("@EndDate", endDate),
                new SqlParameter("@PageSize",  take),    // request N+1
                new SqlParameter("@OffsetRows", offset), // offset uses base size
            };

            var rows = AgentRequestContext.RockContext.Database
                .SqlQuery<PageVisitResult>( _websiteDataSql, parameters.ToArray() )
                .ToList();

            // Populate IdKey for each row
            rows.ForEach( r =>
            {
                var site = SiteCache.Get( r.SiteId );
                r.SiteIdKey = site?.IdKey;
            } );

            var hasMore = rows.Count > basePageSize;
            if ( hasMore )
            {
                rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
            }

            var meta = new Dictionary<string, object>
            {
                { "personKey", personIdKey },
                { "startDate", startDate },
                { "endDate", endDate },
                { "pageNumber", pgNumber },
                { "pageSize", basePageSize },
                { "returnedRows", rows.Count },
                { "hasMore", hasMore }
            };

            if ( !rows.Any() )
            {
                return NoData()
                    .WithMetadata( meta );
            }

            return Success( rows )
                .WithMetadata( meta )
                .WithoutHistoryContent();
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "SummarizePageVisitsForPerson failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
            return Error( "Failed to retrieve site analytics. " + ex.Message );
        }
    }

    #endregion

    #region SQL

    private const string _websiteDataSql = @"
;WITH Filtered AS (
    SELECT
        PageName = COALESCE(NULLIF(LTRIM(RTRIM(i.InteractionSummary)), ''), '(Unknown)'),
        i.InteractionDateTime,
        ich.ChannelEntityId AS SiteId
    FROM dbo.Interaction i
    INNER JOIN dbo.InteractionSession   AS inse  ON inse.Id  = i.InteractionSessionId
    INNER JOIN dbo.InteractionComponent AS icomp ON icomp.Id = i.InteractionComponentId
    INNER JOIN dbo.InteractionChannel   AS ich   ON ich.Id   = icomp.InteractionChannelId
    INNER JOIN dbo.PersonAlias          AS pa    ON pa.Id    = i.PersonAliasId
    WHERE (@SiteId IS NULL OR ich.ChannelEntityId = @SiteId)
      AND i.Operation = 'View'
      AND pa.PersonId = @PersonId
      AND (@StartDate IS NULL OR i.InteractionDateTime >= @StartDate)
      AND (@EndDate   IS NULL OR i.InteractionDateTime <= @EndDate)
),
Grouped AS (
    SELECT
        f.SiteId,
        f.PageName,
        VisitCount = COUNT(*),
        LastVisit  = MAX(f.InteractionDateTime)
    FROM Filtered f
    GROUP BY f.SiteId, f.PageName
)
SELECT
    g.SiteId,
    g.PageName,
    g.VisitCount,
    g.LastVisit
FROM Grouped g
ORDER BY
    g.LastVisit DESC,
    g.SiteId,
    g.PageName
OFFSET @OffsetRows ROWS
FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);";

    #endregion
}
