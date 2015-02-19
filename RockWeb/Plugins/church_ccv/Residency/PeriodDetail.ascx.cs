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
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Period Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a residency period.  For example: Fall 2013/Spring 2014" )]

    public partial class PeriodDetail : RockBlock, IDetailBlock
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
                string periodId = PageParameter( "PeriodId" );
                if ( !string.IsNullOrWhiteSpace( periodId ) )
                {
                    ShowDetail( periodId.AsInteger() );
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
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? periodId = this.PageParameter( pageReference, "PeriodId" ).AsInteger();
            if ( periodId != null )
            {
                Period period = new ResidencyService<Period>( new ResidencyContext() ).Get( periodId.Value );
                if ( period != null )
                {
                    breadCrumbs.Add( new BreadCrumb( period.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Period", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );

            if ( hfPeriodId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ResidencyService<Period> service = new ResidencyService<Period>( new ResidencyContext() );
                Period item = service.Get( hfPeriodId.ValueAsInt() );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ResidencyService<Period> service = new ResidencyService<Period>( new ResidencyContext() );
            Period item = service.Get( hfPeriodId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var residencyContext = new ResidencyContext();
            ResidencyService<Period> periodService = new ResidencyService<Period>( residencyContext );
            Period period;
            int periodId = int.Parse( hfPeriodId.Value );

            if ( periodId == 0 )
            {
                period = new Period();
                periodService.Add( period );
            }
            else
            {
                period = periodService.Get( periodId );
            }

            period.Name = tbName.Text;
            period.Description = tbDescription.Text;
            period.StartDate = dpStartEndDate.LowerValue;
            period.EndDate = dpStartEndDate.UpperValue;

            if ( !period.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges( true );

            if ( hfCloneFromPeriodId.ValueAsInt() > 0 )
            {
                var clonePeriod = periodService.Get( hfCloneFromPeriodId.ValueAsInt() );
                foreach ( var track in clonePeriod.Tracks )
                {
                    // create fresh context per track so that changetracking doesn't slow us down
                    residencyContext = new ResidencyContext();
                    ResidencyService<Track> trackService = new ResidencyService<Track>( residencyContext );
                    ResidencyService<Competency> competencyService = new ResidencyService<Competency>( residencyContext );
                    ResidencyService<Project> projectService = new ResidencyService<Project>( residencyContext );
                    ResidencyService<ProjectPointOfAssessment> projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( residencyContext );
                    var newTrack = new Track
                        {
                            PeriodId = period.Id,
                            Name = track.Name,
                            Description = track.Description,
                            DisplayOrder = track.DisplayOrder
                        };

                    trackService.Add( newTrack );
                    residencyContext.SaveChanges( true );

                    foreach ( var competency in track.Competencies )
                    {
                        var newCompetency = new Competency
                        {
                            TrackId = newTrack.Id,
                            Goals = competency.Goals,
                            CreditHours = competency.CreditHours,
                            SupervisionHours = competency.SupervisionHours,
                            ImplementationHours = competency.ImplementationHours,
                            Name = competency.Name,
                            Description = competency.Description
                        };

                        competencyService.Add( newCompetency );
                        residencyContext.SaveChanges( true );

                        foreach ( var project in competency.Projects )
                        {
                            var newProject = new Project
                            {
                                CompetencyId = newCompetency.Id,
                                MinAssessmentCountDefault = project.MinAssessmentCountDefault,
                                Name = project.Name,
                                Description = project.Description
                            };

                            projectService.Add( newProject );
                            residencyContext.SaveChanges( true );

                            List<ProjectPointOfAssessment> projectPointOfAssessmentList = new List<ProjectPointOfAssessment>();

                            foreach ( var projectPointOfAssessment in project.ProjectPointOfAssessments )
                            {
                                var newProjectPointOfAssessment = new ProjectPointOfAssessment
                                {
                                    ProjectId = newProject.Id,
                                    PointOfAssessmentTypeValueId = projectPointOfAssessment.PointOfAssessmentTypeValueId,
                                    AssessmentOrder = projectPointOfAssessment.AssessmentOrder,
                                    AssessmentText = projectPointOfAssessment.AssessmentText,
                                    IsPassFail = projectPointOfAssessment.IsPassFail
                                };

                                projectPointOfAssessmentList.Add( newProjectPointOfAssessment );
                            }

                            projectPointOfAssessmentService.AddRange( projectPointOfAssessmentList );

                            residencyContext.SaveChanges( true );
                        }
                    }
                }
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["PeriodId"] = period.Id.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( int periodId )
        {
            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            Period period = null;
            if ( !periodId.Equals( 0 ) )
            {
                period = new ResidencyService<Period>( new ResidencyContext() ).Get( periodId );
            }

            if ( period == null )
            {
                period = new Period { Id = 0 };
            }

            hfPeriodId.Value = period.Id.ToString();
            hfCloneFromPeriodId.Value = ( PageParameter( "CloneFromPeriodId" ).AsInteger() ).ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Period.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( period );
            }
            else
            {
                btnEdit.Visible = true;
                if ( period.Id > 0 )
                {
                    ShowReadonlyDetails( period );
                }
                else
                {
                    ShowEditDetails( period );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="period">The residency period.</param>
        private void ShowEditDetails( Period period )
        {
            if ( period.Id == 0 )
            {
                if ( hfCloneFromPeriodId.ValueAsInt() > 0 )
                {
                    var clonePeriod = new ResidencyService<Period>( new ResidencyContext() ).Get( hfCloneFromPeriodId.ValueAsInt() );
                    string title = string.Format( "Clone period from {0}", clonePeriod.Name );
                    nbCloneMessage.Text = string.Format( "This will add a new period, and copy all the tracks, competencies and projects from {0}", clonePeriod.Name );
                    lReadOnlyTitle.Text = title.FormatAsHtmlTitle();
                }
                else
                {
                    lReadOnlyTitle.Text = ActionTitle.Add( Period.FriendlyTypeName ).FormatAsHtmlTitle();
                }

            }
            else
            {
                lReadOnlyTitle.Text = period.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = period.Name;
            tbDescription.Text = period.Description;
            dpStartEndDate.LowerValue = period.StartDate;
            dpStartEndDate.UpperValue = period.EndDate;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="period">The residency project.</param>
        private void ShowReadonlyDetails( Period period )
        {
            lReadOnlyTitle.Text = period.Name.FormatAsHtmlTitle();

            SetEditMode( false );

            lblDescription.Text = new DescriptionList()
                .Add( "Description", period.Description ).Html;

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Date Range", string.Format( "{0} to {1}", ( period.StartDate ?? DateTime.MinValue ).ToShortDateString(), ( period.EndDate ?? DateTime.MaxValue ).ToShortDateString() ) )
                .Html;
        }

        #endregion
    }
}