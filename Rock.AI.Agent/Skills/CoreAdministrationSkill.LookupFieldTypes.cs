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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Looks up the field types installed in Rock.
    /// </summary>
    /// <remarks>
    /// This is what prevents a caller inventing a field type class name, but not
    /// by publishing the correct spelling. It works because the returned key is
    /// the only way to name a field type in any write, so a wrong class name has
    /// nowhere to go.
    /// </remarks>
    [Description( "Looks up the field types installed in Rock, such as Text, Person, or Single Select. Use the returned key whenever a field type must be specified." )]
    [AgentPurpose( "Finds the field type to use when creating an attribute." )]
    [AgentToolGuid( "04F39FBF-A3B4-4F1F-88E7-49E1D3AE73A7" )]
    public AgentToolResult LookupFieldTypes( string partialName = null )
    {
        // No paging and no cap. The set is bounded by installed code rather than
        // by data: churches do not create field types, and only a plugin install
        // changes the count. Two fields per row keeps the whole set small.
        var fieldTypes = FieldTypeCache.All( AgentRequestContext.RockContext ).AsEnumerable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            fieldTypes = fieldTypes.Where( ft => ft.Name.ContainsIgnoreCase( partialName ) );
        }

        var results = fieldTypes
            .OrderBy( ft => ft.Name )
            .Select( ft => new FieldTypeResult
            {
                Id = ft.Id,
                Guid = ft.Guid,
                Name = ft.Name
            } )
            .ToList();

        if ( !results.Any() )
        {
            return NoData()
                .WithInstructions( $"No field type matched '{partialName}'. Call {nameof( LookupFieldTypes )} with no filter to see every field type." );
        }

        // Success does not sanitize the way GetPaginatedResult does, so an
        // unpaged collection has to be sanitized item by item. Matches
        // LookupConnectionTypesAndOpportunities.
        results.ForEach( ft => ft.Sanitize( AgentRequestContext ) );

        // Keyed history. This is the one lookup small enough to keep, and it is
        // consulted constantly.
        return Success( results )
            .WithHistoryKey( "field-types" );
    }

    #endregion
}
