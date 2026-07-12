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

namespace Rock.ViewModels.Blocks.Core.MergeTemplateEntry
{
    /// <summary>
    /// The result of a merge document request.
    /// </summary>
    public class MergeTemplateEntryMergeResponseBag
    {
        /// <summary>
        /// Gets or sets the URL the browser should navigate to in order to download
        /// the generated document. This is empty when the merge did not succeed.
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets a user-facing message describing why the merge failed,
        /// or <c>null</c> when the merge succeeded.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets additional technical detail about a merge failure
        /// (for example, the exception message).
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the error should be shown as a
        /// danger (rather than warning) notification. This is <c>true</c> for
        /// template configuration failures (the template or its type could not be
        /// loaded) and <c>false</c> for runtime merge errors, matching the legacy block.
        /// </summary>
        public bool IsErrorDanger { get; set; }
    }
}
