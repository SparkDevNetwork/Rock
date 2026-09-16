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
    /// A synthetic inbound SMS message submitted by the testing drawer to run
    /// through the live pipeline without involving the SMS provider.
    /// </summary>
    public class SmsActionTestRequestBag
    {
        /// <summary>
        /// Gets or sets the From phone number to simulate.
        /// </summary>
        public string FromNumber { get; set; }

        /// <summary>
        /// Gets or sets the To phone number to simulate.
        /// </summary>
        public string ToNumber { get; set; }

        /// <summary>
        /// Gets or sets the message body to run through the pipeline.
        /// </summary>
        public string Message { get; set; }
    }
}
