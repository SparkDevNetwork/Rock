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
namespace Rock.Tests.Benchmarks
{
    /// <summary>
    /// The unit of time to use when in the summary results.
    /// </summary>
    internal enum TimeUnit
    {
        /// <summary>
        /// Values will be displayed in nanoseconds.
        /// </summary>
        Nanosecond,

        /// <summary>
        /// Values will be displayed in microseconds.
        /// </summary>
        Microsecond,

        /// <summary>
        /// Values will be displayed in milliseconds.
        /// </summary>
        Millisecond,

        /// <summary>
        /// Values will be displayed in seconds.
        /// </summary>
        Second
    }
}
