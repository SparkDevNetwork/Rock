using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Cache;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Notification Group List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Lists notification groups in the system." )]

    [LinkedPage( "Detail Page", "The page that allows the user to edit the details of a notification group.", true, "", "", 0 )]
    public partial class NotificationGroupList : RockBlock
    {
        #region Base Method Overrides

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

            var groupType = GroupTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.GroupType.NOTIFICATION_GROUP );

            gNotificationGroup.DataKeyNames = new string[] { "Id" };
            gNotificationGroup.Actions.AddClick += gNotificationGroup_Add;
            gNotificationGroup.GridRebind += gNotificationGroup_GridRebind;
            gNotificationGroup.Actions.ShowAdd = groupType.IsAuthorized( Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of project types in the system.
        /// </summary>
        private void BindGrid()
        {
            var groupTypeId = GroupTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.GroupType.NOTIFICATION_GROUP.AsGuid() ).Id;
            var groupService = new GroupService( new RockContext() );
            var sortProperty = gNotificationGroup.SortProperty;

            var groups = groupService.Queryable()
                .Where( g => g.GroupTypeId == groupTypeId )
                .ToList()
                .Where( g => g.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .ToList();

            groups.ForEach( g => g.LoadAttributes() );

            var items = groups
                .Select( g => new
                {
                    g.Id,
                    g.Name,
                    Schedule = WatchdogScheduleCollectionCache.Get( g.GetAttributeValue( "ScheduleCollectionId" ).AsInteger() ),
                    CanDelete = g.IsAuthorized( Authorization.EDIT, CurrentPerson )
                } )
                .ToList();

            if ( sortProperty != null )
            {
                items = items.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                items = items.OrderBy( t => t.Name ).ThenBy( t => t.Id ).ToList();
            }

            gNotificationGroup.EntityTypeId = EntityTypeCache.Get<Group>().Id;
            gNotificationGroup.DataSource = items;
            gNotificationGroup.DataBind();
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
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gNotificationGroup_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gNotificationGroup_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( e.RowKeyId );

            if ( group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                groupService.Delete( group );
                rockContext.SaveChanges();

                WatchdogNotificationGroupCache.Remove( e.RowKeyId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gNotificationGroup_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gNotificationGroup_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gNotificationGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gNotificationGroup_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gNotificationGroup.Columns.IndexOf( gNotificationGroup.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( bool ) e.Row.DataItem.GetPropertyValue( "CanDelete" );
            }
        }

        #endregion
    }
}