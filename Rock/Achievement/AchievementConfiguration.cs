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
using Rock.Web.Cache;

namespace Rock.Achievement
{
    /// <summary>
    /// Achievement Configuration
    /// </summary>
    public sealed class AchievementConfiguration
    {
        private readonly Type _sourceType;
        private readonly Type _achieverType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementConfiguration"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="achiever">The achiever.</param>
        public AchievementConfiguration( Type source, Type achiever )
        {
            _sourceType = source;
            _achieverType = achiever;
        }

        /// <summary>
        /// Gets the source entity type cache.
        /// </summary>
        public EntityTypeCache SourceEntityTypeCache => EntityTypeCache.Get( _sourceType );

        /// <summary>
        /// Gets the achiever entity type cache.
        /// </summary>
        public EntityTypeCache AchieverEntityTypeCache => EntityTypeCache.Get( _achieverType );
    }
}
