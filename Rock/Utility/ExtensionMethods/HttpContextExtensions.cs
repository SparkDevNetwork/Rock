using System.Collections;
using System.Web;

namespace Rock
{
    /// <summary>
    /// HttpContext extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Items extension methods

        /// <summary>
        /// Adds or replaces an item in the HTTP context's items collection.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrReplaceItem( this HttpContext httpContext, object key, object value )
        {
            IDictionary items = httpContext?.Items;
            if ( items == null )
            {
                return;
            }

            if ( !items.Contains( key ) )
            {
                items.Add( key, value );
            }
            else
            {
                items[key] = value;
            }
        }

        #endregion Items extension methods
    }
}
