//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Constants;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupTypePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypePicker" /> class.
        /// </summary>
        public GroupTypePicker()
            : base()
        {
            Label = "Group Type";
        }

        /// <summary>
        /// Gets or sets the group types.
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        public List<GroupType> GroupTypes
        {
            set
            {
                this.Items.Clear();

                if ( !Required )
                {
                    this.Items.Add( new ListItem( string.Empty, string.Empty ) );
                }

                foreach ( GroupType groupType in value )
                {
                    ListItem groupTypeItem = new ListItem();
                    groupTypeItem.Value = groupType.Id.ToString();
                    groupTypeItem.Text = groupType.Name;
                    this.Items.Add( groupTypeItem );
                }

            }
        }

        /// <summary>
        /// Gets the selected groupType ids.
        /// </summary>
        /// <value>
        /// The selected groupType ids.
        /// </value>
        public int? SelectedGroupTypeId
        {
            get
            {
                return this.SelectedValueAsInt();
            }
            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }
        }

    }
}