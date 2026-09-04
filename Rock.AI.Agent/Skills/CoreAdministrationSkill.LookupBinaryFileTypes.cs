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
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Looks up the binary file types configured in Rock.
    /// </summary>
    /// <remarks>
    /// A Lookup rather than a List because the set is bounded by configuration, not
    /// by data: binary file types are created by an administrator setting up a
    /// storage area, so the count stays small and the whole set is returned whole.
    /// </remarks>
    [Description( "Looks up the binary file types configured in Rock, such as Default, Image, or Person Image. Use the returned key or guid whenever a binary file type must be specified." )]
    [AgentPurpose( "Finds the binary file type to use when configuring something that stores an uploaded file." )]
    [AgentToolGuid( "150F9017-EA86-42A8-8DFE-FB2254CC397C" )]
    public AgentToolResult LookupBinaryFileTypes( string partialName = null )
    {
        // No paging and no cap. The set is bounded by configuration, and a
        // materialized cache collection can be security filtered in full.
        var binaryFileTypes = BinaryFileTypeCache.All( AgentRequestContext.RockContext )
            .Where( bft => bft.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .AsEnumerable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            binaryFileTypes = binaryFileTypes.Where( bft => bft.Name.ContainsIgnoreCase( partialName ) );
        }

        var results = binaryFileTypes
            .OrderBy( bft => bft.Name )
            .Select( bft => new BinaryFileTypeResult
            {
                Id = bft.Id,
                Guid = bft.Guid,
                Name = bft.Name,
                Description = bft.Description.IsNullOrWhiteSpace() ? null : bft.Description
            } )
            .ToList();

        if ( !results.Any() )
        {
            return NoData()
                .WithInstructions( $"No binary file type matched '{partialName}'. Call {nameof( LookupBinaryFileTypes )} with no filter to see every binary file type." );
        }

        return Success( results )
            .WithHistoryKey( "binary-file-types" );
    }

    #endregion
}
