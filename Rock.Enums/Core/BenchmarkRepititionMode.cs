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
namespace Rock.Enums.Core
{
    /// <summary>
    /// Provides a general mode to the number of repititions to run during
    /// a benchmark.
    /// </summary>
    public enum BenchmarkRepititionMode
    {
        /// <summary>
        /// A normal run. This takes 30-60 seconds.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// A fast run, less accurate. This takes 15-30 seconds.
        /// </summary>
        Fast = 1,

        /// <summary>
        /// An extended run, slightly more accurate. This takes 60-90 seconds.
        /// </summary>
        Extended = 2
    }
}
