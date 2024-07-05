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
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Event Registration Matching" )]
    [Category( "Finance" )]
    [Description( "Used to assign a Registration to a Transaction Detail record" )]

    [Rock.SystemGuid.BlockTypeGuid( "7651F50F-3E32-4437-B71A-FED1855098AD" )]
    public partial class EventRegistrationMatching : RockBlock
    {
        private List<FinancialTransactionDetail> _financialTransactionDetailList;

        #region Keys

        /// <summary>
        /// User Preference Keys
        /// </summary>
        public class UserPreferenceKey
        {
            /// <summary>
            /// The batch identifier
            /// </summary>
            public const string BatchId = "BatchId";

            /// <summary>
            /// The registration template identifier
            /// </summary>
            public const string RegistrationTemplateId = "RegistrationTemplateId";

            /// <summary>
            /// The registration instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        /// <summary>
        /// View State Keys
        /// </summary>
        public class ViewStateKey
        {
            /// <summary>
            /// The batch identifier
            /// </summary>
            public const string BatchId = "BatchId";

            /// <summary>
            /// The registration template identifier
            /// </summary>
            public const string RegistrationTemplateId = "RegistrationTemplateId";

            /// <summary>
            /// The registration instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Keys

        #region Properties

        private int? BatchId { get; set; }
        private int? RegistrationTemplateId { get; set; }
        private int? RegistrationInstanceId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            BatchId = ViewState[ViewStateKey.BatchId] as int?;
            RegistrationTemplateId = ViewState[ViewStateKey.RegistrationTemplateId] as int?;
            RegistrationInstanceId = ViewState[ViewStateKey.RegistrationInstanceId] as int?;

            BindHtmlGrid();
            LoadRegistrationDropDowns();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

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
                cbHideFullyPaidRegistrations.Checked = true;

                LoadDropDowns();

                var preferences = GetBlockPersonPreferences();
                BatchId = preferences.GetValue( UserPreferenceKey.BatchId ).AsIntegerOrNull();
                RegistrationTemplateId = preferences.GetValue( UserPreferenceKey.RegistrationTemplateId ).AsIntegerOrNull();
                RegistrationInstanceId = preferences.GetValue( UserPreferenceKey.RegistrationInstanceId ).AsIntegerOrNull();
                ddlBatch.SetValue( BatchId );
                rtpRegistrationTemplate.SetValue( RegistrationTemplateId );

                LoadRegistrationInstances();
                ddlRegistrationInstance.SetValue( RegistrationInstanceId );

                BindHtmlGrid();
                LoadRegistrationDropDowns();
            }
        }

        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.BatchId] = BatchId;
            ViewState[ViewStateKey.RegistrationTemplateId] = RegistrationTemplateId;
            ViewState[ViewStateKey.RegistrationInstanceId] = RegistrationInstanceId;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BatchId = ddlBatch.SelectedValue.AsIntegerOrNull();
            RegistrationTemplateId = rtpRegistrationTemplate.SelectedValue.AsIntegerOrNull();
            RegistrationInstanceId = ddlRegistrationInstance.SelectedValue.AsIntegerOrNull();
            BindHtmlGrid();
            LoadRegistrationDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBatch_SelectedIndexChanged( object sender, EventArgs e )
        {
            BatchId = ddlBatch.SelectedValue.AsIntegerOrNull();

            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( UserPreferenceKey.BatchId, BatchId.ToStringSafe() );
            preferences.Save();

            BindHtmlGrid();
            LoadRegistrationDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistrationInstance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistrationInstance_SelectedIndexChanged( object sender, EventArgs e )
        {
            RegistrationInstanceId = ddlRegistrationInstance.SelectedValue.AsIntegerOrNull();

            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( UserPreferenceKey.RegistrationInstanceId, RegistrationInstanceId.ToStringSafe() );
            preferences.Save();

            BindHtmlGrid();
            LoadRegistrationDropDowns();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbHideFullyPaidRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbHideFullyPaidRegistrations_CheckedChanged( object sender, EventArgs e )
        {
            BindHtmlGrid();
            LoadRegistrationDropDowns();
        }

        /// <summary>
        /// Handles the SelectItem event of the rtpRegistrationTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rtpRegistrationTemplate_SelectItem( object sender, EventArgs e )
        {
            RegistrationTemplateId = rtpRegistrationTemplate.SelectedValue.AsIntegerOrNull();

            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( UserPreferenceKey.RegistrationTemplateId, RegistrationTemplateId.ToStringSafe() );
            preferences.Save();

            RegistrationInstanceId = null;
            var registrationTemplateId = rtpRegistrationTemplate.SelectedValue.AsIntegerOrNull();
            LoadRegistrationInstances();
            BindHtmlGrid();
            LoadRegistrationDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistration_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlRegistration = sender as RockDropDownList;
            if ( ddlRegistration != null && ddlRegistration.SelectedValue.AsIntegerOrNull().HasValue )
            {
                int? financialTransactionDetailId = ddlRegistration.ID.Replace( "ddlRegistration_", string.Empty ).AsInteger();
                var rockContext = new RockContext();
                var financialTransactionDetail = new FinancialTransactionDetailService( rockContext ).Get( financialTransactionDetailId.Value );
                var registrationEntityTypeId = EntityTypeCache.GetId<Registration>();
                financialTransactionDetail.EntityTypeId = registrationEntityTypeId;
                financialTransactionDetail.EntityId = ddlRegistration.SelectedValue.AsInteger();

                var transactionType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION );
                if ( financialTransactionDetail.Transaction.TransactionTypeValueId != transactionType.Id )
                {
                    financialTransactionDetail.Transaction.TransactionTypeValueId = transactionType.Id;
                }

                rockContext.SaveChanges();
                BindHtmlGrid();
                LoadRegistrationDropDowns();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            var lbDelete = sender as LinkButton;
            if ( lbDelete != null )
            {
                int? financialTransactionDetailId = lbDelete.ID.Replace( "lbDelete_", string.Empty ).AsInteger();
                var rockContext = new RockContext();
                var financialTransactionDetail = new FinancialTransactionDetailService( rockContext ).Get( financialTransactionDetailId.Value );
                financialTransactionDetail.EntityTypeId = null;
                financialTransactionDetail.EntityId = null;
                rockContext.SaveChanges();

                BindHtmlGrid();
                LoadRegistrationDropDowns();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the registration Instances.
        /// </summary>
        private void LoadRegistrationInstances()
        {
            ddlRegistrationInstance.SelectedValue = null;
            ddlRegistrationInstance.Items.Clear();
            if ( RegistrationTemplateId.HasValue )
            {
                var registrationInstanceId = RegistrationInstanceId;
                ddlRegistrationInstance.Items.Add( new ListItem() );

                var registrationInstanceService = new Rock.Model.RegistrationInstanceService( new RockContext() );
                var registrationInstances = registrationInstanceService.Queryable().Where( r => r.RegistrationTemplateId == RegistrationTemplateId.Value && r.IsActive ).OrderBy( a => a.Name ).ToList();

                foreach ( var r in registrationInstances )
                {
                    var item = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    item.Selected = r.Id == registrationInstanceId;
                    ddlRegistrationInstance.Items.Add( item );
                }
            }
        }

        /// <summary>
        /// Creates the table controls.
        /// </summary>
        private void BindHtmlGrid()
        {
            _financialTransactionDetailList = null;
            RockContext rockContext = new RockContext();

            List<DataControlField> tableColumns = new List<DataControlField>();
            tableColumns.Add( new RockLiteralField { ID = "lPerson", HeaderText = "Person" } );
            tableColumns.Add( new RockLiteralField { ID = "lTransactionInfo", HeaderText = "Transaction Info" } );
            tableColumns.Add( new RockLiteralField { ID = "lCheckImage", HeaderText = "Check Image" } );
            tableColumns.Add( new RockLiteralField { ID = "lMatchedRegistration", HeaderText = "Matched Registration" } );
            tableColumns.Add( new RockLiteralField { ID = "lButton" } );

            StringBuilder headers = new StringBuilder();
            foreach ( var tableColumn in tableColumns )
            {
                if ( tableColumn.HeaderStyle.CssClass.IsNotNullOrWhiteSpace() )
                {
                    headers.AppendFormat( "<th class='{0}'>{1}</th>", tableColumn.HeaderStyle.CssClass, tableColumn.HeaderText );
                }
                else
                {
                    headers.AppendFormat( "<th>{0}</th>", tableColumn.HeaderText );
                }
            }

            lHeaderHtml.Text = headers.ToString();
            var registrationEntityTypeId = EntityTypeCache.GetId<Registration>();

            if ( BatchId.HasValue && RegistrationInstanceId.HasValue )
            {
                nbErrorMessage.Visible = false;
                try
                {
                    var financialTransactionDetailQuery = new FinancialTransactionDetailService( rockContext ).Queryable()
                    .Include( a => a.Transaction )
                    .Include( a => a.Transaction.AuthorizedPersonAlias.Person )
                    .Where( a => a.Transaction.BatchId == BatchId.Value && ( !a.EntityTypeId.HasValue || a.EntityTypeId == registrationEntityTypeId ) )
                    .OrderByDescending( a => a.Transaction.TransactionDateTime );

                    _financialTransactionDetailList = financialTransactionDetailQuery.Take( 1000 ).ToList();
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );

                    if ( sqlTimeoutException != null )
                    {
                        nbErrorMessage.NotificationBoxType = NotificationBoxType.Warning;
                        nbErrorMessage.Text = "This report did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                    }
                    else
                    {
                        nbErrorMessage.Text = "There was a problem with one of the filters for this report's dataview.";
                        nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
                        nbErrorMessage.Details = ex.Message;
                        nbErrorMessage.Visible = true;
                        return;
                    }
                }

                phTableRows.Controls.Clear();

                int rowCount = 0;
                foreach ( var financialTransactionDetail in _financialTransactionDetailList )
                {
                    rowCount += 1;
                    var tr = new HtmlGenericContainer( "tr" );
                    tr.ID = "tr_" + rowCount;
                    foreach ( var tableColumn in tableColumns )
                    {
                        var literalControl = new LiteralControl();
                        if ( tableColumn is RockLiteralField )
                        {
                            tr.Controls.Add( literalControl );
                            var literalTableColumn = tableColumn as RockLiteralField;
                            if ( literalTableColumn.ID == "lPerson" )
                            {
                                literalControl.Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Transaction.AuthorizedPersonAlias );
                            }
                            else if ( literalTableColumn.ID == "lTransactionInfo" )
                            {
                                literalControl.Text = string.Format( "<td>{0}<br/>{1}</td>", financialTransactionDetail.Amount.FormatAsCurrency(), financialTransactionDetail.Account.ToString() );
                            }
                            else if ( literalTableColumn.ID == "lCheckImage" )
                            {
                                var primaryImage = financialTransactionDetail.Transaction.Images
                                    .OrderBy( i => i.Order )
                                    .FirstOrDefault();
                                string imageTag = string.Empty;
                                if ( primaryImage != null )
                                {
                                    var imageUrl = FileUrlHelper.GetImageUrl( primaryImage.BinaryFileId );
                                    imageTag = string.Format( "<div class='photo transaction-image' style='max-width: 400px;'><a href='{0}'><img src='{0}'/></a></div>", ResolveRockUrl( imageUrl ) );
                                }

                                literalControl.Text = string.Format( "<td>{0}</td>", imageTag );
                            }
                            else if ( literalTableColumn.ID == "lTransactionType" )
                            {
                                literalControl.ID = "lTransactionType_" + financialTransactionDetail.Id.ToString();
                                literalControl.Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Transaction.TransactionTypeValue );
                            }
                            else if ( literalTableColumn.ID == "lMatchedRegistration" )
                            {
                                if ( financialTransactionDetail.EntityTypeId == registrationEntityTypeId && financialTransactionDetail.EntityId.HasValue )
                                {
                                    literalControl.ID = "lMatchedRegistration_" + financialTransactionDetail.Id.ToString();
                                    literalControl.Text = string.Format( "<td></td>" );
                                }
                                else
                                {
                                    var tdEntityControls = new HtmlGenericContainer( "td" ) { ID = "lMatchedRegistration_" + financialTransactionDetail.Id.ToString() };
                                    tr.Controls.Add( tdEntityControls );
                                    var ddlRegistration = new RockDropDownList { ID = "ddlRegistration_" + financialTransactionDetail.Id.ToString(), EnhanceForLongLists = true };
                                    ddlRegistration.Label = "Registration";
                                    ddlRegistration.AutoPostBack = true;
                                    ddlRegistration.SelectedIndexChanged += ddlRegistration_SelectedIndexChanged;
                                    tdEntityControls.Controls.Add( ddlRegistration );
                                }
                            }
                            else if ( literalTableColumn.ID == "lButton" )
                            {
                                var tdEntityControls = new HtmlGenericContainer( "td" ) { ID = "pnlBtnControls_" + financialTransactionDetail.Id.ToString() };
                                tr.Controls.Add( tdEntityControls );
                                var lbDelete = new LinkButton { ID = "lbDelete_" + financialTransactionDetail.Id.ToString() };
                                lbDelete.CausesValidation = false;
                                lbDelete.Click += lbDelete_Click;
                                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                                buttonIcon.Attributes.Add( "class", "fa fa-close" );
                                lbDelete.Controls.Add( buttonIcon );
                                tdEntityControls.Controls.Add( lbDelete );
                                lbDelete.Visible = financialTransactionDetail.EntityTypeId == registrationEntityTypeId && financialTransactionDetail.EntityId.HasValue;
                            }
                        }
                    }

                    phTableRows.Controls.Add( tr );

                    pnlTransactions.Visible = true;
                }
            }
            else
            {
                pnlTransactions.Visible = false;
            }
        }

        /// <summary>
        /// Get the registration name
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="isEnabled">If true, edit mode is enabled.</param>
        /// <param name="selectedRegistrationInstanceId">The selected registration instance identifier.</param>
        private string GetRegistrationName( Registration registration, bool isEdit, int selectedRegistrationInstanceId )
        {
            string registrationName = string.Empty;
            if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
            {
                registrationName = registration.PersonAlias.Person.FullName;
            }
            else
            {
                registrationName = string.Format( "{0} {1}", registration.FirstName, registration.LastName );
            }

            var registrantNames = registration.Registrants
                .Where( r => r.PersonAlias != null && r.PersonAlias.Person != null );
            if ( isEdit )
            {
                registrationName += string.Format( "- {0} - {1}", registration.CreatedDateTime.ToShortDateString(), registrantNames.Select( a => a.Person.NickName ).ToList().AsDelimited( ", " ) );
            }
            else
            {
                registrationName += string.Format( "<small> {0}</small>", registration.CreatedDateTime.ToShortDateString() );
                if ( registration.RegistrationInstanceId != selectedRegistrationInstanceId )
                {
                    registrationName += string.Format( "</br><small>{0}</small>", registration.RegistrationInstance.Name );
                }

                registrationName += string.Format( "</br><small>{0}</small>", registrantNames.Select( a => a.Person.FullName ).ToList().AsDelimited( ", " ) );
            }

            return registrationName;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var rockContext = new RockContext();
            var financialBatchList = new FinancialBatchService( rockContext ).Queryable()
                .Where( a => a.Status == BatchStatus.Open ).OrderBy( a => a.Name ).Select( a => new
                {
                    a.Id,
                    a.Name,
                    a.BatchStartDateTime
                } ).ToList();

            ddlBatch.Items.Clear();
            ddlBatch.Items.Add( new ListItem() );
            foreach ( var batch in financialBatchList )
            {
                ddlBatch.Items.Add( new ListItem( string.Format( "#{0} {1} ({2})", batch.Id, batch.Name, batch.BatchStartDateTime.Value.ToString( "d" ) ), batch.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Loads the registration drop downs.
        /// </summary>
        private void LoadRegistrationDropDowns()
        {
            if ( _financialTransactionDetailList == null || !RegistrationInstanceId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var registrationService = new RegistrationService( rockContext );

            var registationListForInstance = registrationService
                .Queryable()
                .AsNoTracking()
                .Include( t => t.PersonAlias.Person )
                .Include( a => a.Registrants )
                .Where( a => a.RegistrationInstanceId == RegistrationInstanceId.Value )
                .AsEnumerable();

            if ( cbHideFullyPaidRegistrations.Checked )
            {
                // BalanceDue is a calculated value so we have to get the enumerable before filtering on it.
                registationListForInstance = registationListForInstance.Where( a => a.BalanceDue > 0 );
            }

            /*
             * 2021-05-20 ETD
             * Run the query now to prevent EF from hitting the DB every interation to get the includes.
            */
            var registationListForInstanceFiltered = registationListForInstance.ToList();

            foreach ( var ddlRegistration in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlRegistration_" ) ) )
            {
                ddlRegistration.Items.Clear();
                ddlRegistration.Items.Add( new ListItem() );
                foreach ( var registration in registationListForInstanceFiltered )
                {
                    ddlRegistration.Items.Add( new ListItem( GetRegistrationName( registration, true, RegistrationInstanceId.Value ), registration.Id.ToString() ) );
                }

                // if there is no value, make sure the controls don't have anything selected
                ddlRegistration.SetValue( ( int? ) null );
            }

            Dictionary<int, int?> entityLookup = _financialTransactionDetailList.Where( a => a.EntityId.HasValue ).ToDictionary( k => k.Id, v => v.EntityId );

            var registrationList = registrationService
                .Queryable()
                .AsNoTracking()
                .Include( a => a.RegistrationInstance ).Include( t => t.PersonAlias.Person ).Include( a => a.Registrants )
                .Where( a => entityLookup.Values.Contains( a.Id ) )
                .ToList();

            foreach ( var lMatchedRegistration in phTableRows.ControlsOfTypeRecursive<LiteralControl>().Where( a => a.ID != null && a.ID.StartsWith( "lMatchedRegistration_" ) ) )
            {
                var financialTransactionDetailId = lMatchedRegistration.ID.Replace( "lMatchedRegistration_", string.Empty ).AsInteger();
                var registrationId = entityLookup.GetValueOrNull( financialTransactionDetailId );
                var registration = registrationList.FirstOrDefault( a => a.Id == registrationId );
                if ( registration != null )
                {
                    lMatchedRegistration.Text = "<td>" + GetRegistrationName( registration, false, RegistrationInstanceId.Value ) + "</td>";
                }
            }
        }

        #endregion
    }
}