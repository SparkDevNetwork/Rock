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
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.GivingAnalytics;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block used to view and create new alert types for the giving analytics system.
    /// </summary>
    [DisplayName( "Giving Analytics Configuration" )]
    [Category( "Finance" )]
    [Description( " Block used to view and create new alert types for the giving analytics system." )]

    public partial class GivingAnalyticsConfiguration : Rock.Web.UI.RockBlock
    {
        #region Constants

        /// <summary>
        /// The account type: all tax deductable
        /// </summary>
        private const string AccountTypes_AllTaxDeductable = "AllTaxDeductable";

        /// <summary>
        /// The account type: custom
        /// </summary>
        private const string AccountTypes_Custom = "Custom";

        #endregion Constants

        #region Fields

        private GivingAnalyticsSetting _givingAnalyticsSetting = new GivingAnalyticsSetting();

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>ConnectionOpportunity
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAlerts.DataKeyNames = new string[] { "Id" };
            gAlerts.Actions.ShowAdd = true;
            gAlerts.Actions.AddClick += gAlerts_AddClick;
            gAlerts.GridRebind += gAlerts_GridRebind;
            gAlerts.GridReorder += gAlerts_GridReorder;

            mdDetails.SaveClick += mdDetails_SaveClick;
            mdDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

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

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
            else
            {
                BindAlerts();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblAccountTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblAccountTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            divAccounts.Visible = rblAccountTypes.SelectedValue == AccountTypes_Custom;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlConnectionType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedConnectionTypeId = ddlConnectionType.SelectedValueAsId();
            var connectionOpportunityService = new ConnectionOpportunityService( new RockContext() );
            ddlConnectionOpportunity.Items.Clear();
            ddlConnectionOpportunity.Visible = selectedConnectionTypeId.HasValue;
            if ( selectedConnectionTypeId.HasValue )
            {
                var connectionOpportunities = connectionOpportunityService.Queryable().Where( a => a.ConnectionTypeId == selectedConnectionTypeId.Value );
                ddlConnectionOpportunity.Items.Add( new ListItem() );
                ddlConnectionOpportunity.Items.AddRange( connectionOpportunities.Select( x => new ListItem { Text = x.Name, Value = x.Id.ToString() } ).ToArray() );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAlerts_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAlerts_Delete( object sender, RowEventArgs e )
        {
            DeleteFinancialTransactionAlertType( e.RowKeyId );
            BindAlerts();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAlerts_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAlerts();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAlerts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var alertType = e.Row.DataItem as FinancialTransactionAlertType;

            if ( alertType == null )
            {
                return;
            }
            // Status icons
            var lStatusIcons = e.Row.FindControl( "lStatusIcons" ) as Literal;

            if ( lStatusIcons != null )
            {
                var cssClass = alertType.AlertType == AlertType.Gratitude ? "success" : "warning";
                lStatusIcons.Text = string.Format(
                    "<span class='badge badge-{1} badge-circle' data-toggle='tooltip' data-original-title='{0}' style='inline-block'><span class='sr-only'></span></span>",
                    alertType.AlertType.ToString(),
                    cssClass );
            }

            // Actions Taken
            var lActionsTaken = e.Row.FindControl( "lActionsTaken" ) as Literal;

            if ( lActionsTaken != null )
            {
                string actionHtml = "<div class='status-list'>";
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-cog' style='opacity: {0};' aria-hidden='true'></i>", alertType.WorkflowTypeId.HasValue ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-comment' style='opacity: {0};' aria-hidden='true'></i>", alertType.SystemCommunicationId.HasValue ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-plug' style='opacity: {0};' aria-hidden='true'></i>", alertType.ConnectionOpportunityId.HasValue ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-bus' style='opacity: {0};' aria-hidden='true'></i>", alertType.SendBusEvent ? 1 : 0 );
                actionHtml += "</div>";
                lActionsTaken.Text = actionHtml;
            }
        }

        /// <summary>
        /// Handles the Add event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAlerts_AddClick( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAlerts_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var financialTransactionAlertTypes = GetFinancialTransactionAlertTypes( rockContext );
            if ( financialTransactionAlertTypes != null )
            {
                new FinancialTransactionAlertTypeService( rockContext ).Reorder( financialTransactionAlertTypes.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindAlerts();
        }


        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdDetails_SaveClick( object sender, EventArgs e )
        {
            int financialTransactionAlertTypeId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out financialTransactionAlertTypeId ) )
            {
                financialTransactionAlertTypeId = 0;
            }

            var rockContext = new RockContext();
            var financialTransactionAlertTypeService = new FinancialTransactionAlertTypeService( rockContext );
            FinancialTransactionAlertType financialTransactionAlertType = null;

            if ( financialTransactionAlertTypeId != 0 )
            {
                financialTransactionAlertType = financialTransactionAlertTypeService.Get( financialTransactionAlertTypeId );
            }

            if ( financialTransactionAlertType == null )
            {
                financialTransactionAlertType = new FinancialTransactionAlertType();
                financialTransactionAlertTypeService.Add( financialTransactionAlertType );
            }

            financialTransactionAlertType.Name = tbName.Text;
            financialTransactionAlertType.CampusId = cpCampus.SelectedCampusId;
            financialTransactionAlertType.AlertType = rblAlertType.SelectedValueAsEnum<AlertType>();
            financialTransactionAlertType.ContinueIfMatched = cbContinueIfMatched.Checked;
            financialTransactionAlertType.RepeatPreventionDuration = nbRepeatPreventionDuration.Text.AsIntegerOrNull();
            financialTransactionAlertType.AmountSensitivityScale = nbAmountSensitivityScale.Text.AsDecimalOrNull();
            financialTransactionAlertType.FrequencySensitivityScale = nbFrequencySensitivityScale.Text.AsDecimalOrNull();
            financialTransactionAlertType.MinimumGiftAmount = cbMinimumGiftAmount.Value;
            financialTransactionAlertType.MaximumGiftAmount = cbMaximumGiftAmount.Value;
            financialTransactionAlertType.MinimumMedianGiftAmount = cbMinimumMedianGiftAmount.Value;
            financialTransactionAlertType.MaximumMedianGiftAmount = cbMaximumMedianGiftAmount.Value;
            financialTransactionAlertType.MaximumDaysSinceLastGift = nbMaxDaysSinceLastGift.IntegerValue;
            financialTransactionAlertType.DataViewId = dvpPersonDataView.SelectedValueAsInt();
            financialTransactionAlertType.SendBusEvent = cbSendBusEvent.Checked;
            financialTransactionAlertType.ConnectionOpportunityId = ddlConnectionOpportunity.SelectedValueAsId();
            financialTransactionAlertType.SystemCommunicationId = ddlSystemCommunication.SelectedValueAsId();
            financialTransactionAlertType.WorkflowTypeId = wtpLaunchWorkflow.SelectedValueAsId();
            rockContext.SaveChanges();

            hfIdValue.Value = string.Empty;
            mdDetails.Hide();

            BindAlerts();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var selectedAccountIds = apAccounts.SelectedIds;
            var accountGuids = new List<Guid>();

            if ( selectedAccountIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var accountService = new FinancialAccountService( rockContext );
                    accountGuids = accountService.Queryable()
                        .AsNoTracking()
                        .Where( a => selectedAccountIds.Contains( a.Id ) )
                        .Select( a => a.Guid )
                        .ToList();
                }
            }

            var isCustomAccounts = rblAccountTypes.SelectedValue == AccountTypes_Custom;

            _givingAnalyticsSetting.FinancialAccountGuids = isCustomAccounts ? accountGuids : null;
            _givingAnalyticsSetting.AreChildAccountsIncluded = isCustomAccounts ? cbIncludeChildAccounts.Checked : ( bool? ) null;
            _givingAnalyticsSetting.TransactionTypeGuids = cblTransactionTypes.SelectedValues.AsGuidList();
            _givingAnalyticsSetting.GivingAnalytics.IsEnabled = cbEnableGivingAnalytics.Checked;
            _givingAnalyticsSetting.GivingAnalytics.GiverAnalyticsRunDays = dwpDaysToUpdateAnalytics.SelectedDaysOfWeek;
            _givingAnalyticsSetting.Alerting.GlobalRepeatPreventionDurationDays = nbGlobalRepeatPreventionDuration.Text.AsIntegerOrNull();
            _givingAnalyticsSetting.Alerting.GratitudeRepeatPreventionDurationDays = nbGratitudeRepeatPreventionDuration.Text.AsIntegerOrNull();
            _givingAnalyticsSetting.Alerting.FollowupRepeatPreventionDurationDays = nbFollowupRepeatPreventionDuration.Text.AsIntegerOrNull();

            Rock.Web.SystemSettings.SetValue( SystemSetting.GIVING_ANALYTICS_CONFIGURATION, _givingAnalyticsSetting.ToJson() );

            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }


        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var transactionTypes = BindTransactionTypes();
            BindAccounts();

            // Load values from the system settings
            _givingAnalyticsSetting = Rock.Web.SystemSettings.GetValue( SystemSetting.GIVING_ANALYTICS_CONFIGURATION ).FromJsonOrNull<GivingAnalyticsSetting>() ?? new GivingAnalyticsSetting();

            var savedTransactionTypeGuids =
                _givingAnalyticsSetting.TransactionTypeGuids ??
                new List<Guid> { Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() };
            var savedTransactionTypeGuidStrings = savedTransactionTypeGuids.Select( g => g.ToString() )
                .Intersect( transactionTypes.Select( dv => dv.Guid.ToString() ) );

            var savedAccountGuids = _givingAnalyticsSetting.FinancialAccountGuids ?? new List<Guid>();
            var accounts = new List<FinancialAccount>();
            var areChildAccountsIncluded = _givingAnalyticsSetting.AreChildAccountsIncluded ?? false;

            if (savedAccountGuids.Any())
            {
                using ( var rockContext = new RockContext() )
                {
                    var accountService = new FinancialAccountService( rockContext );
                    accounts = accountService.Queryable()
                        .AsNoTracking()
                        .Where( a => savedAccountGuids.Contains( a.Guid ) )
                        .ToList();
                }
            }

            // Sync the system setting values to the controls
            divAccounts.Visible = savedAccountGuids.Any();
            apAccounts.SetValues( accounts );
            rblAccountTypes.SetValue( savedAccountGuids.Any() ? AccountTypes_Custom : AccountTypes_AllTaxDeductable );
            cbIncludeChildAccounts.Checked = areChildAccountsIncluded;
            cblTransactionTypes.SetValues( savedTransactionTypeGuidStrings );
            cbEnableGivingAnalytics.Checked = _givingAnalyticsSetting.GivingAnalytics.IsEnabled;
            dwpDaysToUpdateAnalytics.SelectedDaysOfWeek = _givingAnalyticsSetting.GivingAnalytics.GiverAnalyticsRunDays;
            nbGlobalRepeatPreventionDuration.Text = _givingAnalyticsSetting.Alerting.GlobalRepeatPreventionDurationDays.ToStringSafe();
            nbGratitudeRepeatPreventionDuration.Text = _givingAnalyticsSetting.Alerting.GratitudeRepeatPreventionDurationDays.ToStringSafe();
            nbFollowupRepeatPreventionDuration.Text = _givingAnalyticsSetting.Alerting.FollowupRepeatPreventionDurationDays.ToStringSafe();

            BindAlerts();
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            if ( rblAccountTypes.Items.Count > 0 )
            {
                return;
            }

            rblAccountTypes.DataTextField = "Text";
            rblAccountTypes.DataValueField = "Value";
            rblAccountTypes.DataSource = new[] {
                new {
                    Text = "All Tax Deductible Accounts",
                    Value = AccountTypes_AllTaxDeductable
                },
                new {
                    Text ="Custom",
                    Value = AccountTypes_Custom
                }
            };
            rblAccountTypes.DataBind();
        }

        /// <summary>
        /// Binds the transaction types.
        /// </summary>
        private List<DefinedValueCache> BindTransactionTypes()
        {
            var transactionTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )
                .DefinedValues
                .Where( dv => dv.IsActive )
                .ToList();

            if ( cblTransactionTypes.Items.Count > 0 )
            {
                return transactionTypes;
            }

            cblTransactionTypes.DataTextField = "Text";
            cblTransactionTypes.DataValueField = "Value";
            cblTransactionTypes.DataSource = transactionTypes.Select( dv => new
            {
                Text = dv.Value,
                Value = dv.Guid.ToString()
            } );

            cblTransactionTypes.DataBind();
            return transactionTypes;
        }

        /// <summary>
        /// Binds the alerts.
        /// </summary>
        private void BindAlerts()
        {
            if ( CampusCache.All().Count == 1 )
            {
                // Hide the campus field if there is only one campus in the system
                var campusField = gAlerts.ColumnsOfType<RockBoundField>().Where( a => a.HeaderText == "Campus" ).FirstOrDefault();

                if ( campusField != null )
                {
                    campusField.Visible = false;
                }
            }

            gAlerts.DataSource = GetFinancialTransactionAlertTypes().ToList();
            gAlerts.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="financialTransactionAlertTypeId">The financial transaction alert type id.</param>
        private void ShowEdit( int financialTransactionAlertTypeId )
        {
            FinancialTransactionAlertType financialTransactionAlertType = null;
            if ( financialTransactionAlertTypeId > 0 )
            {
                financialTransactionAlertType = new FinancialTransactionAlertTypeService( new RockContext() ).Get( financialTransactionAlertTypeId );
            }

            if ( financialTransactionAlertType == null )
            {
                financialTransactionAlertType = new FinancialTransactionAlertType();
            }

            BindControl();
            tbName.Text = financialTransactionAlertType.Name;
            hfIdValue.Value = financialTransactionAlertType.Id.ToString();
            cpCampus.SetValue( financialTransactionAlertType.CampusId );
            rblAlertType.SetValue( ( int ) financialTransactionAlertType.AlertType );
            cbContinueIfMatched.Checked = financialTransactionAlertType.ContinueIfMatched;
            nbRepeatPreventionDuration.Text = financialTransactionAlertType.RepeatPreventionDuration.ToStringSafe();
            nbAmountSensitivityScale.Text = financialTransactionAlertType.AmountSensitivityScale.ToStringSafe();
            nbFrequencySensitivityScale.Text = financialTransactionAlertType.FrequencySensitivityScale.ToStringSafe();
            cbMinimumGiftAmount.Value = financialTransactionAlertType.MinimumGiftAmount;
            cbMaximumGiftAmount.Value = financialTransactionAlertType.MaximumGiftAmount;
            cbMinimumMedianGiftAmount.Value = financialTransactionAlertType.MinimumMedianGiftAmount;
            cbMaximumMedianGiftAmount.Value = financialTransactionAlertType.MaximumMedianGiftAmount;
            nbMaxDaysSinceLastGift.IntegerValue = financialTransactionAlertType.MaximumDaysSinceLastGift;
            dvpPersonDataView.SetValue( financialTransactionAlertType.DataViewId );
            cbSendBusEvent.Checked = financialTransactionAlertType.SendBusEvent;
            wtpLaunchWorkflow.SetValue( financialTransactionAlertType.WorkflowTypeId );

            if ( financialTransactionAlertType.ConnectionOpportunity != null )
            {
                ddlConnectionType.SetValue( financialTransactionAlertType.ConnectionOpportunity.ConnectionTypeId );
                ddlConnectionType_SelectedIndexChanged( null, null );
                ddlConnectionOpportunity.SetValue( financialTransactionAlertType.ConnectionOpportunityId );
            }
            else
            {
                int? connectionTypeId = null;
                ddlConnectionType.SetValue( connectionTypeId );
                ddlConnectionType_SelectedIndexChanged( null, null );
            }

            ddlSystemCommunication.SetValue( financialTransactionAlertType.SystemCommunicationId );
            mdDetails.Show();
        }

        /// <summary>
        /// Binds the data source to the selection control.
        /// </summary>
        private void BindControl()
        {
            rblAlertType.BindToEnum<AlertType>();

            var connectionTypeService = new ConnectionTypeService( new RockContext() );
            ddlConnectionType.Items.Add( new ListItem() );
            ddlConnectionType.Items.AddRange( connectionTypeService.Queryable().Select( x => new ListItem { Text = x.Name, Value = x.Id.ToString() } ).ToArray() );

            dvpPersonDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            var SystemCommunications = new SystemCommunicationService( new RockContext() ).Queryable().OrderBy( e => e.Title );
            ddlSystemCommunication.Items.Clear();
            ddlSystemCommunication.Items.Add( new ListItem() );
            if ( SystemCommunications.Any() )
            {
                ddlSystemCommunication.Items.AddRange( SystemCommunications.Select( x => new ListItem { Text = x.Title, Value = x.Id.ToString() } ).ToArray() );
            }
        }

        /// <summary>
        /// Deletes the financial transaction alert type.
        /// </summary>
        /// <param name="financialTransactionAlertTypeId">The financial transaction alert type id.</param>
        private void DeleteFinancialTransactionAlertType( int financialTransactionAlertTypeId )
        {
            using ( var rockContext = new RockContext() )
            {
                var financialTransactionAlertTypeService = new FinancialTransactionAlertTypeService( rockContext );
                var financialTransactionAlertType = financialTransactionAlertTypeService.Get( financialTransactionAlertTypeId );
                if ( financialTransactionAlertType != null )
                {
                    string errorMessage;
                    if ( !financialTransactionAlertTypeService.CanDelete( financialTransactionAlertType, out errorMessage ) )
                    {
                        ShowError( errorMessage );
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        financialTransactionAlertTypeService.Delete( financialTransactionAlertType );
                        rockContext.SaveChanges();
                    } );
                }
            }
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="text">The text.</param>
        private void ShowError( string text )
        {
            nbEditModeMessage.Title = "Oops";
            nbEditModeMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbEditModeMessage.Text = text;
            nbEditModeMessage.Visible = true;
        }

        /// <summary>
        /// Get the Financial Transaction Alert Types.
        /// </summary>
        private IQueryable<FinancialTransactionAlertType> GetFinancialTransactionAlertTypes( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var financialTransactionAlertTypeService = new FinancialTransactionAlertTypeService( rockContext );
            return financialTransactionAlertTypeService.Queryable().OrderBy( s => s.Order ).ThenBy( s => s.Name );
        }

        #endregion
    }
}