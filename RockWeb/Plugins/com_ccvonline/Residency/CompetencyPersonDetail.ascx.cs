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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Resident Competency Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Simple detail form showing a resident's assignment to a specific competency." )]
    
    public partial class CompetencyPersonDetail : RockBlock, IDetailBlock
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
                    ShowDetail( competencyPersonId.AsInteger(), PageParameter( "PersonId" ).AsIntegerOrNull() );
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

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );

            if ( hfCompetencyPersonId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                // if this page was called from the ResidencyPerson Detail page, return to that
                string personId = PageParameter( "PersonId" );
                if ( !string.IsNullOrWhiteSpace( personId ) )
                {
                    Dictionary<string, string> qryString = new Dictionary<string, string>();
                    qryString["PersonId"] = personId;
                    NavigateToParentPage( qryString );
                }
                else
                {
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ResidencyService<CompetencyPerson> service = new ResidencyService<CompetencyPerson>( new ResidencyContext() );
                CompetencyPerson item = service.Get( hfCompetencyPersonId.ValueAsInt() );
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
            ResidencyService<CompetencyPerson> service = new ResidencyService<CompetencyPerson>( new ResidencyContext() );
            CompetencyPerson item = service.Get( hfCompetencyPersonId.ValueAsInt() );
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
            ResidencyService<CompetencyPerson> competencyPersonService = new ResidencyService<CompetencyPerson>( residencyContext );
            ResidencyService<Competency> competencyService = new ResidencyService<Competency>( residencyContext );
            ResidencyService<CompetencyPersonProject> competencyPersonProjectService = new ResidencyService<CompetencyPersonProject>( residencyContext );

            int competencyPersonId = int.Parse( hfCompetencyPersonId.Value );
            int trackId = ddlTrack.SelectedValueAsInt() ?? 0;
            int personId = hfPersonId.ValueAsInt();

            if ( competencyPersonId == 0 )
            {
                int selectedId = ddlCompetency.SelectedValueAsInt() ?? 0;
                List<int> competencyToAssignIdList = null;

                if ( selectedId == Rock.Constants.All.Id )
                {
                    // add all the Competencies for this Track that they don't have yet
                    var competencyQry = new ResidencyService<Competency>( residencyContext ).Queryable().Where( a => a.TrackId == trackId );

                    // list 
                    List<int> assignedCompetencyIds = new ResidencyService<CompetencyPerson>( residencyContext ).Queryable().Where( a => a.PersonId.Equals( personId ) ).Select( a => a.CompetencyId ).ToList();

                    competencyToAssignIdList = competencyQry.Where( a => !assignedCompetencyIds.Contains( a.Id ) ).OrderBy( a => a.Name ).Select( a => a.Id ).ToList();
                }
                else
                {
                    // just add the selected Competency
                    competencyToAssignIdList = new List<int>();
                    competencyToAssignIdList.Add( selectedId );
                }


                residencyContext.WrapTransaction( () =>
                {
                    foreach ( var competencyId in competencyToAssignIdList )
                    {
                        CompetencyPerson competencyPerson = new CompetencyPerson();
                        competencyPersonService.Add( competencyPerson );
                        competencyPerson.PersonId = hfPersonId.ValueAsInt();
                        competencyPerson.CompetencyId = competencyId;

                        // save changes here first to make sure we can a valid competencyPerson.id
                        residencyContext.SaveChanges();

                        Competency competency = competencyService.Get( competencyId );
                        foreach ( var project in competency.Projects )
                        {
                            // add all the projects associated with the competency
                            CompetencyPersonProject competencyPersonProject = new CompetencyPersonProject
                                {
                                    CompetencyPersonId = competencyPerson.Id,
                                    ProjectId = project.Id,
                                    MinAssessmentCount = null
                                };

                            competencyPersonProjectService.Add( competencyPersonProject );
                        }
                    }

                    residencyContext.SaveChanges();
                } );
            }
            else
            {
                // shouldn't happen, they can only Add
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["PersonId"] = personId.ToString();
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonId">The competency person identifier.</param>
        public void ShowDetail( int competencyPersonId )
        {
            ShowDetail( competencyPersonId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonId">The competency person identifier.</param>
        /// <param name="personId">The person id.</param>
        public void ShowDetail( int competencyPersonId, int? personId )
        {
            pnlDetails.Visible = true;

            var residencyContext = new ResidencyContext();

            // Load depending on Add(0) or Edit
            CompetencyPerson competencyPerson = null;
            if ( !competencyPersonId.Equals( 0 ) )
            {
                competencyPerson = new ResidencyService<CompetencyPerson>( residencyContext ).Get( competencyPersonId );
            }
            
            if ( competencyPerson == null )
            {
                competencyPerson = new CompetencyPerson { Id = 0 };
                competencyPerson.PersonId = personId ?? 0;
                competencyPerson.Person = new ResidencyService<Person>( residencyContext ).Get( competencyPerson.PersonId );
            }

            hfCompetencyPersonId.Value = competencyPerson.Id.ToString();
            hfPersonId.Value = competencyPerson.PersonId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( CompetencyPerson.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( competencyPerson );
            }
            else
            {
                // don't allow edit once a Competency has been assign
                btnEdit.Visible = false;
                if ( competencyPerson.Id > 0 )
                {
                    ShowReadonlyDetails( competencyPerson );
                }
                else
                {
                    ShowEditDetails( competencyPerson );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPeriodTrack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPeriod_SelectedIndexChanged( object sender, EventArgs e )
        {
            int periodId = ddlPeriod.SelectedValueAsInt() ?? 0;
            var trackQry = new ResidencyService<Track>( new ResidencyContext() ).Queryable().Where( a => a.PeriodId.Equals( periodId ) );

            ddlTrack.DataSource = trackQry.OrderBy( a => a.DisplayOrder ).ThenBy( a => a.Name ).ToList();
            ddlTrack.DataBind();
            ddlTrack_SelectedIndexChanged( null, null );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTrack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTrack_SelectedIndexChanged( object sender, EventArgs e )
        {
            var residencyContext = new ResidencyContext();
            int trackId = ddlTrack.SelectedValueAsInt() ?? 0;
            var competencyQry = new ResidencyService<Competency>( residencyContext ).Queryable().Where( a => a.TrackId == trackId );

            // list 
            int personId = hfPersonId.ValueAsInt();
            List<int> assignedCompetencyIds = new ResidencyService<CompetencyPerson>( residencyContext ).Queryable().Where( a => a.PersonId.Equals( personId ) ).Select( a => a.CompetencyId ).ToList();

            var competencyNotYetAssignedList = competencyQry.Where( a => !assignedCompetencyIds.Contains( a.Id ) ).OrderBy( a => a.Name ).ToList();

            ddlCompetency.Visible = competencyNotYetAssignedList.Any();
            nbAllCompetenciesAlreadyAdded.Visible = !competencyNotYetAssignedList.Any();
            btnSave.Visible = competencyNotYetAssignedList.Any();

            competencyNotYetAssignedList.Insert( 0, new Competency { Id = Rock.Constants.All.Id, Name = Rock.Constants.All.Text } );

            ddlCompetency.DataSource = competencyNotYetAssignedList;
            ddlCompetency.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var periodList = new ResidencyService<Period>( new ResidencyContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            var today = RockDateTime.Today;
            
            foreach ( var period in periodList )
            {
                ddlPeriod.Items.Add( new ListItem( period.Name, period.Id.ToString() ) );
            }

            var currentOrNextPeriod = periodList.Where( p => p.EndDate > today ).OrderBy( o => o.StartDate ).FirstOrDefault();
            if ( currentOrNextPeriod != null )
            {
                ddlPeriod.SetValue( ( currentOrNextPeriod.Id ) );
            }
            
            ddlPeriod_SelectedIndexChanged( null, null );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="competencyPerson">The competency person.</param>
        private void ShowEditDetails( CompetencyPerson competencyPerson )
        {
            lReadOnlyTitle.Text = competencyPerson.Person.ToString().FormatAsHtmlTitle();
            
            SetEditMode( true );

            LoadDropDowns();

            ddlCompetency.SetValue( competencyPerson.CompetencyId );

            if ( competencyPerson.Competency != null )
            {
                lblPeriod.Text = competencyPerson.Competency.Track.Period.Name;
                lblTrack.Text = competencyPerson.Competency.Track.Name;
                lblCompetency.Text = competencyPerson.Competency.Name;
            }
            else
            {
                // shouldn't happen, but just in case
                lblCompetency.Text = Rock.Constants.None.Text;
            }

            // only allow a Competency to be assigned when in Add mode
            pnlCompetencyLabels.Visible = competencyPerson.Id != 0;
            pnlCompetencyDropDownLists.Visible = competencyPerson.Id == 0;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="competencyPerson">The competency person.</param>
        private void ShowReadonlyDetails( CompetencyPerson competencyPerson )
        {
            lReadOnlyTitle.Text = competencyPerson.Person.ToString().FormatAsHtmlTitle();
            
            SetEditMode( false );

            lblMainDetails.Text = new DescriptionList()
                .Add( "Competency", competencyPerson.Competency.Name )
                .Html;
        }

        #endregion
    }
}