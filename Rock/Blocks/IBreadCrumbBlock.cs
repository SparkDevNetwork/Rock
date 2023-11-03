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

using Rock.Web;

namespace Rock.Blocks
{
    /// <summary>
    /// A block that supports custom breadcrumbs.
    /// </summary>
    public interface IBreadCrumbBlock
    {
        /// <summary>
        /// Gets the breadcrumbs for the given page reference. The page may not
        /// be the currently requested page so all parameter data must be taken
        /// from <paramref name="pageReference"/>.
        /// </summary>
        /// <param name="pageReference">The page reference that represents the page. This will contain any parameter data.</param>
        /// <returns>An object that contains any breadcrumbs to be added to the page as well as any additional parameters that parent pages may require.</returns>
        BreadCrumbResult GetBreadCrumbs( PageReference pageReference );
    }
}
