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

namespace Rock.ViewModels.Blocks.Communication.SmsPipelineDetail
{
    /// <summary>
    /// The outcome of a single action's processing pass during a test message run.
    /// </summary>
    public class SmsActionTestOutcomeBag
    {
        /// <summary>
        /// Gets or sets the configured action's display name.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the action's filters matched
        /// the inbound message and the action processed it.
        /// </summary>
        public bool ShouldProcess { get; set; }

        /// <summary>
        /// Gets or sets the outbound response message produced by the action, when any.
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the action logged an interaction
        /// record for this run.
        /// </summary>
        public bool IsInteractionLogged { get; set; }

        /// <summary>
        /// Gets or sets the structured error message returned by the action, when any.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the exception message thrown by the action, when any.
        /// </summary>
        public string ExceptionMessage { get; set; }
    }
}
