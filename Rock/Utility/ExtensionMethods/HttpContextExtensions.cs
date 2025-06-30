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
#if WEBFORMS
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
#endif
