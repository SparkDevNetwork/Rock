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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Utilities.CommunityKnowledgeBaseSkill;
using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Data;
using Rock.Net;
using Rock.SystemGuid;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Provides read access to the Rock community knowledge base: product
/// documentation, community content, the Rock source code, and curated topic guides.
/// </summary>
/// <remarks>
/// <para>
/// This is the first agent skill whose data lives outside Rock. Nothing here touches
/// the Rock database except to read the skill's own configuration and Rock's version
/// number.
/// </para>
/// <para>
/// The problem the skill solves is routing rather than access. Three remote stores
/// answer three different kinds of question, and an agent that picks wrong does not
/// fail loudly; it settles for a partial answer from the wrong store and presents it
/// with confidence. That is why the overview tool is a prerequisite of every other
/// tool and why more effort goes into forcing it to be called first than into any
/// single search.
/// </para>
/// <para>
/// Filter values are deliberately not validated before sending. See
/// <see cref="ResolveCategoryFilter"/> and the empty result handling on each search
/// tool for what replaces validation.
/// </para>
/// </remarks>
[Description( "Provides access to the Rock community knowledge base: product documentation, community content, the Rock source code, and curated topic guides." )]
[AgentSkillName( "Community Knowledge Base" )]
[AgentPurpose( "Answers questions about how Rock RMS works, from documentation, community content, curated guides, and the Rock source." )]
[AgentUsage( "Always call GetKnowledgeBaseOverview before the first search in a conversation. It reports what the knowledge base actually holds, which store answers which kind of question, and the exact values every filter accepts. Searching without it means guessing, and a guessed filter value returns nothing rather than an error." )]
[AgentUsage( "Prefer SearchKnowledge for almost every question. Reach for the code tools only when the question is about implementation detail, or when SearchKnowledge has already failed to answer it." )]
[AgentUsage( "When you do not know enough about a subject to know what to search for, open a curated topic instead of guessing at search terms. The overview lists the topics with a hint for each; GetTopic opens one and GetArticle reads down through it, revealing a little at a time so you can choose where to go next. Curated topics are never returned by search, so this is the only route to them." )]
[AgentGuardrail( "Never construct, edit, or guess a topic key or an article key. Take them only from the overview's topic list, a topic's table of contents, or a parent article's child list." )]
[AgentSkillGuid( "DFCBFDE8-6BF2-4DDF-81FE-FDD436E5FD90" )]
[EntityTypeGuid( "959F0B92-A3BB-4AAA-9143-CF7D77895392" )]
internal sealed partial class CommunityKnowledgeBaseSkill : AgentSkillComponent
{
    #region Keys

    /// <summary>
    /// The configuration keys used to configure this skill.
    /// </summary>
    private static class ConfigurationKey
    {
        /// <summary>
        /// The categories knowledge search is scoped to. Empty means every category.
        /// </summary>
        public const string Categories = "categories";
    }

    #endregion

    #region Constants

    /// <summary>
    /// Sent when the organization identifier cannot be read.
    /// </summary>
    /// <remarks>
    /// The identifier is analytics and rate limiting rather than authorization, so a
    /// missing value costs correct attribution and nothing else. Refusing to answer
    /// over it would trade the whole feature for a metric. The empty GUID is also
    /// better than any other placeholder: it is well formed, so it clears the only
    /// check the service performs, and it reads as "not supplied" rather than as a
    /// plausible but wrong organization.
    /// </remarks>
    private const string UnknownOrganizationId = "00000000-0000-0000-0000-000000000000";

    /// <summary>
    /// The page size for the paged search tools, and the most items any tool returns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 8/19/26 - CLAUDE
    ///
    /// Lowered from 50 when these tools began returning the service's payload as it
    /// comes rather than a narrowed projection of it. A knowledge hit carries its
    /// chunk text, which the service caps at 1024 tokens, so the cost of a page is
    /// roughly the page size multiplied by that cap. Fifty hits measured at about
    /// 46,000 tokens, which is not a reasonable size for one tool result; twenty five
    /// halves it, and the article and topic tools exist to fetch the whole of whatever
    /// looks right.
    ///
    /// Reason: The page size is now the only thing bounding the response, so it has to
    /// carry that weight on its own.
    /// </para>
    /// <para>
    /// The service caps a page at 250, so this is Rock's limit rather than the
    /// service's.
    /// </para>
    /// </remarks>
    private const int SearchPageSize = 25;

    /// <summary>
    /// How long the configuration screen waits for the category list.
    /// </summary>
    /// <remarks>
    /// Much shorter than the client's own timeout, because this call blocks a
    /// configuration screen from rendering. An operator staring at a spinner for
    /// thirty seconds will assume the screen is broken, and an empty picker with a
    /// warning is a better answer than a long wait for the same outcome.
    /// </remarks>
    private static readonly TimeSpan ConfigurationTimeout = TimeSpan.FromSeconds( 8 );

