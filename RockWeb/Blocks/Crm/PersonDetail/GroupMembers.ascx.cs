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
using Rock.Drawing.Avatar;
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

    [EnumField(
        "Avatar Style",
        Key = AttributeKey.AvatarStyle,
        Description = "Allows control of the person photo avatar to use either an icon to represent the person's gender and age classification, or first and last name initials when the person does not have a photo.",
        IsRequired = true,
        EnumSourceType = typeof( AvatarStyle ),
        DefaultEnumValue = ( int ) AvatarStyle.Icon,
        Order = 7 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F" )]
    public partial class GroupMembers : Rock.Web.UI.PersonBlock
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
            public const string AvatarStyle = "AvatarStyle";
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

        #endregion Fields

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

            _allowEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            _showCounty = GetAttributeValue( AttributeKey.ShowCounty ).AsBoolean();
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

        #endregion Base Control Methods

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
                        .Where( m => m.PersonId == Person.Id && m.Group.GroupTypeId == _groupType.Id )
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
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            if ( !( e.Item.DataItem is Group group ) )
            {
                return;
            }

            if ( e.Item.FindControl( "hlEditGroup" ) is HyperLink hlEditGroup )
            {
                hlEditGroup.Visible = _allowEdit;
                var pageParams = new Dictionary<string, string>();
                pageParams.Add( "PersonId", Person.Id.ToString() );
                pageParams.Add( "GroupId", group.Id.ToString() );
                hlEditGroup.NavigateUrl = LinkedPageUrl( AttributeKey.GroupEditPage, pageParams );
            }

            var lReorderIcon = e.Item.FindControl( "lReorderIcon" ) as Control;
            lReorderIcon.Visible = _showReorderIcon;

            if ( e.Item.FindControl( "rptrMembers" ) is Repeater rptrMembers )
            {
                // use the _bindGroupsRockContext that is created/disposed in BindFamilies()
                var members = new GroupMemberService( _bindGroupsRockContext )
                    .Queryable( "GroupRole,Person", true )
                    .Where( m => m.GroupId == group.Id && m.PersonId != Person.Id )
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
                        .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) && m.Person.Gender == Gender.Male )
                        .OrderByDescending( m => m.Person.Age ) );

                    // Add adult females
                    orderedMembers.AddRange( members
                        .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) && m.Person.Gender != Gender.Male )
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

                if ( orderedMembers.Count == 0 )
                {
                    var pnlMembersDiv = e.Item.FindControl( "pnlMembersDiv" ) as Control;
                    pnlMembersDiv.Visible = false;
                }

                rptrMembers.DataSource = orderedMembers;
                rptrMembers.DataBind();
            }

            if ( e.Item.FindControl( "rptrAddresses" ) is Repeater rptrAddresses )
            {
                rptrAddresses.DataSource = new GroupLocationService( _bindGroupsRockContext )
                    .Queryable( "Location,GroupLocationTypeValue" )
                    .Where( l => l.GroupId == group.Id )
                    .OrderBy( l => l.GroupLocationTypeValue.Order )
                    .ToList();
                // if datasource has no items, then don't bind
                if ( rptrAddresses.DataSource is List<GroupLocation> groupLocations && groupLocations.Any() )
                {
                    rptrAddresses.DataBind();
                }
                else
                {
                    rptrAddresses.Visible = false;
                }
            }

            ShowGroupAttributes( group, e );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            if ( !( e.Item.DataItem is GroupMember groupMember ) || groupMember.Person == null )
            {
                return;
            }

            var personLink = FormatPersonLink( groupMember.Person.Id.ToString() );

            if ( e.Item.FindControl( "litGroupMemberInfo" ) is Literal litGroupMemberInfo )
            {
                litGroupMemberInfo.Text = $@"
                    <div class=""{FormatPersonCssClass( groupMember.Person.IsDeceased ) }"">
                        <a href=""{personLink}"" class=""photo-link"">
                            <img src=""{GetPersonPhotoUrl( groupMember.Person, 400 )}"" alt class=""img-cover inset-0"">
                            <div class=""photo-shadow inset-0""></div>
                        </a>
                        <a href=""{personLink}"" class=""name-link"">
                            <span class=""name"">{FormatPersonName( groupMember.Person.NickName, groupMember.Person.LastName )}</span>
                            <span class=""person-age"">{groupMember.Person.Age}</span>
                        </a>
                    </div>";
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrAddresses_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            var groupLocation = e.Item.DataItem as GroupLocation;
            if ( groupLocation == null && groupLocation.Location == null )
            {
                return;
            }

            if( e.Item.FindControl( "litAddress" ) is Literal litAddress )
            {
                groupLocation.GroupLocationTypeValue.LoadAttributes();
                var iconCssClass = groupLocation.GroupLocationTypeValue.GetAttributeValue( "IconCSSClass" ) ?? "fa fa-map-marker";

                litAddress.Text = $@"
                    <div class=""profile-row group-hover"">
                        <span class=""profile-row-icon"">
                            <a href=""{groupLocation.Location.GoogleMapLink( Person.FullName )}"" title=""Map This Address"" class=""map"" target=""_blank"">
                                <i class=""{iconCssClass}""></i>
                            </a>
                        </span>
                        <dl class=""m-0"">
                            <dt>{FormatAddressType( groupLocation.GroupLocationTypeValue.Value )}</dt>
                            <dd>{FormatAddress( groupLocation.Location )}</dd>
                        </dl>";
            }

            if ( e.Item.FindControl( "lbVerify" ) is LinkButton lbVerify )
            {
                if ( Rock.Address.VerificationContainer.Instance.Components.Any( c => c.Value.Value.IsActive ) )
                {
                    lbVerify.Visible = true;
                    lbVerify.CommandArgument = groupLocation.Location.Id.ToString();
                }
                else
                {
                    lbVerify.Visible = false;
                }
            }

            if ( e.Item.FindControl( "lbLocationSettings" ) is LinkButton lbLocationSettings )
            {
                if ( UserCanEdit )
                {
                    lbLocationSettings.Visible = true;
                    lbLocationSettings.CommandArgument = groupLocation.Location.Id.ToString();
                }
                else
                {
                    lbLocationSettings.Visible = false;
                }
            }
        }

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
                                service.Verify( location, true );
                                rockContext.SaveChanges();
                                break;

                            case "settings":
                                NavigateToLinkedPage( AttributeKey.LocationDetailPage,
                                    new Dictionary<string, string> { { "LocationId", location.Id.ToString() }, { "PersonId", Person.Id.ToString() } } );
                                break;
                        }
                    }
                }
            }

            BindGroups();
        }

        #endregion Events

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

            // Bind the Groups repeater which will show the Groups with a list of GroupMembers
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

                    groups.Add( groupService.GetInclude( group.Id, g => g.GroupType ) );
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
            if ( !( locationObject is Location location ) )
            {
                return string.Empty;
            }

            if ( !_showCounty )
            {
                return location.FormattedHtmlAddress;
            }

            return $"{location.Street1}<br/>{location.Street2}<br/>{location.City}{( location.County.IsNotNullOrWhiteSpace() ? ", " + location.County : "" )}, {location.State} {location.PostalCode}"
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

            // Look for a subpage route (anything after the "/Person/{id}" part of the URL)
            var subpageRoute = Request.UrlProxySafe().AbsolutePath.ReplaceCaseInsensitive( ResolveRockUrl( $"~/Person/{currentPersonId}" ), "" );

            // If the path is different, then append it onto the link
            if ( subpageRoute != Request.UrlProxySafe().AbsolutePath )
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
            return isDeceased ? "family-member family-member-deceased" : "family-member";
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

        protected void ShowGroupAttributes( Group group, RepeaterItemEventArgs e )
        {
            var pnlGroupAttributes = e.Item.FindControl( "pnlGroupAttributes" ) as Panel;
            var litGroupAttributes = e.Item.FindControl( "litGroupAttributes" ) as Literal;
            var litMoreGroupAttributes = e.Item.FindControl( "litMoreGroupAttributes" ) as Literal;
            var lblShowGroupAttributeTitle = e.Item.FindControl( "lblShowGroupAttributeTitle" ) as Label;

            if ( pnlGroupAttributes == null || litGroupAttributes == null || litMoreGroupAttributes == null )
            {
                return;
            }

            litGroupAttributes.Text = string.Empty;
            litMoreGroupAttributes.Text = string.Empty;
            pnlGroupAttributes.Visible = false;

            group.LoadAttributes();
            var authorizedAttributes = group.GetAuthorizedAttributes( Authorization.VIEW, CurrentPerson ).Select( a => a.Value ).OrderBy( a => a.Order ).ToList();

            foreach ( var attribute in authorizedAttributes )
            {
                string value = attribute.DefaultValue;
                if ( group.AttributeValues.ContainsKey( attribute.Key ) && group.AttributeValues[attribute.Key] != null )
                {
                    value = group.AttributeValues[attribute.Key].ValueFormatted;
                }

                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    var attributeHtml = $@"
                        <dt>{attribute.Name}</dt>
                        <dd>{value}</dd>";

                    if ( attribute.IsGridColumn )
                    {
                        litGroupAttributes.Text += attributeHtml;
                    }
                    else
                    {
                        litMoreGroupAttributes.Text += attributeHtml;
                    }
                }
            }

            // show the group attributes panel if there are any group attributes to be displayed
            if( litGroupAttributes.Text.IsNotNullOrWhiteSpace() || litMoreGroupAttributes.Text.IsNotNullOrWhiteSpace() )
            {
                pnlGroupAttributes.Visible = true;
            }

            if( litGroupAttributes.Text.IsNullOrWhiteSpace() && litMoreGroupAttributes.Text.IsNotNullOrWhiteSpace() )
            {
                lblShowGroupAttributeTitle.Text = group.GroupType.Name + " Attributes <a class='js-show-more-family-attributes stretched-link' href='#' title='Show More " + group.GroupType.Name +" Attributes'><i class='fa fa-chevron-down'></i></a>";
                lblShowGroupAttributeTitle.AddCssClass( "d-flex justify-content-between position-relative" );
            }
            else
            {
                lblShowGroupAttributeTitle.Text = "<a class='js-show-more-family-attributes' href='#' title='Show More " + group.GroupType.Name +" Attributes'><i class='fa fa-chevron-down'></i></a>";
                lblShowGroupAttributeTitle.AddCssClass( "pull-right" );
            }

            if( litMoreGroupAttributes.Text.IsNotNullOrWhiteSpace() )
            {
                litMoreGroupAttributes.Text = $"<div class='js-more-group-attributes mt-2' style='display:none'><dl class='m-0'>{litMoreGroupAttributes.Text}</dl></div>";
            }

            if( litGroupAttributes.Text.IsNotNullOrWhiteSpace())
            {
                litGroupAttributes.Text = $"<dl class='m-0'>{litGroupAttributes.Text}</dl>";
            }
        }

        private string GetPersonPhotoUrl( Person person, int? maxWidth = null, int? maxHeight = null )
        {
            var personPhotoUrl = Person.GetPersonPhotoUrl( person, maxWidth, maxHeight );

            var avatarStyle = GetAttributeValue( AttributeKey.AvatarStyle ).ConvertToEnum<AvatarStyle>( AvatarStyle.Icon );
            if ( avatarStyle == AvatarStyle.Icon )
            {
                var iconUrlParameters = "&Style=icon&BackgroundColor=E4E4E7&ForegroundColor=A1A1AA";
                personPhotoUrl += iconUrlParameters;
            }

            return personPhotoUrl;
        }

        #endregion Methods
    }
}