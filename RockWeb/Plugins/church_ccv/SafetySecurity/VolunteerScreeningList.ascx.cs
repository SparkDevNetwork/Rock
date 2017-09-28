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
    [DisplayName( "Volunteer Screening List" )]
    [Category( "CCV > Safety and Security" )]
    [Description( "Lists volunteer screening instances for the given person." )]

    [ContextAware( typeof( Person ) )]
    [LinkedPage( "Detail Page" )]
    public partial class VolunteerScreeningList : RockBlock
    {
        protected Person TargetPerson { get; set; }
                
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // in case this is used as a Person Block, set the TargetPerson 
            TargetPerson = ContextEntity<Person>();
            InitInstancesGrid( );
        }

        void InitInstancesGrid( )
        {
            gGrid.DataKeyNames = new string[] { "Id" };

            // turn on only the 'add' button
            gGrid.Actions.Visible = true;
            gGrid.Actions.Enabled = true;
            gGrid.Actions.ShowBulkUpdate = false;
            gGrid.Actions.ShowCommunicate = false;
            gGrid.Actions.ShowExcelExport = false;
            gGrid.Actions.ShowMergePerson = false;
            gGrid.Actions.ShowMergeTemplate = false;

            //gGrid.Actions.ShowAdd = IsUserAuthorized( Authorization.EDIT );
            //gGrid.Actions.AddClick += gGrid_AddClick;

            gGrid.GridRebind += gGrid_Rebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            if ( !Page.IsPostBack && TargetPerson != null )
            {   
                using ( RockContext rockContext = new RockContext( ) )
                {
                    BindGrid( rockContext );
                }
            }
        }
        
        #endregion
        
        #region Grid Events (main grid)

        protected void gGrid_AddClick( object sender, EventArgs e )
        {
            using ( RockContext rockContext = new RockContext( ) )
            {
                VolunteerScreening instance = new VolunteerScreening( );
                instance.PersonAliasId = TargetPerson.PrimaryAliasId.Value;
                instance.CreatedDateTime = RockDateTime.Now;
                instance.ModifiedDateTime = RockDateTime.Now;
                instance.Type = (int)VolunteerScreening.Types.Legacy;

                Service<VolunteerScreening> rockService = new Service<VolunteerScreening>( rockContext );
                rockService.Add( instance );

                rockContext.SaveChanges( );
            }
        }

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
            
            var vsForPersonQuery = vsQuery.Join( paQuery, vs => vs.PersonAliasId, pa => pa.Id, ( vs, pa ) => new { VolunteerScreening = vs, PersonAlias = pa } )
                                       .Where( a => a.PersonAlias.PersonId == TargetPerson.Id )
                                       .Select( a => a.VolunteerScreening )
                                       .AsQueryable( );

            var instanceQuery = vsForPersonQuery.Join( wfQuery, vs => vs.Application_WorkflowId, wf => wf.Id, ( vs, wf ) => new { VolunteerScreening = vs, WorkflowStatus = wf.Status } ).AsQueryable( );
            
            if ( instanceQuery.Count( ) > 0 )
            {
                var instances = instanceQuery.Select( vs => new { Id = vs.VolunteerScreening.Id,
                                                                  SentDate = vs.VolunteerScreening.CreatedDateTime.Value,
                                                                  CompletedDate = vs.VolunteerScreening.ModifiedDateTime.Value,
                                                                  vs.WorkflowStatus } ).ToList( );

                gGrid.DataSource = instances.OrderByDescending( vs => vs.SentDate ).OrderByDescending( vs => vs.CompletedDate ).Select( vs => 
                    new {
                            Id = vs.Id,
                            SentDate = vs.SentDate.ToShortDateString( ),
                            CompletedDate = ParseCompletedDate( vs.SentDate, vs.CompletedDate ),
                            State = VolunteerScreening.GetState( vs.SentDate, vs.CompletedDate, vs.WorkflowStatus ),
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
        #endregion
    }
}
