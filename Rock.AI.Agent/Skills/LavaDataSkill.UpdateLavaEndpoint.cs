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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Skills.LavaDataSkill;
using Rock.Configuration;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaDataSkill
{
    #region Tool(s)

    [Description( "Replaces the template of an existing Lava endpoint, optionally adjusts its security mode and enabled Lava commands, and reports the result of test-executing the replacement." )]
    [AgentToolPreamble( "Updating the Lava endpoint." )]
    [AgentUsage( "This replaces the whole template, so send the complete Lava rather than a fragment. Read it with GetLavaEndpoint first if you did not write the current version." )]
    [AgentUsage( "securityMode and enabledLavaCommands are left unchanged when omitted. Use them to correct an endpoint you already created rather than sending the user to the admin pages." )]
    [AgentUsage( "A template that starts using a new command needs that command added here too, or it will silently return nothing where the command was." )]
    [AgentUsage( "Adding 'Sql' to enabledLavaCommands is refused without sqlJustification, exactly as it is on create. Rewriting the template with 'RockEntity', 'RockEntityModify' and 'RockEntityDelete' is the expected response." )]
    [AgentUsage( "Always pass testParameters when the template reads Body or QueryString, with realistic values, so the parameter path is proven rather than assumed." )]
    [AgentUsage( "An endpoint enabling RockEntityModify or RockEntityDelete is not test-executed, so this call returns no evidence the template works." )]
    [AgentToolGuid( "2F92D13B-A2A2-455C-8324-57A181D505C2" )]
    public AgentToolResult UpdateLavaEndpoint(
        [Description( "The slug of the Lava application the endpoint belongs to." )]
        string applicationSlug,

        [Description( "The slug of the endpoint to update." )]
        string endpointSlug,

        [Description( "The new Lava template. This replaces the whole template, so send the complete Lava." )]
        string codeTemplate,

        [Description( "The HTTP method that identifies the endpoint. Defaults to Post." )]
        string httpMethod = null,

        [Description( "The security mode to switch to: EndpointExecute, ApplicationView, ApplicationEdit or ApplicationAdministrate. Omit to leave it unchanged." )]
        string securityMode = null,

        [Description( "The comma-delimited Lava commands to allow. Omit to leave the stored commands unchanged, or clear to remove them all." )]
        SetOrClear<string> enabledLavaCommands = null,

        [Description( "Why raw SQL is unavoidable. Required only when this call adds 'Sql' to enabledLavaCommands, and only after the user explicitly approved it." )]
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
        if ( codeTemplate.IsNullOrWhiteSpace() )
        {
            helper.AddError( "A Lava template is required." );
        }
        else if ( !TryLintTemplate( codeTemplate, out var lintError ) )
        {
            helper.AddError( lintError );
        }

        if ( !TryParseTestParameters( testParameters, out var parsedTestParameters, out var testParametersError ) )
        {
            helper.AddError( testParametersError );
        }

        /*
            8/17/2026 - CLAUDE

            Only what this call is asking for is checked. Leaving
            enabledLavaCommands null means the stored commands are untouched,
            so an endpoint whose SQL the user already approved is not
            re-litigated on every template edit. Adding Sql here goes through
            the same refusal as creating it, and a template that starts using
            {% sql %} without the command being enabled cannot run it anyway.

            Reason: Gate the request to change commands, not every edit to
            the template.
        */
        var isChangingCommands = enabledLavaCommands != null && !enabledLavaCommands.ClearValue;

        if ( isChangingCommands
            && !TryValidateSqlUsage( enabledLavaCommands.Value, sqlJustification, out var sqlError ) )
        {
            helper.AddError( sqlError );
        }

        if ( securityMode.IsNotNullOrWhiteSpace()
            && !TryGetSecurityMode( securityMode, out _, out var securityModeError ) )
        {
            helper.AddError( securityModeError );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var endpoint = GetAuthorizedEndpoint( helper, rockContext, applicationSlug, endpointSlug, httpMethod );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        endpoint.CodeTemplate = codeTemplate;

        // Both of these are left alone when the agent does not mention them,
        // so a template-only edit cannot quietly change who is allowed to
        // run the endpoint.
        if ( securityMode.IsNotNullOrWhiteSpace()
            && TryGetSecurityMode( securityMode, out var newSecurityMode, out _ ) )
        {
            endpoint.SecurityMode = newSecurityMode;
        }

        helper.UpdateProperty( endpoint, e => e.EnabledLavaCommands, enabledLavaCommands );

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

        var url = GetEndpointUrl( endpoint.LavaApplication.Slug, endpoint.Slug );

        var result = Success( new LavaEndpointSaveResult
        {
            ApplicationSlug = endpoint.LavaApplication.Slug,
            EndpointSlug = endpoint.Slug,
            Method = endpoint.HttpMethod.ToString(),
            Url = url,
            TestExecution = TestExecute( codeTemplate, endpoint.EnabledLavaCommands, endpoint.LavaApplication, endpoint.HttpMethod, parsedTestParameters, maxTestOutputLength )
        } )
            .WithHistoryContent( new LavaEndpointReferenceResult
            {
                ApplicationSlug = endpoint.LavaApplication.Slug,
                EndpointSlug = endpoint.Slug,
                Method = endpoint.HttpMethod.ToString(),
                Url = url
            }, "lava-endpoint" );

        // Only when this call is what turned SQL on. An endpoint that
        // already had it approved does not need the warning repeated on
        // every template edit.
        if ( isChangingCommands && IsSqlRequested( enabledLavaCommands.Value ) )
        {
            result.WithInstructions( GetSqlApprovalInstructions( endpoint.Slug, sqlJustification ) );
        }

        return result;
    }

    #endregion
}
