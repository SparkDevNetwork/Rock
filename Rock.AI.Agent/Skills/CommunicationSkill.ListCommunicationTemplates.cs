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
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the communication templates configured in Rock.
    /// </summary>
    /// <remarks>
    /// Communication templates are secured per row through their category and
    /// there is no cache to page over, so this uses cursor paging with per row
    /// authorization, the same as other secured database lists.
    /// </remarks>
    [Description( "Lists the communication templates configured in Rock. A template is the reusable starting point (layout, styling, and default content) for authoring a communication." )]
    [AgentPurpose( "Finds the communication template to start a communication from." )]
    [AgentToolGuid( "E61411E3-1557-44AD-99D5-C13FBB52703B" )]
    public AgentToolResult ListCommunicationTemplates( string partialName = null, string categoryIdKey = null, string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new CommunicationTemplateService( AgentRequestContext.RockContext ).Queryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( t => t.Name.Contains( partialName ) );
        }

        query = helper.WhereOptionalIdKey( query, t => t.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The ThenBy on Id is a required tiebreaker so identically named
        // templates produce distinct, stable cursors.
        var paginator = new CursorPaginator<CommunicationTemplate>( currentPerson, qry => qry
            .OrderBy( t => t.Name )
            .ThenBy( t => t.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( t => new CommunicationTemplateResult
            {
                Id = t.Id,
                Guid = t.Guid,
                Name = t.Name,
                IsActive = t.IsActive,
                Category = t.Category != null
                    ? new KeyNameResult { Id = t.Category.Id, Guid = t.Category.Guid, Name = t.Category.Name }
                    : null
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items
            .Select( t => new KeyNameResult { Id = t.Id, Guid = t.Guid, Name = t.Name } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
