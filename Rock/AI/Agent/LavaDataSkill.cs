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

using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.Cms;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    /*
        8/3/2026 - CLAUDE

        Companion to ObsidianVibeCodingSkill. An authored Obsidian Content component
        needs data, and hunting for an existing REST endpoint is the worst-shaped step
        in that flow: Rock has hundreds of endpoints, almost none return the shape a
        specific dashboard wants, and their permissions are separate from the page's.
        Writing Lava avoids all three, so this skill creates the endpoint instead of
        searching for one.

        The tools mirror PageBuilderSkill for class structure, result helpers, and
        authorization: they are structural, privileged changes, so each one is gated on
        ADMINISTRATE of the target Lava Application. A brand new application has no
        authority to check (LavaApplication deliberately breaks security inheritance),
        so creating one requires membership in the roles LavaApplication itself treats
        as overrides.

        Every write test-executes the template and returns the result. That is the
        point of the skill: the agent finds out the template is broken while it can
        still fix it, rather than a visitor seeing Lava error text later.

        Reason: MCP-driven Lava endpoint authoring that feeds the vibe-coding flow.
    */

    /// <summary>
    /// Agent skill that creates and edits <see cref="Model.LavaEndpoint"/> records so an
    /// authored Obsidian Content component has a data source shaped for exactly what it
    /// renders.
    /// </summary>
    [Description( "Create and edit Lava endpoints that return JSON data to authored components." )]
    [AgentSkillName( "LavaData" )]
    [AgentPurpose( "Create the data endpoints an authored Obsidian Content component calls, by writing Lava rather than searching for an existing REST endpoint." )]
    [AgentUsage( "When an authored component needs data, create a Lava endpoint with CreateLavaEndpoint. Do not search for an existing Rock REST endpoint first; write the Lava that returns exactly the JSON the component renders." )]
    [AgentUsage( "Group all of a block's endpoints under one application, named after the dashboard. Pass the same applicationSlug each time and the application is reused." )]
    [AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp', bind the application once with useLavaApp('application-slug'), then call lavaApp.invoke('endpoint-slug'). Do not hand-roll the URL, the CSRF header, or the JSON parsing." )]
    [AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
    [AgentUsage( "Values sent by invoke arrive in the template under the 'Body' merge field for Post endpoints and 'QueryString' for Get, never as bare merge fields. Read '{{ Body.teamId }}', not '{{ teamId }}'. A bare parameter renders as empty with no error, so a query built from one silently returns wrong data." )]
    [AgentUsage( "The endpoint runs as whoever views the page, not as you. Write the template for the least-privileged viewer, and remember that a newly created application has no security rules until an administrator adds them." )]
    [AgentUsage( "A deleteentity command only deletes the one entity. Child rows whose foreign key does not cascade will block it, and the failure surfaces as a foreign key error rather than anything the user can act on. Check how Rock's own code deletes that entity and remove the same children first." )]
    [AgentUsage( "These tools change site configuration and can run privileged Lava. Confirm the application name, endpoint slug, and enabled Lava commands with the user before creating." )]
    [AgentUsage( "Use entity commands for everything. Reading is '{% connectionrequest where:'...' %}' with the 'RockEntity' command, adding and updating is '{% modifyconnectionrequest %}' with 'RockEntityModify', and deleting is '{% deleteconnectionrequest %}' with 'RockEntityDelete'. Substitute the entity's friendly name with the spaces removed for any other entity." )]
    [AgentUsage( "Charts and totals do not justify raw SQL. Fetch rows with the entity command and group them in Lava with assign and increment, or return the rows and aggregate them in the component. Reach for a wider entity query before reaching for SQL." )]
    [AgentGuardrail( "Never enable the 'Sql' Lava command without first explaining to the user why the entity commands cannot do the job and receiving their explicit approval. The tool rejects a request for 'Sql' that arrives without a sqlJustification." )]
    [AgentGuardrail( "Raw SQL bypasses Rock's per-row security. An endpoint runs as whoever views the page, and '{% sql %}' returns every row the query matches regardless of that person's rights, while the entity commands filter results by them automatically. Treat SQL as a last resort that needs the user's informed consent, not a convenience." )]
    [Rock.SystemGuid.EntityTypeGuid( "CABB72CF-DD09-48CD-9BB9-4819488BC7CA" )]
    [Rock.SystemGuid.AgentSkillGuid( "8660E7C0-1101-4058-BAF5-20B860600027" )]
    internal class LavaDataSkill : AgentSkillComponent
    {
        #region Constants

        /// <summary>
        /// The version segment of the Lava application route. It is part of the route
        /// itself and is not related to the application being addressed.
        /// </summary>
        private const string RouteVersion = "1";

        /// <summary>
        /// Endpoints created by this skill exist to feed authored components, so they
        /// return JSON rather than the historical default of HTML.
        /// </summary>
        private const string JsonContentType = "application/json";

        /// <summary>
        /// The configuration rigging a new application starts with. This has to be valid
        /// JSON rather than left unset, because the value is parsed on every request to the
        /// application and the parser rejects null.
        /// </summary>
        private const string EmptyConfigurationRigging = "{}";

        /// <summary>
        /// The longest test execution output handed back to the agent. A template that
        /// returns a whole dashboard payload can produce hundreds of kilobytes, which is far
        /// more than is needed to tell whether the template worked and is enough to overflow
        /// the tool result on its own.
        /// </summary>
        private const int MaxTestOutputLength = 2000;

        /// <summary>
        /// The hard ceiling on test execution output, reachable by passing
        /// maxTestOutputLength. High enough for a diagnostic dump, low enough that a
        /// dashboard payload cannot flood the tool result.
        /// </summary>
        private const int MaxAllowedTestOutputLength = 10000;

        /// <summary>
        /// The ForeignKey value stamped on applications and endpoints this skill creates.
        /// The delete tools only accept records carrying it, so the skill can clean up
        /// after itself without being able to delete anything a person authored.
        /// </summary>
        private const string AgentProvenanceKey = "AI-Agent:LavaDataSkill";

        /// <summary>
        /// The permission key of the raw SQL Lava command, as returned by
        /// <c>SqlBlock.RequiredPermissionKey</c>. This is the command that requires the
        /// user's approval before an endpoint may use it.
        /// </summary>
        private const string SqlCommandName = "Sql";

        /// <summary>
        /// The Lava command that lets a template add or update entities.
        /// </summary>
        private const string RockEntityModifyCommandName = "RockEntityModify";

        /// <summary>
        /// The Lava command that lets a template delete entities.
        /// </summary>
        private const string RockEntityDeleteCommandName = "RockEntityDelete";

        /*
            8/3/2026 - CLAUDE

            An earlier version of this skill only advised against SQL, in a single line of
            AgentUsage text that said to add it "when the query genuinely needs it". That
            failed in practice: asked for a dashboard with charts and a delete action, an
            agent chose SQL for two of three endpoints, and it neither asked the user nor
            explained itself. Two causes, both fixed here.

            The guidance never named the entity write commands, so for a delete or an update
            the agent had no alternative to offer itself. And "genuinely needs it" is a
            judgment the model resolves in favor of whatever is easiest, which for a chart
            rollup is a GROUP BY.

            Advice alone cannot fix that, so this is enforced. The tool now refuses a request
            for the Sql command unless the caller also passes a justification, which forces a
            round trip through the user. The message below is the whole intervention: it has
            to name the alternative commands and answer the aggregation excuse, or the agent
            will just retry with a justification that repeats the excuse.

            Reason: Advisory text did not stop the agent from silently choosing raw SQL.
        */
        private const string SqlRequiresApprovalMessage = @"This endpoint requests the 'Sql' Lava command, which needs the user's explicit approval before it can be created.

Raw SQL bypasses Rock's per-row security. The endpoint runs as whoever views the page, and '{% sql %}' returns every row the query matches regardless of that person's rights. The entity commands filter results by the viewer automatically, so a mistake in SQL leaks data to every visitor who can call the endpoint.

Do this with entity commands instead. Replace 'connectionrequest' with the entity's friendly name with the spaces removed:
  Read: {% connectionrequest where:'ConnectionStatusId == 3' %} ... {% endconnectionrequest %}   requires the 'RockEntity' command
  Add or update: {% modifyconnectionrequest id:'5' %} ... {% endmodifyconnectionrequest %}   requires the 'RockEntityModify' command
  Delete: {% deleteconnectionrequest id:'5' %}   requires the 'RockEntityDelete' command

Charts, counts and totals do not require SQL. Fetch the rows with the entity command and group them in Lava, or return the rows and aggregate them in the component. A join you cannot express directly is usually a nested entity command or a wider query that you filter afterward.

If SQL is genuinely unavoidable, tell the user which endpoint needs it, what the query reads or changes, and why the entity commands cannot express it. Once they approve, call this tool again and pass that explanation as sqlJustification.";

        #endregion Constants

        #region Tools

        /// <summary>
        /// Creates a Lava endpoint that returns data to an authored component, creating
        /// the containing Lava application first if it does not exist yet.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application the endpoint belongs to.</param>
        /// <param name="applicationName">The name of the application. Only used when the application has to be created.</param>
        /// <param name="endpointSlug">The slug of the new endpoint.</param>
        /// <param name="httpMethod">The HTTP method the endpoint answers: Get, Post, Put or Delete. Defaults to Post.</param>
        /// <param name="codeTemplate">The Lava template that produces the response body.</param>
        /// <param name="enabledLavaCommands">A comma-delimited list of Lava commands the template needs, such as "RockEntity" or "RockEntity,RockEntityModify".</param>
        /// <param name="securityMode">How the endpoint authorizes execution: EndpointExecute, ApplicationView, ApplicationEdit or ApplicationAdministrate. Defaults to ApplicationView so the application's security governs.</param>
        /// <param name="sqlJustification">Why raw SQL is unavoidable, required only when <paramref name="enabledLavaCommands"/> includes "Sql".</param>
        /// <param name="testParameters">A JSON object of the values a component would send with invoke, surfaced to the test execution as the Body merge field (or QueryString for Get endpoints).</param>
        /// <param name="maxTestOutputLength">How many characters of test output to return, up to 10000. Defaults to 2000.</param>
        /// <returns>The application slug, endpoint slug, callable URL, and the result of test-executing the template.</returns>
        [AgentToolName( "CreateLavaEndpoint" )]
        [AgentToolPreamble( "Creating the Lava endpoint." )]
        [AgentUsage( "applicationSlug groups a block's endpoints; reuse the same slug for every endpoint of one dashboard. applicationName is only read when the application does not exist yet." )]
        [AgentUsage( "Endpoints are keyed by slug AND method, so the same slug with Get and with Post are two different endpoints." )]
        [AgentUsage( "enabledLavaCommands must include every command the template uses or the template will fail at runtime. Use 'RockEntity' to read, 'RockEntityModify' to add or update, and 'RockEntityDelete' to delete. These cover almost everything, including charts and totals." )]
        [AgentUsage( "Do not request 'Sql'. It is refused unless you also pass sqlJustification, which you may only supply after telling the user why the entity commands cannot do the job and getting their explicit approval. Rewriting the template with entity commands is nearly always the correct response to that refusal." )]
        [AgentUsage( "Always pass testParameters when the template reads Body or QueryString, with realistic values, so the parameter path is proven rather than assumed. Without it the test renders with no request data and a template that reads Body.x is only exercised down its missing-parameter branch." )]
        [AgentUsage( "Enabling RockEntityModify or RockEntityDelete turns test execution off for that endpoint, because running it would perform real writes. You get no syntax check at all, so keep write endpoints small and put any read logic in a separate RockEntity-only endpoint that can still be tested." )]
        [Rock.SystemGuid.AgentToolGuid( "9066DD4A-2158-4B1C-87E3-4058CBEE1E5C" )]
        public AgentToolResult CreateLavaEndpoint( string applicationSlug, string applicationName, string endpointSlug, string httpMethod, string codeTemplate, string enabledLavaCommands = null, string securityMode = null, string sqlJustification = null, string testParameters = null, int? maxTestOutputLength = null )
        {
            if ( applicationSlug.IsNullOrWhiteSpace() )
            {
                return Error( "An application slug is required." );
            }

            if ( endpointSlug.IsNullOrWhiteSpace() )
            {
                return Error( "An endpoint slug is required." );
            }

            if ( codeTemplate.IsNullOrWhiteSpace() )
            {
                return Error( "A Lava template is required." );
            }

            if ( !TryGetSecurityMode( securityMode, out var endpointSecurityMode, out var securityModeError ) )
            {
                return Error( securityModeError );
            }

            if ( !TryGetHttpMethod( httpMethod, out var method, out var httpMethodError ) )
            {
                return Error( httpMethodError );
            }

            // Refuse raw SQL before anything is written, so the round trip through the user
            // happens instead of an endpoint existing that has to be walked back.
            if ( !TryValidateSqlUsage( enabledLavaCommands, sqlJustification, out var sqlError ) )
            {
                return Error( sqlError );
            }

            if ( !TryLintTemplate( codeTemplate, out var lintError ) )
            {
                return Error( lintError );
            }

            if ( !TryParseTestParameters( testParameters, out var parsedTestParameters, out var testParametersError ) )
            {
                return Error( testParametersError );
            }

            using ( var rockContext = new RockContext() )
            {
                var applicationService = new LavaApplicationService( rockContext );
                var application = applicationService.Queryable().FirstOrDefault( a => a.Slug == applicationSlug );

                if ( !IsAuthorizedToAuthor( application ) )
                {
                    return Error( application == null
                        ? "You are not authorized to create Lava applications."
                        : $"You are not authorized to administrate the '{applicationSlug}' Lava application." );
                }

                var isNewApplication = application == null;

                if ( isNewApplication )
                {
                    if ( applicationName.IsNullOrWhiteSpace() )
                    {
                        return Error( $"No Lava application exists with the slug '{applicationSlug}'. Provide an applicationName so it can be created." );
                    }

                    /*
                        8/3/2026 - CLAUDE

                        ConfigurationRiggingJson has to be set to valid JSON here. Every request
                        to a Lava application reads LavaApplicationCache.ConfigurationRigging,
                        which parses this string, and the parser throws on null rather than
                        returning null. Leaving the property unset therefore makes every endpoint
                        on the application fail with a 500 that names Newtonsoft rather than
                        anything recognizable, and it fails for the person who just created it.

                        The Lava Application Detail block always assigns the property from its
                        bag, so an application created through the admin pages never reaches this
                        state. Only a caller that news up the entity directly can.

                        Reason: An unset rigging value breaks every endpoint on the application.
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
                        return Error( application.ValidationResults.Select( r => r.ErrorMessage ) );
                    }
                }
                else if ( application.LavaEndpoints.Any( e => e.Slug == endpointSlug && e.HttpMethod == method ) )
                {
                    return Error( $"An endpoint already exists at '{applicationSlug}/{endpointSlug}' for the {method} method. Use UpdateLavaEndpoint to replace its template." );
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

                // These endpoints exist to feed components, so they return JSON. Cross-site
                // forgery protection stays on, which is what useLavaApp sends the header for.
                endpoint.SetAdditionalSettings( new LavaEndpointAdditionalSettings
                {
                    EnableCrossSiteForgeryProtection = true,
                    ContentType = JsonContentType
                } );

                new LavaEndpointService( rockContext ).Add( endpoint );

                if ( !endpoint.IsValid )
                {
                    return Error( endpoint.ValidationResults.Select( r => r.ErrorMessage ) );
                }

                rockContext.SaveChanges();

                var result = Success( new
                {
                    ApplicationSlug = application.Slug,
                    EndpointSlug = endpoint.Slug,
                    Method = method.ToString(),
                    Url = GetEndpointUrl( application.Slug, endpoint.Slug ),
                    TestExecution = TestExecute( codeTemplate, endpoint.EnabledLavaCommands, application, method, parsedTestParameters, maxTestOutputLength )
                } );

                /*
                    8/3/2026 - CLAUDE

                    Two different authorization gaps can leave a brand new endpoint uncallable,
                    and they need different advice, so report whichever one applies.

                    In EndpointExecute mode the endpoint answers for itself. A new endpoint has
                    no authorization rules, and while its parent authority is the application,
                    the authorization walk only inspects explicit rules on each parent. It never
                    invokes LavaApplicationCache.IsAuthorized, so that class's override for the
                    Rock Administrators and Lava Application Developers roles is not reached.
                    The result is that nobody can call the endpoint, administrators included.

                    In the application modes the endpoint calls LavaApplication.IsAuthorized
                    directly, which does apply the override, so those two roles can call it
                    immediately and everyone else waits on explicit rules.

                    Reason: The previous message promised administrators access they do not have.
                */
                if ( endpointSecurityMode == LavaEndpointSecurityMode.EndpointExecute )
                {
                    result.WithInstructions( $"The '{endpoint.Slug}' endpoint uses the EndpointExecute security mode and has no authorization rules, so nobody can call it yet, administrators included. Either grant Execute on the endpoint through the Lava Applications admin pages, or recreate it with a securityMode of ApplicationView so it defers to the application. Tell the user this before they test the page, because the call will fail with a 401 rather than an error they can read." );
                }
                else if ( isNewApplication )
                {
                    result.WithInstructions( $"The '{application.Slug}' Lava application was created with no security rules and deliberately does not inherit any. Only the Rock Administrators and Lava Application Developers roles can execute its endpoints until someone grants rights on the application through the Lava Applications admin pages. Tell the user this before they test the page as a normal visitor." );
                }

                // The justification was given to the tool, not to the user, so require that it
                // be repeated out loud rather than trusting it was already discussed.
                if ( IsSqlRequested( endpoint.EnabledLavaCommands ) )
                {
                    result.WithInstructions( GetSqlApprovalInstructions( endpoint.Slug, sqlJustification ) );
                }

                return result;
            }
        }

        /// <summary>
        /// Reads the current template of an endpoint so the agent can iterate on it.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application the endpoint belongs to.</param>
        /// <param name="endpointSlug">The slug of the endpoint to read.</param>
        /// <param name="httpMethod">The HTTP method of the endpoint. Defaults to Post.</param>
        /// <returns>The endpoint's template and configuration, or an error when it does not exist.</returns>
        [AgentToolName( "GetLavaEndpoint" )]
        [AgentToolPreamble( "Reading the Lava endpoint." )]
        [AgentUsage( "Read the endpoint before changing it, so an UpdateLavaEndpoint call replaces the template you expect. Endpoints are keyed by slug AND method." )]
        [Rock.SystemGuid.AgentToolGuid( "11AE1557-1EF3-4E03-9E8E-FCF99F72FCD9" )]
        public AgentToolResult GetLavaEndpoint( string applicationSlug, string endpointSlug, string httpMethod = null )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEndpoint( applicationSlug, endpointSlug, httpMethod, rockContext, out var endpoint, out var error ) )
                {
                    return error;
                }

                return Success( new
                {
                    ApplicationSlug = endpoint.LavaApplication.Slug,
                    EndpointSlug = endpoint.Slug,
                    Method = endpoint.HttpMethod.ToString(),
                    endpoint.Name,
                    endpoint.IsActive,
                    endpoint.CodeTemplate,
                    endpoint.EnabledLavaCommands,
                    SecurityMode = endpoint.SecurityMode.ToString(),
                    ContentType = endpoint.GetAdditionalSettings<LavaEndpointAdditionalSettings>()?.ContentType,
                    Url = GetEndpointUrl( endpoint.LavaApplication.Slug, endpoint.Slug )
                } );
            }
        }

        /// <summary>
        /// Replaces the template of an existing endpoint, optionally adjusts the settings
        /// that decide whether it can run at all, and reports the result of test-executing
        /// the replacement.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application the endpoint belongs to.</param>
        /// <param name="endpointSlug">The slug of the endpoint to update.</param>
        /// <param name="codeTemplate">The new Lava template.</param>
        /// <param name="httpMethod">The HTTP method of the endpoint. Defaults to Post.</param>
        /// <param name="securityMode">The security mode to switch to, or <c>null</c> to leave it alone.</param>
        /// <param name="enabledLavaCommands">The comma-delimited Lava commands to allow, or <c>null</c> to leave them alone.</param>
        /// <param name="sqlJustification">Why raw SQL is unavoidable, required only when this call adds "Sql" to <paramref name="enabledLavaCommands"/>.</param>
        /// <param name="testParameters">A JSON object of the values a component would send with invoke, surfaced to the test execution as the Body merge field (or QueryString for Get endpoints).</param>
        /// <param name="maxTestOutputLength">How many characters of test output to return, up to 10000. Defaults to 2000.</param>
        /// <returns>The endpoint identifiers and the result of test-executing the new template.</returns>
        [AgentToolName( "UpdateLavaEndpoint" )]
        [AgentToolPreamble( "Updating the Lava endpoint." )]
        [AgentUsage( "This replaces the whole template, so send the complete Lava rather than a fragment. Read it with GetLavaEndpoint first if you did not write the current version." )]
        [AgentUsage( "securityMode and enabledLavaCommands are left unchanged when omitted. Use them to correct an endpoint you already created rather than sending the user to the admin pages." )]
        [AgentUsage( "A template that starts using a new command needs that command added here too, or it will silently return nothing where the command was." )]
        [AgentUsage( "Adding 'Sql' to enabledLavaCommands is refused without sqlJustification, exactly as it is on create. Rewriting the template with 'RockEntity', 'RockEntityModify' and 'RockEntityDelete' is the expected response." )]
        [AgentUsage( "Always pass testParameters when the template reads Body or QueryString, with realistic values, so the parameter path is proven rather than assumed." )]
        [AgentUsage( "An endpoint enabling RockEntityModify or RockEntityDelete is not test-executed, so this call returns no evidence the template works." )]
        [Rock.SystemGuid.AgentToolGuid( "2F92D13B-A2A2-455C-8324-57A181D505C2" )]
        public AgentToolResult UpdateLavaEndpoint( string applicationSlug, string endpointSlug, string codeTemplate, string httpMethod = null, string securityMode = null, string enabledLavaCommands = null, string sqlJustification = null, string testParameters = null, int? maxTestOutputLength = null )
        {
            if ( codeTemplate.IsNullOrWhiteSpace() )
            {
                return Error( "A Lava template is required." );
            }

            if ( !TryLintTemplate( codeTemplate, out var lintError ) )
            {
                return Error( lintError );
            }

            if ( !TryParseTestParameters( testParameters, out var parsedTestParameters, out var testParametersError ) )
            {
                return Error( testParametersError );
            }

            /*
                8/3/2026 - CLAUDE

                Only what this call is asking for is checked. Leaving enabledLavaCommands null
                means the stored commands are untouched, so an endpoint whose SQL the user
                already approved is not re-litigated on every template edit. Adding Sql here
                goes through the same refusal as creating it, and a template that starts using
                {% sql %} without the command being enabled cannot run it anyway.

                Reason: Gate the request to change commands, not every edit to the template.
            */
            if ( enabledLavaCommands != null
                && !TryValidateSqlUsage( enabledLavaCommands, sqlJustification, out var sqlError ) )
            {
                return Error( sqlError );
            }

            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEndpoint( applicationSlug, endpointSlug, httpMethod, rockContext, out var endpoint, out var error ) )
                {
                    return error;
                }

                endpoint.CodeTemplate = codeTemplate;

                // Both of these are left alone when the agent does not mention them, so a
                // template-only edit cannot quietly change who is allowed to run the endpoint.
                if ( securityMode.IsNotNullOrWhiteSpace() )
                {
                    if ( !TryGetSecurityMode( securityMode, out var newSecurityMode, out var securityModeError ) )
                    {
                        return Error( securityModeError );
                    }

                    endpoint.SecurityMode = newSecurityMode;
                }

                if ( enabledLavaCommands != null )
                {
                    endpoint.EnabledLavaCommands = enabledLavaCommands;
                }

                if ( !endpoint.IsValid )
                {
                    return Error( endpoint.ValidationResults.Select( r => r.ErrorMessage ) );
                }

                rockContext.SaveChanges();

                var result = Success( new
                {
                    ApplicationSlug = endpoint.LavaApplication.Slug,
                    EndpointSlug = endpoint.Slug,
                    Method = endpoint.HttpMethod.ToString(),
                    Url = GetEndpointUrl( endpoint.LavaApplication.Slug, endpoint.Slug ),
                    TestExecution = TestExecute( codeTemplate, endpoint.EnabledLavaCommands, endpoint.LavaApplication, endpoint.HttpMethod, parsedTestParameters, maxTestOutputLength )
                } );

                // Only when this call is what turned SQL on. An endpoint that already had it
                // approved does not need the warning repeated on every template edit.
                if ( enabledLavaCommands != null && IsSqlRequested( enabledLavaCommands ) )
                {
                    result.WithInstructions( GetSqlApprovalInstructions( endpoint.Slug, sqlJustification ) );
                }

                return result;
            }
        }

        /// <summary>
        /// Deletes an endpoint this skill previously created, so exploration and
        /// diagnostics can clean up after themselves.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application the endpoint belongs to.</param>
        /// <param name="endpointSlug">The slug of the endpoint to delete.</param>
        /// <param name="httpMethod">The HTTP method of the endpoint. Defaults to Post.</param>
        /// <returns>Confirmation of the deletion, or an error.</returns>
        [AgentToolName( "DeleteLavaEndpoint" )]
        [AgentToolPreamble( "Deleting the Lava endpoint." )]
        [AgentUsage( "Only endpoints created by this skill can be deleted; anything a person authored has to be removed through the Lava Applications admin pages. Use this to clean up diagnostic and scratch endpoints instead of leaving them for the user." )]
        [Rock.SystemGuid.AgentToolGuid( "B3E1A5C7-6F24-4D1B-9C88-05D7F42A61E9" )]
        public AgentToolResult DeleteLavaEndpoint( string applicationSlug, string endpointSlug, string httpMethod = null )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEndpoint( applicationSlug, endpointSlug, httpMethod, rockContext, out var endpoint, out var error ) )
                {
                    return error;
                }

                // The provenance stamp is the whole safety model: the skill can only unwind
                // its own work, never something a person built through the admin pages.
                if ( endpoint.ForeignKey != AgentProvenanceKey )
                {
                    return Error( $"The '{endpointSlug}' endpoint was not created by this skill, so it cannot be deleted here. Ask the user to remove it through the Lava Applications admin pages." );
                }

                var application = endpoint.LavaApplication;
                var endpointId = endpoint.Id;

                new LavaEndpointService( rockContext ).Delete( endpoint );
                rockContext.SaveChanges();

                var remainingCount = application.LavaEndpoints.Count( e => e.Id != endpointId );

                var result = Success( new
                {
                    Deleted = true,
                    ApplicationSlug = application.Slug,
                    EndpointSlug = endpointSlug,
                    RemainingEndpointCount = remainingCount
                } );

                if ( remainingCount == 0 && application.ForeignKey == AgentProvenanceKey )
                {
                    result.WithInstructions( $"The '{application.Slug}' application now has no endpoints and was created by this skill. If it is no longer needed, remove it with DeleteLavaApplication so it does not linger as clutter." );
                }

                return result;
            }
        }

        /// <summary>
        /// Deletes a Lava application this skill previously created, along with any
        /// endpoints it created inside it.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application to delete.</param>
        /// <returns>Confirmation of the deletion, or an error.</returns>
        [AgentToolName( "DeleteLavaApplication" )]
        [AgentToolPreamble( "Deleting the Lava application." )]
        [AgentUsage( "Only applications created by this skill, containing only endpoints created by this skill, can be deleted. Use it to clean up scratch applications when a build is finished." )]
        [Rock.SystemGuid.AgentToolGuid( "9A47C2D1-83B5-4E60-A7F3-1B58C90D24E6" )]
        public AgentToolResult DeleteLavaApplication( string applicationSlug )
        {
            if ( applicationSlug.IsNullOrWhiteSpace() )
            {
                return Error( "An application slug is required." );
            }

            using ( var rockContext = new RockContext() )
            {
                var applicationService = new LavaApplicationService( rockContext );
                var application = applicationService.Queryable().FirstOrDefault( a => a.Slug == applicationSlug );

                if ( application == null )
                {
                    return Error( $"No Lava application exists with the slug '{applicationSlug}'." );
                }

                if ( !IsAuthorizedToAuthor( application ) )
                {
                    return Error( $"You are not authorized to administrate the '{applicationSlug}' Lava application." );
                }

                if ( application.ForeignKey != AgentProvenanceKey )
                {
                    return Error( $"The '{applicationSlug}' application was not created by this skill, so it cannot be deleted here. Ask the user to remove it through the Lava Applications admin pages." );
                }

                // A single hand-authored endpoint anywhere in the application blocks the
                // whole delete, so a person's work can never ride along with the cleanup.
                var foreignEndpoints = application.LavaEndpoints
                    .Where( e => e.ForeignKey != AgentProvenanceKey )
                    .Select( e => e.Slug )
                    .ToList();

                if ( foreignEndpoints.Any() )
                {
                    return Error( $"The '{applicationSlug}' application contains endpoints that were not created by this skill ({string.Join( ", ", foreignEndpoints )}), so it cannot be deleted here. Ask the user to remove it through the Lava Applications admin pages." );
                }

                var endpointService = new LavaEndpointService( rockContext );
                var deletedEndpointCount = application.LavaEndpoints.Count;

                endpointService.DeleteRange( application.LavaEndpoints.ToList() );
                applicationService.Delete( application );
                rockContext.SaveChanges();

                return Success( new
                {
                    Deleted = true,
                    ApplicationSlug = applicationSlug,
                    DeletedEndpointCount = deletedEndpointCount
                } );
            }
        }

        #endregion Tools

        #region Methods

        /// <summary>
        /// Determines if the acting person may author the specified Lava application and
        /// its endpoints.
        /// </summary>
        /// <param name="application">The application being changed, or <c>null</c> when a new one is being created.</param>
        /// <returns><c>true</c> if the acting person is authorized.</returns>
        private bool IsAuthorizedToAuthor( LavaApplication application )
        {
            var person = AgentRequestContext.CurrentPerson;

            if ( person == null )
            {
                return false;
            }

            // An existing application is checked the same way PageBuilderSkill checks the
            // page it is about to change: ADMINISTRATE of the target.
            if ( application != null )
            {
                return LavaApplicationCache.Get( application.Id )?.IsAuthorized( Authorization.ADMINISTRATE, person ) == true;
            }

            // A new application has nothing to check against, because LavaApplication
            // intentionally returns a null ParentAuthority to break inheritance. Fall back
            // to the roles LavaApplication itself treats as authorization overrides.
            return RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).IsPersonInRole( person.Guid )
                || RoleCache.Get( Rock.SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() ).IsPersonInRole( person.Guid );
        }

        /// <summary>
        /// Loads an endpoint by application slug, endpoint slug and HTTP method, checking
        /// authorization along the way.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application.</param>
        /// <param name="endpointSlug">The slug of the endpoint.</param>
        /// <param name="httpMethod">The HTTP method of the endpoint, or <c>null</c> to default to Post.</param>
        /// <param name="rockContext">The context to load the endpoint from.</param>
        /// <param name="endpoint">Contains the endpoint when <c>true</c> is returned.</param>
        /// <param name="error">Contains the result to return when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the endpoint was found and the acting person is authorized.</returns>
        private bool TryGetEndpoint( string applicationSlug, string endpointSlug, string httpMethod, RockContext rockContext, out LavaEndpoint endpoint, out AgentToolResult error )
        {
            endpoint = null;
            error = null;

            if ( applicationSlug.IsNullOrWhiteSpace() || endpointSlug.IsNullOrWhiteSpace() )
            {
                error = Error( "An application slug and an endpoint slug are both required." );
                return false;
            }

            var application = new LavaApplicationService( rockContext )
                .Queryable()
                .FirstOrDefault( a => a.Slug == applicationSlug );

            if ( application == null )
            {
                error = Error( $"No Lava application exists with the slug '{applicationSlug}'." );
                return false;
            }

            if ( !IsAuthorizedToAuthor( application ) )
            {
                error = Error( $"You are not authorized to administrate the '{applicationSlug}' Lava application." );
                return false;
            }

            if ( !TryGetHttpMethod( httpMethod, out var method, out var httpMethodError ) )
            {
                error = Error( httpMethodError );
                return false;
            }

            endpoint = application.LavaEndpoints
                .FirstOrDefault( e => e.Slug == endpointSlug && e.HttpMethod == method );

            if ( endpoint == null )
            {
                error = Error( $"No endpoint exists at '{applicationSlug}/{endpointSlug}' for the {method} method." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Renders the template so the agent finds out it is broken while it can still fix
        /// it, instead of a visitor seeing Lava error text later.
        /// </summary>
        /// <param name="codeTemplate">The Lava template to render.</param>
        /// <param name="enabledLavaCommands">The comma-delimited Lava commands the template is allowed to use.</param>
        /// <param name="application">The application whose configuration rigging the template can read.</param>
        /// <param name="method">The HTTP method of the endpoint, which decides whether simulated parameters surface as Body or QueryString.</param>
        /// <param name="testParameters">The parsed request values to simulate, or <c>null</c> to render with no request data.</param>
        /// <param name="maxTestOutputLength">How many characters of output to return, or <c>null</c> for the default.</param>
        /// <returns>A result describing whether the render succeeded, and what it produced.</returns>
        private TestExecutionResult TestExecute( string codeTemplate, string enabledLavaCommands, LavaApplication application, LavaEndpointHttpMethod method, object testParameters, int? maxTestOutputLength )
        {
            /*
                8/12/2026 - CLAUDE

                A template that can write is never test-executed. Rendering it performed real
                inserts, updates and deletes, unattributed, which is an unacceptable price for
                a syntax check.

                Rolling the render back in a transaction was considered and rejected. It is
                achievable (the entity blocks take their RockContext from the Lava context, so
                seeding it and using WrapTransactionIf would cover the SQL), but it cannot be
                made honest: Rock updates and flushes caches during save, and those caches are
                not restored by a rollback, so a "dry run" would leave the instance describing
                rows that no longer exist. Post-save hooks that queue bus messages, RealTime
                notifications and workflows fire regardless of the rollback as well.

                Detection keys off enabledLavaCommands rather than the template text because a
                write cannot execute unless its command is enabled, which makes this exact
                rather than a guess about markup.

                Reason: A partial rollback would be more dangerous than not testing at all.
            */
            if ( IsWriteCapable( enabledLavaCommands ) )
            {
                return new TestExecutionResult
                {
                    IsSkipped = true,
                    Coverage = "Not executed. This endpoint enables a write command (RockEntityModify or RockEntityDelete), and running the template would perform real, unattributed writes. Nothing about this template has been verified, including its syntax. Review it yourself, and have the user exercise it from the page where a failure is visible and recoverable. To get a test result, move the read-only part of the template into a separate endpoint that enables only RockEntity."
                };
            }

            /*
                8/11/2026 - CLAUDE

                Endpoints read their request values from the Body merge field (QueryString for
                Get), and an end-to-end test proved that path is exactly the one this test could
                never exercise: with no request context, a template reading Body.teamId only ever
                runs its missing-parameter branch, and the agent ships the endpoint on faith. The
                simulated field mirrors LavaApplicationRequestHelpers.RequestToDictionary, which
                adds the parsed JSON body as a single "Body" merge field, so a template that
                passes here reads its parameters the same way it will on a real request.

                Reason: The parameter path is the likeliest silent failure and was untestable.
            */
            var requestFieldName = method == LavaEndpointHttpMethod.Get ? "QueryString" : "Body";

            var coverage = testParameters != null
                ? $"Rendered as the current person with a simulated {requestFieldName} merge field built from testParameters. Other request merge fields (Headers, Cookies, RawUrl) were not available."
                : "Rendered as the current person with no HTTP request context, so the Request, QueryString, Body and Headers merge fields were not available. A template that reads Body or QueryString was only exercised down its missing-parameter branch; pass testParameters to prove the parameter path.";

            /*
                8/3/2026 - CLAUDE

                Everything belongs inside the try, including building the merge fields and the
                render context. Those steps looked incapable of failing and were originally left
                outside it, but a null ConfigurationRiggingJson made the parse throw, and because
                the throw happened after the caller had already saved its changes, the agent was
                told the whole tool call failed while the endpoint sat in the database. A skill
                whose job is reporting whether a template works must never itself be the thing
                that throws.

                Reason: A failure while preparing the render is a test result, not a tool failure.
            */
            try
            {
                var mergeFields = LavaHelper.GetCommonMergeFields( null, AgentRequestContext.CurrentPerson );

                // Only parse rigging that is actually there. The parser throws on null instead
                // of returning null, so an application with no rigging would fail the render
                // for a reason that has nothing to do with the template being tested.
                var configurationRigging = application?.ConfigurationRiggingJson.IsNotNullOrWhiteSpace() == true
                    ? application.ConfigurationRiggingJson.FromJsonDynamic()
                    : null;

                mergeFields.AddOrReplace( "ConfigurationRigging", configurationRigging );

                if ( testParameters != null )
                {
                    mergeFields.AddOrReplace( requestFieldName, testParameters );
                }

                var parameters = LavaRenderParameters.WithContext(
                    LavaService.NewRenderContext( mergeFields, enabledLavaCommands.SplitDelimitedValues() ) );

                /*
                    8/3/2026 - CLAUDE

                    The engine's default strategy renders the exception into the output, which
                    would let a broken template look like it succeeded and produced text. This
                    sets Throw for this render only, so a failure is a failure. The engine's
                    global strategy is deliberately left alone.

                    Reason: A silently swallowed error defeats the purpose of test-executing.
                */
                parameters.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw;

                var renderResult = LavaService.RenderTemplate( codeTemplate, parameters );

                if ( renderResult.HasErrors )
                {
                    return new TestExecutionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = AugmentLavaError( renderResult.GetLavaException().Message ),
                        Coverage = coverage
                    };
                }

                return BuildSuccessResult( renderResult.Text, coverage, maxTestOutputLength );
            }
            catch ( Exception ex )
            {
                return new TestExecutionResult
                {
                    IsSuccess = false,
                    ErrorMessage = AugmentLavaError( ex.Message ),
                    Coverage = coverage
                };
            }
        }

        /// <summary>
        /// Appends a hint to the Lava error messages whose real cause is somewhere other
        /// than where the message points, so the agent's next attempt is a fix rather than
        /// another guess.
        /// </summary>
        /// <param name="message">The error message the engine produced.</param>
        /// <returns>The message, with a hint appended when one applies.</returns>
        private static string AugmentLavaError( string message )
        {
            if ( message.IsNullOrWhiteSpace() )
            {
                return message;
            }

            // The engine reports a where clause containing a dotted navigation path as a
            // generic invalid-expression error with no mention of the cause.
            if ( message.IndexOf( "Where expression is invalid", StringComparison.OrdinalIgnoreCase ) >= 0 )
            {
                return message + " Hint: dotted navigation paths (for example 'Group.CampusId') are not supported in where clauses, even though they work in sort, groupby and select. Resolve the related ids in a first query and filter on a scalar property or a literal OR clause.";
            }

            // An unrecognized block tag is reported as a missing end tag somewhere else in
            // the template, which sends the agent to the wrong line.
            if ( message.IndexOf( "was expected", StringComparison.OrdinalIgnoreCase ) >= 0 )
            {
                return message + " Hint: this usually means a block tag was not recognized, so its end tag broke the surrounding structure. Entity command blocks use the entity's own name ('{% group %}...{% endgroup %}'), and the command must also be listed in enabledLavaCommands.";
            }

            return message;
        }

        /// <summary>
        /// Builds the result for a template that rendered, trimming the output to something an
        /// agent can actually read.
        /// </summary>
        /// <param name="output">The full text the template produced.</param>
        /// <param name="coverage">The description of what the test did and did not exercise.</param>
        /// <param name="maxTestOutputLength">The caller's requested output budget, or <c>null</c> for the default.</param>
        /// <returns>A successful result whose output is no longer than the effective limit.</returns>
        private static TestExecutionResult BuildSuccessResult( string output, string coverage, int? maxTestOutputLength )
        {
            var fullText = output.ToStringSafe();

            // Diagnostics legitimately need more than the default, so the caller can raise
            // the budget, but a whole dashboard payload still cannot flood the tool result.
            var effectiveLimit = Math.Min( Math.Max( maxTestOutputLength ?? MaxTestOutputLength, 100 ), MaxAllowedTestOutputLength );

            if ( fullText.Length <= effectiveLimit )
            {
                return new TestExecutionResult
                {
                    IsSuccess = true,
                    Output = fullText,
                    OutputLength = fullText.Length,
                    Coverage = coverage
                };
            }

            // A dashboard endpoint can render hundreds of kilobytes. Returning all of it pushes
            // the tool result past what the caller can accept, which turns a passing test into
            // an apparent failure, so keep the head of it and say how much was cut.
            return new TestExecutionResult
            {
                IsSuccess = true,
                Output = fullText.Substring( 0, effectiveLimit ),
                OutputLength = fullText.Length,
                IsOutputTruncated = true,
                TruncationAdvice = $"Output was truncated at {effectiveLimit} of {fullText.Length} characters. Re-run with a larger maxTestOutputLength (up to {MaxAllowedTestOutputLength}), or emit more compact output.",
                Coverage = coverage
            };
        }

        /// <summary>
        /// Builds the URL that a client uses to call the endpoint.
        /// </summary>
        /// <param name="applicationSlug">The slug of the Lava application.</param>
        /// <param name="endpointSlug">The slug of the endpoint.</param>
        /// <returns>The callable URL of the endpoint.</returns>
        private string GetEndpointUrl( string applicationSlug, string endpointSlug )
        {
            return $"{AgentRequestContext.RootUrlPath}/api/v2/lava-app/{RouteVersion}/{applicationSlug}/{endpointSlug}";
        }

        /// <summary>
        /// Rejects template mistakes whose runtime failures point somewhere other than the
        /// real cause, so the agent hears about them while the template is still in hand.
        /// </summary>
        /// <param name="codeTemplate">The template about to be saved.</param>
        /// <param name="errorMessage">Contains the explanation when <c>false</c> is returned.</param>
        /// <returns><c>true</c> when no lint problem was found.</returns>
        private static bool TryLintTemplate( string codeTemplate, out string errorMessage )
        {
            errorMessage = null;

            /*
                8/11/2026 - CLAUDE

                There is no generic '{% entity <name> %}' block tag; the entity commands
                register one tag per entity name. The engine reports the unknown tag as a
                missing end tag somewhere else entirely, so an agent that writes it burns a
                round trip on a misleading error. An end-to-end test hit exactly this.

                Reason: Catch a known-wrong tag before it is saved, with the real fix named.
            */
            if ( codeTemplate.IndexOf( "{% entity ", StringComparison.OrdinalIgnoreCase ) >= 0
                || codeTemplate.IndexOf( "{%- entity ", StringComparison.OrdinalIgnoreCase ) >= 0
                || codeTemplate.IndexOf( "{% endentity", StringComparison.OrdinalIgnoreCase ) >= 0 )
            {
                errorMessage = "The template uses '{% entity %}', which is not a Lava tag. Entity command blocks use the entity's own name: '{% group %}...{% endgroup %}', '{% groupmember %}...{% endgroupmember %}'. Replace the tag with the entity's friendly name with the spaces removed.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the request values the caller wants simulated during test execution,
        /// using the same deserialization the real request pipeline applies to a JSON body.
        /// </summary>
        /// <param name="testParameters">The JSON object supplied by the caller, or <c>null</c>.</param>
        /// <param name="parsed">Contains the parsed values when <c>true</c> is returned, or <c>null</c> when none were supplied.</param>
        /// <param name="errorMessage">Contains the explanation when <c>false</c> is returned.</param>
        /// <returns><c>true</c> when the parameters are absent or valid JSON.</returns>
        private static bool TryParseTestParameters( string testParameters, out object parsed, out string errorMessage )
        {
            parsed = null;
            errorMessage = null;

            if ( testParameters.IsNullOrWhiteSpace() )
            {
                return true;
            }

            try
            {
                // The same call LavaApplicationRequestHelpers uses on a real JSON body, so
                // the simulated Body behaves like the one a component's invoke produces.
                parsed = Newtonsoft.Json.JsonConvert.DeserializeObject( testParameters );
            }
            catch ( Exception ex )
            {
                errorMessage = $"testParameters must be a valid JSON object, for example {{\"teamId\": 5}}. It could not be parsed: {ex.Message}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the requested commands include raw SQL.
        /// </summary>
        /// <param name="enabledLavaCommands">The comma-delimited commands the caller asked for.</param>
        /// <returns><c>true</c> if the Sql command was requested.</returns>
        private static bool IsSqlRequested( string enabledLavaCommands )
        {
            return enabledLavaCommands
                .SplitDelimitedValues()
                .Any( c => c.Equals( SqlCommandName, StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// Determines whether the requested commands allow the template to change data.
        /// A template can only write when its write command is enabled, so this is exact
        /// rather than an inspection of the markup.
        /// </summary>
        /// <param name="enabledLavaCommands">The comma-delimited commands the caller asked for.</param>
        /// <returns><c>true</c> if the template is able to insert, update or delete.</returns>
        private static bool IsWriteCapable( string enabledLavaCommands )
        {
            // Sql is deliberately absent. It can write, but it is already gated behind the
            // user's explicit approval, and endpoints that use it are overwhelmingly reads
            // that would lose their only syntax check for no gain in safety.
            return enabledLavaCommands
                .SplitDelimitedValues()
                .Any( c => c.Equals( RockEntityModifyCommandName, StringComparison.OrdinalIgnoreCase )
                    || c.Equals( RockEntityDeleteCommandName, StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// Rejects a request for the raw SQL command that does not carry the user's approval,
        /// so choosing SQL costs a round trip through the person who has to live with it.
        /// </summary>
        /// <param name="enabledLavaCommands">The comma-delimited commands the caller asked for.</param>
        /// <param name="sqlJustification">The caller's explanation of why SQL is unavoidable.</param>
        /// <param name="errorMessage">Contains the refusal message when <c>false</c> is returned.</param>
        /// <returns><c>true</c> when the commands are allowed as requested.</returns>
        private static bool TryValidateSqlUsage( string enabledLavaCommands, string sqlJustification, out string errorMessage )
        {
            errorMessage = null;

            if ( !IsSqlRequested( enabledLavaCommands ) || sqlJustification.IsNotNullOrWhiteSpace() )
            {
                return true;
            }

            errorMessage = SqlRequiresApprovalMessage;
            return false;
        }

        /// <summary>
        /// Builds the guidance attached to an endpoint that was allowed to use raw SQL, so the
        /// justification reaches the user rather than staying between the agent and the tool.
        /// </summary>
        /// <param name="endpointSlug">The slug of the endpoint that uses SQL.</param>
        /// <param name="sqlJustification">The explanation the caller supplied.</param>
        /// <returns>The instruction text to attach to the result.</returns>
        private static string GetSqlApprovalInstructions( string endpointSlug, string sqlJustification )
        {
            return $"The '{endpointSlug}' endpoint was created with the raw SQL command enabled, on this justification: {sqlJustification} State plainly in your reply that this endpoint uses raw SQL, repeat that justification, and warn that raw SQL does not honor the viewer's per-row permissions, so the template itself is responsible for every filter. If the user did not already approve this, say so rather than presenting it as settled.";
        }

        /// <summary>
        /// Parses the HTTP method supplied by the agent. Endpoints are keyed by slug and
        /// method, so getting this wrong addresses the wrong endpoint rather than failing
        /// loudly, which is why an unrecognized value is rejected instead of coerced.
        /// </summary>
        /// <param name="httpMethod">The HTTP method name, or <c>null</c> to use the default.</param>
        /// <param name="method">Contains the parsed method when <c>true</c> is returned.</param>
        /// <param name="errorMessage">Contains the error message when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the HTTP method was parsed.</returns>
        private static bool TryGetHttpMethod( string httpMethod, out LavaEndpointHttpMethod method, out string errorMessage )
        {
            errorMessage = null;

            // Post is the default because that is what useLavaApp sends when a component
            // does not ask for anything else.
            if ( httpMethod.IsNullOrWhiteSpace() )
            {
                method = LavaEndpointHttpMethod.Post;
                return true;
            }

            if ( Enum.TryParse( httpMethod, true, out method ) )
            {
                return true;
            }

            errorMessage = $"'{httpMethod}' is not a valid HTTP method. Use one of: {string.Join( ", ", Enum.GetNames( typeof( LavaEndpointHttpMethod ) ) )}.";
            return false;
        }

        /// <summary>
        /// Parses the security mode supplied by the agent.
        /// </summary>
        /// <param name="securityMode">The security mode name, or <c>null</c> to use the default.</param>
        /// <param name="mode">Contains the parsed mode when <c>true</c> is returned.</param>
        /// <param name="errorMessage">Contains the error message when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the security mode was parsed.</returns>
        private static bool TryGetSecurityMode( string securityMode, out LavaEndpointSecurityMode mode, out string errorMessage )
        {
            errorMessage = null;

            /*
                8/11/2026 - CLAUDE

                The default used to be EndpointExecute, matching the entity default, on the
                theory that keeping authorization on the endpoint was safest. In practice a
                new endpoint has no authorization rules and the authorization walk never
                reaches LavaApplicationCache's role override, so the "safe" default was an
                endpoint nobody could call, failing as a bare 401, and every agent hit it.
                ApplicationView defers to the application, where this skill's security is
                actually rigged, so the default now matches where the rules really live.

                Reason: The old default produced an endpoint that no one, admins included,
                could call.

                8/12/2026 - CLAUDE

                REVISIT: the claim above that ApplicationView defers to "where this skill's
                security is actually rigged" is wrong. This skill rigs no security at all.
                CreateLavaEndpoint builds the application with an empty ConfigurationRigging
                and no Auth rows, so switching the default from EndpointExecute to
                ApplicationView moved the problem rather than fixing it:

                - ApplicationView authorizes against the application's EXECUTE_VIEW action.
                - A new application has no Auth rows, LavaApplication.ParentAuthority is
                  deliberately null (see LavaApplication.Logic.cs), and Model.IsAllowedByDefault
                  grants only VIEW and TAG, so EXECUTE_VIEW denies.
                - LavaApplication.IsAuthorized overrides for Rock Administrators and Lava
                  Application Developers on View/Edit/Administrate but explicitly NOT on
                  Execute.

                So a freshly created endpoint still cannot be called by anyone, administrators
                included, exactly as before. Worse, it is now silent: the WithInstructions
                warning in CreateLavaEndpoint fires only for EndpointExecute, so the default
                path ships this failure with no warning attached.

                The fix is for these tools to set the authorization rather than describe it.
                That likely means taking the intended audience as a parameter (staff, all
                authenticated people, or public) and writing the matching EXECUTE_VIEW Auth
                rows when the application is created, so the endpoint is callable by exactly
                the people it should be and no one else. Until then, do not tell the user
                security is handled.

                Reason: The default security mode still yields an uncallable endpoint, and
                now does so without warning.
            */
            if ( securityMode.IsNullOrWhiteSpace() )
            {
                mode = LavaEndpointSecurityMode.ApplicationView;
                return true;
            }

            if ( Enum.TryParse( securityMode, true, out mode ) )
            {
                return true;
            }

            errorMessage = $"'{securityMode}' is not a valid security mode. Use one of: {string.Join( ", ", Enum.GetNames( typeof( LavaEndpointSecurityMode ) ) )}.";
            return false;
        }

        #endregion Methods

        #region Support Classes

        /// <summary>
        /// The outcome of rendering a template so the agent can see whether it works
        /// before anyone visits the page.
        /// </summary>
        private class TestExecutionResult
        {
            /// <summary>
            /// Whether the template rendered without an error. This is <c>false</c> when
            /// <see cref="IsSkipped"/> is <c>true</c>, because nothing was rendered; it does
            /// not mean the template is broken.
            /// </summary>
            public bool IsSuccess { get; set; }

            /// <summary>
            /// Whether the template was deliberately not executed because it can write.
            /// Distinguishes "we did not look" from "we looked and it failed".
            /// </summary>
            public bool IsSkipped { get; set; }

            /// <summary>
            /// The rendered output when <see cref="IsSuccess"/> is <c>true</c>. This is only
            /// the first part of it when <see cref="IsOutputTruncated"/> is <c>true</c>.
            /// </summary>
            public string Output { get; set; }

            /// <summary>
            /// How many characters the template actually produced, which is larger than the
            /// length of <see cref="Output"/> when the output was truncated.
            /// </summary>
            public int? OutputLength { get; set; }

            /// <summary>
            /// Whether <see cref="Output"/> was cut short, so the agent does not mistake the
            /// visible tail for where the template stopped producing text.
            /// </summary>
            public bool IsOutputTruncated { get; set; }

            /// <summary>
            /// How to see the rest of the output when it was truncated.
            /// </summary>
            public string TruncationAdvice { get; set; }

            /// <summary>
            /// The reason the render failed when <see cref="IsSuccess"/> is <c>false</c>.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// What the test did and did not exercise, so the agent does not over-trust
            /// a passing render.
            /// </summary>
            public string Coverage { get; set; }
        }

        #endregion Support Classes
    }
}
