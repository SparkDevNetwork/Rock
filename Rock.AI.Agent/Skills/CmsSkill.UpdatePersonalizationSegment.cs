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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Configuration;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Updates an existing personalization segment's metadata.
    /// </summary>
    /// <remarks>
    /// This updates the segment's name, description, active state, and refresh
    /// schedule only. It does not create segments, and it does not change the
    /// segment's audience: neither the linked data view nor the additional in-line
    /// filter is editable here. Those are managed through Rock's personalization
    /// screens.
    /// </remarks>
    [Description( "Updates an existing personalization segment's name, description, active state, and refresh schedule. It does not create segments or change the segment's audience filters." )]
    [AgentToolPreamble( "Saving the personalization segment." )]
    [AgentUsage( "Pass only the properties to change. Segments cannot be created here, and their audience (data view and filters) is not editable through this tool." )]
    [AgentToolPrerequisite( "Call LookupPersonalizationSegments to determine the personalizationSegmentIdKey." )]
    [AgentToolGuid( "82C5C66C-2DDA-4E17-A4E4-383A4B4DFACA" )]
    public AgentToolResult UpdatePersonalizationSegment(
        string personalizationSegmentIdKey,
        string name = null,
        SetOrClear<string> description = null,
        bool? isActive = null,
        [Description( "How often, in minutes, to refresh the segment's persisted membership. Clear to leave membership non-persisted." )]
        SetOrClear<int> persistedScheduleIntervalMinutes = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var segment = helper.GetRequiredEntity<Model.PersonalizationSegment>( personalizationSegmentIdKey, checkSecurity: false );

        if ( segment == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupPersonalizationSegments function to determine the available segments." );
        }

        helper.UpdateProperty( segment, s => s.Name, name );
        helper.UpdateProperty( segment, s => s.Description, description );
        helper.UpdateProperty( segment, s => s.IsActive, isActive );
        helper.UpdateProperty( segment, s => s.PersistedScheduleIntervalMinutes, persistedScheduleIntervalMinutes );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( !segment.IsValid )
        {
            helper.AddError( segment.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The personalization segment could not be saved." );

            return helper.ErrorResult;
        }

        // Saving is enough to refresh the cache. PersonalizationSegment is
        // ICacheable, and the context updates those entries as part of the save.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var filterDataView = segment.FilterDataViewId.HasValue
            ? DataViewCache.Get( segment.FilterDataViewId.Value, rockContext )
            : null;

        var result = new PersonalizationSegmentDetailResult
        {
            Id = segment.Id,
            Guid = segment.Guid,
            Name = segment.Name,
            SegmentKey = segment.SegmentKey,
            Description = segment.Description,
            IsActive = segment.IsActive,
            FilterDataView = KeyNameResult.FromCache( filterDataView ),
            HasAdditionalFilter = segment.AdditionalFilterJson.IsNotNullOrWhiteSpace(),
            IsPersisted = segment.PersistedScheduleIntervalMinutes.HasValue || segment.PersistedScheduleId.HasValue,
            PersistedScheduleIntervalMinutes = segment.PersistedScheduleIntervalMinutes
        };

        return Success( result )
            .WithInstructions( "The personalization segment has been updated." )
            .WithHistoryContent( new KeyNameResult( segment.Id, segment.Guid, segment.Name ) );
    }

    #endregion
}
