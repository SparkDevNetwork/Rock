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
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// DataViews REST API
    /// </summary>
    public partial class DataViewsController
    {
        #region DataViewByPerson

        /// <summary>
        /// Returns a list of dataviews that a person is a part of
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="entityId"></param>
        /// <param name="categoryGuid"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/DataViews/GetPersistedDataViewsForEntity/{entityTypeId}/{entityId}" )]
        public IQueryable<DataView> GetPersistedDataViewsForEntity( int entityTypeId, int entityId, System.Guid? categoryGuid = null, int categoryId = 0 )
        {
            var rockContext = new RockContext();
            // Get the data view guids from the DataViewPersistedValues table that the Person Id is a part of
            var persistedValuesQuery = rockContext.DataViewPersistedValues.Where( a => a.EntityId == entityId && a.DataView.EntityTypeId == entityTypeId );
            IQueryable<DataView> dataViewList = persistedValuesQuery.Select( a => a.DataView );
            if ( categoryGuid != null )
            {
                dataViewList = dataViewList.Where( a => a.Category.Guid == categoryGuid );
            }
            if ( categoryId != 0 )
            {
                dataViewList = dataViewList.Where( a => a.CategoryId == categoryId );
            }

            // Return DataView as IQueryable
            return dataViewList;
        }

        #endregion
    }
}