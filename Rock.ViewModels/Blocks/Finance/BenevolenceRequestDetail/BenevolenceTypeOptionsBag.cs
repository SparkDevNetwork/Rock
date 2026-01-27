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

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Contains configuration options for a benevolence type.
    /// </summary>
    public class BenevolenceTypeOptionsBag
    {
        /// <summary>
        /// Gets or sets any additional settings for the benevolence type. This is a
        /// <u><em>staging property</em></u> used to retrieve data to be transformed
        /// and is cleared before it is sent to the client.
        /// </summary>
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of documents that can be attached to a benevolence request.
        /// </summary>
        public int MaximumDocuments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether financial results are shown for the benevolence type.
        /// </summary>
        public bool IsShowingFinancialResults { get; set; }
    }
}
