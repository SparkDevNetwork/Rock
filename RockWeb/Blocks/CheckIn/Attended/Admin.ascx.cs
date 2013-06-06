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
        #region Control Methods

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
            else
            {
                phScript.Controls.Clear();
            }
        }

        #endregion

        #region Edit Events

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

            var roomGroupTypes = new List<GroupType>();
            if ( !roomChosen )
            {
                foreach ( RepeaterItem item in rRooms.Items )
                {
                    var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                    GroupType gt = new GroupTypeService().Get( int.Parse( linky.CommandArgument ) );
                    roomGroupTypes.Add( gt );
                }
            }
            else
            {
                foreach ( RepeaterItem item in rRooms.Items )
                {
                    var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                    if ( HasActiveClass( linky ) )
                    {
                        GroupType gt = new GroupTypeService().Get( int.Parse( linky.CommandArgument ) );
                        roomGroupTypes.Add( gt );
                    }
                }
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

            CurrentKioskId = int.Parse( ddlKiosk.SelectedValue );
            hfKiosk.Value = CurrentKioskId.ToString();
            CurrentGroupTypeIds = groupTypeIds;
            hfGroupTypes.Value = CurrentGroupTypeIds.AsDelimited( "," );
            CurrentRoomGroupTypes = roomGroupTypes;
            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();

            NavigateToNextPage();
        }

        protected void rMinistries_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );

            // step 1: if the button isn't already selected, then show the button as selected. otherwise unselect it.
            if ( HasActiveClass( (LinkButton)e.Item.FindControl( "lbSelectMinistries" ) ) )
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
                    GetRooms( int.Parse( linky.CommandArgument ), roomList );
                }
            }

            rRooms.DataSource = roomList;
            rRooms.DataBind();
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

        #endregion

        #region Internal Methods

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
                    var roomList = new List<GroupType>();
                    foreach ( string id in selectedValues.Split( ',' ) )
                    {
                        foreach ( RepeaterItem item in rMinistries.Items )
                        {
                            var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                            if ( linky.CommandArgument == id )
                            {
                                linky.AddCssClass( "active" );
                                GetRooms( int.Parse(id), roomList );
                            }
                        }
                    }

                    rRooms.DataSource = roomList;
                    rRooms.DataBind();
                    if ( CurrentRoomGroupTypeIds != null )
                    {
                        foreach ( int id in CurrentRoomGroupTypeIds )
                        {
                            foreach ( RepeaterItem item in rRooms.Items )
                            {
                                var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                                if ( int.Parse(linky.CommandArgument) == id )
                                {
                                    linky.AddCssClass( "active" );
                                }
                            }
                        }
                    }
                }
                else
                {
                    if ( CurrentGroupTypeIds != null )
                    {
                        var roomList = new List<GroupType>();
                        foreach ( int id in CurrentGroupTypeIds )
                        {
                            foreach ( RepeaterItem item in rMinistries.Items )
                            {
                                var linky = (LinkButton)item.FindControl( "lbSelectMinistries" );
                                if ( int.Parse(linky.CommandArgument) == id )
                                {
                                    linky.AddCssClass( "active" );
                                    GetRooms( id, roomList );
                                }
                            }
                        }

                        rRooms.DataSource = roomList;
                        rRooms.DataBind();
                        if ( CurrentRoomGroupTypeIds != null )
                        {
                            foreach ( int id in CurrentRoomGroupTypeIds )
                            {
                                foreach ( RepeaterItem item in rRooms.Items )
                                {
                                    var linky = (LinkButton)item.FindControl( "lbSelectRooms" );
                                    if ( int.Parse( linky.CommandArgument ) == id )
                                    {
                                        linky.AddCssClass( "active" );
                                    }
                                }
                            }
                        }
                    }
                }
            }
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

        #endregion
    }
}