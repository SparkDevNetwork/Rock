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
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Account Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given financial account." )]
    public partial class AccountDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var accountId = PageParameter("accountId").AsInteger();
            if ( !Page.IsPostBack )
            {
                ShowDetail( accountId );
            }

            // Add any attribute controls. 
            // This must be done here regardless of whether it is a postback so that the attribute values will get saved.
            var account = new FinancialAccountService(new RockContext()).Get(accountId);
            if (account == null)
            {
                account = new FinancialAccount();
            }
            account.LoadAttributes();
            phAttributes.Controls.Clear();
            Helper.AddEditControls(account, phAttributes, true, BlockValidationGroup);
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
            FinancialAccount account;
            var rockContext = new RockContext();

            var accountService = new Rock.Model.FinancialAccountService( rockContext );

            int accountId = hfAccountId.Value.AsInteger();

            if ( accountId == 0 )
            {
                account = new Rock.Model.FinancialAccount();
                accountService.Add( account );
                account.CreatedByPersonAliasId = CurrentPersonAliasId;
                account.CreatedDateTime = RockDateTime.Now;
            }
            else
            {
                account = accountService.Get( accountId );
            }

            account.Name = tbName.Text;
            account.IsActive = cbIsActive.Checked;
            account.IsPublic = cbIsPublic.Checked;
            account.Description = tbDescription.Text;
            account.PublicDescription = cePublicDescription.Text;

            account.ParentAccountId = apParentAccount.SelectedValueAsInt();
            account.AccountTypeValueId = ddlAccountType.SelectedValueAsInt();
            account.PublicName = tbPublicName.Text;
            account.Url = tbUrl.Text;
            account.CampusId = cpCampus.SelectedValueAsInt();

            account.GlCode = tbGLCode.Text;
            account.StartDate = dtpStartDate.SelectedDate;
            account.EndDate = dtpEndDate.SelectedDate;
            account.IsTaxDeductible = cbIsTaxDeductible.Checked;

            account.ModifiedDateTime = RockDateTime.Now;
            account.ModifiedByPersonAliasId = CurrentPersonAliasId;

            account.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, account );

            // if the account IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of account didn't pass.
            // So, make sure a message is displayed in the validation summary
            cvAccount.IsValid = account.IsValid;

            if ( !cvAccount.IsValid )
            {
                cvAccount.ErrorMessage = account.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

            rockContext.SaveChanges();
            account.SaveAttributeValues( rockContext );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public void ShowDetail( int accountId )
        {
            FinancialAccount account = null;

            bool editAllowed = UserCanEdit;

            if ( !accountId.Equals( 0 ) )
            {
                account = new FinancialAccountService( new RockContext() ).Get( accountId );
                editAllowed = editAllowed || account.IsAuthorized( Authorization.EDIT, CurrentPerson );
                pdAuditDetails.SetEntity( account, ResolveRockUrl( "~" ) );
            }

            if ( account == null )
            {
                account = new FinancialAccount { Id = 0, IsActive = true };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfAccountId.Value = account.Id.ToString();

            nbEditModeMessage.Text = string.Empty;
            if (editAllowed)
            {
                ShowEditDetails(account);
            }
            else
            {
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed(FinancialAccount.FriendlyTypeName);
                ShowReadonlyDetails(account);
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="account">The account.</param>
        private void ShowEditDetails( FinancialAccount account )
        {
            if ( account.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( FinancialAccount.FriendlyTypeName ).FormatAsHtmlTitle();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                lActionTitle.Text = account.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !account.IsActive;

            SetEditMode( true );

            LoadDropDowns();

            tbName.Text = account.Name;
            cbIsActive.Checked = account.IsActive;
            cbIsPublic.Checked = account.IsPublic.HasValue ? account.IsPublic.Value : false;
            tbDescription.Text = account.Description;
            cePublicDescription.Text = account.PublicDescription;

            apParentAccount.SetValue( account.ParentAccount );
            ddlAccountType.SetValue( account.AccountTypeValueId );
            tbPublicName.Text = account.PublicName;
            tbUrl.Text = account.Url;
            cpCampus.SelectedCampusId = account.CampusId;

            tbGLCode.Text = account.GlCode;
            cbIsTaxDeductible.Checked = account.IsTaxDeductible;
            dtpStartDate.SelectedDate = account.StartDate;
            dtpEndDate.SelectedDate = account.EndDate;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="account">The account.</param>
        private void ShowReadonlyDetails( FinancialAccount account )
        {
            SetEditMode( false );

            hfAccountId.SetValue( account.Id );
            lActionTitle.Text = account.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !account.IsActive;
            lAccountDescription.Text = account.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( string.Empty, string.Empty );
            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlAccountType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_ACCOUNT_TYPE.AsGuid() ) );

            cpCampus.Campuses = CampusCache.All();
            cpCampus.Visible = cpCampus.Items.Count > 0;
        }

        #endregion
    }
}