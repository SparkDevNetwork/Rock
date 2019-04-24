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
using System.Data.Entity;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Contribution Statement List Lava" )]
    [Category( "Finance" )]
    [Description( "Block for displaying a listing of years where contribution statements are available." )]
    [AccountsField("Accounts", "A selection of accounts to use for checking if transactions for the current user exist. If no accounts are provided then all tax-deductible accounts will be considered.", false, order: 0 )]
    [IntegerField("Max Years To Display", "The maximum number of years to display (including the current year).", true, 3, order:1)]
    [LinkedPage("Detail Page", "The statement detail page.", order: 2)]
    [CodeEditorField("Lava Template", "The Lava template to use for the contribution statement.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, DefaultValue = @"{% assign currentYear = 'Now' | Date:'yyyy' %}

<h4>Available Contribution Statements</h4>

<div class=""margin-b-md"">
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>", Order = 3)]
    [BooleanField("Use Person Context", "Determines if the person context should be used instead of the CurrentPerson.", false, order: 5)]

    [ContextAware]
    public partial class ContributionStatementListLava : RockBlock, ISecondaryBlock
    {
        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( GetAttributeValue( "UsePersonContext" ).AsBoolean() )
            {
                TargetPerson = ContextEntity<Person>();
            }
            else
            {
                TargetPerson = CurrentPerson;
            }

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
                DisplayResults();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        private void DisplayResults()
        {
            var numberOfYears = GetAttributeValue( "MaxYearsToDisplay" ).AsInteger();

            RockContext rockContext = new RockContext();

            FinancialTransactionDetailService financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );

            List<int> personAliasIds;

            // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
            if ( TargetPerson != null )
            {
                personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == TargetPerson.GivingId ).Select( a => a.Id ).ToList();
            }
            else
            {
                personAliasIds = new List<int>();
            }

            // get the transactions for the person or all the members in the person's giving group (Family)
            var qry = financialTransactionDetailService.Queryable().AsNoTracking().Where( t =>
                t.Transaction.AuthorizedPersonAliasId.HasValue
                && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                && t.Transaction.TransactionDateTime.HasValue );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Accounts" ) ) )
            {
                qry = qry.Where( t => t.Account.IsTaxDeductible );
            } else
            {
                var accountGuids = GetAttributeValue( "Accounts" ).Split( ',' ).Select( Guid.Parse ).ToList();
                qry = qry.Where( t => accountGuids.Contains( t.Account.Guid ) );
            }

            var yearQry = qry.GroupBy( t => t.Transaction.TransactionDateTime.Value.Year )
                                .Select( g => new { Year = g.Key } )
                                .OrderByDescending(y => y.Year);

            var statementYears = yearQry.Take( numberOfYears ).ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "DetailPage", LinkedPageRoute( "DetailPage" ) );
            mergeFields.Add( "StatementYears", statementYears );

            if ( TargetPerson != null )
            {
                mergeFields.Add( "PersonGuid", TargetPerson.Guid );
            }
            
            var template = GetAttributeValue( "LavaTemplate" );

            lResults.Text = template.ResolveMergeFields( mergeFields );

            // don't show anything if the person doesn't have any transactions
            lResults.Visible = statementYears.Any();

        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}