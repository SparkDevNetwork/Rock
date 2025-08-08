using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient; // If you're on Microsoft.Data.SqlClient, swap the namespace.
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Utilities;
using Rock.Data;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>

    [Description(
        "🎯 Purpose:\r\n" +
        "Provides site-related insights and utilities in Rock RMS, with an emphasis on person-centric data " +
        "such as activity, analytics, and contextual information. Intended as a central skill for retrieving " +
        "and working with site data across multiple contexts.\r\n\r\n" +
        "🧭 Usage Guidance:\r\n" +
        "- Use this skill to discover available sites, retrieve analytics, and perform other site-level lookups.\r\n" +
        "- Many functions will accept optional filters such as site ID, date ranges, or person identifiers.\r\n" +
        "- Functions are designed to support summarization, reporting, and downstream processing by other skills.\r\n\r\n" +
        "🛡 Guardrails:\r\n" +
        "1. Follow each function’s specific guardrails for context requirements (e.g., call `LookupSites` before analytics).\r\n" +
        "2. Avoid redundant or excessive calls when aggregation or filtering can be applied."
    )]
    [AgentSkillGuid( "DD5FA7DD-3277-4C31-848D-285CD67AC7CA" )]
    [EntityTypeGuid( "12E7BDEA-B67A-48D7-8D1E-245BF8E9B555" )]
    internal sealed class Person : AgentSkillComponent
    {
        #region Fields

        private readonly RockContext _rockContext;
        private readonly ILogger<Site> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Site"/> class.
        /// </summary>
        /// <param name="rockContext">Rock data context used for database access.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public Person( RockContext rockContext, ILogger<Site> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Functions

        // BC: This is really not needed at this time, we may further
        // want to customize the output so at that time we can
        // uncomment this function and customize it as needed.
        //public override IReadOnlyCollection<AgentFunction> GetSemanticFunctions()
        //{
        //    return new List<AgentFunction>
        //    {
        //        new AgentFunction
        //        {
        //            FunctionType = FunctionType.AIPrompt,
        //            EnableLavaPreRendering = false,
        //            Temperature = 0.7,
        //            UsageHint = "Summarizes insights in a human-friendly way, given the output from LookupSiteAnalytics. " +
        //                        "To use this function, first call a function that returns data (such as LookupSiteAnalytics), then pass its result as the insightData parameter.",
        //            Prompt = "Here is a list of web sessions and pages a user visited:\n\n{{ $insightData }}\n\nSummarize the user's web activity. Highlight repeated pages or long sessions, and call out any interesting patterns.",
        //            Name = "SummarizeAnalytics",
        //            Guid = new Guid("97FDE306-E415-40FE-A548-72D300234470"),
        //        }
        //    };
        //}

        /// <summary>
        /// Retrieves website analytics (page visits) for a specific person, optionally filtered by date and/or site.
        /// Results are grouped by site and include visited pages with visit counts.
        /// </summary>
        /// <param name="options">Query parameters including person id, optional site id, and optional start/end dates.</param>
        /// <returns>
        /// A <see cref="LookupFunctionResult{T}"/> where <c>T</c> is <see cref="SitePageVisitData"/>.
        /// The <see cref="LookupFunctionResult{T}.Status"/> will be:
        /// <list type="bullet">
        /// <item><description><c>Success</c> if rows are returned.</description></item>
        /// <item><description><c>NoData</c> if the query succeeds but returns no rows.</description></item>
        /// <item><description><c>Error</c> if validation fails or an exception occurs.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Requires <see cref="LookupSites"/> to have been called previously to populate
        /// session context <c>"site-list"</c> for guardrail enforcement.
        /// Defaults the date range to [now - 1 year, now] if neither start nor end are provided.
        /// </remarks>
        [KernelFunction]
        [Description(
            "🎯 Purpose:\r\n" +
            "Retrieves page visits for a specific person, optionally filtered by date and/or site. \r\n" +
            "Results are grouped by site and include the site type (web, mobile, tv), visited pages and visit counts.\r\n" +
            "To maintain performance, the results are paginated (and the 'PageNumber' parameter is required. \r\n\r\n" +
            "🛡️ Guardrails:\r\n" +
            "1. This function depends on context set by `LookupSites`. Ensure it has been called first to set the site list.\r\n" +
            "2. Do not call this function multiple times per site, unless necessary. It supports all-site aggregation when `siteId` is null."
        )]
        [AgentFunctionGuid( "EFDBC338-CC1C-46D2-A7F6-7AE5081147AE" )]
        public LookupFunctionResult<SitePageVisitData> ListPageVisitsForPerson( ListSiteVisitsParameters options )
        {
            var errors = new List<string>();
            if ( options == null )
            {
                return LookupFunctionResult<SitePageVisitData>.Error( "Options are required." );
            }

            var start = options.StartDate;
            var end = options.EndDate;

            var personId = IdHasher.Instance.GetId( options.PersonKey );
            if ( !personId.HasValue || personId <= 0 )
            {
                errors.Add( "There was an invalid key provided for the person." );
            }

            var siteId = IdHasher.Instance.GetId( options.SiteKey );
            if ( siteId.HasValue && siteId.Value <= 0 )
            {
                errors.Add( "Invalid site ID. Provide a value greater than zero." );
            }

            if ( start.HasValue && end.HasValue && start > end )
            {
                errors.Add( "Invalid date range. Start date cannot be after end date." );
            }

            // Defaults: past year → now
            if ( !start.HasValue && !end.HasValue )
            {
                end = RockDateTime.Now;
                start = end.Value.AddYears( -1 );
            }
            else if ( start.HasValue && !end.HasValue )
            {
                end = RockDateTime.Now;
            }

            // Paging
            var pageNumber = Math.Max( 1, options.PageNumber );
            var basePageSize = 100;
            var offset = ( pageNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            if ( errors.Count > 0 )
            {
                return LookupFunctionResult<SitePageVisitData>.Error( string.Join( " ", errors ) );
            }

            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@SiteId", siteId),
                    GetParameterValueOrDbNull("@StartDate", start),
                    GetParameterValueOrDbNull("@EndDate", end),
                    new SqlParameter("@PageSize",  take),    // request N+1
                    new SqlParameter("@OffsetRows", offset), // offset uses base size
                };

                var rows = _rockContext.Database
                    .SqlQuery<PageVisitGroup>( _websiteDataSql, parameters.ToArray() )
                    .ToList();

                var hasMore = rows.Count > basePageSize;
                if ( hasMore )
                {
                    rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
                }

                var meta = new Dictionary<string, object>
                {
                    { "personKey", options.PersonKey },
                    { "startDate", start },
                    { "endDate", end },
                    { "pageNumber", pageNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", rows.Count },
                    { "hasMore", hasMore }
                };

                if ( !rows.Any() )
                {
                    return LookupFunctionResult<SitePageVisitData>.NoData( meta: meta );
                }

                // Group by site
                var groupedBySite = rows
                    .GroupBy( r => r.SiteId )
                    .Select( g => new SitePageVisitData
                    {
                        SiteKey = IdHasher.Instance.GetHash( g.Key ),
                        SiteName = SiteCache.Get( g.Key )?.Name ?? "(Unknown)",
                        SiteType = SiteCache.Get( g.Key )?.SiteType.ConvertToString( true ) ?? "Unknown",
                        PageVisits = g.Select( v => new PageVisitGroup
                        {
                            PageName = v.PageName,
                            VisitCount = v.VisitCount,
                            LastVisit = v.LastVisit
                        } ).ToList()
                    } )
                    .ToList();

                return LookupFunctionResult<SitePageVisitData>.Success( groupedBySite, meta: meta );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "LookupSiteAnalytics failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
                return LookupFunctionResult<SitePageVisitData>.Error( "Failed to retrieve site analytics. " + ex.Message );
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Creates a SQL parameter with the specified key and value, substituting <see cref="DBNull.Value"/> when the value is <c>null</c>.
        /// </summary>
        /// <param name="key">The parameter name (e.g., <c>@SiteId</c>).</param>
        /// <param name="value">The parameter value, or <c>null</c> to emit <see cref="DBNull.Value"/>.</param>
        /// <returns>A <see cref="SqlParameter"/> instance.</returns>
        private static SqlParameter GetParameterValueOrDbNull( string key, object value )
            => new SqlParameter( key, value ?? ( object ) DBNull.Value );

        #endregion

        #region DTOs

        public class ListSiteVisitsParameters
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [Description( "Optional. The ID Key of site to analyze." )]
            public string SiteKey { get; set; }

            // Required: person key
            public string PersonKey { get; set; }

            [Description( "Optional. The page number to return, starting at 1. Defaults to 1." )]
            public int PageNumber { get; set; } = 1; // 1-based
        }

        public class SitePageVisitData
        {
            public string SiteKey { get; set; }
            public string SiteName { get; set; }
            public string SiteType { get; set; } // e.g., Web, Mobile, TV
            public List<PageVisitGroup> PageVisits { get; set; } = new List<PageVisitGroup>();
        }

        public class PageVisitGroup
        {
            public int SiteId { get; set; }
            public string PageName { get; set; }
            public int VisitCount { get; set; }
            public DateTime? LastVisit { get; set; }
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