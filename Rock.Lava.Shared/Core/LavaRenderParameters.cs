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
using System.Globalization;

namespace Rock.Lava
{
    /// <summary>
    /// A set of options for rendering a Lava template.
    /// </summary>
    public class LavaRenderParameters
    {
        #region Factory Methods

        /// <summary>
        /// Create a new instance with a specified render context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LavaRenderParameters WithContext( ILavaRenderContext context )
        {
            return new LavaRenderParameters { Context = context };
        }

        private static readonly LavaRenderParameters _defaultValue = new LavaRenderParameters();

        /// <summary>
        /// Create a new default instance.
        /// </summary>
        /// <returns></returns>
        public static LavaRenderParameters Default
        {
            get
            {
                return _defaultValue;
            }
        }

        #endregion

        /// <summary>
        /// The context in which the Lava template is rendered.
        /// </summary>
        public ILavaRenderContext Context { get; set; }

        /// <summary>
        /// Should string values be XML/HTML encoded during the rendering process?
        /// </summary>
        public bool ShouldEncodeStringsAsXml { get; set; }

        /// <summary>
        /// The key value to uniquely identify this template for caching purposes.
        /// If not specified, the template content is used to calculate the cache key.
        /// </summary>
        public string CacheKey { get; set; } = null;

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        public ExceptionHandlingStrategySpecifier? ExceptionHandlingStrategy { get; set; }

        /// <summary>
        /// Gets or sets the culture with which to render the template.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets the timezone for rendering dates and times.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Returns a new object with the same properties as the current object.
        /// </summary>
        /// <returns></returns>
        public LavaRenderParameters Clone()
        {
            var clone = new LavaRenderParameters();
            clone.CacheKey = CacheKey;
            clone.Context = Context;
            clone.Culture = Culture;
            clone.ExceptionHandlingStrategy = ExceptionHandlingStrategy;
            clone.ShouldEncodeStringsAsXml = ShouldEncodeStringsAsXml;
            clone.TimeZone = TimeZone;

            return clone;
        }
    }
}
