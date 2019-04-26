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
    public class ReportPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            SetExtraRestParams();

            this.IconCssClass = "fa fa-list-alt";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        private void SetExtraRestParams()
        {
            ItemRestUrlExtraParams = $"?getCategorizedItems=true&showCategoriesThatHaveNoChildren=false";

            // set the entityType of the category
            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Get<Rock.Model.Report>().Id;

            if ( this.CategoryGuids?.Any() == true )
            {
                ItemRestUrlExtraParams += "&includedCategoryIds=" + this.CategoryGuids.Select( a => CategoryCache.Get( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList().AsDelimited( "," );
            }

            // set the itemFilter to only get DataViews with the specified EntityTypeId
            if ( this.EntityTypeId > 0 )
            {
                ItemRestUrlExtraParams += $"&itemFilterPropertyName=EntityTypeId&itemFilterPropertyValue={this.EntityTypeId}";
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
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl => "~/api/Categories/GetChildren/";

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="report">The report.</param>
        public void SetValue( Report report )
        {
            if ( report != null )
            {
                ItemId = report.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = report.CategoryId.HasValue ? CategoryCache.Get( report.CategoryId.Value ) : null;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = report.Name;
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
        /// <param name="reports">The reports.</param>
        public void SetValues( IEnumerable<Report> reports )
        {
            var reportList = reports.ToList();

            if ( reportList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var report in reportList )
                {
                    if ( report != null )
                    {
                        ids.Add( report.Id.ToString() );
                        names.Add( report.Name );
                        var parentCategory = report.Category;

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
            var report = new ReportService( new RockContext() ).Get( ItemId.AsInteger() );
            SetValue( report );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var reports = new ReportService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( reports );
        }
    }
}