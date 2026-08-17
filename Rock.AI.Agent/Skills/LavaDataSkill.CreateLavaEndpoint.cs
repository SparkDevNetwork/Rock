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
using Rock.AI.Agent.Classes.Skills.LavaDataSkill;
using Rock.Cms;
using Rock.Configuration;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaDataSkill
{
    #region Fields

    /// <summary>
    /// The content type new endpoints declare when the definition does not
    /// name one. Endpoints created by this skill exist to feed authored
    /// components, so they return JSON rather than the historical default of
    /// HTML.
    /// </summary>
    private static readonly string JsonContentType = "application/json";

    /// <summary>
    /// The configuration rigging a new application starts with. This has to
    /// be valid JSON rather than left unset, because the value is parsed on
    /// every request to the application and the parser rejects null.
    /// </summary>
    private static readonly string EmptyConfigurationRigging = "{}";

    #endregion

    #region Tool(s)

    [Description( "Creates a Lava endpoint that returns data to an authored component, creating the containing Lava application first if it does not exist yet." )]
    [AgentToolPreamble( "Creating the Lava endpoint." )]
    [AgentUsage( "applicationSlug groups a block's endpoints; reuse the same slug for every endpoint of one dashboard. applicationName is only read when the application does not exist yet." )]
    [AgentUsage( "Endpoints are keyed by slug AND method, so the same slug with Get and with Post are two different endpoints." )]
    [AgentUsage( "definition.enabledLavaCommands must include every command the template uses or the template will fail at runtime. Use 'RockEntity' to read, 'RockEntityModify' to add or update, and 'RockEntityDelete' to delete. These cover almost everything, including charts and totals." )]
    [AgentUsage( "Do not request 'Sql'. It is refused unless you also pass sqlJustification, which you may only supply after telling the user why the entity commands cannot do the job and getting their explicit approval. Rewriting the template with entity commands is nearly always the correct response to that refusal." )]
    [AgentUsage( "Always pass testParameters when the template reads Body or QueryString, with realistic values, so the parameter path is proven rather than assumed. Without it the test renders with no request data and a template that reads Body.x is only exercised down its missing-parameter branch." )]
    [AgentUsage( "Enabling RockEntityModify or RockEntityDelete turns test execution off for that endpoint, because running it would perform real writes. You get no syntax check at all, so keep write endpoints small and put any read logic in a separate RockEntity-only endpoint that can still be tested." )]
    [AgentToolGuid( "9066DD4A-2158-4B1C-87E3-4058CBEE1E5C" )]
    public AgentToolResult CreateLavaEndpoint(
        [Description( "The slug of the Lava application the endpoint belongs to. Reuse one slug per dashboard so all of its endpoints group under one application." )]
        string applicationSlug,

        [Description( "The slug of the new endpoint." )]
        string endpointSlug,

        [Description( "The definition of the endpoint: its Lava template, HTTP method, security mode, enabled Lava commands and content type." )]
        LavaEndpointDefinition definition,

        [Description( "The name of the application. Only used when the application has to be created." )]
        string applicationName = null,

        [Description( "Why raw SQL is unavoidable. Required only when definition.enabledLavaCommands includes 'Sql', and only after the user explicitly approved it." )]
        string sqlJustification = null,

        [Description( "A JSON object of the values a component would send with invoke, surfaced to the test execution as the Body merge field (or QueryString for Get endpoints)." )]
        string testParameters = null,

        [Description( "How many characters of test output to return, up to 10000. Defaults to 2000." )]
        int? maxTestOutputLength = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        // Validate all of the inputs before touching the database so the
        // agent hears about every problem in one round trip.
        if ( applicationSlug.IsNullOrWhiteSpace() )
        {
            helper.AddError( "An application slug is required." );
        }

        if ( endpointSlug.IsNullOrWhiteSpace() )
        {
            helper.AddError( "An endpoint slug is required." );
        }

        var codeTemplate = definition?.CodeTemplate;
        var enabledLavaCommands = definition?.EnabledLavaCommands;

        if ( codeTemplate.IsNullOrWhiteSpace() )
        {
            helper.AddError( "A Lava template is required." );
        }
        else if ( !TryLintTemplate( codeTemplate, out var lintError ) )
        {
            helper.AddError( lintError );
        }

        if ( !TryGetSecurityMode( definition?.SecurityMode, out var endpointSecurityMode, out var securityModeError ) )
        {
            helper.AddError( securityModeError );
        }

        if ( !TryGetHttpMethod( definition?.HttpMethod, out var method, out var httpMethodError ) )
        {
            helper.AddError( httpMethodError );
        }

        // Refuse raw SQL before anything is written, so the round trip
        // through the user happens instead of an endpoint existing that has
        // to be walked back.
        if ( !TryValidateSqlUsage( enabledLavaCommands, sqlJustification, out var sqlError ) )
        {
            helper.AddError( sqlError );
        }

        if ( !TryParseTestParameters( testParameters, out var parsedTestParameters, out var testParametersError ) )
        {
            helper.AddError( testParametersError );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var applicationService = new LavaApplicationService( rockContext );
        var application = applicationService.Queryable().FirstOrDefault( a => a.Slug == applicationSlug );

        if ( !IsAuthorizedToAuthor( application ) )
        {
            helper.AddError( application == null
                ? "You are not authorized to create Lava applications."
                : $"You are not authorized to administrate the '{applicationSlug}' Lava application." );

            return helper.ErrorResult;
        }

        var isNewApplication = application == null;

        if ( isNewApplication )
        {
            if ( applicationName.IsNullOrWhiteSpace() )
            {
                helper.AddError( $"No Lava application exists with the slug '{applicationSlug}'. Provide an applicationName so it can be created." );

                return helper.ErrorResult;
            }

            /*
                8/17/2026 - CLAUDE

                ConfigurationRiggingJson has to be set to valid JSON here.
                Every request to a Lava application reads
                LavaApplicationCache.ConfigurationRigging, which parses this
                string, and the parser throws on null rather than returning
                null. Leaving the property unset therefore makes every
                endpoint on the application fail with a 500 that names
                Newtonsoft rather than anything recognizable, and it fails
                for the person who just created it.

                The Lava Application Detail block always assigns the property
                from its bag, so an application created through the admin
                pages never reaches this state. Only a caller that news up
                the entity directly can.

                Reason: An unset rigging value breaks every endpoint on the
                application.
            */
            application = new LavaApplication
            {
                Name = applicationName,
                Slug = applicationSlug,
                IsActive = true,
                ConfigurationRiggingJson = EmptyConfigurationRigging,
                ForeignKey = AgentProvenanceKey
            };

            applicationService.Add( application );

            if ( !application.IsValid )
            {
                foreach ( var validationResult in application.ValidationResults )
                {
                    helper.AddError( validationResult.ErrorMessage );
                }

                return helper.ErrorResult;
            }
        }
        else if ( application.LavaEndpoints.Any( e => e.Slug == endpointSlug && e.HttpMethod == method ) )
        {
            helper.AddError( $"An endpoint already exists at '{applicationSlug}/{endpointSlug}' for the {method} method. Use UpdateLavaEndpoint to replace its template." );

            return helper.ErrorResult;
        }

        var endpoint = new LavaEndpoint
        {
            LavaApplication = application,
            Name = endpointSlug,
            Slug = endpointSlug,
            HttpMethod = method,
            CodeTemplate = codeTemplate,
            EnabledLavaCommands = enabledLavaCommands.ToStringSafe(),
            SecurityMode = endpointSecurityMode,
            IsActive = true,
            ForeignKey = AgentProvenanceKey
        };

        // These endpoints exist to feed components, so they default to JSON.
        // Cross-site forgery protection stays on, which is what useLavaApp
        // sends the header for.
        endpoint.SetAdditionalSettings( new LavaEndpointAdditionalSettings
        {
            EnableCrossSiteForgeryProtection = true,
            ContentType = definition.ContentType.IsNotNullOrWhiteSpace() ? definition.ContentType : JsonContentType
        } );

        new LavaEndpointService( rockContext ).Add( endpoint );

        if ( !endpoint.IsValid )
        {
            foreach ( var validationResult in endpoint.ValidationResults )
            {
                helper.AddError( validationResult.ErrorMessage );
            }
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var url = GetEndpointUrl( application.Slug, endpoint.Slug );

        var result = Success( new LavaEndpointSaveResult
        {
            ApplicationSlug = application.Slug,
            EndpointSlug = endpoint.Slug,
            Method = method.ToString(),
            Url = url,
            TestExecution = TestExecute( codeTemplate, endpoint.EnabledLavaCommands, application, method, parsedTestParameters, maxTestOutputLength )
        } )
            .WithHistoryContent( new LavaEndpointReferenceResult
            {
                ApplicationSlug = application.Slug,
                EndpointSlug = endpoint.Slug,
                Method = method.ToString(),
                Url = url
            }, "lava-endpoint" );

        /*
            8/17/2026 - CLAUDE

            Two different authorization gaps can leave a brand new endpoint
            uncallable, and they need different advice, so report whichever
            one applies.

            In EndpointExecute mode the endpoint answers for itself. A new
            endpoint has no authorization rules, and while its parent
            authority is the application, the authorization walk only
            inspects explicit rules on each parent. It never invokes
            LavaApplicationCache.IsAuthorized, so that class's override for
            the Rock Administrators and Lava Application Developers roles is
            not reached. The result is that nobody can call the endpoint,
            administrators included.

            In the application modes the endpoint calls
            LavaApplication.IsAuthorized directly, which does apply the
            override, so those two roles can call it immediately and everyone
            else waits on explicit rules.

            Reason: Each mode fails differently for a new endpoint, and a
            message promising administrators access they do not have sends
            the user to the wrong fix.
        */
        if ( endpointSecurityMode == LavaEndpointSecurityMode.EndpointExecute )
        {
            result.WithInstructions( $"The '{endpoint.Slug}' endpoint uses the EndpointExecute security mode and has no authorization rules, so nobody can call it yet, administrators included. Either grant Execute on the endpoint through the Lava Applications admin pages, or recreate it with the definition's securityMode set to ApplicationView so it defers to the application. Tell the user this before they test the page, because the call will fail with a 401 rather than an error they can read." );
        }
        else if ( isNewApplication )
        {
            result.WithInstructions( $"The '{application.Slug}' Lava application was created with no security rules and deliberately does not inherit any. Only the Rock Administrators and Lava Application Developers roles can execute its endpoints until someone grants rights on the application through the Lava Applications admin pages. Tell the user this before they test the page as a normal visitor." );
        }

        // The justification was given to the tool, not to the user, so
        // require that it be repeated out loud rather than trusting it was
        // already discussed.
        if ( IsSqlRequested( endpoint.EnabledLavaCommands ) )
        {
            result.WithInstructions( GetSqlApprovalInstructions( endpoint.Slug, sqlJustification ) );
        }

        return result;
    }

    #endregion
}
