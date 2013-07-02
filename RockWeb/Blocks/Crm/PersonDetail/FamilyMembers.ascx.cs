//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class FamilyMembers : Rock.Web.UI.PersonBlock
    {
        protected override void OnInit(EventArgs e)
        {
            rptrFamilies.ItemDataBound += rptrFamilies_ItemDataBound;
 	        base.OnInit(e);
        }


        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //if (!Page.IsPostBack)
            //{
                BindFamilies();
            //}
        }

        void rptrFamilies_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var group = e.Item.DataItem as Group;
                if (group != null)
                {
                    HyperLink hlEditFamily = e.Item.FindControl( "hlEditFamily" ) as HyperLink;
                    if ( hlEditFamily != null )
                    {
                        hlEditFamily.NavigateUrl = string.Format( "~/EditFamily/{0}/{1}", Person.Id, group.Id );
                    }

                    Repeater rptrMembers = e.Item.FindControl( "rptrMembers" ) as Repeater;
                    if (rptrMembers != null)
                    {
                        rptrMembers.ItemDataBound += rptrMembers_ItemDataBound;
                        rptrMembers.DataSource = group.Members
                            .Where( m => m.PersonId != Person.Id)
                            .OrderBy( m => m.GroupRole.SortOrder )
                            .ToList();
                        rptrMembers.DataBind();
                    }

                    Repeater rptrAddresses =  e.Item.FindControl("rptrAddresses") as Repeater;
                    {
                        rptrAddresses.ItemDataBound += rptrAddresses_ItemDataBound;
                        rptrAddresses.ItemCommand += rptrAddresses_ItemCommand;
                        rptrAddresses.DataSource = group.GroupLocations.ToList();
                        rptrAddresses.DataBind();
                    }
                }
            }
        }

        void rptrMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupMember = e.Item.DataItem as GroupMember;
                if ( groupMember != null && groupMember.Person != null )
                {
                    Person fm = groupMember.Person;

                    System.Web.UI.WebControls.Image imgPerson = e.Item.FindControl( "imgPerson" ) as System.Web.UI.WebControls.Image;
                    if ( imgPerson != null )
                    {
                        imgPerson.Visible = Person.PhotoId.HasValue;
                        if ( Person.PhotoId.HasValue )
                        {
                            imgPerson.ImageUrl = string.Format( "~/image.ashx?id={0}", Person.PhotoId );
                        }
                    }

                }
            }
        }

        void rptrAddresses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupLocation = e.Item.DataItem as GroupLocation;
                if (groupLocation != null && groupLocation.Location != null)
                {
                    Location loc = groupLocation.Location;

                    HtmlAnchor aMap = e.Item.FindControl( "aMap" ) as HtmlAnchor;
                    if ( aMap != null )
                    {
                        aMap.HRef = loc.GoogleMapLink( Person.FullName );
                    }

                    LinkButton lbGeocode = e.Item.FindControl( "lbGeocode" ) as LinkButton;
                    if ( lbGeocode != null )
                    {
                        if ( Rock.Address.GeocodeContainer.Instance.Components.Any( c => c.Value.Value.IsActive ) )
                        {
                            lbGeocode.Visible = true;
                            lbGeocode.CommandName = "geocode";
                            lbGeocode.CommandArgument = loc.Id.ToString();

                            if ( loc.GeocodedDateTime.HasValue )
                            {
                                lbGeocode.ToolTip = string.Format( "{0} {1}",
                                    loc.LocationPoint.Latitude,
                                    loc.LocationPoint.Longitude );
                            }
                            else
                            {
                                lbGeocode.ToolTip = "Geocode Address";
                            }
                        }
                        else
                        {
                            lbGeocode.Visible = false;
                        }
                    }

                    LinkButton lbStandardize = e.Item.FindControl( "lbStandardize" ) as LinkButton;
                    if ( lbStandardize != null )
                    {
                        if ( Rock.Address.StandardizeContainer.Instance.Components.Any( c => c.Value.Value.IsActive ) )
                        {
                            lbStandardize.Visible = true;
                            lbStandardize.CommandName = "standardize";
                            lbStandardize.CommandArgument = loc.Id.ToString();

                            if ( loc.StandardizedDateTime.HasValue )
                            {
                                lbStandardize.ToolTip = "Address Standardized";
                            }
                            else
                            {
                                lbStandardize.ToolTip = "Standardize Address";
                            }
                        }
                        else
                        {
                            lbStandardize.Visible = false;
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( loc.Street2 ) )
                    {
                        PlaceHolder phStreet2 = e.Item.FindControl( "phStreet2" ) as PlaceHolder;
                        if ( phStreet2 != null )
                        {
                            phStreet2.Controls.Add( new LiteralControl( string.Format( "{0}</br>", loc.Street2 ) ) );
                        }
                    }
                }
            }
        }

        void rptrAddresses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int locationId = int.MinValue;
            if ( int.TryParse( e.CommandArgument.ToString(), out locationId ) )
            {
                var service = new LocationService();
                var location = service.Get( locationId );

                switch ( e.CommandName )
                {
                    case "geocode":
                        service.Geocode( location, CurrentPersonId );
                        break;

                    case "standardize":
                        service.Standardize( location, CurrentPersonId );
                        break;
                }

                service.Save( location, CurrentPersonId );
            }

            BindFamilies();
        }

        private void BindFamilies()
        {
            Guid familyGroupGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            var groups = new List<Group>();

            var memberService = new GroupMemberService();
            foreach ( Group group in memberService.Queryable()
                .Where( m =>
                    m.PersonId == Person.Id &&
                    m.Group.GroupType.Guid == familyGroupGuid
                )
                .Select( m => m.Group ) )
            {
                if ( group.IsAuthorized( "View", CurrentPerson ) )
                {
                    groups.Add( group );
                }
            }

            rptrFamilies.DataSource = groups;
            rptrFamilies.DataBind();
        }

    }
}