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

using Rock.CheckIn;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Administration block" )]
    public partial class Admin : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ddlKiosk.Items.Clear();
                ddlKiosk.DataSource = new DeviceService().Queryable().ToList();
                ddlKiosk.DataBind();
                ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

                if ( CurrentKioskId.HasValue )
                {
                    ListItem item = ddlKiosk.Items.FindByValue( CurrentKioskId.Value.ToString() );
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

            CurrentKioskId = Int32.Parse( ddlKiosk.SelectedValue );
            CurrentGroupTypeIds = groupTypeIds;
            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();

            GoToWelcomePage();
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

                if ( CurrentGroupTypeIds != null )
                {
                    foreach ( int id in CurrentGroupTypeIds )
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