using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PrayerSkill
    {
        #region Tool(s)

        [Description( "Provides a list of prayer categories." )]
        [AgentToolGuid( "4E4A5AC6-85DC-4773-A03D-9BC1722366FD" )]
        public RockToolResult LookupPrayerCategories()
        {
            var queryable = GetPrayerCategoriesQueryable( AgentRequestContext.RockContext );

            var prayerCategories = queryable
                .Select( pc => new CategoryResult
                {
                    Id = pc.Id,
                    Description = pc.Description,
                    Name = pc.Name,
                } )
                .ToList();

            // Lose the description for history content.
            var trimmedCategories = prayerCategories.Select( pc => new KeyNameResult
            {
                Id = pc.Id,
                Name = pc.Name,
            } ).ToList();

            return RockToolResult.Success( prayerCategories )
                .WithHistoryContent( trimmedCategories );
        }

        #endregion
    }
}
