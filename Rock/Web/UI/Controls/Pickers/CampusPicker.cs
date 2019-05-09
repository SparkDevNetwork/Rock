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

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class CampusPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusPicker" /> class.
        /// </summary>
        public CampusPicker()
            : base()
        {
            Label = "Campus";
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
                return ViewState["CampusIds"] as List<int> ?? CampusCache.All().Select( c => c.Id ).ToList();

            }

            set
            {
                ViewState["CampusIds"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [include inactive] (defaults to True).
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
                CampusIds = value != null ? value.Select( c => c.Id ).ToList() : new List<int>();
                LoadItems( null );
            }
        }

        /// <summary>
        /// Gets or sets the selected campus identifier.
        /// </summary>
        /// <value>
        /// The selected campus identifier.
        /// </value>
        public int? SelectedCampusId
        {
            get
            {
                return this.SelectedValueAsInt();
            }

            set
            {
                CheckItem( value );

                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
                else
                {
                    // if setting CampusId to NULL or 0, just default to the first item in the list (which should be nothing)
                    if ( this.Items.Count > 0 )
                    {
                        this.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// When setting a value, check to see if the value is in the list. If it's not, and it's a valid campus,
        /// Rebuild the list of items to include it.
        /// </summary>
        /// <param name="value">The value.</param>
        public void CheckItem( int? value )
        {
            if ( value.HasValue && value.Value > 0 &&
                this.Items.FindByValue( value.Value.ToString() ) == null &&
                CampusCache.Get( value.Value ) != null )
            {
                LoadItems( value );
            }
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        /// <param name="selectedValue">The selected value.</param>
        private void LoadItems( int? selectedValue )
        {
            string firstItemText = Items.Count > 0 && Items[0].Value == string.Empty ? Items[0].Text : string.Empty;

            var selectedItems = Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            Items.Clear();

            // add Empty option first
            Items.Add( new ListItem( firstItemText, string.Empty ) );

            var campuses = CampusCache.All()
                .Where( c =>
                    ( CampusIds.Contains( c.Id ) && ( !c.IsActive.HasValue || c.IsActive.Value || IncludeInactive ) ) ||
                    ( selectedValue.HasValue && c.Id == selectedValue.Value ) )
                .OrderBy( c => c.Order )
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