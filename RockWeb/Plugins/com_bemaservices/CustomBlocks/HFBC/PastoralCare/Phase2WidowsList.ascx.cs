using System;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.PastoralCare
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    ///     
    [ContextAware( typeof( Person ) )]
    [DisplayName( "Phase 2 Widows List" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "A summary of all the current widows that have been reported to Pastoral Care." )]
    [WorkflowTypeField( "Deacon Widows Workflow" )]

    public partial class Phase2WidowsList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "DeaconWidowsWorkflow" ) ) )
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
            gReport.Actions.AddButton.Text = "<i class=\"fa fa-plus\" Title=\"Add Widow\"></i>";
            gReport.Actions.AddButton.Enabled = true;
            gReport.Actions.AddClick += addWidow_Click;
            gReport.Actions.ShowMergeTemplate = false;

            gReport.Actions.ShowExcelExport = false;
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
            using ( RockContext rockContext = new RockContext() )
            {
                var qry = GetQuery( rockContext );

                SortProperty sortProperty = gReport.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                gReport.SetLinqDataSource( qry );
                gReport.DataBind();
            }

        }
        protected void addWidow_Click( object sender, EventArgs e )
        {
            string url = "/Pastoral/DeaconWidows/";
            var contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                url += "?PersonId=" + contextEntity.Id;
            }
            Response.Redirect( url );
        }

        private string GetPersonRelationships( Person personToVisit )
        {
            var familyMembers = personToVisit.GetFamilyMembers();
            var familyStrings = familyMembers.Select( gm => gm.Person ).ToList().Select( p => p.FullName + " (" + p.Age + ")" );
            return string.Join( ", ", familyStrings );
        }

        /// <summary>
        /// Adds the grid columns.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        private void AddGridColumns( object item )
        {
            Type oType = item.GetType();

            gReport.Columns.Clear();

            foreach ( var prop in oType.GetProperties() )
            {
                BoundField bf = new BoundField();

                if ( prop.PropertyType == typeof( bool ) ||
                    prop.PropertyType == typeof( bool? ) )
                {
                    bf = new BoolField();
                }

                if ( prop.PropertyType == typeof( DateTime ) ||
                    prop.PropertyType == typeof( DateTime? ) )
                {
                    bf = new DateTimeField();
                }

                bf.DataField = prop.Name;
                bf.SortExpression = prop.Name;
                bf.HeaderText = prop.Name.SplitCase();
                gReport.Columns.Add( bf );
            }
        }
        #endregion

        protected void gReport_RowSelected( object sender, RowEventArgs e )
        {
            Response.Redirect( "~/Pastoral/DeaconWidows/" + e.RowKeyId );
        }

        private void ShowMessage( string message, string header = "Information", string cssClass = "panel panel-warning" )
        {
            pnlMain.Visible = false;
            pnlInfo.Visible = true;
            ltHeading.Text = header;
            ltBody.Text = message;
            pnlInfo.CssClass = cssClass;
        }

        private IQueryable<DeaconWidowRow> GetQuery( RockContext rockContext )
        {
            
            var contextEntity = this.ContextEntity();

            var workflowService = new WorkflowService( rockContext );
            var workflowActivityService = new WorkflowActivityService( rockContext );
            var attributeService = new AttributeService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var definedValueService = new DefinedValueService( rockContext );
            var entityTypeService = new EntityTypeService( rockContext );


            int entityTypeId = entityTypeService.Queryable().Where( et => et.Name == typeof( Workflow ).FullName ).FirstOrDefault().Id;
            string status = ( contextEntity != null ? "Completed" : "Active" );

            Guid windowsWorkflow = GetAttributeValue( "DeaconWidowsWorkflow" ).AsGuid();

            var workflowType = new WorkflowTypeService( rockContext ).Get( windowsWorkflow );
            var workflowTypeIdAsString = workflowType.Id.ToString();

            var attributeIds = attributeService.Queryable()
                .Where( a => a.EntityTypeQualifierColumn == "WorkflowTypeId" && a.EntityTypeQualifierValue == workflowTypeIdAsString )
                .Select( a => a.Id ).ToList();

            // Look up the activity type for "Visitation"
            var visitationActivityIdAsString = workflowType.ActivityTypes.Where( at => at.Name == "Visitation Info" ).Select( at => at.Id.ToString() ).FirstOrDefault();

            var activityAttributeIds = attributeService.Queryable()
                .Where( a => a.EntityTypeQualifierColumn == "ActivityTypeId" && a.EntityTypeQualifierValue == visitationActivityIdAsString )
                .Select( a => a.Id ).ToList();

            var wfTmpqry = workflowService.Queryable().AsNoTracking()
                    .Where( w => ( w.WorkflowType.Guid == windowsWorkflow ) && ( w.Status == "Active" || w.Status == status ) );

            if ( contextEntity != null )
            {
                var personGuid = ( ( Person ) contextEntity ).Aliases.Select( a => a.Guid.ToString() ).ToList();
                var validWorkflowIds = new AttributeValueService( rockContext ).Queryable()
                    .Where( av => av.Attribute.Key == "Widow" && personGuid.Contains( av.Value ) ).Select( av => av.EntityId );
                wfTmpqry = wfTmpqry.Where( w => validWorkflowIds.Contains( w.Id ) );
                gReport.Columns[10].Visible = true;
            }

            var tqry = wfTmpqry.Join( attributeValueService.Queryable(),
                obj => obj.Id,
                av => av.EntityId.Value,
                ( obj, av ) => new { Workflow = obj, AttributeValue = av } )
                .Where( a => attributeIds.Contains( a.AttributeValue.AttributeId ) )
                .GroupBy( obj => obj.Workflow )
                .Select( obj => new { Workflow = obj.Key, AttributeValues = obj.Select( a => a.AttributeValue ) } )
                .GroupJoin( workflowActivityService.Queryable()
                    .Join(
                        attributeValueService.Queryable(),
                        obj => obj.Id,
                        av => av.EntityId.Value,
                        ( obj, av ) => new { WorkflowActivity = obj, AttributeValue = av } )
                        .Where( a => activityAttributeIds.Contains( a.AttributeValue.AttributeId )
                    ).GroupBy( obj => obj.WorkflowActivity )
                    .Select( obj => new { WorkflowActivity = obj.Key, AttributeValues = obj.Select( a => a.AttributeValue ) } ),
                obj => obj.Workflow.Id,
                wa => wa.WorkflowActivity.WorkflowId,
                ( obj, wa ) => new { Workflow = obj.Workflow, AttributeValues = obj.AttributeValues, VisitationActivities = wa } );
            var qry = tqry.ToList();

            if ( contextEntity == null )
            {
                // Make sure they aren't deceased
                qry = qry.AsQueryable().Where( w => !
                    ( personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "Widow" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ) != null ?
                    personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "Widow" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Person.IsDeceased :
                    false ) ).ToList();
            }

            var newQry = qry.Select( w => new DeaconWidowRow
            {
                Id = w.Workflow.Id,
                Workflow = w.Workflow,
                Widow = new Func<Person>( () =>
                {
                    PersonAlias pa = personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "Widow" ).Select( av => av.Value ).FirstOrDefault().AsGuid() );
                    if ( pa != null )
                    {
                        return pa.Person;
                    }
                    return new Person();
                } )(),
                Age = new Func<int?>( () =>
                {
                    PersonAlias pa = personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "Widow" ).Select( av => av.Value ).FirstOrDefault().AsGuid() );
                    if ( pa != null )
                    {
                        return pa.Person.Age.Value;
                    }
                    return null;
                } )(),
                Visits = w.VisitationActivities.Where( a => a.AttributeValues != null && a.AttributeValues.Where( av => av.AttributeKey == "VisitDate" && !string.IsNullOrWhiteSpace( av.Value ) ).Any() ).Count()
            } ).ToList().AsQueryable().OrderBy( p => p.Widow.LastName );

            return newQry;
        }

        public class DeaconWidowRow {

            public int Id { get; set; }
            public Workflow Workflow { get; set; }
            public Person Widow {get;set;}
            public int? Age { get; set; }
            public int Visits { get; set; }
        }
    }
}