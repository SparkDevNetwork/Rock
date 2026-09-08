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
    /// The data access/service class for the <see cref="Rock.Model.PageRoute"/> class.
    /// </summary>
    public partial class PageRouteService
    {
        /// <summary>
        /// Gets an enumerable list of <see cref="Rock.Model.PageRoute"/> entities that are linked to a <see cref="Rock.Model.Page"/> by the 
        /// by the <see cref="Rock.Model.Page">Page's</see> Id.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> value containing the Id of the <see cref="Rock.Model.Page"/> .</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PageRoute"/> entities that reference the supplied PageId.</returns>
        public IQueryable<PageRoute> GetByPageId( int pageId )
        {
            return Queryable().Where( t => t.PageId == pageId );
        }

        /*
            8/31/2026 - CLAUDE

            RockRouteHandler implements System.Web.Routing.IRouteHandler, so the
            projects that must stay clear of System.Web (Rock.Blocks and
            Rock.AI.Agent) cannot call ReregisterRoutes directly. Publishing
            PageRouteWasUpdatedMessage instead looks equivalent but is not: the
            consumer ignores messages sent by its own node, on the understanding
            that the publisher already rebuilt its own table, so the publishing
            node never picks up the change. This shim keeps the System.Web
            dependency inside Rock and gives those projects the correct call.

            Reason: Route changes saved outside RockWeb did not take effect until an application restart.
        */

        /// <summary>
        /// Rebuilds this node's route table so <see cref="PageRoute"/> changes take
        /// effect immediately, then notifies the other web farm nodes to do the same.
        /// Call this after saving or deleting page routes.
        /// </summary>
        public static void RefreshRouteTable()
        {
            Rock.Web.RockRouteHandler.ReregisterRoutes();
        }
    }
}
