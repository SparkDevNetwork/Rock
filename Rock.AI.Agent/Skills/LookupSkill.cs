using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Kernel plugin ("skill") providing simple lookup functions for basic Rock entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are typically “bootstrap” functions that establish context for other,
    /// more detailed functions. For example, <see cref="LookupSites"/> is often called
    /// before other site-specific queries.
    /// </para>
    /// <para>
    /// This skill is intentionally small and generic — it’s a good place for simple,
    /// low-dependency lookups that multiple other skills rely on.
    /// </para>
    /// </remarks>
    [Description(
        "🎯 Purpose:\r\n" +
        "Provides simple lookup functions for common Rock entities, such as sites.\r\n\r\n" +
        "🧭 Usage Guidance:\r\n" +
        "- Use `LookupSites` to fetch available websites and populate context for other functions."
    )]
    [AgentSkillGuid( "FE392ADA-09E6-43F3-A643-9B050CAABA16" )]
    [EntityTypeGuid( "D3BB1138-EC9A-49CE-8CA8-32E240ACC201" )]
    internal sealed class LookupSkill : AgentSkillComponent
    {
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
        [Description(
            "🎯 Purpose:\r\n" +
            "Retrieves a list of all websites, mobile apps, and TV applications configured in the Rock system.\r\n\r\n" +
            "📦 Returns:\r\n" +
            "A JSON array of site objects, each containing the site ID, name, description, and site type.\r\n\r\n" +
            "🧭 Usage Guidance:\r\n" +
            "Use this function to get an overview of available sites in the Rock instance. Useful when needing to select a site for further queries."
        )]
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
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                SiteType = s.SiteType.ConvertToString( true )
            } ).ToList();

            // Store only essential properties in session context to keep it lean.
            var trimmedForContext = siteList.Select( site => new
            {
                site.Id,
                site.Name,
                site.SiteType
            } );

            await AgentRequestContext.ChatAgent.AddSessionContextAsync( "site-list", trimmedForContext.ToJson() );

            return LookupFunctionResult<SiteInfo>.Success( siteList );
        }

        /// <summary>
        /// Minimal site data structure for use with <see cref="LookupSites"/>.
        /// </summary>
        public class SiteInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string SiteType { get; set; }
        }
    }
}