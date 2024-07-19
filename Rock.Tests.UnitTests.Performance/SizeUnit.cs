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
namespace Rock.Tests.UnitTests.Performance
{
    /// <summary>
    /// Defines the size to display byte counters in the summary report.
    /// </summary>
    internal enum SizeUnit
    {
        /// <summary>
        /// Values are displayed as bytes.
        /// </summary>
        B,

        /// <summary>
        /// Values are displayed as kilobytes.
        /// </summary>
        KB,

        /// <summary>
        /// Values are displayed as megabytes.
        /// </summary>
        MB,

        /// <summary>
        /// Values are displayed as gigabytes.
        /// </summary>
        GB,

        /// <summary>
        /// Values are displayed as terabytes.
        /// </summary>
        TB
    }
}
