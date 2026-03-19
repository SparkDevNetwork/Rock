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
    /// Represents an individual exception log entry in the exception hierarchy.
    /// </summary>
    public class ExceptionLogItemBag
    {
        /// <summary>
        /// Gets or sets the identifier of this exception log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the exception class name (e.g., "System.Web.HttpException").
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the source application or object that threw the exception.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the call stack trace for this exception.
        /// </summary>
        public string StackTrace { get; set; }
    }
}
