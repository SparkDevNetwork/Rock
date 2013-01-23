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
                string script = string.Format( @"
    <script>
        $(document).ready(function (e) {{
            if (localStorage) {{
                if (localStorage.checkInKiosk) {{
                    $('[id$=""hfKiosk""]').val(localStorage.checkInKiosk);
                    if (localStorage.checkInGroupTypes) {{
                        $('[id$=""hfGroupTypes""]').val(localStorage.checkInGroupTypes);
                    }}
                    {0};
                }}
            }}
        }});
    </script>
", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                phScript.Controls.Add( new LiteralControl( script ) );

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
            else
            {
                phScript.Controls.Clear();
            }
        }

        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            ListItem item = ddlKiosk.Items.FindByValue(hfKiosk.Value);
            if ( item != null )
            {
                ddlKiosk.SelectedValue = item.Value;
            }

            BindGroupTypes(hfGroupTypes.Value);
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
            BindGroupTypes( string.Empty );
        }

        private void BindGroupTypes( string selectedValues )
        {
            var selectedItems = selectedValues.Split( ',' );

            cblGroupTypes.Items.Clear();

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                var kiosk = new DeviceService().Get( Int32.Parse( ddlKiosk.SelectedValue ) );
                if ( kiosk != null )
                {
                    cblGroupTypes.DataSource = kiosk.GetLocationGroupTypes();
                    cblGroupTypes.DataBind();
                }

                if ( selectedValues != string.Empty )
                {
                    foreach ( string id in selectedValues.Split(',') )
                    {
                        ListItem item = cblGroupTypes.Items.FindByValue( id );
                        if ( item != null )
                        {
                            item.Selected = true;
                        }
                    }
                }
                else
                {
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
}