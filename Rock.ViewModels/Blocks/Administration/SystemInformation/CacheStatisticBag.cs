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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Administration.SystemInformation
{
    /// <summary>
    /// The statistics for a single cache.
    /// </summary>
    public class CacheStatisticBag
    {
        /// <summary>
        /// Gets or sets the name of the cache.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the statistics for the cache, each formatted as "CounterType: Count".
        /// </summary>
        public List<string> Statistics { get; set; }
    }
}
