﻿// <copyright>
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
using Rock.Communication;
using System.Data.Entity;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Block to build a list of people who should receive a photo request.
    /// </summary>
    [DisplayName( "Send Photo Request" )]
    [Category( "CRM > PhotoRequest" )]
    [Description( "Block for selecting criteria to build a list of people who should receive a photo request." )]
    [CommunicationTemplateField( "Photo Request Template", "The template to use with this block to send requests.", true, "B9A0489C-A823-4C5C-A9F9-14A206EC3B88" )]
    [IntegerField( "Maximum Recipients", "The maximum number of recipients allowed before communication will need to be approved", false, 300 )]
    public partial class PhotoSendRequest : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the communication data.
        /// </summary>
        /// <value>
        /// The communication data.
        /// </value>
        protected CommunicationDetails CommunicationData
        {
            get
            {
                var communicationData = ViewState["CommunicationData"] as CommunicationDetails;
                if ( communicationData == null )
                {
                    communicationData = new CommunicationDetails();
                    ViewState["CommunicationData"] = communicationData;
                }
                return communicationData;
            }

            set { ViewState["CommunicationData"] = value; }
        }

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

            nbError.Visible = false;
            nbTestResult.Visible = false;
            nbConfirmMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                BindCheckBoxLists();
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
        /// Handles the Click event of the btnTest control and sends a test communication to the
        /// current person if they have an email address on their record.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTest_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CurrentPerson != null )
            {
                SetActionButtons( resetToSend: true );

                if ( string.IsNullOrWhiteSpace( CurrentPerson.Email ) )
                {
                    nbError.Text = "A test email cannot be sent because you do not have an email address.";
                    nbError.Visible = true;
                    return;
                }

                // Get existing or new communication record
                var communication = GetCommunication( new RockContext(), null );

                if ( communication != null && CurrentPersonAliasId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {

                        // Using a new context (so that changes in the UpdateCommunication() are not persisted )
                        var testCommunication = communication.Clone( false );
                        testCommunication.Id = 0;
                        testCommunication.Guid = Guid.Empty;
                        testCommunication.EnabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );
                        testCommunication.ForeignGuid = null;
                        testCommunication.ForeignId = null;
                        testCommunication.ForeignKey = null;

                        testCommunication.FutureSendDateTime = null;
                        testCommunication.Status = CommunicationStatus.Approved;
                        testCommunication.ReviewedDateTime = RockDateTime.Now;
                        testCommunication.ReviewerPersonAliasId = CurrentPersonAliasId.Value;

                        var testRecipient = new CommunicationRecipient();
                        if ( communication.Recipients.Any() )
                        {
                            var recipient = communication.Recipients.FirstOrDefault();
                            testRecipient.AdditionalMergeValuesJson = recipient.AdditionalMergeValuesJson;
                        }
                        testRecipient.Status = CommunicationRecipientStatus.Pending;
                        testRecipient.PersonAliasId = CurrentPersonAliasId.Value;
                        testRecipient.MediumEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;
                        testCommunication.Recipients.Add( testRecipient );

                        var communicationService = new CommunicationService( rockContext );
                        communicationService.Add( testCommunication );
                        rockContext.SaveChanges();

                        Communication.Send( testCommunication );

                        communicationService.Delete( testCommunication );
                        rockContext.SaveChanges();
                    }

                    nbTestResult.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSend control and displays the "confirm" button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            nbTestResult.Visible = false;
            var people = GetMatchingPeople().ToList();
            CheckApprovalRequired( people.Count );
            if ( people.Count > 0 )
            {
                SetActionButtons( false );
                nbConfirmMessage.Title = "Please Confirm";
                nbConfirmMessage.NotificationBoxType = NotificationBoxType.Info;
                nbConfirmMessage.Text = string.Format( "This will send an email to {0:#,###,###,##0} recipients. Press confirm to continue.", people.Count );
            }
            else
            {
                SetActionButtons( true );
                nbConfirmMessage.Visible = true;
                nbConfirmMessage.NotificationBoxType = NotificationBoxType.Warning;
                nbConfirmMessage.Title = "Warning";
                nbConfirmMessage.Text = string.Format( "Hmm, that didn't match anyone. Try adjusting your criteria.", people.Count );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control and resets the form back to the pre-confirm send state.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetActionButtons( resetToSend: true );
        }

        /// <summary>
        /// Handles the Click event of the btnSendConfirmed control which sends the actual communication.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendConfirmed_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CurrentPerson != null )
            {
                SetActionButtons( resetToSend: true );

                var people = GetMatchingPeople();

                // Get new communication record
                var rockContext = new RockContext();
                var communication = GetCommunication( rockContext, people.Select( p => p.Id ).ToList() );

                if ( communication != null )
                {
                    string message = string.Empty;

                    if ( CheckApprovalRequired( communication.Recipients.Count ) && !IsUserAuthorized( "Approve" ) )
                    {
                        communication.Status = CommunicationStatus.PendingApproval;
                        message = "Communication has been submitted for approval.";
                    }
                    else
                    {
                        communication.Status = CommunicationStatus.Approved;
                        communication.ReviewedDateTime = RockDateTime.Now;
                        communication.ReviewerPersonAliasId = CurrentPersonAliasId;
                        message = "Communication has been queued for sending.";
                    }

                    rockContext.SaveChanges();

                    if ( communication.Status == CommunicationStatus.Approved )
                    {
                        var transaction = new Rock.Transactions.SendCommunicationTransaction();
                        transaction.CommunicationId = communication.Id;
                        transaction.PersonAlias = CurrentPersonAlias;
                        Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                    }

                    pnlSuccess.Visible = true;
                    pnlForm.Visible = false;
                    nbSuccess.Text = message;
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Determines whether approval is required, and sets the submit button text appropriately
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns>
        ///   <c>true</c> if approval is required for the communication; otherwise, <c>false</c>.
        /// </returns>
        private bool CheckApprovalRequired( int numberOfRecipients )
        {
            int maxRecipients = int.MaxValue;
            int.TryParse( GetAttributeValue( "MaximumRecipients" ), out maxRecipients );
            bool approvalRequired = numberOfRecipients > maxRecipients;

            btnSendConfirmed.Text = ( approvalRequired && !IsUserAuthorized( "Approve" ) ? "Confirm and Submit" : "Confirm and Send" ) + " Communication";

            return approvalRequired;
        }

        /// <summary>
        /// Sets the action buttons to the reset state (pre confirm) or the confirm state.
        /// </summary>
        /// <param name="resetToSend">if set to <c>true</c> [reset to send].</param>
        private void SetActionButtons( bool resetToSend )
        {
            btnSendConfirmed.Visible = !resetToSend;
            nbConfirmMessage.Visible = !resetToSend;
            btnCancel.Visible = !resetToSend;
            //btnSend.Visible = resetToSend;
            if ( resetToSend )
            {
                btnSend.RemoveCssClass( "hidden" );
            }
            else
            {
                btnSend.AddCssClass( "hidden" );
            }

            nbTestResult.Visible = false;
        }

        /// <summary>
        /// Gets the matching people for the criteria on the form.
        /// </summary>
        /// <returns>a queryable containing the matching people</returns>
        private IQueryable<Person> GetMatchingPeople()
        {
            PersonService personService = new PersonService( new RockContext() );

            var photoRequestGroup = Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid();

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            List<int> selectedRoleIds = cblRoles.SelectedValuesAsInt;

            var selectedConnectionStatuses = dvpConnectionStatus.SelectedValuesAsInt;
            var ageBirthDate = RockDateTime.Now.AddYears( -nbAge.Text.AsInteger() );
            var photoUpdatedDate = RockDateTime.Now.AddYears( - nbUpdatedLessThan.Text.AsInteger() );
            var people = personService.Queryable( false, false ).Include( p => p.Members );

            // people opted out (or pending)
            var peopleOptedOut = personService.Queryable( false, false ).Include( p => p.Members )
                .Where
                (
                     p => p.Members.Where( gm => gm.Group.Guid == photoRequestGroup
                        && ( gm.GroupMemberStatus == GroupMemberStatus.Inactive || gm.GroupMemberStatus == GroupMemberStatus.Pending ) ).Any()
                );

            // people who have emails addresses
            people = people.Where( p => ! ( p.Email == null || p.Email.Trim() == string.Empty ) );

            // people who match the Connection Status criteria
            people = people.Where( p => dvpConnectionStatus.SelectedValuesAsInt.Contains( p.ConnectionStatusValueId ?? -1 ) );

            // people who are old enough
            people = people.Where( p => p.BirthDate <= ageBirthDate );

            // people who are in the matching role for a family group
            people = people.Where( p => p.Members.Where( gm => gm.Group.GroupTypeId == familyGroupType.Id
                && selectedRoleIds.Contains( gm.GroupRoleId ) ).Any() );

            // photo is null or photo is older than our criteria
            people = people.Where( p => p.PhotoId == null || p.Photo.ModifiedDateTime == null || p.Photo.ModifiedDateTime <= photoUpdatedDate );

            // except people who are in the photo-opt-out group as inactive.
            people = people.Except( peopleOptedOut );

            return people;
        }

        /// <summary>
        /// Gets the communication.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="peopleIds">The people ids.</param>
        /// <returns></returns>
        private Rock.Model.Communication GetCommunication( RockContext rockContext, List<int> peopleIds )
        {
            var communicationService = new CommunicationService(rockContext);
            var recipientService = new CommunicationRecipientService(rockContext);

            if ( GetTemplateData() )
            {
                Rock.Model.Communication communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Transient;
                communication.SenderPersonAliasId = CurrentPersonAliasId;
                communicationService.Add( communication );
                communication.IsBulkCommunication = true;
                communication.CommunicationType = CommunicationType.Email;
                communication.FutureSendDateTime = null;

                // add each person as a recipient to the communication
                if ( peopleIds != null )
                {
                    var emailMediumTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;

                    foreach ( var personId in peopleIds )
                    {
                        if ( !communication.Recipients.Any( r => r.PersonAlias.PersonId == personId ) )
                        {
                            var communicationRecipient = new CommunicationRecipient();
                            communicationRecipient.PersonAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( personId );
                            communicationRecipient.MediumEntityTypeId = emailMediumTypeId;
                            communication.Recipients.Add( communicationRecipient );
                        }
                    }
                }

                CommunicationDetails.Copy( CommunicationData, communication );

                return communication;
            }

            return null;
        }

        /// <summary>
        /// Gets the template data.
        /// </summary>
        /// <exception cref="System.Exception">Missing communication template configuration.</exception>
        private bool GetTemplateData()
        {
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "PhotoRequestTemplate" ) ) )
            {
                nbError.Title = "Configuration Error";
                nbError.Text = "Missing communication template configuration.";
                nbError.Visible = true;
                return false;
            }

            var template = new CommunicationTemplateService( new RockContext() ).Get( GetAttributeValue( "PhotoRequestTemplate" ).AsGuid() );
            if ( template == null )
            {
                nbError.Title = "Configuration Error";
                nbError.Text = "The communication template appears to be missing.";
                nbError.Visible = true;
                return false;
            }

            CommunicationDetails.Copy( template, CommunicationData );

            return true;
        }

        /// <summary>
        /// Binds the checkbox lists.
        /// </summary>
        private void BindCheckBoxLists()
        {
            // roles...
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            cblRoles.DataSource = familyGroupType.Roles;
            cblRoles.DataBind();

            // connection status...
            // NOTE: if we want to bind and preselect one or more items based on an attribute, we can do it like this
            //var statuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).DefinedValues
            //    .OrderBy( v => v.Order )
            //    .ThenBy( v => v.Value )
            //    .Select( v => new ListItem
            //    {
            //        Value = v.Id.ToString(),
            //        Text = v.Value.Pluralize(),
            //        Selected = ( v.GetAttributeValue( "PreSelected" ) ).AsBoolean(false)
            //    } )
            //    .ToList();

            //foreach ( var item in statuses)
            //{
            //    cblConnectionStatus.Items.Add( item );
            //}

            // otherwise we can just bind like this for now
            dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;
        }

        #endregion

}
}
