//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Crm
{
    [GroupTypesField( "Group Types", "Select group types to show in this block.  Leave all unchecked to show all group types.", false )]
    [BooleanField( "Show Edit", "", true )]
    [BooleanField( "Limit to Security Role Groups" )]
    public partial class GroupDetail : RockBlock, IDetailBlock
    {
        #region Child Grid Dictionarys

        /// <summary>
        /// Gets or sets the state of the group member attributes.
        /// </summary>
        /// <value>
        /// The state of the group member attributes.
        /// </value>
        private ViewStateList<Attribute> GroupMemberAttributesState
        {
            get
            {
                return ViewState["GroupMemberAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["GroupMemberAttributesState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroupMemberAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupMemberAttributes.Actions.ShowAdd = true;
            gGroupMemberAttributes.Actions.AddClick += gGroupMemberAttributes_Add;
            gGroupMemberAttributes.GridRebind += gGroupMemberAttributes_GridRebind;
            gGroupMemberAttributes.EmptyDataText = Server.HtmlEncode( None.Text );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return confirmDelete(event, '{0}');", Group.FriendlyTypeName );
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
                string itemId = PageParameter( "groupId" );
                string parentGroupId = PageParameter( "parentGroupId" );

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( parentGroupId ) )
                    {
                        ShowDetail( "groupId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "groupId", int.Parse( itemId ), int.Parse( parentGroupId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            if ( pnlDetails.Visible )
            {
                var group = new Group { GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0 };
                if ( group.GroupTypeId > 0 )
                {
                    group.LoadAttributes();
                    phGroupAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, false );
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupId.Value.Equals( "0" ) )
            {
                if ( this.CurrentPage.Layout.Equals( "TwoColumnLeft" ) )
                {
                    // Cancelling on Add.  Return to tree view with parent category selected
                    var qryParams = new Dictionary<string, string>();

                    string parentGroupId = PageParameter( "parentGroupId" );
                    if ( !string.IsNullOrWhiteSpace( parentGroupId ) )
                    {
                        qryParams["groupId"] = parentGroupId;
                    }

                    NavigateToPage( this.CurrentPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                GroupService groupService = new GroupService();
                Group group = groupService.Get( int.Parse( hfGroupId.Value ) );
                ShowReadonlyDetails( group );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            GroupService groupService = new GroupService();
            Group group = groupService.Get( int.Parse( hfGroupId.Value ) );
            ShowEditDetails( group );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? parentGroupId = null;

            // NOTE: Very similar code in GroupList.gGroups_Delete
            RockTransactionScope.WrapTransaction( () =>
            {
                GroupService groupService = new GroupService();
                AuthService authService = new AuthService();
                Group group = groupService.Get( int.Parse( hfGroupId.Value ) );

                if ( group != null )
                {
                    parentGroupId = group.ParentGroupId;
                    string errorMessage;
                    if ( !groupService.CanDelete( group, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    bool isSecurityRoleGroup = group.IsSecurityRole;
                    if ( isSecurityRoleGroup )
                    {
                        foreach ( var auth in authService.Queryable().Where( a => a.GroupId.Equals( group.Id ) ).ToList() )
                        {
                            authService.Delete( auth, CurrentPersonId );
                            authService.Save( auth, CurrentPersonId );
                        }
                    }

                    groupService.Delete( group, CurrentPersonId );
                    groupService.Save( group, CurrentPersonId );

                    if ( isSecurityRoleGroup )
                    {
                        Rock.Security.Authorization.Flush();
                        Rock.Security.Role.Flush( group.Id );
                    }
                }
            } );

            // reload page, selecting the deleted group's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentGroupId != null )
            {
                qryParams["groupId"] = parentGroupId.ToString();
            }

            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.DimOtherBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Group group;
            bool wasSecurityRole = false;

            using ( new UnitOfWorkScope() )
            {
                GroupService groupService = new GroupService();
                AttributeService attributeService = new AttributeService();

                int groupId = int.Parse( hfGroupId.Value );

                if ( groupId == 0 )
                {
                    group = new Group();
                    group.IsSystem = false;
                    group.Name = string.Empty;
                }
                else
                {
                    group = groupService.Get( groupId );
                    wasSecurityRole = group.IsSecurityRole;
                }

                if ( ( ddlGroupType.SelectedValueAsInt() ?? 0 ) == 0 )
                {
                    ddlGroupType.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( GroupType.FriendlyTypeName ) );
                    return;
                }

                group.Name = tbName.Text;
                group.Description = tbDescription.Text;
                group.CampusId = ddlCampus.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlCampus.SelectedValue );
                group.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
                group.ParentGroupId = gpParentGroup.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( gpParentGroup.SelectedValue );
                group.IsSecurityRole = cbIsSecurityRole.Checked;
                group.IsActive = cbIsActive.Checked;

                if ( group.ParentGroupId == group.Id )
                {
                    gpParentGroup.ShowErrorMessage( "Group cannot be a Parent Group of itself." );
                    return;
                }

                group.LoadAttributes();

                Rock.Attribute.Helper.GetEditValues( phGroupAttributes, group );

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !group.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                RockTransactionScope.WrapTransaction( () =>
                {
                    if ( group.Id.Equals( 0 ) )
                    {
                        groupService.Add( group, CurrentPersonId );
                    }

                    groupService.Save( group, CurrentPersonId );
                    Rock.Attribute.Helper.SaveAttributeValues( group, CurrentPersonId );

                    /* Take care of Group Member Attributes */

                    // delete GroupMemberAttributes that are no longer configured in the UI
                    string qualifierValue = group.Id.ToString();
                    var groupMemberAttributesQry = attributeService.GetByEntityTypeId( new GroupMember().TypeId ).AsQueryable()
                        .Where( a => a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase )
                        && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

                    var deletedGroupMemberAttributes = from attr in groupMemberAttributesQry
                                                       where !( from d in GroupMemberAttributesState
                                                                select d.Guid ).Contains( attr.Guid )
                                                       select attr;

                    deletedGroupMemberAttributes.ToList().ForEach( a =>
                    {
                        var attr = attributeService.Get( a.Guid );
                        Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                        attributeService.Delete( attr, CurrentPersonId );
                        attributeService.Save( attr, CurrentPersonId );
                    } );

                    // add/update the GroupMemberAttributes that are assigned in the UI
                    foreach ( var attributeState in GroupMemberAttributesState )
                    {
                        // remove old qualifiers in case they changed
                        var qualifierService = new AttributeQualifierService();
                        foreach ( var oldQualifier in qualifierService.GetByAttributeId( attributeState.Id ).ToList() )
                        {
                            qualifierService.Delete( oldQualifier, CurrentPersonId );
                            qualifierService.Save( oldQualifier, CurrentPersonId );
                        }

                        Attribute attribute = groupMemberAttributesQry.FirstOrDefault( a => a.Guid.Equals( attributeState.Guid ) );
                        if ( attribute == null )
                        {
                            attribute = attributeState.Clone() as Rock.Model.Attribute;
                            attributeService.Add( attribute, CurrentPersonId );
                        }
                        else
                        {
                            attributeState.Id = attribute.Id;
                            attribute.FromDictionary( attributeState.ToDictionary() );

                            foreach ( var qualifier in attributeState.AttributeQualifiers )
                            {
                                attribute.AttributeQualifiers.Add( qualifier.Clone() as AttributeQualifier );
                            }

                        }

                        attribute.EntityTypeQualifierColumn = "GroupId";
                        attribute.EntityTypeQualifierValue = group.Id.ToString();
                        attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( GroupMember ) ).Id;
                        Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                        attributeService.Save( attribute, CurrentPersonId );
                    }
                } );

            }

            if ( group != null && wasSecurityRole )
            {
                if ( !group.IsSecurityRole )
                {
                    // if this group was a SecurityRole, but no longer is, flush
                    Rock.Security.Role.Flush( group.Id );
                    Rock.Security.Authorization.Flush();
                }
            }
            else
            {
                if ( group.IsSecurityRole )
                {
                    // new security role, flush
                    Rock.Security.Authorization.Flush();
                }
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["groupId"] = group.Id.ToString();

            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the number of active members in a group role.
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="roleId">The role id.</param>
        /// <returns></returns>
        private int GetGroupRoleMemberCount( int groupId, int roleId )
        {
            return new GroupMemberService().Queryable()
                        .Where( m => m.GroupId == groupId )
                        .Where( m => m.GroupRoleId == roleId )
                        .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Count();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            CampusService campusService = new CampusService();
            List<Campus> campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList();
            campuses.Insert( 0, new Campus { Id = None.Id, Name = None.Text } );
            ddlCampus.DataSource = campuses;
            ddlCampus.DataBind();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlParentGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlParentGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            var groupTypeQry = groupTypeService.Queryable();

            // limit GroupType selection to what Block Attributes allow
            List<Guid> groupTypeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( groupTypeGuids.Count > 0 )
            {
                groupTypeQry = groupTypeQry.Where( a => groupTypeGuids.Contains( a.Guid ) );
            }

            // next, limit GroupType to ChildGroupTypes that the ParentGroup allows
            int? parentGroupId = gpParentGroup.SelectedValueAsInt();
            if ( ( parentGroupId ?? 0 ) != 0 )
            {
                Group parentGroup = new GroupService().Get( parentGroupId.Value );
                List<int> allowedChildGroupTypeIds = parentGroup.GroupType.ChildGroupTypes.Select( a => a.Id ).ToList();
                groupTypeQry = groupTypeQry.Where( a => allowedChildGroupTypeIds.Contains( a.Id ) );
            }

            List<GroupType> groupTypes = groupTypeQry.OrderBy( a => a.Name ).ToList();
            if ( groupTypes.Count() > 1 )
            {
                // add a empty option so they are forced to choose
                groupTypes.Insert( 0, new GroupType { Id = 0, Name = string.Empty } );
            }

            // If the currently selected GroupType isn't an option anymore, set selected GroupType to null
            if ( ddlGroupType.SelectedValue != null )
            {
                int? selectedGroupTypeId = ddlGroupType.SelectedValueAsInt();
                if ( !groupTypes.Any( a => a.Id.Equals( selectedGroupTypeId ?? 0 ) ) )
                {
                    ddlGroupType.SelectedValue = null;
                }
            }

            ddlGroupType.DataSource = groupTypes;
            ddlGroupType.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The group id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? parentGroupId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "groupId" ) )
            {
                return;
            }

            Group group = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                group = new GroupService().Get( itemKeyValue );
            }
            else
            {
                group = new Group { Id = 0, IsActive = true, ParentGroupId = parentGroupId };
            }

            if ( group == null ) 
            {
                return;
            }

            pnlDetails.Visible = true;
            hfGroupId.Value = group.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( group.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
            }

            bool roleBelowMimumumMembership = false;

            if ( bool.TryParse( PageParameter( "roleBelowMin" ), out roleBelowMimumumMembership ) )
            {
                int roleId = 0;

                int.TryParse( PageParameter( "roleId" ), out roleId );

                if ( roleBelowMimumumMembership && roleId > 0 )
                {
                    GroupRole role = new GroupRoleService().Get( roleId );

                    if(role.MinCount != null)
                    {
                        int groupRoleMemberCount = GetGroupRoleMemberCount( itemKeyValue, roleId );

                        string minumumMemberText =  string.Format( "The {0} role is currently below it's minimum active membership requirement of {1} {2}.", 
                                                        role.Name,
                                                        role.MinCount,
                                                        role.MinCount == 1 ? "member" : "members"
                                                        );

                        if ( nbEditModeMessage.Text.Length > 0 )
                        {
                            nbEditModeMessage.Text = nbEditModeMessage.Text + " <br> " + minumumMemberText;
                        }
                        else
                        {
                            nbEditModeMessage.Text = "INFO: " + minumumMemberText;
                        }
                    }
                }
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( group );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                if ( group.Id > 0 )
                {
                    ShowReadonlyDetails( group );
                }
                else
                {
                    ShowEditDetails( group );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowEditDetails( Group group )
        {

            if (group.Id == 0)
            {
                lReadOnlyTitle.Text = ActionTitle.Add(Group.FriendlyTypeName).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();
            }
                       
            SetEditMode( true );

            tbName.Text = group.Name;
            tbDescription.Text = group.Description;
            cbIsSecurityRole.Checked = group.IsSecurityRole;
            cbIsActive.Checked = group.IsActive;

            LoadDropDowns();

            GroupMemberAttributesState = new ViewStateList<Attribute>();

            gpParentGroup.SetValue( group.ParentGroup ?? new GroupService().Get( group.ParentGroupId ?? 0 ) );

            // GroupType depends on Selected ParentGroup
            ddlParentGroup_SelectedIndexChanged( null, null );
            gpParentGroup.Label = "Parent Group";

            if ( group.Id == 0 && ddlGroupType.Items.Count > 1 )
            {
                if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
                {
                    // default GroupType for new Group to "Security Roles"  if LimittoSecurityRoleGroups
                    var securityRoleGroupType = new GroupTypeService().Queryable().FirstOrDefault( a => a.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE ) ) );
                    if ( securityRoleGroupType != null )
                    {
                        ddlGroupType.SetValue( securityRoleGroupType.Id );
                    }
                    else
                    {
                        ddlGroupType.SelectedIndex = 0;
                    }
                }
                else
                {
                    // if this is a new group (and not "LimitToSecurityRoleGroups", and there is more than one choice for GroupType, default to no selection so they are forced to choose (vs unintentionallly choosing the default one)
                    ddlGroupType.SelectedIndex = 0;
                }
            }
            else
            {
                ddlGroupType.SetValue( group.GroupTypeId );
            }

            ddlCampus.SetValue( group.CampusId );

            phGroupTypeAttributes.Controls.Clear();
            GroupType groupType = new GroupTypeService().Get( group.GroupTypeId );
            if ( groupType != null )
            {
                groupType.LoadAttributes();
                Rock.Attribute.Helper.AddDisplayControls( groupType, phGroupTypeAttributes );
            }

            phGroupAttributes.Controls.Clear();
            group.LoadAttributes();
            Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, true );

            // if this block's attribute limit group to SecurityRoleGroups, don't let them edit the SecurityRole checkbox value
            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                cbIsSecurityRole.Enabled = false;
                cbIsSecurityRole.Checked = true;
            }

            AttributeService attributeService = new AttributeService();

            string qualifierValue = group.Id.ToString();
            var qryGroupMemberAttributes = attributeService.GetByEntityTypeId( new GroupMember().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            GroupMemberAttributesState.AddAll( qryGroupMemberAttributes.ToList() );
            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Group group )
        {
            SetEditMode( false );

            string groupIconHtml = string.Empty;
            if ( !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) )
            {
                groupIconHtml = string.Format( "<i class='{0} icon-large' ></i>", group.GroupType.IconCssClass );
            }
            else
            {
                var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                string imageUrlFormat = "<img src='" + appPath + "GetImage.ashx?id={0}&width=50&height=50' />";
                if ( group.GroupType.IconLargeFileId != null )
                {
                    groupIconHtml = string.Format( imageUrlFormat, group.GroupType.IconLargeFileId );
                }
                else if ( group.GroupType.IconSmallFileId != null )
                {
                    groupIconHtml = string.Format( imageUrlFormat, group.GroupType.IconSmallFileId );
                }
            }

            hfGroupId.SetValue( group.Id );
            lGroupIconHtml.Text = groupIconHtml;
            lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();
            
            hlInactive.Visible = !group.IsActive;
            hlType.Text = group.GroupType.Name;

            lGroupDescription.Text = group.Description;
            
            DescriptionList descriptionList = new DescriptionList();

            if ( group.ParentGroup != null )
            {
                descriptionList.Add( "Parent Group", group.ParentGroup.Name );
            }

            if (group.Campus != null)
            {
                hlCampus.Visible = true;
                hlCampus.Text = group.Campus.Name;
            }
            else
            {
                hlCampus.Visible = false;
            }

            lblMainDetails.Text = descriptionList.Html;

            GroupType groupType = new GroupTypeService().Get( group.GroupTypeId );
            groupType.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( groupType, phGroupTypeAttributesReadOnly );

            group.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( group, phGroupAttributesReadOnly );
        }

        #endregion

        #region GroupMemberAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_Add( object sender, EventArgs e )
        {
            gGroupMemberAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gGroupMemberAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group member attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupMemberAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlGroupMemberAttribute.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                edtGroupMemberAttributes.ActionTitle = ActionTitle.Add( "attribute for group members of " + tbName.Text );
            }
            else
            {
                attribute = GroupMemberAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupMemberAttributes.ActionTitle = ActionTitle.Edit( "attribute for group members of " + tbName.Text );
            }

            edtGroupMemberAttributes.SetAttributeProperties( attribute, typeof( GroupMember ) );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            GroupMemberAttributesState.RemoveEntity( attributeGuid );

            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_GridRebind( object sender, EventArgs e )
        {
            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveGroupMemberAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveGroupMemberAttribute_Click( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtGroupMemberAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            GroupMemberAttributesState.RemoveEntity( attribute.Guid );
            GroupMemberAttributesState.Add( attribute );

            pnlDetails.Visible = true;
            pnlGroupMemberAttribute.Visible = false;

            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelGroupMemberAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelGroupMemberAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlGroupMemberAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the group member attributes grid.
        /// </summary>
        private void BindGroupMemberAttributesGrid()
        {
            gGroupMemberAttributes.DataSource = GroupMemberAttributesState.OrderBy( a => a.Name ).ToList();
            gGroupMemberAttributes.DataBind();
        }

        #endregion
    }
}