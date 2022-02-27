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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Xml.Linq;
using OfficeOpenXml;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_secc.PastoralCare
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [DisplayName( "Communion List" )]
    [Category( "SECC > Pastoral  Care" )]
    [Description( "A list of all the pastoral care patients/residents that have been requested communion." )]
    [WorkflowTypeField( "Hospital Admission Workflow", "", false, true, "", "Workflows" )]
    [WorkflowTypeField( "Nursing Home Resident Workflow", "", false, true, "", "Workflows" )]
    [WorkflowTypeField( "Homebound Person Workflow", "", false, true, "", "Workflows" )]
    [DefinedTypeField( "Hospital List" )]
    [DefinedTypeField( "Nursing Home List" )]
    public partial class CommunionList : RockBlock
    {
        #region Control Methods

        public enum COMMUNION_STATES { KY = 1, IN = 2, Other = 3 }

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

                cpCampus.Campuses = Rock.Web.Cache.CampusCache.All();
                cblState.BindToEnum<COMMUNION_STATES>();
            }
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
            gReport.SetLinqDataSource<CommunionData>( getQuery<CommunionData>() );
            gReport.DataBind();

        }

        private IQueryable<CommunionData> getQuery<T>()
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowService = new WorkflowService( rockContext );
                var attributeService = new AttributeService( rockContext );
                var attributeValueService = new AttributeValueService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var personService = new PersonService( rockContext );
                var definedValueService = new DefinedValueService( rockContext );

                Guid hospitalWorkflow = GetAttributeValue( "HospitalAdmissionWorkflow" ).AsGuid();
                Guid nursingHomeAdmissionWorkflow = GetAttributeValue( "NursingHomeResidentWorkflow" ).AsGuid();
                Guid homeBoundPersonWorkflow = GetAttributeValue( "HomeboundPersonWorkflow" ).AsGuid();
                Guid hospitalList = GetAttributeValue( "HospitalList" ).AsGuid();
                Guid nursingHomeList = GetAttributeValue( "NursingHomeList" ).AsGuid();

                var workflowTypesIdAsStrings = new WorkflowTypeService( rockContext ).Queryable()
                    .Where( wt =>
                         wt.Guid == hospitalWorkflow
                         || wt.Guid == nursingHomeAdmissionWorkflow
                         || wt.Guid == homeBoundPersonWorkflow
                        )
                    .ToList()
                    .Select( wf => wf.Id.ToString() )
                    .ToList();

                var attributeIds = attributeService.Queryable()
                    .Where( a => a.EntityTypeQualifierColumn == "WorkflowTypeId" && workflowTypesIdAsStrings.Contains( a.EntityTypeQualifierValue ) )
                    .Select( a => a.Id ).ToList();

                var wfTmpqry = workflowService.Queryable().AsNoTracking()
                     .Where( w => (
                        w.WorkflowType.Guid == hospitalWorkflow
                        || w.WorkflowType.Guid == nursingHomeAdmissionWorkflow
                        || w.WorkflowType.Guid == homeBoundPersonWorkflow
                     ) && ( w.Status == "Active" ) );

                var tqry = wfTmpqry.Join( attributeValueService.Queryable(),
                    obj => obj.Id,
                    av => av.EntityId.Value,
                    ( obj, av ) => new { Workflow = obj, AttributeValue = av } )
                    .Where( a => attributeIds.Contains( a.AttributeValue.AttributeId ) )
                    .GroupBy( obj => obj.Workflow )
                    .Select( obj => new { Workflow = obj.Key, AttributeValues = obj.Select( a => a.AttributeValue ) } );
                var qry = tqry.ToList();


                List<DefinedValueCache> facilities = DefinedTypeCache.All().Where( dv => dv.Guid == hospitalList || dv.Guid == nursingHomeList ).SelectMany(dv => dv.DefinedValues).ToList();

                var newQry = qry.Select( w => new CommunionData
                {
                    Campus = new Func<Campus>( () =>
                    {
                        Campus campus = null;
                        AttributeValue personAliasAV = w.AttributeValues.AsQueryable().Where( av => av.AttributeKey == "PersonToVisit" || av.AttributeKey == "HomeboundPerson" ).FirstOrDefault();
                        if ( personAliasAV != null )
                        {
                            PersonAlias pa = personAliasService.Get( personAliasAV.Value.AsGuid() );
                            if ( pa != null )
                            {
                                campus = pa.Person.GetCampus();
                            }
                        }
                        if ( campus == null )
                        {
                            campus = new Campus() { Name = "Unknown" };
                        }
                        return campus;
                    } )(),
                    Person = GetPerson( personAliasService, w.AttributeValues ),
                    Age = GetPerson( personAliasService, w.AttributeValues ).Age,
                    Description = w.AttributeValues.AsQueryable().Where( av => av.AttributeKey == "VisitationRequestDescription" || av.AttributeKey == "HomeboundResidentDescription" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    Location = new Func<string>( () =>
                    {
                        return w.AttributeValues.AsQueryable().Where( av => av.AttributeKey == "NursingHome" || av.AttributeKey == "Hospital" ).Select( av => av.ValueFormatted ).DefaultIfEmpty( "Home" ).FirstOrDefault();
                    } )(),
                    Address = GetLocation( personService, w.AttributeValues, facilities ).Street1 + " " + GetLocation( personService, w.AttributeValues, facilities ).Street2,
                    City = GetLocation( personService, w.AttributeValues, facilities ).City,
                    State = GetLocation( personService, w.AttributeValues, facilities ).State,
                    PostalCode = GetLocation( personService, w.AttributeValues, facilities ).PostalCode,
                    FacilityNumber = GetFacilityNumber( personService, w.AttributeValues, facilities ),
                    Room = w.AttributeValues.AsQueryable().Where( av => av.AttributeKey == "Room" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    AdmitDate = w.AttributeValues.AsQueryable().Where( av => av.AttributeKey == "AdmitDate" || av.AttributeKey == "StartDate" ).Select( av => av.ValueFormatted ).FirstOrDefault(),
                    Status = w.Workflow.Status,
                    Communion = w.AttributeValues.AsQueryable().Where( av => av.AttributeKey == "Communion" ).FirstOrDefault().ValueFormatted
                } )
                .Where( o => o.Communion.AsBoolean() && !o.Person.IsDeceased )
                .OrderBy( a => a.PostalCode )
                .ThenBy( a => a.Address )
                .ToList()
                .AsQueryable();


                List<COMMUNION_STATES> states = cblState.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => ( COMMUNION_STATES ) int.Parse( i.Value ) ).ToList();

                if ( states.Count > 0 )
                {
                    newQry = newQry.Where( o => ( states.Contains( COMMUNION_STATES.KY ) && o.State == "KY" )
                    || ( states.Contains( COMMUNION_STATES.IN ) && o.State == "IN" )
                    || ( ( states.Contains( COMMUNION_STATES.Other ) && o.State != "IN" && o.State != "KY" ) ) );
                }
                List<int> campuses = cpCampus.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => int.Parse( i.Value ) ).ToList();

                if ( campuses.Count > 0 )
                {
                    newQry = newQry.Where( o => campuses.Contains( o.Campus.Id ) );
                }


                //AddGridColumns( newQry.FirstOrDefault() );

                SortProperty sortProperty = gReport.SortProperty;
                if ( sortProperty != null )
                {
                    newQry = newQry.Sort( sortProperty );
                }

                return newQry;

            }
        }

        private Person GetPerson( PersonAliasService personAliasService, IEnumerable<AttributeValue> attributeValues )
        {

            AttributeValue personAliasAV = attributeValues.AsQueryable().Where( av => av.AttributeKey == "PersonToVisit" || av.AttributeKey == "HomeboundPerson" ).FirstOrDefault();
            if ( personAliasAV != null )
            {
                PersonAlias pa = personAliasService.Get( personAliasAV.Value.AsGuid() );
                if ( pa != null )
                {
                    return pa.Person;
                }
            }
            return new Person();
        }

        private Location GetLocation( PersonService personService, IEnumerable<AttributeValue> attributeValues, List<DefinedValueCache> facilities )
        {

            string locationGuid = attributeValues.AsQueryable().Where( av => av.AttributeKey == "NursingHome" || av.AttributeKey == "Hospital" ).Select( av => av.Value ).FirstOrDefault();

            if ( locationGuid != null )
            {
                DefinedValueCache dv = facilities.Where( h => h.Guid == locationGuid.AsGuid() ).FirstOrDefault();
                Location location = new Location();
                if ( dv != null )
                {
                    location.Street1 = dv.AttributeValues["Qualifier1"].ValueFormatted;
                    location.City = dv.AttributeValues["Qualifier2"].ValueFormatted;
                    location.State = dv.AttributeValues["Qualifier3"].ValueFormatted;
                    location.PostalCode = dv.AttributeValues["Qualifier4"].ValueFormatted;
                }
                return location;
            }

            int? personId = attributeValues.AsQueryable().Where( av => av.AttributeKey == "HomeboundPerson" || av.AttributeKey == "PersonToVisit" ).Select( av => av.ValueAsPersonId ).FirstOrDefault();
            if ( personId.HasValue )
            {
                Person p = personService.Get( personId.Value );
                if ( p != null && p.GetHomeLocation() != null )
                {
                    return p.GetHomeLocation();
                }
            }
            return new Location();
        }

        private string GetFacilityNumber( PersonService personService, IEnumerable<AttributeValue> attributeValues, List<DefinedValueCache> facilities )
        {

            string facility = attributeValues.AsQueryable().Where( av => av.AttributeKey == "NursingHome" || av.AttributeKey == "Hospital" ).Select( av => av.Value ).FirstOrDefault();

            if ( facility != null )
            {
                DefinedValueCache dv = facilities.Where( h => h.Guid == facility.AsGuid() ).FirstOrDefault();
                if (dv != null)
                {
                    return dv.AttributeValues["Qualifier5"].ValueFormatted;
                }
            }
            return "";
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

        private void ShowMessage( string message, string header = "Information", string cssClass = "panel panel-warning" )
        {
            pnlMain.Visible = false;
            pnlInfo.Visible = true;
            ltHeading.Text = header;
            ltBody.Text = message;
            pnlInfo.CssClass = cssClass;
        }
        #endregion

        protected void gFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrid();
        }


        /// <summary>
        /// Handles the ExcelExportClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Actions_ExcelExportClick( object sender, EventArgs e )
        {

            // create default settings
            string filename = gReport.ExportFilename;
            string workSheetName = "List";
            string title = "Communion List - " + Rock.RockDateTime.Today.ToString( "MMMM d, yyyy" );

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

            //// write data to worksheet there are three supported data sources
            //// DataTables, DataViews and ILists

            int rowCounter = 4;
            int columnCounter = 0;

            // print headings
            foreach ( String column in new List<String>() { "Zip", "Name", "Campus", "Address", "Phone", "Notes" } )
            {
                columnCounter++;
                worksheet.Cells[3, columnCounter].Value = column.SplitCase();
            }
            PhoneNumberService phoneNumberService = new PhoneNumberService( new RockContext() );
            Guid homePhone = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
            // print data
            foreach ( CommunionData row in getQuery<CommunionData>() )
            {
                SetExcelValue( worksheet.Cells[rowCounter, 1], row.PostalCode.Length > 5 ? row.PostalCode.Substring( 0, 5 ) : row.PostalCode );
                SetExcelValue( worksheet.Cells[rowCounter, 2], row.Person.FullName );
                SetExcelValue( worksheet.Cells[rowCounter, 3], row.Campus );
                SetExcelValue( worksheet.Cells[rowCounter, 4], ( row.Location != "Home" ? row.Location + "\r\n" : "" ) 
                    + row.Address + ( !string.IsNullOrEmpty( row.Room ) ? "\r\nRoom: " + row.Room : "" ) 
                    + ( !string.IsNullOrWhiteSpace( row.FacilityNumber ) ? "\r\n" + row.FacilityNumber : "" ) );
                SetExcelValue( worksheet.Cells[rowCounter, 5], phoneNumberService.GetByPersonId( row.Person.Id ).Where( p => p.NumberTypeValue.Guid == homePhone ).Select( p => p.NumberFormatted ).FirstOrDefault() );
                SetExcelValue( worksheet.Cells[rowCounter, 6], row.Description );
                worksheet.Cells[rowCounter, 6].Style.WrapText = true;
                
                rowCounter++;
            }
            var range = worksheet.Cells[3, 1, rowCounter, columnCounter];

            // use conditionalFormatting to create the alternate row style
            var conditionalFormatting = range.ConditionalFormatting.AddExpression();
            conditionalFormatting.Formula = "MOD(ROW()+1,2)=0";
            conditionalFormatting.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            conditionalFormatting.Style.Fill.BackgroundColor.Color = Color.FromArgb( 240, 240, 240 );

            var table = worksheet.Tables.Add( range, "table1" );

            // ensure each column in the table has a unique name
            var columnNames = worksheet.Cells[3, 1, 3, columnCounter].Select( a => new { OrigColumnName = a.Text, Cell = a } ).ToList();
            columnNames.Reverse();
            foreach ( var col in columnNames )
            {
                int duplicateSuffix = 0;
                string uniqueName = col.OrigColumnName;

                // increment the suffix by 1 until there is only one column with that name
                while ( columnNames.Where( a => a.Cell.Text == uniqueName ).Count() > 1 )
                {
                    duplicateSuffix++;
                    uniqueName = col.OrigColumnName + duplicateSuffix.ToString();
                    col.Cell.Value = uniqueName;
                }
            }

            table.ShowFilter = true;
            table.TableStyle = OfficeOpenXml.Table.TableStyles.None;

            // format header range
            using ( ExcelRange r = worksheet.Cells[3, 1, 3, columnCounter] )
            {
                r.Style.Font.Bold = true;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 223, 223, 223 ) );
                r.Style.Font.Color.SetColor( Color.Black );
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }

            // format and set title
            worksheet.Cells[1, 1].Value = title;
            using ( ExcelRange r = worksheet.Cells[1, 1, 1, columnCounter] )
            {
                r.Merge = true;
                r.Style.Font.SetFromFont( new Font( "Calibri", 22, FontStyle.Regular ) );
                r.Style.Font.Color.SetColor( Color.White );
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );

                // set border
                r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            using ( ExcelRange r = worksheet.Cells[3, 1, rowCounter, columnCounter] )
            {
                r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // TODO: add image to worksheet

            // freeze panes
            worksheet.View.FreezePanes( 3, 1 );

            // autofit columns for all cells
            worksheet.Cells.AutoFitColumns( 0 );

            // Set all the column widths
            worksheet.Column( 2 ).Width = 20;
            worksheet.Column( 4 ).Width = 30;
            worksheet.Column( 6 ).Width = 45;

            // add alternating highlights

            // set some footer text
            worksheet.HeaderFooter.OddHeader.CenteredText = title;
            worksheet.HeaderFooter.OddFooter.RightAlignedText = string.Format( "Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages );
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

        protected class CommunionData
        {
            public Campus Campus { get; set; }
            public Person Person { get; set; }
            public int? Age { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string FacilityNumber { get; set; }
            public string Room { get; set; }
            public string AdmitDate { get; set; }
            public string Status { get; set; }
            public string Communion { get; set; }
        }


    }
}