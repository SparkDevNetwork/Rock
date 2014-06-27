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
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Project List" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays a list of projects for a competency." )]

    [LinkedPage( "Detail Page" )]
    public partial class ProjectList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.DataKeyNames = new string[] { "id" };
            gList.Actions.ShowAdd = true;
            gList.Actions.AddClick += gList_Add;
            gList.GridRebind += gList_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            gList.Actions.ShowAdd = canAddEditDelete;
            gList.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                int? competencyId = this.PageParameter( "CompetencyId" ).AsInteger();
                hfCompetencyId.Value = competencyId.ToString();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_Add( object sender, EventArgs e )
        {
            gList_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            gList_ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Gs the list_ show edit.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void gList_ShowEdit( int id )
        {
            NavigateToLinkedPage( "DetailPage", "ProjectId", id, "CompetencyId", hfCompetencyId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the RowCommand event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void gList_RowCommand( object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e )
        {
            int? rowIndex = ( e.CommandArgument as string ).AsIntegerOrNull();
            if ( rowIndex != null )
            {
                GridViewRow row = ( sender as Grid ).Rows[rowIndex.Value];
                RowEventArgs rowEventArgs = new RowEventArgs( row );

                if ( e.CommandName.Equals( "clone" ) )
                {
                    Dictionary<string, string> qryParams = new Dictionary<string, string>();
                    qryParams.Add( "ProjectId", "0" );
                    qryParams.Add( "CompetencyId", hfCompetencyId.Value );
                    qryParams.Add( "CloneFromProjectId", rowEventArgs.RowKeyId.ToString() );
                    NavigateToLinkedPage( "DetailPage", qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var residencyContext = new ResidencyContext();
            var projectService = new ResidencyService<Project>( residencyContext );

            Project project = projectService.Get( e.RowKeyId );
            if ( project != null )
            {
                string errorMessage;
                if ( !projectService.CanDelete( project, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                projectService.Delete( project );
                residencyContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var projectService = new ResidencyService<Project>( new ResidencyContext() );
            int competencyId = hfCompetencyId.ValueAsInt();
            SortProperty sortProperty = gList.SortProperty;
            var qry = projectService.Queryable();
            qry = qry.Where( a => a.CompetencyId.Equals( competencyId ) );

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( s => s.Name );
            }

            gList.DataSource = qry.ToList();

            gList.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
        
    }
}