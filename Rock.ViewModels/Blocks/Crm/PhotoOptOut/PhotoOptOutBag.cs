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

namespace Rock.ViewModels.Blocks.Crm.PhotoOptOut
{
    /// <summary>
    /// 
    /// </summary>
    public class PhotoOptOutBag
    {
        /// <summary>
        /// Gets or sets the TagId of the Rock.Model.Tag that this TaggedItem is tagged with.
        /// </summary>
        public bool IsOptOutSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message. A non-empty value indicates that
        /// an error is preventing the block from being displayed.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>
        /// The success message.
        /// </value>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the alert.
        /// </summary>
        /// <value>
        /// The type of the alert.
        /// </value>
        public string AlertType { get; set; }
    }
}
