// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

    [GroupTypeField( "Group Type", "Pending members from groups of this grouptype will be displayed", true )]
    [GroupField( "Root Group", "Pending members from groups under this group will be displayed", true )]
    [LinkedPage( "Group Member Detail Page" )]
    [LinkedPage( "Group Detail Page" )]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 2, "PersonProfilePage" )]
    public partial class PendingGroupMemberList : RockBlock
    {
        #region Private Variables

        private bool _canView = false;
        private GroupType _groupType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

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

            if ( _groupType == null )
            {
                var groupTypeGuid = GetAttributeValue( "GroupType" ).AsGuidOrNull();
                if ( groupTypeGuid != null )
                {
                    _groupType = new GroupTypeService( new RockContext() ).Get( groupTypeGuid.Value );
                }
            }

            if ( IsUserAuthorized( Authorization.VIEW ) )
            {
                _canView = true;

                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                gGroupMembers.DataKeyNames = new string[] { "Id" };
                gGroupMembers.CommunicateMergeFields = new List<string> { "GroupRole" };
                gGroupMembers.PersonIdField = "PersonId";
                gGroupMembers.GridRebind += gGroupMembers_GridRebind;
                if ( _groupType != null )
                {
                    gGroupMembers.RowItemText = _groupType.GroupTerm + " " + _groupType.GroupMemberTerm;
                }

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

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
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
            rFilter.SaveUserPreference( "Role", "Role", cblRole.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "Campus", "Campus", cpCampusFilter.SelectedCampusId.ToString() );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            rFilter.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => a.Key == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            if ( e.Key == "First Name" )
            {
                return;
            }
            else if ( e.Key == "Last Name" )
            {
                return;
            }
            else if ( e.Key == "Role" )
            {
                e.Value = ResolveValues( e.Value, cblRole );
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
            if ( _groupType != null )
            {
                cblRole.DataSource = _groupType.Roles.OrderBy( a => a.Order ).ToList();
                cblRole.DataBind();
            }

            cpCampusFilter.Campuses = CampusCache.All();

            BindAttributes();
            AddDynamicControls();

            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            cpCampusFilter.SelectedCampusId = rFilter.GetUserPreference( "Campus" ).AsIntegerOrNull();

            string roleValue = rFilter.GetUserPreference( "Role" );
            if ( !string.IsNullOrWhiteSpace( roleValue ) )
            {
                cblRole.SetValues( roleValue.Split( ';' ).ToList() );
            }
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();
            if ( _groupType != null )
            {
                int entityTypeId = new GroupMember().TypeId;
                string groupTypeQualifier = _groupType.Id.ToString();
                foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        ( a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) && a.EntityTypeQualifierValue.Equals( groupTypeQualifier ) ) )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Read( attributeModel ) );
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gGroupMembers.Columns.OfType<AttributeField>().ToList() )
            {
                gGroupMembers.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = (IRockControl)control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = rFilter.GetUserPreference( attribute.Key );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                                // intentionally ignore
                            }
                        }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gGroupMembers.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gGroupMembers.Columns.Add( boundField );
                    }
                }
            }
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
                    if ( _groupType == null )
                    {
                        var groupTypeGuid = GetAttributeValue( "GroupType" ).AsGuidOrNull();
                        if ( groupTypeGuid != null )
                        {
                            _groupType = new GroupTypeService( rockContext ).Get( groupTypeGuid.Value );
                        }
                    }

                    if ( _groupType != null )
                    {
                        pnlGroupMembers.Visible = true;
                        lHeading.Text = string.Format( "Pending {0} {1}", _groupType.GroupTerm, _groupType.GroupMemberTerm.Pluralize() );

                        if ( _groupType.Roles.Any() )
                        {
                            nbRoleWarning.Visible = false;
                            rFilter.Visible = true;
                            gGroupMembers.Visible = true;

                            //GetGroups
                            List<GroupTreeItem> groupQry = groupService.GetAllDescendents( rootGroup.Id )
                                .Select( g => new GroupTreeItem { Id = g.Id, ParentGroupId = g.ParentGroupId, Name = g.Name } )
                                .ToList();
                            groupQry.Add( new GroupTreeItem { Id = rootGroup.Id, ParentGroupId = rootGroup.ParentGroupId, Name = rootGroup.Name } );

                            List<int> groupIdList = groupQry.Select( g => g.Id ).ToList();

                            // Get Group Members
                            var qry = groupMemberService.Queryable( "Person,GroupRole", true ).AsNoTracking()
                                .Where( m =>
                                    groupIdList.Contains( m.GroupId ) &&
                                    m.Group.GroupTypeId == _groupType.Id );

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

                            // Filter by role
                            var validGroupTypeRoles = _groupType.Roles.Select( r => r.Id ).ToList();
                            var roles = new List<int>();
                            foreach ( string role in cblRole.SelectedValues )
                            {
                                if ( !string.IsNullOrWhiteSpace( role ) )
                                {
                                    int roleId = int.MinValue;
                                    if ( int.TryParse( role, out roleId ) && validGroupTypeRoles.Contains( roleId ) )
                                    {
                                        roles.Add( roleId );
                                    }
                                }
                            }

                            if ( roles.Any() )
                            {
                                qry = qry.Where( m => roles.Contains( m.GroupRoleId ) );
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

                            // Filter query by any configured attribute filters
                            if ( AvailableAttributes != null && AvailableAttributes.Any() )
                            {
                                var attributeValueService = new AttributeValueService( rockContext );
                                var parameterExpression = attributeValueService.ParameterExpression;

                                foreach ( var attribute in AvailableAttributes )
                                {
                                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                                    if ( filterControl != null )
                                    {
                                        var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                        var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                                        if ( expression != null )
                                        {
                                            var attributeValues = attributeValueService
                                                .Queryable()
                                                .Where( v => v.Attribute.Id == attribute.Id );

                                            attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                            qry = qry.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                                        }
                                    }
                                }
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
                            nbRoleWarning.Text = string.Format(
                                "{0} cannot be added to this {1} because the '{2}' group type does not have any roles defined.",
                                _groupType.GroupMemberTerm.Pluralize(),
                                _groupType.GroupTerm,
                                _groupType.Name );

                            nbRoleWarning.Visible = true;
                            rFilter.Visible = false;
                            gGroupMembers.Visible = false;
                        }
                    }
                    else
                    {
                        nbRoleWarning.Text = "Please provide a group type.";
                        nbRoleWarning.Visible = true;
                        pnlGroupMembers.Visible = false;
                    }
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