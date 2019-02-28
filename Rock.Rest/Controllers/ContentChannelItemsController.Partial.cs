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
using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// ContentChannelItems REST API
    /// </summary>
    public partial class ContentChannelItemsController
    {
        #region ContentByDataViewGuids
        /// <summary>
        /// Returns a list of content based on list of dataview guids
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        [HttpGet]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ContentChannelItems/GetFromPersonDataView" )]
        public List<ContentChannelItem> GetFromPersonDataView( string guids )
        {
            var rockContext = new RockContext();

            // Turn the comma separated list of guids into an array of strings
            string[] parsedGuids = guids.Split( ',' );

            // Get any attribute values with a value that matches a passed in guid
            var attributeValuesPerGuid = new AttributeValueService( rockContext ).Queryable().Where( a => parsedGuids.Contains( a.Value ) );

            // Only get the EntityIds - these are the content channel items
            List<int?> mainEntityIds = attributeValuesPerGuid.Select( a => a.EntityId ).ToList();

            // Get any content channel items with an id that matches an entity id from the mainEntityIds list
            var contentChannelItems = rockContext.ContentChannelItems.Where( a => mainEntityIds.Contains( a.Id ) );

            // Turn that Queryable into a list
            IQueryable<ContentChannelItem> finalList = contentChannelItems.Select( a => a );

            // Return list
            return finalList.ToList();
        }
        #endregion
    }
}