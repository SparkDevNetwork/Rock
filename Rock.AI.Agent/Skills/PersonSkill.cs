using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient; // If you're on Microsoft.Data.SqlClient, swap the namespace.
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.Core.AI.Agent;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides **person-centric data lookup and analytics functions** in Rock.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Discovering context data for a person (e.g., site list, profile-related info)</description></item>
    ///   <item><description>Retrieving detailed analytics (e.g., web page visits, grouped by site)</description></item>
    ///   <item><description>Supporting summarization or downstream processing by other skills</description></item>
    /// </list>
    /// </remarks>

    [Description(
        "🎯 Purpose:\r\n" +
        "Provides data insights for individuals in the Rock system, such as site activity, financial analytics, etc.\r\n\r\n" +
        "🧭 Usage Guidance:\r\n" +
        "- Use `LookupSites` to fetch available websites and populate context for other functions.\r\n"
    )]
    [AgentSkillGuid( "613D7110-6453-4BAB-892B-064222F8397C" )]
    [EntityTypeGuid( "7A63570D-6FC3-4573-BDF2-89CFF605D5AB" )]
    internal sealed class PersonSkill : AgentSkillComponent
    {
        #region Fields

        private readonly RockContext _rockContext;
        private readonly ILogger<PersonSkill> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSkill"/> class.
        /// </summary>
        /// <param name="rockContext">Rock data context used for database access.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public PersonSkill( RockContext rockContext, ILogger<PersonSkill> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Functions

        /// <inheritdoc />
        public override IReadOnlyCollection<AgentFunction> GetSemanticFunctions()
        {
            return new List<AgentFunction>
            {
                new AgentFunction
                {
                    FunctionType = FunctionType.AIPrompt,
                    EnableLavaPreRendering = false,
                    Temperature = 0.7,
                    UsageHint = "Summarizes insights in a human-friendly way, given the output from LookupSiteAnalytics. " +
                                "To use this function, first call a function that returns data (such as LookupSiteAnalytics), then pass its result as the insightData parameter.",
                    Prompt = "Here is a list of web sessions and pages a user visited:\n\n{{ $insightData }}\n\nSummarize the user's web activity. Highlight repeated pages or long sessions, and call out any interesting patterns.",
                    Name = "SummarizeAnalytics",
                    Guid = new Guid("97FDE306-E415-40FE-A548-72D300234470"),
                }
            };
        }

        /// <summary>
        /// Retrieves website analytics (page visits) for a specific person, optionally filtered by date and/or site.
        /// Results are grouped by site and include visited pages with visit counts.
        /// </summary>
        /// <param name="options">Query parameters including person id, optional site id, and optional start/end dates.</param>
        /// <returns>
        /// A <see cref="LookupFunctionResult{T}"/> where <c>T</c> is <see cref="WebsiteSessionInsight"/>.
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
        [KernelFunction( "LookupSiteAnalytics" )]
        [Description(
            "🎯 Purpose:\r\n" +
            "Retrieves website analytics (page visits) for a specific person, optionally filtered by date and/or site. " +
            "Results are grouped by site and include visited pages with visit counts.\r\n\r\n" +
            "📦 Returns:\r\n" +
            "A JSON array where each item includes the site ID, name, and a list of pages the person visited, including visit counts.\r\n\r\n" +
            "🧭 Usage Guidance:\r\n" +
            "- Set only `personId` to get data across all sites.\r\n" +
            "- Set `siteId` to get data for one site only.\r\n" +
            "- Use `startDate` and `endDate` to define a date range. Defaults to the past year if omitted.\r\n\r\n" +
            "- You can use the site analytics function to check if a specific page (like \"Giving Page\") was visited by a person, by filtering the PagesVisited results.\r\n" +
            "🛡️ Guardrails:\r\n" +
            "1. If no data is returned, do not retry the function with other person IDs. No data simply means no relevant activity was found.\r\n" +
            "2. CRITICAL: This function depends on context set by `LookupSites`. Ensure it has been called first to set the site list.\r\n\r\n" +
            "🛑 Do not call this function multiple times per site, unless necessary. It supports all-site aggregation when `siteId` is null."
        )]
        [AgentFunctionGuid( "EFDBC338-CC1C-46D2-A7F6-7AE5081147AE" )]
        public LookupFunctionResult<WebsiteSessionInsight> LookupSiteAnalytics( LookupSiteAnalyticsParameters options )
        {
            var errors = new List<string>();

            if ( options == null )
            {
                errors.Add( "Options are required." );
                return LookupFunctionResult<WebsiteSessionInsight>.Error( string.Join( " ", errors ) );
            }

            var siteId = options.SiteId;
            var start = options.StartDate;
            var end = options.EndDate;
            var personId = IdHasher.Instance.GetId( options.PersonKey );

            if ( !personId.HasValue || personId <= 0 )
            {
                errors.Add( "Invalid person ID. Provide a value greater than zero." );
            }

            if ( siteId.HasValue && siteId.Value <= 0 )
            {
                errors.Add( "Invalid site ID. Provide a value greater than zero." );
            }

            if ( start.HasValue && end.HasValue && start > end )
            {
                errors.Add( "Invalid date range. Start date cannot be after end date." );
            }

            // Default: past year up to now if neither provided
            if ( !start.HasValue && !end.HasValue )
            {
                end = RockDateTime.Now;
                start = end.Value.AddYears( -1 );
            }
            else if ( start.HasValue && !end.HasValue )
            {
                end = RockDateTime.Now;
            }
            // else: only end provided → allow open start (SQL handles NULL)

            if ( errors.Count > 0 )
            {
                return LookupFunctionResult<WebsiteSessionInsight>.Error( string.Join( " ", errors ) );
            }

            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@SiteId", siteId),
                    GetParameterValueOrDbNull("@StartDate", start),
                    GetParameterValueOrDbNull("@EndDate", end)
                };

                var results = _rockContext.Database
                    .SqlQuery<WebsiteSessionInsight>( _websiteDataSql, parameters.ToArray() )
                    .ToList();

                var meta = new Dictionary<string, object>
                {
                    { "personId", personId },
                    { "siteId", siteId },
                    { "startDate", start },
                    { "endDate", end }
                };

                return LookupFunctionResult<WebsiteSessionInsight>.Success( results, meta: meta );
            }
            catch ( Exception ex )
            {
                // Surface a friendly message; log the full exception for diagnostics
                _logger.LogError( ex, "LookupSiteAnalytics failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
                return LookupFunctionResult<WebsiteSessionInsight>.Error( "Failed to retrieve site analytics. " + ex.Message );
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

        /// <summary>
        /// Parameter object for <see cref="LookupSiteAnalytics(LookupSiteAnalyticsParameters)"/>.
        /// </summary>
        public class LookupSiteAnalyticsParameters
        {
            /// <summary>
            /// Optional. The start date of the window to analyze (inclusive). Null to ignore.
            /// </summary>
            [Description( "Optional. The start date of the window to analyze (inclusive). Null to ignore." )]
            public DateTime? StartDate { get; set; }

            /// <summary>
            /// Optional. The end date of the window to analyze (inclusive). Null to ignore.
            /// </summary>
            [Description( "Optional. The end date of the window to analyze (inclusive). Null to ignore." )]
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// Optional. The ID of the site to analyze. Null for all sites.
            /// </summary>
            [Description( "Optional. The ID of site to analyze." )]
            public int? SiteId { get; set; }

            /// <summary>
            /// Required. The person ID for whom to fetch website session insights.
            /// </summary>
            [Description( "The person ID Key for whom to fetch website session insights." )]
            public string PersonKey { get; set; }
        }

        /// <summary>
        /// Represents a grouped snapshot of website sessions/visits for a site.
        /// </summary>
        public class WebsiteSessionInsight
        {
            /// <summary>
            /// The unique identifier of the site.
            /// </summary>
            public int SiteId { get; set; }

            /// <summary>
            /// The display name of the site.
            /// </summary>
            public string SiteName { get; set; }

            /// <summary>
            /// JSON payload containing pages visited and visit counts for the person within the specified window.
            /// </summary>
            public string SessionSnapshot { get; set; }
        }

        #endregion

        #region SQL

        private const string _websiteDataSql = @"
-- Get interaction channels tied to a site and ""Website"" medium
WITH InteractionChannels AS (
    SELECT
        ich.Id AS ChannelId,
        ich.ChannelEntityId AS SiteId
    FROM [InteractionChannel] ich
    INNER JOIN [DefinedValue] m ON m.Id = ich.ChannelTypeMediumValueId
    WHERE (@SiteId IS NULL OR ich.ChannelEntityId = @SiteId)
      AND m.Guid = 'e503e77d-cf35-e09f-41a2-b213184f48e8' -- Website
)
SELECT
    s.Id AS SiteId,
    s.Name AS SiteName,
    (
        SELECT
            i.InteractionSummary AS PageName,
            COUNT(*) AS VisitCount
        FROM Interaction i
        INNER JOIN InteractionComponent ic ON ic.Id = i.InteractionComponentId
        INNER JOIN InteractionChannel ichInner ON ichInner.Id = ic.InteractionChannelId
        INNER JOIN PersonAlias pa ON pa.Id = i.PersonAliasId
        WHERE ichInner.ChannelEntityId = s.Id
          AND pa.PersonId = @PersonId
          AND (@StartDate IS NULL OR i.InteractionDateTime >= @StartDate)
          AND (@EndDate IS NULL OR i.InteractionDateTime <= @EndDate)
        GROUP BY i.InteractionSummary
        FOR JSON PATH
    ) AS SessionSnapshot
FROM InteractionChannels ch
INNER JOIN Site s ON s.Id = ch.SiteId
WHERE EXISTS (
    SELECT 1
    FROM Interaction i
    INNER JOIN InteractionComponent ic ON ic.Id = i.InteractionComponentId
    INNER JOIN InteractionChannel ichInner ON ichInner.Id = ic.InteractionChannelId
    INNER JOIN PersonAlias pa ON pa.Id = i.PersonAliasId
    WHERE ichInner.ChannelEntityId = s.Id
      AND pa.PersonId = @PersonId
      AND (@StartDate IS NULL OR i.InteractionDateTime >= @StartDate)
      AND (@EndDate IS NULL OR i.InteractionDateTime <= @EndDate)
);";

        #endregion
    }
}