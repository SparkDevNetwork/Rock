using System;

namespace Rock.AI.Agent.Classes
{
    /// <summary>
    /// Wrapper indicating an intent to either set a value or clear an existing value.
    /// </summary>
    /// <typeparam name="T">The value type. Usually nullable for clearing semantics.</typeparam>
    /// <remarks>
    /// Null instance ⇒ no change. <see cref="ClearValue"/> takes precedence over <see cref="Value"/>.
    /// </remarks>
    internal class SetOrClear<T>
    {
        /// <summary>
        /// The value to set when <see cref="ClearValue"/> is false. Ignored when clearing.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// When true, clear the target field regardless of <see cref="Value"/>.
        /// </summary>
        public bool ClearValue { get; set; }
    }

    /// <summary>
    /// The set of utility methods for working with <see cref="SetOrClear{T}"/> wrappers.
    /// </summary>
    internal static class SetOrClearUtilities
    {
        /// <summary>
        /// Applies a <see cref="SetOrClear{T}"/> wrapper to a target using delegates.
        /// </summary>
        /// <typeparam name="T">The wrapped value type.</typeparam>
        /// <param name="source">Null = no change. ClearValue=true clears; otherwise Value (when not null) is applied.</param>
        /// <param name="setAction">Invoked to set the value.</param>
        /// <param name="clearAction">Invoked to clear the value.</param>
        public static void SetOrClearValue<T>( SetOrClear<T> source, Action<T> setAction, Action clearAction )
        {
            if ( source == null )
            {
                return;
            }
            if ( source.ClearValue )
            {
                clearAction();
            }
            else if ( source.Value != null )
            {
                setAction( source.Value );
            }
        }
    }
}
