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
    [DisplayName( "Competency Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a competency." )]

    public partial class CompetencyDetail : RockBlock, IDetailBlock
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
                string competencyId = PageParameter( "competencyId" );
                if ( !string.IsNullOrWhiteSpace( competencyId ) )
                {
                    ShowDetail( competencyId.AsInteger(), PageParameter( "TrackId" ).AsIntegerOrNull() );
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

            int? competencyId = this.PageParameter( pageReference, "CompetencyId" ).AsInteger();
            if ( competencyId != null )
            {
                Competency competency = new ResidencyService<Competency>( new ResidencyContext() ).Get( competencyId.Value );
                if ( competency != null )
                {
                    breadCrumbs.Add( new BreadCrumb( competency.Name, pageReference ) );
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

            if ( hfCompetencyId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                // if this page was called from the Track Detail page, return to that
                string trackId = PageParameter( "TrackId" );
                if ( !string.IsNullOrWhiteSpace( trackId ) )
                {
                    Dictionary<string, string> qryString = new Dictionary<string, string>();
                    qryString["TrackId"] = trackId;
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
                ResidencyService<Competency> service = new ResidencyService<Competency>( new ResidencyContext() );
                Competency item = service.Get( hfCompetencyId.ValueAsInt() );
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
            ResidencyService<Competency> service = new ResidencyService<Competency>( new ResidencyContext() );
            Competency item = service.Get( hfCompetencyId.ValueAsInt() );
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
            Competency competency;
            var residencyContext = new ResidencyContext();
            ResidencyService<Competency> competencyService = new ResidencyService<Competency>( residencyContext );

            int competencyId = int.Parse( hfCompetencyId.Value );

            if ( competencyId == 0 )
            {
                competency = new Competency();
                competencyService.Add( competency );
            }
            else
            {
                competency = competencyService.Get( competencyId );
            }

            competency.Name = tbName.Text;
            competency.Description = tbDescription.Text;
            competency.TrackId = hfTrackId.ValueAsInt();
            competency.TeacherOfRecordPersonId = ppTeacherOfRecord.PersonId;
            competency.FacilitatorPersonId = ppFacilitator.PersonId;
            competency.Goals = tbGoals.Text;
            competency.CreditHours = tbCreditHours.Text.AsIntegerOrNull();
            competency.SupervisionHours = tbSupervisionHours.Text.AsIntegerOrNull();
            competency.ImplementationHours = tbImplementationHours.Text.AsIntegerOrNull();

            if ( !competency.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["CompetencyId"] = competency.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyId">The competency identifier.</param>
        public void ShowDetail( int competencyId )
        {
            ShowDetail( competencyId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyId">The competency identifier.</param>
        /// <param name="trackId">The residency track id.</param>
        public void ShowDetail( int competencyId, int? trackId )
        {
            pnlDetails.Visible = true;

            var residencyContext = new ResidencyContext();

            // Load depending on Add(0) or Edit
            Competency competency = null;
            if ( !competencyId.Equals( 0 ) )
            {
                competency = new ResidencyService<Competency>( residencyContext ).Get( competencyId );
            }
            
            if ( competency == null )
            {
                competency = new Competency { Id = 0 };
                competency.TrackId = trackId ?? 0;
                competency.Track = new ResidencyService<Track>( residencyContext ).Get( competency.TrackId );
            }

            hfCompetencyId.Value = competency.Id.ToString();
            hfTrackId.Value = competency.TrackId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Competency.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( competency );
            }
            else
            {
                btnEdit.Visible = true;
                if ( competency.Id > 0 )
                {
                    ShowReadonlyDetails( competency );
                }
                else
                {
                    ShowEditDetails( competency );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="competency">The residency competency.</param>
        private void ShowEditDetails( Competency competency )
        {
            if ( competency.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Competency.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = competency.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = competency.Name;
            tbDescription.Text = competency.Description;
            lblPeriod.Text = competency.Track.Period.Name;
            lblTrack.Text = competency.Track.Name;
            ppTeacherOfRecord.SetValue( competency.TeacherOfRecordPerson );
            ppFacilitator.SetValue( competency.FacilitatorPerson );
            tbGoals.Text = competency.Goals;
            tbCreditHours.Text = competency.CreditHours.ToString();
            tbSupervisionHours.Text = competency.SupervisionHours.ToString();
            tbImplementationHours.Text = competency.ImplementationHours.ToString();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="competency">The residency competency.</param>
        private void ShowReadonlyDetails( Competency competency )
        {
            lReadOnlyTitle.Text = competency.Name.FormatAsHtmlTitle();
            
            SetEditMode( false );

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Description", competency.Description )
                .Add( "Period", competency.Track.Period.Name )
                .Add( "Track", competency.Track.Name ).Html;

            lblMainDetailsCol2.Text = new DescriptionList()
                .Add( "Teacher of Record", competency.TeacherOfRecordPerson )
                .Add( "Facilitator", competency.FacilitatorPerson )
                .Add( "Credit Hours", competency.CreditHours )
                .Html;
        }

        #endregion
    }
}