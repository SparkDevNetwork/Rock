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
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [RockObsolete( "1.8" )]
    [Obsolete("Use DataViewItemPicker instead")]
    public class DataViewPicker : RockDropDownList
    {

        /// <summary>
        /// Gets or sets a value indicating if items should be recreated whenever a property value is changed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic rebuild items]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoLoadItems
        {
            get
            {
                return ViewState["AutoLoadItems"] as bool? ?? true;
            }

            set
            {
                ViewState["AutoLoadItems"] = value;
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
                if ( AutoLoadItems )
                {
                    LoadDropDownItems();
                }
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

                if ( AutoLoadItems )
                {
                    LoadDropDownItems();
                }
            }
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        public void LoadDropDownItems()
        {
            this.Items.Clear();

            if ( EntityTypeId.HasValue )
            {
                // add Empty option first
                this.Items.Add( new ListItem() );

                // Get 
                var categoryGuids = CategoryGuids ?? new List<Guid>();

                using ( var rockContext = new RockContext() )
                {
                    var allEntityFilters = new DataViewFilterService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( f => f.EntityTypeId == EntityTypeId )
                        .ToList();
                        
                    foreach ( var dataView in new DataViewService( rockContext )
                        .GetByEntityTypeId( EntityTypeId.Value )
                        .Include( "DataViewFilter" )
                        .AsNoTracking() )
                    {
                        var category = dataView.CategoryId.HasValue ? CategoryCache.Get( dataView.CategoryId.Value ) : null;
                        if ( !categoryGuids.Any() || ( category != null && categoryGuids.Contains( category.Guid ) ) )
                        { 
                            var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                            if ( dataView.IsAuthorized( Authorization.VIEW, currentPerson ) &&
                                dataView.DataViewFilter.IsAuthorized( Authorization.VIEW, currentPerson, allEntityFilters ) )
                            {
                                this.Items.Add( new ListItem( dataView.Name, dataView.Id.ToString() ) );
                            }
                        }
                    }
                }
            }
        }
    }
}