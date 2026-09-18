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
//
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaApplicationBuilderSkill
{
    #region Fields

    /// <summary>
    /// The configuration rigging a new application starts with. This has to
    /// be valid JSON rather than left unset, because the value is parsed on
    /// every request to the application and the parser rejects null.
    /// </summary>
    private static readonly string EmptyConfigurationRigging = "{}";

    #endregion

    #region Tool(s)

    /*
        9/15/2026 - CLAUDE

        This tool no longer takes an audiences parameter or writes Auth rows.
        Who may execute an application's endpoints is authored with the Core
        Administration skill's authorization tools (AddOrUpdateAuthorizationForEntity
        on the application for ExecuteView, and on an EndpointExecute endpoint
        for Execute), which both Code Composer agents carry. The result names
        the identifiers those tools take and states plainly that a new
        application answers 401 to every real visitor until the rules exist,
        which is the failure the earlier audience parameter was added to
        prevent.

        Reason: One authorization surface for every authoring skill.
    */

    [Description( "Adds a new Lava application or updates an existing one the current person can administrate. Applications group a block's endpoints and must exist before endpoints can be added. Who may call the endpoints is configured separately with the authorization tools." )]
    [AgentToolPreamble( "Saving the Lava application." )]
    [AgentUsage( "Create one application per block, named after the feature, then pass its slug to every AddOrUpdateLavaEndpoint call so security is configured once for the whole block." )]
    [AgentUsage( "The slug cannot be changed after creation; it is the address every component's useLavaApp binding uses. To rename what the user sees, update the name." )]
    [AgentUsage( "A new application has no authorization rules, so its endpoints work for Rock Administrators and Lava Application Developers and return 401 to everyone else. Secure it immediately with the Core Administration skill: call AddOrUpdateAuthorizationForEntity with the returned entityTypeIdKey and idKey for action ExecuteView, allowing each intended role (groupIdKey, from ResolveAudience) in order and then denying specialRole AllUsers. Allow specialRole AllUsers only when the user explicitly wants anonymous visitors to read the data." )]
    [AgentToolGuid( "26C5F1A8-3D94-4E67-90B2-7A45D8E1C6F3" )]
    public AgentToolResult AddOrUpdateLavaApplication(
        [Description( "Required when editing an existing Lava application. Do not provide when adding a new one." )]
        string lavaApplicationIdKey = null,

        [Description( "The kebab-case slug the application is addressed by, such as 'serving-dashboard'. Required when adding; cannot be changed when updating." )]
        string slug = null,

        [Description( "The name of the application. Required when adding." )]
        SetOrClear<string> name = null,

        [Description( "What the application is for." )]
        SetOrClear<string> description = null,

        [Description( "Whether the application and its endpoints can be called." )]
        bool? isActive = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var isAdd = lavaApplicationIdKey.IsNullOrWhiteSpace();
        var applicationService = new LavaApplicationService( rockContext );
        LavaApplication application = null;

        if ( name?.ClearValue == true )
        {
            helper.AddError( "The name of a Lava application cannot be cleared." );
        }

        if ( isAdd )
        {
            if ( slug.IsNullOrWhiteSpace() || slug.Contains( " " ) )
            {
                helper.AddError( "A kebab-case slug with no spaces is required when adding a Lava application." );
            }
            else if ( applicationService.Queryable().Any( a => a.Slug == slug ) )
            {
                helper.AddError( $"A Lava application already exists with the slug '{slug}'. Read it with {nameof( GetLavaApplication )}, or choose a different slug." );
            }

            if ( name?.Value.IsNullOrWhiteSpace() != false )
            {
                helper.AddError( "A name is required when adding a Lava application." );
            }
        }
        else
        {
            application = helper.GetRequiredEntity<LavaApplication>( lavaApplicationIdKey, checkSecurity: false );

            if ( application != null && slug.IsNotNullOrWhiteSpace() && slug != application.Slug )
            {
                helper.AddError( $"The slug of a Lava application cannot be changed; it is the address every component's useLavaApp binding uses. Update the name instead, or create a new application." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( !IsAuthorizedToAuthor( application ) )
        {
            helper.AddError( isAdd
                ? "You are not authorized to create Lava applications."
                : $"You are not authorized to administrate the '{application.Slug}' Lava application." );

            return helper.ErrorResult;
        }

        if ( isAdd )
        {
            /*
                8/18/2026 - CLAUDE

                ConfigurationRiggingJson has to be set to valid JSON here.
                Every request to a Lava application reads
                LavaApplicationCache.ConfigurationRigging, which parses this
                string, and the parser throws on null rather than returning
                null. Leaving the property unset therefore makes every
                endpoint on the application fail with a 500 that names
                Newtonsoft rather than anything recognizable, and it fails
                for the person who just created it.

                Reason: An unset rigging value breaks every endpoint on the
                application.
            */
            application = new LavaApplication
            {
                Name = name.Value,
                Slug = slug,
                IsActive = isActive ?? true,
                ConfigurationRiggingJson = EmptyConfigurationRigging
            };

            applicationService.Add( application );
        }
        else
        {
            helper.UpdateProperty( application, a => a.Name, name );
            helper.UpdateProperty( application, a => a.IsActive, isActive );
        }

        helper.UpdateProperty( application, a => a.Description, description );

        if ( !application.IsValid )
        {
            foreach ( var validationResult in application.ValidationResults )
            {
                helper.AddError( validationResult.ErrorMessage );
            }
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var detail = CreateApplicationDetailResult( rockContext, application );

        var result = Success( detail )
            .WithHistoryContent( new LavaApplicationReferenceResult
            {
                Id = application.Id,
                Name = application.Name,
                ApplicationSlug = application.Slug
            }, "lava-application" )
            .WithInstructions( $"The '{application.Slug}' Lava application has been {( isAdd ? "created" : "updated" )}." );

        if ( isAdd )
        {
            result.WithInstructions( $"Secure the application now, before writing endpoints. It has no authorization rules, so its endpoints work for Rock Administrators and Lava Application Developers and return 401 to every other visitor. Using the Core Administration skill, call AddOrUpdateAuthorizationForEntity with entityTypeIdKey '{detail.EntityTypeIdKey}', entityIdKey '{detail.IdKey}', and action ExecuteView: allow each role the page is for (groupIdKey, from ResolveAudience) in order, then deny specialRole AllUsers. Allow specialRole AllUsers only when the user explicitly wants anonymous visitors to read the data. ExecuteView governs endpoints in ApplicationView mode; do not grant ExecuteEdit or ExecuteAdministrate unless the user asks for write endpoints to be callable beyond those two roles. Confirm with ListAuthorizationForEntity." );

            /*
                8/28/2026 - CLAUDE

                Same delivery pattern as the Coding Guide pointer on
                GetRockVersion: creating the application is the one step every
                data-backed build passes through before its first endpoint, and
                tool results always land in the client's context, so the Lava
                guidance reaches even clients that never read the seeded
                instructions.

                Reason: Mandate the Coding Guide routing on the channel that
                survives instruction drift.
            */
            result.WithInstructions( "Before writing an endpoint template for this application, follow the coding guide route the Community Knowledge Base skill's GetKnowledgeBaseOverview result points you to, and retrieve every article and source lookup it assigns for the endpoint outcome. SearchKnowledge is not authoritative evidence for exact entity property names. Never construct or guess a topic or article key." );
        }
        else
        {
            result.WithInstructions( $"Authorization rules were not changed. Read them with the Core Administration skill's ListAuthorizationForEntity (entityTypeIdKey '{detail.EntityTypeIdKey}', entityIdKey '{detail.IdKey}') and change them with AddOrUpdateAuthorizationForEntity only when the user asked for a security change." );
        }

        return result;
    }

    #endregion
}
