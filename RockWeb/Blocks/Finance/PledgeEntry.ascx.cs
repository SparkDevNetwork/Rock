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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
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

    [CodeEditorField( "Receipt Text", "The text (or HTML) to display as the pledge receipt. <span class='tip tip-lava'></span> <span class='tip tip-html'>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, Order = 9, DefaultValue =
        @"
<h1>Thank You!</h1>
<p>
{{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.
</p>
<p>
    <a href='~/page/186?PledgeId={{ FinancialPledge.Id }}' class='btn btn-default' >Setup a Giving Profile</a>
</p>
" )]

    [SystemCommunicationField( "Confirmation Email Template", "Email template to use after submitting a new pledge. Leave blank to not send an email.", false, "", Order = 10 )]
    [GroupTypeField( "Select Group Type", "Optional Group Type that if selected will display a selection of groups that current user belongs to that can then be associated with the pledge", false, "", "", 12 )]
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

            nbInvalid.Visible = false;

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
            financialPledge.GroupId = ddlGroup.SelectedValueAsInt();

            var financialAccount = financialAccountService.Get( GetAttributeValue( "Account" ).AsGuid() );
            if ( financialAccount != null )
            {
                financialPledge.AccountId = financialAccount.Id;
            }

            financialPledge.TotalAmount = tbTotalAmount.Text.AsDecimal();

            var pledgeFrequencySelection = DefinedValueCache.Get( ddlFrequency.SelectedValue.AsInteger() );
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

            if ( financialPledge.IsValid )
            {
                financialPledgeService.Add( financialPledge );

                rockContext.SaveChanges();

                // populate account so that Liquid can access it
                financialPledge.Account = financialAccount;

                // populate PledgeFrequencyValue so that Liquid can access it
                financialPledge.PledgeFrequencyValue = definedValueService.Get( financialPledge.PledgeFrequencyValueId ?? 0 );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "FinancialPledge", financialPledge );
                mergeFields.Add( "PledgeFrequency", pledgeFrequencySelection );
                mergeFields.Add( "Account", financialAccount );
                lReceipt.Text = GetAttributeValue( "ReceiptText" ).ResolveMergeFields( mergeFields );

                // Resolve any dynamic url references
                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                lReceipt.Text = lReceipt.Text.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                lReceipt.Visible = true;
                pnlAddPledge.Visible = false;
                pnlConfirm.Visible = false;

                // if a ConfirmationEmailTemplate is configured, send an email
                var confirmationEmailTemplateGuid = GetAttributeValue( "ConfirmationEmailTemplate" ).AsGuidOrNull();
                if ( confirmationEmailTemplateGuid.HasValue )
                {
                    var emailMessage = new RockEmailMessage( confirmationEmailTemplateGuid.Value );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                    emailMessage.AppRoot = ResolveRockUrl( "~/" );
                    emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                    emailMessage.Send();
                }
            }
            else
            {
                ShowInvalidResults( financialPledge.ValidationResults );
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
                lName.Text = CurrentPerson.FullName;
                lName.Visible = true;

                tbFirstName.Visible = false;
                tbLastName.Visible = false;
                tbEmail.Visible = false;

                using ( var rockContext = new RockContext() )
                {
                    Guid? groupTypeGuid = GetAttributeValue( "SelectGroupType" ).AsGuidOrNull();
                    if ( groupTypeGuid.HasValue )
                    {
                        var groups = new GroupMemberService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m =>
                                m.Group.GroupType.Guid == groupTypeGuid.Value &&
                                m.PersonId == CurrentPerson.Id &&
                                m.GroupMemberStatus == GroupMemberStatus.Active &&
                                m.Group.IsActive && !m.Group.IsArchived )
                            .Select( m => new
                            {
                                m.GroupId,
                                Name = m.Group.Name,
                                GroupTypeName = m.Group.GroupType.Name
                            } )
                            .ToList()
                            .Distinct()
                            .OrderBy( g => g.Name )
                            .ToList();
                        if ( groups.Any() )
                        {
                            ddlGroup.Label = "For " + groups.First().GroupTypeName;
                            ddlGroup.DataSource = groups;
                            ddlGroup.DataBind();
                            ddlGroup.Visible = true;
                        }
                        else
                        {
                            ddlGroup.Visible = false;
                        }
                    }
                    else
                    {
                        ddlGroup.Visible = false;
                    }
                }
            }
            else
            {
                lName.Visible = false;
                ddlGroup.Visible = false;

                tbFirstName.Visible = true;
                tbLastName.Visible = true;
                tbEmail.Visible = true;

                tbFirstName.Text = string.Empty;
                tbLastName.Text = string.Empty;
                tbEmail.Text = string.Empty;
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

            ddlFrequency.Items.Clear();
            var frequencies = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value );
            foreach ( var frequency in frequencies )
            {
                ddlFrequency.Items.Add( new ListItem( frequency.Value, frequency.Id.ToString() ) );
            }

            ddlFrequency.Visible = GetAttributeValue( "ShowPledgeFrequency" ).AsBooleanOrNull() ?? false;
            ddlFrequency.SelectedValue = null;

            // if Frequency is Visible, require it if RequirePledgeFrequency
            ddlFrequency.Required = ddlFrequency.Visible && ( GetAttributeValue( "RequirePledgeFrequency" ).AsBooleanOrNull() ?? false );

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

                // Same logic as TransactionEntry.ascx.cs
                var personQuery = new PersonService.PersonMatchQuery( firstName, tbLastName.Text, tbEmail.Text, string.Empty);
                person = personService.FindPerson( personQuery, true );
            }

            if ( person == null )
            {
                var definedValue = DefinedValueCache.Get( GetAttributeValue( "NewConnectionStatus" ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
                person = new Person
                {
                    FirstName = tbFirstName.Text,
                    LastName = tbLastName.Text,
                    Email = tbEmail.Text,
                    EmailPreference = Rock.Model.EmailPreference.EmailAllowed,
                    ConnectionStatusValueId = definedValue.Id,
                };

                person.IsSystem = false;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                person.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                PersonService.SaveNewPerson( person, rockContext, null, false );
            }

            return person;
        }

        /// <summary>
        /// Shows the invalid results.
        /// </summary>
        /// <param name="validationResults">The validation results.</param>
        private void ShowInvalidResults( List<ValidationResult> validationResults )
        {
            nbInvalid.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul>", validationResults.AsDelimited( "</li><li>" ) );
            nbInvalid.Visible = true;
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
