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
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// ContentChannelItems REST API
    /// </summary>
    public partial class ContentChannelItemsController
    {
        #region ContentByDataViewGuids
        /// <summary>
        /// Returns a list of content channel items based on a list of dataview guids
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ContentChannelItems/GetFromPersonDataView" )]
        public IQueryable<ContentChannelItem> GetFromPersonDataView( string guids )
        {
            RockContext rockContext = new RockContext();

            // Turn the comma separated list of guids into a list of strings.
            List<string> guidList = ( guids ?? "" ).Split( ',' ).ToList();

            // Get the Id of the Rock.Model.ContentChannelItem Entity.
            int contentChannelItemEntityTypeId = EntityTypeCache.Get( "Rock.Model.ContentChannelItem" ).Id;

            // Get the Field Type (Attribute Type) Id of the Data View Field Type.
            int fieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.DATAVIEWS.AsGuid() ).Id;

            // Get the list of attributes that are of the Rock.Model.ContentChannelItem entity type
            // and that are of the Data View field type.
            List<int> attributeIdList = new AttributeService( rockContext )
                .GetByEntityTypeId( contentChannelItemEntityTypeId )
                .Where( item => item.FieldTypeId == fieldTypeId )
                .Select( a => a.Id )
                .ToList();

            // I want a list of content channel items whose ids match up to attribute values that represent entity ids
            IQueryable<ContentChannelItem> contentChannelItemList = new ContentChannelItemService( rockContext )
                .Queryable()
                .WhereAttributeValue( rockContext, av => attributeIdList.Contains( av.AttributeId ) && guidList.Contains( av.Value ) );

            // Return this list
            return contentChannelItemList;
        }
        #endregion
    }
}