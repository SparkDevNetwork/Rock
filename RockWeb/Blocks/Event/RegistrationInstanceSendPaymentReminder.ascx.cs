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
    /// Sends payment reminders for paid registrations that have a remaining balance.
    /// </summary>
    [DisplayName( "Registration Instance Send Payment Reminder" )]
    [Category( "Event" )]
    [Description( "Sends payment reminders for paid registrations that have a remaining balance." )]
    public partial class RegistrationInstancePaymentReminder : Rock.Web.UI.RockBlock
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
            gRegistrations.GridRebind += gRegistrations_GridRebind;
            gRegistrations.RowDataBound += gRegistrations_RowDataBound;
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
                LoadData();
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
        /// Handles the GridRebind event of the GRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void gRegistrations_GridRebind( object sender, EventArgs e )
        {
            LoadOutstandingBalances();
        }

        /// <summary>
        /// Handles the Click event of the btnSendReminders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendReminders_Click( object sender, EventArgs e )
        {
            var registrationsSelected = new List<int>();

            int sendCount = 0;

            gRegistrations.SelectedKeys.ToList().ForEach( r => registrationsSelected.Add( r.ToString().AsInteger() ) );
            if ( registrationsSelected.Any() )
            {
                var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "PublicApplicationRoot" );
                
                if ( _registrationInstance == null )
                {
                    int? registrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();

                    using ( RockContext rockContext = new RockContext() )
                    {
                        RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );
                        _registrationInstance = registrationInstanceService.Queryable( "RegistrationTemplate" ).AsNoTracking()
                                                .Where( r => r.Id == registrationInstanceId ).FirstOrDefault();
                    }

                    foreach( var registrationId in registrationsSelected )
                    {
                        // use a new rockContext for each registration so that ChangeTracker doesn't get bogged down
                        using ( RockContext rockContext = new RockContext() )
                        {
                            var registrationService = new RegistrationService( rockContext );

                            var registration = registrationService.Get( registrationId );
                            if ( registration != null && !string.IsNullOrWhiteSpace(registration.ConfirmationEmail) )
                            {
                                Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                                mergeObjects.Add( "Registration", registration );
                                mergeObjects.Add( "RegistrationInstance", _registrationInstance );

                                var emailMessage = new RockEmailMessage( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid() );
                                emailMessage.AdditionalMergeFields = mergeObjects;
                                emailMessage.FromEmail = txtFromEmail.Text;
                                emailMessage.FromName = txtFromName.Text;
                                emailMessage.Subject = txtFromSubject.Text;
                                emailMessage.AddRecipient( new RecipientData( registration.ConfirmationEmail, mergeObjects ) );
                                emailMessage.Message = ceEmailMessage.Text;
                                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                                emailMessage.CreateCommunicationRecord = false;
                                emailMessage.Send();

                                registration.LastPaymentReminderDateTime = RockDateTime.Now;
                                rockContext.SaveChanges();

                                sendCount++;
                            }
                        }
                    }
                }
            }

            pnlSend.Visible = false;
            pnlComplete.Visible = true;
            nbResult.Text = string.Format("Payment reminders have been sent to {0}.", "individuals".ToQuantity( sendCount ));
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gRegistrations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var row = e.Row.DataItem as Registration;
                var cell = e.Row.Cells[gRegistrations.Columns.OfType<SelectField>().First().ColumnIndex];
                var selectBox = cell.Controls[0] as CheckBox;

                if ( row != null )
                {
                    if ( row.LastPaymentReminderDateTime.HasValue && _registrationInstance.RegistrationTemplate.PaymentReminderTimeSpan.HasValue )
                    {
                        var daysSinceLastReminder = (int)Math.Ceiling( ((DateTime)RockDateTime.Now - row.LastPaymentReminderDateTime.Value).TotalDays );

                        if (daysSinceLastReminder >= _registrationInstance.RegistrationTemplate.PaymentReminderTimeSpan.Value )
                        {
                            selectBox.Checked = true;
                        }
                        else
                        {
                            e.Row.AddCssClass( "is-inactive" );
                        }
                    }
                }
            }
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

                        // NOTE: Do not use AsNoTracking because lava might need to lazy load some stuff
                        _registrationInstance = registrationInstanceService.Queryable( "RegistrationTemplate" )
                                                    .Where( r => r.Id == registrationInstanceId ).FirstOrDefault();


                        var registrationSample = _registrationInstance.Registrations.Where( r => r.BalanceDue > 0 ).FirstOrDefault();

                        if ( registrationSample != null )
                        {
                            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                            mergeObjects.Add( "Registration", registrationSample );
                            mergeObjects.Add( "RegistrationInstance", _registrationInstance );

                            ifEmailPreview.Attributes["srcdoc"] = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );
                            
                            // needed to work in IE
                            ifEmailPreview.Src = "javascript: window.frameElement.getAttribute('srcdoc');";
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
        #endregion

        #region Methods

        /// <summary>
        /// Loads the registration.
        /// </summary>
        private void LoadData()
        {
            int? registrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                using ( RockContext rockContext = new RockContext() )
                {
                    RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );

                    // NOTE: Do not use AsNoTracking because lava might need to lazy load some stuff
                    _registrationInstance = registrationInstanceService.Queryable( "RegistrationTemplate" )
                                                .Where( r => r.Id == registrationInstanceId ).FirstOrDefault();


                    var registrationSample = _registrationInstance.Registrations.Where( r => r.BalanceDue > 0 ).FirstOrDefault();

                    if ( registrationSample != null )
                    {
                        Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                        mergeObjects.Add( "Registration", registrationSample );
                        mergeObjects.Add( "RegistrationInstance", _registrationInstance );

                        ceEmailMessage.Text = _registrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate;

                        ifEmailPreview.Attributes["srcdoc"] = _registrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate.ResolveMergeFields( mergeObjects );

                        // needed to work in IE
                        ifEmailPreview.Src = "javascript: window.frameElement.getAttribute('srcdoc');";

                        txtFromEmail.Text = _registrationInstance.RegistrationTemplate.PaymentReminderFromEmail.ResolveMergeFields( mergeObjects );
                        txtFromName.Text = _registrationInstance.RegistrationTemplate.PaymentReminderFromName.ResolveMergeFields( mergeObjects );
                        txtFromSubject.Text = _registrationInstance.RegistrationTemplate.PaymentReminderSubject.ResolveMergeFields( mergeObjects );

                        if ( _registrationInstance.RegistrationTemplate.PaymentReminderTimeSpan.HasValue )
                        {
                            lBalanceInstructions.Text = string.Format( "<p>Below is a list of registrations with outstanding balances. Individuals who have not been reminded of their balance in {0} days have been pre-selected. Those who have been recently added or notified of their balance are greyed out. They can be still be included by either selecting them or selecting all transactions.</p>", _registrationInstance.RegistrationTemplate.PaymentReminderTimeSpan.Value );
                        }
                        
                    }
                    else
                    {
                        pnlPreview.Visible = false;
                        nbMessages.Text = "<strong>Good News!</strong> No registrations have an outstanding balance at this time.";
                    }

                    LoadOutstandingBalances();
                }
            }
        }

        /// <summary>
        /// Loads the outstanding balances.
        /// </summary>
        private void LoadOutstandingBalances()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                if ( _registrationInstance == null )
                {
                    int? registrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();

                    RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );
                    _registrationInstance = registrationInstanceService.Queryable( "RegistrationTemplate" ).AsNoTracking()
                                                .Where( r => r.Id == registrationInstanceId ).FirstOrDefault();
                }

                var outstandingBalances = _registrationInstance.Registrations.Where( r => r.BalanceDue > 0 );

                var sortProperty = gRegistrations.SortProperty;

                if ( sortProperty != null )
                {
                    switch ( sortProperty.Property )
                    {
                        case "BalanceDue":
                            {
                                if ( sortProperty.Direction == SortDirection.Ascending )
                                {
                                    outstandingBalances = outstandingBalances.OrderBy( b => b.BalanceDue );
                                }
                                else
                                {
                                    outstandingBalances = outstandingBalances.OrderByDescending( b => b.BalanceDue );
                                }

                                break;
                            }
                        case "LastPaymentReminderDateTime":
                            {
                                if ( sortProperty.Direction == SortDirection.Ascending )
                                {
                                    outstandingBalances = outstandingBalances.OrderBy( b => b.LastPaymentReminderDateTime );
                                }
                                else
                                {
                                    outstandingBalances = outstandingBalances.OrderByDescending( b => b.LastPaymentReminderDateTime );
                                }

                                break;
                            }
                        case "CreatedDateTime":
                            {
                                if ( sortProperty.Direction == SortDirection.Ascending )
                                {
                                    outstandingBalances = outstandingBalances.OrderBy( b => b.CreatedDateTime );
                                }
                                else
                                {
                                    outstandingBalances = outstandingBalances.OrderByDescending( b => b.CreatedDateTime );
                                }

                                break;
                            }
                        case "Name":
                            {
                                if ( sortProperty.Direction == SortDirection.Ascending )
                                {
                                    outstandingBalances = outstandingBalances.OrderBy( b => b.LastName ).ThenBy(b=> b.FirstName);
                                }
                                else
                                {
                                    outstandingBalances = outstandingBalances.OrderByDescending( b => b.LastName ).ThenBy( b => b.FirstName );
                                }

                                break;
                            }
                    }

                }

                gRegistrations.DataSource = outstandingBalances;
                gRegistrations.DataBind();
            }
        }

        /// <summary>
        /// Dayses the since last reminder.
        /// </summary>
        /// <param name="lastReminderDate">The last reminder date.</param>
        /// <returns></returns>
        protected string DaysSinceLastReminder(DateTime? lastReminderDate )
        {
            if ( lastReminderDate.HasValue )
            {
                var days = ((DateTime)RockDateTime.Now - lastReminderDate.Value ).TotalDays;
                if (days < 1 )
                {
                    return "Today";
                }
                return  "days".ToQuantity( (int)Math.Ceiling( days) );
            }
            else
            {
                return "Unknown";
            }
        }

        #endregion        
    }
}