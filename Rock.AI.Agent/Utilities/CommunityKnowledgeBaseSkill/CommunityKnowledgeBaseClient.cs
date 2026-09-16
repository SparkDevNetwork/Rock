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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Rock.AI.Agent.Utilities.CommunityKnowledgeBaseSkill;

/// <summary>
/// Performs the HTTP calls to the Rock community knowledge base and turns the
/// response into either a parsed payload or a described failure.
/// </summary>
/// <remarks>
/// <para>
/// The client never throws for a failed request. Every outcome is expressed on
/// <see cref="CommunityKnowledgeBaseResponse"/> so that the calling tool decides
/// whether a condition is an error, a miss, or a success, which is a decision that
/// differs per tool.
/// </para>
/// <para>
/// One <see cref="HttpClient"/> is held statically. A skill instance is constructed
/// per request, so a client per instance would exhaust sockets under load. This
/// follows <c>AzureBlobStorageClient</c>.
/// </para>
/// </remarks>
internal static class CommunityKnowledgeBaseClient
{
    #region Constants

    /// <summary>
    /// The knowledge base host. Fixed rather than configurable: there is one
    /// knowledge base, and a setting for it would be a field nobody should change
    /// sitting in front of every operator.
    /// </summary>
    private const string ApiHost = "https://knowledge.rockrms.com";

    /// <summary>
    /// The API version segment. A breaking change on the service ships as v2, so
    /// this skill keeps working until it is deliberately moved.
    /// </summary>
    private const string ApiVersion = "v1";

    /// <summary>
    /// Thirty seconds, because semantic search over a large corpus is not instant
    /// and a short timeout turns a slow answer into a wrong one.
    /// </summary>
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds( 30 );

    #endregion

    #region Fields

