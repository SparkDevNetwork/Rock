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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Contribution Statement Template Entry" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Used for merging contribution data into output documents, such as Word, Html, using a pre-defined template." )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, "", 0 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Statement Frequency Attribute" )]

    [BinaryFileTypeField( "File Type", "The file type used to save the contribution statements.", true, "FC7218EE-EA28-4EA4-8C3D-F30750A2FE59" )]
    [SystemEmailField( "Notify Requester Email", "The system email used to notify the requester that the statements have been generated" )]
    public partial class ContributionStatementTemplateEntry : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
            }
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
            //
        }

        /// <summary>
        /// Handles the Click event of the btnMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMerge_Click( object sender, EventArgs e )
        {
            nbNotification.Visible = false;

            var rockContext = new RockContext();

            MergeTemplate statementTemplate = new MergeTemplateService( rockContext ).Get( mtpStatement.SelectedValue.AsInteger() );
            MergeTemplate letterTemplate = new MergeTemplateService( rockContext ).Get( mtpLetter.SelectedValue.AsInteger() );

            BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( GetAttributeValue( "FileType" ).AsGuid() );
            if ( binaryFileType == null )
            {
                nbWarningMessage.Text = "Unable to get file type";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            // Get the accounts that we want to list independently
            var accountService = new FinancialAccountService( rockContext );

            FinancialAccount account1 = new FinancialAccount();
            if ( apAccount1.SelectedValueAsId().HasValue )
            {
                account1 = accountService.Get( apAccount1.SelectedValueAsInt().Value );
            }

            FinancialAccount account2 = new FinancialAccount();
            if ( apAccount2.SelectedValueAsId().HasValue )
            {
                account2 = accountService.Get( apAccount2.SelectedValueAsInt().Value );
            }

            FinancialAccount account3 = new FinancialAccount();
            if ( apAccount3.SelectedValueAsId().HasValue )
            {
                account3 = accountService.Get( apAccount3.SelectedValueAsInt().Value );
            }

            FinancialAccount account4 = new FinancialAccount();
            if ( apAccount4.SelectedValueAsId().HasValue )
            {
                account4 = accountService.Get( apAccount4.SelectedValueAsInt().Value );
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDates.DelimitedValues );

            var transaction = new com.centralaz.Finance.Transactions.GenerateContributionStatementTransaction();
            transaction.Context = Context;
            transaction.Response = Response;
            transaction.DatabaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull();
            transaction.Account1 = account1;
            transaction.Account2 = account2;
            transaction.Account3 = account3;
            transaction.Account4 = account4;
            transaction.CampusIds = cblCampus.SelectedValuesAsInt;
            transaction.DefinedTypeAttribute = AttributeCache.Read( GetAttributeValue( "StatementFrequencyAttribute" ).AsGuid() );
            transaction.DefinedValueIds = cblStatementFrequency.SelectedValuesAsInt;
            transaction.ExcludedGroupId = gpExcludedGroup.SelectedValueAsId();
            transaction.StartDate = dateRange.Start;
            transaction.EndDate = dateRange.End;
            transaction.LetterTemplate = letterTemplate;
            transaction.StatementTemplate = statementTemplate;
            transaction.BinaryFileType = binaryFileType;
            transaction.SystemEmailGuid = GetAttributeValue( "NotifyRequesterEmail" ).AsGuidOrNull();
            transaction.Requestor = CurrentPerson;
            transaction.MinimumContributionAmount = nbMinimumAmount.Text.AsDecimal();
            transaction.ChapterSize = nbChapterSize.Text.AsInteger();
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            SetBlockUserPreference( "ChapterSize", nbChapterSize.Text );
            SetBlockUserPreference( "MinimumContribution", nbMinimumAmount.Text );
            SetBlockUserPreference( "StatementTemplate", mtpStatement.SelectedValue );
            SetBlockUserPreference( "LetterTemplate", mtpLetter.SelectedValue );
            SetBlockUserPreference( "Account1", apAccount1.SelectedValue );
            SetBlockUserPreference( "Account2", apAccount2.SelectedValue );
            SetBlockUserPreference( "Account3", apAccount3.SelectedValue );
            SetBlockUserPreference( "Account4", apAccount4.SelectedValue );
            SetBlockUserPreference( "Campuses", cblCampus.SelectedValuesAsInt.AsDelimited( "|" ) );
            SetBlockUserPreference( "Date Range", drpDates.DelimitedValues );
            SetBlockUserPreference( "ExcludedGroup", gpExcludedGroup.SelectedValue );
            SetBlockUserPreference( "StatementFrequencies", cblStatementFrequency.SelectedValuesAsInt.AsDelimited( "|" ) );


            nbNotification.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            nbNotification.Visible = false;
            var delimitedDateValues = GetBlockUserPreference( "Date Range" );

            if ( !String.IsNullOrWhiteSpace( delimitedDateValues ) )
            {
                drpDates.DelimitedValues = delimitedDateValues;
            }
            else
            {
                int year = DateTime.Now.Year;
                drpDates.DateRangeModeStart = new DateTime( year, 1, 1 );
                drpDates.DateRangeModeEnd = new DateTime( year, 12, 31 );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "ChapterSize" ) ) )
            {
                nbChapterSize.Text = GetBlockUserPreference( "ChapterSize" );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "MinimumContribution" ) ) )
            {
                nbMinimumAmount.Text = GetBlockUserPreference( "MinimumContribution" );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "StatementTemplate" ) ) )
            {
                mtpStatement.SetValue( GetBlockUserPreference( "StatementTemplate" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "LetterTemplate" ) ) )
            {
                mtpLetter.SetValue( GetBlockUserPreference( "LetterTemplate" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account1" ) ) )
            {
                apAccount1.SetValue( GetBlockUserPreference( "Account1" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account2" ) ) )
            {
                apAccount2.SetValue( GetBlockUserPreference( "Account2" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account3" ) ) )
            {
                apAccount3.SetValue( GetBlockUserPreference( "Account3" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account4" ) ) )
            {
                apAccount4.SetValue( GetBlockUserPreference( "Account4" ).AsIntegerOrNull() );
            }

            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Campuses" ) ) )
            {
                cblCampus.SetValues( GetBlockUserPreference( "Campuses" ).SplitDelimitedValues().AsIntegerList() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "ExcludedGroup" ) ) )
            {
                gpExcludedGroup.SetValue( GetBlockUserPreference( "ExcludedGroup" ).AsIntegerOrNull() );
            }

            var attributeGuid = GetAttributeValue( "StatementFrequencyAttribute" ).AsGuid();
            var personAttribute = AttributeCache.Read( attributeGuid );
            if ( personAttribute != null )
            {
                var qualifierValue = personAttribute.QualifierValues.Where( qv => qv.Key == "definedtype" ).First().Value;
                if ( qualifierValue != null && qualifierValue.Value != null )
                {
                    var definedType = DefinedTypeCache.Read( int.Parse( qualifierValue.Value ) );
                    if ( definedType != null )
                    {
                        cblStatementFrequency.Visible = true;
                        cblStatementFrequency.BindToDefinedType( definedType );

                        if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "StatementFrequencies" ) ) )
                        {
                            cblStatementFrequency.SetValues( GetBlockUserPreference( "StatementFrequencies" ).SplitDelimitedValues().AsIntegerList() );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the type of the merge template.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <returns></returns>
        private MergeTemplateType GetMergeTemplateType( RockContext rockContext, MergeTemplate mergeTemplate )
        {
            mergeTemplate = new MergeTemplateService( rockContext ).Get( mtpStatement.SelectedValue.AsInteger() );
            if ( mergeTemplate == null )
            {
                return null;
            }

            return mergeTemplate.GetMergeTemplateType();
        }

        #endregion
    }
}