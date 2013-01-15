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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
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
            gChildGroupTypes.Actions.IsAddEnabled = true;
            gChildGroupTypes.Actions.AddClick += gChildGroupTypes_Add;
            gChildGroupTypes.GridRebind += gChildGroupTypes_GridRebind;
            gChildGroupTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gLocationTypes.DataKeyNames = new string[] { "key" };
            gLocationTypes.Actions.IsAddEnabled = true;
            gLocationTypes.Actions.AddClick += gLocationTypes_Add;
            gLocationTypes.GridRebind += gLocationTypes_GridRebind;
            gLocationTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gGroupTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupTypeAttributes.Actions.IsAddEnabled = true;
            gGroupTypeAttributes.Actions.AddClick += gGroupTypeAttributes_Add;
            gGroupTypeAttributes.GridRebind += gGroupTypeAttributes_GridRebind;
            gGroupTypeAttributes.EmptyDataText = Server.HtmlEncode( None.Text );

            gGroupAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupAttributes.Actions.IsAddEnabled = true;
            gGroupAttributes.Actions.AddClick += gGroupAttributes_Add;
            gGroupAttributes.GridRebind += gGroupAttributes_GridRebind;
            gGroupAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
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
        private void LoadDropDowns()
        {
            GroupRoleService groupRoleService = new GroupRoleService();
            List<GroupRole> groupRoles = groupRoleService.Queryable().OrderBy( a => a.Name ).ToList();
            groupRoles.Insert( 0, new GroupRole { Id = None.Id, Name = None.Text } );
            ddlDefaultGroupRole.DataSource = groupRoles;
            ddlDefaultGroupRole.DataBind();

            ddlAttendanceRule.BindToEnum( typeof( Rock.Model.AttendanceRule ) );
            ddlAttendancePrintTo.BindToEnum( typeof( Rock.Model.PrintTo ) );
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

            if ( !itemKeyValue.Equals( 0 ) )
            {
                groupType = new GroupTypeService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( GroupType.FriendlyTypeName );
            }
            else
            {
                groupType = new GroupType { Id = 0 };
                groupType.ChildGroupTypes = new List<GroupType>();
                groupType.LocationTypes = new List<GroupTypeLocationType>();
                lActionTitle.Text = ActionTitle.Add( GroupType.FriendlyTypeName );
            }

            LoadDropDowns();

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
            tbIconCssClass.Text = groupType.IconCssClass;
            imgIconSmall.ImageId = groupType.IconSmallFileId;
            imgIconLarge.ImageId = groupType.IconLargeFileId;

            cbTakesAttendance.Checked = groupType.TakesAttendance;
            ddlAttendanceRule.SetValue( (int)groupType.AttendanceRule );
            ddlAttendancePrintTo.SetValue( (int)groupType.AttendancePrintTo );
            cbAllowMultipleLocations.Checked = groupType.AllowMultipleLocations;
            groupType.ChildGroupTypes.ToList().ForEach( a => ChildGroupTypesDictionary.Add( a.Id, a.Name ) );
            groupType.LocationTypes.ToList().ForEach( a => LocationTypesDictionary.Add( a.LocationTypeValueId, a.LocationTypeValue.Name ) );

            AttributeService attributeService = new AttributeService();

            var qryGroupTypeAttributes = attributeService.GetByEntityTypeId( new GroupType().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( groupType.Id.ToString() ) );

            GroupTypeAttributesState.AddAll( qryGroupTypeAttributes.ToList() );
            BindGroupTypeAttributesGrid();

            var qryGroupAttributes = attributeService.GetByEntityTypeId( new Group().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( groupType.Id.ToString() ) );

            GroupAttributesState.AddAll( qryGroupAttributes.ToList() );
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
            gChildGroupTypes.Enabled = !readOnly;
            gLocationTypes.Enabled = !readOnly;
            btnSave.Visible = !readOnly;

            BindChildGroupTypesGrid();
            BindLocationTypesGrid();
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

            // populate dropdown with all grouptypes that aren't already childgroups or the current grouptype
            var qry = from gt in groupTypeService.Queryable()
                      where gt.Id != currentGroupTypeId
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
            var qry = from dlt in definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE )
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
            GroupType groupType;
            GroupTypeService groupTypeService = new GroupTypeService();

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
            groupType.IconCssClass = tbIconCssClass.Text;
            groupType.IconSmallFileId = imgIconSmall.ImageId;
            groupType.IconLargeFileId = imgIconLarge.ImageId;
            groupType.TakesAttendance = cbTakesAttendance.Checked;
            groupType.AttendanceRule = ddlAttendanceRule.SelectedValueAsEnum<AttendanceRule>();
            groupType.AttendancePrintTo = ddlAttendancePrintTo.SelectedValueAsEnum<PrintTo>();
            groupType.AllowMultipleLocations = cbAllowMultipleLocations.Checked;

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

            // check for duplicates
            if ( groupTypeService.Queryable().Count( a => a.Name.Equals( groupType.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( groupType.Id ) ) > 0 )
            {
                tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", GroupType.FriendlyTypeName ) );
                return;
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

                    AttributeService attributeService = new AttributeService();
                    /* Take care of Group Type Attributes */

                    // delete GroupTypeAttributes that are no longer configured in the UI
                    var groupTypeAttributesQry = attributeService.GetByEntityTypeId( new GroupType().TypeId ).AsQueryable()
                        .Where( a => a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase )
                        && a.EntityTypeQualifierValue.Equals( groupType.Id.ToString() ) );

                    var deletedGroupTypeAttributes = from attr in groupTypeAttributesQry
                                            where !( from d in GroupTypeAttributesState
                                                     select d.Guid ).Contains( attr.Guid )
                                            select attr;

                    deletedGroupTypeAttributes.ToList().ForEach( a =>
                    {
                        var attr = attributeService.Get( a.Guid );
                        attributeService.Delete( attr, CurrentPersonId );
                        attributeService.Save( attr, CurrentPersonId );
                    } );

                    // add/update the GroupTypeAttributes that are assigned in the UI
                    foreach ( var attributeState in GroupTypeAttributesState )
                    {
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
                        }

                        attribute.EntityTypeQualifierColumn = "GroupTypeId";
                        attribute.EntityTypeQualifierValue = groupType.Id.ToString();
                        attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( new GroupType().TypeName ).Id;
                        attributeService.Save( attribute, CurrentPersonId );
                    }

                    /* Take care of Group Attributes */

                    // delete GroupAttributes that are no longer configured in the UI
                    var groupAttributesQry = attributeService.GetByEntityTypeId( new Group().TypeId ).AsQueryable()
                        .Where( a => a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase )
                        && a.EntityTypeQualifierValue.Equals( groupType.Id.ToString() ) );

                    var deletedGroupAttributes = from attr in groupAttributesQry
                                                     where !( from d in GroupAttributesState
                                                              select d.Guid ).Contains( attr.Guid )
                                                     select attr;

                    deletedGroupAttributes.ToList().ForEach( a =>
                    {
                        var attr = attributeService.Get( a.Guid );
                        attributeService.Delete( attr, CurrentPersonId );
                        attributeService.Save( attr, CurrentPersonId );
                    } );

                    // add/update the GroupAttributes that are assigned in the UI
                    foreach ( var attributeState in GroupAttributesState )
                    {
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
                        }

                        attribute.EntityTypeQualifierColumn = "GroupTypeId";
                        attribute.EntityTypeQualifierValue = groupType.Id.ToString();
                        attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( new Group().TypeName ).Id;
                        attributeService.Save( attribute, CurrentPersonId );
                    }
                } );

            NavigateToParentPage();
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
            Attribute attribute;
            string actionTitle;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                actionTitle = ActionTitle.Add( "attribute for group type " + tbName.Text );
            }
            else
            {
                attribute = GroupTypeAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                actionTitle = ActionTitle.Edit( "attribute for group type " + tbName.Text );
            }

            edtGroupTypeAttributes.EditAttribute( attribute, actionTitle );
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
            edtGroupTypeAttributes.GetAttributeValues( attribute );

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
            gGroupTypeAttributes.DataSource = GroupTypeAttributesState.OrderBy( a => a.Name ).ToList();
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
            Attribute attribute;
            string actionTitle;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                actionTitle = ActionTitle.Add( "attribute for groups of group type " + tbName.Text );
            }
            else
            {
                attribute = GroupAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                actionTitle = ActionTitle.Edit( "attribute for groups of group type " + tbName.Text );
            }

            edtGroupAttributes.EditAttribute( attribute, actionTitle );
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
            edtGroupAttributes.GetAttributeValues( attribute );

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
            gGroupAttributes.DataSource = GroupAttributesState.OrderBy( a => a.Name ).ToList();
            gGroupAttributes.DataBind();
        }

        #endregion
    }
}