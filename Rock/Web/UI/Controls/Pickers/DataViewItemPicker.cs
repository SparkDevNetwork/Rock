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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select data view items for a particular pre-configured data view
    /// </summary>
    public class DataViewItemPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            SetExtraRestParams();

            this.IconCssClass = "fa fa-filter";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        private void SetExtraRestParams()
        {
            ItemRestUrlExtraParams = $"?getCategorizedItems=true&showCategoriesThatHaveNoChildren=false";

            // set the entityType of the category
            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Get<Rock.Model.DataView>().Id;

            if ( this.CategoryGuids?.Any() == true )
            {
                ItemRestUrlExtraParams += "&includedCategoryIds=" + this.CategoryGuids.Select( a => CategoryCache.Get( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList().AsDelimited( "," );
            }

            // set the itemFilter to only get DataViews with the specified EntityTypeId
            if ( this.EntityTypeId > 0 )
            {
                ItemRestUrlExtraParams += $"&itemFilterPropertyName=EntityTypeId&itemFilterPropertyValue={this.EntityTypeId}";
            }
            if ( this.DisplayPersistedOnly )
            {
                ItemRestUrlExtraParams += $"&displayPersistedOnly=true";
            }
        }

        /// <summary>
        /// Gets or sets the data view entity type identifier.
        /// </summary>
        /// <value>
        /// The data view entity type identifier.
        /// </value>
        public int? EntityTypeId
        {
            get
            {
                return ViewState["EntityTypeId"] as int?;
            }

            set
            {
                ViewState["EntityTypeId"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets or sets an optional list of category guids to limit dataviews by
        /// </summary>
        /// <value>
        /// The category guids.
        /// </value>
        public List<Guid> CategoryGuids
        {
            get
            {
                return ViewState["CategoryGuids"] as List<Guid>;
            }

            set
            {
                ViewState["CategoryGuids"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display persisted only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display persisted only]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayPersistedOnly
        {
            get
            {
                return ViewState["DisplayPersistedOnly"] as bool? ?? false;
            }

            set
            {
                ViewState["DisplayPersistedOnly"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl => "~/api/Categories/GetDataView/";

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        public void SetValue( DataView dataView )
        {
            if ( dataView != null )
            {
                ItemId = dataView.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = dataView.CategoryId.HasValue ? CategoryCache.Get( dataView.CategoryId.Value ) : null;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                ExpandedCategoryIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = dataView.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="dataViews">The data views.</param>
        public void SetValues( IEnumerable<DataView> dataViews )
        {
            var dataViewList = dataViews.ToList();

            if ( dataViewList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var dataView in dataViewList )
                {
                    if ( dataView != null )
                    {
                        ids.Add( dataView.Id.ToString() );
                        names.Add( dataView.Name );
                        var parentCategory = dataView.Category;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                ExpandedCategoryIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var dataViewId = ItemId.AsIntegerOrNull();
            DataView dataView = null;
            if ( dataViewId.HasValue && dataViewId > 0 )
            {
                dataView = new DataViewService( new RockContext() ).Get( ItemId.AsInteger() );
            }

            SetValue( dataView );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var dataViewIds = ItemIds.AsIntegerList().Where( a => a > 0 ).ToList();

            var dataViews = new DataViewService( new RockContext() ).Queryable().Where( g => dataViewIds.Contains( g.Id ) );
            this.SetValues( dataViews );
        }
    }
}