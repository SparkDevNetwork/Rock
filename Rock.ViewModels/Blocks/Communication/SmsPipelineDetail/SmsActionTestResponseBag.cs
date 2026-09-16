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

namespace Rock.ViewModels.Blocks.Communication.SmsPipelineDetail
{
    /// <summary>
    /// The result of running a test message through the pipeline.
    /// </summary>
    public class SmsActionTestResponseBag
    {
        /// <summary>
        /// Gets or sets the outbound response selected from the action outcomes.
        /// </summary>
        /// <remarks>Null when no action produced a response.</remarks>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the per-action outcomes for the test run, in pipeline order.
        /// </summary>
        public List<SmsActionTestOutcomeBag> Outcomes { get; set; }

        /// <summary>
        /// Gets or sets the structured error message returned when the pipeline
        /// could not be processed.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
