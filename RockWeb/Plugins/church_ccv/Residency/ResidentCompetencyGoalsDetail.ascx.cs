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
using System.ComponentModel;
using System.Web.UI;
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Competency Goals Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a resident's competency's goals." )]

    public partial class ResidentCompetencyGoalsDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string competencyPersonId = PageParameter( "CompetencyPersonId" );
                if ( !string.IsNullOrWhiteSpace( competencyPersonId ) )
                {
                    ShowDetail( competencyPersonId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonId">The competency person identifier.</param>
        public void ShowDetail( int competencyPersonId )
        {
            pnlDetails.Visible = true;

            CompetencyPerson competencyPerson = new ResidencyService<CompetencyPerson>( new ResidencyContext() ).Get( competencyPersonId );

            if ( competencyPerson != null )
            {
                lblGoals.Text = ( competencyPerson.Competency.Goals ?? string.Empty ).Replace( "\n", "<br>" );
            }
        }

        #endregion
    }
}