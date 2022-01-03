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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Follow
{
    /// <summary>
    /// Block for viewing list of following events
    /// </summary>
    [DisplayName( "Event List" )]
    [Category( "Follow" )]
    [Description( "Block for viewing list of following events." )]
    [LinkedPage( "Detail Page" )]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to change following events." )]
    public partial class EventList : RockBlock, ICustomGridColumns
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            bool canAdministrate = IsUserAuthorized( Authorization.ADMINISTRATE );

            rGridEvent.DataKeyNames = new string[] { "Id" };
            rGridEvent.Actions.ShowAdd = canEdit;
            rGridEvent.Actions.AddClick += rGridEvent_Add;
            rGridEvent.GridReorder += rGridEvent_GridReorder;
            rGridEvent.GridRebind += rGridEvents_GridRebind;
            rGridEvent.IsDeleteEnabled = canEdit;

            var securityField = rGridEvent.ColumnsOfType<SecurityField>().FirstOrDefault();

            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( FollowingEventType ) ).Id;
                securityField.Visible = canAdministrate;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the rGridEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridEvent_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "EventId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Edit event of the rGridEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridEvent_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "EventId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Delete event of the rGridEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridEvent_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var eventService = new FollowingEventTypeService( rockContext );
            var followingEvent = eventService.Get( e.RowKeyId );
            if ( followingEvent != null )
            {
                string errorMessage;
                if ( !eventService.CanDelete( followingEvent, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                eventService.Delete( followingEvent );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGridEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGridEvent_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new FollowingEventTypeService( rockContext );
            var followingEvents = service.Queryable().OrderBy( i => i.Order ).ToList();
            service.Reorder( followingEvents, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridEvents_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the Event list grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new FollowingEventTypeService( rockContext )
                    .Queryable( "EntityType" ).AsNoTracking()
                    .OrderBy( e => e.Order );

                rGridEvent.DataSource = qry.ToList();
                rGridEvent.DataBind();
            }
        }

        protected string GetComponentName( object entityTypeObject )
        {
            var entityType = entityTypeObject as EntityType;
            if ( entityType != null )
            {
                string name = Rock.Follow.EventContainer.GetComponentName( entityType.Name );
                if ( !string.IsNullOrWhiteSpace(name))
                {
                    return name.SplitCase();
                }
            }

            return string.Empty;
        }

        #endregion
    }
}