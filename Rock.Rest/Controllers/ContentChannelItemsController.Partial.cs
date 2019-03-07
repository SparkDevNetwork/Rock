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
        /// Returns a list of content based on a dataview guid
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        [HttpGet]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ContentChannelItems/GetFromPersonDataView" )]
        public List<ContentChannelItem> GetFromPersonDataView( string guids )
        {
            RockContext rockContext = new RockContext();

            // Turn the comma separated list of guids into a list of strings.
            List<string> guidList = ( guids ?? "" ).Split( ',' ).ToList();

            // Get the Id of the Rock.Model.ContentChannelItem Entity.
            int contentChannelItemEntityTypeId = EntityTypeCache.Get( "Rock.Model.ContentChannelItem" ).Id;

            // Get the Field Type (Attribute Type) Id of the Data View Field Type.
            int fieldTypeId = new FieldTypeService( rockContext ).GetByName( "Data Views" ).Select( ft => ft.Id ).FirstOrDefault();

            // Get the list of attributes that are of the Rock.Model.ContentChannelItem entity type.
            IQueryable<Model.Attribute> attributeQueryable = new AttributeService( rockContext ).GetByEntityTypeId( contentChannelItemEntityTypeId );

            // Further refine the list of attributes by only returning attributes that are of the Data View field type.
            List<int> attributeIdList = attributeQueryable.Where( item => item.FieldTypeId == fieldTypeId ).Select( a => a.Id ).ToList();

            // Get the list of attribute values that contain an attribute id from the attributeList.
            IQueryable<AttributeValue> attributeValueList = new AttributeValueService( rockContext ).Queryable()
                .Where( av => attributeIdList.Contains( av.AttributeId ) );

            // Get a list of attribute values that contain a value that matches one of the data view guids that were passed in.
            IQueryable<int?> attributeValueEntityIdList = attributeValueList.AsQueryable()
                .Where( av => guidList.Contains( av.Value ) )
                .Select( av => av.EntityId );

            // Get a list of content channel items whose ids match one of the entity ids in the attributeValueEntityIdList.
            List<ContentChannelItem> contentChannelItemList = new ContentChannelItemService( rockContext ).Queryable()
                .Where( cci => attributeValueEntityIdList.Contains( cci.Id ) ).ToList();

            // Return this list
            return contentChannelItemList;
        }
        #endregion
    }
}