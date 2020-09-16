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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Web.Cache;

namespace RockWeb.Plugins.com_bemadev.RoomReservation
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Requestor Change" )]
    [Category( "com_bemadev > Room Reservation" )]
    [Description( "Template block for developers to use to start a new detail block." )]
    public partial class RequestorChange : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                var personGuid = PageParameter( "PersonGuid" ).AsGuidOrNull();
                if ( personGuid.HasValue )
                {
                    var personAlias = new PersonAliasService( new RockContext() ).Get( personGuid.Value );
                    if ( personAlias != null )
                    {
                        ppOld.SetValue( personAlias.Person );
                    }
                }
                cblMinistry.DataSource = ReservationMinistryCache.All().DistinctBy( rmc => rmc.Name ).OrderBy( m => m.Name );
                cblMinistry.DataBind();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void btnChange_Click( object sender, EventArgs e )
        {
            if ( ppOld.PersonId.HasValue && ppNew.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var oldPerson = personService.Get( ppOld.PersonId.Value );
                    var newPerson = personService.Get( ppNew.PersonId.Value );
                    if ( oldPerson != null && newPerson != null )
                    {
                        try
                        {
                            List<String> roles = cblRole.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value ).ToList();

                            var newPersonAliasId = newPerson.PrimaryAliasId;
                            var oldAliasList = oldPerson.Aliases.Select( pa => pa.Id ).ToList();
                            var reservationService = new ReservationService( rockContext );
                            var reservations = reservationService.Queryable().Where( r =>
                               ( roles.Contains( "Requester" ) && r.RequesterAliasId.HasValue && oldAliasList.Contains( r.RequesterAliasId.Value ) ) ||
                               ( roles.Contains( "EventContact" ) && r.EventContactPersonAliasId.HasValue && oldAliasList.Contains( r.EventContactPersonAliasId.Value ) ) ||
                               ( roles.Contains( "AdminContact" ) && r.AdministrativeContactPersonAliasId.HasValue && oldAliasList.Contains( r.AdministrativeContactPersonAliasId.Value ) )
                                );

                            // Filter by Ministry
                            List<String> ministryNames = cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Text ).ToList();
                            if ( ministryNames.Any() )
                            {
                                reservations = reservations
                                    .Where( r =>
                                        r.ReservationMinistryId.HasValue &&
                                        ministryNames.Contains( r.ReservationMinistry.Name ) );
                            }

                            foreach ( var reservation in reservations )
                            {
                                if ( roles.Contains( "Requester" ) )
                                {
                                    reservation.RequesterAliasId = newPersonAliasId;
                                }
                                if ( roles.Contains( "EventContact" ) )
                                {
                                    reservation.EventContactPersonAliasId = newPersonAliasId;
                                }
                                if ( roles.Contains( "AdminContact" ) )
                                {
                                    reservation.AdministrativeContactPersonAliasId = newPersonAliasId;
                                }
                            }
                            rockContext.SaveChanges();

                            nbMessage.NotificationBoxType = NotificationBoxType.Success;
                            nbMessage.Text = "Transfer Success.";
                            nbMessage.Visible = true;
                        }
                        catch
                        {
                            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                            nbMessage.Text = "An error occurred.";
                            nbMessage.Visible = true;
                        }
                    }
                    else
                    {
                        nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                        nbMessage.Text = "Invalid people.";
                        nbMessage.Visible = true;
                    }
                }
            }
            else
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = "Please select both an old and new requestor";
                nbMessage.Visible = true;
            }
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}