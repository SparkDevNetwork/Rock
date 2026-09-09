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
using System.Linq;

using Newtonsoft.Json.Linq;

using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
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

    The shared helpers of LavaApplicationBuilderSkill (the class-level
    attributes and constructor live in LavaApplicationBuilderSkill.cs; the
    tool methods live in the per-tool partials). An authored
    Forge Content component needs data, and hunting for an existing REST
    endpoint is the worst-shaped step in that flow: Rock has hundreds of
    endpoints, almost none return the shape a specific dashboard wants, and
    their permissions are separate from the page's. Writing Lava avoids all
    three, so these tools create the endpoint instead of searching for one.

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

    Authorization is the whole safety model. An earlier version also stamped
    a provenance value into ForeignKey and refused to change or delete
    anything without it; that repurposed a column meant for foreign-system
    identifiers, diverged from the authorization-only shape every other
    shipped delete tool uses, and left the agent unable to help with any
    application built through the admin pages. A skill may now change any
    application the acting person can administrate, and the destructive
    tools rely on confirm-first usage guidance the way the Cms skill's do.

    Reason: MCP-driven Lava endpoint authoring that feeds the Forge Content
    flow, gated on ADMINISTRATE.
*/

internal sealed partial class LavaApplicationBuilderSkill
{
    #region Fields

    /// <summary>
    /// The version segment of the Lava application route. It is part of the
    /// route itself and is not related to the application being addressed.
    /// </summary>
    private static readonly string RouteVersion = "1";

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
    /// The audience keyword that grants execute-view to everyone, including
    /// anonymous visitors.
    /// </summary>
    private static readonly string PublicAudienceKeyword = "Public";

    /// <summary>
    /// The audience keyword that grants execute-view to anyone who is logged
    /// in, regardless of role membership.
    /// </summary>
    private static readonly string AllAuthenticatedAudienceKeyword = "AllAuthenticatedPeople";

    /// <summary>
    /// The most security roles named in an audience resolution error. Enough
    /// to choose from, small enough that a large instance's role list cannot
    /// flood the tool result.
    /// </summary>
    private const int MaxAudienceRoleSuggestions = 25;

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

    /*
        9/1/2026 - CLAUDE

        The audience is resolved inside the tool instead of through a
        role-listing tool. The agent proposes an audience while building a
        dashboard; it almost never needs to browse roles first, and when a
        role name misses, the error below carries the candidate roles with
        their descriptions. Discovery through the recovery hint keeps the
        agent's tool count down and only spends the tokens when the list is
        actually needed.

        Roles are matched against IsSecurityRole rather than the Security
        Role group type, because any group can be marked as a security role
        and the authorization engine honors the flag, not the type.

        Reason: Resolve-or-suggest inside the tool replaces a separate role
        discovery tool.

        9/8/2026 - CLAUDE

        Two changes on top of that. An application can now be opened to
        several audiences at once (a page for both staff and a volunteer
        team is the common case), so the resolver takes a list and every
        value has to resolve before anything is rigged. And the recovery
        hint turned out not to be enough on its own: a user says "the
        worship team leaders" and the model has to guess a role name to get
        the hint, so ResolveAudience exists to turn that description into
        candidate values up front. Role loading is shared so the tool and
        the resolver see the same roles.

        Reason: Multi-role audiences and description-to-role mapping.
    */

