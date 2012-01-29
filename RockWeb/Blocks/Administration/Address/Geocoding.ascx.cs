//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock.Address;
using Rock.Extension;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration.Address
{
    /// <summary>
    /// Used to manage the <see cref="Rock.Extension.GeocodeComponent"/> classes found through MEF.  Provides a way to edit the value
    /// of the attributes specified in each class
    /// </summary>
    public partial class Geocoding : Rock.Web.UI.Block
	{
        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
                rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                }
            }
            else
            {
                rGrid.Visible = false;
                nbMessage.Text = "You are not authorized to edit the geocoding services";
                nbMessage.Visible = true;
            }

            if ( Page.IsPostBack && hfServiceId.Value != string.Empty )
                ShowEdit( Int32.Parse( hfServiceId.Value ), false );

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Controls.GridReorderEventArgs"/> instance containing the event data.</param>
        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var services = GeocodeContainer.Instance.Services.ToList();
            var movedItem = services[e.OldIndex];
            services.RemoveAt( e.OldIndex );
            if ( e.NewIndex >= services.Count )
                services.Add( movedItem );
            else
                services.Insert( e.NewIndex, movedItem );

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                int order = 0;
                foreach ( KeyValuePair<int, Lazy<GeocodeComponent, Rock.Extension.IComponentData>> service in services )
                {
                    foreach ( var category in service.Value.Value.Attributes )
                        foreach ( var attribute in category.Value )
                            if ( attribute.Key == "Order" )
                                Rock.Attribute.Helper.SaveAttributeValue( service.Value.Value, attribute, order.ToString(), CurrentPersonId );
                    order++;
                }
            }

            GeocodeContainer.Instance.Refresh();

            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"], true );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            hfServiceId.Value = string.Empty;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            GeocodeComponent service = GeocodeContainer.Instance.Services[Int32.Parse( hfServiceId.Value )].Value;

            Rock.Attribute.Helper.GetEditValues( olProperties, service );

            foreach ( var category in service.Attributes )
                foreach ( var attribute in category.Value )
                    Rock.Attribute.Helper.SaveAttributeValue( service, attribute, service.AttributeValues[attribute.Key].Value, CurrentPersonId );

            hfServiceId.Value = string.Empty;

            BindGrid();

            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var dataSource = new List<ComponentDescription>();
            foreach ( KeyValuePair<int, Lazy<GeocodeComponent, Rock.Extension.IComponentData>> service in GeocodeContainer.Instance.Services )
            {
                Type type = service.Value.Value.GetType();
                if ( Rock.Attribute.Helper.UpdateAttributes( type, type.FullName, string.Empty, string.Empty, null ) )
                    Rock.Attribute.Helper.LoadAttributes( service.Value.Value );
                dataSource.Add( new ComponentDescription( service.Key, service.Value.Value ) );
            }

            rGrid.DataSource = dataSource;
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit panel
        /// </summary>
        /// <param name="serviceId">The service id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEdit( int serviceId, bool setValues )
        {
            GeocodeComponent service = GeocodeContainer.Instance.Services[serviceId].Value;
            hfServiceId.Value = serviceId.ToString();

            olProperties.Controls.Clear();
            foreach ( HtmlGenericControl li in Rock.Attribute.Helper.GetEditControls( service, setValues ) )
                if (li.Attributes["attribute-key"] != "Order")
                    olProperties.Controls.Add( li );

            pnlDetails.Visible = true;
        }


        #endregion
    }
}