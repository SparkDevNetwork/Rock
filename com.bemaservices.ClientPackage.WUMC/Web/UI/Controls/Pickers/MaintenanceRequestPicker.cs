using System.Collections.Generic;
using System.Web.UI.WebControls;

using com.bemaservices.ClientPackage.WUMC.Model;

using Rock;
using Rock.Web.UI.Controls;

namespace com.bemaservices.ClientPackage.WUMC.Web.UI.Controls.Pickers
{
    public class MaintenanceRequestPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceRequestPicker" /> class.
        /// </summary>
        public MaintenanceRequestPicker() : base()
        {
            Label = "Maintenance Request";
        }

        /// <summary>
        /// Gets or sets the Pto Types.
        /// </summary>
        /// <value>
        /// The Pto Types.
        /// </value>
        public List<MaintenanceRequest> MaintenanceRequests
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( MaintenanceRequest maintenanceRequest in value )
                {
                    this.Items.Add( new ListItem( string.Format( "{0}:{1}", maintenanceRequest.RequestorPersonAlias.Person.FullName, maintenanceRequest.Location != null ? maintenanceRequest.Location.Name : "" ), maintenanceRequest.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected Pto Type identifier.
        /// </summary>
        /// <value>
        /// The selected Pto Type identifier.
        /// </value>
        public int? SelectedMaintenanceRequestId
        {
            get
            {
                return this.SelectedValueAsId();
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