    /// <summary>
    /// Reads the active security roles of this instance, alphabetically.
    /// Shared by audience resolution and by <c>ResolveAudience</c> so both
    /// see the same roles.
    /// </summary>
    /// <param name="rockContext">The context to read security roles from.</param>
    /// <returns>The active security roles.</returns>
    private static List<SecurityRole> GetSecurityRoles( RockContext rockContext )
    {
        return new GroupService( rockContext )
            .Queryable()
            .Where( g => g.IsSecurityRole && g.IsActive )
            .OrderBy( g => g.Name )
            .Select( g => new SecurityRole
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description
            } )
            .ToList();
    }

    /// <summary>
    /// Resolves every audience value the caller supplied to the grants they
    /// name, so an application can be opened to several roles at once. Every
    /// value has to resolve; one unresolvable value fails the whole set, with
    /// candidates, so nothing is rigged half-way.
    /// </summary>
    /// <param name="rockContext">The context to read security roles from.</param>
    /// <param name="audiences">The audience values the caller supplied.</param>
    /// <param name="grants">The resolved grants, de-duplicated, when <c>true</c> is returned.</param>
    /// <param name="errorMessage">The explanation of every value that failed, when <c>false</c> is returned.</param>
    /// <returns><c>true</c> when every audience resolved to exactly one grant.</returns>
    private static bool TryResolveAudiences( RockContext rockContext, List<string> audiences, out List<AudienceGrant> grants, out string errorMessage )
    {
        grants = new List<AudienceGrant>();
        errorMessage = null;

        var roles = GetSecurityRoles( rockContext );
        var errors = new List<string>();

        foreach ( var audience in audiences.Where( a => a.IsNotNullOrWhiteSpace() ) )
        {
            if ( !TryResolveAudience( roles, audience, out var grant, out var audienceError ) )
            {
                errors.Add( audienceError );
                continue;
            }

            // The same role named twice, or Public alongside a role it
            // already covers, must not produce duplicate Auth rows.
            var isDuplicate = grants.Any( g => g.SpecialRole == grant.SpecialRole && g.GroupId == grant.GroupId );

            if ( !isDuplicate )
            {
                grants.Add( grant );
            }
        }

        if ( errors.Any() )
        {
            errorMessage = string.Join( "\n\n", errors );

            return false;
        }

        if ( !grants.Any() )
        {
            errorMessage = $"At least one audience is required. Pass '{PublicAudienceKeyword}', '{AllAuthenticatedAudienceKeyword}', or one or more security role names. Call {nameof( ResolveAudience )} to map a description of the people the page is for onto those values.";

            return false;
        }

        return true;
    }

    /// <summary>
    /// Resolves an audience value to the authorization grant it names:
    /// everyone, all authenticated people, or a single security role.
    /// </summary>
    /// <param name="roles">The active security roles, from <see cref="GetSecurityRoles"/>.</param>
    /// <param name="audience">The audience value the caller supplied.</param>
    /// <param name="grant">The resolved grant when <c>true</c> is returned.</param>
    /// <param name="errorMessage">The explanation, including candidate roles, when <c>false</c> is returned.</param>
    /// <returns><c>true</c> when the audience resolved to exactly one grant.</returns>
    private static bool TryResolveAudience( List<SecurityRole> roles, string audience, out AudienceGrant grant, out string errorMessage )
    {
        grant = null;
        errorMessage = null;

        var normalizedAudience = audience.Trim().Replace( " ", string.Empty );

        if ( normalizedAudience.Equals( PublicAudienceKeyword, StringComparison.OrdinalIgnoreCase ) )
        {
            grant = new AudienceGrant
            {
                SpecialRole = SpecialRole.AllUsers,
                Description = "everyone, including anonymous visitors"
            };

            return true;
        }

        // "AllAuthenticatedUsers" is accepted as a synonym because it is the
        // SpecialRole enum name and models reach for it.
        if ( normalizedAudience.Equals( AllAuthenticatedAudienceKeyword, StringComparison.OrdinalIgnoreCase )
            || normalizedAudience.Equals( SpecialRole.AllAuthenticatedUsers.ToString(), StringComparison.OrdinalIgnoreCase ) )
        {
            grant = new AudienceGrant
            {
                SpecialRole = SpecialRole.AllAuthenticatedUsers,
                Description = "anyone who is logged in"
            };

            return true;
        }

        // Anything else names a security role. Exact name matches win so a
        // role whose name contains another role's name stays addressable.
        var trimmedAudience = audience.Trim();

        var exactMatches = roles
            .Where( r => r.Name.Equals( trimmedAudience, StringComparison.OrdinalIgnoreCase ) )
            .ToList();
        var matches = exactMatches.Any()
            ? exactMatches
            : roles.Where( r => r.Name.IndexOf( trimmedAudience, StringComparison.OrdinalIgnoreCase ) >= 0 ).ToList();

        if ( matches.Count == 1 )
        {
            grant = new AudienceGrant
            {
                GroupId = matches[0].Id,
                Description = $"members of the '{matches[0].Name}' security role"
            };

            return true;
        }

        // Zero or many. Both errors carry the roles to choose from, with
        // descriptions, so the retry does not need another discovery call.
        var candidates = ( matches.Count > 1 ? matches : roles )
            .Take( MaxAudienceRoleSuggestions )
            .Select( r => r.Description.IsNotNullOrWhiteSpace() ? $"'{r.Name}': {r.Description}" : $"'{r.Name}'" )
            .ToList();

        var problem = matches.Count > 1
            ? $"The audience '{trimmedAudience}' matches more than one security role."
            : $"No security role matches the audience '{trimmedAudience}'.";

        errorMessage = $"{problem} Pass '{PublicAudienceKeyword}', '{AllAuthenticatedAudienceKeyword}', or one of these security role names, or call {nameof( ResolveAudience )} with a description of the people the page is for:\n"
            + string.Join( "\n", candidates );

        return false;
    }

    /// <summary>
    /// Describes a set of grants back to the user as one phrase, for example
    /// "members of the 'Staff' security role and anyone who is logged in".
    /// </summary>
    /// <param name="grants">The grants to describe.</param>
    /// <returns>The combined phrase.</returns>
    private static string DescribeAudienceGrants( List<AudienceGrant> grants )
    {
        var descriptions = grants.Select( g => g.Description ).ToList();

        if ( descriptions.Count <= 1 )
        {
            return descriptions.FirstOrDefault() ?? string.Empty;
        }

        if ( descriptions.Count == 2 )
        {
            return $"{descriptions[0]} and {descriptions[1]}";
        }

        return string.Join( ", ", descriptions.Take( descriptions.Count - 1 ) ) + $", and {descriptions.Last()}";
    }

    /// <summary>
    /// Describes who is allowed an action on an entity, from its allow
    /// rules, so a read tool can report who may execute without the agent
    /// having to remember the create-time result.
    /// </summary>
    /// <param name="rockContext">The context to read the rules from.</param>
    /// <param name="entityTypeId">The entity type of the secured entity.</param>
    /// <param name="entityId">The identifier of the secured entity.</param>
    /// <param name="action">The authorization action whose rules are described.</param>
    /// <returns>One phrase per allow rule, in rule order. Empty when the entity has no allow rules for the action.</returns>
    private static List<string> GetAudienceDescriptions( RockContext rockContext, int entityTypeId, int entityId, string action )
    {
        return new AuthService( rockContext )
            .GetAuths( entityTypeId, entityId, action )
            .Where( a => a.AllowOrDeny == "A" )
            .OrderBy( a => a.Order )
            .Select( a => new { a.SpecialRole, GroupName = a.Group != null ? a.Group.Name : null } )
            .ToList()
            .Select( a =>
            {
                if ( a.SpecialRole == SpecialRole.AllUsers )
                {
                    return "everyone, including anonymous visitors";
                }

                if ( a.SpecialRole == SpecialRole.AllAuthenticatedUsers )
                {
                    return "anyone who is logged in";
                }

                if ( a.GroupName.IsNotNullOrWhiteSpace() )
                {
                    return $"members of the '{a.GroupName}' security role";
                }

                // A rule for one specific person. The tools never write these,
                // but an administrator can.
                return "a specific person";
            } )
            .ToList();
    }

    /// <summary>
    /// Removes every authorization rule of an entity that is being deleted.
    /// Auth rows are tied to their entity by loose id, so nothing else removes
    /// them and they would otherwise linger as orphans.
    /// </summary>
    /// <param name="rockContext">The context to delete the rules with. The caller saves.</param>
    /// <param name="entityTypeId">The entity type of the entity being deleted.</param>
    /// <param name="entityId">The identifier of the entity being deleted.</param>
    private static void DeleteAuthRules( RockContext rockContext, int entityTypeId, int entityId )
    {
        var authService = new AuthService( rockContext );

        var rules = authService
            .Get( entityTypeId, entityId )
            .ToList();

        authService.DeleteRange( rules );
    }

    /*
        9/1/2026 - CLAUDE

        A new Lava application has no Auth rows, its ParentAuthority is
        deliberately null, and Model.IsAllowedByDefault grants only VIEW and
        TAG, so EXECUTE_VIEW denies for everyone the cache override does not
        cover. The result was an application that worked for the
        administrator building it (LavaApplicationCache.IsAuthorized grants
        override roles every action, Execute* included) and returned a bare
        401 to every real visitor. Writing the rules here, from the tool's
        required audience parameter, closes that gap at the only moment the
        intended audience is reliably known.

        Passing audiences replaces every existing rule for the action, the
        way the Security dialog would if an administrator rewrote the list.
        The tool descriptions say so, and the agent instructions tell the
        agent to read the current audience first and change it only when the
        user asked for a security change.

        Reason: Endpoints must be callable by the audience the user chose,
        not just by the administrator who built them.
    */

    /// <summary>
    /// Replaces the rules for one action on an entity with one allow rule
    /// per resolved audience and a deny-all tail. Used for an application's
    /// ExecuteView and for an endpoint's Execute.
    /// </summary>
    /// <param name="rockContext">The context to write the rules with.</param>
    /// <param name="entityTypeId">The entity type of the secured entity.</param>
    /// <param name="entityId">The identifier of the secured entity, which must already be saved.</param>
    /// <param name="action">The authorization action being rigged.</param>
    /// <param name="grants">The audiences to grant the action to. Must contain at least one grant.</param>
    private static void SetAudienceRules( RockContext rockContext, int entityTypeId, int entityId, string action, List<AudienceGrant> grants )
    {
        var authService = new AuthService( rockContext );

        var existingRules = authService
            .GetAuths( entityTypeId, entityId, action )
            .ToList();

        foreach ( var rule in existingRules )
        {
            authService.Delete( rule );
        }

        // Public covers every other grant, so when it is present it is the
        // only rule written; the narrower rules would never be evaluated and
        // would only clutter the Security dialog.
        var isPublic = grants.Any( g => g.SpecialRole == SpecialRole.AllUsers );
        var effectiveGrants = isPublic
            ? grants.Where( g => g.SpecialRole == SpecialRole.AllUsers ).Take( 1 ).ToList()
            : grants;

        var order = 0;

        foreach ( var grant in effectiveGrants )
        {
            authService.Add( new Auth
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                Order = order++,
                Action = action,
                AllowOrDeny = "A",
                SpecialRole = grant.SpecialRole,
                GroupId = grant.GroupId
            } );
        }

        // A deny-all tail after an allow-all rule would never be reached, so
        // the public audience is a single rule. The narrower audiences get
        // the tail to make the boundary visible in the Security dialog.
        if ( !isPublic )
        {
            authService.Add( new Auth
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                Order = order,
                Action = action,
                AllowOrDeny = "D",
                SpecialRole = SpecialRole.AllUsers
            } );
        }

        rockContext.SaveChanges();

        // The authorization dictionary caches rules per action and does not
        // observe direct AuthService writes, so refresh it the way the
        // Authorization helpers themselves do.
        Authorization.RefreshAction( entityTypeId, entityId, action, rockContext );
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
        var verificationWarnings = GetVerificationWarnings( fullText );
        var renderOnlyCoverage = $"Render-only verification. This does not prove that expected records were returned, that item shapes match a component, or that a business operation succeeded. {coverage}";

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
                Coverage = renderOnlyCoverage,
                VerificationWarnings = verificationWarnings
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
            Coverage = renderOnlyCoverage,
            VerificationWarnings = verificationWarnings
        };
    }

    /// <summary>
    /// Identifies output states that are syntactically valid but are commonly
    /// mistaken for functional endpoint success.
    /// </summary>
    /// <param name="output">The complete rendered output.</param>
    /// <returns>Warnings the agent must reconcile with the expected scenario.</returns>
    private static List<string> GetVerificationWarnings( string output )
    {
        var warnings = new List<string>();

        if ( output.IsNullOrWhiteSpace() )
        {
            warnings.Add( "The template rendered an empty response body. This does not verify that a caller received usable data." );
            return warnings;
        }

        JToken response;

        try
        {
            response = JToken.Parse( output );
        }
        catch
        {
            // Endpoints can intentionally return non-JSON content, so only
            // inspect JSON when the output actually is JSON.
            return warnings;
        }

        if ( response is JArray rootArray && !rootArray.HasValues )
        {
            warnings.Add( "The JSON response is an empty collection. If the tested scenario expects configured records, investigate and retry before connecting a dependent control." );
        }

        if ( response is not JContainer container )
        {
            return warnings;
        }

        foreach ( var property in container.Descendants().OfType<JProperty>() )
        {
            if ( property.Value is JArray array && !array.HasValues )
            {
                warnings.Add( $"The JSON collection '{property.Path}' is empty. If the tested scenario expects configured records, this does not verify the query or item shape." );
            }

            var isBusinessSuccessProperty = property.Name.Equals( "success", StringComparison.OrdinalIgnoreCase )
                || property.Name.Equals( "isSuccess", StringComparison.OrdinalIgnoreCase );

            if ( isBusinessSuccessProperty
                && property.Value.Type == JTokenType.Boolean
                && !property.Value.Value<bool>() )
            {
                warnings.Add( $"The JSON response reports '{property.Path}' as false. Rendering succeeded, but the business operation did not." );
            }

            if ( warnings.Count >= 10 )
            {
                break;
            }
        }

        return warnings.Distinct().ToList();
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
    /// <param name="rockContext">The context to read the rigged audiences from.</param>
    /// <param name="application">The application to describe.</param>
    /// <returns>The detail result.</returns>
    private LavaApplicationDetailResult CreateApplicationDetailResult( RockContext rockContext, LavaApplication application )
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
                Audiences = e.SecurityMode == LavaEndpointSecurityMode.EndpointExecute
                    ? GetAudienceDescriptions( rockContext, e.TypeId, e.Id, Authorization.EXECUTE )
                    : null,
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
            ReadAudiences = GetAudienceDescriptions( rockContext, application.TypeId, application.Id, LavaApplication.EXECUTE_VIEW ),
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

            ApplicationView authorizes against the application's
            EXECUTE_VIEW action, which AddOrUpdateLavaApplication rigs from
            its required audience parameter (see SetApplicationReadAudience),
            so a new endpoint in this mode is callable by the audience the
            user chose. Rock Administrators and Lava Application Developers
            can always call it through LavaApplicationCache.IsAuthorized's
            role override, which grants every action including Execute*.

            Reason: Default to the application's rigged read audience.
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

    #region Supporting Classes

    /// <summary>
    /// The resolved target of an audience value: either a special role or a
    /// security role group, plus the phrase used to describe the grant back
    /// to the user.
    /// </summary>
    private sealed class AudienceGrant
    {
        /// <summary>
        /// The special role being granted execute-view, or
        /// <see cref="SpecialRole.None"/> when <see cref="GroupId"/> carries
        /// the grant instead.
        /// </summary>
        public SpecialRole SpecialRole { get; set; } = SpecialRole.None;

        /// <summary>
        /// The identifier of the security role group being granted
        /// execute-view, or <c>null</c> when <see cref="SpecialRole"/>
        /// carries the grant instead.
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// The human-readable phrase describing who the grant covers, used
        /// in the tool's follow-up instructions.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// An active security role, as read by <see cref="GetSecurityRoles"/>.
    /// Only the fields audience resolution and suggestion need.
    /// </summary>
    private sealed class SecurityRole
    {
        /// <summary>
        /// The identifier of the security role group.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the security role, which is the value an audience
        /// names it by.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the security role, or <c>null</c>.
        /// </summary>
        public string Description { get; set; }
    }

    #endregion
}
