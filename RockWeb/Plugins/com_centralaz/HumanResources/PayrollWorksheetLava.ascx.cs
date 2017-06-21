// <copyright>
// Copyright by Central Christian Church
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
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using com.centralaz.HumanResources.Model;
using Rock.Lava;

namespace RockWeb.Plugins.com_centralaz.HumanResources
{
    [DisplayName( "Payroll Worksheet Lava" )]
    [Category( "com_centralaz > Human Resources" )]
    [Description( "Provides a payroll worksheet for the user to print off." )]

    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~/Plugins/com_centralaz/HumanResources/Lava/PayrollWorksheetLava.lava' %}", "", 0 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 1 )]
    public partial class PayrollWorksheetLava : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Generates the statement.
        /// </summary>
        private void ShowDetail()
        {
            using ( var rockContext = new RockContext() )
            {
                var year = PageParameter( "Year" ).AsIntegerOrNull();
                if ( year != null )
                {
                    var personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                    if ( personId != null )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                            mergeFields.Add( "Person", person );
                            mergeFields.Add( "Year", year );

                            DateTime yearStart = new DateTime( year.Value, 8, 1 );
                            DateTime defaultTime = new DateTime( 1900, 1, 1 );

                            var salaries = new SalaryService( rockContext ).Queryable().Where( s => s.PersonAlias.PersonId == person.Id ).OrderBy( s => s.EffectiveDate ).ToList();
                            Salary previousSalary = salaries.Where( s => s.EffectiveDate >= yearStart.AddYears( -1 ) && s.EffectiveDate < yearStart ).OrderByDescending( s => s.EffectiveDate ).FirstOrDefault();
                            Salary currentSalary = salaries.Where( s => s.EffectiveDate >= yearStart && s.EffectiveDate < yearStart.AddYears( 1 ) ).OrderByDescending( s => s.EffectiveDate ).FirstOrDefault();
                            Salary futureSalary = salaries.Where( s => s.EffectiveDate >= yearStart && s.EffectiveDate < yearStart.AddYears( 2 ) ).OrderByDescending( s => s.EffectiveDate ).FirstOrDefault();
                            mergeFields.Add( "PreviousSalary", previousSalary );
                            mergeFields.Add( "CurrentSalary", currentSalary );
                            mergeFields.Add( "FutureSalary", futureSalary );

                            var contributions = new ContributionElectionService( rockContext ).Queryable().Where( c =>
                            c.PersonAlias.PersonId == person.Id ).ToList().Where( c =>
                              c.ActiveDate < yearStart.AddYears( 2 ) &&
                              c.ActiveDate >= defaultTime &&
                              ( c.InactiveDate == null || c.InactiveDate == defaultTime || c.InactiveDate >= yearStart.AddYears( -1 ) ) )
                            .OrderBy( c => c.ActiveDate ).ToList();
                            var previousContributions = contributions.Where( c => c.ActiveDate >= defaultTime && c.ActiveDate < yearStart ).ToList();
                            var currentContributions = contributions.Where( c => c.ActiveDate >= defaultTime && c.ActiveDate < yearStart.AddYears( 1 ) && ( c.InactiveDate == null || c.InactiveDate == defaultTime || c.InactiveDate >= DateTime.Now ) ).ToList();
                            var lavaContributions = contributions.GroupJoin( previousContributions, c => c.FinancialAccountId, pc => pc.FinancialAccountId, ( c, pc ) => new { c, pc } )
                                .GroupJoin( currentContributions, x => x.c.FinancialAccountId, cc => cc.FinancialAccountId, ( x, cc ) => new { x.c, x.pc, cc } )
                                .Select( lc => new
                                {
                                    FinancialAccount = lc.c.FinancialAccount.Name,
                                    Order = lc.c.FinancialAccount.Order,
                                    CurrentActiveDate = lc.cc.OrderByDescending( cc => cc.ActiveDate ).FirstOrDefault() != null ? lc.cc.OrderByDescending( cc => cc.ActiveDate ).FirstOrDefault().ActiveDate : (DateTime?)null,
                                    CurrentFixed = lc.cc.OrderByDescending( cc => cc.ActiveDate ).FirstOrDefault() != null ? lc.cc.OrderByDescending( cc => cc.ActiveDate ).FirstOrDefault().IsFixedAmount : (bool?)null,
                                    CurrentAmount = lc.cc.OrderByDescending( cc => cc.ActiveDate ).FirstOrDefault() != null ? lc.cc.OrderByDescending( cc => cc.ActiveDate ).FirstOrDefault().Amount : (double?)null,
                                    PreviousActiveDate = lc.pc.OrderByDescending( pc => pc.ActiveDate ).FirstOrDefault() != null ? lc.pc.OrderByDescending( pc => pc.ActiveDate ).FirstOrDefault().ActiveDate : (DateTime?)null,
                                    PreviousFixed = lc.pc.OrderByDescending( pc => pc.ActiveDate ).FirstOrDefault() != null ? lc.pc.OrderByDescending( pc => pc.ActiveDate ).FirstOrDefault().IsFixedAmount : (bool?)null,
                                    PreviousAmount = lc.pc.OrderByDescending( pc => pc.ActiveDate ).FirstOrDefault() != null ? lc.pc.OrderByDescending( pc => pc.ActiveDate ).FirstOrDefault().Amount : (double?)null
                                } )
                                .Distinct()
                                .OrderBy( lc => lc.Order )
                                .ToList();
                            mergeFields.Add( "Contributions", lavaContributions );

                            var retirementFunds = new RetirementFundService( rockContext ).Queryable().Where( r => r.PersonAlias.PersonId == person.Id ).ToList().Where( r =>
                              r.ActiveDate < yearStart.AddYears( 2 ) &&
                              r.ActiveDate >= defaultTime &&
                              ( r.InactiveDate == null || r.InactiveDate == defaultTime || r.InactiveDate >= yearStart.AddYears( -1 ) ) )
                            .OrderBy( r => r.ActiveDate ).ToList();
                            var previousRetirementFunds = retirementFunds.Where( r => r.ActiveDate >= defaultTime && r.ActiveDate < yearStart ).ToList();
                            var rurrentRetirementFunds = retirementFunds.Where( r => r.ActiveDate >= defaultTime && r.ActiveDate < yearStart.AddYears( 1 ) && ( r.InactiveDate == null || r.InactiveDate == defaultTime || r.InactiveDate >= DateTime.Now ) ).ToList();
                            var lavaRetirementFunds = retirementFunds.GroupJoin( previousRetirementFunds, r => r.FundValueId, pc => pc.FundValueId, ( r, pr ) => new { r, pr } )
                                .GroupJoin( rurrentRetirementFunds, x => x.r.FundValueId, cr => cr.FundValueId, ( x, cr ) => new { x.r, x.pr, cr } )
                                .Select( lr => new
                                {
                                    FinancialAccount = lr.r.FundValue.Value,
                                    Order = lr.r.FundValue.Order,
                                    CurrentActiveDate = lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault() != null ? lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault().ActiveDate : (DateTime?)null,
                                    CurrentFixed = lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault() != null ? lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault().IsFixedAmount : false,
                                    CurrentEmployeeAmount = lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault() != null ? lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault().EmployeeAmount : 0,
                                    CurrentEmployerAmount = lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault() != null ? lr.cr.OrderByDescending( cr => cr.ActiveDate ).FirstOrDefault().EmployerAmount : 0,
                                    PreviousActiveDate = lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault() != null ? lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault().ActiveDate : (DateTime?)null,
                                    PreviousFixed = lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault() != null ? lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault().IsFixedAmount : false,
                                    PreviousEmployeeAmount = lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault() != null ? lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault().EmployeeAmount : 0,
                                    PreviousEmployerAmount = lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault() != null ? lr.pr.OrderByDescending( pr => pr.ActiveDate ).FirstOrDefault().EmployerAmount : 0
                                } )
                                .Distinct()
                                .OrderBy( lc => lc.Order )
                                .ToList();
                            mergeFields.Add( "RetirementFunds", lavaRetirementFunds );

                            lContent.Text = string.Empty;
                            if ( GetAttributeValue( "EnableDebug" ).AsBooleanOrNull().GetValueOrDefault( false ) )
                            {
                                lContent.Text = mergeFields.lavaDebugInfo( rockContext );
                            }

                            string template = GetAttributeValue( "LavaTemplate" );

                            lContent.Text += template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
                        }
                    }
                }
            }
        }

        #endregion
    }
}