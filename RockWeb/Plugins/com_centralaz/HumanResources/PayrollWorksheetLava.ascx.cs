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

                            var salaries = new SalaryService( rockContext ).Queryable().Where( s => s.PersonAlias.PersonId == person.Id ).OrderBy( s => s.EffectiveDate ).ToList();
                            mergeFields.Add( "Salaries", salaries );

                            var contributions = new ContributionElectionService( rockContext ).Queryable().Where( c => c.PersonAlias.PersonId == person.Id ).OrderBy( c => c.ActiveDate ).ToList();
                            mergeFields.Add( "Contributions", contributions );

                            var retirementFunds = new RetirementFundService( rockContext ).Queryable().Where( r => r.PersonAlias.PersonId == person.Id ).OrderBy( r => r.ActiveDate ).ToList();
                            mergeFields.Add( "RetirementFunds", retirementFunds );

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