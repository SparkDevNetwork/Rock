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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single personalization segment in full detail.
    /// </summary>
    /// <remarks>
    /// The additional in-line filter is reported as present or absent rather than
    /// returned; it is a filter tree edited through Rock's personalization screens.
    /// </remarks>
    [Description( "Gets a single personalization segment in full detail, including the data view that defines its audience." )]
    [AgentPurpose( "Retrieves how a personalization segment is defined." )]
    [AgentToolPrerequisite( "Call LookupPersonalizationSegments to determine the personalizationSegmentIdKey." )]
    [AgentToolGuid( "6FC6793C-E92E-4484-830F-3C81D2F28B38" )]
    public AgentToolResult GetPersonalizationSegment( string personalizationSegmentIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var segment = helper.GetRequiredEntity<Model.PersonalizationSegment>( personalizationSegmentIdKey, checkSecurity: false );

        if ( segment == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupPersonalizationSegments function to determine the available segments." );
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
            .WithHistoryContent( new KeyNameResult( segment.Id, segment.Guid, segment.Name ) );
    }

    #endregion
}
