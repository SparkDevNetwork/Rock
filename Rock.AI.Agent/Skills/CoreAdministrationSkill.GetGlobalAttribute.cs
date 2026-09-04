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
    /// Gets a single global attribute in full detail, including its current value.
    /// </summary>
    /// <remarks>
    /// This is where a caller obtains a global attribute's Guid and current value.
    /// The list tool returns identity only so it is not repeated on every row.
    /// </remarks>
    [Description( "Gets a single global attribute in full detail, including its definition and current value." )]
    [AgentPurpose( "Retrieves one global attribute's configuration and current organization-wide value." )]
    [AgentToolPrerequisite( "Call ListGlobalAttributes to determine the globalAttributeIdKey." )]
    [AgentToolGuid( "06587DF8-4857-4F50-91F7-60C5D0B32969" )]
    public AgentToolResult GetGlobalAttribute( string globalAttributeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var attribute = helper.GetRequiredEntity<Rock.Model.Attribute>( globalAttributeIdKey, checkSecurity: false );

        if ( attribute == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListGlobalAttributes )} function to determine the available global attributes." );
        }

        // Only an attribute with no owning entity type is a global attribute. Any
        // other attribute is reached through the entity it belongs to.
        if ( attribute.EntityTypeId.HasValue )
        {
            return Error( "That attribute is not a global attribute." )
                .WithInstructions( $"Call the {nameof( ListGlobalAttributes )} function to determine the available global attributes." );
        }

        // Global attributes are not secured per attribute, so no view authorization
        // check is applied here.
        var attributeCache = AttributeCache.Get( attribute.Id, AgentRequestContext.RockContext );

        if ( attributeCache == null )
        {
            return Error( "The global attribute could not be loaded." );
        }

        var result = new GlobalAttributeDetailResult
        {
            Id = attributeCache.Id,
            Guid = attributeCache.Guid,
            Key = attributeCache.Key,
            Name = attributeCache.Name,
            Description = attributeCache.Description.IsNullOrWhiteSpace() ? null : attributeCache.Description,
            FieldType = KeyNameResult.FromCache( attributeCache.FieldType ),
            IsRequired = attributeCache.IsRequired,
            IsSystem = attributeCache.IsSystem,
            DefaultValue = attributeCache.DefaultValue,
            Value = GlobalAttributesCache.Get().GetValue( attributeCache.Key, AgentRequestContext.RockContext ),
            Categories = attributeCache.Categories
                .Select( c => KeyNameResult.FromCache( c ) )
                .ToList()
        };

        return Success( result )
            .WithHistoryContent( KeyNameResult.FromCache( attributeCache ) );
    }

    #endregion
}
