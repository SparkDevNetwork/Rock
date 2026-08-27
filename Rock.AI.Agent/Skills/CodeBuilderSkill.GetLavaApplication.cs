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
using Rock.AI.Agent.Classes.Skills.CodeBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CodeBuilderSkill
{
    #region Tool(s)

    [Description( "Reads a Lava application and lists its endpoints so existing work can be discovered before adding more." )]
    [AgentToolPreamble( "Reading the Lava application." )]
    [AgentUsage( "Call before adding endpoints to an application you did not create in this conversation, so an existing endpoint is updated rather than duplicated. Read a listed endpoint's template with GetLavaEndpoint." )]
    [AgentToolGuid( "9A078C57-946C-4D5F-8EBE-5009E6390EF2" )]
    public AgentToolResult GetLavaApplication(
        [Description( "The slug of the Lava application to read." )]
        string applicationSlug )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( applicationSlug.IsNullOrWhiteSpace() )
        {
            helper.AddError( "An application slug is required." );

            return helper.ErrorResult;
        }

        var application = new LavaApplicationService( rockContext )
            .Queryable()
            .FirstOrDefault( a => a.Slug == applicationSlug );

        if ( application == null )
        {
            helper.AddError( $"No Lava application exists with the slug '{applicationSlug}'. Create it with {nameof( AddOrUpdateLavaApplication )}." );

            return helper.ErrorResult;
        }

        if ( !IsAuthorizedToAuthor( application ) )
        {
            helper.AddError( $"You are not authorized to administrate the '{applicationSlug}' Lava application." );

            return helper.ErrorResult;
        }

        return Success( CreateApplicationDetailResult( application ) )
            .WithHistoryContent( new LavaApplicationReferenceResult
            {
                Id = application.Id,
                Name = application.Name,
                ApplicationSlug = application.Slug
            }, "lava-application" );
    }

    #endregion
}
