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
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.LavaApplicationSkill;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/*
    8/17/2026 - CLAUDE

    Companion to CustomComponentSkill. An authored Custom Component needs
    data, and hunting for an existing REST endpoint is the worst-shaped step
    in that flow: Rock has hundreds of endpoints, almost none return the
    shape a specific dashboard wants, and their permissions are separate from
    the page's. Writing Lava avoids all three, so this skill creates the
    endpoint instead of searching for one.

    Endpoint authoring is a structural, privileged change, so each tool is
    gated on ADMINISTRATE of the target Lava application. A brand new
    application has no authority to check (LavaApplication deliberately
    breaks security inheritance), so creating one requires membership in the
    roles LavaApplication itself treats as overrides.

    Every write test-executes the template and returns the result. That is
    the point of the skill: the agent finds out the template is broken while
    it can still fix it, rather than a visitor seeing Lava error text later.
    The one exception is a template that enables a write-capable command,
    which is never test-executed; see TestExecute for why.

    Everything this skill creates is stamped with a ForeignKey provenance
    value, and the tools that change or remove existing records refuse any
    record that does not carry it. That stamp is the entire safety model for
    destructive operations: the skill can rework and unwind its own work and
    nothing else.

    Reason: MCP-driven Lava endpoint authoring that feeds the Custom
    Component flow, gated on ADMINISTRATE and scoped by provenance.
*/

/// <summary>
/// Agent skill that creates and edits <see cref="LavaEndpoint"/> records so
/// an authored Custom Component has a data source shaped for exactly what it
/// renders.
/// </summary>
[Description( "Create and edit Lava applications and endpoints that return JSON data to authored components." )]
[AgentPurpose( "Create the data endpoints an authored Custom Component calls, by writing Lava rather than searching for an existing REST endpoint." )]
[AgentUsage( "When an authored Custom Component needs data, create a Lava application with AddOrUpdateLavaApplication, then create its endpoints with AddOrUpdateLavaEndpoint. Do not search for an existing Rock REST endpoint first; write the Lava that returns exactly the JSON the component renders." )]
[AgentUsage( "Create one application per block, named after the dashboard, and group all of the block's endpoints under it by passing the same applicationSlug each time. Use GetLavaApplication to see what an application already contains before adding to it." )]
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
[AgentSkillGuid( "8660E7C0-1101-4058-BAF5-20B860600027" )]
[EntityTypeGuid( "CABB72CF-DD09-48CD-9BB9-4819488BC7CA" )]
internal sealed partial class LavaApplicationSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The version segment of the Lava application route. It is part of the
    /// route itself and is not related to the application being addressed.
    /// </summary>
    private static readonly string RouteVersion = "1";

    /// <summary>
    /// The ForeignKey value stamped on applications and endpoints this skill
    /// creates. The destructive tools only accept records carrying it, so
    /// the skill can clean up after itself without being able to change or
    /// delete anything a person authored. This literal predates the class
    /// rename from LavaDataSkill and must never change: stamped rows exist,
    /// and the string is an opaque provenance token, not a class reference.
    /// </summary>
    private static readonly string AgentProvenanceKey = "AI-Agent:LavaDataSkill";

    /// <summary>
    /// The permission key of the raw SQL Lava command, as returned by
    /// <c>SqlBlock.RequiredPermissionKey</c>. This is the command that
    /// requires the user's approval before an endpoint may use it.
    /// </summary>
    private static readonly string SqlCommandName = "Sql";

    /// <summary>
    /// The Lava command that lets a template add or update entities.
    /// </summary>
    private static readonly string RockEntityModifyCommandName = "RockEntityModify";

    /// <summary>
    /// The Lava command that lets a template delete entities.
    /// </summary>
    private static readonly string RockEntityDeleteCommandName = "RockEntityDelete";

    /// <summary>
    /// The longest test execution output handed back to the agent. A template
    /// that returns a whole dashboard payload can produce hundreds of
    /// kilobytes, which is far more than is needed to tell whether the
    /// template worked and is enough to overflow the tool result on its own.
    /// </summary>
    private const int MaxTestOutputLength = 2000;

    /// <summary>
    /// The hard ceiling on test execution output, reachable by passing
    /// maxTestOutputLength. High enough for a diagnostic dump, low enough
    /// that a dashboard payload cannot flood the tool result.
    /// </summary>
    private const int MaxAllowedTestOutputLength = 10000;

    /*
        8/17/2026 - CLAUDE

        An earlier version of this skill only advised against SQL, in a
        single line of AgentUsage text that said to add it "when the query
        genuinely needs it". That failed in practice: asked for a dashboard
        with charts and a delete action, an agent chose SQL for two of three
        endpoints, and it neither asked the user nor explained itself. Two
        causes, both fixed here.

        The guidance never named the entity write commands, so for a delete
        or an update the agent had no alternative to offer itself. And
        "genuinely needs it" is a judgment the model resolves in favor of
        whatever is easiest, which for a chart rollup is a GROUP BY.

        Advice alone cannot fix that, so this is enforced. The tools refuse a
        request for the Sql command unless the caller also passes a
        justification, which forces a round trip through the user. This
        matters because raw SQL bypasses Rock's per-row entity security: the
        endpoint runs as whoever views the page, and '{% sql %}' returns
        every matching row regardless of that person's rights. The message
        below is the whole intervention: it has to name the alternative
        commands and answer the aggregation excuse, or the agent will just
        retry with a justification that repeats the excuse.

        Reason: Advisory text did not stop the agent from silently choosing
        raw SQL, which bypasses per-row entity security.
    */
    private static readonly string SqlRequiresApprovalMessage = @"This endpoint requests the 'Sql' Lava command, which needs the user's explicit approval before it can be saved.

