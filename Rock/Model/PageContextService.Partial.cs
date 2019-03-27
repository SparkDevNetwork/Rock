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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for the <see cref="Rock.Model.PageContext"/> model object. This class inherits from the Service class.
    /// </summary>
    public partial class PageContextService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PageContext"/> entities that are used on a page.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> that contains the Id of the <see cref="Rock.Model.Page"/> to search by.</param>
        /// <returns>An enumerable list of <see cref="Rock.Model.PageContext">PageContexts</see> that are referenced on the page.</returns>
        public IQueryable<PageContext> GetByPageId( int pageId )
        {
            return Queryable().Where( t => t.PageId == pageId );
        }
    }
}
