using System;

namespace Rock
{
    /// <summary>
    /// Boolean Extensions
    /// </summary>
    public static class BooleanExtensions
    {
        #region Boolean Extensions

        /// <summary>
        /// Returns a numeric 1 (if true) or 0 (if false).
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static int Bit( this Boolean field )
        {
            return field ? 1 : 0;
        }

        /// <summary>
        /// Returns either "Yes" or "No".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToYesNo( this bool value )
        {
            return value ? "Yes" : "No";
        }

        /// <summary>
        /// Returns the string "True" or "False".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToTrueFalse( this bool value )
        {
            return value ? "True" : "False";
        }

        /// <summary>
        /// Use AsBoolean() instead.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [Obsolete( "Use AsBoolean() instead" )]
        public static bool FromTrueFalse( this string value )
        {
            return value.Equals( "True" );
        }

        #endregion Boolean Extensions
    }
}
