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

using DotLiquid;

namespace Rock.Cache
{
    /// <summary>
    /// Information about a definedValue that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class CacheLavaTemplate : ItemCache<CacheLavaTemplate>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private CacheLavaTemplate()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the defined type id.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        public Template Template { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns LavaTemplate object from cache.  If definedValue does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static CacheLavaTemplate Get( string content )
        {
            return GetOrAddExisting( content, () => Load( content ), new TimeSpan( 0, 10, 0 ) );
        }

        private static CacheLavaTemplate Load( string content )
        {
            var lavaTemplate = new CacheLavaTemplate { Template = Template.Parse( content ) };
            return lavaTemplate;
        }

        #endregion
    }
}