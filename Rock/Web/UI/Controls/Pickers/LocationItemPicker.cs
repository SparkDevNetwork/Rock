//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationItemPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            this.ItemRestUrlExtraParams = "/0";
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

                string parentLocationIds = string.Empty;
                var parentLocation = location.ParentLocation;
                while ( parentLocation != null )
                {
                    parentLocationIds = parentLocation.Id + "," + parentLocationIds;
                    parentLocation = parentLocation.ParentLocation;
                }

                InitialItemParentIds = parentLocationIds.TrimEnd( new char[] { ',' } );
                ItemName = location.ToString();
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
                var parentLocationIds = string.Empty;

                foreach ( var location in theLocations )
                {
                    if ( location != null )
                    {
                        ids.Add( location.Id.ToString() );
                        names.Add( location.Name );
                        var parentLocation = location.ParentLocation;

                        while ( parentLocation != null )
                        {
                            parentLocationIds += parentLocation.Id.ToString() + ",";
                            parentLocation = parentLocation.ParentLocation;
                        }
                    }
                }

                InitialItemParentIds = parentLocationIds.TrimEnd( new[] { ',' } );
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
            var item = new LocationService().Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var ids = this.SelectedValuesAsInt().ToList();
            var items = new LocationService().Queryable().Where( i => ids.Contains( i.Id ) );
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
            get { return "~/api/locations/getchildren/"; }
        }

        
    }
}