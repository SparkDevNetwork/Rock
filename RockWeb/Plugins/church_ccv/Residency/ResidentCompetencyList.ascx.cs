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
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Competency List" )]
    [Category( "CCV > Residency" )]
    [Description( "Lists all of a resident's competencies." )]

    [LinkedPage( "Detail Page" )]
    public partial class ResidentCompetencyList : RockBlock, ISecondaryBlock
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
                BindRepeater();
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the RowSelected event of the gCompetencyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gCompetencyList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CompetencyPersonId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gCompetencyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gCompetencyList_GridRebind( object sender, EventArgs e )
        {
            // shouldn't happen
            throw new NotImplementedException();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the repeater.
        /// </summary>
        private void BindRepeater()
        {
            var residencyContext = new ResidencyContext();
            var trackService = new ResidencyService<Track>( residencyContext );
            var competencyPersonService = new ResidencyService<CompetencyPerson>( residencyContext );
            int currentPersonId = this.CurrentPersonId ?? 0;

            List<int> residentCompetencyIds = competencyPersonService.Queryable().Where( a => a.PersonId.Equals( currentPersonId ) )
                .Select( x => x.CompetencyId ).Distinct().ToList();

            var qryPersonTracks = trackService.Queryable().Where( a => residentCompetencyIds.Any( rc => a.Competencies.Select( c => c.Id ).Contains( rc ) ) )
                .OrderBy( o => o.DisplayOrder );

            rpTracks.DataSource = qryPersonTracks.ToList();
            rpTracks.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rpTracks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rpTracks_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            Track track = e.Item.DataItem as Track;
            if ( track != null )
            {
                int trackId = track.Id;
                int currentPersonId = this.CurrentPersonId ?? 0;
                Grid gCompetencyList = e.Item.FindControl( "gCompetencyList" ) as Grid;
                var competencyPersonService = new ResidencyService<CompetencyPerson>( new ResidencyContext() );

                var qryPersonCompetencies = competencyPersonService.Queryable()
                    .Where( a => a.PersonId.Equals( currentPersonId ) && a.Competency.TrackId.Equals( trackId ) )
                    .Select( a => new
                    {
                        Id = a.Id,
                        CompetencyName = a.Competency.Name,
                        CompletedProjectAssessmentsTotal = a.CompetencyPersonProjects.Select( p => p.CompetencyPersonProjectAssessments ).SelectMany( x => x ).Where( n => n.AssessmentDateTime != null ).Count(),
                        MinProjectAssessmentsTotal = a.CompetencyPersonProjects.Select( p => p.MinAssessmentCount ?? p.Project.MinAssessmentCountDefault ?? 0 ).DefaultIfEmpty().Sum()
                    } )
                    .OrderBy( o => o.CompetencyName );

                gCompetencyList.DataKeyNames = new string[] { "Id" };
                gCompetencyList.DisplayType = GridDisplayType.Light;
                gCompetencyList.Actions.ShowAdd = false;
                gCompetencyList.RowSelected += gCompetencyList_RowSelected;
                gCompetencyList.GridRebind += gCompetencyList_GridRebind;
                gCompetencyList.DataSource = qryPersonCompetencies.ToList();
                gCompetencyList.DataBind();
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            foreach ( var item in rpTracks.Items.OfType<RepeaterItem>() )
            {
                foreach ( var grid in item.Controls.OfType<Grid>() )
                {
                    grid.Visible = visible;
                }

                foreach ( var label in item.Controls.OfType<Label>() )
                {
                    label.Visible = visible;
                }
            }
        }

        #endregion
    }
}