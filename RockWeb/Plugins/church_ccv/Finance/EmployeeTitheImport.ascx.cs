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

namespace RockWeb.Plugins.church_ccv.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Employee Tithe Import" )]
    [Category( "CCV > Finance" )]
    [Description( "Imports Employee Tithe records" )]

    public partial class EmployeeTitheImport : Rock.Web.UI.RockBlock
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
            int lastNameCol = 0;
            int firstNameCol = 1;
            int locationCol = 2;
            int payDateCol = 3;
            int employeeNumberCol = 4;
            int campusColStartCol = 5;

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
            public int? AccountId { get; set; }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {

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
    }
}