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

namespace Rock.Lava
{
    /// <summary>
    /// A set of options for rendering a Lava template.
    /// </summary>
    public class LavaRenderParameters
    {
        #region Factory Methods

        public static LavaRenderParameters WithContext( ILavaRenderContext context )
        {
            return new LavaRenderParameters { Context = context };
        }

        public static LavaRenderParameters Default()
        {
            return new LavaRenderParameters();
        }

        #endregion

        /// <summary>
        /// The context in which the Lava template is rendered.
        /// </summary>
        public ILavaRenderContext Context { get; set; }

        /// <summary>
        /// Should string values be XML encoded?
        /// </summary>
        public bool ShouldEncodeStringsAsXml { get; set; }

        /// <summary>
        /// The key value to uniquely identify this template for caching purposes.
        /// If not specified, the template content is used to calculate the cache key.
        /// </summary>
        public string CacheKey { get; set; } = null;
    }
}
