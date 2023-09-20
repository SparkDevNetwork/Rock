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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
    [ContextAware( typeof( Person ) )]
    [DisplayName( "Nursing Home List" )]
    [Category( "SECC > Pastoral Care" )]
    [Description( "A summary of all the current nursing home residents that have been reported to Pastoral Care." )]
    [WorkflowTypeField( "Nursing Home Resident Workflow" )]
    [DefinedTypeField( "Nursing Home List" )]

    [GroupField( "Volunteer Group", "Group to use for list of Volunteers on the Excel export. If omitted, volunteers will not be displayed.", false, "", "", 0 )]
    public partial class NursingHomeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "NursingHomeResidentWorkflow" ) ) )
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
                var newQry = GetQuery( rockContext );

                SortProperty sortProperty = gReport.SortProperty;
                if ( sortProperty != null )
                {
                    newQry = newQry.Sort( sortProperty );
                }
                gReport.SetLinqDataSource( newQry );
                gReport.DataBind();
            }
        }
        protected void addHospitalization_Click( object sender, EventArgs e )
        {
            string url = "/Pastoral/NursingHome/";
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

                var nursingHomes = newQry.Select( q => q.NursingHome ).DistinctBy( n => n ).ToList();

                // create default settings
                string filename = gReport.ExportFilename;
                string workSheetName = "List";
                string title = "Nursing Home Residents List";

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
                using ( ExcelRange r = worksheet.Cells[1, 1, 1, 15] )
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
                using ( ExcelRange r = worksheet.Cells[2, 1, 2, 15] )
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

                foreach ( var nursigngHome in nursingHomes )
                {

                    //Nursing Home header
                    var nursingHomeInfo = newQry
                        .Where( q => q.NursingHome == nursigngHome )
                        .FirstOrDefault();
                    worksheet.Cells[rowCounter, 1].Value = nursigngHome;
                    worksheet.Cells[rowCounter, 6].Value = nursingHomeInfo.PhoneNumber;
                    worksheet.Cells[rowCounter, 11].Value = nursingHomeInfo.Address;

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
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 11, rowCounter, 15] )
                    {
                        r.Merge = true;
                        r.Style.Font.SetFromFont( new Font( "Calibri", 20, FontStyle.Regular ) );
                        r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        r.Style.Font.Color.SetColor( Color.White );
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );
                    }
                    rowCounter++;

                    worksheet.Cells[rowCounter, 1].Value = "Pastoral Minister: " + nursingHomeInfo.PastoralMinister;
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 15] )
                    {
                        r.Merge = true;
                        r.Style.Font.SetFromFont( new Font( "Calibri", 15, FontStyle.Regular ) );
                        r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        r.Style.Font.Color.SetColor( Color.White );
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );
                    }
                    rowCounter++;
            
                    if ( nursingHomeInfo.Volunteers.IsNotNullOrWhiteSpace() )
                    {
                        worksheet.Cells[rowCounter, 1].Value = "Volunteers: " + nursingHomeInfo.Volunteers;
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 15] )
                        {
                            r.Merge = true;
                            r.Style.Font.SetFromFont( new Font( "Calibri", 15, FontStyle.Regular ) );
                            r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            r.Style.Font.Color.SetColor( Color.White );
                            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );
                        }
                        rowCounter++;
                    }

                    //Person header
                    worksheet.Cells[rowCounter, 1].Value = "Name";

                    worksheet.Cells[rowCounter, 3].Value = "Age";

                    worksheet.Cells[rowCounter, 4].Value = "M/F";

                    worksheet.Cells[rowCounter, 5].Value = "Membership";

                    worksheet.Cells[rowCounter, 6].Value = "Admit Date";

                    worksheet.Cells[rowCounter, 7].Value = "Room";

                    worksheet.Cells[rowCounter, 8].Value = "Notified By";
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 8, rowCounter, 9] )
                    {
                        r.Merge = true;
                    }

                    worksheet.Cells[rowCounter, 10].Value = "Description";
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 10, rowCounter, 14] )
                    {
                        r.Merge = true;
                    }

                    worksheet.Cells[rowCounter, 15].Value = "Visits";
                    using ( ExcelRange r = worksheet.Cells[rowCounter, 15, rowCounter, 15] )
                    {
                        r.Merge = true;
                    }

                    using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 15] )
                    {
                        r.Style.Font.Bold = true;
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 200, 200, 200 ) );
                    }

                    rowCounter++;

                    //Resident info
                    var residents = newQry.Where( q => q.NursingHome == nursigngHome );
                    foreach ( var resident in residents )
                    {
                        SetExcelValue( worksheet.Cells[rowCounter, 1], resident.Person.FullName );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 2] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 3], resident.Person.FormatAge() );

                        SetExcelValue( worksheet.Cells[rowCounter, 4], resident.Person.Gender );

                        SetExcelValue( worksheet.Cells[rowCounter, 5], resident.Person.ConnectionStatusValue );

                        SetExcelValue( worksheet.Cells[rowCounter, 6], resident.AdmitDate.HasValue ? resident.AdmitDate.Value.Date.ToShortDateString() : "" );

                        SetExcelValue( worksheet.Cells[rowCounter, 7], resident.Room );

                        SetExcelValue( worksheet.Cells[rowCounter, 8], resident.NotifiedBy );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 8, rowCounter, 9] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 10], resident.Description );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 10, rowCounter, 14] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 15], resident.Visits.ToString() );//ToString to make formatting better

                        rowCounter++;

                        //Second line
                        SetExcelValue( worksheet.Cells[rowCounter, 1], "Relationships:" );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 1] )
                        {
                            r.Style.Font.Bold = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 2], GetPersonRelationships( resident.Person ) );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 2, rowCounter, 7] )
                        {
                            r.Merge = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 8], "Last Visit:" );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 8, rowCounter, 8] )
                        {
                            r.Merge = true;
                            r.Style.Font.Bold = true;
                        }

                        SetExcelValue( worksheet.Cells[rowCounter, 9], ( resident.LastVisitor != "N/A" ? resident.LastVisitor + " " : "" ) + ( resident.LastVisitDate != "N/A" ? " on " + resident.LastVisitDate + ": " : "" ) + resident.LastVisitNotes );
                        using ( ExcelRange r = worksheet.Cells[rowCounter, 9, rowCounter, 15] )
                        {
                            r.Merge = true;
                        }

                        using ( ExcelRange r = worksheet.Cells[rowCounter, 1, rowCounter, 15] )
                        {
                            r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        }

                        rowCounter++;
                    }
                }
                // autofit columns for all cells
                worksheet.Cells.AutoFitColumns( 0 );

                for ( var i = 1; i < 16; i++ )
                {
                    worksheet.Column( i ).Width = 16;

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

        protected void gReport_RowSelected( object sender, RowEventArgs e )
        {
            Response.Redirect( "~/Pastoral/NursingHome/" + e.RowKeyId );
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
                    WorkflowActivity workflowActivity = WorkflowActivity.Activate( WorkflowActivityTypeCache.Get( workflowActivityType.Id, rockContext ), workflow, rockContext );

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

        public IQueryable<NursingHomeRow> GetQuery( RockContext rockContext )
        {
            var contextEntity = this.ContextEntity();

            var workflowService = new WorkflowService( rockContext );
            var workflowActivityService = new WorkflowActivityService( rockContext );
            var attributeService = new AttributeService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var definedValueService = new DefinedValueService( rockContext );
            var entityTypeService = new EntityTypeService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );

            Guid nursingHomeAdmissionWorkflow = GetAttributeValue( "NursingHomeResidentWorkflow" ).AsGuid();
            Guid nursingHomeList = GetAttributeValue( "NursingHomeList" ).AsGuid();
            Guid volunteerGroup = GetAttributeValue( "VolunteerGroup" ).AsGuid();
            Group groupId = new Group();

            List<DefinedValueCache> facilities = DefinedTypeCache.Get( nursingHomeList ).DefinedValues;
            Dictionary<Guid, string> volunteerList = new Dictionary<Guid, string>();
            groupId = groupService.GetByGuid( volunteerGroup );

            var groupMemberEntityTypeId = EntityTypeCache.Get( typeof( GroupMember ) ).Id;

            var groupMemberAttributeQry = attributeService.Queryable()
                .Where( a => a.EntityTypeId == groupMemberEntityTypeId )
                .Select( a => a.Id );

            var groupMemberAttributeValueQry = attributeValueService.Queryable()
               .Where( av => groupMemberAttributeQry.Contains( av.AttributeId ) );

            if ( groupId.IsNotNull() )
            {
                var groupMemberList = groupMemberService.Queryable()
                   .Where( a => a.GroupId == groupId.Id && a.GroupMemberStatus == GroupMemberStatus.Active )
                   .GroupJoin( groupMemberAttributeValueQry,
                        gm => gm.Id,
                        av => av.EntityId,
                        ( gm, av ) => new { GroupMember = gm, GroupMemberAttributeValues = av } )
                     .ToList();

                var groupMembers = new List<GroupMember>();

                foreach ( var set in groupMemberList )
                {
                    var groupMember = set.GroupMember;

                    groupMember.Attributes = set.GroupMemberAttributeValues
                        .ToDictionary( av => av.AttributeKey, av => AttributeCache.Get( av.AttributeId ) );

                    groupMember.AttributeValues = set.GroupMemberAttributeValues
                        .ToDictionary( av => av.AttributeKey, av => new AttributeValueCache( av ) );

                    groupMembers.Add( groupMember );
                }

                foreach ( var nursingHome in facilities )
                {
                    foreach ( var groupMember in groupMembers )
                    {
                        if ( groupMember.GetAttributeValue( "NursingHomes" ).IsNotNullOrWhiteSpace() )
                        {
                           
                            if ( groupMember.GetAttributeValue( "NursingHomes" ).ToLower().Contains( nursingHome.Guid.ToString().ToLower() ) )
                            {
                                if ( volunteerList.ContainsKey( nursingHome.Guid ) )
                                {
                                    volunteerList[ nursingHome.Guid ] = volunteerList[ nursingHome.Guid ] + ", " + groupMember.EntityStringValue;
                                }
                            
                                else
                                {
                                    volunteerList.Add( nursingHome.Guid, groupMember.EntityStringValue );
                                }
                            }
                        }
                    }

                }

            }
            int entityTypeId = entityTypeService.Queryable().Where( et => et.Name == typeof( Workflow ).FullName ).FirstOrDefault().Id;
            string status = ( contextEntity != null ? "Completed" : "Active" );

            var workflowType = new WorkflowTypeService( rockContext ).Get( nursingHomeAdmissionWorkflow );
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
                    .Where( w => ( w.WorkflowType.Guid == nursingHomeAdmissionWorkflow ) && ( w.Status == "Active" || w.Status == status ) );

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

            var qry = workflows.AsQueryable().GroupJoin( visits.AsQueryable(), wf => wf.Workflow.Id, wa => wa.WorkflowActivity.WorkflowId, ( Workflow, wa ) => new { Workflow = Workflow, WorkflowActivities = wa } )
                .Select( obj => new { Workflow = obj.Workflow.Workflow, AttributeValues = obj.Workflow.AttributeValues, VisitationActivities = obj.WorkflowActivities } ).ToList();

            if ( contextEntity == null )
            {
                // Make sure they aren't deceased
                qry = qry.AsQueryable().Where( w => !
                    ( personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ) != null ?
                    personAliasService.Get( w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Person.IsDeceased :
                    false ) ).ToList();
            }

            var newQry = qry.Select( w => new NursingHomeRow
            {
                Id = w.Workflow.Id,
                Workflow = w.Workflow,
                NursingHome = new Func<string>( () =>
                {
                    if ( w.AttributeValues.Where( av => av.AttributeKey == "NursingHome" ).Select( av => av.Value ).Any() )
                    {
                        return facilities.Where( h => h.Guid == w.AttributeValues.Where( av => av.AttributeKey == "NursingHome" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).Select( dv => dv.Value ).FirstOrDefault();
                    }
                    return "N/A";
                } )(),
                Person = new Func<Person>( () =>
                {
                    AttributeValue personAliasAV = w.AttributeValues.Where( av => av.AttributeKey == "PersonToVisit" ).FirstOrDefault();
                    if ( personAliasAV != null )
                    {
                        PersonAlias pa = personAliasService.Get( personAliasAV.Value.AsGuid() );

                        return pa != null ? pa.Person : new Person();
                    }
                    return new Person();
                } )(),
                Address = new Func<string>( () =>
                {
                    DefinedValueCache dv = facilities.Where( h => h.Guid == w.AttributeValues.Where( av => av.AttributeKey == "NursingHome" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).FirstOrDefault();
                    if ( dv != null )
                    {
                        return dv.AttributeValues["Qualifier1"].ValueFormatted + " " +
                            dv.AttributeValues["Qualifier2"].ValueFormatted + " " +
                            dv.AttributeValues["Qualifier3"].ValueFormatted + ", " +
                            dv.AttributeValues["Qualifier4"].ValueFormatted;
                    }
                    return "";
                } )(),
                PastoralMinister = new Func<string>( () =>
                {
                    DefinedValueCache dv = facilities.Where( h => h.Guid == w.AttributeValues.Where( av => av.AttributeKey == "NursingHome" ).Select( av => av.Value ).FirstOrDefault().AsGuid() ).FirstOrDefault();
                    if ( dv != null && ( dv.AttributeValues.ContainsKey( "PastoralMinister" ) || dv.AttributeValues.ContainsKey( "Qualifier6" ) ) )
                    {
                        return dv.AttributeValues.ContainsKey( "PastoralMinister" ) ? dv.AttributeValues["PastoralMinister"].ValueFormatted : dv.AttributeValues["Qualifier6"].ValueFormatted;
                    }
                    return "";
                } )(),
                Volunteers = new Func<string>( () =>
                {
                    String vList = "";
                    if ( volunteerList.TryGetValue( w.AttributeValues.Where( av => av.AttributeKey == "NursingHome" ).Select( av => av.Value ).FirstOrDefault().AsGuid(), out vList ) )
                    {
                        return vList;
                    }
                    else
                    {
                        return "";
                    }
                } )(),
                Room = w.AttributeValues.Where( av => av.AttributeKey == "Room" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                AdmitDate = w.AttributeValues.Where( av => av.AttributeKey == "AdmitDate" ).Select( av => av.ValueAsDateTime ).FirstOrDefault(),
                Description = w.AttributeValues.Where( av => av.AttributeKey == "VisitationRequestDescription" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                Visits = w.VisitationActivities.Where( a => a.AttributeValues != null && a.AttributeValues.Where( av => av.AttributeKey == "VisitDate" && !string.IsNullOrWhiteSpace( av.Value ) ).Any() ).Count(),
                LastVisitor = new Func<string>( () =>
                 {
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
            } ).ToList().AsQueryable().OrderBy( p => p.NursingHome ).ThenBy( p => p.Person.FullName );
            return newQry;
        }

        public class NursingHomeRow
        {

            public int Id { get; set; }
            public Workflow Workflow { get; set; }
            public string Name { get; set; }
            public string NursingHome { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
            public string PastoralMinister { get; set; }
            public string Volunteers { get; set; }
            public string NotifiedBy { get; set; }
            public Person Person { get; set; }
            public int? Age { get; set; }
            public string Room { get; set; }
            public DateTime? AdmitDate { get; set; }
            public string Description { get; set; }
            public int Visits { get; set; }
            public string LastVisitor { get; set; }
            public string LastVisitDate { get; set; }
            public string LastVisitNotes { get; set; }
            public string DischargeDate { get; set; }
            public string Status { get; set; }
            public string Communion { get; set; }
            public string Actions { get; set; }
        }
        #endregion
    }
}