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

using Rock.Web;

namespace Rock.Blocks
{
    /// <summary>
    /// The result of a call to <see cref="IBreadCrumbBlock.GetBreadCrumbs(PageReference)"/>.
    /// </summary>
    public class BreadCrumbResult
    {
        /// <summary>
        /// Gets or sets the breadcrumbs that represents the page.
        /// </summary>
        /// <value>
        /// The breadcrumbs that represents the page.
        /// </value>
        public List<IBreadCrumb> BreadCrumbs { get; set; }

        /// <summary>
        /// Gets or sets the set of additional parameters that should be
        /// provided to parent pages. Any duplicate existing parameters will
        /// be replaced by these new ones.
        /// </summary>
        /// <value>
        /// The set of additional parameters that should be provided to parent pages.
        /// </value>
        public Dictionary<string, string> AdditionalParameters { get; set; }
    }
}
