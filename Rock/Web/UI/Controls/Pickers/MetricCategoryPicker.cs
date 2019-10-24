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
    /// 
    /// </summary>
    public class MetricCategoryPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "?getCategorizedItems=true&showUnnamedEntityItems=true&showCategoriesThatHaveNoChildren=false";
            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Get( Rock.SystemGuid.EntityType.METRICCATEGORY.AsGuid() ).Id;
            this.IconCssClass = "fa fa-bar-chart-o";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="metricCategory">The metric category.</param>
        public void SetValue( MetricCategory metricCategory )
        {
            if ( metricCategory != null )
            {
                ItemId = metricCategory.Id.ToString();

                var parentCategoryIds = new List<string>();
                var parentCategory = metricCategory.Category;
                while ( parentCategory != null )
                {
                    if ( !parentCategoryIds.Contains( parentCategory.Id.ToString() ) )
                    {
                        parentCategoryIds.Insert( 0, parentCategory.Id.ToString() );
                    }
                    else
                    {
                        // infinite recursion
                        break;
                    }

                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.AsDelimited( "," );
                ItemName = metricCategory.Name;
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
        /// <param name="metricCategories">The metric categories.</param>
        public void SetValues( IEnumerable<MetricCategory> metricCategories )
        {
            var metricCategoriesList = metricCategories.ToList();

            if ( metricCategoriesList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var metricCategory in metricCategoriesList )
                {
                    if ( metricCategory != null )
                    {
                        ids.Add( metricCategory.Id.ToString() );
                        names.Add( metricCategory.Name );
                        var parentCategory = metricCategory.Category;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
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
            var metricCategory = new MetricCategoryService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( metricCategory );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var metricCategories = new MetricCategoryService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( metricCategories );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/Categories/GetChildren/"; }
        }
    }
}