    #endregion

    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunityKnowledgeBaseSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public CommunityKnowledgeBaseSkill( ILogger<CommunityKnowledgeBaseSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Configuration

    /// <inheritdoc/>
    public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
    {
        // No values are threaded through, because there is nothing left to thread.
        // The host is a constant and the organization id resolves itself, so this
        // fetch always has what it needs and works on a brand new skill instance
        // with nothing filled in.
        return new DynamicComponentDefinitionBag
        {
            Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/AI/Skills/communityKnowledgeBaseSkill.obs" ),
            Options = new Dictionary<string, string>
            {
                ["categories"] = GetCategoryOptions().ToCamelCaseJson( false, false )
            }
        };
    }

    /// <inheritdoc/>
    public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
    {
        var publicConfiguration = new Dictionary<string, string>();

        if ( privateConfiguration.TryGetValue( ConfigurationKey.Categories, out var categories ) )
        {
            publicConfiguration[ConfigurationKey.Categories] = categories;
        }

        return publicConfiguration;
    }

    /// <inheritdoc/>
    public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
    {
        var privateConfiguration = new Dictionary<string, string>();

        if ( publicConfiguration.TryGetValue( ConfigurationKey.Categories, out var categories ) )
        {
            privateConfiguration[ConfigurationKey.Categories] = categories;
        }

        return privateConfiguration;
    }

    /// <summary>
    /// Gets the categories the picker offers, read from the remote managed lists.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the only call the skill makes to the managed lists route, and it
    /// happens at configuration time rather than per request. Nothing validates
    /// filters at request time, so the lists have no other consumer.
    /// </para>
    /// <para>
    /// No version is sent. Version validation runs in the service's shared request
    /// wrapper and applies even to routes that ignore the parameter, so a version
    /// here could only fail, and it would fail at the worst moment: an operator
    /// configuring the skill on a Rock newer than the corpus.
    /// </para>
    /// <para>
    /// There is no compiled in fallback list. A stale hard coded set is worse than an
    /// empty one, because an operator who picks from it gets a scope that silently
    /// matches nothing, while an empty picker is an obvious problem that gets
    /// reported.
    /// </para>
    /// </remarks>
    /// <returns>The categories offered by the service, or an empty list when it cannot be reached.</returns>
    private List<ListItemBag> GetCategoryOptions()
    {
        try
        {
            var organizationId = GetOrganizationId();

            // Task.Run moves the call off the request's synchronization context
            // before it is waited on. GetComponentDefinition is a synchronous
            // override, so the wait itself is unavoidable, and waiting directly on
            // the request thread deadlocks: the continuation cannot resume onto a
            // context that the wait is holding. The symptom is not an error but a
            // request that never returns, which surfaced as a skill whose edit
            // button stayed disabled forever.
            //
            // ConfigureAwait( false ) inside the client covers the same ground.
            // Both are kept, because one of them protects against downstream code
            // that forgets the other.
            var response = Task.Run( async () =>
                await CommunityKnowledgeBaseClient
                    .GetAsync( organizationId, "managed-lists", null, new CancellationTokenSource( ConfigurationTimeout ).Token )
                    .ConfigureAwait( false ) )
                .GetAwaiter()
                .GetResult();

            if ( !response.IsSuccess )
            {
                _logger.LogWarning( "Could not retrieve knowledge base categories. {Detail}", response.Detail );

                return new List<ListItemBag>();
            }

            var categories = response.Data?["categories"]?.Values<string>() ?? Enumerable.Empty<string>();

            return categories
                .Where( c => c.IsNotNullOrWhiteSpace() )
                .Select( c => new ListItemBag
                {
                    Value = c,
                    Text = ToDisplayText( c )
                } )
                .ToList();
        }
        catch ( Exception ex )
        {
            _logger.LogWarning( ex, "Could not retrieve knowledge base categories." );

            return new List<ListItemBag>();
        }
    }

    /// <summary>
    /// Converts a category slug into something readable.
    /// </summary>
    /// <remarks>
    /// Display only. The stored value is always the raw slug, because that is what
    /// the service accepts.
    /// </remarks>
    /// <param name="slug">The slug as the service returned it, such as "product-documentation".</param>
    /// <returns>The slug with hyphens replaced by spaces and each word capitalized.</returns>
    private static string ToDisplayText( string slug )
    {
        var words = slug
            .Split( '-' )
            .Where( w => w.IsNotNullOrWhiteSpace() )
            .Select( w => char.ToUpperInvariant( w[0] ) + w.Substring( 1 ) );

        return string.Join( " ", words );
    }

    #endregion

    #region Shared Helpers

    /// <summary>
    /// Builds the metadata for a paged tool: everything the service reported about
    /// the response, plus the paging facts it does not report.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 8/19/26 - CLAUDE
    ///
    /// Both halves are needed and neither replaces the other. The service's own meta
    /// is passed through whole so a field it adds later arrives without a code change,
    /// which is the same reason the payload is no longer mapped. But it describes the
    /// request in the service's terms, offset and limit, and says nothing about which
    /// page that is or whether another exists.
    ///
    /// Reason: The service knows what it returned; only this code knows what was
    /// asked for.
    /// </para>
    /// <para>
    /// <c>hasMoreItems</c> is derived from a full page rather than from
    /// <c>estimated_total</c>, because that total is approximate on large result sets
    /// by the service's own description and would give a confident wrong answer near
    /// the end of a result set. A full page is a weaker signal but an honest one.
    /// </para>
    /// <para>
    /// <c>estimated_total</c> is also restated as <c>approximateTotalMatches</c>. The
    /// raw name reads like a count and will be quoted as one, and the caveat belongs
    /// where the number is rather than in documentation nobody re-reads.
    /// </para>
    /// </remarks>
    /// <param name="response">The response being described.</param>
    /// <param name="pageNumber">The page that was requested.</param>
    /// <param name="returnedItemCount">How many items are being returned.</param>
    /// <returns>The metadata for the tool result.</returns>
    private static Dictionary<string, object> BuildPagedMetadata( CommunityKnowledgeBaseResponse response, int pageNumber, int returnedItemCount )
    {
        var metadata = response.Meta.ToPlainMetadata();

        metadata["pageNumber"] = pageNumber;
        metadata["returnedItemCount"] = returnedItemCount;
        metadata["hasMoreItems"] = returnedItemCount == SearchPageSize;

        var estimatedTotal = response.Meta.GetNullableInt( "estimated_total" );

        if ( estimatedTotal.HasValue )
        {
            metadata["approximateTotalMatches"] = estimatedTotal.Value;
        }

        return metadata;
    }

    /// <summary>
    /// Gets the organization identifier for the request path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The identifier is a version 5 UUID hash of the Rock organization GUID,
    /// computed under a namespace each knowledge base deployment holds privately.
    /// Rock cannot compute it. Spark provisions it and hands it to Rock through the
    /// Connected Services manifest, which is why this is read rather than configured
    /// and must never be derived from the organization GUID here.
    /// </para>
    /// <para>
    /// Every hop can be null, and none of them are exceptional: the service is null
    /// when Connected Services is not registered, the configuration is null before a
    /// manifest has been fetched, and the knowledge base entry is null for an
    /// organization that has not subscribed. All three fall through to the empty
    /// GUID.
    /// </para>
    /// <para>
    /// The value is never logged, echoed in a result, or surfaced in an error.
    /// Connected Services names it an API key, and whatever it proves to be on the
    /// wire, it is provisioned per organization and costs nothing to keep quiet.
    /// </para>
    /// </remarks>
    /// <returns>The organization identifier, or the empty GUID when it cannot be read.</returns>
    private string GetOrganizationId()
    {
        try
        {
            var provider = RockApp.Current.GetService<ConnectedServicesProvider>();
            var apiKey = provider?.GetConfiguration()?.KnowledgeBase?.ApiKey;

            if ( apiKey.IsNotNullOrWhiteSpace() )
            {
                return apiKey.Trim().ToLowerInvariant();
            }
        }
        catch ( Exception ex )
        {
            _logger.LogDebug( ex, "Could not read the knowledge base organization identifier from connected services." );
        }

        _logger.LogDebug( "No knowledge base organization identifier is configured. Requests will be unattributed." );

        return UnknownOrganizationId;
    }

    /// <summary>
    /// Gets the Rock version to scope content requests to.
    /// </summary>
    /// <remarks>
    /// The service expects the major release only, as a bare number such as "19" or
    /// "20". Not "19.1", and not "19.0"; a major release carries no trailing minor.
    /// Nothing checks the version against the corpus before sending. If the running
    /// Rock is newer than anything indexed the service answers 400
    /// <c>unknown-rock-version</c> and names the versions it holds, which is both more
    /// current than a cached list and cheaper than looking one up on every session.
    /// </remarks>
    /// <returns>The running Rock major version.</returns>
    private static string GetRockVersion()
    {
        return VersionInfo.VersionInfo
            .GetRockSemanticVersionNumber()
            .Split( '.' )
            .First();
    }

    /// <summary>
    /// Gets the categories this skill is scoped to.
    /// </summary>
    /// <remarks>
    /// Sent as saved, with no check against the service. A category renamed on the
    /// server since configuration simply matches nothing, so the search returns empty
    /// and the problem is visible. Filtering the scope against a cached list would be
    /// the dangerous version: dropping every unrecognized name would leave no
    /// category filter at all, silently widening a deliberately narrow skill to the
    /// whole corpus.
    /// </remarks>
    /// <returns>The configured categories, empty when the skill is scoped to everything.</returns>
    private List<string> GetConfiguredCategories()
    {
        if ( !ConfigurationValues.TryGetValue( ConfigurationKey.Categories, out var raw ) || raw.IsNullOrWhiteSpace() )
        {
            return new List<string>();
        }

        return raw.SplitDelimitedValues().ToList();
    }

    /// <summary>
    /// Works out the value to send as the category filter for one request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The result is always a single comma joined value, never a repeated parameter.
    /// On this service commas within one value mean OR and a repeated parameter means
    /// AND, so emitting one parameter per configured category would return only
    /// documents carrying every category at once, which is close to nothing. An
    /// operator who scoped a skill to four categories to get better answers would
    /// instead get none, with no error to connect it to the setting.
    /// </para>
    /// <para>
    /// A category outside the configured scope is refused rather than quietly
    /// widened. This is the one refusal the skill makes about a filter value, and it
    /// is about honoring an operator's configuration rather than validating against
    /// the corpus, which is why the message does not call the category invalid.
    /// </para>
    /// </remarks>
    /// <param name="requestedCategory">The category the caller asked for, which may be a comma separated list, or null.</param>
    /// <param name="filterValue">The value to send, or null when no category filter applies.</param>
    /// <returns><c>null</c> when the scope resolved, otherwise the error to return.</returns>
    private AgentToolResult ResolveCategoryFilter( string requestedCategory, out string filterValue )
    {
        var configured = GetConfiguredCategories();

        if ( requestedCategory.IsNullOrWhiteSpace() )
        {
            filterValue = configured.Any() ? string.Join( ",", configured ) : null;

            return null;
        }

        var requested = requestedCategory.SplitDelimitedValues().ToList();

        if ( configured.Any() )
        {
            var outOfScope = requested
                .Where( r => !configured.Any( c => c.Equals( r, StringComparison.OrdinalIgnoreCase ) ) )
                .ToList();

            if ( outOfScope.Any() )
            {
                filterValue = null;

                return Error( $"The category '{string.Join( ", ", outOfScope )}' is outside the categories this skill is scoped to." )
                    .WithInstructions( $"This skill is scoped to: {string.Join( ", ", configured )}." );
            }
        }

        filterValue = string.Join( ",", requested );

        return null;
    }

    /// <summary>
    /// Turns a failed knowledge base call into the result the agent should see.
    /// </summary>
    /// <remarks>
    /// Shared so that every tool reports the same conditions the same way. The
    /// service's <c>detail</c> is surfaced rather than replaced, because it names the
    /// valid values and is usually enough for a model to correct itself in one turn.
    /// </remarks>
    /// <param name="response">The failed response.</param>
    /// <returns>An error result describing the failure.</returns>
    private AgentToolResult DescribeFailure( CommunityKnowledgeBaseResponse response )
    {
        if ( response.IsTransportFailure )
        {
            return Error( response.Detail )
                .WithInstructions( "This is a connection problem rather than a problem with the request. Rephrasing will not help." );
        }

        if ( response.IsRateLimited )
        {
            var wait = response.RetryAfterSeconds.HasValue
                ? $" The limit resets in {response.RetryAfterSeconds} seconds."
                : string.Empty;

            return Error( $"The knowledge base rate limit was reached.{wait}" )
                .WithInstructions( "Do not call this tool again immediately. Answer from what you already have, or tell the person the knowledge base is temporarily unavailable." );
        }

        if ( response.ProblemType == "invalid-organization" )
        {
            // Should be unreachable: the identifier is either provisioned by Spark or
            // is the empty GUID, and both are well formed. Reaching here means the
            // resolver is broken rather than the request.
            _logger.LogError( "The knowledge base rejected the organization identifier. {Detail}", response.Detail );

            return Error( "The knowledge base rejected this Rock instance's identifier." )
                .WithInstructions( "This is a configuration problem an administrator must correct. Rephrasing will not help." );
        }

        var detail = response.Detail.IsNotNullOrWhiteSpace()
            ? response.Detail
            : $"The knowledge base returned {( int? ) response.StatusCode}.";

        return Error( detail );
    }

    #endregion
}
