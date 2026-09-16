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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// Represents the approval status of an HTML content version as shown in
    /// the HTML Content block. This is a transport value only; it is derived
    /// from the IsApproved flag and approver fields on the HtmlContent entity
    /// and is never persisted.
    /// </summary>
    public enum HtmlContentApprovalStatus
    {
        /// <summary>
        /// The content has not been reviewed. IsApproved is false and no
        /// approver is recorded.
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// The content has been approved. IsApproved is true and the approver
        /// is recorded.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The content was reviewed and denied. IsApproved is false and the
        /// denier is recorded in the approver fields.
        /// </summary>
        Denied = 3
    }
}
