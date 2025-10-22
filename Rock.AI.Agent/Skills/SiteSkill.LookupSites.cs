using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.SiteSkill;
using Rock.SystemGuid;

using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class SiteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves all websites (sites) configured in Rock.
        /// Also persists a trimmed list (Id, Name, SiteType) into the agent’s session context
        /// under the key <c>"site-list"</c> so that subsequent calls can reference it.
        /// </summary>
        [Description( "Retrieves all configured websites in Rock." )]
        [AgentToolGuid( "6234BB68-99B8-4B7C-884D-0D760B1F081C" )]
        public RockToolResult LookupSites()
        {
            var sites = SiteCache.All( AgentRequestContext.RockContext );

            if ( !sites.Any() )
            {
                return RockToolResult.NoData();
            }

            // If the agent is running in an internal context (e.g., staff user), include inactive sites.
            var isInternal = AgentRequestContext.AudienceType == Enums.AI.Agent.AudienceType.Internal;
            sites = sites.Where( s => isInternal || s.IsActive ).ToList();

            var siteList = sites.Select( s => new SiteResult
            {
                IdKey = s.IdKey,
                Name = s.Name,
                Description = s.Description,
                SiteType = s.SiteType.ConvertToString( true ),
                ExternalUrl = s.ExternalUrl
            } ).ToList();

            // Store only essential properties in session context to keep it lean.
            var trimmedForHistory = siteList.Select( site => new
            {
                site.IdKey,
                site.Name,
                site.SiteType,
            } );

            return RockToolResult.Success( siteList )
                .WithHistoryContent( trimmedForHistory, "site-list" );
        }

        #endregion
    }
}
