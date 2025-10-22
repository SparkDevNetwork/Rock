using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves website analytics (page visits) for a specific person, optionally filtered by date and/or site.
        /// Results are grouped by site and include visited pages with visit counts.
        /// </summary>
        /// <param name="arguments">Query parameters including person id, optional site id, and optional start/end dates.</param>
        /// <remarks>
        /// Requires <see cref="LookupSites"/> to have been called previously to populate
        /// session context <c>"site-list"</c> for guardrail enforcement.
        /// Defaults the date range to [now - 1 year, now] if neither start nor end are provided.
        /// </remarks>
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
        public RockToolResult SummarizePageVisitsForPerson( string personIdKey, DateTime? startDate = null, DateTime? endDate = null, string siteIdKey = "", int pageNumber = 1 )
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
                return RockToolResult.Error( errors );
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
                    return RockToolResult.NoData()
                        .WithMetadata( meta );
                }

                return RockToolResult.Success( rows )
                    .WithMetadata( meta )
                    .WithoutHistoryContent();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "SummarizePageVisitsForPerson failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
                return RockToolResult.Error( "Failed to retrieve site analytics. " + ex.Message );
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
}
