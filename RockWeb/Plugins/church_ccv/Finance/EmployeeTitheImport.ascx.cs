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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Finance
{
    /// <summary>
    /// Imports Employee Tithe records
    /// </summary>
    [DisplayName( "Employee Tithe Import" )]
    [Category( "CCV > Finance" )]
    [Description( "Imports Employee Tithe records" )]

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

            string keyPrefix = string.Format( "employee-tithe-import-{0}-", this.BlockId );

            

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
        }

        /// <summary>
        /// 
        /// </summary>
        public class ImportRowData
        {
            public string EmployeeId { get; set; }
            public string ImportFirstName { get; set; }
            public string ImportLastName { get; set; }
            public DateTime? PayDate { get; set; }
            public Person RockPerson { get; set; }
            public List<FinancialAccountAmountInfo> FinancialAccountAmountInfoList { get; set; }

            public string ImportPersonName
            {
                get
                {
                    return string.Format( "{0} {1}", ImportFirstName, ImportLastName );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            string keyPrefix = string.Format( "employee-tithe-import-{0}-", this.BlockId );
            foreach ( var row in gMapAccounts.Rows.OfType<GridViewRow>() )
            {
                var ddlAccount = row.FindControl( "ddlAccount" ) as DropDownList;
                var hfCampusCode = row.FindControl( "hfCampusCode" ) as HiddenField;
                if (ddlAccount != null && hfCampusCode != null)
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
                        campusAccountMappingList.Add( campusAccountMapping );
                    }
                }

                foreach ( var campusAccountMapping in campusAccountMappingList )
                {
                    CallbackField callbackField = new CallbackField();
                    callbackField.DataField = "FinancialAccountAmountInfoList";

                    callbackField.OnFormatDataValue += ( s, args ) =>
                    {
                        var financialAccountAmountInfoList = args.DataValue as List<FinancialAccountAmountInfo>;
                        if ( financialAccountAmountInfoList != null )
                        {
                            var employeeAccountAmount = financialAccountAmountInfoList.FirstOrDefault( a => a.FinancialAccount == campusAccountMapping.FinancialAccount );
                            if ( employeeAccountAmount != null )
                            {
                                args.FormattedValue = employeeAccountAmount.Amount.FormatAsCurrency();
                            }
                            else
                            {
                                args.FormattedValue = string.Empty;
                            }
                        }
                    };

                    gImportPreview.Columns.Add( callbackField );
                }

                List<ImportRowData> importDataRows = new List<ImportRowData>();

                var importDataLines = importLines.Skip( 1 ).ToList();
                foreach ( var importDataLine in importDataLines )
                {
                    var importDataLineParts = importDataLine.Split( ',' );
                    if ( importDataLines.Count >= campusColStartCol )
                    {
                        if ( importDataLineParts.Any( a => !string.IsNullOrWhiteSpace( a ) ) )
                        {
                            var importDataRow = new ImportRowData();
                            importDataRow.EmployeeId = importDataLineParts[employeeNumberCol];
                            importDataRow.ImportLastName = importDataLineParts[lastNameCol];
                            importDataRow.ImportFirstName = importDataLineParts[firstNameCol];
                            importDataRow.PayDate = importDataLineParts[payDateCol].AsDateTime();
                            importDataRow.FinancialAccountAmountInfoList = new List<FinancialAccountAmountInfo>();

                            for ( int campusColumnIndex = campusColStartCol; campusColumnIndex < importDataLineParts.Count(); campusColumnIndex++ )
                            {
                                FinancialAccountAmountInfo financialAccountAmountInfo = new FinancialAccountAmountInfo();
                                financialAccountAmountInfo.Amount = importDataLineParts[campusColumnIndex].AsDecimalOrNull();
                                importDataRow.FinancialAccountAmountInfoList.Add( financialAccountAmountInfo );
                            }

                            importDataRows.Add( importDataRow );
                        }
                    }
                }

                int unmatchedCount = importDataRows.Where( a => a.RockPerson == null ).Count();
                string labelClass = unmatchedCount > 0 ? "danger" : "success";
                lUnmatchedRecords.Text = string.Format( "<span class='label label-{0}'>Unmatched Records: {1}</span>", labelClass, unmatchedCount );

                gImportPreview.DataSource = importDataRows;
                gImportPreview.DataBind();
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

        }
        
}
}