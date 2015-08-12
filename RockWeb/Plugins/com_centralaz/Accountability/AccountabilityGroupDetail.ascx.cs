using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    [DisplayName( "Accountability Group Detail" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Displays the details of the given accountability group." )]

    [GroupTypesField( "Group Types", "Select group types to show in this block.  Leave all unchecked to show all group types.", false, "", "", 0 )]
    [BooleanField( "Show Edit", "", true, "", 1 )]
    [BooleanField( "Limit to Security Role Groups", "", false, "", 2 )]
    [BooleanField( "Limit to Group Types that are shown in navigation", "", false, "", 3, "LimitToShowInNavigationGroupTypes" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The style of maps to use", false, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK, "", 4 )]
    [LinkedPage( "Group Map Page", "The page to display detailed group map." )]
    public partial class AccountabilityGroupDetail : RockBlock, IDetailBlock
    {
        #region Constants

        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";

        #endregion

        #region Fields

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };

        private string LocationTypeTab
        {
            get
            {
                object currentProperty = ViewState["LocationTypeTab"];
                return currentProperty != null ? currentProperty.ToString() : MEMBER_LOCATION_TAB_TITLE;
            }

            set
            {
                ViewState["LocationTypeTab"] = value;
            }
        }

        #endregion

        #region Properties

        private List<GroupLocation> GroupLocationsState { get; set; }
        private List<InheritedAttribute> GroupMemberAttributesInheritedState { get; set; }
        private List<Attribute> GroupMemberAttributesState { get; set; }
        private bool AllowMultipleLocations { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["GroupLocationsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupLocationsState = new List<GroupLocation>();
            }
            else
            {
                GroupLocationsState = JsonConvert.DeserializeObject<List<GroupLocation>>( json );
            }

            json = ViewState["GroupMemberAttributesInheritedState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMemberAttributesInheritedState = new List<InheritedAttribute>();
            }
            else
            {
                GroupMemberAttributesInheritedState = JsonConvert.DeserializeObject<List<InheritedAttribute>>( json );
            }

            json = ViewState["GroupMemberAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMemberAttributesState = new List<Attribute>();
            }
            else
            {
                GroupMemberAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

            AllowMultipleLocations = ViewState["AllowMultipleLocations"] as bool? ?? false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );


            sbtnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Group ) ).Id;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlGroupList );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( IsPersonMember( PageParameter( "GroupId" ).AsInteger() ) || IsUserAuthorized( Authorization.EDIT ) )
            {
                base.OnLoad( e );

                if ( !Page.IsPostBack )
                {
                    string groupId = PageParameter( "GroupId" );

                    if ( !string.IsNullOrWhiteSpace( groupId ) )
                    {
                        ShowDetail( groupId.AsInteger(), PageParameter( "ParentGroupId" ).AsIntegerOrNull() );
                    }
                    else
                    {
                        pnlDetails.Visible = false;
                    }
                }
                else
                {

                }

                // Rebuild the attribute controls on postback based on group type
                if ( pnlDetails.Visible )
                {
                    var group = new Group { GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0 };
                    if ( group.GroupTypeId > 0 )
                    {
                        ShowGroupTypeEditDetails( GroupTypeCache.Read( group.GroupTypeId ), group, false );
                    }
                }
            }
            else
            {
                RockPage.Layout.Site.RedirectToPageNotFoundPage();
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
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["GroupLocationsState"] = JsonConvert.SerializeObject( GroupLocationsState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesInheritedState"] = JsonConvert.SerializeObject( GroupMemberAttributesInheritedState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesState"] = JsonConvert.SerializeObject( GroupMemberAttributesState, Formatting.None, jsonSetting );
            ViewState["AllowMultipleLocations"] = AllowMultipleLocations;

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupId = PageParameter( pageReference, "GroupId" ).AsIntegerOrNull();
            if ( groupId != null )
            {
                Group group = new GroupService( new RockContext() ).Get( groupId.Value );
                if ( group != null )
                {
                    breadCrumbs.Add( new BreadCrumb( group.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Group", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
        }

        
        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            Group group;
            bool wasSecurityRole = false;

            RockContext rockContext = new RockContext();

            GroupService groupService = new GroupService( rockContext );
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );

            if ( ( ddlGroupType.SelectedValueAsInt() ?? 0 ) == 0 )
            {
                ddlGroupType.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( GroupType.FriendlyTypeName ) );
                return;
            }

            int groupId = int.Parse( hfGroupId.Value );

            if ( groupId == 0 )
            {
                group = new Group();
                group.IsSystem = false;
                group.Name = string.Empty;
            }
            else
            {
                group = groupService.Queryable( "GroupLocations.Schedules" ).Where( g => g.Id == groupId ).FirstOrDefault();
                wasSecurityRole = group.IsSecurityRole;

                var selectedLocations = GroupLocationsState.Select( l => l.Guid );
                foreach ( var groupLocation in group.GroupLocations.Where( l => !selectedLocations.Contains( l.Guid ) ).ToList() )
                {
                    group.GroupLocations.Remove( groupLocation );
                    groupLocationService.Delete( groupLocation );
                }
            }

            foreach ( var groupLocationState in GroupLocationsState )
            {
                GroupLocation groupLocation = group.GroupLocations.Where( l => l.Guid == groupLocationState.Guid ).FirstOrDefault();
                if ( groupLocation == null )
                {
                    groupLocation = new GroupLocation();
                    group.GroupLocations.Add( groupLocation );
                }
                else
                {
                    groupLocationState.Id = groupLocation.Id;
                    groupLocationState.Guid = groupLocation.Guid;

                    var selectedSchedules = groupLocationState.Schedules.Select( s => s.Guid ).ToList();
                    foreach ( var schedule in groupLocation.Schedules.Where( s => !selectedSchedules.Contains( s.Guid ) ).ToList() )
                    {
                        groupLocation.Schedules.Remove( schedule );
                    }
                }

                groupLocation.CopyPropertiesFrom( groupLocationState );

                var existingSchedules = groupLocation.Schedules.Select( s => s.Guid ).ToList();
                foreach ( var scheduleState in groupLocationState.Schedules.Where( s => !existingSchedules.Contains( s.Guid ) ).ToList() )
                {
                    var schedule = scheduleService.Get( scheduleState.Guid );
                    if ( schedule != null )
                    {
                        groupLocation.Schedules.Add( schedule );
                    }
                }
            }

            group.Name = dtbName.Text;
            group.Description = dtbDescription.Text;
            group.IsActive = cbIsActive.Checked;
            group.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );

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

            // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
            rockContext.WrapTransaction( () =>
            {
                if ( group.Id.Equals( 0 ) )
                {
                    groupService.Add( group );
                }
                rockContext.SaveChanges();

                group.SaveAttributeValues( rockContext );

                /* Take care of Group Member Attributes */
                var entityTypeId = EntityTypeCache.Read( typeof( GroupMember ) ).Id;
                string qualifierColumn = "GroupId";
                string qualifierValue = group.Id.ToString();

                // Get the existing attributes for this entity type and qualifier value
                var attributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

                // Delete any of those attributes that were removed in the UI
                var selectedAttributeGuids = GroupMemberAttributesState.Select( a => a.Guid );
                foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
                {
                    Rock.Web.Cache.AttributeCache.Flush( attr.Id );

                    attributeService.Delete( attr );
                }

                // Update the Attributes that were assigned in the UI
                foreach ( var attributeState in GroupMemberAttributesState )
                {
                    Rock.Attribute.Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
                }

                rockContext.SaveChanges();
            } );

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
            qryParams["GroupId"] = group.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupId.Value.Equals( "0" ) )
            {
                int? parentGroupId = PageParameter( "ParentGroupId" ).AsIntegerOrNull();
                if ( parentGroupId.HasValue )
                {
                    // Cancelling on Add, and we know the parentGroupID, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    if ( parentGroupId != 0 )
                    {
                        qryParams["GroupId"] = parentGroupId.ToString();
                    }
                    NavigateToPage( RockPage.Guid, qryParams );
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
                ShowReadonlyDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
            }
        }

        #endregion

        #region Control Events


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowReadonlyDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // grouptype changed, so load up the new attributes and set controls to the default attribute values
            var group = new Group { GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0 };
            if ( group.GroupTypeId > 0 )
            {
                ShowGroupTypeEditDetails( GroupTypeCache.Read( group.GroupTypeId ), group, true );
            }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public void ShowDetail( int groupId )
        {
            ShowDetail( groupId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="parentGroupId">The parent group identifier.</param>
        public void ShowDetail( int groupId, int? parentGroupId )
        {
            Group group = null;

            bool editAllowed = true;

            if ( !groupId.Equals( 0 ) )
            {
                group = GetGroup( groupId );
                if ( group != null )
                {
                    editAllowed = group.IsAuthorized( Authorization.EDIT, CurrentPerson ) || IsPersonLeader( groupId );
                }
            }

            if ( group == null )
            {
                group = new Group { Id = 0, IsActive = true, ParentGroupId = parentGroupId, Name = "" };

            }

            pnlDetails.Visible = true;

            hfGroupId.Value = group.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed && !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( group.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( Group.FriendlyTypeName );
            }

            var roleLimitWarnings = new StringBuilder();

            if ( group.GroupType != null && group.GroupType.Roles != null && group.GroupType.Roles.Any() )
            {
                foreach ( var role in group.GroupType.Roles )
                {
                    int curCount = 0;
                    if ( group.Members != null )
                    {
                        curCount = group.Members
                            .Where( m => m.GroupRoleId == role.Id && m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Count();
                    }

                    if ( role.MinCount.HasValue && role.MinCount.Value > curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently below its minimum requirement of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize(), role.Name, role.MinCount, role.MinCount == 1 ? group.GroupType.GroupMemberTerm : group.GroupType.GroupMemberTerm.Pluralize() );
                    }

                    if ( role.MaxCount.HasValue && role.MaxCount.Value < curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently above its maximum limit of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize(), role.Name, role.MaxCount, role.MaxCount == 1 ? group.GroupType.GroupMemberTerm : group.GroupType.GroupMemberTerm.Pluralize() );
                    }
                }
            }

            nbRoleLimitWarning.Text = roleLimitWarnings.ToString();
            nbRoleLimitWarning.Visible = roleLimitWarnings.Length > 0;

            if ( readOnly )
            {
                pnlDetails.Visible = false;
                ShowReadonlyDetails( group );
            }
            else
            {
                lbEdit.Visible = true;
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
            if ( group.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Group.FriendlyTypeName ).FormatAsHtmlTitle();
                hlblInactive.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            dtbName.Text = group.Name;
            dtbDescription.Text = group.Description;
            cbIsActive.Checked = group.IsActive;

            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns();

            if ( group.Id == 0 && ddlGroupType.Items.Count > 1 )
            {
                if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean() )
                {
                    // default GroupType for new Group to "Security Roles"  if LimittoSecurityRoleGroups
                    var securityRoleGroupType = GroupTypeCache.GetSecurityRoleGroupType();
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

            //GroupLocationsState = groupLocations;
            GroupLocationsState = group.GroupLocations.ToList();

            ShowGroupTypeEditDetails( GroupTypeCache.Read( group.GroupTypeId ), group, true );

            string qualifierValue = group.Id.ToString();
            GroupMemberAttributesState = attributeService.GetByEntityTypeId( new GroupMember().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();

            BindInheritedAttributes( group.GroupTypeId, attributeService );
        }

        /// <summary>
        /// Shows the group type edit details.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="group">The group.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowGroupTypeEditDetails( GroupTypeCache groupType, Group group, bool setValues )
        {
            if ( group != null )
            {
                // Save value to viewstate for use later when binding location grid
                AllowMultipleLocations = groupType != null && groupType.AllowMultipleLocations;
                phGroupAttributes.Controls.Clear();
                group.LoadAttributes();

                if ( group.Attributes != null && group.Attributes.Any() )
                {
                    Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, setValues, BlockValidationGroup );
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Group group )
        {
            SetEditMode( false );
            var rockContext = new RockContext();

            string groupIconHtml = string.Empty;
            if ( group.GroupType != null )
            {
                groupIconHtml = !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) ?
                    groupIconHtml = string.Format( "<i class='{0}' ></i>", group.GroupType.IconCssClass ) : string.Empty;
            }

            hfGroupId.SetValue( group.Id );
            lGroupIconHtml.Text = groupIconHtml;
            lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();

            hlblInactive.Visible = !group.IsActive;
            hlblType.Text = group.GroupType.Name;

            lGroupDescription.Text = group.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( group.ParentGroup != null )
            {
                descriptionList.Add( "Parent Group", group.ParentGroup.Name );
            }

            if ( group.Campus != null )
            {
                hlblCampus.Visible = true;
                hlblCampus.Text = group.Campus.Name;
            }
            else
            {
                hlblCampus.Visible = false;
            }

            lMainDetails.Text = descriptionList.Html;

            var attributes = new List<Rock.Web.Cache.AttributeCache>();

            // Get the attributes inherited from group type
            GroupType groupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
            groupType.LoadAttributes();
            attributes = groupType.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            // Combine with the group attributes
            group.LoadAttributes();
            attributes.AddRange( group.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) );

            // display attribute values
            var attributeCategories = Helper.GetAttributeCategories( attributes );
            Rock.Attribute.Helper.AddDisplayControls( group, attributeCategories, phAttributes, null, false );

            var pageParams = new Dictionary<string, string>();
            pageParams.Add( "GroupId", group.Id.ToString() );
            string groupMapUrl = LinkedPageUrl( "GroupMapPage", pageParams );

            // Get Map Style
            phMaps.Controls.Clear();
            var mapStyleValue = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ) );
            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            if ( mapStyleValue != null )
            {
                string mapStyle = mapStyleValue.GetAttributeValue( "StaticMapStyle" );
                if ( !string.IsNullOrWhiteSpace( mapStyle ) )
                {
                    foreach ( GroupLocation groupLocation in group.GroupLocations.OrderBy( gl => ( gl.GroupLocationTypeValue != null ) ? gl.GroupLocationTypeValue.Order : int.MaxValue ) )
                    {
                        if ( groupLocation.Location != null )
                        {
                            if ( groupLocation.Location.GeoPoint != null )
                            {
                                string markerPoints = string.Format( "{0},{1}", groupLocation.Location.GeoPoint.Latitude, groupLocation.Location.GeoPoint.Longitude );
                                string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", markerPoints );
                                mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", string.Empty );
                                mapLink += "&sensor=false&size=350x200&zoom=13&format=png";
                                var literalcontrol = new Literal()
                                {
                                    Text = string.Format(
                                    "<div class='group-location-map'>{0}<a href='{1}'><img src='{2}'/></a></div>",
                                    groupLocation.GroupLocationTypeValue != null ? ( "<h4>" + groupLocation.GroupLocationTypeValue.Value + "</h4>" ) : string.Empty,
                                    groupMapUrl,
                                    mapLink ),
                                    Mode = LiteralMode.PassThrough
                                };
                                phMaps.Controls.Add( literalcontrol );
                            }
                            else if ( groupLocation.Location.GeoFence != null )
                            {
                                string polygonPoints = "enc:" + groupLocation.Location.EncodeGooglePolygon();
                                string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", string.Empty );
                                mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", polygonPoints );
                                mapLink += "&sensor=false&size=350x200&format=png";
                                phMaps.Controls.Add(
                                    new LiteralControl( string.Format(
                                        "<div class='group-location-map'>{0}<a href='{1}'><img src='{2}'/></a></div>",
                                        groupLocation.GroupLocationTypeValue != null ? ( "<h4>" + groupLocation.GroupLocationTypeValue.Value + "</h4>" ) : string.Empty,
                                        groupMapUrl,
                                        mapLink ) ) );
                            }
                        }
                    }
                }
            }

            hlMap.Visible = !string.IsNullOrWhiteSpace( groupMapUrl );
            hlMap.NavigateUrl = groupMapUrl;

            sbtnSecurity.Visible = group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            sbtnSecurity.EntityId = group.Id;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        private Group GetGroup( int groupId )
        {
            string key = string.Format( "Group:{0}", groupId );
            Group group = RockPage.GetSharedItem( key ) as Group;
            if ( group == null )
            {
                group = new GroupService( new RockContext() ).Queryable( "GroupType,GroupLocations.Schedules" )
                    .Where( g => g.Id == groupId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, group );
            }

            return group;
        }

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( property.ToString() == LocationTypeTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            GroupTypeService groupTypeService = new GroupTypeService( new RockContext() );
            var purpose = new DefinedValueService( new RockContext() ).GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() ).Where( a => a.Value == "Accountability Group" ).FirstOrDefault();
            List<GroupType> groupTypes = groupTypeService.Queryable().Where( a => a.GroupTypePurposeValueId == purpose.Id ).OrderBy( a => a.Name ).ToList();
            groupTypes.Insert( 0, new GroupType { Id = None.Id, Name = None.Text } );
            ddlGroupType.DataSource = groupTypes;
            ddlGroupType.DataBind();
        }

        /// <summary>
        /// Binds the inherited attributes.
        /// </summary>
        /// <param name="inheritedGroupTypeId">The inherited group type identifier.</param>
        /// <param name="attributeService">The attribute service.</param>
        private void BindInheritedAttributes( int? inheritedGroupTypeId, AttributeService attributeService )
        {
            GroupMemberAttributesInheritedState = new List<InheritedAttribute>();

            while ( inheritedGroupTypeId.HasValue )
            {
                var inheritedGroupType = GroupTypeCache.Read( inheritedGroupTypeId.Value );
                if ( inheritedGroupType != null )
                {
                    string qualifierValue = inheritedGroupType.Id.ToString();

                    foreach ( var attribute in attributeService.GetByEntityTypeId( new GroupMember().TypeId ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        GroupMemberAttributesInheritedState.Add( new InheritedAttribute(
                            attribute.Name,
                            attribute.Key,
                            attribute.Description,
                            Page.ResolveUrl( "~/GroupType/" + attribute.EntityTypeQualifierValue ),
                            inheritedGroupType.Name ) );
                    }

                    inheritedGroupTypeId = inheritedGroupType.InheritedGroupTypeId;
                }
                else
                {
                    inheritedGroupTypeId = null;
                }
            }


        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetAttributeListOrder( List<Attribute> attributeList )
        {
            int order = 0;
            attributeList.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderAttributeList( List<Attribute> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Returns true if the current person is a group leader.
        /// </summary>
        /// <param name="groupId">The group Id</param>
        /// <returns>A boolean: true if the person is a leader, false if not.</returns>
        protected bool IsPersonLeader( int groupId )
        {
            int count = new GroupMemberService( new RockContext() ).Queryable( "GroupTypeRole" )
                .Where( m =>
                    m.PersonId == CurrentPersonId &&
                    m.GroupId == groupId &&
                    m.GroupRole.Name == "Leader"
                    )
                 .Count();
            if ( count == 1 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the current person is a group member.
        /// </summary>
        /// <param name="groupId">The group Id</param>
        /// <returns>A boolean: true if the person is a member, false if not.</returns>
        protected bool IsPersonMember( int groupId )
        {
            int count = new GroupMemberService( new RockContext() ).Queryable( "GroupTypeRole" )
                .Where( m =>
                    m.PersonId == CurrentPersonId &&
                    m.GroupId == groupId
                    )
                 .Count();
            if ( count == 1 )
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