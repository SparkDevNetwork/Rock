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
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

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
        8/18/2026 - CLAUDE

        This tool is the skill's only create path for applications. The
        endpoint upsert used to create the containing application implicitly,
        which made applicationName a parameter whose meaning depended on
        hidden state and turned a misspelled applicationSlug into a silently
        created phantom application. The established parent-child shape
        (AddOrUpdateContentChannelItem) requires the parent to exist, so
        application creation moved here and AddOrUpdateLavaEndpoint now
        errors when the application is missing.

        Updates are keyed by IdKey rather than slug so a typo cannot upsert a
        second application, and the slug itself is not updatable: it is the
        application's address, baked into every component's useLavaApp
        binding, so renaming it silently breaks pages.

        Reason: One create path for applications, keyed updates, no implicit
        creation side effects.
    */
    [Description( "Adds a new Lava application or updates one this skill created. Applications group a block's endpoints and must exist before endpoints can be added." )]
    [AgentToolPreamble( "Saving the Lava application." )]
    [AgentUsage( "Create one application per block, named after the dashboard, then pass its slug to every AddOrUpdateLavaEndpoint call so security is rigged once for the whole block." )]
    [AgentUsage( "The slug cannot be changed after creation; it is the address every component's useLavaApp binding uses. To rename what the user sees, update the name." )]
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

            if ( application != null )
            {
                // The provenance stamp is the whole safety model: the skill
                // can only rework its own applications, never something a
                // person built through the admin pages.
                if ( application.ForeignKey != AgentProvenanceKey )
                {
                    helper.AddError( $"The '{application.Slug}' Lava application was not created by this skill, so it cannot be changed here. Ask the user to edit it through the Lava Applications admin pages." );
                }

                if ( slug.IsNotNullOrWhiteSpace() && slug != application.Slug )
                {
                    helper.AddError( $"The slug of a Lava application cannot be changed; it is the address every component's useLavaApp binding uses. Update the name instead, or create a new application." );
                }
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
                ConfigurationRiggingJson = EmptyConfigurationRigging,
                ForeignKey = AgentProvenanceKey
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

        var result = Success( CreateApplicationDetailResult( application ) )
            .WithHistoryContent( new LavaApplicationReferenceResult
            {
                Id = application.Id,
                Name = application.Name,
                ApplicationSlug = application.Slug
            }, "lava-application" )
            .WithInstructions( $"The '{application.Slug}' Lava application has been {( isAdd ? "created" : "updated" )}." );

        if ( isAdd )
        {
            result.WithInstructions( $"The '{application.Slug}' Lava application was created with no security rules and deliberately does not inherit any. Only the Rock Administrators and Lava Application Developers roles can execute its endpoints until someone grants rights on the application through the Lava Applications admin pages. Tell the user this before they test the page as a normal visitor." );

            /*
                8/28/2026 - CLAUDE

                Same delivery pattern as the composition-rules pointer on
                GetRockVersion: creating the application is the one step every
                data-backed build passes through before its first endpoint, and
                tool results always land in the client's context, so the Lava
                guidance reaches even clients that never read the seeded
                instructions.

                Reason: Mandate the endpoint-Lava article on the channel that
                survives instruction drift.
            */
            result.WithInstructions( "Before writing any endpoint template for this application, read the Writing Endpoint Lava article: call the Community Knowledge Base skill's GetArticle tool with articleKey 'coding-guide/data-and-endpoints/writing-endpoint-lava'. It governs entity commands versus sql, aggregates with groupby and count, parameters under Body and QueryString, explicit limits, and the JSON output pattern; do not author a template without it in context this session. If any endpoint writes data or returns personal data, also read 'coding-guide/data-and-endpoints/security-and-permissions'." );
        }

        return result;
    }

    #endregion
}
