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

                string script2 = string.Format( @"
                    <script>
                        $(document).ready(function (e) {{
                            if (localStorage) {{
                                localStorage.checkInKiosk = '{0}';
                            }}
                        }});
                    </script>
                ", CurrentKioskId );
                lbOk.Attributes.Add( "OnClick", script2 );

                ddlKiosk.Items.Clear();
                ddlKiosk.DataSource = new DeviceService().Queryable().ToList();
                ddlKiosk.DataBind();
                ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

                CurrentGroupTypeIds = null;

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
            CurrentGroupTypeIds = null;
            BindGroupTypes();
        }

        protected void lbOk_Click( object sender, EventArgs e )
        {
            if ( ddlKiosk.SelectedValue == None.IdValue )
            {
                maWarning.Show( "A Kiosk Device needs to be selected!", ModalAlertType.Warning );
                return;
            }

            var ministryChosen = false;
            foreach ( RepeaterItem item in rMinistries.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                if ( HasActiveClass( linky ) )
                {
                    ministryChosen = true;
                }
            }

            if (!ministryChosen)
            {
                maWarning.Show( "At least one ministry must be selected!", ModalAlertType.Warning );
                return;
            }

            var roomChosen = false;
            foreach ( RepeaterItem item in rRooms.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                if ( HasActiveClass( linky ) )
                {
                    roomChosen = true;
                }
            }

            if (!roomChosen)
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

            var roomGroupTypeIds = new List<int>();
            foreach ( RepeaterItem item in rRooms.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                if ( HasActiveClass( linky ) )
                {
                    roomGroupTypeIds.Add( int.Parse( linky.CommandArgument ) );
                }
            }

            CurrentKioskId = int.Parse( ddlKiosk.SelectedValue );
            CurrentGroupTypeIds = groupTypeIds;
            CurrentRoomGroupTypeIds = roomGroupTypeIds;
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

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                var kiosk = new DeviceService().Get( int.Parse( ddlKiosk.SelectedValue ) );
                if ( kiosk != null )
                {
                    rMinistries.DataSource = kiosk.GetLocationGroupTypes();
                    rMinistries.DataBind();
                    rRooms.DataSource = null;
                    rRooms.DataBind();
                }

                if ( selectedValues != string.Empty )
                {
                    foreach ( string id in selectedValues.Split(',') )
                    {
                        foreach ( RepeaterItem item in rMinistries.Items )
                        {
                            var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                            if ( linky.CommandArgument == id )
                            {
                                linky.AddCssClass( "active" );
                            }
                        }
                    }
                }
                else
                {
                    if ( CurrentGroupTypeIds != null )
                    {
                        foreach ( int id in CurrentGroupTypeIds )
                        {
                            foreach ( RepeaterItem item in rMinistries.Items )
                            {
                                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                                if ( int.Parse(linky.CommandArgument) == id )
                                {
                                    linky.AddCssClass( "active" );
                                }
                            }
                        }
                    }
                }
            }
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
            List<GroupType> roomList = new List<GroupType>();
            foreach ( RepeaterItem item in rMinistries.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                if ( HasActiveClass( linky ) )
                {
                    GetRooms( int.Parse(linky.CommandArgument), roomList );
                }
            }
            
            rRooms.DataSource = roomList;
            rRooms.DataBind();
        }

        protected List<GroupType> GetRooms( int parentGroupTypeId, List<GroupType> returnGroupType )
        {
            GroupType parentGroupType = new GroupTypeService().Get( parentGroupTypeId );
            List<int> childGroupTypes = parentGroupType.ChildGroupTypes.Select( a => a.Id ).ToList();
            foreach ( var childGroupType in childGroupTypes )
            {
                GroupType theParentGroupType = new GroupTypeService().Get( childGroupType );
                if ( theParentGroupType.ChildGroupTypes.Count > 0 )
                {
                    GetRooms( theParentGroupType.Id, returnGroupType );
                }
                else
                {

                    GroupType groupType = new GroupTypeService().Get( theParentGroupType.Id );
                    returnGroupType.Add( groupType );
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

            // if the button isn't already selected, then show the button as selected. otherwise unselect it.
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