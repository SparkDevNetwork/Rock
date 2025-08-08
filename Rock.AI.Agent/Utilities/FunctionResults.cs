using System;
using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Enums;

namespace Rock.AI.Agent.Utilities
{
    /// <summary>
    /// Minimal, standardized envelope for function results returned by kernel functions.
    /// Use this for single-payload actions (SendEmail, etc).
    /// </summary>
    /// <typeparam name="TPayload">The payload type for the function result. Must be a reference type.</typeparam>
    public class RockFunctionResult<TPayload> where TPayload : class
    {
        /// <summary>Outcome of the function.</summary>
        public FunctionStatus Status { get; protected set; }

        /// <summary>Error message when <see cref="Status"/> is <see cref="FunctionStatus.Error"/>; empty otherwise.</summary>
        public string ErrorMessage { get; protected set; } = string.Empty;

        /// <summary>Primary payload; null when not applicable.</summary>
        public TPayload Payload { get; protected set; }

        /// <summary>Extensible metadata (correlation IDs, timings, echoed inputs, etc.).</summary>
        public Dictionary<string, object> Meta { get; protected set; } = new Dictionary<string, object>();

        /// <summary>Create a successful result with an optional payload and metadata.</summary>
        public static RockFunctionResult<TPayload> Success( TPayload payload = null, Dictionary<string, object> meta = null )
        {
            return new RockFunctionResult<TPayload>
            {
                Status = FunctionStatus.Success,
                Payload = payload,
                Meta = meta ?? new Dictionary<string, object>()
            };
        }

        /// <summary>Create a no-data result (commonly used by lookups).</summary>
        public static RockFunctionResult<TPayload> NoData( Dictionary<string, object> meta = null )
        {
            return new RockFunctionResult<TPayload>
            {
                Status = FunctionStatus.NoData,
                Payload = null,
                Meta = meta ?? new Dictionary<string, object>()
            };
        }

        /// <summary>Create an error result with a message and optional metadata.</summary>
        public static RockFunctionResult<TPayload> Error( string message, Dictionary<string, object> meta = null )
        {
            return new RockFunctionResult<TPayload>
            {
                Status = FunctionStatus.Error,
                ErrorMessage = message ?? string.Empty,
                Payload = null,
                Meta = meta ?? new Dictionary<string, object>()
            };
        }
    }

    /// <summary>
    /// Convenience non-generic wrapper for scenarios where no payload is needed.
    /// </summary>
    public sealed class RockFunctionResult : RockFunctionResult<object>
    {
        public static RockFunctionResult Success( Dictionary<string, object> meta = null )
            => ( RockFunctionResult ) RockFunctionResult<object>.Success( null, meta );

        public static new RockFunctionResult NoData( Dictionary<string, object> meta = null )
            => ( RockFunctionResult ) RockFunctionResult<object>.NoData( meta );

        public static new RockFunctionResult Error( string message, Dictionary<string, object> meta = null )
            => ( RockFunctionResult ) RockFunctionResult<object>.Error( message, meta );
    }

    /// <summary>
    /// Standardized envelope for lookup-style results (lists of items).
    /// Inherits the base function result and sets the payload to a list of items.
    /// </summary>
    /// <typeparam name="TItem">The element type of the result set. Must be a reference type.</typeparam>
    public class LookupFunctionResult<TItem> : RockFunctionResult<List<TItem>> where TItem : class
    {
        /// <summary>
        /// Creates a result with <see cref="FunctionStatus.Success"/> when <paramref name="results"/> has items,
        /// or <see cref="FunctionStatus.NoData"/> when it does not.
        /// </summary>
        public static LookupFunctionResult<TItem> Success( IEnumerable<TItem> results, Dictionary<string, object> meta = null )
        {
            var list = results != null ? results.ToList() : new List<TItem>();
            return new LookupFunctionResult<TItem>
            {
                Status = list.Any() ? FunctionStatus.Success : FunctionStatus.NoData,
                Payload = list,
                Meta = meta ?? new Dictionary<string, object>()
            };
        }

        /// <summary>Create an explicit no-data result.</summary>
        public static new LookupFunctionResult<TItem> NoData( Dictionary<string, object> meta = null )
        {
            return new LookupFunctionResult<TItem>
            {
                Status = FunctionStatus.NoData,
                Payload = new List<TItem>(),
                Meta = meta ?? new Dictionary<string, object>()
            };
        }

        /// <summary>Create an error result with a message and optional metadata.</summary>
        public static new LookupFunctionResult<TItem> Error( string message, Dictionary<string, object> meta = null )
        {
            return new LookupFunctionResult<TItem>
            {
                Status = FunctionStatus.Error,
                ErrorMessage = message ?? string.Empty,
                Payload = new List<TItem>(),
                Meta = meta ?? new Dictionary<string, object>()
            };
        }
    }
}
