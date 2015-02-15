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
using System.Linq;
using System.Web.UI;
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web;
using System.ComponentModel;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Assessments Report" )]
    [Category( "CCV > Residency" )]
    [Description( "Reports a summary of competency assessments for a resident." )]
    
    /// <summary>
    /// Reports summary of Assessments for ResidencyCompetencies for a Person
    /// </summary>
    public partial class CompetencyPersonAssessmentReport : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAssessmentSummary.DataKeyNames = new string[] { "id" };
            gAssessmentSummary.Actions.ShowAdd = false;
            gAssessmentSummary.GridRebind += gList_GridRebind;
            gAssessmentSummary.Actions.ShowAdd = false;
            gAssessmentSummary.IsDeleteEnabled = false;

            gAssessmentDetails.DataKeyNames = new string[] { "Id" };
            gAssessmentDetails.Actions.ShowAdd = false;
            gAssessmentDetails.IsDeleteEnabled = false;
            gAssessmentDetails.GridRebind += gList_GridRebind;
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
                // set reasonable defaults for Date/Range and Semester Name
                var currentResidencyPeriod = new ResidencyService<Period>( new ResidencyContext() ).Queryable().Where( a => a.StartDate <= DateTime.Now ).OrderByDescending( a => a.StartDate ).FirstOrDefault();
                string classSemesterName = string.Empty;
                if ( currentResidencyPeriod != null )
                {
                    if ( RockDateTime.Today >= currentResidencyPeriod.EndDate )
                    {
                        pDateRange.LowerValue = new DateTime( currentResidencyPeriod.EndDate.Value.Year, 1, 1 );
                        pDateRange.UpperValue = currentResidencyPeriod.EndDate;
                        classSemesterName = "Fall " + currentResidencyPeriod.StartDate.Value.Year.ToString();
                    }
                    else
                    {
                        pDateRange.LowerValue = currentResidencyPeriod.StartDate;
                        pDateRange.UpperValue = new DateTime( currentResidencyPeriod.StartDate.Value.Year, 12, 31 );
                        classSemesterName = "Spring " + currentResidencyPeriod.EndDate.Value.Year.ToString();
                    }

                    classSemesterName = "Fall Semester";
                }

                tbSemesterName.Text = classSemesterName;
                BindGrids();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handles the Click event of the btnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApply_Click( object sender, EventArgs e )
        {
            BindGrids();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrids()
        {
            var residencyContext = new ResidencyContext();
            int personId = this.PageParameter( "PersonId" ).AsInteger();
            var person = new PersonService( new RockContext() ).Get( personId );

            if ( person != null )
            {
                lbResidentName.Text = person.FullName;
            }

            lbSemester.Text = tbSemesterName.Text;
            lbCurrentDate.Text = DateTime.Today.ToShortDateString();
            DateTime startDate = pDateRange.LowerValue ?? DateTime.MinValue;
            DateTime endDate;
            if ( pDateRange.UpperValue.HasValue )
            {
                endDate = pDateRange.UpperValue.Value.AddDays( 1 );
            }
            else
            {
                endDate = DateTime.MaxValue;
            }

            DateTime dateStartFilter = pDateRange.LowerValue ?? DateTime.Today;
            var closestResidencyPeriodForDateRange = new ResidencyService<Period>( residencyContext ).Queryable().Where( a => a.StartDate <= dateStartFilter ).OrderByDescending( a => a.StartDate ).FirstOrDefault();
            if ( closestResidencyPeriodForDateRange != null )
            {
                lbYear.Text = closestResidencyPeriodForDateRange.Name;
            }

            // Assessment Summary Grid
            var competencyPersonService = new ResidencyService<CompetencyPerson>( residencyContext );

            var qryAssessmentSummary = competencyPersonService.Queryable()
                .Where( a => a.PersonId.Equals( personId ) )
                .Select( a => new
                {
                    Id = a.Id,
                    TrackDisplayOrder = a.Competency.Track.DisplayOrder,
                    TrackName = a.Competency.Track.Name,
                    CompetencyName = a.Competency.Name,
                    OverallRating = a.CompetencyPersonProjects.Select( p => p.CompetencyPersonProjectAssessments ).SelectMany( x => x )
                        .Where( n => n.AssessmentDateTime != null )
                        .Where( n => n.AssessmentDateTime > startDate && n.AssessmentDateTime <= endDate )
                        .Select( s => s.OverallRating )
                        .Average()
                } );

            qryAssessmentSummary = qryAssessmentSummary.OrderBy( s => s.TrackDisplayOrder ).ThenBy( s => s.TrackName ).ThenBy( s => s.CompetencyName );
            gAssessmentSummary.DataSource = qryAssessmentSummary.ToList();
            gAssessmentSummary.DataBind();

            // Assessment Details Grid
            var personProjectAssessmentList = new ResidencyService<CompetencyPersonProjectAssessment>( residencyContext )
                .Queryable( "AssessorPerson,CompetencyPersonProject" )
                .Where( a => a.CompetencyPersonProject.CompetencyPerson.PersonId == personId )
                .OrderBy( a => a.CompetencyPersonProject.CompetencyPerson.Person.LastName )
                .ThenBy( a => a.CompetencyPersonProject.CompetencyPerson.Person.NickName )
                .ThenBy( a => a.CompetencyPersonProject.CompetencyPerson.Person.FirstName )
                .ThenBy( a => a.CompetencyPersonProject.CompetencyPerson.Competency.Name )
                .ThenBy( a => a.CompetencyPersonProject.Project.Name )
                .ThenBy( a => a.AssessmentDateTime )
                .Where( n => n.AssessmentDateTime > startDate && n.AssessmentDateTime <= endDate )
                .Select( s => new
                {
                    Id = s.Id,
                    Competency = s.CompetencyPersonProject.Project.Competency,
                    ProjectName = s.CompetencyPersonProject.Project.Name,
                    ProjectDescription = s.CompetencyPersonProject.Project.Description,
                    Evaluator = s.AssessorPerson,
                    s.AssessmentDateTime,
                    s.OverallRating,
                    s.RatingNotes,
                    s.ResidentComments
                } )
                .ToList();

            gAssessmentDetails.DataSource = personProjectAssessmentList;

            gAssessmentDetails.DataBind();
        }

        #endregion
    }
}