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
using System.Data.Entity;
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

    #region Block Attributes

    [GroupTypeField(
        "Group Type",
        Key = AttributeKey.GroupType,
        Description = "The group type to display groups for (default is Family)",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        Order = 0 )]

    [BooleanField(
        "Auto Create Group",
        Key = AttributeKey.AutoCreateGroup,
        Description = "If person doesn't belong to a group of this type, should one be created for them (default is Yes).",
        DefaultBooleanValue = true,
        Order = 1 )]

    [LinkedPage(
        "Group Edit Page",
        Key = AttributeKey.GroupEditPage,
        Description = "Page used to edit the members of the selected group.",
        IsRequired = true,
        Order = 2 )]

    [LinkedPage(
        "Location Detail Page",
        Key = AttributeKey.LocationDetailPage,
        Description = "Page used to edit the settings for a particular location.",
        IsRequired = false,
        Order = 3 )]

    [BooleanField(
        "Show County",
        Key = AttributeKey.ShowCounty,
        Description = "Should County be displayed when editing an address?.",
        DefaultBooleanValue = false,
        Order = 4 )]

    [CodeEditorField(
        "Group Header Lava",
        Key = AttributeKey.GroupHeaderLava,
        Description = "Lava to put at the top of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 5 )]

    [CodeEditorField(
        "Group Footer Lava",
        Key = AttributeKey.GroupFooterLava,
        Description = "Lava to put at the bottom of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 6 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "FC137BDA-4F05-4ECE-9899-A249C90D11FC" )]
    public partial class GroupMembersV1 : Rock.Web.UI.PersonBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string AutoCreateGroup = "AutoCreateGroup";
            public const string GroupEditPage = "GroupEditPage";
            public const string LocationDetailPage = "LocationDetailPage";
            public const string ShowCounty = "ShowCounty";
            public const string GroupHeaderLava = "GroupHeaderLava";
            public const string GroupFooterLava = "GroupFooterLava";
        }
        #endregion Attribute Keys

        #region Fields

        private GroupTypeCache _groupType = null;
        private bool _IsFamilyGroupType = false;
        private bool _allowEdit = false;
        private bool _showCounty = false;

        // private global rockContext that is specifically for rptrGroups binding and rptrGroups_ItemDataBound
        private RockContext _bindGroupsRockContext = null;
        private bool _showReorderIcon;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _groupType = GroupTypeCache.Get( GetAttributeValue( AttributeKey.GroupType ).AsGuid() );
            if ( _groupType == null )
            {
                _groupType = GroupTypeCache.GetFamilyGroupType();
            }
            _IsFamilyGroupType = _groupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            rptrGroups.ItemDataBound += rptrGroups_ItemDataBound;

            _allowEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            _showCounty = GetAttributeValue( AttributeKey.ShowCounty ).AsBoolean();

            RegisterScripts();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            BindGroups();

            // handle sort events
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "re-order-panel-widget" ) )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            SortGroupsForGroupMember( eventParam, values );
                        }
                    }
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Sorts the groups for group member.
        /// </summary>
        /// <param name="eventParam">The event parameter.</param>
        /// <param name="values">The values.</param>
        private void SortGroupsForGroupMember( string eventParam, string[] values )
        {
            string panelWidgetClientId = values[0];
            int newIndex = int.Parse( values[1] );

            if ( Person != null && Person.Id > 0 )
            {
                Panel pnlWidget = this.ControlsOfTypeRecursive<Panel>().FirstOrDefault( a => a.ClientID == panelWidgetClientId );
                HiddenField hfGroupId = pnlWidget.FindControl( "hfGroupId" ) as HiddenField;
                var groupId = hfGroupId.Value.AsInteger();

                using ( _bindGroupsRockContext = new RockContext() )
                {
                    var memberService = new GroupMemberService( _bindGroupsRockContext );
                    var groupMemberGroups = memberService.Queryable( true )
                        .Where( m =>
                            m.PersonId == Person.Id &&
                            m.Group.GroupTypeId == _groupType.Id )
                        .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                        .ToList();

                    var groupMember = groupMemberGroups.FirstOrDefault( a => a.GroupId == groupId );
                    if ( groupMember != null )
                    {
                        memberService.ReorderGroupMemberGroup( groupMemberGroups, groupMemberGroups.IndexOf( groupMember ), newIndex );
                        _bindGroupsRockContext.SaveChanges();
                    }

                    BindGroups();
                }
            }
        }

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
                        hlEditGroup.NavigateUrl = LinkedPageUrl( AttributeKey.GroupEditPage, pageParams );
                    }

                    var lReorderIcon = e.Item.FindControl( "lReorderIcon" ) as Control;
                    lReorderIcon.Visible = _showReorderIcon;

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

                        var groupHeaderLava = GetAttributeValue( AttributeKey.GroupHeaderLava );
                        var groupFooterLava = GetAttributeValue( AttributeKey.GroupFooterLava );

                        if ( groupHeaderLava.IsNotNullOrWhiteSpace() || groupFooterLava.IsNotNullOrWhiteSpace() )
                        {
                            // add header and footer information
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                            mergeFields.Add( "Group", group );
                            mergeFields.Add( "GroupMembers", members );

                            Literal lGroupHeader = e.Item.FindControl( "lGroupHeader" ) as Literal;
                            Literal lGroupFooter = e.Item.FindControl( "lGroupFooter" ) as Literal;

                            lGroupHeader.Text = groupHeaderLava.ResolveMergeFields( mergeFields );
                            lGroupFooter.Text = groupFooterLava.ResolveMergeFields( mergeFields );
                        }

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

                    if ( pnlGroupAttributes != null && hlShowMoreAttributes != null && phGroupAttributes != null && phMoreGroupAttributes != null )
                    {
                        hlShowMoreAttributes.Visible = false;
                        phGroupAttributes.Controls.Clear();
                        phMoreGroupAttributes.Controls.Clear();

                        group.LoadAttributes();
                        var attributes = group.GetAuthorizedAttributes( Authorization.VIEW, CurrentPerson )
                            .Select( a => a.Value )
                            .OrderBy( a => a.Order )
                            .ToList();

                        foreach ( var attribute in attributes )
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
                        if ( UserCanEdit )
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
                                    NavigateToLinkedPage( AttributeKey.LocationDetailPage,
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
            if ( Person == null || Person.Id == 0 || Person.IsNameless() )
            {
                return;
            }


            // If this is a Family GroupType and they belong to multiple families,
            // first make sure that the GroupMember.GroupOrder is set for this Person's Families.
            // This will ensure that other spots that rely on the GroupOrder provide consistent results.
            if ( this._IsFamilyGroupType )
            {
                using ( var rockContext = new RockContext() )
                {
                    var memberService = new GroupMemberService( rockContext );
                    var groupMemberGroups = memberService.Queryable( true )
                        .Where( m =>
                            m.PersonId == Person.Id &&
                            m.Group.GroupTypeId == _groupType.Id )
                        .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                        .ToList();

                    if ( groupMemberGroups.Count > 1 && memberService.SetGroupMemberGroupOrder( groupMemberGroups ) )
                    {
                        rockContext.SaveChanges();
                    }
                }
            }

            // Gind the Groups repeater which will show the Groups with a list of GroupMembers
            using ( _bindGroupsRockContext = new RockContext() )
            {
                var memberService = new GroupMemberService( _bindGroupsRockContext );
                var groups = memberService.Queryable( true )
                    .Where( m =>
                        m.PersonId == Person.Id &&
                        m.Group.GroupTypeId == _groupType.Id )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                    .Select( m => m.Group )
                    .AsNoTracking()
                    .ToList();

                if ( !groups.Any() && GetAttributeValue( AttributeKey.AutoCreateGroup ).AsBoolean( true ) )
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

                _showReorderIcon = groups.Count > 1;
                rptrGroups.DataBind();
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
        /// Formats the address.
        /// </summary>
        /// <param name="locationObject">The location object.</param>
        /// <returns></returns>
        protected string FormatAddress( object locationObject )
        {
            var location = locationObject as Location;
            if ( location == null )
            {
                return string.Empty;
            }

            if ( !_showCounty )
            {
                return location.FormattedHtmlAddress;
            }

            return string.Format(
                "{0}<br/>{1}<br/>{2}{3}, {4} {5}",
                location.Street1,
                location.Street2,
                location.City,
                location.County.IsNotNullOrWhiteSpace() ? ", " + location.County : "",
                location.State,
                location.PostalCode )
                .ReplaceWhileExists( "  ", " " )
                .ReplaceWhileExists( "<br/><br/>", "<br/>" );
        }

        /// <summary>
        /// Formats the person link...keeping the subpage route on the person link.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>a link to the profile page of the given person</returns>
        protected string FormatPersonLink( string personId )
        {
            var currentPersonId = Person.Id.ToString();

            if ( PageCache.PageContexts.ContainsKey( Person.TypeName ) )
            {
                currentPersonId = PageParameter( PageCache.PageContexts[Person.TypeName] );
            }

            // Look for a subpage route (anything after the "v1/Person/{id}" part of the URL)
            var subpageRoute = Request.UrlProxySafe().AbsolutePath.ReplaceCaseInsensitive( ResolveRockUrl( $"~/v1/Person/{currentPersonId}" ), "" );

            // If the path is different, then append it onto the link
            if ( subpageRoute != Request.UrlProxySafe().AbsolutePath )
            {
                return ResolveRockUrl( string.Format( "~/v1/Person/{0}{1}", personId, subpageRoute ) );
            }
            else
            {
                return ResolveRockUrl( string.Format( "~/v1/Person/{0}", personId ) );
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
                    ( gm.Person.FormatAge( true ) ) :
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
    $('.js-show-more-family-attributes').on('click', function (e) {
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