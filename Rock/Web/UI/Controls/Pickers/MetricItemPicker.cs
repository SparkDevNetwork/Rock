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
    /// Selects one or more metrics.
    /// </summary>
    public class MetricItemPicker : ItemPicker
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
            ItemRestUrlExtraParams = string.Empty;

            if ( this.CategoryGuids?.Any() == true )
            {
                string includedCategoryIds = this.CategoryGuids
                    .Select( a => CategoryCache.Get( a ) )
                    .Where( a => a != null )
                    .Select( a => a.Id )
                    .ToList().AsDelimited( "," );

                AddRestQueryValue( "includedCategoryIds", includedCategoryIds );
            }
        }

        /// <summary>
        /// Adds a name/value pair to ItemRestUrlExtraParams with the appropriate delimiter.
        /// </summary>
        /// <param name="name">The name of the query string variable.</param>
        /// <param name="value">The value of the variable.</param>
        private void AddRestQueryValue( string name, string value )
        {
            if ( string.IsNullOrWhiteSpace( this.ItemRestUrlExtraParams ) )
            {
                ItemRestUrlExtraParams = "?" + name + "=" + value;
            }
            else
            {
                ItemRestUrlExtraParams += "&" + name + "=" + value;
            }
        }

        /// <summary>
        /// Gets or sets an optional list of category guids to limit metrics by
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
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl => "~/api/Categories/GetMetricChildren/";

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="metric">The metric.</param>
        public void SetValue( Metric metric )
        {
            if ( metric != null )
            {
                ItemId = metric.Id.ToString();

                string parentCategoryIds = string.Empty;
                foreach ( var metricCategory in metric.MetricCategories )
                {
                    var parentCategory = metricCategory.Category;
                    while ( parentCategory != null )
                    {
                        parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                        parentCategory = parentCategory.ParentCategory;
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = metric.Title;
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
        /// <param name="metrics">The metrics.</param>
        public void SetValues( IEnumerable<Metric> metrics )
        {
            var metricList = metrics.ToList();

            if ( metricList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var metric in metricList )
                {
                    if ( metric != null )
                    {
                        ids.Add( metric.Id.ToString() );
                        names.Add( metric.Title );

                        foreach ( var metricCategory in metric.MetricCategories )
                        {

                            var parentCategory = metricCategory.Category;

                            while ( parentCategory != null )
                            {
                                parentCategoryIds += parentCategory.Id.ToString() + ",";
                                parentCategory = parentCategory.ParentCategory;
                            }
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
            var metric = new MetricService( new RockContext() ).Get( ItemId.AsInteger() );
            SetValue( metric );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var metrics = new MetricService( new RockContext() ).Queryable().Where( m => ItemIds.Contains( m.Id.ToString() ) );
            this.SetValues( metrics );
        }
    }
}