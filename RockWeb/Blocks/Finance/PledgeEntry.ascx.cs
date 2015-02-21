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
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Pledge Entry" )]
    [Category( "Finance" )]
    [Description( "Allows a website visitor to create pledge for the configured accounts, start and end date. This block also creates a new person record if a matching person could not be found." )]

    [BooleanField( "Enable Smart Names", "Check the first name for 'and' and '&' and split it to just use the first name provided.", true, Order = 1 )]
    [AccountField( "Account", "The account that new pledges will be allocated toward", true, Rock.SystemGuid.FinancialAccount.GENERAL_FUND, "", Order = 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "New Connection Status", "Person connection status to assign to a new user.", true, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT, Order = 3 )]
    [DateRangeField( "Pledge Date Range", "Date range of the pledge.", false, Order = 4 )]

    [BooleanField( "Show Pledge Frequency", "Show the pledge frequency option to the user.", DefaultValue = "false", Order = 5 )]
    [BooleanField( "Require Pledge Frequency", "Require that a user select a specific pledge frequency (when pledge frequency is shown)", DefaultValue = "false", Order = 6 )]

    [TextField( "Save Button Text", "The Text to shown on the Save button", true, "Save", Order = 7 )]
    [TextField( "Note Message", "Message to show at the bottom of the create pledge block.", false, "Note: This commitment is a statement of intent and may be changed as your circumstances change.", Order = 8 )]

    [CodeEditorField( "Receipt Text", "The text (or html) to display as the pledge receipt. <span class='tip tip-lava'></span> <span class='tip tip-html'>", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, Order = 9, DefaultValue =
        @"
<h1>Thank You!</h1>
<p>
{{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.
</p>
<p>
    <a href='~/page/186?PledgeId={{ FinancialPledge.Id  }}' class='btn btn-default' >Setup a Giving Profile</a>
</p>
" )]

    [SystemEmailField( "Confirmation Email Template", "Email template to use after submitting a new pledge. Leave blank to not send an email.", false, Rock.SystemGuid.SystemEmail.FINANCE_PLEDGE_CONFIRMATION, Order = 10 )]
    [BooleanField( "Enable Debug", "Outputs the object graph to help create your liquid syntax.", false, Order = 11 )]
    public partial class PledgeEntry : RockBlock
    {
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

            if ( !IsPostBack )
            {
                ShowForm();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowForm();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var financialPledgeService = new FinancialPledgeService( rockContext );
            var financialAccountService = new FinancialAccountService( rockContext );
            var definedValueService = new DefinedValueService( rockContext );
            var person = FindPerson( rockContext );

            FinancialPledge financialPledge = new FinancialPledge();

            financialPledge.PersonAliasId = person.PrimaryAliasId;
            var financialAccount = financialAccountService.Get( GetAttributeValue( "Account" ).AsGuid() );
            if ( financialAccount != null )
            {
                financialPledge.AccountId = financialAccount.Id;
            }

            financialPledge.TotalAmount = tbTotalAmount.Text.AsDecimal();

            var pledgeFrequencySelection = DefinedValueCache.Read( bddlFrequency.SelectedValue.AsInteger() );
            if ( pledgeFrequencySelection != null )
            {
                financialPledge.PledgeFrequencyValueId = pledgeFrequencySelection.Id;
            }

            financialPledge.StartDate = drpDateRange.LowerValue ?? DateTime.MinValue;
            financialPledge.EndDate = drpDateRange.UpperValue ?? DateTime.MaxValue;

            if ( sender != btnConfirm )
            {
                var duplicatePledges = financialPledgeService.Queryable()
                    .Where( a => a.PersonAlias.PersonId == person.Id )
                    .Where( a => a.AccountId == financialPledge.AccountId )
                    .Where( a => a.StartDate == financialPledge.StartDate )
                    .Where( a => a.EndDate == financialPledge.EndDate ).ToList();

                if ( duplicatePledges.Any() )
                {
                    pnlAddPledge.Visible = false;
                    pnlConfirm.Visible = true;
                    nbDuplicatePledgeWarning.Text = "The following pledges have already been entered for you:";
                    nbDuplicatePledgeWarning.Text += "<ul>";
                    foreach ( var pledge in duplicatePledges.OrderBy( a => a.StartDate ).ThenBy( a => a.Account.Name ) )
                    {
                        nbDuplicatePledgeWarning.Text += string.Format( "<li>{0} {1} {2}</li>", pledge.Account, pledge.PledgeFrequencyValue, pledge.TotalAmount );
                    }

                    nbDuplicatePledgeWarning.Text += "</ul>";

                    return;
                }
            }

            financialPledgeService.Add( financialPledge );

            rockContext.SaveChanges();

            // populate account so that Liquid can access it
            financialPledge.Account = financialAccount;

            // populate PledgeFrequencyValue so that Liquid can access it
            financialPledge.PledgeFrequencyValue = definedValueService.Get( financialPledge.PledgeFrequencyValueId ?? 0 );

            var mergeObjects = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( this.CurrentPerson );
            mergeObjects.Add( "Person", person );
            mergeObjects.Add( "FinancialPledge", financialPledge );
            mergeObjects.Add( "PledgeFrequency", pledgeFrequencySelection );
            mergeObjects.Add( "Account", financialAccount );
            lReceipt.Text = GetAttributeValue( "ReceiptText" ).ResolveMergeFields( mergeObjects );

            // Resolve any dynamic url references
            string appRoot = ResolveRockUrl( "~/" );
            string themeRoot = ResolveRockUrl( "~~/" );
            lReceipt.Text = lReceipt.Text.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            // show liquid help for debug
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lReceipt.Text += mergeObjects.lavaDebugInfo();
            }

            lReceipt.Visible = true;
            pnlAddPledge.Visible = false;
            pnlConfirm.Visible = false;

            // if a ConfirmationEmailTemplate is configured, send an email
            var confirmationEmailTemplateGuid = GetAttributeValue( "ConfirmationEmailTemplate" ).AsGuidOrNull();
            if ( confirmationEmailTemplateGuid.HasValue )
            {
                var recipients = new List<Rock.Communication.RecipientData>();

                // add person and the mergeObjects (same mergeobjects as receipt)
                recipients.Add( new Rock.Communication.RecipientData( person.Email, mergeObjects ) );

                Rock.Communication.Email.Send( confirmationEmailTemplateGuid.Value, recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
            }
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowForm()
        {
            lReceipt.Visible = false;
            pnlAddPledge.Visible = true;

            if ( CurrentPerson != null )
            {
                tbFirstName.Text = CurrentPerson.FirstName;
                tbLastName.Text = CurrentPerson.LastName;
                tbEmail.Text = CurrentPerson.Email;
            }

            // Warn if Financial Account is not specified (must be set up by administrator)
            var financialAccount = new FinancialAccountService( new RockContext() ).Get( GetAttributeValue( "Account" ).AsGuid() );
            if ( financialAccount == null )
            {
                nbWarningMessage.Text = "Warning: No Account is specified for this pledge.  Please contact the administrator.";
                nbWarningMessage.Visible = true;
            }
            else
            {
                nbWarningMessage.Visible = false;
            }

            drpDateRange.DelimitedValues = GetAttributeValue( "PledgeDateRange" );

            // only show the date range picker if the block setting for date range isn't fully specified
            drpDateRange.Visible = drpDateRange.LowerValue == null || drpDateRange.UpperValue == null;

            bddlFrequency.Items.Clear();
            var frequencies = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value );
            foreach ( var frequency in frequencies )
            {
                bddlFrequency.Items.Add( new ListItem( frequency.Value, frequency.Id.ToString() ) );
            }

            bddlFrequency.Visible = GetAttributeValue( "ShowPledgeFrequency" ).AsBooleanOrNull() ?? false;
            bddlFrequency.SelectedValue = null;

            // if Frequency is Visible, require it if RequirePledgeFrequency
            bddlFrequency.Required = bddlFrequency.Visible && ( GetAttributeValue( "RequirePledgeFrequency" ).AsBooleanOrNull() ?? false );

            string saveButtonText = GetAttributeValue( "SaveButtonText" );
            if ( !string.IsNullOrWhiteSpace( saveButtonText ) )
            {
                btnSave.Text = saveButtonText;
            }
            else
            {
                btnSave.Text = "Save";
            }

            lNote.Text = GetAttributeValue( "NoteMessage" );
        }

        /// <summary>
        /// Finds the person if they're logged in, or by email and name. If not exactly one found, creates a new person (and family)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person FindPerson( RockContext rockContext )
        {
            Person person;
            var personService = new PersonService( rockContext );

            if ( CurrentPerson != null )
            {
                person = CurrentPerson;
            }
            else
            {
                string firstName = tbFirstName.Text;
                if ( GetAttributeValue( "EnableSmartNames" ).AsBooleanOrNull() ?? true )
                {
                    // If they tried to specify first name as multiple first names, like "Steve and Judy" or "Bob & Sally", just take the first first name
                    var parts = firstName.Split( new string[] { " and ", " & " }, StringSplitOptions.RemoveEmptyEntries );
                    if ( parts.Length > 0 )
                    {
                        firstName = parts[0];
                    }
                }

                // Same logic as AddTransaction.ascx.cs
                var personMatches = personService.GetByMatch( firstName, tbLastName.Text, tbEmail.Text );
                if ( personMatches.Count() == 1 )
                {
                    person = personMatches.FirstOrDefault();
                }
                else
                {
                    person = null;
                }
            }

            if ( person == null )
            {
                var definedValue = DefinedValueCache.Read( GetAttributeValue( "NewConnectionStatus" ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
                person = new Person
                {
                    FirstName = tbFirstName.Text,
                    LastName = tbLastName.Text,
                    Email = tbEmail.Text,
                    EmailPreference = Rock.Model.EmailPreference.EmailAllowed,
                    ConnectionStatusValueId = definedValue.Id,
                };

                person.IsSystem = false;
                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                person.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                PersonService.SaveNewPerson( person, rockContext, null, false );
            }

            return person;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // reload page to start with a clean pledge entry
            Response.Redirect( Request.RawUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
