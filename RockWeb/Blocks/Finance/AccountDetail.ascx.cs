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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Account Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given financial account." )]
    [Rock.SystemGuid.BlockTypeGuid( "DCD63280-B661-48AA-8DEB-F5ED63C7AB77" )]
    public partial class AccountDetail : RockBlock
    {
        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string AccountParticipantJSON = "AccountParticipantJSON";
        }

        #endregion ViewStateKeys

        private List<AccountParticipant> AccountParticipantState { get; set; }

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAccountParticipants.DataKeyNames = new string[] { "PersonAliasId" };
            gAccountParticipants.ShowActionRow = true;
            gAccountParticipants.Actions.ShowAdd = true;
            gAccountParticipants.Actions.AddClick += gAccountParticipants_AddClick;
            gAccountParticipants.GridRebind += gAccountParticipants_GridRebind;

            this.BlockUpdated += AccountDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( pnlAccountListUpdatePanel );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            var accountParticipantJSON = ViewState[ViewStateKey.AccountParticipantJSON] as string ?? string.Empty;
            AccountParticipantState = accountParticipantJSON.FromJsonOrNull<List<AccountParticipant>>() ?? new List<AccountParticipant>();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.</returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.AccountParticipantJSON] = AccountParticipantState?.ToJson();
            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the AccountDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AccountDetail_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            var accountId = PageParameter( "AccountId" ).AsInteger();
            if ( !Page.IsPostBack )
            {
                ShowDetail( accountId );
            }

            base.OnLoad( e );
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
            account.AccountTypeValueId = dvpAccountType.SelectedValueAsInt();
            account.PublicName = tbPublicName.Text;
            account.Url = tbUrl.Text;
            account.CampusId = cpCampus.SelectedValueAsInt();

            account.GlCode = tbGLCode.Text;
            account.StartDate = dtpStartDate.SelectedDate;
            account.EndDate = dtpEndDate.SelectedDate;
            account.IsTaxDeductible = cbIsTaxDeductible.Checked;

            account.ModifiedDateTime = RockDateTime.Now;
            account.ModifiedByPersonAliasId = CurrentPersonAliasId;

            int? oldImageId = null;
            if ( account.ImageBinaryFileId != imgBinaryFile.BinaryFileId )
            {
                oldImageId = account.ImageBinaryFileId;
                account.ImageBinaryFileId = imgBinaryFile.BinaryFileId;
            }

            account.LoadAttributes( rockContext );
            avcAttributes.GetEditValues( account );

            // if the account IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of account didn't pass.
            // So, make sure a message is displayed in the validation summary
            cvAccount.IsValid = account.IsValid;

            if ( !cvAccount.IsValid )
            {
                cvAccount.ErrorMessage = account.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

            if ( accountId == 0 )
            {
                // just added, but we'll need an Id to set account participants
                rockContext.SaveChanges();
                accountId = new FinancialAccountService( new RockContext() ).GetId( account.Guid ) ?? 0;
            }

            var accountParticipantsPersonAliasIdsByPurposeKey = AccountParticipantState.GroupBy( a => a.PurposeKey ).ToDictionary( k => k.Key, v => v.Select( x => x.PersonAliasId ).ToList() );
            foreach ( var purposeKey in accountParticipantsPersonAliasIdsByPurposeKey.Keys )
            {
                var accountParticipantsPersonAliasIds = accountParticipantsPersonAliasIdsByPurposeKey.GetValueOrNull( purposeKey );
                if ( accountParticipantsPersonAliasIds?.Any() == true )
                {
                    var accountParticipants = new PersonAliasService( rockContext ).GetByIds( accountParticipantsPersonAliasIds ).ToList();
                    accountService.SetAccountParticipants( accountId, accountParticipants, purposeKey );
                }
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                account.SaveAttributeValues( rockContext );

                if ( oldImageId.HasValue || account.ImageBinaryFileId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    if ( oldImageId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( oldImageId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old image as IsTemporary so it gets cleaned up later.
                            binaryFile.IsTemporary = true;
                        }
                    }

                    if ( account.ImageBinaryFileId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( account.ImageBinaryFileId.Value );
                        if ( binaryFile != null )
                        {
                            // mark the new image as not Temporary so it doesn't get cleaned up later.
                            binaryFile.IsTemporary = false;
                        }
                    }

                    rockContext.SaveChanges();
                }
            } );


            var qryParams = new Dictionary<string, string>();
            qryParams["AccountId"] = account.Id.ToString();
            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfAccountId.Value.Equals( "0" ) )
            {
                int? parentAccountId = PageParameter( "ParentAccountId" ).AsIntegerOrNull();
                if ( parentAccountId.HasValue )
                {
                    // Cancelling on Add, and we know the parentGroupID, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    if ( parentAccountId != 0 )
                    {
                        qryParams["AccountId"] = parentAccountId.ToString();
                    }

                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToPage( RockPage.Guid, null );
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                FinancialAccountService service = new FinancialAccountService( new RockContext() );
                FinancialAccount account = service.Get( hfAccountId.ValueAsInt() );
                ShowReadonlyDetails( account );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            FinancialAccountService service = new FinancialAccountService( new RockContext() );
            FinancialAccount account = service.Get( hfAccountId.ValueAsInt() );
            ShowEditDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? parentAccountId = null;
            RockContext rockContext = new RockContext();

            FinancialAccountService accountService = new FinancialAccountService( rockContext );
            AuthService authService = new AuthService( rockContext );
            FinancialAccount account = accountService.Get( hfAccountId.Value.AsInteger() );

            if ( account != null )
            {
                if ( !account.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this account.", ModalAlertType.Information );
                    return;
                }

                parentAccountId = account.ParentAccountId;
                string errorMessage;
                if ( !accountService.CanDelete( account, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                accountService.Delete( account );

                rockContext.SaveChanges();
            }

            // reload page, selecting the deleted account's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentAccountId != null )
            {
                qryParams["AccountId"] = parentAccountId.ToString();
            }

            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            NavigateToPage( RockPage.Guid, qryParams );
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
                int? parentAccountId = PageParameter( "ParentAccountId" ).AsIntegerOrNull();
                account = new FinancialAccount { Id = 0, ParentAccountId = parentAccountId, IsActive = true };
                if ( parentAccountId.HasValue )
                {
                    var parentAccount = new FinancialAccountService( new RockContext() ).Get( parentAccountId.Value );
                    if ( parentAccount != null )
                    {
                        account.ParentAccount = parentAccount;
                    }
                }

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfAccountId.Value = account.Id.ToString();

            nbEditModeMessage.Text = string.Empty;

            if ( !editAllowed )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( account );
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialAccount.FriendlyTypeName );
            }
            else
            {
                btnEdit.Visible = true;

                if ( account.Id > 0 )
                {
                    ShowReadonlyDetails( account );
                }
                else
                {
                    ShowEditDetails( account );
                }
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
            dvpAccountType.SetValue( account.AccountTypeValueId );
            tbPublicName.Text = account.PublicName;
            tbUrl.Text = account.Url;
            cpCampus.SelectedCampusId = account.CampusId;

            tbGLCode.Text = account.GlCode;
            cbIsTaxDeductible.Checked = account.IsTaxDeductible;
            dtpStartDate.SelectedDate = account.StartDate;
            dtpEndDate.SelectedDate = account.EndDate;

            this.AccountParticipantState = GetAccountParticipantStateFromDatabase();

            imgBinaryFile.BinaryFileId = account.ImageBinaryFileId;

            BindAccountParticipantsGrid();

            account.LoadAttributes();
            avcAttributes.AddEditControls( account, Rock.Security.Authorization.EDIT, CurrentPerson );
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

            DescriptionList leftDescription = new DescriptionList();
            leftDescription.Add( "Public Name", account.PublicName );
            leftDescription.Add( "Campus", account.Campus != null ? account.Campus.Name : string.Empty );
            leftDescription.Add( "GLCode", account.GlCode );
            leftDescription.Add( "Is Tax Deductible", account.IsTaxDeductible );

            if ( account.ImageBinaryFileId.HasValue )
            {
                string imageUrl = FileUrlHelper.GetImageUrl( account.ImageBinaryFileId.Value );
                string imgTag = GetImageTag( account.ImageBinaryFileId, 150, 150, false, true );
                leftDescription.Add( "Image", $"<a href='{ResolveRockUrl(imageUrl)}'>{imgTag}</a>" );
            }

            lLeftDetails.Text = leftDescription.Html;

            var followingsFromDatabase = GetAccountParticipantStateFromDatabase().OrderBy( a => a.PersonFullName ).ToList();
            var accountParticipantsHtml = followingsFromDatabase.Select( a => $"{a.PersonFullName} ({a.PurposeKeyDescription})" ).ToList().AsDelimited( "<br>\n" );

            DescriptionList rightDescription = new DescriptionList();
            rightDescription.Add( "Account Participants", accountParticipantsHtml );
            lRightDetails.Text = rightDescription.Html;

            account.LoadAttributes();
            Helper.AddDisplayControls( account, Helper.GetAttributeCategories( account, true, false ), phAttributesView, null, false );
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
            dvpAccountType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_ACCOUNT_TYPE.AsGuid() ).Id;

            cpCampus.Campuses = CampusCache.All();

            ddlAccountParticipantPurposeKey.Items.Clear();
            ddlAccountParticipantPurposeKey.Items.Add( new ListItem( RelatedEntityPurposeKey.GetPurposeKeyFriendlyName( RelatedEntityPurposeKey.FinancialAccountGivingAlert ), RelatedEntityPurposeKey.FinancialAccountGivingAlert ) );
        }

        #endregion

        #region Account Participants

        /// <summary>
        /// Gets the account participants state from database.
        /// </summary>
        /// <param name="purposeKeys">The purpose keys.</param>
        /// <returns>List&lt;AccountParticipantInfo&gt;.</returns>
        private List<AccountParticipant> GetAccountParticipantStateFromDatabase()
        {
            var accountId = hfAccountId.Value.AsInteger();

            var financialAccountService = new FinancialAccountService( new RockContext() );
            var accountParticipantsQuery = financialAccountService.GetAccountParticipantsAndPurpose( accountId );

            var participantsState = accountParticipantsQuery
                .ToList()
                .Select( a => new AccountParticipant
                {
                    PersonAliasId = a.PersonAlias.Id,
                    PersonFullName = a.PersonAlias.Person.FullName,
                    PurposeKey = a.PurposeKey
                } ).ToList();

            return participantsState;
        }

        /// <summary>
        /// Binds the account participants grid.
        /// </summary>
        private void BindAccountParticipantsGrid()
        {
            gAccountParticipants.DataSource = AccountParticipantState;
            gAccountParticipants.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccountParticipants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gAccountParticipants_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAccountParticipantsGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gAccountParticipants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAccountParticipants_AddClick( object sender, EventArgs e )
        {
            ppAccountParticipantPerson.SetValue( null );
            mdAddAccountParticipant.Show();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gAccountParticipants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountParticipants_DeleteClick( object sender, RowEventArgs e )
        {
            var participantPersonAliasId = e.RowKeyValue as int?;
            if ( participantPersonAliasId.HasValue )
            {
                AccountParticipant accountParticipantInfo = AccountParticipantState.FirstOrDefault( a => a.PersonAliasId == participantPersonAliasId.Value );
                if ( accountParticipantInfo != null )
                {
                    AccountParticipantState.Remove( accountParticipantInfo );
                    BindAccountParticipantsGrid();
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddAccountParticipant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddAccountParticipant_SaveClick( object sender, EventArgs e )
        {
            mdAddAccountParticipant.Hide();

            var personId = ppAccountParticipantPerson.PersonId;
            if ( !personId.HasValue )
            {
                // shouldn't happen, just ignore
                return;
            }

            var person = new PersonService( new RockContext() ).Get( personId.Value );
            var personAliasId = person?.PrimaryAliasId;
            if ( !personAliasId.HasValue )
            {
                // shouldn't happen, just ignore
                return;
            }

            var purposeKey = ddlAccountParticipantPurposeKey.SelectedValue ?? RelatedEntityPurposeKey.FinancialAccountGivingAlert;

            if ( !AccountParticipantState.Any( a => a.PersonAliasId == personAliasId.Value ) )
            {
                AccountParticipantState.Add( new AccountParticipant
                {
                    PersonAliasId = personAliasId.Value,
                    PersonFullName = person.FullName,
                    PurposeKey = purposeKey
                } );
            }

            BindAccountParticipantsGrid();
        }

        #endregion Account Participants

        private class AccountParticipant : RockDynamic
        {
            public int PersonAliasId { get; set; }

            public string PersonFullName { get; set; }

            public string PurposeKey { get; set; }

            public string PurposeKeyDescription => RelatedEntityPurposeKey.GetPurposeKeyFriendlyName( PurposeKey );
        }
    }
}