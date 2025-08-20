using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Represents a standardized envelope for function results returned by agent skills and kernel functions.
    /// Provides a clean JSON shape and a simple builder-style API for attaching metadata and guidance.
    /// </summary>
    /// <remarks>
    /// - Exactly one of <see cref="Content"/> or <see cref="Results"/> will be set when using the factory methods.
    /// - <see cref="HistoryContent"/> is not serialized and is only used for chat history plumbing.
    /// - Properties are marked <see langword="internal"/> but included in JSON via <see cref="JsonIncludeAttribute"/> to minimize public surface area.
    /// </remarks>
    public sealed class RockToolResult
    {
        #region Properties

        /// <summary>
        /// Gets the overall outcome for the function call.
        /// </summary>
        [JsonInclude]
        internal FunctionStatus Status { get; private set; }

        /// <summary>
        /// Gets the error messages when <see cref="Status"/> is <see cref="FunctionStatus.Error"/>;
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
        /// Gets arbitrary content that should be added to chat history but not serialized in the function result payload.
        /// </summary>
        [JsonIgnore]
        internal object HistoryContent { get; private set; }

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

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents direct instantiation. Use the static factory methods to create instances.
        /// </summary>
        private RockToolResult() { }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a <see cref="FunctionStatus.Success"/> result wrapping the specified payload.
        /// If <paramref name="payload"/> implements <see cref="IEnumerable"/> (and is not a <see cref="string"/>)
        /// then it will be assigned to <see cref="Results"/>; otherwise to <see cref="Content"/>.
        /// </summary>
        /// <param name="payload">The value to include in the result.</param>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Success( object payload )
        {
            var result = new RockToolResult { Status = FunctionStatus.Success };

            SetContent( result, payload );

            return result;
        }

        /// <summary>
        /// Creates a <see cref="FunctionStatus.Success"/> result with no payload.
        /// </summary>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Success()
        {
            return new RockToolResult
            {
                Status = FunctionStatus.Success,
            };
        }

        /// <summary>
        /// Creates a <see cref="FunctionStatus.NoData"/> result with no payload.
        /// </summary>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult NoData() =>
            new RockToolResult
            {
                Status = FunctionStatus.NoData
            };

        /// <summary>
        /// Creates a <see cref="FunctionStatus.Error"/> result with a single error message.
        /// </summary>
        /// <param name="message">The error message. If <c>null</c> or whitespace, an empty string is added.</param>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Error( string message ) =>
            new RockToolResult
            {
                Status = FunctionStatus.Error,
                ErrorMessages = new List<string> { message ?? string.Empty }
            };

        /// <summary>
        /// Creates a <see cref="FunctionStatus.Error"/> result with one or more error messages.
        /// </summary>
        /// <param name="messages">The collection of error messages.</param>
        /// <returns>A new <see cref="RockToolResult"/> instance.</returns>
        public static RockToolResult Error( IEnumerable<string> messages ) =>
            new RockToolResult
            {
                Status = FunctionStatus.Error,
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
        /// Sets non-serialized history content on this result and returns the same instance.
        /// </summary>
        /// <param name="value">The value to store in chat history only.</param>
        /// <returns>The same <see cref="RockToolResult"/> instance for further chaining.</returns>
        public RockToolResult WithHistoryContent( object value )
        {
            HistoryContent = value;
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
