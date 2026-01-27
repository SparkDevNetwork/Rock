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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Represents a container for storing information about a benevolence result, including its type, amount, and
    /// summary details.
    /// </summary>
    /// <remarks>This class is used to encapsulate the details of a benevolence result, which may include the
    /// type of result, the amount involved, and a summary of the result.</remarks>
    public class BenevolenceResultBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Id of the Defined Value representing the type of Benevolence Result.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the type of Benevolence Result.
        /// </value>
        public int ResultTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the amount of benevolence
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the amount of benevolence.
        /// </value>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the text of the result details.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the details of the result.
        /// </value>
        public string ResultSummary { get; set; }
    }
}
