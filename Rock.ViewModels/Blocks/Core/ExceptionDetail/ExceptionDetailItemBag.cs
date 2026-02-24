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

namespace Rock.ViewModels.Blocks.Core.ExceptionDetail
{
    /// <summary>
    /// Represents a single entry in the exception hierarchy for the Exception Detail block.
    /// </summary>
    public class ExceptionDetailItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this exception log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the exception type name (e.g., System.NullReferenceException).
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the source of the exception — the application or object that caused the error.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the exception description message.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the stack trace for this exception entry.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a stack trace is available to display.
        /// When false, the stack trace toggle button should be hidden.
        /// </summary>
        public bool HasStackTrace { get; set; }
    }
}
