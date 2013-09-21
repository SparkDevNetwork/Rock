//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Crm
{
    public partial class GroupTypes : RockBlock, IDetailBlock
    {
        #region Child Grid Dictionarys

        /// <summary>
        /// Gets the child group types dictionary.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> ChildGroupTypesDictionary
        {
            get
            {
                Dictionary<int, string> childGroupTypesDictionary = ViewState["ChildGroupTypesDictionary"] as Dictionary<int, string>;
                return childGroupTypesDictionary;
            }

            set
            {
                ViewState["ChildGroupTypesDictionary"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the location types dictionary.
        /// </summary>
        /// <value>
        /// The location types dictionary.
        /// </value>
        private Dictionary<int, string> LocationTypesDictionary
        {
            get
            {
                Dictionary<int, string> locationTypesDictionary = ViewState["LocationTypesDictionary"] as Dictionary<int, string>;
                return locationTypesDictionary;
            }

            set
            {
                ViewState["LocationTypesDictionary"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of the group type attributes.
        /// </summary>
        /// <value>
        /// The state of the group type attributes.
        /// </value>
        private ViewStateList<Attribute> GroupTypeAttributesState
        {
            get
            {
                return ViewState["GroupTypeAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["GroupTypeAttributesState"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of the group attributes.
        /// </summary>
        /// <value>
        /// The state of the group attributes.
        /// </value>
        private ViewStateList<Attribute> GroupAttributesState
        {
            get
            {
                return ViewState["GroupAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["GroupAttributesState"] = value;
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

            gChildGroupTypes.DataKeyNames = new string[] { "key" };
            gChildGroupTypes.Actions.ShowAdd = true;
            gChildGroupTypes.Actions.AddClick += gChildGroupTypes_Add;
            gChildGroupTypes.GridRebind += gChildGroupTypes_GridRebind;
            gChildGroupTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gLocationTypes.DataKeyNames = new string[] { "key" };
            gLocationTypes.Actions.ShowAdd = true;
            gLocationTypes.Actions.AddClick += gLocationTypes_Add;
            gLocationTypes.GridRebind += gLocationTypes_GridRebind;
            gLocationTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gGroupTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupTypeAttributes.Actions.ShowAdd = true;
            gGroupTypeAttributes.Actions.AddClick += gGroupTypeAttributes_Add;
            gGroupTypeAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupTypeAttributes.RowDataBound += gAttributes_RowDataBound;
            gGroupTypeAttributes.GridRebind += gGroupTypeAttributes_GridRebind;

            gGroupAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupAttributes.Actions.ShowAdd = true;
            gGroupAttributes.Actions.AddClick += gGroupAttributes_Add;
            gGroupAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupAttributes.RowDataBound += gAttributes_RowDataBound;
            gGroupAttributes.GridRebind += gGroupAttributes_GridRebind;
              
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
                string itemId = PageParameter( "groupTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "groupTypeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns(int? groupTypeId)
        {
            GroupRoleService groupRoleService = new GroupRoleService();
            List<GroupRole> groupRoles = groupRoleService.Queryable().Where(a => a.GroupTypeId == groupTypeId).OrderBy( a => a.Name ).ToList();
            groupRoles.Insert( 0, new GroupRole { Id = None.Id, Name = None.Text } );
            ddlDefaultGroupRole.DataSource = groupRoles;
            ddlDefaultGroupRole.DataBind();

            ddlAttendanceRule.BindToEnum( typeof( Rock.Model.AttendanceRule ) );
            ddlAttendancePrintTo.BindToEnum( typeof( Rock.Model.PrintTo ) );
            ddlLocationSelectionMode.BindToEnum( typeof( Rock.Model.LocationPickerMode ) );
            gtpInheritedGroupType.GroupTypes = new GroupTypeService().Queryable()
                .Where( g => g.Id != groupTypeId)
                .ToList();

            var groupTypePurposeList = new DefinedValueService().GetByDefinedTypeGuid(new Guid(Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE)).OrderBy(a => a.Name).ToList();
            
            ddlGroupTypePurpose.Items.Clear();
            ddlGroupTypePurpose.Items.Add( Rock.Constants.None.ListItem );
            foreach ( var item in groupTypePurposeList )
            {
                ddlGroupTypePurpose.Items.Add(new ListItem(item.Name, item.Id.ToString()));
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "groupTypeId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            GroupType groupType = null;

            using ( new UnitOfWorkScope() )
            {
                var groupTypeService = new GroupTypeService();
                var attributeService = new AttributeService();

                if ( !itemKeyValue.Equals( 0 ) )
                {
                    groupType = groupTypeService.Get( itemKeyValue );
                    lActionTitle.Text = ActionTitle.Edit( GroupType.FriendlyTypeName );
                }
                else
                {
                    groupType = new GroupType { Id = 0, ShowInGroupList = true };
                    groupType.ChildGroupTypes = new List<GroupType>();
                    groupType.LocationTypes = new List<GroupTypeLocationType>();
                    lActionTitle.Text = ActionTitle.Add( GroupType.FriendlyTypeName );
                }

                LoadDropDowns(groupType.Id);

                ChildGroupTypesDictionary = new Dictionary<int, string>();
                LocationTypesDictionary = new Dictionary<int, string>();
                GroupTypeAttributesState = new ViewStateList<Attribute>();
                GroupAttributesState = new ViewStateList<Attribute>();

                hfGroupTypeId.Value = groupType.Id.ToString();
                tbName.Text = groupType.Name;
                tbDescription.Text = groupType.Description;
                tbGroupTerm.Text = groupType.GroupTerm;
                tbGroupMemberTerm.Text = groupType.GroupMemberTerm;
                ddlDefaultGroupRole.SetValue( groupType.DefaultGroupRoleId );
                cbShowInGroupList.Checked = groupType.ShowInGroupList;
                cbShowInNavigation.Checked = groupType.ShowInNavigation;
                tbIconCssClass.Text = groupType.IconCssClass;
                imgIconSmall.BinaryFileId = groupType.IconSmallFileId;
                imgIconLarge.BinaryFileId = groupType.IconLargeFileId;

                cbTakesAttendance.Checked = groupType.TakesAttendance;
                ddlAttendanceRule.SetValue( (int)groupType.AttendanceRule );
                ddlAttendancePrintTo.SetValue( (int)groupType.AttendancePrintTo );
                ddlLocationSelectionMode.SetValue( (int)groupType.LocationSelectionMode );
                ddlGroupTypePurpose.SetValue( groupType.GroupTypePurposeValueId );
                cbAllowMultipleLocations.Checked = groupType.AllowMultipleLocations;
                gtpInheritedGroupType.SelectedGroupTypeId = groupType.InheritedGroupTypeId;
                groupType.ChildGroupTypes.ToList().ForEach( a => ChildGroupTypesDictionary.Add( a.Id, a.Name ) );
                groupType.LocationTypes.ToList().ForEach( a => LocationTypesDictionary.Add( a.LocationTypeValueId, a.LocationTypeValue.Name ) );

                string qualifierValue = groupType.Id.ToString();
                var qryGroupTypeAttributes = attributeService.GetByEntityTypeId( new GroupType().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) );

                var qryGroupAttributes = attributeService.GetByEntityTypeId( new Group().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) );

                GroupTypeAttributesState.AddAll( qryGroupTypeAttributes
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList() );

                GroupAttributesState.AddAll( qryGroupAttributes
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList() );

                if ( groupType.InheritedGroupTypeId.HasValue )
                {
                    RebuildAttributeLists( groupType.InheritedGroupTypeId, groupTypeService, attributeService, false );
                }
            }

            BindGroupTypeAttributesGrid();
            BindGroupAttributesGrid();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupType.FriendlyTypeName );
            }

            if ( groupType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( GroupType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( GroupType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            ddlDefaultGroupRole.Enabled = !readOnly;
            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbGroupTerm.ReadOnly = readOnly;
            tbGroupMemberTerm.ReadOnly = readOnly;
            cbShowInGroupList.Enabled = !readOnly;
            cbShowInNavigation.Enabled = !readOnly;
            tbIconCssClass.ReadOnly = readOnly;
            imgIconLarge.Enabled = !readOnly;
            imgIconSmall.Enabled = !readOnly;
            cbTakesAttendance.Enabled = !readOnly;
            ddlAttendanceRule.Enabled = !readOnly;
            ddlAttendancePrintTo.Enabled = !readOnly;
            ddlLocationSelectionMode.Enabled = !readOnly;
            ddlGroupTypePurpose.Enabled = !readOnly;
            cbAllowMultipleLocations.Enabled = !readOnly;
            gtpInheritedGroupType.Enabled = !readOnly;
            gGroupTypeAttributes.Enabled = !readOnly;
            gGroupAttributes.Enabled = !readOnly;

            gChildGroupTypes.Enabled = !readOnly;
            gLocationTypes.Enabled = !readOnly;
            btnSave.Visible = !readOnly;

            BindChildGroupTypesGrid();
            BindLocationTypesGrid();
        }

        private void RebuildAttributeLists( int? inheritedGroupTypeId, GroupTypeService groupTypeService, AttributeService attributeService,
            bool clearList )
        {
            string qualifierValue = PageParameter( "GroupTypeId" );

            if ( clearList )
            {
                var attributes = new List<Attribute>();
                foreach ( var attribute in GroupTypeAttributesState )
                {
                    if ( string.IsNullOrWhiteSpace( attribute.EntityTypeQualifierValue ) ||
                        attribute.EntityTypeQualifierValue == qualifierValue )
                    {
                        attributes.Add( attribute );
                    }
                }
                GroupTypeAttributesState.Clear();
                GroupTypeAttributesState.AddAll( attributes );

                attributes = new List<Attribute>();
                foreach ( var attribute in GroupAttributesState )
                {
                    if ( string.IsNullOrWhiteSpace( attribute.EntityTypeQualifierValue ) ||
                        attribute.EntityTypeQualifierValue == qualifierValue )
                    {
                        attributes.Add( attribute );
                    }
                }
                GroupAttributesState.Clear();
                GroupAttributesState.AddAll( attributes );
            }

            while ( inheritedGroupTypeId.HasValue )
            {
                var inheritedGroupType = groupTypeService.Get( inheritedGroupTypeId.Value );
                if ( inheritedGroupType != null )
                {
                    qualifierValue = inheritedGroupType.Id.ToString();

                    var qryGroupTypeAttributes = attributeService.GetByEntityTypeId( new GroupType().TypeId ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifierValue ) );

                    var qryGroupAttributes = attributeService.GetByEntityTypeId( new Group().TypeId ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifierValue ) );

                    // TODO: Should probably add GroupTypeCache object so that it can be used during the 
                    // databind of the attribute grids, instead of having to do this hack of putting 
                    // the inherited group type name into the description property of the attribute 
                    // (which is currently how the databound method gets the inherited attributes group type)
                    qryGroupTypeAttributes
                        .ToList()
                        .ForEach( a => a.Description = inheritedGroupType.Name );

                    qryGroupAttributes
                        .ToList()
                        .ForEach( a => a.Description = inheritedGroupType.Name );

                    GroupTypeAttributesState.InsertAll( qryGroupTypeAttributes
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() );

                    GroupAttributesState.InsertAll( qryGroupAttributes
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() );

                    inheritedGroupTypeId = inheritedGroupType.InheritedGroupTypeId;
                }
                else
                {
                    inheritedGroupTypeId = null;
                }

            }

        }

        #endregion

        #region Child GroupType Grid and Picker

        /// <summary>
        /// Handles the Add event of the gChildGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChildGroupTypes_Add( object sender, EventArgs e )
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            int currentGroupTypeId = int.Parse( hfGroupTypeId.Value );

            // populate dropdown with all grouptypes that aren't already childgroups
            var qry = from gt in groupTypeService.Queryable()
                      where !( from cgt in ChildGroupTypesDictionary.Keys
                               select cgt ).Contains( gt.Id )
                      select gt;

            List<GroupType> list = qry.ToList();
            if ( list.Count == 0 )
            {
                list.Add( new GroupType { Id = None.Id, Name = None.Text } );
                btnAddChildGroupType.Enabled = false;
                btnAddChildGroupType.CssClass = "btn btn-primary disabled";
            }
            else
            {
                btnAddChildGroupType.Enabled = true;
                btnAddChildGroupType.CssClass = "btn btn-primary";
            }

            ddlChildGroupType.DataSource = list;
            ddlChildGroupType.DataBind();
            pnlChildGroupTypePicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Delete event of the gChildGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gChildGroupTypes_Delete( object sender, RowEventArgs e )
        {
            int childGroupTypeId = (int)e.RowKeyValue;
            ChildGroupTypesDictionary.Remove( childGroupTypeId );
            BindChildGroupTypesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gChildGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChildGroupTypes_GridRebind( object sender, EventArgs e )
        {
            BindChildGroupTypesGrid();
        }

        /// <summary>
        /// Binds the child group types grid.
        /// </summary>
        private void BindChildGroupTypesGrid()
        {
            gChildGroupTypes.DataSource = ChildGroupTypesDictionary.OrderBy( a => a.Value ).ToList();
            gChildGroupTypes.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnAddChildGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAddChildGroupType_Click( object sender, EventArgs e )
        {
            ChildGroupTypesDictionary.Add( int.Parse( ddlChildGroupType.SelectedValue ), ddlChildGroupType.SelectedItem.Text );

            pnlChildGroupTypePicker.Visible = false;
            pnlDetails.Visible = true;

            BindChildGroupTypesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAddChildGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAddChildGroupType_Click( object sender, EventArgs e )
        {
            pnlChildGroupTypePicker.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion

        #region LocationType Grid and Picker

        /// <summary>
        /// Handles the Add event of the gLocationTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLocationTypes_Add( object sender, EventArgs e )
        {
            DefinedValueService definedValueService = new DefinedValueService();

            // populate dropdown with all locationtypes that aren't already locationtypes
            var qry = from dlt in definedValueService.GetByDefinedTypeGuid( new Guid( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE ) )
                      where !( from lt in LocationTypesDictionary.Keys
                               select lt ).Contains( dlt.Id )
                      select dlt;

            List<DefinedValue> list = qry.ToList();
            if ( list.Count == 0 )
            {
                list.Add( new DefinedValue { Id = None.Id, Name = None.Text } );
                btnAddLocationType.Enabled = false;
                btnAddLocationType.CssClass = "btn btn-primary disabled";
            }
            else
            {
                btnAddLocationType.Enabled = true;
                btnAddLocationType.CssClass = "btn btn-primary";
            }

            ddlLocationType.DataSource = list;
            ddlLocationType.DataBind();
            pnlLocationTypePicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Delete event of the gLocationTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLocationTypes_Delete( object sender, RowEventArgs e )
        {
            int locationTypeId = (int)e.RowKeyValue;
            LocationTypesDictionary.Remove( locationTypeId );
            BindLocationTypesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLocationTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLocationTypes_GridRebind( object sender, EventArgs e )
        {
            BindLocationTypesGrid();
        }

        /// <summary>
        /// Binds the location types grid.
        /// </summary>
        private void BindLocationTypesGrid()
        {
            gLocationTypes.DataSource = LocationTypesDictionary.OrderBy( a => a.Value ).ToList();
            gLocationTypes.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnAddLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAddLocationType_Click( object sender, EventArgs e )
        {
            LocationTypesDictionary.Add( int.Parse( ddlLocationType.SelectedValue ), ddlLocationType.SelectedItem.Text );

            pnlLocationTypePicker.Visible = false;
            pnlDetails.Visible = true;

            BindLocationTypesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAddLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAddLocationType_Click( object sender, EventArgs e )
        {
            pnlLocationTypePicker.Visible = false;
            pnlDetails.Visible = true;
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
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                GroupType groupType;
                GroupTypeService groupTypeService = new GroupTypeService();
                AttributeService attributeService = new AttributeService();

                int groupTypeId = int.Parse( hfGroupTypeId.Value );

                if ( groupTypeId == 0 )
                {
                    groupType = new GroupType();
                    groupTypeService.Add( groupType, CurrentPersonId );
                }
                else
                {
                    groupType = groupTypeService.Get( groupTypeId );
                }

                groupType.Name = tbName.Text;

                groupType.Description = tbDescription.Text;
                groupType.GroupTerm = tbGroupTerm.Text;
                groupType.GroupMemberTerm = tbGroupMemberTerm.Text;
                groupType.DefaultGroupRoleId = ddlDefaultGroupRole.SelectedValueAsInt();
                groupType.ShowInGroupList = cbShowInGroupList.Checked;
                groupType.ShowInNavigation = cbShowInNavigation.Checked;
                groupType.IconCssClass = tbIconCssClass.Text;
                groupType.IconSmallFileId = imgIconSmall.BinaryFileId;
                groupType.IconLargeFileId = imgIconLarge.BinaryFileId;
                groupType.TakesAttendance = cbTakesAttendance.Checked;
                groupType.AttendanceRule = ddlAttendanceRule.SelectedValueAsEnum<AttendanceRule>();
                groupType.AttendancePrintTo = ddlAttendancePrintTo.SelectedValueAsEnum<PrintTo>();
                groupType.LocationSelectionMode = ddlLocationSelectionMode.SelectedValueAsEnum<LocationPickerMode>();
                groupType.GroupTypePurposeValueId = ddlGroupTypePurpose.SelectedValueAsInt();
                groupType.AllowMultipleLocations = cbAllowMultipleLocations.Checked;
                groupType.InheritedGroupTypeId = gtpInheritedGroupType.SelectedGroupTypeId;

                groupType.ChildGroupTypes = new List<GroupType>();
                groupType.ChildGroupTypes.Clear();
                foreach ( var item in ChildGroupTypesDictionary )
                {
                    var childGroupType = groupTypeService.Get( item.Key );
                    if ( childGroupType != null )
                    {
                        groupType.ChildGroupTypes.Add( childGroupType );
                    }
                }

                DefinedValueService definedValueService = new DefinedValueService();

                groupType.LocationTypes = new List<GroupTypeLocationType>();
                groupType.LocationTypes.Clear();
                foreach ( var item in LocationTypesDictionary )
                {
                    var locationType = definedValueService.Get( item.Key );
                    if ( locationType != null )
                    {
                        groupType.LocationTypes.Add( new GroupTypeLocationType { LocationTypeValueId = locationType.Id } );
                    }
                }

                if ( !groupType.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                RockTransactionScope.WrapTransaction( () =>
                    {
                        groupTypeService.Save( groupType, CurrentPersonId );

                        // get it back to make sure we have a good Id for it for the Attributes
                        groupType = groupTypeService.Get( groupType.Guid );

                        /* Take care of Group Type Attributes */

                        // delete GroupTypeAttributes that are no longer configured in the UI
                        var groupTypeAttributesQry = attributeService.Get( new GroupType().TypeId, "Id", groupType.Id.ToString() );
                        var selectedAttributes = GroupTypeAttributesState.Select( a => a.Guid);
                        foreach(var attr in groupTypeAttributesQry.Where( a => !selectedAttributes.Contains( a.Guid)))
                        {
                            Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                            attributeService.Delete( attr, CurrentPersonId );
                            attributeService.Save( attr, CurrentPersonId );
                        }

                        string qualifierValue = groupType.Id.ToString();

                        // add/update the GroupTypeAttributes that are assigned in the UI
                        foreach ( var attributeState in GroupTypeAttributesState
                            .Where( a => 
                                a.EntityTypeQualifierValue == null ||
                                a.EntityTypeQualifierValue.Trim() == string.Empty || 
                                a.EntityTypeQualifierValue.Equals(qualifierValue)))
                        {
                            // remove old qualifiers in case they changed
                            var qualifierService = new AttributeQualifierService();
                            foreach ( var oldQualifier in qualifierService.GetByAttributeId( attributeState.Id ).ToList() )
                            {
                                qualifierService.Delete( oldQualifier, CurrentPersonId );
                                qualifierService.Save( oldQualifier, CurrentPersonId );
                            }

                            Attribute attribute = groupTypeAttributesQry.FirstOrDefault( a => a.Guid.Equals( attributeState.Guid ) );
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

                            attribute.EntityTypeQualifierColumn = "Id";
                            attribute.EntityTypeQualifierValue = qualifierValue;
                            attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( GroupType ) ).Id;
                            Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                            attributeService.Save( attribute, CurrentPersonId );
                        }

                        /* Take care of Group Attributes */

                        // delete GroupAttributes that are no longer configured in the UI
                        var groupAttributesQry = attributeService.Get( new Group().TypeId, "GroupTypeId", groupType.Id.ToString() );
                        selectedAttributes = GroupAttributesState.Select( a => a.Guid);
                        foreach( var attr in groupAttributesQry.Where( a => !selectedAttributes.Contains( a.Guid)))
                        {
                            Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                            attributeService.Delete( attr, CurrentPersonId );
                            attributeService.Save( attr, CurrentPersonId );
                        }

                        // add/update the GroupAttributes that are assigned in the UI
                        foreach ( var attributeState in GroupAttributesState 
                            .Where( a => 
                                a.EntityTypeQualifierValue == null ||
                                a.EntityTypeQualifierValue.Trim() == string.Empty || 
                                a.EntityTypeQualifierValue.Equals(qualifierValue)))
                        {
                            // remove old qualifiers in case they changed
                            var qualifierService = new AttributeQualifierService();
                            foreach ( var oldQualifier in qualifierService.GetByAttributeId( attributeState.Id ).ToList() )
                            {
                                qualifierService.Delete( oldQualifier, CurrentPersonId );
                                qualifierService.Save( oldQualifier, CurrentPersonId );
                            }

                            Attribute attribute = groupAttributesQry.FirstOrDefault( a => a.Guid.Equals( attributeState.Guid ) );
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

                            attribute.EntityTypeQualifierColumn = "GroupTypeId";
                            attribute.EntityTypeQualifierValue = qualifierValue;
                            attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( Group ) ).Id;
                            Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                            attributeService.Save( attribute, CurrentPersonId );
                        }
                    } );

            }
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the gtpInheritedGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gtpInheritedGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                var groupTypeService = new GroupTypeService();
                var attributeService = new AttributeService();
                RebuildAttributeLists( gtpInheritedGroupType.SelectedValueAsInt(), groupTypeService, attributeService, true );
            }

            BindGroupTypeAttributesGrid();
            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the attribute grids.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        void gAttributes_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                Attribute attribute = e.Row.DataItem as Attribute;
                if ( attribute != null )
                {
                    if ( !string.IsNullOrEmpty( attribute.EntityTypeQualifierValue ) &&
                        attribute.EntityTypeQualifierValue != PageParameter( "GroupTypeId" ) )
                    {

                        // Inherited Attribute
                        e.Row.Cells[0].Text = string.Format(
                            "<span class='muted'>{0} <span class='inherited'>(Inherited from <a href='{1}' target='_blank'>{2}</a>)</span></span>",
                            attribute.Name,
                            Page.ResolveUrl( "~/GroupType/" + attribute.EntityTypeQualifierValue ),
                            attribute.Description );   // TODO, once a GroupTypeCache object exists, the name could be retrieved from the cache object instead of using the description property to hold it (we don't want to do a db qry on every row)

                        e.Row.Cells[1].Controls.Clear();  // Edit
                        e.Row.Cells[2].Controls.Clear();  // Delete
                    }
                }
            }
        }

        #endregion

        #region GroupTypeAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributes_Add( object sender, EventArgs e )
        {
            gGroupTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gGroupTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group type attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlGroupTypeAttribute.Visible = true;

            edtGroupTypeAttributes.AttributeEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( GroupType ) ).Id;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                edtGroupTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for group type " + tbName.Text );
            }
            else
            {
                attribute = GroupTypeAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for group type " + tbName.Text );
            }

            edtGroupTypeAttributes.SetAttributeProperties( attribute, typeof( GroupType ) );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupTypeAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            GroupTypeAttributesState.RemoveEntity( attributeGuid );

            BindGroupTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributes_GridRebind( object sender, EventArgs e )
        {
            BindGroupTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveGroupTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveGroupTypeAttribute_Click( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtGroupTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            GroupTypeAttributesState.RemoveEntity( attribute.Guid );
            GroupTypeAttributesState.Add( attribute );

            pnlDetails.Visible = true;
            pnlGroupTypeAttribute.Visible = false;

            BindGroupTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelGroupTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelGroupTypeAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlGroupTypeAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindGroupTypeAttributesGrid()
        {
            gGroupTypeAttributes.DataSource = GroupTypeAttributesState.ToList();
            gGroupTypeAttributes.DataBind();
        }

        #endregion

        #region GroupAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributes_Add( object sender, EventArgs e )
        {
            gGroupAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gGroupAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlGroupAttribute.Visible = true;

            edtGroupAttributes.AttributeEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( Group ) ).Id;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                edtGroupAttributes.ActionTitle = ActionTitle.Add( "attribute for groups of group type " + tbName.Text );
            }
            else
            {
                attribute = GroupAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupAttributes.ActionTitle = ActionTitle.Edit( "attribute for groups of group type " + tbName.Text );
            }

            edtGroupAttributes.SetAttributeProperties( attribute, typeof( Group ) );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            GroupAttributesState.RemoveEntity( attributeGuid );

            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributes_GridRebind( object sender, EventArgs e )
        {
            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveGroupAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveGroupAttribute_Click( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtGroupAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            GroupAttributesState.RemoveEntity( attribute.Guid );
            GroupAttributesState.Add( attribute );

            pnlDetails.Visible = true;
            pnlGroupAttribute.Visible = false;

            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelGroupAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelGroupAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlGroupAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindGroupAttributesGrid()
        {
            gGroupAttributes.DataSource = GroupAttributesState.ToList();
            gGroupAttributes.DataBind();
        }

        #endregion
    }
}