using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Cache;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Notification Group Detail" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Views the details of a notification group." )]

    public partial class NotificationGroupDetail : RockBlock
    {
        #region Properties

        private List<NameValueEntity> DeviceListState { get; set; }

        private List<NameValueEntity> DeviceGroupListState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            DeviceListState = ViewState["DeviceList"] as List<NameValueEntity>;
            DeviceGroupListState = ViewState["DeviceGroupList"] as List<NameValueEntity>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gMembers.Actions.AddClick += gMembers_AddClick;
            gMembers.DataKeyNames = new string[] { "Id" };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( PageParameter( "Id" ).AsInteger() != 0 )
                {
                    ShowDetail( PageParameter( "Id" ).AsInteger() );
                }
                else
                {
                    ShowEdit( 0 );
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
            ViewState["DeviceList"] = DeviceListState;
            ViewState["DeviceGroupList"] = DeviceGroupListState;

            return base.SaveViewState();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        private void ShowDetail( int groupId )
        {
            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupId );
            var groupMemberService = new GroupMemberService( rockContext );
            var deviceService = new WatchdogDeviceService( rockContext );
            var deviceGroupService = new WatchdogDeviceGroupService( rockContext );

            pnlDetails.Visible = false;
            pnlMembers.Visible = false;
            pnlEdit.Visible = false;

            if ( group == null )
            {
                return;
            }

            if ( !group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( group.GetType().GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlMembers.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            pnlDetails.Visible = true;
            pnlMembers.Visible = true;
            pdAuditDetails.SetEntity( group, ResolveRockUrl( "~" ) );

            group.LoadAttributes( rockContext );

            lTitle.Text = group.Name;
            lStates.Text = string.Join( ", ", group.GetAttributeValues( "States" ) );

            var scheduleId = group.GetAttributeValue( "ScheduleCollectionId" ).AsIntegerOrNull();
            var schedule = scheduleId.HasValue ? WatchdogScheduleCollectionCache.Get( scheduleId.Value ) : null;
            lSchedule.Text = schedule != null ? schedule.Name : string.Empty;

            var deviceIds = group.GetAttributeValues( "Devices" ).AsIntegerList();
            var devices = deviceService.Queryable().AsNoTracking()
                .Where( d => deviceIds.Contains( d.Id ) )
                .OrderBy( d => d.Name );
            lDevices.Text = string.Join( "<br>", devices.Select( d => d.Name ) );

            var deviceGroupIds = group.GetAttributeValues( "DeviceGroups" ).AsIntegerList();
            var deviceGroups = deviceGroupService.Queryable().AsNoTracking()
                .Where( g => deviceGroupIds.Contains( g.Id ) )
                .OrderBy( g => g.Name );
            lDeviceGroups.Text = string.Join( "<br>", deviceGroups.Select( g => g.Name ) );

            lbEdit.Visible = group.IsAuthorized( Authorization.EDIT, CurrentPerson );
            gMembers.Actions.ShowAdd = group.IsAuthorized( Authorization.EDIT, CurrentPerson );

            //
            // Bind the members grid.
            //
            BindMembersGrid( group.Id, rockContext );
        }

        /// <summary>
        /// Binds the members grid.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void BindMembersGrid( int groupId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var groupMemberService = new GroupMemberService( rockContext );
            var sortProperty = gMembers.SortProperty;
            var members = groupMemberService.Queryable().AsNoTracking()
                .Where( m => m.GroupId == groupId )
                .ToList();
            members.ForEach( m => m.LoadAttributes( rockContext ) );
            var memberList = members
                .Select( m => new
                {
                    m.Id,
                    Name = m.Person.FullName,
                    Email = m.GetAttributeValues( "NotificationMethod" ).Contains( "Email" ),
                    SMS = m.GetAttributeValues( "NotificationMethod" ).Contains( "SMS" ),
                    CanDelete = m.Group.IsAuthorized( Authorization.EDIT, CurrentPerson )
                } )
                .ToList();

            if ( sortProperty != null )
            {
                memberList = memberList.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                memberList = memberList.OrderBy( t => t.Name ).ThenBy( t => t.Id ).ToList();
            }

            gMembers.EntityTypeId = EntityTypeCache.Get<GroupMember>().Id;
            gMembers.DataSource = memberList;
            gMembers.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        private void ShowEdit( int groupId )
        {
            var rockContext = new RockContext();
            var deviceService = new WatchdogDeviceService( rockContext );
            var deviceGroupService = new WatchdogDeviceGroupService( rockContext );
            var group = new GroupService( rockContext ).Get( groupId );

            if ( group == null )
            {
                group = new Group
                {
                    GroupTypeId = GroupTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.GroupType.NOTIFICATION_GROUP.AsGuid() ).Id
                };
                lEditTitle.Text = "Add New Notification Group";
            }
            else
            {
                lEditTitle.Text = "Edit Notification Group";
            }

            group.LoadAttributes( rockContext );
            var deviceIds = group.GetAttributeValues( "Devices" ).AsIntegerList();
            var deviceGroupIds = group.GetAttributeValues( "DeviceGroups" ).AsIntegerList();

            tbName.Text = group.Name;

            ddlSchedule.Items.Clear();
            ddlSchedule.Items.Add( new ListItem() );
            foreach ( var schedule in new WatchdogScheduleCollectionService( rockContext ).Queryable().OrderBy( p => p.Name ) )
            {
                ddlSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }
            ddlSchedule.SetValue( group.GetAttributeValue( "ScheduleCollectionId" ).AsIntegerOrNull() );

            //
            // Setup the states.
            //
            cblStates.Items.Clear();
            cblStates.Items.Add( com.blueboxmoon.WatchdogMonitor.Checks.ServiceCheckState.OK.ToString() );
            cblStates.Items.Add( com.blueboxmoon.WatchdogMonitor.Checks.ServiceCheckState.Warning.ToString() );
            cblStates.Items.Add( com.blueboxmoon.WatchdogMonitor.Checks.ServiceCheckState.Error.ToString() );
            cblStates.SetValues( group.GetAttributeValues( "States" ) );

            //
            // Set the devices repeater.
            //
            DeviceListState = deviceService.Queryable().AsNoTracking()
                .Where( d => deviceIds.Contains( d.Id ) )
                .ToList()
                .Select( d => new NameValueEntity( d.Id, d.Name ) )
                .OrderBy( n => n.Name )
                .ToList();
            BindDevicesRepeater();

            //
            // Set the device groups repeater.
            //
            DeviceGroupListState = deviceGroupService.Queryable().AsNoTracking()
                .Where( g => deviceGroupIds.Contains( g.Id ) )
                .ToList()
                .Select( g => new NameValueEntity( g.Id, g.Name ) )
                .OrderBy( n => n.Name )
                .ToList();
            BindDeviceGroupsRepeater();

            //
            // Bind the devices drop down list.
            //
            ddlDevice.Items.Clear();
            ddlDevice.Items.Add( new ListItem() );
            deviceService.Queryable().AsNoTracking()
                .Select( d => new
                {
                    d.Id,
                    d.Name
                } )
                .OrderBy( d => d.Name )
                .ToList()
                .ForEach( d => ddlDevice.Items.Add( new ListItem( d.Name, d.Id.ToString() ) ) );

            //
            // Bind the device groups drop down list.
            //
            ddlDeviceGroup.Items.Clear();
            ddlDeviceGroup.Items.Add( new ListItem() );
            var deviceEntityTypeId = EntityTypeCache.Get<WatchdogDevice>().Id;
            deviceGroupService.Queryable().AsNoTracking()
                .OrderBy( d => d.Name )
                .ToList()
                .ForEach( d => ddlDeviceGroup.Items.Add( new ListItem( d.Name, d.Id.ToString() ) ) );

            pnlDetails.Visible = false;
            pnlMembers.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Shows the edit member.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        private void ShowEditMember( int groupMemberId )
        {
            var groupMemberService = new GroupMemberService( new RockContext() );
            var groupMember = groupMemberService.Get( groupMemberId );

            if ( groupMember == null )
            {
                groupMember = new GroupMember
                {
                    GroupId = PageParameter( "Id" ).AsInteger()
                };
            }

            groupMember.LoadAttributes();

            hfEditMemberId.Value = groupMember.Id.ToString();
            ppEditMember.SetValue( groupMember.Person );
            ppEditMember.Enabled = groupMember.Id == 0;
            cblEditMemberNotificationMethod.SetValues( groupMember.GetAttributeValues( "NotificationMethod" ) );

            mdlEditMember.Show();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var deviceService = new WatchdogDeviceService( rockContext );
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( PageParameter( "Id" ).AsInteger() );

            if ( group == null )
            {
                group = new Group
                {
                    GroupTypeId = GroupTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.GroupType.NOTIFICATION_GROUP.AsGuid() ).Id
                };
                groupService.Add( group );
            }

            if ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                group.LoadAttributes( rockContext );

                group.Name = tbName.Text;
                group.SetAttributeValue( "ScheduleCollectionId", ddlSchedule.SelectedValueAsId() );

                group.SetAttributeValue( "States", string.Join( ",", cblStates.SelectedValues ) );
                group.SetAttributeValue( "Devices", string.Join( ",", DeviceListState.Select( d => d.Id ) ) );
                group.SetAttributeValue( "DeviceGroups", string.Join( ",", DeviceGroupListState.Select( g => g.Id ) ) );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    group.SaveAttributeValues( rockContext );
                } );

                WatchdogNotificationGroupCache.Remove( group.Id );
            }

            NavigateToCurrentPage( new Dictionary<string, string> { { "Id", group.Id.ToString() } } );
        }

        /// <summary>
        /// Handles the Click event of the gMembersDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gMembersDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMember = groupMemberService.Get( e.RowKeyId );

            if ( groupMember != null && groupMember.Group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                groupMemberService.Delete( groupMember );

                rockContext.SaveChanges();

                WatchdogNotificationGroupCache.Remove( PageParameter( "Id" ).AsInteger() );
            }

            BindMembersGrid( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the GridRebind event of the gMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gMembers_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindMembersGrid( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the GridReorder event of the gMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gMembers_GridReorder( object sender, Rock.Web.UI.Controls.GridReorderEventArgs e )
        {
            BindMembersGrid( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the RowSelected event of the gMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gMembers_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ShowEditMember( e.RowKeyId );
        }

        /// <summary>
        /// Handles the AddClick event of the gMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gMembers_AddClick( object sender, EventArgs e )
        {
            ShowEditMember( 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMembers_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gMembers.Columns.IndexOf( gMembers.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( bool ) e.Row.DataItem.GetPropertyValue( "CanDelete" );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlEditMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlEditMember_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMember = groupMemberService.Get( hfEditMemberId.Value.AsInteger() );

            if ( groupMember == null )
            {
                var group = new GroupService( rockContext ).Get( PageParameter( "Id" ).AsInteger() );

                groupMember = new GroupMember
                {
                    GroupId = group.Id,
                    GroupRoleId = group.GroupType.DefaultGroupRoleId.Value
                };

                groupMemberService.Add( groupMember );
            }

            groupMember.LoadAttributes( rockContext );

            if ( groupMember.Id == 0 )
            {
                groupMember.PersonId = ppEditMember.PersonId.Value;
            }

            groupMember.SetAttributeValue( "NotificationMethod", string.Join( ",", cblEditMemberNotificationMethod.SelectedValues ) );

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                groupMember.SaveAttributeValues( rockContext );
            } );

            mdlEditMember.Hide();
            WatchdogNotificationGroupCache.Remove( PageParameter( "Id" ).AsInteger() );
            WatchdogNotificationGroupMemberCache.Remove( groupMember.Id );

            BindMembersGrid( PageParameter( "Id" ).AsInteger() );
        }

        #endregion

        #region Devices Repeater

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rpDevices_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = e.CommandArgument.ToString().AsInteger();

            if ( e.CommandName == "Delete" )
            {
                var nve = DeviceListState.Where( a => a.Id == personId ).FirstOrDefault();

                if ( nve != null )
                {
                    DeviceListState.Remove( nve );
                    BindDevicesRepeater();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDevice_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlDevice.SelectedValueAsId().HasValue && !DeviceListState.Any( d => d.Id == ddlDevice.SelectedValueAsId().Value ) )
            {
                DeviceListState.Add( new NameValueEntity( ddlDevice.SelectedValueAsId().Value, ddlDevice.SelectedItem.Text ) );
            }

            ddlDevice.SetValue( ( int? ) null );
            BindDevicesRepeater();
        }

        /// <summary>
        /// Binds the devices repeater.
        /// </summary>
        private void BindDevicesRepeater()
        {
            rpDevices.DataSource = DeviceListState;
            rpDevices.DataBind();
        }

        #endregion

        #region Devices Repeater

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rpDeviceGroups_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = e.CommandArgument.ToString().AsInteger();

            if ( e.CommandName == "Delete" )
            {
                var nve = DeviceGroupListState.Where( a => a.Id == personId ).FirstOrDefault();

                if ( nve != null )
                {
                    DeviceGroupListState.Remove( nve );
                    BindDeviceGroupsRepeater();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDeviceGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDeviceGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlDeviceGroup.SelectedValueAsId().HasValue && !DeviceGroupListState.Any( d => d.Id == ddlDeviceGroup.SelectedValueAsId().Value ) )
            {
                DeviceGroupListState.Add( new NameValueEntity( ddlDeviceGroup.SelectedValueAsId().Value, ddlDeviceGroup.SelectedItem.Text ) );
            }

            ddlDeviceGroup.SetValue( ( int? ) null );
            BindDeviceGroupsRepeater();
        }

        /// <summary>
        /// Binds the device groups repeater.
        /// </summary>
        private void BindDeviceGroupsRepeater()
        {
            rpDeviceGroups.DataSource = DeviceGroupListState;
            rpDeviceGroups.DataBind();
        }

        #endregion

        #region Private Classes

        [Serializable]
        private class NameValueEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public NameValueEntity( int id, string name )
            {
                Id = id;
                Name = name;
            }
        }

        #endregion
    }
}