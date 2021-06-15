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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Lists attribute values and demonstrates server-side paging and filtering.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Large Dataset Grid" )]
    [Category( "Obsidian > Example" )]
    [Description( "Lists attribute values and demonstrates server-side paging and filtering." )]
    [IconCssClass( "fa fa-dumbbell" )]

    public class LargeDatasetGrid : ObsidianBlockType
    {
        #region Actions

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="filterOptions">The filter options.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttributeValues( FilterOptions filterOptions, SortProperty sortProperty )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var query = attributeValueService.Queryable()
                    .AsNoTracking();

                // Filter
                if ( filterOptions.AttributeId.HasValue )
                {
                    query = query.Where( av => av.AttributeId == filterOptions.AttributeId );
                }

                if ( !filterOptions.ValueStartsWith.IsNullOrWhiteSpace() )
                {
                    query = query.Where( av => av.Value.StartsWith( filterOptions.ValueStartsWith ) );
                }

                // Sort
                var hasSort = sortProperty?.Property.IsNullOrWhiteSpace() == false;

                if ( hasSort && sortProperty.DirectionString == "ASC" )
                {
                    query = query.Sort( sortProperty ).ThenBy( av => av.Id );
                }
                else if ( hasSort )
                {
                    query = query.Sort( sortProperty ).ThenByDescending( av => av.Id );
                }
                else
                {
                    query = query.OrderBy( av => av.Id );
                }

                // Paginate
                var totalCount = query.Count();
                query = query.Skip( filterOptions.Skip ).Take( filterOptions.Take );

                // Select necessary data to enumerate
                var selectQuery = query.Select( av => new
                {
                    Id = av.Id,
                    Guid = av.Guid,
                    Attribute = av.AttributeId + " " + av.Attribute.Name,
                    Value = av.Value
                } );

                // Enumerate and use non-linq-capable functions like FullName
                return new BlockActionResult( HttpStatusCode.OK, new BlockActionGridResponse
                {
                    CurrentPageData = selectQuery.ToList(),
                    TotalCount = totalCount
                } );
            }
        }

        #endregion Actions

        #region View Models

        /// <summary>
        /// Filter Options
        /// </summary>
        public class FilterOptions
        {
            /// <summary>
            /// Gets or sets the top.
            /// </summary>
            /// <value>
            /// The top.
            /// </value>
            public int Take { get; set; }

            /// <summary>
            /// Gets or sets the offset.
            /// </summary>
            /// <value>
            /// The offset.
            /// </value>
            public int Skip { get; set; }

            /// <summary>
            /// Gets or sets the attribute identifier.
            /// </summary>
            /// <value>
            /// The attribute identifier.
            /// </value>
            public int? AttributeId { get; set; }

            /// <summary>
            /// Gets or sets the value starts with.
            /// </summary>
            /// <value>
            /// The value starts with.
            /// </value>
            public string ValueStartsWith { get; set; }
        }

        #endregion View Models
    }
}
