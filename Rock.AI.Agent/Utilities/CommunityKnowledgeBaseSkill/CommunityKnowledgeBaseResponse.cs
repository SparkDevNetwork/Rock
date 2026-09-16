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

using System.Net;

using Newtonsoft.Json.Linq;

namespace Rock.AI.Agent.Utilities.CommunityKnowledgeBaseSkill;

/// <summary>
/// The outcome of one call to the community knowledge base.
/// </summary>
/// <remarks>
/// A failed call is described rather than thrown, because whether a given failure is
/// an error, a miss, or a recoverable condition is a decision each tool makes
/// differently. A 404 is a miss on a topic key and a defect on a code document id.
/// </remarks>
internal class CommunityKnowledgeBaseResponse
{
    /// <summary>
    /// Whether the request succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// The <c>data</c> member of the response envelope, or <c>null</c> on failure and
    /// on the plain text route.
    /// </summary>
    public JToken Data { get; set; }

    /// <summary>
    /// The <c>meta</c> member of the response envelope.
    /// </summary>
    /// <remarks>
    /// Carries paging, totals, and the values the service resolved on the caller's
    /// behalf, including the Rock version actually applied and whether a release has
    /// code indexed.
    /// </remarks>
    public JToken Meta { get; set; }

    /// <summary>
    /// The response body for routes that return <c>text/plain</c>.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The HTTP status code, when a response arrived.
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; }

    /// <summary>
    /// The problem type from an RFC 9457 body, such as <c>unknown-rock-version</c>.
    /// </summary>
    /// <remarks>
    /// This is what tools branch on. The status code alone is not specific enough:
    /// several distinct conditions share a 400 and need different handling.
    /// </remarks>
    public string ProblemType { get; set; }

    /// <summary>
    /// The problem title from an RFC 9457 body.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The problem detail from an RFC 9457 body.
    /// </summary>
    /// <remarks>
    /// Surfaced to the agent rather than replaced. The service writes these to name
    /// the valid values, which is usually enough to correct a call in one turn.
    /// </remarks>
    public string Detail { get; set; }

    /// <summary>
    /// Seconds until the rate limit window resets, when the service sent one.
    /// </summary>
    public int? RetryAfterSeconds { get; set; }

    /// <summary>
    /// Whether the request failed before a response arrived.
    /// </summary>
    /// <remarks>
    /// Worth distinguishing, because a transport failure is the one case where
    /// rephrasing or retrying a query cannot possibly help and the agent should be
    /// told so plainly.
    /// </remarks>
    public bool IsTransportFailure { get; set; }

    /// <summary>
    /// Whether the failure was a rate limit.
    /// </summary>
    public bool IsRateLimited => StatusCode.HasValue && ( int ) StatusCode.Value == 429;

    /// <summary>
    /// Whether the failure was a not found.
    /// </summary>
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
}