    /// <summary>
    /// The shared client. See the remarks on the class for why this is static.
    /// </summary>
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = RequestTimeout
    };

    #endregion

    #region Methods

    /// <summary>
    /// Performs a GET returning the standard <c>{ data, meta }</c> envelope.
    /// </summary>
    /// <param name="organizationId">The organization identifier for the path segment.</param>
    /// <param name="path">The route below the organization segment, already escaped.</param>
    /// <param name="parameters">The query string values to send. Null and blank values are dropped.</param>
    /// <param name="cancellationToken">Cancels the request. Used to apply a shorter deadline than the client's own timeout.</param>
    /// <returns>A response carrying either the parsed envelope or a description of the failure.</returns>
    public static async Task<CommunityKnowledgeBaseResponse> GetAsync( string organizationId, string path, IDictionary<string, string> parameters = null, CancellationToken cancellationToken = default )
    {
        var url = BuildUrl( organizationId, path, parameters );

        try
        {
            using ( var response = await _httpClient.GetAsync( url, cancellationToken ).ConfigureAwait( false ) )
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

                if ( !response.IsSuccessStatusCode )
                {
                    return DescribeFailure( response, body );
                }

                var envelope = JObject.Parse( body );

                return new CommunityKnowledgeBaseResponse
                {
                    IsSuccess = true,
                    Data = envelope["data"],
                    Meta = envelope["meta"]
                };
            }
        }
        catch ( Exception ex )
        {
            return DescribeTransportFailure( ex );
        }
    }

    /// <summary>
    /// Performs a GET returning <c>text/plain</c> rather than the envelope.
    /// </summary>
    /// <remarks>
    /// Only the raw code document route behaves this way, because wrapping a source
    /// file in JSON would escape every newline. The failure path still returns
    /// problem+json, so the status code decides which parser runs rather than the
    /// route. A handler that assumes an envelope on success would work everywhere
    /// except here, and here it would work on failures only.
    /// </remarks>
    /// <param name="organizationId">The organization identifier for the path segment.</param>
    /// <param name="path">The route below the organization segment, already escaped.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A response carrying either the plain text body or a description of the failure.</returns>
    public static async Task<CommunityKnowledgeBaseResponse> GetTextAsync( string organizationId, string path, CancellationToken cancellationToken = default )
    {
        var url = BuildUrl( organizationId, path, null );

        try
        {
            using ( var response = await _httpClient.GetAsync( url, cancellationToken ).ConfigureAwait( false ) )
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

                if ( !response.IsSuccessStatusCode )
                {
                    return DescribeFailure( response, body );
                }

                return new CommunityKnowledgeBaseResponse
                {
                    IsSuccess = true,
                    Text = body
                };
            }
        }
        catch ( Exception ex )
        {
            return DescribeTransportFailure( ex );
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Assembles the request URL.
    /// </summary>
    /// <remarks>
    /// The path is expected to be escaped already. That is deliberate: article keys
    /// contain slashes and are used as a catch all path segment, so escaping the
    /// whole path here would turn every namespaced key into a not found. See
    /// <c>EscapeKey</c>.
    /// </remarks>
    /// <param name="organizationId">The organization identifier for the path segment.</param>
    /// <param name="path">The route below the organization segment.</param>
    /// <param name="parameters">The query string values to send.</param>
    /// <returns>The absolute request URL.</returns>
    private static string BuildUrl( string organizationId, string path, IDictionary<string, string> parameters )
    {
        var url = new StringBuilder( $"{ApiHost}/api/{ApiVersion}/{organizationId}/{path.TrimStart( '/' )}" );

        // Blank values are dropped rather than sent empty. An empty filter is not
        // the same request as an absent one on several of these routes.
        var populated = parameters?
            .Where( p => p.Value.IsNotNullOrWhiteSpace() )
            .ToList();

        if ( populated != null && populated.Any() )
        {
            var query = populated
                .Select( p => $"{Uri.EscapeDataString( p.Key )}={Uri.EscapeDataString( p.Value )}" );

            url.Append( "?" ).Append( string.Join( "&", query ) );
        }

        return url.ToString();
    }

    /// <summary>
    /// Escapes one path segment of a retrieval key.
    /// </summary>
    /// <remarks>
    /// Article keys look like <c>db-schema/attendance-model</c> and the route is a
    /// catch all segment. The service splits the path into segments before decoding
    /// them, so a percent encoded slash never survives as a separator and a single
    /// <c>Uri.EscapeDataString</c> over the whole key turns every namespaced key into
    /// a 404. Escaping each segment separately and rejoining is the correct form, and
    /// it is the call a careful developer would otherwise get wrong by being careful.
    /// </remarks>
    /// <param name="key">The retrieval key, exactly as it was supplied by the service.</param>
    /// <returns>The key with each segment escaped and the separators preserved.</returns>
    public static string EscapeKey( string key )
    {
        if ( key.IsNullOrWhiteSpace() )
        {
            return string.Empty;
        }

        return string.Join( "/", key.Split( '/' ).Select( Uri.EscapeDataString ) );
    }

    /// <summary>
    /// Reads an RFC 9457 problem details body into a described failure.
    /// </summary>
    /// <remarks>
    /// The <c>detail</c> string is preserved rather than replaced with a friendlier
    /// message. The service writes those strings to name the valid values, which is
    /// usually enough for a model to correct itself in one turn.
    /// </remarks>
    /// <param name="response">The failed response.</param>
    /// <param name="body">The response body, which is expected to be problem+json.</param>
    /// <returns>A response describing the failure.</returns>
    private static CommunityKnowledgeBaseResponse DescribeFailure( HttpResponseMessage response, string body )
    {
        var failure = new CommunityKnowledgeBaseResponse
        {
            IsSuccess = false,
            StatusCode = response.StatusCode
        };

        // A rate limited response names the seconds until the window resets. Passing
        // that number through turns "do not call again immediately" from a rule the
        // agent has to guess at into one it can act on.
        if ( response.StatusCode == ( HttpStatusCode ) 429 )
        {
            failure.RetryAfterSeconds = response.Headers.RetryAfter?.Delta?.TotalSeconds is double seconds
                ? ( int? ) Math.Ceiling( seconds )
                : null;
        }

        try
        {
            var problem = JObject.Parse( body );

            failure.ProblemType = problem["type"]?.ToString();
            failure.Detail = problem["detail"]?.ToString();
            failure.Title = problem["title"]?.ToString();
        }
        catch
        {
            // Not every failure is well formed problem+json. A proxy or gateway can
            // return HTML. Keep the status code and move on rather than masking the
            // original failure with a parse error.
            failure.Detail = body.IsNotNullOrWhiteSpace() && body.Length <= 500 ? body : null;
        }

        return failure;
    }

    /// <summary>
    /// Describes a failure that happened before any response arrived.
    /// </summary>
    /// <param name="exception">The exception raised while sending the request.</param>
    /// <returns>A response describing the failure.</returns>
    private static CommunityKnowledgeBaseResponse DescribeTransportFailure( Exception exception )
    {
        return new CommunityKnowledgeBaseResponse
        {
            IsSuccess = false,
            IsTransportFailure = true,
            Detail = $"Could not reach {ApiHost}. {exception.Message}"
        };
    }

    #endregion
}
