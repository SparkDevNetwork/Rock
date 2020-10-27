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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a location
    /// </summary>
    public class LocationItemPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            this.ItemRestUrlExtraParams = $"/{RootLocationId}";
            this.IconCssClass = "fa fa-home";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="location">The location.</param>
        public void SetValue( Rock.Model.Location location )
        {
            if ( location != null )
            {
                ItemId = location.Id.ToString();
                List<int> parentLocationIds = new List<int>();
                var parentLocation = location.ParentLocation;

                while ( parentLocation != null )
                {
                    if ( parentLocationIds.Contains( parentLocation.Id ) )
                    {
                        // infinite recursion
                        break;
                    }

                    parentLocationIds.Insert( 0, parentLocation.Id ); ;
                    parentLocation = parentLocation.ParentLocation;
                }

                InitialItemParentIds = parentLocationIds.AsDelimited( "," );
                ItemName = location.Name;
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
        /// <param name="locations">The locations.</param>
        public void SetValues( IEnumerable<Location> locations )
        {
            var theLocations = locations.ToList();

            if ( theLocations.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                List<int> parentLocationIds = new List<int>();

                foreach ( var location in theLocations )
                {
                    if ( location != null )
                    {
                        ids.Add( location.Id.ToString() );
                        names.Add( location.Name );
                        var parentLocation = location.ParentLocation;

                        while ( parentLocation != null )
                        {
                            if ( parentLocationIds.Contains( parentLocation.Id ) )
                            {
                                // infinite recursion
                                break;
                            }

                            parentLocationIds.Insert( 0, parentLocation.Id ); ;
                            parentLocation = parentLocation.ParentLocation;
                        }
                    }
                }

                InitialItemParentIds = parentLocationIds.AsDelimited( "," );
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
            var item = new LocationService( new RockContext() ).Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var ids = this.SelectedValuesAsInt().ToList();
            var items = new LocationService( new RockContext() ).Queryable().Where( i => ids.Contains( i.Id ) );
            this.SetValues( items );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get
            {
                if ( IncludeInactive )
                {
                    return "~/api/locations/getchildren/";
                }
                else
                {
                    return "~/api/locations/getactivechildren/";
                }
            }
        }

        /// <summary>
        /// Private reference to IncludeInactive.
        /// </summary>
        private bool? _includeInactive;

        /// <summary>
        /// Determines whether or not to include active locations.
        /// </summary>
        public bool IncludeInactive
        {
            get
            {
                return _includeInactive ?? false;
            }
            set
            {
                _includeInactive = value;
            }
        }

        /// <summary>
        /// Private reference to RootLocationId.
        /// </summary>
        private int _rootLocationId = 0;

        /// <summary>
        /// Gets or sets the root location identifier, which will be passed to the Rest endpoint. Leave the default value of 0 to get all locations.
        /// Note: Setting this property will overwrite any value currently in the ItemPicker.ItemRestUrlExtraParams property.
        /// </summary>
        /// <value>
        /// The root location identifier.
        /// </value>
        public int RootLocationId
        {
            get
            {
                return _rootLocationId;
            }
            set
            {
                _rootLocationId = value;
                this.ItemRestUrlExtraParams = $"/{value}";
            }
        }
    }
}