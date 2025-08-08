using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Utilities;
using Rock.Data;
using Rock.SystemGuid;
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
    [AgentSkillGuid( "613D7110-6453-4BAB-892B-064222F8397C" )]
    [EntityTypeGuid( "7A63570D-6FC3-4573-BDF2-89CFF605D5AB" )]
    internal sealed class Site : AgentSkillComponent
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
        public Site( RockContext rockContext, ILogger<Site> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Functions

        /// <summary>
        /// Retrieves all websites (sites) configured in Rock.
        /// Also persists a trimmed list (Id, Name, SiteType) into the agent’s session context
        /// under the key <c>"site-list"</c> so that subsequent calls can reference it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="LookupFunctionResult{T}.Results"/> will include <c>Description</c>,
        /// but the stored session context omits it to reduce token usage.
        /// </para>
        /// <para>
        /// Returns <c>NoData</c> if there are no sites configured.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A <see cref="LookupFunctionResult{T}"/> of <see cref="SiteInfo"/> objects.
        /// </returns>
        [KernelFunction( "LookupSites" )]
        [AgentFunctionGuid( "6234BB68-99B8-4B7C-884D-0D760B1F081C" )]
        public async Task<LookupFunctionResult<SiteInfo>> LookupSites()
        {
            var sites = SiteCache.All();

            if ( !sites.Any() )
            {
                return LookupFunctionResult<SiteInfo>.NoData();
            }

            var siteList = sites.Select( s => new SiteInfo
            {
                Key = s.IdKey,
                Name = s.Name,
                Description = s.Description,
                SiteType = s.SiteType.ConvertToString( true )
            } ).ToList();

            // Store only essential properties in session context to keep it lean.
            var trimmedForContext = siteList.Select( site => new
            {
                site.Key,
                site.Name,
                site.SiteType
            } );

            await AgentRequestContext.ChatAgent.AddSessionContextAsync( "site-list", trimmedForContext.ToJson() );

            return LookupFunctionResult<SiteInfo>.Success( siteList );
        }

        #endregion

        #region DTOs

        /// <summary>
        /// Minimal site data structure for use with <see cref="LookupSites"/>.
        /// </summary>
        public class SiteInfo
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string SiteType { get; set; }
        }

        #endregion
    }
}