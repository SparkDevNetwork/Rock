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
using System.Data.Entity;
using Humanizer;
using Rock.Communication;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Moves the person from the wait list to be a full registrant.
    /// </summary>
    [DisplayName( "Registrant Wait List Move" )]
    [Category( "Event" )]
    [Description( "Moves the person from the wait list to be a full registrant." )]
    public partial class RegistrantWaitListMove : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private RegistrationInstance _registrationInstance = null;

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
                RockContext rockContext = new RockContext();

                var entitySetId = PageParameter( "WaitListSetId" ).AsIntegerOrNull();

                if ( entitySetId.HasValue )
                {
                    // get entity set
                    var entitySetItems = new EntitySetItemService( rockContext ).Queryable().Where( s => s.EntitySetId == entitySetId ).ToList();

                    MoveFromWaitList( entitySetItems );

                    // get first item from entity set to get registration information
                    var firstRegistrantId = entitySetItems.First().EntityId;

                    var registrationInstance = new RegistrationRegistrantService( new RockContext() ).Queryable()
                                            .Where( r => r.Id == firstRegistrantId )
                                            .Select( r => r.Registration.RegistrationInstance )
                                            .FirstOrDefault();

                    if ( registrationInstance != null )
                    {
                        tbFromEmail.Text = registrationInstance.ContactEmail;

                        if ( registrationInstance.ContactPersonAlias != null )
                        {
                            tbFromName.Text = registrationInstance.ContactPersonAlias.Person.FullName;
                        }

                        tbFromSubject.Text = string.Format( "Wait List Update for {0}", registrationInstance.Name );
                    }
                }
                else
                {
                    nbUpdate.NotificationBoxType = NotificationBoxType.Warning;
                    nbUpdate.Text = "Could not find an entity set in the query string.";
                }

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

        /// <summary>
        /// Handles the CheckedChanged event of the tglEmailBodyView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglEmailBodyView_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglEmailBodyView.Checked )
            {
                ceEmailMessage.Visible = false;
                ifEmailPreview.Visible = true;

                // reload preview
                int? registrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();

                if ( registrationInstanceId.HasValue )
                {
                    using ( RockContext rockContext = new RockContext() )
                    {
                        RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );
                        _registrationInstance = registrationInstanceService.Queryable( "RegistrationTemplate" ).AsNoTracking()
                                                    .Where( r => r.Id == registrationInstanceId ).FirstOrDefault();


                        var registrationSample = _registrationInstance.Registrations.Where( r => r.BalanceDue > 0 ).FirstOrDefault();

                        if ( registrationSample != null )
                        {
                            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                            mergeObjects.Add( "Registration", registrationSample );
                            mergeObjects.Add( "RegistrationInstance", _registrationInstance );

                            ifEmailPreview.Attributes["srcdoc"] = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );
                        }
                    }
                }
            }
            else
            {
                ceEmailMessage.Visible = true;
                ifEmailPreview.Visible = false;
            }
        }

        protected void cbShowEmail_CheckedChanged( object sender, EventArgs e )
        {
            pnlEmail.Visible = cbShowEmail.Checked;
        }
        #endregion

        #region Methods

        private void MoveFromWaitList(List<EntitySetItem> entitySetItems)
        {
            RockContext rockContext = new RockContext();
            var registrantService = new RegistrationRegistrantService( rockContext );

            foreach ( var item in entitySetItems )
            {
                var registrant = registrantService.Get( item.Id );

                // registrant.OnWaitList = false;

                rockContext.SaveChanges();
            }

            nbUpdate.Text = string.Format( "<strong>Wait List Updated</strong> {0} {1} moved from the wait list to registrants.", "individuals".ToQuantity( entitySetItems.Count ), entitySetItems.Count == 1 ? "was" : "were" );
            
        }

        #endregion        
    }
}