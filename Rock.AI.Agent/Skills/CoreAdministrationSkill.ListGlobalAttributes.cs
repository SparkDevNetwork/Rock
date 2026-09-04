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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the global attributes configured in Rock.
    /// </summary>
    /// <remarks>
    /// A List rather than a Lookup because churches add global attributes freely,
    /// so the set grows with data. It returns identity only; the field type,
    /// description, categories, and current value belong to
    /// <see cref="GetGlobalAttribute"/>.
    /// </remarks>
    [Description( "Lists the global attributes configured in Rock. A global attribute is an organization-wide setting, such as Organization Name or Public Application Root, referenced from Lava and code by its key." )]
    [AgentPurpose( "Finds a global attribute so its current value or configuration can be retrieved." )]
    [AgentToolGuid( "CEAEE758-6B78-4B50-87AF-EEA9E98887A6" )]
    public AgentToolResult ListGlobalAttributes( string partialName = null, string categoryIdKey = null, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        // Read from the global attributes cache. Global attributes are not secured
        // per attribute, so there is nothing to filter by view authorization here.
        var globalAttributes = GlobalAttributesCache.Get().Attributes
            .AsEnumerable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            globalAttributes = globalAttributes
                .Where( a => a.Name.ContainsIgnoreCase( partialName ) || a.Key.ContainsIgnoreCase( partialName ) );
        }

        if ( categoryIdKey.IsNotNullOrWhiteSpace() )
        {
            var category = helper.GetOptionalEntity<Rock.Model.Category>( categoryIdKey );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( category != null )
            {
                globalAttributes = globalAttributes.Where( a => a.Categories.Any( c => c.Id == category.Id ) );
            }
        }

        var orderedAttributes = globalAttributes
            .OrderBy( a => a.Name )
            .ThenBy( a => a.Id )
            .Select( a => new GlobalAttributeResult
            {
                Id = a.Id,
                Guid = a.Guid,
                Key = a.Key,
                Name = a.Name
            } );

        var page = helper.GetPaginatedItems( orderedAttributes, pageNumber );

        var historyPage = page.WithItems( page.Items
            .Select( a => new KeyNameResult { Id = a.Id, Guid = a.Guid, Name = a.Name } ) );

        return helper.GetPaginatedResult( page, historyPage );
    }

    #endregion
}