Raw SQL bypasses Rock's per-row security. The endpoint runs as whoever views the page, and '{% sql %}' returns every row the query matches regardless of that person's rights. The entity commands filter results by the viewer automatically, so a mistake in SQL leaks data to every visitor who can call the endpoint.

Do this with entity commands instead. Replace 'connectionrequest' with the entity's friendly name with the spaces removed:
  Read: {% connectionrequest where:'ConnectionStatusId == 3' %} ... {% endconnectionrequest %}   requires the 'RockEntity' command
  Add or update: {% modifyconnectionrequest id:'5' %} ... {% endmodifyconnectionrequest %}   requires the 'RockEntityModify' command
  Delete: {% deleteconnectionrequest id:'5' %}   requires the 'RockEntityDelete' command

Charts, counts and totals do not require SQL. Fetch the rows with the entity command and group them in Lava, or return the rows and aggregate them in the component. A join you cannot express directly is usually a nested entity command or a wider query that you filter afterward.

If SQL is genuinely unavoidable, tell the user which endpoint needs it, what the query reads or changes, and why the entity commands cannot express it. Once they approve, call this tool again and pass that explanation as sqlJustification.";

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LavaApplicationSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public LavaApplicationSkill( ILogger<LavaApplicationSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines if the acting person may author the specified Lava
    /// application and its endpoints.
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

        // An existing application is checked the same way other structural
        // skills check the thing they are about to change: ADMINISTRATE of
        // the target.
        if ( application != null )
        {
            return LavaApplicationCache.Get( application.Id )?.IsAuthorized( Authorization.ADMINISTRATE, person ) == true;
        }

        // A new application has nothing to check against, because
        // LavaApplication intentionally returns a null ParentAuthority to
        // break inheritance. Fall back to the roles LavaApplication itself
        // treats as authorization overrides.
        return RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).IsPersonInRole( person.Guid )
            || RoleCache.Get( Rock.SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() ).IsPersonInRole( person.Guid );
    }

    /// <summary>
    /// Loads an endpoint by application slug, endpoint slug and HTTP method,
    /// checking authorization along the way. Any failure is recorded on the
    /// helper and <c>null</c> is returned.
    /// </summary>
    /// <param name="helper">The helper that accumulates errors for the current tool call.</param>
    /// <param name="rockContext">The context to load the endpoint from.</param>
    /// <param name="applicationSlug">The slug of the Lava application.</param>
    /// <param name="endpointSlug">The slug of the endpoint.</param>
    /// <param name="httpMethod">The HTTP method of the endpoint, or <c>null</c> to default to Post.</param>
    /// <returns>The endpoint when it was found and the acting person is authorized; otherwise <c>null</c>.</returns>
    private LavaEndpoint GetAuthorizedEndpoint( AgentToolHelper helper, RockContext rockContext, string applicationSlug, string endpointSlug, string httpMethod )
    {
        if ( applicationSlug.IsNullOrWhiteSpace() || endpointSlug.IsNullOrWhiteSpace() )
        {
            helper.AddError( "An application slug and an endpoint slug are both required." );

            return null;
        }

        if ( !TryGetHttpMethod( httpMethod, out var method, out var httpMethodError ) )
        {
            helper.AddError( httpMethodError );

            return null;
        }

        var application = new LavaApplicationService( rockContext )
            .Queryable()
            .FirstOrDefault( a => a.Slug == applicationSlug );

        if ( application == null )
        {
            helper.AddError( $"No Lava application exists with the slug '{applicationSlug}'." );

            return null;
        }

        if ( !IsAuthorizedToAuthor( application ) )
        {
            helper.AddError( $"You are not authorized to administrate the '{applicationSlug}' Lava application." );

            return null;
        }

        var endpoint = application.LavaEndpoints
            .FirstOrDefault( e => e.Slug == endpointSlug && e.HttpMethod == method );

        if ( endpoint == null )
        {
            helper.AddError( $"No endpoint exists at '{applicationSlug}/{endpointSlug}' for the {method} method." );

            return null;
        }

        return endpoint;
    }

    /// <summary>
    /// Renders the template so the agent finds out it is broken while it can
    /// still fix it, instead of a visitor seeing Lava error text later.
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
            8/17/2026 - CLAUDE

            A template that can write is never test-executed. Rendering it
            performed real inserts, updates and deletes, unattributed, which
            is an unacceptable price for a syntax check.

            Rolling the render back in a transaction was considered and
            rejected. It is achievable (the entity blocks take their
            RockContext from the Lava context, so seeding it and wrapping the
            render in a transaction would cover the SQL), but it cannot be
            made honest: Rock updates and flushes caches during save, and
            those caches are not restored by a rollback, so a "dry run" would
            leave the instance describing rows that no longer exist.
            Post-save hooks that queue bus messages, RealTime notifications
            and workflows fire regardless of the rollback as well.

            Detection keys off enabledLavaCommands rather than the template
            text because a write cannot execute unless its command is
            enabled, which makes this exact rather than a guess about markup.

            Reason: Test-executing a write-capable template performs real,
            unattributed writes, and a partial rollback would be more
            dangerous than not testing at all.
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
            8/17/2026 - CLAUDE

            Endpoints read their request values from the Body merge field
            (QueryString for Get), and an end-to-end test proved that path is
            exactly the one this test could never exercise: with no request
            context, a template reading Body.teamId only ever runs its
            missing-parameter branch, and the agent ships the endpoint on
            faith. The simulated field mirrors
            LavaApplicationRequestHelpers.RequestToDictionary, which adds the
            parsed JSON body as a single "Body" merge field, so a template
            that passes here reads its parameters the same way it will on a
            real request.

            Reason: The parameter path is the likeliest silent failure and
            was untestable without a simulated request field.
        */
        var requestFieldName = method == LavaEndpointHttpMethod.Get ? "QueryString" : "Body";

        var coverage = testParameters != null
            ? $"Rendered as the current person with a simulated {requestFieldName} merge field built from testParameters. Other request merge fields (Headers, Cookies, RawUrl) were not available."
            : "Rendered as the current person with no HTTP request context, so the Request, QueryString, Body and Headers merge fields were not available. A template that reads Body or QueryString was only exercised down its missing-parameter branch; pass testParameters to prove the parameter path.";

        /*
            8/17/2026 - CLAUDE

            Everything belongs inside the try, including building the merge
            fields and the render context. Those steps looked incapable of
            failing and were originally left outside it, but a null
            ConfigurationRiggingJson made the parse throw, and because the
            throw happened after the caller had already saved its changes,
            the agent was told the whole tool call failed while the endpoint
            sat in the database. A skill whose job is reporting whether a
            template works must never itself be the thing that throws.

            Reason: A failure while preparing the render is a test result,
            not a tool failure.
        */
        try
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( null, AgentRequestContext.CurrentPerson );

            // Only parse rigging that is actually there. The parser throws
            // on null instead of returning null, so an application with no
            // rigging would fail the render for a reason that has nothing to
            // do with the template being tested.
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
                8/17/2026 - CLAUDE

                The engine's default strategy renders the exception into the
                output, which would let a broken template look like it
                succeeded and produced text. This sets Throw for this render
                only, so a failure is a failure. The engine's global strategy
                is deliberately left alone.

                Reason: A silently swallowed error defeats the purpose of
                test-executing.
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

            return BuildRenderedTestResult( renderResult.Text, coverage, maxTestOutputLength );
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
    /// Appends a hint to the Lava error messages whose real cause is
    /// somewhere other than where the message points, so the agent's next
    /// attempt is a fix rather than another guess.
    /// </summary>
    /// <param name="message">The error message the engine produced.</param>
    /// <returns>The message, with a hint appended when one applies.</returns>
    private static string AugmentLavaError( string message )
    {
        if ( message.IsNullOrWhiteSpace() )
        {
            return message;
        }

        // The engine reports a where clause containing a dotted navigation
        // path as a generic invalid-expression error with no mention of the
        // cause.
        if ( message.IndexOf( "Where expression is invalid", StringComparison.OrdinalIgnoreCase ) >= 0 )
        {
            return message + " Hint: dotted navigation paths (for example 'Group.CampusId') are not supported in where clauses, even though they work in sort, groupby and select. Resolve the related ids in a first query and filter on a scalar property or a literal OR clause.";
        }

        // An unrecognized block tag is reported as a missing end tag
        // somewhere else in the template, which sends the agent to the wrong
        // line.
        if ( message.IndexOf( "was expected", StringComparison.OrdinalIgnoreCase ) >= 0 )
        {
            return message + " Hint: this usually means a block tag was not recognized, so its end tag broke the surrounding structure. Entity command blocks use the entity's own name ('{% group %}...{% endgroup %}'), and the command must also be listed in enabledLavaCommands.";
        }

        return message;
    }

    /// <summary>
    /// Builds the result for a template that rendered, trimming the output
    /// to something an agent can actually read.
    /// </summary>
    /// <param name="output">The full text the template produced.</param>
    /// <param name="coverage">The description of what the test did and did not exercise.</param>
    /// <param name="maxTestOutputLength">The caller's requested output budget, or <c>null</c> for the default.</param>
    /// <returns>A successful result whose output is no longer than the effective limit.</returns>
    private static TestExecutionResult BuildRenderedTestResult( string output, string coverage, int? maxTestOutputLength )
    {
        var fullText = output.ToStringSafe();

        // Diagnostics legitimately need more than the default, so the caller
        // can raise the budget, but a whole dashboard payload still cannot
        // flood the tool result.
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

        // A dashboard endpoint can render hundreds of kilobytes. Returning
        // all of it pushes the tool result past what the caller can accept,
        // which turns a passing test into an apparent failure, so keep the
        // head of it and say how much was cut.
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
    /// Builds the application result shared by GetLavaApplication and
    /// AddOrUpdateLavaApplication: the application itself plus a summarized
    /// list of its endpoints, templates excluded.
    /// </summary>
    /// <param name="application">The application to describe.</param>
    /// <returns>The detail result.</returns>
    private LavaApplicationDetailResult CreateApplicationDetailResult( LavaApplication application )
    {
        var endpoints = application.LavaEndpoints
            .OrderBy( e => e.Slug )
            .ThenBy( e => e.HttpMethod )
            .Select( e => new LavaEndpointSummaryResult
            {
                EndpointSlug = e.Slug,
                Method = e.HttpMethod.ToString(),
                Name = e.Name,
                SecurityMode = e.SecurityMode.ToString(),
                IsActive = e.IsActive,
                Url = GetEndpointUrl( application.Slug, e.Slug )
            } )
            .ToList();

        return new LavaApplicationDetailResult
        {
            Id = application.Id,
            Guid = application.Guid,
            Name = application.Name,
            ApplicationSlug = application.Slug,
            Description = application.Description,
            IsActive = application.IsActive,
            Endpoints = endpoints
        };
    }

    /// <summary>
    /// Rejects template mistakes whose runtime failures point somewhere
    /// other than the real cause, so the agent hears about them while the
    /// template is still in hand.
    /// </summary>
    /// <param name="codeTemplate">The template about to be saved.</param>
    /// <param name="errorMessage">Contains the explanation when <c>false</c> is returned.</param>
    /// <returns><c>true</c> when no lint problem was found.</returns>
    private static bool TryLintTemplate( string codeTemplate, out string errorMessage )
    {
        errorMessage = null;

        /*
            8/17/2026 - CLAUDE

            There is no generic '{% entity <name> %}' block tag; the entity
            commands register one tag per entity name. The engine reports the
            unknown tag as a missing end tag somewhere else entirely, so an
            agent that writes it burns a round trip on a misleading error. An
            end-to-end test hit exactly this.

            Reason: Catch a known-wrong tag before it is saved, with the real
            fix named.
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
    /// Parses the request values the caller wants simulated during test
    /// execution, using the same deserialization the real request pipeline
    /// applies to a JSON body.
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
            // The same call LavaApplicationRequestHelpers uses on a real JSON
            // body, so the simulated Body behaves like the one a component's
            // invoke produces.
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
    /// Determines whether the requested commands allow the template to
    /// change data. A template can only write when its write command is
    /// enabled, so this is exact rather than an inspection of the markup.
    /// </summary>
    /// <param name="enabledLavaCommands">The comma-delimited commands the caller asked for.</param>
    /// <returns><c>true</c> if the template is able to insert, update or delete.</returns>
    private static bool IsWriteCapable( string enabledLavaCommands )
    {
        // Sql is deliberately absent. It can write, but it is already gated
        // behind the user's explicit approval, and endpoints that use it are
        // overwhelmingly reads that would lose their only syntax check for
        // no gain in safety.
        return enabledLavaCommands
            .SplitDelimitedValues()
            .Any( c => c.Equals( RockEntityModifyCommandName, StringComparison.OrdinalIgnoreCase )
                || c.Equals( RockEntityDeleteCommandName, StringComparison.OrdinalIgnoreCase ) );
    }

    /// <summary>
    /// Rejects a request for the raw SQL command that does not carry the
    /// user's approval, so choosing SQL costs a round trip through the
    /// person who has to live with it.
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
    /// Builds the guidance attached to an endpoint that was allowed to use
    /// raw SQL, so the justification reaches the user rather than staying
    /// between the agent and the tool.
    /// </summary>
    /// <param name="endpointSlug">The slug of the endpoint that uses SQL.</param>
    /// <param name="sqlJustification">The explanation the caller supplied.</param>
    /// <returns>The instruction text to attach to the result.</returns>
    private static string GetSqlApprovalInstructions( string endpointSlug, string sqlJustification )
    {
        return $"The '{endpointSlug}' endpoint was saved with the raw SQL command enabled, on this justification: {sqlJustification} State plainly in your reply that this endpoint uses raw SQL, repeat that justification, and warn that raw SQL does not honor the viewer's per-row permissions, so the template itself is responsible for every filter. If the user did not already approve this, say so rather than presenting it as settled.";
    }

    /// <summary>
    /// Parses the HTTP method supplied by the agent. Endpoints are keyed by
    /// slug and method, so getting this wrong addresses the wrong endpoint
    /// rather than failing loudly, which is why an unrecognized value is
    /// rejected instead of coerced.
    /// </summary>
    /// <param name="httpMethod">The HTTP method name, or <c>null</c> to use the default.</param>
    /// <param name="method">Contains the parsed method when <c>true</c> is returned.</param>
    /// <param name="errorMessage">Contains the error message when <c>false</c> is returned.</param>
    /// <returns><c>true</c> if the HTTP method was parsed.</returns>
    private static bool TryGetHttpMethod( string httpMethod, out LavaEndpointHttpMethod method, out string errorMessage )
    {
        errorMessage = null;

        // Post is the default because that is what useLavaApp sends when a
        // component does not ask for anything else.
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
            8/17/2026 - CLAUDE

            The default used to be EndpointExecute, matching the entity
            default, on the theory that keeping authorization on the endpoint
            was safest. In practice a new endpoint has no authorization rules
            and the authorization walk never reaches LavaApplicationCache's
            role override, so the "safe" default was an endpoint nobody could
            call, failing as a bare 401, and every agent hit it. The default
            is now ApplicationView so the endpoint defers to the application.

            REVISIT: ApplicationView has not fixed this, only moved it. This
            skill rigs no security at all. AddOrUpdateLavaEndpoint builds the
            application with an empty ConfigurationRigging and no Auth rows:

            - ApplicationView authorizes against the application's
              EXECUTE_VIEW action.
            - A new application has no Auth rows, LavaApplication's
              ParentAuthority is deliberately null (see
              LavaApplication.Logic.cs), and Model.IsAllowedByDefault grants
              only VIEW and TAG, so EXECUTE_VIEW denies.
            - LavaApplication.IsAuthorized overrides for Rock Administrators
              and Lava Application Developers on View/Edit/Administrate but
              explicitly NOT on Execute.

            So a freshly created endpoint still cannot be called by anyone,
            administrators included, and the default path is silent because
            the WithInstructions warning in AddOrUpdateLavaEndpoint fires
            only for EndpointExecute. The fix is for these tools to set the
            authorization rather than describe it, likely by taking the
            intended audience as a parameter (staff, all authenticated
            people, or public) and writing the matching EXECUTE_VIEW Auth
            rows when the application is created. Until then, do not tell the
            user security is handled.

            Reason: The default security mode still yields an uncallable
            endpoint, and does so without warning.
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

    #endregion
}
