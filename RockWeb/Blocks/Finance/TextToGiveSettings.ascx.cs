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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Text to Give Settings
    /// </summary>
    /// <seealso cref="RockBlock" />

    [RockObsolete( "1.14.1" )]
    [DisplayName( "Text To Give Settings (Obsolete)" )]
    [Category( "Finance" )]
    [Description( "Obsolete. Use the Giving Configuration block." )]

    [LinkedPage(
        "Parent Page",
        Description = "Page that will be navigated to when finished with this block.",
        Key = AttributeKey.ParentPage,
        IsRequired = true,
        Order = 0 )]

    [LinkedPage(
        "Add Saved Account Page",
        Description = "Page that will be navigated to for the purpose of creating a new saved account.",
        Key = AttributeKey.AddSavedAccountPage,
        IsRequired = false,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "9069F894-FDA5-4546-93EB-CEC448B142AA" )]
    public partial class TextToGiveSettings : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The parent page
            /// </summary>
            public const string ParentPage = "ParentPage";

            /// <summary>
            /// The add saved account page
            /// </summary>
            public const string AddSavedAccountPage = "AddSavedAccountPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The person identifier
            /// </summary>
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region LifeCycle Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindBlockTitle();
                RenderState();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var person = GetPerson();

            if ( person == null )
            {
                ShowError( "A person is required to configure settings" );
                return;
            }

            var selectedSavedAccountId = ddlSavedAccount.SelectedValueAsInt();
            var selectedFinancialAccountId = apAccountPicker.SelectedValueAsInt();

            var rockContext = GetRockContext();
            var personService = GetPersonService();

            var errorMessage = string.Empty;
            var success = personService.ConfigureTextToGive( person.Id, selectedFinancialAccountId, selectedSavedAccountId, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ShowError( errorMessage );
                return;
            }

            if ( !success )
            {
                ShowError( "The action was not successful, but no error was specified." );
                return;
            }

            rockContext.SaveChanges();
            hfIsEditMode.Value = string.Empty;

            ClearBlockData();
            RenderState();
        }

        /// <summary>
        /// Button to go to the parent page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancelOnView_Click( object sender, EventArgs e )
        {
            var person = GetPerson();

            if ( person != null )
            {
                NavigateToLinkedPage( AttributeKey.ParentPage, new Dictionary<string, string> { { PageParameterKey.PersonId, person.Id.ToString() } } );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.ParentPage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelOnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelOnEdit_Click( object sender, EventArgs e )
        {
            hfIsEditMode.Value = string.Empty;
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            hfIsEditMode.Value = "true";
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnAddSavedAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddSavedAccount_Click( object sender, EventArgs e )
        {
            var person = GetPerson();

            if ( person != null )
            {
                NavigateToLinkedPage( AttributeKey.AddSavedAccountPage, new Dictionary<string, string> { { PageParameterKey.PersonId, person.Id.ToString() } } );
            }
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Display an error in the browser window
        /// </summary>
        /// <param name="message"></param>
        private void ShowError( string message )
        {
            nbMessage.Text = message;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Called by a related block to show the detail for a specific entity.
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            if ( GetPerson() == null )
            {
                pnlEditDetails.Visible = false;
                pnlViewDetails.Visible = false;
                ShowError( "A person is required to configure Text To Give Settings" );
            }
            else if ( IsEditMode() )
            {
                ShowEditMode();
            }
            else
            {
                ShowViewMode();
            }
        }

        /// <summary>
        /// Shows the mode where the user can edit settings
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode() )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            btnAddSavedAccount.Visible = !GetAttributeValue( AttributeKey.AddSavedAccountPage ).IsNullOrWhiteSpace();
            BindSavedAccounts();

            var financialAccount = GetDefaultFinancialAccount();
            apAccountPicker.SetValue( financialAccount );

            var defaultSavedAccount = GetDefaultSavedAccount();
            ddlSavedAccount.SelectedValue = defaultSavedAccount == null ? string.Empty : defaultSavedAccount.Id.ToString();
        }

        /// <summary>
        /// Shows the mode where the user is only viewing settings
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            var financialAccount = GetDefaultFinancialAccount();
            var descriptionListLeft = new DescriptionList();
            descriptionListLeft.Add( "Default Account", financialAccount == null ? "None" : financialAccount.PublicName );

            var defaultSavedAccount = GetDefaultSavedAccount();
            var descriptionListRight = new DescriptionList();
            descriptionListRight.Add( "Saved Account", defaultSavedAccount == null ? "None" : GetSavedAccountName( defaultSavedAccount ) );

            lDescriptionLeft.Text = descriptionListLeft.Html;
            lDescriptionRight.Text = descriptionListRight.Html;
        }

        /// <summary>
        /// Populate the appropriate saved accounts for the person and gateway in the drop down list
        /// </summary>
        private void BindSavedAccounts()
        {
            var selectedId = ddlSavedAccount.SelectedValue.AsIntegerOrNull();
            ddlSavedAccount.Items.Clear();

            // Get the saved accounts for the person
            var savedAccounts = GetSavedAccounts();

            // Bind the accounts
            if ( savedAccounts != null && savedAccounts.Any() )
            {
                var viewModels = savedAccounts.Select( sa => new
                {
                    Id = ( int? ) sa.Id,
                    Name = GetSavedAccountName( sa )
                } ).ToList();

                // Add a blank option to unset the default account altogether
                viewModels.Insert( 0, new
                {
                    Id = ( int? ) null,
                    Name = string.Empty,
                } );

                ddlSavedAccount.DataSource = viewModels;
                ddlSavedAccount.Enabled = true;
            }
            else
            {
                ddlSavedAccount.Enabled = false;
                ddlSavedAccount.DataSource = new List<object>
                {
                    new {
                        Name = "No Saved Accounts",
                        Id = (int?) null
                    }
                };
            }

            ddlSavedAccount.DataBind();

            // Try to select the previously selected account
            if ( selectedId.HasValue && savedAccounts.Any( sa => sa.Id == selectedId ) )
            {
                ddlSavedAccount.SelectedValue = selectedId.Value.ToString();
            }
        }

        /// <summary>
        /// Get the name of the saved account
        /// </summary>
        /// <param name="savedAccount"></param>
        /// <returns></returns>
        private string GetSavedAccountName( FinancialPersonSavedAccount savedAccount )
        {
            const string unnamed = "<Unnamed>";

            if ( savedAccount == null )
            {
                return unnamed;
            }

            var name = savedAccount.Name.IsNullOrWhiteSpace() ? unnamed : savedAccount.Name.Trim();

            if ( savedAccount.FinancialPaymentDetail != null )
            {
                var expirationMonth = savedAccount.FinancialPaymentDetail.ExpirationMonth;
                var expirationYear = savedAccount.FinancialPaymentDetail.ExpirationYear;

                if ( expirationMonth.HasValue || expirationYear.HasValue )
                {
                    var monthString = expirationMonth.HasValue ?
                        ( expirationMonth.Value < 10 ? ( "0" + expirationMonth.Value.ToString() ) : expirationMonth.Value.ToString() ) :
                        "??";
                    var yearString = expirationYear.HasValue ?
                        ( expirationYear.Value % 100 ).ToString() :
                        "??";

                    name += string.Format( " ({0}/{1})", monthString, yearString );
                }
            }

            return name;
        }

        /// <summary>
        /// Binds the block title.
        /// </summary>
        private void BindBlockTitle()
        {
            var person = GetPerson();

            if ( person == null )
            {
                lTitle.Text = "Text To Give Settings";
            }
            else
            {
                lTitle.Text = "Text To Give Settings for " + person.FullName;
            }
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Is this block currently editing the settings
        /// </summary>
        /// <returns></returns>
        private bool IsEditMode()
        {
            return hfIsEditMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Is the block currently showing the settings
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return !IsEditMode();
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Get the person service
        /// </summary>
        /// <returns></returns>
        private PersonService GetPersonService()
        {
            if ( _personService == null )
            {
                var rockContext = GetRockContext();
                _personService = new PersonService( rockContext );
            }

            return _personService;
        }
        private PersonService _personService = null;

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            if ( _person == null )
            {
                var personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

                if ( personId.HasValue )
                {
                    var personService = GetPersonService();
                    _person = personService.Queryable( "ContributionFinancialAccount" ).AsNoTracking()
                        .FirstOrDefault( p => p.Id == personId.Value );
                }
            }

            return _person;
        }
        private Person _person;

        /// <summary>
        /// Gets the saved accounts.
        /// </summary>
        /// <returns></returns>
        private List<FinancialPersonSavedAccount> GetSavedAccounts()
        {
            if ( _savedAccounts == null )
            {
                var person = GetPerson();

                if ( person != null )
                {
                    var supportedGatewayIds = GetSupportedGatewayIds();

                    if ( supportedGatewayIds != null && supportedGatewayIds.Any() )
                    {
                        var rockContext = GetRockContext();
                        var service = new FinancialPersonSavedAccountService( rockContext );

                        _savedAccounts = service
                            .GetByPersonId( person.Id )
                            .Include( sa => sa.FinancialPaymentDetail )
                            .AsNoTracking()
                            .Where( sa =>
                                sa.FinancialGatewayId.HasValue &&
                                supportedGatewayIds.Contains( sa.FinancialGatewayId.Value ) )
                            .OrderBy( sa => sa.IsDefault )
                            .ThenByDescending( sa => sa.CreatedDateTime )
                            .ToList();
                    }
                }
            }

            return _savedAccounts;
        }
        private List<FinancialPersonSavedAccount> _savedAccounts = null;

        /// <summary>
        /// Gets the default saved account.
        /// </summary>
        /// <returns></returns>
        private FinancialPersonSavedAccount GetDefaultSavedAccount()
        {
            var savedAccounts = GetSavedAccounts();
            return savedAccounts == null ? null : savedAccounts.FirstOrDefault( sa => sa.IsDefault );
        }

        /// <summary>
        /// Gets the default financial account.
        /// </summary>
        /// <returns></returns>
        private FinancialAccount GetDefaultFinancialAccount()
        {
            var person = GetPerson();
            return person == null ? null : person.ContributionFinancialAccount;
        }

        /// <summary>
        /// Gets the supported gateway ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSupportedGatewayIds()
        {
            if ( _supportedGatewayIds == null )
            {
                var rockContext = GetRockContext();
                var gatewayService = new FinancialGatewayService( rockContext );
                var activeGatewayEntityTypes = gatewayService.Queryable( "EntityType" ).AsNoTracking()
                    .Where( fg => fg.IsActive )
                    .GroupBy( fg => fg.EntityType )
                    .ToList();

                var supportedTypes = Rock.Reflection.FindTypes( typeof( IAutomatedGatewayComponent ) );
                _supportedGatewayIds = new List<int>();

                foreach ( var entityType in activeGatewayEntityTypes )
                {
                    if ( supportedTypes.Any( t => t.Value.FullName == entityType.Key.Name ) )
                    {
                        _supportedGatewayIds.AddRange( entityType.Select( fg => fg.Id ) );
                    }
                }
            }

            return _supportedGatewayIds;
        }
        private List<int> _supportedGatewayIds = null;

        /// <summary>
        /// Clears the block data.
        /// </summary>
        private void ClearBlockData()
        {
            _rockContext = null;
            _personService = null;
            _person = null;
            _savedAccounts = null;
            _supportedGatewayIds = null;
        }

        #endregion Data Interface Methods
    }
}