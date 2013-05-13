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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
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

            var IsMinistryChosen = false;
            foreach ( RepeaterItem item in rMinistries.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                if ( HasActiveClass( linky ) )
                {
                    IsMinistryChosen = true;
                }
            }
            if (!IsMinistryChosen)
            {
                maWarning.Show( "At least one ministry must be selected!", ModalAlertType.Warning );
                return;
            }

            var IsRoomChosen = false;
            foreach ( RepeaterItem item in rRooms.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                if ( HasActiveClass( linky ) )
                {
                    IsRoomChosen = true;
                }
            }
            if (!IsRoomChosen)
            {
                maWarning.Show( "At least one room must be selected!", ModalAlertType.Warning );
                return;
            }

            var groupTypeIds = new List<int>();
            foreach ( RepeaterItem item in rMinistries.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                if ( HasActiveClass( linky ) )
                {
                    groupTypeIds.Add( int.Parse( linky.CommandArgument ) );
                }
            }

            CurrentKioskId = Int32.Parse( ddlKiosk.SelectedValue );
            CurrentGroupTypeIds = groupTypeIds;
            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();

            NavigateToNextPage();
        }

        private void BindGroupTypes()
        {
            BindGroupTypes( string.Empty );
        }

        private void BindGroupTypes( string selectedValues )
        {
            var selectedItems = selectedValues.Split( ',' );

            //cblGroupTypes.Items.Clear();

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                var kiosk = new DeviceService().Get( Int32.Parse( ddlKiosk.SelectedValue ) );
                if ( kiosk != null )
                {
                    //cblGroupTypes.DataSource = kiosk.GetLocationGroupTypes();
                    //cblGroupTypes.DataBind();
                    rMinistries.DataSource = kiosk.GetLocationGroupTypes();
                    rMinistries.DataBind();
                }

                if ( selectedValues != string.Empty )
                {
                    foreach ( string id in selectedValues.Split(',') )
                    {
                        
                        //RepeaterItem rItem = rMinistryTypes.Items[int.Parse(id)];
                        //if ( rItem != null )
                        //{
                        //    CheckBox check = (CheckBox)rItem.FindControl( "lcbMinistry" );
                        //    check.Checked = true;
                        //}
                        
                        //ListItem item = cblGroupTypes.Items.FindByValue( id );
                        //if ( item != null )
                        //{
                        //    item.Selected = true;
                        //}
                    }
                }
                else
                {
                    if ( CurrentGroupTypeIds != null )
                    {
                        foreach ( int id in CurrentGroupTypeIds )
                        {
                            //RepeaterItem rItem = rMinistryTypes.Items[id];
                            //if ( rItem != null )
                            //{
                            //    CheckBox check = (CheckBox)rItem.FindControl( "lcbMinistry" );
                            //    check.Checked = true;
                            //}
                            //ListItem item = cblGroupTypes.Items.FindByValue( id.ToString() );
                            //if ( item != null )
                            //{
                            //    item.Selected = true;
                            //}
                        }
                    }
                }

            }

            //Load all the possible rooms up front
            //GroupTypeService groupTypeService = new GroupTypeService();
            //var groupTypeQry = groupTypeService.Queryable();
            //groupTypeQry = groupTypeQry.Where( a => a.TakesAttendance == true);
            //List<GroupType> groupTypes = groupTypeQry.ToList();
            //rRooms.DataSource = groupTypes;
            //rRooms.DataBind();
        }

        protected void rMinistries_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );

            // step 1: if the button isn't already selected, then show the button as selected. otherwise unselect it.
            if ( HasActiveClass((LinkButton)e.Item.FindControl("lbSelectMinistries")) )
            {
                // the button is already selected, so unselect it.
                ( (LinkButton)e.Item.FindControl( "lbSelectMinistries" ) ).RemoveCssClass( "active" );
            }
            else
            {
                // the button isn't already selected. Select it.
                ( (LinkButton)e.Item.FindControl( "lbSelectMinistries" ) ).AddCssClass( "active" );
            }

            // step 2: go through the buttons and load the appropriate rooms for the selected ministries.
            List<GroupType> RoomList = new List<GroupType>();
            List<GroupType> TotalRoomList = new List<GroupType>();
            foreach ( RepeaterItem item in rMinistries.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                if ( HasActiveClass( linky ) )
                {
                    GetRooms( int.Parse(linky.CommandArgument), RoomList );
                }
            }
            rRooms.DataSource = RoomList;
            rRooms.DataBind();


        }

        protected List<GroupType> GetRooms( int parentGroupTypeId, List<GroupType> returnGroupType )
        {
            GroupType pGroupType = new GroupTypeService().Get( parentGroupTypeId );
            List<int> cGroupTypes = pGroupType.ChildGroupTypes.Select( a => a.Id ).ToList();
            foreach ( var cGT in cGroupTypes )
            {
                GroupType pG = new GroupTypeService().Get( cGT );
                if ( pG.ChildGroupTypes.Count > 0 )
                {
                    GetRooms( pG.Id, returnGroupType );
                }
                else
                {
                    GroupType gt = new GroupTypeService().Get( pG.Id );
                    returnGroupType.Add( gt );
                }
            }
            return returnGroupType;
        }

        protected bool HasActiveClass( WebControl webcontrol )
        {
            string match = @"\s*\b" + "active" + @"\b";
            string css = webcontrol.CssClass;
            if ( System.Text.RegularExpressions.Regex.IsMatch( css, match, System.Text.RegularExpressions.RegexOptions.IgnoreCase ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void rRooms_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );

            // step 1: if the button isn't already selected, then show the button as selected. otherwise unselect it.
            if ( HasActiveClass( (LinkButton)e.Item.FindControl( "lbSelectRooms" ) ) )
            {
                // the button is already selected, so unselect it.
                ( (LinkButton)e.Item.FindControl( "lbSelectRooms" ) ).RemoveCssClass( "active" );
            }
            else
            {
                // the button isn't already selected. Select it.
                ( (LinkButton)e.Item.FindControl( "lbSelectRooms" ) ).AddCssClass( "active" );
            }
        }
    }
}