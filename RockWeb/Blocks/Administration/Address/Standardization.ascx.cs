using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Address;
using Rock.Controls;

namespace RockWeb.Blocks.Administration.Address
{
    public partial class Standardization : Rock.Cms.CmsBlock
	{
        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                rGrid.DataKeyNames = new string[] { "id" };
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

            if ( Page.IsPostBack && hfServiceId.Value != string.Empty )
                ShowEdit( Int32.Parse( hfServiceId.Value ), false );

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            var services = StandardizeContainer.Instance.Services.ToList();
            KeyValuePair<int, StandardizeService> movedItem = services[e.OldIndex];
            services.RemoveAt( e.OldIndex );
            if ( e.NewIndex >= services.Count )
                services.Add( movedItem );
            else
                services.Insert( e.NewIndex, movedItem );

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                int order = 0;
                foreach ( KeyValuePair<int, StandardizeService> service in services )
                {
                    foreach ( Rock.Cms.Cached.Attribute attribute in service.Value.Attributes )
                        if ( attribute.Key == "Order" )
                            Rock.Attribute.Helper.SaveAttributeValue( service.Value, attribute, order.ToString(), CurrentPersonId );
                    order++;
                }
            }

            StandardizeContainer.Instance.Refresh();

            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"], true );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            hfServiceId.Value = string.Empty;
            pnlDetails.Visible = false;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            StandardizeService service = StandardizeContainer.Instance.Services[Int32.Parse( hfServiceId.Value )];

            Rock.Attribute.Helper.GetEditValues( olProperties, service );

            foreach ( Rock.Cms.Cached.Attribute attribute in service.Attributes )
                Rock.Attribute.Helper.SaveAttributeValue( service, attribute, service.AttributeValues[attribute.Key].Value, CurrentPersonId );

            hfServiceId.Value = string.Empty;

            BindGrid();

            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            var dataSource = new List<ServiceDescription>();
            foreach ( KeyValuePair<int, StandardizeService> service in StandardizeContainer.Instance.Services )
            {
                Type type = service.Value.GetType();
                Rock.Attribute.Helper.CreateAttributes( type, type.FullName, string.Empty, string.Empty, null );
                dataSource.Add( new ServiceDescription( service.Key, service.Value ) );
            }

            rGrid.DataSource = dataSource;
            rGrid.DataBind();
        }

        protected void ShowEdit( int serviceId, bool setValues )
        {
            StandardizeService service = StandardizeContainer.Instance.Services[serviceId];
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