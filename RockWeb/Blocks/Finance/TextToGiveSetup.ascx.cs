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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Text To Give Setup" )]
    [Category( "Finance" )]
    [Description( "Allow an SMS sender to configure their SMS based giving." )]

    [FinancialGatewayField(
        key: AttributeKeys.CreditCardGateway,
        name: "Credit Card Gateway",
        description: "The payment gateway to use for Credit Card transactions",
        required: false,
        order: 1 )]

    [FinancialGatewayField(
        key: AttributeKeys.AchGateway,
        name: "ACH Gateway",
        description: "The payment gateway to use for ACH transactions",
        required: false,
        order: 2 )]

    [BooleanField(
        key: AttributeKeys.Impersonation,
        name: "Impersonation",
        trueText: "Allow (only use on an internal page used by staff)",
        falseText: "Don't Allow",
        description: "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users",
        defaultValue: false,
        order: 3 )]

    public partial class TextToGiveSetup : RockBlock
    {
        private static class AttributeKeys
        {
            public const string Impersonation = "Impersonation";
            public const string CreditCardGateway = "CreditCardGateway";
            public const string AchGateway = "AchGateway";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!IsPostBack)
            {
                using ( var rockContext = new RockContext() )
                {
                    PrefillForm( rockContext );
                    BindSavedAccounts( rockContext );
                }
            }
        }

        /// <summary>
        /// Fill out the form with the target person's details
        /// </summary>
        private void PrefillForm( RockContext rockContext )
        {
            var person = GetTargetPerson( rockContext );

            if ( person == null )
            {
                return;
            }

            tbFirstName.Text = person.FirstName;
            tbLastName.Text = person.LastName;
            tbEmail.Text = person.Email;
            acAddress.SetValues( person.GetHomeLocation() );

            var campus = person.GetCampus();
            caapDetailPicker.CampusId = campus == null ? (int?)null : campus.Id;
        }

        /// <summary>
        /// Get the person from the impersonation token or use the current person
        /// </summary>
        /// <param name="rockContext"></param>
        private Person GetTargetPerson( RockContext rockContext )
        {
            Person person = null;

            // If impersonation is allowed, and a valid person key was used, set the target to that person
            if ( GetAttributeValue( AttributeKeys.Impersonation ).AsBoolean() )
            {
                string personKey = PageParameter( "Person" );
                if ( !string.IsNullOrWhiteSpace( personKey ) )
                {
                    var incrementKeyUsage = !this.IsPostBack;
                    person = new PersonService( rockContext ).GetByImpersonationToken( personKey, incrementKeyUsage, this.PageCache.Id );

                    if ( person == null )
                    {
                        ShowError( "Invalid or Expired Person Token specified" );
                        return person;
                    }
                }
            }

            if ( person == null && CurrentPersonId.HasValue )
            {
                // Don't use CurrentPerson because we need an entity associated with the RockContext so it can be updated
                person = new PersonService( rockContext ).Get( CurrentPersonId.Value );
            }

            return person;
        }

        /// <summary>
        /// Populate the appropriate saved accounts for the person and gateway in the drop down list
        /// </summary>
        /// <param name="rockContext"></param>
        private void BindSavedAccounts( RockContext rockContext )
        {
            var selectedId = ddlSavedAccountPicker.SelectedValue.AsIntegerOrNull();
            ddlSavedAccountPicker.Items.Clear();

            // Get the stripe saved accounts for the person
            var savedAccounts = GetPersonSavedAccounts( rockContext );

            // Bind the accounts
            if ( savedAccounts.Any() )
            {
                ddlSavedAccountPicker.DataSource = savedAccounts;
                ddlSavedAccountPicker.Enabled = true;
            }
            else
            {
                ddlSavedAccountPicker.Enabled = false;
                ddlSavedAccountPicker.DataSource = new List<object>
                {
                    new {
                        Name = "No Saved Accounts",
                        Id = (int?) null
                    }
                };
            }

            ddlSavedAccountPicker.DataBind();

            // Try to select the previously selected account
            if ( selectedId.HasValue )
            {
                ddlSavedAccountPicker.SelectedValue = selectedId.Value.ToString();
            }

            // Pick the first one if not already selected something else            
            if ( savedAccounts.Any() && ddlSavedAccountPicker.SelectedValue == string.Empty )
            {
                ddlSavedAccountPicker.Items[0].Selected = true;
            }
        }

        /// <summary>
        /// Returns a list of the saved accounts for the selected person
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<FinancialPersonSavedAccount> GetPersonSavedAccounts( RockContext rockContext )
        {
            var person = GetTargetPerson( rockContext );

            if ( person == null )
            {
                return new List<FinancialPersonSavedAccount>();
            }

            return new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( person.Id )
                .AsNoTracking()
                .OrderBy( sa => sa.IsDefault )
                .ThenByDescending( sa => sa.CreatedDateTime )
                .ToList();
        }

        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = GetTargetPerson( rockContext );

                if ( person == null )
                {
                    ShowError( "Invalid or Expired Person Token specified" );
                    return;
                }

                person.FirstName = tbFirstName.Text;
                person.LastName = tbLastName.Text;
                person.Email = tbEmail.Text;
                person.ContributionFinancialAccountId = caapDetailPicker.SelectedAccountId;

                // This seems to be an error with the CAAPicker. It returns 0 instead of null
                if ( person.ContributionFinancialAccountId == 0 )
                {
                    person.ContributionFinancialAccountId = null;
                }

                var homeLocation = person.GetHomeLocation() ?? new Location();
                acAddress.GetValues( homeLocation );

                var selectedSavedAccountId = ddlSavedAccountPicker.SelectedValue.AsInteger();
                var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
                var personsSavedAccounts = savedAccountService.Queryable()
                    .Include( sa => sa.PersonAlias )
                    .Where( sa => sa.PersonAlias.PersonId == person.Id )
                    .ToList();

                // Loop through each saved account. Set default to false unless the args dictate that it is the default
                var foundDefaultAccount = false;

                foreach ( var savedAccount in personsSavedAccounts )
                {
                    if ( !foundDefaultAccount && savedAccount.Id == selectedSavedAccountId )
                    {
                        savedAccount.IsDefault = true;
                        foundDefaultAccount = true;
                    }
                    else
                    {
                        savedAccount.IsDefault = false;
                    }
                }

                if ( !foundDefaultAccount )
                {
                    ShowError( "The saved account selected is not valid" );
                    return;
                }

                if ( person.IsValid )
                {
                    rockContext.SaveChanges();
                }
                else if ( person.ValidationResults.Any() )
                {
                    ShowError( person.ValidationResults.First().ErrorMessage );
                }
                else
                {
                    ShowError( "The form is not valid" );
                }
            }
        }

        private void ShowError( string message )
        {
            nbAlert.Text = message;
            nbAlert.NotificationBoxType = NotificationBoxType.Danger;
            nbAlert.Visible = true;
        }
    }
}