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
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReportingSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single data view in full detail, including a summary of its filters.
    /// </summary>
    /// <remarks>
    /// The raw filter tree is large and nested and is not returned. Instead a
    /// human-readable description is built from the filter, using the same
    /// component formatting Rock's own screens use.
    /// </remarks>
    [Description( "Gets a single data view in full detail, including a human-readable summary of its filters." )]
    [AgentPurpose( "Reads what a data view does before running it." )]
    [AgentToolPrerequisite( "Call ListDataViews to determine the dataViewIdKey." )]
    [AgentToolGuid( "61E0F59C-9885-4224-8D0D-7E24BD71E3D2" )]
    public AgentToolResult GetDataView( string dataViewIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var dataView = helper.GetRequiredEntity<Rock.Model.DataView>( dataViewIdKey );

        if ( dataView == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListDataViews function to determine the available data views." );
        }

        var category = dataView.CategoryId.HasValue
            ? CategoryCache.Get( dataView.CategoryId.Value, rockContext )
            : null;

        var entityType = dataView.EntityTypeId.HasValue
            ? EntityTypeCache.Get( dataView.EntityTypeId.Value, rockContext )
            : null;

        var transformEntityType = dataView.TransformEntityTypeId.HasValue
            ? EntityTypeCache.Get( dataView.TransformEntityTypeId.Value, rockContext )
            : null;

        var isPersisted = dataView.PersistedScheduleIntervalMinutes.HasValue || dataView.PersistedScheduleId.HasValue;

        var result = new DataViewDetailResult
        {
            Id = dataView.Id,
            Guid = dataView.Guid,
            Name = dataView.Name,
            Description = dataView.Description,
            Category = KeyNameResult.FromCache( category ),
            EntityType = KeyNameResult.FromCache( entityType ),
            IsPersisted = isPersisted,
            PersistedScheduleIntervalMinutes = dataView.PersistedScheduleIntervalMinutes,
            TransformEntityType = KeyNameResult.FromCache( transformEntityType ),
            IncludeDeceased = dataView.IncludeDeceased,
            FilterDescription = GetFilterDescription( dataView, rockContext )
        };

        return Success( result )
            .WithHistoryContent( new KeyNameResult( dataView.Id, dataView.Guid, dataView.Name ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Builds a human-readable summary of a data view's filters.
    /// </summary>
    /// <param name="dataView">The data view.</param>
    /// <param name="rockContext">The context used to resolve the entity type.</param>
    /// <returns>The description, or <c>null</c> if it cannot be built.</returns>
    private string GetFilterDescription( Rock.Model.DataView dataView, Rock.Data.RockContext rockContext )
    {
        if ( dataView.DataViewFilter == null || !dataView.EntityTypeId.HasValue )
        {
            return null;
        }

        var filteredType = EntityTypeCache.Get( dataView.EntityTypeId.Value, rockContext )?.GetEntityType();

        if ( filteredType == null )
        {
            return null;
        }

        try
        {
            var description = dataView.DataViewFilter.ToString( filteredType );

            return description.IsNullOrWhiteSpace() ? null : description;
        }
        catch
        {
            // Intentionally ignored: a filter component that cannot format its
            // selection should not fail the whole read. The rest of the detail is
            // still useful, and the caller can run the view to see what it selects.
            return null;
        }
    }

    #endregion
}
