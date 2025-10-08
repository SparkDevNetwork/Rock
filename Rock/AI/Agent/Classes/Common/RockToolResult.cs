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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Rock.Enums.AI.Agent;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Represents a standardized envelope for tool results returned by agent skills and kernel tools.
    /// Provides a clean JSON shape and a simple builder-style API for attaching metadata and guidance.
    /// </summary>
    /// <remarks>
    /// - Exactly one of <see cref="Content"/> or <see cref="Results"/> will be set when using the factory methods.
    /// - <see cref="HistoryContent"/> is not serialized and is only used for chat history plumbing.
    /// - Properties are marked <see langword="internal"/> but included in JSON via <see cref="JsonIncludeAttribute"/> to minimize public surface area.
    /// </remarks>
    internal sealed class RockToolResult
    {
        #region Properties

        /// <summary>
        /// Gets the overall outcome for the tool call.
        /// </summary>
        [JsonInclude]
        internal ToolStatus Status { get; private set; }

        /// <summary>
        /// Gets the error messages when <see cref="Status"/> is <see cref="ToolStatus.Error"/>;
        /// otherwise <c>null</c> or an empty list.
        /// </summary>
        [JsonInclude, JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal List<string> ErrorMessages { get; private set; }

        /// <summary>
        /// Gets optional, model-facing guidance about what to do next (for example, ask for missing inputs).
        /// </summary>
        [JsonInclude, JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal string Instructions { get; private set; }

        /// <summary>
        /// Gets arbitrary content that should be added to chat history but not serialized in the tool result payload.
        /// </summary>
        [JsonIgnore]
        internal object HistoryContent { get; private set; }

        /// <summary>
        /// The key of the history content. This is so we can remove specific history content if needed.
        /// </summary>
        [JsonIgnore]
        internal string HistoryContentKey { get; private set; }

        /// <summary>
        /// Gets the primary payload when the result represents a single value.
        /// Mutually exclusive with <see cref="Results"/>.
        /// </summary>
        [JsonInclude, JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal object Content { get; private set; }

        /// <summary>
        /// Gets the primary payload when the result represents a collection of values.
        /// Mutually exclusive with <see cref="Content"/>.
        /// </summary>
        [JsonInclude, JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal IEnumerable Results { get; private set; }

        /// <summary>
        /// Gets optional metadata for diagnostics, correlation IDs, echoed inputs, etc.
        /// </summary>
        [JsonInclude, JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal Dictionary<string, object> Meta { get; private set; }

        /// <summary>
        /// Gets optional reference URL to include in the payload.
        /// </summary>
        [JsonInclude, JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal ReferenceUrlResult ReferenceUrl { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents direct instantiation. Use the static factory methods to create instances.
        /// </summary>
        private RockToolResult() { }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a <see cref="ToolStatus.Success"/> result wrapping the specified payload.
        /// If <paramref name="payload"/> implements <see cref="IEnumerable"/> (and is not a <see cref="string"/>)
        /// then it will be assigned to <see cref="Results"/>; otherwise to <see cref="Content"/>.
        /// </summary>
        /// <param name="payload">The value to include in the result.</param>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Success( object payload )
        {
            var result = new RockToolResult { Status = ToolStatus.Success };

            SetContent( result, payload );

            // By default, also set the history content to the same value.
            result.HistoryContent = payload;

            return result;
        }

        /// <summary>
        /// Creates a <see cref="ToolStatus.Success"/> result with no payload.
        /// </summary>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Success()
        {
            return new RockToolResult
            {
                Status = ToolStatus.Success,
                HistoryContent = string.Empty
            };
        }

        /// <summary>
        /// Creates a <see cref="ToolStatus.NoData"/> result with no payload.
        /// </summary>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult NoData() =>
            new RockToolResult
            {
                Status = ToolStatus.NoData,
            };

        /// <summary>
        /// Creates a <see cref="ToolStatus.Error"/> result with a single error message.
        /// </summary>
        /// <param name="message">The error message. If <c>null</c> or whitespace, an empty string is added.</param>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Error( string message ) =>
            new RockToolResult
            {
                Status = ToolStatus.Error,
                ErrorMessages = new List<string> { message ?? string.Empty },
            };

        /// <summary>
        /// Creates a <see cref="ToolStatus.Error"/> result with one or more error messages.
        /// </summary>
        /// <param name="messages">The collection of error messages.</param>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Error( IEnumerable<string> messages ) =>
            new RockToolResult
            {
                Status = ToolStatus.Error,
                ErrorMessages = ( messages ?? Enumerable.Empty<string>() ).Select( m => m ?? string.Empty ).ToList()
            };

        #endregion

        #region Fluent API

        /// <summary>
        /// Adds optional, model-facing guidance to this result and returns the same instance.
        /// </summary>
        /// <param name="instructions">The guidance text to include.</param>
        /// <returns>The same <see cref="RockToolResult"/> instance for further chaining.</returns>
        public RockToolResult WithInstructions( string instructions )
        {
            Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Adds a reference URL to this result, optionally performing security checks
        /// before including it. Useful for attaching “learn more” or follow-up links
        /// to the tool’s response.
        /// </summary>
        /// <param name="context">
        /// The current request context, used for authorization checks if <paramref name="secured"/> is true.
        /// </param>
        /// <param name="text">
        /// The display text to show for the reference link (e.g. “View Profile”).
        /// </param>
        /// <param name="route">
        /// The absolute or relative URL of the reference.
        /// </param>
        /// <param name="secured">
        /// If true, the URL is only included if the current user is authorized for the route.  
        /// Defaults to true.
        /// </param>
        /// <returns>
        /// The same <see cref="RockToolResult"/> instance for fluent chaining.
        /// </returns>
        public RockToolResult WithReferenceRoute( RockRequestContext context, string text, string route, bool secured = true )
        {
            bool allowed = !secured || IsAuthorizedForRoute( context, route );

            if ( allowed )
            {
                ReferenceUrl = new ReferenceUrlResult
                {
                    Text = text,
                    Url = ResolveRockUrlIncludeRoot( context, route )
                };
            }

            return this;
        }

        /// <summary>
        /// Sets the content of the result and returns the updated <see cref="RockToolResult"/> instance.
        /// </summary>
        /// <param name="payload">The content to set. This can be any object representing the result's content.</param>
        /// <returns>The current <see cref="RockToolResult"/> instance with the updated content.</returns>
        public RockToolResult WithContent( object payload )
        {
            SetContent( this, payload );
            return this;
        }

        /// <summary>
        /// Sets the history content key on this result and returns the same instance.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RockToolResult WithHistoryKey( string key )
        {
            HistoryContentKey = key;
            return this;
        }

        /// <summary>
        /// Sets non-serialized history content on this result and returns the same instance.
        /// </summary>
        /// <param name="value">The value to store in chat history only.</param>
        /// <param name="key">The key of the history content.</param>
        /// <returns>The same <see cref="RockToolResult"/> instance for further chaining.</returns>
        public RockToolResult WithHistoryContent( object value, string key = "" )
        {
            HistoryContent = value;
            HistoryContentKey = key;

            return this;
        }

        /// <summary>
        /// Sets the history content to <c>null</c> so that nothing is added to chat history.
        /// </summary>
        /// <returns>The same <see cref="RockToolResult"/> instance for further chaining.</returns>
        public RockToolResult WithoutHistoryContent()
        {
            HistoryContent = null;
            return this;
        }

        /// <summary>
        /// Attaches metadata to this result and returns the same instance.
        /// Replaces any existing metadata dictionary.
        /// </summary>
        /// <param name="meta">The metadata dictionary to attach.</param>
        /// <returns>The same <see cref="RockToolResult"/> instance for further chaining.</returns>
        public RockToolResult WithMetadata( Dictionary<string, object> meta )
        {
            Meta = meta;
            return this;
        }

        /// <summary>
        /// Adds a single metadata entry to this result, creating the dictionary if needed.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The same <see cref="RockToolResult"/> instance for further chaining.</returns>
        public RockToolResult WithMetadata( string key, object value )
        {
            if ( Meta == null )
            {
                Meta = new Dictionary<string, object>();
            }

            Meta[key] = value;
            return this;
        }

        #endregion

        #region Helpers

        internal class ReferenceUrlResult
        {
            public string Text { get; set; }
            public string Url { get; set; }
        }

        /// <summary>
        /// Determines whether the person making the request has access to
        /// the page identified by the route.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="route">The route to be checked.</param>
        /// <returns><c>true</c> if the route was found and the requesting person is authorized; otherwise, <c>false</c>.</returns>
        private static bool IsAuthorizedForRoute( RockRequestContext context, string route )
        {
            try
            {
                // Replace any parameters in the route with fake values.
                route = new Regex( "{[^}]+}" ).Replace( route, "1" );

                // Resolve the route based on the current request.
                route = ResolveRockUrlIncludeRoot( context, route );

                // Try to parse the URL, if we can't then assume they can't
                // access the page.
                if ( !Uri.TryCreate( route, UriKind.Absolute, out var uri ) )
                {
                    return false;
                }

                // Find a page ref based on the uri.
                var pageRef = new Rock.Web.PageReference( uri, "/" );

                if ( pageRef.IsValid )
                {
                    // If a valid pageref was found, check the security of the page
                    var page = PageCache.Get( pageRef.PageId );

                    if ( page != null )
                    {
                        return page.IsAuthorized( Rock.Security.Authorization.VIEW, context.CurrentPerson );
                    }
                }
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex );
                // Log and move on...
            }

            return false;
        }

        /// <summary>
        /// Resolves the rock URL and includes the original scheme and domain
        /// from the request.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="url">The URL to ben resolved.</param>
        /// <returns>A new string resolved to the proper domain.</returns>
        private static string ResolveRockUrlIncludeRoot( RockRequestContext context, string url )
        {
            var virtualPath = context.ResolveRockUrl( url );

            // If they gave us an absolute URL with a hostname, just return it.
            if ( virtualPath.Contains( "://" ) )
            {
                return virtualPath;
            }

            if ( context.RootUrlPath.IsNotNullOrWhiteSpace() )
            {
                return $"{context.RootUrlPath}{virtualPath}";
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
        }

        /// <summary>
        /// Sets the content of this result based on the provided payload.
        /// </summary>
        /// <param name="result">The <see cref="RockToolResult"/> to modify.</param>
        /// <param name="payload">The payload to set as the content.</param>
        private static void SetContent( RockToolResult result, object payload )
        {
            if ( IsEnumerablePayload( payload ) )
            {
                result.Results = ( IEnumerable ) payload;
            }
            else
            {
                result.Content = payload;
            }
        }

        /// <summary>
        /// Determines whether the specified payload should be emitted as a sequence.
        /// </summary>
        /// <param name="payload">The payload to check.</param>
        /// <returns><c>true</c> if the payload implements <see cref="IEnumerable"/> and is not a <see cref="string"/>; otherwise <c>false</c>.</returns>
        private static bool IsEnumerablePayload( object payload )
        {
            if ( payload == null )
            {
                return false;
            }
            if ( payload is string ) { return false; }

            return payload is IEnumerable;
        }

        #endregion
    }
}
