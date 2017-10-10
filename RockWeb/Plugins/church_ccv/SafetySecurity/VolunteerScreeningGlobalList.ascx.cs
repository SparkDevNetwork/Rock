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
        // the actual types of Volunteer Screening Applications aren't
        // defined anywhere. However, in order to filter on them, we need thm listed.
        // we put them here since this is the only area in all of Volunteer Screening that cares about them.
        const string sApplicationType_Standard = "Standard";
        const string sApplicationType_KidsStudents = "Kids & Students";
        const string sApplicationType_SafetySecurity = "Safety & Security";
        const string sApplicationType_STARS = "STARS";
        const string sApplicationType_Renewal = "Renewal";

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
            rFilter.SaveUserPreference( "Application Type", ddlApplicationType.SelectedValue );
            rFilter.SaveUserPreference( "Applicant Name", tbApplicantName.Text );

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

                case "Application Type":
                {
                    e.Value = rFilter.GetUserPreference( "Application Type" );
                    break;
                }

                case "Applicant Name":
                {
                    e.Value = rFilter.GetUserPreference( "Applicant Name" );
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
            ddlStatus.Items.Add( VolunteerScreening.sState_Waiting );
            ddlStatus.Items.Add( VolunteerScreening.sState_InReviewWithCampus );
            ddlStatus.Items.Add( VolunteerScreening.sState_InReviewWithSecurity );
            ddlStatus.Items.Add( VolunteerScreening.sState_Accepted );

            // JHM 7-10-17
            // HORRIBLE HACK - If the application was sent before we ended testing, we need to support old states and attributes.
            // We need to do this because we have 100+ applications that were sent out (and not yet completed) during our testing phase. I was hoping for like, 10.
            // We can get rid of this when all workflows of type 202 are marked as 'completed'
            ddlStatus.Items.Add( VolunteerScreening.sState_HandedOff_TestVersion );
            ddlStatus.Items.Add( VolunteerScreening.sState_InReview_TestVersion );

            ddlStatus.SetValue( rFilter.GetUserPreference( "Status" ) );

            // setup the application types
            ddlApplicationType.Items.Clear( );
            ddlApplicationType.Items.Add( string.Empty );
            ddlApplicationType.Items.Add( sApplicationType_Standard );
            ddlApplicationType.Items.Add( sApplicationType_KidsStudents );
            ddlApplicationType.Items.Add( sApplicationType_SafetySecurity );
            ddlApplicationType.Items.Add( sApplicationType_STARS );
            ddlApplicationType.Items.Add( sApplicationType_Renewal );
            ddlApplicationType.SetValue( rFilter.GetUserPreference( "Application Type" ) );

            // setup the Applicant Name
            tbApplicantName.Text = rFilter.GetUserPreference( "Applicant Name" );
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
                List<CampusCache> campusCache = CampusCache.All( );
                PersonAliasService paService = new PersonAliasService( rockContext );

                // get all volunteer screening instances. This is complicated, so I'll explain:

                // Each instance is stored in the VolunteerScreening table, with Ids (pointers) to a person and workflow.
                // Additionally, the workflows store the Campus Attribute, which is important for organizing these per-campus.
                // So, we have to join FIVE tables to get everything.

                // First, we simply join the 3 "core" tables--volunteer screening, personAlias, and workflow.
                var vsQuery = new Service<VolunteerScreening>( rockContext ).Queryable( ).AsNoTracking( );
                var paQuery = new Service<PersonAlias>( rockContext ).Queryable( ).AsNoTracking( );
                var wfQuery = new Service<Workflow>( rockContext ).Queryable( ).AsNoTracking( );
                var coreQuery = vsQuery.Join( paQuery, vs => vs.PersonAliasId, pa => pa.Id, ( vs, pa ) => new { VolunteerScreening = vs, PersonName = pa.Person.FirstName + " " + pa.Person.LastName } )
                                       .Join( wfQuery, vs => vs.VolunteerScreening.Application_WorkflowId, wf => wf.Id, ( vs, wf ) => new { VolunteerScreeningWithPerson = vs, Workflow = wf } );


                // Now, since the Campus Attribute Value depends on the Attribute table, we need to join those two tables, and then join that to the query we built above.
                var attribQuery = new AttributeService( rockContext ).Queryable( ).AsNoTracking( );
                var avQuery = new AttributeValueService( rockContext ).Queryable( ).AsNoTracking( );
                var attribWithValue = attribQuery.Join( avQuery, a => a.Id, av => av.AttributeId, ( a, av ) => new { Attribute = a, AttribValue = av } )
                                                 .Where( a => a.Attribute.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) );

                // join the attributeValues so we can add in the campus
                var instanceQuery = coreQuery.Join( attribWithValue, vs => vs.Workflow.Id, av => av.AttribValue.EntityId, ( vs, av ) => new { VS = vs, AV = av } )
                                             .Where( a => a.AV.Attribute.Key == "Campus" )
                                             .Select( a => new { Id = a.VS.VolunteerScreeningWithPerson.VolunteerScreening.Id,
                                                                 SentDate = a.VS.VolunteerScreeningWithPerson.VolunteerScreening.CreatedDateTime.Value,
                                                                 CompletedDate = a.VS.VolunteerScreeningWithPerson.VolunteerScreening.ModifiedDateTime.Value,
                                                                 PersonName = a.VS.VolunteerScreeningWithPerson.PersonName,
                                                                 Workflow = a.VS.Workflow,
                                                                 CampusGuid = a.AV.AttribValue.Value } )
                                             .ToList( );

                // In the end, we've joined the following tables:
                // VolunteerScreening, Workflow, PersonAlias, Attribute, AttributeValue
                // and we're selecting the properties needed to filter and display each Application.
                // We now have an object with: 
                // The Volunteer Screening Id (Taken from the VS table)
                // Its SentDate, CompletedDate (Taken from the WF table)
                // Its Person (Taken from the PersonAlias table)
                // Its Campus (Taken from the AttributeValue table)

                // get a list of all "MinistryLead" attribute values, which we'll match up with each Application when binding the rows
                var ministryLeadResult = attribWithValue.Where( a => a.Attribute.Key == "MinistryLead" )
                                                        .Select( a => new MinistryLeadResult {  EntityId = a.AttribValue.EntityId,
                                                                                                MinistryLead = a.AttribValue.Value }  )
                                                        .ToList( );

                // JHM 7-10-17
                // HORRIBLE HACK - If the application was sent before we ended testing, we need to support old states and attributes.
                // We need to do this because we have 100+ applications that were sent out (and not yet completed) during our testing phase. I was hoping for like, 10.
                // We can get rid of this when all workflows of type 202 are marked as 'completed'
                var starsQueryResult = attribWithValue.Where( a => a.Attribute.Key == "ApplyingForStars" )
                                                      .Select( a => new ApplyingForStars{  EntityId = a.AttribValue.EntityId,
                                                                                           Applying = a.AttribValue.Value } )
                                                      .ToList( );

                // ---- Apply Filters ----
                var filteredQuery = instanceQuery;

                // Campus
                List<int> campusIds = cblCampus.SelectedValuesAsInt;
                if( campusIds.Count > 0 )
                {
                    // the workflows store the campus by guid, so convert the selected Ids to guids
                    List<Guid> selectedCampusNames = campusCache.Where( cc => campusIds.Contains( cc.Id ) ).Select( cc => cc.Guid ).ToList( );

                    filteredQuery = filteredQuery.Where( vs => selectedCampusNames.Contains( vs.CampusGuid.AsGuid( ) ) ).ToList( );
                }

                // Status
                string statusValue = rFilter.GetUserPreference( "Status" );
                if ( string.IsNullOrWhiteSpace( statusValue ) == false )
                {
                    filteredQuery = filteredQuery.Where( vs => VolunteerScreening.GetState( vs.SentDate, vs.CompletedDate, vs.Workflow.Status ) == statusValue ).ToList( );
                }

                // Application Type
                string appType = rFilter.GetUserPreference( "Application Type" );
                if ( string.IsNullOrWhiteSpace( appType ) == false )
                {
                    filteredQuery = filteredQuery.Where( vs => ParseApplicationType( vs.Workflow, starsQueryResult ) == appType ).ToList( );
                }

                // Name
                string personName = rFilter.GetUserPreference( "Applicant Name" );
                if ( string.IsNullOrWhiteSpace( personName ) == false )
                {
                    filteredQuery = filteredQuery.Where( vs => vs.PersonName.ToLower( ).Contains( personName.ToLower( ).Trim( ) ) ).ToList( );
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
                                Campus = campusCache.Where( c => c.Guid == vs.CampusGuid.AsGuid( ) ).SingleOrDefault( ).Name,
                                MinistryLeader = TryGetMinistryLead( vs.Workflow, ministryLeadResult, paService ),
                                ApplicationType = ParseApplicationType( vs.Workflow, starsQueryResult )
                            } ).ToList( );
                }

                gGrid.DataBind( );
            }
        }
        #endregion

        #region Helper Methods
        // JHM 7-10-17
        // HORRIBLE HACK - If the application was sent before we ended testing, we need to support old states and attributes.
        // We need to do this because we have 100+ applications that were sent out (and not yet completed) during our testing phase. I was hoping for like, 10.
        // We can get rid of this when all workflows of type 202 are marked as 'completed'
        public class ApplyingForStars
        {
            public int? EntityId { get; set; }
            public string Applying { get; set; }
        }

        string ParseApplicationType( Workflow workflow, List<ApplyingForStars> starsQueryResult )
        {
            // JHM 7-10-17
            // HORRIBLE HACK - If the application was sent before we ended testing, we need to support old states and attributes.
            // We need to do this because we have 100+ applications that were sent out (and not yet completed) during our testing phase. I was hoping for like, 10.
            // We can get rid of this when all workflows of type 202 are marked as 'completed'
            ApplyingForStars applyingForStars = starsQueryResult.Where( av => av.EntityId == workflow.Id ).SingleOrDefault( );
            if ( applyingForStars != null )
            {
                return applyingForStars.Applying == "True" ? sApplicationType_STARS : sApplicationType_Standard;
            }
            else
            {
                // given the name of the workflow (which is always in the format 'FirstName LastName Application (Specific Type)' we'll return
                // either what's in parenthesis, or if nothing's there, "Standard" to convey it wasn't for a specific area.
                int appTypeStartIndex = workflow.Name.LastIndexOf( '(' );
                if ( appTypeStartIndex > -1 )
                {
                    // there was an ending "()", so take just that part of the workflow name
                    string applicationType = workflow.Name.Substring( appTypeStartIndex );

                    // take the character after the first, up to just before the closing ')', which removes the ()s
                    return applicationType.Substring( 1, applicationType.Length - 2 );
                }
                else
                {
                    return sApplicationType_Standard;
                }
            }
        }

        public class MinistryLeadResult
        {
            public int? EntityId { get; set; }
            public string MinistryLead { get; set; }
        }

        string TryGetMinistryLead( Workflow workflow, List<MinistryLeadResult> ministryLeadQueryResult, PersonAliasService paService )
        {
            // it's possible that no ministry lead has been assigned yet. In that case, we'll return an empty string
            MinistryLeadResult ministryLead = ministryLeadQueryResult.Where( ml => ml.EntityId == workflow.Id ).SingleOrDefault( );
            if ( ministryLead != null )
            {
                // make sure the person exists
                Person person = paService.Get( ministryLead.MinistryLead.AsGuid( ) ).Person;
                if ( person != null )
                {
                    return person.FullName;
                }
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
