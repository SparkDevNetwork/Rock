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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Finance
{
    /// <summary>
    /// Imports Employee Tithe records
    /// </summary>
    [DisplayName( "Employee Tithe Import" )]
    [Category( "CCV > Finance" )]
    [Description( "Imports Employee Tithe records" )]

    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Payroll Employee ID Attribute", "Sets which attribute to use as the Payroll Employee ID Attribute. If nothing is selected, defaults to 'PayrollEmployeeID'" )]
    [LinkedPage( "Financial Batch Page" )]
    [TextField( "Default Batch Name Format", defaultValue: "Employee Tithe Import {{ 'Now' | Date:'MM/dd/yyyy' }}" )]
    public partial class EmployeeTitheImport : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            gImportPreview.GridRebind += gImportPreview_GridRebind;
            gImportPreview.Actions.ShowMergeTemplate = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnViewBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewBatch_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "FinancialBatchPage", "BatchId", hfBatchId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the FileUploaded event of the fuImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fuImport_FileUploaded( object sender, EventArgs e )
        {
            pnlStart.Visible = false;
            pnlConfigure.Visible = true;
            pnlImportPreview.Visible = false;
            pnlDone.Visible = false;
            int campusColStartCol = 5;

            string keyPrefix = GetUserPreferencesKeyPrefix();

            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( fuImport.BinaryFileId ?? 0 );
            List<CampusAccountMapping> campusCodes = new List<CampusAccountMapping>();
            if ( binaryFile != null )
            {
                string importData = binaryFile.ContentsToString();
                var importLines = importData.Split( new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries );
                if ( importLines.Count() > 0 )
                {
                    string headerLine = importLines[0];
                    var headerLineParts = headerLine.Split( ',' );
                    if ( headerLineParts.Count() > 5 )
                    {
                        for ( int campusColumnIndex = campusColStartCol; campusColumnIndex < headerLineParts.Count(); campusColumnIndex++ )
                        {
                            var campusAccountMapping = new CampusAccountMapping { CampusCode = headerLineParts[campusColumnIndex] };
                            campusAccountMapping.FinancialAccountId = this.GetUserPreference( keyPrefix + campusAccountMapping.CampusCode + "_account_id" ).AsIntegerOrNull();
                            campusCodes.Add( campusAccountMapping );
                        }
                    }
                }
            }

            gMapAccounts.DataSource = campusCodes;
            gMapAccounts.DataBind();

            var currencyTypeValues = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value );

            ddlCurrencyType.Items.Clear();
            foreach ( var currencyType in currencyTypeValues )
            {
                ddlCurrencyType.Items.Add( new ListItem( currencyType.Value, currencyType.Id.ToString() ) );
            }

            ddlCurrencyType.SetValue( this.GetUserPreference( keyPrefix + "currency-type-value-id" ) );

            tbBatchNameFormat.Text = this.GetUserPreference( keyPrefix + "batch-name-format" );
            if ( string.IsNullOrWhiteSpace( tbBatchNameFormat.Text ) )
            {
                tbBatchNameFormat.Text = this.GetAttributeValue("DefaultBatchNameFormat");
            }
        }

        /// <summary>
        /// Gets the user preferences key prefix.
        /// </summary>
        /// <returns></returns>
        private string GetUserPreferencesKeyPrefix()
        {
            string keyPrefix = string.Format( "employee-tithe-import-{0}-", this.BlockId );
            return keyPrefix;
        }

        /// <summary>
        /// 
        /// </summary>
        public class CampusAccountMapping
        {
            /// <summary>
            /// Gets or sets the campus code.
            /// </summary>
            /// <value>
            /// The campus code.
            /// </value>
            public string CampusCode { get; set; }

            /// <summary>
            /// Gets or sets the account identifier.
            /// </summary>
            /// <value>
            /// The account identifier.
            /// </value>
            public int? FinancialAccountId { get; set; }

            /// <summary>
            /// Gets or sets the financial account.
            /// </summary>
            /// <value>
            /// The financial account.
            /// </value>
            public FinancialAccount FinancialAccount { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class FinancialAccountAmountInfo
        {
            /// <summary>
            /// Gets or sets the financial account.
            /// </summary>
            /// <value>
            /// The financial account.
            /// </value>
            public FinancialAccount FinancialAccount { get; set; }

            /// <summary>
            /// Gets or sets the amount.
            /// </summary>
            /// <value>
            /// The amount.
            /// </value>
            public decimal? Amount { get; set; }

            public override string ToString()
            {
                if ( Amount.HasValue )
                {
                    return Amount.FormatAsCurrency();
                }
                else
                {
                    return base.ToString();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ImportRowData
        {
            /// <summary>
            /// Gets or sets the employee identifier.
            /// </summary>
            /// <value>
            /// The employee identifier.
            /// </value>
            public int EmployeeId { get; set; }

            /// <summary>
            /// Gets or sets the first name of the import.
            /// </summary>
            /// <value>
            /// The first name of the import.
            /// </value>
            public string ImportFirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the import.
            /// </summary>
            /// <value>
            /// The last name of the import.
            /// </value>
            public string ImportLastName { get; set; }

            /// <summary>
            /// Gets or sets the pay date.
            /// </summary>
            /// <value>
            /// The pay date.
            /// </value>
            public DateTime? PayDate { get; set; }

            /// <summary>
            /// Gets or sets the rock person.
            /// </summary>
            /// <value>
            /// The rock person.
            /// </value>
            public Person RockPerson { get; set; }

            /// <summary>
            /// Gets or sets the financial account amount information list.
            /// </summary>
            /// <value>
            /// The financial account amount information list.
            /// </value>
            public List<FinancialAccountAmountInfo> FinancialAccountAmountInfoList { get; set; }

            /// <summary>
            /// Gets the name of the import person.
            /// </summary>
            /// <value>
            /// The name of the import person.
            /// </value>
            public string ImportPersonName
            {
                get
                {
                    return string.Format( "{0} {1}", ImportFirstName, ImportLastName );
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return this.RockPerson != null ? this.RockPerson.ToString() : this.ImportPersonName;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gImportPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gImportPreview_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            string keyPrefix = GetUserPreferencesKeyPrefix();
            foreach ( var row in gMapAccounts.Rows.OfType<GridViewRow>() )
            {
                var ddlAccount = row.FindControl( "ddlAccount" ) as DropDownList;
                var hfCampusCode = row.FindControl( "hfCampusCode" ) as HiddenField;
                if ( ddlAccount != null && hfCampusCode != null )
                {
                    this.SetUserPreference( keyPrefix + hfCampusCode.Value + "_account_id", ddlAccount.SelectedValue );
                }
            }

            this.SetUserPreference( keyPrefix + "currency-type-value-id", ddlCurrencyType.SelectedValue );

            pnlStart.Visible = false;
            pnlConfigure.Visible = false;
            pnlImportPreview.Visible = true;
            pnlDone.Visible = false;
            int lastNameCol = 0;
            int firstNameCol = 1;

            //// not Used
            //// int locationCol = 2;

            int payDateCol = 3;
            int employeeNumberCol = 4;
            int campusColStartCol = 5;
            List<CampusAccountMapping> campusAccountMappingList = new List<CampusAccountMapping>();

            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var financialAccountService = new FinancialAccountService( rockContext );
            var personService = new PersonService( rockContext );
            var binaryFile = binaryFileService.Get( fuImport.BinaryFileId ?? 0 );
            if ( binaryFile != null )
            {
                string importData = binaryFile.ContentsToString();
                var importLines = importData.Split( new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries );
                string headerLine = importLines[0];
                var headerLineParts = headerLine.Split( ',' );
                if ( headerLineParts.Count() > 5 )
                {
                    for ( int campusColumnIndex = campusColStartCol; campusColumnIndex < headerLineParts.Count(); campusColumnIndex++ )
                    {
                        var campusAccountMapping = new CampusAccountMapping { CampusCode = headerLineParts[campusColumnIndex] };
                        campusAccountMapping.FinancialAccountId = this.GetUserPreference( keyPrefix + campusAccountMapping.CampusCode + "_account_id" ).AsIntegerOrNull();
                        if ( campusAccountMapping.FinancialAccountId.HasValue )
                        {
                            campusAccountMapping.FinancialAccount = financialAccountService.Get( campusAccountMapping.FinancialAccountId.Value );
                        }

                        campusAccountMappingList.Add( campusAccountMapping );
                    }
                }

                foreach ( var callbackField in gImportPreview.Columns.OfType<CallbackField>().ToList() )
                {
                    gImportPreview.Columns.Remove( callbackField );
                }

                foreach ( var campusAccountMapping in campusAccountMappingList )
                {
                    CallbackField callbackField = new CallbackField();
                    callbackField.DataField = "FinancialAccountAmountInfoList";
                    if ( campusAccountMapping.FinancialAccount != null )
                    {
                        callbackField.HeaderText = campusAccountMapping.FinancialAccount.ToString();
                    }
                    else
                    {
                        callbackField.HeaderText = "<span class='label label-danger'>" + campusAccountMapping.CampusCode + ": Not Mapped</span>";
                        callbackField.HtmlEncode = false;
                    }

                    callbackField.OnFormatDataValue += ( s, args ) =>
                    {
                        var financialAccountAmountInfoList = args.DataValue as List<FinancialAccountAmountInfo>;
                        if ( financialAccountAmountInfoList != null )
                        {
                            var employeeAccountAmount = financialAccountAmountInfoList.FirstOrDefault( a => a.FinancialAccount == campusAccountMapping.FinancialAccount );
                            if ( employeeAccountAmount != null && employeeAccountAmount.Amount.HasValue && employeeAccountAmount.Amount != 0)
                            {
                                args.FormattedValue = employeeAccountAmount.Amount.FormatAsCurrency();
                            }
                            else
                            {
                                args.FormattedValue = string.Empty;
                            }
                        }
                    };

                    if ( !gImportPreview.Columns.OfType<CallbackField>().Any( a => a.HeaderText == callbackField.HeaderText ) )
                    {
                        gImportPreview.Columns.Add( callbackField );
                    }
                }

                List<ImportRowData> importDataRows = new List<ImportRowData>();

                var employeePayrollIDAttributeGuid = this.GetAttributeValue( "PayrollEmployeeIDAttribute" ).AsGuidOrNull();
                string employeePayrollIDAttributeKey = "PayrollEmployeeID";
                if ( employeePayrollIDAttributeGuid.HasValue )
                {
                    var employeePayrollIDAttribute = AttributeCache.Read( employeePayrollIDAttributeGuid.Value );
                    if ( employeePayrollIDAttribute != null )
                    {
                        employeePayrollIDAttributeKey = employeePayrollIDAttribute.Key;
                    }
                }

                var importDataLines = importLines.Skip( 1 ).ToList();
                foreach ( var importDataLine in importDataLines )
                {
                    var importDataLineParts = importDataLine.Split( ',' );
                    if ( importDataLines.Count >= campusColStartCol )
                    {
                        if ( importDataLineParts.Any( a => !string.IsNullOrWhiteSpace( a ) ) )
                        {
                            var importDataRow = new ImportRowData();
                            importDataRow.EmployeeId = importDataLineParts[employeeNumberCol].AsInteger();
                            importDataRow.ImportLastName = importDataLineParts[lastNameCol];
                            importDataRow.ImportFirstName = importDataLineParts[firstNameCol];
                            importDataRow.PayDate = importDataLineParts[payDateCol].AsDateTime();
                            importDataRow.FinancialAccountAmountInfoList = new List<FinancialAccountAmountInfo>();

                            // Payroll Employee ID is an Integer Attribute, so make sure to trim any leading zeros
                            int employeeId = importDataRow.EmployeeId;
                            importDataRow.RockPerson = personService.Queryable().WhereAttributeValue( rockContext, employeePayrollIDAttributeKey, employeeId.ToString() ).FirstOrDefault();

                            for ( int campusColumnIndex = campusColStartCol; campusColumnIndex < importDataLineParts.Count(); campusColumnIndex++ )
                            {
                                FinancialAccountAmountInfo financialAccountAmountInfo = new FinancialAccountAmountInfo();
                                financialAccountAmountInfo.Amount = importDataLineParts[campusColumnIndex].AsDecimalOrNull();
                                if ( headerLineParts.Count() > campusColumnIndex )
                                {
                                    var mapped = campusAccountMappingList.FirstOrDefault( a => a.CampusCode == headerLineParts[campusColumnIndex] );
                                    if ( mapped != null )
                                    {
                                        financialAccountAmountInfo.FinancialAccount = mapped.FinancialAccount;
                                    }
                                }

                                importDataRow.FinancialAccountAmountInfoList.Add( financialAccountAmountInfo );
                            }

                            importDataRows.Add( importDataRow );
                        }
                    }
                }

                int unmatchedCount = importDataRows.Where( a => a.RockPerson == null ).Count();
                string labelClass = unmatchedCount > 0 ? "danger" : "success";
                lUnmatchedRecords.Text = string.Format( "<span class='label label-{0}'>Unmatched Records: {1}</span>", labelClass, unmatchedCount );
                nbImportWarning.Visible = unmatchedCount > 0;
                btnImport.Enabled = unmatchedCount == 0;

                if ( gImportPreview.SortProperty != null )
                {
                    if ( gImportPreview.SortProperty.Property == "RockPerson" )
                    {
                        importDataRows = importDataRows.OrderBy( a => a.RockPerson == null ? string.Empty : a.RockPerson.LastName ).ThenBy( a => a.RockPerson == null ? string.Empty : a.RockPerson.FirstName ).ToList();
                        if ( gImportPreview.SortProperty.Direction == SortDirection.Descending )
                        {
                            importDataRows.Reverse();
                        }
                    }
                    else
                    {
                        importDataRows = importDataRows.AsQueryable().Sort( gImportPreview.SortProperty ).ToList();
                    }
                }

                gImportPreview.DataSource = importDataRows;
                gImportPreview.DataBind();

                var summaryList = importDataRows
                    .SelectMany( a => a.FinancialAccountAmountInfoList )
                    .GroupBy( a => a.FinancialAccount )
                    .Select( x => new
                    {
                        Name = x.Key != null ? x.Key.Name : "?",
                        TotalAmount = x.Sum( xx => xx.Amount ?? 0.00M )
                    } ).OrderBy( a => a.Name );

                lGrandTotal.Text = summaryList.Sum( a => a.TotalAmount ).FormatAsCurrency();

                rptAccountSummary.DataSource = summaryList.Select( a => new { a.Name, TotalAmount = a.TotalAmount.FormatAsCurrency() } ).ToList();
                rptAccountSummary.DataBind();
            }
        }

        #endregion

        private List<FinancialAccount> _financialAccountList { get; set; }

        /// <summary>
        /// Handles the RowCreated event of the gMapAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMapAccounts_RowCreated( object sender, GridViewRowEventArgs e )
        {
            if ( _financialAccountList == null )
            {
                _financialAccountList = new FinancialAccountService( new RockContext() ).Queryable().OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            }

            var ddlAccount = e.Row.FindControl( "ddlAccount" ) as RockDropDownList;
            if ( ddlAccount != null )
            {
                ddlAccount.Items.Clear();
                foreach ( var account in _financialAccountList )
                {
                    ddlAccount.Items.Add( new ListItem( account.Name, account.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gMapAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMapAccounts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var ddlAccount = e.Row.FindControl( "ddlAccount" ) as RockDropDownList;
            var hfCampusCode = e.Row.FindControl( "hfCampusCode" ) as HiddenField;
            CampusAccountMapping campusAccountMapping = e.Row.DataItem as CampusAccountMapping;
            if ( ddlAccount != null && campusAccountMapping != null && hfCampusCode != null )
            {
                ddlAccount.SelectedValue = campusAccountMapping.FinancialAccountId.ToString();
                hfCampusCode.Value = campusAccountMapping.CampusCode;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            BindGrid();
            this.SetUserPreference( GetUserPreferencesKeyPrefix() + "batch-name-format", tbBatchNameFormat.Text );
            List<ImportRowData> importDataRows = gImportPreview.DataSource as List<ImportRowData>;

            var rockContext = new RockContext();
            var financialBatchService = new FinancialBatchService( rockContext );
            var financialTransactionServcie = new FinancialTransactionService( rockContext );

            var currentDateTime = RockDateTime.Now;

            var financialBatch = new FinancialBatch();
            financialBatch.Name = tbBatchNameFormat.Text.ResolveMergeFields( GlobalAttributesCache.GetMergeFields( this.CurrentPerson ) );
            financialBatch.BatchStartDateTime = currentDateTime.Date;
            financialBatch.ControlAmount = lGrandTotal.Text.AsDecimal();
            financialBatch.Status = BatchStatus.Open;

            financialBatchService.Add( financialBatch );
            financialBatch.Transactions = new List<FinancialTransaction>();
            var contributionValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
            var sourceTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid() ).Id;
            foreach ( var importDataRow in importDataRows )
            {
                FinancialTransaction financialTransaction = new FinancialTransaction();
                financialTransaction.AuthorizedPersonAliasId = importDataRow.RockPerson != null ? importDataRow.RockPerson.PrimaryAliasId : null;
                financialTransaction.TransactionDateTime = RockDateTime.Now;
                financialTransaction.TransactionTypeValueId = contributionValueId;
                financialTransaction.SourceTypeValueId = sourceTypeValueId;
                financialTransaction.FinancialPaymentDetail = new FinancialPaymentDetail { CurrencyTypeValueId = ddlCurrencyType.SelectedValue.AsInteger() };
                financialTransaction.TransactionDetails = new List<FinancialTransactionDetail>();
                foreach ( var accountInfo in importDataRow.FinancialAccountAmountInfoList.Where( a => a.Amount.HasValue && a.Amount != 0 ) )
                {
                    var transactionDetail = new FinancialTransactionDetail();
                    transactionDetail.AccountId = accountInfo.FinancialAccount.Id;
                    transactionDetail.Amount = accountInfo.Amount.Value;
                    financialTransaction.TransactionDetails.Add( transactionDetail );
                }

                financialBatch.Transactions.Add( financialTransaction );
            }

            rockContext.SaveChanges();
            hfBatchId.Value = financialBatch.Id.ToString();

            pnlStart.Visible = false;
            pnlConfigure.Visible = false;
            pnlImportPreview.Visible = false;
            pnlDone.Visible = true;
            nbSuccess.Text = string.Format( "{0} records imported.", financialBatch.Transactions.Count() );
        }
    }
}