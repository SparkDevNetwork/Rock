using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Api.Crm.Address;
using Rock.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Geocoding : Rock.Cms.CmsBlock
	{
        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                rGrid.GridRebind += new Rock.Controls.GridRebindEventHandler( rGrid_GridRebind );
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

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            var services = GeocodeContainer.Instance.Services;
            IGeocodeService movedItem = services[e.OldIndex];
            services.RemoveAt( e.OldIndex );
            if ( e.NewIndex >= services.Count )
                services.Add( movedItem );
            else
                services.Insert( e.NewIndex, movedItem );

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                int order = 0;
                foreach ( IGeocodeService service in services )
                {
                    foreach ( Rock.Models.Core.Attribute attribute in service.Attributes )
                        if ( attribute.Key == "Order" )
                            Rock.Attribute.Helper.SaveAttributeValue( service, attribute, order.ToString(), CurrentPersonId );
                    order++;
                }
            }

            GeocodeContainer.Instance.Refresh();

            BindGrid();
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            var dataSource = new List<ServiceDesc>();
            foreach ( IGeocodeService service in GeocodeContainer.Instance.Services )
                dataSource.Add( new ServiceDesc(service) );

            rGrid.DataSource = dataSource;
            rGrid.DataBind();
        }

        #endregion
    }

    class ServiceDesc
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ServiceDesc( IGeocodeService service )
        {
            Type type = service.GetType();

            Name = type.FullName;
            Description = "Description Coming";
        }
    }

}