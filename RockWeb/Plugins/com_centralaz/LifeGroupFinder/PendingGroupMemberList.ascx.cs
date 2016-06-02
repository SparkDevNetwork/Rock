// <copyright>
// Copyright by Central Christian Church
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
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.LifeGroupFinder
{
    [DisplayName( "Pending Group Member List" )]
    [Category( "com_centralaz > Groups" )]
    [Description( "Lists all the pending members of a group type" )]

    [GroupTypesField( "Group Types", "Pending members from groups of these grouptypes will be displayed", true )]
    [GroupField( "Root Group", "Pending members from groups under this group will be displayed", true )]
    [LinkedPage( "Group Member Detail Page" )]
    [LinkedPage( "Group Detail Page" )]
    public partial class PendingGroupMemberList : RockBlock
    {
        #region Private Variables

        private bool _canView = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('.js-person-popover').popover({
        placement: 'right', 
        trigger: 'manual',
        delay: 500,
        html: true,
        content: function() {
            var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' +  $(this).attr('personid') + '/false';

            var result = $.ajax({ 
                                type: 'GET', 
                                url: dataUrl, 
                                dataType: 'json', 
                                contentType: 'application/json; charset=utf-8',
                                async: false }).responseText;
            
            var resultObject = jQuery.parseJSON(result);

            return resultObject.PickerItemDetailsHtml;

        }
    }).on('mouseenter', function () {
        var _this = this;
        $(this).popover('show');
        $(this).siblings('.popover').on('mouseleave', function () {
            $(_this).popover('hide');
        });
    }).on('mouseleave', function () {
        var _this = this;
        setTimeout(function () {
            if (!$('.popover:hover').length) {
                $(_this).popover('hide')
            }
        }, 100);
    });

   // $('.js-person-popover').popover('show'); // uncomment for styling
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "person-link-popover", script, true );

            if ( IsUserAuthorized( Authorization.VIEW ) )
            {
                _canView = true;

                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                gGroupMembers.DataKeyNames = new string[] { "Id" };
                gGroupMembers.CommunicateMergeFields = new List<string> { "GroupRole" };
                gGroupMembers.PersonIdField = "PersonId";
                gGroupMembers.GridRebind += gGroupMembers_GridRebind;

                // make sure they have Auth to edit the block OR edit to the Group
                bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            }

            string deleteScript = @"
    $('table.js-grid-group-members a.grid-delete-button').click(function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this group member?', function (result) {
            if (result) {
                if ( $btn.closest('tr').hasClass('js-has-registration') ) {
                    Rock.dialogs.confirm('This group member was added through a registration. Are you really sure that you want to delete this group member and remove the link from the registration? ', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gGroupMembers, gGroupMembers.GetType(), "deleteInstanceScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                pnlContent.Visible = _canView;
                if ( _canView )
                {
                    SetFilter();
                    BindGroupMembersGrid();
                }
            }
        }

        #endregion

        #region GroupMembers Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "First Name", "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name", "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( "Campus", "Campus", cpCampusFilter.SelectedCampusId.ToString() );

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "First Name" )
            {
                return;
            }
            else if ( e.Key == "Last Name" )
            {
                return;
            }
            else if ( e.Key == "Campus" )
            {
                var campusId = e.Value.AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    var campusCache = CampusCache.Read( campusId.Value );
                    e.Value = campusCache.Name;
                }
                else
                {
                    e.Value = null;
                }
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupMembers_GridRebind( object sender, EventArgs e )
        {
            BindGroupMembersGrid( !gGroupMembers.AllowPaging );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            cpCampusFilter.Campuses = CampusCache.All();

            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            cpCampusFilter.SelectedCampusId = rFilter.GetUserPreference( "Campus" ).AsIntegerOrNull();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid( bool selectAll = false )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var rootGroupGuid = GetAttributeValue( "RootGroup" ).AsGuidOrNull();
            if ( rootGroupGuid != null )
            {
                var rootGroup = groupService.Get( rootGroupGuid.Value );
                if ( rootGroup != null )
                {
                    pnlGroupMembers.Visible = true;

                    nbRoleWarning.Visible = false;
                    rFilter.Visible = true;
                    gGroupMembers.Visible = true;

                    //GetGroups
                    List<GroupTreeItem> groupQry = groupService.GetAllDescendents( rootGroup.Id )
                        .Select( g => new GroupTreeItem { Id = g.Id, ParentGroupId = g.ParentGroupId, Name = g.Name } )
                        .ToList();
                    groupQry.Add( new GroupTreeItem { Id = rootGroup.Id, ParentGroupId = rootGroup.ParentGroupId, Name = rootGroup.Name } );

                    List<int> groupIdList = groupQry.Select( g => g.Id ).ToList();
                    List<Guid> groupTypeIncludeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();

                    // Get Group Members
                    var qry = groupMemberService.Queryable( "Person,GroupRole", true ).AsNoTracking()
                        .Where( m =>
                            groupIdList.Contains( m.GroupId ) &&
                            groupTypeIncludeGuids.Contains( m.Group.GroupType.Guid )
                            );

                    // Filter by First Name
                    string firstName = tbFirstName.Text;
                    if ( !string.IsNullOrWhiteSpace( firstName ) )
                    {
                        qry = qry.Where( m => m.Person.FirstName.StartsWith( firstName ) );
                    }

                    // Filter by Last Name
                    string lastName = tbLastName.Text;
                    if ( !string.IsNullOrWhiteSpace( lastName ) )
                    {
                        qry = qry.Where( m => m.Person.LastName.StartsWith( lastName ) );
                    }

                    // Filter by Pending Status
                    qry = qry.Where( m => m.GroupMemberStatus == GroupMemberStatus.Pending );

                    // Filter by Campus
                    if ( cpCampusFilter.SelectedCampusId.HasValue )
                    {
                        Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                        int campusId = cpCampusFilter.SelectedCampusId.Value;
                        var qryFamilyMembersForCampus = new GroupMemberService( rockContext ).Queryable().Where( a => a.Group.GroupType.Guid == familyGuid && a.Group.CampusId == campusId );
                        qry = qry.Where( a => qryFamilyMembersForCampus.Any( f => f.PersonId == a.PersonId ) );
                    }

                    List<GroupMember> groupMembersList = qry.ToList();

                    gGroupMembers.DataSource = groupMembersList.Select( m => new
                    {
                        m.Id,
                        m.Guid,
                        m.PersonId,
                        GroupTree = GetGroupTree( groupQry, groupQry.Where( g => g.Id == m.GroupId ).FirstOrDefault(), rootGroup ),
                        Name = GetMemberDetailLink( m.Id, m.Person.FullName ),
                        LastName = m.Person.LastName,
                        GroupRole = m.GroupRole.Name,
                        m.GroupMemberStatus,
                        RecordStatusValueId = m.Person.RecordStatusValueId,
                    } )
                    .OrderBy( m => m.GroupTree ).ThenBy( m => m.LastName )
                    .ToList();
                    gGroupMembers.DataBind();
                }
                else
                {
                    nbRoleWarning.Text = "Please provide a valid root group.";
                    nbRoleWarning.Visible = true;
                    pnlGroupMembers.Visible = false;
                }
            }
            else
            {
                nbRoleWarning.Text = "Please provide a root group.";
                nbRoleWarning.Visible = true;
                pnlGroupMembers.Visible = false;
            }
        }

        /// <summary>
        /// Gets the member detail link.
        /// </summary>
        /// <param name="memberId">The member identifier.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        private object GetMemberDetailLink( int memberId, string memberName )
        {
            string result = Server.HtmlEncode( memberName );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupMemberId", memberId.ToString() );
            result = string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( "GroupMemberDetailPage", queryParams ), result );
            return result;
        }

        /// <summary>
        /// Gets the group tree.
        /// </summary>
        /// <param name="groupQry">The group qry.</param>
        /// <param name="group">The group.</param>
        /// <param name="rootGroup">The root group.</param>
        /// <returns></returns>
        private string GetGroupTree( List<GroupTreeItem> groupQry, GroupTreeItem group, Group rootGroup )
        {
            string result = Server.HtmlEncode( group.Name );

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", group.Id.ToString() );
            result = string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( "GroupDetailPage", queryParams ), result );

            GroupTreeItem parentGroup = groupQry.Where( g => g.Id == group.ParentGroupId ).FirstOrDefault();
            while ( parentGroup != null )
            {
                queryParams = new Dictionary<string, string>();
                queryParams.Add( "GroupId", parentGroup.Id.ToString() );
                result = string.Format( "<a href='{0}'>{1}</a> > {2}", LinkedPageUrl( "GroupDetailPage", queryParams ), Server.HtmlEncode( parentGroup.Name ), result );
                parentGroup = groupQry.Where( g => g.Id == parentGroup.ParentGroupId ).FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        #endregion

        /// <summary>
        /// A helper class to quickly find parent groups
        /// </summary>
        class GroupTreeItem
        {
            public int Id { get; set; }

            public int? ParentGroupId { get; set; }

            public string Name { get; set; }
        }
    }
}