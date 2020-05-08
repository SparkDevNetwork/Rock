// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemadev.PastoralCare
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [ContextAware( typeof( Person ) )]
    [DisplayName( "Homebound List" )]
    [Category( "com_bemaservices > Pastoral Care" )]
    [Description( "A summary of all the current homebound residents that have been reported to Pastoral Care." )]
    [WorkflowTypeField( "Homebound Person Workflow" )]
    public partial class HomeboundList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "HomeboundPersonWorkflow" ) ) )
            {
                ShowMessage( "Block not configured. Please configure to use.", "Configuration Error", "panel panel-danger" );
                return;
            }

            gReport.GridRebind += gReport_GridRebind;

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
            gReport.Actions.ShowAdd = true;
            gReport.Actions.AddButton.Text = "<i class=\"fa fa-plus\" Title=\"Add Homebound Resident\"></i>";
            gReport.Actions.AddButton.Enabled = true;
            gReport.Actions.AddClick += addHomeboundResident_Click;
            gReport.Actions.ShowMergeTemplate = false;
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
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
            using ( var rockContext = new RockContext() )
            {

                var contextEntity = this.ContextEntity();

                var workflowService = new WorkflowService( rockContext );
                var attributeService = new AttributeService( rockContext );
                var attributeValueService = new AttributeValueService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var entityTypeService = new EntityTypeService( rockContext );

                Guid homeBoundPersonWorkflow = GetAttributeValue( "HomeboundPersonWorkflow" ).AsGuid();

                int entityTypeId = entityTypeService.Queryable().Where( et => et.Name == typeof( Workflow ).FullName ).FirstOrDefault().Id;
                string status = ( contextEntity != null ? "Completed" : "Active" );

                var workflowTypeIdAsString = new WorkflowTypeService( rockContext ).Get( homeBoundPersonWorkflow ).Id.ToString();

                var attributeIds = attributeService.Queryable()
                    .Where( a => a.EntityTypeQualifierColumn == "WorkflowTypeId" && a.EntityTypeQualifierValue == workflowTypeIdAsString )
                    .Select( a => a.Id ).ToList();

                var wfTmpqry = workflowService.Queryable().AsNoTracking()
                     .Where( w => ( w.WorkflowType.Guid == homeBoundPersonWorkflow ) && ( w.Status == "Active" || w.Status == status ) );

                if ( contextEntity != null )
                {
                    var personGuid = ( ( Person ) contextEntity ).Aliases.Select( a => a.Guid.ToString() ).ToList();
                    var validWorkflowIds = new AttributeValueService( rockContext ).Queryable()
                        .Where( av => av.Attribute.Key == "HomeboundPerson" && personGuid.Contains( av.Value ) ).Select( av => av.EntityId );
                    wfTmpqry = wfTmpqry.Where( w => validWorkflowIds.Contains( w.Id ) );
                    gReport.Columns[10].Visible = true;
                }

                var qry = wfTmpqry.Join( attributeValueService.Queryable(),
                    obj => obj.Id,
                    av => av.EntityId.Value,
                    ( obj, av ) => new { Workflow = obj, AttributeValue = av } )
                    .Where( a => attributeIds.Contains( a.AttributeValue.AttributeId ) )
                    .GroupBy( obj => obj.Workflow )
                    .Select( obj => new { Workflow = obj.Key, AttributeValues = obj.Select( a => a.AttributeValue ) } )
                    .ToList();

                if (contextEntity == null)
                {
                    // Make sure they aren't deceased
                    qry = qry.AsQueryable().Where( w => !
                        ( personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "HomeboundPerson" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ) != null ?
                        personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "HomeboundPerson" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Person.IsDeceased :
                        false ) ).ToList();
                }

                qry.ForEach(
                 w =>
                 {
                     w.Workflow.Activities.ToList().ForEach( a => { a.LoadAttributes(); } );
                 } );

                var newQry = qry.Select( w => new
                {
                    Id = w.Workflow.Id,
                    Workflow = w.Workflow,
                    Name = w.Workflow.Name,
                    Address = new Func<string>( () =>
                    {
                        PersonAlias p = personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "HomeboundPerson" ).Select( av => av.Value ).FirstOrDefault().AsGuid() );
                        Location homeLocation = p.Person.GetHomeLocation();
                        if ( homeLocation == null )
                        {
                            return "";
                        }
                        return homeLocation.Street1 +
                            homeLocation.City + " " +
                            homeLocation.State + ", " +
                            homeLocation.PostalCode;
                    } )(),
                    HomeboundPerson = new Func<Person>( () =>
                    {
                        return personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "HomeboundPerson" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Person;
                    } )(),
                    Age = personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "HomeboundPerson" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Person.Age,
                    StartDate = w.AttributeValues.Where( av => av.AttributeKey == "StartDate" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    Description = w.AttributeValues.Where( av => av.AttributeKey == "HomeboundResidentDescription" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    Visits = w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).Count(),
                    LastVisitor = ( w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).Any() ) ? w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).LastOrDefault().AttributeValues["Visitor"].ValueFormatted : "N/A",
                    LastVisitDate = ( w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).Any() ) ? w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).LastOrDefault().AttributeValues["VisitDate"].ValueFormatted : "N/A",
                    LastVisitNotes = ( w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).Any() ) ? w.Workflow.Activities.Where( a => a.ActivityType.Name == "Visitation Info" ).LastOrDefault().AttributeValues["VisitNote"].ValueFormatted : "N/A",
                    EndDate = w.AttributeValues.Where( av => av.AttributeKey == "EndDate" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    Status = w.Workflow.Status,
                    Communion = w.AttributeValues.Where( av => av.AttributeKey == "Communion" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    Actions = ""
                } ).OrderBy( w => w.Name ).ToList().AsQueryable();

                SortProperty sortProperty = gReport.SortProperty;
                if ( sortProperty != null )
                {
                    gReport.SetLinqDataSource( newQry.Sort( sortProperty ) );
                }
                else
                {
                    gReport.SetLinqDataSource( newQry.OrderBy( p => p.Name ) );
                }
                gReport.DataBind();



            }
        }
        protected void addHomeboundResident_Click( object sender, EventArgs e )
        {
            string url = "/Pastoral/Homebound/";
            var contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                url += "?PersonId=" + contextEntity.Id;
            }
            Response.Redirect( url );
        }

        protected void gReport_RowSelected( object sender, RowEventArgs e )
        {
            Response.Redirect( "~/Pastoral/Homebound/" + e.RowKeyId );
        }

        protected void gReport_OpenWorkflow( object sender, RowEventArgs e )
        {


        }

        private void ShowMessage( string message, string header = "Information", string cssClass = "panel panel-warning" )
        {
            pnlMain.Visible = false;
            pnlInfo.Visible = true;
            ltHeading.Text = header;
            ltBody.Text = message;
            pnlInfo.CssClass = cssClass;
        }

        protected void btnReopen_Command( object sender, CommandEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {

                WorkflowService workflowService = new WorkflowService( rockContext );
                Workflow workflow = workflowService.Get( e.CommandArgument.ToString().AsInteger() );
                if ( workflow != null && !workflow.IsActive )
                {
                    workflow.Status = "Active";
                    workflow.CompletedDateTime = null;

                    // Find the summary activity and activate it.
                    WorkflowActivityType workflowActivityType = workflow.WorkflowType.ActivityTypes.Where( at => at.Name.Contains( "Summary" ) ).FirstOrDefault();
                    WorkflowActivity workflowActivity = WorkflowActivity.Activate( WorkflowActivityTypeCache.Get( workflowActivityType.Guid ), workflow, rockContext );

                }
                rockContext.SaveChanges();
            }
            BindGrid();
        }
        #endregion
    }
}