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

using Rock.Security;

namespace Rock.Model
{
    public partial class WebFarmNodeMetric
    {
        /// <summary>
        /// Represents the severity of the log entry.
        /// </summary>
        public enum TypeOfMetric
        {
            /// <summary>
            /// The percent of total available CPU power being used by the node.
            /// A MetricValue of 1 is 1% and 99.99 is 99.99%.
            /// </summary>
            CpuUsagePercent = 0,

            /// <summary>
            /// The number of megabytes of RAM currently being utilized by the node.
            /// </summary>
            MemoryUsageMegabytes = 1,
        }

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => WebFarmNode ?? base.ParentAuthority;

        #endregion
    }
}
