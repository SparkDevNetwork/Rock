using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Minimal, non-generic result envelope for kernel/agent functions.
    /// <para>
    /// Return this type from your methods and use the static factories
    /// <see cref="Success(object, string, Dictionary{string, object})"/>,
    /// <see cref="NoData(string, Dictionary{string, object})"/>, and
    /// <see cref="Error(string, string, Dictionary{string, object})"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This implementation avoids inheritance and generics entirely. The payload is carried in either
    /// <see cref="Content"/> (for single objects, including <see cref="string"/>) or <see cref="Results"/>
    /// (for sequences implementing <see cref="IEnumerable"/>). Only one of these is set for a given instance.
    /// </remarks>
    public sealed class RockFunctionResult
    {
        /// <summary>
        /// Outcome of the function.
        /// </summary>
        [JsonInclude]
        internal FunctionStatus Status { get; private set; }

        /// <summary>
        /// Error message when <see cref="Status"/> is <see cref="FunctionStatus.Error"/>; omitted otherwise.
        /// </summary>
        [JsonInclude]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal string ErrorMessage { get; private set; }

        /// <summary>
        /// Optional LLM-facing guidance on how to proceed after a failure or when no data is available.
        /// </summary>
        [JsonInclude]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal string Instructions { get; private set; }

        /// <summary>
        /// For single-object payloads (including strings). Omitted from JSON when <c>null</c>.
        /// </summary>
        [JsonInclude]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal object Content { get; private set; }

        /// <summary>
        /// For sequence payloads (any <see cref="IEnumerable"/> except <see cref="string"/>). Omitted when <c>null</c>.
        /// </summary>
        [JsonInclude]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal IEnumerable Results { get; private set; }

        /// <summary>
        /// Optional metadata to include alongside the result (paging info, correlation ids, etc.).
        /// Omitted from JSON when <c>null</c>.
        /// </summary>
        [JsonInclude]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        internal Dictionary<string, object> Meta { get; private set; }

        /// <summary>
        /// Creates a success result for the specified <paramref name="payload"/>. If <paramref name="payload"/>
        /// implements <see cref="IEnumerable"/> (and is not a <see cref="string"/>), it is emitted via
        /// <see cref="Results"/>; otherwise it is emitted via <see cref="Content"/>.
        /// </summary>
        /// <param name="payload">The payload object or sequence.</param>
        /// <param name="instructions">Optional LLM-facing guidance to include.</param>
        /// <param name="meta">Optional metadata to include.</param>
        /// <returns>A success <see cref="RockFunctionResult"/>.</returns>
        public static RockFunctionResult Success(
            object payload,
            string instructions = null,
            Dictionary<string, object> meta = null )
        {
            var r = new RockFunctionResult
            {
                Status = FunctionStatus.Success,
                Instructions = instructions
            };

            if ( payload != null && IsEnumerablePayload( payload ) )
            {
                r.Results = ( IEnumerable ) payload;
            }
            else
            {
                r.Content = payload;
            }

            if ( meta != null && meta.Count > 0 )
            {
                r.Meta = meta;
            }

            return r;
        }

        /// <summary>
        /// Creates a no-data result. Both <see cref="Content"/> and <see cref="Results"/> are omitted.
        /// </summary>
        /// <param name="instructions">Optional LLM-facing guidance on how to recover or proceed.</param>
        /// <param name="meta">Optional metadata to include (paging, ids, etc.).</param>
        public static RockFunctionResult NoData(
            string instructions = null,
            Dictionary<string, object> meta = null )
        {
            var r = new RockFunctionResult
            {
                Status = FunctionStatus.NoData,
                Instructions = instructions
            };

            if ( meta != null && meta.Count > 0 )
            {
                r.Meta = meta;
            }

            return r;
        }

        /// <summary>
        /// Creates an error result.
        /// </summary>
        /// <param name="message">Human-readable error message.</param>
        /// <param name="instructions">Optional LLM-facing guidance on how to recover or proceed.</param>
        /// <param name="meta">Optional metadata to include (paging, ids, etc.).</param>
        public static RockFunctionResult Error(
            string message,
            string instructions = null,
            Dictionary<string, object> meta = null )
        {
            var r = new RockFunctionResult
            {
                Status = FunctionStatus.Error,
                ErrorMessage = message ?? string.Empty,
                Instructions = instructions
            };

            if ( meta != null && meta.Count > 0 )
            {
                r.Meta = meta;
            }

            return r;
        }

        /// <summary>
        /// Helper: returns <c>true</c> when the object should be emitted as a sequence (i.e., implements
        /// <see cref="IEnumerable"/> and is not a <see cref="string"/>).
        /// </summary>
        private static bool IsEnumerablePayload( object payload )
        {
            if ( payload == null ) return false;
            if ( payload is string ) return false;
            return payload is IEnumerable;
        }
    }
}
