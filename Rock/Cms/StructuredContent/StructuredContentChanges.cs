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
using System.Collections.Generic;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Contains the detected changes to the content that the developer might
    /// need to take action on.
    /// </summary>
    public class StructuredContentChanges
    {
        #region Fields

        /// <summary>
        /// The component data stores custom data from components.
        /// </summary>
        private readonly Dictionary<Type, object> _componentData = new Dictionary<Type, object>();

        #endregion

        #region Methods

        /// <summary>
        /// Sets the data to the specified value. Plugins can use
        /// this to store custom data to act on later.
        /// </summary>
        /// <param name="value">The extension data.</param>
        public void AddOrReplaceData<TData>( TData value )
            where TData : class
        {
            _componentData.AddOrReplace( typeof( TData ), value );
        }

        /// <summary>
        /// Gets the data for the given key. Plugins can use this to
        /// retrieve previously stored data.
        /// </summary>
        /// <typeparam name="TData">The data type to be retrieved.</typeparam>
        /// <returns>The data value or <c>null</c> if not found.</returns>
        public TData GetData<TData>()
            where TData : class
        {
            if ( _componentData.TryGetValue( typeof(TData), out var value ) )
            {
                return ( TData ) value;
            }

            return null;
        }

        #endregion
    }
}
