using System;
using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Enums;

/// <summary>
/// A minimal, standardized envelope for lookup-style results returned by kernel functions or repositories.
/// </summary>
/// <typeparam name="T">
/// The element type of the result set. Must be a reference type.
/// </typeparam>
/// <remarks>
/// <para>
/// This wrapper provides a consistent JSON shape for Semantic Kernel tool/function results and for internal consumers.
/// It encapsulates the result list, a status code, an error message (if any), and a small metadata bag for optional extras
/// (e.g., correlation IDs, timing, or echoed inputs).
/// </para>
/// <para>
/// Instances are created via the factory methods:
/// <see cref="Success(IEnumerable{T}, Dictionary{string, object})"/>,
/// <see cref="NoData(Dictionary{string, object})"/>, and
/// <see cref="Error(string, Dictionary{string, object})"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Success with items
/// // No data
/// var empty = LookupFunctionResult&lt;Person&gt;.NoData();
///
/// // Error
/// var error = LookupFunctionResult&lt;Person&gt;.Error("Connection timed out", new() { ["correlationId"] = cid });
/// </code>
/// </example>
public class LookupFunctionResult<T> where T : class
{
    /// <summary>
    /// Gets the outcome of the lookup operation.
    /// </summary>
    /// <value>
    /// One of <see cref="LookupStatus.Success"/>, <see cref="LookupStatus.NoData"/>, or <see cref="LookupStatus.Error"/>.
    /// </value>
    public LookupStatus Status { get; private set; }

    /// <summary>
    /// Gets a human-readable description of the error when <see cref="Status"/> is <see cref="LookupStatus.Error"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="string.Empty"/> for non-error states.
    /// </remarks>
    public string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the collection of items returned by the lookup.
    /// </summary>
    /// <remarks>
    /// Never <c>null</c>. For <see cref="LookupStatus.NoData"/> and <see cref="LookupStatus.Error"/>,
    /// this will be an empty list.
    /// </remarks>
    public List<T> Results { get; private set; } = new List<T>();

    /// <summary>
    /// Gets an extensible bag of metadata associated with the result.
    /// </summary>
    /// <remarks>
    /// Typical values include timing information, echoed input parameters for traceability,
    /// correlation identifiers, or other lightweight context relevant to the caller.
    /// </remarks>
    public Dictionary<string, object> Meta { get; private set; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates a result with <see cref="LookupStatus.Success"/> when <paramref name="results"/> contains items,
    /// or <see cref="LookupStatus.NoData"/> when it does not.
    /// </summary>
    /// <param name="results">
    /// The items to include in the result. If <c>null</c> or empty, the status will be <see cref="LookupStatus.NoData"/>.
    /// </param>
    /// <param name="meta">
    /// Optional metadata dictionary to attach to the result. If <c>null</c>, an empty dictionary is used.
    /// </param>
    /// <returns>
    /// A <see cref="LookupFunctionResult{T}"/> whose <see cref="Status"/> is either
    /// <see cref="LookupStatus.Success"/> or <see cref="LookupStatus.NoData"/>, depending on <paramref name="results"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var ok = LookupFunctionResult&lt;Order&gt;.Success(orders, new() { ["elapsedMs"] = 12 });
    /// </code>
    /// </example>
    public static LookupFunctionResult<T> Success(
        IEnumerable<T> results,
        Dictionary<string, object> meta = null )
    {
        return new LookupFunctionResult<T>
        {
            Status = ( results != null && results.Any() ) ? LookupStatus.Success : LookupStatus.NoData,
            Results = results != null ? results.ToList() : new List<T>(),
            Meta = meta ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a result that represents a successful lookup with no items.
    /// </summary>
    /// <param name="meta">
    /// Optional metadata dictionary to attach to the result.
    /// </param>
    /// <returns>
    /// A <see cref="LookupFunctionResult{T}"/> with <see cref="Status"/> set to <see cref="LookupStatus.NoData"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var none = LookupFunctionResult&lt;Product&gt;.NoData(new() { ["query"] = "color:red" });
    /// </code>
    /// </example>
    public static LookupFunctionResult<T> NoData( Dictionary<string, object> meta = null )
    {
        return new LookupFunctionResult<T>
        {
            Status = LookupStatus.NoData,
            Results = new List<T>(),
            Meta = meta ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a result that represents a failed lookup.
    /// </summary>
    /// <param name="message">
    /// The error message describing the failure. If <c>null</c>, an empty string is used.
    /// </param>
    /// <param name="meta">
    /// Optional metadata dictionary to attach to the result (e.g., correlation IDs, exception types, retry hints).
    /// </param>
    /// <returns>
    /// A <see cref="LookupFunctionResult{T}"/> with <see cref="Status"/> set to <see cref="LookupStatus.Error"/>
    /// and <see cref="ErrorMessage"/> populated.
    /// </returns>
    /// <example>
    /// <code>
    /// var fail = LookupFunctionResult&lt;Invoice&gt;.Error("SQL timeout", new() { ["retryAfterMs"] = 500 });
    /// </code>
    /// </example>
    public static LookupFunctionResult<T> Error( string message, Dictionary<string, object> meta = null )
    {
        return new LookupFunctionResult<T>
        {
            Status = LookupStatus.Error,
            ErrorMessage = message ?? string.Empty,
            Results = new List<T>(),
            Meta = meta ?? new Dictionary<string, object>()
        };
    }
}