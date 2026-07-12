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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SmsPipelineDetail
{
    /// <summary>
    /// The SMS pipeline details for the SMS Pipeline Detail block.
    /// </summary>
    public class SmsPipelineBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the pipeline name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the pipeline description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pipeline is active.
        /// </summary>
        /// <remarks>Inactive pipelines do not process inbound SMS messages.</remarks>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the public webhook URL that the configured SMS transport
        /// exposes for this pipeline.
        /// </summary>
        /// <remarks>Null when the active transport does not implement <c>ISmsPipelineWebhook</c>.</remarks>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of actions configured on the pipeline.
        /// </summary>
        public List<SmsActionBag> Actions { get; set; }
    }
}
