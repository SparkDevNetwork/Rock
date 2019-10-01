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

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Select multiple campuses
    /// </summary>
    public class CampusesPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusPicker" /> class.
        /// </summary>
        public CampusesPicker()
            : base()
        {
            Label = "Campuses";
            this.RepeatDirection = RepeatDirection.Horizontal;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                LoadItems( null );
            }
        }

        /// <summary>
        /// Gets or sets the campus ids.
        /// </summary>
        /// <value>
        /// The campus ids.
        /// </value>
        private List<int> CampusIds
        {
            get
            {
                return ViewState["CampusIds"] as List<int>;
            }

            set
            {
                ViewState["CampusIds"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [include inactive].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return ViewState["IncludeInactive"] as bool? ?? true;
            }

            set
            {
                ViewState["IncludeInactive"] = value;
                LoadItems( null );
            }
        }

        /// <summary>
        /// Gets or sets the campuses.
        /// </summary>
        /// <value>
        /// The campuses.
        /// </value>
        public List<CampusCache> Campuses
        {
            set
            {
                CampusIds = value?.Select( c => c.Id ).ToList();
                LoadItems( null );
            }
        }

        /// <summary>
        /// Gets the available campus ids.
        /// </summary>
        /// <value>
        /// The available campus ids.
        /// </value>
        public List<int> AvailableCampusIds
        {
            get
            {
                return this.Items.OfType<ListItem>().Select( a => a.Value ).AsIntegerList();
            }
        }

        /// <summary>
        /// Gets the selected campus ids.
        /// </summary>
        /// <value>
        /// The selected campus ids.
        /// </value>
        public List<int> SelectedCampusIds
        {
            get
            {
                return this.Items.OfType<ListItem>()
                    .Where( l => l.Selected )
                    .Select( a => a.Value ).AsIntegerList();
            }

            set
            {
                CheckItems( value );

                foreach ( ListItem campusItem in this.Items )
                {
                    campusItem.Selected = value.Exists( a => a.Equals( campusItem.Value.AsInteger() ) );
                }
            }
        }

        /// <summary>
        /// Checks the items.
        /// </summary>
        /// <param name="values">The values.</param>
        public void CheckItems( List<int> values )
        {
            if ( values.Any() )
            {
                foreach ( int value in values )
                {
                    if ( this.Items.FindByValue( value.ToString() ) == null &&
                    CampusCache.Get( value ) != null )
                    {
                        LoadItems( values );
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void LoadItems( List<int> selectedValues )
        {
            var selectedItems = Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            Items.Clear();

            var allCampuses = CampusCache.All();

            var campusIds = this.CampusIds ?? allCampuses.Select( a => a.Id ).ToList();

            var campuses = allCampuses
                .Where( c =>
                    ( campusIds.Contains( c.Id ) && ( !c.IsActive.HasValue || c.IsActive.Value || IncludeInactive ) ) ||
                    ( selectedValues != null && selectedValues.Contains( c.Id ) ) )
                .OrderBy( c => c.Name )
                .ToList();

            foreach ( CampusCache campus in campuses )
            {
                var li = new ListItem( campus.Name, campus.Id.ToString() );
                li.Selected = selectedItems.Contains( campus.Id );
                Items.Add( li );
            }
        }
    }
}