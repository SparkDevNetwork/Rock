//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class CampusPicker : LabeledDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusPicker" /> class.
        /// </summary>
        public CampusPicker()
            : base()
        {
            LabelText = "Campus";
        }

        /// <summary>
        /// Gets or sets the campuses.
        /// </summary>
        /// <value>
        /// The campuses.
        /// </value>
        public List<Campus> Campuses
        {
            set
            {
                this.Items.Clear();
                foreach ( Campus campus in value )
                {
                    ListItem campusItem = new ListItem();
                    campusItem.Value = campus.Id.ToString();
                    campusItem.Text = campus.Name;
                    this.Items.Add( campusItem );
                }

            }
        }

        /// <summary>
        /// Gets the selected campus ids.
        /// </summary>
        /// <value>
        /// The selected campus ids.
        /// </value>
        public int? SelectedCampusId
        {
            get
            {
                int campusId = int.MinValue;
                if ( int.TryParse( this.SelectedValue, out campusId ) && campusId > 0 )
                {
                    return campusId;
                }
                return null;
            }
            set
            {
                this.SelectedValue = value.ToString();
            }
        }

    }
}