// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [DisplayName( "Group Members" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to view the other members of a group person belongs to (e.g. Family groups)." )]

    [GroupTypeField("Group Type", "The group type to display groups for (default is Family)", false, Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "", 0)]
    [BooleanField("Auto Create Group", "If person doesn't belong to a group of this type, should one be created for them (default is Yes).", true, "", 1)]
    [LinkedPage("Group Edit Page", "Page used to edit the members of the selected group.", true, "", "", 2)]
    [LinkedPage( "Location Detail Page", "Page used to edit the settings for a particular location.", false, "", "", 3 )]
    [CodeEditorField( "Group Header Lava", "Lava to put at the top of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, order: 4)]
    [CodeEditorField( "Group Footer Lava", "Lava to put at the bottom of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, order: 5 )]
    public partial class GroupMembers : Rock.Web.UI.PersonBlock
    {
        #region Fields

        private GroupTypeCache _groupType = null;
        private bool _IsFamilyGroupType = false;
        private bool _allowEdit = false;

        // private global rockContext that is specifically for rptrGroups binding and rptrGroups_ItemDataBound
        private RockContext _bindGroupsRockContext = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _groupType = GroupTypeCache.Read( GetAttributeValue( "GroupType" ).AsGuid() );
            if ( _groupType == null )
            {
                _groupType = GroupTypeCache.GetFamilyGroupType();
            }
            _IsFamilyGroupType = _groupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            rptrGroups.ItemDataBound += rptrGroups_ItemDataBound;

            _allowEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            RegisterScripts();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            BindGroups();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemDataBound event of the rptrGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrGroups_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var group = e.Item.DataItem as Group;
                if ( group != null )
                {
                    HyperLink hlEditGroup = e.Item.FindControl( "hlEditGroup" ) as HyperLink;
                    if ( hlEditGroup != null )
                    {
                        hlEditGroup.Visible = _allowEdit;
                        var pageParams = new Dictionary<string, string>();
                        pageParams.Add( "PersonId", Person.Id.ToString() );
                        pageParams.Add( "GroupId", group.Id.ToString() );
                        hlEditGroup.NavigateUrl = LinkedPageUrl( "GroupEditPage", pageParams );
                    }

                    Repeater rptrMembers = e.Item.FindControl( "rptrMembers" ) as Repeater;
                    if ( rptrMembers != null )
                    {
                        // use the _bindGroupsRockContext that is created/disposed in BindFamilies()
                        var members = new GroupMemberService( _bindGroupsRockContext ).Queryable( "GroupRole,Person", true )
                            .Where( m =>
                                m.GroupId == group.Id &&
                                m.PersonId != Person.Id )
                            .OrderBy( m => m.GroupRole.Order )
                            .ToList();

                        // add header and footer information
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
                        mergeFields.Add( "Group", group );
                        mergeFields.Add( "GroupMembers", members );

                        Literal lGroupHeader = e.Item.FindControl( "lGroupHeader" ) as Literal;
                        Literal lGroupFooter = e.Item.FindControl( "lGroupFooter" ) as Literal;

                        lGroupHeader.Text = GetAttributeValue( "GroupHeaderLava" ).ResolveMergeFields( mergeFields );
                        lGroupFooter.Text = GetAttributeValue( "GroupFooterLava" ).ResolveMergeFields( mergeFields );

                        var orderedMembers = new List<GroupMember>();

                        if ( _IsFamilyGroupType )
                        {
                            // Add adult males
                            orderedMembers.AddRange( members
                                .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                    m.Person.Gender == Gender.Male )
                                .OrderByDescending( m => m.Person.Age ) );

                            // Add adult females
                            orderedMembers.AddRange( members
                                .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                    m.Person.Gender != Gender.Male )
                                .OrderByDescending( m => m.Person.Age ) );

                            // Add non-adults
                            orderedMembers.AddRange( members
                                .Where( m => !m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                                .OrderByDescending( m => m.Person.Age ) );
                        }
                        else
                        {
                            orderedMembers = members
                                .OrderBy( m => m.GroupRole.Order )
                                .ThenBy( m => m.Person.LastName )
                                .ThenBy( m => m.Person.NickName )
                                .ToList();
                        }
                        rptrMembers.ItemDataBound += rptrMembers_ItemDataBound;
                        rptrMembers.DataSource = orderedMembers;
                        rptrMembers.DataBind();
                    }

                    Repeater rptrAddresses = e.Item.FindControl( "rptrAddresses" ) as Repeater;
                    if ( rptrAddresses != null )
                    {
                        rptrAddresses.ItemDataBound += rptrAddresses_ItemDataBound;
                        rptrAddresses.ItemCommand += rptrAddresses_ItemCommand;
                        rptrAddresses.DataSource = new GroupLocationService( _bindGroupsRockContext ).Queryable( "Location,GroupLocationTypeValue" )
                            .Where( l => l.GroupId == group.Id )
                            .OrderBy( l => l.GroupLocationTypeValue.Order )
                            .ToList();
                        rptrAddresses.DataBind();
                    }

                    Panel pnlGroupAttributes = e.Item.FindControl( "pnlGroupAttributes" ) as Panel;
                    HyperLink hlShowMoreAttributes = e.Item.FindControl( "hlShowMoreAttributes" ) as HyperLink;
                    PlaceHolder phGroupAttributes = e.Item.FindControl( "phGroupAttributes" ) as PlaceHolder;
                    PlaceHolder phMoreGroupAttributes = e.Item.FindControl( "phMoreGroupAttributes" ) as PlaceHolder;

                    if ( pnlGroupAttributes  != null && hlShowMoreAttributes != null && phGroupAttributes != null && phMoreGroupAttributes != null )
                    {
                        hlShowMoreAttributes.Visible = false;
                        phGroupAttributes.Controls.Clear();
                        phMoreGroupAttributes.Controls.Clear();

                        group.LoadAttributes();
                        var attributes = group.GetAuthorizedAttributes( Authorization.VIEW, CurrentPerson )
                            .Select( a => a.Value )
                            .OrderBy( a => a.Order )
                            .ToList();

                        foreach( var attribute in attributes )
                        {
                            if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            {
                                string value = attribute.DefaultValue;
                                if ( group.AttributeValues.ContainsKey( attribute.Key ) && group.AttributeValues[attribute.Key] != null )
                                {
                                    value = group.AttributeValues[attribute.Key].ValueFormatted;
                                }

                                if ( !string.IsNullOrWhiteSpace( value ) )
                                {
                                    var literalControl = new RockLiteral();
                                    literalControl.ID = string.Format( "familyAttribute_{0}", attribute.Id );
                                    literalControl.Label = attribute.Name;
                                    literalControl.Text = value;

                                    var div = new HtmlGenericControl( "div" );
                                    div.AddCssClass( "col-md-3 col-sm-6" );
                                    div.Controls.Add( literalControl );

                                    if ( attribute.IsGridColumn )
                                    {
                                        phGroupAttributes.Controls.Add( div );
                                    }
                                    else
                                    {
                                        hlShowMoreAttributes.Visible = true;
                                        phMoreGroupAttributes.Controls.Add( div );
                                    }
                                }
                            }
                        }

                        pnlGroupAttributes.Visible = phGroupAttributes.Controls.Count > 0 || phMoreGroupAttributes.Controls.Count > 0;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupMember = e.Item.DataItem as GroupMember;
                if ( groupMember != null && groupMember.Person != null )
                {
                    Person fm = groupMember.Person;

                    // very similar code in EditFamily.ascx.cs
                    HtmlControl divPersonImage = e.Item.FindControl( "divPersonImage" ) as HtmlControl;
                    if ( divPersonImage != null )
                    {
                        divPersonImage.Style.Add( "background-image", @String.Format( @"url({0})", Person.GetPersonPhotoUrl( fm ) + "&width=65" ) );
                        divPersonImage.Style.Add( "background-size", "cover" );
                        divPersonImage.Style.Add( "background-position", "50%" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrAddresses_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupLocation = e.Item.DataItem as GroupLocation;
                if ( groupLocation != null && groupLocation.Location != null )
                {
                    Location loc = groupLocation.Location;

                    HtmlAnchor aMap = e.Item.FindControl( "aMap" ) as HtmlAnchor;
                    if ( aMap != null )
                    {
                        aMap.HRef = loc.GoogleMapLink( Person.FullName );
                    }

                    LinkButton lbVerify = e.Item.FindControl( "lbVerify" ) as LinkButton;
                    if ( lbVerify != null )
                    {
                        if ( Rock.Address.VerificationContainer.Instance.Components.Any( c => c.Value.Value.IsActive ) )
                        {
                            lbVerify.Visible = true;
                            lbVerify.CommandArgument = loc.Id.ToString();
                        }
                        else
                        {
                            lbVerify.Visible = false;
                        }
                    }

                    LinkButton lbLocationSettings = e.Item.FindControl( "lbLocationSettings" ) as LinkButton;
                    if ( lbLocationSettings != null )
                    {
                        if ( UserCanAdministrate )
                        {
                            lbLocationSettings.Visible = true;
                            lbLocationSettings.CommandArgument = loc.Id.ToString();
                        }
                        else
                        {
                            lbLocationSettings.Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptrAddresses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrAddresses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int locationId = int.MinValue;
            if ( int.TryParse( e.CommandArgument.ToString(), out locationId ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new LocationService( rockContext );
                    var location = service.Get( locationId );

                    if ( location != null )
                    {
                        switch ( e.CommandName )
                        {
                            case "verify":
                                {
                                    service.Verify( location, true );
                                    rockContext.SaveChanges();
                                    break;
                                }
                            case "settings":
                                {
                                    NavigateToLinkedPage( "LocationDetailPage", 
                                        new Dictionary<string, string> { { "LocationId", location.Id.ToString() }, { "PersonId", Person.Id.ToString() } } );
                                    break;
                                }
                        }
                    }
                }
            }

            BindGroups();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the groups.
        /// </summary>
        private void BindGroups()
        {
            if ( Person != null && Person.Id > 0 )
            {
                using ( _bindGroupsRockContext = new RockContext() )
                {
                    var memberService = new GroupMemberService( _bindGroupsRockContext );
                    var groups = memberService.Queryable( true )
                        .Where( m =>
                            m.PersonId == Person.Id &&
                            m.Group.GroupTypeId == _groupType.Id )
                        .Select( m => m.Group )
                        .ToList();

                    if ( !groups.Any() && GetAttributeValue("AutoCreateGroup").AsBoolean(true) )
                    {
                        // ensure that the person is in a group

                        var groupService = new GroupService( _bindGroupsRockContext );
                        var group = new Group();
                        group.Name = Person.LastName;
                        group.GroupTypeId = _groupType.Id;
                        groupService.Add( group );
                        _bindGroupsRockContext.SaveChanges();

                        var groupMember = new GroupMember();
                        groupMember.PersonId = Person.Id;
                        groupMember.GroupRoleId = _groupType.DefaultGroupRoleId.Value;
                        groupMember.GroupId = group.Id;
                        group.Members.Add( groupMember );
                        _bindGroupsRockContext.SaveChanges();

                        groups.Add( groupService.Get( group.Id ) );
                    }
                    
                    rptrGroups.DataSource = groups;
                    rptrGroups.DataBind();
                }
            }
        }

        /// <summary>
        /// Formats the type of the address.
        /// </summary>
        /// <param name="addressType">Type of the address.</param>
        /// <returns></returns>
        protected string FormatAddressType( object addressType )
        {
            string type = addressType != null ? addressType.ToString() : "Unknown";
            return type.EndsWith( "Address", StringComparison.CurrentCultureIgnoreCase ) ? type : type + " Address";
        }

        /// <summary>
        /// Formats the person link...keeping the subpage route on the person link.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>a link to the profile page of the given person</returns>
        protected string FormatPersonLink( string personId )
        {
            // Look for a subpage route (anything after the "/Person/{id}" part of the URL)
            var subpageRoute = Request.Url.AbsolutePath.Replace( ResolveRockUrl( string.Format("~/Person/{0}", Person.Id ) ), "" );

            // If the path is different, then append it onto the link
            if ( subpageRoute != Request.Url.AbsolutePath )
            {
                return ResolveRockUrl( string.Format( "~/Person/{0}{1}", personId, subpageRoute ) );
            }
            else
            {
                return ResolveRockUrl( string.Format( "~/Person/{0}", personId ) );
            }
        }

        /// <summary>
        /// Formats the name of the person.
        /// </summary>
        /// <param name="nickName">Name of the nick.</param>
        /// <param name="lastName">The last name.</param>
        /// <returns></returns>
        protected string FormatPersonName( string nickName, string lastName )
        {
            if ( Person != null && ( Person.LastName != lastName || !_IsFamilyGroupType ) )
            {
                return string.Format( "{0} {1}", nickName, lastName );
            }

            return nickName;
        }

        /// <summary>
        /// Formats the person CSS class.
        /// </summary>
        /// <param name="isDeceased">The is deceased.</param>
        /// <returns></returns>
        protected string FormatPersonCssClass( bool isDeceased )
        {
            return isDeceased ? "member deceased" : "member";
        }

        /// <summary>
        /// Formats the details.
        /// </summary>
        /// <param name="dataitem">The dataitem.</param>
        /// <returns></returns>
        protected string FormatPersonDetails( object dataitem )
        {
            var gm = dataitem as GroupMember;
            if ( gm != null )
            {
                return _IsFamilyGroupType ?
                    ( gm.Person.FormatAge(true) ) :
                    gm.GroupRole.Name;
            }
            return string.Empty;
        }

        /// <summary>
        /// Formats as HTML title.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        protected string FormatAsHtmlTitle( string str )
        {
            return str.FormatAsHtmlTitle();
        }

        private void RegisterScripts()
        {
            string script = @"
    $('.js-show-more-family-attributes').click(function (e) {
        var $pnl = $(this).closest('.js-persondetails-group');
        var $moreAttributes = $pnl.find('.js-more-group-attributes').first();
        if ( $moreAttributes.is(':visible') ) {
            $moreAttributes.slideUp();
            $(this).html('<i class=""fa fa-chevron-down""></i>');
        } else {
            $moreAttributes.slideDown();
            $(this).html('<i class=""fa fa-chevron-up""></i>');
        }
    });";
            ScriptManager.RegisterStartupScript( upGroupMembers, upGroupMembers.GetType(), "showmore", script, true );

        }

        #endregion
    }
}