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

namespace Rock.ViewModels.Blocks.Core.ExceptionDetail
{
    /// <summary>
    /// The bag containing the data for the Exception Detail block.
    /// </summary>
    public class ExceptionDetailBag
    {
        /// <summary>
        /// Gets or sets the root (outermost) exception's display data.
        /// </summary>
        public ExceptionLogItemBag RootException { get; set; }

        /// <summary>
        /// Gets or sets the raw HTML content of the cookies at the time of the exception.
        /// </summary>
        public string Cookies { get; set; }

        /// <summary>
        /// Gets or sets the raw HTML content of the server variables at the time of the exception.
        /// </summary>
        public string ServerVariables { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of inner (non-root) exception log items.
        /// </summary>
        public List<ExceptionLogItemBag> InnerExceptions { get; set; }
    }
}
