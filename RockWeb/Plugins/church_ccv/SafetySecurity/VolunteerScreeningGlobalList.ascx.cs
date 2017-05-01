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
using church.ccv.SafetySecurity.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Plugins.church_ccv.SafetySecurity
{
    [DisplayName( "Volunteer Screening Global List" )]
    [Category( "CCV > Safety and Security" )]
    [Description( "Lists all volunteer screening instances newer than 2 months." )]
    
    [LinkedPage( "Detail Page" )]
    public partial class VolunteerScreeningGlobalList : RockBlock
    {                
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            InitInstancesGrid( );
        }

        void InitInstancesGrid( )
        {
            gGrid.DataKeyNames = new string[] { "Id" };
            
            gGrid.Actions.Visible = true;
            gGrid.Actions.Enabled = true;
            gGrid.Actions.ShowBulkUpdate = false;
            gGrid.Actions.ShowCommunicate = false;
            gGrid.Actions.ShowExcelExport = false;
            gGrid.Actions.ShowMergePerson = false;
            gGrid.Actions.ShowMergeTemplate = false;

            gGrid.GridRebind += gGrid_Rebind;
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
                using ( RockContext rockContext = new RockContext( ) )
                {
                    BindGrid( rockContext );
                }
            }
        }
        
        #endregion
        
        #region Grid Events (main grid)
        
        /// <summary>
        /// Handles the GridRebind event of the gPromotions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGrid_Rebind( object sender, EventArgs e )
        {
            using ( RockContext rockContext = new RockContext( ) )
            {
                BindGrid( rockContext );
            }
        }

        protected void gGrid_Edit( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "VolunteerScreeningInstanceId", e.RowKeyId.ToString() );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( RockContext rockContext )
        {
            // get all the volunteer screening instances tied to this person
            var vsQuery = new Service<VolunteerScreening>( rockContext ).Queryable( ).AsNoTracking( );
            var paQuery = new Service<PersonAlias>( rockContext ).Queryable( ).AsNoTracking( );
            var wfQuery = new Service<Workflow>( rockContext ).Queryable( ).AsNoTracking( );
            
            var instanceQuery = vsQuery.Join( paQuery, vs => vs.PersonAliasId, pa => pa.Id, ( vs, pa ) => new { VolunteerScreening = vs, PersonName = pa.Person.FirstName + " " + pa.Person.LastName } )
                                       .Join( wfQuery, vs => vs.VolunteerScreening.Application_WorkflowId, wf => wf.Id, ( vs, wf ) => new { VolunteerScreeningWithPerson = vs, WorkflowStatus = wf.Status } )
                                       .Select( a => new { Id = a.VolunteerScreeningWithPerson.VolunteerScreening.Id,
                                                           SentDate = a.VolunteerScreeningWithPerson.VolunteerScreening.CreatedDateTime.Value,
                                                           CompletedDate = a.VolunteerScreeningWithPerson.VolunteerScreening.ModifiedDateTime.Value,
                                                           PersonName = a.VolunteerScreeningWithPerson.PersonName,
                                                           WorkflowStatus = a.WorkflowStatus } ).ToList( );
            
            if ( instanceQuery.Count( ) > 0 )
            {
                gGrid.DataSource = instanceQuery.OrderByDescending( vs => vs.SentDate ).OrderByDescending( vs => vs.CompletedDate ).Select( vs => 
                    new {
                            Name = vs.PersonName,
                            Id = vs.Id,
                            SentDate = vs.SentDate.ToShortDateString( ),
                            CompletedDate = ParseCompletedDate( vs.SentDate, vs.CompletedDate ),
                            State = ParseState( vs.SentDate, vs.CompletedDate, vs.WorkflowStatus ),
                        } ).ToList( );
            }
            
            gGrid.DataBind();
        }

        string ParseCompletedDate( DateTime sentDate, DateTime completedDate )
        {
            if( sentDate == completedDate )
            {
                return "Not Completed";
            }
            else
            {
                return completedDate.ToShortDateString( );
            }
        }

        string ParseState( DateTime sentDate, DateTime completedDate, string workflowStatus )
        {
            // there are 3 overall states for the screening process:
            // Application Sent (modifiedDateTime == createdDateTime)
            // Application Completed and in Review (modifiedDateTime > createdDateTime)
            // Application Approved and now being reviewed by security (workflow == complete)
            if( workflowStatus == "Completed" )
            {
                return "Handed off to Security";
            }
            else if ( completedDate > sentDate )
            {
                return "Application in Review";
            }
            else
            {
                return "Waiting for Applicant to Complete";
            }
        }
        
        #endregion
    }
}
