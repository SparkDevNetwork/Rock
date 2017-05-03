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
using Rock.Web.Cache;
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
        
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            InitFilter( );
            InitGrid( );
        }
        
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            if ( !Page.IsPostBack )
            {   
                BindFilter( );
                BindGrid( );
            }
        }
        
        #endregion
        
        #region Filter Methods

        void InitFilter( )
        {
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
        }

        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Campus", "Campus", cblCampus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            rFilter.SaveUserPreference( "STARS Applicant", ddlStarsApp.SelectedValue );

            BindFilter( );
            BindGrid( );
        }

        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Campus":
                {
                    var values = new List<string>();
                    foreach ( string value in e.Value.Split( ';' ) )
                    {
                        var item = cblCampus.Items.FindByValue( value );
                        if ( item != null )
                        {
                            values.Add( item.Text );
                        }
                    }
                    e.Value = values.AsDelimited( ", " );
                    break;
                }

                case "Status":
                {
                    e.Value = rFilter.GetUserPreference( "Status" );
                    break;
                }

                case "STARS Applicant":
                {
                    e.Value = rFilter.GetUserPreference( "STARS Applicant" );
                    break;
                }

                default:
                {
                    e.Value = string.Empty;
                    break;
                }
            }
        }

        private void BindFilter()
        {
            // setup the campus
            cblCampus.DataSource = CampusCache.All( false );
            cblCampus.DataBind();

            string campusValue = rFilter.GetUserPreference( "Campus" );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }

            // setup the status / state
            ddlStatus.Items.Clear( );
            ddlStatus.Items.Add( string.Empty );
            ddlStatus.Items.Add( VolunteerScreening.sState_HandedOff );
            ddlStatus.Items.Add( VolunteerScreening.sState_InReview );
            ddlStatus.Items.Add( VolunteerScreening.sState_Waiting );
            ddlStatus.SetValue( rFilter.GetUserPreference( "Status" ) );

            // setup the STARS applicants
            ddlStarsApp.Items.Clear( );
            ddlStarsApp.Items.Add( string.Empty );
            ddlStarsApp.Items.Add( "Yes" );
            ddlStarsApp.Items.Add( "No" );
            ddlStarsApp.SetValue( rFilter.GetUserPreference( "STARS Applicant" ) );
        }
        #endregion

        #region Grid Methods

        void InitGrid( )
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

        private void gGrid_Rebind( object sender, EventArgs e )
        {
            BindFilter( );
            BindGrid( );
        }

        protected void gGrid_Edit( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "VolunteerScreeningInstanceId", e.RowKeyId.ToString() );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }
        
        private void BindGrid( )
        {
            using ( RockContext rockContext = new RockContext( ) )
            {
                // get all the volunteer screening instances tied to this person
                var vsQuery = new Service<VolunteerScreening>( rockContext ).Queryable( ).AsNoTracking( );
                var paQuery = new Service<PersonAlias>( rockContext ).Queryable( ).AsNoTracking( );
                var wfQuery = new Service<Workflow>( rockContext ).Queryable( ).AsNoTracking( );
            
                var instanceQuery = vsQuery.Join( paQuery, vs => vs.PersonAliasId, pa => pa.Id, ( vs, pa ) => new { VolunteerScreening = vs, PersonName = pa.Person.FirstName + " " + pa.Person.LastName } )
                                           .Join( wfQuery, vs => vs.VolunteerScreening.Application_WorkflowId, wf => wf.Id, ( vs, wf ) => new { VolunteerScreeningWithPerson = vs, Workflow = wf } )
                                           .Select( a => new { Id = a.VolunteerScreeningWithPerson.VolunteerScreening.Id,
                                                               SentDate = a.VolunteerScreeningWithPerson.VolunteerScreening.CreatedDateTime.Value,
                                                               CompletedDate = a.VolunteerScreeningWithPerson.VolunteerScreening.ModifiedDateTime.Value,
                                                               PersonName = a.VolunteerScreeningWithPerson.PersonName,
                                                               Workflow = a.Workflow } ).ToList( );

                // load all attributes for the workflows, since we need them for the grid
                foreach( var queryObj in instanceQuery )
                {
                    queryObj.Workflow.LoadAttributes( );
                }

                // ---- Apply Filters ----
                var filteredQuery = instanceQuery;

                // Campus
                List<int> campusIds = cblCampus.SelectedValuesAsInt;
                if( campusIds.Count > 0 )
                {
                    // the workflows store the campus by name, so convert the selected Ids to names
                    List<string> selectedCampusNames = CampusCache.All( ).Where( cc => campusIds.Contains( cc.Id ) ).Select( cc => cc.Name ).ToList( );

                    filteredQuery = filteredQuery.Where( vs => ContainsCampus( selectedCampusNames, vs.Workflow ) ).ToList( );
                }

                // Status
                string statusValue = rFilter.GetUserPreference( "Status" );
                if ( string.IsNullOrWhiteSpace( statusValue ) == false )
                {
                    filteredQuery = filteredQuery.Where( vs => VolunteerScreening.GetState( vs.SentDate, vs.CompletedDate, vs.Workflow.Status ) == statusValue ).ToList( );
                }

                // STARS Applicant
                string starsApp = rFilter.GetUserPreference( "STARS Applicant" );
                if ( string.IsNullOrWhiteSpace( starsApp ) == false )
                {
                    filteredQuery = filteredQuery.Where( vs => IsStars( vs.Workflow ) == starsApp ).ToList( );
                }

                // ---- End Filters ----
            
                if ( filteredQuery.Count( ) > 0 )
                {
                    gGrid.DataSource = filteredQuery.OrderByDescending( vs => vs.SentDate ).OrderByDescending( vs => vs.CompletedDate ).Select( vs => 
                        new {
                                Name = vs.PersonName,
                                Id = vs.Id,
                                SentDate = vs.SentDate.ToShortDateString( ),
                                CompletedDate = ParseCompletedDate( vs.SentDate, vs.CompletedDate ),
                                State = VolunteerScreening.GetState( vs.SentDate, vs.CompletedDate, vs.Workflow.Status ),
                                Campus = GetCampus( vs.Workflow ),
                                IsStars = IsStars( vs.Workflow )
                            } ).ToList( );
                }

                gGrid.DataBind( );
            }
        }
        #endregion

        #region Helper Methods
        bool ContainsCampus( List<string> selectedCampusNames, Workflow  workflow )
        {
            if( workflow.AttributeValues.ContainsKey( "Campus" ) )
            {
                // check for the campus in the selected list. if somehow campus is null, we'll go ahead and display 
                // the entry
                string workflowCampus = workflow.AttributeValues [ "Campus" ].ToString( );
                if( string.IsNullOrEmpty( workflowCampus ) == false )
                {
                    return selectedCampusNames.Contains( workflowCampus );
                }
            }

            return true;
        }

        string GetCampus( Workflow workflow )
        {
            if( workflow.AttributeValues.ContainsKey( "Campus" ) )
            {
                return workflow.AttributeValues [ "Campus" ].ToString( );
            }

            return string.Empty;
        }

        string IsStars( Workflow workflow )
        {
            if( workflow.AttributeValues.ContainsKey( "ApplyingForStars" ) )
            {
                return workflow.AttributeValues [ "ApplyingForStars" ].ToString( );
            }

            return string.Empty;
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
