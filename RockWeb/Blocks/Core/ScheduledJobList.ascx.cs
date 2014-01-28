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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Scheduled Job List" )]
    [Category( "Core" )]
    [Description( "Lists all scheduled jobs." )]

    [LinkedPage("Detail Page")]
    public partial class ScheduledJobList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            gScheduledJobs.DataKeyNames = new string[] { "id" };
            gScheduledJobs.Actions.ShowAdd = true;
            gScheduledJobs.Actions.AddClick += gScheduledJobs_Add;
            gScheduledJobs.GridRebind += gScheduledJobs_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gScheduledJobs.Actions.ShowAdd = canAddEditDelete;
            gScheduledJobs.IsDeleteEnabled = canAddEditDelete;

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "serviceJobId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "serviceJobId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the grdScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                var jobService = new ServiceJobService();
                ServiceJob job = jobService.Get( (int)e.RowKeyValue );

                string errorMessage;
                if ( !jobService.CanDelete( job, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                jobService.Delete( job, CurrentPersonAlias );
                jobService.Save( job, CurrentPersonAlias );
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gScheduledJobs_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the scheduled jobs.
        /// </summary>
        private void BindGrid()
        {
            var jobService = new ServiceJobService();
            SortProperty sortProperty = gScheduledJobs.SortProperty;

            if ( sortProperty != null )
            {
                gScheduledJobs.DataSource = jobService.GetAllJobs().Sort( sortProperty ).ToList();
            }
            else
            {
                gScheduledJobs.DataSource = jobService.GetAllJobs().OrderByDescending( a => a.LastRunDateTime ).ToList();
            }
            
            gScheduledJobs.DataBind();
        }

        #endregion
    }
}