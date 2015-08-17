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
    /// "Block for users to select which following events they would like to subscribe to."
    /// </summary>
    [DisplayName( "Event Subscription" )]
    [Category( "Follow" )]
    [Description( "Block for users to select which following events they would like to subscribe to." )]
    public partial class EventSubscription : RockBlock
    {

        #region Fields

        private RockContext _rockContext = null;
        protected List<int> _currentSubscriptions = new List<int>();

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbSaved.Visible = false;
            _rockContext = new RockContext();

            if ( !Page.IsPostBack && CurrentPersonId.HasValue && CurrentPersonAliasId.HasValue )
            {
                GetData();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetData();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemDataBound event of the rptEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptEntityType_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var rptEvent = e.Item.FindControl( "rptEvent" ) as Repeater;
            var followedEntityType = e.Item.DataItem as EntityType;
            if ( rptEvent != null && followedEntityType != null )
            {
                var qry = new FollowingEventTypeService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( f =>
                        f.FollowedEntityTypeId.HasValue &&
                        f.FollowedEntityTypeId.Value == followedEntityType.Id &&
                        f.IsActive )
                    .OrderBy( f => f.Name );

                rptEvent.DataSource = qry
                    .Select( f => new
                        {
                            f.Id,
                            f.IsNoticeRequired,
                            Name = f.IsNoticeRequired ? f.Name + " (required)" : f.Name,
                            f.Description,
                            Selected = f.IsNoticeRequired || _currentSubscriptions.Contains( f.Id )
                        } )
                    .ToList();
                rptEvent.DataBind();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( CurrentPersonId.HasValue && CurrentPersonAliasId.HasValue )
            {
                var service = new FollowingEventSubscriptionService( _rockContext );
                var existingSubscriptions = service.Queryable()
                    .Where( s => s.PersonAlias.PersonId == CurrentPersonId )
                    .ToList();

                foreach ( RepeaterItem entityTypeItem in rptEntityType.Items )
                {
                    var rptEvent = entityTypeItem.FindControl( "rptEvent" ) as Repeater;
                    if ( rptEvent != null )
                    {
                        foreach ( RepeaterItem eventItem in rptEvent.Items )
                        {
                            var hfEvent = eventItem.FindControl( "hfEvent" ) as HiddenField;
                            var cbEvent = eventItem.FindControl( "cbEvent" ) as RockCheckBox;
                            if ( hfEvent != null && cbEvent != null )
                            {
                                int eventTypeId = hfEvent.ValueAsInt();
                                if ( cbEvent.Checked )
                                {
                                    if ( !existingSubscriptions.Any( s => s.EventTypeId == eventTypeId ) )
                                    {
                                        var subscription = new FollowingEventSubscription();
                                        subscription.EventTypeId = eventTypeId;
                                        subscription.PersonAliasId = CurrentPersonAliasId.Value;
                                        service.Add( subscription );
                                    }
                                }
                                else
                                {
                                    foreach ( var subscription in existingSubscriptions
                                        .Where( s => s.EventTypeId == eventTypeId ) )
                                    {
                                        service.Delete( subscription );
                                    }
                                }
                            }
                        }
                    }
                }

                _rockContext.SaveChanges();
                nbSaved.Visible = true;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        private void GetData()
        {
            if ( CurrentPersonId.HasValue )
            {
                _currentSubscriptions = new FollowingEventSubscriptionService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => s.PersonAlias.PersonId == CurrentPersonId )
                    .Select( s => s.EventTypeId )
                    .Distinct()
                    .ToList();
            }

            var qry = new FollowingEventTypeService( _rockContext )
                .Queryable().AsNoTracking()
                .Where( e => 
                    e.IsActive && 
                    e.FollowedEntityType != null )
                .Select( e => e.FollowedEntityType )
                .OrderBy( e => e.Name )
                .Distinct();

            rptEntityType.DataSource = qry.ToList();
            rptEntityType.DataBind();
        }

        #endregion

}
}