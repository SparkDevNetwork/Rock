// <copyright>
// Copyright by Triumph Tech
//
// NOTICE: All information contained herein is, and remains
// the property of Triumph Tech LLC. The intellectual and technical concepts contained
// herein are proprietary to Triumph Tech LLC  and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
//
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from Triumph Tech LLC.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Transactions;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

using tech.triumph.WistiaIntegration.Model;
using tech.triumph.WistiaIntegration.Transactions;

namespace RockWeb.Plugins.tech_triumph.WistiaIntegration
{
    /// <summary>
    /// Displays the details of the given Wistia Account.
    /// </summary>
    [DisplayName( "Account Detail" )]
    [Category( "Triumph Tech > Wistia Integration" )]
    [Description( "Displays the details of the given Wistia Account." )]
    public partial class AccountDetail : RockBlock
    {
        #region Base Control Methods

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

            var accountId = PageParameter( "accountId" ).AsInteger();

            if ( !Page.IsPostBack )
            {
                ShowDetail( accountId );
            }

        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? accountId = PageParameter( pageReference, "AccountId" ).AsIntegerOrNull();
            if ( accountId != null )
            {
                string accountName = new WistiaAccountService( new RockContext() )
                    .Queryable().AsNoTracking()
                    .Where( a => a.Id == accountId.Value )
                    .Select( a => a.Name )
                    .FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( accountName ) )
                {
                    breadCrumbs.Add( new BreadCrumb( accountName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Account", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            WistiaAccount account = null;
            WistiaAccountService accountService = new WistiaAccountService( rockContext );

            int accountId = hfAccountId.ValueAsInt();
            if ( accountId > 0 )
            {
                account = accountService.Get( accountId );
            }

            if ( account == null )
            {
                account = new WistiaAccount();
                accountService.Add( account );
            }

            account.Name = tbName.Text;
            account.ApiKey = tbApiKey.Text;
            account.IsActive = cbIsActive.Checked;

            if ( !account.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            ShowReadonlyDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            WistiaAccountService accountService = new WistiaAccountService( new RockContext() );
            WistiaAccount account = accountService.Get( hfAccountId.ValueAsInt() );
            ShowEditDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfAccountId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                WistiaAccountService accountService = new WistiaAccountService( new RockContext() );
                WistiaAccount account = accountService.Get( hfAccountId.ValueAsInt() );
                ShowReadonlyDetails( account );
            }
        }

        protected void btnSync_Click( object sender, EventArgs e )
        {
            var syncTransaction = new WistiaSyncAccountTransaction
            {
                WistiaAccountId = hfAccountId.ValueAsInt()
            };

            RockQueue.GetStandardQueuedTransactions().Add( syncTransaction );

            mdMessages.Show( "A sync transactions has been launched for this account. This process could take a few minutes to complete.", Rock.Web.UI.Controls.ModalAlertType.Information );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="account">Type of the defined.</param>
        private void ShowReadonlyDetails( WistiaAccount account )
        {
            SetEditMode( false );

            hfAccountId.SetValue( account.Id );
            lTitle.Text = account.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !account.IsActive;

            DescriptionList detailDescription = new DescriptionList();
            detailDescription.Add( "Name", account.Name );
            //detailDescription.Add( "Api Key", account.ApiKey );
            if ( !string.IsNullOrEmpty( account.WistiaName ) )
            {
                detailDescription.Add( "Wistia Name", account.WistiaName );
            }
            if ( !string.IsNullOrEmpty( account.WistiaUrl ) )
            {
                detailDescription.Add( "Wistia URL", account.WistiaUrl );
            }

            if ( account.AccountDataInfo.MediaCount.HasValue )
            {
                lMediaCount.Text = account.AccountDataInfo.MediaCount.Value.ToString();
            }
            else
            {
                lMediaCount.Text = "0";
            }

            if ( account.HoursWatched.HasValue )
            {
                lHoursWatched.Text = account.HoursWatched.Value.ToString( "#,##0.0" );
            }
            else
            {
                lHoursWatched.Text = "0";
            }

            if ( account.PlayCount.HasValue )
            {
                lPlayCount.Text = account.PlayCount.Value.ToString( "N0" );
            }
            else
            {
                lPlayCount.Text = "0";
            }

            if ( account.LoadCount.HasValue )
            {
                lLoadCount.Text = account.LoadCount.Value.ToString( "N0" );
            }
            else
            {
                lLoadCount.Text = "0";
            }

            lblMainDetails.Text = detailDescription.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="account">Type of the defined.</param>
        private void ShowEditDetails( WistiaAccount account )
        {
            if ( account.Id == 0 )
            {
                lTitle.Text = ActionTitle.Add( FinancialAccount.FriendlyTypeName ).FormatAsHtmlTitle();
                account.IsActive = true;

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                lTitle.Text = account.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !account.IsActive;

            SetEditMode( true );

            tbName.Text = account.Name;
            cbIsActive.Checked = account.IsActive;
            tbApiKey.Text = account.ApiKey;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="accountId">The defined type identifier.</param>
        public void ShowDetail( int accountId )
        {
            pnlDetails.Visible = true;
            WistiaAccount account = null;

            if ( !accountId.Equals( 0 ) )
            {
                account = new WistiaAccountService( new RockContext() ).Get( accountId );
            }

            if ( account == null )
            {
                account = new WistiaAccount { Id = 0 };
            }

            hfAccountId.SetValue( account.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !UserCanEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WistiaAccount.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( account );
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

        #endregion
    }
}