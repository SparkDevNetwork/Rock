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
using System.Text;

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
        private List<RegistrationRegistrant> _registrants = null;
        private List<EntitySetItem> _entitySetItems = null;
        private RockContext _rockContext = new RockContext();
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

            var entitySetId = PageParameter( "WaitListSetId" ).AsIntegerOrNull();

            if ( entitySetId.HasValue )
            {
                // get entity set
                _entitySetItems = new EntitySetItemService( _rockContext ).Queryable().Where( s => s.EntitySetId == entitySetId ).ToList();

                // update wait list status
                var registrantService = new RegistrationRegistrantService( _rockContext );

                var registrantIds = _entitySetItems.Select( s => s.EntityId ).ToList();
                _registrants = registrantService.Queryable( "Registration" ).Where( r => registrantIds.Contains( r.Id ) ).ToList();

                rptRecipients.DataSource = _registrants
                                                    .GroupBy( r => r.Registration )
                                                    .Select( r => new RegistrationSummary { Registration = r.Key, Registrants = r.Key.Registrants.Where( g => registrantIds.Contains( g.Id )).ToList() } );
                rptRecipients.DataBind();
            }
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
                if (_registrants != null) { 

                    foreach ( var registrant in _registrants )
                    {
                        registrant.OnWaitList = false;
                    }

                    _rockContext.SaveChanges();

                    nbUpdate.Text = string.Format( "<strong>Wait List Updated</strong> {0} {1} moved from the wait list.", "individuals".ToQuantity( _entitySetItems.Count ), _entitySetItems.Count == 1 ? "was" : "were" );

                    // get first item from entity set to get registration information
                    var firstRegistrantId = _entitySetItems.First().EntityId;

                    var registration = new RegistrationRegistrantService( new RockContext() ).Queryable("RegistrationInstance")
                                            .Where( r => r.Id == firstRegistrantId )
                                            .Select( r => r.Registration )
                                            .FirstOrDefault();

                    if ( registration != null )
                    {
                        Dictionary<string, object> mergeObjects = GetMergeObjects( registration );
                        
                        tbFromSubject.Text = registration.RegistrationInstance.RegistrationTemplate.WaitListTransitionSubject.ResolveMergeFields( mergeObjects );
                        tbFromName.Text = registration.RegistrationInstance.RegistrationTemplate.WaitListTransitionFromName.ResolveMergeFields( mergeObjects );
                        tbFromEmail.Text = registration.RegistrationInstance.RegistrationTemplate.WaitListTransitionFromEmail.ResolveMergeFields( mergeObjects );
                        ceEmailMessage.Text = registration.RegistrationInstance.RegistrationTemplate.WaitListTransitionEmailTemplate;
                        ifEmailPreview.Attributes["srcdoc"] = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );
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

                if ( _registrants != null )
                {
                    // get first item from entity set to get registration information
                    var firstRegistrantId = _entitySetItems.First().EntityId;

                    var registration = new RegistrationRegistrantService( new RockContext() ).Queryable( "RegistrationInstance" )
                                            .Where( r => r.Id == firstRegistrantId )
                                            .Select( r => r.Registration )
                                            .FirstOrDefault();

                    if ( registration != null )
                    {
                        Dictionary<string, object> mergeObjects = GetMergeObjects( registration );

                        ifEmailPreview.Attributes["srcdoc"] = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );
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

        /// <summary>
        /// Handles the ItemDataBound event of the rptRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptRecipients_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var registrationSummary = (RegistrationSummary)e.Item.DataItem;

                StringBuilder recipientText = new StringBuilder();
                recipientText.Append( string.Format( "{0} {1} <small>({2})</small><br />"
                                        , registrationSummary.Registration.FirstName
                                        , registrationSummary.Registration.LastName
                                        , registrationSummary.Registration.ConfirmationEmail ) );

                recipientText.Append( string.Format( "<small>Registrants: {0}</small>"
                                        , string.Join( ", ", registrationSummary.Registrants.Select( r => r.Person.FullName ) ) ) );

                CheckBox cbEmailRecipient = (CheckBox)e.Item.FindControl( "cbEmailRecipient" );

                if ( cbEmailRecipient != null )
                {
                    cbEmailRecipient.Text = recipientText.ToString();
                    cbEmailRecipient.ClientIDMode = ClientIDMode.Static;
                    cbEmailRecipient.Attributes.Add( "Id", registrationSummary.Registration.Id.ToString() );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSendEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendEmail_Click( object sender, EventArgs e )
        {
            var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "ExternalApplicationRoot" );
            int sendCount = 0;

            foreach ( RepeaterItem repeaterItem in rptRecipients.Items )
            {
                CheckBox cbEmailRecipient = (CheckBox)repeaterItem.FindControl( "cbEmailRecipient" );

                if ( cbEmailRecipient  != null )
                {
                    if ( cbEmailRecipient.Checked )
                    {
                        int? registrationId = cbEmailRecipient.Attributes["Id"].AsIntegerOrNull();

                        if ( registrationId.HasValue )
                        {
                            var registration = _registrants.Where( r => r.RegistrationId == registrationId ).Select( r => r.Registration ).FirstOrDefault();

                            var mergeObjects = GetMergeObjects( registration );

                            var recipients = new List<string>();
                            recipients.Add( registration.ConfirmationEmail );

                            string message = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );

                            Email.Send( tbFromEmail.Text, tbFromName.Text, tbFromSubject.Text, recipients, message, appRoot );

                            sendCount++;
                        }
                    }
                }
            }

            pnlSend.Visible = false;
            pnlComplete.Visible = true;
            nbResult.Text = string.Format( "Wait List Transition emails have been sent to {0}.", "individuals".ToQuantity( sendCount ) );
        }
        #endregion

        #region Methods

        private Dictionary<string, object> GetMergeObjects(Registration registration )
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "RegistrationInstance", registration.RegistrationInstance );
            mergeObjects.Add( "Registration", registration );
            mergeObjects.Add( "TransitionedRegistrants", _registrants.Where( r => r.RegistrationId == registration.Id ).ToList() );

            bool additionalFieldsNeeded = false;

            foreach ( var forms in registration.RegistrationInstance.RegistrationTemplate.Forms )
            {
                foreach ( var field in forms.Fields )
                {
                    if ( !field.ShowOnWaitlist )
                    {
                        additionalFieldsNeeded = true;
                        break;
                    }
                }
            }

            mergeObjects.Add( "AdditionalFieldsNeeded", additionalFieldsNeeded );

            return mergeObjects;
        }
        
        #endregion


        /// <summary>
        /// Summary Class for Registrants
        /// </summary>
        public class RegistrationSummary
        {
            /// <summary>
            /// Gets or sets the registration.
            /// </summary>
            /// <value>
            /// The registration.
            /// </value>
            public Registration Registration { get; set; }

            /// <summary>
            /// Gets or sets the registrants.
            /// </summary>
            /// <value>
            /// The registrants.
            /// </value>
            public List<RegistrationRegistrant> Registrants { get; set; }
        }
    }
}