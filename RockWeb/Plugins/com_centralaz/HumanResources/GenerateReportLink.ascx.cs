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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using com.centralaz.Accountability.Data;
using com.centralaz.Accountability.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.HumanResources
{
    [DisplayName( "Generate Report Link" )]
    [Category( "com_centralaz > Human Resources" )]
    [Description( "A block used to direct people to the report for a specified year." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [DateField( "Financial Year Start Date", "The date the financial year starts", true, "7/1/2016" )]
    public partial class GenerateReportLink : PersonBlock
    {

        #region Control Methods

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

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        #endregion

        #region Events
        protected void btnLastYear_Click( object sender, EventArgs e )
        {
            NavigateToWorksheetPage( -1 );
        }

        protected void btnCurrentYear_Click( object sender, EventArgs e )
        {
            NavigateToWorksheetPage( 0 );
        }

        protected void btnNextYear_Click( object sender, EventArgs e )
        {
            NavigateToWorksheetPage( 1 );
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Grabs the dates for the due date message and passes them to the WriteDueDateMessage method.
        /// </summary>
        protected void ShowDetail()
        {
            int currentYear = RockDateTime.Now.Year;
            var financialYearStartDate = GetAttributeValue( "FinancialYearStartDate" ).AsDateTime();
            if ( financialYearStartDate != null )
            {
                var fyStartDate = new DateTime( RockDateTime.Now.Year, financialYearStartDate.Value.Month, financialYearStartDate.Value.Day );
                if ( fyStartDate <= RockDateTime.Now )
                {
                    btnLastYear.Text = String.Format( "{0} / {1} Payroll Worksheet", ( currentYear - 1 ).ToString(), currentYear.ToString() );
                    btnCurrentYear.Text = String.Format( "{0} / {1} Payroll Worksheet", currentYear.ToString(), ( currentYear + 1 ).ToString() );
                    btnNextYear.Text = String.Format( "{0} / {1} Payroll Worksheet", ( currentYear + 1 ).ToString(), ( currentYear + 2 ).ToString() );
                }
                else
                {
                    btnLastYear.Text = String.Format( "{0} / {1} Payroll Worksheet", ( currentYear - 2 ).ToString(), ( currentYear - 1 ).ToString() );
                    btnCurrentYear.Text = String.Format( "{0} / {1} Payroll Worksheet", ( currentYear - 1 ).ToString(), currentYear.ToString() );
                    btnNextYear.Text = String.Format( "{0} / {1} Payroll Worksheet", currentYear.ToString(), ( currentYear + 1 ).ToString() );
                }
            }
        }

        protected void NavigateToWorksheetPage( int yearOffset )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "PersonId", Person.Id.ToString() );

            var financialYearStartDate = GetAttributeValue( "FinancialYearStartDate" ).AsDateTime();
            if ( financialYearStartDate != null )
            {
                var fyStartDate = new DateTime( RockDateTime.Now.Year, financialYearStartDate.Value.Month, financialYearStartDate.Value.Day );
                if ( fyStartDate <= RockDateTime.Now )
                {
                    queryParams.Add( "Year", RockDateTime.Now.AddYears( yearOffset ).Year.ToString() );
                }
                else
                {
                    queryParams.Add( "Year", RockDateTime.Now.AddYears( yearOffset - 1 ).Year.ToString() );
                }
            }

            NavigateToLinkedPage( "DetailPage", queryParams );

        }

        #endregion
    }
}