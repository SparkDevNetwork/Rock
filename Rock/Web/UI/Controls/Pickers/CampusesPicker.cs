// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Select multiple campuses
    /// NOTE: Campuses must be set first (it doesn't automatically load campuses). Hint: Use CampusCache.All()
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
                this.Items.Clear();
                foreach ( CampusCache campus in value )
                {
                    ListItem campusItem = new ListItem();
                    campusItem.Value = campus.Id.ToString();
                    campusItem.Text = campus.Name;
                    this.Items.Add( campusItem );
                }
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
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).AsIntegerList();
            }

            set
            {
                foreach ( ListItem campusItem in this.Items )
                {
                    campusItem.Selected = value.Exists( a => a.Equals( campusItem.Value.AsInteger() ) );
                }
            }
        }
    }
}