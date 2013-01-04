//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Administration screen" )]
    [TextField( 0, "Welcome Page Url", "", "The url of the Check-In welcome page", false, "~/checkin/welcome" )]
    public partial class Admin : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ddlKiosk.Items.Clear();
                ddlKiosk.DataSource = new DeviceService().Queryable().ToList();
                ddlKiosk.DataBind();
                ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

                if ( Session["CheckInKioskId"] != null )
                {
                    ListItem item = ddlKiosk.Items.FindByValue( Session["CheckInKioskId"].ToString() );
                    if ( item != null )
                    {
                        item.Selected = true;
                        BindGroupTypes();
                    }
                }
            }
        }

        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGroupTypes();
        }

        protected void lbOk_Click( object sender, EventArgs e )
        {
            if ( ddlKiosk.SelectedValue == None.IdValue )
            {
                maWarning.Show( "A Kiosk Device needs to be selected!", ModalAlertType.Warning );
                return;
            }

            var groupTypeIds = new List<int>();
            foreach(ListItem item in cblGroupTypes.Items)
            {
                if ( item.Selected )
                {
                    groupTypeIds.Add( Int32.Parse( item.Value ) );
                }
            }

            Session["CheckInKioskId"] = Int32.Parse( ddlKiosk.SelectedValue );
            Session["CheckInGroupTypeIds"] = groupTypeIds;

            Response.Redirect( GetAttributeValue("WelcomePageUrl"), false );
        }

        private void BindGroupTypes()
        {
            cblGroupTypes.Items.Clear();

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                var kiosk = new DeviceService().Get( Int32.Parse( ddlKiosk.SelectedValue ) );
                if ( kiosk != null )
                {
                    cblGroupTypes.DataSource = kiosk.GetLocationGroupTypes();
                    cblGroupTypes.DataBind();
                }

                if ( Session["CheckInGroupTypeIds"] != null )
                {
                    foreach ( int id in Session["CheckInGroupTypeIds"] as List<int> )
                    {
                        ListItem item = cblGroupTypes.Items.FindByValue( id.ToString() );
                        if ( item != null )
                        {
                            item.Selected = true;
                        }
                    }
                }

            }
        }
    }
}