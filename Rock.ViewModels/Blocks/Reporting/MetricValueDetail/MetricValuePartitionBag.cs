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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reporting.MetricValueDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class MetricValuePartitionBag
    {
        /// <summary>
        /// Gets or sets the configuration details of the entity type as an attribute so a matching field type control is rendered.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public PublicAttributeBag Attribute { get; set; }

        /// <summary>
        /// Gets or sets the saved entity value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the metric partition unique identifier.
        /// </summary>
        /// <value>
        /// The metric partition unique identifier.
        /// </value>
        public Guid MetricPartitionGuid { get; set; }
    }
}
