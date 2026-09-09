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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
using Rock.Cms;
using Rock.Configuration;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaApplicationBuilderSkill
{
    #region Fields

    /// <summary>
    /// The content type new endpoints declare when the definition does not
    /// name one. Endpoints created through this tool exist to feed authored
    /// components, so they return JSON rather than the historical default of
    /// HTML.
    /// </summary>
    private static readonly string JsonContentType = "application/json";

    #endregion

    #region Tool(s)

    /*
        8/17/2026 - CLAUDE

        This tool is an upsert rather than a separate create and update pair.
        The repo's tool-verb vocabulary has no "Create" anywhere; the
        established write shape is a single AddOrUpdateX plus DeleteX, as in
        AddOrUpdateContentChannelItem, AddOrUpdateNote,
        AddOrUpdatePrayerRequest, AddOrUpdateReminder and AddOrUpdateStep.
        The endpoint's natural key (application slug, endpoint slug, HTTP
        method) decides the path: missing means add, present means update.

        Both paths are gated on ADMINISTRATE of the containing application,
        and the update path re-gates Sql only when this call changes the
        enabled commands. An earlier version also required a provenance
        stamp on the endpoint; see the note in LavaAuthoring.cs for why that
        was removed.

        The containing application must already exist. This tool used to
        create it implicitly, which turned a misspelled applicationSlug into
        a silently created phantom application; now the miss is a loud error
        pointing at AddOrUpdateLavaApplication, matching the parent-child
        shape AddOrUpdateContentChannelItem established.

        Reason: Match the established AddOrUpdateX + DeleteX write shape
        without loosening either path's safety model.
    */
    [Description( "Adds a new Lava endpoint or updates an existing one, keyed by slug and HTTP method, within an existing Lava application. Returns the result of test-executing the template." )]
    [AgentToolPreamble( "Saving the Lava endpoint." )]
    [AgentUsage( "applicationSlug groups a block's endpoints; reuse the same slug for every endpoint of one feature. The application must already exist; create it with AddOrUpdateLavaApplication first." )]
    [AgentUsage( "Endpoints are keyed by slug AND method, so the same slug with Get and with Post are two different endpoints. When the endpoint already exists it is updated in place; otherwise it is created." )]
    [AgentUsage( "An update replaces the whole template, so send the complete Lava rather than a fragment. Read it with GetLavaEndpoint first if you did not write the current version. Omitted definition fields are left unchanged on an update, so a template-only edit cannot quietly change the endpoint's security mode or commands." )]
    [AgentUsage( "Any endpoint in an application the current person can administrate can be updated, including ones built through the admin pages. Read it with GetLavaEndpoint before replacing a template you did not write, and tell the user when you are changing something they or a colleague authored." )]
    [AgentUsage( "definition.enabledLavaCommands must include every command the template uses or the template will fail at runtime. Use 'RockEntity' to read, 'RockEntityModify' to add or update, and 'RockEntityDelete' to delete. These cover almost everything, including charts and totals. A template that starts using a new command needs that command added here too, or it will silently return nothing where the command was." )]
    [AgentUsage( "Do not request 'Sql'. It is refused unless you also pass sqlJustification, which you may only supply after telling the user why the entity commands cannot do the job and getting their explicit approval. Rewriting the template with entity commands is nearly always the correct response to that refusal." )]
    [AgentUsage( "Always pass testParameters when the template reads Body or QueryString, with realistic values, so the parameter path is proven rather than assumed. Without it the test renders with no request data and a template that reads Body.x is only exercised down its missing-parameter branch." )]
    [AgentUsage( "testExecution.isSuccess means only that Lava rendered without an exception. It does not mean expected records were returned, that option items match a control's required shape, or that a JSON success flag is true. State the expected test outcome first, inspect testExecution.output and verificationWarnings, and correct unexpected empty collections or business failures before using the endpoint." )]
    [AgentUsage( "Do not save a placeholder endpoint whose normal valid-input path always returns success false, not implemented, or instructions to research later. Research and implement the real boundary before saving it. If a concrete blocker remains, preserve working state and report that exact blocker." )]
    [AgentUsage( "Enabling RockEntityModify or RockEntityDelete turns automatic test execution off because running it would perform real writes. This is an expected safety constraint, not a failed or unverifiable build and not a reason to stop. Keep write endpoints small, put read and option logic in separate RockEntity-only endpoints that can be tested, inspect the write against the retrieved contracts and domain behavior, then verify it through the safest available real workflow." )]
    [AgentUsage( "By default an endpoint inherits the application's read audience (ApplicationView). When one endpoint must be callable by a narrower or different set of people than the rest of the application, for example a write endpoint only leaders may call, pass definition.audiences with one or more values ('Public', 'AllAuthenticatedPeople', or exact security role names, mapped with ResolveAudience). That switches the endpoint to EndpointExecute and writes its own Execute rules. In EndpointExecute mode nobody outside the listed audiences can call it, Rock Administrators included, so add the administrator's role if they need to test it." )]
    [AgentToolGuid( "5F1E8C29-A47B-4D63-B905-E26A1D79F4C8" )]
    public AgentToolResult AddOrUpdateLavaEndpoint(
        [Description( "The slug of the Lava application the endpoint belongs to. Reuse one slug per feature so all of its endpoints group under one application." )]
        string applicationSlug,

        [Description( "The slug of the endpoint to add or update." )]
        string endpointSlug,

        [Description( "The definition of the endpoint: its Lava template, HTTP method, security mode, optional endpoint-specific audiences, enabled Lava commands and content type. On an update, omitted fields are left unchanged." )]
        LavaEndpointDefinition definition,

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
        var isSettingCommands = enabledLavaCommands != null && !enabledLavaCommands.ClearValue;

        if ( codeTemplate.IsNullOrWhiteSpace() )
        {
            helper.AddError( "A Lava template is required." );
        }
        else if ( !TryLintTemplate( codeTemplate, out var lintError ) )
        {
            helper.AddError( lintError );
        }

        if ( !TryGetSecurityMode( definition?.SecurityMode, out var securityMode, out var securityModeError ) )
        {
            helper.AddError( securityModeError );
        }

        /*
            9/8/2026 - CLAUDE

            Endpoint-level audiences exist for the endpoint whose callers
            differ from the rest of its application: the one write endpoint
            in a dashboard everyone on staff can read. They only mean
            anything in EndpointExecute mode, because the application modes
            never consult the endpoint's own rules, so passing them together
            with a different explicit mode is a contradiction the caller has
            to resolve rather than something to guess at. When no mode is
            given, audiences imply EndpointExecute.

            Reason: Per-endpoint security without a silent mode change.
        */
        var hasEndpointAudiences = definition?.Audiences?.Any( a => a.IsNotNullOrWhiteSpace() ) == true;
        List<AudienceGrant> endpointAudienceGrants = null;

        if ( hasEndpointAudiences )
        {
            if ( definition.SecurityMode.IsNotNullOrWhiteSpace() && securityMode != LavaEndpointSecurityMode.EndpointExecute )
            {
                helper.AddError( $"definition.audiences only applies when the endpoint answers for itself, but securityMode was set to {securityMode}. Either omit securityMode (audiences implies EndpointExecute) or omit audiences and let the application's security govern." );
            }
            else if ( TryResolveAudiences( rockContext, definition.Audiences, out endpointAudienceGrants, out var endpointAudienceError ) )
            {
                securityMode = LavaEndpointSecurityMode.EndpointExecute;
            }
            else
            {
                helper.AddError( endpointAudienceError );
            }
        }

        if ( !TryGetHttpMethod( definition?.HttpMethod, out var method, out var httpMethodError ) )
        {
            helper.AddError( httpMethodError );
        }

        /*
            8/17/2026 - CLAUDE

            Only what this call is asking for is checked. Leaving
            enabledLavaCommands unset on an update means the stored commands
            are untouched, so an endpoint whose SQL the user already approved
            is not re-litigated on every template edit. Asking for Sql here,
            on either path, goes through the refusal below, and a template
            that starts using {% sql %} without the command being enabled
            cannot run it anyway. The refusal happens before anything is
            written, so the round trip through the user happens instead of an
            endpoint existing that has to be walked back.

            Reason: Gate the request to change commands, not every edit to
            the template.
        */
        if ( isSettingCommands
            && !TryValidateSqlUsage( enabledLavaCommands.Value, sqlJustification, out var sqlError ) )
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

        if ( application == null )
        {
            helper.AddError( $"No Lava application exists with the slug '{applicationSlug}'. Create it first with {nameof( AddOrUpdateLavaApplication )}." );

            return helper.ErrorResult;
        }

        if ( !IsAuthorizedToAuthor( application ) )
        {
            helper.AddError( $"You are not authorized to administrate the '{applicationSlug}' Lava application." );

            return helper.ErrorResult;
        }

        // The endpoint's natural key decides the path: missing means add,
        // present means update.
        var endpoint = application.LavaEndpoints.FirstOrDefault( e => e.Slug == endpointSlug && e.HttpMethod == method );

        var isNewEndpoint = endpoint == null;
        if ( isNewEndpoint )
        {
            endpoint = new LavaEndpoint
            {
                LavaApplication = application,
                Name = endpointSlug,
                Slug = endpointSlug,
                HttpMethod = method,
                CodeTemplate = codeTemplate,
                EnabledLavaCommands = isSettingCommands ? enabledLavaCommands.Value.ToStringSafe() : string.Empty,
                SecurityMode = securityMode,
                IsActive = true
            };

            // These endpoints exist to feed components, so they default to
            // JSON. Cross-site forgery protection stays on, which is what
            // useLavaApp sends the header for.
            endpoint.SetAdditionalSettings( new LavaEndpointAdditionalSettings
            {
                EnableCrossSiteForgeryProtection = true,
                ContentType = definition.ContentType.IsNotNullOrWhiteSpace() ? definition.ContentType : JsonContentType
            } );

            new LavaEndpointService( rockContext ).Add( endpoint );
        }
        else
        {
            endpoint.CodeTemplate = codeTemplate;

            // Security mode, commands and content type are left alone when
            // the definition does not mention them, so a template-only edit
            // cannot quietly change who is allowed to run the endpoint or
            // what it may do. Audiences imply EndpointExecute.
            if ( definition.SecurityMode.IsNotNullOrWhiteSpace() || hasEndpointAudiences )
            {
                endpoint.SecurityMode = securityMode;
            }

            helper.UpdateProperty( endpoint, e => e.EnabledLavaCommands, enabledLavaCommands );

            if ( definition.ContentType.IsNotNullOrWhiteSpace() )
            {
                // Only the content type changes; the stored cross-site
                // forgery protection setting rides along untouched.
                var additionalSettings = endpoint.GetAdditionalSettings<LavaEndpointAdditionalSettings>() ?? new LavaEndpointAdditionalSettings();

                additionalSettings.ContentType = definition.ContentType;
                endpoint.SetAdditionalSettings( additionalSettings );
            }
        }

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

        // The endpoint has to be saved before it can be rigged, because the
        // Auth rows reference its Id.
        if ( endpointAudienceGrants != null )
        {
            SetAudienceRules( rockContext, endpoint.TypeId, endpoint.Id, Authorization.EXECUTE, endpointAudienceGrants );
        }

        var url = GetEndpointUrl( application.Slug, endpoint.Slug );

        var invocationExample = method == LavaEndpointHttpMethod.Get
            ? $"`lavaApp.invoke(\"{endpoint.Slug}\", parametersOrUndefined, {{ method: \"GET\" }})`"
            : $"`lavaApp.invoke(\"{endpoint.Slug}\", payload)`";

        var testExecution = TestExecute( codeTemplate, endpoint.EnabledLavaCommands, application, method, parsedTestParameters, maxTestOutputLength );

        var result = Success( new LavaEndpointSaveResult
        {
            ApplicationSlug = application.Slug,
            EndpointSlug = endpoint.Slug,
            Method = method.ToString(),
            Url = url,
            TestExecution = testExecution
        } )
            .WithHistoryContent( new LavaEndpointReferenceResult
            {
                ApplicationSlug = application.Slug,
                EndpointSlug = endpoint.Slug,
                Method = method.ToString(),
                Url = url
            }, "lava-endpoint" )
            .WithInstructions( $"The '{endpoint.Slug}' {method} endpoint was {( isNewEndpoint ? "created" : "updated" )}. Invoke it from useLavaApp with {invocationExample}. invoke defaults to Post, so a Get endpoint must pass the method option explicitly. Before reporting the integration as verified, compare the component's application slug, endpoint slug, method, parameters, and expected response shape with this saved endpoint." );

        if ( testExecution.IsSkipped )
        {
            result.WithInstructions( "The endpoint was not executed. Do not describe it as tested or passing. Review the complete template and verify it through the safest available real workflow." );
        }
        else if ( !testExecution.IsSuccess )
        {
            result.WithInstructions( "The endpoint was saved, but its test render failed. Fix the reported failure and call AddOrUpdateLavaEndpoint again before connecting a dependent component or reporting completion." );
        }
        else
        {
            result.WithInstructions( "testExecution.isSuccess confirms only that Lava rendered without an exception. Inspect the full output and verificationWarnings against the expected scenario. When records are expected, require a nonempty collection and inspect at least one complete item for the exact keys and value shapes the component consumes." );
        }

        /*
            8/17/2026 - CLAUDE

            A brand new endpoint in EndpointExecute mode is uncallable in a
            way that needs its own advice. (The equivalent gap for a brand
            new application is reported by AddOrUpdateLavaApplication, which
            is where applications are created now.)

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
        if ( endpointAudienceGrants != null )
        {
            result.WithInstructions( $"The '{endpoint.Slug}' endpoint now answers for itself (EndpointExecute) and can be executed by {DescribeAudienceGrants( endpointAudienceGrants )}. The application's audience does not apply to it, and in this mode Rock Administrators and Lava Application Developers are not automatically allowed either: an administrator outside these audiences gets a 401. If the user needs to test it as themselves, add their role to definition.audiences. To change the audiences later, call this tool again with a new list; it replaces the current one." );
        }
        else if ( isNewEndpoint && endpoint.SecurityMode == LavaEndpointSecurityMode.EndpointExecute )
        {
            result.WithInstructions( $"The '{endpoint.Slug}' endpoint uses the EndpointExecute security mode and has no authorization rules, so nobody can call it yet, administrators included. Either call AddOrUpdateLavaEndpoint again with definition.audiences naming who may call it, grant Execute on the endpoint through the Lava Applications admin pages, or set the definition's securityMode to ApplicationView so it defers to the application. Tell the user this before they test the page, because the call will fail with a 401 rather than an error they can read." );
        }

        // A write endpoint left in ApplicationView mode is runnable by the
        // application's whole read audience, which AddOrUpdateLavaApplication
        // may have rigged as broadly as the anonymous public. Only speak up
        // when this call created that state (a new endpoint, or a change to
        // the commands or mode), so template-only edits are not nagged.
        var isSecurityShapeChangedByThisCall = isNewEndpoint
            || isSettingCommands
            || definition.SecurityMode.IsNotNullOrWhiteSpace();

        if ( isSecurityShapeChangedByThisCall
            && endpoint.SecurityMode == LavaEndpointSecurityMode.ApplicationView
            && IsWriteCapable( endpoint.EnabledLavaCommands ) )
        {
            result.WithInstructions( $"The '{endpoint.Slug}' endpoint can write data but uses the ApplicationView security mode, so everyone in the application's read audience can trigger its writes. If the read audience is broader than the people who should write, call AddOrUpdateLavaEndpoint again with definition.audiences naming the roles that may call this endpoint (which switches it to EndpointExecute), or set the definition's securityMode to ApplicationEdit and tell the user that ApplicationEdit endpoints are callable only by Rock Administrators and Lava Application Developers until an administrator grants ExecuteEdit rights on the application." );
        }

        // Only when this call is what asked for SQL: on a new endpoint that
        // is whatever the definition enabled, and on an update it is only a
        // change to the commands. An endpoint that already had SQL approved
        // does not need the warning repeated on every template edit. The
        // justification was given to the tool, not to the user, so require
        // that it be repeated out loud rather than trusting it was already
        // discussed.
        var isSqlEnabledByThisCall = isNewEndpoint
            ? IsSqlRequested( endpoint.EnabledLavaCommands )
            : isSettingCommands && IsSqlRequested( enabledLavaCommands.Value );

        if ( isSqlEnabledByThisCall )
        {
            result.WithInstructions( GetSqlApprovalInstructions( endpoint.Slug, sqlJustification ) );
        }

        return result;
    }

    #endregion
}
