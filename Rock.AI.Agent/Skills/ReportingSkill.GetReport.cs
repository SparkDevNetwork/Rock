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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.Reporting;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReportingSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single report in full detail, including its columns.
    /// </summary>
    /// <remarks>
    /// Each field reports whether it is sortable, which is what determines the
    /// columns a caller may pass to <see cref="GetReportItems"/>.
    /// </remarks>
    [Description( "Gets a single report in full detail, including its columns and which of them can be sorted on." )]
    [AgentPurpose( "Reads a report's configuration and columns before running it." )]
    [AgentToolPrerequisite( "Call ListReports to determine the reportIdKey." )]
    [AgentToolGuid( "A811A466-90B5-4ACF-B887-9DDDF8CB1145" )]
    public AgentToolResult GetReport( string reportIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var report = helper.GetRequiredEntity<Rock.Model.Report>( reportIdKey );

        if ( report == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListReports function to determine the available reports." );
        }

        var category = report.CategoryId.HasValue
            ? CategoryCache.Get( report.CategoryId.Value, rockContext )
            : null;

        var entityType = report.EntityTypeId.HasValue
            ? EntityTypeCache.Get( report.EntityTypeId.Value, rockContext )
            : null;

        var dataView = report.DataViewId.HasValue
            ? DataViewCache.Get( report.DataViewId.Value, rockContext )
            : null;

        var fields = report.ReportFields
            .OrderBy( f => f.ColumnOrder )
            .Select( f => new ReportFieldResult
            {
                Id = f.Id,
                Guid = f.Guid,
                ReportFieldType = f.ReportFieldType,
                Selection = f.Selection,
                ColumnHeaderText = f.ColumnHeaderText,
                ColumnOrder = f.ColumnOrder,
                SortOrder = f.SortOrder,
                IsSortable = ReportQueryBuilder.IsFieldSortable( f ),
                ShowInGrid = f.ShowInGrid
            } )
            .ToList();

        var result = new ReportDetailResult
        {
            Id = report.Id,
            Guid = report.Guid,
            Name = report.Name,
            Description = report.Description,
            Category = KeyNameResult.FromCache( category ),
            EntityType = KeyNameResult.FromCache( entityType ),
            DataView = KeyNameResult.FromCache( dataView ),
            FetchTop = report.FetchTop,
            Fields = fields
        };

        return Success( result )
            .WithHistoryContent( new KeyNameResult( report.Id, report.Guid, report.Name ) );
    }

    #endregion
}
