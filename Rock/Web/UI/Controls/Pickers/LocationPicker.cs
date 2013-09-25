//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            ItemRestUrlExtraParams = "/0";
            this.AllowMultiSelect = false;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="location">The location.</param>
        public void SetValue( Location location )
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

                InitialItemParentIds = parentLocationIds.TrimEnd( new[] { ',' } );
                ItemName = location.Name;
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
            var location = new LocationService().Get( int.Parse( ItemId ) );
            SetValue( location );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValuesOnSelect()
        {
            // this control doesn't support multiselect
            throw new NotImplementedException();
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