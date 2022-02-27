// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_secc.PastoralCare
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    ///     
    [ContextAware( typeof( Person ) )]
    [DisplayName( "Hospital List" )]
    [Category( "SECC > Pastoral Care" )]
    [Description( "A summary of all the current hospitalizations that have been reported to Pastoral Care." )]
    [WorkflowTypeField( "Hospital Admission Workflow" )]

    public partial class HospitalList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "HospitalAdmissionWorkflow" ) ) )
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
            gReport.Actions.AddButton.Text = "<i class=\"fa fa-plus\" Title=\"Add Hospitalization\"></i>";
            gReport.Actions.AddButton.Enabled = true;
            gReport.Actions.AddClick += addHospitalization_Click;
            gReport.Actions.ShowMergeTemplate = false;

            gReport.Actions.ShowExcelExport = false;

            if ( this.ContextEntity() == null )
            {
                LinkButton excel = new LinkButton()
                {
                    ID = "btnExcel",
                    Text = "<i class='fa fa-table'></i>",
                    CssClass = "btn btn-default btn-sm"
                };
                gReport.Actions.Controls.Add( excel );
                excel.Click += GenerateExcel;
                ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( excel );
            }
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
        protected void addHospitalization_Click( object sender, EventArgs e )
        {
            string url = "/Pastoral/Hospitalization/";
            var contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                url += "?PersonId=" + contextEntity.Id;
            }
            Response.Redirect( url );
        }

        private void GenerateExcel( object sender, EventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var newQry = GetQuery( rockContext );
                var hospitals = newQry.Select( q => q.Hospital ).DistinctBy( h => h ).ToList();

                // create default settings
                string filename = gReport.ExportFilename;
                string workSheetName = "List";
                string title = "Hospital Report";

                ExcelPackage excel = new ExcelPackage();
                excel.Workbook.Properties.Title = title;

                // add author info
                Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
                if ( userLogin != null )
                {
                    excel.Workbook.Properties.Author = userLogin.Person.FullName;
                }
                else
                {
                    excel.Workbook.Properties.Author = "Rock";
                }

                // add the page that created this
                excel.Workbook.Properties.SetCustomPropertyValue( "Source", this.Page.Request.Url.OriginalString );

                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( workSheetName );
                worksheet.PrinterSettings.LeftMargin = .5m;
                worksheet.PrinterSettings.RightMargin = .5m;
                worksheet.PrinterSettings.TopMargin = .5m;
                worksheet.PrinterSettings.BottomMargin = .5m;

                //Print Title
                // format and set title
                worksheet.Cells[1, 1].Value = title;
                using ( ExcelRange r = worksheet.Cells[1, 1, 1, 7] )
                {
                    r.Merge = true;
                    r.Style.Font.SetFromFont( new Font( "Calibri", 28, FontStyle.Regular ) );
                    r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // set border
                    r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                worksheet.Cells[2, 1].Value = Rock.RockDateTime.Today.ToString( "MMMM d, yyyy" );
                using ( ExcelRange r = worksheet.Cells[2, 1, 2, 7] )
                {
                    r.Merge = true;
                    r.Style.Font.SetFromFont( new Font( "Calibri", 20, FontStyle.Regular ) );
                    r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // set border
                    r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                int rowCounter = 3;

                foreach ( var hospital in hospitals )
                {

                    //Hospital header
                    var hospitalInfo = newQry
                        .Where( q => q.Hospital == hospital )
                        .FirstOrDefault();
                    worksheet.Cells[rowCounter, 1].Value = hospital;
                    worksheet.Cells[rowCounter, 6].Value = hospitalInfo.HospitalPhone;

                    using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 5] )
                    {
                        r.Merge = true;
                        r.Style.Font.SetFromFont( new Font( "Calibri", 20, FontStyle.Regular ) );
                        r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        r.Style.Font.Color.SetColor( Color.White );
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );
                    }
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 6, rowCounter, 10] )
                    {
                        r.Merge = true;
                        r.Style.Font.SetFromFont( new Font( "Calibri", 20, FontStyle.Regular ) );
                        r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        r.Style.Font.Color.SetColor( Color.White );
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );
                    }

                    rowCounter++; //Put the address on second line
                    worksheet.Cells[rowCounter, 1].Value = hospitalInfo.HospitalAddress;
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 7] )
                    {
                        r.Merge = true;
                        r.Style.Font.SetFromFont( new Font( "Calibri", 18, FontStyle.Regular ) );
                        r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        r.Style.Font.Color.SetColor( Color.White );
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );
                    }
                    rowCounter++;

                    //Person header
                    worksheet.Cells[rowCounter, 1].Value = "Name";

                    worksheet.Cells[rowCounter, 3].Value = "Age";

                    worksheet.Cells[rowCounter, 4].Value = "M/F";

                    worksheet.Cells[rowCounter, 5].Value = "Membership";

                    worksheet.Cells[rowCounter, 6].Value = "Admit Date";

                    worksheet.Cells[rowCounter, 7].Value = "Room";

                    using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 7] )
                    {
                        r.Style.Font.Bold = true;
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 200, 200, 200 ) );
                    }


                    rowCounter++;

                    //Patient info
                    var patients = newQry.Where( q => q.Hospital == hospital );
                    foreach ( var patient in patients )
                    {
                        SetExcelValue( worksheet.Cells[rowCounter, 1], patient.PersonToVisit.FullName );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 2] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 3], patient.PersonToVisit.FormatAge() );

                        SetExcelValue( worksheet.Cells[rowCounter, 4], patient.PersonToVisit.Gender );

                        SetExcelValue( worksheet.Cells[rowCounter, 5], patient.PersonToVisit.ConnectionStatusValue );

                        SetExcelValue( worksheet.Cells[rowCounter, 6], patient.AdmitDate.HasValue?patient.AdmitDate.Value.Date.ToShortDateString():"" );

                        SetExcelValue( worksheet.Cells[rowCounter, 7], patient.Room );
                        rowCounter++;


                        //Second line
                        SetExcelValue( worksheet.Cells[rowCounter, 1], "Relationships:" );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 1] )
                        {
                            r.Style.Font.Bold = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 2], GetPersonRelationships( patient.PersonToVisit ) );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 2, rowCounter, 7] )
                        {
                            r.Merge = true;
                        }
                        rowCounter++;

                        //begin third row
                        SetExcelValue( worksheet.Cells[rowCounter, 1], "Notifier: " + patient.NotifiedBy );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 2] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 3], patient.Description );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 3, rowCounter, 6] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 7], "Visits: " + patient.Visits.ToString() );//ToString to make formatting better
                        rowCounter++;

                        //Fourth row
                        SetExcelValue( worksheet.Cells[rowCounter, 1], "Last Visit:" );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 1] )
                        {
                            r.Merge = true;
                            r.Style.Font.Bold = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 2], ( patient.LastVisitor != "N/A" ? patient.LastVisitor + " " : "" ) + ( patient.LastVisitDate != "N/A" ? " on " + patient.LastVisitDate + ": " : "" ) + patient.LastVisitNotes );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 2, rowCounter, 7] )
                        {
                            r.Merge = true;
                        }

                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 7] )
                        {
                            r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        }
                        rowCounter++;
                    }
                }
                // autofit columns for all cells
                worksheet.Cells.AutoFitColumns( 0 );

                for ( var i = 1; i < 8; i++ )
                {
                    worksheet.Column( i ).Width = 18;

                }

                byte[] byteArray;
                using ( MemoryStream ms = new MemoryStream() )
                {
                    excel.SaveAs( ms );
                    byteArray = ms.ToArray();
                }

                // send the spreadsheet to the browser
                this.Page.EnableViewState = false;
                this.Page.Response.Clear();
                this.Page.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                this.Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=" + filename );

                this.Page.Response.Charset = string.Empty;
                this.Page.Response.BinaryWrite( byteArray );
                this.Page.Response.Flush();
                this.Page.Response.End();
            }
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
            Response.Redirect( "~/Pastoral/Hospitalization/" + e.RowKeyId );
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
                    WorkflowActivity workflowActivity = WorkflowActivity.Activate( WorkflowActivityTypeCache.Get(workflowActivityType.Id, rockContext), workflow, rockContext );

                }
                rockContext.SaveChanges();
            }
            BindGrid();
        }


        /// <summary>
        /// Formats the export value.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="exportValue">The export value.</param>
        private void SetExcelValue( ExcelRange range, object exportValue )
        {
            if ( exportValue != null &&
                ( exportValue is decimal || exportValue is decimal? ||
                exportValue is int || exportValue is int? ||
                exportValue is double || exportValue is double? ||
                exportValue is DateTime || exportValue is DateTime? ) )
            {
                range.Value = exportValue;
            }
            else
            {
                string value = exportValue != null ? exportValue.ToString().ConvertBrToCrLf().Replace( "&nbsp;", " " ) : string.Empty;
                range.Value = value;
                if ( value.Contains( Environment.NewLine ) )
                {
                    range.Style.WrapText = true;
                }
            }
        }


        private IQueryable<HospitalRow> GetQuery( RockContext rockContext )
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

            Guid hospitalWorkflow = GetAttributeValue( "HospitalAdmissionWorkflow" ).AsGuid();

            var workflowType = new WorkflowTypeService( rockContext ).Get( hospitalWorkflow );
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
                    .Where( w => ( w.WorkflowType.Guid == hospitalWorkflow ) && ( w.Status == "Active" || w.Status == status ) );

            var visitQry = workflowActivityService.Queryable()
                    .Join(
                        attributeValueService.Queryable(),
                        wa => wa.Id,
                        av => av.EntityId.Value,
                        ( wa, av ) => new { WorkflowActivity = wa, AttributeValue = av } )
                .Where( a => activityAttributeIds.Contains( a.AttributeValue.AttributeId ) )
                .GroupBy( wa => wa.WorkflowActivity )
                .Select( obj => new { WorkflowActivity = obj.Key, AttributeValues = obj.Select( a => a.AttributeValue ) } );

            if ( contextEntity != null )
            {
                var personGuid = ( ( Person ) contextEntity ).Aliases.Select( a => a.Guid.ToString() ).ToList();
                var validWorkflowIds = new AttributeValueService( rockContext ).Queryable()
                    .Where( av => av.Attribute.Key == "PersonToVisit" && personGuid.Contains( av.Value ) ).Select( av => av.EntityId );
                wfTmpqry = wfTmpqry.Where( w => validWorkflowIds.Contains( w.Id ) );
                visitQry = visitQry.Where( w => validWorkflowIds.Contains( w.WorkflowActivity.WorkflowId ) );
                gReport.Columns[10].Visible = true;
            }

            var visits = visitQry.ToList();

            var workflows = wfTmpqry.Join(
                    attributeValueService.Queryable(),
                    obj => obj.Id,
                    av => av.EntityId.Value,
                    ( obj, av ) => new { Workflow = obj, AttributeValue = av } )
                .Where( a => attributeIds.Contains( a.AttributeValue.AttributeId ) )
                .GroupBy( obj => obj.Workflow )
                .Select( obj => new { Workflow = obj.Key, AttributeValues = obj.Select( a => a.AttributeValue ) } )
                .ToList();

            var qry = workflows.AsQueryable().GroupJoin( visits.AsQueryable(), wf => wf.Workflow.Id, wa => wa.WorkflowActivity.WorkflowId, ( wf, wa ) => new { Workflow = wf, WorkflowActivities = wa } )
                .Select( obj => new { Workflow = obj.Workflow.Workflow, AttributeValues = obj.Workflow.AttributeValues, VisitationActivities = obj.WorkflowActivities } ).ToList();


            if ( contextEntity == null )
            {
                // Make sure they aren't deceased
                qry = qry.AsQueryable().Where( w => !
                    ( personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ) != null ?
                    personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Person.IsDeceased :
                    false ) ).ToList();
            }

            var newQry = qry.Select( w => new HospitalRow
            {
                Id = w.Workflow.Id,
                Workflow = w.Workflow,
                Name = w.Workflow.Name,
                Hospital = w.AttributeValues.Where( av => av.AttributeKey == "Hospital" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                HospitalAddress = new Func<string>( () =>
                {
                    DefinedValueCache dv = DefinedValueCache.Get( w.AttributeValues.Where( av => av.AttributeKey == "Hospital" ).Select( av => av.Value ).FirstOrDefault().AsGuid() );
                    return dv.AttributeValues["Qualifier1"].ValueFormatted + " " +
                        dv.AttributeValues["Qualifier2"].ValueFormatted + " " +
                        dv.AttributeValues["Qualifier3"].ValueFormatted + ", " +
                        dv.AttributeValues["Qualifier4"].ValueFormatted;
                } )(),
                PersonToVisit = new Func<Person>( () =>
                {
                    PersonAlias pa = personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).Select( av => av.Value ).FirstOrDefault().AsGuid() );
                    if ( pa != null )
                    {
                        return pa.Person;
                    }
                    return new Person();
                } )(),
                Age = new Func<int?>( () =>
                {
                    PersonAlias pa = personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).Select( av => av.Value ).FirstOrDefault().AsGuid() );
                    if ( pa != null )
                    {
                        return pa.Person.Age;
                    }
                    return null;
                } )(),
                Room = w.AttributeValues.Where( av => av.AttributeKey == "Room" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                AdmitDate = w.AttributeValues.Where( av => av.AttributeKey == "AdmitDate" ).Select( av => av.ValueAsDateTime ).FirstOrDefault(),
                Description = w.AttributeValues.Where( av => av.AttributeKey == "VisitationRequestDescription" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                Visits = w.VisitationActivities.Where( a => a.AttributeValues != null && a.AttributeValues.Where( av => av.AttributeKey == "VisitDate" && !string.IsNullOrWhiteSpace( av.Value ) ).Any() ).Count(),
                LastVisitor = new Func<string>( () => {
                    var visitor = w.VisitationActivities.Where( a => a.AttributeValues != null && a.AttributeValues.Where( av => av.AttributeKey == "VisitDate" && !string.IsNullOrWhiteSpace( av.Value ) ).Any() ).Select( va => va.AttributeValues.Where( av => av.AttributeKey == "Visitor" ).LastOrDefault() ).LastOrDefault();
                    if ( visitor != null )
                    {
                        return visitor.ValueFormatted;
                    }
                    return "N/A";
                } )(),
                LastVisitDate = w.VisitationActivities.Where( a => a.AttributeValues != null && a.AttributeValues.Where( av => av.AttributeKey == "VisitDate" && !string.IsNullOrWhiteSpace( av.Value ) ).Any() ).Select( va => va.AttributeValues.Where( av => av.AttributeKey == "VisitDate" ).LastOrDefault() ).Select( av => av == null ? "N/A" : av.ValueFormatted ).DefaultIfEmpty( "N/A" ).LastOrDefault(),
                LastVisitNotes = w.VisitationActivities.Where( a => a.AttributeValues != null && a.AttributeValues.Where( av => av.AttributeKey == "VisitDate" && !string.IsNullOrWhiteSpace( av.Value ) ).Any() ).Select( va => va.AttributeValues.Where( av => av.AttributeKey == "VisitNote" ).LastOrDefault() ).Select( av => av == null ? "N/A" : av.ValueFormatted ).DefaultIfEmpty( "N/A" ).LastOrDefault(),
                DischargeDate = w.AttributeValues.Where( av => av.AttributeKey == "DischargeDate" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                Status = w.Workflow.Status,
                Communion = w.AttributeValues.Where( av => av.AttributeKey == "Communion" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                Actions = ""
            } ).ToList().AsQueryable().OrderBy( p => p.Hospital ).ThenBy( p => p.PersonToVisit.LastName );

            return newQry;
        }

        public class HospitalRow {

            public int Id { get; set; }
            public Workflow Workflow { get; set; }
            public string Name { get; set; }
            public string Hospital { get; set; }
            public string HospitalAddress { get; set; }
            public string HospitalPhone { get; set; }
            public string NotifiedBy { get; set; }
            public Person PersonToVisit { get; set; }
            public int? Age { get; set; }
            public string Room { get; set; }
            public DateTime? AdmitDate { get; set; }
            public string Description { get; set; }
            public int Visits { get; set; }
            public string LastVisitor  { get; set; }
            public string LastVisitDate { get; set; }
            public string LastVisitNotes { get; set; }
            public string DischargeDate { get; set; }
            public string Status { get; set; }
            public string Communion { get; set; }
            public string Actions { get; set; }
        }
    }
}