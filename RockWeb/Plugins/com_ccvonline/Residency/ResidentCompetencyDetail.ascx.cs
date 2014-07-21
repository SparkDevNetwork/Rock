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
using System.Web.UI;
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Resident Competency Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a resident's competency." )]

    [LinkedPage( "Detail Page" )]
    public partial class ResidentCompetencyDetail : RockBlock, IDetailBlock
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

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? competencyPersonId = this.PageParameter( pageReference, "CompetencyPersonId" ).AsInteger();
            if ( competencyPersonId != null )
            {
                CompetencyPerson competencyPerson = new ResidencyService<CompetencyPerson>( new ResidencyContext() ).Get( competencyPersonId.Value );
                if ( competencyPerson != null )
                {
                    breadCrumbs.Add( new BreadCrumb( competencyPerson.Competency.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Competency", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonId">The competency person identifier.</param>
        public void ShowDetail( int competencyPersonId )
        {
            pnlDetails.Visible = true;

            CompetencyPerson competencyPerson = new ResidencyService<CompetencyPerson>( new ResidencyContext() ).Get( competencyPersonId );

            if ( competencyPerson == null || competencyPerson.PersonId != CurrentPersonId )
            {
                // somebody besides the Resident is logged in
                NavigateToParentPage();
                return;
            }

            hfCompetencyPersonId.Value = competencyPersonId.ToString();
            lReadOnlyTitle.Text  = competencyPerson.Competency.Name.FormatAsHtmlTitle();

            lblFacilitator.Text = competencyPerson.Competency.FacilitatorPerson != null ? competencyPerson.Competency.FacilitatorPerson.FullName : Rock.Constants.None.TextHtml;
            lblDescription.Text = !string.IsNullOrWhiteSpace( competencyPerson.Competency.Description ) ? competencyPerson.Competency.Description : Rock.Constants.None.TextHtml;
        }
    }
}